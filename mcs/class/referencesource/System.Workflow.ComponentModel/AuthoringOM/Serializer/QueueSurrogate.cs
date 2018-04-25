namespace System.Workflow.ComponentModel.Serialization
{
    using System;
    using System.Xml;
    using System.Runtime.Serialization;
    using System.Reflection;
    using System.IO;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Collections;
    using System.Collections.Generic;

    #region QueueSurrogate
    internal sealed class QueueSurrogate : ISerializationSurrogate
    {
        internal QueueSurrogate() { }
        void ISerializationSurrogate.GetObjectData(object obj, SerializationInfo info, StreamingContext context)
        {
            object[] items = ((Queue)obj).ToArray();
            if (items.Length == 1)
                info.AddValue("item", items[0]);
            else
                info.AddValue("items", items);
            info.SetType(typeof(QRef));
        }

        object ISerializationSurrogate.SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
        {
            return null;
        }

        #region QRef
        [Serializable]
        private sealed class QRef : IObjectReference, IDeserializationCallback
        {
            [OptionalField]
            private IList items = null;
            [OptionalField]
            private object item = null;

            [NonSerialized]
            private Queue queue = null;

            Object IObjectReference.GetRealObject(StreamingContext context)
            {
                if (this.queue == null)
                {
                    this.queue = new Queue();
                }
                return this.queue;
            }
            void IDeserializationCallback.OnDeserialization(Object sender)
            {
                if (this.queue != null)
                {
                    if (this.items != null)
                    {
                        for (int n = 0; n < this.items.Count; n++)
                            this.queue.Enqueue(items[n]);
                    }
                    else
                    {
                        this.queue.Enqueue(this.item);
                    }
                    this.queue = null;
                }
            }
        }
        #endregion
    }
    #endregion
}
