using ChartInUWP.Interfaces;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI.Xaml;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage.Streams;
using Windows.UI;

namespace ChartInUWP.ModelServices
{
  public class ImageService : IImageService
  {
    #region Fields

    CanvasImageSource _canvasImageSource;
    ConcurrentDictionary<int, float[]> _imageLines;

    #endregion Fields

    public ImageService(CanvasImageSource canvasSource)
    {
      _canvasImageSource = canvasSource;
      _imageLines = new ConcurrentDictionary<int, float[]>();
    }

    #region Methods

    public void AddLineValues(int line, float[] values)
    {
      _imageLines.TryAdd(line, values);
    }

    public async Task Render()
    {
      using (var session = _canvasImageSource.CreateDrawingSession(Colors.White))
      {
        await CreateBitmap(session, _canvasImageSource.Size);
      }
    }

    public async Task CreateBitmap(CanvasDrawingSession session, Size size)
    {
      if (0 < _imageLines.Count)
      {
        using (var stream = new InMemoryRandomAccessStream())
        {
          using (var writer = new DataWriter(stream))
          {
            foreach (var key in _imageLines.Keys.OrderBy(k => k))
            {
              if (_imageLines.TryGetValue(key, out var line))
              {
                line.ToList().ForEach(v => writer.WriteSingle(v));
              }
            }
            await writer.StoreAsync();
            await writer.FlushAsync();
            writer.DetachStream();
          }

          using (var reader = new DataReader(stream.GetInputStreamAt(0)))
          {
            var bytes = new byte[stream.Size];
            await reader.LoadAsync((uint)stream.Size);
            reader.ReadBytes(bytes);

            var bitmap = CanvasBitmap.CreateFromBytes(
              _canvasImageSource.Device, bytes, 10, 10, 
              Windows.Graphics.DirectX.DirectXPixelFormat.A8UIntNormalized
            );

            using (var ds = _canvasImageSource.CreateDrawingSession(Colors.Gray))
            {
              ds.DrawImage(bitmap);
            }
          }
        }
      }
    }

    #endregion Methods
  }
}
