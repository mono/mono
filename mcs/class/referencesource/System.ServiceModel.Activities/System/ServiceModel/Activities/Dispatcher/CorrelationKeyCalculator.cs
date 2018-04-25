//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.ServiceModel.Activities.Dispatcher
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime;
    using System.Runtime.Collections;
    using System.Runtime.Diagnostics;
    using System.Runtime.DurableInstancing;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Dispatcher;
    using System.Xml.Linq;
    using System.Text;
    using SR2 = System.ServiceModel.Activities.SR;

    class CorrelationKeyCalculator
    {
        MessageBufferCalculator bufferCalculator;
        MessageCalculator messageCalculator;
        XName scopeName;
        MessageFilterTable<SelectRuntime> whereRuntime;
        CorrelationKeyCache keyCache;

        public CorrelationKeyCalculator(XName scopeName)
        {
            this.whereRuntime = new MessageFilterTable<SelectRuntime>();
            this.scopeName = scopeName;
            this.keyCache = new CorrelationKeyCache();
        }

        public void AddQuery(MessageFilter where, MessageQueryTable<string> select,
            IDictionary<string, MessageQueryTable<string>> selectAdditional, bool isContextQuery)
        {
            SelectRuntime selectRuntime = new SelectRuntime { Select = select, SelectAdditional = selectAdditional, IsContextQuery = isContextQuery };
            this.whereRuntime.Add(where, selectRuntime);
        }

        public bool CalculateKeys(Message message, out InstanceKey instanceKey,
            out ICollection<InstanceKey> additionalKeys)
        {
            MessageCalculator calculator = this.messageCalculator;

            if (calculator == null)
            {
                calculator = this.messageCalculator = new MessageCalculator(this);
            }

            return calculator.CalculateKeys(message, null, out instanceKey, out additionalKeys);
        }

        public bool CalculateKeys(MessageBuffer buffer, Message messageToReadHeaders, out InstanceKey instanceKey,
          out ICollection<InstanceKey> additionalKeys)
        {
            MessageBufferCalculator calculator = this.bufferCalculator;

            if (calculator == null)
            {
                calculator = this.bufferCalculator = new MessageBufferCalculator(this);
            }

            return calculator.CalculateKeys(buffer, messageToReadHeaders, out instanceKey, out additionalKeys);
        }

        abstract class Calculator<T>
        {
            CorrelationKeyCalculator parent;

            public Calculator(CorrelationKeyCalculator parent)
            {
                this.parent = parent;
            }

            public bool CalculateKeys(T target, Message messageToReadHeaders, out InstanceKey instanceKey,
                out ICollection<InstanceKey> additionalKeys)
            {
                SelectRuntime select;

                instanceKey = InstanceKey.InvalidKey;
                additionalKeys = null;

                // this is a query on the serverside, either Receive or SendReply
                // Where
                if (!this.ExecuteWhere(target, messageToReadHeaders, this.parent.whereRuntime, out select))
                {
                    return false;
                }

                Dictionary<string, string> values = new Dictionary<string, string>();

                // Select
                if (select.Select.Count > 0)
                {
                    bool allOptional = true;

                    foreach (KeyValuePair<MessageQuery, string> result in this.ExecuteSelect(target, messageToReadHeaders, select.Select, select.IsContextQuery))
                    {
                        if (!(result.Key is OptionalMessageQuery))
                        {
                            allOptional = false;
                        }

                        if (!string.IsNullOrEmpty(result.Value))
                        {
                            values.Add(select.Select[result.Key], result.Value);
                        }
                    }

                    if (values.Count == 0)
                    {
                        if (!allOptional)
                        {
                            throw FxTrace.Exception.AsError(new ProtocolException(SR2.EmptyCorrelationQueryResults));
                        }
                    }
                    else
                    {
                        instanceKey = this.GetInstanceKey(values);
                        if (TD.TraceCorrelationKeysIsEnabled())
                        {
                            TraceCorrelationKeys(instanceKey, values);
                        }
                    }
                }

                // SelectAdditional                
                foreach (KeyValuePair<string, MessageQueryTable<string>> item in select.SelectAdditional)
                {
                    if (additionalKeys == null)
                    {
                        additionalKeys = new List<InstanceKey>();
                    }

                    values.Clear();

                    InstanceKey additionalKey = InstanceKey.InvalidKey;
                    bool allOptional = true;

                    foreach (KeyValuePair<MessageQuery, string> result in this.ExecuteSelect(target, messageToReadHeaders, item.Value, select.IsContextQuery))
                    {
                        if (!(result.Key is OptionalMessageQuery))
                        {
                            allOptional = false;
                        }

                        if (!string.IsNullOrEmpty(result.Value))
                        {
                            values.Add(item.Value[result.Key], result.Value);
                        }
                    }

                    if (values.Count == 0)
                    {
                        if (!allOptional)
                        {
                            throw FxTrace.Exception.AsError(new ProtocolException(SR2.EmptyCorrelationQueryResults));
                        }
                    }
                    else
                    {
                        additionalKey = new CorrelationKey(values, this.parent.scopeName.ToString(), null) { Name = item.Key };
                        if (TD.TraceCorrelationKeysIsEnabled())
                        {
                            TraceCorrelationKeys(additionalKey, values);
                        }
                    }

                    additionalKeys.Add(additionalKey);
                }

                return true;
            }

            CorrelationKey GetInstanceKey(Dictionary<string, string> values)
            {
                // We only optimize for upto 3 keys                
                if (values.Count <= 3)
                {
                    CorrelationKey correlationKey;
                    CorrelationCacheKey cacheKey = CorrelationCacheKey.CreateKey(values);
                    if (this.parent.keyCache.TryGetValue(cacheKey, out correlationKey))
                    {
                        return correlationKey;
                    }

                    correlationKey = new CorrelationKey(values, this.parent.scopeName.ToString(), null);
                    this.parent.keyCache.Add(cacheKey, correlationKey);
                    return correlationKey;
                }

                return new CorrelationKey(values, this.parent.scopeName.ToString(), null);
            }

            protected abstract IEnumerable<KeyValuePair<MessageQuery, string>> ExecuteSelect(T target, Message messageToReadHeaders,
                MessageQueryTable<string> select, bool IsContextQuery);

            protected abstract bool ExecuteWhere(T target, Message messageToReadHeaders, MessageFilterTable<SelectRuntime> whereRuntime,
                out SelectRuntime select);

            void TraceCorrelationKeys(InstanceKey instanceKey, Dictionary<string, string> values)
            {
                StringBuilder keyValueAsString = new StringBuilder();
                foreach (KeyValuePair<string, string> pair in values)
                {
                    keyValueAsString.Append(pair.Key).Append(":").Append(pair.Value).Append(',');
                }
                TD.TraceCorrelationKeys(instanceKey.Value, keyValueAsString.ToString(), this.parent.scopeName.ToString());
            }

        }

        class MessageBufferCalculator : Calculator<MessageBuffer>
        {

            public MessageBufferCalculator(CorrelationKeyCalculator parent)
                : base(parent)
            {

            }

            protected override IEnumerable<KeyValuePair<MessageQuery, string>> ExecuteSelect(MessageBuffer target, Message messageToReadHeaders,
                MessageQueryTable<string> select, bool isContextQuery)
            {
                if (isContextQuery && messageToReadHeaders != null)
                {
                    //we can pass in the message directly in this case since we know it is a context query that will read from the header
                    return select.Evaluate<string>(messageToReadHeaders);
                }
                else
                {
                    return select.Evaluate<string>(target);
                }
            }

            protected override bool ExecuteWhere(MessageBuffer target, Message messageToReadHeaders, MessageFilterTable<SelectRuntime> whereRuntime,
                out SelectRuntime select)
            {
                return whereRuntime.GetMatchingValue(target, messageToReadHeaders, out select);
            }
        }

        [SuppressMessage(FxCop.Category.Performance, FxCop.Rule.AvoidUncalledPrivateCode,
            Justification = "Will use this once correlation with streaming is fixed")]
        class MessageCalculator : Calculator<Message>
        {
            [SuppressMessage(FxCop.Category.Performance, FxCop.Rule.AvoidUncalledPrivateCode,
                Justification = "Will use this once correlation with streaming is fixed")]
            public MessageCalculator(CorrelationKeyCalculator parent)
                : base(parent)
            {
            }

            protected override IEnumerable<KeyValuePair<MessageQuery, string>> ExecuteSelect(Message target, Message messageToReadHeaders,
                MessageQueryTable<string> select, bool isContextQuery)
            {
                return select.Evaluate<string>(target);
            }

            protected override bool ExecuteWhere(Message target, Message messageToReadHeaders, MessageFilterTable<SelectRuntime> whereRuntime,
                out SelectRuntime select)
            {
                // messageToReadHeaders is not used in case of MessageCalculator
                return whereRuntime.GetMatchingValue(target, out select);
            }
        }

        class SelectRuntime
        {
            public MessageQueryTable<string> Select { get; set; }
            public IDictionary<string, MessageQueryTable<string>> SelectAdditional { get; set; }
            internal bool IsContextQuery { get; set; }
        }

        // Needs to seperate from the generic calculator as all jitted types 
        // should share the same cache. 
        class CorrelationKeyCache
        {
            HopperCache cache;
            object cacheLock;

            internal CorrelationKeyCache()
            {
                this.cache = new HopperCache(128, false);
                this.cacheLock = new object();
            }

            internal void Add(CorrelationCacheKey key, CorrelationKey value)
            {
                Fx.Assert(key != null, "Cannot add a null CorrelationCacheKey to the cache.");
                lock (this.cacheLock)
                {
                    this.cache.Add(key, value);
                }
            }

            internal bool TryGetValue(CorrelationCacheKey key, out CorrelationKey value)
            {
                value = (CorrelationKey)this.cache.GetValue(this.cacheLock, key);
                return (value != null);
            }
        }

        abstract class CorrelationCacheKey
        {
            static internal CorrelationCacheKey CreateKey(Dictionary<string, string> keys)
            {
                if (keys.Count == 1)
                {
                    return new SingleCacheKey(keys);
                }
                else
                {
                    return new MultipleCacheKey(keys);
                }
            }

            static int CombineHashCodes(int h1, int h2)
            {
                return (((h1 << 5) + h1) ^ h2);
            }

            class SingleCacheKey : CorrelationCacheKey
            {
                int hashCode;
                string key;
                string value;

                public SingleCacheKey(Dictionary<string, string> keys)
                {
                    Fx.Assert(keys.Count == 1, "Cannot intialize CorrelationCacheSingleKey with multiple key values.");
                    foreach (KeyValuePair<string, string> keyValue in keys)
                    {
                        this.key = keyValue.Key;
                        this.value = keyValue.Value;
                        this.hashCode = CombineHashCodes(this.key.GetHashCode(), this.value.GetHashCode());
                        return;
                    }
                }

                public override bool Equals(object obj)
                {
                    SingleCacheKey target = obj as SingleCacheKey;
                    return (target != null &&
                        (this.hashCode == target.hashCode) &&
                        ((this.key == target.key) && (this.value == target.value)));
                }

                public override int GetHashCode()
                {
                    return this.hashCode;
                }
            }

            class MultipleCacheKey : CorrelationCacheKey
            {
                Dictionary<string, string> keyValues;
                int hashCode;

                public MultipleCacheKey(Dictionary<string, string> keys)
                {
                    this.keyValues = keys;

                    foreach (KeyValuePair<string, string> keyValue in this.keyValues)
                    {
                        int hash1 = CombineHashCodes(this.hashCode, keyValue.Key.GetHashCode());
                        this.hashCode = CombineHashCodes(hash1, keyValue.Value.GetHashCode());
                    }
                }

                public override bool Equals(object obj)
                {
                    MultipleCacheKey target = obj as MultipleCacheKey;
                    if (target != null)
                    {
                        if ((this.hashCode == target.hashCode) &&
                            (this.keyValues.Count == target.keyValues.Count))
                        {
                            string sourceValue;
                            foreach (KeyValuePair<string, string> targetKeyValue in target.keyValues)
                            {
                                if (!this.keyValues.TryGetValue(targetKeyValue.Key, out sourceValue) ||
                                    sourceValue != targetKeyValue.Value)
                                {
                                    return false;
                                }
                            }

                            //All keys and values are the same
                            return true;
                        }
                    }

                    return false;
                }

                public override int GetHashCode()
                {
                    return this.hashCode;
                }
            }
        }

    }
}
