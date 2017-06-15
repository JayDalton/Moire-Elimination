using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Brushes;
using Microsoft.Graphics.Canvas.Geometry;
using Microsoft.Graphics.Canvas.Text;
using Microsoft.Graphics.Canvas.UI.Xaml;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Collections.ObjectModel;
using Windows.UI.Xaml.Media.Imaging;

// Die Elementvorlage "Leere Seite" wird unter https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x407 dokumentiert.

namespace ChartInUWP
{
  /// <summary>
  /// Eine leere Seite, die eigenständig verwendet oder zu der innerhalb eines Rahmens navigiert werden kann.
  /// </summary>
  public sealed partial class MainPage : Page
  {
    private const float DataStrokeThickness = 1;
    private readonly Dictionary<int, List<double>> _data;
    private readonly ChartRenderer _chartRenderer;

    int MovingHorizontalValue = default(int);
    int MovingVerticalValue = default(int);

    public MainPage()
    {
      this.InitializeComponent();
      _data = new Dictionary<int, List<double>>();
      _chartRenderer = new ChartRenderer();
    }

    private async void Page_Loaded(object sender, RoutedEventArgs e)
    {
      //RenderRawImage();
      await ReadInputData();
    }

    private async Task RenderRawImage()
    {
      const int WIDTH = 4318;
      const int HEIGHT = 4320;
      var picker = new FileOpenPicker();
      picker.ViewMode = PickerViewMode.List;
      picker.SuggestedStartLocation = PickerLocationId.ComputerFolder;
      picker.FileTypeFilter.Add(".raw");

      StorageFile file = await picker.PickSingleFileAsync();
      if (file != null)
      {
        using (var stream = await file.OpenAsync(FileAccessMode.Read))
        {
          var bytes = new byte[stream.Size];
          await stream.ReadAsync(bytes.AsBuffer(), (uint)stream.Size, InputStreamOptions.None);
          var softwareBitmap = new SoftwareBitmap(BitmapPixelFormat.Gray16, WIDTH, HEIGHT);
          softwareBitmap.CopyFromBuffer(bytes.AsBuffer());

          if (softwareBitmap.BitmapPixelFormat != BitmapPixelFormat.Bgra8 ||
              softwareBitmap.BitmapAlphaMode == BitmapAlphaMode.Straight)
          {
            softwareBitmap = SoftwareBitmap.Convert(
              softwareBitmap, 
              BitmapPixelFormat.Bgra8, 
              BitmapAlphaMode.Premultiplied
            );
          }

          var softwareBitmapeSource = new SoftwareBitmapSource();
          await softwareBitmapeSource.SetBitmapAsync(softwareBitmap);
          GreyImage.Source = softwareBitmapeSource;
        } 
      }
    }

    private async Task ReadInputData()
    {
      var picker = new FileOpenPicker();
      picker.ViewMode = PickerViewMode.List;
      picker.SuggestedStartLocation = PickerLocationId.ComputerFolder;
      picker.FileTypeFilter.Add(".txt");
      picker.FileTypeFilter.Add(".log");
      picker.FileTypeFilter.Add(".dat");

      StorageFile file = await picker.PickSingleFileAsync();
      if (file != null)
      {
        try
        {
          var rows = default(int);
          var content = await FileIO.ReadLinesAsync(file);
          foreach (var line in content)
          {
            var fields = line.Split(';');
            var values = new List<double>();
            foreach (var field in fields)
            {
              if (!string.IsNullOrWhiteSpace(field))
              {
                values.Add(Convert.ToDouble(field, CultureInfo.InvariantCulture));
              }
            }
            _data.Add(rows++, values);
          }
          if (_data.Count > 0)
          {
            MovingHorizontalSlider.Minimum = 0;
            MovingHorizontalSlider.Maximum = _data[0].Count;
            MovingVerticalSlider.Minimum = 0;
            MovingVerticalSlider.Maximum = _data.Count;
            GraphCanvas.Invalidate();
          }
        }
        catch (Exception ex)
        {
          Debug.WriteLine(ex.Message);
          throw;
        }
      }
    }

    private void OnDrawGraph(CanvasControl sender, CanvasDrawEventArgs args)
    {
      args.DrawingSession.Clear(Colors.White);
      if (_data.ContainsKey(MovingVerticalValue))
      {
        int index = Math.Min(MovingHorizontalValue, _data[MovingVerticalValue].Count - 1);
        int count = Math.Min((int)sender.ActualWidth, _data[MovingVerticalValue].Count - 1 - index);
        var values = _data[MovingVerticalValue].GetRange(index, count);
        _chartRenderer.RenderData(GraphCanvas, args, Colors.Black, DataStrokeThickness, values, false, _data[MovingVerticalValue].Max());
        _chartRenderer.RenderAxes(GraphCanvas, args);
      }
    }

