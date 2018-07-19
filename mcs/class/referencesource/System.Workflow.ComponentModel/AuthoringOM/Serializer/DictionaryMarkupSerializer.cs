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

    #region Class DictionaryMarkupSerializer
    internal class DictionaryMarkupSerializer : WorkflowMarkupSerializer
    {
        private bool deserializingDictionary = false;
        private IDictionary keylookupDictionary;

        protected internal override IList GetChildren(WorkflowMarkupSerializationManager serializationManager, object obj)
        {
            IDictionary dictionary = obj as IDictionary;
            if (dictionary == null)
                throw new InvalidOperationException(SR.GetString(SR.Error_DictionarySerializerNonDictionaryObject));

            List<object> childEntries = new List<object>();
            foreach (DictionaryEntry dictionaryEntry in dictionary)
            {
                childEntries.Add(dictionaryEntry);
            }
            return childEntries;
        }

        protected internal override PropertyInfo[] GetProperties(WorkflowMarkupSerializationManager serializationManager, object obj)
        {
            return new PropertyInfo[] { };
        }

        protected internal override bool ShouldSerializeValue(WorkflowMarkupSerializationManager serializationManager, object value)
        {
            if (value == null)
                return false;

            if (!(value is IDictionary))
                throw new InvalidOperationException(SR.GetString(SR.Error_DictionarySerializerNonDictionaryObject));

            return (((IDictionary)value).Count > 0);
        }

        protected internal override void ClearChildren(WorkflowMarkupSerializationManager serializationManager, object deserializedObject)
        {
            if (deserializedObject == null)
                throw new ArgumentNullException("deserializedObject");

            IDictionary dictionary = deserializedObject as IDictionary;
            if (dictionary == null)
                throw new InvalidOperationException(SR.GetString(SR.Error_DictionarySerializerNonDictionaryObject));

            dictionary.Clear();
        }

        protected internal override void AddChild(WorkflowMarkupSerializationManager serializationManager, object parentObj, object childObj)
        {
            if (parentObj == null)
                throw new ArgumentNullException("parentObj");

            if (childObj == null)
                throw new ArgumentNullException("childObj");

            IDictionary dictionary = parentObj as IDictionary;
            if (dictionary == null)
                throw new InvalidOperationException(SR.GetString(SR.Error_DictionarySerializerNonDictionaryObject));

            object key = null;
            foreach (DictionaryEntry entry in keylookupDictionary)
            {
                if ((!entry.Value.GetType().IsValueType && entry.Value == childObj) ||
                    (entry.Value.GetType().IsValueType && entry.Value.Equals(childObj)))
                {
                    key = entry.Key;
                    break;
                }
            }

            if (key == null)
                throw new InvalidOperationException(SR.GetString(SR.Error_DictionarySerializerKeyNotFound, childObj.GetType().FullName));

            dictionary.Add(key, childObj);
            keylookupDictionary.Remove(key);
        }

        internal override void OnBeforeSerializeContents(WorkflowMarkupSerializationManager serializationManager, object obj)
        {
            base.OnBeforeSerializeContents(serializationManager, obj);

            serializationManager.ExtendedPropertiesProviders.Add(this);
            this.keylookupDictionary = new Hashtable();
        }

        protected override void OnAfterSerialize(WorkflowMarkupSerializationManager serializationManager, object obj)
        {
            base.OnAfterSerialize(serializationManager, obj);

            serializationManager.ExtendedPropertiesProviders.Remove(this);
            this.keylookupDictionary = null;
        }

        internal override void OnBeforeDeserializeContents(WorkflowMarkupSerializationManager serializationManager, object obj)
        {
            base.OnBeforeDeserializeContents(serializationManager, obj);

            serializationManager.ExtendedPropertiesProviders.Add(this);
            this.keylookupDictionary = new Hashtable();
            this.deserializingDictionary = true;
        }

        protected override void OnAfterDeserialize(WorkflowMarkupSerializationManager serializationManager, object obj)
        {
            base.OnAfterDeserialize(serializationManager, obj);

            serializationManager.ExtendedPropertiesProviders.Remove(this);
            this.keylookupDictionary = null;
            this.deserializingDictionary = false;
        }

        internal override ExtendedPropertyInfo[] GetExtendedProperties(WorkflowMarkupSerializationManager manager, object extendee)
        {
            List<ExtendedPropertyInfo> extendedProperties = new List<ExtendedPropertyInfo>();
            DictionaryEntry? entry = null;
            if (manager.WorkflowMarkupStack[typeof(DictionaryEntry)] != null)
                entry = (DictionaryEntry)manager.WorkflowMarkupStack[typeof(DictionaryEntry)];
            if (this.deserializingDictionary || (entry.HasValue && entry.Value.Value == extendee))
            {
                ExtendedPropertyInfo extendedProperty =
                    new ExtendedPropertyInfo(typeof(DictionaryEntry).GetProperty("Key", BindingFlags.Public | BindingFlags.Instance),
                    new GetValueHandler(OnGetKeyValue),
                    new SetValueHandler(OnSetKeyValue),
                    new GetQualifiedNameHandler(OnGetXmlQualifiedName), manager);

                extendedProperties.Add(extendedProperty);
            }
            return extendedProperties.ToArray();
        }

        private object OnGetKeyValue(ExtendedPropertyInfo extendedProperty, object extendee)
        {
            DictionaryEntry? entry = null;
            if (extendedProperty.SerializationManager.WorkflowMarkupStack[typeof(DictionaryEntry)] != null)
                entry = (DictionaryEntry)extendedProperty.SerializationManager.WorkflowMarkupStack[typeof(DictionaryEntry)];
            else
                Debug.Assert(false, "Dictionary Entry not found in the WorkflowMarkupStack");

            if (entry.HasValue && entry.Value.Value == extendee)
                return entry.Value.Key;
            return null;
        }

        private void OnSetKeyValue(ExtendedPropertyInfo extendedProperty, object extendee, object value)
        {
            if (extendee != null && value != null && !this.keylookupDictionary.Contains(value))
                this.keylookupDictionary.Add(value, extendee);
        }

        private XmlQualifiedName OnGetXmlQualifiedName(ExtendedPropertyInfo extendedProperty, WorkflowMarkupSerializationManager manager, out string prefix)
        {
            prefix = StandardXomlKeys.Definitions_XmlNs_Prefix;
            return new XmlQualifiedName(extendedProperty.Name, StandardXomlKeys.Definitions_XmlNs);
        }
    }
    #endregion

}

