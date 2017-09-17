﻿using ChartInUWP.Models;
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
  public class DicomSource : IPixelDataSource
  {
    #region Fields

    StorageFile _storageFile;
    DicomFile _dicomFile = new DicomFile();

    #endregion Fields

    public DicomSource() {}

    #region Properties
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

      _storageFile = await picker.PickSingleFileAsync();
      if (_storageFile != null)
      {
        try
        {
          var stream = await _storageFile.OpenStreamForReadAsync();
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
          _storageFile = file;
          var stream = await file.OpenStreamForReadAsync();
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
      return await Task.Run(() => {
        if (ContainsData())
        {
          var dicomImage = new DicomImage(_dicomFile.Dataset);
          return dicomImage.RenderImage().As<ImageSource>();
        }
        return default(ImageSource);
      });
    }

    public async Task<MatrixStruct<ushort>> GetPixelDataAsync()
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
        return default(MatrixStruct<ushort>);
      });
    }

    #endregion Methods
  }

}
