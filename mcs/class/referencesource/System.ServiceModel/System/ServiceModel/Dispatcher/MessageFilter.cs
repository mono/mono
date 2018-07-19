//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.ServiceModel.Channels;
    using System.ServiceModel;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Runtime.Serialization;
    using System.ServiceModel.Diagnostics;
    using System.Runtime;
    /// <summary>
    /// Abstract base class for different classes of message filters
    /// </summary>
    [DataContract]
    [KnownType(typeof(XPathMessageFilter))]
    [KnownType(typeof(ActionMessageFilter))]
    [KnownType(typeof(MatchAllMessageFilter))]
    [KnownType(typeof(MatchNoneMessageFilter))]
    //[KnownType(typeof(EndpointAddressMessageFilter))]
    [Fx.Tag.XamlVisible(true)]
    public abstract class MessageFilter
    {
        protected MessageFilter()
        {
        }

        protected internal virtual IMessageFilterTable<FilterData> CreateFilterTable<FilterData>()
        {
            return null;
        }

        /// <summary>
        /// Tests whether the filter matches the given message.
        /// </summary>                
        public abstract bool Match(MessageBuffer buffer);

        /// <summary>
        /// Tests whether the filter matches the given message without examining its body.
        /// Note: since this method never probes the message body, it should NOT close the message
        /// If the filter probes the message body, then the filter must THROW an Exception. The filter should not return false
        /// This is deliberate - we don't want to produce false positives. 
        /// </summary>
        public abstract bool Match(Message message);
    }

    internal class SequentialMessageFilterTable<FilterData> : IMessageFilterTable<FilterData>
    {
        Dictionary<MessageFilter, FilterData> filters;

        public SequentialMessageFilterTable()
        {
            this.filters = new Dictionary<MessageFilter, FilterData>();
        }

        //
        // IMessageFilterTable<FilterData> methods
        //

        public int Count
        {
            get
            {
                return this.filters.Count;
            }
        }

        public void Clear()
        {
            this.filters.Clear();
        }

        public bool GetMatchingValue(Message message, out FilterData data)
        {
            bool dataSet = false;
            MessageFilter filter = null;
            data = default(FilterData);
            foreach (KeyValuePair<MessageFilter, FilterData> item in this.filters)
            {
                if (item.Key.Match(message))
                {
                    if (dataSet)
                    {
                        Collection<MessageFilter> f = new Collection<MessageFilter>();
                        f.Add(filter);
                        f.Add(item.Key);
                        throw TraceUtility.ThrowHelperError(new MultipleFilterMatchesException(SR.GetString(SR.FilterMultipleMatches), null, f), message);
                    }

                    filter = item.Key;
                    data = item.Value;
                    dataSet = true;
                }
            }

            return dataSet;
        }

        public bool GetMatchingValue(MessageBuffer buffer, out FilterData data)
        {
            bool dataSet = false;
            MessageFilter filter = null;
            data = default(FilterData);
            foreach (KeyValuePair<MessageFilter, FilterData> item in this.filters)
            {
                if (item.Key.Match(buffer))
                {
                    if (dataSet)
                    {
                        Collection<MessageFilter> f = new Collection<MessageFilter>();
                        f.Add(filter);
                        f.Add(item.Key);
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MultipleFilterMatchesException(SR.GetString(SR.FilterMultipleMatches), null, f));
                    }

                    filter = item.Key;
                    data = item.Value;
                    dataSet = true;
                }
            }

            return dataSet;
        }

        public bool GetMatchingValues(Message message, ICollection<FilterData> results)
        {
            int count = results.Count;
            foreach (KeyValuePair<MessageFilter, FilterData> item in this.filters)
            {
                if (item.Key.Match(message))
                {
                    results.Add(item.Value);
                }
            }
            return count != results.Count;
        }

        public bool GetMatchingValues(MessageBuffer buffer, ICollection<FilterData> results)
        {
            int count = results.Count;
            foreach (KeyValuePair<MessageFilter, FilterData> item in this.filters)
            {
                if (item.Key.Match(buffer))
                {
                    results.Add(item.Value);
                }
            }
            return count != results.Count;
        }

        public bool GetMatchingFilter(Message message, out MessageFilter filter)
        {
            filter = null;
            foreach (KeyValuePair<MessageFilter, FilterData> item in this.filters)
            {
                if (item.Key.Match(message))
                {
                    if (filter != null)
                    {
                        Collection<MessageFilter> f = new Collection<MessageFilter>();
                        f.Add(filter);
                        f.Add(item.Key);
                        throw TraceUtility.ThrowHelperError(new MultipleFilterMatchesException(SR.GetString(SR.FilterMultipleMatches), null, f), message);
                    }

                    filter = item.Key;
                }
            }

            return filter != null;
        }

        public bool GetMatchingFilter(MessageBuffer buffer, out MessageFilter filter)
        {
            filter = null;
            foreach (KeyValuePair<MessageFilter, FilterData> item in this.filters)
            {
                if (item.Key.Match(buffer))
                {
                    if (filter != null)
                    {
                        Collection<MessageFilter> f = new Collection<MessageFilter>();
                        f.Add(filter);
                        f.Add(item.Key);
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MultipleFilterMatchesException(SR.GetString(SR.FilterMultipleMatches), null, f));
                    }

                    filter = item.Key;
                }
            }

            return filter != null;
        }

        public bool GetMatchingFilters(Message message, ICollection<MessageFilter> results)
        {
            int count = results.Count;
            foreach (KeyValuePair<MessageFilter, FilterData> item in this.filters)
            {
                if (item.Key.Match(message))
                {
                    results.Add(item.Key);
                }
            }
            return count != results.Count;
        }

        public bool GetMatchingFilters(MessageBuffer buffer, ICollection<MessageFilter> results)
        {
            int count = results.Count;
            foreach (KeyValuePair<MessageFilter, FilterData> item in this.filters)
            {
                if (item.Key.Match(buffer))
                {
                    results.Add(item.Key);
                }
            }
            return count != results.Count;
        }

        //
        // IDictionary<MessageFilter,FilterData> methods
        //   

        public FilterData this[MessageFilter key]
        {
            get
            {
                return this.filters[key];
            }
            set
            {
                this.filters[key] = value;
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

        public bool ContainsKey(MessageFilter key)
        {
            return this.filters.ContainsKey(key);
        }

        public void Add(MessageFilter key, FilterData value)
        {
            this.filters.Add(key, value);
        }

        public bool Remove(MessageFilter key)
        {
            return this.filters.Remove(key);
        }

        //
        // ICollection<KeyValuePair<MessageFilter,FilterData>> methods
        //

        bool ICollection<KeyValuePair<MessageFilter, FilterData>>.IsReadOnly
        {
            get
            {
                return false;
            }
        }

        void ICollection<KeyValuePair<MessageFilter, FilterData>>.Add(KeyValuePair<MessageFilter, FilterData> item)
        {
            ((ICollection<KeyValuePair<MessageFilter, FilterData>>)this.filters).Add(item);
        }

        bool ICollection<KeyValuePair<MessageFilter, FilterData>>.Contains(KeyValuePair<MessageFilter, FilterData> item)
        {
            return ((ICollection<KeyValuePair<MessageFilter, FilterData>>)this.filters).Contains(item);
        }

        void ICollection<KeyValuePair<MessageFilter, FilterData>>.CopyTo(KeyValuePair<MessageFilter, FilterData>[] array, int arrayIndex)
        {
            ((ICollection<KeyValuePair<MessageFilter, FilterData>>)this.filters).CopyTo(array, arrayIndex);
        }

        bool ICollection<KeyValuePair<MessageFilter, FilterData>>.Remove(KeyValuePair<MessageFilter, FilterData> item)
        {
            return ((ICollection<KeyValuePair<MessageFilter, FilterData>>)this.filters).Remove(item);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<KeyValuePair<MessageFilter, FilterData>>)this).GetEnumerator();
        }

        IEnumerator<KeyValuePair<MessageFilter, FilterData>> IEnumerable<KeyValuePair<MessageFilter, FilterData>>.GetEnumerator()
        {
            return ((ICollection<KeyValuePair<MessageFilter, FilterData>>)this.filters).GetEnumerator();
        }

        public bool TryGetValue(MessageFilter filter, out FilterData data)
        {
            return this.filters.TryGetValue(filter, out data);
        }
    }
}
