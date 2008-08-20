//
// WsdlExporter.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//	Ankit Jain <JAnkit@novell.com>
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
using System.Collections;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Web.Services.Description;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Runtime.Serialization;

using SMBinding = System.ServiceModel.Channels.Binding;
using SMMessage = System.ServiceModel.Channels.Message;

using WSServiceDescription = System.Web.Services.Description.ServiceDescription;
using WSBinding = System.Web.Services.Description.Binding;
using WSMessage = System.Web.Services.Description.Message;
using QName = System.Xml.XmlQualifiedName;

namespace System.ServiceModel.Description
{
	[MonoTODO]
	public class WsdlExporter : MetadataExporter
	{
		private MetadataSet metadata;
		private ServiceDescriptionCollection wsdl_colln;
		private XsdDataContractExporter xsd_exporter;
		private Hashtable exported_contracts;

		public override MetadataSet GetGeneratedMetadata ()
		{
			if (metadata != null)
				return metadata;

			metadata = new MetadataSet ();
			foreach (WSServiceDescription sd in GeneratedWsdlDocuments)
				metadata.MetadataSections.Add (
					MetadataSection.CreateFromServiceDescription (sd));

			foreach (XmlSchema xs in GeneratedXmlSchemas.Schemas ())
				metadata.MetadataSections.Add (
					MetadataSection.CreateFromSchema (xs));
				
			return metadata;
		}

		public override void ExportContract (ContractDescription contract)
		{
			ExportContractInternal (contract);	
		}

		List<IWsdlExportExtension> ExportContractInternal (ContractDescription contract)
		{
			QName qname = new QName (contract.Name, contract.Namespace);
			if (ExportedContracts.ContainsKey (qname))
				throw new ArgumentException (String.Format (
					"A ContractDescription with Namespace : {0} and Name : {1} has already been exported.", 
					contract.Namespace, contract.Name));

			WSServiceDescription sd = GetServiceDescription (contract.Namespace);

			List<IWsdlExportExtension> extensions = new List<IWsdlExportExtension> ();
			foreach (IWsdlExportExtension extn in contract.Behaviors.FindAll<IWsdlExportExtension> ())
				extensions.Add (extn);

			XmlDocument xdoc = new XmlDocument ();

			PortType ws_port = new PortType ();
			ws_port.Name = contract.Name;

			foreach (OperationDescription sm_op in contract.Operations) {
				Operation ws_op = new Operation ();
				ws_op.Name = sm_op.Name;

				foreach (MessageDescription sm_md in sm_op.Messages) {
					//OperationMessage
					OperationMessage ws_opmsg;
					WSMessage ws_msg = new WSMessage ();
					MessagePart ws_msgpart;
					if (sm_md.Direction == MessageDirection.Input) {
						ws_opmsg = new OperationInput ();
						ws_msg.Name = String.Concat (ws_port.Name, "_", ws_op.Name, "_", "InputMessage");
						ws_msgpart = ExportMessageBodyDescription (sm_md.Body, ws_op.Name, sd.TargetNamespace);
					} else {
						ws_opmsg = new OperationOutput ();
						ws_msg.Name = String.Concat (ws_port.Name, "_", ws_op.Name, "_", "OutputMessage");
						ws_msgpart = ExportMessageBodyDescription (sm_md.Body, ws_op.Name + "Response", sd.TargetNamespace);
					}
					ws_msg.Parts.Add (ws_msgpart);	

					/* FIXME: Faults */

					//Action
					XmlAttribute attr = xdoc.CreateAttribute ("wsaw", "Action", "http://www.w3.org/2006/05/addressing/wsdl");
					attr.Value = sm_md.Action;
					ws_opmsg.ExtensibleAttributes = new XmlAttribute [] { attr };
					
					//FIXME: Set .Input & .Output

					ws_opmsg.Message = new QName (ws_msg.Name, sd.TargetNamespace);
					ws_op.Messages.Add (ws_opmsg);
					sd.Messages.Add (ws_msg);
				}

				ws_port.Operations.Add (ws_op);

				foreach (IWsdlExportExtension extn in sm_op.Behaviors.FindAll<IWsdlExportExtension> ())
					extensions.Add (extn);
			}

			//Add Imports for <types
			XmlSchema xs_import = new XmlSchema ();
			xs_import.TargetNamespace = String.Concat (
					contract.Namespace, 
					contract.Namespace.EndsWith ("/") ? "" : "/",
					"Imports");
			foreach (XmlSchema schema in GeneratedXmlSchemas.Schemas ()) {
				XmlSchemaImport imp = new XmlSchemaImport ();
				imp.Namespace = schema.TargetNamespace;
				xs_import.Includes.Add (imp);
			}
			sd.Types.Schemas.Add (xs_import);

			sd.PortTypes.Add (ws_port);
			ExportedContracts [qname] = contract;

			WsdlContractConversionContext context = new WsdlContractConversionContext (contract, ws_port);
			foreach (IWsdlExportExtension extn in extensions)
				extn.ExportContract (this, context);

			return extensions;
		}

