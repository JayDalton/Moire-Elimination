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

      CanvasDevice device = CanvasDevice.GetSharedDevice();
      CanvasSource = new CanvasImageSource(device, 300, 300, 96);

      if (DesignMode.DesignModeEnabled)
      {

        ImageDisplayName = "Design Mode";
      }
      else
      {
        ImageDisplayName = "Datei xyz; rows: ... cols: ...";
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
      get { return _contentTitle; }
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
          Debug.WriteLine(value);
          RenderTest((int)value);
          //_canvasControl.Invalidate();
        }
      }
    }

    #endregion Properties

    #region Commands

    public ICommand LoadDicomCommand => new DelegateCommand(async () => {
      await _editor.LoadDicomFileAsync();
      ImageProgressing = true;
      ImageDisplayName = _editor.StorageFile.DisplayName;
      ImageSource = await _editor.GetImageSourceAsync();
      ImageProgressing = false;
    });

    public ICommand AnalyzingCommand => new DelegateCommand(async () => {
      ChartProgressing = true;
      CanvasSource = await _editor.GetCanvasSourceAsync();
      await _editor.LoadChartDataAsync();
      SliderRangeMaximum = _editor.NumberOfRows;
      _editor.RenderChartLine(0);
      ChartProgressing = false;
    });

    public ICommand FilteringCommand => new ActionCommand(Test);
    public ICommand SaveDicomCommand => new ActionCommand(Test);
    public ICommand NextLineCommand => new ActionCommand(Test);

    #endregion Commands

    #region Methods

    private void Test()
    {

    }

    private void RenderTest(int line)
    {
      _editor.RenderChartLine(line);
      //using (CanvasDrawingSession ds = _canvasSource.CreateDrawingSession(Colors.Black))
      //{
      //  ds.FillRectangle(20 + line, 30 + line, 5, 6, Colors.Blue);
      //}
    }

    private void RenderChartCanvas(CanvasControl sender, CanvasDrawEventArgs args)
    {
      var size = new Size(sender.ActualHeight, sender.ActualWidth);
      _editor.RenderChart(_rangeValue, size, args.DrawingSession);
    }

    //private async void loadDicomFileAsync()
    //{
    //  await _editor.LoadDicomFileAsync();
    //  ImageDisplayName = _editor.StorageFile.DisplayName;
    //  ImageProgressing = true;
    //  ImageSource = await _editor.GetImageSourceAsync();
    //  ImageProgressing = false;
    //}

    //private async void loadChartDataAsync()
    //{
    //  ChartProgressing = true;
    //  await _editor.LoadChartDataAsync();
    //  SliderRangeMaximum = _editor.NumberOfRows;
    //  ChartProgressing = false;
    //}

    #endregion Methods
  }

}
