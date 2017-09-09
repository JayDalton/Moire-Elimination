using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChartInUWP
{
  [MessagePackObject]
  public struct ChartMatrix
  {
    [Key(0)]
    public ushort rows { get; set; }

    [Key(1)]
    public ushort cols { get; set; }

    [Key(2)]
    public float[] data { get; set; } // IList<short>
  }
}
