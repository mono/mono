// 
// System.Web.Services.Description.BasicProfileChecker.cs
//
// Author:
//   Lluis Sanchez (lluis@novell.com)
//   Atsushi Enomoto (atsushi@ximian.com)
//
// Copyright (C) Novell, Inc., 2004
//

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

#if NET_2_0

using System.Xml.Schema;
using System.Xml.Serialization;
using System.Xml;
using System.Collections;

namespace System.Web.Services.Description 
{
	internal class BasicProfileChecker: ConformanceChecker
	{
		public static BasicProfileChecker Instance = new BasicProfileChecker ();
		
		public override WsiProfiles Claims { 
			get { return WsiProfiles.BasicProfile1_1; }
		}
		
		public override void Check (ConformanceCheckContext ctx, Import value) 
		{
			if (value.Location == "" || value.Location == null) {
				ctx.ReportRuleViolation (value, BasicProfileRules.R2007);
				return;
			}
			
			object doc = ctx.GetDocument (value.Location);
			if (doc == null) ctx.ReportError (value, "Document '" + value.Location + "' not found");
			
			if (doc is XmlSchema)
				ctx.ReportRuleViolation (value, BasicProfileRules.R2002);
				
			ServiceDescription imported = doc as ServiceDescription;
			if (imported == null) {
				ctx.ReportRuleViolation (value, BasicProfileRules.R2001);
				return;
			}
				
			// TODO: rule R2003
			
			if (imported.TargetNamespace != value.Namespace)
				ctx.ReportRuleViolation (value, BasicProfileRules.R2005);
		}
		
		public override void Check (ConformanceCheckContext ctx, ServiceDescription value)
		{
			// R4005
			if (value.Namespaces != null)
				foreach (XmlQualifiedName qname in value.Namespaces.ToArray ())
					if (qname.Namespace == "http://www.w3.org/XML/1998/namespace")
						ctx.ReportRuleViolation (value, BasicProfileRules.R4005);
		}
		
		public override void Check (ConformanceCheckContext ctx, ServiceDescriptionFormatExtension value)
		{
			if (value.Required)
				ctx.ReportRuleViolation (value, BasicProfileRules.R2026);
		}
		
		public override void Check (ConformanceCheckContext ctx, MessagePart value)
		{
			CheckWsdlQName (ctx, value, value.Type);
			CheckWsdlQName (ctx, value, value.Element);
			
			if (value.DefinedByElement && value.Element.Namespace == XmlSchema.Namespace)
				ctx.ReportRuleViolation (value, BasicProfileRules.R2206);
		}
		
		public override void Check (ConformanceCheckContext ctx, Types value)
		{
		}
		
		public override void Check (ConformanceCheckContext ctx, Message value)
		{
		}
		
		public override void Check (ConformanceCheckContext ctx, Binding value)
		{
			SoapBinding sb = (SoapBinding) value.Extensions.Find (typeof(SoapBinding));
			if (sb == null) {
				ctx.ReportRuleViolation (value, BasicProfileRules.R2401);
				return;
			}
			if (sb.Transport == null || sb.Transport == "") {
				ctx.ReportRuleViolation (value, BasicProfileRules.R2701);
				return;
			}
			
			if (sb.Transport != "http://schemas.xmlsoap.org/soap/http")
				ctx.ReportRuleViolation (value, BasicProfileRules.R2702);
			
			LiteralType type = GetLiteralBindingType (value);
			if (type == LiteralType.NotLiteral)
				ctx.ReportRuleViolation (value, BasicProfileRules.R2706);
			else if (type == LiteralType.Inconsistent)
				ctx.ReportRuleViolation (value, BasicProfileRules.R2705);
			
			// Collect all parts referenced from this type
			
			Hashtable parts = new Hashtable ();
			PortType port = ctx.Services.GetPortType (value.Type);
			foreach (Operation op in port.Operations) {
				foreach (OperationMessage om in op.Messages) {
					Message msg = ctx.Services.GetMessage (om.Message);
					foreach (MessagePart part in msg.Parts)
						parts.Add (part,part);
				}
			}
			
			foreach (OperationBinding ob in value.Operations) {
				if (ob.Input != null) CheckMessageBinding (ctx, parts, ob.Input);
				if (ob.Output != null) CheckMessageBinding (ctx, parts, ob.Output);
				foreach (FaultBinding fb in ob.Faults)
					CheckMessageBinding (ctx, parts, fb);
			}
			
			if (parts.Count > 0)
				ctx.ReportRuleViolation (value, BasicProfileRules.R2209);
		}
		
