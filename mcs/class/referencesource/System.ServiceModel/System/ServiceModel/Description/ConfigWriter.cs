//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Description
{
    using System;
    using System.ServiceModel.Channels;
    using System.Configuration;
    using System.Collections.Generic;
    using System.ServiceModel;
    using System.ServiceModel.Configuration;
    using System.ServiceModel.Diagnostics;

    internal class ConfigWriter
    {
        readonly Dictionary<Binding, BindingDictionaryValue> bindingTable;
        readonly BindingsSection bindingsSection;
        readonly ChannelEndpointElementCollection channels;
        readonly Configuration config;

        internal ConfigWriter(Configuration configuration)
        {
            this.bindingTable = new Dictionary<Binding, BindingDictionaryValue>();

            this.bindingsSection = BindingsSection.GetSection(configuration);

            ServiceModelSectionGroup serviceModelSectionGroup = ServiceModelSectionGroup.GetSectionGroup(configuration);
            this.channels = serviceModelSectionGroup.Client.Endpoints;
            this.config = configuration;
        }

        internal ChannelEndpointElement WriteChannelDescription(ServiceEndpoint endpoint, string typeName)
        {
            ChannelEndpointElement channelElement = null;

            // Create Binding
            BindingDictionaryValue bindingDV = CreateBindingConfig(endpoint.Binding);


            channelElement = new ChannelEndpointElement(endpoint.Address, typeName);

            // [....]: review: Use decoded form to preserve the user-given friendly name, however, beacuse our Encoding algorithm
            // does not touch ASCII names, a name that looks like encoded name will not roundtrip(Example: "_x002C_" will turned into ",")
            channelElement.Name = NamingHelper.GetUniqueName(NamingHelper.CodeName(endpoint.Name), this.CheckIfChannelNameInUse, null);

            channelElement.BindingConfiguration = bindingDV.BindingName;
            channelElement.Binding = bindingDV.BindingSectionName;
            channels.Add(channelElement);

            return channelElement;            
        }

        internal void WriteBinding(Binding binding, out string bindingSectionName, out string configurationName)
        {
            BindingDictionaryValue result = CreateBindingConfig(binding);

            configurationName = result.BindingName;
            bindingSectionName = result.BindingSectionName;
        }

        BindingDictionaryValue CreateBindingConfig(Binding binding)
        {
            BindingDictionaryValue bindingDV;
            if (!bindingTable.TryGetValue(binding, out bindingDV))
            {
                // [....]: review: Use decoded form to preserve the user-given friendly name, however, beacuse our Encoding algorithm
                // does not touch ASCII names, a name that looks like encoded name will not roundtrip(Example: "_x002C_" will turned into ",")
                string bindingName = NamingHelper.GetUniqueName(NamingHelper.CodeName(binding.Name), this.CheckIfBindingNameInUse, null);
                string bindingSectionName;

                if (!BindingsSection.TryAdd(bindingName, binding, config, out bindingSectionName))
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.GetString(SR.ConfigBindingCannotBeConfigured), "endpoint.Binding"));

                bindingDV = new BindingDictionaryValue(bindingName, bindingSectionName);
                bindingTable.Add(binding, bindingDV);
            }
            return bindingDV;
        }

        bool CheckIfBindingNameInUse(string name, object nameCollection)
        {
            foreach (BindingCollectionElement bindingCollectionElement in this.bindingsSection.BindingCollections)
                if (bindingCollectionElement.ContainsKey(name))
                    return true;

            return false;
        }

        bool CheckIfChannelNameInUse(string name, object namingCollection)
        {
            foreach (ChannelEndpointElement element in this.channels)
                if (element.Name == name)
                    return true;

            return false;
        }

        sealed class BindingDictionaryValue
        {
            public readonly string BindingName;
            public readonly string BindingSectionName;

            public BindingDictionaryValue(string bindingName, string bindingSectionName)
            {
                this.BindingName = bindingName;
                this.BindingSectionName = bindingSectionName;
            }
        }

    }
}
