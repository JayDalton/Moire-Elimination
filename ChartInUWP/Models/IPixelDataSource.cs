using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml.Media;

namespace ChartInUWP.Models
{
  public interface IPixelDataSource
  {
    bool ContainsData();
    StorageFile File { get; }
    Task<bool> OpenFileAsync();
    Task<ImageSource> GetImageSourceAsync();
    Task<MatrixStruct<ushort>> GetPixelDataAsync();
  }
}
