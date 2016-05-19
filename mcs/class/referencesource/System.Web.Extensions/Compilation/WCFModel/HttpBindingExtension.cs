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
using System.ServiceModel.Description;
using System.Xml;
using System.Xml.Schema;
using WsdlNS = System.Web.Services.Description;

#if WEB_EXTENSIONS_CODE
namespace System.Web.Compilation.WCFModel
#else
namespace Microsoft.VSDesigner.WCFModel
#endif
{
    /// <summary>
    /// Wsdl import extension to remove HttpGet and HttpPost bindings for ASMX services.
    /// See detail in dev10 792007
    /// </summary>
    [SecurityCritical]
    [PermissionSet(SecurityAction.InheritanceDemand, Name = "FullTrust")]
    internal class HttpBindingExtension : IWsdlImportExtension
    {
        readonly HashSet<ContractDescription> httpBindingContracts = new HashSet<ContractDescription>();

        static bool ContainsHttpBindingExtension(WsdlNS.Binding wsdlBinding)
        {
            //avoiding using wsdlBinding.Extensions.Find(typeof(WsdlNS.HttpBinding)) so the extension won't be marked as handled
            foreach (object extension in wsdlBinding.Extensions)
            {
                if (extension is WsdlNS.HttpBinding)
                {
                    string httpVerb = ((WsdlNS.HttpBinding)extension).Verb;
                    if (httpVerb.Equals("GET", StringComparison.OrdinalIgnoreCase) || httpVerb.Equals("POST", StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public bool IsHttpBindingContract(ContractDescription contract)
        {
            return contract != null && httpBindingContracts.Contains(contract);
        }

        [SecuritySafeCritical]
        void IWsdlImportExtension.BeforeImport(WsdlNS.ServiceDescriptionCollection wsdlDocuments, XmlSchemaSet xmlSchemas, ICollection<XmlElement> policy)
        {
        }

        [SecuritySafeCritical]
        void IWsdlImportExtension.ImportContract(WsdlImporter importer, WsdlContractConversionContext context)
        {
        }

        [SecuritySafeCritical]
        void IWsdlImportExtension.ImportEndpoint(WsdlImporter importer, WsdlEndpointConversionContext context)
        {
            if (context != null && context.WsdlBinding != null && ContainsHttpBindingExtension(context.WsdlBinding))
            {
                httpBindingContracts.Add(context.ContractConversionContext.Contract);
            }
        }
    }
}
