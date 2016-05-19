//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.Generic;
    using System.Runtime;
    using System.Threading;

    abstract class DeliveryStrategy<ItemType> : IDisposable
        where ItemType : class, IDisposable
    {
        InputQueueChannel<ItemType> channel;
        Action dequeueCallback;
        int quota;

        public DeliveryStrategy(InputQueueChannel<ItemType> channel, int quota)
        {
            if (quota <= 0)
            {
                throw Fx.AssertAndThrow("Argument quota must be positive.");
            }

            this.channel = channel;
            this.quota = quota;
        }

        protected InputQueueChannel<ItemType> Channel
        {
            get
            {
                return this.channel;
            }
        }

        public Action DequeueCallback
        {
            get
            {
                return this.dequeueCallback;
            }
            set
            {
                this.dequeueCallback = value;
            }
        }

        public abstract int EnqueuedCount
        {
            get;
        }

        protected int Quota
        {
            get
            {
                return this.quota;
            }
        }

        public abstract bool CanEnqueue(Int64 sequenceNumber);

        public virtual void Dispose()
        {
        }

        public abstract bool Enqueue(ItemType item, Int64 sequenceNumber);
    }

    class OrderedDeliveryStrategy<ItemType> : DeliveryStrategy<ItemType>
        where ItemType : class, IDisposable
    {
        bool isEnqueueInOrder;
        Dictionary<Int64, ItemType> items;
        Action<object> onDispatchCallback;
        Int64 windowStart;

        public OrderedDeliveryStrategy(
            InputQueueChannel<ItemType> channel,
            int quota,
            bool isEnqueueInOrder)
            : base(channel, quota)
        {
            this.isEnqueueInOrder = isEnqueueInOrder;
            this.items = new Dictionary<Int64, ItemType>();
            this.windowStart = 1;
        }

        public override int EnqueuedCount
        {
            get
            {
                return this.Channel.InternalPendingItems + this.items.Count;
            }
        }

        Action<object> OnDispatchCallback
        {
            get
            {
                if (this.onDispatchCallback == null)
                {
                    this.onDispatchCallback = this.OnDispatch;
                }

                return this.onDispatchCallback;
            }
        }

        public override bool CanEnqueue(long sequenceNumber)
        {
            if (this.EnqueuedCount >= this.Quota)
            {
                return false;
            }

            if (this.isEnqueueInOrder && (sequenceNumber > this.windowStart))
            {
                return false;
            }

            return (this.Channel.InternalPendingItems + sequenceNumber - this.windowStart < this.Quota);
        }

        public override bool Enqueue(ItemType item, long sequenceNumber)
        {
            if (sequenceNumber > this.windowStart)
            {
                this.items.Add(sequenceNumber, item);
                return false;
            }

            this.windowStart++;

            while (this.items.ContainsKey(this.windowStart))
            {
                if (this.Channel.EnqueueWithoutDispatch(item, this.DequeueCallback))
                {
                    ActionItem.Schedule(this.OnDispatchCallback, null);
                }

                item = this.items[this.windowStart];
                this.items.Remove(this.windowStart);
                this.windowStart++;
            }

            return this.Channel.EnqueueWithoutDispatch(item, this.DequeueCallback);
        }

        static void DisposeItems(Dictionary<Int64, ItemType>.Enumerator items)
        {
            if (items.MoveNext())
            {
                using (ItemType item = items.Current.Value)
                {
                    DisposeItems(items);
                }
            }
        }

        public override void Dispose()
        {
            DisposeItems(this.items.GetEnumerator());
            this.items.Clear();

            base.Dispose();
        }

        void OnDispatch(object state)
        {
            this.Channel.Dispatch();
        }
    }

    class UnorderedDeliveryStrategy<ItemType> : DeliveryStrategy<ItemType>
        where ItemType : class, IDisposable
    {
        public UnorderedDeliveryStrategy(InputQueueChannel<ItemType> channel, int quota)
            : base(channel, quota)
        {
        }

        public override int EnqueuedCount
        {
            get
            {
                return this.Channel.InternalPendingItems;
            }
        }

        public override bool CanEnqueue(Int64 sequenceNumber)
        {
            return (this.EnqueuedCount < this.Quota);
        }

        public override bool Enqueue(ItemType item, long sequenceNumber)
        {
            return this.Channel.EnqueueWithoutDispatch(item, this.DequeueCallback);
        }
    }
}
