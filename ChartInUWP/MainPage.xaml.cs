﻿using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Text;
using Microsoft.Graphics.Canvas.UI.Xaml;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// Die Elementvorlage "Leere Seite" wird unter https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x407 dokumentiert.

namespace ChartInUWP
{
  /// <summary>
  /// Eine leere Seite, die eigenständig verwendet oder zu der innerhalb eines Rahmens navigiert werden kann.
  /// </summary>
  public sealed partial class MainPage : Page
  {
    public MainPage()
    {
      this.InitializeComponent();
      Scenarios = new List<Scenario>();
    }

    public List<Scenario> Scenarios { get; private set; }

    private void OnDrawGraph(CanvasControl sender, CanvasDrawEventArgs args)
    {
      var graphDrawer = new GraphDrawer((float)sender.ActualWidth, (float)sender.ActualHeight, Scenarios, e => e.CpuTimeInMs, "CPU");
      graphDrawer.Draw(args.DrawingSession);
    }
  }


  public sealed class Scenario : INotifyPropertyChanged
  {
    public enum DrawMethod
    {
      Win2DSpriteBatch,
      CppWin2DSpriteBatch,
      D2DSpriteBatch,
      DrawImage
    }

    public DrawMethod Method { get; private set; }
    public CanvasSpriteSortMode SortMode { get; private set; }

    public CanvasImageSource Image { get; private set; }

    static Color[] colors =
    {
                Colors.Red,
                Colors.Green,
                Colors.Blue,
                Colors.DarkGoldenrod,
                Colors.Purple,
                Colors.DarkOliveGreen
            };

    public SolidColorBrush Color
    {
      get
      {
        int index = (int)Method + (int)SortMode * Enum.GetValues(typeof(DrawMethod)).Length;
        return new SolidColorBrush(colors[index % colors.Length]);
      }
    }

    public string Name
    {
      get
      {
        string n;

        switch (Method)
        {
          case DrawMethod.Win2DSpriteBatch: n = "C# Win2D"; break;
          case DrawMethod.CppWin2DSpriteBatch: n = "C++ Win2D"; break;
          case DrawMethod.D2DSpriteBatch: n = "C++ D2D"; break;
          case DrawMethod.DrawImage: n = "C# DrawImage"; break;
          default: throw new Exception();
        }

        if (SortMode == CanvasSpriteSortMode.Bitmap)
          n = n + " (sorted)";

        return n;
      }
    }

    public string Summary
    {
      get
      {
        if (Data == null || Data.Count == 0)
          return "no data";

        return string.Format("CPU/GPU avg: {0:0.00}ms/{1:0.00}ms",
            Data.Values.Select(e => e.CpuTimeInMs).Average(),
            Data.Values.Where(e => e.GpuTimeInMs >= 0.0).Select(e => e.GpuTimeInMs).Average());
      }
    }

    public Dictionary<int, CpuGpuTime> Data { get; private set; }

    public event PropertyChangedEventHandler PropertyChanged;

    public Scenario(DrawMethod method, CanvasSpriteSortMode sortMode)
    {
      Method = method;
      SortMode = sortMode;
      Reset();
    }

    //IScenarioRunner MakeScenarioRunner()
    //{
    //  switch (Method)
    //  {
    //    case DrawMethod.Win2DSpriteBatch:
    //    case DrawMethod.DrawImage:
    //      return new Win2DScenarioRunner(Method);

    //    case DrawMethod.CppWin2DSpriteBatch:
    //      return new CppWin2DScenarioRunner();

    //    case DrawMethod.D2DSpriteBatch:
    //      return new Direct2DScenarioRunner();

    //    default:
    //      throw new Exception();
    //  }
    //}

    public struct CpuGpuTime
    {
      public double CpuTimeInMs;
      public double GpuTimeInMs;
    }

    //internal async Task Go(CancellationToken cancellationToken, CanvasDevice device, ScenarioData scenarioData)
    //{
    //  var runner = MakeScenarioRunner();
    //  scenarioData.AddDataToScenarioRunner(runner);

    //  // Create the image source and the render target.  These sizes are hardcoded and independent of the 
    //  // display's DPI since we just want a small image to convince ourselves that the scenarios really are 
    //  // rendering the right thing.
    //  Image = new CanvasImageSource(device, 128, 128, 96);

    //  using (var rt = new CanvasRenderTarget(device, 128, 128, 96))
    //  {
    //    // We actually run the scenario on the thread pool - XAML really falls down if you overload the UI thread.
    //    var time = await Task.Run(() =>
    //    {
    //      // Run the scenario multiple times to try and avoid too much noise
    //      List<CpuGpuTime> times = new List<CpuGpuTime>();

    //      for (int i = 0; i < 10; ++i)
    //      {
    //        // Hold the device lock while we run the scenario - this prevents other threads
    //        // from interacting with the device and interfering with our recorded time.
    //        using (var deviceLock = device.Lock())
    //        {
    //          times.Add(RunScenario(runner, rt));
    //        }
    //        if (cancellationToken.IsCancellationRequested)
    //          return new CpuGpuTime();
    //      }

