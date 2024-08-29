using System.Runtime.InteropServices;

namespace System.Web.Caching
{
  [StructLayout(LayoutKind.Explicit)]
  internal struct UsageEntry
  {
    [FieldOffset(0)]
    internal UsageEntryLink _ref1;
    [FieldOffset(4)]
    internal int _cFree;
    [FieldOffset(8)]
    internal UsageEntryLink _ref2;
    [FieldOffset(16)]
    internal DateTime _utcDate;
    [FieldOffset(24)]
    internal CacheEntry _cacheEntry;
  }
}
