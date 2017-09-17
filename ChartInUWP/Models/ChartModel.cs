using Dicom;
using Dicom.Imaging;
using Dicom.Imaging.Render;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media;

namespace ChartInUWP
{
  public class ChartModel
  {
    #region Fields

    private MatrixStruct<float> _matrix;
    private DicomFile _dicomFile;
    //private ImageSource _imageSource;

    #endregion Fields

    public ChartModel() {}

    #region Properties

    public uint Rows => _matrix.rows;

    public uint Cols => _matrix.cols;

    public float GlobalMaxValue { get; private set; }

    public float GlobalMinValue { get; private set; }

    public ImageSource ImageSource { get; private set; }

    #endregion Properties

    #region Methods

    // GetRow(0)
    public IEnumerable<float> GetRow(int row)
    {
      var start = row < _matrix.rows ? row * _matrix.cols : 0;
      return _matrix.data.Skip(start).Take(_matrix.cols);
    }

    // GetRow(2, 20, 15)
    public IEnumerable<float> GetRowRange(int row, int col = 0, int len = 0)
    {
      var start = row < _matrix.rows ? row * _matrix.cols : 0;
      var length = len < _matrix.cols ? len : 0;
      return _matrix.data.Skip(start).Take(length);
    }

    public async Task LoadFromPackedFileAsync()
    {
      var picker = new FileOpenPicker();
      picker.ViewMode = PickerViewMode.List;
      picker.SuggestedStartLocation = PickerLocationId.ComputerFolder;
      picker.FileTypeFilter.Add("*");

      StorageFile file = await picker.PickSingleFileAsync();
      if (file != null)
      {
        try
        {
          var content = await FileIO.ReadBufferAsync(file);
          _matrix = MessagePackSerializer.Deserialize<MatrixStruct<float>>(content.AsStream());

          GlobalMaxValue = _matrix.data.Max();
          GlobalMinValue = _matrix.data.Min();
        }
        catch (Exception ex)
        {
          Debug.WriteLine(ex.Message);
          throw;
        }
      }
    }

    public async Task LoadFromPlainFileAsync()
    {
      //const int WIDTH = 4318;
      //const int HEIGHT = 4320;
      var picker = new FileOpenPicker();
      picker.ViewMode = PickerViewMode.List;
      picker.SuggestedStartLocation = PickerLocationId.ComputerFolder;
      picker.FileTypeFilter.Add("*");

      StorageFile file = await picker.PickSingleFileAsync();
      if (file != null)
      {
        using (var stream = await file.OpenAsync(FileAccessMode.Read))
        {
          var endian = await getByteArrayFromStream(stream, true);
          var normal = await getByteArrayFromStream(stream);

          //await setImageSource(GreyImageLeft, normal);
          //await setImageSource(GreyImageRight, endian);
        }
      }
    }

    private async Task<float[]> getFloatListFromStream(IRandomAccessStream stream, ushort rows, ushort cols)
    {
      var result = new float[stream.Size];

      return result;
    }

    private async Task<byte[]> getByteArrayFromStream(IRandomAccessStream stream, bool swap = false)
    {
      var result = new byte[stream.Size];
      if (swap)
      {
        using (var output = new InMemoryRandomAccessStream())
        {
          using (var reader = new DataReader(stream.GetInputStreamAt(0)))
          {
            //reader.ByteOrder = ByteOrder.LittleEndian;
            using (var writer = new DataWriter(output))
            {
              //writer.ByteOrder = ByteOrder.BigEndian;
              await reader.LoadAsync((uint)stream.Size);
              while (0 < reader.UnconsumedBufferLength)
              {
                var number = reader.ReadUInt16();
                var bytes_number = BitConverter.GetBytes(number);
                writer.WriteBytes(bytes_number);
              }
              await writer.StoreAsync();
              await writer.FlushAsync();
              writer.DetachStream();
              output.Seek(0);
            }
            reader.DetachStream();
          }
          await output.ReadAsync(result.AsBuffer(), (uint)stream.Size, InputStreamOptions.None);
          var folder = await DownloadsFolder.CreateFileAsync(DateTime.UtcNow.Ticks + ".out");
          using (var file = await folder.OpenStreamForWriteAsync())
          {
            await file.WriteAsync(result, 0, result.Length);
            await file.FlushAsync();
          }
        }
      }
      else
      {
        await stream.ReadAsync(result.AsBuffer(), (uint)stream.Size, InputStreamOptions.None);
      }
      return result;
    }

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
