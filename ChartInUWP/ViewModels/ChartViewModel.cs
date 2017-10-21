using ChartInUWP.ViewModels.Commands;
using MathNet.Numerics;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI.Xaml;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml.Media;

namespace ChartInUWP.ViewModels
{
  public class ChartViewModel : BaseNotification
  {
    #region Fields

      ImageEditor _editor;

    #endregion Fields

    public ChartViewModel()
    {
      _editor = new ImageEditor();
      if (DesignMode.DesignModeEnabled)
      {
        ImageDisplayName = "Design Mode";
      }
    }

    #region Properties

    ImageSource _imageSource;
    public ImageSource ImageSource
    {
      get { return _imageSource; }
      set { SetProperty(ref _imageSource, value); }
    }

    CanvasImageSource _canvasSource;
    public CanvasImageSource CanvasSource
    {
      get { return _canvasSource; }
      set { SetProperty(ref _canvasSource, value); }
    }

    string _contentTitle;
    public string ImageDisplayName
    {
      get { return _contentTitle ?? "Select file"; }
      set { SetProperty(ref _contentTitle, value); }
    }

    bool _imageProgressing = false;
    public bool ImageProgressing
    {
      get { return _imageProgressing; }
      set { SetProperty(ref _imageProgressing, value); }
    }

    bool _chartProgressing = false;
    public bool ChartProgressing
    {
      get { return _chartProgressing; }
      set { SetProperty(ref _chartProgressing, value); }
    }

    double _rangeMinimum = 0;
    public double SliderRangeMinimum
    {
      get { return _rangeMinimum; }
      set { SetProperty(ref _rangeMinimum, value); }
    }

    double _rangeMaximum = 100;
    public double SliderRangeMaximum
    {
      get { return _rangeMaximum; }
      set { SetProperty(ref _rangeMaximum, value); }
    }

    float _rangeValue = default;
    public float SliderRangeValue
    {
      get { return _rangeValue; }
      set
      {
        if (SetProperty(ref _rangeValue, value))
        {
          RenderThisLineChart((int)value);
        }
      }
    }

    #endregion Properties

    #region Commands

    public ICommand LoadDicomCommand => new DelegateCommand(loadDicomFileAsync);
    public ICommand AnalyzingCommand => new DelegateCommand(analysingChartAsync);

    public ICommand PrevLineCommand => new DelegateCommand(RenderPrevLineChart, () => 0 < _editor.CurrentRow);
    public ICommand NextLineCommand => new DelegateCommand(RenderNextLineChart, () => _editor.CurrentRow < _editor.NumberRows);

    public ICommand FilteringCommand => new ActionCommand(Test);
    public ICommand SaveDicomCommand => new ActionCommand(Test);    

    #endregion Commands

    #region Methods

    private void Test()
    {
    }

    private void RenderPrevLineChart()
    {
      if (0 < _editor.CurrentRow)
      {
        _editor.RenderChartLine(_editor.CurrentRow - 1);
      }
    }

    private void RenderNextLineChart()
    {
      if (_editor.CurrentRow < _editor.NumberRows)
      {
        _editor.RenderChartLine(_editor.CurrentRow + 1);
      }
    }

    private void RenderThisLineChart(int line)
    {
      if (0 <= line && line < _editor.NumberRows)
      {
        _editor.RenderChartLine(line);
      }
    }

    private async void loadDicomFileAsync()
    {
      ImageDisplayName = await _editor.OpenDicomFileAsync();
      ImageProgressing = true;
      ImageSource = await _editor.GetImageSourceAsync();
      CanvasSource = _editor.CanvasSource;
      ImageProgressing = false;
    }

    private async void analysingChartAsync()
    {
      CanvasSource = _editor.CanvasSource;

      ChartProgressing = true;
      await _editor.LoadChartDataAsync();
      ChartProgressing = false;

      //SliderRangeMaximum = _editor.NumberRows;
      _editor.RenderChartLine(0);
    }

    #endregion Methods
  }

}
