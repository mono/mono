//----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------------------
namespace System.ServiceModel.Description
{
    using System.Collections.Generic;
    using System.Runtime;
    using System.ServiceModel.Dispatcher;
    using System.Xml;
    using WsdlNS = System.Web.Services.Description;

    static class SoapHelper
    {
        static object SoapVersionStateKey = new object();
        static XmlDocument xmlDocument;

        static XmlDocument Document
        {
            get
            {
                if (xmlDocument == null)
                    xmlDocument = new XmlDocument();
                return xmlDocument;
            }
        }

        static XmlAttribute CreateLocalAttribute(string name, string value)
        {
            XmlAttribute attribute = Document.CreateAttribute(name);
            attribute.Value = value;
            return attribute;
        }
        // -----------------------------------------------------------------------------------------------------------------------
        // Developers Note: We go through a little song an dance here to Get or Create an exsisting SoapBinding from the WSDL
        // Extensions for a number of reasons:
        //      1. Multiple Extensions may contribute to the settings in the soap binding and so to make this work without
        //          relying on ordering, we need the GetOrCreate method.
        //      2. There are diffrent classes for diffrent SOAP versions and the extensions that determines the version is
        //          also un-ordered so when we finally figure out the version we may need to recreate the BindingExtension and
        //          clone it.

        internal static WsdlNS.SoapAddressBinding GetOrCreateSoapAddressBinding(WsdlNS.Binding wsdlBinding, WsdlNS.Port wsdlPort, WsdlExporter exporter)
        {
            if (GetSoapVersionState(wsdlBinding, exporter) == EnvelopeVersion.None)
                return null;

            WsdlNS.SoapAddressBinding existingSoapAddressBinding = GetSoapAddressBinding(wsdlPort);
            EnvelopeVersion version = GetSoapVersion(wsdlBinding);

            if (existingSoapAddressBinding != null)
                return existingSoapAddressBinding;

            WsdlNS.SoapAddressBinding soapAddressBinding = CreateSoapAddressBinding(version, wsdlPort);
            return soapAddressBinding;
        }

        internal static WsdlNS.SoapBinding GetOrCreateSoapBinding(WsdlEndpointConversionContext endpointContext, WsdlExporter exporter)
        {
            if (GetSoapVersionState(endpointContext.WsdlBinding, exporter) == EnvelopeVersion.None)
                return null;

            WsdlNS.SoapBinding existingSoapBinding = GetSoapBinding(endpointContext);
            if (existingSoapBinding != null)
            {
                return existingSoapBinding;
            }

            EnvelopeVersion version = GetSoapVersion(endpointContext.WsdlBinding);
            WsdlNS.SoapBinding soapBinding = CreateSoapBinding(version, endpointContext.WsdlBinding);
            return soapBinding;
        }

        internal static WsdlNS.SoapOperationBinding GetOrCreateSoapOperationBinding(WsdlEndpointConversionContext endpointContext, OperationDescription operation, WsdlExporter exporter)
        {
            if (GetSoapVersionState(endpointContext.WsdlBinding, exporter) == EnvelopeVersion.None)
                return null;

            WsdlNS.SoapOperationBinding existingSoapOperationBinding = GetSoapOperationBinding(endpointContext, operation);
            WsdlNS.OperationBinding wsdlOperationBinding = endpointContext.GetOperationBinding(operation);
            EnvelopeVersion version = GetSoapVersion(endpointContext.WsdlBinding);

            if (existingSoapOperationBinding != null)
                return existingSoapOperationBinding;

            WsdlNS.SoapOperationBinding soapOperationBinding = CreateSoapOperationBinding(version, wsdlOperationBinding);
            return soapOperationBinding;
        }

        internal static WsdlNS.SoapBodyBinding GetOrCreateSoapBodyBinding(WsdlEndpointConversionContext endpointContext, WsdlNS.MessageBinding wsdlMessageBinding, WsdlExporter exporter)
        {
            if (GetSoapVersionState(endpointContext.WsdlBinding, exporter) == EnvelopeVersion.None)
                return null;

            WsdlNS.SoapBodyBinding existingSoapBodyBinding = GetSoapBodyBinding(endpointContext, wsdlMessageBinding);
            EnvelopeVersion version = GetSoapVersion(endpointContext.WsdlBinding);

            if (existingSoapBodyBinding != null)
                return existingSoapBodyBinding;

            WsdlNS.SoapBodyBinding soapBodyBinding = CreateSoapBodyBinding(version, wsdlMessageBinding);
            return soapBodyBinding;
        }

