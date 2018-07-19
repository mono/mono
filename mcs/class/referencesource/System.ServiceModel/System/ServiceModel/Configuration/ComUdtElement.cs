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

    public sealed partial class ComUdtElement : ConfigurationElement
    {
        public ComUdtElement()
            : base()
        {
        }
        public ComUdtElement(string typeDefID)
            : this()
        {
            this.TypeDefID = typeDefID;
        }

        [ConfigurationProperty(ConfigurationStrings.Name, DefaultValue = "", Options = ConfigurationPropertyOptions.None)]
        [StringValidator(MinLength = 0)]
        public string Name
        {
            get { return (string)base[ConfigurationStrings.Name]; }
            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    value = String.Empty;
                }

                base[ConfigurationStrings.Name] = value;
            }
        }

        [ConfigurationProperty(ConfigurationStrings.TypeLibID, Options = ConfigurationPropertyOptions.IsRequired)]
        [StringValidator(MinLength = 1)]
        public string TypeLibID
        {
            get { return (string)base[ConfigurationStrings.TypeLibID]; }
            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    value = String.Empty;
                }

                base[ConfigurationStrings.TypeLibID] = value;
            }
        }

        [ConfigurationProperty(ConfigurationStrings.TypeLibVersion, Options = ConfigurationPropertyOptions.IsRequired)]
        [StringValidator(MinLength = 1)]
        public string TypeLibVersion
        {
            get { return (string)base[ConfigurationStrings.TypeLibVersion]; }
            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    value = String.Empty;
                }

                base[ConfigurationStrings.TypeLibVersion] = value;
            }
        }

        [ConfigurationProperty(ConfigurationStrings.TypeDefID, Options = ConfigurationPropertyOptions.IsKey | ConfigurationPropertyOptions.IsRequired)]
        [StringValidator(MinLength = 1)]
        public string TypeDefID
        {
            get { return (string)base[ConfigurationStrings.TypeDefID]; }
            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    value = String.Empty;
                }

                base[ConfigurationStrings.TypeDefID] = value;
            }
        }

    }

}