		public override void Check (ConformanceCheckContext ctx, OperationBinding ob) 
		{
		}
		
		void CheckMessageBinding (ConformanceCheckContext ctx, Hashtable portParts, MessageBinding value)
		{
			SoapBodyBinding sbb = (SoapBodyBinding) value.Extensions.Find (typeof(SoapBodyBinding));
			Message msg = FindMessage (ctx, value);
			LiteralType bt = GetLiteralBindingType (value.OperationBinding.Binding);
			
			if (sbb != null) 
			{
				if (bt == LiteralType.Document)
				{
					if (sbb.Parts != null && sbb.Parts.Length > 1)
						ctx.ReportRuleViolation (value, BasicProfileRules.R2201);

					if (sbb.Parts == null) {
						if (msg.Parts != null && msg.Parts.Count > 1)
							ctx.ReportRuleViolation (value, BasicProfileRules.R2210);
						if (msg.Parts.Count == 1)
							portParts.Remove (msg.Parts[0]);
					}
					else {
						if (sbb.Parts.Length == 0 && msg.Parts.Count == 1) {
							portParts.Remove (msg.Parts[0]);
						} else {
							foreach (string part in sbb.Parts) {
								MessagePart mp = msg.FindPartByName (part);
								portParts.Remove (mp);
								if (!mp.DefinedByElement)
									ctx.ReportRuleViolation (value, BasicProfileRules.R2204);
							}
						}
					}
				}
				else if (bt == LiteralType.Rpc) 
				{
					if (sbb.Parts != null) {
						foreach (string part in sbb.Parts) {
							MessagePart mp = msg.FindPartByName (part);
							portParts.Remove (mp);
							if (!mp.DefinedByType)
								ctx.ReportRuleViolation (value, BasicProfileRules.R2203);
						}
					}
				}
			}
			
			SoapHeaderBinding shb = (SoapHeaderBinding) value.Extensions.Find (typeof(SoapHeaderBinding));
			if (shb != null) {
				Message hm = ctx.Services.GetMessage (shb.Message);
				MessagePart mp = hm.FindPartByName (shb.Part);
				portParts.Remove (mp);
				if (mp != null && !mp.DefinedByElement)
					ctx.ReportRuleViolation (value, BasicProfileRules.R2205);
			}
			
			SoapHeaderFaultBinding shfb = (SoapHeaderFaultBinding) value.Extensions.Find (typeof(SoapHeaderFaultBinding));
			if (shfb != null) {
				Message hm = ctx.Services.GetMessage (shfb.Message);
				MessagePart mp = hm.FindPartByName (shfb.Part);
				portParts.Remove (mp);
				if (mp != null && !mp.DefinedByElement)
					ctx.ReportRuleViolation (value, BasicProfileRules.R2205);
			}
			
			// TODO: SoapFaultBinding ??
		}
		
		Message FindMessage (ConformanceCheckContext ctx, MessageBinding mb)
		{
			PortType pt = ctx.Services.GetPortType (mb.OperationBinding.Binding.Type);
			foreach (Operation op in pt.Operations)
				if (op.IsBoundBy (mb.OperationBinding)) {
					OperationMessage om;
					if (mb is InputBinding) om = op.Messages.Input;
					else if (mb is OutputBinding) om = op.Messages.Output;
					else if (mb is FaultBinding) om = op.Faults [mb.Name];
					else return null;
					if (om != null)
						return ctx.Services.GetMessage (om.Message);
					else
						return null;
				}
			return null;
		}
		
		public override void Check (ConformanceCheckContext ctx, Operation value) { }
		public override void Check (ConformanceCheckContext ctx, OperationMessage value) { }
		public override void Check (ConformanceCheckContext ctx, Port value) { }
		public override void Check (ConformanceCheckContext ctx, PortType value) { }
		public override void Check (ConformanceCheckContext ctx, Service value) { }
		