    //      var cpuTimes = from entry in times select entry.CpuTimeInMs;
    //      var gpuTimes = from entry in times select entry.GpuTimeInMs;

    //      return new CpuGpuTime
    //      {
    //        CpuTimeInMs = GetAverage(cpuTimes),
    //        GpuTimeInMs = GetAverage(gpuTimes)
    //      };
    //    }, cancellationToken);

    //    Data[scenarioData.Value] = time;

    //    if (cancellationToken.IsCancellationRequested)
    //      return;

    //    using (var ds = Image.CreateDrawingSession(Colors.Transparent))
    //    {
    //      ds.DrawImage(rt);

    //      var timing = string.Format("{0:0.00}ms\n{1:0.00}ms", time.CpuTimeInMs, time.GpuTimeInMs);
    //      ds.DrawText(timing, 0, 0, Colors.White);
    //      ds.DrawText(timing, 1, 1, Colors.Black);
    //    }
    //  }

    //  if (PropertyChanged != null)
    //  {
    //    PropertyChanged(this, new PropertyChangedEventArgs("Image"));
    //    PropertyChanged(this, new PropertyChangedEventArgs("Summary"));
    //  }
    //}

    static double GetAverage(IEnumerable<double> times)
    {
      var list = times.ToList();

      if (list.Count > 2)
      {
        // Return the median value
        list.Sort();
        if (list.Count % 2 == 0)
          return list[list.Count / 2];
        else
          return list.GetRange(list.Count / 2, 2).Average();
      }
      else
      {
        return list.Average();
      }
    }

    //CpuGpuTime RunScenario(IScenarioRunner runner, CanvasRenderTarget rt)
    //{
    //  while (true)
    //  {
    //    Stopwatch stopWatch = new Stopwatch();

    //    using (GpuStopWatch gpuStopWatch = new GpuStopWatch(rt.Device))
    //    {

    //      gpuStopWatch.Start();
    //      stopWatch.Start();

    //      using (var ds = rt.CreateDrawingSession())
    //      {
    //        ds.Clear(Colors.Black);
    //        runner.RunScenario(ds, SortMode);
    //      }

    //      stopWatch.Stop();
    //      gpuStopWatch.Stop();

    //      var gpuTime = gpuStopWatch.GetGpuTimeInMs();
    //      if (gpuTime < 0)
    //      {
    //        // try again until we get a time that isn't disjoint
    //        continue;
    //      }

    //      return new CpuGpuTime
    //      {
    //        CpuTimeInMs = stopWatch.Elapsed.TotalMilliseconds,
    //        GpuTimeInMs = gpuTime
    //      };
    //    }
    //  }
    //}

    public void Reset()
    {
      Data = new Dictionary<int, CpuGpuTime>();
      if (PropertyChanged != null)
        PropertyChanged(this, new PropertyChangedEventArgs("Summary"));
    }

    public void PopulateWithThumbnailData()
    {
      Reset();

      int seed = (int)Method + (int)SortMode * Enum.GetValues(typeof(DrawMethod)).Length;

      var rnd = new Random(seed);

      double scale = rnd.NextDouble() * 30;
      double jitter = 10;

      for (int i = 0; i < 100; ++i)
      {
        Data.Add(i * 100, new CpuGpuTime()
        {
          CpuTimeInMs = (i / 100.0) * scale + rnd.NextDouble() * jitter,
          GpuTimeInMs = (i / 100.0) * scale + rnd.NextDouble() * jitter
        });
      }
    }
  }

  class GraphDrawer
  {
    float actualHeight;
    float actualWidth;
    List<Scenario> scenarios;
    Func<Scenario.CpuGpuTime, double> selector;
    string title;

    float maxXValue;
    float maxYValue;

    Vector2 origin;
    Vector2 xAxisEnd;
    Vector2 yAxisEnd;

    public GraphDrawer(float actualWidth, float actualHeight, List<Scenario> scenarios, Func<Scenario.CpuGpuTime, double> selector, string title)
    {
      this.actualWidth = actualWidth;
      this.actualHeight = actualHeight;
      this.scenarios = scenarios;
      this.selector = selector;
      this.title = title;

      maxXValue = (float)(Math.Ceiling(FindMaxCount() / 100) * 100); // round to next 100
      maxYValue = (float)(Math.Ceiling(FindMaxTime() / 2) * 2); // round to next 2
    }

    CanvasTextFormat axisFormat = new CanvasTextFormat()
    {
      FontSize = 12,
      HorizontalAlignment = CanvasHorizontalAlignment.Right,
      VerticalAlignment = CanvasVerticalAlignment.Center
    };

    public void Draw(CanvasDrawingSession ds)
    {
      var widestLayoutRect = new CanvasTextLayout(ds, string.Format("{0}\t", maxYValue.ToString()), axisFormat, 500, 100).LayoutBounds;

      origin = new Vector2((float)widestLayoutRect.Width, actualHeight - (float)widestLayoutRect.Height);
      yAxisEnd = new Vector2(origin.X, (float)widestLayoutRect.Height);
      xAxisEnd = new Vector2(actualWidth, origin.Y);

      foreach (var scenario in scenarios)
      {
        DrawSeries(ds, scenario);
      }

      DrawAxes(ds);

      ds.DrawText(title, new Vector2(origin.X + 10, 10), Colors.White);
    }

