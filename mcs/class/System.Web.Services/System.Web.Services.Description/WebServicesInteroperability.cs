// 
// System.Web.Services.Description.WebServicesInteroperability.cs
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

using System.Collections;
using System.Xml.Schema;

namespace System.Web.Services.Description 
{
	public sealed class WebServicesInteroperability
	{
		private WebServicesInteroperability ()
		{
		}
		
		public static bool CheckConformance (WsiProfiles claims, ServiceDescription service, BasicProfileViolationCollection violations)
		{
			ServiceDescriptionCollection col = new ServiceDescriptionCollection ();
			col.Add (service);
			ConformanceCheckContext ctx = new ConformanceCheckContext (col, violations);
			return Check (claims, ctx, col);
		}

		public static bool CheckConformance (WsiProfiles claims, ServiceDescriptionCollection services, BasicProfileViolationCollection violations)
		{
			ConformanceCheckContext ctx = new ConformanceCheckContext (services, violations);
			return Check (claims, ctx, services);
		}

		public static bool CheckConformance (WsiProfiles claims, WebReference webReference, BasicProfileViolationCollection violations)
		{
			ConformanceCheckContext ctx = new ConformanceCheckContext (webReference, violations);
			return Check (claims, ctx, webReference.Documents.Values);
		}
		
		static bool Check (WsiProfiles claims, ConformanceCheckContext ctx, IEnumerable documents)
		{
			ConformanceChecker[] checkers = GetCheckers (claims);
			if (checkers == null) return true;
			
			foreach (object doc in documents) {
				if (!(doc is ServiceDescription)) continue;
				
				foreach (ConformanceChecker c in checkers)
					Check (ctx, c, (ServiceDescription)doc);
			}
				
			return ctx.Violations.Count == 0;
		}
		
		internal static ConformanceChecker[] GetCheckers (WsiProfiles claims)
		{
			if ((claims & WsiProfiles.BasicProfile1_1) != 0)
				return new ConformanceChecker[] { BasicProfileChecker.Instance };
			return null;
		}

		internal static void Check (ConformanceCheckContext ctx, ConformanceChecker checker, Binding b)
		{
			checker.Check (ctx, b);
			CheckExtensions (ctx, checker, b.Extensions);

			foreach (OperationBinding oper in b.Operations) {
				CheckExtensions (ctx, checker, oper.Extensions);

				foreach (MessageBinding mb in oper.Faults) {
					checker.Check (ctx, mb);
					CheckExtensions (ctx, checker, mb.Extensions);
				}

				checker.Check (ctx, oper.Input);
				CheckExtensions (ctx, checker, oper.Input.Extensions);

				if (oper.Output != null) {
					checker.Check (ctx, oper.Output);
					CheckExtensions (ctx, checker, oper.Output.Extensions);
				}
			}
		}
		
		static void Check (ConformanceCheckContext ctx, ConformanceChecker checker, ServiceDescription sd)
		{
			ctx.ServiceDescription = sd;
			ctx.Checker = checker;
			
			checker.Check (ctx, sd);
			CheckExtensions (ctx, checker, sd.Extensions);
			
			foreach (Import i in sd.Imports) {
				checker.Check (ctx, i);
			}
			
			foreach (Service s in sd.Services) {
				checker.Check (ctx, s);
				foreach (Port p in s.Ports) {
					checker.Check (ctx, p);
					CheckExtensions (ctx, checker, p.Extensions);
				}
			}

			checker.Check (ctx, sd.Bindings);
			foreach (Binding b in sd.Bindings)
				Check (ctx, checker, b);
			
			foreach (PortType pt in sd.PortTypes)
			{
				checker.Check (ctx, pt);
				
				foreach (Operation oper in pt.Operations) {
					checker.Check (ctx, oper);
					foreach (OperationMessage msg in oper.Messages)
						checker.Check (ctx, msg);
					
					foreach (OperationMessage msg in oper.Faults)
						checker.Check (ctx, msg);
				}
			}
			
			foreach (Message msg in sd.Messages)
			{
				checker.Check (ctx, msg);
				foreach (MessagePart part in msg.Parts)
					checker.Check (ctx, part);
			}
			
			if (sd.Types != null) {
				checker.Check (ctx, sd.Types);
				if (sd.Types.Schemas != null) {
					foreach (XmlSchema s in sd.Types.Schemas) {
						ctx.CurrentSchema = s;
						checker.Check (ctx, s);
						CheckObjects (ctx, checker, new Hashtable (), s.Items);
					}
				}
			}
		}
		
