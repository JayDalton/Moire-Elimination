using ChartInUWP.Models;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
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
using ChartInUWP.Interfaces;

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

    IPixelSource _pixelSource;
    BlockingCollection<(int, Complex32[])> _complex32;
    BlockingCollection<(int, float[])> _magnitude;
    BlockingCollection<(int, float[])> _transform;

    #endregion

    public FourierService(IPixelSource pixelSource)
    {
      _pixelSource = pixelSource;
      _complex32 = new BlockingCollection<(int, Complex32[])>();
      _magnitude = new BlockingCollection<(int, float[])>();
      _transform = new BlockingCollection<(int, float[])>();
    }

    #region Properties

    public ushort DataRows { get; private set; }
    public ushort DataCols { get; private set; }
    public IEnumerable<(int, float[])> Magnitudes => _magnitude.GetConsumingEnumerable();

    #endregion Properties

    #region Methods

    public async Task StartAnalysingAsync()
    {
      var parallel = Environment.ProcessorCount;

      var matrix = await _pixelSource.GetFloatsMatrixAsync();
      var step1 = Task.Factory.StartNew(() => 
      {
        try
        {
          for (int i = 0; i < matrix.Rows; i++)
          {
            float[] line = new float[optimizedSize(matrix.Cols)];
            Array.Copy(matrix.Data.Skip(i * matrix.Cols).Take(matrix.Cols).ToArray(), line, matrix.Cols);
            _complex32.Add((i, line.Select(v => new Complex32(v, 0)).ToArray()));
          }
        }
        finally { _complex32.CompleteAdding(); }
      });

      var step2 = Task.Factory.StartNew(() =>
      {
        try
        {
          foreach (var line in _complex32.GetConsumingEnumerable().AsParallel())
          {
            Fourier.BluesteinForward(line.Item2, FourierOptions.Default);
            var magnitudes = line.Item2.Select(c => MathF.Log10(c.Magnitude + 1)).ToArray();
            //var vec = MathNet.Numerics.LinearAlgebra.Vector<float>.Build;
            
            _magnitude.Add((line.Item1, magnitudes.Take(magnitudes.Length / 2 + 1).ToArray()));
          }
        }
        finally { _magnitude.CompleteAdding(); }
      });

      //Task.WaitAll(step1, step2);
    }

    public async Task LoadGraphDataAsync()
    {
      //if (_complex.Data.Length > 0)
      //{
      //  var parallel = Environment.ProcessorCount;
      //  var sw = Stopwatch.StartNew();

      //  var optSize = optimizedSize(4318);

      //  // perform ffts
      //  _transform = new ConcurrentDictionary<int, Complex32[]>();
      //  _magnitude = new ConcurrentDictionary<int, float[]>();
      //  //var _magnitude2 = new BlockingCollection<float[]>();
      //  var row = 200;
      //  var rows = _complex.Rows;
      //  var cols = _complex.Cols;
      //  var line = _complex.Data.Skip(row * cols).Take(cols).ToArray();
        

      //  sw.Restart();
      //  var result = line.Clone() as Complex32[];
      //  Fourier.NaiveForward(result, FourierOptions.Default);
      //  Debug.WriteLine(sw.ElapsedMilliseconds);
      //  var mag1 = result.Select(c => c.Magnitude).ToArray();
      //  var max1 = mag1.Max();
      //  var min1 = mag1.Min();
      //  var avg1 = mag1.Average();

      //  sw.Restart();
      //  result = line.Clone() as Complex32[];
      //  Fourier.NaiveForward(result, FourierOptions.NoScaling);
      //  Debug.WriteLine(sw.ElapsedMilliseconds);
      //  var mag2 = result.Select(c => c.Magnitude).ToArray();
      //  var max2 = mag2.Max();
      //  var min2 = mag2.Min();
      //  var avg2 = mag2.Average();

      //  sw.Restart();
      //  result = line.Clone() as Complex32[];
      //  result.ToList().Add(new Complex32());
      //  result.ToList().Add(new Complex32());
      //  Fourier.Forward(result, FourierOptions.Default);
      //  Debug.WriteLine(sw.ElapsedMilliseconds);
      //  var mag3 = result.Select(c => c.Magnitude).ToArray();
      //  var max3 = mag3.Max();
      //  var min3 = mag3.Min();
      //  var avg3 = mag3.Average();

      //  sw.Restart();
      //  result = line.Clone() as Complex32[];
      //  result.ToList().Add(new Complex32());
      //  result.ToList().Add(new Complex32());
      //  Fourier.Forward(result, FourierOptions.NoScaling);
      //  Debug.WriteLine(sw.ElapsedMilliseconds);
      //  var mag4 = result.Select(c => c.Magnitude).ToArray();
      //  var max4 = mag4.Max();
      //  var min4 = mag4.Min();
      //  var avg4 = mag4.Average();

      //  sw.Restart();
      //  result = line.Clone() as Complex32[];
      //  result.ToList().Add(new Complex32());
      //  result.ToList().Add(new Complex32());
      //  Fourier.BluesteinForward(result, FourierOptions.Default);
      //  Debug.WriteLine(sw.ElapsedMilliseconds);
      //  var mag5 = result.Select(c => c.Magnitude).ToArray();
      //  var max5 = mag5.Max();
      //  var min5 = mag5.Min();
      //  var avg5 = mag5.Average();

      //  sw.Restart();
      //  result = line.Clone() as Complex32[];
      //  result.ToList().Add(new Complex32());
      //  result.ToList().Add(new Complex32());
      //  Fourier.BluesteinForward(result, FourierOptions.NoScaling);
      //  Debug.WriteLine(sw.ElapsedMilliseconds);
      //  var mag6 = result.Select(c => c.Magnitude).ToArray();
      //  var max6 = mag6.Max();
      //  var min6 = mag6.Min();
      //  var avg6 = mag6.Average();



      //  sw.Stop();
      //  //await Task.Run(() => {
      //  //  var options = new ParallelOptions() { MaxDegreeOfParallelism = 2 };
      //  //  Parallel.For(0, rows, options, row =>
      //  //  {
      //  //    var oneLine = _complex.data.Skip(row * cols).Take(cols).ToArray();
      //  //    Fourier.Forward(oneLine, FourierOptions.Default);
      //  //    _transform.TryAdd(row, oneLine);
      //  //    _magnitude.TryAdd(row, oneLine.Select(c => c.Magnitude).ToArray());
      //  //  });

      //  //  //Parallel.ForEach(_complex.data.Split(cols), options, row =>
      //  //  //{
      //  //  //  var oneLine = row.ToArray();
      //  //  //  Fourier.Forward(oneLine, FourierOptions.Default);
      //  //  //  //_magnitude.TryAdd(row, oneLine.Select(c => c.Magnitude).ToArray());
      //  //  //  _magnitude.TryAdd(oneLine.Select(c => c.Magnitude).ToArray());
      //  //  //});

      //  //  //long total = 0;
      //  //  //Parallel.For<long>(0, rows, () => 0, 
      //  //  //  (j, loop, subtotal) => {
      //  //  //    return 0;
      //  //  //  }, 
      //  //  //  (x) => Interlocked.Add(ref total, x)
      //  //  //);
      //  //});
      //  Debug.WriteLine(" : " + sw.ElapsedMilliseconds);
      //  //GlobalMaxValue = 4000;
      //  //var complex = Samples.Select(v => new Complex32(v, 0)).ToArray();
      //  //var x = _magnitude.OrderBy(v => v.Key);// Samples.Max();
      //}
    }

    /// <summary>
    /// Calculates magnitude
    /// </summary>
    /// <returns>
    /// Returns half size magnitude
    /// </returns>
    //public BitmapMatrix<float> GetMatrixMagnitude()
    //{
    //  return new BitmapMatrix<float>() {
    //    Rows = (ushort)_magnitude.Count,
    //    Cols = (ushort)_magnitude[0].Length,
    //    Data = _magnitude.Values.SelectMany(i => i).ToArray(),
    //  };
    //}

    private int optimizedSize(int size)
    {
      // 2, 3, 5
      while (size % 2 != 0 && size % 3 != 0 && size % 5 != 0)
      {
        size++;
      }
      return size;
    }

    #endregion Methods
  }
}
