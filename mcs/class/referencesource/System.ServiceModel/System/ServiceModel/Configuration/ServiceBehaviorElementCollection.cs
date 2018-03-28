//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System.Collections;
    using System.Configuration;
    using System.Globalization;
    using System.ServiceModel.Description;
    using System.Collections.ObjectModel;
    using System.ServiceModel.Channels;
    using System.Collections.Generic;

    [ConfigurationCollection(typeof(ServiceBehaviorElement), AddItemName = ConfigurationStrings.Behavior)]
    public sealed class ServiceBehaviorElementCollection : ServiceModelEnhancedConfigurationElementCollection<ServiceBehaviorElement>
    {

        public ServiceBehaviorElementCollection()
            : base(ConfigurationStrings.Behavior)
        { }

        protected override bool ThrowOnDuplicate
        {
            get { return false; }
        }

        protected override void DeserializeElement(System.Xml.XmlReader reader, bool serializeCollectionKey)
        {
            base.DeserializeElement(reader, serializeCollectionKey);
        }

        protected override Object GetElementKey(ConfigurationElement element)
        {
            if (element == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("element");
            }

            ServiceBehaviorElement configElementKey = (ServiceBehaviorElement)element;
            return configElementKey.Name;
        }

        protected override void BaseAdd(ConfigurationElement element)
        {
            if (null == element)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("element");
            }

            ServiceBehaviorElement childServiceBehaviorElement = element as ServiceBehaviorElement;
            string serviceBehaviorElementName = childServiceBehaviorElement.Name;
            ServiceBehaviorElement parentServiceBehaviorElement = this.BaseGet(serviceBehaviorElementName) as ServiceBehaviorElement;
            List<BehaviorExtensionElement> parentExtensionElements = new List<BehaviorExtensionElement>();
            if (parentServiceBehaviorElement != null)
            {
                foreach (BehaviorExtensionElement parentBehaviorElement in parentServiceBehaviorElement)
                {
                    parentExtensionElements.Add(parentBehaviorElement);
                }
            }
            childServiceBehaviorElement.MergeWith(parentExtensionElements);
            base.BaseAdd(childServiceBehaviorElement);
        }
    }
}

