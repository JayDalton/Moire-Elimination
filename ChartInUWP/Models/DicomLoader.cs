using Dicom;
using Dicom.Imaging;
using Dicom.Imaging.Render;
using MathNet.Numerics;
using MathNet.Numerics.IntegralTransforms;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml.Media;

namespace ChartInUWP
{
  public class DicomLoader /*: BaseNotification*/
  {
    #region Fields

    DicomFile _dicomFile = new DicomFile();
    ConcurrentDictionary<int, float[]> _magnitude;

    #endregion Fields

    public DicomLoader()
    {
      _magnitude = new ConcurrentDictionary<int, float[]>();
    }

    #region Properties

    public ushort Rows { get; set; }
    //private ushort _rows = default(ushort);
    //public ushort Rows
    //{
    //  get { return _rows; }
    //  private set { SetProperty(ref _rows, value); }
    //}

    public ushort Cols { get; set; }
    //private ushort _cols = default(ushort);
    //public ushort Cols
    //{
    //  get { return _cols; }
    //  private set { SetProperty(ref _cols, value); }
    //}

    public float GlobalMaxValue { get; set; }
    //private float _maxValue = float.MinValue;
    //public float GlobalMaxValue
    //{
    //  get { return _maxValue; }
    //  private set { SetProperty(ref _maxValue, value); }
    //}

    public float GlobalMinValue { get; set; }
    //private float _minValue = float.MaxValue;
    //public float GlobalMinValue
    //{
    //  get { return _minValue; }
    //  private set { SetProperty(ref _minValue, value); }
    //}

    public ImageSource ImageSource { get; set; }
    //private ImageSource _imageSource;
    //public ImageSource ImageSource
    //{
    //  get { return _imageSource; }
    //  private set { SetProperty(ref _imageSource, value); }
    //}

    #endregion Properties

    #region Methods

    // GetRow(0)
    public float[] GetRow(int row)
    {
      //var start = row < Rows ? row * Cols : 0;
      //return Magnitudes.Skip(start).Take(Cols);
      return (_magnitude.TryGetValue(row, out var line)) ? line : new float[0];
    }

    // GetRow(2, 20, 15)
    public float[] GetRowRange(int row, int col = 0, int len = 0)
    {
      //var start = row < Rows ? row * Cols : 0;
      //var length = len < Cols ? len : 0;
      //return Magnitudes.Skip(start).Take(length);
      return GetRow(row).Skip(col).Take(len).ToArray();
    }

    public async Task OpenDicomFileAsync()
    {
      var picker = new FileOpenPicker();
      picker.FileTypeFilter.Add(".dcm");
      picker.FileTypeFilter.Add(".dic");

      StorageFile file = await picker.PickSingleFileAsync();
      if (file != null)
      {
        var stream = await file.OpenStreamForReadAsync();
        _dicomFile = await DicomFile.OpenAsync(stream);
        try
        {
          //if (_dicomFile.Dataset.Contains(DicomTag.PixelData))
          //{
          //  var dicomImage = new DicomImage(_dicomFile.Dataset);
          //  ImageSource = dicomImage.RenderImage().As<ImageSource>();
          //}
        }
        catch (Exception ex)
        {
          Debug.WriteLine(ex.Message);
          throw;
        }
      }
    }

    public T GetDicomRenderImage<T>()
    {
      if (_dicomFile.Dataset.Contains(DicomTag.PixelData))
      {
        var dicomImage = new DicomImage(_dicomFile.Dataset);
        return dicomImage.RenderImage().As<T>();
      }
      return default(T);
    }

    public MatrixStruct<ushort> GetDicomPixelData()
    {
      if (_dicomFile.Dataset.Contains(DicomTag.PixelData))
      {
        var header = DicomPixelData.Create(_dicomFile.Dataset);
        var pixelData = PixelDataFactory.Create(header, 0);
        if (pixelData is GrayscalePixelDataU16)
        {
          return new MatrixStruct<ushort> {
            rows = header.Height,
            cols = header.Width,
            data = (pixelData as GrayscalePixelDataU16).Data
          };
        }
      }
      return default(MatrixStruct<ushort>);
    }

