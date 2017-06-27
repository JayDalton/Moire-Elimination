﻿using Microsoft.Graphics.Canvas;
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

    //int MovingHorizontalValue = default(int);
    //int MovingVerticalValue = default(int);

    //double maxInputValue = double.MinValue;
    //double minInputValue = double.MaxValue;

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
        LoadProgress.IsActive = true;
        LoadProgress.Visibility = Visibility.Visible;
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
          //GreyImage.Source = softwareBitmapeSource;
        }

        LoadProgress.IsActive = false;
        LoadProgress.Visibility = Visibility.Collapsed;
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
        LoadProgress.IsActive = true;
        LoadProgress.Visibility = Visibility.Visible;
        try
        {
          _data.Clear();
          var rows = default(int);
          var maxInputValue = double.MinValue;
          var minInputValue = double.MaxValue;
          var content = await FileIO.ReadLinesAsync(file);

          foreach (var line in content)
          {
            var fields = line.Split(';');
            var values = new List<double>();
            foreach (var field in fields)
            {
              values.Add(Convert.ToDouble(field, CultureInfo.InvariantCulture));
            }
            maxInputValue = Math.Max(maxInputValue, values.Max());
            minInputValue = Math.Min(minInputValue, values.Min());
            _data.Add(rows++, values);
          }

          if (_data.Count > 0)
          {
            GraphMoveY.Value = 0;
            GraphMoveY.Minimum = 0;
            GraphMoveY.Maximum = _data.Count - 1;

            GraphScaleY.Value = minInputValue;
            GraphScaleY.Minimum = minInputValue;
            GraphScaleY.Maximum = maxInputValue;

            GraphMoveX.Value = 0;
            GraphMoveX.Minimum = 0;
            GraphMoveX.Maximum = _data[0].Count - 1;
            GraphCanvas.Invalidate();
          }
        }
        catch (Exception ex)
        {
          Debug.WriteLine(ex.Message);
          throw;
        }
        LoadProgress.IsActive = false;
        LoadProgress.Visibility = Visibility.Collapsed;
      }
    }

    private void GraphMoveY_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
    {
      GraphCanvas.Invalidate();
    }

    private void GraphMoveYIncrease_Click(object sender, RoutedEventArgs e)
    {
      if (GraphMoveY.Value < GraphMoveY.Maximum)
      {
        GraphMoveY.Value++; 
      }
    }

    private void GraphMoveYDecrease_Click(object sender, RoutedEventArgs e)
    {
      if (0 < GraphMoveY.Value)
      {
        GraphMoveY.Value--;
      }
    }

    private void GraphScaleY_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
    {
      GraphCanvas.Invalidate();
    }

    private void GraphScaleYDecrease_Click(object sender, RoutedEventArgs e)
    {
      if (0 < GraphScaleY.Value)
      {
        GraphScaleY.Value--;
      }
    }

    private void GraphScaleYIncrease_Click(object sender, RoutedEventArgs e)
    {
      if (GraphScaleY.Value < GraphScaleY.Maximum)
      {
        GraphScaleY.Value++;
      }
    }

    private void GraphScaleX_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
    {
      GraphCanvas.Invalidate();
    }

    private void GraphScaleXIncrease_Click(object sender, RoutedEventArgs e)
    {
      if (GraphScaleX.Value < GraphScaleX.Maximum)
      {
        GraphScaleX.Value++;
      }
    }

    private void GraphScaleXDecrease_Click(object sender, RoutedEventArgs e)
    {
      if (0 < GraphScaleX.Value)
      {
        GraphScaleX.Value--;
      }
    }

    private void GraphMoveX_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
    {
      GraphCanvas.Invalidate();
    }

    private void GraphMoveXIncrease_Click(object sender, RoutedEventArgs e)
    {
      if (GraphMoveX.Value < GraphMoveX.Maximum)
      {
        GraphMoveX.Value++;
      }
    }

    private void GraphMoveXDecrease_Click(object sender, RoutedEventArgs e)
    {
      if (0 < GraphMoveX.Value)
      {
        GraphMoveX.Value--;
      }
    }

    private async void Button_Click(object sender, RoutedEventArgs e)
    {
      await ReadInputData();
    }

    private async void LoadRawBitmapButton(object sender, RoutedEventArgs e)
    {
      await RenderRawImage();
    }

    private void OnDrawGraph(CanvasControl sender, CanvasDrawEventArgs args)
    {
      args.DrawingSession.Clear(Colors.White);
      if (_data.ContainsKey((int)GraphMoveY.Value))
      {
        //int index = Math.Min(MovingHorizontalValue, _data[MovingVerticalValue].Count - 1);
        //int count = Math.Min((int)sender.ActualWidth, _data[MovingVerticalValue].Count - 1 - index);
        var values = _data[(int)GraphMoveY.Value]/*.GetRange(index, count)*/;
        _chartRenderer.RenderData(
          GraphCanvas, args, Colors.Black, DataStrokeThickness, 
          values, false, GraphScaleY.Value, 0
          );
      }
      _chartRenderer.RenderAxes(GraphCanvas, args, GraphScaleY.Value);
    }

  }

  class ChartRenderer
  {
    public void RenderAxes(CanvasControl canvas, CanvasDrawEventArgs args, double maxY)
    {
      var width = (float)canvas.ActualWidth;
      var height = (float)(canvas.ActualHeight);
      var midWidth = (float)(width * .025);
      var midHeight = (float)(height * .975);

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
      args.DrawingSession.DrawText(maxY.ToString(), midWidth + 5, 5, Colors.Gray);
    }

    public void RenderData(
      CanvasControl canvas, CanvasDrawEventArgs args, Color color, 
      float thickness, List<double> data, bool renderArea, 
      double maxY, double minY
      )
    {
      if (data.Count == 0) return;

      float scaleX = (float)canvas.ActualWidth / data.Count;

      using (var cpb = new CanvasPathBuilder(args.DrawingSession))
      {
        //cpb.BeginFigure(new Vector2(0, (float)(data[0])));
        cpb.BeginFigure(new Vector2(0, (float)(canvas.ActualHeight - data[0] * canvas.ActualHeight / maxY)));
        //cpb.BeginFigure(new Vector2(0, (float)(data[0] * canvas.ActualHeight / maxY)));

        for (int i = 1; i < data.Count; i++)
        {
          //cpb.AddLine(new Vector2(i, (float)(data[i])));
          cpb.AddLine(new Vector2(i * scaleX, (float)(canvas.ActualHeight - data[i] * canvas.ActualHeight / maxY)));
          //cpb.AddLine(new Vector2(i, (float)(data[i] * canvas.ActualHeight / maxY)));
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
