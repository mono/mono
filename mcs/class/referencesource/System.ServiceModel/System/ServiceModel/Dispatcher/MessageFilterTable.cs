//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.ServiceModel.Channels;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.ServiceModel.Diagnostics;
    using System.Runtime;
    using System.Runtime.Serialization;

    [DataContract]
    public class MessageFilterTable<TFilterData> : IMessageFilterTable<TFilterData>
    {
        Dictionary<Type, Type> filterTypeMappings;
        Dictionary<MessageFilter, TFilterData> filters;
        SortedBuffer<FilterTableEntry, TableEntryComparer> tables;
        int defaultPriority;

        static readonly TableEntryComparer staticComparerInstance = new TableEntryComparer();

        public MessageFilterTable()
            : this(0)
        {
        }

        public MessageFilterTable(int defaultPriority)
        {
            Init(defaultPriority);
        }

        [OnDeserializing]
        private void OnDeserializing(StreamingContext context)
        {
            Init(0);
        }

        void Init(int defaultPriority)
        {
            CreateEmptyTables();
            this.defaultPriority = defaultPriority;
        }

        public TFilterData this[MessageFilter filter]
        {
            get
            {
                return this.filters[filter];
            }
            set
            {
                if (this.ContainsKey(filter))
                {
                    int p = this.GetPriority(filter);
                    this.Remove(filter);
                    this.Add(filter, value, p);
                }
                else
                {
                    this.Add(filter, value, this.defaultPriority);
                }
            }
        }

        public int Count
        {
            get
            {
                return this.filters.Count;
            }
        }

        [DataMember]
        public int DefaultPriority
        {
            get
            {
                return this.defaultPriority;
            }
            set
            {
                this.defaultPriority = value;
            }
        }

        [DataMember]
        Entry[] Entries
        {
            get
            {
                Entry[] entries = new Entry[Count];
                int i = 0;
                foreach (KeyValuePair<MessageFilter, TFilterData> item in this.filters)
                {
                    entries[i++] = new Entry(item.Key, item.Value, GetPriority(item.Key));
                }
                return entries;
            }
            set
            {
                for (int i = 0; i < value.Length; ++i)
                {
                    Entry e = value[i];
                    Add(e.filter, e.data, e.priority);
                }
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public ICollection<MessageFilter> Keys
        {
            get
            {
                return this.filters.Keys;
            }
        }

        public ICollection<TFilterData> Values
        {
            get
            {
                return this.filters.Values;
            }
        }

        public void Add(MessageFilter filter, TFilterData data)
        {
            this.Add(filter, data, this.defaultPriority);
        }

        [SuppressMessage(FxCop.Category.Usage, "CA2301:EmbeddableTypesInContainersRule", MessageId = "filterTypeMappings", Justification = "No need to support type equivalence here.")]
        public void Add(MessageFilter filter, TFilterData data, int priority)
        {
            if (filter == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("filter");
            }

            if (this.filters.ContainsKey(filter))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("filter", SR.GetString(SR.FilterExists));
            }

#pragma warning suppress 56506 // [....], PreSharp generates a false warning here
            Type filterType = filter.GetType();
            Type tableType = null;
            IMessageFilterTable<TFilterData> table = null;

            if (this.filterTypeMappings.TryGetValue(filterType, out tableType))
            {
                for (int i = 0; i < this.tables.Count; ++i)
                {
                    if (this.tables[i].priority == priority && this.tables[i].table.GetType().Equals(tableType))
                    {
                        table = this.tables[i].table;
                        break;
                    }
                }
                if (table == null)
                {
                    table = CreateFilterTable(filter);
                    ValidateTable(table);
                    if (!table.GetType().Equals(tableType))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.FilterTableTypeMismatch)));
                    }
                    table.Add(filter, data);
                    this.tables.Add(new FilterTableEntry(priority, table));
                }
                else
                {
                    table.Add(filter, data);
                }
            }
            else
            {
                table = CreateFilterTable(filter);
                ValidateTable(table);
                this.filterTypeMappings.Add(filterType, table.GetType());

                FilterTableEntry entry = new FilterTableEntry(priority, table);
                int idx = this.tables.IndexOf(entry);
                if (idx >= 0)
                {
                    table = this.tables[idx].table;
                }
                else
                {
                    this.tables.Add(entry);
                }

                table.Add(filter, data);
            }

            this.filters.Add(filter, data);
        }

        public void Add(KeyValuePair<MessageFilter, TFilterData> item)
        {
            this.Add(item.Key, item.Value);
        }

        public void Clear()
        {
            this.filters.Clear();
            this.tables.Clear();
        }

        public bool Contains(KeyValuePair<MessageFilter, TFilterData> item)
        {
            return ((ICollection<KeyValuePair<MessageFilter, TFilterData>>)this.filters).Contains(item);
        }

        public bool ContainsKey(MessageFilter filter)
        {
            return this.filters.ContainsKey(filter);
        }

        public void CopyTo(KeyValuePair<MessageFilter, TFilterData>[] array, int arrayIndex)
        {
            ((ICollection<KeyValuePair<MessageFilter, TFilterData>>)this.filters).CopyTo(array, arrayIndex);
        }

        void CreateEmptyTables()
        {
            this.filterTypeMappings = new Dictionary<Type, Type>();
            this.filters = new Dictionary<MessageFilter, TFilterData>();
            this.tables = new SortedBuffer<FilterTableEntry, TableEntryComparer>(staticComparerInstance);
        }

        protected virtual IMessageFilterTable<TFilterData> CreateFilterTable(MessageFilter filter)
        {
            IMessageFilterTable<TFilterData> ft = filter.CreateFilterTable<TFilterData>();

            if (ft == null)
                return new SequentialMessageFilterTable<TFilterData>();

            return ft;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public IEnumerator<KeyValuePair<MessageFilter, TFilterData>> GetEnumerator()
        {
            return ((ICollection<KeyValuePair<MessageFilter, TFilterData>>)this.filters).GetEnumerator();
        }

        public int GetPriority(MessageFilter filter)
        {
            TFilterData d = this.filters[filter];
            for (int i = 0; i < this.tables.Count; ++i)
            {
                if (this.tables[i].table.ContainsKey(filter))
                {
                    return this.tables[i].priority;
                }
            }


            throw DiagnosticUtility.ExceptionUtility.ThrowHelperCritical(new InvalidOperationException(SR.GetString(SR.FilterTableInvalidForLookup)));
        }

        public bool GetMatchingValue(Message message, out TFilterData data)
        {
            bool dataSet = false;
            int pri = int.MinValue;

            data = default(TFilterData);
            for (int i = 0; i < this.tables.Count; ++i)
            {
                // Watch for the end of a bucket
                if (pri > this.tables[i].priority && dataSet)
                {
                    break;
                }
                pri = this.tables[i].priority;

                TFilterData currentData;

                if (this.tables[i].table.GetMatchingValue(message, out currentData))
                {
                    if (dataSet)
                    {
                        throw TraceUtility.ThrowHelperError(new MultipleFilterMatchesException(SR.GetString(SR.FilterMultipleMatches), null, null), message);
                    }

                    data = currentData;
                    dataSet = true;
                }
            }

            return dataSet;
        }

        internal bool GetMatchingValue(Message message, out TFilterData data, out bool addressMatched)
        {
            bool dataSet = false;
            int pri = int.MinValue;
            data = default(TFilterData);
            addressMatched = false;
            for (int i = 0; i < this.tables.Count; ++i)
            {
                // Watch for the end of a bucket
                if (pri > this.tables[i].priority && dataSet)
                {
                    break;
                }
                pri = this.tables[i].priority;

                bool matchResult;
                TFilterData currentData;
                IMessageFilterTable<TFilterData> table = this.tables[i].table;
                AndMessageFilterTable<TFilterData> andTable = table as AndMessageFilterTable<TFilterData>;
                if (andTable != null)
                {
                    bool addressResult;
                    matchResult = andTable.GetMatchingValue(message, out currentData, out addressResult);
                    addressMatched |= addressResult;
                }
                else
                {
                    matchResult = table.GetMatchingValue(message, out currentData);
                }

                if (matchResult)
                {
                    if (dataSet)
                    {
                        throw TraceUtility.ThrowHelperError(new MultipleFilterMatchesException(SR.GetString(SR.FilterMultipleMatches), null, null), message);
                    }

                    addressMatched = true;
                    data = currentData;
                    dataSet = true;
                }
            }

            return dataSet;
        }

        public bool GetMatchingValue(MessageBuffer buffer, out TFilterData data)
        {
            return this.GetMatchingValue(buffer, null, out data);
        }

        // this optimization is only for CorrelationActionMessageFilter and ActionMessageFilter if they override CreateFilterTable to return ActionMessageFilterTable
        internal bool GetMatchingValue(MessageBuffer buffer, Message messageToReadHeaders, out TFilterData data)
        {
            bool dataSet = false;
            int pri = int.MinValue;
            data = default(TFilterData);
            for (int i = 0; i < this.tables.Count; ++i)
            {
                // Watch for the end of a bucket
                if (pri > this.tables[i].priority && dataSet)
                {
                    break;
                }
                pri = this.tables[i].priority;

                TFilterData currentData;
                bool result = false;
                if (messageToReadHeaders != null && this.tables[i].table is ActionMessageFilterTable<TFilterData>)
                {
                    // this is an action message, in this case we can pass in the message itself since the filter will only read from the header
                    result = this.tables[i].table.GetMatchingValue(messageToReadHeaders, out currentData);
                }
                else
                {
                    // this is a custom filter that might read from the message body, pass in the message buffer itself in this case
                    result = this.tables[i].table.GetMatchingValue(buffer, out currentData);
                }
                if (result)
                {
                    if (dataSet)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MultipleFilterMatchesException(SR.GetString(SR.FilterMultipleMatches), null, null));
                    }

                    data = currentData;
                    dataSet = true;
                }
            }

            return dataSet;
        }

        public bool GetMatchingValues(Message message, ICollection<TFilterData> results)
        {
            if (results == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("results");
            }
            int count = results.Count;
            int pri = int.MinValue;
            for (int i = 0; i < this.tables.Count; ++i)
            {
                // Watch for the end of a bucket
                if (pri > this.tables[i].priority && count != results.Count)
                {
                    break;
                }
                pri = this.tables[i].priority;
                this.tables[i].table.GetMatchingValues(message, results);
            }

            return count != results.Count;
        }

        public bool GetMatchingValues(MessageBuffer buffer, ICollection<TFilterData> results)
        {
            if (results == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("results");
            }
            int count = results.Count;
            int pri = int.MinValue;
            for (int i = 0; i < this.tables.Count; ++i)
            {
                // Watch for the end of a bucket
                if (pri > this.tables[i].priority && count != results.Count)
                {
                    break;
                }
                pri = this.tables[i].priority;
                this.tables[i].table.GetMatchingValues(buffer, results);
            }

            return count != results.Count;
        }

        public bool GetMatchingFilter(Message message, out MessageFilter filter)
        {
            MessageFilter f;
            int pri = int.MinValue;
            filter = null;
            for (int i = 0; i < this.tables.Count; ++i)
            {
                // Watch for the end of a bucket
                if (pri > this.tables[i].priority && filter != null)
                {
                    break;
                }
                pri = this.tables[i].priority;

                if (this.tables[i].table.GetMatchingFilter(message, out f))
                {
                    if (filter == null)
                    {
                        filter = f;
                    }
                    else
                    {
                        Collection<MessageFilter> c = new Collection<MessageFilter>();
                        c.Add(filter);
                        c.Add(f);
                        throw TraceUtility.ThrowHelperError(new MultipleFilterMatchesException(SR.GetString(SR.FilterMultipleMatches), null, c), message);
                    }
                }
            }

            return filter != null;
        }

        public bool GetMatchingFilter(MessageBuffer buffer, out MessageFilter filter)
        {
            MessageFilter f;
            int pri = int.MinValue;
            filter = null;
            for (int i = 0; i < this.tables.Count; ++i)
            {
                // Watch for the end of a bucket
                if (pri > this.tables[i].priority && filter != null)
                {
                    break;
                }
                pri = this.tables[i].priority;

                if (this.tables[i].table.GetMatchingFilter(buffer, out f))
                {
                    if (filter == null)
                    {
                        filter = f;
                    }
                    else
                    {
                        Collection<MessageFilter> c = new Collection<MessageFilter>();
                        c.Add(filter);
                        c.Add(f);
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MultipleFilterMatchesException(SR.GetString(SR.FilterMultipleMatches), null, c));
                    }
                }
            }

            return filter != null;
        }

        public bool GetMatchingFilters(Message message, ICollection<MessageFilter> results)
        {
            if (results == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("results");
            }
            int count = results.Count;
            int pri = int.MinValue;
            for (int i = 0; i < this.tables.Count; ++i)
            {
                // Watch for the end of a bucket
                if (pri > this.tables[i].priority && count != results.Count)
                {
                    break;
                }
                pri = this.tables[i].priority;
                this.tables[i].table.GetMatchingFilters(message, results);
            }

            return count != results.Count;
        }

        public bool GetMatchingFilters(MessageBuffer buffer, ICollection<MessageFilter> results)
        {
            if (results == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("results");
            }
            int count = results.Count;
            int pri = int.MinValue;
            for (int i = 0; i < this.tables.Count; ++i)
            {
                // Watch for the end of a bucket
                if (pri > this.tables[i].priority && count != results.Count)
                {
                    break;
                }
                pri = this.tables[i].priority;
                this.tables[i].table.GetMatchingFilters(buffer, results);
            }

            return count != results.Count;
        }

        public bool Remove(MessageFilter filter)
        {
            for (int i = 0; i < this.tables.Count; ++i)
            {
                if (this.tables[i].table.Remove(filter))
                {
                    if (this.tables[i].table.Count == 0)
                    {
                        this.tables.RemoveAt(i);
                    }
                    return this.filters.Remove(filter);
                }
            }
            return false;
        }

        public bool Remove(KeyValuePair<MessageFilter, TFilterData> item)
        {
            if (((ICollection<KeyValuePair<MessageFilter, TFilterData>>)this.filters).Contains(item))
            {
                return this.Remove(item.Key);
            }
            return false;
        }

        public bool TryGetValue(MessageFilter filter, out TFilterData data)
        {
            return this.filters.TryGetValue(filter, out data);
        }

        void ValidateTable(IMessageFilterTable<TFilterData> table)
        {
            Type t = this.GetType();
            if (t.IsInstanceOfType(table))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.FilterBadTableType)));
            }
        }

        ///////////////////////////////////////////////////

        struct FilterTableEntry
        {
            internal IMessageFilterTable<TFilterData> table;
            internal int priority;

            internal FilterTableEntry(int pri, IMessageFilterTable<TFilterData> t)
            {
                this.priority = pri;
                this.table = t;
            }
        }

        class TableEntryComparer : IComparer<FilterTableEntry>
        {
            public TableEntryComparer() { }

            public int Compare(FilterTableEntry x, FilterTableEntry y)
            {
                // Highest priority first
                int p = y.priority.CompareTo(x.priority);
                if (p != 0)
                {
                    return p;
                }

                return x.table.GetType().FullName.CompareTo(y.table.GetType().FullName);
            }

            public bool Equals(FilterTableEntry x, FilterTableEntry y)
            {
                // Highest priority first
                int p = y.priority.CompareTo(x.priority);
                if (p != 0)
                {
                    return false;
                }

                return x.table.GetType().FullName.Equals(y.table.GetType().FullName);
            }

            public int GetHashCode(FilterTableEntry table)
            {
                return table.GetHashCode();
            }
        }

        [DataContract]
        class Entry
        {
            [DataMember(IsRequired = true)]
            internal MessageFilter filter;

            [DataMember(IsRequired = true)]
            internal TFilterData data;

            [DataMember(IsRequired = true)]
            internal int priority;

            internal Entry(MessageFilter f, TFilterData d, int p)
            {
                filter = f;
                data = d;
                priority = p;
            }
        }
    }
}
