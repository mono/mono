namespace System.Web.Caching
{
  internal sealed class UsageBucket
  {
    internal const int NUM_ENTRIES = 127;
    private const int LENGTH_ENTRIES = 128;
    private const int MIN_PAGES_INCREMENT = 10;
    private const int MAX_PAGES_INCREMENT = 340;
    private const double MIN_LOAD_FACTOR = 0.5;
    private CacheUsage _cacheUsage;
    private byte _bucket;
    private UsagePage[] _pages;
    private int _cEntriesInUse;
    private int _cPagesInUse;
    private int _cEntriesInFlush;
    private int _minEntriesInUse;
    private UsagePageList _freePageList;
    private UsagePageList _freeEntryList;
    private UsageEntryRef _lastRefHead;
    private UsageEntryRef _lastRefTail;
    private UsageEntryRef _addRef2Head;
    private bool _blockReduce;

    internal UsageBucket(CacheUsage cacheUsage, byte bucket)
    {
      this._cacheUsage = cacheUsage;
      this._bucket = bucket;
      this.InitZeroPages();
    }

    private void InitZeroPages()
    {
      this._pages = (UsagePage[]) null;
      this._minEntriesInUse = -1;
      this._freePageList._head = -1;
      this._freePageList._tail = -1;
      this._freeEntryList._head = -1;
      this._freeEntryList._tail = -1;
    }

    private void AddToListHead(int pageIndex, ref UsagePageList list)
    {
      this._pages[pageIndex]._pagePrev = -1;
      this._pages[pageIndex]._pageNext = list._head;
      if (list._head != -1)
        this._pages[list._head]._pagePrev = pageIndex;
      else
        list._tail = pageIndex;
      list._head = pageIndex;
    }

    private void AddToListTail(int pageIndex, ref UsagePageList list)
    {
      this._pages[pageIndex]._pageNext = -1;
      this._pages[pageIndex]._pagePrev = list._tail;
      if (list._tail != -1)
        this._pages[list._tail]._pageNext = pageIndex;
      else
        list._head = pageIndex;
      list._tail = pageIndex;
    }

    private int RemoveFromListHead(ref UsagePageList list)
    {
      int head = list._head;
      this.RemoveFromList(head, ref list);
      return head;
    }

    private void RemoveFromList(int pageIndex, ref UsagePageList list)
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

    private void MoveToListHead(int pageIndex, ref UsagePageList list)
    {
      if (list._head == pageIndex)
        return;
      this.RemoveFromList(pageIndex, ref list);
      this.AddToListHead(pageIndex, ref list);
    }

    private void MoveToListTail(int pageIndex, ref UsagePageList list)
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
      this._pages[pageIndex]._entries = (UsageEntry[]) null;
      --this._cPagesInUse;
      if (this._cPagesInUse == 0)
        this.InitZeroPages();
      else
        this.UpdateMinEntries();
    }

    private UsageEntryRef GetFreeUsageEntry()
    {
      int head = this._freeEntryList._head;
      UsageEntry[] entries = this._pages[head]._entries;
      int ref1Index = entries[0]._ref1._next.Ref1Index;
      entries[0]._ref1._next = entries[ref1Index]._ref1._next;
      --entries[0]._cFree;
      if (entries[0]._cFree == 0)
        this.RemoveFromList(head, ref this._freeEntryList);
      return new UsageEntryRef(head, ref1Index);
    }

    private void AddUsageEntryToFreeList(UsageEntryRef entryRef)
    {
      UsageEntry[] entries = this._pages[entryRef.PageIndex]._entries;
      int ref1Index = entryRef.Ref1Index;
      entries[ref1Index]._utcDate = DateTime.MinValue;
      entries[ref1Index]._ref1._prev = UsageEntryRef.INVALID;
      entries[ref1Index]._ref2._next = UsageEntryRef.INVALID;
      entries[ref1Index]._ref2._prev = UsageEntryRef.INVALID;
      entries[ref1Index]._ref1._next = entries[0]._ref1._next;
      entries[0]._ref1._next = entryRef;
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
        UsagePage[] usagePageArray = new UsagePage[Math.Min(Math.Max(index1 + 10, val2), index1 + 340)];
        for (int index2 = 0; index2 < index1; ++index2)
          usagePageArray[index2] = this._pages[index2];
        for (int index2 = index1; index2 < usagePageArray.Length; ++index2)
        {
          usagePageArray[index2]._pagePrev = index2 - 1;
          usagePageArray[index2]._pageNext = index2 + 1;
        }
        usagePageArray[index1]._pagePrev = -1;
        usagePageArray[usagePageArray.Length - 1]._pageNext = -1;
        this._freePageList._head = index1;
        this._freePageList._tail = usagePageArray.Length - 1;
        this._pages = usagePageArray;
      }
      int pageIndex = this.RemoveFromListHead(ref this._freePageList);
      this.AddToListHead(pageIndex, ref this._freeEntryList);
      UsageEntry[] usageEntryArray = new UsageEntry[128];
      usageEntryArray[0]._cFree = (int) sbyte.MaxValue;
      for (int index = 0; index < usageEntryArray.Length - 1; ++index)
        usageEntryArray[index]._ref1._next = new UsageEntryRef(pageIndex, index + 1);
      usageEntryArray[usageEntryArray.Length - 1]._ref1._next = UsageEntryRef.INVALID;
      this._pages[pageIndex]._entries = usageEntryArray;
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
        UsageEntry[] entries1 = this._pages[this._freeEntryList._tail]._entries;
        if (this._cPagesInUse * (int) sbyte.MaxValue - entries1[0]._cFree - this._cEntriesInUse < (int) sbyte.MaxValue - entries1[0]._cFree)
          break;
        for (int entryIndex = 1; entryIndex < entries1.Length; ++entryIndex)
        {
          if (entries1[entryIndex]._cacheEntry != null)
          {
            UsageEntryRef freeUsageEntry = this.GetFreeUsageEntry();
            UsageEntryRef usageEntryRef1 = new UsageEntryRef(freeUsageEntry.PageIndex, -freeUsageEntry.Ref1Index);
            UsageEntryRef usageEntryRef2 = new UsageEntryRef(this._freeEntryList._tail, entryIndex);
            UsageEntryRef usageEntryRef3 = new UsageEntryRef(usageEntryRef2.PageIndex, -usageEntryRef2.Ref1Index);
            entries1[entryIndex]._cacheEntry.UsageEntryRef = freeUsageEntry;
            UsageEntry[] entries2 = this._pages[freeUsageEntry.PageIndex]._entries;
            entries2[freeUsageEntry.Ref1Index] = entries1[entryIndex];
            ++entries1[0]._cFree;
            UsageEntryRef usageEntryRef4 = entries2[freeUsageEntry.Ref1Index]._ref1._prev;
            UsageEntryRef usageEntryRef5 = entries2[freeUsageEntry.Ref1Index]._ref1._next;
            if (usageEntryRef5 == usageEntryRef3)
              usageEntryRef5 = usageEntryRef1;
            if (usageEntryRef4.IsRef1)
              this._pages[usageEntryRef4.PageIndex]._entries[usageEntryRef4.Ref1Index]._ref1._next = freeUsageEntry;
            else if (usageEntryRef4.IsRef2)
              this._pages[usageEntryRef4.PageIndex]._entries[usageEntryRef4.Ref2Index]._ref2._next = freeUsageEntry;
            else
              this._lastRefHead = freeUsageEntry;
            if (usageEntryRef5.IsRef1)
              this._pages[usageEntryRef5.PageIndex]._entries[usageEntryRef5.Ref1Index]._ref1._prev = freeUsageEntry;
            else if (usageEntryRef5.IsRef2)
              this._pages[usageEntryRef5.PageIndex]._entries[usageEntryRef5.Ref2Index]._ref2._prev = freeUsageEntry;
            else
              this._lastRefTail = freeUsageEntry;
            usageEntryRef4 = entries2[freeUsageEntry.Ref1Index]._ref2._prev;
            if (usageEntryRef4 == usageEntryRef2)
              usageEntryRef4 = freeUsageEntry;
            usageEntryRef5 = entries2[freeUsageEntry.Ref1Index]._ref2._next;
            if (usageEntryRef4.IsRef1)
              this._pages[usageEntryRef4.PageIndex]._entries[usageEntryRef4.Ref1Index]._ref1._next = usageEntryRef1;
            else if (usageEntryRef4.IsRef2)
              this._pages[usageEntryRef4.PageIndex]._entries[usageEntryRef4.Ref2Index]._ref2._next = usageEntryRef1;
            else
              this._lastRefHead = usageEntryRef1;
            if (usageEntryRef5.IsRef1)
              this._pages[usageEntryRef5.PageIndex]._entries[usageEntryRef5.Ref1Index]._ref1._prev = usageEntryRef1;
            else if (usageEntryRef5.IsRef2)
              this._pages[usageEntryRef5.PageIndex]._entries[usageEntryRef5.Ref2Index]._ref2._prev = usageEntryRef1;
            else
              this._lastRefTail = usageEntryRef1;
            if (this._addRef2Head == usageEntryRef3)
              this._addRef2Head = usageEntryRef1;
          }
        }
        this.RemovePage(this._freeEntryList._tail);
      }
    }

    internal void AddCacheEntry(CacheEntry cacheEntry)
    {
      lock (this)
      {
        if (this._freeEntryList._head == -1)
          this.Expand();
        UsageEntryRef freeUsageEntry = this.GetFreeUsageEntry();
        UsageEntryRef usageEntryRef1 = new UsageEntryRef(freeUsageEntry.PageIndex, -freeUsageEntry.Ref1Index);
        cacheEntry.UsageEntryRef = freeUsageEntry;
        UsageEntry[] entries = this._pages[freeUsageEntry.PageIndex]._entries;
        int ref1Index = freeUsageEntry.Ref1Index;
        entries[ref1Index]._cacheEntry = cacheEntry;
        entries[ref1Index]._utcDate = DateTime.UtcNow;
        entries[ref1Index]._ref1._prev = UsageEntryRef.INVALID;
        entries[ref1Index]._ref2._next = this._addRef2Head;
        if (this._lastRefHead.IsInvalid)
        {
          entries[ref1Index]._ref1._next = usageEntryRef1;
          entries[ref1Index]._ref2._prev = freeUsageEntry;
          this._lastRefTail = usageEntryRef1;
        }
        else
        {
          entries[ref1Index]._ref1._next = this._lastRefHead;
          if (this._lastRefHead.IsRef1)
            this._pages[this._lastRefHead.PageIndex]._entries[this._lastRefHead.Ref1Index]._ref1._prev = freeUsageEntry;
          else if (this._lastRefHead.IsRef2)
            this._pages[this._lastRefHead.PageIndex]._entries[this._lastRefHead.Ref2Index]._ref2._prev = freeUsageEntry;
          else
            this._lastRefTail = freeUsageEntry;
          UsageEntryRef usageEntryRef2;
          UsageEntryRef usageEntryRef3;
          if (this._addRef2Head.IsInvalid)
          {
            usageEntryRef2 = this._lastRefTail;
            usageEntryRef3 = UsageEntryRef.INVALID;
          }
          else
          {
            usageEntryRef2 = this._pages[this._addRef2Head.PageIndex]._entries[this._addRef2Head.Ref2Index]._ref2._prev;
            usageEntryRef3 = this._addRef2Head;
          }
          entries[ref1Index]._ref2._prev = usageEntryRef2;
          if (usageEntryRef2.IsRef1)
            this._pages[usageEntryRef2.PageIndex]._entries[usageEntryRef2.Ref1Index]._ref1._next = usageEntryRef1;
          else if (usageEntryRef2.IsRef2)
            this._pages[usageEntryRef2.PageIndex]._entries[usageEntryRef2.Ref2Index]._ref2._next = usageEntryRef1;
          else
            this._lastRefHead = usageEntryRef1;
          if (usageEntryRef3.IsRef1)
            this._pages[usageEntryRef3.PageIndex]._entries[usageEntryRef3.Ref1Index]._ref1._prev = usageEntryRef1;
          else if (usageEntryRef3.IsRef2)
            this._pages[usageEntryRef3.PageIndex]._entries[usageEntryRef3.Ref2Index]._ref2._prev = usageEntryRef1;
          else
            this._lastRefTail = usageEntryRef1;
        }
        this._lastRefHead = freeUsageEntry;
        this._addRef2Head = usageEntryRef1;
        ++this._cEntriesInUse;
      }
    }

    private void RemoveEntryFromLastRefList(UsageEntryRef entryRef)
    {
      UsageEntry[] entries = this._pages[entryRef.PageIndex]._entries;
      int ref1Index = entryRef.Ref1Index;
      UsageEntryRef prev = entries[ref1Index]._ref1._prev;
      UsageEntryRef next = entries[ref1Index]._ref1._next;
      if (prev.IsRef1)
        this._pages[prev.PageIndex]._entries[prev.Ref1Index]._ref1._next = next;
      else if (prev.IsRef2)
        this._pages[prev.PageIndex]._entries[prev.Ref2Index]._ref2._next = next;
      else
        this._lastRefHead = next;
      if (next.IsRef1)
        this._pages[next.PageIndex]._entries[next.Ref1Index]._ref1._prev = prev;
      else if (next.IsRef2)
        this._pages[next.PageIndex]._entries[next.Ref2Index]._ref2._prev = prev;
      else
        this._lastRefTail = prev;
      prev = entries[ref1Index]._ref2._prev;
      next = entries[ref1Index]._ref2._next;
      UsageEntryRef usageEntryRef = new UsageEntryRef(entryRef.PageIndex, -entryRef.Ref1Index);
      if (prev.IsRef1)
        this._pages[prev.PageIndex]._entries[prev.Ref1Index]._ref1._next = next;
      else if (prev.IsRef2)
        this._pages[prev.PageIndex]._entries[prev.Ref2Index]._ref2._next = next;
      else
        this._lastRefHead = next;
      if (next.IsRef1)
        this._pages[next.PageIndex]._entries[next.Ref1Index]._ref1._prev = prev;
      else if (next.IsRef2)
        this._pages[next.PageIndex]._entries[next.Ref2Index]._ref2._prev = prev;
      else
        this._lastRefTail = prev;
      if (!(this._addRef2Head == usageEntryRef))
        return;
      this._addRef2Head = next;
    }

    internal void RemoveCacheEntry(CacheEntry cacheEntry)
    {
      lock (this)
      {
        UsageEntryRef usageEntryRef = cacheEntry.UsageEntryRef;
        if (usageEntryRef.IsInvalid)
          return;
        UsageEntry[] entries = this._pages[usageEntryRef.PageIndex]._entries;
        int ref1Index = usageEntryRef.Ref1Index;
        cacheEntry.UsageEntryRef = UsageEntryRef.INVALID;
        entries[ref1Index]._cacheEntry = (CacheEntry) null;
        this.RemoveEntryFromLastRefList(usageEntryRef);
        this.AddUsageEntryToFreeList(usageEntryRef);
        this.Reduce();
      }
    }

    internal void UpdateCacheEntry(CacheEntry cacheEntry)
    {
      lock (this)
      {
        UsageEntryRef usageEntryRef1 = cacheEntry.UsageEntryRef;
        if (usageEntryRef1.IsInvalid)
          return;
        UsageEntry[] entries = this._pages[usageEntryRef1.PageIndex]._entries;
        int ref1Index = usageEntryRef1.Ref1Index;
        UsageEntryRef usageEntryRef2 = new UsageEntryRef(usageEntryRef1.PageIndex, -usageEntryRef1.Ref1Index);
        UsageEntryRef prev = entries[ref1Index]._ref2._prev;
        UsageEntryRef next = entries[ref1Index]._ref2._next;
        if (prev.IsRef1)
          this._pages[prev.PageIndex]._entries[prev.Ref1Index]._ref1._next = next;
        else if (prev.IsRef2)
          this._pages[prev.PageIndex]._entries[prev.Ref2Index]._ref2._next = next;
        else
          this._lastRefHead = next;
        if (next.IsRef1)
          this._pages[next.PageIndex]._entries[next.Ref1Index]._ref1._prev = prev;
        else if (next.IsRef2)
          this._pages[next.PageIndex]._entries[next.Ref2Index]._ref2._prev = prev;
        else
          this._lastRefTail = prev;
        if (this._addRef2Head == usageEntryRef2)
          this._addRef2Head = next;
        entries[ref1Index]._ref2 = entries[ref1Index]._ref1;
        prev = entries[ref1Index]._ref2._prev;
        next = entries[ref1Index]._ref2._next;
        if (prev.IsRef1)
          this._pages[prev.PageIndex]._entries[prev.Ref1Index]._ref1._next = usageEntryRef2;
        else if (prev.IsRef2)
          this._pages[prev.PageIndex]._entries[prev.Ref2Index]._ref2._next = usageEntryRef2;
        else
          this._lastRefHead = usageEntryRef2;
        if (next.IsRef1)
          this._pages[next.PageIndex]._entries[next.Ref1Index]._ref1._prev = usageEntryRef2;
        else if (next.IsRef2)
          this._pages[next.PageIndex]._entries[next.Ref2Index]._ref2._prev = usageEntryRef2;
        else
          this._lastRefTail = usageEntryRef2;
        entries[ref1Index]._ref1._prev = UsageEntryRef.INVALID;
        entries[ref1Index]._ref1._next = this._lastRefHead;
        if (this._lastRefHead.IsRef1)
          this._pages[this._lastRefHead.PageIndex]._entries[this._lastRefHead.Ref1Index]._ref1._prev = usageEntryRef1;
        else if (this._lastRefHead.IsRef2)
          this._pages[this._lastRefHead.PageIndex]._entries[this._lastRefHead.Ref2Index]._ref2._prev = usageEntryRef1;
        else
          this._lastRefTail = usageEntryRef1;
        this._lastRefHead = usageEntryRef1;
      }
    }

    internal int FlushUnderUsedItems(int maxFlush, bool force, ref int publicEntriesFlushed, ref int ocEntriesFlushed)
    {
      if (this._cEntriesInUse == 0)
        return 0;
      UsageEntryRef usageEntryRef1 = UsageEntryRef.INVALID;
      int num = 0;
      try
      {
        this._cacheUsage.CacheSingle.BlockInsertIfNeeded();
        lock (this)
        {
          if (this._cEntriesInUse == 0)
            return 0;
          DateTime utcNow = DateTime.UtcNow;
          UsageEntryRef prev;
          for (UsageEntryRef usageEntryRef2 = this._lastRefTail; this._cEntriesInFlush < maxFlush && !usageEntryRef2.IsInvalid; usageEntryRef2 = prev)
          {
            prev = this._pages[usageEntryRef2.PageIndex]._entries[usageEntryRef2.Ref2Index]._ref2._prev;
            while (prev.IsRef1)
              prev = this._pages[prev.PageIndex]._entries[prev.Ref1Index]._ref1._prev;
            UsageEntry[] entries = this._pages[usageEntryRef2.PageIndex]._entries;
            int ref2Index = usageEntryRef2.Ref2Index;
            if (!force)
            {
              DateTime utcDate = entries[ref2Index]._utcDate;
              if (utcNow - utcDate <= CacheUsage.NEWADD_INTERVAL && utcNow >= utcDate)
                continue;
            }
            UsageEntryRef entryRef = new UsageEntryRef(usageEntryRef2.PageIndex, usageEntryRef2.Ref2Index);
            CacheEntry cacheEntry = entries[ref2Index]._cacheEntry;
            cacheEntry.UsageEntryRef = UsageEntryRef.INVALID;
            if (cacheEntry.IsPublic)
              ++publicEntriesFlushed;
            else if (cacheEntry.IsOutputCache)
              ++ocEntriesFlushed;
            this.RemoveEntryFromLastRefList(entryRef);
            entries[ref2Index]._ref1._next = usageEntryRef1;
            usageEntryRef1 = entryRef;
            ++num;
            ++this._cEntriesInFlush;
          }
          if (num == 0)
            return 0;
          this._blockReduce = true;
        }
      }
      finally
      {
        this._cacheUsage.CacheSingle.UnblockInsert();
      }
      CacheSingle cacheSingle = this._cacheUsage.CacheSingle;
      UsageEntryRef entryRef1;
      UsageEntryRef next1;
      for (entryRef1 = usageEntryRef1; !entryRef1.IsInvalid; entryRef1 = next1)
      {
        UsageEntry[] entries = this._pages[entryRef1.PageIndex]._entries;
        int ref1Index = entryRef1.Ref1Index;
        next1 = entries[ref1Index]._ref1._next;
        CacheEntry cacheEntry = entries[ref1Index]._cacheEntry;
        entries[ref1Index]._cacheEntry = (CacheEntry) null;
        cacheSingle.Remove((CacheKey) cacheEntry, CacheItemRemovedReason.Underused);
      }
      try
      {
        this._cacheUsage.CacheSingle.BlockInsertIfNeeded();
        lock (this)
        {
          UsageEntryRef next2;
          for (entryRef1 = usageEntryRef1; !entryRef1.IsInvalid; entryRef1 = next2)
          {
            next2 = this._pages[entryRef1.PageIndex]._entries[entryRef1.Ref1Index]._ref1._next;
            --this._cEntriesInFlush;
            this.AddUsageEntryToFreeList(entryRef1);
          }
          this._blockReduce = false;
          this.Reduce();
        }
      }
      finally
      {
        this._cacheUsage.CacheSingle.UnblockInsert();
      }
      return num;
    }
  }
}