        internal static WsdlNS.SoapHeaderBinding CreateSoapHeaderBinding(WsdlEndpointConversionContext endpointContext, WsdlNS.MessageBinding wsdlMessageBinding)
        {
            EnvelopeVersion version = GetSoapVersion(endpointContext.WsdlBinding);

            WsdlNS.SoapHeaderBinding soapHeaderBinding = CreateSoapHeaderBinding(version, wsdlMessageBinding);
            return soapHeaderBinding;
        }

        internal static void CreateSoapFaultBinding(string name, WsdlEndpointConversionContext endpointContext, WsdlNS.FaultBinding wsdlFaultBinding, bool isEncoded)
        {
            EnvelopeVersion version = GetSoapVersion(endpointContext.WsdlBinding);
            XmlElement fault = CreateSoapFaultBinding(version);
            fault.Attributes.Append(CreateLocalAttribute("name", name));
            fault.Attributes.Append(CreateLocalAttribute("use", isEncoded ? "encoded" : "literal"));
            wsdlFaultBinding.Extensions.Add(fault);
        }

        internal static void SetSoapVersion(WsdlEndpointConversionContext endpointContext, WsdlExporter exporter, EnvelopeVersion version)
        {
            SetSoapVersionState(endpointContext.WsdlBinding, exporter, version);

            //Convert all SOAP extensions to the right version.
            if (endpointContext.WsdlPort != null)
                SoapConverter.ConvertExtensions(endpointContext.WsdlPort.Extensions, version, SoapConverter.ConvertSoapAddressBinding);

            SoapConverter.ConvertExtensions(endpointContext.WsdlBinding.Extensions, version, SoapConverter.ConvertSoapBinding);

            foreach (WsdlNS.OperationBinding operationBinding in endpointContext.WsdlBinding.Operations)
            {
                SoapConverter.ConvertExtensions(operationBinding.Extensions, version, SoapConverter.ConvertSoapOperationBinding);

                //Messages
                {
                    if (operationBinding.Input != null)
                        SoapConverter.ConvertExtensions(operationBinding.Input.Extensions, version, SoapConverter.ConvertSoapMessageBinding);
                    if (operationBinding.Output != null)
                        SoapConverter.ConvertExtensions(operationBinding.Output.Extensions, version, SoapConverter.ConvertSoapMessageBinding);

                    foreach (WsdlNS.MessageBinding faultBinding in operationBinding.Faults)
                        SoapConverter.ConvertExtensions(faultBinding.Extensions, version, SoapConverter.ConvertSoapMessageBinding);
                }
            }

        }

        internal static EnvelopeVersion GetSoapVersion(WsdlNS.Binding wsdlBinding)
        {
            foreach (object o in wsdlBinding.Extensions)
            {
                if (o is WsdlNS.SoapBinding)
                    return o is WsdlNS.Soap12Binding ? EnvelopeVersion.Soap12 : EnvelopeVersion.Soap11;
            }
            return EnvelopeVersion.Soap12;
        }

        private static void SetSoapVersionState(WsdlNS.Binding wsdlBinding, WsdlExporter exporter, EnvelopeVersion version)
        {
            object versions = null;

            if (!exporter.State.TryGetValue(SoapVersionStateKey, out versions))
            {
                versions = new Dictionary<WsdlNS.Binding, EnvelopeVersion>();
                exporter.State[SoapVersionStateKey] = versions;
            }

            ((Dictionary<WsdlNS.Binding, EnvelopeVersion>)versions)[wsdlBinding] = version;
        }

        private static EnvelopeVersion GetSoapVersionState(WsdlNS.Binding wsdlBinding, WsdlExporter exporter)
        {
            object versions = null;

            if (exporter.State.TryGetValue(SoapVersionStateKey, out versions))
            {
                if (versions != null && ((Dictionary<WsdlNS.Binding, EnvelopeVersion>)versions).ContainsKey(wsdlBinding))
                {
                    return ((Dictionary<WsdlNS.Binding, EnvelopeVersion>)versions)[wsdlBinding];
                }
            }
            return null;
        }

