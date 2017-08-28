using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Pickers;

namespace ChartInUWP
{
  public class ChartViewModel : ViewModelBase
  {
    #region Fields

    private bool progressing = false;
    private ChartLoader chartLoader;
    private ChartRenderer chartRenderer;

    #endregion Fields

    public ChartViewModel()
    {
      chartLoader = new ChartLoader();
      chartRenderer = new ChartRenderer();
    }

    #region Properties

    public bool Progressing
    {
      get { return progressing; }
      set { SetProperty(ref progressing, value); }
    }

    #endregion Properties

    #region Methods

    public async Task LoadChartFileData()
    {
      await chartLoader.LoadFileSelection();
    }

    #endregion Methods
  }
}
