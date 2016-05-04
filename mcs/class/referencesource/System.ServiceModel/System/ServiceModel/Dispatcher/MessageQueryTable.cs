//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime;
    using System.ServiceModel.Channels;

    public class MessageQueryTable<TItem> : IDictionary<MessageQuery, TItem>
    {
        Dictionary<Type, MessageQueryCollection> collectionsByType;
        Dictionary<MessageQuery, TItem> dictionary;        

        public MessageQueryTable()
        {            
            this.dictionary = new Dictionary<MessageQuery, TItem>();
            this.collectionsByType = new Dictionary<Type, MessageQueryCollection>();           
        }

        public int Count
        {
            get { return this.dictionary.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public ICollection<MessageQuery> Keys
        {
            get { return this.dictionary.Keys; }
        }

        public ICollection<TItem> Values
        {
            get { return this.dictionary.Values; }
        }

        public TItem this[MessageQuery key]
        {
            get
            {
                return this.dictionary[key];
            }
            set
            {
                this.Add(key, value);
            }
        }

        [SuppressMessage(FxCop.Category.Usage, "CA2301:EmbeddableTypesInContainersRule", MessageId = "collectionsByType", Justification = "No need to support type equivalence here.")]
        public void Add(MessageQuery key, TItem value)
        {
            if (key == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("key");
            }

            Type queryType = key.GetType();
            MessageQueryCollection collection;

            if (!this.collectionsByType.TryGetValue(queryType, out collection))
            {
                collection = key.CreateMessageQueryCollection();

                if (collection == null)
                {
                    collection = new SequentialMessageQueryCollection();                    
                }

                this.collectionsByType.Add(queryType, collection);
            }
           
           collection.Add(key);
           this.dictionary.Add(key, value);
        }

        public void Add(KeyValuePair<MessageQuery, TItem> item)
        {
            this.Add(item.Key, item.Value);
        }

        public void Clear()
        {            
            this.collectionsByType.Clear();
            this.dictionary.Clear();
        }

        public bool Contains(KeyValuePair<MessageQuery, TItem> item)
        {
            return ((ICollection<KeyValuePair<MessageQuery, TItem>>) this.dictionary).Contains(item);
        }

        public bool ContainsKey(MessageQuery key)
        {
            return this.dictionary.ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<MessageQuery, TItem>[] array, int arrayIndex)
        {
            ((ICollection<KeyValuePair<MessageQuery, TItem>>) this.dictionary).CopyTo(array, arrayIndex);
        }

        public IEnumerable<KeyValuePair<MessageQuery, TResult>> Evaluate<TResult>(Message message)
        {
            if (message == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("message");
            }

            return new MessageEnumerable<TResult>(this, message);
        }

        public IEnumerable<KeyValuePair<MessageQuery, TResult>> Evaluate<TResult>(MessageBuffer buffer)
        {
            if (buffer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("buffer");
            }

            return new MessageBufferEnumerable<TResult>(this, buffer);
        }

        public IEnumerator<KeyValuePair<MessageQuery, TItem>> GetEnumerator()
        {
            return ((ICollection<KeyValuePair<MessageQuery, TItem>>) this.dictionary).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        [SuppressMessage(FxCop.Category.Usage, "CA2301:EmbeddableTypesInContainersRule", MessageId = "collectionsByType", Justification = "No need to support type equivalence here.")]
        public bool Remove(MessageQuery key)
        {
            if (this.dictionary.Remove(key))
            {
                MessageQueryCollection collection;
                Type queryType = key.GetType();

                collection = this.collectionsByType[queryType];
                collection.Remove(key);

                if (collection.Count == 0)
                {
                    this.collectionsByType.Remove(queryType);
                }
               
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool Remove(KeyValuePair<MessageQuery, TItem> item)
        {
            return this.Remove(item.Key);
        }

        public bool TryGetValue(MessageQuery key, out TItem value)
        {
            return this.dictionary.TryGetValue(key, out value);
        }

        class SequentialMessageQueryCollection : MessageQueryCollection
        {
            public override IEnumerable<KeyValuePair<MessageQuery, TResult>> Evaluate<TResult>(Message message)
            {
                return new MessageSequentialResultEnumerable<TResult>(this, message);
            }

            public override IEnumerable<KeyValuePair<MessageQuery, TResult>> Evaluate<TResult>(MessageBuffer buffer)
            {
                return new MessageBufferSequentialResultEnumerable<TResult>(this, buffer);
            }

            abstract class SequentialResultEnumerable<TSource, TResult> : IEnumerable<KeyValuePair<MessageQuery, TResult>>
            {
                SequentialMessageQueryCollection collection;
                TSource source;

                public SequentialResultEnumerable(SequentialMessageQueryCollection collection, TSource source)
                {
                    this.collection = collection;
                    this.source = source;
                }

                SequentialMessageQueryCollection Collection
                {
                    get
                    {
                        return this.collection;
                    }
                }

                protected TSource Source
                {
                    get
                    {
                        return this.source;
                    }
                }

                public IEnumerator<KeyValuePair<MessageQuery, TResult>> GetEnumerator()
                {
                    return new SequentialResultEnumerator(this);
                }

                IEnumerator IEnumerable.GetEnumerator()
                {
                    return this.GetEnumerator();
                }

                protected abstract TResult Evaluate(MessageQuery query);                

                class SequentialResultEnumerator : IEnumerator<KeyValuePair<MessageQuery, TResult>>
                {
                    SequentialResultEnumerable<TSource, TResult> enumerable;
                    IEnumerator<MessageQuery> queries;

                    public SequentialResultEnumerator(SequentialResultEnumerable<TSource, TResult> enumerable)
                    {
                        this.enumerable = enumerable;
                        this.queries = enumerable.Collection.GetEnumerator();
                    }
                   
                    public KeyValuePair<MessageQuery, TResult> Current
                    {
                        get 
                        {
                            MessageQuery query = queries.Current;
                            TResult result = enumerable.Evaluate(query);

                            return new KeyValuePair<MessageQuery, TResult>(query, result);
                        }
                    }  

                    object IEnumerator.Current
                    {
                        get 
                        { 
                            return this.Current; 
                        }
                    }

                    public void Dispose()
                    {                        
                    } 
                    
                    public bool MoveNext()
                    {
                        return this.queries.MoveNext();
                    }

                    public void Reset()
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
                    }                    
                }
            }

            class MessageSequentialResultEnumerable<TResult> : SequentialResultEnumerable<Message, TResult>
            {
                public MessageSequentialResultEnumerable(
                    SequentialMessageQueryCollection collection,  Message message)
                    : base(collection, message)
                {
                }

                protected override TResult Evaluate(MessageQuery query)
                {
                    return query.Evaluate<TResult>(this.Source);
                }
            }

            class MessageBufferSequentialResultEnumerable<TResult> : SequentialResultEnumerable<MessageBuffer, TResult>
            {
                public MessageBufferSequentialResultEnumerable(
                    SequentialMessageQueryCollection collection, MessageBuffer buffer)
                    : base(collection, buffer)
                {                    
                }

                protected override TResult Evaluate(MessageQuery query)
                {
                    return query.Evaluate<TResult>(this.Source);
                }
            }
        }

        abstract class Enumerable<TSource, TResult> : IEnumerable<KeyValuePair<MessageQuery, TResult>>
        {
            TSource source;
            MessageQueryTable<TItem> table;

            public Enumerable(MessageQueryTable<TItem> table, TSource source)
            {
                this.table = table;
                this.source = source;
            }

            protected TSource Source
            {
                get
                {
                    return this.source;
                }
            }

            public IEnumerator<KeyValuePair<MessageQuery, TResult>> GetEnumerator()
            {
                return new Enumerator(this);
            }

            protected abstract IEnumerator<KeyValuePair<MessageQuery, TResult>> GetInnerEnumerator(MessageQueryCollection collection);

            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }

            class Enumerator : IEnumerator<KeyValuePair<MessageQuery, TResult>>
            {
                Enumerable<TSource, TResult> enumerable;
                IEnumerator<KeyValuePair<MessageQuery, TResult>> innerEnumerator;
                IEnumerator<MessageQueryCollection> outerEnumerator;

                public Enumerator(Enumerable<TSource, TResult> enumerable)
                {
                    this.outerEnumerator = enumerable.table.collectionsByType.Values.GetEnumerator();
                    this.enumerable = enumerable;
                }

                public KeyValuePair<MessageQuery, TResult> Current
                {
                    get { return this.innerEnumerator.Current; }
                }

                object IEnumerator.Current
                {
                    get
                    {
                        return this.Current;
                    }
                }

                public void Dispose()
                {
                }

                public bool MoveNext()
                {
                    if (innerEnumerator == null || !this.innerEnumerator.MoveNext())
                    {
                        if (!this.outerEnumerator.MoveNext())
                        {
                            return false;
                        }
                        else
                        {
                            MessageQueryCollection collection = this.outerEnumerator.Current;

                            this.innerEnumerator = this.enumerable.GetInnerEnumerator(collection);
                            return this.innerEnumerator.MoveNext();
                        }
                    }
                    else
                    {
                        return true;
                    }
                }

                public void Reset()
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
                }
            }
        }

        class MessageBufferEnumerable<TResult> : Enumerable<MessageBuffer, TResult>
        {
            public MessageBufferEnumerable(MessageQueryTable<TItem> table, MessageBuffer buffer)
                : base(table, buffer)
            {
            }

            protected override IEnumerator<KeyValuePair<MessageQuery, TResult>> GetInnerEnumerator(
                MessageQueryCollection collection)
            {
                return collection.Evaluate<TResult>(this.Source).GetEnumerator();
            }
        }

        class MessageEnumerable<TResult> : Enumerable<Message, TResult>
        {
            public MessageEnumerable(MessageQueryTable<TItem> table, Message message)
                : base(table, message)
            {
            }

            protected override IEnumerator<KeyValuePair<MessageQuery, TResult>> GetInnerEnumerator(
                MessageQueryCollection collection)
            {
                return collection.Evaluate<TResult>(this.Source).GetEnumerator();
            }
        }
    }
}
