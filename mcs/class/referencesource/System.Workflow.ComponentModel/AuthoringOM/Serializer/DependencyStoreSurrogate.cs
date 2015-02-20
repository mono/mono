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

    #region DependencyStoreSurrogate
    internal sealed class DependencyStoreSurrogate : ISerializationSurrogate
    {
        internal DependencyStoreSurrogate() { }
        void ISerializationSurrogate.GetObjectData(object obj, SerializationInfo info, StreamingContext context)
        {
            IDictionary<DependencyProperty, object> store = obj as IDictionary<DependencyProperty, object>;
            if (store == null)
                throw new ArgumentException("obj");

            ArrayList properties = new ArrayList();
            ArrayList values = new ArrayList();

            foreach (KeyValuePair<DependencyProperty, object> kvp in store)
            {
                if (!kvp.Key.DefaultMetadata.IsNonSerialized)
                {
                    if (kvp.Key.IsKnown)
                        properties.Add(kvp.Key.KnownIndex);
                    else
                        properties.Add(kvp.Key);
                    values.Add(kvp.Value);
                }
            }

            info.AddValue("keys", properties.ToArray());
            info.AddValue("values", values.ToArray());

            info.SetType(typeof(DependencyStoreRef));
        }
        object ISerializationSurrogate.SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
        {
            return null;
        }

        #region DependencyStoreRef
        [Serializable]
        private sealed class DependencyStoreRef : IObjectReference, IDeserializationCallback
        {
            private IList keys = null;
            private IList values = null;

            [NonSerialized]
            private IDictionary<DependencyProperty, object> store = null;

            Object IObjectReference.GetRealObject(StreamingContext context)
            {
                if (this.store == null)
                    this.store = new Dictionary<DependencyProperty, object>();

                return this.store;
            }
            void IDeserializationCallback.OnDeserialization(Object sender)
            {
                if (this.store != null)
                {
                    for (int index = 0; index < this.keys.Count; index++)
                    {
                        DependencyProperty dp = this.keys[index] as DependencyProperty;
                        if (dp == null)
                            dp = DependencyProperty.FromKnown((byte)this.keys[index]);
                        this.store.Add(dp, this.values[index]);
                    }
                }
                this.store = null;
            }
        }
        #endregion
    }
    #endregion
}
