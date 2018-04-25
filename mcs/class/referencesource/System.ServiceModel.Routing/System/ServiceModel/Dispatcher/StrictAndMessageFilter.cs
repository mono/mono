//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Dispatcher;
    using System.Collections;
    using System.ServiceModel.Routing;

    [SuppressMessage(FxCop.Category.Xaml, FxCop.Rule.TypesMustHaveXamlCallableConstructors)]
    [SuppressMessage(FxCop.Category.Xaml, FxCop.Rule.TypesShouldHavePublicParameterlessConstructors)]
    public class StrictAndMessageFilter : MessageFilter
    {
        MessageFilter filter1;
        MessageFilter filter2;

        public StrictAndMessageFilter(MessageFilter filter1, MessageFilter filter2)
        {
            if (filter1 == null || filter2 == null)
            {
                throw FxTrace.Exception.ArgumentNull(filter1 == null ? "filter1" : "filter2");
            }

            this.filter1 = filter1;
            this.filter2 = filter2;
        }

        public override bool Match(Message message)
        {
            throw FxTrace.Exception.AsError(new NotSupportedException());
        }

        public override bool Match(MessageBuffer buffer)
        {
            throw FxTrace.Exception.AsError(new NotSupportedException());
        }

        protected internal override IMessageFilterTable<TFilterData> CreateFilterTable<TFilterData>()
        {
            StrictAndMessageFilterTable<TFilterData> table = new StrictAndMessageFilterTable<TFilterData>();
            return table;
        }

        class StrictAndMessageFilterTable<TFilterData> : IMessageFilterTable<TFilterData>
        {
            Dictionary<MessageFilter, TFilterData> andFilters;
            MessageFilterTable<StrictAndMessageFilter> filterTable;

            public StrictAndMessageFilterTable()
            {
                this.andFilters = new Dictionary<MessageFilter, TFilterData>();
                this.filterTable = new MessageFilterTable<StrictAndMessageFilter>();
            }

            public int Count
            {
                get { return this.andFilters.Count; }
            }

            public bool IsReadOnly
            {
                get { return true; }
            }

            public ICollection<MessageFilter> Keys
            {
                get { return this.andFilters.Keys; }
            }

            public ICollection<TFilterData> Values
            {
                get { return this.andFilters.Values; }
            }

            public TFilterData this[MessageFilter key]
            {
                get
                {
                    return this.andFilters[key];
                }
                set
                {
                    this.andFilters[key] = value;
                }
            }

            public bool GetMatchingFilter(MessageBuffer messageBuffer, out MessageFilter filter)
            {
                throw FxTrace.Exception.AsError(new NotImplementedException());
            }

            public bool GetMatchingFilter(Message message, out MessageFilter filter)
            {
                throw FxTrace.Exception.AsError(new NotImplementedException());
            }

            public bool GetMatchingFilters(MessageBuffer messageBuffer, ICollection<MessageFilter> results)
            {
                if (messageBuffer == null)
                {
                    throw FxTrace.Exception.ArgumentNull("messageBuffer");
                }

                List<MessageFilter> firstPassResults = new List<MessageFilter>();
                if (this.filterTable.GetMatchingFilters(messageBuffer, firstPassResults))
                {
                    IList<StrictAndMessageFilter> matchingFilters = FindMatchingAndFilters(firstPassResults);
                    foreach (StrictAndMessageFilter andFilter in matchingFilters)
                    {
                        results.Add(andFilter);
                    }
                    return (matchingFilters.Count > 0);
                }
                return false;
            }

            public bool GetMatchingFilters(Message message, ICollection<MessageFilter> results)
            {
                if (message == null)
                {
                    throw FxTrace.Exception.ArgumentNull("message");
                }

                List<MessageFilter> firstPassResults = new List<MessageFilter>();
                if (this.filterTable.GetMatchingFilters(message, firstPassResults))
                {
                    IList<StrictAndMessageFilter> matchingFilters = FindMatchingAndFilters(firstPassResults);
                    foreach (StrictAndMessageFilter andFilter in matchingFilters)
                    {
                        results.Add(andFilter);
                    }
                    return (matchingFilters.Count > 0);
                }
                return false;
            }

            public bool GetMatchingValue(MessageBuffer messageBuffer, out TFilterData value)
            {
                value = default(TFilterData);
                List<TFilterData> results = new List<TFilterData>();
                bool result = this.GetMatchingValues(messageBuffer, results);
                if (results.Count == 1)
                {
                    value = results[0];
                }
                if (results.Count > 1)
                {
                    throw FxTrace.Exception.AsError(new MultipleFilterMatchesException());
                }
                return result;
            }

            public bool GetMatchingValue(Message message, out TFilterData value)
            {
                value = default(TFilterData);
                List<TFilterData> results = new List<TFilterData>();
                bool result = this.GetMatchingValues(message, results);
                if (results.Count == 1)
                {
                    value = results[0];
                }
                else if (results.Count > 1)
                {
                    throw FxTrace.Exception.AsError(new MultipleFilterMatchesException());
                }
                return result;
            }

            public bool GetMatchingValues(MessageBuffer messageBuffer, ICollection<TFilterData> results)
            {
                if (messageBuffer == null)
                {
                    throw FxTrace.Exception.ArgumentNull("messageBuffer");
                }

                List<MessageFilter> firstPassResults = new List<MessageFilter>();
                if (this.filterTable.GetMatchingFilters(messageBuffer, firstPassResults))
                {
                    IList<StrictAndMessageFilter> matchingFilters = FindMatchingAndFilters(firstPassResults);
                    foreach (StrictAndMessageFilter andFilter in matchingFilters)
                    {
                        results.Add(this.andFilters[andFilter]);
                    }
                    return (matchingFilters.Count > 0);
                }
                return false;
            }

            public bool GetMatchingValues(Message message, ICollection<TFilterData> results)
            {
                if (message == null)
                {
                    throw FxTrace.Exception.ArgumentNull("message");
                }

                List<MessageFilter> firstPassResults = new List<MessageFilter>();
                if (this.filterTable.GetMatchingFilters(message, firstPassResults))
                {
                    IList<StrictAndMessageFilter> matchingFilters = FindMatchingAndFilters(firstPassResults);
                    foreach (StrictAndMessageFilter andFilter in matchingFilters)
                    {
                        results.Add(this.andFilters[andFilter]);
                    }
                    return (matchingFilters.Count > 0);
                }
                return false;
            }

            IList<StrictAndMessageFilter> FindMatchingAndFilters(List<MessageFilter> firstPassResults)
            {
                IList<StrictAndMessageFilter> matchingFilters = new List<StrictAndMessageFilter>();
                foreach (MessageFilter filter in firstPassResults)
                {
                    StrictAndMessageFilter andFilter = this.filterTable[filter];
                    // Check if this StrictAndMessageFilter is already in our result set
                    if (!matchingFilters.Contains(andFilter))
                    {
                        if (firstPassResults.Contains(andFilter.filter1) && firstPassResults.Contains(andFilter.filter2))
                        {
                            matchingFilters.Add(andFilter);
                        }
                    }
                }
                return matchingFilters;
            }

            public void Add(MessageFilter key, TFilterData value)
            {
                StrictAndMessageFilter andFilter = (StrictAndMessageFilter)key;
                this.andFilters.Add(andFilter, value);
                this.filterTable.Add(andFilter.filter1, andFilter);
                this.filterTable.Add(andFilter.filter2, andFilter);
            }

            public bool ContainsKey(MessageFilter key)
            {
                return this.andFilters.ContainsKey(key);
            }

            public bool Remove(MessageFilter key)
            {
                StrictAndMessageFilter andFilter = (StrictAndMessageFilter)key;
                if (this.andFilters.Remove(andFilter))
                {
                    this.filterTable.Remove(andFilter.filter1);
                    this.filterTable.Remove(andFilter.filter2);
                    return true;
                }
                return false;
            }

            public bool TryGetValue(MessageFilter key, out TFilterData value)
            {
                return this.andFilters.TryGetValue(key, out value);
            }

            public void Add(KeyValuePair<MessageFilter, TFilterData> item)
            {
                this.Add(item.Key, item.Value);
            }

            public void Clear()
            {
                this.andFilters.Clear();
                this.filterTable.Clear();
            }

            public bool Contains(KeyValuePair<MessageFilter, TFilterData> item)
            {
                return this.andFilters.Contains(item);
            }

            public void CopyTo(KeyValuePair<MessageFilter, TFilterData>[] array, int arrayIndex)
            {
                throw FxTrace.Exception.AsError(new NotImplementedException());
            }

            public bool Remove(KeyValuePair<MessageFilter, TFilterData> item)
            {
                return this.andFilters.Remove(item.Key);
            }

            public IEnumerator<KeyValuePair<MessageFilter, TFilterData>> GetEnumerator()
            {
                throw FxTrace.Exception.AsError(new NotImplementedException());
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                throw FxTrace.Exception.AsError(new NotImplementedException());
            }
        }
    }
}
