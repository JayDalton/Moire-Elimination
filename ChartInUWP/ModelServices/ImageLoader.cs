using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChartInUWP
{
  public class ImageLoader
  {
    #region Fields

    private DicomLoader _imageLoader;

    #endregion Fields

    public ImageLoader()
    {
      _imageLoader = new DicomLoader();
    }

    #region Properties
    #endregion Properties

    #region Events
    #endregion Events

    #region Methods

    public async Task LoadImageDataFromFile()
    {
      //await _imageLoader.LoadImageSelection();
    }

    #endregion Methods
  }
}