        static class SoapConverter
        {

            // Microsoft, this could be simplified if we used generics.
            internal static void ConvertExtensions(WsdlNS.ServiceDescriptionFormatExtensionCollection extensions, EnvelopeVersion version, ConvertExtension conversionMethod)
            {
                bool foundOne = false;
                for (int i = extensions.Count - 1; i >= 0; i--)
                {
                    object o = extensions[i];
                    if (conversionMethod(ref o, version))
                    {
                        if (o == null)
                            extensions.Remove(extensions[i]);
                        else
                            extensions[i] = o;
                        foundOne = true;
                    }
                }

                if (!foundOne)
                {
                    object o = null;
                    conversionMethod(ref o, version);
                    if (o != null)
                        extensions.Add(o);
                }

            }

            // This is the delegate implemented by the 4 methods below. It is expected to:
            // If given a null reference for src, a new instance of the extension can be set.
            internal delegate bool ConvertExtension(ref object src, EnvelopeVersion version);

            internal static bool ConvertSoapBinding(ref object src, EnvelopeVersion version)
            {
                WsdlNS.SoapBinding binding = src as WsdlNS.SoapBinding;

                if (src != null)
                {
                    if (binding == null)
                        return false; // not a soap object
                    else if (GetBindingVersion<WsdlNS.Soap12Binding>(src) == version)
                        return true; // matched but same version; no change
                }

                if (version == EnvelopeVersion.None)
                {
                    src = null;
                    return true;
                }

                WsdlNS.SoapBinding dest = version == EnvelopeVersion.Soap12 ? new WsdlNS.Soap12Binding() : new WsdlNS.SoapBinding();
                if (binding != null)
                {
                    dest.Required = binding.Required;
                    dest.Style = binding.Style;
                    dest.Transport = binding.Transport;
                }

                src = dest;
                return true;
            }

            internal static bool ConvertSoapAddressBinding(ref object src, EnvelopeVersion version)
            {
                WsdlNS.SoapAddressBinding binding = src as WsdlNS.SoapAddressBinding;

                if (src != null)
                {
                    if (binding == null)
                        return false; // no match
                    else if (GetBindingVersion<WsdlNS.Soap12AddressBinding>(src) == version)
                        return true; // matched but same version; no change
                }

                if (version == EnvelopeVersion.None)
                {
                    src = null;
                    return true;
                }

                WsdlNS.SoapAddressBinding dest = version == EnvelopeVersion.Soap12 ? new WsdlNS.Soap12AddressBinding() : new WsdlNS.SoapAddressBinding();
                if (binding != null)
                {
                    dest.Required = binding.Required;
                    dest.Location = binding.Location;
                }

                src = dest;
                return true;
            }


            // returns true if src is an expected type; updates src in place; should handle null
            internal static bool ConvertSoapOperationBinding(ref object src, EnvelopeVersion version)
            {
                WsdlNS.SoapOperationBinding binding = src as WsdlNS.SoapOperationBinding;

                if (src != null)
                {
                    if (binding == null)
                        return false; // no match
                    else if (GetBindingVersion<WsdlNS.Soap12OperationBinding>(src) == version)
                        return true; // matched but same version
                }

                if (version == EnvelopeVersion.None)
                {
                    src = null;
                    return true;
                }

                WsdlNS.SoapOperationBinding dest = version == EnvelopeVersion.Soap12 ? new WsdlNS.Soap12OperationBinding() : new WsdlNS.SoapOperationBinding();
                if (src != null)
                {
                    dest.Required = binding.Required;
                    dest.Style = binding.Style;
                    dest.SoapAction = binding.SoapAction;
                }

                src = dest;
                return true;
            }

