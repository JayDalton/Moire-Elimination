using Dicom;
using Dicom.Imaging;
using Dicom.Imaging.Render;
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

    public IEnumerable<float> Values { get; private set; }

    #endregion Properties

    #region Events
    #endregion Events

    #region Methods

    // GetRow(0)
    public IEnumerable<float> GetRow(int row)
    {
      var start = row < Rows ? row * Cols : 0;
      return Values.Skip(start).Take(Cols);
    }

    // GetRow(2, 20, 15)
    public IEnumerable<float> GetRowRange(int row, int col = 0, int len = 0)
    {
      var start = row < Rows ? row * Cols : 0;
      var length = len < Cols ? len : 0;
      return Values.Skip(start).Take(length);
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
              Values = (pixelData as GrayscalePixelDataU16).Data
                .Select(Convert.ToSingle)
                .Select(v => v * (1.0f / ushort.MaxValue))
                .ToArray();
              GlobalMaxValue = Values.Max();
              GlobalMinValue = Values.Min();
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

    //private async Task<byte[]> getByteArrayFromStream(IRandomAccessStream stream, bool swap = false)
    //{
    //  var result = new byte[stream.Size];
    //  if (swap)
    //  {
    //    using (var output = new InMemoryRandomAccessStream())
    //    {
    //      using (var reader = new DataReader(stream.GetInputStreamAt(0)))
    //      {
    //        //reader.ByteOrder = ByteOrder.LittleEndian;
    //        using (var writer = new DataWriter(output))
    //        {
    //          //writer.ByteOrder = ByteOrder.BigEndian;
    //          await reader.LoadAsync((uint)stream.Size);
    //          while (0 < reader.UnconsumedBufferLength)
    //          {
    //            var number = reader.ReadUInt16();
    //            var bytes_number = BitConverter.GetBytes(number);
    //            writer.WriteBytes(bytes_number);
    //          }
    //          await writer.StoreAsync();
    //          await writer.FlushAsync();
    //          writer.DetachStream();
    //          output.Seek(0);
    //        }
    //        reader.DetachStream();
    //      }
    //      await output.ReadAsync(result.AsBuffer(), (uint)stream.Size, InputStreamOptions.None);
    //      var folder = await DownloadsFolder.CreateFileAsync(DateTime.UtcNow.Ticks + ".out");
    //      using (var file = await folder.OpenStreamForWriteAsync())
    //      {
    //        await file.WriteAsync(result, 0, result.Length);
    //        await file.FlushAsync();
    //      }
    //    }
    //  }
    //  else
    //  {
    //    await stream.ReadAsync(result.AsBuffer(), (uint)stream.Size, InputStreamOptions.None);
    //  }
    //  return result;
    //}

    //private async Task setImageSource(Image image, byte[] bytes)
    //{
    //  const int WIDTH = 4318;
    //  const int HEIGHT = 4320;

    //  var softwareBitmap = new SoftwareBitmap(BitmapPixelFormat.Gray16, WIDTH, HEIGHT);
    //  softwareBitmap.CopyFromBuffer(bytes.AsBuffer());

    //  if (softwareBitmap.BitmapPixelFormat != BitmapPixelFormat.Bgra8 ||
    //      softwareBitmap.BitmapAlphaMode == BitmapAlphaMode.Straight)
    //  {
    //    softwareBitmap = SoftwareBitmap.Convert(
    //      softwareBitmap,
    //      BitmapPixelFormat.Bgra8,
    //      BitmapAlphaMode.Premultiplied
    //    );
    //  }

    //  var softwareBitmapeSource = new SoftwareBitmapSource();
    //  await softwareBitmapeSource.SetBitmapAsync(softwareBitmap);
    //  image.Source = softwareBitmapeSource;
    //}

    //public async Task RenderRawImage()
    //{
    //  //const int WIDTH = 4318;
    //  //const int HEIGHT = 4320;
    //  var picker = new FileOpenPicker();
    //  picker.ViewMode = PickerViewMode.List;
    //  picker.SuggestedStartLocation = PickerLocationId.ComputerFolder;
    //  picker.FileTypeFilter.Add(".raw");

    //  StorageFile file = await picker.PickSingleFileAsync();
    //  if (file != null)
    //  {
    //    Progressing = true;
    //    using (var stream = await file.OpenAsync(FileAccessMode.Read))
    //    {
    //      var endian = await getByteArrayFromStream(stream, true);
    //      var normal = await getByteArrayFromStream(stream);

    //      //await setImageSource(GreyImageLeft, normal);
    //      //await setImageSource(GreyImageRight, endian);
    //    }
    //    Progressing = false;
    //  }
    //}

    //private async Task ReadBinaryData()
    //{
    //  var picker = new FileOpenPicker();
    //  picker.ViewMode = PickerViewMode.List;
    //  picker.SuggestedStartLocation = PickerLocationId.ComputerFolder;
    //  picker.FileTypeFilter.Add("*");

    //  StorageFile file = await picker.PickSingleFileAsync();
    //  if (file != null)
    //  {
    //    LoadProgress.IsActive = true;
    //    LoadProgress.Visibility = Visibility.Visible;
    //    try
    //    {
    //      _data.Clear();
    //      //var rows = default(int);
    //      var maxInputValue = double.MinValue;
    //      var minInputValue = double.MaxValue;

    //      const int WIDTH = 4320;
    //      const int HEIGHT = 4320;

    //      var content = await FileIO.ReadBufferAsync(file);

    //      var serializer = MessagePackSerializer.Deserialize<Matrix>(content.AsStream());
    //      var cols = serializer.cols;
    //      var rows = serializer.rows;
    //      var data = serializer.data;

    //      if (content.Length != WIDTH * HEIGHT * sizeof(float))
    //      {
    //        return;
    //      }

    //      using (var reader = new BinaryReader(content.AsStream()))
    //      {
    //        for (int row = 0; row < HEIGHT; ++row)
    //        {
    //          var values = new List<double>();
    //          for (int col = 0; col < WIDTH; ++col)
    //          {
    //            values.Add(reader.ReadSingle());
    //          }
    //          maxInputValue = Math.Max(maxInputValue, values.Max());
    //          minInputValue = Math.Min(minInputValue, values.Min());
    //          _data.Add(row, values);
    //        }
    //      }

    //      if (_data.Count > 0)
    //      {
    //        GraphMoveY.Maximum = _data.Count - 1;
    //        GraphMoveY.Minimum = 0;
    //        GraphMoveY.Value = 0;

    //        GraphScaleY.Maximum = maxInputValue;
    //        GraphScaleY.Minimum = minInputValue;
    //        GraphScaleY.Value = maxInputValue;

    //        GraphMoveX.Maximum = _data[0].Count - 1;
    //        GraphMoveX.Minimum = 0;
    //        GraphMoveX.Value = 0;

    //        GraphScaleX.Maximum = 2;    // 
    //        GraphScaleX.Minimum = GraphCanvas.ActualWidth / _data[0].Count;    // da passt ALLES rein
    //        GraphScaleX.Value = GraphCanvas.ActualWidth / _data[0].Count;
    //        GraphScaleX.SnapsTo = SliderSnapsTo.StepValues;
    //        GraphScaleX.StepFrequency = Math.Abs(GraphScaleX.Maximum - GraphScaleX.Minimum) / 10;

    //        GraphCanvas.Invalidate();
    //      }
    //    }
    //    catch (Exception ex)
    //    {
    //      Debug.WriteLine(ex.Message);
    //      throw;
    //    }
    //    LoadProgress.IsActive = false;
    //    LoadProgress.Visibility = Visibility.Collapsed;
    //  }
    //}

    //private async Task ReadInputData()
    //{
    //  var picker = new FileOpenPicker();
    //  picker.ViewMode = PickerViewMode.List;
    //  picker.SuggestedStartLocation = PickerLocationId.ComputerFolder;
    //  picker.FileTypeFilter.Add(".txt");
    //  picker.FileTypeFilter.Add(".log");
    //  picker.FileTypeFilter.Add(".dat");

    //  StorageFile file = await picker.PickSingleFileAsync();
    //  if (file != null)
    //  {
    //    LoadProgress.IsActive = true;
    //    LoadProgress.Visibility = Visibility.Visible;
    //    try
    //    {
    //      _data.Clear();
    //      var rows = default(int);
    //      var maxInputValue = double.MinValue;
    //      var minInputValue = double.MaxValue;
    //      var content = await FileIO.ReadLinesAsync(file);

    //      foreach (var line in content)
    //      {
    //        var fields = line.Split(';');
    //        var values = new List<double>();
    //        foreach (var field in fields)
    //        {
    //          values.Add(Convert.ToDouble(field, CultureInfo.InvariantCulture));
    //        }
    //        maxInputValue = Math.Max(maxInputValue, values.Max());
    //        minInputValue = Math.Min(minInputValue, values.Min());
    //        _data.Add(rows++, values);
    //      }

    //      if (_data.Count > 0)
    //      {
    //        GraphMoveY.Maximum = _data.Count - 1;
    //        GraphMoveY.Minimum = 0;
    //        GraphMoveY.Value = 0;

    //        GraphScaleY.Maximum = maxInputValue;
    //        GraphScaleY.Minimum = minInputValue;
    //        GraphScaleY.Value = maxInputValue;

    //        GraphMoveX.Maximum = _data[0].Count - 1;
    //        GraphMoveX.Minimum = 0;
    //        GraphMoveX.Value = 0;

    //        GraphScaleX.Maximum = 2;    // 
    //        GraphScaleX.Minimum = GraphCanvas.ActualWidth / _data[0].Count;    // da passt ALLES rein
    //        GraphScaleX.Value = GraphCanvas.ActualWidth / _data[0].Count;
    //        GraphScaleX.SnapsTo = SliderSnapsTo.StepValues;
    //        GraphScaleX.StepFrequency = Math.Abs(GraphScaleX.Maximum - GraphScaleX.Minimum) / 10;

    //        GraphCanvas.Invalidate();
    //      }
    //    }
    //    catch (Exception ex)
    //    {
    //      Debug.WriteLine(ex.Message);
    //      throw;
    //    }
    //    LoadProgress.IsActive = false;
    //    LoadProgress.Visibility = Visibility.Collapsed;
    //  }
    //}

    #endregion Methods
  }
}
