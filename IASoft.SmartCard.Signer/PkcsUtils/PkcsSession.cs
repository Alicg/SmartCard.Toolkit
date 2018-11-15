using System;
using Net.Pkcs11Interop.Common;
using Net.Pkcs11Interop.HighLevelAPI;

namespace IASoft.SmartCard.Signer.PkcsUtils
{
    public class PkcsSession : IDisposable
    {
        private PkcsSession(Session session)
        {
            this.Session = session;
        }
        
        public Session Session { get; }

        public static PkcsSession StartNewSession(Slot slot, string pin)
        {
            var session = slot.OpenSession(true);
            session.CloseWhenDisposed = true;
            session.Login(CKU.CKU_USER, pin);
            return new PkcsSession(session);
        }

        public void Dispose()
        {
            this.Session.Dispose();
        }
    }
}