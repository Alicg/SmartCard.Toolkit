namespace IASoft.SmartCard.Signer
{
    public interface IConfigurationService
    {
        string TokenLabel { get; }
        
        string CertificateLabel { get; }
        
        string GetPkcsLibPath();
    }
}