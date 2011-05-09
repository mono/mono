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
		IWsdlImportExtension impl = new WsdlDataImportExtensionInternal ();

		public bool Enabled { get; set; }

		void IWsdlImportExtension.BeforeImport (
			ServiceDescriptionCollection wsdlDocuments,
			XmlSchemaSet xmlSchemas,
			ICollection<XmlElement> policy)
		{
			if (wsdlDocuments == null)
				throw new ArgumentNullException ("wsdlDocuments");
			if (xmlSchemas == null)
				throw new ArgumentNullException ("xmlSchemas");

			if (!Enabled)
				return;

			impl.BeforeImport (wsdlDocuments, xmlSchemas, policy);
		}

		void IWsdlImportExtension.ImportContract (WsdlImporter importer,
			WsdlContractConversionContext context)
		{
			if (importer == null)
				throw new ArgumentNullException ("importer");
			if (context == null)
				throw new ArgumentNullException ("context");

			if (!Enabled)
				return;

			impl.ImportContract (importer, context);
		}

		void IWsdlImportExtension.ImportEndpoint (WsdlImporter importer,
			WsdlEndpointConversionContext context)
		{
		}
	}

	class WsdlDataImportExtensionInternal : IWsdlImportExtension
	{
		WsdlImporter importer;
		WsdlContractConversionContext context;

		XsdDataContractImporter dc_importer;

		XmlSchemaSet schema_set_in_use;

		public bool ImportXmlType { get; set; }

		public void ImportEndpoint (WsdlImporter importer, WsdlEndpointConversionContext context)
		{
			// not implemented? nothing to do?
		}

		public void BeforeImport (ServiceDescriptionCollection wsdlDocuments, XmlSchemaSet xmlSchemas, ICollection<XmlElement> policy)
		{
		}

		public void ImportContract (WsdlImporter importer, WsdlContractConversionContext context)
		{
			if (this.importer != null || this.context != null)
				throw new SystemException ("INTERNAL ERROR: unexpected recursion of ImportContract method call");

			dc_importer = new XsdDataContractImporter ();
			schema_set_in_use = new XmlSchemaSet ();
			schema_set_in_use.Add (importer.XmlSchemas);
			foreach (WSDL wsdl in importer.WsdlDocuments)
				foreach (XmlSchema xs in wsdl.Types.Schemas)
					schema_set_in_use.Add (xs);

			// commenting out this import operation, but might be required (I guess not).
			//dc_importer.Import (schema_set_in_use);
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

		void DoImportContract ()
		{
			PortType port_type = context.WsdlPortType;
			ContractDescription contract = context.Contract;
			int i, j;
			List<MessagePartDescription> parts = new List<MessagePartDescription> ();

			i = 0;
			foreach (Operation op in port_type.Operations) {
				OperationDescription opdescr = contract.Operations [i];

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

		void resolveMessage (Message msg, MessageBodyDescription body, List<MessagePartDescription> parts)
		{
			foreach (MessagePart part in msg.Parts) {
				if (part.Name == "parameters") {
					if (!part.Element.IsEmpty) {
						body.WrapperName = part.Element.Name;
						ImportPartsBySchemaElement (part.Element, parts, body.WrapperNamespace);
					} else {
						body.WrapperName = part.Type.Name;
						resolveType (part.Type, parts, body.WrapperNamespace);
					}
				}
				//FIXME: non-parameters?
			}
		}
		
		void ImportPartsBySchemaElement (QName qname, List<MessagePartDescription> parts, string ns)
		{
			XmlSchemaElement element = (XmlSchemaElement) schema_set_in_use.GlobalElements [qname];
			if (element == null)
				//FIXME: What to do here?
				throw new Exception ("Could not resolve : " + qname.ToString ());

			var ct = element.ElementSchemaType as XmlSchemaComplexType;
			MessagePartDescription part;
			if (ct == null) { // simple type
				if ((part = CreateMessagePart (element)) != null)
					parts.Add (part);
			} else { // complex type
				foreach (var elem in GetElementsInParticle (ct.ContentTypeParticle))
					if ((part = CreateMessagePart (elem)) != null)
						parts.Add (part);
			}
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

		MessagePartDescription CreateMessagePart (XmlSchemaElement elem)
		{
			var part = new MessagePartDescription (elem.QualifiedName.Name, elem.QualifiedName.Namespace);
			if (part.Importer != null) // already generated.
				return null;
			if (!dc_importer.CanImport (schema_set_in_use, elem))
				return null;
			part.Importer = dc_importer;
			var typeQName = dc_importer.Import (schema_set_in_use, elem);
			part.CodeTypeReference = dc_importer.GetCodeTypeReference (typeQName);
			return part;
		}

		void resolveType (QName qname, List<MessagePartDescription> parts, string ns)
		{
			/*foreach (XmlSchema xs in importer.Schemas)
				if (xs.Types [qname] != null)
					return resolveParameters ((XmlSchemaElement) xs.Types [qname]., msgdescr, importer);

			//FIXME: What to do here?
			throw new Exception ("Could not resolve : " + qname.ToString ());*/
			throw new NotImplementedException ();
		}

		internal static string GetCLRTypeName (QName qname)
		{
			switch (qname.Namespace) {
			case "http://schemas.microsoft.com/2003/10/Serialization/":
				if (qname.Name == "duration")
					return "System.TimeSpan";
				if (qname.Name == "guid")
					return "System.Guid";
				break;
			case "http://www.w3.org/2001/XMLSchema":
				return GetCLRTypeName (qname.Name);
			}
			return null;
		}

		internal static string GetCLRTypeName (string xsdName)
		{
			switch (xsdName) {
			case "anyURI":
				return "System.String";
			case "boolean":
				return "System.Boolean";

			//FIXME: case "base64Binary":
			case "dateTime":
				return "System.DateTime";
			case "QName":
				return "System.String";
			case "decimal":
				return "System.Decimal";
			case "double":
				return "System.Double";
			case "float":
				return "System.Double";
			case "byte":
				return "System.SByte";
			case "short":
				return "System.Int16";
			case "int":
				return "System.Int32";
			case "long":
				return "System.Int64";
			case "unsignedByte":
				return "System.Byte";
			case "unsignedShort":
				return "System.UInt16";
			case "unsignedInt":
				return "System.UInt32";
			case "unsignedLong":
				return "System.UInt64";
			case "string":
				return "System.String";
			/* FIXME:
			case "anyType":
				return true;
			default:
				return false;*/
			}

			return null;
		}
	}
}
