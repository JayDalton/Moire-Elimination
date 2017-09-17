using ChartInUWP.Models;
using MessagePack;
using System.Linq;

namespace ChartInUWP
{
  [MessagePackObject]
  public class MatrixStruct<T>
  {
    public MatrixStruct()
    {

    }

    public MatrixStruct(MatrixStruct<T> mtx)
    {
      rows = mtx.rows;
      cols = mtx.cols;
      data = mtx.data;
    }

    [Key(0)]
    public ushort rows { get; set; }

    [Key(1)]
    public ushort cols { get; set; }

    [Key(2)]
    public T[] data { get; set; } // IList<short>

    public T[] GetRowSkip(int row)
    {
      var start = row < rows ? row * cols : 0;
      return data.Skip(start).Take(cols).ToArray();
    }

    public T[] GetRowCopy(int row)
    {
      var start = row < rows ? row * cols : 0;
      return data.SubArray(start, cols);
    }

    // GetRow(2, 20, 15)
    public T[] GetRowRange(int row, int col = 0, int len = 0)
    {
      //var start = row < Rows ? row * Cols : 0;
      //var length = len < Cols ? len : 0;
      //return Magnitudes.Skip(start).Take(length);
      return GetRowSkip(row).Skip(col).Take(len).ToArray();
    }

  }
}
