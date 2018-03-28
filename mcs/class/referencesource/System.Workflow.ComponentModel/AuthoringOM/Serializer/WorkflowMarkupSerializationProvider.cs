namespace System.Workflow.ComponentModel.Serialization
{
    using System;
    using System.IO;
    using System.CodeDom;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.ComponentModel.Design.Serialization;
    using System.Collections;
    using System.Xml;
    using System.Xml.Serialization;
    using System.Reflection;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Text;
    using System.Globalization;
    using System.Workflow.ComponentModel.Compiler;
    using System.Workflow.ComponentModel.Design;
    using System.Runtime.Serialization;
    using System.Security.Permissions;
    using System.Collections.ObjectModel;
    using System.Drawing;

    #region Class WorkflowMarkupSerializationProvider
    internal class WorkflowMarkupSerializationProvider : IDesignerSerializationProvider
    {
        public virtual object GetSerializer(IDesignerSerializationManager manager, object currentSerializer, Type objectType, Type serializerType)
        {
            // If this isn't a serializer type we recognize, do nothing.  Also, if metadata specified
            // a custom serializer, then use it.
            if (serializerType != typeof(WorkflowMarkupSerializer) || currentSerializer != null)
                return null;

            //DO NOT CHANGE THIS ORDER ELSE DICTIONARY WILL START GETTING SERIALIZED AS COLLECTION
            if (typeof(IDictionary).IsAssignableFrom(objectType))
                return new DictionaryMarkupSerializer();

            if (CollectionMarkupSerializer.IsValidCollectionType(objectType))
                return new CollectionMarkupSerializer();

            return new WorkflowMarkupSerializer();
        }
    }
    #endregion
}

