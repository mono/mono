// 
// SampleGenerator.cs
//
// Author:
//   Lluis Sanchez Gual (lluis@ximian.com)
//
// Copyright (C) 2004 Novel Inc.
//

using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Web.Services.Description;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Mono.WebServices
{
	public class ConsoleSampleGenerator: SampleGenerator
	{
		public ConsoleSampleGenerator (ServiceDescriptionCollection services, XmlSchemas schemas)
		: base (services, schemas)
		{
		}
		
		public static void Generate (ArrayList services, ArrayList schemas, string binOper, string protocol)
		{
			ServiceDescriptionCollection descCol = new ServiceDescriptionCollection ();
			foreach (ServiceDescription sd in services)
				descCol.Add (sd);
				
			XmlSchemas schemaCol;

			if (schemas.Count > 0) {
				schemaCol = new XmlSchemas ();
				foreach (XmlSchema sc in schemas)
					schemaCol.Add (sc);
			}
			else
				schemaCol = descCol[0].Types.Schemas;
				
			string oper, bin = null; 
			
			int i = binOper.IndexOf ('/');
			if (i != -1) {
				oper = binOper.Substring (i+1);
				bin = binOper.Substring (0,i);
			}
			else
				oper = binOper;
			
			ConsoleSampleGenerator sg = new ConsoleSampleGenerator (descCol, schemaCol);
			
			string req, resp;
			sg.GenerateMessages (oper, bin, protocol, out req, out resp);
			
			Console.WriteLine ();
			Console.WriteLine ("Sample request message:");
			Console.WriteLine ();
			Console.WriteLine (req);
			Console.WriteLine ();
			Console.WriteLine ("Sample response message:");
			Console.WriteLine ();
			Console.WriteLine (resp);
		}
		
		public void GenerateMessages (string operation, string bindingName, string protocol, out string req, out string resp)
		{
			Port port = FindPort (bindingName, protocol);
			Binding binding = descriptions.GetBinding (port.Binding);
			if (binding == null) throw new InvalidOperationException ("Binding " + bindingName + " not found");
			
			PortType portType = descriptions.GetPortType (binding.Type);
			Operation oper = FindOperation (portType, operation);
			if (oper == null) throw new InvalidOperationException ("Operation " + operation + " not found");
			OperationBinding obin = FindOperation (binding, operation);
			
			req = GenerateMessage (port, obin, oper, protocol, true);
			resp = GenerateMessage (port, obin, oper, protocol, false);
		}
		
		Port FindPort (string portName, string protocol)
		{
			Service service = descriptions[0].Services[0];
			foreach (Port port in service.Ports)
			{
				if (portName == null)
				{
					Binding binding = descriptions.GetBinding (port.Binding);
					if (GetProtocol (binding) == protocol) return port;
				}
				else if (port.Name == portName)
					return port;
			}
			return null;
		}
		
		string GetProtocol (Binding binding)
		{
			if (binding.Extensions.Find (typeof(SoapBinding)) != null) return "Soap";
			HttpBinding hb = (HttpBinding) binding.Extensions.Find (typeof(HttpBinding));
			if (hb == null) return "";
			if (hb.Verb == "POST") return "HttpPost";
			if (hb.Verb == "GET") return "HttpGet";
			return "";
		}
		
		Operation FindOperation (PortType portType, string name)
		{
			foreach (Operation oper in portType.Operations) {
				if (oper.Messages.Input.Name != null) {
					if (oper.Messages.Input.Name == name) return oper;
				}
				else
					if (oper.Name == name) return oper;
			}
				
			return null;
		}
		
		OperationBinding FindOperation (Binding binding, string name)
		{
			foreach (OperationBinding oper in binding.Operations) {
				if (oper.Input.Name != null) {
					if (oper.Input.Name == name) return oper;
				}
				else 
					if (oper.Name == name) return oper;
			}
				
			return null;
		}
	}
	
	//
	// Sample generator class	
	//
	
	public class SampleGenerator
	{
		protected ServiceDescriptionCollection descriptions;
		protected XmlSchemas schemas;
		XmlSchemaElement anyElement;
		ArrayList queue;
		SoapBindingUse currentUse;
		XmlDocument document = new XmlDocument ();
		
		static readonly XmlQualifiedName anyType = new XmlQualifiedName ("anyType",XmlSchema.Namespace);
		static readonly XmlQualifiedName arrayType = new XmlQualifiedName ("Array","http://schemas.xmlsoap.org/soap/encoding/");
		static readonly XmlQualifiedName arrayTypeRefName = new XmlQualifiedName ("arrayType","http://schemas.xmlsoap.org/soap/encoding/");
		const string SoapEnvelopeNamespace = "http://schemas.xmlsoap.org/soap/envelope/";
		const string WsdlNamespace = "http://schemas.xmlsoap.org/wsdl/";
		const string SoapEncodingNamespace = "http://schemas.xmlsoap.org/soap/encoding/";
		
		class EncodedType
		{
			public EncodedType (string ns, XmlSchemaElement elem) { Namespace = ns; Element = elem; }
			public string Namespace;
			public XmlSchemaElement Element;
		}

		public SampleGenerator (ServiceDescriptionCollection services, XmlSchemas schemas)
		{
			descriptions = services;
			this.schemas = schemas;
			queue = new ArrayList ();
		}
		
		public string GenerateMessage (Port port, OperationBinding obin, Operation oper, string protocol, bool generateInput)
		{
			OperationMessage msg = null;
			foreach (OperationMessage opm in oper.Messages)
			{
				if (opm is OperationInput && generateInput) msg = opm;
				else if (opm is OperationOutput && !generateInput) msg = opm;
			}
			if (msg == null) return null;
			
			switch (protocol) {
				case "Soap": return GenerateHttpSoapMessage (port, obin, oper, msg);
				case "HttpGet": return GenerateHttpGetMessage (port, obin, oper, msg);
				case "HttpPost": return GenerateHttpPostMessage (port, obin, oper, msg);
			}
			return "Unknown protocol";
		}
		
		public string GenerateHttpSoapMessage (Port port, OperationBinding obin, Operation oper, OperationMessage msg)
		{
			string req = "";
			
			if (msg is OperationInput)
			{
				SoapAddressBinding sab = port.Extensions.Find (typeof(SoapAddressBinding)) as SoapAddressBinding;
				SoapOperationBinding sob = obin.Extensions.Find (typeof(SoapOperationBinding)) as SoapOperationBinding;
				req += "POST " + new Uri (sab.Location).AbsolutePath + "\n";
				req += "SOAPAction: " + sob.SoapAction + "\n";
				req += "Content-Type: text/xml; charset=utf-8\n";
				req += "Content-Length: " + GetLiteral ("string") + "\n";
				req += "Host: " + GetLiteral ("string") + "\n\n";
			}
			else
			{
				req += "HTTP/1.0 200 OK\n";
				req += "Content-Type: text/xml; charset=utf-8\n";
				req += "Content-Length: " + GetLiteral ("string") + "\n\n";
			}
			
			req += GenerateSoapMessage (obin, oper, msg);
			return req;
		}
		
		public string GenerateHttpGetMessage (Port port, OperationBinding obin, Operation oper, OperationMessage msg)
		{
			string req = "";
			
			if (msg is OperationInput)
			{
				HttpAddressBinding sab = port.Extensions.Find (typeof(HttpAddressBinding)) as HttpAddressBinding;
				HttpOperationBinding sob = obin.Extensions.Find (typeof(HttpOperationBinding)) as HttpOperationBinding;
				string location = new Uri (sab.Location).AbsolutePath + sob.Location + "?" + BuildQueryString (msg);
				req += "GET " + location + "\n";
				req += "Host: " + GetLiteral ("string");
			}
			else
			{
				req += "HTTP/1.0 200 OK\n";
				req += "Content-Type: text/xml; charset=utf-8\n";
				req += "Content-Length: " + GetLiteral ("string") + "\n\n";
			
				MimeXmlBinding mxb = (MimeXmlBinding) obin.Output.Extensions.Find (typeof(MimeXmlBinding)) as MimeXmlBinding;
				if (mxb == null) return req;
				
				Message message = descriptions.GetMessage (msg.Message);
				XmlQualifiedName ename = null;
				foreach (MessagePart part in message.Parts)
					if (part.Name == mxb.Part) ename = part.Element;
					
				if (ename == null) return req + GetLiteral("string");
				
				StringWriter sw = new StringWriter ();
				XmlTextWriter xtw = new XmlTextWriter (sw);
				xtw.Formatting = Formatting.Indented;
				currentUse = SoapBindingUse.Literal;
				WriteRootElementSample (xtw, ename);
				xtw.Close ();
				req += sw.ToString ();
			}
			
			return req;
		}
		
		public string GenerateHttpPostMessage (Port port, OperationBinding obin, Operation oper, OperationMessage msg)
		{
			string req = "";
			
			if (msg is OperationInput)
			{
				HttpAddressBinding sab = port.Extensions.Find (typeof(HttpAddressBinding)) as HttpAddressBinding;
				HttpOperationBinding sob = obin.Extensions.Find (typeof(HttpOperationBinding)) as HttpOperationBinding;
				string location = new Uri (sab.Location).AbsolutePath + sob.Location;
				req += "POST " + location + "\n";
				req += "Content-Type: application/x-www-form-urlencoded\n";
				req += "Content-Length: " + GetLiteral ("string") + "\n";
				req += "Host: " + GetLiteral ("string") + "\n\n";
				req += BuildQueryString (msg);
			}
			else return GenerateHttpGetMessage (port, obin, oper, msg);
			
			return req;
		}
		
		string BuildQueryString (OperationMessage opm)
		{
			string s = "";
			Message msg = descriptions.GetMessage (opm.Message);
			foreach (MessagePart part in msg.Parts)
			{
				if (s.Length != 0) s += "&";
				s += part.Name + "=" + GetLiteral (part.Type.Name);
			}
			return s;
		}
		
		public string GenerateSoapMessage (OperationBinding obin, Operation oper, OperationMessage msg)
		{
			SoapOperationBinding sob = obin.Extensions.Find (typeof(SoapOperationBinding)) as SoapOperationBinding;
			SoapBindingStyle style = (sob != null) ? sob.Style : SoapBindingStyle.Document;
			
			MessageBinding msgbin = (msg is OperationInput) ? (MessageBinding) obin.Input : (MessageBinding)obin.Output;
			SoapBodyBinding sbb = msgbin.Extensions.Find (typeof(SoapBodyBinding)) as SoapBodyBinding;
			SoapBindingUse bodyUse = (sbb != null) ? sbb.Use : SoapBindingUse.Literal;
			
			StringWriter sw = new StringWriter ();
			XmlTextWriter xtw = new XmlTextWriter (sw);
			xtw.Formatting = Formatting.Indented;
			
			xtw.WriteStartDocument ();
			xtw.WriteStartElement ("soap", "Envelope", SoapEnvelopeNamespace);
			xtw.WriteAttributeString ("xmlns", "xsi", null, XmlSchema.InstanceNamespace);
			xtw.WriteAttributeString ("xmlns", "xsd", null, XmlSchema.Namespace);
			
			if (bodyUse == SoapBindingUse.Encoded) 
			{
				xtw.WriteAttributeString ("xmlns", "soapenc", null, SoapEncodingNamespace);
				xtw.WriteAttributeString ("xmlns", "tns", null, msg.Message.Namespace);
			}

			// Serialize headers
			
			bool writtenHeader = false;
			foreach (object ob in msgbin.Extensions)
			{
				SoapHeaderBinding hb = ob as SoapHeaderBinding;
				if (hb == null) continue;
				
				if (!writtenHeader) {
					xtw.WriteStartElement ("soap", "Header", SoapEnvelopeNamespace);
					writtenHeader = true;
				}
				
				WriteHeader (xtw, hb);
			}
			
			if (writtenHeader)
				xtw.WriteEndElement ();

			// Serialize body
			xtw.WriteStartElement ("soap", "Body", SoapEnvelopeNamespace);
			
			currentUse = bodyUse;
			WriteBody (xtw, oper, msg, sbb, style);
			
			xtw.WriteEndElement ();
			xtw.WriteEndElement ();
			xtw.Close ();
			return sw.ToString ();
		}
		
		void WriteHeader (XmlTextWriter xtw, SoapHeaderBinding header)
		{
			Message msg = descriptions.GetMessage (header.Message);
			if (msg == null) throw new InvalidOperationException ("Message " + header.Message + " not found");
			MessagePart part = msg.Parts [header.Part];
			if (part == null) throw new InvalidOperationException ("Message part " + header.Part + " not found in message " + header.Message);

			currentUse = header.Use;
			
			if (currentUse == SoapBindingUse.Literal)
				WriteRootElementSample (xtw, part.Element);
			else
				WriteTypeSample (xtw, part.Type);
		}
		
		void WriteBody (XmlTextWriter xtw, Operation oper, OperationMessage opm, SoapBodyBinding sbb, SoapBindingStyle style)
		{
			Message msg = descriptions.GetMessage (opm.Message);
			if (msg.Parts.Count > 0 && msg.Parts[0].Name == "parameters")
			{
				MessagePart part = msg.Parts[0];
				if (part.Element == XmlQualifiedName.Empty)
					WriteTypeSample (xtw, part.Type);
				else
					WriteRootElementSample (xtw, part.Element);
			}
			else
			{
				string elemName = oper.Name;
				string ns = "";
				if (opm is OperationOutput) elemName += "Response";
				
				if (style == SoapBindingStyle.Rpc) {
					xtw.WriteStartElement (elemName, sbb.Namespace);
					ns = sbb.Namespace;
				}
					
				foreach (MessagePart part in msg.Parts)
				{
					if (part.Element == XmlQualifiedName.Empty)
					{
						XmlSchemaElement elem = new XmlSchemaElement ();
						elem.SchemaTypeName = part.Type;
						elem.Name = part.Name;
						WriteElementSample (xtw, ns, elem);
					}
					else
						WriteRootElementSample (xtw, part.Element);
				}
				
				if (style == SoapBindingStyle.Rpc)
					xtw.WriteEndElement ();
			}
			WriteQueuedTypeSamples (xtw);
		}
		
		void WriteRootElementSample (XmlTextWriter xtw, XmlQualifiedName qname)
		{
			XmlSchemaElement elem = (XmlSchemaElement) schemas.Find (qname, typeof(XmlSchemaElement));
			if (elem == null) throw new InvalidOperationException ("Element not found: " + qname);
			WriteElementSample (xtw, qname.Namespace, elem);
		}
		
		void WriteElementSample (XmlTextWriter xtw, string ns, XmlSchemaElement elem)
		{
			XmlQualifiedName root;
			
			if (!elem.RefName.IsEmpty) {
				XmlSchemaElement refElem = FindRefElement (elem);
				if (refElem == null) throw new InvalidOperationException ("Global element not found: " + elem.RefName);
				root = elem.RefName;
				elem = refElem;
			}
			else
				root = new XmlQualifiedName (elem.Name, ns);
			
			if (!elem.SchemaTypeName.IsEmpty)
			{
				XmlSchemaComplexType st = FindComplexTyype (elem.SchemaTypeName);
				if (st != null) 
					WriteComplexTypeSample (xtw, st, root);
				else
				{
					xtw.WriteStartElement (root.Name, root.Namespace);
					if (currentUse == SoapBindingUse.Encoded) 
						xtw.WriteAttributeString ("type", XmlSchema.InstanceNamespace, GetQualifiedNameString (xtw, elem.SchemaTypeName));
					xtw.WriteString (GetLiteral (FindBuiltInType (elem.SchemaTypeName)));
					xtw.WriteEndElement ();
				}
			}
			else if (elem.SchemaType == null)
			{
				xtw.WriteStartElement ("any");
				xtw.WriteEndElement ();
			}
			else
				WriteComplexTypeSample (xtw, (XmlSchemaComplexType) elem.SchemaType, root);
		}
		
		void WriteTypeSample (XmlTextWriter xtw, XmlQualifiedName qname)
		{
			XmlSchemaComplexType ctype = FindComplexTyype (qname);
			if (ctype != null) {
				WriteComplexTypeSample (xtw, ctype, qname);
				return;
			}
			
			XmlSchemaSimpleType stype = (XmlSchemaSimpleType) schemas.Find (qname, typeof(XmlSchemaSimpleType));
			if (stype != null) {
				WriteSimpleTypeSample (xtw, stype);
				return;
			}
			
			xtw.WriteString (GetLiteral (FindBuiltInType (qname)));
			throw new InvalidOperationException ("Type not found: " + qname);
		}
		
		void WriteComplexTypeSample (XmlTextWriter xtw, XmlSchemaComplexType stype, XmlQualifiedName rootName)
		{
			WriteComplexTypeSample (xtw, stype, rootName, -1);
		}
		
		void WriteComplexTypeSample (XmlTextWriter xtw, XmlSchemaComplexType stype, XmlQualifiedName rootName, int id)
		{
			string ns = rootName.Namespace;
			
			if (rootName.Name.IndexOf ("[]") != -1) rootName = arrayType;
			
			if (currentUse == SoapBindingUse.Encoded) {
				string pref = xtw.LookupPrefix (rootName.Namespace);
				if (pref == null) pref = "q1";
				xtw.WriteStartElement (pref, rootName.Name, rootName.Namespace);
				ns = "";
			}
			else
				xtw.WriteStartElement (rootName.Name, rootName.Namespace);
			
			if (id != -1)
			{
				xtw.WriteAttributeString ("id", "id" + id);
				if (rootName != arrayType)
					xtw.WriteAttributeString ("type", XmlSchema.InstanceNamespace, GetQualifiedNameString (xtw, rootName));
			}
			
			WriteComplexTypeAttributes (xtw, stype);
			WriteComplexTypeElements (xtw, ns, stype);
			
			xtw.WriteEndElement ();
		}
		
		void WriteComplexTypeAttributes (XmlTextWriter xtw, XmlSchemaComplexType stype)
		{
			WriteAttributes (xtw, stype.Attributes, stype.AnyAttribute);
		}
		
		void WriteComplexTypeElements (XmlTextWriter xtw, string ns, XmlSchemaComplexType stype)
		{
			if (stype.Particle != null)
				WriteParticleComplexContent (xtw, ns, stype.Particle);
			else
			{
				if (stype.ContentModel is XmlSchemaSimpleContent)
					WriteSimpleContent (xtw, (XmlSchemaSimpleContent)stype.ContentModel);
				else if (stype.ContentModel is XmlSchemaComplexContent)
					WriteComplexContent (xtw, ns, (XmlSchemaComplexContent)stype.ContentModel);
			}
		}

		void WriteAttributes (XmlTextWriter xtw, XmlSchemaObjectCollection atts, XmlSchemaAnyAttribute anyat)
		{
			foreach (XmlSchemaObject at in atts)
			{
				if (at is XmlSchemaAttribute)
				{
					XmlSchemaAttribute attr = (XmlSchemaAttribute)at;
					XmlSchemaAttribute refAttr = attr;
					
					// refAttr.Form; TODO
					
					if (!attr.RefName.IsEmpty) {
						refAttr = FindRefAttribute (attr.RefName);
						if (refAttr == null) throw new InvalidOperationException ("Global attribute not found: " + attr.RefName);
					}
					
					string val;
					if (!refAttr.SchemaTypeName.IsEmpty) val = FindBuiltInType (refAttr.SchemaTypeName);
					else val = FindBuiltInType ((XmlSchemaSimpleType) refAttr.SchemaType);
					
					xtw.WriteAttributeString (refAttr.Name, val);
				}
				else if (at is XmlSchemaAttributeGroupRef)
				{
					XmlSchemaAttributeGroupRef gref = (XmlSchemaAttributeGroupRef)at;
					XmlSchemaAttributeGroup grp = (XmlSchemaAttributeGroup) schemas.Find (gref.RefName, typeof(XmlSchemaAttributeGroup));
					WriteAttributes (xtw, grp.Attributes, grp.AnyAttribute);
				}
			}
			
			if (anyat != null)
				xtw.WriteAttributeString ("custom-attribute","value");
		}
		
		void WriteParticleComplexContent (XmlTextWriter xtw, string ns, XmlSchemaParticle particle)
		{
			WriteParticleContent (xtw, ns, particle, false);
		}
		
		void WriteParticleContent (XmlTextWriter xtw, string ns, XmlSchemaParticle particle, bool multiValue)
		{
			if (particle is XmlSchemaGroupRef)
				particle = GetRefGroupParticle ((XmlSchemaGroupRef)particle);

			if (particle.MaxOccurs > 1) multiValue = true;
			
			if (particle is XmlSchemaSequence) {
				WriteSequenceContent (xtw, ns, ((XmlSchemaSequence)particle).Items, multiValue);
			}
			else if (particle is XmlSchemaChoice) {
				if (((XmlSchemaChoice)particle).Items.Count == 1)
					WriteSequenceContent (xtw, ns, ((XmlSchemaChoice)particle).Items, multiValue);
				else
					WriteChoiceContent (xtw, ns, (XmlSchemaChoice)particle, multiValue);
			}
			else if (particle is XmlSchemaAll) {
				WriteSequenceContent (xtw, ns, ((XmlSchemaAll)particle).Items, multiValue);
			}
		}

		void WriteSequenceContent (XmlTextWriter xtw, string ns, XmlSchemaObjectCollection items, bool multiValue)
		{
			foreach (XmlSchemaObject item in items)
				WriteContentItem (xtw, ns, item, multiValue);
		}
		
		void WriteContentItem (XmlTextWriter xtw, string ns, XmlSchemaObject item, bool multiValue)
		{
			if (item is XmlSchemaGroupRef)
				item = GetRefGroupParticle ((XmlSchemaGroupRef)item);
					
			if (item is XmlSchemaElement)
			{
				XmlSchemaElement elem = (XmlSchemaElement) item;
				XmlSchemaElement refElem;
				if (!elem.RefName.IsEmpty) refElem = FindRefElement (elem);
				else refElem = elem;

				int num = (elem.MaxOccurs == 1 && !multiValue) ? 1 : 2;
				for (int n=0; n<num; n++)
				{
					if (currentUse == SoapBindingUse.Literal)
						WriteElementSample (xtw, ns, refElem);
					else
						WriteRefTypeSample (xtw, ns, refElem);
				}
			}
			else if (item is XmlSchemaAny)
			{
				xtw.WriteString (GetLiteral ("xml"));
			}
			else if (item is XmlSchemaParticle) {
				WriteParticleContent (xtw, ns, (XmlSchemaParticle)item, multiValue);
			}
		}
		
		void WriteChoiceContent (XmlTextWriter xtw, string ns, XmlSchemaChoice choice, bool multiValue)
		{
			foreach (XmlSchemaObject item in choice.Items)
				WriteContentItem (xtw, ns, item, multiValue);
		}

		void WriteSimpleContent (XmlTextWriter xtw, XmlSchemaSimpleContent content)
		{
			XmlSchemaSimpleContentExtension ext = content.Content as XmlSchemaSimpleContentExtension;
			if (ext != null)
				WriteAttributes (xtw, ext.Attributes, ext.AnyAttribute);
				
			XmlQualifiedName qname = GetContentBaseType (content.Content);
			xtw.WriteString (GetLiteral (FindBuiltInType (qname)));
		}

		string FindBuiltInType (XmlQualifiedName qname)
		{
			if (qname.Namespace == XmlSchema.Namespace)
				return qname.Name;

			XmlSchemaComplexType ct = FindComplexTyype (qname);
			if (ct != null)
			{
				XmlSchemaSimpleContent sc = ct.ContentModel as XmlSchemaSimpleContent;
				if (sc == null) throw new InvalidOperationException ("Invalid schema");
				return FindBuiltInType (GetContentBaseType (sc.Content));
			}
			
			XmlSchemaSimpleType st = (XmlSchemaSimpleType) schemas.Find (qname, typeof(XmlSchemaSimpleType));
			if (st != null)
				return FindBuiltInType (st);

			throw new InvalidOperationException ("Definition of type " + qname + " not found");
		}

		string FindBuiltInType (XmlSchemaSimpleType st)
		{
			if (st.Content is XmlSchemaSimpleTypeRestriction) {
				return FindBuiltInType (GetContentBaseType (st.Content));
			}
			else if (st.Content is XmlSchemaSimpleTypeList) {
				string s = FindBuiltInType (GetContentBaseType (st.Content));
				return s + " " + s + " ...";
			}
			else if (st.Content is XmlSchemaSimpleTypeUnion)
			{
				// Check if all types of the union are equal. If not, then will use anyType.
				XmlSchemaSimpleTypeUnion uni = (XmlSchemaSimpleTypeUnion) st.Content;
				string utype = null;

				// Anonymous types are unique
				if (uni.BaseTypes.Count != 0 && uni.MemberTypes.Length != 0)
					return "string";

				foreach (XmlQualifiedName mt in uni.MemberTypes)
				{
					string qn = FindBuiltInType (mt);
					if (utype != null && qn != utype) return "string";
					else utype = qn;
				}
				return utype;
			}
			else
				return "string";
		}
		

		XmlQualifiedName GetContentBaseType (XmlSchemaObject ob)
		{
			if (ob is XmlSchemaSimpleContentExtension)
				return ((XmlSchemaSimpleContentExtension)ob).BaseTypeName;
			else if (ob is XmlSchemaSimpleContentRestriction)
				return ((XmlSchemaSimpleContentRestriction)ob).BaseTypeName;
			else if (ob is XmlSchemaSimpleTypeRestriction)
				return ((XmlSchemaSimpleTypeRestriction)ob).BaseTypeName;
			else if (ob is XmlSchemaSimpleTypeList)
				return ((XmlSchemaSimpleTypeList)ob).ItemTypeName;
			else
				return null;
		}

		void WriteComplexContent (XmlTextWriter xtw, string ns, XmlSchemaComplexContent content)
		{
			XmlQualifiedName qname;

			XmlSchemaComplexContentExtension ext = content.Content as XmlSchemaComplexContentExtension;
			if (ext != null) qname = ext.BaseTypeName;
			else {
				XmlSchemaComplexContentRestriction rest = (XmlSchemaComplexContentRestriction)content.Content;
				qname = rest.BaseTypeName;
				if (qname == arrayType) {
					ParseArrayType (rest, out qname);
					XmlSchemaElement elem = new XmlSchemaElement ();
					elem.Name = "Item";
					elem.SchemaTypeName = qname;
					
					xtw.WriteAttributeString ("arrayType", SoapEncodingNamespace, qname.Name + "[2]");
					WriteContentItem (xtw, ns, elem, true);
					return;
				}
			}
			
			// Add base map members to this map
			XmlSchemaComplexType ctype = FindComplexTyype (qname);
			WriteComplexTypeAttributes (xtw, ctype);
			
			if (ext != null) {
				// Add the members of this map
				WriteAttributes (xtw, ext.Attributes, ext.AnyAttribute);
				if (ext.Particle != null)
					WriteParticleComplexContent (xtw, ns, ext.Particle);
			}
			
			WriteComplexTypeElements (xtw, ns, ctype);
		}
		
		void ParseArrayType (XmlSchemaComplexContentRestriction rest, out XmlQualifiedName qtype)
		{
			XmlSchemaAttribute arrayTypeAt = FindArrayAttribute (rest.Attributes);
			XmlAttribute[] uatts = arrayTypeAt.UnhandledAttributes;
			if (uatts == null || uatts.Length == 0) throw new InvalidOperationException ("arrayType attribute not specified in array declaration");
			
			XmlAttribute xat = null;
			foreach (XmlAttribute at in uatts)
				if (at.LocalName == "arrayType" && at.NamespaceURI == WsdlNamespace)
					{ xat = at; break; }
			
			if (xat == null) 
				throw new InvalidOperationException ("arrayType attribute not specified in array declaration");
			
			string arrayType = xat.Value;
			string type, ns;
			int i = arrayType.LastIndexOf (":");
			if (i == -1) ns = "";
			else ns = arrayType.Substring (0,i);
			
			int j = arrayType.IndexOf ("[", i+1);
			if (j == -1) throw new InvalidOperationException ("Cannot parse WSDL array type: " + arrayType);
			type = arrayType.Substring (i+1);
			type = type.Substring (0, type.Length-2);
			
			qtype = new XmlQualifiedName (type, ns);
		}
		
		XmlSchemaAttribute FindArrayAttribute (XmlSchemaObjectCollection atts)
		{
			foreach (object ob in atts)
			{
				XmlSchemaAttribute att = ob as XmlSchemaAttribute;
				if (att != null && att.RefName == arrayTypeRefName) return att;
				
				XmlSchemaAttributeGroupRef gref = ob as XmlSchemaAttributeGroupRef;
				if (gref != null)
				{
					XmlSchemaAttributeGroup grp = (XmlSchemaAttributeGroup) schemas.Find (gref.RefName, typeof(XmlSchemaAttributeGroup));
					att = FindArrayAttribute (grp.Attributes);
					if (att != null) return att;
				}
			}
			return null;
		}
		
		void WriteSimpleTypeSample (XmlTextWriter xtw, XmlSchemaSimpleType stype)
		{
			xtw.WriteString (GetLiteral (FindBuiltInType (stype)));
		}
		
		XmlSchemaParticle GetRefGroupParticle (XmlSchemaGroupRef refGroup)
		{
			XmlSchemaGroup grp = (XmlSchemaGroup) schemas.Find (refGroup.RefName, typeof (XmlSchemaGroup));
			return grp.Particle;
		}

		XmlSchemaElement FindRefElement (XmlSchemaElement elem)
		{
			if (elem.RefName.Namespace == XmlSchema.Namespace)
			{
				if (anyElement != null) return anyElement;
				anyElement = new XmlSchemaElement ();
				anyElement.Name = "any";
				anyElement.SchemaTypeName = anyType;
				return anyElement;
			}
			return (XmlSchemaElement) schemas.Find (elem.RefName, typeof(XmlSchemaElement));
		}
		
		XmlSchemaAttribute FindRefAttribute (XmlQualifiedName refName)
		{
			if (refName.Namespace == XmlSchema.Namespace)
			{
				XmlSchemaAttribute at = new XmlSchemaAttribute ();
				at.Name = refName.Name;
				at.SchemaTypeName = new XmlQualifiedName ("string",XmlSchema.Namespace);
				return at;
			}
			return (XmlSchemaAttribute) schemas.Find (refName, typeof(XmlSchemaAttribute));
		}
		
		void WriteRefTypeSample (XmlTextWriter xtw, string ns, XmlSchemaElement elem)
		{
			if (elem.SchemaTypeName.Namespace == XmlSchema.Namespace || schemas.Find (elem.SchemaTypeName, typeof(XmlSchemaSimpleType)) != null)
				WriteElementSample (xtw, ns, elem);
			else
			{
				xtw.WriteStartElement (elem.Name, ns);
				xtw.WriteAttributeString ("href", "#id" + (queue.Count+1));
				xtw.WriteEndElement ();
				queue.Add (new EncodedType (ns, elem));
			}
		}
		
		void WriteQueuedTypeSamples (XmlTextWriter xtw)
		{
			for (int n=0; n<queue.Count; n++)
			{
				EncodedType ec = (EncodedType) queue[n];
				XmlSchemaComplexType st = FindComplexTyype (ec.Element.SchemaTypeName);
				WriteComplexTypeSample (xtw, st, ec.Element.SchemaTypeName, n+1);
			}
		}
		
		XmlSchemaComplexType FindComplexTyype (XmlQualifiedName qname)
		{
			if (qname.Name.IndexOf ("[]") != -1)
			{
				XmlSchemaComplexType stype = new XmlSchemaComplexType ();
				stype.ContentModel = new XmlSchemaComplexContent ();
				
				XmlSchemaComplexContentRestriction res = new XmlSchemaComplexContentRestriction ();
				stype.ContentModel.Content = res;
				res.BaseTypeName = arrayType;
				
				XmlSchemaAttribute att = new XmlSchemaAttribute ();
				att.RefName = arrayTypeRefName;
				res.Attributes.Add (att);
				
				XmlAttribute xat = document.CreateAttribute ("arrayType", WsdlNamespace);
				xat.Value = qname.Namespace + ":" + qname.Name;
				att.UnhandledAttributes = new XmlAttribute[] {xat};
				return stype;
			}
				
			return (XmlSchemaComplexType) schemas.Find (qname, typeof(XmlSchemaComplexType));
		}
		
		string GetQualifiedNameString (XmlTextWriter xtw, XmlQualifiedName qname)
		{
			string pref = xtw.LookupPrefix (qname.Namespace);
			if (pref != null) return pref + ":" + qname.Name;
			
			xtw.WriteAttributeString ("xmlns", "q1", null, qname.Namespace);
			return "q1:" + qname.Name;
		}
				
		protected virtual string GetLiteral (string s)
		{
			return s;
		}
	}
	
	
	
}

