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
		
		public override void Check (ConformanceCheckContext ctx, XmlSchemaObject value)
		{
			if (value is XmlSchemaImport) {
				XmlSchemaImport import = (XmlSchemaImport) value;
				XmlSchema doc = ctx.GetDocument (import.SchemaLocation) as XmlSchema;
				if (doc == null) ctx.ReportError (value, "Schema '" + import.SchemaLocation + "' not found");
				
				// TODO: rule R2004, R2010, R2011
				
			}
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
	}
	
	/* 
		The following rules cannot be checked:
		R2002, R2003, R4004, R4003
			There is no access to the unerlying xml 
	*/
}

#endif
