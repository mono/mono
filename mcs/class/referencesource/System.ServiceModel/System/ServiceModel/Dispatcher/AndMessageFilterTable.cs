//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.ServiceModel.Dispatcher
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Diagnostics;

    class AndMessageFilterTable<FilterData> : IMessageFilterTable<FilterData>
    {
        Dictionary<MessageFilter, FilterData> filters;
        Dictionary<MessageFilter, FilterDataPair> filterData;
        MessageFilterTable<FilterDataPair> table;

        public AndMessageFilterTable()
        {
            this.filters = new Dictionary<MessageFilter, FilterData>();
            this.filterData = new Dictionary<MessageFilter, FilterDataPair>();
            this.table = new MessageFilterTable<FilterDataPair>();
        }

        public FilterData this[MessageFilter filter]
        {
            get
            {
                return this.filters[filter];
            }
            set
            {
                if (this.filters.ContainsKey(filter))
                {
                    this.filters[filter] = value;
                    this.filterData[filter].data = value;
                }
                else
                {
                    Add(filter, value);
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

        public ICollection<FilterData> Values
        {
            get
            {
                return this.filters.Values;
            }
        }

        public void Add(MessageFilter filter, FilterData data)
        {
            if (filter == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("filter");
            }

            this.Add((AndMessageFilter)filter, data);
        }

        public void Add(KeyValuePair<MessageFilter, FilterData> item)
        {
            this.Add(item.Key, item.Value);

        }
        public void Add(AndMessageFilter filter, FilterData data)
        {
            if (filter == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("filter");
            }

            this.filters.Add(filter, data);

            FilterDataPair pair = new FilterDataPair(filter, data);
            this.filterData.Add(filter, pair);

            this.table.Add(filter.Filter1, pair);
        }

        public void Clear()
        {
            this.filters.Clear();
            this.filterData.Clear();
            this.table.Clear();
        }

        public bool Contains(KeyValuePair<MessageFilter, FilterData> item)
        {
            return ((ICollection<KeyValuePair<MessageFilter, FilterData>>)this.filters).Contains(item);
        }

        public bool ContainsKey(MessageFilter filter)
        {
            if (filter == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("filter");
            }
            return this.filters.ContainsKey(filter);
        }

        public void CopyTo(KeyValuePair<MessageFilter, FilterData>[] array, int arrayIndex)
        {
            ((ICollection<KeyValuePair<MessageFilter, FilterData>>)this.filters).CopyTo(array, arrayIndex);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public IEnumerator<KeyValuePair<MessageFilter, FilterData>> GetEnumerator()
        {
            return this.filters.GetEnumerator();
        }

        FilterDataPair InnerMatch(Message message)
        {
            List<FilterDataPair> pairs = new List<FilterDataPair>();
            this.table.GetMatchingValues(message, pairs);

            FilterDataPair pair = null;
            for (int i = 0; i < pairs.Count; ++i)
            {
                if (pairs[i].filter.Filter2.Match(message))
                {
                    if (pair != null)
                    {
                        Collection<MessageFilter> matches = new Collection<MessageFilter>();
                        matches.Add(pair.filter);
                        matches.Add(pairs[i].filter);
                        throw TraceUtility.ThrowHelperError(new MultipleFilterMatchesException(SR.GetString(SR.FilterMultipleMatches), null, matches), message);
                    }
                    pair = pairs[i];
                }
            }

            return pair;
        }

        FilterDataPair InnerMatch(MessageBuffer messageBuffer)
        {
            List<FilterDataPair> pairs = new List<FilterDataPair>();
            this.table.GetMatchingValues(messageBuffer, pairs);

            FilterDataPair pair = null;
            for (int i = 0; i < pairs.Count; ++i)
            {
                if (pairs[i].filter.Filter2.Match(messageBuffer))
                {
                    if (pair != null)
                    {
                        Collection<MessageFilter> matches = new Collection<MessageFilter>();
                        matches.Add(pair.filter);
                        matches.Add(pairs[i].filter);
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MultipleFilterMatchesException(SR.GetString(SR.FilterMultipleMatches), null, matches));
                    }
                    pair = pairs[i];
                }
            }

            return pair;
        }

        void InnerMatch(Message message, ICollection<MessageFilter> results)
        {
            List<FilterDataPair> pairs = new List<FilterDataPair>();
            this.table.GetMatchingValues(message, pairs);

            for (int i = 0; i < pairs.Count; ++i)
            {
                if (pairs[i].filter.Filter2.Match(message))
                {
                    results.Add(pairs[i].filter);
                }
            }
        }

        void InnerMatchData(Message message, ICollection<FilterData> results)
        {
            List<FilterDataPair> pairs = new List<FilterDataPair>();
            this.table.GetMatchingValues(message, pairs);

            for (int i = 0; i < pairs.Count; ++i)
            {
                if (pairs[i].filter.Filter2.Match(message))
                {
                    results.Add(pairs[i].data);
                }
            }
        }

        void InnerMatch(MessageBuffer messageBuffer, ICollection<MessageFilter> results)
        {
            List<FilterDataPair> pairs = new List<FilterDataPair>();
            this.table.GetMatchingValues(messageBuffer, pairs);

            for (int i = 0; i < pairs.Count; ++i)
            {
                if (pairs[i].filter.Filter2.Match(messageBuffer))
                {
                    results.Add(pairs[i].filter);
                }
            }
        }

        void InnerMatchData(MessageBuffer messageBuffer, ICollection<FilterData> results)
        {
            List<FilterDataPair> pairs = new List<FilterDataPair>();
            this.table.GetMatchingValues(messageBuffer, pairs);

            for (int i = 0; i < pairs.Count; ++i)
            {
                if (pairs[i].filter.Filter2.Match(messageBuffer))
                {
                    results.Add(pairs[i].data);
                }
            }
        }

        internal bool GetMatchingValue(Message message, out FilterData data, out bool addressMatched)
        {
            if (message == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("message");
            }

            List<FilterDataPair> pairs = new List<FilterDataPair>();
            addressMatched = this.table.GetMatchingValues(message, pairs);

            FilterDataPair pair = null;
            for (int i = 0; i < pairs.Count; ++i)
            {
                if (pairs[i].filter.Filter2.Match(message))
                {
                    if (pair != null)
                    {
                        Collection<MessageFilter> matches = new Collection<MessageFilter>();
                        matches.Add(pair.filter);
                        matches.Add(pairs[i].filter);
                        throw TraceUtility.ThrowHelperError(new MultipleFilterMatchesException(SR.GetString(SR.FilterMultipleMatches), null, matches), message);
                    }
                    pair = pairs[i];
                }
            }

            if (pair == null)
            {
                data = default(FilterData);
                return false;
            }

            data = pair.data;
            return true;
        }

        public bool GetMatchingValue(Message message, out FilterData data)
        {
            if (message == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("message");
            }

            FilterDataPair pair = InnerMatch(message);
            if (pair == null)
            {
                data = default(FilterData);
                return false;
            }

            data = pair.data;
            return true;
        }

        public bool GetMatchingValue(MessageBuffer messageBuffer, out FilterData data)
        {
            if (messageBuffer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("messageBuffer");
            }

            FilterDataPair pair = InnerMatch(messageBuffer);

            if (pair == null)
            {
                data = default(FilterData);
                return false;
            }

            data = pair.data;
            return true;
        }

        public bool GetMatchingFilter(Message message, out MessageFilter filter)
        {
            if (message == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("message");
            }

            FilterDataPair pair = InnerMatch(message);
            if (pair == null)
            {
                filter = null;
                return false;
            }

            filter = pair.filter;
            return true;
        }

        public bool GetMatchingFilter(MessageBuffer messageBuffer, out MessageFilter filter)
        {
            if (messageBuffer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("messageBuffer");
            }

            FilterDataPair pair = InnerMatch(messageBuffer);

            if (pair == null)
            {
                filter = null;
                return false;
            }

            filter = pair.filter;
            return true;
        }

        public bool GetMatchingFilters(Message message, ICollection<MessageFilter> results)
        {
            if (message == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("message");
            }

            if (results == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("results");
            }

            int count = results.Count;
            InnerMatch(message, results);
            return count != results.Count;
        }

        public bool GetMatchingFilters(MessageBuffer messageBuffer, ICollection<MessageFilter> results)
        {
            if (messageBuffer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("messageBuffer");
            }

            if (results == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("results");
            }

            int count = results.Count;
            InnerMatch(messageBuffer, results);
            return count != results.Count;
        }

        public bool GetMatchingValues(Message message, ICollection<FilterData> results)
        {
            if (message == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("message");
            }

            if (results == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("results");
            }

            int count = results.Count;
            InnerMatchData(message, results);
            return count != results.Count;
        }

        public bool GetMatchingValues(MessageBuffer messageBuffer, ICollection<FilterData> results)
        {
            if (messageBuffer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("messageBuffer");
            }

            if (results == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("results");
            }

            int count = results.Count;
            InnerMatchData(messageBuffer, results);
            return count != results.Count;
        }

        public bool Remove(MessageFilter filter)
        {
            if (filter == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("filter");
            }

            AndMessageFilter sbFilter = filter as AndMessageFilter;
            if (sbFilter != null)
            {
                return Remove(sbFilter);
            }
            return false;
        }

        public bool Remove(KeyValuePair<MessageFilter, FilterData> item)
        {
            if (((ICollection<KeyValuePair<MessageFilter, FilterData>>)this.filters).Contains(item))
            {
                return Remove(item.Key);
            }
            return false;
        }

        public bool Remove(AndMessageFilter filter)
        {
            if (filter == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("filter");
            }

            if (this.filters.Remove(filter))
            {
                this.filterData.Remove(filter);
                this.table.Remove(filter.Filter1);

                return true;
            }
            return false;
        }

        internal class FilterDataPair
        {
            internal AndMessageFilter filter;
            internal FilterData data;

            internal FilterDataPair(AndMessageFilter filter, FilterData data)
            {
                this.filter = filter;
                this.data = data;
            }
        }

        public bool TryGetValue(MessageFilter filter, out FilterData data)
        {
            return this.filters.TryGetValue(filter, out data);
        }
    }
}
