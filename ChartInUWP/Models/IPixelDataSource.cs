using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml.Media;

namespace ChartInUWP.Models
{
  /// <summary>
  /// Hält eine File-Quelle von Pixeldaten bereit, 
  /// unterstützt
  /// </summary>
  public interface IPixelDataSource
  {
    bool ContainsData();
    StorageFile File { get; }
    Task<bool> OpenFileAsync();
    Task<ImageSource> GetImageSourceAsync();
    Task<MatrixStruct<ushort>> GetPixelShortsAsync();
    Task<MatrixStruct<float>> GetPixelFloatsAsync();
  }

  public abstract class AbstractBitmapModel
  {
    public abstract void GetData();

    public virtual void GetMore()
    {

    }
  }
}