		public override void Check (ConformanceCheckContext ctx, XmlSchema s)
		{
			if (s.TargetNamespace == null || s.TargetNamespace == "") {
				foreach (XmlSchemaObject ob in s.Items)
					if (!(ob is XmlSchemaImport) && !(ob is XmlSchemaAnnotation)) {
						ctx.ReportRuleViolation (s, BasicProfileRules.R2105);
						break;
					}
			}
		}
		
		public override void Check (ConformanceCheckContext ctx, XmlSchemaImport value)
		{
			XmlSchema doc = ctx.GetDocument (value.SchemaLocation) as XmlSchema;
			if (doc == null) ctx.ReportError (value, "Schema '" + value.SchemaLocation + "' not found");
		}
		
		public override void Check (ConformanceCheckContext ctx, XmlSchemaAttribute value)
		{
			CheckSchemaQName (ctx, value, value.RefName);
			CheckSchemaQName (ctx, value, value.SchemaTypeName);
			
			XmlAttribute[] uatts = value.UnhandledAttributes;
			if (uatts != null) {
				foreach (XmlAttribute at in uatts)
					if (at.LocalName == "arrayType" && at.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/")
						ctx.ReportRuleViolation (value, BasicProfileRules.R2111);
			}
		}
		
		public override void Check (ConformanceCheckContext ctx, XmlSchemaAttributeGroupRef value)
		{
			CheckSchemaQName (ctx, value, value.RefName);
		}
		
		public override void Check (ConformanceCheckContext ctx, XmlSchemaComplexContentExtension value)
		{
			CheckSchemaQName (ctx, value, value.BaseTypeName);
			if (value.BaseTypeName.Namespace == "http://schemas.xmlsoap.org/soap/encoding/" && value.BaseTypeName.Name == "Array")
				ctx.ReportRuleViolation (value, BasicProfileRules.R2110);
		}
		
		public override void Check (ConformanceCheckContext ctx, XmlSchemaComplexContentRestriction value)
		{
			CheckSchemaQName (ctx, value, value.BaseTypeName);
			if (value.BaseTypeName.Namespace == "http://schemas.xmlsoap.org/soap/encoding/" && value.BaseTypeName.Name == "Array")
				ctx.ReportRuleViolation (value, BasicProfileRules.R2110);
		}
		
		public override void Check (ConformanceCheckContext ctx, XmlSchemaElement value)
		{
			CheckSchemaQName (ctx, value, value.RefName);
			CheckSchemaQName (ctx, value, value.SubstitutionGroup);
			CheckSchemaQName (ctx, value, value.SchemaTypeName);
		}
		
		public override void Check (ConformanceCheckContext ctx, XmlSchemaGroupRef value)
		{
			CheckSchemaQName (ctx, value, value.RefName);
		}
		
		public override void Check (ConformanceCheckContext ctx, XmlSchemaKeyref value)
		{
			CheckSchemaQName (ctx, value, value.Refer);
		}
		
		public override void Check (ConformanceCheckContext ctx, XmlSchemaSimpleContentExtension value)
		{
			CheckSchemaQName (ctx, value, value.BaseTypeName);
		}
		
		public override void Check (ConformanceCheckContext ctx, XmlSchemaSimpleContentRestriction value)
		{
			CheckSchemaQName (ctx, value, value.BaseTypeName);
		}
		
		public override void Check (ConformanceCheckContext ctx, XmlSchemaSimpleTypeList value)
		{
			CheckSchemaQName (ctx, value, value.ItemTypeName);
		}
		
		public override void Check (ConformanceCheckContext ctx, XmlSchemaSimpleTypeRestriction value)
		{
			CheckSchemaQName (ctx, value, value.BaseTypeName);
		}
		
		public override void Check (ConformanceCheckContext ctx, XmlSchemaSimpleTypeUnion value)
		{
			if (value.MemberTypes != null) {
				foreach (XmlQualifiedName name in value.MemberTypes)
					CheckSchemaQName (ctx, value, name);
			}
		}
		
		// Helper methods
		
		void CheckWsdlQName (ConformanceCheckContext ctx, object element, XmlQualifiedName name)
		{
			if (name == null || name == XmlQualifiedName.Empty) return;
			if (name.Namespace == "" || name.Namespace == XmlSchema.Namespace) return;
			
			if (ctx.ServiceDescription.Types != null && ctx.ServiceDescription.Types.Schemas != null) 
			{
				foreach (XmlSchema s in ctx.ServiceDescription.Types.Schemas)
				{
					if (s.TargetNamespace == name.Namespace) return;
					foreach (XmlSchemaObject i in s.Includes)
						if ((i is XmlSchemaImport) && ((XmlSchemaImport)i).Namespace == name.Namespace) return;
				}
			}
			ctx.ReportRuleViolation (element, BasicProfileRules.R2101);
		}
		
		void CheckSchemaQName (ConformanceCheckContext ctx, object element, XmlQualifiedName name)
		{
			if (name == null || name == XmlQualifiedName.Empty) return;
			if (name.Namespace == "" || name.Namespace == XmlSchema.Namespace) return;
			if (ctx.CurrentSchema.TargetNamespace == name.Namespace) return;
			
			foreach (XmlSchemaObject i in ctx.CurrentSchema.Includes)
				if ((i is XmlSchemaImport) && ((XmlSchemaImport)i).Namespace == name.Namespace) return;
				
			ctx.ReportRuleViolation (element, BasicProfileRules.R2102);
		}
		
		LiteralType GetLiteralBindingType (Binding b)
		{
			SoapBinding sb = (SoapBinding) b.Extensions.Find (typeof(SoapBinding));
			SoapBindingStyle style = (sb != null) ? sb.Style : SoapBindingStyle.Document;
			if (style == SoapBindingStyle.Default) style = SoapBindingStyle.Document;
			
			foreach (OperationBinding ob in b.Operations) {
				SoapOperationBinding sob = (SoapOperationBinding) ob.Extensions.Find (typeof(SoapOperationBinding));
				if (sob.Style != SoapBindingStyle.Default && sob.Style != style)
					return LiteralType.Inconsistent;
				if (ob.Input != null) {
					SoapBodyBinding sbb = (SoapBodyBinding) ob.Input.Extensions.Find (typeof(SoapBodyBinding));
					if (sbb != null && sbb.Use != SoapBindingUse.Literal) return LiteralType.NotLiteral;
					SoapFaultBinding sfb = (SoapFaultBinding) ob.Input.Extensions.Find (typeof(SoapFaultBinding));
					if (sfb != null && sfb.Use != SoapBindingUse.Literal) return LiteralType.NotLiteral;
					SoapHeaderBinding shb = (SoapHeaderBinding) ob.Input.Extensions.Find (typeof(SoapHeaderBinding));
					if (shb != null && shb.Use != SoapBindingUse.Literal) return LiteralType.NotLiteral;
					SoapHeaderFaultBinding shfb = (SoapHeaderFaultBinding) ob.Input.Extensions.Find (typeof(SoapHeaderFaultBinding));
					if (shfb != null && shfb.Use != SoapBindingUse.Literal) return LiteralType.NotLiteral;
				}
				if (ob.Output != null) {
					SoapBodyBinding sbb = (SoapBodyBinding) ob.Output.Extensions.Find (typeof(SoapBodyBinding));
					if (sbb != null && sbb.Use != SoapBindingUse.Literal) return LiteralType.NotLiteral;
					SoapFaultBinding sfb = (SoapFaultBinding) ob.Input.Extensions.Find (typeof(SoapFaultBinding));
					if (sfb != null && sfb.Use != SoapBindingUse.Literal) return LiteralType.NotLiteral;
					SoapHeaderBinding shb = (SoapHeaderBinding) ob.Input.Extensions.Find (typeof(SoapHeaderBinding));
					if (shb != null && shb.Use != SoapBindingUse.Literal) return LiteralType.NotLiteral;
					SoapHeaderFaultBinding shfb = (SoapHeaderFaultBinding) ob.Input.Extensions.Find (typeof(SoapHeaderFaultBinding));
					if (shfb != null && shfb.Use != SoapBindingUse.Literal) return LiteralType.NotLiteral;
				}
			}
			if (style == SoapBindingStyle.Document) return LiteralType.Document;
			else return LiteralType.Rpc;
		}
		
		enum LiteralType {
			NotLiteral,
			Inconsistent,
			Rpc,
			Document
		}
	}
	
