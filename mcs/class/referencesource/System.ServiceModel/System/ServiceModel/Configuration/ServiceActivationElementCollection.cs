//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System;
    using System.Collections;
    using System.Configuration;
    using System.Globalization;

    [ConfigurationCollection(typeof(ServiceActivationElement))]
    public sealed class ServiceActivationElementCollection : ServiceModelConfigurationElementCollection<ServiceActivationElement>
    {
        public ServiceActivationElementCollection()
            : base(ConfigurationElementCollectionType.AddRemoveClearMap, ConfigurationStrings.Add)
        { 
        }        

        protected override ConfigurationElement CreateNewElement()
        {
            return new ServiceActivationElement();
        }

        protected override Object GetElementKey(ConfigurationElement element)
        {
            if (element == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("element");
            }

            ServiceActivationElement configElementKey = (ServiceActivationElement)element;
            return configElementKey.RelativeAddress;
        }

        protected override bool ThrowOnDuplicate
        {
            get { return true; }
        }        
    }
}
