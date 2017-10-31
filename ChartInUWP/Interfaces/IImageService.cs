using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChartInUWP.Interfaces
{
  public interface IImageService
  {
    void AddLineValues(int idx, float[] values);
  }
}
