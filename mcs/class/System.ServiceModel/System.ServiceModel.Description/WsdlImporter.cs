//
// WsdlImporter.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//	Ankit Jain <jankit@novell.com>	
//
// Copyright (C) 2005 Novell, Inc.  http://www.novell.com
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Web.Services.Description;
using System.Xml;
using System.Xml.Schema;

using SMBinding = System.ServiceModel.Channels.Binding;
using WSServiceDescription = System.Web.Services.Description.ServiceDescription;
using WSBinding = System.Web.Services.Description.Binding;
using WSMessage = System.Web.Services.Description.Message;
using QName = System.Xml.XmlQualifiedName;

namespace System.ServiceModel.Description
{
	[MonoTODO]
	public class WsdlImporter : MetadataImporter
	{
		ServiceDescriptionCollection wsdl_documents;
		XmlSchemaSet xmlschemas;
		List<XmlElement> policies; /* ?? */
		MetadataSet metadata;

		KeyedByTypeCollection<IWsdlImportExtension> wsdl_extensions;
			
		//Imported
		Collection<ContractDescription> contracts = null;
		ServiceEndpointCollection endpoint_colln = null;

		public WsdlImporter (
			MetadataSet metadata,
			IEnumerable<IPolicyImportExtension> policyImportExtensions,
			IEnumerable<IWsdlImportExtension> wsdlImportExtensions)
			: base (policyImportExtensions)
		{
			if (metadata == null)
				throw new ArgumentNullException ("metadata");
			
			if (wsdlImportExtensions == null) {
				wsdl_extensions = new KeyedByTypeCollection<IWsdlImportExtension> ();

				wsdl_extensions.Add (new DataContractSerializerMessageContractImporter ());
				wsdl_extensions.Add (new XmlSerializerMessageContractImporter ());
				//wsdl_extensions.Add (new MessageEncodingBindingElementImporter ());
				wsdl_extensions.Add (new TransportBindingElementImporter ());
				wsdl_extensions.Add (new StandardBindingImporter ());
			} else {
				wsdl_extensions = new KeyedByTypeCollection<IWsdlImportExtension> (wsdlImportExtensions);
			}

			// It is okay to fill these members immediately when WsdlImporter.ctor() is invoked
			// i.e. after this .ctor(), those metadata docs are not considered anymore.
			this.metadata = metadata;
			this.wsdl_documents = new ServiceDescriptionCollection ();
			this.xmlschemas = new XmlSchemaSet ();
			this.policies = new List<XmlElement> ();

			foreach (MetadataSection ms in metadata.MetadataSections) {
				if (ms.Dialect == MetadataSection.ServiceDescriptionDialect &&
					ms.Metadata.GetType () == typeof (WSServiceDescription))
					wsdl_documents.Add ((WSServiceDescription) ms.Metadata);
				else
				if (ms.Dialect == MetadataSection.XmlSchemaDialect &&
					ms.Metadata.GetType () == typeof (XmlSchema))
					xmlschemas.Add ((XmlSchema) ms.Metadata);
			}
		}

		public WsdlImporter (MetadataSet metadata)
			: this (metadata, null, null)
		{
		}

		public ServiceDescriptionCollection WsdlDocuments {
			get { return wsdl_documents; }
		}

		public KeyedByTypeCollection <IWsdlImportExtension> WsdlImportExtensions {
			get { return wsdl_extensions; }
		}

		public XmlSchemaSet XmlSchemas {
			get { return xmlschemas; }
		}

		public Collection<SMBinding> ImportAllBindings ()
		{
			Collection<SMBinding> bindings = new Collection<SMBinding> ();

			foreach (WSServiceDescription sd in wsdl_documents)
				foreach (WSBinding binding in sd.Bindings)
					bindings.Add (ImportBinding (binding));

			return bindings;
		}

		public SMBinding ImportBinding (WSBinding binding)
		{
			/* Default, CustomBinding.. */
			CustomBinding smbinding = new CustomBinding ();

			foreach (IWsdlImportExtension extension in wsdl_extensions)
				extension.BeforeImport (wsdl_documents, xmlschemas, policies);

			smbinding.Name = binding.Name;
			smbinding.Namespace = binding.ServiceDescription.TargetNamespace;

			//FIXME: Added by MessageEncodingBindingElementConverter.ImportPolicy
			smbinding.Elements.Add (new TextMessageEncodingBindingElement ());

			/*ImportContract
			PortType portType = null;
			foreach (WSServiceDescription sd in wsdl_documents) {
				portType = sd.PortTypes [binding.Type.Name];
				if (portType != null)
					break;
			}

			//FIXME: if portType == null
			*/
			
			// FIXME: ImportContract here..

			return smbinding;
		}

		public override Collection<ContractDescription> ImportAllContracts ()
		{
			if (contracts != null)
				return contracts;

			contracts = new Collection<ContractDescription> ();

			foreach (WSServiceDescription sd in wsdl_documents) {
				foreach (PortType pt in sd.PortTypes)
					contracts.Add (ImportContract (pt));
			}

			return contracts;
		}

