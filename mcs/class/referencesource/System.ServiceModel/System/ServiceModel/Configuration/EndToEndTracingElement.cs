//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System;
    using System.Configuration;

    public sealed partial class EndToEndTracingElement : ConfigurationElement
    {
        // These three constructors are used by the configuration system. 
        public EndToEndTracingElement()
            : base()
        {
        }

        [ConfigurationProperty(ConfigurationStrings.PropagateActivity, DefaultValue = false)]
        public bool PropagateActivity
        {
            get { return (bool)base[ConfigurationStrings.PropagateActivity]; }
            set { base[ConfigurationStrings.PropagateActivity] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.ActivityTracing, DefaultValue = false)]
        public bool ActivityTracing
        {
            get { return (bool)base[ConfigurationStrings.ActivityTracing]; }
            set { base[ConfigurationStrings.ActivityTracing] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.MessageFlowTracing, DefaultValue = false)]
        public bool MessageFlowTracing
        {
            get { return (bool)base[ConfigurationStrings.MessageFlowTracing]; }
            set { base[ConfigurationStrings.MessageFlowTracing] = value; }
        }
    }
}



