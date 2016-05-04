//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Configuration
{
    using System;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Configuration;
    using System.Diagnostics.CodeAnalysis;
    using System.Reflection;
    using System.Runtime;
    using System.ServiceModel.Description;
    using System.ServiceModel.Persistence;
    using System.Workflow.Runtime;
    using System.Xml;

    [Obsolete("The WF3 types are deprecated.  Instead, please use the new WF4 types from System.Activities.*")]
    public class PersistenceProviderElement : BehaviorExtensionElement
    {
        const string persistenceOperationTimeoutParameter = "persistenceOperationTimeout";
        const string typeParameter = "type";
        int argumentsHash;

        NameValueCollection persistenceProviderArguments;

        public PersistenceProviderElement()
        {
            this.persistenceProviderArguments = new NameValueCollection();
            this.argumentsHash = this.ComputeArgumentsHash();
        }

        // This property is not supposed to be exposed in config. 
        [SuppressMessage("Configuration", "Configuration102:ConfigurationPropertyAttributeRule")]
        public override Type BehaviorType
        {
            get { return typeof(PersistenceProviderBehavior); }
        }

        [ConfigurationProperty(
            persistenceOperationTimeoutParameter,
            IsRequired = false,
            DefaultValue = PersistenceProviderBehavior.DefaultPersistenceOperationTimeoutString)]
        [TypeConverter(typeof(TimeSpanOrInfiniteConverter))]
        [PositiveTimeSpanValidator]
        public TimeSpan PersistenceOperationTimeout
        {
            get { return (TimeSpan) base[persistenceOperationTimeoutParameter]; }
            set { base[persistenceOperationTimeoutParameter] = value; }
        }

        [SuppressMessage("Configuration", "Configuration102:ConfigurationPropertyAttributeRule")]
        public NameValueCollection PersistenceProviderArguments
        {
            get { return this.persistenceProviderArguments; }
        }

        [ConfigurationProperty(typeParameter, IsRequired = true)]
        [StringValidator(MinLength = 0)]
        public string Type
        {
            get { return (string) base[typeParameter]; }
            set { base[typeParameter] = value; }
        }

        protected internal override object CreateBehavior()
        {
            Fx.Assert(this.PersistenceOperationTimeout > TimeSpan.Zero,
                "This should have been guaranteed by the validator on the setter.");

            PersistenceProviderFactory providerFactory;

            Type providerType = System.Type.GetType((string) base[typeParameter]);

            if (providerType == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new InvalidOperationException(SR2.GetString(SR2.PersistenceProviderTypeNotFound)));
            }

            ConstructorInfo cInfo = providerType.GetConstructor(new Type[] { typeof(NameValueCollection) });

            if (cInfo != null)
            {
                providerFactory = (PersistenceProviderFactory) cInfo.Invoke(new object[] { this.persistenceProviderArguments });
            }
            else
            {
                cInfo = providerType.GetConstructor(new Type[] { });

                Fx.Assert(cInfo != null,
                    "The constructor should have been found - this should have been validated elsewhere.");

                providerFactory = (PersistenceProviderFactory) cInfo.Invoke(null);
            }

            return new PersistenceProviderBehavior(providerFactory, this.PersistenceOperationTimeout);
        }

        protected override bool IsModified()
        {
            return base.IsModified() || this.argumentsHash != this.ComputeArgumentsHash();
        }

        protected override bool OnDeserializeUnrecognizedAttribute(string name, string value)
        {
            persistenceProviderArguments.Add(name, value);

            return true;
        }

        protected override void PostDeserialize()
        {
            this.argumentsHash = this.ComputeArgumentsHash();
            base.PostDeserialize();
        }

        protected override bool SerializeElement(XmlWriter writer, bool serializeCollectionKey)
        {
            bool result;

            if (writer != null)
            {
                foreach (string key in this.persistenceProviderArguments.AllKeys)
                {
                    writer.WriteAttributeString(key, this.persistenceProviderArguments[key]);
                }

                result = base.SerializeElement(writer, serializeCollectionKey);
                result |= this.persistenceProviderArguments.Count > 0;
                this.argumentsHash = this.ComputeArgumentsHash();
            }
            else
            {
                result = base.SerializeElement(writer, serializeCollectionKey);
            }

            return result;
        }

        protected override void Unmerge(ConfigurationElement sourceElement, ConfigurationElement parentElement, ConfigurationSaveMode saveMode)
        {
            PersistenceProviderElement persistenceProviderElement = (PersistenceProviderElement) sourceElement;
            this.persistenceProviderArguments = new NameValueCollection(persistenceProviderElement.persistenceProviderArguments);
            this.argumentsHash = persistenceProviderElement.argumentsHash;
            base.Unmerge(sourceElement, parentElement, saveMode);
        }

        int ComputeArgumentsHash()
        {
            int result = 0;

            foreach (string key in this.persistenceProviderArguments.AllKeys)
            {
                result ^= key.GetHashCode() ^ this.persistenceProviderArguments[key].GetHashCode();
            }

            return result;
        }
    }
}
