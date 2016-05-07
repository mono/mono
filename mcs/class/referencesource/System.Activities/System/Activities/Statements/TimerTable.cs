//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.Statements
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime;
    using System.Runtime.Serialization;

    // This class won't be thread safe, it relies on the callers to synchronize addTimer and removeTimer
    [DataContract]
    class TimerTable : IDisposable
    {
        SortedTimerList sortedTimerList;

        bool isImmutable;
        DurableTimerExtension timerExtension;

        HybridCollection<Bookmark> pendingRemoveBookmark;
        HybridCollection<Bookmark> pendingRetryBookmark;

        public TimerTable(DurableTimerExtension timerExtension)
        {
            this.sortedTimerList = new SortedTimerList();
            this.timerExtension = timerExtension;
        }

        public int Count
        {
            get
            {
                return this.sortedTimerList.Count;
            }
        }

        [DataMember(Name = "sortedTimerList")]
        internal SortedTimerList SerializedSortedTimerList
        {
            get { return this.sortedTimerList; }
            set { this.sortedTimerList = value; }
        }

        public void AddTimer(TimeSpan timeout, Bookmark bookmark)
        {
            // Add timer is only called on the workflow thread, 
            // It can't be racing with the persistence thread. 
            // So the table MUST be mutable when this method is called
            Fx.Assert(!this.isImmutable, "Add timer is called when table is immutable");
            DateTime dueTime = TimeoutHelper.Add(DateTime.UtcNow, timeout);
            TimerData timerData = new TimerData(bookmark, dueTime);
            timerData.IOThreadTimer = new IOThreadTimer(this.timerExtension.OnTimerFiredCallback, bookmark, false, 0);
            timerData.IOThreadTimer.Set(timeout);
            this.sortedTimerList.Add(timerData);
        }

        public void RemoveTimer(Bookmark bookmark)
        {
            // When IOThread Timer calls back, it will call remove timer
            // In another thread, we may be in the middle of persistence. 
            // During persisting, we will mark the table as immutable
            // After we are done writing to the database, we will buffer the remove request
            // Meanwhile, since we are not scheduling any IOThreadTimers, 
            // we can only have at most one pending Remove request
            // We don't want to remove 
            if (!this.isImmutable)
            {
                TimerData expirationTimeData;
                if (this.sortedTimerList.TryGetValue(bookmark, out expirationTimeData))
                {
                    this.sortedTimerList.Remove(bookmark);
                    expirationTimeData.IOThreadTimer.Cancel();
                }
            }
            else
            {
                if (this.pendingRemoveBookmark == null)
                {
                    this.pendingRemoveBookmark = new HybridCollection<Bookmark>(bookmark);
                }
                else
                {
                    this.pendingRemoveBookmark.Add(bookmark);
                }
            }
        }

        // Remove the timer from the table, and set expiration date to a new value.
        public void RetryTimer(Bookmark bookmark)
        {
            // This value controls how many seconds do we retry
            const int retryDuration = 10;

            // When IOThread Timer calls back, it might call RetryTimer timer if ResumeBookmark returned notReady
            // In another thread, we may be in the middle of persistence. 
            // During persisting, we will mark the table as immutable
            // After we are done writing to the database, we will buffer the remove request
            // Meanwhile, since we are not scheduling any IOThreadTimers, 
            // we can only have at most one pending Remove request
            // We don't want to remove 
            if (!this.isImmutable)
            {
                // We only retry the timer IFF no one has removed it from the table
                // Otherwise, we are just retrying a timer that doesn't exist
                if (this.sortedTimerList.ContainsKey(bookmark))
                {
                    this.RemoveTimer(bookmark);

                    // Update it to the retry time and put it back to the timer list
                    this.AddTimer(TimeSpan.FromSeconds(retryDuration), bookmark);
                }
            }
            else
            {
                if (this.pendingRetryBookmark == null)
                {
                    this.pendingRetryBookmark = new HybridCollection<Bookmark>(bookmark);
                }
                else
                {
                    this.pendingRetryBookmark.Add(bookmark);
                }
            }
        }

        public DateTime GetNextDueTime()
        {
            if (this.sortedTimerList.Count > 0)
            {
                return this.sortedTimerList.Timers[0].ExpirationTime;
            }
            else
            {
                return DateTime.MaxValue;
            }
        }

        public void OnLoad(DurableTimerExtension timerExtension)
        {
            this.timerExtension = timerExtension;
            this.sortedTimerList.OnLoad();

            foreach (TimerData timerData in this.sortedTimerList.Timers)
            {
                timerData.IOThreadTimer = new IOThreadTimer(this.timerExtension.OnTimerFiredCallback, timerData.Bookmark, false, 0);
                if (timerData.ExpirationTime <= DateTime.UtcNow)
                {
                    // If the timer expired, we want to fire it immediately to win the ---- against UnloadOnIdle policy
                    timerExtension.OnTimerFiredCallback(timerData.Bookmark);
                }
                else
                {
                    timerData.IOThreadTimer.Set(timerData.ExpirationTime - DateTime.UtcNow);
                }
            }
        }

        public void MarkAsImmutable()
        {
            this.isImmutable = true;
        }

        public void MarkAsMutable()
        {
            if (this.isImmutable)
            {
                int index = 0;
                this.isImmutable = false;

                if (this.pendingRemoveBookmark != null)
                {
                    for (index = 0; index < this.pendingRemoveBookmark.Count; index++)
                    {
                        this.RemoveTimer(this.pendingRemoveBookmark[index]);
                    }
                    this.pendingRemoveBookmark = null;
                }

                if (this.pendingRetryBookmark != null)
                {
                    for (index = 0; index < this.pendingRemoveBookmark.Count; index++)
                    {
                        this.RetryTimer(this.pendingRetryBookmark[index]);
                    }
                    this.pendingRetryBookmark = null;
                }
            }
        }

        public void Dispose()
        {
            // Cancel the active timer so we stop retrying
            foreach (TimerData timerData in this.sortedTimerList.Timers)
            {
                timerData.IOThreadTimer.Cancel();
            }

            // And we clear the table and other member variables that might cause the retry logic
            this.sortedTimerList.Clear();
            this.pendingRemoveBookmark = null;
            this.pendingRetryBookmark = null;
        }

        [DataContract]
        internal class TimerData
        {
            Bookmark bookmark;
            DateTime expirationTime;

            public TimerData(Bookmark timerBookmark, DateTime expirationTime)
            {
                this.Bookmark = timerBookmark;
                this.ExpirationTime = expirationTime;
            }
            
            public Bookmark Bookmark
            {
                get
                {
                    return this.bookmark;
                }
                private set
                {
                    this.bookmark = value;
                }
            }
            
            public DateTime ExpirationTime
            {
                get
                {
                    return this.expirationTime;
                }
                private set
                {
                    this.expirationTime = value;
                }
            }

            public IOThreadTimer IOThreadTimer
            {
                get;
                set;
            }

            [DataMember(Name = "Bookmark")]
            internal Bookmark SerializedBookmark
            {
                get { return this.Bookmark; }
                set { this.Bookmark = value; }
            }

            [DataMember(Name = "ExpirationTime")]
            internal DateTime SerializedExpirationTime
            {
                get { return this.ExpirationTime; }
                set { this.ExpirationTime = value; }
            }
        }

        // In Dev11 we don't need to keep the timers in sorted order, since they each have their own IOThreadTimer.
        // However we still sort it for back-compat with Dev10.
        [DataContract]
        internal class SortedTimerList
        {
            List<TimerData> list;

            Dictionary<Bookmark, TimerData> dictionary;

            public SortedTimerList()
            {
                this.list = new List<TimerData>();
                this.dictionary = new Dictionary<Bookmark, TimerData>();
            }

            public List<TimerData> Timers
            {
                get
                {
                    return this.list;
                }
            }

            public int Count
            {
                get
                {
                    return this.list.Count;
                }
            }

            [DataMember(Name = "list")]
            internal List<TimerData> SerializedList
            {
                get { return this.list; }
                set { this.list = value; }
            }

            [DataMember(Name = "dictionary")]
            internal Dictionary<Bookmark, TimerData> SerializedDictionary
            {
                get { return this.dictionary; }
                set { this.dictionary = value; }
            }

            public void Add(TimerData timerData)
            {
                int index = this.list.BinarySearch(timerData, TimerComparer.Instance);
                if (index < 0)
                {
                    this.list.Insert(~index, timerData);
                    this.dictionary.Add(timerData.Bookmark, timerData);
                }
            }

            public bool ContainsKey(Bookmark bookmark)
            {
                return this.dictionary.ContainsKey(bookmark);
            }

            public void OnLoad()
            {
                // If upgrading from Dev10, the dictionary will be empty, so we need to create it
                if (this.dictionary == null)
                {
                    this.dictionary = new Dictionary<Bookmark, TimerData>();
                    for (int i = 0; i < this.list.Count; i++)
                    {
                        this.dictionary.Add(this.list[i].Bookmark, this.list[i]);
                    }
                }
            }

            public void Remove(Bookmark bookmark)
            {
                TimerData timerData;
                if (this.dictionary.TryGetValue(bookmark, out timerData))
                {
                    int index = this.list.BinarySearch(timerData, TimerComparer.Instance);
                    this.list.RemoveAt(index);
                    this.dictionary.Remove(bookmark);
                }
            }

            public bool TryGetValue(Bookmark bookmark, out TimerData timerData)
            {
                return this.dictionary.TryGetValue(bookmark, out timerData);
            }

            public void Clear()
            {
                this.list.Clear();
                this.dictionary.Clear();
            }
        }

        class TimerComparer : IComparer<TimerData>
        {
            internal static readonly TimerComparer Instance = new TimerComparer();

            public int Compare(TimerData x, TimerData y)
            {
                if (object.ReferenceEquals(x, y))
                {
                    return 0;
                }
                else
                {
                    if (x == null)
                    {
                        return -1;
                    }
                    else
                    {
                        if (y == null)
                        {
                            return 1;
                        }
                        else
                        {
                            if (x.ExpirationTime == y.ExpirationTime)
                            {
                                if (x.Bookmark.IsNamed)
                                {
                                    if (y.Bookmark.IsNamed)
                                    {
                                        return string.Compare(x.Bookmark.Name, y.Bookmark.Name, StringComparison.OrdinalIgnoreCase);
                                    }
                                    else
                                    {
                                        return 1;
                                    }
                                }
                                else
                                {
                                    if (y.Bookmark.IsNamed)
                                    {
                                        return -1;
                                    }
                                    else
                                    {
                                        return x.Bookmark.Id.CompareTo(y.Bookmark.Id);
                                    }
                                }
                            }
                            else
                            {
                                return x.ExpirationTime.CompareTo(y.ExpirationTime);
                            }
                        }
                    }
                }
            }
        }
    }
}