	internal class BasicProfileRules
	{
		#region "Basic Profile 1.1 Section 4 (Service Description)"

	// 4.1 Required Description
		// Can't check: R0001

	// 4.2 Document Structure

		// R2028, R2029: schema conformance, depends on underlying XML

		public static readonly ConformanceRule R2001 = new ConformanceRule (
			"R2001", 
			"A DESCRIPTION MUST only use the WSDL \"import\" statement to import another WSDL description",
			"");

		// FIXME: R2803

		public static readonly ConformanceRule R2002 = new ConformanceRule (
			"R2002", 
			"To import XML Schema Definitions, a DESCRIPTION MUST use the XML Schema \"import\" statement",
			"");

		// R2003: depends on ServiceDescription raw XML.
		// R2004, R2009, R2010, R2011: requires schema resolution

		public static readonly ConformanceRule R2007 = new ConformanceRule (
			"R2007", 
			"A DESCRIPTION MUST specify a non-empty location attribute on the wsdl:import element",
			"");

		// R2008: denotes a possibility that cannot be verified.

		// R2022, R2023, R4004: depends on underlying XML

		public static readonly ConformanceRule R4005 = new ConformanceRule (
			"R4005",
			"A DESCRIPTION SHOULD NOT contain the namespace declaration xmlns:xml=\"http://www.w3.org/XML/1998/namespace\"",
			"");

