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

namespace ChartInUWP
{
  public class ChartLoader
  {
    #region Fields

    #endregion Fields

    public ChartLoader()
    {

    }

    #region Methods

    public async Task LoadFileSelection()
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
          //_data.Clear();
          var maxInputValue = double.MinValue;
          var minInputValue = double.MaxValue;

          const int WIDTH = 4320;
          const int HEIGHT = 4320;

          var content = await FileIO.ReadBufferAsync(file);

          var mtx = new Matrix
          {
            cols = 2,
            rows = 3,
            data = new double[] { 1, 2, 3 }
          };

          var serial = MessagePackSerializer.Serialize<Matrix>(mtx);
          var matrix = MessagePackSerializer.Deserialize<Matrix>(serial);
          var a = matrix.cols;
          var b = matrix.rows;
          var c = matrix.data;

          var serializer = MessagePackSerializer.Deserialize<Matrix>(content.AsStream());
          var cols = serializer.cols;
          var rows = serializer.rows;
          var data = serializer.data;

          if (content.Length != WIDTH * HEIGHT * sizeof(float))
          {
            return;
          }

          using (var reader = new BinaryReader(content.AsStream()))
          {
            for (int row = 0; row < HEIGHT; ++row)
            {
              var values = new List<double>();
              for (int col = 0; col < WIDTH; ++col)
              {
                values.Add(reader.ReadSingle());
              }
              maxInputValue = Math.Max(maxInputValue, values.Max());
              minInputValue = Math.Min(minInputValue, values.Min());
              //_data.Add(row, values);
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

    #endregion Methods
  }
}
