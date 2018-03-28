//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.PeerResolvers
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Configuration;
    using System.Globalization;
    using System.Net.Security;
    using System.ServiceModel.Configuration;
    using System.ServiceModel.Channels;

    public class PeerCustomResolverSettings
    {
        EndpointAddress address;
        Binding binding;
        string bindingSection, bindingConfiguration;
        PeerResolver resolver;

        public PeerCustomResolverSettings() { }
        public EndpointAddress Address
        {
            get
            {
                return address;
            }
            set
            {
                address = value;
            }
        }
        public Binding Binding
        {
            get
            {
                if (binding == null)
                {
                    if (!String.IsNullOrEmpty(this.bindingSection) && !String.IsNullOrEmpty(this.bindingConfiguration))
                        binding = ConfigLoader.LookupBinding(this.bindingSection, this.bindingConfiguration);
                }
                return binding;
            }
            set
            {
                binding = value;
            }
        }
        public bool IsBindingSpecified
        {
            get
            {
                return ((this.binding != null) || (!String.IsNullOrEmpty(this.bindingSection) && !String.IsNullOrEmpty(this.bindingConfiguration)));
            }
        }
        public PeerResolver Resolver
        {
            get
            {
                return resolver;
            }
            set
            {
                resolver = value;
            }
        }
        internal string BindingSection
        {
            get
            {
                return bindingSection;
            }
            set
            {
                bindingSection = value;
            }
        }
        internal string BindingConfiguration
        {
            get
            {
                return bindingConfiguration;
            }
            set
            {
                bindingConfiguration = value;
            }
        }

    }

}
