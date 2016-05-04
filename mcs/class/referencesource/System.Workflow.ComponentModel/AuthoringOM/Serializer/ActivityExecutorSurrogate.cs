namespace System.Workflow.ComponentModel.Serialization
{
    using System;
    using System.IO;
    using System.Reflection;
    using System.Collections;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Binary;

    internal sealed class ActivityExecutorSurrogate : ISerializationSurrogate
    {
        public ActivityExecutorSurrogate()
        {
        }
        public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
        {
            info.AddValue("executorType", obj.GetType());
            info.SetType(typeof(ActivityExecutorRef));
        }
        public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
        {
            return null;
        }

        [Serializable]
        private sealed class ActivityExecutorRef : IObjectReference
        {
            private Type executorType = null;
            Object IObjectReference.GetRealObject(StreamingContext context)
            {
                return ActivityExecutors.GetActivityExecutorFromType(this.executorType);
            }
        }
    }
}
