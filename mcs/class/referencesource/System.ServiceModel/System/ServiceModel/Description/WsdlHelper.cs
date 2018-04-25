// <copyright>
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace System.ServiceModel.Description
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Runtime;
    using System.Text;
    using System.Xml;
    using System.Xml.Schema;
    using System.Xml.Serialization;
    using WsdlNS = System.Web.Services.Description;

    internal static class WsdlHelper
    {
        public static WsdlNS.ServiceDescription GetSingleWsdl(MetadataSet metadataSet)
        {
            if (metadataSet.MetadataSections.Count < 1)
            {
                return null;
            }

            List<WsdlNS.ServiceDescription> wsdls = new List<WsdlNS.ServiceDescription>();
            List<XmlSchema> xsds = new List<XmlSchema>();

            foreach (MetadataSection section in metadataSet.MetadataSections)
            {
                if (section.Metadata is WsdlNS.ServiceDescription)
                {
                    wsdls.Add((WsdlNS.ServiceDescription)section.Metadata);
                }

                if (section.Metadata is XmlSchema)
                {
                    xsds.Add((XmlSchema)section.Metadata);
                }
            }

            VerifyContractNamespace(wsdls);
            WsdlNS.ServiceDescription singleWsdl = GetSingleWsdl(CopyServiceDescriptionCollection(wsdls));

            // Inline XML schemas
            foreach (XmlSchema schema in xsds)
            {
                XmlSchema newSchema = CloneXsd(schema);
                RemoveSchemaLocations(newSchema);
                singleWsdl.Types.Schemas.Add(newSchema);
            }

            return singleWsdl;
        }

        private static void RemoveSchemaLocations(XmlSchema schema)
        {
            foreach (XmlSchemaObject schemaObject in schema.Includes)
            {
                XmlSchemaExternal external = schemaObject as XmlSchemaExternal;
                if (external != null)
                {
                    external.SchemaLocation = null;
                }
            }
        }

        private static WsdlNS.ServiceDescription GetSingleWsdl(List<WsdlNS.ServiceDescription> wsdls)
        {
            // Use WSDL that has the contracts as the base for single WSDL
            WsdlNS.ServiceDescription singleWsdl = wsdls.First(wsdl => wsdl.PortTypes.Count > 0);
            if (singleWsdl == null)
            {
                singleWsdl = new WsdlNS.ServiceDescription();
            }
            else
            {
                singleWsdl.Types.Schemas.Clear();
                singleWsdl.Imports.Clear();
            }

            Dictionary<XmlQualifiedName, XmlQualifiedName> bindingReferenceChanges = new Dictionary<XmlQualifiedName, XmlQualifiedName>();
            foreach (WsdlNS.ServiceDescription wsdl in wsdls)
            {
                if (wsdl != singleWsdl)
                {
                    MergeWsdl(singleWsdl, wsdl, bindingReferenceChanges);
                }              
            }

            EnsureSingleNamespace(singleWsdl, bindingReferenceChanges);
            return singleWsdl;
        }

        private static List<WsdlNS.ServiceDescription> CopyServiceDescriptionCollection(List<WsdlNS.ServiceDescription> wsdls)
        {
            List<WsdlNS.ServiceDescription> newWsdls = new List<WsdlNS.ServiceDescription>();
            foreach (WsdlNS.ServiceDescription wsdl in wsdls)
            {
                newWsdls.Add(CloneWsdl(wsdl));
            }

            return newWsdls;
        }

        private static void MergeWsdl(WsdlNS.ServiceDescription singleWsdl, WsdlNS.ServiceDescription wsdl, Dictionary<XmlQualifiedName, XmlQualifiedName> bindingReferenceChanges)
        {
            if (wsdl.Services.Count > 0)
            {
                singleWsdl.Name = wsdl.Name;
            }

            foreach (WsdlNS.Binding binding in wsdl.Bindings)
            {
                string uniqueBindingName = NamingHelper.GetUniqueName(binding.Name, WsdlHelper.IsBindingNameUsed, singleWsdl.Bindings);
                if (binding.Name != uniqueBindingName)
                {
                    bindingReferenceChanges.Add(
                        new XmlQualifiedName(binding.Name, binding.ServiceDescription.TargetNamespace),
                        new XmlQualifiedName(uniqueBindingName, singleWsdl.TargetNamespace));
                    UpdatePolicyKeys(binding, uniqueBindingName, wsdl);
                    binding.Name = uniqueBindingName;
                }

                singleWsdl.Bindings.Add(binding);
            }

            foreach (object extension in wsdl.Extensions)
            {
                singleWsdl.Extensions.Add(extension);
            }

            foreach (WsdlNS.Message message in wsdl.Messages)
            {
                singleWsdl.Messages.Add(message);
            }

            foreach (WsdlNS.Service service in wsdl.Services)
            {
                singleWsdl.Services.Add(service);
            }

            foreach (string warning in wsdl.ValidationWarnings)
            {
                singleWsdl.ValidationWarnings.Add(warning);
            }
        }

        private static void UpdatePolicyKeys(WsdlNS.Binding binding, string newBindingName, WsdlNS.ServiceDescription wsdl)
        {
            string oldBindingName = binding.Name;

            // policy
            IEnumerable<XmlElement> bindingPolicies = FindAllElements(wsdl.Extensions, MetadataStrings.WSPolicy.Elements.Policy);
            string policyIdStringPrefixFormat = "{0}_";
            foreach (XmlElement policyElement in bindingPolicies)
            {
                XmlNode policyId = policyElement.Attributes.GetNamedItem(MetadataStrings.Wsu.Attributes.Id, MetadataStrings.Wsu.NamespaceUri);
                string policyIdString = policyId.Value;
                string policyIdStringWithOldBindingName = string.Format(CultureInfo.InvariantCulture, policyIdStringPrefixFormat, oldBindingName);
                string policyIdStringWithNewBindingName = string.Format(CultureInfo.InvariantCulture, policyIdStringPrefixFormat, newBindingName);
                if (policyId != null && policyIdString != null && policyIdString.StartsWith(policyIdStringWithOldBindingName, StringComparison.Ordinal))
                {
                    policyId.Value = policyIdStringWithNewBindingName + policyIdString.Substring(policyIdStringWithOldBindingName.Length);
                }
            }

            // policy reference
            UpdatePolicyReference(binding.Extensions, oldBindingName, newBindingName);
            foreach (WsdlNS.OperationBinding operationBinding in binding.Operations)
            {
                UpdatePolicyReference(operationBinding.Extensions, oldBindingName, newBindingName);
                if (operationBinding.Input != null)
                {
                    UpdatePolicyReference(operationBinding.Input.Extensions, oldBindingName, newBindingName);
                }

                if (operationBinding.Output != null)
                {
                    UpdatePolicyReference(operationBinding.Output.Extensions, oldBindingName, newBindingName);
                }

                foreach (WsdlNS.FaultBinding fault in operationBinding.Faults)
                {
                    UpdatePolicyReference(fault.Extensions, oldBindingName, newBindingName);
                }
            }
        }

        private static void UpdatePolicyReference(WsdlNS.ServiceDescriptionFormatExtensionCollection extensions, string oldBindingName, string newBindingName)
        {
            IEnumerable<XmlElement> bindingPolicyReferences = FindAllElements(extensions, MetadataStrings.WSPolicy.Elements.PolicyReference);
            string policyReferencePrefixFormat = "#{0}_";
            foreach (XmlElement policyReferenceElement in bindingPolicyReferences)
            {
                XmlNode policyReference = policyReferenceElement.Attributes.GetNamedItem(MetadataStrings.WSPolicy.Attributes.URI);
                string policyReferenceValue = policyReference.Value;
                string policyReferenceValueWithOldBindingName = string.Format(CultureInfo.InvariantCulture, policyReferencePrefixFormat, oldBindingName);
                string policyReferenceValueWithNewBindingName = string.Format(CultureInfo.InvariantCulture, policyReferencePrefixFormat, newBindingName);
                if (policyReference != null && policyReferenceValue != null && policyReferenceValue.StartsWith(policyReferenceValueWithOldBindingName, StringComparison.Ordinal))
                {
                    policyReference.Value = policyReferenceValueWithNewBindingName + policyReference.Value.Substring(policyReferenceValueWithOldBindingName.Length);
                }
            }
        }

        private static IEnumerable<XmlElement> FindAllElements(WsdlNS.ServiceDescriptionFormatExtensionCollection extensions, string elementName)
        {
            List<XmlElement> policyReferences = new List<XmlElement>();
            for (int i = 0; i < extensions.Count; i++)
            {
                XmlElement element = extensions[i] as XmlElement;
                if (element != null && element.LocalName == elementName)
                {
                    policyReferences.Add(element);
                }
            }

            return policyReferences;
        }

        private static void VerifyContractNamespace(List<WsdlNS.ServiceDescription> wsdls)
        {
            IEnumerable<WsdlNS.ServiceDescription> contractWsdls = wsdls.Where(serviceDescription => serviceDescription.PortTypes.Count > 0);
            if (contractWsdls.Count() > 1)
            {
                IEnumerable<string> namespaces = contractWsdls.Select<WsdlNS.ServiceDescription, string>(wsdl => wsdl.TargetNamespace);
                string contractNamespaces = string.Join(", ", namespaces);
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.SingleWsdlNotGenerated, contractNamespaces)));
            }
        }

        private static void EnsureSingleNamespace(WsdlNS.ServiceDescription wsdl, Dictionary<XmlQualifiedName, XmlQualifiedName> bindingReferenceChanges)
        {
            string targetNamespace = wsdl.TargetNamespace;
            foreach (WsdlNS.Binding binding in wsdl.Bindings)
            {
                if (binding.Type.Namespace != targetNamespace)
                {
                    binding.Type = new XmlQualifiedName(binding.Type.Name, targetNamespace);
                }
            }

            foreach (WsdlNS.PortType portType in wsdl.PortTypes)
            {
                foreach (WsdlNS.Operation operation in portType.Operations)
                {
                    WsdlNS.OperationInput messageInput = operation.Messages.Input;
                    if (messageInput != null && messageInput.Message.Namespace != targetNamespace)
                    {
                        messageInput.Message = new XmlQualifiedName(messageInput.Message.Name, targetNamespace);
                    }

                    WsdlNS.OperationOutput messageOutput = operation.Messages.Output;
                    if (messageOutput != null && messageOutput.Message.Namespace != targetNamespace)
                    {
                        messageOutput.Message = new XmlQualifiedName(messageOutput.Message.Name, targetNamespace);
                    }

                    foreach (WsdlNS.OperationFault fault in operation.Faults)
                    {
                        if (fault.Message.Namespace != targetNamespace)
                        {
                            fault.Message = new XmlQualifiedName(fault.Message.Name, targetNamespace);
                        }
                    }
                }
            }

            foreach (WsdlNS.Service service in wsdl.Services)
            {
                foreach (WsdlNS.Port port in service.Ports)
                {
                    XmlQualifiedName newPortBinding;
                    if (bindingReferenceChanges.TryGetValue(port.Binding, out newPortBinding))
                    {
                        port.Binding = newPortBinding;
                    }
                    else if (port.Binding.Namespace != targetNamespace)
                    {
                        port.Binding = new XmlQualifiedName(port.Binding.Name, targetNamespace);
                    }
                }
            }
        }

        private static bool IsBindingNameUsed(string name, object collection)
        {
            WsdlNS.BindingCollection bindings = (WsdlNS.BindingCollection)collection;
            foreach (WsdlNS.Binding binding in bindings)
            {
                if (binding.Name == name)
                {
                    return true;
                }
            }

            return false;
        }

        private static WsdlNS.ServiceDescription CloneWsdl(WsdlNS.ServiceDescription originalWsdl)
        {
            Fx.Assert(originalWsdl != null, "originalWsdl must not be null");
            WsdlNS.ServiceDescription newWsdl;
            using (MemoryStream memoryStream = new MemoryStream())
            {
                originalWsdl.Write(memoryStream);
                memoryStream.Seek(0, SeekOrigin.Begin);
                newWsdl = WsdlNS.ServiceDescription.Read(memoryStream);
            }

            return newWsdl;
        }

        [SuppressMessage("Microsoft.Security.Xml", "CA3054:DoNotAllowDtdOnXmlTextReader")]
        [SuppressMessage("Microsoft.Security.Xml", "CA3069:ReviewDtdProcessingAssignment", Justification = "This is trusted server code from the application only. We should allow the customer add dtd.")]
        private static XmlSchema CloneXsd(XmlSchema originalXsd)
        {
            Fx.Assert(originalXsd != null, "originalXsd must not be null");
            XmlSchema newXsd;
            using (MemoryStream memoryStream = new MemoryStream())
            {
                originalXsd.Write(memoryStream);
                memoryStream.Seek(0, SeekOrigin.Begin);
                newXsd = XmlSchema.Read(new XmlTextReader(memoryStream) { DtdProcessing = DtdProcessing.Parse }, null);
            }

            return newXsd;
        }
    }
}
