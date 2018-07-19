//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Dispatcher
{
    using System.Diagnostics;
    using System.ServiceModel.Channels;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Runtime.Serialization;
    using System.Xml.XPath;
    using System.ServiceModel.Diagnostics;

    /// <summary>
    /// Multi-reader, single writer
    /// </summary>
    [DataContract]
    public class XPathMessageFilterTable<TFilterData> : IMessageFilterTable<TFilterData>
    {
        internal Dictionary<MessageFilter, TFilterData> filters;
        InverseQueryMatcher iqMatcher;  // inverse query matcher

        public XPathMessageFilterTable()
        {
            Init(-1);
        }

        public XPathMessageFilterTable(int capacity)
        {
            if (capacity < 0)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("capacity", capacity, SR.GetString(SR.FilterCapacityNegative)));

            Init(capacity);
        }

        [OnDeserializing]
        private void OnDeserializing(StreamingContext context)
        {
            Init(-1);
        }

        void Init(int capacity)
        {
            if (capacity <= 0)
                this.filters = new Dictionary<MessageFilter, TFilterData>();
            else
                this.filters = new Dictionary<MessageFilter, TFilterData>(capacity);

            if (this.iqMatcher == null)
                this.iqMatcher = new InverseQueryMatcher(true);
        }

        bool CanMatch
        {
            get
            {
                return (this.filters.Count > 0 && null != this.iqMatcher);
            }
        }

        public TFilterData this[MessageFilter filter]
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
                }
                else
                {
                    this.Add(filter, value);
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
        Entry[] Entries
        {
            get
            {
                Entry[] entries = new Entry[Count];
                int i = 0;
                foreach (KeyValuePair<MessageFilter, TFilterData> item in filters)
                    entries[i++] = new Entry(item.Key, item.Value);

                return entries;
            }
            set
            {
                Init(value.Length);

                for (int i = 0; i < value.Length; ++i)
                    Add(value[i].filter, value[i].data);
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

        /// <summary>
        /// Some filters could be extremely expensive to evaluate or are very long running (infinite). A
        /// filter table could have a very large number of relatively simple filters that taken as a whole would have
        /// a very long running time. XPathFilters could be created using XPath off the wire, which may be malicious. 
        /// Since filters operate on Xml infosets, a natural and simple way to set computational limits on filter tables
        /// is to specify the maximum # of nodes that should be looked at while evaluating ANY of the filters in this
        /// table. 
        /// </summary>
        [DataMember]
        public int NodeQuota
        {
            get
            {
                //return (null == this.iqMatcher) ? int.MaxValue : this.iqMatcher.NodeQuota;
                return this.iqMatcher.NodeQuota;
            }
            set
            {
                if (value <= 0)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("NodeQuota", value, SR.GetString(SR.FilterQuotaRange)));
                }

                if (null == this.iqMatcher)
                {
                    this.iqMatcher = new InverseQueryMatcher(true);
                }
                this.iqMatcher.NodeQuota = value;
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
            this.Add((XPathMessageFilter)filter, data);
        }

        public void Add(KeyValuePair<MessageFilter, TFilterData> item)
        {
            this.Add(item.Key, item.Value);
        }

        public void Add(XPathMessageFilter filter, TFilterData data)
        {
            this.Add(filter, data, false);
        }

        internal void Add(XPathMessageFilter filter, TFilterData data, bool forceExternal)
        {
            if (null == filter)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("filter");
            }

            //this.EnsureMatcher();            
            this.filters.Add(filter, data);
            this.iqMatcher.Add(filter.XPath, filter.Namespaces, filter, forceExternal);
        }

        public void Clear()
        {
            this.iqMatcher.Clear();
            this.filters.Clear();
        }

        public bool Contains(KeyValuePair<MessageFilter, TFilterData> item)
        {
            return ((IDictionary<MessageFilter, TFilterData>)this.filters).Contains(item);
        }

        public bool ContainsKey(MessageFilter filter)
        {
            return this.filters.ContainsKey(filter);
        }

        public void CopyTo(KeyValuePair<MessageFilter, TFilterData>[] array, int arrayIndex)
        {
            ((IDictionary<MessageFilter, TFilterData>)this.filters).CopyTo(array, arrayIndex);
        }

#if NO
        void EnsureMatcher()
        {
            if (null == this.iqMatcher)
            {
                this.iqMatcher = new InverseQueryMatcher();
            }
        }
#endif

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public IEnumerator<KeyValuePair<MessageFilter, TFilterData>> GetEnumerator()
        {
            return ((IDictionary<MessageFilter, TFilterData>)this.filters).GetEnumerator();
        }

        public bool GetMatchingValue(Message message, out TFilterData data)
        {
            if (null == message)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("message");
            }

            if (this.CanMatch)
            {
                return this.ProcessMatch(this.iqMatcher.Match(message, false, null), out data);
            }

            data = default(TFilterData);
            return false;
        }

        public bool GetMatchingValue(MessageBuffer messageBuffer, out TFilterData data)
        {
            if (null == messageBuffer)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("messageBuffer");
            }

            if (this.CanMatch)
            {
                return this.ProcessMatch(this.iqMatcher.Match(messageBuffer, null), out data);
            }

            data = default(TFilterData);
            return false;
        }

        public bool GetMatchingValue(SeekableXPathNavigator navigator, out TFilterData data)
        {
            if (null == navigator)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("navigator");
            }

            if (this.CanMatch)
            {
                return this.ProcessMatch(this.iqMatcher.Match(navigator, null), out data);
            }

            data = default(TFilterData);
            return false;
        }

        public bool GetMatchingValue(XPathNavigator navigator, out TFilterData data)
        {
            if (null == navigator)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("navigator");
            }

            if (this.CanMatch)
            {
                return this.ProcessMatch(this.iqMatcher.Match(navigator, null), out data);
            }

            data = default(TFilterData);
            return false;
        }

        public bool GetMatchingFilter(Message message, out MessageFilter filter)
        {
            Collection<MessageFilter> filters = new Collection<MessageFilter>();
            this.GetMatchingFilters(message, filters);
            if (filters.Count > 1)
            {
                throw TraceUtility.ThrowHelperError(new MultipleFilterMatchesException(SR.GetString(SR.FilterMultipleMatches), null, filters), message);
            }
            else if (filters.Count == 1)
            {
                filter = filters[0];
                return true;
            }
            else
            {
                filter = null;
                return false;
            }
        }

        public bool GetMatchingFilter(MessageBuffer messageBuffer, out MessageFilter filter)
        {
            Collection<MessageFilter> filters = new Collection<MessageFilter>();
            this.GetMatchingFilters(messageBuffer, filters);
            if (filters.Count > 1)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MultipleFilterMatchesException(SR.GetString(SR.FilterMultipleMatches), null, filters));
            }
            else if (filters.Count == 1)
            {
                filter = filters[0];
                return true;
            }
            else
            {
                filter = null;
                return false;
            }
        }

        public bool GetMatchingFilter(SeekableXPathNavigator navigator, out MessageFilter filter)
        {
            Collection<MessageFilter> filters = new Collection<MessageFilter>();
            this.GetMatchingFilters(navigator, filters);
            if (filters.Count > 1)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MultipleFilterMatchesException(SR.GetString(SR.FilterMultipleMatches), null, filters));
            }
            else if (filters.Count == 1)
            {
                filter = filters[0];
                return true;
            }
            else
            {
                filter = null;
                return false;
            }
        }

        public bool GetMatchingFilter(XPathNavigator navigator, out MessageFilter filter)
        {
            Collection<MessageFilter> filters = new Collection<MessageFilter>();
            this.GetMatchingFilters(navigator, filters);
            if (filters.Count > 1)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MultipleFilterMatchesException(SR.GetString(SR.FilterMultipleMatches), null, filters));
            }
            else if (filters.Count == 1)
            {
                filter = filters[0];
                return true;
            }
            else
            {
                filter = null;
                return false;
            }
        }

        public bool GetMatchingFilters(Message message, ICollection<MessageFilter> results)
        {
            if (null == message)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("message");
            }
            if (null == results)
            {
                throw TraceUtility.ThrowHelperArgumentNull("results", message);
            }

            if (this.CanMatch)
            {
                int count = results.Count;
                this.iqMatcher.ReleaseResult(this.iqMatcher.Match(message, false, results));
                return count != results.Count;
            }
            return false;
        }

        public bool GetMatchingFilters(MessageBuffer messageBuffer, ICollection<MessageFilter> results)
        {
            if (null == messageBuffer)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("messageBuffer");
            }
            if (null == results)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("results");
            }

            if (this.CanMatch)
            {
                int count = results.Count;
                this.iqMatcher.ReleaseResult(iqMatcher.Match(messageBuffer, results));
                return count != results.Count;
            }
            return false;
        }

        public bool GetMatchingFilters(SeekableXPathNavigator navigator, ICollection<MessageFilter> results)
        {
            if (null == navigator)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("navigator");
            }
            if (null == results)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("results");
            }

            if (this.CanMatch)
            {
                int count = results.Count;
                this.iqMatcher.ReleaseResult(this.iqMatcher.Match(navigator, results));
                return count != results.Count;
            }
            return false;
        }

        public bool GetMatchingFilters(XPathNavigator navigator, ICollection<MessageFilter> results)
        {
            if (null == navigator)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("navigator");
            }
            if (null == results)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("results");
            }

            if (this.CanMatch)
            {
                int count = results.Count;
                this.iqMatcher.ReleaseResult(this.iqMatcher.Match(navigator, results));
                return count != results.Count;
            }
            return false;
        }

        public bool GetMatchingValues(Message message, ICollection<TFilterData> results)
        {
            if (null == message)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("message");
            }
            if (null == results)
            {
                throw TraceUtility.ThrowHelperArgumentNull("results", message);
            }

            if (this.CanMatch)
            {
                int count = results.Count;
                this.ProcessMatches(this.iqMatcher.Match(message, false, null), results);
                return count != results.Count;
            }
            return false;
        }

        public bool GetMatchingValues(MessageBuffer messageBuffer, ICollection<TFilterData> results)
        {
            if (null == messageBuffer)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("messageBuffer");
            }
            if (null == results)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("results");
            }

            if (this.CanMatch)
            {
                int count = results.Count;
                this.ProcessMatches(this.iqMatcher.Match(messageBuffer, null), results);
                return count != results.Count;
            }
            return false;
        }

        public bool GetMatchingValues(SeekableXPathNavigator navigator, ICollection<TFilterData> results)
        {
            if (null == navigator)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("navigator");
            }
            if (null == results)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("results");
            }

            if (this.CanMatch)
            {
                int count = results.Count;
                this.ProcessMatches(this.iqMatcher.Match(navigator, null), results);
                return count != results.Count;
            }
            return false;
        }

        public bool GetMatchingValues(XPathNavigator navigator, ICollection<TFilterData> results)
        {
            if (null == navigator)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("navigator");
            }
            if (null == results)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("results");
            }

            if (this.CanMatch)
            {
                int count = results.Count;
                this.ProcessMatches(this.iqMatcher.Match(navigator, null), results);
                return count != results.Count;
            }
            return false;
        }

        bool ProcessMatch(FilterResult result, out TFilterData data)
        {
            bool retVal = false;
            data = default(TFilterData);
            MessageFilter match = result.GetSingleMatch();
            if (null != match)
            {
                data = this.filters[match];
                retVal = true;
            }
            this.iqMatcher.ReleaseResult(result);
            return retVal;
        }

        void ProcessMatches(FilterResult result, ICollection<TFilterData> results)
        {
            Collection<MessageFilter> matches = result.Processor.MatchList;
            for (int i = 0, count = matches.Count; i < count; ++i)
            {
                results.Add(this.filters[matches[i]]);
            }
            this.iqMatcher.ReleaseResult(result);
        }

        public bool Remove(MessageFilter filter)
        {
            if (null == filter)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("filter");
            }

            XPathMessageFilter xpf = filter as XPathMessageFilter;
            if (xpf != null)
            {
                return this.Remove(xpf);
            }
            return false;
        }

        public bool Remove(KeyValuePair<MessageFilter, TFilterData> item)
        {
            if (((IDictionary<MessageFilter, TFilterData>)this.filters).Remove(item))
            {
                this.iqMatcher.Remove((XPathMessageFilter)item.Key);
                return true;
            }

            return false;
        }

        public bool Remove(XPathMessageFilter filter)
        {
            if (null == filter)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("filter");
            }

            if (this.filters.Remove(filter))
            {
                this.iqMatcher.Remove(filter);
                return true;
            }

            // Not in this table
            return false;
        }

        public void TrimToSize()
        {
            this.iqMatcher.Trim();
        }

        public bool TryGetValue(MessageFilter filter, out TFilterData data)
        {
            return this.filters.TryGetValue(filter, out data);
        }

        [DataContract]
        class Entry
        {
            [DataMember(IsRequired = true)]
            internal MessageFilter filter;

            [DataMember(IsRequired = true)]
            internal TFilterData data;

            internal Entry(MessageFilter f, TFilterData d)
            {
                filter = f;
                data = d;
            }
        }
    }
}
