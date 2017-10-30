using ChartInUWP.Interfaces;
using ChartInUWP.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace ChartInUWP
{
  public class ImageModel : IPixelSource
  {
    #region Fields

    BitmapDecoder _decoder;
    //StorageFile _storageFile;

    #endregion Fields

    public ImageModel()
    {
    }

    #region Properties

    public StorageFile File { get; private set; }

    #endregion Properties

    #region Events
    #endregion Events

    #region Methods

    public bool ContainsData()
    {
      throw new NotImplementedException();
    }

    public async Task<bool> OpenFileAsync()
    {
      var picker = new FileOpenPicker();
      picker.FileTypeFilter.Add(".png");
      picker.FileTypeFilter.Add(".jpg");
      picker.FileTypeFilter.Add(".tif");

      File = await picker.PickSingleFileAsync();
      if (File != null)
      {
        using (var stream = await File.OpenAsync(FileAccessMode.Read))
        {
          _decoder = await BitmapDecoder.CreateAsync(stream);
          return true;
        }
      }
      return false;
    }

    public async Task<ImageSource> GetImageSourceAsync()
    {
      if (_decoder != null)
      {
        var bitmap = await _decoder.GetSoftwareBitmapAsync();
        var source = new SoftwareBitmapSource();
        await source.SetBitmapAsync(bitmap);
      }
      return default(ImageSource);
    }

    public async Task<BitmapMatrix<ushort>> GetShortsMatrixAsync()
    {
      if (_decoder != null)
      {
        var bitmap = await _decoder.GetSoftwareBitmapAsync();
        if (bitmap.BitmapPixelFormat == BitmapPixelFormat.Gray16)
        {
          var pixelData = await _decoder.GetPixelDataAsync();
          byte[] buffer = pixelData.DetachPixelData();
          Enumerable.Range(0, buffer.Length / 2)
            .Select(idx => BitConverter.ToUInt16(buffer, idx));
          return new BitmapMatrix<ushort>
          {
            Rows = (ushort)bitmap.PixelHeight,
            Cols = (ushort)bitmap.PixelWidth,
            Data = Enumerable.Range(0, buffer.Length / 2)
              .Select(idx => BitConverter.ToUInt16(buffer, idx)).ToArray()
          };
        }
      }
      return default(BitmapMatrix<ushort>);
    }

    public async Task LoadImageDataFromFile()
    {
      //await _imageLoader.LoadImageSelection();
    }

    public Task<BitmapMatrix<float>> GetFloatsMatrixAsync()
    {
      throw new NotImplementedException();
    }

    public IEnumerable<float[]> GetFloatsIterator()
    {
      throw new NotImplementedException();
    }

    #endregion Methods
  }
}
