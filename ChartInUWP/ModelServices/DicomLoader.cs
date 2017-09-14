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
  public class DicomLoader
  {
    #region Fields

    DicomFile _dicomFile;
    ConcurrentDictionary<int, IEnumerable<float>> _magnitude;

    #endregion Fields

    public DicomLoader() {}

    #region Properties

    public ushort Rows { get; private set; }

    public ushort Cols { get; private set; }

    public float GlobalMaxValue { get; private set; }

    public float GlobalMinValue { get; private set; }

    public ImageSource ImageSource { get; private set; }

    private IEnumerable<float> Samples { get; set; }

    //private IEnumerable<float> Magnitudes { get; set; }

    #endregion Properties

    #region Events
    #endregion Events

    #region Methods

    // GetRow(0)
    public IEnumerable<float> GetRow(int row)
    {
      //var start = row < Rows ? row * Cols : 0;
      //return Magnitudes.Skip(start).Take(Cols);
      return (_magnitude.TryGetValue(row, out var line)) ? line : new List<float>();
    }

    // GetRow(2, 20, 15)
    public IEnumerable<float> GetRowRange(int row, int col = 0, int len = 0)
    {
      //var start = row < Rows ? row * Cols : 0;
      //var length = len < Cols ? len : 0;
      //return Magnitudes.Skip(start).Take(length);
      return GetRow(row).Skip(col).Take(len);
    }

    public async Task LoadFromDicomFileAsync()
    {
      var picker = new FileOpenPicker();
      picker.FileTypeFilter.Add(".dcm");
      picker.FileTypeFilter.Add(".dic");

      StorageFile file = await picker.PickSingleFileAsync();
      if (file != null)
      {
        try
        {
          var stream = await file.OpenStreamForReadAsync();
          _dicomFile = await DicomFile.OpenAsync(stream);
          if (_dicomFile.Dataset.Contains(DicomTag.PixelData))
          {
            var dicomImage = new DicomImage(_dicomFile.Dataset);
            ImageSource = dicomImage.RenderImage().As<ImageSource>();
            var header = DicomPixelData.Create(_dicomFile.Dataset);
            Rows = header.Height;
            Cols = header.Width;
          }
        }
        catch (Exception ex)
        {
          Debug.WriteLine(ex.Message);
          throw;
        }
      }
    }

    public void LoadDicomGraph()
    {
      if (_dicomFile != null && _dicomFile.Dataset.Contains(DicomTag.PixelData))
      {
        var header = DicomPixelData.Create(_dicomFile.Dataset);
        Rows = header.Height;
        Cols = header.Width;
        var pixelData = PixelDataFactory.Create(header, 0);
        if (pixelData is GrayscalePixelDataU16)
        {
          // perform fourier
          var sw = Stopwatch.StartNew();
          var complex = (pixelData as GrayscalePixelDataU16).Data
            .Select(Convert.ToSingle)
            .Select(v => v * (1.0f / ushort.MaxValue))
            .Select(v => new Complex32(v, 0))
            .ToArray();

          //var complex = Samples.Select(v => new Complex32(v, 0)).ToArray();
          _magnitude = new ConcurrentDictionary<int, IEnumerable<float>>();
          Parallel.For(0, Rows, row => {
            var oneLine = complex.Skip(row * Cols).Take(Cols).ToArray();
            Fourier.Forward(oneLine, FourierOptions.Matlab);
            _magnitude.TryAdd(row, oneLine.Select(c => c.Magnitude));
          });
          sw.Stop();
          Debug.WriteLine(" : " + sw.ElapsedMilliseconds);
          GlobalMaxValue = 4000;
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