    private void DrawSeries(CanvasDrawingSession ds, Scenario scenario)
    {
      var data = scenario.Data;

      var lastPoint = origin;
      var color = scenario.Color.Color;

      foreach (var key in data.Keys.OrderBy(k => k))
      {
        var point = new Vector2(GetX(key), GetY((float)selector(data[key])));

        ds.DrawLine(lastPoint, point, color, 2);

        lastPoint = point;
      }
    }

    private void DrawAxes(CanvasDrawingSession ds)
    {
      ds.DrawLine(origin, xAxisEnd, Colors.White, 1);
      ds.DrawLine(origin, yAxisEnd, Colors.White, 1);

      for (int i = 0; i <= 9; ++i)
      {
        float y = (maxYValue / 9.0f) * (float)i;

        ds.DrawText(string.Format("{0}", (int)y), new Vector2(origin.X - 5, GetY(y)), Colors.White, axisFormat);
      }

      axisFormat.VerticalAlignment = CanvasVerticalAlignment.Top;

      for (int i = 0; i <= 9; ++i)
      {
        float x = (maxXValue / 9.0f) * (float)i;

        ds.DrawText(string.Format("{0}", (int)x), new Vector2(GetX(x), origin.Y), Colors.White, axisFormat);
      }
    }

    double FindMaxTime()
    {
      double maxTime = 0;

      foreach (var scenario in scenarios)
      {
        if (scenario.Data.Count > 0)
          maxTime = Math.Max(maxTime, scenario.Data.Values.Select(selector).Max());
      }

      return maxTime;
    }

    float FindMaxCount()
    {
      float maxCount = 0;

      foreach (var scenario in scenarios)
      {
        if (scenario.Data.Count > 0)
          maxCount = Math.Max(maxCount, scenario.Data.Keys.Max());
      }

      return maxCount;
    }

    float GetY(float value)
    {
      return Vector2.Lerp(origin, yAxisEnd, (value / maxYValue)).Y;
    }

    float GetX(float value)
    {
      return Vector2.Lerp(origin, xAxisEnd, (value / maxXValue)).X;
    }
  }

  //internal class ScenarioData : IDisposable
  //{
  //  public int DrawCount { get; private set; }
  //  public int BitmapCount { get; private set; }
  //  public int Value { get; private set; }

  //  public struct DrawInfo
  //  {
  //    public CanvasBitmap Bitmap;
  //    public Vector4 Tint;
  //    public Vector2 Position;
  //    public float Rotation;
  //  }

  //  List<DrawInfo> drawInfo;

  //  public ScenarioData(CanvasDevice device, int drawCount, int bitmapCount, int value)
  //  {
  //    DrawCount = drawCount;
  //    BitmapCount = bitmapCount;
  //    Value = value;

  //    var bitmaps = new List<CanvasBitmap>();
  //    for (int i = 0; i < bitmapCount; ++i)
  //    {
  //      var bitmap = new CanvasRenderTarget(device, 32, 32, 96);
  //      using (var ds = bitmap.CreateDrawingSession())
  //      {
  //        ds.FillCircle(16, 16, 16, Colors.White);
  //        ds.DrawText(i.ToString(), new Rect(0, 0, 32, 32), Colors.Black,
  //            new CanvasTextFormat()
  //            {
  //              FontFamily = "Comic Sans MS",
  //              HorizontalAlignment = CanvasHorizontalAlignment.Center,
  //              VerticalAlignment = CanvasVerticalAlignment.Center
  //            });
  //      }
  //      bitmaps.Add(bitmap);
  //    }

  //    var rng = new Random();

  //    drawInfo = new List<DrawInfo>();
  //    for (int i = 0; i < drawCount; ++i)
  //    {
  //      drawInfo.Add(new DrawInfo()
  //      {
  //        Bitmap = bitmaps[i % bitmaps.Count],
  //        Position = new Vector2(64.0f + (float)Math.Sin(i * 0.1) * 16, 64.0f + (float)Math.Cos(i * 0.1) * 16),
  //        Rotation = (float)(i * 0.3),
  //        Tint = new Vector4((float)rng.NextDouble(), (float)rng.NextDouble(), (float)rng.NextDouble(), (float)rng.NextDouble())
  //      });
  //    }
  //  }

  //  public void Dispose()
  //  {
  //    foreach (var d in drawInfo)
  //    {
  //      d.Bitmap.Dispose();
  //    }
  //  }

  //  public void AddDataToScenarioRunner(IScenarioRunner runner)
  //  {
  //    foreach (var d in drawInfo)
  //    {
  //      runner.AddSprite(d.Bitmap, d.Tint, d.Position, d.Rotation);
  //    }
  //  }
  //}
}
