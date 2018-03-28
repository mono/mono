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

    #region Class CollectionMarkupSerializer
    internal class CollectionMarkupSerializer : WorkflowMarkupSerializer
    {
        protected internal override IList GetChildren(WorkflowMarkupSerializationManager serializationManager, object obj)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");



            if (!IsValidCollectionType(obj.GetType()))
                throw new Exception(SR.GetString(SR.Error_SerializerTypeRequirement, obj.GetType().FullName, typeof(ICollection).FullName, typeof(ICollection<>).FullName));

            IEnumerable enumerable = obj as IEnumerable;
            ArrayList arrayList = new ArrayList();
            foreach (object containedObj in enumerable)
                arrayList.Add(containedObj);
            return arrayList;
        }

        protected internal override PropertyInfo[] GetProperties(WorkflowMarkupSerializationManager serializationManager, object obj)
        {
            return new PropertyInfo[] { };
        }

        protected internal override bool ShouldSerializeValue(WorkflowMarkupSerializationManager serializationManager, object value)
        {
            if (value == null)
                return false;

            if (!IsValidCollectionType(value.GetType()))
                throw new Exception(SR.GetString(SR.Error_SerializerTypeRequirement, value.GetType().FullName, typeof(ICollection).FullName, typeof(ICollection<>).FullName));

            IEnumerable enumerable = value as IEnumerable;
            foreach (object obj in enumerable)
                return true;

            return false;
        }

        protected internal override void ClearChildren(WorkflowMarkupSerializationManager serializationManager, object obj)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");

            if (!IsValidCollectionType(obj.GetType()))
                throw new Exception(SR.GetString(SR.Error_SerializerTypeRequirement, obj.GetType().FullName, typeof(ICollection).FullName, typeof(ICollection<>).FullName));

            ICollection collection = obj as ICollection;
            if (collection == null)
                obj.GetType().InvokeMember("Clear", BindingFlags.Public | BindingFlags.InvokeMethod | BindingFlags.Instance, null, obj, new object[] { }, CultureInfo.InvariantCulture);
        }

        protected internal override void AddChild(WorkflowMarkupSerializationManager serializationManager, object parentObj, object childObj)
        {
            if (parentObj == null)
                throw new ArgumentNullException("parentObj");

            if (!IsValidCollectionType(parentObj.GetType()))
                throw new Exception(SR.GetString(SR.Error_SerializerTypeRequirement, parentObj.GetType().FullName, typeof(ICollection).FullName, typeof(ICollection<>).FullName));

            parentObj.GetType().InvokeMember("Add", BindingFlags.Public | BindingFlags.InvokeMethod | BindingFlags.Instance, null, parentObj, new object[] { childObj }, CultureInfo.InvariantCulture);
        }

        internal static bool IsValidCollectionType(Type collectionType)
        {
            if (collectionType == null)
                return false;

            if (typeof(Array).IsAssignableFrom(collectionType))
                return false;

            return (typeof(ICollection).IsAssignableFrom(collectionType) ||
                    (collectionType.IsGenericType &&
                    (typeof(ICollection<>).IsAssignableFrom(collectionType.GetGenericTypeDefinition()) ||
                    typeof(IList<>).IsAssignableFrom(collectionType.GetGenericTypeDefinition()))));
        }
    }
    #endregion

}

