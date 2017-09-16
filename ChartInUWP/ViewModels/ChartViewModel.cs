using MathNet.Numerics;
using Microsoft.Graphics.Canvas.UI.Xaml;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media;

namespace ChartInUWP
{
  public class ChartViewModel : BaseNotification
  {
    #region Fields

    double _numberOfRows = 0;
    bool imageProgressing = false;
    bool chartProgressing = false;
    ImageSource _imageSource;
    ChartRenderer _chart;

    private ChartService _chartService;

    private CanvasControl _canvasControl;
    private ImageLoader _image;

    #endregion Fields

    public ChartViewModel(/*CanvasControl canvasControl*/)
    {
      //_canvasControl = canvasControl;
      _chart = new ChartRenderer();
      _image = new ImageLoader();
    }

    #region Properties

    public bool ImageProgressing
    {
      get { return imageProgressing; }
      set { SetProperty(ref imageProgressing, value); }
    }

    public bool ChartProgressing
    {
      get { return chartProgressing; }
      set { SetProperty(ref chartProgressing, value); }
    }

    public double NumberOfRows
    {
      get { return _numberOfRows; }
      set { SetProperty(ref _numberOfRows, value); }
    }

    public ImageSource ImageSource
    {
      get { return _imageSource; }
      set { SetProperty(ref _imageSource, value); }
    }

    public double ChartMoveRowSlider
    {
      get { return _chart.CurrentRow; }
      set
      {
        if (SetProperty(_chart.CurrentRow, value, () => _chart.CurrentRow = (int)value))
        {
          _canvasControl.Invalidate();
        }
      }
    }

    #endregion Properties

    #region Events

    public void RenderChartCanvas(CanvasControl sender, CanvasDrawEventArgs args)
    {
      _canvasControl = sender;
      _chart.OnDrawGraph(sender, args);
    }

    public async Task LoadDicomFile_Click()
    {
      await _chart.LoadDicomFileAsync();
      ImageProgressing = true;
      ImageSource = _chart.GetDicomImage();
      ImageProgressing = false;
    }

    public async Task LoadPackedFile_Click()
    {
      await _chart.LoadChartDataAsync();
      _canvasControl.Invalidate();
    }

    public async Task LoadChartData_Click()
    {
      ChartProgressing = true;
      await _chart.LoadChartDataAsync();
      NumberOfRows = _chart.NumberOfRows;
      _canvasControl.Invalidate();
      ChartProgressing = false;
    }

    #endregion Events

    #region Methods

    //private void GraphMoveY_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
    //{
    //  GraphCanvas.Invalidate();
    //}

    //private void GraphMoveYIncrease_Click(object sender, RoutedEventArgs e)
    //{
    //  if (GraphMoveY.Value < GraphMoveY.Maximum)
    //  {
    //    GraphMoveY.Value++; 
    //  }
    //}

    //private void GraphMoveYDecrease_Click(object sender, RoutedEventArgs e)
    //{
    //  if (0 < GraphMoveY.Value)
    //  {
    //    GraphMoveY.Value--;
    //  }
    //}

    //private void GraphScaleY_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
    //{
    //  GraphCanvas.Invalidate();
    //}

    //private void GraphScaleYDecrease_Click(object sender, RoutedEventArgs e)
    //{
    //  if (0 < GraphScaleY.Value)
    //  {
    //    GraphScaleY.Value--;
    //  }
    //}

    //private void GraphScaleYIncrease_Click(object sender, RoutedEventArgs e)
    //{
    //  if (GraphScaleY.Value < GraphScaleY.Maximum)
    //  {
    //    GraphScaleY.Value++;
    //  }
    //}

    //private void GraphScaleX_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
    //{
    //  var x = e.NewValue;

    //  GraphCanvas.Invalidate();
    //}

    //private void GraphScaleXIncrease_Click(object sender, RoutedEventArgs e)
    //{
    //  if (GraphScaleX.Value < GraphScaleX.Maximum)
    //  {
    //    GraphScaleX.Value++;
    //  }
    //}

    //private void GraphScaleXDecrease_Click(object sender, RoutedEventArgs e)
    //{
    //  if (0 < GraphScaleX.Value)
    //  {
    //    GraphScaleX.Value--;
    //  }
    //}

    //private void GraphMoveX_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
    //{
    //  GraphCanvas.Invalidate();
    //}

    //private void GraphMoveXIncrease_Click(object sender, RoutedEventArgs e)
    //{
    //  if (GraphMoveX.Value < GraphMoveX.Maximum)
    //  {
    //    GraphMoveX.Value++;
    //  }
    //}

    //private void GraphMoveXDecrease_Click(object sender, RoutedEventArgs e)
    //{
    //  if (0 < GraphMoveX.Value)
    //  {
    //    GraphMoveX.Value--;
    //  }
    //}

    //private async void LoadDataButton_Click(object sender, RoutedEventArgs e)
    //{
    //  GraphCanvas.Visibility = Visibility.Visible;
    //  GreyImageGrid.Visibility = Visibility.Collapsed;
    //  await viewModel.LoadChartMatrixFile();
    //  //await ReadBinaryData();
    //  //await ReadInputData();
    //}

    //private async void LoadImageButton_Click(object sender, RoutedEventArgs e)
    //{
    //  GraphCanvas.Visibility = Visibility.Collapsed;
    //  GreyImageGrid.Visibility = Visibility.Visible;
    //  await viewModel.RenderRawImage();
    //  //await RenderRawImage();
    //}



    #endregion Methods
  }

}
