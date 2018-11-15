using System;
using System.Threading.Tasks;
using IASoft.PrismCommons;
using IASoft.WPFCommons;
using IASoft.WPFCommons.Events;
using IASoft.WPFCommons.Reactive;
using Prism.Commands;
using Utils.Extensions;

namespace IASoft.SmartCard.Signer
{
    public class SignerViewModel : BasePrismViewModel
    {
        private readonly SmartCardStatusViewModel smartCardStatusViewModel;
        private readonly PdfSigner pdfSigner;
        private readonly IIoDialogsService ioDialogsService;
        private bool signing;
        private string lastStatusDescription;
        private bool lastDocumentSigned;

        public SignerViewModel(SmartCardStatusViewModel smartCardStatusViewModel,
            PdfSigner pdfSigner,
            IIoDialogsService ioDialogsService,
            IReactiveEventAggregator eventAggregator) : base(eventAggregator)
        {
            this.smartCardStatusViewModel = smartCardStatusViewModel;
            this.pdfSigner = pdfSigner;
            this.ioDialogsService = ioDialogsService;

            this.ChoosePdfToSignCommand = new DelegateCommand(this.ChoosePdfToSignExecute,
                () => smartCardStatusViewModel.SmartCardDisconnected.HasValue && !smartCardStatusViewModel.SmartCardDisconnected.Value);
            
            this.SignPdfCommand = new DelegateCommand<string>(this.SignPdfExecute,
                v => smartCardStatusViewModel.SmartCardDisconnected.HasValue && !smartCardStatusViewModel.SmartCardDisconnected.Value);

            smartCardStatusViewModel.ObservableFromPropertyChanged(nameof(SmartCardStatusViewModel.SmartCardDisconnected))
                .Subscribe(v => this.ChoosePdfToSignCommand.RaiseCanExecuteChanged());
        }

        public DelegateCommand ChoosePdfToSignCommand { get; }
        
        public DelegateCommand<string> SignPdfCommand { get; }

        public bool Signing
        {
            get => this.signing;
            set
            {
                this.signing = value;
                this.RaisePropertyChanged();
            }
        }

        public string LastStatusDescription
        {
            get => this.lastStatusDescription;
            set
            {
                this.lastStatusDescription = value;
                this.RaisePropertyChanged();
            }
        }

        public bool LastDocumentSigned
        {
            get => this.lastDocumentSigned;
            set
            {
                this.lastDocumentSigned = value;
                this.RaisePropertyChanged();
            }
        }

        private void ChoosePdfToSignExecute()
        {
            var inputFile = this.ioDialogsService.OpenFile(@"C:\", ".pdf", "PDF files|*.pdf;", "Select file to sign:");
            if (inputFile != null)
            {
                this.SignPdfExecute(inputFile);
            }
        }

        private void SignPdfExecute(string inputPath)
        {
            var outputPath = this.ioDialogsService.SaveFile(@"C:\", "signed.pdf", "PDF files|*.pdf", out bool cancelled, "Choose where to save signed file:");
            if (!cancelled)
            {
                this.ViewModelPopupRequests.AskForStringForm("Enter a pin for the smart card",
                    async pin =>
                    {
                        if (pin.IsConfirmed)
                        {
                            try
                            {
                                this.Signing = true;
                                await Task.Run(() => this.SignPdfDocument(inputPath, outputPath, (string) pin.Content));
                                this.LastStatusDescription = "Successfully signed";
                                this.LastDocumentSigned = true;
                            }
                            catch(Exception e)
                            {
                                this.LastStatusDescription = e.GetFullMessage();
                                this.LastDocumentSigned = false;
                            }
                            finally
                            {
                                this.Signing = false;
                            }
                        }
                    });
            }
        }

        private void SignPdfDocument(string inputPath, string outputPath, string pin)
        {
            this.pdfSigner.SignPdf(inputPath, outputPath, pin);
        }
    }
}