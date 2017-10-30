using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    bool ContainsData();
    StorageFile File { get; }
    Task<bool> OpenFileAsync();
    Task<ImageSource> GetImageSourceAsync();
    IEnumerable<float[]> GetFloatsIterator();
    Task<BitmapMatrix<ushort>> GetShortsMatrixAsync();
    Task<BitmapMatrix<float>> GetFloatsMatrixAsync();
  }

  public abstract class AbstractBitmapModel
  {
    public abstract void GetData();

    public virtual void GetMore()
    {

    }
  }
}
