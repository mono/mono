//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System.ServiceModel.Description;
    using System.Xml;

    static class UdpTransportBindingElementPolicy
    {
        static XmlDocument document;

        static XmlDocument Document
        {
            get
            {
                if (document == null)
                {
                    document = new XmlDocument();
                }

                return document;
            }
        }

        internal static void ExportRetransmissionEnabledAssertion(UdpTransportBindingElement bindingElement, PolicyAssertionCollection assertions)
        {
            if (bindingElement == null)
            {
                throw FxTrace.Exception.ArgumentNull("bindingElement");
            }

            if (assertions == null)
            {
                throw FxTrace.Exception.ArgumentNull("assertions");
            }

            if (bindingElement.RetransmissionSettings.Enabled)
            {
                XmlElement assertion = Document.CreateElement(UdpConstants.WsdlSoapUdpTransportPrefix, UdpConstants.RetransmissionEnabled, UdpConstants.WsdlSoapUdpTransportNamespace);
                assertions.Add(assertion);
            }
        }
    }
}
