//
// DataContractSerializerMessageContractImporter.cs
//
// Author: Atsushi Enomoto (atsushi@ximian.com)
//	   Ankit Jain (jankit@novell.com)
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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
using System.CodeDom;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Web.Services.Description;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

using QName = System.Xml.XmlQualifiedName;
using WSDL = System.Web.Services.Description.ServiceDescription;

namespace System.ServiceModel.Description
{
	[MonoTODO]
	public class DataContractSerializerMessageContractImporter
		: IWsdlImportExtension
	{
		MessageContractImporterInternal impl = new DataContractMessageContractImporterInternal ();
		bool enabled = true;

		public bool Enabled {
			get { return enabled; }
			set { enabled = value; }
		}

		void IWsdlImportExtension.BeforeImport (
			ServiceDescriptionCollection wsdlDocuments,
			XmlSchemaSet xmlSchemas,
			ICollection<XmlElement> policy)
		{
			if (!Enabled)
				return;

			impl.BeforeImport (wsdlDocuments, xmlSchemas, policy);
		}

		void IWsdlImportExtension.ImportContract (WsdlImporter importer,
			WsdlContractConversionContext context)
		{
			if (!Enabled)
				return;

			impl.ImportContract (importer, context);
		}

		void IWsdlImportExtension.ImportEndpoint (WsdlImporter importer,
			WsdlEndpointConversionContext context)
		{
			if (!Enabled)
				return;

			impl.ImportEndpoint (importer, context);
		}
	}

	abstract class MessageContractImporterInternal : IWsdlImportExtension
	{
		public void ImportContract (WsdlImporter importer,
			WsdlContractConversionContext context)
		{
			if (importer == null)
				throw new ArgumentNullException ("importer");
			if (context == null)
				throw new ArgumentNullException ("context");
			if (this.importer != null || this.context != null)
				throw new SystemException ("INTERNAL ERROR: unexpected recursion of ImportContract method call");

			schema_set_in_use = new XmlSchemaSet ();
			schema_set_in_use.Add (importer.XmlSchemas);
			foreach (WSDL wsdl in importer.WsdlDocuments)
				foreach (XmlSchema xs in wsdl.Types.Schemas)
					schema_set_in_use.Add (xs);

			schema_set_in_use.Compile ();

			this.importer = importer;
			this.context = context;
			try {
				DoImportContract ();
			} finally {
				this.importer = null;
				this.context = null;
			}
		}

		WsdlImporter importer;
		WsdlContractConversionContext context;

		internal XmlSchemaSet schema_set_in_use;

		public void BeforeImport (
			ServiceDescriptionCollection wsdlDocuments,
			XmlSchemaSet xmlSchemas,
			ICollection<XmlElement> policy)
		{
		}

		void DoImportContract ()
		{
			PortType port_type = context.WsdlPortType;
			ContractDescription contract = context.Contract;
			int i, j;
			List<MessagePartDescription> parts = new List<MessagePartDescription> ();

			i = 0;
			foreach (Operation op in port_type.Operations) {
				OperationDescription opdescr = contract.Operations [i];
				if (IsOperationImported (port_type, op))
					continue;
				if (!CanImportOperation (port_type, op))
					continue;

				j = 0;
				foreach (OperationMessage opmsg in op.Messages) {
					//SM.MessageDescription
					MessageDescription msgdescr = opdescr.Messages [j];

					//OpMsg's corresponding WSMessage
					Message msg = port_type.ServiceDescription.Messages [opmsg.Message.Name];

					msgdescr.Body.WrapperNamespace = port_type.ServiceDescription.TargetNamespace;

					if (opmsg is OperationOutput) {
						//ReturnValue
						msg = port_type.ServiceDescription.Messages [opmsg.Message.Name];
						
						resolveMessage (msg, msgdescr.Body, parts);
						if (parts.Count > 0) {
							msgdescr.Body.ReturnValue = parts [0];
							parts.Clear ();
						}
						continue;
					}

					/* OperationInput */
					
					/* Parts, MessagePartDescription */
					resolveMessage (msg, msgdescr.Body, parts);
					foreach (MessagePartDescription p in parts)
						msgdescr.Body.Parts.Add (p);
					parts.Clear ();

					j ++;
				}


				i ++;
			}

		}

		bool IsOperationImported (PortType pt, Operation op)
		{
			foreach (OperationMessage opmsg in op.Messages) {
				var parts = context.GetMessageDescription (opmsg).Body.Parts;
				foreach (var part in parts)
					if (part.Importer != null)
						return true;
			}
			return false;
		}