            internal static bool ConvertSoapMessageBinding(ref object src, EnvelopeVersion version)
            {
                WsdlNS.SoapBodyBinding body = src as WsdlNS.SoapBodyBinding;
                if (body != null)
                {
                    src = ConvertSoapBodyBinding(body, version);
                    return true;
                }

                WsdlNS.SoapHeaderBinding header = src as WsdlNS.SoapHeaderBinding;
                if (header != null)
                {
                    src = ConvertSoapHeaderBinding(header, version);
                    return true;
                }

                WsdlNS.SoapFaultBinding fault = src as WsdlNS.SoapFaultBinding;
                if (fault != null)
                {
                    src = ConvertSoapFaultBinding(fault, version);
                    return true;
                }

                XmlElement element = src as XmlElement;
                if (element != null)
                {
                    if (IsSoapFaultBinding(element))
                    {
                        src = ConvertSoapFaultBinding(element, version);
                        return true;
                    }
                }

                return src == null; // "match" only if nothing passed in
            }

            static WsdlNS.SoapBodyBinding ConvertSoapBodyBinding(WsdlNS.SoapBodyBinding src, EnvelopeVersion version)
            {
                if (version == EnvelopeVersion.None)
                    return null;

                EnvelopeVersion srcVersion = GetBindingVersion<WsdlNS.Soap12BodyBinding>(src);
                if (srcVersion == version)
                    return src;

                WsdlNS.SoapBodyBinding dest = version == EnvelopeVersion.Soap12 ? new WsdlNS.Soap12BodyBinding() : new WsdlNS.SoapBodyBinding();
                if (src != null)
                {
                    if (XmlSerializerOperationFormatter.GetEncoding(srcVersion) == src.Encoding)
                        dest.Encoding = XmlSerializerOperationFormatter.GetEncoding(version);
                    dest.Encoding = XmlSerializerOperationFormatter.GetEncoding(version);
                    dest.Namespace = src.Namespace;
                    dest.Parts = src.Parts;
                    dest.PartsString = src.PartsString;
                    dest.Use = src.Use;
                    dest.Required = src.Required;
                }
                return dest;
            }

            static XmlElement ConvertSoapFaultBinding(XmlElement src, EnvelopeVersion version)
            {
                if (src == null)
                    return null;

                if (version == EnvelopeVersion.Soap12)
                {
                    if (src.NamespaceURI == WsdlNS.Soap12Binding.Namespace)
                        return src;
                }
                else if (version == EnvelopeVersion.Soap11)
                {
                    if (src.NamespaceURI == WsdlNS.SoapBinding.Namespace)
                        return src;
                }
                else
                {
                    return null;
                }

                XmlElement dest = CreateSoapFaultBinding(version);
                if (src.HasAttributes)
                {
                    foreach (XmlAttribute attribute in src.Attributes)
                    {
                        dest.SetAttribute(attribute.Name, attribute.Value);
                    }
                }
                return dest;
            }

            static WsdlNS.SoapFaultBinding ConvertSoapFaultBinding(WsdlNS.SoapFaultBinding src, EnvelopeVersion version)
            {
                if (version == EnvelopeVersion.None)
                    return null;

                if (GetBindingVersion<WsdlNS.Soap12FaultBinding>(src) == version)
                    return src;

                WsdlNS.SoapFaultBinding dest = version == EnvelopeVersion.Soap12 ? new WsdlNS.Soap12FaultBinding() : new WsdlNS.SoapFaultBinding();
                if (src != null)
                {
                    dest.Encoding = src.Encoding;
                    dest.Name = src.Name;
                    dest.Namespace = src.Namespace;
                    dest.Use = src.Use;
                    dest.Required = src.Required;
                }
                return dest;
            }

            static WsdlNS.SoapHeaderBinding ConvertSoapHeaderBinding(WsdlNS.SoapHeaderBinding src, EnvelopeVersion version)
            {
                if (version == EnvelopeVersion.None)
                    return null;

                if (GetBindingVersion<WsdlNS.Soap12HeaderBinding>(src) == version)
                    return src;

                WsdlNS.SoapHeaderBinding dest = version == EnvelopeVersion.Soap12 ? new WsdlNS.Soap12HeaderBinding() : new WsdlNS.SoapHeaderBinding();
                if (src != null)
                {
                    dest.Fault = src.Fault;
                    dest.MapToProperty = src.MapToProperty;
                    dest.Message = src.Message;
                    dest.Part = src.Part;
                    dest.Encoding = src.Encoding;
                    dest.Namespace = src.Namespace;
                    dest.Use = src.Use;
                    dest.Required = src.Required;
                }
                return dest;
            }

