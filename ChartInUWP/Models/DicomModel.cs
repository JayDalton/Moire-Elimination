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
      if (File != null)
      {
        try
        {
          var stream = await File.OpenStreamForReadAsync();
          _dicomFile = await DicomFile.OpenAsync(stream);
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

    public async Task<bool> OpenFileAsync(StorageFile file)
    {
      if (file != null)
      {
        try
        {
          File = file;
          var stream = await File.OpenStreamForReadAsync();
          _dicomFile = await DicomFile.OpenAsync(stream);
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

    public async Task<BitmapMatrix<ushort>> GetShortsMatrixAsync()
    {
      return await Task.Run(() => {
        if (ContainsData())
        {
          var header = DicomPixelData.Create(_dicomFile.Dataset);
          var pixelData = PixelDataFactory.Create(header, 0);
          return new BitmapMatrix<ushort>
          {
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
