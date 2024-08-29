namespace System.Web.Caching
{
  internal struct UsageEntryRef
  {
    internal static readonly UsageEntryRef INVALID = new UsageEntryRef(0, 0);
    private const uint ENTRY_MASK = 255;
    private const uint PAGE_MASK = 4294967040;
    private const int PAGE_SHIFT = 8;
    private uint _ref;

    internal UsageEntryRef(int pageIndex, int entryIndex)
    {
      this._ref = (uint) (pageIndex << 8 | entryIndex & (int) byte.MaxValue);
    }

    public override bool Equals(object value)
    {
      if (value is UsageEntryRef)
        return (int) this._ref == (int) ((UsageEntryRef) value)._ref;
      return false;
    }

    public static bool operator ==(UsageEntryRef r1, UsageEntryRef r2)
    {
      return (int) r1._ref == (int) r2._ref;
    }

    public static bool operator !=(UsageEntryRef r1, UsageEntryRef r2)
    {
      return (int) r1._ref != (int) r2._ref;
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

    internal int Ref1Index
    {
      get
      {
        return (int) (sbyte) ((int) this._ref & (int) byte.MaxValue);
      }
    }

    internal int Ref2Index
    {
      get
      {
        return (int) -(sbyte) ((int) this._ref & (int) byte.MaxValue);
      }
    }

    internal bool IsRef1
    {
      get
      {
        return (int) (sbyte) ((int) this._ref & (int) byte.MaxValue) > 0;
      }
    }

    internal bool IsRef2
    {
      get
      {
        return (int) (sbyte) ((int) this._ref & (int) byte.MaxValue) < 0;
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
