using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Net.Pkcs11Interop.Common;
using Net.Pkcs11Interop.HighLevelAPI;
using Net.Pkcs11Interop.PDF;

namespace IASoft.SmartCard.Signer.PkcsUtils
{
    public static class SmartCardUtils
    {
        public static Slot SaferFindSlot(string pkcsLibPath, string signingTokenLabel)
        {
            const int TimesToTryFindSlot = 3;
            // sometimes GetTokenInfo can return native exception, try again usually works fine.
            for (var i = 0; i < TimesToTryFindSlot; i++)
            {
                try
                {
                    return new Pkcs11(pkcsLibPath, true).GetSlotList(true).FirstOrDefault(v => v.GetTokenInfo().Label == signingTokenLabel);
                }
                catch
                {
                    Task.Delay(200);
                    // ignored
                }
            }

            return null;
        }

        public static Task<Slot> FindSlotAsync(string pkcsLibPath, string signingTokenLabel)
        {
            return Task.Run(() => SaferFindSlot(pkcsLibPath, signingTokenLabel));
        }

        public static X509Certificate2 FindSigningCertificate(PkcsSession session, string certificateLabel)
        {
            var certificateHandle = session.Session
                .FindAllObjects(new List<ObjectAttribute> {new ObjectAttribute(CKA.CKA_LABEL, certificateLabel)})
                .FirstOrDefault();

            if (certificateHandle == null)
            {
                throw new InvalidOperationException("Certificate with specified label wasn't found: " + certificateLabel);
            }
            
            var certificateObject = session.Session.GetAttributeValue(certificateHandle, new List<CKA> {CKA.CKA_VALUE}).First();
            
            var rawCertificate = certificateObject.GetValueAsByteArray();
            return ParseCertificate(rawCertificate);
        }

        public static IEnumerable<X509Certificate2> GetCertificateChain(X509Certificate2 certificate)
        {
            using(var chain = new X509Chain())
            {
                if (chain.Build(certificate))
                {
                    return chain.ChainElements.Cast<X509ChainElement>().Select(v => v.Certificate).ToArray();
                }
            }

            return new X509Certificate2[0];
        }

        public static string FindSigningCertificateId(PkcsSession session, string certificateLabel)
        {
            var certificateHandle = session.Session
                .FindAllObjects(new List<ObjectAttribute> {new ObjectAttribute(CKA.CKA_LABEL, certificateLabel)})
                .FirstOrDefault();
            
            var certificateObject = session.Session.GetAttributeValue(certificateHandle, new List<CKA> {CKA.CKA_ID}).First();
            var idInDecimal = certificateObject.GetValueAsByteArray()[0];
            return idInDecimal.ToString("X");
        }

        public static X509Certificate2 ParseCertificate(byte[] rawCertificate)
        {
            var file = Path.Combine(Path.GetTempPath(), "SmartCard.Admin-" + Guid.NewGuid());
            try
            {
                File.WriteAllBytes(file, rawCertificate);
                return new X509Certificate2(file);
            }
            finally
            {
                File.Delete(file);
            }
        }

        public static Pkcs11RsaSignature SaferCreateSignature(string pkcsLibPath, string tokenLabel, string tokenPin, string signingCertificateId)
        {
            for (var i = 0; i < 3; i++)
            {
                try
                {
                    return new Pkcs11RsaSignature(pkcsLibPath, null, tokenLabel, tokenPin, null, signingCertificateId, HashAlgorithm.SHA256);
                }
                catch
                {
                    Task.Delay(200);
                    // ignored
                }
            }

            return null;
        }

        public static byte[] SaferGetSigningCertificate(this Pkcs11RsaSignature signature)
        {
            for (var i = 0; i < 3; i++)
            {
                try
                {
                    return signature.GetSigningCertificate();
                }
                catch
                {
                    Task.Delay(200);
                    // ignored
                }
            }

            return null;
        }
    }
}