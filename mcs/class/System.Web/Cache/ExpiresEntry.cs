using System.Runtime.InteropServices;

namespace System.Web.Caching
{
  [StructLayout(LayoutKind.Explicit)]
  internal struct ExpiresEntry
  {
    [FieldOffset(0)]
    internal DateTime _utcExpires;
    [FieldOffset(0)]
    internal ExpiresEntryRef _next;
    [FieldOffset(4)]
    internal int _cFree;
    [FieldOffset(8)]
    internal CacheEntry _cacheEntry;
  }
}