    public async Task LoadDicomGraph()
    {
      if (_dicomFile != null && _dicomFile.Dataset.Contains(DicomTag.PixelData))
      {
        var header = DicomPixelData.Create(_dicomFile.Dataset);
        var pixelData = PixelDataFactory.Create(header, 0);
        if (pixelData is GrayscalePixelDataU16)
        {
          // preparations
          _magnitude.Clear();
          Rows = header.Height;
          Cols = header.Width;

          var provider = Control.LinearAlgebraProvider;

          var sw = Stopwatch.StartNew();
          var complex = (pixelData as GrayscalePixelDataU16).Data
            .Select(Convert.ToSingle)
            .Select(v => v * (1.0f / ushort.MaxValue))
            .Select(v => new Complex32(v, 0))
            .Split(Rows)
            .Where(v => v.Count() % 2 == 0)
            //.ToArray()
            ;

          var options = new ParallelOptions { MaxDegreeOfParallelism = 3 };
          // perform ffts
          _magnitude = new ConcurrentDictionary<int, float[]>();
          var _magnitude2 = new BlockingCollection<float[]>();
          await Task.Run(() => {
            Parallel.ForEach(complex, options, row => {
              var oneLine = row.ToArray();
              Fourier.Forward(oneLine, FourierOptions.Default);
              //_magnitude.TryAdd(row, oneLine.Select(c => c.Magnitude).ToArray());
              _magnitude2.Add(oneLine.Select(c => c.Magnitude).ToArray());
            });

            //Parallel.For(0, Rows, options, row => {
            //  //var oneLine = complex[row].ToArray();
            //  var oneLine = complex.Skip(row * Cols).Take(Cols).ToArray();
            //  Fourier.Forward(oneLine, FourierOptions.Default);
            //  _magnitude.TryAdd(row, oneLine.Select(c => c.Magnitude).ToArray());
            //});
          });
          sw.Stop();
          Debug.WriteLine(" : " + sw.ElapsedMilliseconds);
          GlobalMaxValue = 4000;
            //var complex = Samples.Select(v => new Complex32(v, 0)).ToArray();
          //var x = _magnitude.OrderBy(v => v.Key);// Samples.Max();
        }
      }
    }

    #endregion Methods
  }


  public static class Helper
  {
    public static IEnumerable<IEnumerable<T>> Split<T>(this T[] array, int size)
    {
      for (var i = 0; i < (float)array.Length / size; i++)
      {
        yield return array.Skip(i * size).Take(size);
      }
    }

    public static IEnumerable<IEnumerable<T>> Split<T>(this IEnumerable<T> array, int size)
    {
      foreach (var item in Enumerable.Range(0, size))
      {
        yield return array.Skip(item * size).Take(size);
      }
    }

    // reverse byte order (16-bit)
    public static UInt16 ReverseBytes(this UInt16 value)
    {
      return (UInt16)((value & 0xFFU) << 8 | (value & 0xFF00U) >> 8);
    }

    // reverse byte order (32-bit)
    public static UInt32 ReverseBytes(this UInt32 value)
    {
      return (value & 0x000000FFU) << 24 | (value & 0x0000FF00U) << 8 |
             (value & 0x00FF0000U) >> 8 | (value & 0xFF000000U) >> 24;
    }

    // reverse byte order (64-bit)
    public static UInt64 ReverseBytes(this UInt64 value)
    {
      return (value & 0x00000000000000FFUL) << 56 | (value & 0x000000000000FF00UL) << 40 |
             (value & 0x0000000000FF0000UL) << 24 | (value & 0x00000000FF000000UL) << 8 |
             (value & 0x000000FF00000000UL) >> 8 | (value & 0x0000FF0000000000UL) >> 24 |
             (value & 0x00FF000000000000UL) >> 40 | (value & 0xFF00000000000000UL) >> 56;
    }
  }
}
