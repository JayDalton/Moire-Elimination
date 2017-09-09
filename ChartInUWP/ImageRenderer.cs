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

    private ImageLoader _imageLoader;

    #endregion Fields

    public ImageRenderer()
    {
      _imageLoader = new ImageLoader();
    }

    #region Properties
    #endregion Properties

    #region Events
    #endregion Events

    #region Methods

    public async Task LoadImageDataFromFile()
    {
      await _imageLoader.LoadImageSelection();
    }

    #endregion Methods
  }
}
