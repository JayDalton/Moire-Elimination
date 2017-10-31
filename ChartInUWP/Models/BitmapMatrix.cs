using ChartInUWP.Interfaces;
using ChartInUWP.Models;
using MessagePack;
using System;
using System.Linq;

namespace ChartInUWP
{
  [MessagePackObject]
  public class BitmapMatrix<T> : IImageMatrix<T>
  {
    public BitmapMatrix()
    {
    }

    public BitmapMatrix(BitmapMatrix<T> mtx)
    {
      Type = typeof(T);
      Rows = mtx.Rows;
      Cols = mtx.Cols;
      Data = mtx.Data;
    }

    #region Properties

    public int Step { get; set; }     // only for bytes

    public int Size { get; set; }

    public Type Type { get; set; }    // ushort, float, ..

    [Key(0)]
    public ushort Rows { get; set; }

    [Key(1)]
    public ushort Cols { get; set; }

    [Key(2)]
    public T[] Data { get; set; } // IList<short>

    #endregion Properties

    #region Methods

    public T[] GetRowSkip(int row)
    {
      var start = row < Rows ? row * Cols : 0;
      return Data.Skip(start).Take(Cols).ToArray();
    }

    public T[] GetRowCopy(int row)
    {
      var start = row < Rows ? row * Cols : 0;
      return Data.SubArray(start, Cols);
    }

    // GetRow(2, 20, 15)
    public T[] GetRowRange(int row, int col = 0, int len = 0)
    {
      //var start = row < Rows ? row * Cols : 0;
      //var length = len < Cols ? len : 0;
      //return Magnitudes.Skip(start).Take(length);
      return GetRowSkip(row).Skip(col).Take(len).ToArray();
    }

    #endregion Methods
  }
}
