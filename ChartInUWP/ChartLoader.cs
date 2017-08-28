using System;
using System.Collections.Generic;
using System.Linq;
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


      }
    }

    #endregion Methods
  }
}
