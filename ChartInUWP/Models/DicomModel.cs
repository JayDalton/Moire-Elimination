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
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml.Media;

namespace ChartInUWP
{
  public class DicomModel : IPixelDataSource
  {
    #region Fields

    //StorageFile _storageFile;
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
      await Task.Delay(0);
      if (ContainsData())
      {
        var dicomImage = new DicomImage(_dicomFile.Dataset);
        return dicomImage.RenderImage().As<ImageSource>();
      }
      return default(ImageSource);
    }

    public async Task<MatrixStruct<ushort>> GetPixelShortsAsync()
    {
      return await Task.Run(() => {
        if (ContainsData())
        {
          var header = DicomPixelData.Create(_dicomFile.Dataset);
          var pixelData = PixelDataFactory.Create(header, 0);
          if (pixelData is GrayscalePixelDataU16)
          {
            return new MatrixStruct<ushort>
            {
              rows = header.Height,
              cols = header.Width,
              data = (pixelData as GrayscalePixelDataU16).Data
            };
          }
        }
        return default;
      });
    }

    public async Task<MatrixStruct<float>> GetPixelFloatsAsync()
    {
      return await Task.Run(() => {
        if (ContainsData())
        {
          var header = DicomPixelData.Create(_dicomFile.Dataset);
          var pixelData = PixelDataFactory.Create(header, 0);
          if (pixelData is GrayscalePixelDataU16)
          {
            return new MatrixStruct<float>
            {
              rows = header.Height,
              cols = header.Width,
              data = 
                (pixelData as GrayscalePixelDataU16).Data
                .Select(Convert.ToSingle)
                .Select(v => v * (1.0f / ushort.MaxValue))
                .ToArray()
            };
          }
        }
        return default;
      });
    }

    #endregion Methods
  }

}
