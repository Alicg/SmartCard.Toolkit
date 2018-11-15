using System;
using System.Windows;
using IASoft.SmartCard.Commons;
using IASoft.SmartCard.Signer.PkcsUtils;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
using Unity;
using Unity.Lifetime;

namespace IASoft.SmartCard.Signer
{
    public class SignerModule : IModule
    {
        private readonly IUnityContainer unityContainer;
        private readonly IRegionManager regionManager;

        public SignerModule(IUnityContainer unityContainer, IRegionManager regionManager)
        {
            this.unityContainer = unityContainer;
            this.regionManager = regionManager;
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            this.unityContainer.RegisterType<IEacPkcsLibSearcher, EacPkcsLibSearcher>(new ContainerControlledLifetimeManager());
            this.unityContainer.RegisterType<IConfigurationService, RealConfigurationService>(new ContainerControlledLifetimeManager());
            this.unityContainer.RegisterType<PdfSigner, PdfSigner>(new ContainerControlledLifetimeManager());
            this.unityContainer.RegisterType<SignerViewModel, SignerViewModel>(new ContainerControlledLifetimeManager());
            this.unityContainer.RegisterType<SmartCardStatusViewModel, SmartCardStatusViewModel>(new ContainerControlledLifetimeManager());
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
            this.RegisterRegions();
            this.InitResources();
        }

        private void RegisterRegions()
        {
            var signerViewModel = this.unityContainer.Resolve<SignerViewModel>();
            this.regionManager.RegisterViewWithRegion(RegionNames.MainContentRegion, () => signerViewModel);
            
            var smartCardStatusViewModel = this.unityContainer.Resolve<SmartCardStatusViewModel>();
            this.regionManager.RegisterViewWithRegion(RegionNames.ToolbarRegion, () => new SmartCardStatusView{DataContext = smartCardStatusViewModel});
        }

        private void InitResources()
        {
            var dict = new ResourceDictionary {Source = new Uri("/IASoft.SmartCard.Signer;component/SignerResources.xaml", UriKind.Relative)};
            Application.Current.Resources.MergedDictionaries.Add(dict);
        }
    }
}