﻿using ChartInUWP.Interfaces;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Brushes;
using Microsoft.Graphics.Canvas.Geometry;
using Microsoft.Graphics.Canvas.Text;
using Microsoft.Graphics.Canvas.UI.Xaml;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;

namespace ChartInUWP.ModelServices
{
  public class ChartService : IImageService
  {
    #region Fields

    bool renderArea = false;
    float DataStrokeThickness = 1;

    Color DataStrokeColor = Colors.Black;
    CanvasRenderTarget _canvasImageSource;
    BlockingCollection<float[]> _inputPipeline;
    ConcurrentDictionary<int, float[]> _content;

    #endregion Fields

    public ChartService(CanvasRenderTarget renderTarget)
    {
      _canvasImageSource = renderTarget;
      _content = new ConcurrentDictionary<int, float[]>();
    }

    #region Properties

    public int DataLineCount => _content.Count;
    public float DataMinimum { get; private set; } = float.MaxValue;
    public float DataMaximum { get; private set; } = float.MinValue;

    #endregion Properties

    #region Methods

    public void ClearValues() => _content.Clear();

    public void ClearScreen(Color color)
    {
      using (var ds = _canvasImageSource.CreateDrawingSession())
      {
        ds.Clear(color);
      }
    }

    public void AddLineValues(int idx, float[] values)
    {
      DataMaximum = values.Max();
      DataMinimum = values.Min();
      _content.TryAdd(idx, values);
    }

    public void LoadData(BlockingCollection<float[]> input)
    {
      _inputPipeline = input;
    }

    public void RenderChartRow(int line)
    {
      using (var session = _canvasImageSource.CreateDrawingSession())
      {
        session.Clear(Colors.White);
        RenderChartRow(line, _canvasImageSource.Size, session);
      }
    }

    public void RenderChartRow(int row, Size size, CanvasDrawingSession session)
    {
      RenderAxes(size, session, 1, 1);

      if (_content.TryGetValue(row, out var line))
      {
        RenderData(size, session, line);
      }
    }

    private void RenderInfo(Size size, CanvasDrawingSession session, string text)
    {
      session.Clear(Colors.DarkRed);
      session.DrawText(text, 
        new Vector2((float)size.Width / 2.0f, (float)size.Height / 2.0f), 
        Colors.DarkRed
      );
    }

    private void RenderData(Size size, CanvasDrawingSession session, float[] values)
    {
      var canvasWidth = (float)size.Width;

      var canvasHeight = (float)size.Height;
      var localMin = values.Min();
      var localMax = values.Max();
      var localAvg = values.Average();
      var scaleY = canvasHeight / values.Max();
      var scaleX = canvasWidth / values.Length;

      using (var cpb = new CanvasPathBuilder(session))
      {
        // first value
        cpb.BeginFigure(new Vector2(0, (canvasHeight - values[0] * scaleY)));
        //cpb.BeginFigure(new Vector2(0, values[0] * scaleY));

        // values
        for (int i = 1; i < values.Length; i++)
        {
          cpb.AddLine(new Vector2(i * scaleX, (canvasHeight - values[i] * scaleY)));
          //cpb.AddLine(new Vector2(i * scaleX, values[i] * scaleY));
        }

        if (renderArea)
        {
          cpb.AddLine(new Vector2(values.Count(), canvasHeight));
          cpb.AddLine(new Vector2(0, canvasHeight));
          cpb.EndFigure(CanvasFigureLoop.Closed);
          session.FillGeometry(CanvasGeometry.CreatePath(cpb), Colors.LightGreen);
        }
        else
        {
          cpb.EndFigure(CanvasFigureLoop.Open);
          session.DrawGeometry(CanvasGeometry.CreatePath(cpb), DataStrokeColor, DataStrokeThickness);
        }
      }
    }

    private void RenderAxes(Size size, CanvasDrawingSession session, double maxY, double scaleX, int count = 4320)
    {
      var width = (float)size.Width;
      var height = (float)(size.Height);
      var midWidth = (float)(width * .025);
      var midHeight = (float)(height * .975);

      using (var cpb = new CanvasPathBuilder(session))
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
        session.DrawGeometry(CanvasGeometry.CreatePath(cpb), Colors.DarkRed, 1);
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

    private void RenderAveragesAsColumns(CanvasControl canvas, CanvasDrawEventArgs args, int columnAvgDataRange, float columnWidth, List<double> data)
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

    private void RenderAveragesAsPieChart(CanvasControl canvas, CanvasDrawEventArgs args, List<double> pieValues, List<Color> palette)
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

    private void RenderMovingAverage(CanvasControl canvas, CanvasDrawEventArgs args, Color color, float thickness, int movingAverageRange, List<double> data)
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

    #endregion Methods
  }
}
