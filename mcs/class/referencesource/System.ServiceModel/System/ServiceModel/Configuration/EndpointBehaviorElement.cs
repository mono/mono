//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System;
    using System.ServiceModel;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Configuration;
    using System.Globalization;
    using System.Xml;

    public partial class EndpointBehaviorElement : NamedServiceModelExtensionCollectionElement<BehaviorExtensionElement>
    {
        public EndpointBehaviorElement()
            : this(null)
        {
        }

        public EndpointBehaviorElement(string name)
            : base(ConfigurationStrings.BehaviorExtensions, name)
        {
        }

        // Verify that the behavior being added implements IEndpointBehavior
        public override void Add(BehaviorExtensionElement element)
        {
            // If element is null, let base.Add() throw for consistency reasons
            if (null != element)
            {
                if (element is ClearBehaviorElement || element is RemoveBehaviorElement)
                {
                    base.AddItem(element);
                    return;
                }
                if (!typeof(System.ServiceModel.Description.IEndpointBehavior).IsAssignableFrom(element.BehaviorType))
                {
#pragma warning disable 56506 //[....]; element.ElementInformation is guaranteed to be non-null(System.Configuration)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(SR.GetString(SR.ConfigInvalidEndpointBehaviorType,
                        element.ConfigurationElementName,
                        this.Name),
                        element.ElementInformation.Source,
                        element.ElementInformation.LineNumber));
#pragma warning restore
                }
            }

            base.Add(element);
        }

        // Verify that the behavior being added implements IEndpointBehavior
        public override bool CanAdd(BehaviorExtensionElement element)
        {
            // If element is null, let base.CanAdd() throw for consistency reasons
            if (null != element)
            {
                if (element is ClearBehaviorElement || element is RemoveBehaviorElement)
                {
                    return true;
                }
                if (!typeof(System.ServiceModel.Description.IEndpointBehavior).IsAssignableFrom(element.BehaviorType))
                {
#pragma warning disable 56506 //[....]; element.ElementInformation is guaranteed to be non-null(System.Configuration)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(SR.GetString(SR.ConfigInvalidEndpointBehaviorType,
                        element.ConfigurationElementName,
                        this.Name),
                        element.ElementInformation.Source,
                        element.ElementInformation.LineNumber));
#pragma warning restore
                }
            }

            return base.CanAdd(element);
        }
    }
}
