//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System.Collections;
    using System.Configuration;
    using System.Globalization;
    using System.Collections.Generic;

    [ConfigurationCollection(typeof(EndpointBehaviorElement), AddItemName = ConfigurationStrings.Behavior)]
    public sealed class EndpointBehaviorElementCollection : ServiceModelEnhancedConfigurationElementCollection<EndpointBehaviorElement>
    {
        public EndpointBehaviorElementCollection()
            : base(ConfigurationStrings.Behavior)
        { }

        protected override bool ThrowOnDuplicate
        {
            get { return false; }
        }

        protected override Object GetElementKey(ConfigurationElement element)
        {
            if (element == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("element");
            }

            EndpointBehaviorElement configElementKey = (EndpointBehaviorElement)element;
            return configElementKey.Name;
        }

        protected override void BaseAdd(ConfigurationElement element)
        {
            if (null == element)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("element");
            }

            EndpointBehaviorElement childEndpointBehaviorElement = element as EndpointBehaviorElement;
            string endpointBehaviorElementName = childEndpointBehaviorElement.Name;
            EndpointBehaviorElement parentEndpointBehaviorElement = this.BaseGet(endpointBehaviorElementName) as EndpointBehaviorElement;
            List<BehaviorExtensionElement> parentExtensionElements = new List<BehaviorExtensionElement>();
            if (parentEndpointBehaviorElement != null)
            {
                foreach (BehaviorExtensionElement parentBehaviorElement in parentEndpointBehaviorElement)
                {
                    parentExtensionElements.Add(parentBehaviorElement);
                }
            }
            childEndpointBehaviorElement.MergeWith(parentExtensionElements);
            base.BaseAdd(childEndpointBehaviorElement);
        }
    }
}
