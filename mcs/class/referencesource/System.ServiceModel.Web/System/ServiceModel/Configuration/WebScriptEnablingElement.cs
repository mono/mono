//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System.ServiceModel.Description;

    public sealed partial class WebScriptEnablingElement : BehaviorExtensionElement
    {
        public WebScriptEnablingElement()
        {
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Configuration", "Configuration102:ConfigurationPropertyAttributeRule", MessageId = "System.ServiceModel.Configuration.WebScriptEnablingElement.BehaviorType",
            Justification = "Not a configurable property; a property that had to be overridden from abstract parent class")]
        public override Type BehaviorType
        {
            get { return typeof(WebScriptEnablingBehavior); }
        }

        internal protected override object CreateBehavior()
        {
            return new WebScriptEnablingBehavior();
        }

    }
}
