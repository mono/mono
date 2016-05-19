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

    #region DictionarySurrogate
    internal sealed class DictionarySurrogate : ISerializationSurrogate
    {
        void ISerializationSurrogate.GetObjectData(object obj, SerializationInfo info, StreamingContext context)
        {
            if (!obj.GetType().IsGenericType || obj.GetType().GetGenericTypeDefinition() != typeof(Dictionary<,>))
                throw new ArgumentException(SR.GetString(SR.Error_InvalidArgumentValue), "obj");

            Type[] args = obj.GetType().GetGenericArguments();
            if (args.Length != 2)
                throw new ArgumentException(SR.GetString(SR.Error_InvalidArgumentValue), "obj");

            PropertyInfo keysProperty = obj.GetType().GetProperty("Keys");
            if (keysProperty == null)
                throw new NullReferenceException("keysProperty");

            ArrayList keys = new ArrayList(keysProperty.GetValue(obj, null) as ICollection);

            PropertyInfo valuesProperty = obj.GetType().GetProperty("Values");
            if (valuesProperty == null)
                throw new NullReferenceException("valuesProperty");

            ArrayList values = new ArrayList(valuesProperty.GetValue(obj, null) as ICollection);
            if (keys.Count == 1)
            {
                info.AddValue("key", keys[0]);
                info.AddValue("value", values[0]);
            }
            else if (keys.Count > 1)
            {
                info.AddValue("keys", keys.ToArray());
                info.AddValue("values", values.ToArray());
            }
            info.AddValue("keyType", args[0]);
            info.AddValue("valueType", args[1]);

            info.SetType(typeof(DictionaryRef));
        }
        object ISerializationSurrogate.SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
        {
            return null;
        }

        #region DictionaryRef
        [Serializable]
        private sealed class DictionaryRef : IObjectReference, IDeserializationCallback
        {
            [OptionalField]
            private IList keys = null;
            [OptionalField]
            private IList values = null;
            [OptionalField]
            private object key = null;
            [OptionalField]
            private object value = null;

            private Type keyType = null;
            private Type valueType = null;

            [NonSerialized]
            private object dictionary = null;

            Object IObjectReference.GetRealObject(StreamingContext context)
            {
                if (this.dictionary == null)
                {
                    Type dictionaryType = typeof(Dictionary<int, int>).GetGenericTypeDefinition().MakeGenericType(keyType, valueType);
                    this.dictionary = dictionaryType.GetConstructor(Type.EmptyTypes).Invoke(null);
                }
                return this.dictionary;
            }
            void IDeserializationCallback.OnDeserialization(Object sender)
            {
                if (this.dictionary != null)
                {
                    MethodInfo addMethod = this.dictionary.GetType().GetMethod("Add");
                    if (addMethod == null)
                        throw new NullReferenceException("addMethod");

                    object[] kvp = new object[2];
                    if (this.keys != null)
                    {
                        for (int index = 0; index < this.keys.Count; index++)
                        {
                            kvp[0] = this.keys[index];
                            kvp[1] = this.values[index];
                            addMethod.Invoke(this.dictionary, kvp);
                        }
                    }
                    else if (this.key != null)
                    {
                        kvp[0] = this.key;
                        kvp[1] = this.value;
                        addMethod.Invoke(this.dictionary, kvp);
                    }
                }
                this.dictionary = null;
            }
        }
        #endregion
    }
    #endregion

}
