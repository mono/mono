namespace System.Web.Caching
{
  internal struct ExpiresEntryRef
  {
    internal static readonly ExpiresEntryRef INVALID = new ExpiresEntryRef(0, 0);
    private const uint ENTRY_MASK = 255;
    private const uint PAGE_MASK = 4294967040;
    private const int PAGE_SHIFT = 8;
    private uint _ref;

    internal ExpiresEntryRef(int pageIndex, int entryIndex)
    {
      this._ref = (uint) (pageIndex << 8 | entryIndex & (int) byte.MaxValue);
    }

    public override bool Equals(object value)
    {
      if (value is ExpiresEntryRef)
        return (int) this._ref == (int) ((ExpiresEntryRef) value)._ref;
      return false;
    }

    public static bool operator !=(ExpiresEntryRef r1, ExpiresEntryRef r2)
    {
      return (int) r1._ref != (int) r2._ref;
    }

    public static bool operator ==(ExpiresEntryRef r1, ExpiresEntryRef r2)
    {
      return (int) r1._ref == (int) r2._ref;
    }

    public override int GetHashCode()
    {
      return (int) this._ref;
    }

    internal int PageIndex
    {
      get
      {
        return (int) (this._ref >> 8);
      }
    }

    internal int Index
    {
      get
      {
        return (int) this._ref & (int) byte.MaxValue;
      }
    }

    internal bool IsInvalid
    {
      get
      {
        return (int) this._ref == 0;
      }
    }
  }
}