            internal static EnvelopeVersion GetBindingVersion<T12>(object src)
            {
                return src is T12 ? EnvelopeVersion.Soap12 : EnvelopeVersion.Soap11;
            }

        }

        static WsdlNS.SoapAddressBinding CreateSoapAddressBinding(EnvelopeVersion version, WsdlNS.Port wsdlPort)
        {
            WsdlNS.SoapAddressBinding soapAddressBinding = null;

            if (version == EnvelopeVersion.Soap12)
            {
                soapAddressBinding = new WsdlNS.Soap12AddressBinding();
            }
            else if (version == EnvelopeVersion.Soap11)
            {
                soapAddressBinding = new WsdlNS.SoapAddressBinding();
            }
            Fx.Assert(soapAddressBinding != null, "EnvelopeVersion is not recognized. Please update the SoapHelper class");

            wsdlPort.Extensions.Add(soapAddressBinding);
            return soapAddressBinding;
        }

        // 
        static WsdlNS.SoapBinding CreateSoapBinding(EnvelopeVersion version, WsdlNS.Binding wsdlBinding)
        {
            WsdlNS.SoapBinding soapBinding = null;

            if (version == EnvelopeVersion.Soap12)
            {
                soapBinding = new WsdlNS.Soap12Binding();
            }
            else if (version == EnvelopeVersion.Soap11)
            {
                soapBinding = new WsdlNS.SoapBinding();
            }
            Fx.Assert(soapBinding != null, "EnvelopeVersion is not recognized. Please update the SoapHelper class");

            wsdlBinding.Extensions.Add(soapBinding);
            return soapBinding;
        }

        static WsdlNS.SoapOperationBinding CreateSoapOperationBinding(EnvelopeVersion version, WsdlNS.OperationBinding wsdlOperationBinding)
        {
            WsdlNS.SoapOperationBinding soapOperationBinding = null;

            if (version == EnvelopeVersion.Soap12)
            {
                soapOperationBinding = new WsdlNS.Soap12OperationBinding();
            }
            else if (version == EnvelopeVersion.Soap11)
            {
                soapOperationBinding = new WsdlNS.SoapOperationBinding();
            }
            Fx.Assert(soapOperationBinding != null, "EnvelopeVersion is not recognized. Please update the SoapHelper class");

            wsdlOperationBinding.Extensions.Add(soapOperationBinding);
            return soapOperationBinding;
        }

        static WsdlNS.SoapBodyBinding CreateSoapBodyBinding(EnvelopeVersion version, WsdlNS.MessageBinding wsdlMessageBinding)
        {
            WsdlNS.SoapBodyBinding soapBodyBinding = null;

            if (version == EnvelopeVersion.Soap12)
            {
                soapBodyBinding = new WsdlNS.Soap12BodyBinding();
            }
            else if (version == EnvelopeVersion.Soap11)
            {
                soapBodyBinding = new WsdlNS.SoapBodyBinding();
            }
            Fx.Assert(soapBodyBinding != null, "EnvelopeVersion is not recognized. Please update the SoapHelper class");

            wsdlMessageBinding.Extensions.Add(soapBodyBinding);
            return soapBodyBinding;
        }

        static WsdlNS.SoapHeaderBinding CreateSoapHeaderBinding(EnvelopeVersion version, WsdlNS.MessageBinding wsdlMessageBinding)
        {
            WsdlNS.SoapHeaderBinding soapHeaderBinding = null;

            if (version == EnvelopeVersion.Soap12)
            {
                soapHeaderBinding = new WsdlNS.Soap12HeaderBinding();
            }
            else if (version == EnvelopeVersion.Soap11)
            {
                soapHeaderBinding = new WsdlNS.SoapHeaderBinding();
            }
            Fx.Assert(soapHeaderBinding != null, "EnvelopeVersion is not recognized. Please update the SoapHelper class");

            wsdlMessageBinding.Extensions.Add(soapHeaderBinding);
            return soapHeaderBinding;
        }

