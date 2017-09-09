using MessagePack;
using Microsoft.Graphics.Canvas.UI.Xaml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace ChartInUWP
{
  public class ChartViewModel : ViewModelBase
  {
    #region Fields

    private bool progressing = false;

    private ChartService _chartService;

    private CanvasControl _canvasControl;
    private ChartRenderer _chartRenderer;
    private ImageRenderer _imageRenderer;

    #endregion Fields

    public ChartViewModel(CanvasControl canvasControl)
    {
      _canvasControl = canvasControl;
      _chartRenderer = new ChartRenderer();
      _imageRenderer = new ImageRenderer();
    }

    #region Properties

    public bool Progressing
    {
      get { return progressing; }
      set { SetProperty(ref progressing, value); }
    }

    double _chartScaleRow = default(double);
    public double ChartScaleRow
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
    public double ChartScaleCol
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

    private double _chartMoveRow = default(double);
    public double ChartMoveRow
    {
      get { return _chartMoveRow; }
      set
      {
        if (SetProperty(ref _chartMoveRow, value))
        {
          _canvasControl.Invalidate();
        }
      }
    }

    private double _chartMoveCol = default(double);
    public double ChartMoveCol
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
      var x = 0;
    }

    public async Task LoadChartDataFile_Click()
    {
      await _chartRenderer.LoadChartDataFromFile();
    }

    public async Task LoadImageDataFile_Click()
    {
      await _imageRenderer.LoadImageDataFromFile();
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