		// R4002, R4003: depends on underlying XML

		public static readonly ConformanceRule R2005 = new ConformanceRule (
			"R2005", 
			"The targetNamespace attribute on the wsdl:definitions element of a description that is being imported MUST have same the value as the namespace attribute on the wsdl:import element in the importing DESCRIPTION",
			"");

		// R2030: is satisfied by API nature (DocumentableItem).

		// R2025: cannot be checked.

		public static readonly ConformanceRule R2026 = new ConformanceRule (
			"R2026", 
			"A DESCRIPTION SHOULD NOT include extension elements with a wsdl:required attribute value of \"true\" on any WSDL construct (wsdl:binding,  wsdl:portType, wsdl:message, wsdl:types or wsdl:import) that claims conformance to the Profile",
			"");

		// R2027: is about the CONSUMER, cannot be checked.

	// 4.3 Types

		public static readonly ConformanceRule R2101 = new ConformanceRule (
			"R2101", 
			"A DESCRIPTION MUST NOT use QName references to elements in namespaces that have been neither imported, nor defined in the referring WSDL document",
			"");
			
		public static readonly ConformanceRule R2102 = new ConformanceRule (
			"R2102", 
			"A QName reference to a Schema component in a DESCRIPTION MUST use the namespace defined in the targetNamespace attribute on the xsd:schema element, or to a namespace defined in the namespace attribute on an xsd:import element within the xsd:schema element",
			"");
			
		public static readonly ConformanceRule R2105 = new ConformanceRule (
			"R2105", 
			"All xsd:schema elements contained in a wsdl:types element of a DESCRIPTION MUST have a targetNamespace attribute with a valid and non-null value, UNLESS the xsd:schema element has xsd:import and/or xsd:annotation as its only child element(s)",
			"");
			
		public static readonly ConformanceRule R2110 = new ConformanceRule (
			"R2110", 
			"In a DESCRIPTION, array declarations MUST NOT extend or restrict the soapenc:Array type",
			"");
			
		public static readonly ConformanceRule R2111 = new ConformanceRule (
			"R2111", 
			"In a DESCRIPTION, array declarations MUST NOT use wsdl:arrayType attribute in the type declaration",
			"");
			
		// FIXME: R2112

		// R2113: is about ENVELOPE.

		// R2114: is satisfied by our processor.

	// 4.4 Messages
	
