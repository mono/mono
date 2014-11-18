//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
using System.ComponentModel;
using System.Configuration;
using System.IdentityModel.Selectors;

namespace System.IdentityModel.Configuration
{
#pragma warning disable 1591
    /// <summary>
    /// Manages the configuration of the audienceUris section.
    /// </summary>
    [ConfigurationCollection(typeof(AudienceUriElement),
        CollectionType = ConfigurationElementCollectionType.AddRemoveClearMap)]
    public sealed partial class AudienceUriElementCollection : ConfigurationElementCollection
    {
        const AudienceUriMode DefaultAudienceUriMode = AudienceUriMode.Always;

        protected override void Init()
        {
            base.Init();
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new AudienceUriElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((AudienceUriElement)element).Value;
        }

        /// <summary>
        /// Audience restriction mode. Optional, default is Always.
        /// </summary>
        [ConfigurationProperty(ConfigurationStrings.Mode, IsRequired = false, DefaultValue = DefaultAudienceUriMode)]
        [StandardRuntimeEnumValidator(typeof(AudienceUriMode))]
        public AudienceUriMode Mode
        {
            get { return (AudienceUriMode)this[ConfigurationStrings.Mode]; }
            set { this[ConfigurationStrings.Mode] = value; }
        }

        /// <summary>
        /// Returns a value indicating whether this element has been configured with non-default values.
        /// </summary>
        internal bool IsConfigured
        {
            get
            {
                return ((ElementInformation.Properties[ConfigurationStrings.Mode].ValueOrigin != PropertyValueOrigin.Default) || (Count > 0));
            }
        }
    }
#pragma warning restore 1591
}