		public override void ExportEndpoint (ServiceEndpoint endpoint)
		{
			List<IWsdlExportExtension> extensions = ExportContractInternal (endpoint.Contract);
			
			//FIXME: Namespace
			WSServiceDescription sd = GetServiceDescription ("http://tempuri.org/");
			if (sd.TargetNamespace != endpoint.Contract.Namespace) {
				sd.Namespaces.Add ("i0", endpoint.Contract.Namespace);

				//Import
				Import import = new Import ();
				import.Namespace = endpoint.Contract.Namespace;

				sd.Imports.Add (import);
			}
			
			if (endpoint.Binding == null)
				throw new ArgumentException (String.Format (
					"Binding for ServiceEndpoint named '{0}' is null",
					endpoint.Name));

			bool msg_version_none =
				endpoint.Binding.MessageVersion != null &&
				endpoint.Binding.MessageVersion.Equals (MessageVersion.None);
			//ExportBinding
			WSBinding ws_binding = new WSBinding ();
			
			//<binding name = .. 
			ws_binding.Name = String.Concat (endpoint.Binding.Name, "_", endpoint.Contract.Name);

			//<binding type = ..
			ws_binding.Type = new QName (endpoint.Contract.Name, endpoint.Contract.Namespace);
			sd.Bindings.Add (ws_binding);

			if (!msg_version_none) {
				SoapBinding soap_binding = new SoapBinding ();
				soap_binding.Transport = SoapBinding.HttpTransport;
				soap_binding.Style = SoapBindingStyle.Document;
				ws_binding.Extensions.Add (soap_binding);
			}

			//	<operation
			foreach (OperationDescription sm_op in endpoint.Contract.Operations){
				OperationBinding op_binding = new OperationBinding ();
				op_binding.Name = sm_op.Name;

				//FIXME: Move to IWsdlExportExtension .. ?
				foreach (MessageDescription sm_md in sm_op.Messages) {
					if (sm_md.Direction == MessageDirection.Input) {
						//<input
						InputBinding in_binding = new InputBinding ();

						if (!msg_version_none) {
							SoapBodyBinding soap_body_binding = new SoapBodyBinding ();
							soap_body_binding.Use = SoapBindingUse.Literal;
							in_binding.Extensions.Add (soap_body_binding);

							//Set Action
							//<operation > <soap:operation soapAction .. >
							SoapOperationBinding soap_operation_binding = new SoapOperationBinding ();
							soap_operation_binding.SoapAction = sm_md.Action;
							soap_operation_binding.Style = SoapBindingStyle.Document;
							op_binding.Extensions.Add (soap_operation_binding);
						}

						op_binding.Input = in_binding;
					} else {
						//<output
						OutputBinding out_binding = new OutputBinding ();

						if (!msg_version_none) {
							SoapBodyBinding soap_body_binding = new SoapBodyBinding ();
							soap_body_binding.Use = SoapBindingUse.Literal;
							out_binding.Extensions.Add (soap_body_binding);
						}
						
						op_binding.Output = out_binding;
					}
				}

				ws_binding.Operations.Add (op_binding);
			}

			//Add <service
			Port ws_port = ExportService (sd, ws_binding, endpoint.Address, msg_version_none);

			//Call IWsdlExportExtension.ExportEndpoint
			WsdlContractConversionContext contract_context = new WsdlContractConversionContext (
				endpoint.Contract, sd.PortTypes [endpoint.Contract.Name]);
			WsdlEndpointConversionContext endpoint_context = new WsdlEndpointConversionContext (
				contract_context, endpoint, ws_port, ws_binding);

			foreach (IWsdlExportExtension extn in extensions)
				extn.ExportEndpoint (this, endpoint_context);

				
		}

