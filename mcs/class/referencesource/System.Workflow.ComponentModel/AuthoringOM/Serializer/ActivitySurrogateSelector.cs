namespace System.Workflow.ComponentModel.Serialization
{
    using System;
    using System.IO;
    using System.Reflection;
    using System.Collections;
    using System.Collections.Generic;
    using System.Xml;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Security.Permissions;

    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public sealed class ActivitySurrogateSelector : SurrogateSelector
    {
        private ActivitySurrogate activitySurrogate = new ActivitySurrogate();
        private ActivityExecutorSurrogate activityExecutorSurrogate = new ActivityExecutorSurrogate();
        private ObjectSurrogate objectSurrogate = new ObjectSurrogate();
        private DependencyStoreSurrogate dependencyStoreSurrogate = new DependencyStoreSurrogate();
        private XmlDocumentSurrogate domDocSurrogate = new XmlDocumentSurrogate();
        private DictionarySurrogate dictionarySurrogate = new DictionarySurrogate();
        private GenericQueueSurrogate genericqueueSurrogate = new GenericQueueSurrogate();
        private QueueSurrogate queueSurrogate = new QueueSurrogate();
        private ListSurrogate listSurrogate = new ListSurrogate();
        private SimpleTypesSurrogate simpleTypesSurrogate = new SimpleTypesSurrogate();

        private static ActivitySurrogateSelector defaultActivitySurrogateSelector = new ActivitySurrogateSelector();
        private static Dictionary<Type, ISerializationSurrogate> surrogateCache = new Dictionary<Type, ISerializationSurrogate>();
        public static ActivitySurrogateSelector Default
        {
            get
            {
                return defaultActivitySurrogateSelector;
            }
        }

        public override ISerializationSurrogate GetSurrogate(Type type, StreamingContext context, out ISurrogateSelector selector)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            selector = this;
            ISerializationSurrogate result = null;
            bool found;

            lock (surrogateCache)
            {
                found = surrogateCache.TryGetValue(type, out result);
            }
            if (found)
            {
                return result == null ? base.GetSurrogate(type, context, out selector) : result;
            }

            // if type is assignable to activity, then return the surrogate
            if (typeof(Activity).IsAssignableFrom(type))
            {
                result = this.activitySurrogate;
            }
            else if (typeof(ActivityExecutor).IsAssignableFrom(type))
            {
                result = this.activityExecutorSurrogate;
            }
            else if (typeof(IDictionary<DependencyProperty, object>).IsAssignableFrom(type))
            {
                result = this.dependencyStoreSurrogate;
            }
            else if (typeof(XmlDocument).IsAssignableFrom(type))
            {
                result = this.domDocSurrogate;
            }
            /*else if (type.IsGenericType)
            {
                Type genericType = type.GetGenericTypeDefinition();
                if (genericType == typeof(Dictionary<,>))
                {
                    result = this.dictionarySurrogate;
                }
                else if (genericType == typeof(Queue<>))
                {
                    result = this.genericqueueSurrogate;
                }
                else if (genericType == typeof(List<>))
                {
                    result = this.listSurrogate;
                }
            }*/
            else if (typeof(Queue) == type)
            {
                result = this.queueSurrogate;
            }
            else if (typeof(Guid) == type)
            {
                result = this.simpleTypesSurrogate;
            }

            // 
            else if (typeof(ActivityBind).IsAssignableFrom(type))
            {
                result = this.objectSurrogate;
            }

            // 
            else if (typeof(DependencyObject).IsAssignableFrom(type))
            {
                result = this.objectSurrogate;
            }

            lock (surrogateCache)
            {
                surrogateCache[type] = result;
            }

            return result == null ? base.GetSurrogate(type, context, out selector) : result;
        }

        private sealed class ObjectSurrogate : ISerializationSurrogate
        {
            public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
            {
                info.AddValue("type", obj.GetType());
                string[] names = null;
                MemberInfo[] members = FormatterServicesNoSerializableCheck.GetSerializableMembers(obj.GetType(), out names);
                object[] memberDatas = FormatterServices.GetObjectData(obj, members);
                info.AddValue("memberDatas", memberDatas);

                info.SetType(typeof(ObjectSerializedRef));
            }
            public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
            {
                return null;
            }

            [Serializable]
            private sealed class ObjectSerializedRef : IObjectReference, IDeserializationCallback
            {
                private Type type = null;
                private object[] memberDatas = null;

                [NonSerialized]
                private object returnedObject = null;

                Object IObjectReference.GetRealObject(StreamingContext context)
                {
                    if (this.returnedObject == null)
                        this.returnedObject = FormatterServices.GetUninitializedObject(this.type);
                    return this.returnedObject;
                }
                void IDeserializationCallback.OnDeserialization(object sender)
                {
                    if (this.returnedObject != null)
                    {
                        string[] names = null;
                        MemberInfo[] members = FormatterServicesNoSerializableCheck.GetSerializableMembers(this.type, out names);
                        FormatterServices.PopulateObjectMembers(this.returnedObject, members, this.memberDatas);
                        this.returnedObject = null;
                    }
                }
            }
        }
    }
}
