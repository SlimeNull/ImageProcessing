using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ImageProcessingWpf.Extensions;
using ImageProcessingWpf.Models;
using Microsoft.Win32;
using SkiaSharp;

namespace ImageProcessingWpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    [ObservableObject]
    public partial class MainWindow : Window
    {
        private readonly SemaphoreSlim _semaphoreSlim = new(0);
        private readonly Thread _backgroundProcessThread;

        private readonly OpenFileDialog _openImageDialog = new OpenFileDialog()
        {
            Title = "Open Image",
            Filter = "Image Files|*.png;*.jpg;*.jpeg;*.bmp;*.gif;*.tiff|All Files|*.*",
            CheckFileExists = true,
        };

        private readonly SaveFileDialog _saveImageDialog = new SaveFileDialog()
        {
            Title = "Save Image",
            Filter = "PNG Image|*.png|JPEG Image|*.jpg;*.jpeg|Bitmap Image|*.bmp|GIF Image|*.gif|TIFF Image|*.tiff|All Files|*.*",
            CheckPathExists = true,
        };

        private SKBitmap? _sourceBitmap;
        private SKBitmap? _processedBitmap;

        public MainWindow()
        {
            _backgroundProcessThread = new Thread(ProcessThread)
            {
                IsBackground = true,
            };

            DataContext = this;
            InitializeComponent();
        }

        [ObservableProperty]
        private ImageSource? _displaySourceImage;

        [ObservableProperty]
        private ImageSource? _displayProcessedImage;

        [ObservableProperty]
        private bool _isCreateProcessorDialogVisible;

        [ObservableProperty]
        private ImageProcessorInfoCreation? _selectedImageProcessorInfoCreation;

        public ObservableCollection<ImageProcessorInfo> ImageProcessorInfos { get; } = new();

        private void Process()
        {

        }

        private void ProcessThread()
        {
            while (true)
            {
                _semaphoreSlim.Wait();


            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _backgroundProcessThread.Start();
        }

        [RelayCommand]
        private void OpenImage()
        {
            if (_openImageDialog.ShowDialog() != true)
            {
                return;
            }

            _sourceBitmap = SKBitmap.Decode(_openImageDialog.FileName);

            DisplaySourceImage = BitmapSource.Create(
                _sourceBitmap.Width, _sourceBitmap.Height,
                96, 96,
                _sourceBitmap.ColorType.ToWpf(), null,
                _sourceBitmap.GetPixels(), _sourceBitmap.ByteCount, _sourceBitmap.RowBytes);
        }

        [RelayCommand]
        private void OpenCreateProcessorDialog()
        {
            IsCreateProcessorDialogVisible = true;
        }


        [RelayCommand]
        private void CloseCreateProcessorDialog()
        {
            IsCreateProcessorDialogVisible = false;
        }

        [RelayCommand]
        private void CommitCreateProcessorDialog()
        {
            if (SelectedImageProcessorInfoCreation is not null)
            {
                ImageProcessorInfos.Add(SelectedImageProcessorInfoCreation.Instantiate());
            }

            IsCreateProcessorDialogVisible = false;
        }
    }
}