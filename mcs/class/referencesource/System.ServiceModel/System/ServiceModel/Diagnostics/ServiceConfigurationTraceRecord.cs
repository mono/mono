//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Diagnostics
{
    using System.Globalization;
    using System.Runtime.Diagnostics;
    using System.ServiceModel.Configuration;
    using System.Xml;
    
    class ServiceConfigurationTraceRecord : TraceRecord
    {
        ServiceElement serviceElement;

        internal ServiceConfigurationTraceRecord(ServiceElement serviceElement)
        {
            this.serviceElement = serviceElement;
        }

        internal override string EventId { get { return BuildEventId("ServiceConfiguration"); } }

        internal override void WriteTo(XmlWriter xml)
        {
            xml.WriteElementString("FoundServiceElement", ((bool)(this.serviceElement != null)).ToString(CultureInfo.InvariantCulture)); 
            if (this.serviceElement != null)
            {
                if (!string.IsNullOrEmpty(this.serviceElement.ElementInformation.Source))
                {
                    xml.WriteElementString("ConfigurationFileSource", this.serviceElement.ElementInformation.Source);
                    xml.WriteElementString("ConfigurationFileLineNumber", this.serviceElement.ElementInformation.LineNumber.ToString(CultureInfo.InvariantCulture));
                }
                xml.WriteStartElement("ServiceConfigurationInformation");
                    this.WriteElementString("ServiceName", this.serviceElement.Name, xml);
                    this.WriteElementString("BehaviorConfiguration", this.serviceElement.BehaviorConfiguration, xml);
                    xml.WriteStartElement("Host");
                        xml.WriteStartElement("Timeouts");
                            xml.WriteElementString("OpenTimeout", this.serviceElement.Host.Timeouts.OpenTimeout.ToString());
                            xml.WriteElementString("CloseTimeout", this.serviceElement.Host.Timeouts.CloseTimeout.ToString());
                        xml.WriteEndElement();
                        if (this.serviceElement.Host.BaseAddresses.Count > 0)
                        {
                            xml.WriteStartElement("BaseAddresses");
                            foreach (BaseAddressElement baseAddress in this.serviceElement.Host.BaseAddresses)
                            {
                                this.WriteElementString("BaseAddress", baseAddress.BaseAddress, xml);
                            }
                            xml.WriteEndElement();
                        }
                    xml.WriteEndElement();
                    xml.WriteStartElement("Endpoints");
                        foreach (ServiceEndpointElement serviceEndpoint in this.serviceElement.Endpoints)
                        {
                            xml.WriteStartElement("Endpoint");
                                if (serviceEndpoint.Address != null)
                                {
                                    this.WriteElementString("Address", serviceEndpoint.Address.ToString(), xml);
                                }
                                this.WriteElementString("Binding", serviceEndpoint.Binding, xml);
                                this.WriteElementString("BindingConfiguration", serviceEndpoint.BindingConfiguration, xml);
                                this.WriteElementString("BindingName", serviceEndpoint.BindingName, xml);
                                this.WriteElementString("BindingNamespace", serviceEndpoint.BindingNamespace, xml);
                                this.WriteElementString("Contract", serviceEndpoint.Contract, xml);
                                if (serviceEndpoint.ListenUri != null)
                                {
                                    xml.WriteElementString("ListenUri", serviceEndpoint.ListenUri.ToString());
                                }
                                xml.WriteElementString("ListenUriMode", serviceEndpoint.ListenUriMode.ToString());
                                this.WriteElementString("Name", serviceEndpoint.Name, xml);
                            xml.WriteEndElement();
                        }
                    xml.WriteEndElement();
                xml.WriteEndElement();
            }
        }

        void WriteElementString(string name, string value, XmlWriter xml)
        {
            if (!string.IsNullOrEmpty(value))
            {
                xml.WriteElementString(name, value);
            }
        }
    }
}
