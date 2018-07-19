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

    public sealed partial class ComMethodElement : ConfigurationElement
    {
        public ComMethodElement()
            : base()
        {
        }

        public ComMethodElement(string method)
            : this()
        {
            this.ExposedMethod = method;
        }

        [ConfigurationProperty(ConfigurationStrings.ExposedMethod, Options = ConfigurationPropertyOptions.IsKey | ConfigurationPropertyOptions.IsRequired)]
        [StringValidator(MinLength = 1)]
        public string ExposedMethod
        {
            get { return (string)base[ConfigurationStrings.ExposedMethod]; }
            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    value = String.Empty;
                }

                base[ConfigurationStrings.ExposedMethod] = value;
            }
        }



    }


}
