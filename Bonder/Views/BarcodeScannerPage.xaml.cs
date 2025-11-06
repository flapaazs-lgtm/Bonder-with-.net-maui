using Bonder.Services;
using HomeKit;
using Bonder.ViewModels;

namespace Bonder.Views;

public partial class BarcodeScannerPage : ContentPage
{
    private readonly BarcodeScannerViewModel _viewModel;
    private bool _isProcessing = false;

    public BarcodeScannerPage(BarcodeScannerViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    private async void OnBarcodesDetected(object sender, BarcodeDetectionEventArgs e)
    {
        if (_isProcessing)
            return;

        var barcode = e.Results?.FirstOrDefault();
        if (barcode != null)
        {
            _isProcessing = true;

            // Stop detection
            CameraView.IsDetecting = false;

            // Process barcode
            await _viewModel.ProcessBarcodeAsync(barcode.Value);

            // Allow time before restarting detection
            await Task.Delay(2000);

            _isProcessing = false;
            CameraView.IsDetecting = true;
        }
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        HMCameraView.IsDetecting = true;
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        CameraView.IsDetecting = false;
    }
}