		static void CheckObjects (ConformanceCheckContext ctx, ConformanceChecker checker, Hashtable visitedObjects, XmlSchemaObjectCollection col)
		{
			foreach (XmlSchemaObject item in col)
				Check (ctx, checker, visitedObjects, item);
		}
		
		static void Check (ConformanceCheckContext ctx, ConformanceChecker checker, Hashtable visitedObjects, XmlSchemaObject value)
		{
			if (value == null) return;
			
			if (visitedObjects.Contains (value)) return;
			visitedObjects.Add (value, value);
			
			if (value is XmlSchemaImport) {
				XmlSchemaImport so = (XmlSchemaImport) value;
				checker.Check (ctx, so);
			}
			else if (value is XmlSchemaAll) {
				XmlSchemaAll so = (XmlSchemaAll) value;
				checker.Check (ctx, so);
				CheckObjects (ctx, checker, visitedObjects, so.Items);
			}
			else if (value is XmlSchemaAnnotation) {
				XmlSchemaAnnotation so = (XmlSchemaAnnotation) value;
				checker.Check (ctx, so);
				CheckObjects (ctx, checker, visitedObjects, so.Items);
			}
			else if (value is XmlSchemaAttribute) {
				XmlSchemaAttribute so = (XmlSchemaAttribute) value;
				checker.Check (ctx, so);
			}
			else if (value is XmlSchemaAttributeGroup) {
				XmlSchemaAttributeGroup so = (XmlSchemaAttributeGroup) value;
				checker.Check (ctx, so);
				CheckObjects (ctx, checker, visitedObjects, so.Attributes);
				Check (ctx, checker, visitedObjects, so.AnyAttribute);
				Check (ctx, checker, visitedObjects, so.RedefinedAttributeGroup);
			}
			else if (value is XmlSchemaAttributeGroupRef) {
				XmlSchemaAttributeGroupRef so = (XmlSchemaAttributeGroupRef) value;
				checker.Check (ctx, so);
			}
			else if (value is XmlSchemaChoice) {
				XmlSchemaChoice so = (XmlSchemaChoice) value;
				checker.Check (ctx, so);
				CheckObjects (ctx, checker, visitedObjects, so.Items);
			}
			else if (value is XmlSchemaComplexContent) {
				XmlSchemaComplexContent so = (XmlSchemaComplexContent) value;
				checker.Check (ctx, so);
				Check (ctx, checker, visitedObjects, so.Content);
			}
			else if (value is XmlSchemaComplexContentExtension) {
				XmlSchemaComplexContentExtension so = (XmlSchemaComplexContentExtension) value;
				checker.Check (ctx, so);
				Check (ctx, checker, visitedObjects, so.Particle);
				CheckObjects (ctx, checker, visitedObjects, so.Attributes);
				Check (ctx, checker, visitedObjects, so.AnyAttribute);
			}
			else if (value is XmlSchemaComplexContentRestriction) {
				XmlSchemaComplexContentRestriction so = (XmlSchemaComplexContentRestriction) value;
				checker.Check (ctx, so);
				Check (ctx, checker, visitedObjects, so.Particle);
				CheckObjects (ctx, checker, visitedObjects, so.Attributes);
				Check (ctx, checker, visitedObjects, so.AnyAttribute);
			}
			else if (value is XmlSchemaComplexType) {
				XmlSchemaComplexType so = (XmlSchemaComplexType) value;
				checker.Check (ctx, so);
				Check (ctx, checker, visitedObjects, so.ContentModel);
				Check (ctx, checker, visitedObjects, so.Particle);
				CheckObjects (ctx, checker, visitedObjects, so.Attributes);
				Check (ctx, checker, visitedObjects, so.AnyAttribute);
				Check (ctx, checker, visitedObjects, so.ContentTypeParticle);
				Check (ctx, checker, visitedObjects, so.AttributeWildcard);
			}
			else if (value is XmlSchemaElement) {
				XmlSchemaElement so = (XmlSchemaElement) value;
				checker.Check (ctx, so);
				Check (ctx, checker, visitedObjects, so.SchemaType);
				CheckObjects (ctx, checker, visitedObjects, so.Constraints);
			}
			else if (value is XmlSchemaGroup) {
				XmlSchemaGroup so = (XmlSchemaGroup) value;
				checker.Check (ctx, so);
				Check (ctx, checker, visitedObjects, so.Particle);
			}
			else if (value is XmlSchemaGroupRef) {
				XmlSchemaGroupRef so = (XmlSchemaGroupRef) value;
				checker.Check (ctx, so);
			}
			else if (value is XmlSchemaIdentityConstraint) {
				XmlSchemaIdentityConstraint so = (XmlSchemaIdentityConstraint) value;
				checker.Check (ctx, so);
				CheckObjects (ctx, checker, visitedObjects, so.Fields);
				Check (ctx, checker, visitedObjects, so.Selector);
			}
			else if (value is XmlSchemaKeyref) {
				XmlSchemaKeyref so = (XmlSchemaKeyref) value;
				checker.Check (ctx, so);
			}
			else if (value is XmlSchemaRedefine) {
				XmlSchemaRedefine so = (XmlSchemaRedefine) value;
				checker.Check (ctx, so);
				CheckObjects (ctx, checker, visitedObjects, so.Items);
			}
			else if (value is XmlSchemaSequence) {
				XmlSchemaSequence so = (XmlSchemaSequence) value;
				checker.Check (ctx, so);
				CheckObjects (ctx, checker, visitedObjects, so.Items);
			}
			else if (value is XmlSchemaSimpleContent) {
				XmlSchemaSimpleContent so = (XmlSchemaSimpleContent) value;
				checker.Check (ctx, so);
				Check (ctx, checker, visitedObjects, so.Content);
			}
			else if (value is XmlSchemaSimpleContentExtension) {
				XmlSchemaSimpleContentExtension so = (XmlSchemaSimpleContentExtension) value;
				checker.Check (ctx, so);
				CheckObjects (ctx, checker, visitedObjects, so.Attributes);
				Check (ctx, checker, visitedObjects, so.AnyAttribute);
			}
			else if (value is XmlSchemaSimpleContentRestriction) {
				XmlSchemaSimpleContentRestriction so = (XmlSchemaSimpleContentRestriction) value;
				checker.Check (ctx, so);
				CheckObjects (ctx, checker, visitedObjects, so.Attributes);
				Check (ctx, checker, visitedObjects, so.AnyAttribute);
				CheckObjects (ctx, checker, visitedObjects, so.Facets);
			}
			else if (value is XmlSchemaSimpleType) {
				XmlSchemaSimpleType so = (XmlSchemaSimpleType) value;
				checker.Check (ctx, so);
				Check (ctx, checker, visitedObjects, so.Content);
			}
			else if (value is XmlSchemaSimpleTypeList) {
				XmlSchemaSimpleTypeList so = (XmlSchemaSimpleTypeList) value;
				checker.Check (ctx, so);
			}
			else if (value is XmlSchemaSimpleTypeRestriction) {
				XmlSchemaSimpleTypeRestriction so = (XmlSchemaSimpleTypeRestriction) value;
				checker.Check (ctx, so);
				CheckObjects (ctx, checker, visitedObjects, so.Facets);
			}
			else if (value is XmlSchemaSimpleTypeUnion) {
				XmlSchemaSimpleTypeUnion so = (XmlSchemaSimpleTypeUnion) value;
				checker.Check (ctx, so);
			}
		}
				
		
		static void CheckExtensions (ConformanceCheckContext ctx, ConformanceChecker checker, ServiceDescriptionFormatExtensionCollection extensions)
		{
			foreach (object o in extensions) {
				ServiceDescriptionFormatExtension ext = o as ServiceDescriptionFormatExtension;
				if (ext != null)
					checker.Check (ctx, ext);
			}
		}
	}
}

#endif
