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
using ImageProcessingWpf.Helpers;
using ImageProcessingWpf.Models;
using LibImageProcessing;
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
        private readonly IProgress<int> _processProgress;

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
        private CancellationTokenSource? _processCancellationTokenSource;

        public MainWindow()
        {
            _backgroundProcessThread = new Thread(ProcessThread)
            {
                IsBackground = true,
            };

            _processProgress = new DelegateProgress<int>(processed =>
            {
                if (Dispatcher.CheckAccess())
                {
                    ProgressCurrent = processed;
                }
                else
                {
                    Dispatcher.Invoke(() => ProgressCurrent = processed);
                }
            });

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

        [ObservableProperty]
        private int _progressTotal;

        [ObservableProperty]
        private int _progressCurrent;

        [ObservableProperty]
        private string _statusText = "Ready";

        public ObservableCollection<ImageProcessorInfo> ImageProcessorInfos { get; } = new();

        private void Process(CancellationToken cancellationToken)
        {
            if (_sourceBitmap is null ||
                cancellationToken.IsCancellationRequested)
            {
                return;
            }

            SKSizeI currentSize = new SKSizeI(_sourceBitmap.Width, _sourceBitmap.Height);
            List<IImageProcessor> imageProcessors = new List<IImageProcessor>();

            try
            {
                foreach (var processorInfo in ImageProcessorInfos)
                {
                    var processor = processorInfo.CreateProcessor(currentSize.Width, currentSize.Height);

                    imageProcessors.Add(processor);
                    currentSize = new SKSizeI(processor.OutputWidth, processor.OutputHeight);
                }
            }
            catch
            {
                Dispatcher.Invoke(() =>
                {
                    ProgressTotal = 0;
                    ProgressCurrent = 0;
                    StatusText = "Invalid processor parameter";
                });

                return;
            }

            Dispatcher.Invoke(() =>
            {
                ProgressTotal = imageProcessors.Count;
                ProgressCurrent = 0;
                StatusText = "Processing";
            });

            if (_processedBitmap is null ||
                _processedBitmap.Width != currentSize.Width ||
                _processedBitmap.Height != currentSize.Height)
            {
                if (_processedBitmap is not null)
                {
                    _processedBitmap.Dispose();
                }

                _processedBitmap = new SKBitmap(currentSize.Width, currentSize.Height, SKColorType.Bgra8888, SKAlphaType.Unpremul);
            }

            try
            {
                ImageProcessing.Process(_sourceBitmap, _processedBitmap, _processProgress, imageProcessors);

                Dispatcher.Invoke(() =>
                {
                    ProgressCurrent = ProgressTotal;
                    StatusText = "OK";
                });
            }
            catch
            {
                ProgressTotal = 0;
                ProgressCurrent = 0;
                StatusText = "Failed to process";
            }
        }

        private void ProcessThread()
        {
            while (true)
            {
                while (_semaphoreSlim.CurrentCount > 1)
                {
                    _semaphoreSlim.Wait();
                }

                _semaphoreSlim.Wait();

                _processCancellationTokenSource?.Cancel();
                _processCancellationTokenSource = new CancellationTokenSource();
                Process(_processCancellationTokenSource.Token);

                if (_processedBitmap is null)
                {
                    continue;
                }

                Dispatcher.Invoke(() =>
                {
                    DisplayProcessedImage = BitmapSource.Create(
                        _processedBitmap.Width, _processedBitmap.Height,
                        96, 96,
                        _processedBitmap.ColorType.ToWpf(), null,
                        _processedBitmap.GetPixels(), _processedBitmap.ByteCount, _processedBitmap.RowBytes);
                });
            }
        }

        private void RequestProcess()
        {
            _processCancellationTokenSource?.Cancel();
            _semaphoreSlim.Release();
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

            RequestProcess();
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
                var imageProcessorInfo = SelectedImageProcessorInfoCreation.Instantiate();

                imageProcessorInfo.PropertyChanged += ImageProcessorInfo_PropertyChanged;
                ImageProcessorInfos.Add(imageProcessorInfo);
            }

            IsCreateProcessorDialogVisible = false;
            RequestProcess();
        }

        private void ImageProcessorInfo_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            RequestProcess();
        }
    }
}