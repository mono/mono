//------------------------------------------------------------------------------
// <copyright file="TransformerInfoCollection.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Configuration {

    using System;
    using System.Configuration;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Security.Principal;
    using System.Web;
    using System.Web.Compilation;
    using System.Web.Configuration;
    using System.Web.UI;
    using System.Web.UI.WebControls;
    using System.Web.UI.WebControls.WebParts;
    using System.Web.Util;
    using System.Xml;
    using System.Security.Permissions;

    [ConfigurationCollection(typeof(TransformerInfo))]
    public sealed class TransformerInfoCollection : ConfigurationElementCollection {

        private static ConfigurationPropertyCollection _properties;

        private Hashtable _transformerEntries;

        static TransformerInfoCollection() {
            _properties = new ConfigurationPropertyCollection();
        }

        /// <internalonly />
        protected override ConfigurationPropertyCollection Properties {
            get {
                return _properties;
            }
        }

        public TransformerInfo this[int index] {
            get {
                return (TransformerInfo)BaseGet(index);
            }
            set {
                if (BaseGet(index) != null) {
                    BaseRemoveAt(index);
                }
                BaseAdd(index, value);
            }
        }

        public void Add(TransformerInfo transformerInfo) {
            BaseAdd(transformerInfo);
        }

        public void Clear() {
            BaseClear();
        }

        /// <internalonly />
        protected override ConfigurationElement CreateNewElement() {
            return new TransformerInfo();
        }

        public void Remove(string s) {
            BaseRemove(s);
        }

        public void RemoveAt(int index) {
            BaseRemoveAt(index);
        }

        /// <internalonly />
        protected override object GetElementKey(ConfigurationElement element) {
            return ((TransformerInfo)element).Name;
        }

        internal Hashtable GetTransformerEntries() {
            if (_transformerEntries == null) {
                lock (this) {
                    if (_transformerEntries == null) {
                        _transformerEntries = new Hashtable(StringComparer.OrdinalIgnoreCase);

                        foreach (TransformerInfo ti in this) {
                            Type transformerType = ConfigUtil.GetType(ti.Type, "type", ti);

                            if (transformerType.IsSubclassOf(typeof(WebPartTransformer)) == false) {
                                throw new ConfigurationErrorsException(
                                    SR.GetString(SR.Type_doesnt_inherit_from_type, 
                                        ti.Type, 
                                        typeof(WebPartTransformer).FullName),
                                    ti.ElementInformation.Properties["type"].Source, 
                                    ti.ElementInformation.Properties["type"].LineNumber);
                            }

                            Type consumerType;
                            Type providerType;
                            try {
                                consumerType = WebPartTransformerAttribute.GetConsumerType(transformerType);
                                providerType = WebPartTransformerAttribute.GetProviderType(transformerType);
                            }
                            catch (Exception e) {
                                throw new ConfigurationErrorsException(
                                    SR.GetString(SR.Transformer_attribute_error, e.Message),
                                    e, 
                                    ti.ElementInformation.Properties["type"].Source, 
                                    ti.ElementInformation.Properties["type"].LineNumber);
                            }

                            if (_transformerEntries.Count != 0) {
                                foreach (DictionaryEntry entry in _transformerEntries) {
                                    Type existingTransformerType = (Type)entry.Value;

                                    // We know these methods will not throw, because for the type to be in the transformers
                                    // collection, we must have successfully gotten the types previously without an exception.
                                    Type existingConsumerType = 
                                        WebPartTransformerAttribute.GetConsumerType(existingTransformerType);
                                    Type existingProviderType = 
                                        WebPartTransformerAttribute.GetProviderType(existingTransformerType);

                                    if ((consumerType == existingConsumerType) && (providerType == existingProviderType)) {
                                        throw new ConfigurationErrorsException(
                                            SR.GetString(SR.Transformer_types_already_added, 
                                                (string)entry.Key, 
                                                ti.Name),
                                            ti.ElementInformation.Properties["type"].Source, 
                                            ti.ElementInformation.Properties["type"].LineNumber);
                                    }
                                }
                            }

                            _transformerEntries[ti.Name] = transformerType;
                        }
                    }
                }
            }
            // 
            return _transformerEntries;
        }
    }
}
