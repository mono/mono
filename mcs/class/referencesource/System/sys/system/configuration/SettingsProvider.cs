//------------------------------------------------------------------------------
// <copyright file="SettingsProvider.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Configuration {
    using  System.Collections.Specialized;
    using  System.Runtime.Serialization;
    using  System.Configuration.Provider;

    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>

    public abstract class SettingsProvider : ProviderBase
    {
        public abstract SettingsPropertyValueCollection GetPropertyValues(SettingsContext context, SettingsPropertyCollection collection);
        public abstract void SetPropertyValues(SettingsContext context, SettingsPropertyValueCollection collection);
        public abstract string ApplicationName { get; set; }
    }
}
