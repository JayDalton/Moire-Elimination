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
using System.Threading;

namespace ChartInUWP.ModelServices
{
  /// <summary>
  /// Holds the image data as float array and offers 
  /// calculation along magnitudes and filtering
  /// </summary>
  /// <typeparam name="T"></typeparam>
  public class FourierService
  {
    #region Fields

    MatrixStruct<float> _source;
    MatrixStruct<Complex32> _complex;
    ConcurrentDictionary<int, float[]> _magnitude;
    ConcurrentDictionary<int, Complex32[]> _transform;

    #endregion

    public FourierService(MatrixStruct<float> source)
    {
      _source = source;
      _complex = new MatrixStruct<Complex32>
      {
        rows = source.rows,
        cols = source.cols,
        data = source.data.Select(v => new Complex32(v, 0)).ToArray()
      };
    }

    #region Properties

    public ushort DataRows { get; set; }
    public ushort DataCols { get; set; }
    public float DataMinimum { get; set; }
    public float DataMaximum { get; set; }

    #endregion Properties

    #region Methods

    public async Task LoadGraphDataAsync()
    {
      if (_complex.data.Length > 0)
      {
        var parallel = Environment.ProcessorCount;
        var sw = Stopwatch.StartNew();

        // perform ffts
        _transform = new ConcurrentDictionary<int, Complex32[]>();
        _magnitude = new ConcurrentDictionary<int, float[]>();
        //var _magnitude2 = new BlockingCollection<float[]>();
        var row = 200;
        var rows = _complex.rows;
        var cols = _complex.cols;
        var line = _complex.data.Skip(row * cols).Take(cols).ToArray();

        sw.Restart();
        var result = line.Clone() as Complex32[];
        Fourier.NaiveForward(result, FourierOptions.Default);
        Debug.WriteLine(sw.ElapsedMilliseconds);
        var mag1 = result.Select(c => c.Magnitude).ToArray();
        var max1 = mag1.Max();
        var min1 = mag1.Min();
        var avg1 = mag1.Average();

        sw.Restart();
        result = line.Clone() as Complex32[];
        Fourier.NaiveForward(result, FourierOptions.NoScaling);
        Debug.WriteLine(sw.ElapsedMilliseconds);
        var mag2 = result.Select(c => c.Magnitude).ToArray();
        var max2 = mag2.Max();
        var min2 = mag2.Min();
        var avg2 = mag2.Average();

        sw.Restart();
        result = line.Clone() as Complex32[];
        result.ToList().Add(new Complex32());
        result.ToList().Add(new Complex32());
        Fourier.Forward(result, FourierOptions.Default);
        Debug.WriteLine(sw.ElapsedMilliseconds);
        var mag3 = result.Select(c => c.Magnitude).ToArray();
        var max3 = mag3.Max();
        var min3 = mag3.Min();
        var avg3 = mag3.Average();

        sw.Restart();
        result = line.Clone() as Complex32[];
        result.ToList().Add(new Complex32());
        result.ToList().Add(new Complex32());
        Fourier.Forward(result, FourierOptions.NoScaling);
        Debug.WriteLine(sw.ElapsedMilliseconds);
        var mag4 = result.Select(c => c.Magnitude).ToArray();
        var max4 = mag4.Max();
        var min4 = mag4.Min();
        var avg4 = mag4.Average();

        sw.Stop();
        //await Task.Run(() => {
        //  var options = new ParallelOptions() { MaxDegreeOfParallelism = 2 };
        //  Parallel.For(0, rows, options, row =>
        //  {
        //    var oneLine = _complex.data.Skip(row * cols).Take(cols).ToArray();
        //    Fourier.Forward(oneLine, FourierOptions.Default);
        //    _transform.TryAdd(row, oneLine);
        //    _magnitude.TryAdd(row, oneLine.Select(c => c.Magnitude).ToArray());
        //  });

        //  //Parallel.ForEach(_complex.data.Split(cols), options, row =>
        //  //{
        //  //  var oneLine = row.ToArray();
        //  //  Fourier.Forward(oneLine, FourierOptions.Default);
        //  //  //_magnitude.TryAdd(row, oneLine.Select(c => c.Magnitude).ToArray());
        //  //  _magnitude.TryAdd(oneLine.Select(c => c.Magnitude).ToArray());
        //  //});

        //  //long total = 0;
        //  //Parallel.For<long>(0, rows, () => 0, 
        //  //  (j, loop, subtotal) => {
        //  //    return 0;
        //  //  }, 
        //  //  (x) => Interlocked.Add(ref total, x)
        //  //);
        //});
        Debug.WriteLine(" : " + sw.ElapsedMilliseconds);
        //GlobalMaxValue = 4000;
        //var complex = Samples.Select(v => new Complex32(v, 0)).ToArray();
        //var x = _magnitude.OrderBy(v => v.Key);// Samples.Max();
      }
    }

    /// <summary>
    /// Calculates magnitude
    /// </summary>
    /// <returns>
    /// Returns half size magnitude
    /// </returns>
    public MatrixStruct<float> GetMatrixMagnitude()
    {
      return new MatrixStruct<float>() {
        rows = (ushort)_magnitude.Count,
        cols = (ushort)_magnitude[0].Length,
        data = _magnitude.Values.SelectMany(i => i).ToArray(),
      };
    }

    #endregion Methods
  }
}
