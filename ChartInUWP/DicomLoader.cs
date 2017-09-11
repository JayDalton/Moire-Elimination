using Dicom;
using Dicom.Imaging;
using Dicom.Imaging.Render;
using MathNet.Numerics;
using MathNet.Numerics.IntegralTransforms;
using System;
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

    private DicomFile _dicomFile;
    
    #endregion Fields

    public DicomLoader() {}

    #region Properties

    public ushort Rows { get; private set; }

    public ushort Cols { get; private set; }

    public float GlobalMaxValue { get; private set; }

    public float GlobalMinValue { get; private set; }

    public ImageSource ImageSource { get; private set; }

    public IEnumerable<float> Samples { get; private set; }

    public IEnumerable<float> Magnitudes { get; private set; }

    #endregion Properties

    #region Events
    #endregion Events

    #region Methods

    // GetRow(0)
    public IEnumerable<float> GetRow(int row)
    {
      var start = row < Rows ? row * Cols : 0;
      return Magnitudes.Skip(start).Take(Cols);
    }

    // GetRow(2, 20, 15)
    public IEnumerable<float> GetRowRange(int row, int col = 0, int len = 0)
    {
      var start = row < Rows ? row * Cols : 0;
      var length = len < Cols ? len : 0;
      return Magnitudes.Skip(start).Take(length);
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
            var pixelData = PixelDataFactory.Create(header, 0);
            if (pixelData is GrayscalePixelDataU16)
            {
              Samples = (pixelData as GrayscalePixelDataU16).Data
                .Select(Convert.ToSingle)
                .Select(v => v * (1.0f / ushort.MaxValue))
                .ToArray();
              GlobalMaxValue = Samples.Max();
              GlobalMinValue = Samples.Min();
              performFourierTransform();
            }
          }
        }
        catch (Exception ex)
        {
          Debug.WriteLine(ex.Message);
          throw;
        }
      }
    }

    private void performFourierTransform()
    {
      try
      {
        var complex = Samples.Select(v => new Complex32(v, 0)).ToArray();
        Fourier.Forward(complex, FourierOptions.Matlab);
        Magnitudes = complex.Select(c => c.Magnitude);
        var magnitude = complex.First().Magnitude;
      }
      catch (Exception ex)
      {
        Debug.WriteLine(ex.Message);
        throw;
      }
    }

    // reverse byte order (16-bit)
    public static UInt16 ReverseBytes(UInt16 value)
    {
      return (UInt16)((value & 0xFFU) << 8 | (value & 0xFF00U) >> 8);
    }

    // reverse byte order (32-bit)
    public static UInt32 ReverseBytes(UInt32 value)
    {
      return (value & 0x000000FFU) << 24 | (value & 0x0000FF00U) << 8 |
             (value & 0x00FF0000U) >> 8 | (value & 0xFF000000U) >> 24;
    }

    // reverse byte order (64-bit)
    public static UInt64 ReverseBytes(UInt64 value)
    {
      return (value & 0x00000000000000FFUL) << 56 | (value & 0x000000000000FF00UL) << 40 |
             (value & 0x0000000000FF0000UL) << 24 | (value & 0x00000000FF000000UL) << 8 |
             (value & 0x000000FF00000000UL) >> 8 | (value & 0x0000FF0000000000UL) >> 24 |
             (value & 0x00FF000000000000UL) >> 40 | (value & 0xFF00000000000000UL) >> 56;
    }

    #endregion Methods
  }
}
