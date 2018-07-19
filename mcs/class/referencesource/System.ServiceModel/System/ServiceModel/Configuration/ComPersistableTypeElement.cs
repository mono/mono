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

    public sealed partial class ComPersistableTypeElement : ConfigurationElement
    {
        public ComPersistableTypeElement()
            : base()
        {
        }
        public ComPersistableTypeElement(string ID)
            : this()
        {
            this.ID = ID;
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

        [ConfigurationProperty(ConfigurationStrings.ID, Options = ConfigurationPropertyOptions.IsKey | ConfigurationPropertyOptions.IsRequired)]
        [StringValidator(MinLength = 1)]
        public string ID
        {
            get { return (string)base[ConfigurationStrings.ID]; }
            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    value = String.Empty;
                }

                base[ConfigurationStrings.ID] = value;
            }
        }      
    }
}
