using System;
using System.CodeDom;
using System.Reactive.Linq;
using IASoft.PrismCommons;
using IASoft.SmartCard.Signer.PkcsUtils;
using IASoft.WPFCommons.Events;

namespace IASoft.SmartCard.Signer
{
    public class SmartCardStatusViewModel : BasePrismViewModel
    {
        private const int RefreshStatusIntervalInSeconds = 5;
        private readonly IConfigurationService configurationService;
        private bool? smartCardDisconnected;
        private bool statusRefreshing;
        private string smartCardConnectionDescription;

        public SmartCardStatusViewModel(IConfigurationService configurationService, IReactiveEventAggregator eventAggregator) : base(eventAggregator)
        {
            this.configurationService = configurationService;
            
            Observable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(RefreshStatusIntervalInSeconds)).ObserveOnDispatcher().Synchronize().Subscribe(v => this.RefreshStatus());
        }

        public string SmartCardConnectionDescription
        {
            get => this.smartCardConnectionDescription;
            set
            {
                if (this.smartCardConnectionDescription == value)
                {
                    return;
                }
                this.smartCardConnectionDescription = value;
                this.RaisePropertyChanged();
            }
        }

        public bool? SmartCardDisconnected
        {
            get => this.smartCardDisconnected;
            set
            {
                if (this.smartCardDisconnected == value)
                {
                    return;
                }
                this.smartCardDisconnected = value;
                this.RaisePropertyChanged();
            }
        }

        public bool StatusRefreshing
        {
            get => this.statusRefreshing;
            set
            {
                if (this.statusRefreshing == value)
                {
                    return;
                }
                this.statusRefreshing = value;
                this.RaisePropertyChanged();
            }
        }

        private async void RefreshStatus()
        {
            // if card is connected do not show progress.
            if (this.SmartCardDisconnected.HasValue && this.SmartCardDisconnected.Value)
            {
                this.StatusRefreshing = true;
                this.SmartCardDisconnected = null;
            }
            
            var signingSlot = await SmartCardUtils.FindSlotAsync(this.configurationService.GetPkcsLibPath(), this.configurationService.TokenLabel);
            this.SmartCardDisconnected = signingSlot == null;
            this.StatusRefreshing = false;

            if (this.SmartCardDisconnected.Value)
            {
                this.SmartCardConnectionDescription = $"No connected smart card is found.";
            }
            else
            {
                this.SmartCardConnectionDescription = $"Smart card (slot id: {signingSlot.SlotId}) is connected.";
            }
        }
    }
}