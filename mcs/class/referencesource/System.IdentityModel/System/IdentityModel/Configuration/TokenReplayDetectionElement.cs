//------------------------------------------------------------------------------
//     Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

using System.Configuration;
using System.ComponentModel;

namespace System.IdentityModel.Configuration
{
    /// <summary>
    /// Manages the configuration of a TokenReplayDetection element
    /// </summary>
    public sealed partial class TokenReplayDetectionElement : ConfigurationElement
    {   
        /// <summary>
        /// Enabled, optional.  Specifies if replays should be detected.
        /// </summary>
        [ConfigurationProperty( ConfigurationStrings.Enabled, IsRequired = false, DefaultValue = false )]
        public bool Enabled
        {
            get { return (bool)this[ConfigurationStrings.Enabled]; }
            set { this[ConfigurationStrings.Enabled] = value; }
        }

        /// <summary>
        /// ExpirationPeriod optional. Specifies the maximum amount of time before an item is considered expired and removed from the cache.
        /// </summary>
        [ConfigurationProperty(ConfigurationStrings.ExpirationPeriod, IsRequired = false, DefaultValue = ConfigurationStrings.TimeSpanMaxValue)]
        [TypeConverter(typeof(TimeSpanOrInfiniteConverter))]
        [IdentityModelTimeSpanValidator(MinValueString = ConfigurationStrings.TimeSpanZero, MaxValueString = ConfigurationStrings.TimeSpanMaxValue)]
        public TimeSpan ExpirationPeriod
        {
            get { return (TimeSpan)this[ConfigurationStrings.ExpirationPeriod]; }
            set { this[ConfigurationStrings.ExpirationPeriod] = value; }
        }        

        /// <summary>
        /// Returns a value indicating whether this element has been configured with non-default values.
        /// </summary>
        internal bool IsConfigured
        {
            get
            {
                return (ElementInformation.Properties[ConfigurationStrings.Enabled].ValueOrigin != PropertyValueOrigin.Default ||
                         ElementInformation.Properties[ConfigurationStrings.ExpirationPeriod].ValueOrigin != PropertyValueOrigin.Default);
            }
        }
    }
}