		Port ExportService (WSServiceDescription sd, WSBinding ws_binding, EndpointAddress address, bool msg_version_none)
		{
			if (address == null)
				return null;

			Service ws_svc = GetService (sd, "service");
			sd.Name = "service";

				Port ws_port = new Port ();
				ws_port.Name = ws_binding.Name;
				ws_port.Binding = new QName (ws_binding.Name, sd.TargetNamespace);

				if (!msg_version_none) {
					SoapAddressBinding soap_addr = new SoapAddressBinding ();
					soap_addr.Location = address.Uri.AbsoluteUri;

					ws_port.Extensions.Add (soap_addr);
				}

			ws_svc.Ports.Add (ws_port);

			return ws_port;
		}

		Service GetService (WSServiceDescription sd, string name)
		{
			Service svc = sd.Services [name];
			if (svc != null)
				return svc;

			svc = new Service ();
			svc.Name = name;
			sd.Services.Add (svc);

			return svc;
		}

		WSServiceDescription GetServiceDescription (string ns)
		{
			foreach (WSServiceDescription sd in GeneratedWsdlDocuments) {
				if (sd.TargetNamespace == ns)
					return sd;
			}

			WSServiceDescription ret = new WSServiceDescription ();
			ret.TargetNamespace = ns;
			ret.Namespaces = GetNamespaces (ns);
			GeneratedWsdlDocuments.Add (ret);

			metadata = null;

			return ret;
		}

		public ServiceDescriptionCollection GeneratedWsdlDocuments {
			get {
				if (wsdl_colln == null)
					wsdl_colln = new ServiceDescriptionCollection ();
				return wsdl_colln;
			}
		}

		public XmlSchemaSet GeneratedXmlSchemas {
			get { return XsdExporter.Schemas; }
		}

		public void ExportEndpoints (
			IEnumerable<ServiceEndpoint> endpoints,
			XmlQualifiedName name)
		{
			throw new NotImplementedException ();
		}

		XsdDataContractExporter XsdExporter {
			get {
				if (xsd_exporter == null)
					xsd_exporter = new XsdDataContractExporter ();

				return xsd_exporter;
			}
		}

		Hashtable ExportedContracts {
			get {
				if (exported_contracts == null)
					exported_contracts = new Hashtable ();
				return exported_contracts;
			}
		}
		
