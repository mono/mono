//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Description
{
    using System.Globalization;
    using System.ServiceModel.Channels;
    using System.ServiceModel;
    using System.Collections.Generic;

    public static class MetadataExchangeBindings
    {
        static Binding httpBinding;
        static Binding httpGetBinding;
        static Binding httpsBinding;
        static Binding httpsGetBinding;
        static Binding tcpBinding;
        static Binding pipeBinding;

        internal static Binding Http
        {
            get
            {
                // don't need to lock because no guarantee of instance identity
                if (httpBinding == null)
                {
                    httpBinding = CreateHttpBinding();
                }

                return httpBinding;
            }
        }

        internal static Binding HttpGet
        {
            get
            {
                // don't need to lock because no guarantee of instance identity
                if (httpGetBinding == null)
                {
                    httpGetBinding = CreateHttpGetBinding();
                }

                return httpGetBinding;
            }
        }

        internal static Binding Https
        {
            get
            {
                // don't need to lock because no guarantee of instance identity
                if (httpsBinding == null)
                {
                    httpsBinding = CreateHttpsBinding();
                }

                return httpsBinding;
            }
        }

        internal static Binding HttpsGet
        {
            get
            {
                // don't need to lock because no guarantee of instance identity
                if (httpsGetBinding == null)
                {
                    httpsGetBinding = CreateHttpsGetBinding();
                }

                return httpsGetBinding;
            }
        }

        internal static Binding Tcp
        {
            get
            {
                // don't need to lock because no guarantee of instance identity
                if (tcpBinding == null)
                {
                    tcpBinding = CreateTcpBinding();
                }

                return tcpBinding;
            }
        }

        internal static Binding NamedPipe
        {
            get
            {
                // don't need to lock because no guarantee of instance identity
                if (pipeBinding == null)
                {
                    pipeBinding = CreateNamedPipeBinding();
                }

                return pipeBinding;
            }
        }

        public static Binding CreateMexHttpBinding()
        {
            return MetadataExchangeBindings.CreateHttpBinding();
        }

        public static Binding CreateMexHttpsBinding()
        {
            return MetadataExchangeBindings.CreateHttpsBinding();
        }

        public static Binding CreateMexTcpBinding()
        {
            return MetadataExchangeBindings.CreateTcpBinding();
        }

        public static Binding CreateMexNamedPipeBinding()
        {
            return MetadataExchangeBindings.CreateNamedPipeBinding();
        }

        internal static Binding GetBindingForScheme(string scheme)
        {
            Binding binding = null;
            TryGetBindingForScheme(scheme, out binding);
            return binding;
        }

        internal static bool TryGetBindingForScheme(string scheme, out Binding binding)
        {
            if (String.Compare(scheme, "http", StringComparison.OrdinalIgnoreCase) == 0)
            {
                binding = Http;
            }
            else if (String.Compare(scheme, "https", StringComparison.OrdinalIgnoreCase) == 0)
            {
                binding = Https;
            }
            else if (String.Compare(scheme, "net.tcp", StringComparison.OrdinalIgnoreCase) == 0) 
            {
                binding = Tcp;
            }
            else if (String.Compare(scheme, "net.pipe", StringComparison.OrdinalIgnoreCase) == 0) 
            {
                binding = NamedPipe;
            }
            else
            {
                binding = null;
            }

            return binding != null;
        }

        static WSHttpBinding CreateHttpBinding()
        {
            WSHttpBinding binding = new WSHttpBinding(SecurityMode.None, /* reliableSessionEnabled */ false);
            binding.Name = MetadataStrings.MetadataExchangeStrings.HttpBindingName;
            binding.Namespace = MetadataStrings.MetadataExchangeStrings.BindingNamespace;
            return binding;
        }

        static WSHttpBinding CreateHttpsBinding()
        {
            WSHttpBinding binding = new WSHttpBinding(
                new WSHttpSecurity(SecurityMode.Transport, new HttpTransportSecurity(), new NonDualMessageSecurityOverHttp()), /* reliableSessionEnabled */ false);
            binding.Name = MetadataStrings.MetadataExchangeStrings.HttpsBindingName;
            binding.Namespace = MetadataStrings.MetadataExchangeStrings.BindingNamespace;
            
            return binding;
        }

        static CustomBinding CreateHttpGetBinding()
        {
            return CreateGetBinding(new HttpTransportBindingElement());
        }

        static CustomBinding CreateHttpsGetBinding()
        {
            return CreateGetBinding(new HttpsTransportBindingElement());
        }

        static CustomBinding CreateGetBinding(HttpTransportBindingElement httpTransport)
        {
            TextMessageEncodingBindingElement textEncoding = new TextMessageEncodingBindingElement();
            textEncoding.MessageVersion = MessageVersion.None;
            httpTransport.Method = "GET";
            httpTransport.InheritBaseAddressSettings = true;
            return new CustomBinding(textEncoding, httpTransport);
        }

        static CustomBinding CreateTcpBinding()
        {
            CustomBinding binding = new CustomBinding(MetadataStrings.MetadataExchangeStrings.TcpBindingName, MetadataStrings.MetadataExchangeStrings.BindingNamespace);
            TcpTransportBindingElement tcpTransport = new TcpTransportBindingElement();
            binding.Elements.Add(tcpTransport);
            return binding;
        }

        static CustomBinding CreateNamedPipeBinding()
        {
            CustomBinding binding = new CustomBinding(MetadataStrings.MetadataExchangeStrings.NamedPipeBindingName, MetadataStrings.MetadataExchangeStrings.BindingNamespace);
            NamedPipeTransportBindingElement pipeTransport = new NamedPipeTransportBindingElement();
            binding.Elements.Add(pipeTransport);
            return binding;
        }

        internal static bool IsSchemeSupported(string scheme)
        {
            Binding binding;
            return TryGetBindingForScheme(scheme, out binding);
        }
    }
}