        static XmlElement CreateSoapFaultBinding(EnvelopeVersion version)
        {
            string prefix = null;
            string ns = null;
            if (version == EnvelopeVersion.Soap12)
            {
                ns = WsdlNS.Soap12Binding.Namespace;
                prefix = "soap12";
            }
            else if (version == EnvelopeVersion.Soap11)
            {
                ns = WsdlNS.SoapBinding.Namespace;
                prefix = "soap";
            }
            Fx.Assert(ns != null, "EnvelopeVersion is not recognized. Please update the SoapHelper class");
            return Document.CreateElement(prefix, "fault", ns);
        }


        internal static WsdlNS.SoapAddressBinding GetSoapAddressBinding(WsdlNS.Port wsdlPort)
        {
            foreach (object o in wsdlPort.Extensions)
            {
                if (o is WsdlNS.SoapAddressBinding)
                    return (WsdlNS.SoapAddressBinding)o;
            }
            return null;
        }
        static WsdlNS.SoapBinding GetSoapBinding(WsdlEndpointConversionContext endpointContext)
        {
            foreach (object o in endpointContext.WsdlBinding.Extensions)
            {
                if (o is WsdlNS.SoapBinding)
                    return (WsdlNS.SoapBinding)o;
            }
            return null;
        }

        static WsdlNS.SoapOperationBinding GetSoapOperationBinding(WsdlEndpointConversionContext endpointContext, OperationDescription operation)
        {
            WsdlNS.OperationBinding wsdlOperationBinding = endpointContext.GetOperationBinding(operation);

            foreach (object o in wsdlOperationBinding.Extensions)
            {
                if (o is WsdlNS.SoapOperationBinding)
                    return (WsdlNS.SoapOperationBinding)o;
            }
            return null;
        }

        static WsdlNS.SoapBodyBinding GetSoapBodyBinding(WsdlEndpointConversionContext endpointContext, WsdlNS.MessageBinding wsdlMessageBinding)
        {
            foreach (object o in wsdlMessageBinding.Extensions)
            {
                if (o is WsdlNS.SoapBodyBinding)
                    return (WsdlNS.SoapBodyBinding)o;
            }
            return null;
        }

        internal static string ReadSoapAction(WsdlNS.OperationBinding wsdlOperationBinding)
        {
            WsdlNS.SoapOperationBinding soapOperationBinding = (WsdlNS.SoapOperationBinding)wsdlOperationBinding.Extensions.Find(typeof(WsdlNS.SoapOperationBinding));
            return soapOperationBinding != null ? soapOperationBinding.SoapAction : null;
        }

        internal static WsdlNS.SoapBindingStyle GetStyle(WsdlNS.Binding binding)
        {
            WsdlNS.SoapBindingStyle style = WsdlNS.SoapBindingStyle.Default;
            if (binding != null)
            {
                WsdlNS.SoapBinding soapBinding = binding.Extensions.Find(typeof(WsdlNS.SoapBinding)) as WsdlNS.SoapBinding;
                if (soapBinding != null)
                    style = soapBinding.Style;
            }
            return style;
        }

        internal static WsdlNS.SoapBindingStyle GetStyle(WsdlNS.OperationBinding operationBinding, WsdlNS.SoapBindingStyle defaultBindingStyle)
        {
            WsdlNS.SoapBindingStyle style = defaultBindingStyle;
            if (operationBinding != null)
            {
                WsdlNS.SoapOperationBinding soapOperationBinding = operationBinding.Extensions.Find(typeof(WsdlNS.SoapOperationBinding)) as WsdlNS.SoapOperationBinding;
                if (soapOperationBinding != null)
                {
                    if (soapOperationBinding.Style != WsdlNS.SoapBindingStyle.Default)
                        style = soapOperationBinding.Style;
                }
            }
            return style;
        }

        internal static bool IsSoapFaultBinding(XmlElement element)
        {
            return (element != null && element.LocalName == "fault" && (element.NamespaceURI == WsdlNS.Soap12Binding.Namespace || element.NamespaceURI == WsdlNS.SoapBinding.Namespace));
        }

        internal static bool IsEncoded(XmlElement element)
        {
            Fx.Assert(element != null, "");
            XmlAttribute attribute = element.GetAttributeNode("use");
            if (attribute == null)
                return false;
            return attribute.Value == "encoded";
        }
    }
}
