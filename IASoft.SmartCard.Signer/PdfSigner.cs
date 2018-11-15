using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.security;
using IASoft.SmartCard.Signer.PkcsUtils;
using Net.Pkcs11Interop.PDF;

namespace IASoft.SmartCard.Signer
{
    public class PdfSigner
    {
        private readonly string pkcsLibPath;
        private readonly string tokenLabel;
        private readonly string ckaLabel;

        public PdfSigner(IConfigurationService configurationService)
        {
            this.pkcsLibPath = configurationService.GetPkcsLibPath();
            this.tokenLabel = configurationService.TokenLabel;
            this.ckaLabel = configurationService.CertificateLabel;
        }

        public void SignPdf(string inputPdfPath, string signedPdfPath, string tokenPin)
        {
            // Pkcs11RsaSignature can't find a private key by certificate label, only by certificate id.
            var signingCertificateId = this.FindSigningCertificateId(tokenPin, this.ckaLabel);

            var pkcs11RsaSignature = SmartCardUtils.SaferCreateSignature(this.pkcsLibPath, this.tokenLabel, tokenPin, signingCertificateId);
            if (pkcs11RsaSignature == null)
            {
                throw new InvalidOperationException("Smart card read error.");
            }
            try
            {
                var rawSigningCertificate = pkcs11RsaSignature.SaferGetSigningCertificate();
                var signingCertificate = SmartCardUtils.ParseCertificate(rawSigningCertificate);
                var signatureAuthor = GetCertificateCn(signingCertificate.Subject);
                var certificateChain = SmartCardUtils.GetCertificateChain(signingCertificate);

                var certPath = CertUtils.BuildCertPath(rawSigningCertificate, certificateChain.Select(v => v.RawData).ToList());
                
                using (var pdfReader = new PdfReader(inputPdfPath))
                {
                    using (var outputStream = new FileStream(signedPdfPath, FileMode.Create))
                    {
                        // Create PdfStamper that applies extra content to the PDF document
                        using (var pdfStamper = PdfStamper.CreateSignature(pdfReader, outputStream, '\0', Path.GetTempFileName(), true))
                        {
                            pdfStamper.SignatureAppearance.SignatureCreator = signatureAuthor;
                            pdfStamper.SignatureAppearance.SignDate = DateTime.Now;
                            // Sign PDF document
                            MakeSignature.SignDetached(pdfStamper.SignatureAppearance, pkcs11RsaSignature, certPath, null, null, null, 0, CryptoStandard.CADES);
                        }
                    }
                }
            }
            finally
            {
                pkcs11RsaSignature.Dispose();
            }
        }

        private string FindSigningCertificateId(string tokenPin, string certificateLabel)
        {
            var signingSlot = SmartCardUtils.SaferFindSlot(this.pkcsLibPath, this.tokenLabel);
            if (signingSlot == null)
            {
                throw new InvalidOperationException("No Smart Card was found.");
            }
            using (var session = PkcsSession.StartNewSession(signingSlot, tokenPin))
            {
                return SmartCardUtils.FindSigningCertificateId(session, certificateLabel);
            }
        }

        private static string GetCertificateCn(string subject)
        {
            var pattern = ".*CN=(?<CN>.*?),";
            return Regex.Match(subject, pattern).Groups["CN"].Value;
        }
    }
}