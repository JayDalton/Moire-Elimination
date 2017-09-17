using ChartInUWP.Models;
using ChartInUWP.ModelServices;
using Microsoft.Graphics.Canvas.Brushes;
using Microsoft.Graphics.Canvas.Geometry;
using Microsoft.Graphics.Canvas.Text;
using Microsoft.Graphics.Canvas.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI;
using Windows.UI.Xaml.Media;
using Microsoft.Graphics.Canvas;
using Windows.Foundation;

namespace ChartInUWP
{
  public class ImageEditor
  {
    #region Fields

    IPixelDataSource _inputSource;  // input source
    ChartService _chartService; // render chart
    FourierService _fourierService;

    #endregion Fields

    public ImageEditor()
    {
      _chartService = new ChartService();
    }

    #region Properties

    public int CurrentRow { get; set; }

    public double NumberOfRows { get { return _chartService.DataRows; } }

    public ImageSource ImageSource { get; private set; }

    public MatrixStruct<ushort> ImageMatrix { get; private set; }

    #endregion Properties

    #region Methods

    public async Task LoadDicomFileAsync()
    {
      _inputSource = new DicomModel();
      if (await _inputSource.OpenFileAsync())
      {
        var imgSource = await _inputSource.GetImageSourceAsync();
        var imgMatrix = await _inputSource.GetPixelDataAsync();
      }
    }

    public async Task LoadImageFileAsync()
    {
      _inputSource = new ImageModel();
      if (await _inputSource.OpenFileAsync())
      {
        var imgSource = await _inputSource.GetImageSourceAsync();
        var imgMatrix = await _inputSource.GetPixelDataAsync();
      }
    }

    public void RenderChart(double row, Size size, CanvasDrawingSession ds)
    {
      _chartService.RenderChartRow(row, size, ds);
    }

    private void handleMatrxToChart(MatrixStruct<ushort> matrix)
    {
      var fourier = new FourierService(_inputSource);
      var magnitude = fourier.GetMatrixMagnitude();   // half width
      var renderer = new ChartService();
      //renderer
    }

    public async Task LoadMatrixFileAsync()
    {
      // formats: pgm + MessagePack<matrix>
    }

    public async Task<ImageSource> GetDicomImageAsync()
    {
      return await _inputSource.GetImageSourceAsync();
    }

    public async Task LoadChartDataAsync()
    {
      var imgMatrix = await _inputSource.GetPixelDataAsync();
    }

    #endregion Methods
  }
}
