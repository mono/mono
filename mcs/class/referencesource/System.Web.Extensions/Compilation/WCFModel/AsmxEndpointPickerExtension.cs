#region Copyright (c) Microsoft Corporation
/// <copyright company='Microsoft Corporation'>
///    Copyright (c) Microsoft Corporation. All Rights Reserved.
///    Information Contained Herein is Proprietary and Confidential.
/// </copyright>
#endregion

using System;
using System.Collections.Generic;
using System.Security;
using System.Security.Permissions;
using System.Web.Services.Description;

#if WEB_EXTENSIONS_CODE
namespace System.Web.Compilation.WCFModel
#else
namespace Microsoft.VSDesigner.WCFModel
#endif
{
    /// <summary>
    /// Wsdl import extension to remove the soap1.2 endpoint for ASMX services.
    /// By default, ASMX services expose two endpoints, soap & soap1.2. In order 
    /// to have easy-of-use-parity with VS2005 ASMX web service consumption
    /// we remove one of the endpoints for this special case.
    /// </summary>
    [SecurityCritical]
    [PermissionSet(SecurityAction.InheritanceDemand, Name = "FullTrust")]
    internal class AsmxEndpointPickerExtension : System.ServiceModel.Description.IWsdlImportExtension
    {
        [SecuritySafeCritical]
        void System.ServiceModel.Description.IWsdlImportExtension.ImportContract(System.ServiceModel.Description.WsdlImporter importer, System.ServiceModel.Description.WsdlContractConversionContext context)
        {
            // We don't really care...
        }

        [SecuritySafeCritical]
        void System.ServiceModel.Description.IWsdlImportExtension.ImportEndpoint(System.ServiceModel.Description.WsdlImporter importer, System.ServiceModel.Description.WsdlEndpointConversionContext context)
        {
            // We don't really care...
        }

        /// <summary>
        /// Remove the Soap1.2 endpoint for ASMX web services if the service exposes
        /// both a soap and a  soap 1.2 endpoint
        /// </summary>
        /// <param name="wsdlDocuments">WSDL documents to modify</param>
        /// <param name="xmlSchemas">Ignored</param>
        /// <param name="policy">Ignored</param>
        [SecuritySafeCritical]
        void System.ServiceModel.Description.IWsdlImportExtension.BeforeImport(System.Web.Services.Description.ServiceDescriptionCollection wsdlDocuments, System.Xml.Schema.XmlSchemaSet xmlSchemas, System.Collections.Generic.ICollection<System.Xml.XmlElement> policy)
        {
            if (wsdlDocuments == null)
            {
                throw new ArgumentNullException("wsdlDocuments");
            }

            foreach (ServiceDescription document in wsdlDocuments)
            {
                foreach (Service service in document.Services)
                {
                    // We only touch services that have exactly two endpoints
                    // (soap & soap 1.2)
                    if (service.Ports.Count != 2) continue;

                    Port portToDelete = null;

                    // Check both ports to see if they are a soap & soap 1.2 pair
                    if (IsSoapAsmxPort(typeof(SoapAddressBinding), service.Ports[0]) && IsSoapAsmxPort(typeof(Soap12AddressBinding), service.Ports[1]))
                    {
                        portToDelete = service.Ports[1];
                    }
                    else if (IsSoapAsmxPort(typeof(SoapAddressBinding), service.Ports[1]) && IsSoapAsmxPort(typeof(Soap12AddressBinding), service.Ports[0]))
                    {
                        portToDelete = service.Ports[0];
                    }

                    if (portToDelete != null)
                    {
                        service.Ports.Remove(portToDelete);

                        if (portToDelete.Binding != null)
                        {
                            // Find any associated bindings so that we can remove
                            // them as well...
                            List<Binding> bindingsToDelete = new List<Binding>();

                            foreach (Binding binding in document.Bindings)
                            {
                                if (String.Equals(binding.Name, portToDelete.Binding.Name, StringComparison.Ordinal))
                                {
                                    bindingsToDelete.Add(binding);
                                }
                            }

                            foreach (Binding bindingToDelete in bindingsToDelete)
                            {
                                document.Bindings.Remove(bindingToDelete);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Is the given port an ASMX endpoint with the given SOAP address type?
        /// </summary>
        /// <param name="addressType"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        private bool IsSoapAsmxPort(System.Type addressType, Port port)
        {
            SoapAddressBinding addressBinding = port.Extensions.Find(addressType) as SoapAddressBinding;
            if (addressBinding != null && addressBinding.GetType() == addressType && IsAsmxUri(addressBinding.Location))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Is the given location an URL that has a .asmx file extension
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
        private bool IsAsmxUri(string location)
        {
            Uri uri = null;

            // Invalid URI - that can't be an ASMX service...
            if (!System.Uri.TryCreate(location, UriKind.Absolute, out uri))
            {
                return false;
            }

            //  Check if the "filename" part of the URL has a .asmx file extension
            string[] segments = uri.Segments;

            if (segments.Length > 0)
            {
                try
                {
                    string fileName = segments[segments.Length - 1];
                    if (String.Equals(System.IO.Path.GetExtension(fileName), ".asmx", StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
                catch (System.ArgumentException)
                {
                    // This was most likely an invalid path... well, let's just treat this as if 
                    // this is not an ASMX endpoint...
                }
            }
            return false;
        }
    }
}
