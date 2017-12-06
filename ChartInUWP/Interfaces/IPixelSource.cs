using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using Windows.UI.Xaml.Media;

namespace ChartInUWP.Interfaces
{
  /// <summary>
  /// Hält eine File-Quelle von Pixeldaten bereit, 
  /// unterstützt
  /// </summary>
  public interface IPixelSource
  {
    //Size Size { get; }
    uint Width { get; }
    uint Height { get; }
    bool ContainsData();
    StorageFile File { get; }
    Task<bool> OpenFileAsync();
    Task<ImageSource> GetImageSourceAsync();
    IEnumerable<float[]> GetFloatsIterator();
    IEnumerable<ushort> GetContentAsUShort();
    Task<BitmapMatrix<byte>> GetBytesMatrixAsync();
    Task<BitmapMatrix<float>> GetFloatsMatrixAsync();
    Task<BitmapMatrix<ushort>> GetShortsMatrixAsync();
  }

  public abstract class AbstractBitmapModel
  {
    public abstract void GetData();

    public virtual void GetMore()
    {

    }
  }
}
