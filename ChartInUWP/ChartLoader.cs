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

    private ChartMatrix _matrix;

    #endregion Fields

    public ChartLoader() {}

    #region Properties

    public uint Rows => _matrix.rows;

    public uint Cols => _matrix.cols;

    public float GlobalMaxValue { get; private set; }

    public float GlobalMinValue { get; private set; }

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

    public async Task LoadFromFileSelection()
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
          _matrix = MessagePackSerializer.Deserialize<ChartMatrix>(content.AsStream());

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

    #endregion Methods
  }
}
