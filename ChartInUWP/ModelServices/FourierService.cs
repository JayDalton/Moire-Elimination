using ChartInUWP.Models;
using MathNet.Numerics;
using MathNet.Numerics.IntegralTransforms;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChartInUWP.ModelServices
{
  public class FourierService
  {
    #region Fields

    IPixelDataSource _source;
    ConcurrentDictionary<int, float[]> _magnitude;

    #endregion

    public FourierService(IPixelDataSource source)
    {
      _source = source;
      _magnitude = new ConcurrentDictionary<int, float[]>();
    }

    #region Properties

    public ushort DataRows { get; set; }
    public ushort DataCols { get; set; }
    public float DataMinimum { get; set; }
    public float DataMaximum { get; set; }

    #endregion Properties

    #region Methods

    void LoadPixelDataAsync()
    {
    }

    public float[] GetRow(int row)
    {
      return new float[0];
    }

    public async Task LoadGraphDataAsync()
    {
      if (_source != null && _source.ContainsData())
      {
        var sw = Stopwatch.StartNew();
        var pixelData = await _source.GetPixelDataAsync();
        var complex = pixelData.data
          .Select(Convert.ToSingle)
          .Select(v => v * (1.0f / ushort.MaxValue))
          .Select(v => new Complex32(v, 0))
          .Split(DataRows)
          .Where(v => v.Count() % 2 == 0)
          //.ToArray()
          ;

          // perform ffts
          _magnitude = new ConcurrentDictionary<int, float[]>();
          //var _magnitude2 = new BlockingCollection<float[]>();
          await Task.Run(() => {
            var rows = pixelData.rows;
            var cols = pixelData.cols;
            //Parallel.ForEach(complex, options, row => {
            //  var oneLine = row.ToArray();
            //  Fourier.Forward(oneLine, FourierOptions.Default);
            //  //_magnitude.TryAdd(row, oneLine.Select(c => c.Magnitude).ToArray());
            //  _magnitude2.Add(oneLine.Select(c => c.Magnitude).ToArray());
            //});

            Parallel.For(0, rows, row =>
            {
              //var oneLine = complex[row].ToArray();
              //var oneLine = complex.Skip(row * cols).Take(DataCols).ToArray();
              //Fourier.Forward(oneLine, FourierOptions.Default);
              //_magnitude.TryAdd(row, oneLine.Select(c => c.Magnitude).ToArray());
            });
          });
          sw.Stop();
          Debug.WriteLine(" : " + sw.ElapsedMilliseconds);
          //GlobalMaxValue = 4000;
          //var complex = Samples.Select(v => new Complex32(v, 0)).ToArray();
          //var x = _magnitude.OrderBy(v => v.Key);// Samples.Max();
      }
    }

    public MatrixStruct<float> GetMatrixMagnitude()
    {
      return default(MatrixStruct<float>);
    }

    #endregion Methods
  }
}
