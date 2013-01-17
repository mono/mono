//
// WsdlImporter.cs
//
// Authors:
//	Atsushi Enomoto <atsushi@ximian.com>
//	Ankit Jain <jankit@novell.com>	
//	Martin Baulig <martin.baulig@xamarin.com>
//
// Copyright (C) 2005 Novell, Inc.  http://www.novell.com
// Copyright (c) 2012 Xamarin Inc. (http://www.xamarin.com)
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
using WS = System.Web.Services.Description;
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
		bool beforeImportCalled;

		KeyedByTypeCollection<IWsdlImportExtension> wsdl_extensions;
			
		//Imported
		Collection<ContractDescription> contracts = null;
		ServiceEndpointCollection endpoint_colln = null;

		// Contract by PortType
		Dictionary<PortType, ContractDescription> contractHash = null;
		// ServiceEndpoint by WSBinding
		Dictionary<WSBinding, ServiceEndpoint> bindingHash = null;
		// ServiceEndpoint by Port
		Dictionary<Port, ServiceEndpoint> endpointHash = null;

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
				wsdl_extensions.Add (new MessageEncodingBindingElementImporter ());
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
			this.contractHash = new Dictionary<PortType, ContractDescription> ();
			this.bindingHash = new Dictionary<WSBinding, ServiceEndpoint> ();
			this.endpointHash = new Dictionary<Port, ServiceEndpoint> ();

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

			foreach (WSServiceDescription sd in wsdl_documents) {
				foreach (WSBinding binding in sd.Bindings) {
					var endpoint = ImportBinding (binding, false);
					if (endpoint != null)
						bindings.Add (endpoint.Binding);
				}
			}

			return bindings;
		}

		void BeforeImport ()
		{
			if (beforeImportCalled)
				return;

			foreach (IWsdlImportExtension extension in wsdl_extensions)
				extension.BeforeImport (wsdl_documents, xmlschemas, policies);
			
			beforeImportCalled = true;
		}

		public SMBinding ImportBinding (WSBinding binding)
		{
			return ImportBinding (binding, true).Binding;
		}

		ServiceEndpoint ImportBinding (WSBinding binding, bool throwOnError)
		{
			if (bindingHash.ContainsKey (binding)) {
				var sep = bindingHash [binding];
				if (sep != null)
					return sep;

				if (!throwOnError)
					return null;
				
				throw new InvalidOperationException (String.Format (
					"Failed to import binding {0}, an error has " +
					"already been reported before.", binding.Name));
			}

			try {
				var port_type = GetPortTypeFromBinding (binding);
				var contract = ImportContract (port_type);
				var contract_context = new WsdlContractConversionContext (contract, port_type);
			
				var sep = ImportBinding (binding, contract_context);
				bindingHash.Add (binding, sep);
				return sep;
			} catch (MetadataImportException) {
				bindingHash.Add (binding, null);
				if (throwOnError)
					throw;
				return null;
			} catch (Exception ex) {
				bindingHash.Add (binding, null);
				var error = AddError (
					"Failed to import binding `{0}': {1}", binding.Name, ex.Message);
				if (throwOnError)
					throw new MetadataImportException (error, ex);
				return null;
			}
		}

		ServiceEndpoint ImportBinding (WSBinding binding,
		                               WsdlContractConversionContext contract_context)
		{
			BeforeImport ();

			var sep = new ServiceEndpoint (contract_context.Contract);
			
			var custom = new CustomBinding ();
			custom.Name = binding.Name;
			custom.Namespace = binding.ServiceDescription.TargetNamespace;
			
			sep.Binding = custom;

			try {
				ImportPolicy (binding, sep);
			} catch (Exception ex) {
				// FIXME: Policy import is still experimental.
				AddWarning ("Exception while trying to import policy for " +
				            "binding `{0}': {1}", binding.Name, ex.Message);
			}
			
			var endpoint_context = new WsdlEndpointConversionContext (
				contract_context, sep, null, binding);
			
			foreach (IWsdlImportExtension extension in wsdl_extensions)
				extension.ImportEndpoint (this, endpoint_context);
			
			return sep;
		}

		void ImportPolicy (WSBinding binding, ServiceEndpoint endpoint)
		{
			var context = new Description.CustomPolicyConversionContext (binding, endpoint);
			var assertions = context.GetBindingAssertions ();

			foreach (var ext in binding.Extensions) {
				var xml = ext as XmlElement;
				if (xml == null)
					continue;
				if (!xml.NamespaceURI.Equals (Constants.WspNamespace))
					continue;
				if (xml.LocalName.Equals ("Policy")) {
					context.AddPolicyAssertion (xml);
					continue;
				}
				if (!xml.LocalName.Equals ("PolicyReference"))
					continue;
				var uri = xml.GetAttribute ("URI");

				if (!uri.StartsWith ("#")) {
					// FIXME
					AddWarning (
						"Failed to resolve unknown policy reference `{0}' for " +
						"binding `{1}'.", uri, binding.Name);
					continue;
				}

				foreach (var sext in binding.ServiceDescription.Extensions) {
					var sxml = sext as XmlElement;
					if (sxml == null)
						continue;
					if (!sxml.NamespaceURI.Equals (Constants.WspNamespace))
						continue;
					if (!sxml.LocalName.Equals ("Policy"))
						continue;
					var id = sxml.GetAttribute ("Id", Constants.WsuNamespace);
					if (!uri.Substring (1).Equals (id))
						continue;
					context.AddPolicyAssertion (sxml);
				}
			}

			foreach (IPolicyImportExtension extension in PolicyImportExtensions) {
				try {
					extension.ImportPolicy (this, context);
				} catch (Exception ex) {
					AddWarning (
						"PolicyImportException `{0}' threw an exception while " +
						"trying to import policy references for endpoint `{1}': {2}",
						extension.GetType ().Name, endpoint.Name, ex.Message);
				}
			}
		}

		PortType GetPortTypeFromBinding (WSBinding binding)
		{
			foreach (WSServiceDescription sd in wsdl_documents) {
				var port_type = sd.PortTypes [binding.Type.Name];
				if (port_type != null)
					return port_type;
			}
			
			throw new MetadataImportException (AddError (
				"PortType named {0} not found in namespace {1}.",
				binding.Type.Name, binding.Type.Namespace));
		}
		
		public override Collection<ContractDescription> ImportAllContracts ()
		{
			if (contracts != null)
				return contracts;

			contracts = new Collection<ContractDescription> ();

			foreach (WSServiceDescription sd in wsdl_documents) {
				foreach (PortType pt in sd.PortTypes) {
					var cd = ImportContract (pt, false);
					if (cd != null)
						contracts.Add (cd);
				}
			}

			return contracts;
		}

		public override ServiceEndpointCollection ImportAllEndpoints ()
		{
			if (endpoint_colln != null)
				return endpoint_colln;

			endpoint_colln = new ServiceEndpointCollection ();

			foreach (WSServiceDescription wsd in wsdl_documents) {
				foreach (Service service in wsd.Services) {
					foreach (Port port in service.Ports) {
						var sep = ImportEndpoint (port, false);
						if (sep != null)
							endpoint_colln.Add (sep);
					}
				}
			}

			return endpoint_colln;
		}

		public ContractDescription ImportContract (PortType wsdlPortType)
		{
			return ImportContract (wsdlPortType, true);
		}

		ContractDescription ImportContract (PortType portType, bool throwOnError)
		{
			if (contractHash.ContainsKey (portType)) {
				var cd = contractHash [portType];
				if (cd != null)
					return cd;

				if (!throwOnError)
					return null;

				throw new InvalidOperationException (String.Format (
					"Failed to import contract for port type `{0}', " +
					"an error has already been reported.", portType.Name));
			}

			try {
				var cd = DoImportContract (portType);
				contractHash.Add (portType, cd);
				return cd;
			} catch (MetadataImportException) {
				contractHash.Add (portType, null);
				if (throwOnError)
					throw;
				return null;
			} catch (Exception ex) {
				contractHash.Add (portType, null);
				var error = AddError (
					"Failed to import contract for port type `{0}': {1}",
					portType.Name, ex.Message);
				if (throwOnError)
					throw new MetadataImportException (error, ex);
				return null;
			}
		}

		ContractDescription DoImportContract (PortType wsdlPortType)
		{
			BeforeImport ();

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
			return ImportEndpoint (wsdlPort, true);
		}

		ServiceEndpoint ImportEndpoint (Port port, bool throwOnError)
		{
			ServiceEndpoint endpoint;
			if (endpointHash.ContainsKey (port)) {
				endpoint = endpointHash [port];
				if (endpoint != null)
					return endpoint;
				
				if (!throwOnError)
					return null;
				
				throw new InvalidOperationException (String.Format (
					"Failed to import port `{0}', an error has " +
					"already been reported before.", port.Name));
			}

			var binding = port.Service.ServiceDescription.Bindings [port.Binding.Name];
			if (binding == null) {
				endpointHash.Add (port, null);
				var error = AddError (
					"Failed to import port `{0}': cannot find binding `{1}' that " +
					"this port depends on.", port.Name, port.Binding.Name);
				if (throwOnError)
					throw new MetadataImportException (error);
				return null;
			}

			try {
				endpoint = ImportBinding (binding, throwOnError);
			} catch (Exception ex) {
				endpointHash.Add (port, null);
				var error = AddError (
					"Failed to import port `{0}': error while trying to import " +
					"binding `{1}' that this port depends on: {2}",
					port.Name, port.Binding.Name, ex.Message);
				if (throwOnError)
					throw new MetadataImportException (error, ex);
				return null;
			}

			if (endpoint == null) {
				endpointHash.Add (port, null);
				AddError (
					"Failed to import port `{0}': error while trying to import " +
					"binding `{1}' that this port depends on.",
					port.Name, port.Binding.Name);
				return null;
			}

			try {
				ImportEndpoint (port, binding, endpoint, throwOnError);
				endpointHash.Add (port, endpoint);
				return endpoint;
			} catch (MetadataImportException) {
				endpointHash.Add (port, null);
				if (throwOnError)
					throw;
				return null;
			} catch (Exception ex) {
				endpointHash.Add (port, null);
				var error = AddError (
					"Failed to import port `{0}': {1}", port.Name, ex.Message);
				if (throwOnError)
					throw new MetadataImportException (error, ex);
				return null;
			}
		}

		void ImportEndpoint (Port port, WSBinding wsb, ServiceEndpoint sep, bool throwOnError)
		{
			BeforeImport ();

			var port_type = GetPortTypeFromBinding (wsb);

			var contract_context = new WsdlContractConversionContext (sep.Contract, port_type);
			WsdlEndpointConversionContext endpoint_context = new WsdlEndpointConversionContext (
					contract_context, sep, port, wsb);

			foreach (IWsdlImportExtension extension in wsdl_extensions)
				extension.ImportEndpoint (this, endpoint_context);
		}

		void ImportEndpoints (ServiceEndpointCollection coll, WSBinding binding)
		{
			foreach (WSServiceDescription wsd in wsdl_documents) {
				foreach (WS.Service service in wsd.Services) {
					foreach (WS.Port port in service.Ports) {
						if (!binding.Name.Equals (port.Binding.Name))
							continue;
						var sep = ImportEndpoint (port, false);
						if (sep != null)
							coll.Add (sep);
					}
				}
			}
		}

		public ServiceEndpointCollection ImportEndpoints (WSBinding binding)
		{
			var coll = new ServiceEndpointCollection ();
			ImportEndpoints (coll, binding);
			return coll;
		}

		public ServiceEndpointCollection ImportEndpoints (PortType portType)
		{
			var coll = new ServiceEndpointCollection ();

			foreach (WSServiceDescription wsd in wsdl_documents) {
				foreach (WS.Binding binding in wsd.Bindings) {
					if (!binding.Type.Name.Equals (portType.Name))
						continue;

					ImportEndpoints (coll, binding);
				}
			}

			return coll;
		}

		public ServiceEndpointCollection ImportEndpoints (Service service)
		{
			var coll = new ServiceEndpointCollection ();
			
			foreach (Port port in service.Ports) {
				var sep = ImportEndpoint (port, false);
				if (sep != null)
					coll.Add (sep);
			}

			return coll;
		}
	}
}
