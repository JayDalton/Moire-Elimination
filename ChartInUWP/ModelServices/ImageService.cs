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

    CanvasRenderTarget _canvasRenderTarget;
    ConcurrentDictionary<int, float[]> _content;

    #endregion Fields

    public ImageService(CanvasRenderTarget renderTarget)
    {
      _canvasRenderTarget = renderTarget;
      _content = new ConcurrentDictionary<int, float[]>();
    }

    #region Methods

    public void ClearValues() => _content.Clear();

    public void AddLineValues(int line, float[] values)
    {
      _content.TryAdd(line, values);
    }

    public void ClearScreen(Color color)
    {
      using (var session = _canvasRenderTarget.CreateDrawingSession())
      {
        session.Clear(color);
      }
    }

    public async Task Render()
    {
      using (var session = _canvasRenderTarget.CreateDrawingSession())
      {
        session.Clear(Colors.Gray);
        await CreateBitmap(session, _canvasRenderTarget.Size);
      }
    }

    public async Task CreateBitmap(CanvasDrawingSession session, Size size)
    {
      if (0 < _content.Count)
      {
        using (var stream = new InMemoryRandomAccessStream())
        {
          using (var writer = new DataWriter(stream))
          {
            foreach (var key in _content.Keys.OrderBy(k => k))
            {
              if (_content.TryGetValue(key, out var line))
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
              _canvasRenderTarget.Device, bytes, 10, 10, 
              Windows.Graphics.DirectX.DirectXPixelFormat.A8UIntNormalized
            );

            CanvasRenderTarget crt = new CanvasRenderTarget(_canvasRenderTarget.Device, 100, 100, 96);

            //crt.SetPixelBytes();

            using (var ds = _canvasRenderTarget.CreateDrawingSession())
            {
              ds.Clear(Colors.Gray);
              ds.DrawImage(crt);
              ds.DrawImage(bitmap);
            }
          }
        }
      }
    }

    #endregion Methods
  }
}
