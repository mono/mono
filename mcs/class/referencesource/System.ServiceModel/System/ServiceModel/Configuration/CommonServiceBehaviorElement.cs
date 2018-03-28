//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System;
    using System.Configuration;
    using System.Xml;

    public partial class CommonServiceBehaviorElement : ServiceModelExtensionCollectionElement<BehaviorExtensionElement>
    {
        public CommonServiceBehaviorElement()
            : base(ConfigurationStrings.BehaviorExtensions)
        {
        }

        // Verify that the behavior being added implements IServiceBehavior
        public override void Add(BehaviorExtensionElement element)
        {
            // If element is null, let base.Add() throw for consistency reasons
            if (null != element)
            {
                if (!typeof(System.ServiceModel.Description.IServiceBehavior).IsAssignableFrom(element.BehaviorType))
                {
#pragma warning disable 56506 //Microsoft; element.ElementInformation is guaranteed to be non-null(System.Configuration)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(SR.GetString(SR.ConfigInvalidCommonServiceBehaviorType,
                        element.ConfigurationElementName,
                        typeof(System.ServiceModel.Description.IServiceBehavior).FullName),
                        element.ElementInformation.Source,
                        element.ElementInformation.LineNumber));
#pragma warning restore
                }
            }

            base.Add(element);
        }

        // Verify that the behavior being added implements IServiceBehavior
        public override bool CanAdd(BehaviorExtensionElement element)
        {
            // If element is null, let base.CanAdd() throw for consistency reasons
            if (null != element)
            {
                if (!typeof(System.ServiceModel.Description.IServiceBehavior).IsAssignableFrom(element.BehaviorType))
                {
#pragma warning disable 56506 //Microsoft; element.ElementInformation is guaranteed to be non-null(System.Configuration)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(SR.GetString(SR.ConfigInvalidCommonServiceBehaviorType,
                        element.ConfigurationElementName,
                        typeof(System.ServiceModel.Description.IServiceBehavior).FullName),
                        element.ElementInformation.Source,
                        element.ElementInformation.LineNumber));
#pragma warning restore
                }
            }

            return base.CanAdd(element);
        }
    }
}

