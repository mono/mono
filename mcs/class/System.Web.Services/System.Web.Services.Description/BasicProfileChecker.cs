// 
// System.Web.Services.Description.BasicProfileChecker.cs
//
// Author:
//   Lluis Sanchez (lluis@novell.com)
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

namespace System.Web.Services.Description 
{
	internal class BasicProfileChecker: ConformanceChecker
	{
		public static BasicProfileChecker Instance = new BasicProfileChecker ();
		
		public override WsiClaims Claims { 
			get { return WsiClaims.BP10; }
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
		}
		
		public override void Check (ConformanceCheckContext ctx, Types value)
		{
		}
		
		public override void Check (ConformanceCheckContext ctx, Message value)
		{
			// TODO: R2113
		}
		
		public override void Check (ConformanceCheckContext ctx, Binding value) { }
		public override void Check (ConformanceCheckContext ctx, MessageBinding value) { }
		public override void Check (ConformanceCheckContext ctx, Operation value) { }
		public override void Check (ConformanceCheckContext ctx, OperationBinding value) { }
		public override void Check (ConformanceCheckContext ctx, OperationMessage value) { }
		public override void Check (ConformanceCheckContext ctx, Port value) { }
		public override void Check (ConformanceCheckContext ctx, PortType value) { }
		public override void Check (ConformanceCheckContext ctx, Service value) { }
		
		public override void Check (ConformanceCheckContext ctx, XmlSchema s)
		{
			if (s.TargetNamespace == null || s.TargetNamespace == "") {
				foreach (XmlSchemaObject ob in s.Items)
					if (!(ob is XmlSchemaImport) && !(ob is XmlSchemaAnotation)) {
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
					if (at.LocalName == "arrayType" && at.NamespaceURI == XmlSerializer.WsdlNamespace)
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
			foreach (XmlQualifiedName name in value.MemberTypes)
				CheckSchemaQName (ctx, value, name);
		}
		
		// Helper methods
		
		void CheckWsdlQName (ConformanceCheckContext ctx, object element, XmlQualifiedName name)
		{
			if (name == null || name == XmlQualifiedName.Empty) return;
			if (name.Namespace == "") return;
			
			if (ctx.ServiceDescription.Types != null && ctx.ServiceDescription.Types.Schemas != null) 
			{
				foreach (XmlSchema s in ctx.ServiceDescription.Types.Schemas)
				{
					if (s.TargetNamespace == name.Namespace) return;
					foreach (XmlSchemaImport i in s.Imports)
						if (i.Namespace == name.Namespace) return;
				}
			}
			ctx.ReportRuleViolation (element, BasicProfileRules.R2101);
		}
		
		void CheckSchemaQName (ConformanceCheckContext ctx, XmlSchema schema, object element, XmlQualifiedName name)
		{
			if (name == null || name == XmlQualifiedName.Empty) return;
			if (name.Namespace == "") return;
			if (ctx.CurrentSchema.TargetNamespace == name.Namespace) return;
			
			foreach (XmlSchemaImport i in ctx.CurrentSchema.Imports)
				if (i.Namespace == name.Namespace) return;
				
			ctx.ReportRuleViolation (element, BasicProfileRules.R2102);
		}
	}
	
	internal class BasicProfileRules
	{
		public static readonly ConformanceRule R2001 = new ConformanceRule (
			"R2001", 
			"A DESCRIPTION MUST only use the WSDL \"import\" statement to import another WSDL description",
			"");
			
		public static readonly ConformanceRule R2002 = new ConformanceRule (
			"R2002", 
			"To import XML Schema Definitions, a DESCRIPTION MUST use the XML Schema \"import\" statement",
			"");
			
		public static readonly ConformanceRule R2007 = new ConformanceRule (
			"R2007", 
			"A DESCRIPTION MUST specify a non-empty location attribute on the wsdl:import element",
			"");
			
		public static readonly ConformanceRule R2005 = new ConformanceRule (
			"R2005", 
			"The targetNamespace attribute on the wsdl:definitions element of a description that is being imported MUST have same the value as the namespace attribute on the wsdl:import element in the importing DESCRIPTION",
			"");
			
		public static readonly ConformanceRule R2026 = new ConformanceRule (
			"R2026", 
			"A DESCRIPTION SHOULD NOT include extension elements with a wsdl:required attribute value of \"true\" on any WSDL construct (wsdl:binding,  wsdl:portType, wsdl:message, wsdl:types or wsdl:import) that claims conformance to the Profile",
			"");
			
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
	}
	
	/* 
		The following rules cannot be checked:
		R2002, R2003, R4004, R4003, R2022, R2023, R2004, R2010, R2011
			There is no access to the unerlying xml 
			
		The following are suggestions:
		R2008, R2112
		
		The following are optional
		R4002, R2020, R2021, R2024, R2114
		
		Can't be checked:
		R2025
		
		Process related
		R2027
		
		TODO: section 5.3
	*/
}

#endif