    private void MovingHorizontalSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
    {
      MovingHorizontalValue = (int)e.NewValue;
      GraphCanvas.Invalidate();
    }

    private void MovingVerticalSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
    {
      MovingVerticalValue = (int)e.NewValue;
      GraphCanvas.Invalidate();
    }

  }

  class ChartRenderer
  {
    public void RenderAxes(CanvasControl canvas, CanvasDrawEventArgs args)
    {
      var width = (float)canvas.ActualWidth;
      var height = (float)(canvas.ActualHeight);
      var midWidth = (float)(width * .5);
      var midHeight = (float)(height * .5);

      using (var cpb = new CanvasPathBuilder(args.DrawingSession))
      {
        // Horizontal line
        cpb.BeginFigure(new Vector2(0, midHeight));
        cpb.AddLine(new Vector2(width, midHeight));
        cpb.EndFigure(CanvasFigureLoop.Open);

        // Horizontal line arrow
        cpb.BeginFigure(new Vector2(width - 10, midHeight - 3));
        cpb.AddLine(new Vector2(width, midHeight));
        cpb.AddLine(new Vector2(width - 10, midHeight + 3));
        cpb.EndFigure(CanvasFigureLoop.Open);

        args.DrawingSession.DrawGeometry(CanvasGeometry.CreatePath(cpb), Colors.Gray, 1);
      }

      args.DrawingSession.DrawText("0", 5, midHeight - 30, Colors.Gray);
      args.DrawingSession.DrawText(canvas.ActualWidth.ToString(), width - 50, midHeight - 30, Colors.Gray);

      using (var cpb = new CanvasPathBuilder(args.DrawingSession))
      {
        // Vertical line
        cpb.BeginFigure(new Vector2(midWidth, 0));
        cpb.AddLine(new Vector2(midWidth, height));
        cpb.EndFigure(CanvasFigureLoop.Open);

        // Vertical line arrow
        cpb.BeginFigure(new Vector2(midWidth - 3, 10));
        cpb.AddLine(new Vector2(midWidth, 0));
        cpb.AddLine(new Vector2(midWidth + 3, 10));
        cpb.EndFigure(CanvasFigureLoop.Open);

        args.DrawingSession.DrawGeometry(CanvasGeometry.CreatePath(cpb), Colors.Gray, 1);
      }

      args.DrawingSession.DrawText("0", midWidth + 5, height - 30, Colors.Gray);
      args.DrawingSession.DrawText("1", midWidth + 5, 5, Colors.Gray);
    }

    public void RenderData(CanvasControl canvas, CanvasDrawEventArgs args, Color color, float thickness, List<double> data, bool renderArea, double maxY)
    {
      if (data.Count == 0) return;

      //var maxY = data.Max();
      var maxX = canvas.ActualWidth;

      using (var cpb = new CanvasPathBuilder(args.DrawingSession))
      {
        cpb.BeginFigure(new Vector2(0, (float)(data[0] * canvas.ActualHeight / maxY)));

        for (int i = 1; i < data.Count; i++)
        {
          cpb.AddLine(new Vector2(i, (float)(data[i] * canvas.ActualHeight / maxY)));
        }

        if (renderArea)
        {
          cpb.AddLine(new Vector2(data.Count, (float)canvas.ActualHeight));
          cpb.AddLine(new Vector2(0, (float)canvas.ActualHeight));
          cpb.EndFigure(CanvasFigureLoop.Closed);
          args.DrawingSession.FillGeometry(CanvasGeometry.CreatePath(cpb), Colors.LightGreen);
        }
        else
        {
          cpb.EndFigure(CanvasFigureLoop.Open);
          args.DrawingSession.DrawGeometry(CanvasGeometry.CreatePath(cpb), color, thickness);
        }
      }
    }

    public void RenderAveragesAsColumns(CanvasControl canvas, CanvasDrawEventArgs args, int columnAvgDataRange, float columnWidth, List<double> data)
    {
      var padding = .5 * (columnAvgDataRange - columnWidth);
      for (int start = 0; start < data.Count; start += columnAvgDataRange)
      {
        double total = 0;
        var range = Math.Min(columnAvgDataRange, data.Count - start);

        for (int i = start; i < start + range; i++)
        {
          total += data[i];
        }

        args.DrawingSession.FillRectangle(
            start + (float)padding,
            (float)(canvas.ActualHeight * (1 - total / range)),
            columnWidth,
            (float)(canvas.ActualHeight * (total / range)),
            Colors.WhiteSmoke);
      }
    }

