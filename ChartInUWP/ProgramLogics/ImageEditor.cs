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

namespace ChartInUWP
{
  /// <summary>
  /// Program logic, holding pixeldata and manage pixelimage and pixelgraph
  /// </summary>
  public class ImageEditor
  {
    #region Fields

    IPixelSource _pixelSource;  // pixel source
    ChartService _chartService; // render chart
    FourierService _fourierService;

    #endregion Fields

    public ImageEditor()
    {
      CanvasDevice device = CanvasDevice.GetSharedDevice();
      CanvasSource = new CanvasImageSource(device, 800, 600, 96);
      _chartService = new ChartService(CanvasSource);
    }

    #region Properties

    public int CurrentRow { get; set; }

    public double NumberRows { get { return _chartService.DataLineCount; } }

    public StorageFile StorageFile { get; private set; }

    public ImageSource ImageSource { get; private set; }

    public CanvasImageSource CanvasSource { get; private set; }

    public BitmapMatrix<ushort> ImageMatrix { get; private set; }

    #endregion Properties

    #region Methods

    /// <summary>
    /// Load a DICOM file
    /// </summary>
    /// <returns></returns>
    public async Task<string> OpenDicomFileAsync()
    {
      _pixelSource = new DicomModel();
      if (await _pixelSource.OpenFileAsync()) // open
      {
        StorageFile = _pixelSource.File;
        return StorageFile.DisplayName;
      }
      return default;
    }

    /// <summary>
    /// Load a Image file
    /// </summary>
    /// <returns></returns>
    public async Task<string> OpenImageFileAsync()
    {
      _pixelSource = new ImageModel();
      if (await _pixelSource.OpenFileAsync())
      {
        StorageFile = _pixelSource.File;
        return StorageFile.DisplayName;
      }
      return default;
    }

    public async Task<ImageSource> GetImageSourceAsync()
    {
      return await _pixelSource.GetImageSourceAsync();
    }

    public async Task LoadChartDataAsync()
    {
      var fourier = new FourierService(_pixelSource);

      var adding = Task.Factory.StartNew(() => 
      {
        foreach (var (idx, values) in fourier.Magnitudes)
        {
          _chartService.AddDataLine(idx, values);
        }
      });

      await fourier.StartAnalysingAsync();
    }

    public void RenderChartLine(int line)
    {
      _chartService.RenderChartRow(line);
    }

    public void RenderChart(double row, Size size, CanvasDrawingSession ds)
    {
      if (_chartService != null)
      {
        _chartService.RenderChartRow((int)row, size, ds);
      }
    }

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
