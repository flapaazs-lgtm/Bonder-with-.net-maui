using Bonder.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonder.ViewModels;
public class BarcodeScannerViewModel : BaseViewModel
{
    private readonly IBarcodeScannerService _scannerService;
    private readonly IStorageService _storageService;
    private bool _isScanning = true;

    public bool IsScanning
    {
        get => _isScanning;
        set => SetProperty(ref _isScanning, value);
    }

    public Command CancelCommand { get; }
    public Command ManualEntryCommand { get; }

    public BarcodeScannerViewModel(
        IBarcodeScannerService scannerService,
        IStorageService storageService)
    {
        _scannerService = scannerService;
        _storageService = storageService;

        CancelCommand = new Command(async () => await CancelScanAsync());
        ManualEntryCommand = new Command(async () => await ManualEntryAsync());
    }

    public async Task ProcessBarcodeAsync(string barcode)
    {
        try
        {
            IsScanning = false;

            // Vibrate to indicate successful scan
            Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(100));

            // Look up book by ISBN
            var book = await _scannerService.LookupBookByISBNAsync(barcode);

            if (book != null)
            {
                // Navigate to book details
                await Shell.Current.GoToAsync($"//BookDetails?bookId={book.Id}");
            }
            else
            {
                await Shell.Current.DisplayAlert("Not Found",
                    "Could not find a book with this ISBN. Try manual search instead.",
                    "OK");
                IsScanning = true;
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error",
                $"Failed to process barcode: {ex.Message}",
                "OK");
            IsScanning = true;
        }
    }

    private async Task CancelScanAsync()
    {
        await Shell.Current.GoToAsync("..");
    }

    private async Task ManualEntryAsync()
    {
        var isbn = await Shell.Current.DisplayPromptAsync(
            "Manual ISBN Entry",
            "Enter the 10 or 13 digit ISBN:",
            "Search",
            "Cancel",
            keyboard: Keyboard.Numeric,
            maxLength: 13);

        if (!string.IsNullOrEmpty(isbn))
        {
            await ProcessBarcodeAsync(isbn);
        }
    }
}