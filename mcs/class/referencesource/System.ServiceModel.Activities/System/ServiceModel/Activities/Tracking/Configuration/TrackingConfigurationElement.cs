//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Activities.Tracking.Configuration
{
    using System.Configuration;
    using System.Globalization;
    using System.Runtime;
    using System.Diagnostics.CodeAnalysis;

    // Base class for all the workflow tracking configuration elements
    [Fx.Tag.XamlVisible(false)]
    public abstract class TrackingConfigurationElement : ConfigurationElement
    {
        [SuppressMessage(FxCop.Category.Configuration, FxCop.Rule.ConfigurationPropertyAttributeRule,
            Justification = "This property is defined by the base class to compute unique key.")]
        public abstract object ElementKey { get; } 

        protected static string GetStringPairKey(string value1, string value2)
        {
            return String.Format(CultureInfo.InvariantCulture, "{0}-{1}{2}", 
                ((value1 == null) ? 0 : value1.Length), value1, value2);
        }
    }
}
