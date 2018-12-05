using System;
using System.Configuration;
using System.IO;
using System.Windows;
using IASoft.SmartCard.Signer.PkcsUtils;
using Utils.Extensions;

namespace IASoft.SmartCard.Signer
{
    public class RealConfigurationService : IConfigurationService
    {
        private const string PkcsLibPathConfigName = "pkcs11_lib_path";
        private const string SmardCardTokenLabel = "SmardCardTokenLabel";
        private const string SigningCertificateLabel = "SigningCertificateLabel";
        
        private readonly IEacPkcsLibSearcher eacPkcsLibSearcher;

        public RealConfigurationService(IEacPkcsLibSearcher eacPkcsLibSearcher)
        {
            this.eacPkcsLibSearcher = eacPkcsLibSearcher;

            this.TokenLabel = ConfigurationManager.AppSettings[SmardCardTokenLabel];
            this.CertificateLabel = ConfigurationManager.AppSettings[SigningCertificateLabel];
        }

        public string TokenLabel { get; }
        
        public string CertificateLabel { get; }

        public string GetPkcsLibPath()
        {
            var pkcsLibPathInConfig = ConfigurationManager.AppSettings[PkcsLibPathConfigName];
            if (File.Exists(pkcsLibPathInConfig))
            {
                return pkcsLibPathInConfig;
            }

            try
            {
                var foundPkcsLibPath = this.eacPkcsLibSearcher.FindPcksLib();
                ConfigurationManager.AppSettings[PkcsLibPathConfigName] = foundPkcsLibPath;
                ConfigurationManager.RefreshSection("appSetting");

                return foundPkcsLibPath;
            }
            catch (InvalidOperationException e)
            {
                MessageBox.Show("No PKCS11 lib was found. Please install PCKS11 provider and launch this tool again\r\n\r\n" + e.GetFullMessage(), "No PKCS11 lib was found", MessageBoxButton.OK, MessageBoxImage.Error);
                Environment.Exit(0);
                throw;
            }
        }
    }
}