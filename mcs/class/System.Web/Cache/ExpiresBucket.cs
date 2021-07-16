namespace System.Web.Caching
{
  internal sealed class ExpiresBucket
  {
    private static readonly TimeSpan COUNT_INTERVAL = new TimeSpan(CacheExpires._tsPerBucket.Ticks / 4L);
    internal const int NUM_ENTRIES = 127;
    private const int LENGTH_ENTRIES = 128;
    private const int MIN_PAGES_INCREMENT = 10;
    private const int MAX_PAGES_INCREMENT = 340;
    private const double MIN_LOAD_FACTOR = 0.5;
    private const int COUNTS_LENGTH = 4;
    private readonly CacheExpires _cacheExpires;
    private readonly byte _bucket;
    private ExpiresPage[] _pages;
    private int _cEntriesInUse;
    private int _cPagesInUse;
    private int _cEntriesInFlush;
    private int _minEntriesInUse;
    private ExpiresPageList _freePageList;
    private ExpiresPageList _freeEntryList;
    private bool _blockReduce;
    private DateTime _utcMinExpires;
    private int[] _counts;
    private DateTime _utcLastCountReset;

    internal ExpiresBucket(CacheExpires cacheExpires, byte bucket, DateTime utcNow)
    {
      this._cacheExpires = cacheExpires;
      this._bucket = bucket;
      this._counts = new int[4];
      this.ResetCounts(utcNow);
      this.InitZeroPages();
    }

    private void InitZeroPages()
    {
      this._pages = (ExpiresPage[]) null;
      this._minEntriesInUse = -1;
      this._freePageList._head = -1;
      this._freePageList._tail = -1;
      this._freeEntryList._head = -1;
      this._freeEntryList._tail = -1;
    }

    private void ResetCounts(DateTime utcNow)
    {
      this._utcLastCountReset = utcNow;
      this._utcMinExpires = DateTime.MaxValue;
      for (int index = 0; index < this._counts.Length; ++index)
        this._counts[index] = 0;
    }

    private int GetCountIndex(DateTime utcExpires)
    {
      int val1 = 0;
      TimeSpan timeSpan = utcExpires - this._utcLastCountReset;
      long ticks1 = timeSpan.Ticks;
      timeSpan = ExpiresBucket.COUNT_INTERVAL;
      long ticks2 = timeSpan.Ticks;
      int val2 = (int) (ticks1 / ticks2);
      return Math.Max(val1, val2);
    }

    private void AddCount(DateTime utcExpires)
    {
      int countIndex = this.GetCountIndex(utcExpires);
      for (int index = this._counts.Length - 1; index >= countIndex; --index)
        ++this._counts[index];
      if (!(utcExpires < this._utcMinExpires))
        return;
      this._utcMinExpires = utcExpires;
    }

    private void RemoveCount(DateTime utcExpires)
    {
      int countIndex = this.GetCountIndex(utcExpires);
      for (int index = this._counts.Length - 1; index >= countIndex; --index)
        --this._counts[index];
    }

    private int GetExpiresCount(DateTime utcExpires)
    {
      if (utcExpires < this._utcMinExpires)
        return 0;
      int countIndex = this.GetCountIndex(utcExpires);
      if (countIndex >= this._counts.Length)
        return this._cEntriesInUse;
      return this._counts[countIndex];
    }

    private void AddToListHead(int pageIndex, ref ExpiresPageList list)
    {
      this._pages[pageIndex]._pagePrev = -1;
      this._pages[pageIndex]._pageNext = list._head;
      if (list._head != -1)
        this._pages[list._head]._pagePrev = pageIndex;
      else
        list._tail = pageIndex;
      list._head = pageIndex;
    }

    private void AddToListTail(int pageIndex, ref ExpiresPageList list)
    {
      this._pages[pageIndex]._pageNext = -1;
      this._pages[pageIndex]._pagePrev = list._tail;
      if (list._tail != -1)
        this._pages[list._tail]._pageNext = pageIndex;
      else
        list._head = pageIndex;
      list._tail = pageIndex;
    }

    private int RemoveFromListHead(ref ExpiresPageList list)
    {
      int head = list._head;
      this.RemoveFromList(head, ref list);
      return head;
    }

    private void RemoveFromList(int pageIndex, ref ExpiresPageList list)
    {
      if (this._pages[pageIndex]._pagePrev != -1)
        this._pages[this._pages[pageIndex]._pagePrev]._pageNext = this._pages[pageIndex]._pageNext;
      else
        list._head = this._pages[pageIndex]._pageNext;
      if (this._pages[pageIndex]._pageNext != -1)
        this._pages[this._pages[pageIndex]._pageNext]._pagePrev = this._pages[pageIndex]._pagePrev;
      else
        list._tail = this._pages[pageIndex]._pagePrev;
      this._pages[pageIndex]._pagePrev = -1;
      this._pages[pageIndex]._pageNext = -1;
    }

    private void MoveToListHead(int pageIndex, ref ExpiresPageList list)
    {
      if (list._head == pageIndex)
        return;
      this.RemoveFromList(pageIndex, ref list);
      this.AddToListHead(pageIndex, ref list);
    }

    private void MoveToListTail(int pageIndex, ref ExpiresPageList list)
    {
      if (list._tail == pageIndex)
        return;
      this.RemoveFromList(pageIndex, ref list);
      this.AddToListTail(pageIndex, ref list);
    }

    private void UpdateMinEntries()
    {
      if (this._cPagesInUse <= 1)
      {
        this._minEntriesInUse = -1;
      }
      else
      {
        this._minEntriesInUse = (int) ((double) (this._cPagesInUse * (int) sbyte.MaxValue) * 0.5);
        if (this._minEntriesInUse - 1 <= (this._cPagesInUse - 1) * (int) sbyte.MaxValue)
          return;
        this._minEntriesInUse = -1;
      }
    }

    private void RemovePage(int pageIndex)
    {
      this.RemoveFromList(pageIndex, ref this._freeEntryList);
      this.AddToListHead(pageIndex, ref this._freePageList);
      this._pages[pageIndex]._entries = (ExpiresEntry[]) null;
      --this._cPagesInUse;
      if (this._cPagesInUse == 0)
        this.InitZeroPages();
      else
        this.UpdateMinEntries();
    }

    private ExpiresEntryRef GetFreeExpiresEntry()
    {
      int head = this._freeEntryList._head;
      ExpiresEntry[] entries = this._pages[head]._entries;
      int index = entries[0]._next.Index;
      entries[0]._next = entries[index]._next;
      --entries[0]._cFree;
      if (entries[0]._cFree == 0)
        this.RemoveFromList(head, ref this._freeEntryList);
      return new ExpiresEntryRef(head, index);
    }

    private void AddExpiresEntryToFreeList(ExpiresEntryRef entryRef)
    {
      ExpiresEntry[] entries = this._pages[entryRef.PageIndex]._entries;
      int index = entryRef.Index;
      entries[index]._cFree = 0;
      entries[index]._next = entries[0]._next;
      entries[0]._next = entryRef;
      --this._cEntriesInUse;
      int pageIndex = entryRef.PageIndex;
      ++entries[0]._cFree;
      if (entries[0]._cFree == 1)
      {
        this.AddToListHead(pageIndex, ref this._freeEntryList);
      }
      else
      {
        if (entries[0]._cFree != (int) sbyte.MaxValue)
          return;
        this.RemovePage(pageIndex);
      }
    }

    private void Expand()
    {
      if (this._freePageList._head == -1)
      {
        int index1 = this._pages != null ? this._pages.Length : 0;
        int val2 = index1 * 2;
        ExpiresPage[] expiresPageArray = new ExpiresPage[Math.Min(Math.Max(index1 + 10, val2), index1 + 340)];
        for (int index2 = 0; index2 < index1; ++index2)
          expiresPageArray[index2] = this._pages[index2];
        for (int index2 = index1; index2 < expiresPageArray.Length; ++index2)
        {
          expiresPageArray[index2]._pagePrev = index2 - 1;
          expiresPageArray[index2]._pageNext = index2 + 1;
        }
        expiresPageArray[index1]._pagePrev = -1;
        expiresPageArray[expiresPageArray.Length - 1]._pageNext = -1;
        this._freePageList._head = index1;
        this._freePageList._tail = expiresPageArray.Length - 1;
        this._pages = expiresPageArray;
      }
      int pageIndex = this.RemoveFromListHead(ref this._freePageList);
      this.AddToListHead(pageIndex, ref this._freeEntryList);
      ExpiresEntry[] expiresEntryArray = new ExpiresEntry[128];
      expiresEntryArray[0]._cFree = (int) sbyte.MaxValue;
      for (int index = 0; index < expiresEntryArray.Length - 1; ++index)
        expiresEntryArray[index]._next = new ExpiresEntryRef(pageIndex, index + 1);
      expiresEntryArray[expiresEntryArray.Length - 1]._next = ExpiresEntryRef.INVALID;
      this._pages[pageIndex]._entries = expiresEntryArray;
      ++this._cPagesInUse;
      this.UpdateMinEntries();
    }

    private void Reduce()
    {
      if (this._cEntriesInUse >= this._minEntriesInUse || this._blockReduce)
        return;
      int num = 63;
      int tail = this._freeEntryList._tail;
      int pageIndex = this._freeEntryList._head;
      while (true)
      {
        int pageNext = this._pages[pageIndex]._pageNext;
        if (this._pages[pageIndex]._entries[0]._cFree > num)
          this.MoveToListTail(pageIndex, ref this._freeEntryList);
        else
          this.MoveToListHead(pageIndex, ref this._freeEntryList);
        if (pageIndex != tail)
          pageIndex = pageNext;
        else
          break;
      }
      while (this._freeEntryList._tail != -1)
      {
        ExpiresEntry[] entries = this._pages[this._freeEntryList._tail]._entries;
        if (this._cPagesInUse * (int) sbyte.MaxValue - entries[0]._cFree - this._cEntriesInUse < (int) sbyte.MaxValue - entries[0]._cFree)
          break;
        for (int index = 1; index < entries.Length; ++index)
        {
          if (entries[index]._cacheEntry != null)
          {
            ExpiresEntryRef freeExpiresEntry = this.GetFreeExpiresEntry();
            entries[index]._cacheEntry.ExpiresEntryRef = freeExpiresEntry;
            this._pages[freeExpiresEntry.PageIndex]._entries[freeExpiresEntry.Index] = entries[index];
            ++entries[0]._cFree;
          }
        }
        this.RemovePage(this._freeEntryList._tail);
      }
    }

    internal void AddCacheEntry(CacheEntry cacheEntry)
    {
      lock (this)
      {
        if ((cacheEntry.State & (CacheEntry.EntryState.AddingToCache | CacheEntry.EntryState.AddedToCache)) == CacheEntry.EntryState.NotInCache)
          return;
        ExpiresEntryRef expiresEntryRef = cacheEntry.ExpiresEntryRef;
        if ((int) cacheEntry.ExpiresBucket != (int) byte.MaxValue || !expiresEntryRef.IsInvalid)
          return;
        if (this._freeEntryList._head == -1)
          this.Expand();
        ExpiresEntryRef freeExpiresEntry = this.GetFreeExpiresEntry();
        cacheEntry.ExpiresBucket = this._bucket;
        cacheEntry.ExpiresEntryRef = freeExpiresEntry;
        ExpiresEntry[] entries = this._pages[freeExpiresEntry.PageIndex]._entries;
        int index = freeExpiresEntry.Index;
        entries[index]._cacheEntry = cacheEntry;
        entries[index]._utcExpires = cacheEntry.UtcExpires;
        this.AddCount(cacheEntry.UtcExpires);
        ++this._cEntriesInUse;
        if ((cacheEntry.State & (CacheEntry.EntryState.AddingToCache | CacheEntry.EntryState.AddedToCache)) != CacheEntry.EntryState.NotInCache)
          return;
        this.RemoveCacheEntryNoLock(cacheEntry);
      }
    }

    private void RemoveCacheEntryNoLock(CacheEntry cacheEntry)
    {
      ExpiresEntryRef expiresEntryRef = cacheEntry.ExpiresEntryRef;
      if ((int) cacheEntry.ExpiresBucket != (int) this._bucket || expiresEntryRef.IsInvalid)
        return;
      ExpiresEntry[] entries = this._pages[expiresEntryRef.PageIndex]._entries;
      int index = expiresEntryRef.Index;
      this.RemoveCount(entries[index]._utcExpires);
      cacheEntry.ExpiresBucket = byte.MaxValue;
      cacheEntry.ExpiresEntryRef = ExpiresEntryRef.INVALID;
      entries[index]._cacheEntry = (CacheEntry) null;
      this.AddExpiresEntryToFreeList(expiresEntryRef);
      if (this._cEntriesInUse == 0)
        this.ResetCounts(DateTime.UtcNow);
      this.Reduce();
    }

    internal void RemoveCacheEntry(CacheEntry cacheEntry)
    {
      lock (this)
        this.RemoveCacheEntryNoLock(cacheEntry);
    }

    internal void UtcUpdateCacheEntry(CacheEntry cacheEntry, DateTime utcExpires)
    {
      lock (this)
      {
        ExpiresEntryRef expiresEntryRef = cacheEntry.ExpiresEntryRef;
        if ((int) cacheEntry.ExpiresBucket != (int) this._bucket || expiresEntryRef.IsInvalid)
          return;
        ExpiresEntry[] entries = this._pages[expiresEntryRef.PageIndex]._entries;
        int index = expiresEntryRef.Index;
        this.RemoveCount(entries[index]._utcExpires);
        this.AddCount(utcExpires);
        entries[index]._utcExpires = utcExpires;
        cacheEntry.UtcExpires = utcExpires;
      }
    }

    internal int FlushExpiredItems(DateTime utcNow, bool useInsertBlock)
    {
      if (this._cEntriesInUse == 0 || this.GetExpiresCount(utcNow) == 0)
        return 0;
      ExpiresEntryRef expiresEntryRef = ExpiresEntryRef.INVALID;
      int num1 = 0;
      try
      {
        if (useInsertBlock)
          this._cacheExpires.CacheSingle.BlockInsertIfNeeded();
        lock (this)
        {
          if (this._cEntriesInUse == 0 || this.GetExpiresCount(utcNow) == 0)
            return 0;
          this.ResetCounts(utcNow);
          int cPagesInUse = this._cPagesInUse;
          for (int pageIndex = 0; pageIndex < this._pages.Length; ++pageIndex)
          {
            ExpiresEntry[] entries = this._pages[pageIndex]._entries;
            if (entries != null)
            {
              int num2 = (int) sbyte.MaxValue - entries[0]._cFree;
              for (int entryIndex = 1; entryIndex < entries.Length; ++entryIndex)
              {
                CacheEntry cacheEntry = entries[entryIndex]._cacheEntry;
                if (cacheEntry != null)
                {
                  if (entries[entryIndex]._utcExpires > utcNow)
                  {
                    this.AddCount(entries[entryIndex]._utcExpires);
                  }
                  else
                  {
                    cacheEntry.ExpiresBucket = byte.MaxValue;
                    cacheEntry.ExpiresEntryRef = ExpiresEntryRef.INVALID;
                    entries[entryIndex]._cFree = 1;
                    entries[entryIndex]._next = expiresEntryRef;
                    expiresEntryRef = new ExpiresEntryRef(pageIndex, entryIndex);
                    ++num1;
                    ++this._cEntriesInFlush;
                  }
                  --num2;
                  if (num2 == 0)
                    break;
                }
              }
              --cPagesInUse;
              if (cPagesInUse == 0)
                break;
            }
          }
          if (num1 == 0)
            return 0;
          this._blockReduce = true;
        }
      }
      finally
      {
        if (useInsertBlock)
          this._cacheExpires.CacheSingle.UnblockInsert();
      }
      CacheSingle cacheSingle = this._cacheExpires.CacheSingle;
      ExpiresEntryRef entryRef;
      ExpiresEntryRef next1;
      for (entryRef = expiresEntryRef; !entryRef.IsInvalid; entryRef = next1)
      {
        ExpiresEntry[] entries = this._pages[entryRef.PageIndex]._entries;
        int index = entryRef.Index;
        next1 = entries[index]._next;
        CacheEntry cacheEntry = entries[index]._cacheEntry;
        entries[index]._cacheEntry = (CacheEntry) null;
        cacheSingle.Remove((CacheKey) cacheEntry, CacheItemRemovedReason.Expired);
      }
      try
      {
        if (useInsertBlock)
          this._cacheExpires.CacheSingle.BlockInsertIfNeeded();
        lock (this)
        {
          ExpiresEntryRef next2;
          for (entryRef = expiresEntryRef; !entryRef.IsInvalid; entryRef = next2)
          {
            next2 = this._pages[entryRef.PageIndex]._entries[entryRef.Index]._next;
            --this._cEntriesInFlush;
            this.AddExpiresEntryToFreeList(entryRef);
          }
          this._blockReduce = false;
          this.Reduce();
        }
      }
      finally
      {
        if (useInsertBlock)
          this._cacheExpires.CacheSingle.UnblockInsert();
      }
      return num1;
    }
  }
}
