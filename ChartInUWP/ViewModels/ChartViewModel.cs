using ChartInUWP.ModelServices;
using ChartInUWP.ViewModels.Commands;
using MathNet.Numerics;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI.Xaml;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
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

      var device = CanvasDevice.GetSharedDevice();
      ImageSource = new CanvasRenderTarget(device, 1000, 1000, 96);
      //var service = new ImageService(ImageSource);
      //service.ClearScreen(Colors.Orange);
      using (var session = ImageSource.CreateDrawingSession())
      {
        session.Clear(Colors.Orange);
      }

      CanvasImageSource cis = new CanvasImageSource(device, 1000, 1000, 96);
      

      ImageTarget = new CanvasImageSource(device, 1000, 1000, 96);
      using (var session = ImageTarget.CreateDrawingSession(Colors.Blue))
      {
        session.DrawImage(ImageSource);
      }

      FlipSource = new List<CanvasImageSource>();
      FlipSource.Add(ImageTarget);
    }

    #region Properties

    private List<CanvasImageSource> _flipSource;
    public List<CanvasImageSource> FlipSource
    {
      get { return _flipSource; }
      set { SetProperty(ref _flipSource, value); }
    }

    CanvasRenderTarget _imageSource;
    public CanvasRenderTarget ImageSource
    {
      get { return _imageSource; }
      set { SetProperty(ref _imageSource, value); }
    }

    CanvasImageSource _imageTarget;
    public CanvasImageSource ImageTarget
    {
      get { return _imageTarget; }
      set { SetProperty(ref _imageTarget, value); }
    }

    CanvasRenderTarget _chartTarget;
    public CanvasRenderTarget ChartTarget
    {
      get { return _chartTarget; }
      set { SetProperty(ref _chartTarget, value); }
    }

    private bool _imageVisible = true;
    public bool ImageVisible
    {
      get { return _imageVisible; }
      set { SetProperty(ref _imageVisible, value); }
    }

    private bool _canvasVisible = true;
    public bool ChartVisible
    {
      get { return _canvasVisible; }
      set { SetProperty(ref _canvasVisible, value); }
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

    public ICommand LoadDicomCommand => new DelegateCommand(openDicomFileAsync);
    public ICommand AnalyzingCommand => new DelegateCommand(analysingChartAsync);

    public ICommand PrevLineCommand => new DelegateCommand(RenderPrevLineChart, () => 0 < _editor.CurrentRow);
    public ICommand NextLineCommand => new DelegateCommand(RenderNextLineChart, () => _editor.CurrentRow < _editor.StorageSize.Height);

    public ICommand DisplayImageCommand => new DelegateCommand(() => ImageVisible = ImageVisible ? false : true);
    public ICommand DisplayChartCommand => new DelegateCommand(() => ChartVisible = ChartVisible ? false : true);

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
      if (_editor.CurrentRow < _editor.StorageSize.Height)
      {
        _editor.RenderChartLine(_editor.CurrentRow + 1);
      }
    }

    private void RenderThisLineChart(int line)
    {
      if (0 <= line && line < _editor.StorageSize.Height)
      {
        _editor.RenderChartLine(line);
      }
    }

    private async void openDicomFileAsync()
    {
      if (await _editor.OpenDicomFileAsync())
      {
        ImageDisplayName = $"{_editor.StorageFile.DisplayName} ({_editor.StorageSize})";
        ImageProgressing = true;
        await _editor.LoadImageSourceAsync();
        //await _editor.LoadChartTargetAsync();
        ImageProgressing = false;
        ImageSource = _editor.ImageSource;
        //ImageTarget = _editor.ImageTarget;
      }
    }

    private async void openImageFileAsync()
    {
      if (await _editor.OpenImageFileAsync())
      {
        ImageDisplayName = _editor.StorageFile.DisplayName;
        ImageProgressing = true;
        await _editor.LoadImageSourceAsync();
        ImageProgressing = false;
        //ImageTarget = _editor.ImageTarget;
      }
    }

    private async void analysingChartAsync()
    {
      ChartTarget = _editor.ChartTarget;

      ChartProgressing = true;
      await _editor.LoadChartDataAsync();
      ChartProgressing = false;

      //SliderRangeMaximum = _editor.NumberRows;
      _editor.RenderChartLine(0);
    }

    #endregion Methods
  }

}
