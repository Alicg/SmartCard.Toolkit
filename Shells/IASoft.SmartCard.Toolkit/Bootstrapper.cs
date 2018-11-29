using System;
using System.Linq;
using System.Reactive.Concurrency;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using IASoft.MaterialDesignCommons;
using IASoft.PrismCommons.PendingProcess;
using IASoft.PrismCommons.PrismExtensions;
using IASoft.SmartCard.Signer;
using IASoft.WPFCommons;
using IASoft.WPFCommons.Background;
using IASoft.WPFCommons.Events;
using Prism.Modularity;
using Prism.Regions;
using Prism.Unity;
using Unity;
using Unity.Interception.ContainerIntegration;
using Unity.Lifetime;
using UnityUtils;

namespace IASoft.SmartCard.Toolkit
{
    public class Bootstrapper : UnityBootstrapper
    {
        protected override void InitializeShell()
        {
            base.InitializeShell();

            Application.Current.MainWindow = (MainWindow) this.Shell;
            Application.Current.MainWindow?.Show();

            Application.Current.Dispatcher.UnhandledException += HandleDispatcherError;
            Application.Current.Exit += (sender, args) => this.Container.Dispose();
            AppDomain.CurrentDomain.UnhandledException += HandleAppDomainError;

            new MaterialDesignPopupHandler(this.Container.Resolve<IReactiveEventAggregator>());
        }

        protected override DependencyObject CreateShell()
        {
            this.InitDependencies();

            var shell = this.Container.TryResolve<MainWindow>();
            //shell.DataContext = this.Container.TryResolve<ShellViewModel>();

            this.ClearPendingProcessesWhenAppCloses(shell);

            return shell;
        }

        protected override void ConfigureModuleCatalog()
        {
            base.ConfigureModuleCatalog();

            this.ModuleCatalog.AddModule(new ModuleInfo("Signer", typeof(SignerModule).AssemblyQualifiedName));
        }

        protected override RegionAdapterMappings ConfigureRegionAdapterMappings()
        {
            RegionAdapterMappings mappings = base.ConfigureRegionAdapterMappings();
            mappings.RegisterMapping(typeof(StackPanel), this.Container.Resolve<StackPanelRegionAdapter>());
            mappings.RegisterMapping(typeof(ToolBarTray), this.Container.Resolve<ToolBarTrayRegionAdapter>());
            return mappings;
        }

        private void InitDependencies()
        {
            GlobalUnityContainer.ReplaceContainer(this.Container);
            GlobalUnityContainer.Register<IReactiveEventAggregator, ReactiveEventAggregator>(new ContainerControlledLifetimeManager());
            GlobalUnityContainer.RegisterInstance<IScheduler>(DispatcherScheduler.Current, new ContainerControlledLifetimeManager());
            GlobalUnityContainer.Register<IIoDialogsService, IoDialogsService>(new ContainerControlledLifetimeManager());
            this.Container.AddNewExtension<Interception>();
            this.Container.RegisterType<BackgroundTasksQueue, BackgroundTasksQueue>(new ContainerControlledLifetimeManager());
            this.Container.RegisterType<BackgroundProcessesQueue, BackgroundProcessesQueue>(new ContainerControlledLifetimeManager());
        }

        private void ClearPendingProcessesWhenAppCloses(MainWindow shell)
        {
            shell.Closing += (sender, args) =>
            {
                this.Container.Resolve<BackgroundProcessesQueue>().CancelAll();
                var backgroundTasksQueue = this.Container.Resolve<BackgroundTasksQueue>();
                var eventAggregator = this.Container.Resolve<IReactiveEventAggregator>();
                using (PendingProcess.StartNew(eventAggregator,
                    "Waiting for background tasks to complete:" +
                    backgroundTasksQueue.ActiveBackgroundTasks.Aggregate("", (t, c) => t + "\r\n" + c)))
                {
                    while (!backgroundTasksQueue.ActiveBackgroundTasks.IsEmpty)
                    {
                        Task.Delay(100).Wait();
                    }
                }
            };
            Application.Current.DispatcherUnhandledException += (sender, args) => this.Container.Resolve<BackgroundProcessesQueue>().CancelAll();
            AppDomain.CurrentDomain.UnhandledException += (sender, args) => this.Container.Resolve<BackgroundProcessesQueue>().CancelAll();
        }

        private static void RegisterApplicationError(string title, string message, Exception e)
        {
            ProgramRuntime.ShowErrorMessage(message, e);
        }

        private static void HandleDispatcherError(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            RegisterApplicationError("Unexpected UI Dispatcher error.", "Unexpected UI Dispatcher error. See log file for error trace details.", e.Exception);
            e.Handled = true;
        }

        private static void HandleAppDomainError(object sender, UnhandledExceptionEventArgs ea)
        {
            RegisterApplicationError("General application error.",
                "General application error. See log file for error trace details.",
                ea.ExceptionObject as Exception);
            if (ea.IsTerminating)
            {
                ProgramRuntime.Shutdown();
            }
        }
    }
}