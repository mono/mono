namespace System.Workflow.ComponentModel.Serialization
{
    using System;
    using System.Reflection;
    using System.Xml;
    using System.ComponentModel;
    using System.ComponentModel.Design.Serialization;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Workflow.ComponentModel.Compiler;

    internal sealed class StringCollectionMarkupSerializer : WorkflowMarkupSerializer
    {
        protected internal override PropertyInfo[] GetProperties(WorkflowMarkupSerializationManager manager, object obj)
        {
            return new PropertyInfo[] { };
        }

        protected internal override bool CanSerializeToString(WorkflowMarkupSerializationManager serializationManager, object value)
        {
            if (serializationManager == null)
                throw new ArgumentNullException("serializationManager");
            if (value == null)
                throw new ArgumentNullException("value");

            return (value is ICollection<String>);
        }

        protected internal override string SerializeToString(WorkflowMarkupSerializationManager serializationManager, object value)
        {
            if (serializationManager == null)
                throw new ArgumentNullException("serializationManager");
            if (value == null)
                throw new ArgumentNullException("value");

            return SynchronizationHandlesTypeConverter.Stringify(value as ICollection<String>);
        }

        protected internal override object DeserializeFromString(WorkflowMarkupSerializationManager serializationManager, Type propertyType, string value)
        {
            if (serializationManager == null)
                throw new ArgumentNullException("serializationManager");
            if (propertyType == null)
                throw new ArgumentNullException("propertyType");
            if (value == null)
                throw new ArgumentNullException("value");

            // Work around For Bind based properties whose base type is an 
            // ICollection<string> or its derivative, special case! (A synchronization
            // handle cannot begin with a * because it won't be a language independent
            // identifier :) )
            if (IsValidCompactAttributeFormat(value))
                return DeserializeFromCompactFormat(serializationManager, serializationManager.WorkflowMarkupStack[typeof(XmlReader)] as XmlReader, value);
            else
                return SynchronizationHandlesTypeConverter.UnStringify(value);
        }
    }
}
