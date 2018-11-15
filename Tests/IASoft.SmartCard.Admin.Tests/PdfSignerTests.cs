using System.IO;
using IASoft.SmartCard.Signer;
using Moq;
using NUnit.Framework;

namespace SmartCard.Admin.Tests
{
    [TestFixture]
    public class PdfSignerTests
    {
        [Test]
        public void SignPdfTest()
        {
            const string InputPdf = @"D:\Dima\Work\Home\SmartCard.Admin\Tests\SmartCard.Admin.Tests\TestFiles\unsignedPDF.pdf";
            const string OutputPDF = @"D:\Dima\Work\Home\SmartCard.Admin\Tests\SmartCard.Admin.Tests\TestFiles\signedPDF.pdf";
            const string PkcsLibPath = @"C:\Program Files (x86)\EAC MW klient\pkcs11_x86.dll";
            const string SigningTokenLabel = "Sig_ZEP";
            const string SigningCertificateLabel = "Certifikat k podpisovemu klucu";

            var configurationMock = new Mock<IConfigurationService>();
            configurationMock.Setup(v => v.GetPkcsLibPath()).Returns(PkcsLibPath);
            configurationMock.Setup(v => v.TokenLabel).Returns(SigningTokenLabel);
            configurationMock.Setup(v => v.CertificateLabel).Returns(SigningCertificateLabel);
            
            var pdfSigner = new PdfSigner(configurationMock.Object);
            pdfSigner.SignPdf(InputPdf, OutputPDF, "200860");
            
            Assert.IsTrue(File.Exists(OutputPDF));
        }
    }
}