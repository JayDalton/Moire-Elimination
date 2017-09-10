using Microsoft.Graphics.Canvas.UI.Xaml;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media;

namespace ChartInUWP
{
  public class ChartViewModel : ViewModelBase
  {
    #region Fields

    private bool progressing = false;
    private ImageSource _imageSource;

    private ChartService _chartService;

    private CanvasControl _canvasControl;
    private ChartRenderer _chart;
    private ImageRenderer _image;

    #endregion Fields

    public ChartViewModel(CanvasControl canvasControl)
    {
      _canvasControl = canvasControl;
      _chart = new ChartRenderer();
      _image = new ImageRenderer();
    }

    #region Properties

    public bool Progressing
    {
      get { return progressing; }
      set { SetProperty(ref progressing, value); }
    }

    public ImageSource ImageSource
    {
      get { return _imageSource; }
      set { SetProperty(ref _imageSource, value); }
    }

    private bool _isImageVisible;
    public bool IsImageVisible
    {
      get { return _isImageVisible; }
      set { SetProperty(ref _isImageVisible, value); }
    }

    private bool _isChartVisible;
    public bool IsChartVisible
    {
      get { return _isChartVisible; }
      set { SetProperty(ref _isChartVisible, value); }
    }

    double _chartScaleRow = default(double);
    public double ChartScaleRowSlider
    {
      get { return _chartScaleRow; }
      set
      {
        if (SetProperty(ref _chartScaleRow, value))
        {
          _canvasControl.Invalidate();
        }
      }
    }

    double _chartScaleCol = default(double);
    public double ChartScaleColSlider
    {
      get { return _chartScaleCol; }
      set
      {
        if (SetProperty(ref _chartScaleCol, value))
        {
          _canvasControl.Invalidate();
        }
      }
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

    private double _chartMoveCol = default(double);
    public double ChartMoveColSlider
    {
      get { return _chartMoveCol; }
      set
      {
        if (SetProperty(ref _chartMoveCol, value))
        {
          _canvasControl.Invalidate();
        }
      }
    }

    #endregion Properties

    #region Events

    public void RenderChartCanvas(CanvasControl sender, CanvasDrawEventArgs args)
    {
      _chart.OnDrawGraph(sender, args);
    }

    public async Task LoadDicomFile_Click()
    {
      await _chart.LoadDicomFileAsync();
      ImageSource = _chart.ImageSource;
      IsChartVisible = false;
      IsImageVisible = true;
    }

    public async Task LoadChartData_Click()
    {
      await _chart.LoadChartDataFromDicomFileAsync();
      _canvasControl.Invalidate();
      IsImageVisible = false;
      IsChartVisible = true;
    }

    public async Task LoadPackedDataFile_Click()
    {
      await _chart.LoadChartDataFromPackedFileAsync();
      _canvasControl.Invalidate();
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
