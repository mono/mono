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

using System.IO;
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
		
		/*
		private string GetAbsoluteUri (string baseUri, string relativeUri)
		{
			string actualBaseUri = baseUri ?? Path.GetFullPath (".") + Path.DirectorySeparatorChar;
			Uri uri = new Uri (new Uri (actualBaseUri), relativeUri);
			return uri.ToString ();
		}
		*/

		public override void Check (ConformanceCheckContext ctx, Import value) 
		{
			if (value.Location == "" || value.Location == null) {
				ctx.ReportRuleViolation (value, BasicProfileRules.R2007);
				return;
			}
			
			if (!new Uri (value.Namespace, UriKind.RelativeOrAbsolute).IsAbsoluteUri)
				ctx.ReportRuleViolation (value, BasicProfileRules.R2803);

			// LAMESPEC: RetrievalUrl does not seem to help here (in .NET)
			//ServiceDescription importer = value.ServiceDescription;
			//string absUri = GetAbsoluteUri (importer != null ? importer.RetrievalUrl : null, value.Location);
			object doc = ctx.GetDocument (/*absUri*/value.Location, value.Namespace);
			if (doc == null) // and looks like .net ignores non-resolvable documentation... I dunno if it makes sense. I don't care :/
				return; //ctx.ReportError (value, "Document '" + value.Location + "' not found");
			
			if (doc is XmlSchema)
				ctx.ReportRuleViolation (value, BasicProfileRules.R2002);
				
			ServiceDescription imported = doc as ServiceDescription;
			if (imported == null) {
				ctx.ReportRuleViolation (value, BasicProfileRules.R2001);
				return;
			}
				
			if (imported.TargetNamespace != value.Namespace)
				ctx.ReportRuleViolation (value, BasicProfileRules.R2005);
		}
		
		public override void Check (ConformanceCheckContext ctx, ServiceDescription value)
		{
			// R4005 (and R1034, which turned out to be redundant)
			if (value.Namespaces != null)
				foreach (XmlQualifiedName qname in value.Namespaces.ToArray ())
					if (qname.Namespace == "http://www.w3.org/XML/1998/namespace")
						ctx.ReportRuleViolation (value, BasicProfileRules.R4005);

			CheckDuplicateSoapAddressBinding (ctx, value);
		}
		
		void CheckDuplicateSoapAddressBinding (ConformanceCheckContext ctx, ServiceDescription value)
		{
			ArrayList locations = new ArrayList ();
			foreach (PortType p in value.PortTypes) {
				SoapAddressBinding b = (SoapAddressBinding) p.Extensions.Find (typeof (SoapAddressBinding));
				if (b == null || b.Location == null || b.Location.Length == 0)
					continue;
				if (locations.Contains (b.Location)) {
					ctx.ReportRuleViolation (value, BasicProfileRules.R2711);
					// One report for one ServiceDescription should be enough.
					return;
				}
				locations.Add (b.Location);
			}
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

			if (value.Type != null && value.Type != XmlQualifiedName.Empty &&
			    value.Element != null && value.Element != XmlQualifiedName.Empty)
				ctx.ReportRuleViolation (value, BasicProfileRules.R2306);
		}
		
		public override void Check (ConformanceCheckContext ctx, Types value)
		{
		}
		
		public override void Check (ConformanceCheckContext ctx, Message value)
		{
		}

		public override void Check (ConformanceCheckContext ctx, BindingCollection value) {
			foreach (Binding b in value)
				foreach (object ext in b.Extensions)
					if (ext.GetType () == typeof (SoapBinding))
						return;

			ctx.ReportRuleViolation (value, BasicProfileRules.R2401);
		}

		public override void Check (ConformanceCheckContext ctx, Binding value)
		{
			SoapBinding sb = (SoapBinding) value.Extensions.Find (typeof(SoapBinding));
			if (sb == null)
				return;

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
						parts [part] = part; // do not use Add() - there could be the same MessagePart instance.
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

			// check existence of corresponding operation in portType for each binding operation
			if (CheckCorrespondingOperationsForBinding (ctx, value, port))
				ctx.ReportRuleViolation (value, BasicProfileRules.R2718);

			// check duplicate operation signature.
			ArrayList sigs = new ArrayList ();
			foreach (OperationBinding ob in value.Operations) {
				if (sigs.Contains (ob.Name))
					ctx.ReportRuleViolation (value, BasicProfileRules.R2710);
				sigs.Add (ob.Name);
			}

			// check namespace declarations.
			switch (type) {
			case LiteralType.Document:
			case LiteralType.Rpc:
				CheckSoapBindingExtensions (ctx, value, type);
				break;
			}
		}
		
		bool CheckCorrespondingOperationsForBinding (ConformanceCheckContext ctx, Binding value, PortType port)
		{
			if (value.Operations.Count != port.Operations.Count)
				return true;
			foreach (OperationBinding b in value.Operations) {
				Operation op = port.Operations.Find (b.Name);
				if (op == null)
					return true;

				bool msg, bind;
				// input
				msg = op.Messages.Input != null;
				bind = b.Input != null;
				if (msg != bind)
					return true;
				// output
				msg = op.Messages.Output != null;
				bind = b.Output != null;
				if (msg != bind)
					return true;
				// faults
				foreach (FaultBinding fb in b.Faults)
					if (op.Messages.Find (fb.Name) == null)
						return true;
			}
			return false;
		}
		
		void CheckSoapBindingExtensions (ConformanceCheckContext ctx, Binding value, LiteralType type)
		{
			bool violationNS = false;
			bool violation2717 = false;
			bool violation2720 = false;
			bool violation2721 = false;

			foreach (OperationBinding op in value.Operations) {
				SoapBodyBinding sbb = op.Extensions.Find (typeof (SoapBodyBinding)) as SoapBodyBinding;
				if (sbb != null) {
					if (type == LiteralType.Document && sbb.Namespace != null)
						violationNS = true;
					if (type == LiteralType.Rpc && sbb.Namespace == null)
						violation2717 = true;
				}

				SoapHeaderBinding shb = op.Extensions.Find (typeof (SoapHeaderBinding)) as SoapHeaderBinding;
				if (shb != null) {
					violationNS |= shb.Namespace != null;
					violation2720 |= !IsValidPart (shb.Part);
				}

				SoapHeaderFaultBinding sfhb = op.Extensions.Find (typeof (SoapHeaderFaultBinding)) as SoapHeaderFaultBinding;
				if (sfhb != null) {
					violationNS |= sfhb.Namespace != null;
					violation2720 |= !IsValidPart (sfhb.Part);
				}

				SoapFaultBinding sfb = op.Extensions.Find (typeof (SoapFaultBinding)) as SoapFaultBinding;
				if (sfb != null) {
					violation2721 |= sfb.Name == null;
					violationNS |= sfb.Namespace != null;
				}
			}
			if (violationNS)
				ctx.ReportRuleViolation (value,
					type == LiteralType.Document ?
					BasicProfileRules.R2716 :
					BasicProfileRules.R2726);
			if (violation2717)
				ctx.ReportRuleViolation (value, BasicProfileRules.R2717);
			if (violation2720)
				ctx.ReportRuleViolation (value, BasicProfileRules.R2720);
			if (violation2721)
				ctx.ReportRuleViolation (value, BasicProfileRules.R2721);
		}
		
		bool IsValidPart (string part)
		{
			if (part == null)
				return false;
			try {
				XmlConvert.VerifyNMTOKEN (part);
				return true;
			} catch (XmlException) {
				return false;
			}
		}
		
		public override void Check (ConformanceCheckContext ctx, OperationBinding value) 
		{
			bool r2754 = false;
			bool r2723 = false;
			foreach (FaultBinding fb in value.Faults) {
				SoapFaultBinding sfb = (SoapFaultBinding) value.Extensions.Find (typeof (SoapFaultBinding));
				if (sfb == null)
					continue;
				r2754 |= sfb.Name != fb.Name;
				r2723 |= sfb.Use == SoapBindingUse.Encoded;
			}
			if (r2754)
				ctx.ReportRuleViolation (value, BasicProfileRules.R2754);
			if (r2723)
				ctx.ReportRuleViolation (value, BasicProfileRules.R2723);
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
		
		public override void Check (ConformanceCheckContext ctx, Operation value)
		{
			switch (value.Messages.Flow) {
			case OperationFlow.SolicitResponse:
			case OperationFlow.Notification:
				ctx.ReportRuleViolation (value, BasicProfileRules.R2303);
				break;
			}

			CheckR2305 (ctx, value);
		}

		void CheckR2305 (ConformanceCheckContext ctx, Operation value)
		{
			string [] order = value.ParameterOrder;
			ServiceDescription sd = value.PortType.ServiceDescription;
			Message omitted = null;
			foreach (OperationMessage m in value.Messages) {
				if (m.Name == null)
					continue; // it is doubtful, but R2305 is not to check such cases anyways.
				Message msg = sd.Messages [m.Name];
				if (msg == null)
					continue; // it is doubtful, but R2305 is not to check such cases anyways.
				foreach (MessagePart p in msg.Parts) {
					if (order != null && Array.IndexOf (order, p.Name) >= 0)
						continue;
					if (omitted == null) {
						omitted = msg;
						continue;
					}
					ctx.ReportRuleViolation (value, BasicProfileRules.R2305);
					return;
				}
			}
		}

		public override void Check (ConformanceCheckContext ctx, OperationMessage value) { }
		public override void Check (ConformanceCheckContext ctx, Port value) { }

		public override void Check (ConformanceCheckContext ctx, PortType value)
		{
			ArrayList names = new ArrayList ();
			foreach (Operation o in value.Operations) {
				if (names.Contains (o.Name))
					ctx.ReportRuleViolation (value, BasicProfileRules.R2304);
				else
					names.Add (o.Name);
			}
		}

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
			// LAMESPEC: same here to Check() for Import.
			XmlSchema doc = ctx.GetDocument (value.SchemaLocation, value.Namespace) as XmlSchema;
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
			if (value.Name != null && value.Name.StartsWith ("ArrayOf", StringComparison.Ordinal))
				ctx.ReportRuleViolation (value, BasicProfileRules.R2112);
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

		// (BTW R1034 turned out to be a spec bug.)

	// 4.1 Required Description
		// Can't check: R0001

	// 4.2 Document Structure

		// R2028, R2029: schema conformance, depends on underlying XML

		public static readonly ConformanceRule R2001 = new ConformanceRule (
			"R2001", 
			"A DESCRIPTION MUST only use the WSDL \"import\" statement to import another WSDL description",
			"");

		public static readonly ConformanceRule R2803 = new ConformanceRule (
			"R2803", 
			"In a DESCRIPTION, the namespace attribute of the wsdl:import MUST NOT be a relative URI.",
			"");

		public static readonly ConformanceRule R2002 = new ConformanceRule (
			"R2002", 
			"To import XML Schema Definitions, a DESCRIPTION MUST use the XML Schema \"import\" statement",
			"");

		// R2003: depends on ServiceDescription raw XML.
		// R2004, R2009, R2010, R2011: requires schema resolution
		// which depends on XmlResolver, while 1) XmlUrlResolver
		// might not always be proper (e.g. network resolution) and
		// 2) custom XmlResolver might resolve non-XML.

		public static readonly ConformanceRule R2007 = new ConformanceRule (
			"R2007", 
			"A DESCRIPTION MUST specify a non-empty location attribute on the wsdl:import element",
			"");

		// R2008: denotes a possibility that cannot be verified.

		// R2022, R2023, R4004: depends on underlying XML, which 
		// is impossible when ServiceDescription is already read
		// (WebServiceInteroperability.CheckConformance() is the case).

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
			
		public static readonly ConformanceRule R2112 = new ConformanceRule (
			"R2112", 
			"In a DESCRIPTION, elements SHOULD NOT be named using the convention ArrayOfXXX.",
			"");

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

		// R2211: Related to ENVELOPE
		// R2207: is about allowed condition (MAY).

		public static readonly ConformanceRule R2204 = new ConformanceRule (
			"R2204", 
			"A document-literal binding in a DESCRIPTION MUST refer, in each of its soapbind:body element(s), only to wsdl:part element(s) that have been defined using the element attribute",
			"");

		// R2208: is about allowed condition (MAY).
		// R2212, R2213, R2214: related to ENVELOPE

		public static readonly ConformanceRule R2205 = new ConformanceRule (
			"R2205", 
			"A wsdl:binding in a DESCRIPTION MUST refer, in each of its soapbind:header, soapbind:headerfault and soapbind:fault elements, only to wsdl:part element(s) that have been defined using the element attribute",
			"");

		public static readonly ConformanceRule R2206 = new ConformanceRule (
			"R2206", 
			"A wsdl:message in a DESCRIPTION containing a wsdl:part that uses the element attribute MUST refer, in that attribute, to a global element declaration",
			"");

	// 4.5 Port Types

		// R2301: Related to ENVELOPE.
		// R2302: Optional

		// btw it's not on Basic Profile TAD
		public static readonly ConformanceRule R2303 = new ConformanceRule (
			"R2303", 
			"A DESCRIPTION MUST NOT use Solicit-Response and Notification type operations in a wsdl:portType definition.",
			"");

		public static readonly ConformanceRule R2304 = new ConformanceRule (
			"R2304", 
			"A wsdl:portType in a DESCRIPTION MUST have operations with distinct values for their name attributes.",
			"");

		public static readonly ConformanceRule R2305 = new ConformanceRule (
			"R2305", 
			"A wsdl:operation element child of a wsdl:portType element in a DESCRIPTION MUST be constructed so that the parameterOrder attribute, if present, omits at most 1 wsdl:part from the output message.",
			"");

		public static readonly ConformanceRule R2306 = new ConformanceRule (
			"R2306", 
			"A wsdl:message in a DESCRIPTION MUST NOT specify both type and element attributes on the same wsdl:part.",
			"");

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

		public static readonly ConformanceRule R2710 = new ConformanceRule (
			"R2710", 
			"The operations in a wsdl:binding in a DESCRIPTION MUST result in operation signatures that are different from one another.",
			"");
			
		public static readonly ConformanceRule R2711 = new ConformanceRule (
			"R2711", 
			"A DESCRIPTION SHOULD NOT have more than one wsdl:port with the same value for the location attribute of the soapbind:address element.",
			"");
			
		// R2712: related to ENVELOPE.
		// R2714: related to INSTANCE.
		// R2750, R2727: related to CONSUMER.

		public static readonly ConformanceRule R2716 = new ConformanceRule (
			"R2716", 
			"A document-literal binding in a DESCRIPTION MUST NOT have the namespace attribute specified on contained soapbind:body, soapbind:header, soapbind:headerfault and soapbind:fault elements.",
			"");
			
		public static readonly ConformanceRule R2717 = new ConformanceRule (
			"R2717", 
			"An rpc-literal binding in a DESCRIPTION MUST have the namespace attribute specified, the value of which MUST be an absolute URI, on contained  soapbind:body elements.",
			"");
			
		public static readonly ConformanceRule R2726 = new ConformanceRule (
			"R2726", 
			"An rpc-literal binding in a DESCRIPTION MUST NOT have the namespace attribute specified on contained soapbind:header,  soapbind:headerfault and soapbind:fault elements.",
			"");
			

		public static readonly ConformanceRule R2718 = new ConformanceRule (
			"R2718", 
			"A wsdl:binding in a DESCRIPTION MUST have the same set of wsdl:operations as the wsdl:portType to which it refers.",
			"");
			

		// R2719: is about allowed condition (MAY).
		// R2740, R2741: no way to detect known faults here.
		// R2742, R2743: related to ENVELOPE.

		public static readonly ConformanceRule R2720 = new ConformanceRule (
			"R2720", 
			"A wsdl:binding in a DESCRIPTION MUST use the part attribute with a schema type of \"NMTOKEN\" on all contained soapbind:header and soapbind:headerfault elements.",
			"");
			

		// R2749: is satisfied by API nature.

		public static readonly ConformanceRule R2721 = new ConformanceRule (
			"R2721", 
			"A wsdl:binding in a DESCRIPTION MUST have the name  attribute specified on all contained soapbind:fault elements.",
			"");

		public static readonly ConformanceRule R2754 = new ConformanceRule (
			"R2754", 
			"In a DESCRIPTION, the value of the name attribute on a soapbind:fault element MUST match the value of the name attribute on its parent wsdl:fault element.",
			"");

		// R2722: is about allowed condition (MAY).

		public static readonly ConformanceRule R2723 = new ConformanceRule (
			"R2723", 
			"f in a wsdl:binding in a DESCRIPTION the use attribute on a contained soapbind:fault element is present, its value MUST be \"literal\".",
			"");

		// R2707: is satisfied by our implementation.
		// R2724, R2725: related to INSTANCE.
		// R2729, R2735: related to ENVELOPE.
		// R2755: related to MESSAGE.
		// R2737, R2738, R2739, R2753: related to ENVELOPE.
		// R2751, R2752: related to ENVELOPE.
		// R2744, R2745: related to MESSAGE.
		// R2747, R2748: related to CONSUMER.

	// 4.8 Use of XML Schema

		// R2800: satisfied by API nature.
		// R2801: ditto.

		#endregion

		/*

		Below are the combination of these documents:
		http://www.ws-i.org/Profiles/BasicProfile-1.1-2004-08-24.html
		http://www.ws-i.org/Testing/Tools/2005/01/BP11_TAD_1-1.htm

		TAD No.	component	recomm.	WS-I Req.
		BP2010	portType		R2304	
		BP2011	types			R2011	
		BP2012	binding			R2204	
		BP2013	binding			R2203	
		BP2014	operation		R2305	
		BP2017	binding			R2705,R2706
		BP2018	definitions		R2023,R2030
		BP2019	binding			R2716	
		BP2020	binding			R2717	
		BP2021	binding			R2720,R2749
		BP2022	binding			R2721	
		BP2032	binding			R2754	
		BP2034	definitions	rec.	R1034,R4005
		BP2098	import			R2007	
		BP2101	definitions		R2001	
		BP2103	definitions		R2003	
		BP2104	definitions		R2005	
		BP2105	definitions		R2022,R2030
		BP2107	types			R2105	
		BP2108	types			R2110,R2111
		BP2110	types		rec.	R2112	
		BP2111	binding			R2201	
		BP2112	binding			R2207	
		BP2113	binding			R2205	
		BP2114	binding		rec.	R2209	
		BP2115	message			R2206	
		BP2116	message			R2306	
		BP2117	binding			R2726	
		BP2118	binding			R2718	
		BP2119	binding			R2210	
		BP2120	binding			R2710	
		BP2122	types			R2801	
		BP2123	definitions	rec.	R2026	
		BP2803	import			R2803	

		*/
	}
}

#endif
