//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System.Collections.Generic;
    using System.Configuration;
    using System.Diagnostics;
    using System.Runtime;
    using System.Runtime.Diagnostics;
    using System.Security;
    using System.Security.Permissions;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;
    using System.Xml;

    [ConfigurationPermission(SecurityAction.InheritanceDemand, Unrestricted = true)]
    public abstract class ServiceModelExtensionElement : ServiceModelConfigurationElement, IConfigurationContextProviderInternal
    {
        [Fx.Tag.SecurityNote(Critical = "Stores information used in a security decision.")]
        [SecurityCritical]
        EvaluationContextHelper contextHelper;

        ContextInformation containingEvaluationContext = null;
        string configurationElementName = String.Empty;
        string extensionCollectionName = String.Empty;
        bool modified = false;
        Type thisType;

        protected ServiceModelExtensionElement()
            : base()
        {
        }

        [Fx.Tag.SecurityNote(Critical = "Calls SecurityCritical method UnsafeLookupCollection which elevates in order to load config.",
            Safe = "Does not leak any config objects.")]
        [SecuritySafeCritical]
        internal bool CanAdd(string extensionCollectionName, ContextInformation evaluationContext)
        {
            bool retVal = false;

            ExtensionElementCollection collection = ExtensionsSection.UnsafeLookupCollection(extensionCollectionName, evaluationContext);
            if (null != collection && collection.Count != 0)
            {
                string thisAssemblyQualifiedName = ThisType.AssemblyQualifiedName;
                string thisTypeName = ExtensionElement.GetTypeName(thisAssemblyQualifiedName);
                foreach (ExtensionElement extensionElement in collection)
                {
                    string extensionTypeName = extensionElement.Type;
                    if (extensionTypeName.Equals(thisAssemblyQualifiedName, StringComparison.Ordinal))
                    {
                        retVal = true;
                        break;
                    }

                    if (extensionElement.TypeName.Equals(thisTypeName, StringComparison.Ordinal))
                    {
                        Type extensionType = Type.GetType(extensionTypeName, false);
                        if (extensionType != null && extensionType.Equals(ThisType))
                        {
                            retVal = true;
                            break;
                        }
                    }
                }

                if (!retVal && DiagnosticUtility.ShouldTraceWarning)
                {
                    TraceUtility.TraceEvent(TraceEventType.Warning,
                        TraceCode.ConfiguredExtensionTypeNotFound,
                        SR.GetString(SR.TraceCodeConfiguredExtensionTypeNotFound),
                        this.CreateCanAddRecord(extensionCollectionName), this, null);
                }
            }
            else if (DiagnosticUtility.ShouldTraceWarning)
            {
                int traceCode;
                string traceDescription;
                if (collection != null && collection.Count == 0)
                {
                    traceCode = TraceCode.ExtensionCollectionIsEmpty;
                    traceDescription = SR.GetString(SR.TraceCodeExtensionCollectionIsEmpty);
                }
                else
                {
                    traceCode = TraceCode.ExtensionCollectionDoesNotExist;
                    traceDescription = SR.GetString(SR.TraceCodeExtensionCollectionDoesNotExist);
                }
                TraceUtility.TraceEvent(TraceEventType.Warning,
                    traceCode, traceDescription, this.CreateCanAddRecord(extensionCollectionName), this, null);
            }

            return retVal;
        }

        public string ConfigurationElementName
        {
            get
            {
                if (String.IsNullOrEmpty(this.configurationElementName))
                {
                    this.configurationElementName = this.GetConfigurationElementName();
                }

                return this.configurationElementName;
            }

            internal set
            {
                if (!string.IsNullOrEmpty(this.configurationElementName))
                {
                    Fx.Assert(this.configurationElementName == value,
                        string.Format(System.Globalization.CultureInfo.InvariantCulture,
                            "The configuration element name has already being set to '{0} and cannot be reset to '{1}'",
                            this.configurationElementName, value));
                    
                    return;
                }

                this.configurationElementName = value;
            }
        }

        internal ContextInformation ContainingEvaluationContext
        {
            get { return this.containingEvaluationContext; }
            set { this.containingEvaluationContext = value; }
        }

        Type ThisType
        {
            get
            {
                if (thisType == null)
                {
                    thisType = this.GetType();
                }
                return thisType;
            }
        }

        public virtual void CopyFrom(ServiceModelExtensionElement from)
        {
            if (this.IsReadOnly())
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(SR.GetString(SR.ConfigReadOnly)));
            }
            if (from == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("from");
            }
        }

        DictionaryTraceRecord CreateCanAddRecord(string extensionCollectionName)
        {
            Dictionary<string, string> values = new Dictionary<string, string>(2);
            values["ElementType"] = System.Runtime.Diagnostics.DiagnosticTraceBase.XmlEncode(ThisType.AssemblyQualifiedName);
            values["CollectionName"] = ConfigurationStrings.ExtensionsSectionPath + "/" + extensionCollectionName;
            return new DictionaryTraceRecord(values);
        }

        internal void DeserializeInternal(XmlReader reader, bool serializeCollectionKey)
        {
            this.DeserializeElement(reader, serializeCollectionKey);
        }

        internal string ExtensionCollectionName
        {
            set { this.extensionCollectionName = value; }
            get { return this.extensionCollectionName; }
        }

        internal ContextInformation EvalContext
        {
            get { return this.EvaluationContext; }
        }

        internal object FromProperty(ConfigurationProperty property)
        {
            return this[property];
        }

        [Fx.Tag.SecurityNote(Critical = "Calls SecurityCritical methods UnsafeLookupCollection and UnsafeLookupAssociatedCollection which elevate in order to load config.",
            Safe = "Does not leak any config objects.")]
        [SecuritySafeCritical]
        string GetConfigurationElementName()
        {
            string configurationElementName = String.Empty;
            ExtensionElementCollection collection = null;
            Type extensionSectionType = ThisType;

            ContextInformation evaluationContext = this.ContainingEvaluationContext;
            if (evaluationContext == null)
            {
                evaluationContext = ConfigurationHelpers.GetEvaluationContext(this);
            }

            if (String.IsNullOrEmpty(this.extensionCollectionName))
            {
                if (DiagnosticUtility.ShouldTraceWarning)
                {
                    TraceUtility.TraceEvent(TraceEventType.Warning,
                        TraceCode.ExtensionCollectionNameNotFound,
                        SR.GetString(SR.TraceCodeExtensionCollectionNameNotFound),
                        this,
                        (Exception)null);
                }

                collection = ExtensionsSection.UnsafeLookupAssociatedCollection(ThisType, evaluationContext, out this.extensionCollectionName);
            }
            else
            {
                collection = ExtensionsSection.UnsafeLookupCollection(this.extensionCollectionName, evaluationContext);
            }

            if (null == collection)
            {
                if (String.IsNullOrEmpty(this.extensionCollectionName))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(SR.GetString(SR.ConfigNoExtensionCollectionAssociatedWithType,
                        extensionSectionType.AssemblyQualifiedName),
                        this.ElementInformation.Source,
                        this.ElementInformation.LineNumber));
                }
                else
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(SR.GetString(SR.ConfigExtensionCollectionNotFound,
                        this.extensionCollectionName),
                        this.ElementInformation.Source,
                        this.ElementInformation.LineNumber));
                }
            }

            for (int i = 0; i < collection.Count; i++)
            {
                ExtensionElement collectionElement = collection[i];

                // Optimize for assembly qualified names.
                if (collectionElement.Type.Equals(extensionSectionType.AssemblyQualifiedName, StringComparison.Ordinal))
                {
                    configurationElementName = collectionElement.Name;
                    break;
                }

                // Check type directly for the case that the extension is registered with something less than
                // an full assembly qualified name.
                Type collectionElementType = Type.GetType(collectionElement.Type, false);
                if (null != collectionElementType && extensionSectionType.Equals(collectionElementType))
                {
                    configurationElementName = collectionElement.Name;
                    break;
                }
            }

            if (String.IsNullOrEmpty(configurationElementName))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(SR.GetString(SR.ConfigExtensionTypeNotRegisteredInCollection,
                    extensionSectionType.AssemblyQualifiedName,
                    this.extensionCollectionName),
                    this.ElementInformation.Source,
                    this.ElementInformation.LineNumber));
            }

            return configurationElementName;
        }

        internal void InternalInitializeDefault()
        {
            this.InitializeDefault();
        }

        protected override bool IsModified()
        {
            return this.modified | base.IsModified();
        }

        internal bool IsModifiedInternal()
        {
            return this.IsModified();
        }

        internal ConfigurationPropertyCollection PropertiesInternal
        {
            get { return this.Properties; }
        }

        internal void ResetModifiedInternal()
        {
            this.ResetModified();
        }

        protected override bool SerializeElement(XmlWriter writer, bool serializeCollectionKey)
        {
            base.SerializeElement(writer, serializeCollectionKey);
            return true;
        }

        internal bool SerializeInternal(XmlWriter writer, bool serializeCollectionKey)
        {
            return this.SerializeElement(writer, serializeCollectionKey);
        }

        internal void SetReadOnlyInternal()
        {
            this.SetReadOnly();
        }

        [Fx.Tag.SecurityNote(Critical = "Accesses critical field contextHelper.")]
        [SecurityCritical]
        protected override void Reset(ConfigurationElement parentElement)
        {
            this.contextHelper.OnReset(parentElement);

            base.Reset(parentElement);
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
