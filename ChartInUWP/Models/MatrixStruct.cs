using MessagePack;

namespace ChartInUWP
{
  [MessagePackObject]
  public struct MatrixStruct<T>
  {
    [Key(0)]
    public ushort rows { get; set; }

    [Key(1)]
    public ushort cols { get; set; }

    [Key(2)]
    public T[] data { get; set; } // IList<short>
  }
}
