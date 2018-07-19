//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Configuration;
    using System.Globalization;
    using System.Xml;

    public sealed partial class ComContractElement : ConfigurationElement
    {

        public ComContractElement() : base() { }
        public ComContractElement(string contractType)
            : this()
        {
            this.Contract = contractType;
        }

        [ConfigurationProperty(ConfigurationStrings.Contract, Options = ConfigurationPropertyOptions.IsKey | ConfigurationPropertyOptions.IsRequired)]
        [StringValidator(MinLength = 1)]
        public string Contract
        {
            get { return (string)base[ConfigurationStrings.Contract]; }
            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    value = String.Empty;
                }

                base[ConfigurationStrings.Contract] = value;
            }
        }

        [ConfigurationProperty(ConfigurationStrings.ComMethodCollection, Options = ConfigurationPropertyOptions.None)]
        public ComMethodElementCollection ExposedMethods
        {
            get { return (ComMethodElementCollection)base[ConfigurationStrings.ComMethodCollection]; }
        }

        [ConfigurationProperty(ConfigurationStrings.ComContractName, DefaultValue = "", Options = ConfigurationPropertyOptions.None)]
        [StringValidator(MinLength = 0)]
        public string Name
        {
            get { return (string)base[ConfigurationStrings.ComContractName]; }
            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    value = String.Empty;
                }

                base[ConfigurationStrings.ComContractName] = value;
            }
        }

        [ConfigurationProperty(ConfigurationStrings.ComContractNamespace, DefaultValue = "", Options = ConfigurationPropertyOptions.None)]
        [StringValidator(MinLength = 0)]
        public string Namespace
        {
            get { return (string)base[ConfigurationStrings.ComContractNamespace]; }
            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    value = String.Empty;
                }

                base[ConfigurationStrings.ComContractNamespace] = value;
            }
        }

        [ConfigurationProperty(ConfigurationStrings.ComPersistableTypes)]
        public ComPersistableTypeElementCollection PersistableTypes
        {
            get { return (ComPersistableTypeElementCollection)base[ConfigurationStrings.ComPersistableTypes]; }
        }

        [ConfigurationProperty(ConfigurationStrings.ComSessionRequired, DefaultValue = true)]
        public bool RequiresSession
        {
            get { return (bool)base[ConfigurationStrings.ComSessionRequired]; }
            set
            {
                base[ConfigurationStrings.ComSessionRequired] = value;
            }
        }

        [ConfigurationProperty(ConfigurationStrings.ComUdtCollection)]
        public ComUdtElementCollection UserDefinedTypes
        {
            get { return (ComUdtElementCollection)base[ConfigurationStrings.ComUdtCollection]; }
        }

    }


}
