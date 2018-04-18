//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Diagnostics;
    using System.Runtime;
    using System.Runtime.Diagnostics;
    using System.Security;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;
    using System.Xml;

    public abstract class ServiceModelExtensionCollectionElement<TServiceModelExtensionElement> : ConfigurationElement, ICollection<TServiceModelExtensionElement>, IConfigurationContextProviderInternal
        where TServiceModelExtensionElement : ServiceModelExtensionElement
    {
        [Fx.Tag.SecurityNote(Critical = "Stores information used in a security decision.")]
        [SecurityCritical]
        EvaluationContextHelper contextHelper;

        string extensionCollectionName = null;
        bool modified = false;
        List<TServiceModelExtensionElement> items = null;
        ConfigurationPropertyCollection properties = null;

        internal ServiceModelExtensionCollectionElement(string extensionCollectionName)
        {
            this.extensionCollectionName = extensionCollectionName;
        }

        public TServiceModelExtensionElement this[int index]
        {
            get { return this.Items[index]; }
        }

        public TServiceModelExtensionElement this[Type extensionType]
        {
            get
            {
                if (extensionType == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("extensionType");
                }

                if (!this.CollectionElementBaseType.IsAssignableFrom(extensionType))
                {
#pragma warning disable 56506 //Microsoft; Variable 'extensionType' checked for null previously
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("extensionType",
                        SR.GetString(SR.ConfigInvalidExtensionType,
                        extensionType.ToString(),
                        this.CollectionElementBaseType.FullName,
                        this.extensionCollectionName));
#pragma warning restore
                }
                TServiceModelExtensionElement retval = null;

                foreach (TServiceModelExtensionElement collectionElement in this)
                {
                    if (null != collectionElement)
                    {
                        if (collectionElement.GetType() == extensionType)
                        {
                            retval = collectionElement;
                        }
                    }
                }

                return retval;
            }
        }

        public int Count
        {
            get { return this.Items.Count; }
        }

        bool ICollection<TServiceModelExtensionElement>.IsReadOnly
        {
            get { return this.IsReadOnly(); }
        }

        internal List<TServiceModelExtensionElement> Items
        {
            get
            {
                if (this.items == null)
                {
                    this.items = new List<TServiceModelExtensionElement>();
                }
                return this.items;
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    this.properties = new ConfigurationPropertyCollection();
                }
                return this.properties;
            }
        }

        public virtual void Add(TServiceModelExtensionElement element)
        {
            if (this.IsReadOnly())
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(SR.GetString(SR.ConfigReadOnly)));
            }
            if (element == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("element");
            }

            element.ExtensionCollectionName = this.extensionCollectionName;

            if (this.Contains(element))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("element", SR.GetString(SR.ConfigDuplicateKey, element.ConfigurationElementName));
            }
            else if (!this.CanAdd(element))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("element",
                    SR.GetString(SR.ConfigElementTypeNotAllowed,
                    element.ConfigurationElementName,
                    this.extensionCollectionName));
            }
            else
            {
                element.ContainingEvaluationContext = ConfigurationHelpers.GetEvaluationContext(this);
                ConfigurationProperty configProperty = new ConfigurationProperty(element.ConfigurationElementName, element.GetType(), null);
                this.Properties.Add(configProperty);
                this[configProperty] = element;
                this.Items.Add(element);
                this.modified = true;
            }
        }

        internal void AddItem(TServiceModelExtensionElement element)
        {
            if (this.IsReadOnly())
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(SR.GetString(SR.ConfigReadOnly)));
            }
            element.ExtensionCollectionName = this.extensionCollectionName;
            element.ContainingEvaluationContext = ConfigurationHelpers.GetEvaluationContext(this);
            this.Items.Add(element);
            this.modified = true;
        }

        public virtual bool CanAdd(TServiceModelExtensionElement element)
        {
            if (null == element)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("element");
            }

            bool retval = false;
            Type elementType = element.GetType();

            if (!this.IsReadOnly())
            {
                if (!this.ContainsKey(elementType))
                {
                    retval = element.CanAdd(this.extensionCollectionName, ConfigurationHelpers.GetEvaluationContext(this));
                }
                else if (DiagnosticUtility.ShouldTraceWarning)
                {
                    TraceUtility.TraceEvent(TraceEventType.Warning,
                        TraceCode.ExtensionElementAlreadyExistsInCollection,
                        SR.GetString(SR.TraceCodeExtensionElementAlreadyExistsInCollection),
                        this.CreateCanAddRecord(this[elementType]), this, null);
                }
            }
            else if (DiagnosticUtility.ShouldTraceWarning)
            {
                TraceUtility.TraceEvent(TraceEventType.Warning,
                    TraceCode.ConfigurationIsReadOnly,
                    SR.GetString(SR.TraceCodeConfigurationIsReadOnly),
                    null, this, null);
            }
            return retval;
        }

        DictionaryTraceRecord CreateCanAddRecord(TServiceModelExtensionElement element)
        {
            return this.CreateCanAddRecord(element, new Dictionary<string, string>(3));
        }

        DictionaryTraceRecord CreateCanAddRecord(TServiceModelExtensionElement element, Dictionary<string, string> values)
        {
            values["ElementType"] = System.Runtime.Diagnostics.DiagnosticTraceBase.XmlEncode(typeof(TServiceModelExtensionElement).AssemblyQualifiedName);
            values["ConfiguredSectionName"] = element.ConfigurationElementName;
            values["CollectionName"] = ConfigurationStrings.ExtensionsSectionPath + "/" + this.extensionCollectionName;
            return new DictionaryTraceRecord(values);
        }

        public void Clear()
        {
            if (this.IsReadOnly())
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(SR.GetString(SR.ConfigReadOnly)));
            }
            if (this.Properties.Count > 0)
            {
                this.modified = true;
            }

            List<string> propertiesToRemove = new List<string>(this.Items.Count);
            foreach (TServiceModelExtensionElement item in this.Items)
            {
                propertiesToRemove.Add(item.ConfigurationElementName);
            }

            this.Items.Clear();

            foreach (string name in propertiesToRemove)
            {
                this.Properties.Remove(name);
            }
        }

        internal Type CollectionElementBaseType
        {
            get { return typeof(TServiceModelExtensionElement); }
        }

        public bool Contains(TServiceModelExtensionElement element)
        {
            if (element == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("element");
            }
            return this.ContainsKey(element.GetType());
        }

        public bool ContainsKey(Type elementType)
        {
            if (elementType == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("elementType");
            }
            return (this[elementType] != null);
        }

        public bool ContainsKey(string elementName)
        {
            if (string.IsNullOrEmpty(elementName))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("elementName");
            }
            bool retval = false;
            foreach (TServiceModelExtensionElement element in this)
            {
                if (null != element)
                {
                    string configuredSectionName = element.ConfigurationElementName;
                    if (configuredSectionName.Equals(elementName, StringComparison.Ordinal))
                    {
                        retval = true;
                        break;
                    }
                }
            }
            return retval;
        }

        public void CopyTo(TServiceModelExtensionElement[] elements, int start)
        {
            if (elements == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("elements");
            }
            if (start < 0 || start >= elements.Length)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("start",
                    SR.GetString(SR.ConfigInvalidStartValue,
                    elements.Length - 1,
                    start));
            }

            foreach (TServiceModelExtensionElement element in this)
            {
                if (null != element)
                {
                    string configuredSectionName = element.ConfigurationElementName;

                    TServiceModelExtensionElement copiedElement = this.CreateNewSection(configuredSectionName);
                    if ((copiedElement != null) && (start < elements.Length))
                    {
                        copiedElement.CopyFrom(element);
                        elements[start] = copiedElement;
                        ++start;
                    }
                }
            }
        }

        /// <summary>
        /// Returns the extension element, or null if the type cannot be loaded in certain situations (see the code for details).
        /// </summary>
        TServiceModelExtensionElement CreateNewSection(string name)
        {
            if (this.ContainsKey(name) && !(name == ConfigurationStrings.Clear || name == ConfigurationStrings.Remove))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(SR.GetString(SR.ConfigDuplicateItem,
                    name,
                    this.GetType().Name),
                    this.ElementInformation.Source,
                    this.ElementInformation.LineNumber));
            }

            TServiceModelExtensionElement retval = null;

            Type elementType;
            ContextInformation evaluationContext = ConfigurationHelpers.GetEvaluationContext(this);
            try
            {
                elementType = GetExtensionType(evaluationContext, name);
            }
            catch (ConfigurationErrorsException e)
            {
                // Work-around for bug 219506@CSDMain: if the extension type cannot be loaded, we'll ignore 
                // the exception when running in win8 app container and reading from machine.config.
                if (System.ServiceModel.Channels.AppContainerInfo.IsRunningInAppContainer && evaluationContext.IsMachineLevel)
                {
                    DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                    return null;
                }
                else
                {
                    throw;
                }
            }

            if (null != elementType)
            {
                if (this.CollectionElementBaseType.IsAssignableFrom(elementType))
                {
                    retval = (TServiceModelExtensionElement)Activator.CreateInstance(elementType);
                    retval.ExtensionCollectionName = this.extensionCollectionName;
                    retval.ConfigurationElementName = name;
                    retval.InternalInitializeDefault();
                }
                else
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(SR.GetString(SR.ConfigInvalidExtensionElement,
                        name,
                        this.CollectionElementBaseType.FullName),
                        this.ElementInformation.Source,
                        this.ElementInformation.LineNumber));
                }
            }
            else
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(SR.GetString(SR.ConfigInvalidExtensionElementName,
                    name,
                    this.extensionCollectionName),
                    this.ElementInformation.Source,
                    this.ElementInformation.LineNumber));
            }

            return retval;
        }

        [Fx.Tag.SecurityNote(Critical = "Calls SecurityCritical method UnsafeLookupCollection which elevates in order to load config.",
            Safe = "Does not leak any config objects.")]
        [SecuritySafeCritical]
        Type GetExtensionType(ContextInformation evaluationContext, string name)
        {
            ExtensionElementCollection collection = ExtensionsSection.UnsafeLookupCollection(this.extensionCollectionName, evaluationContext);
            if (collection.ContainsKey(name))
            {
                ExtensionElement element = collection[name];
                Type elementType = Type.GetType(element.Type, false);
                if (null == elementType)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(SR.GetString(SR.ConfigInvalidType, element.Type, element.Name),
                        this.ElementInformation.Source,
                        this.ElementInformation.LineNumber));
                }
                return elementType;
            }
            return null;
        }

        internal void MergeWith(List<TServiceModelExtensionElement> parentExtensionElements)
        {
            ServiceModelExtensionCollectionElement<TServiceModelExtensionElement>.Merge(parentExtensionElements, this);
            this.Clear();
            foreach (TServiceModelExtensionElement parentExtensionElement in parentExtensionElements)
            {
                this.Add(parentExtensionElement);
            }
        }

        static void Merge(List<TServiceModelExtensionElement> parentExtensionElements, IEnumerable<TServiceModelExtensionElement> childExtensionElements)
        {
            foreach (TServiceModelExtensionElement childExtensionElement in childExtensionElements)
            {
                if (childExtensionElement is ClearBehaviorElement)
                {
                    parentExtensionElements.Clear();
                }
                else if (childExtensionElement is RemoveBehaviorElement)
                {
                    string childExtensionElementName = (childExtensionElement as RemoveBehaviorElement).Name;
                    if (!string.IsNullOrEmpty(childExtensionElementName))
                    {
                        parentExtensionElements.RemoveAll(element => element != null && element.ConfigurationElementName == childExtensionElementName);
                    }
                }
                else
                {
                    Type childExtensionElementType = childExtensionElement.GetType();
                    parentExtensionElements.RemoveAll(element => element != null && element.GetType() == childExtensionElementType);
                    parentExtensionElements.Add(childExtensionElement);
                }
            }
        }

        [Fx.Tag.SecurityNote(Critical = "Uses the critical helper SetIsPresent.",
            Safe = "Controls how/when SetIsPresent is used, not arbitrarily callable from PT (method is protected and class is sealed).")]
        [SecuritySafeCritical]
        protected override void DeserializeElement(XmlReader reader, bool serializeCollectionKey)
        {
            SetIsPresent();
            DeserializeElementCore(reader);
        }

        private void DeserializeElementCore(XmlReader reader)
        {
            if (reader.HasAttributes && 0 < reader.AttributeCount)
            {
                while (reader.MoveToNextAttribute())
                {
                    if (this.Properties.Contains(reader.Name))
                    {
                        this[reader.Name] = this.Properties[reader.Name].Converter.ConvertFromString(reader.Value);
                    }
                    else
                    {
                        this.OnDeserializeUnrecognizedAttribute(reader.Name, reader.Value);
                    }
                }
            }

            if (XmlNodeType.Element != reader.NodeType)
            {
                reader.MoveToElement();
            }

            XmlReader subTree = reader.ReadSubtree();
            if (subTree.Read())
            {
                while (subTree.Read())
                {
                    if (XmlNodeType.Element == subTree.NodeType)
                    {
                        // Create new child element and add it to the property collection to
                        // associate the element with an EvaluationContext.  Then deserialize
                        // XML further to set actual values.
                        TServiceModelExtensionElement collectionElement = this.CreateNewSection(subTree.Name);
                        if (collectionElement != null)
                        {
                            this.Add(collectionElement);
                            collectionElement.DeserializeInternal(subTree, false);
                        }
                    }
                }
            }
        }

        [Fx.Tag.SecurityNote(Critical = "Calls ConfigurationHelpers.SetIsPresent which elevates in order to set a property.")]
        [SecurityCritical]
        void SetIsPresent()
        {
            ConfigurationHelpers.SetIsPresent(this);
        }

        public System.Collections.Generic.IEnumerator<TServiceModelExtensionElement> GetEnumerator()
        {
            for (int index = 0; index < this.Items.Count; ++index)
            {
                TServiceModelExtensionElement currentValue = items[index];
                yield return currentValue;
            }
        }

        protected override bool IsModified()
        {
            bool retval = this.modified;
            if (!retval)
            {
                for (int i = 0; i < this.Items.Count; i++)
                {
                    TServiceModelExtensionElement element = this.Items[i];
                    if (element.IsModifiedInternal())
                    {
                        retval = true;
                        break;
                    }
                }
            }
            return retval;
        }

        protected override bool OnDeserializeUnrecognizedElement(string elementName, XmlReader reader)
        {
            // When this is used as a DefaultCollection (i.e. CommonBehaviors)
            // the element names are unrecognized by the parent tag, which delegates
            // to the collection's OnDeserializeUnrecognizedElement.  In this case,
            // an unrecognized element may be expected, simply try to deserialize the
            // element and let DeserializeElement() throw the appropriate exception if
            // an error is hit.
            this.DeserializeElement(reader, false);
            return true;
        }

        public bool Remove(TServiceModelExtensionElement element)
        {
            if (element == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("element");
            }
            bool retval = false;
            if (this.Contains(element))
            {
                string configuredSectionName = element.ConfigurationElementName;

                TServiceModelExtensionElement existingElement = (TServiceModelExtensionElement)this[element.GetType()];
                this.Items.Remove(existingElement);
                this.Properties.Remove(configuredSectionName);
                this.modified = true;
                retval = true;
            }
            return retval;
        }

        [Fx.Tag.SecurityNote(Critical = "Accesses critical field contextHelper.")]
        [SecurityCritical]
        protected override void Reset(ConfigurationElement parentElement)
        {
            ServiceModelExtensionCollectionElement<TServiceModelExtensionElement> collection =
                (ServiceModelExtensionCollectionElement<TServiceModelExtensionElement>)parentElement;
            foreach (TServiceModelExtensionElement collectionElement in collection.Items)
            {
                this.Items.Add(collectionElement);
            }
            // Update my properties
            this.UpdateProperties(collection);

            this.contextHelper.OnReset(parentElement);

            base.Reset(parentElement);
        }

        protected override void ResetModified()
        {
            for (int i = 0; i < this.Items.Count; i++)
            {
                TServiceModelExtensionElement collectionElement = this.Items[i];
                collectionElement.ResetModifiedInternal();
            }
            this.modified = false;
        }

        protected void SetIsModified()
        {
            this.modified = true;
        }

        protected override void SetReadOnly()
        {
            base.SetReadOnly();

            for (int i = 0; i < this.Items.Count; i++)
            {
                TServiceModelExtensionElement element = this.Items[i];
                element.SetReadOnlyInternal();
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        protected override void Unmerge(ConfigurationElement sourceElement, ConfigurationElement parentElement, ConfigurationSaveMode saveMode)
        {
            if (sourceElement == null)
            {
                return;
            }

            ServiceModelExtensionCollectionElement<TServiceModelExtensionElement> sourceCollectionElement = (ServiceModelExtensionCollectionElement<TServiceModelExtensionElement>)sourceElement;

            this.UpdateProperties(sourceCollectionElement);
            base.Unmerge(sourceElement, parentElement, saveMode);
        }

        void UpdateProperties(ServiceModelExtensionCollectionElement<TServiceModelExtensionElement> sourceElement)
        {
            foreach (ConfigurationProperty property in sourceElement.Properties)
            {
                if (!this.Properties.Contains(property.Name))
                {
                    this.Properties.Add(property);
                }
            }
            foreach (TServiceModelExtensionElement extension in this.Items)
            {
                if (extension is ClearBehaviorElement || extension is RemoveBehaviorElement)
                    continue;

                string configuredSectionName = extension.ConfigurationElementName;
                if (!this.Properties.Contains(configuredSectionName))
                {
                    ConfigurationProperty configProperty = new ConfigurationProperty(configuredSectionName, extension.GetType(), null);
                    this.Properties.Add(configProperty);
                }
            }
        }

        ContextInformation IConfigurationContextProviderInternal.GetEvaluationContext()
        {
            return this.EvaluationContext;
        }

        [Fx.Tag.SecurityNote(Critical = "Accesses critical field contextHelper.",
            Miscellaneous = "RequiresReview -- the return value will be used for a security decision -- see comment in interface definition.")]
        [SecurityCritical]
        ContextInformation IConfigurationContextProviderInternal.GetOriginalEvaluationContext()
        {
            return this.contextHelper.GetOriginalContext(this);
        }
    }
}

