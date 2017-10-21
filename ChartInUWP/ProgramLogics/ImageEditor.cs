using ChartInUWP.Models;
using ChartInUWP.ModelServices;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml.Media;
using Microsoft.Graphics.Canvas;
using Windows.Foundation;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Windows.UI;

namespace ChartInUWP
{
  /// <summary>
  /// Program logic, holding pixeldata and manage pixelimage and pixelgraph
  /// </summary>
  public class ImageEditor
  {
    #region Fields

    IPixelDataSource _inputSource;  // input source
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

    public double NumberRows { get { return _chartService.DataRows; } }

    public ImageSource ImageSource { get; private set; }

    public CanvasImageSource CanvasSource { get; private set; }

    public StorageFile StorageFile { get; private set; }

    public MatrixStruct<ushort> ImageMatrix { get; private set; }

    #endregion Properties

    #region Methods

    /// <summary>
    /// Load a DICOM file
    /// </summary>
    /// <returns></returns>
    public async Task<string> OpenDicomFileAsync()
    {
      _inputSource = new DicomModel();
      if (await _inputSource.OpenFileAsync()) // open
      {
        StorageFile = _inputSource.File;
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
      _inputSource = new ImageModel();
      if (await _inputSource.OpenFileAsync())
      {
        StorageFile = _inputSource.File;
        return StorageFile.DisplayName;
      }
      return default;
    }

    public async Task<ImageSource> GetImageSourceAsync()
    {
      return await _inputSource.GetImageSourceAsync();
    }

    public async Task LoadChartDataAsync()
    {
      var imgShorts = await _inputSource.GetPixelShortsAsync();
      var mtxFloats = await _inputSource.GetPixelFloatsAsync();
      // load float image data
      var fourier = new FourierService(mtxFloats);

      // load complex numbers
      // calc fourier lines
      // get magnitude
      await fourier.LoadGraphDataAsync();

      var magnitude = fourier.GetMatrixMagnitude();   // half width

      // needs data and drawsession
      _chartService.LoadChartData(magnitude);
      //renderer.
      // todo
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