		void resolveMessage (Message msg, MessageBodyDescription body, List<MessagePartDescription> parts)
		{
			foreach (MessagePart part in msg.Parts) {
				if (part.Name == "parameters") {
					if (!part.Element.IsEmpty) {
						body.WrapperName = part.Element.Name;
						ImportPartsBySchemaElement (part.Element, parts, msg, part);
					} else {
						body.WrapperName = part.Type.Name;
						ResolveType (part.Type, parts, body.WrapperNamespace);
					}
				}
				//FIXME: non-parameters?
			}
		}

		public void ImportEndpoint (WsdlImporter importer,
			WsdlEndpointConversionContext context)
		{
		}

		protected abstract void ImportPartsBySchemaElement (QName qname, List<MessagePartDescription> parts, Message msg, MessagePart part);

		protected abstract void ResolveType (QName qname, List<MessagePartDescription> parts, string ns);

		protected abstract bool CanImportOperation (PortType portType, Operation op);
	}

	class DataContractMessageContractImporterInternal : MessageContractImporterInternal
	{
		XsdDataContractImporter dc_importer = new XsdDataContractImporter ();
		
		protected override void ImportPartsBySchemaElement (QName qname, List<MessagePartDescription> parts, Message msg, MessagePart part)
		{
			XmlSchemaElement element = (XmlSchemaElement) schema_set_in_use.GlobalElements [qname];
			if (element == null)
				//FIXME: What to do here?
				throw new Exception ("Could not resolve : " + qname.ToString ());

			var ct = element.ElementSchemaType as XmlSchemaComplexType;
			if (ct == null) // simple type
				parts.Add (CreateMessagePart (element, msg, part));
			else // complex type
				foreach (var elem in GetElementsInParticle (ct.ContentTypeParticle))
					parts.Add (CreateMessagePart (elem, msg, part));
		}

		IEnumerable<XmlSchemaElement> GetElementsInParticle (XmlSchemaParticle p)
		{
			if (p is XmlSchemaElement) {
				yield return (XmlSchemaElement) p;
			} else {
				var gb = p as XmlSchemaGroupBase;
				if (gb != null)
					foreach (XmlSchemaParticle pp in gb.Items)
						foreach (var e in GetElementsInParticle (pp))
							yield return e;
			}
		}

		MessagePartDescription CreateMessagePart (XmlSchemaElement elem, Message msg, MessagePart msgPart)
		{
			var part = new MessagePartDescription (elem.QualifiedName.Name, elem.QualifiedName.Namespace);
			part.Importer = dc_importer;
			if (dc_importer.CanImport (schema_set_in_use, elem)) {
				var typeQName = dc_importer.Import (schema_set_in_use, elem);
				part.CodeTypeReference = dc_importer.GetCodeTypeReference (typeQName);
			}
			return part;
		}

		protected override void ResolveType (QName qname, List<MessagePartDescription> parts, string ns)
		{
			/*foreach (XmlSchema xs in importer.Schemas)
				if (xs.Types [qname] != null)
					return resolveParameters ((XmlSchemaElement) xs.Types [qname]., msgdescr, importer);

			//FIXME: What to do here?
			throw new Exception ("Could not resolve : " + qname.ToString ());*/
			throw new NotImplementedException ();
		}

		protected override bool CanImportOperation (PortType portType, Operation op)
		{
			foreach (OperationMessage om in op.Messages)
				if (!dc_importer.CanImport (schema_set_in_use, om.Message))
					return false;
			return true;
		}
	}

	class XmlSerializerMessageContractImporterInternal : MessageContractImporterInternal
	{
		CodeCompileUnit ccu;
		XmlSchemaSet schema_set_cache;
		XmlSchemaImporter schema_importer;
		XmlCodeExporter code_exporter;
		
		protected override void ImportPartsBySchemaElement (QName qname, List<MessagePartDescription> parts, Message msg, MessagePart msgPart)
		{
			if (schema_set_cache != schema_set_in_use) {
				schema_set_cache = schema_set_in_use;
				var xss = new XmlSchemas ();
				foreach (XmlSchema xs in schema_set_cache.Schemas ())
					xss.Add (xs);
				schema_importer = new XmlSchemaImporter (xss);
				ccu = new CodeCompileUnit ();
				var cns = new CodeNamespace ();
				ccu.Namespaces.Add (cns);
				code_exporter = new XmlCodeExporter (cns, ccu);
			}

			var part = new MessagePartDescription (qname.Name, qname.Namespace);
			part.XsCodeCompileUnit = ccu;
			var mbrNS = msg.ServiceDescription.TargetNamespace;
			code_exporter.ExportMembersMapping (schema_importer.ImportMembersMapping (qname));

			parts.Add (part);
		}

		protected override void ResolveType (QName qname, List<MessagePartDescription> parts, string ns)
		{
			throw new NotImplementedException ();
		}

		protected override bool CanImportOperation (PortType portType, Operation op)
		{
			// FIXME: implement
			return true;
		}
	}
}