		public override ServiceEndpointCollection ImportAllEndpoints ()
		{
			if (endpoint_colln != null)
				return endpoint_colln;

			endpoint_colln = new ServiceEndpointCollection ();

			foreach (IWsdlImportExtension extension in wsdl_extensions) {
				extension.BeforeImport (wsdl_documents, xmlschemas, policies);
			}

			foreach (WSServiceDescription wsd in wsdl_documents)
				foreach (Service service in wsd.Services)
					foreach (Port port in service.Ports)
						endpoint_colln.Add (ImportEndpoint (port));

			return endpoint_colln;
		}

		public ContractDescription ImportContract (PortType wsdlPortType)
		{
			foreach (IWsdlImportExtension extension in wsdl_extensions) {
				extension.BeforeImport (wsdl_documents, xmlschemas, policies);
			}

			ContractDescription cd = new ContractDescription (wsdlPortType.Name, wsdlPortType.ServiceDescription.TargetNamespace);

			foreach (Operation op in wsdlPortType.Operations) {
				OperationDescription op_descr = new OperationDescription (op.Name, cd);

				foreach (OperationMessage opmsg in op.Messages) {
					/* OperationMessageCollection */
					MessageDescription msg_descr;
					MessageDirection dir = MessageDirection.Input;
					string action = "";

					if (opmsg.GetType () == typeof (OperationInput))
						dir = MessageDirection.Input;
					else if (opmsg.GetType () == typeof (OperationOutput))
						dir = MessageDirection.Output;
					/* FIXME: OperationFault--> OperationDescription.Faults ? */

					if (opmsg.ExtensibleAttributes != null) {
						for (int i = 0; i < opmsg.ExtensibleAttributes.Length; i++) {
							if (opmsg.ExtensibleAttributes [i].LocalName == "Action" &&
								opmsg.ExtensibleAttributes [i].NamespaceURI == "http://www.w3.org/2006/05/addressing/wsdl")
								/* addressing:Action */
								action = opmsg.ExtensibleAttributes [i].Value;
							/* FIXME: other attributes ? */
						}
					}

					// fill Action from operation binding if required.
					if (action == "") {
						if (dir != MessageDirection.Input)
							action = GetActionFromOperationBinding (wsdlPortType, op.Name);
						else
							action = "*";
					}

					msg_descr = new MessageDescription (action, dir);
					/* FIXME: Headers ? */

					op_descr.Messages.Add (msg_descr);
				}

				cd.Operations.Add (op_descr);
			}

			WsdlContractConversionContext context = new WsdlContractConversionContext (cd, wsdlPortType);
			foreach (IWsdlImportExtension extension in wsdl_extensions)
				extension.ImportContract (this, context);

			return cd;
		}

		string GetActionFromOperationBinding (PortType pt, string opName)
		{
			foreach (WSBinding binding in pt.ServiceDescription.Bindings) {
				foreach (OperationBinding ob in binding.Operations) {
					if (ob.Name != opName)
						continue;
					foreach (var ext in ob.Extensions) {
						var sob = ext as SoapOperationBinding;
						if (sob == null)
							continue;
						return sob.SoapAction;
					}
					return String.Empty;
				}
			}
			return String.Empty;
		}

		public ServiceEndpoint ImportEndpoint (Port wsdlPort)
		{
			foreach (IWsdlImportExtension extension in wsdl_extensions) {
				extension.BeforeImport (wsdl_documents, xmlschemas, policies);
			}

			//Get the corresponding contract
			//via the PortType
			WSBinding wsb = wsdlPort.Service.ServiceDescription.Bindings [wsdlPort.Binding.Name];
			if (wsb == null)
				//FIXME
				throw new Exception (String.Format ("Binding named {0} not found.", wsdlPort.Binding.Name));

			SMBinding binding = ImportBinding (wsb);

			PortType port_type = null;
			foreach (WSServiceDescription sd in wsdl_documents) {
				port_type = sd.PortTypes [wsb.Type.Name];
				if (port_type != null)
					break;
			}

			if (port_type == null)
				//FIXME
				throw new Exception (String.Format ("PortType named {0} not found.", wsb.Type.Name));

			ContractDescription contract = ImportContract (port_type);
			ServiceEndpoint sep = new ServiceEndpoint (contract);

			sep.Binding = binding;

			WsdlContractConversionContext contract_context = new WsdlContractConversionContext (contract, port_type);
			WsdlEndpointConversionContext endpoint_context = new WsdlEndpointConversionContext (
					contract_context, sep, wsdlPort, wsb);

			foreach (IWsdlImportExtension extension in wsdl_extensions)
				extension.ImportEndpoint (this, endpoint_context);

			return sep;
		}

		public ServiceEndpointCollection ImportEndpoints (
			WSBinding binding)
		{
			throw new NotImplementedException ();
		}

		public ServiceEndpointCollection ImportEndpoints (
			PortType portType)
		{
			throw new NotImplementedException ();
		}

		public ServiceEndpointCollection ImportEndpoints (
			Service service)
		{
			throw new NotImplementedException ();
		}
	}
}
