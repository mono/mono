//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.ServiceModel.ComIntegration
{
    using System;
    using System.ServiceModel.Channels;
    using System.ServiceModel;
    using System.Runtime.InteropServices;
    using System.Runtime.InteropServices.ComTypes;
    using Microsoft.Win32;
    using System.Reflection;
    using System.Collections.Generic;
    using System.Threading;
    using System.Text;
    internal static class MonikerHelper
    {
        internal enum MonikerAttribute
        {
            Address,
            Contract,
            Wsdl,
            SpnIdentity,
            UpnIdentity,
            DnsIdentity,
            Binding,
            BindingConfiguration,
            MexAddress,
            MexBinding,
            MexBindingConfiguration,
            BindingNamespace,
            ContractNamespace,
            MexSpnIdentity,
            MexUpnIdentity,
            MexDnsIdentity,
            Serializer
        }

        internal struct KeywordInfo
        {
            internal KeywordInfo(string name, MonikerAttribute attrib)
            {
                Name = name;
                Attrib = attrib;
            }
            internal string Name;
            internal MonikerAttribute Attrib;
            internal static readonly KeywordInfo[] KeywordCollection = new KeywordInfo[] 
               {
                  new KeywordInfo ("address", MonikerAttribute.Address), 
                  new KeywordInfo ("contract", MonikerAttribute.Contract),
                  new KeywordInfo ("wsdl", MonikerAttribute.Wsdl), 
                  new KeywordInfo ("spnidentity", MonikerAttribute.SpnIdentity),
                  new KeywordInfo ("upnidentity", MonikerAttribute.UpnIdentity),  
                  new KeywordInfo ("dnsidentity", MonikerAttribute.DnsIdentity),
                  new KeywordInfo ("binding", MonikerAttribute.Binding), 
                  new KeywordInfo ("bindingconfiguration", MonikerAttribute.BindingConfiguration),
                  new KeywordInfo ("mexaddress", MonikerAttribute.MexAddress),
                  new KeywordInfo ("mexbindingconfiguration", MonikerAttribute.MexBindingConfiguration),
                  new KeywordInfo ("mexbinding", MonikerAttribute.MexBinding),
                  new KeywordInfo ("bindingnamespace", MonikerAttribute.BindingNamespace),
                  new KeywordInfo ("contractnamespace", MonikerAttribute.ContractNamespace),
                  new KeywordInfo ("mexspnidentity", MonikerAttribute.MexSpnIdentity),
                  new KeywordInfo ("mexupnidentity", MonikerAttribute.MexUpnIdentity),
                  new KeywordInfo ("mexdnsidentity", MonikerAttribute.MexDnsIdentity),
                   new KeywordInfo ("serializer", MonikerAttribute.Serializer)
               };
        }
    }
};