		XmlSerializerNamespaces GetNamespaces (string target_namespace)
		{
			XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces ();

			namespaces.Add ("soap", "http://schemas.xmlsoap.org/wsdl/soap/");
			namespaces.Add ("wsu", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd");
			namespaces.Add ("soapenc", "http://schemas.xmlsoap.org/soap/encoding/");
			namespaces.Add ("tns", target_namespace);
			namespaces.Add ("wsa", "http://schemas.xmlsoap.org/ws/2004/08/addressing");
			namespaces.Add ("wsp", "http://schemas.xmlsoap.org/ws/2004/09/policy");
			namespaces.Add ("wsap", "http://schemas.xmlsoap.org/ws/2004/08/addressing/policy");
			namespaces.Add ("msc", "http://schemas.microsoft.com/ws/2005/12/wsdl/contract");
			namespaces.Add ("wsaw", "http://www.w3.org/2006/05/addressing/wsdl");
			namespaces.Add ("soap12", "http://schemas.xmlsoap.org/wsdl/soap12/");
			namespaces.Add ("wsa10", "http://www.w3.org/2005/08/addressing");
			namespaces.Add ("wsdl", "http://schemas.xmlsoap.org/wsdl/");

			return namespaces;
		}

		MessagePart ExportMessageBodyDescription (MessageBodyDescription msgbody, string name, string ns)
		{
			MessagePart msgpart = new MessagePart ();
			string part_name = IsTypeMessage (msgbody);

			if (part_name != null) {
				msgpart.Name = part_name;
				msgpart.Type = ExportTypeMessage (); //FIXME: Cache this
			} else {
				msgpart.Name = "parameters";
				msgpart.Element = ExportParameters (msgbody, name, ns);
			}
			return msgpart;
		}

		/* Sets the @name if the param or return type is SMMessage */
		string IsTypeMessage (MessageBodyDescription msgbody)
		{
			MessagePartDescription part = null;

			if (msgbody.Parts.Count == 0)
				part = msgbody.ReturnValue;
			else if (msgbody.Parts.Count == 1)
				part = msgbody.Parts [0];

			if (part != null && (part.Type.FullName == typeof (SMMessage).FullName))
				return part.Name;

			return null;
		}

		QName ExportParameters (MessageBodyDescription msgbody, string name, string ns)
		{
			XmlSchema xs = GetSchema (ns);
			//FIXME: Extract to a HasElement method ?
			foreach (XmlSchemaObject o in xs.Items) {
				XmlSchemaElement e = o as XmlSchemaElement;
				if (e == null)
					continue;

				if (e.Name == name)
					throw new InvalidOperationException (String.Format (
						"Message element named '{0}:{1}' has already been exported.",
						ns, name));
			}
				
			//Create the element for "parameters"
			XmlSchemaElement schema_element = new XmlSchemaElement ();
			schema_element.Name = name;

			XmlSchemaComplexType complex_type = new XmlSchemaComplexType ();
			//Generate Sequence representing the message/parameters
			//FIXME: MessageContractAttribute
		
			XmlSchemaSequence sequence = new XmlSchemaSequence ();
			XmlSchemaElement element = null;

			if (msgbody.ReturnValue == null) {
				//parameters
				foreach (MessagePartDescription part in msgbody.Parts) {
					if (part.Type == null)
						//FIXME: Eg. when WsdlImporter is used to import a wsdl
						throw new NotImplementedException ();
					
					element = GetSchemaElementForPart (part, xs);
					sequence.Items.Add (element);
				}
			} else {
				//ReturnValue
				if (msgbody.ReturnValue.Type != typeof (void)) {
					element = GetSchemaElementForPart (msgbody.ReturnValue, xs);
					sequence.Items.Add (element);
				}
			}

			complex_type.Particle = sequence;
			schema_element.SchemaType = complex_type;

			xs.Items.Add (schema_element);
			GeneratedXmlSchemas.Reprocess (xs);

			return new QName (schema_element.Name, xs.TargetNamespace);
		}

		//Exports <xs:type for SMMessage
		//FIXME: complex type for this can be made static
		QName ExportTypeMessage ()
		{
			XmlSchema xs = GetSchema ("http://schemas.microsoft.com/Message");
			QName qname = new QName ("MessageBody", xs.TargetNamespace);

			foreach (XmlSchemaObject o in xs.Items) {
				XmlSchemaComplexType ct = o as XmlSchemaComplexType;
				if (ct == null)
					continue;

				if (ct.Name == "MessageBody")
					//Already exported
					return qname;
			}

			XmlSchemaComplexType complex_type = new XmlSchemaComplexType ();
			complex_type.Name = "MessageBody";
			XmlSchemaSequence sequence = new XmlSchemaSequence ();

			XmlSchemaAny any = new XmlSchemaAny ();
			any.MinOccurs = 0;
			any.MaxOccursString = "unbounded";
			any.Namespace = "##any";

			sequence.Items.Add (any);
			complex_type.Particle = sequence;

			xs.Items.Add (complex_type);
			GeneratedXmlSchemas.Reprocess (xs);
			
			return qname;
		}

		XmlSchemaElement GetSchemaElementForPart (MessagePartDescription part, XmlSchema schema)
		{
			XmlSchemaElement element = new XmlSchemaElement ();

			element.Name = part.Name;
			XsdExporter.Export (part.Type);
			element.SchemaTypeName = XsdExporter.GetSchemaTypeName (part.Type);
			AddImport (schema, element.SchemaTypeName.Namespace);

			//FIXME: nillable, minOccurs
			if (XsdExporter.GetSchemaType (part.Type) is XmlSchemaComplexType ||
				part.Type == typeof (string))
				element.IsNillable = true;
			element.MinOccurs = 0;

			return element;
		}

		//FIXME: Replace with a dictionary ?
		void AddImport (XmlSchema schema, string ns)
		{
			if (ns == XmlSchema.Namespace || schema.TargetNamespace == ns)
				return;

			foreach (XmlSchemaObject o in schema.Includes) {
				XmlSchemaImport import = o as XmlSchemaImport;
				if (import == null)
					continue;
				if (import.Namespace == ns)
					return;
			}

			XmlSchemaImport imp = new XmlSchemaImport ();
			imp.Namespace = ns;
			schema.Includes.Add (imp);
		}

		XmlSchema GetSchema (string ns)
		{
			ICollection colln = GeneratedXmlSchemas.Schemas (ns);
			if (colln.Count > 0) { 
				if (colln.Count > 1)
					throw new Exception ("More than 1 schema found for ns = " + ns);
				//FIXME: HORRIBLE!
				foreach (object o in colln)
					return (o as XmlSchema);
			}

			XmlSchema schema = new XmlSchema ();
			schema.TargetNamespace = ns;
			schema.ElementFormDefault = XmlSchemaForm.Qualified;
			GeneratedXmlSchemas.Add (schema);

			return schema;
		}

	}
}
