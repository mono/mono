// <copyright>
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace System.ServiceModel.Configuration
{
    using System.Configuration;
    using System.Runtime;

    /// <summary>
    /// DelegatingHandlerElementCollection for DelegatingHandlers
    /// </summary>
    [ConfigurationCollection(typeof(DelegatingHandlerElement), AddItemName = ConfigurationStrings.Handler, CollectionType = ConfigurationElementCollectionType.BasicMap)]
    public sealed class DelegatingHandlerElementCollection : ServiceModelConfigurationElementCollection<DelegatingHandlerElement>
    {
        public DelegatingHandlerElementCollection()
            : base(ConfigurationElementCollectionType.BasicMap, ConfigurationStrings.Handler)
        {
        }

        protected override bool ThrowOnDuplicate
        {
            get
            {
                return false;
            }
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            if (element == null)
            {
                throw FxTrace.Exception.ArgumentNull("element");
            }

            DelegatingHandlerElement delegatingHandlerElement = element as DelegatingHandlerElement;
            if (delegatingHandlerElement == null)
            {
                throw FxTrace.Exception.Argument("element", SR.GetString(SR.InputMustBeDelegatingHandlerElementError, typeof(ConfigurationElement).Name, typeof(DelegatingHandlerElement).Name));
            }

            return delegatingHandlerElement.Id;
        }
    }
}
