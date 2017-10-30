using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChartInUWP.Interfaces
{
  public interface IImageMatrix<T>
  {
    ushort Rows { get; set; }
    ushort Cols { get; set; }
    T[] Data { get; set; }

    T[] GetRowSkip(int row);
    T[] GetRowCopy(int row);
    T[] GetRowRange(int row, int col = 0, int len = 0);
  }
}
