//------------------------------------------------------------------------------
// <copyright file="Soap12ProtocolReflector.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Services.Description {

    using System.Web.Services;
    using System.Web.Services.Protocols;
    using System.Xml;
    using System.Xml.Serialization;
    using System.Xml.Schema;
    using System.Collections;
    using System;
    using System.Reflection;
    using System.Web.Services.Configuration;

    internal class Soap12ProtocolReflector : SoapProtocolReflector {
        Hashtable requestElements;
        Hashtable actions;
        XmlQualifiedName soap11PortType;

        internal override WsiProfiles ConformsTo {
            get { return WsiProfiles.None; }
        }

        public override string ProtocolName {
            get { return "Soap12"; }
        }

        protected override void BeginClass() {
            requestElements = new Hashtable();
            actions = new Hashtable();
            soap11PortType = null;
            
            base.BeginClass();
        }

        protected override bool ReflectMethod() {
            if (base.ReflectMethod()) {
                if (Binding != null) {
                    // SoapMethod.portType tracks the first portType created for this method
                    // we want to make sure there's only one portType and set of messages per method
                    // so we delete ours if an existing portType already exists for this method.
                    soap11PortType = SoapMethod.portType;
                    if (soap11PortType != Binding.Type)
                        HeaderMessages.Clear();
                }
                return true;
            }
            return false;
        }

        protected override void EndClass() {
            if (PortType == null || Binding == null) return; // external binding;
            
            if (soap11PortType != null && soap11PortType != Binding.Type) {
                // we want to share soap 1.1's portType and messages so we delete ours and reference theirs
                foreach (Operation op in PortType.Operations) {
                    foreach (OperationMessage msg in op.Messages) {
                        ServiceDescription sd = GetServiceDescription(msg.Message.Namespace);
                        if (sd != null) {
                            Message m = sd.Messages[msg.Message.Name];
                            if (m != null)
                                sd.Messages.Remove(m);
                        }
                    }
                }

                Binding.Type = soap11PortType;
                PortType.ServiceDescription.PortTypes.Remove(PortType);
            }
        }

        protected override SoapBinding CreateSoapBinding(SoapBindingStyle style) {
            Soap12Binding soapBinding = new Soap12Binding();
            soapBinding.Transport = Soap12Binding.HttpTransport;
            soapBinding.Style = style;
            return soapBinding;
        }

        protected override SoapAddressBinding CreateSoapAddressBinding(string serviceUrl) {
            Soap12AddressBinding soapAddress = new Soap12AddressBinding();
            soapAddress.Location = serviceUrl;
            if (this.UriFixups != null)
            {
                this.UriFixups.Add(delegate(Uri current)
                {
                    soapAddress.Location = DiscoveryServerType.CombineUris(current, soapAddress.Location);
                });
            }
            return soapAddress;
        }

        protected override SoapOperationBinding CreateSoapOperationBinding(SoapBindingStyle style, string action) {
            Soap12OperationBinding soapOperation = new Soap12OperationBinding();
            soapOperation.SoapAction = action;
            soapOperation.Style = style;
            soapOperation.Method = SoapMethod;

            DealWithAmbiguity(action, SoapMethod.requestElementName.ToString(), soapOperation);
            
            return soapOperation;
        }

        protected override SoapBodyBinding CreateSoapBodyBinding(SoapBindingUse use, string ns) {
            Soap12BodyBinding soapBodyBinding = new Soap12BodyBinding();
            soapBodyBinding.Use = use;
            if (use == SoapBindingUse.Encoded)
                soapBodyBinding.Encoding = Soap12.Encoding;
            soapBodyBinding.Namespace = ns;
            return soapBodyBinding;
        }

        protected override SoapHeaderBinding CreateSoapHeaderBinding(XmlQualifiedName message, string partName, SoapBindingUse use) {
            return CreateSoapHeaderBinding(message, partName, null, use);
        }

        protected override SoapHeaderBinding CreateSoapHeaderBinding(XmlQualifiedName message, string partName, string ns, SoapBindingUse use) {
            Soap12HeaderBinding soapHeaderBinding = new Soap12HeaderBinding();
            soapHeaderBinding.Message = message;
            soapHeaderBinding.Part = partName;
            soapHeaderBinding.Namespace = ns;
            soapHeaderBinding.Use = use;
            if (use == SoapBindingUse.Encoded)
                soapHeaderBinding.Encoding = Soap12.Encoding;
            return soapHeaderBinding;
        }

        private void DealWithAmbiguity(string action, string requestElement, Soap12OperationBinding operation) {

            Soap12OperationBinding duplicateActionOperation = (Soap12OperationBinding)actions[action];
            if (duplicateActionOperation != null) {
                operation.DuplicateBySoapAction = duplicateActionOperation;
                duplicateActionOperation.DuplicateBySoapAction = operation;
                CheckOperationDuplicates(duplicateActionOperation);
            }
            else
                actions[action] = operation;

            Soap12OperationBinding duplicateRequestElementOperation = (Soap12OperationBinding)requestElements[requestElement];
            if (duplicateRequestElementOperation != null) {
                operation.DuplicateByRequestElement = duplicateRequestElementOperation;
                duplicateRequestElementOperation.DuplicateByRequestElement = operation;
                CheckOperationDuplicates(duplicateRequestElementOperation);
            }
            else
                requestElements[requestElement] = operation;

            CheckOperationDuplicates(operation);
        }

        private void CheckOperationDuplicates(Soap12OperationBinding operation) {
            // we require soap action if we can't route on request element 
            if (operation.DuplicateByRequestElement != null) {
                // except if we also can't route on soap action, which is an error
                if (operation.DuplicateBySoapAction != null)
                    throw new InvalidOperationException(Res.GetString(Res.TheMethodsAndUseTheSameRequestElementAndSoapActionXmlns6, operation.Method.name, operation.DuplicateByRequestElement.Method.name, operation.Method.requestElementName.Name, operation.Method.requestElementName.Namespace, operation.DuplicateBySoapAction.Method.name, operation.Method.action));
                else
                    operation.SoapActionRequired = true;
            }
            else
                operation.SoapActionRequired = false;
        }
    }
}
