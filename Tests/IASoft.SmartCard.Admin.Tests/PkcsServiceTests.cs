using IASoft.SmartCard.Signer.PkcsUtils;
using NUnit.Framework;

namespace IASoft.SmartCard.Admin.Tests
{
    [TestFixture]
    public class PkcsServiceTests
    {
        [Test]
        public void SelectAllSlotsTest()
        {
            var slot = SmartCardUtils.SaferFindSlot(@"C:\Program Files (x86)\EAC MW klient\pkcs11_x86.dll", "Sig_ZEP");
            Assert.IsNotNull(slot);
        }

        [Test]
        public void FindSigningCertificate()
        {
            var slot = SmartCardUtils.SaferFindSlot(@"C:\Program Files (x86)\EAC MW klient\pkcs11_x86.dll", "Sig_ZEP");
            using (var session = PkcsSession.StartNewSession(slot, "200860"))
            {
                var signingCertificate = SmartCardUtils.FindSigningCertificate(session, "Certifikat k podpisovemu klucu");
                Assert.IsNotNull(signingCertificate);

                var certificateChain = SmartCardUtils.GetCertificateChain(signingCertificate);
                Assert.IsNotEmpty(certificateChain);
            }
        }
    }
}