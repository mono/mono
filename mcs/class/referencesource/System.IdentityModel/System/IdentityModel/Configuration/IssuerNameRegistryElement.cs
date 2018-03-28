//------------------------------------------------------------------------------
//     Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

using System.Configuration;

namespace System.IdentityModel.Configuration
{
    public sealed partial class IssuerNameRegistryElement : ConfigurationElementInterceptor
    {
        public IssuerNameRegistryElement()
        {
        }

        internal IssuerNameRegistryElement(string type)
        {
            Type = type;
        }

        /// <summary>
        /// Special case: type may be omitted but inner configuration may be present
        /// </summary>
        internal bool IsConfigured
        {
            get
            {
                return (ElementInformation.Properties[ConfigurationStrings.Type].ValueOrigin != PropertyValueOrigin.Default) || ((ChildNodes != null) && (ChildNodes.Count > 0));
            }
        }

        [ConfigurationProperty(ConfigurationStrings.Type, IsRequired = false, IsKey = false)]
        [StringValidator(MinLength = 0)]
        public string Type
        {
            get { return (string)this[ConfigurationStrings.Type]; }
            set { this[ConfigurationStrings.Type] = value; }
        }
    }
}
