//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System.Configuration;
    using System.Diagnostics.Contracts;

    /// <summary>
    /// Binding-related Configuration elements use this base class for WCF-wide commonalities
    /// </summary>
    public abstract class ServiceModelConfigurationElement : ConfigurationElement
    {
        /// <summary>
        /// Used by InitializeFrom() pattern to avoid writing default values to generated .config files.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="propertyName">ConfigurationProperty.Name for the configuration property to set</param>
        /// <param name="value">Value to set</param>
        protected void SetPropertyValueIfNotDefaultValue<T>(string propertyName, T value)
        {
            var configurationProperty = this.Properties[propertyName];
            Contract.Assert(configurationProperty != null, "Parameter 'propertyName' should be the name of a configuration property of type T");
            Contract.Assert(configurationProperty.Type.IsAssignableFrom(typeof(T)), "Parameter 'propertyName' should be the name of a configuration property of type T");

            if (!object.Equals(value, configurationProperty.DefaultValue))
            {
                SetPropertyValue(configurationProperty, value, /*ignoreLocks = */ false);
            }
        }

    }
}
