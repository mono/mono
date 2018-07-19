//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System.Configuration;

    public sealed partial class RemoveBehaviorElement : BehaviorExtensionElement
    {
        public RemoveBehaviorElement() { }
        
        [ConfigurationProperty(ConfigurationStrings.Name, Options = ConfigurationPropertyOptions.IsRequired)]
        [StringValidator(MinLength = 1)]
        public string Name
        {
            get { return (string)base[ConfigurationStrings.Name]; }
            set { base[ConfigurationStrings.Name] = value; }
        }
        
        public override void CopyFrom(ServiceModelExtensionElement from)
        {
            base.CopyFrom(from);

            RemoveBehaviorElement source = (RemoveBehaviorElement) from;
#pragma warning suppress 56506 //Microsoft; base.CopyFrom() checks for 'from' being null
            this.Name = source.Name;
        }

        protected internal override object CreateBehavior()
        {
            return null;
        }

        public override Type BehaviorType
        {
            get { return null; }
        }
    }
}
