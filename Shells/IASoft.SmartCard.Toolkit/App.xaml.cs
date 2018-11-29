using System.Windows;

namespace IASoft.SmartCard.Toolkit
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            
            var bootstrapper = new Bootstrapper();
            bootstrapper.Run();
        }
    }
}