    public void RenderAveragesAsPieChart(CanvasControl canvas, CanvasDrawEventArgs args, List<double> pieValues, List<Color> palette)
    {
      var total = pieValues.Sum();

      var w = (float)canvas.ActualWidth;
      var h = (float)canvas.ActualHeight;
      var midx = w / 2;
      var midy = h / 2;
      var padding = 50;
      var lineOffset = 20;
      var r = Math.Min(w, h) / 2 - padding;

      float angle = 0f;
      var center = new Vector2(midx, midy);

      for (int i = 0; i < pieValues.Count; i++)
      {
        float sweepAngle = (float)(2 * Math.PI * pieValues[i] / total);
        var arcStartPoint = new Vector2((float)(midx + r * Math.Sin(angle)), (float)(midy - r * Math.Cos(angle)));

        using (var cpb = new CanvasPathBuilder(args.DrawingSession))
        {
          cpb.BeginFigure(center);
          cpb.AddLine(arcStartPoint);
          cpb.AddArc(new Vector2(midx, midy), r, r, angle - (float)(Math.PI / 2), sweepAngle);
          cpb.EndFigure(CanvasFigureLoop.Closed);
          args.DrawingSession.FillGeometry(CanvasGeometry.CreatePath(cpb), palette[i % palette.Count]);
        }

        angle += sweepAngle;
      }

      angle = 0f;

      var lineBrush = new CanvasSolidColorBrush(args.DrawingSession, Colors.Black);

      for (int i = 0; i < pieValues.Count; i++)
      {
        float sweepAngle = (float)(2 * Math.PI * pieValues[i] / total);
        var midAngle = angle + sweepAngle / 2;
        var isRightHalf = midAngle < Math.PI;
        var isTopHalf = midAngle <= Math.PI / 2 || midAngle >= Math.PI * 3 / 2;
        var p0 = new Vector2((float)(midx + (r - lineOffset) * Math.Sin(midAngle)), (float)(midy - (r - lineOffset) * Math.Cos(midAngle)));
        var p1 = new Vector2((float)(midx + (r + lineOffset) * Math.Sin(midAngle)), (float)(midy - (r + lineOffset) * Math.Cos(midAngle)));
        var p2 = isRightHalf ? new Vector2(p1.X + 50, p1.Y) : new Vector2(p1.X - 50, p1.Y);

        using (var cpb = new CanvasPathBuilder(args.DrawingSession))
        {
          cpb.BeginFigure(p0);
          cpb.AddLine(p1);
          cpb.AddLine(p2);
          cpb.EndFigure(CanvasFigureLoop.Open);

          args.DrawingSession.DrawGeometry(
              CanvasGeometry.CreatePath(cpb),
              lineBrush,
              1);
        }

        args.DrawingSession.DrawText(
            pieValues[i].ToString("F2"),
            p1,
            Colors.Black,
            new CanvasTextFormat
            {
              HorizontalAlignment = isRightHalf ? CanvasHorizontalAlignment.Left : CanvasHorizontalAlignment.Right,
              VerticalAlignment = isTopHalf ? CanvasVerticalAlignment.Bottom : CanvasVerticalAlignment.Top,
              FontSize = 18
            });

        angle += sweepAngle;
      }
    }

    public void RenderMovingAverage(CanvasControl canvas, CanvasDrawEventArgs args, Color color, float thickness, int movingAverageRange, List<double> data)
    {
      using (var cpb = new CanvasPathBuilder(args.DrawingSession))
      {
        cpb.BeginFigure(new Vector2(0, (float)(canvas.ActualHeight * (1 - data[0]))));

        double total = data[0];

        int previousRangeLeft = 0;
        int previousRangeRight = 0;

        for (int i = 1; i < data.Count; i++)
        {
          var range = Math.Max(0, Math.Min(movingAverageRange / 2, Math.Min(i, data.Count - 1 - i)));
          int rangeLeft = i - range;
          int rangeRight = i + range;

          for (int j = previousRangeLeft; j < rangeLeft; j++)
          {
            total -= data[j];
          }

          for (int j = previousRangeRight + 1; j <= rangeRight; j++)
          {
            total += data[j];
          }

          previousRangeLeft = rangeLeft;
          previousRangeRight = rangeRight;

          cpb.AddLine(new Vector2(i, (float)(canvas.ActualHeight * (1 - total / (range * 2 + 1)))));
        }

        cpb.EndFigure(CanvasFigureLoop.Open);

        args.DrawingSession.DrawGeometry(CanvasGeometry.CreatePath(cpb), color, thickness);
      }
    }
  }
}
