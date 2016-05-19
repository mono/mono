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

    #region ListSurrogate
    internal sealed class ListSurrogate : ISerializationSurrogate
    {
        internal ListSurrogate() { }
        void ISerializationSurrogate.GetObjectData(object obj, SerializationInfo info, StreamingContext context)
        {
            if (!obj.GetType().IsGenericType || obj.GetType().GetGenericTypeDefinition() != typeof(List<>))
                throw new ArgumentException(SR.GetString(SR.Error_InvalidArgumentValue), "obj");

            Type[] args = obj.GetType().GetGenericArguments();
            if (args.Length != 1)
                throw new ArgumentException(SR.GetString(SR.Error_InvalidArgumentValue), "obj");

            ArrayList items = new ArrayList(obj as IList);
            if (items.Count == 1)
                info.AddValue("item", items[0]);
            else
                info.AddValue("items", items.ToArray());

            info.AddValue("itemType", args[0]);
            info.SetType(typeof(ListRef));
        }
        object ISerializationSurrogate.SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
        {
            return null;
        }

        #region ListRef
        [Serializable]
        private sealed class ListRef : IObjectReference, IDeserializationCallback
        {
            [OptionalField]
            private IList items = null;
            [OptionalField]
            private object item = null;
            private Type itemType = null;

            [NonSerialized]
            private object list = null;

            Object IObjectReference.GetRealObject(StreamingContext context)
            {
                if (this.list == null)
                {
                    Type listType = typeof(List<int>).GetGenericTypeDefinition().MakeGenericType(itemType);
                    this.list = listType.GetConstructor(Type.EmptyTypes).Invoke(null);
                }
                return this.list;
            }
            void IDeserializationCallback.OnDeserialization(Object sender)
            {
                if (this.list != null)
                {
                    MethodInfo addMethod = this.list.GetType().GetMethod("Add");
                    if (addMethod == null)
                        throw new NullReferenceException("addMethod");

                    if (this.items != null)
                    {
                        for (int n = 0; n < items.Count; n++)
                            addMethod.Invoke(this.list, new object[] { this.items[n] });
                    }
                    else
                    {
                        addMethod.Invoke(this.list, new object[] { this.item });
                    }
                    this.list = null;
                }
            }
        }
        #endregion
    }
    #endregion
}
