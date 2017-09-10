using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChartInUWP
{
  public class ImageRenderer
  {
    #region Fields

    private DicomLoader _imageLoader;

    #endregion Fields

    public ImageRenderer()
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
