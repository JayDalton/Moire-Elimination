using ChartInUWP.Interfaces;
using ChartInUWP.Models;
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
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Core;
using Windows.UI.Xaml.Media;

namespace ChartInUWP
{
  public class DicomModel : IPixelSource
  {
    #region Fields

    DicomFile _dicomFile = new DicomFile();

    #endregion Fields

    public DicomModel() {}

    #region Properties

    public Size Size { get; set; }

    public StorageFile File { get; private set; }

    #endregion Properties

    #region Methods

    public bool ContainsData()
    {
      return (_dicomFile != null && _dicomFile.Dataset.Contains(DicomTag.PixelData));
    }

    public async Task<bool> OpenFileAsync()
    {
      var picker = new FileOpenPicker();
      picker.FileTypeFilter.Add(".dcm");
      picker.FileTypeFilter.Add(".dic");

      File = await picker.PickSingleFileAsync();
      return await OpenFileAsync(File);
    }

    public async Task<bool> OpenFileAsync(StorageFile file)
    {
      if (File != null)
      {
        try
        {
          var stream = await File.OpenStreamForReadAsync();
          _dicomFile = await DicomFile.OpenAsync(stream);
          var header = DicomPixelData.Create(_dicomFile.Dataset);
          Size = new Size(header.Width, header.Height);
          return true;
        }
        catch (Exception ex)
        {
          Debug.WriteLine(ex.Message);
          throw;
        }
      }
      return false;
    }

    public async Task<ImageSource> GetImageSourceAsync()
    {
      // refactoring run async dispatcher
      if (ContainsData())
      {
        var dicomImage = new DicomImage(_dicomFile.Dataset);
        var source = dicomImage.RenderImage().As<ImageSource>();
        return source;
      }
      return default;
    }

    public IEnumerable<ushort> GetContentAsUShort()
    {
      if (ContainsData())
      {
        var header = DicomPixelData.Create(_dicomFile.Dataset);
        var pixelData = PixelDataFactory.Create(header, 0);
        switch (pixelData)
        {
          case GrayscalePixelDataU16 temp:
            return temp.Data;//.Select(v => BitConverter.GetBytes(v)).SelectMany(b => b);
            //return temp.Data.Select(Convert.ToSingle).Select(v => v * (1.0 / double.MaxValue));
            //  break;
        }
      }
      return null;
    }

    public async Task<BitmapMatrix<byte>> GetBytesMatrixAsync()
    {
      return await Task.Run(() => {
        if (ContainsData())
        {
          var header = DicomPixelData.Create(_dicomFile.Dataset);
          var pixelData = PixelDataFactory.Create(header, 0);
          return new BitmapMatrix<byte>
          {
            Step = 1,
            Type = typeof(byte),
            Rows = header.Height,
            Cols = header.Width,
            Data = convertToBytes(pixelData),
            Size = 1,
          };
        }
        return default;
      });
    }

    public async Task<BitmapMatrix<ushort>> GetShortsMatrixAsync()
    {
      return await Task.Run(() => {
        if (ContainsData())
        {
          var header = DicomPixelData.Create(_dicomFile.Dataset);
          var pixelData = PixelDataFactory.Create(header, 0);
          return new BitmapMatrix<ushort>
          {
            Type = typeof(ushort),
            Rows = header.Height,
            Cols = header.Width,
            Data = convertToUShorts(pixelData)
          };
        }
        return default;
      });
    }

    public async Task<BitmapMatrix<float>> GetFloatsMatrixAsync()
    {
      return await Task.Run(() => {
        if (ContainsData())
        {
          var header = DicomPixelData.Create(_dicomFile.Dataset);
          var pixelData = PixelDataFactory.Create(header, 0);
          return new BitmapMatrix<float>
          {
            Type = typeof(float),
            Rows = header.Height,
            Cols = header.Width,
            Data = convertToFloats(pixelData)
          };
        }
        return default;
      });
    }

    private ushort[] convertToUShorts(IPixelData data)
    {
      switch (data)
      {
        case GrayscalePixelDataU16 temp:
          return temp.Data;

        default:
          break;
      }
      return default;
    }

    private byte[] convertToBytes(IPixelData data)
    {
      switch (data)
      {
        case GrayscalePixelDataU16 temp:
          return temp.Data.Select(v => BitConverter.GetBytes(v)).SelectMany(b => b).ToArray();

        default:
          break;
      }
      return default;
    }

    private float[] convertToFloats(IPixelData data)
    {
      switch (data)
      {
        case GrayscalePixelDataU16 temp:
          return temp.Data.Select(Convert.ToSingle).Select(v => v * (1.0f / ushort.MaxValue)).ToArray();

        default:
          break;
      }
      return default;
    }

    private double[] convertToDoubles(IPixelData data)
    {
      switch (data)
      {
        case GrayscalePixelDataU16 temp:
          return temp.Data.Select(Convert.ToSingle).Select(v => v * (1.0 / double.MaxValue)).ToArray();

        default:
          break;
      }
      return default;
    }

    public IEnumerable<float[]> GetFloatsIterator()
    {
      throw new NotImplementedException();
    }

    #endregion Methods
  }

}
