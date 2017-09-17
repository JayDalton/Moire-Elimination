using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media;

namespace ChartInUWP.Models
{
  public interface IPixelDataSource
  {
    bool ContainsData();
    Task<bool> OpenFileAsync();
    Task<ImageSource> GetImageSourceAsync();
    Task<MatrixStruct<ushort>> GetPixelDataAsync();
    //ushort[] GetRow(int row);
    //ushort[] GetRowRange(int row, int col = 0, int len = 0);
  }
}
