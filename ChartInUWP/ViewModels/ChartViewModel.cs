using MathNet.Numerics;
using Microsoft.Graphics.Canvas.UI.Xaml;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml.Media;

namespace ChartInUWP
{
  public class ChartViewModel : BaseNotification
  {
    #region Fields

    double _currentRow = 0;
    double _numberOfRows = 0;
    bool imageProgressing = false;
    bool chartProgressing = false;
    CanvasControl _canvasControl;
    ImageSource _imageSource;
    ImageEditor _editor;

    #endregion Fields

    public ChartViewModel()
    {
      _editor = new ImageEditor();
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
      get { return _currentRow; }
      set
      {
        if (SetProperty(ref _currentRow, value))
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
      
      // currentRow, args.DrawingSession, Size()
      _editor.RenderChart(
        _currentRow, 
        new Size(sender.ActualHeight, sender.ActualWidth), 
        args.DrawingSession
      );
    }

    public async Task LoadDicomFile_Click()
    {
      await _editor.LoadDicomFileAsync();
      ImageProgressing = true;
      ImageSource = await _editor.GetDicomImageAsync();
      ImageProgressing = false;
    }

    public async Task LoadPackedFile_Click()
    {
      await _editor.LoadChartDataAsync();
      _canvasControl.Invalidate();
    }

    public async Task LoadChartData_Click()
    {
      ChartProgressing = true;
      await _editor.LoadChartDataAsync();
      NumberOfRows = _editor.NumberOfRows;
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
