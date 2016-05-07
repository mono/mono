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

    #region GenericQueueSurrogate
    internal sealed class GenericQueueSurrogate : ISerializationSurrogate
    {
        internal GenericQueueSurrogate() { }
        void ISerializationSurrogate.GetObjectData(object obj, SerializationInfo info, StreamingContext context)
        {
            if (!obj.GetType().IsGenericType || obj.GetType().GetGenericTypeDefinition() != typeof(Queue<>))
                throw new ArgumentException(SR.GetString(SR.Error_InvalidArgumentValue), "obj");

            Type[] args = obj.GetType().GetGenericArguments();
            if (args.Length != 1)
                throw new ArgumentException(SR.GetString(SR.Error_InvalidArgumentValue), "obj");

            ArrayList items = new ArrayList(obj as ICollection);
            if (items.Count == 1)
                info.AddValue("item", items[0]);
            else
                info.AddValue("items", items.ToArray());
            info.AddValue("itemType", args[0]);
            info.SetType(typeof(GenericQRef));
        }
        object ISerializationSurrogate.SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
        {
            return null;
        }

        #region GenericQRef
        [Serializable]
        private sealed class GenericQRef : IObjectReference, IDeserializationCallback
        {
            [OptionalField]
            private IList items = null;
            [OptionalField]
            private object item = null;
            private Type itemType = null;

            [NonSerialized]
            private object queue = null;

            Object IObjectReference.GetRealObject(StreamingContext context)
            {
                if (this.queue == null)
                {
                    Type queueType = typeof(Queue<int>).GetGenericTypeDefinition().MakeGenericType(itemType);
                    this.queue = queueType.GetConstructor(Type.EmptyTypes).Invoke(null);
                }
                return this.queue;
            }
            void IDeserializationCallback.OnDeserialization(Object sender)
            {
                if (this.queue != null)
                {
                    MethodInfo enqueueMethod = this.queue.GetType().GetMethod("Enqueue");
                    if (enqueueMethod == null)
                        throw new NullReferenceException("enqueueMethod");

                    if (this.items != null)
                    {
                        for (int n = 0; n < items.Count; n++)
                            enqueueMethod.Invoke(this.queue, new object[] { this.items[n] });
                    }
                    else
                    {
                        enqueueMethod.Invoke(this.queue, new object[] { this.item });
                    }
                    this.queue = null;
                }
            }
        }
        #endregion
    }
    #endregion

}
