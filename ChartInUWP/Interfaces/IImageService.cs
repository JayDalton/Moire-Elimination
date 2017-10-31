using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;

namespace ChartInUWP.Interfaces
{
  public interface IImageService
  {
    void ClearValues();
    void ClearScreen(Color color);
    void AddLineValues(int idx, float[] values);
  }
}
