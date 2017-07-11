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

    public MainPage()
    {
      this.InitializeComponent();
      _data = new Dictionary<int, List<double>>();
      _chartRenderer = new ChartRenderer();
    }

    private async void Page_Loaded(object sender, RoutedEventArgs e)
    {
      //RenderRawImage();
      //await ReadInputData();
    }

    private async Task<byte[]> getByteArrayFromStream(IRandomAccessStream stream, bool swap = false)
    {
      var result = new byte[stream.Size];
      if (swap)
      {
        using (var output = new InMemoryRandomAccessStream())
        {
          using (var reader = new DataReader(stream.GetInputStreamAt(0)))
          {
            //reader.ByteOrder = ByteOrder.LittleEndian;
            using (var writer = new DataWriter(output))
            {
              //writer.ByteOrder = ByteOrder.BigEndian;
              await reader.LoadAsync((uint)stream.Size);
              while (0 < reader.UnconsumedBufferLength)
              {
                var number = reader.ReadUInt16();
                var bytes_number = BitConverter.GetBytes(number);
                writer.WriteBytes(bytes_number);
              }
              await writer.StoreAsync();
              await writer.FlushAsync();
              writer.DetachStream();
              output.Seek(0);
            }
            reader.DetachStream();
          }
          await output.ReadAsync(result.AsBuffer(), (uint)stream.Size, InputStreamOptions.None);
          var folder = await DownloadsFolder.CreateFileAsync(DateTime.UtcNow.Ticks + ".out");
          using (var file = await folder.OpenStreamForWriteAsync())
          {
            await file.WriteAsync(result, 0, result.Length);
            await file.FlushAsync();
          }
        }
      }
      else
      {
        await stream.ReadAsync(result.AsBuffer(), (uint)stream.Size, InputStreamOptions.None);
      }
      return result;
    }

    private async Task setImageSource(Image image, byte[] bytes)
    {
      const int WIDTH = 4318;
      const int HEIGHT = 4320;

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
      image.Source = softwareBitmapeSource;
    }

    private async Task RenderRawImage()
    {
      //const int WIDTH = 4318;
      //const int HEIGHT = 4320;
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
          var endian = await getByteArrayFromStream(stream, true);
          var normal = await getByteArrayFromStream(stream);

          await setImageSource(GreyImageLeft, normal);
          await setImageSource(GreyImageRight, endian);
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
            GraphMoveY.Maximum = _data.Count - 1;
            GraphMoveY.Minimum = 0;
            GraphMoveY.Value = 0;

            GraphScaleY.Maximum = maxInputValue;
            GraphScaleY.Minimum = minInputValue;
            GraphScaleY.Value = maxInputValue;

            GraphMoveX.Maximum = _data[0].Count - 1;
            GraphMoveX.Minimum = 0;
            GraphMoveX.Value = 0;

            GraphScaleX.Maximum = 2;    // 
            GraphScaleX.Minimum = GraphCanvas.ActualWidth / _data[0].Count;    // da passt ALLES rein
            GraphScaleX.Value = GraphCanvas.ActualWidth / _data[0].Count;
            GraphScaleX.SnapsTo = SliderSnapsTo.StepValues;
            GraphScaleX.StepFrequency = Math.Abs(GraphScaleX.Maximum - GraphScaleX.Minimum) / 10;

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
      var x = e.NewValue;

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

    private async void LoadDataButton_Click(object sender, RoutedEventArgs e)
    {
      GraphCanvas.Visibility = Visibility.Visible;
      GreyImageGrid.Visibility = Visibility.Collapsed;
      await ReadInputData();
    }

    private async void LoadImageButton_Click(object sender, RoutedEventArgs e)
    {
      GraphCanvas.Visibility = Visibility.Collapsed;
      GreyImageGrid.Visibility = Visibility.Visible;
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
          values, false, GraphScaleY.Value, GraphScaleX.Value
          );
      }
      _chartRenderer.RenderAxes(GraphCanvas, args, GraphScaleY.Value, GraphScaleX.Value);
    }

    // reverse byte order (16-bit)
    public static UInt16 ReverseBytes(UInt16 value)
    {
      return (UInt16)((value & 0xFFU) << 8 | (value & 0xFF00U) >> 8);
    }

    // reverse byte order (32-bit)
    public static UInt32 ReverseBytes(UInt32 value)
    {
      return (value & 0x000000FFU) << 24 | (value & 0x0000FF00U) << 8 |
             (value & 0x00FF0000U) >>  8 | (value & 0xFF000000U) >> 24;
    }

    // reverse byte order (64-bit)
    public static UInt64 ReverseBytes(UInt64 value)
    {
      return (value & 0x00000000000000FFUL) << 56 | (value & 0x000000000000FF00UL) << 40 |
             (value & 0x0000000000FF0000UL) << 24 | (value & 0x00000000FF000000UL) <<  8 |
             (value & 0x000000FF00000000UL) >>  8 | (value & 0x0000FF0000000000UL) >> 24 |
             (value & 0x00FF000000000000UL) >> 40 | (value & 0xFF00000000000000UL) >> 56;
    }
  }

  class ChartRenderer
  {
    public void RenderAxes(CanvasControl canvas, CanvasDrawEventArgs args, double maxY, double scaleX, int count = 4320)
    {
      var width = (float)canvas.ActualWidth;
      var height = (float)(canvas.ActualHeight);
      var midWidth = (float)(width * .025);
      var midHeight = (float)(height * .975);

      using (var cpb = new CanvasPathBuilder(args.DrawingSession))
      {
        float tick = 0.0f;
        for (int idx = 0; idx < count; ++idx)
        {
          if (idx % 100 == 0)
          {
            cpb.BeginFigure(new Vector2(idx * (float)scaleX, 0));
            cpb.AddLine(new Vector2(idx * (float)scaleX, 100));
            cpb.EndFigure(CanvasFigureLoop.Open);
            tick++;
          }
        }
        args.DrawingSession.DrawGeometry(CanvasGeometry.CreatePath(cpb), Colors.DarkRed, 1);
      }

      //using (var cpb = new CanvasPathBuilder(args.DrawingSession))
      //{
      //  // Horizontal line
      //  cpb.BeginFigure(new Vector2(0, midHeight));
      //  cpb.AddLine(new Vector2(width, midHeight));
      //  cpb.EndFigure(CanvasFigureLoop.Open);

      //  // Horizontal line arrow
      //  cpb.BeginFigure(new Vector2(width - 10, midHeight - 3));
      //  cpb.AddLine(new Vector2(width, midHeight));
      //  cpb.AddLine(new Vector2(width - 10, midHeight + 3));
      //  cpb.EndFigure(CanvasFigureLoop.Open);

      //  args.DrawingSession.DrawGeometry(CanvasGeometry.CreatePath(cpb), Colors.Gray, 1);
      //}

      //args.DrawingSession.DrawText("0", 5, midHeight - 30, Colors.Gray);
      //args.DrawingSession.DrawText(canvas.ActualWidth.ToString(), width - 50, midHeight - 30, Colors.Gray);

      //using (var cpb = new CanvasPathBuilder(args.DrawingSession))
      //{
      //  // Vertical line
      //  cpb.BeginFigure(new Vector2(midWidth, 0));
      //  cpb.AddLine(new Vector2(midWidth, height));
      //  cpb.EndFigure(CanvasFigureLoop.Open);

      //  // Vertical line arrow
      //  cpb.BeginFigure(new Vector2(midWidth - 3, 10));
      //  cpb.AddLine(new Vector2(midWidth, 0));
      //  cpb.AddLine(new Vector2(midWidth + 3, 10));
      //  cpb.EndFigure(CanvasFigureLoop.Open);

      //  args.DrawingSession.DrawGeometry(CanvasGeometry.CreatePath(cpb), Colors.Gray, 1);
      //}

      //args.DrawingSession.DrawText("0", midWidth + 5, height - 30, Colors.Gray);
      //args.DrawingSession.DrawText(maxY.ToString(), midWidth + 5, 5, Colors.Gray);
    }

    public void RenderData(
      CanvasControl canvas, CanvasDrawEventArgs args, Color color, 
      float thickness, List<double> data, bool renderArea, 
      double maxY, double scaleX
      )
    {
      if (data.Count == 0) return;

      using (var cpb = new CanvasPathBuilder(args.DrawingSession))
      {
        cpb.BeginFigure(
          new Vector2(0, 
            (float)(/*canvas.ActualHeight - */data[0] * canvas.ActualHeight / maxY)
          )
        );

        for (int i = 1; i < data.Count; i++)
        {
          cpb.AddLine(
            new Vector2(
              i * (float)scaleX, 
              (float)(/*canvas.ActualHeight - */data[i] * canvas.ActualHeight / maxY)
            )
          );
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