		public static readonly ConformanceRule R2201 = new ConformanceRule (
			"R2201", 
			"A document-literal binding in a DESCRIPTION MUST, in each of its soapbind:body element(s), have at most one part listed in the parts attribute, if the parts attribute is specified",
			"");

		public static readonly ConformanceRule R2209 = new ConformanceRule (
			"R2209", 
			"A wsdl:binding in a DESCRIPTION SHOULD bind every wsdl:part of a wsdl:message in the wsdl:portType to which it refers to one of soapbind:body, soapbind:header, soapbind:fault  or soapbind:headerfault",
			"");
		
		public static readonly ConformanceRule R2210 = new ConformanceRule (
			"R2210", 
			"If a document-literal binding in a DESCRIPTION does not specify the parts attribute on a soapbind:body element, the corresponding abstract wsdl:message MUST define zero or one wsdl:parts",
			"");

		// R2202: Suggestion.

		public static readonly ConformanceRule R2203 = new ConformanceRule (
			"R2203", 
			"An rpc-literal binding in a DESCRIPTION MUST refer, in its soapbind:body element(s), only to wsdl:part element(s) that have been defined using the type attribute",
			"");

		// R2211: Related to message structure
		// R2207: Optional

		public static readonly ConformanceRule R2204 = new ConformanceRule (
			"R2204", 
			"A document-literal binding in a DESCRIPTION MUST refer, in each of its soapbind:body element(s), only to wsdl:part element(s) that have been defined using the element attribute",
			"");

		// R2208: Optional
		// FIXME: R2212, R2213, R2214

		public static readonly ConformanceRule R2205 = new ConformanceRule (
			"R2205", 
			"A wsdl:binding in a DESCRIPTION MUST refer, in each of its soapbind:header, soapbind:headerfault and soapbind:fault elements, only to wsdl:part element(s) that have been defined using the element attribute",
			"");

		public static readonly ConformanceRule R2206 = new ConformanceRule (
			"R2206", 
			"A wsdl:message in a DESCRIPTION containing a wsdl:part that uses the element attribute MUST refer, in that attribute, to a global element declaration",
			"");

	// 4.5 Port Types

		// FIXME: R2301, R2302, R2303, R2304, R2305, R2306

	// 4.6 Bindings

		public static readonly ConformanceRule R2401 = new ConformanceRule (
			"R2401", 
			"A wsdl:binding element in a DESCRIPTION MUST use WSDL SOAP Binding as defined in WSDL 1.1 Section 3.",
			"");

	// 4.7 SOAP Binding
		
		public static readonly ConformanceRule R2701 = new ConformanceRule (
			"R2701", 
			"The wsdl:binding element in a DESCRIPTION MUST be constructed so that its soapbind:binding child element specifies the transport attribute",
			"");
			
		public static readonly ConformanceRule R2702 = new ConformanceRule (
			"R2702", 
			"A wsdl:binding element in a DESCRIPTION MUST specify the HTTP transport protocol with SOAP binding. Specifically, the transport attribute of its soapbind:binding child MUST have the value \"http://schemas.xmlsoap.org/soap/http\"",
			"");
			
		public static readonly ConformanceRule R2705 = new ConformanceRule (
			"R2705", 
			"A wsdl:binding in a DESCRIPTION MUST use either be a rpc-literal binding or a document-literal binding",
			"");
			
		public static readonly ConformanceRule R2706 = new ConformanceRule (
			"R2706", 
			"A wsdl:binding in a DESCRIPTION MUST use the value of \"literal\" for the use attribute in all soapbind:body, soapbind:fault, soapbind:header and soapbind:headerfault elements",
			"");
			
		// R2709: Suggestion.

		// FIXME: R2710, R2711, R2712, R2714, R2750, R2727,
		// R2716, R2717, R2726, R2718, R2719, R2740, R2741,
		// R2742, R2743, R2720, R2749, R2721, R2754, R2722,
		// R2723, R2707, R2724, R2725, R2729, R2735, R2755,
		// R2737, R2738, R2739, R2753, R2751, R2752, R2744,
		// R2745, R2747, R2748

	// 4.8 Use of XML Schema

		// R2800: satisfied by API nature.
		// R2801: ditto.

		#endregion
	}
}

#endif
