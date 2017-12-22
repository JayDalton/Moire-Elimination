using ChartInUWP.Models;
using ChartInUWP.ModelServices;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml.Media;
using Microsoft.Graphics.Canvas;
using Windows.Foundation;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Windows.UI;
using ChartInUWP.Interfaces;
using Windows.Graphics.Imaging;
using Windows.UI.Xaml.Media.Imaging;
using System.Collections.Concurrent;
using System;
using ChartWRCLibrary;
using System.Linq;
using System.Diagnostics;
using ChartWRCLibrary.Converter;

namespace ChartInUWP
{
  /// <summary>
  /// Program logic, holding pixeldata and manage pixelimage and pixelgraph
  /// </summary>
  public class ImageEditor
  {
    #region Fields

    IPixelSource _pixelSource;  // pixel source
    CanvasDevice _renderDevice; // render device
    ChartService _chartService; // render chart
    ImageService _imageService; // render image
    FftHelper _fourierHelp;
    Class1 _class1;
    RatingConverter _converter;

    #endregion Fields

    public ImageEditor()
    {
      _class1 = new Class1();
      _fourierHelp = new FftHelper();
      _renderDevice = CanvasDevice.GetSharedDevice();
      var pixelSize = _renderDevice.MaximumBitmapSizeInPixels;
      var cacheSize = _renderDevice.MaximumCacheSize;

      //ChartTarget = new CanvasRenderTarget(_renderDevice, 800, 800, 96);
      //ImageTarget = new CanvasRenderTarget(_renderDevice, 4320, 4320, 96);
      //_chartService = new ChartService(ChartTarget);
      //_imageService = new ImageService(ImageTarget);
    }

    #region Properties

    public int CurrentRow { get; private set; }

    public Size StorageSize { get; private set; }

    public StorageFile StorageFile { get; private set; }

    public CanvasRenderTarget ImageSource { get; private set; }

    public CanvasRenderTarget ImageTarget { get; private set; }

    public CanvasRenderTarget ChartTarget { get; private set; }

    #endregion Properties

    #region Methods

    /// <summary>
    /// Load a DICOM file
    /// </summary>
    /// <returns></returns>
    public async Task<bool> OpenDicomFileAsync()
    {
      _pixelSource = new DicomModel();
      if (await _pixelSource.OpenFileAsync()) // open
      {
        StorageSize = new Size(_pixelSource.Width, _pixelSource.Height);
        StorageFile = _pixelSource.File;
        return true;
      }
      return false;
    }

    /// <summary>
    /// Load a Image file
    /// </summary>
    /// <returns></returns>
    public async Task<bool> OpenImageFileAsync()
    {
      _pixelSource = new ImageModel();
      if (await _pixelSource.OpenFileAsync())
      {
        StorageSize = new Size(_pixelSource.Width, _pixelSource.Height);
        StorageFile = _pixelSource.File;
        return true;
      }
      return false;
    }

    public async Task LoadImageSourceAsync()
    {
      var temp = _pixelSource.GetContentAsUShort();

      var old = await _class1.GetPrimesOrdered(3, 2100000);

      var res = _fourierHelp.SetContent(_pixelSource.Width, _pixelSource.Height, temp.ToList());

      foreach (var (values, line) in res.Select((v, i) => (v, i)))
      {
        Debug.WriteLine($"key: {line} with {values.Count}");
        foreach (var item in values.Take(10))
        {
          Debug.Write($"{item:0.00} ");
        }
        Debug.WriteLine("");
      }

      var image = await _pixelSource.GetBytesMatrixAsync();

      ImageSource = new CanvasRenderTarget(_renderDevice, image.Cols, image.Rows, 96);
      _imageService = new ImageService(ImageSource);
      _imageService.ClearScreen(Colors.Orange);
    }

    public async Task LoadImageTargetAsync()
    {
      var image = await _pixelSource.GetBytesMatrixAsync();
      ImageTarget = new CanvasRenderTarget(_renderDevice, image.Cols, image.Rows, 96);
      _imageService = new ImageService(ImageTarget);
      _imageService.ClearScreen(Colors.Violet);
    }

    public async Task LoadChartTargetAsync()
    {

    }

    public async Task LoadChartDataAsync()
    {
      var fourier = new FourierService(_pixelSource);

      var adding = Task.Factory.StartNew(() => 
      {
        foreach (var (idx, values) in fourier.Magnitudes)
        {
          _chartService.AddLineValues(idx, values);
          _imageService.AddLineValues(idx, values);
        }
      });

      await fourier.StartAnalysingAsync();
    }

    public void RenderChartLine(int line)
    {
      _chartService.RenderChartRow(line);
    }

    //public void RenderChart(double row, Size size, CanvasDrawingSession ds)
    //{
    //  if (_chartService != null)
    //  {
    //    _chartService.RenderChartRow((int)row, size, ds);
    //  }
    //}

    //public async Task LoadMatrixFileAsync()
    //{
    //  // formats: pgm + MessagePack<matrix>
    //}

    //private void handleMatrxToChart(MatrixStruct<ushort> matrix)
    //{
    //  var fourier = new FourierService(_inputSource);
    //  var magnitude = fourier.GetMatrixMagnitude();   // half width
    //  var renderer = new ChartService(magnitude);
    //  //renderer
    //}

    #endregion Methods
  }
}
