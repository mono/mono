// 
// System.Web.Services.Description.ConformanceChecker.cs
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
	internal abstract class ConformanceChecker
	{
		public abstract WsiClaims Claims { get; }
		public virtual void Check (ConformanceCheckContext ctx, Binding value) { }
		public virtual void Check (ConformanceCheckContext ctx, MessageBinding value) { }
		public virtual void Check (ConformanceCheckContext ctx, Import value) { }
		public virtual void Check (ConformanceCheckContext ctx, Message value) { }
		public virtual void Check (ConformanceCheckContext ctx, MessagePart value) { }
		public virtual void Check (ConformanceCheckContext ctx, Operation value) { }
		public virtual void Check (ConformanceCheckContext ctx, OperationBinding value) { }
		public virtual void Check (ConformanceCheckContext ctx, OperationMessage value) { }
		public virtual void Check (ConformanceCheckContext ctx, Port value) { }
		public virtual void Check (ConformanceCheckContext ctx, PortType value) { }
		public virtual void Check (ConformanceCheckContext ctx, Service value) { }
		public virtual void Check (ConformanceCheckContext ctx, ServiceDescription value) { }
		public virtual void Check (ConformanceCheckContext ctx, Types value) { }
		public virtual void Check (ConformanceCheckContext ctx, ServiceDescriptionFormatExtension value) {}
		public virtual void Check (ConformanceCheckContext ctx, XmlSchemaObject value) {}
	}
	
	internal class ConformanceRule
	{
		public string NormativeStatement;
		public string Details;
		public string Recommendation;
		
		public ConformanceRule (string name, string desc, string rec)
		{
			NormativeStatement = name;
			Details = desc;
			Recommendation = rec;
		}
	}
	
	internal class ConformanceCheckContext
	{
		BasicProfileViolationCollection violations;
		ServiceDescriptionCollection collection;
		WebReference webReference;
		ConformanceChecker checker;		
		public ServiceDescription ServiceDescription;
		
		public ConformanceCheckContext (ServiceDescriptionCollection collection, BasicProfileViolationCollection violations)
		{
			this.collection = collection;
			this.violations = violations;
		}
		
		public ConformanceCheckContext (WebReference webReference, BasicProfileViolationCollection violations)
		{
			this.webReference = webReference;
			this.violations = violations;
		}
		
		public ConformanceChecker Checker {
			get { return checker; }
			set { checker = value; }
		}
		
		public BasicProfileViolationCollection Violations {
			get { return violations; }
		}
		
		public object GetDocument (string url)
		{
			if (collection != null)
				return null;
			else
				return webReference.Documents [url];
		}
		
		public void ReportError (object currentObject, string msg)
		{
			throw new InvalidOperationException (msg + " (" + GetDescription (currentObject) + ")");
		}
		
		public void ReportRuleViolation (object currentObject, ConformanceRule rule)
		{
			BasicProfileViolation v = null;
			foreach (BasicProfileViolation bpv in violations) {
				if (bpv.NormativeStatement == rule.NormativeStatement) {
					v = bpv;
					break;
				}
			}
			
			if (v == null) {
				v = new BasicProfileViolation (checker.Claims, rule);
				violations.Add (v);
			}
			
			v.Elements.Add (GetDescription (currentObject));
		}
		
		string GetDescription (object obj)
		{
			if (obj is ServiceDescription) {
				return "Service Description '" + ServiceDescription.TargetNamespace + "'";
			}
			else if (obj is Binding || obj is Message || obj is PortType || obj is Service) {
				return GetNamedItemDescription (obj, ServiceDescription);
			}
			else if (obj is Import) {
				return GetItemDescription (obj, ServiceDescription, ((Import)obj).Location);
			}
			else if (obj is MessageBinding) {
				return GetNamedItemDescription (obj, ((MessageBinding)obj).OperationBinding);
			}
			else if (obj is MessagePart) {
				return GetNamedItemDescription (obj, ((MessagePart)obj).Message);
			}
			else if (obj is Operation) {
				return GetNamedItemDescription (obj, ((Operation)obj).PortType);
			}
			else if (obj is OperationBinding) {
				return GetNamedItemDescription (obj, ((OperationBinding)obj).Binding);
			}
			else if (obj is OperationMessage) {
				return GetNamedItemDescription (obj, ((OperationMessage)obj).Operation);
			}
			else if (obj is Port) {
				return GetNamedItemDescription (obj, ((Port)obj).Service);
			}
			else if (obj is ServiceDescriptionFormatExtension) {
				ServiceDescriptionFormatExtension ext = (ServiceDescriptionFormatExtension) obj;
				return GetItemDescription (ext, ext.Parent, ext.GetType().Name);
			}
			return obj.GetType().Name;
		}
		
		string GetNamedItemDescription (object item, object parent)
		{
			return item.GetType().Name + " '" + ((NamedItem)item).Name + "', in " + GetDescription (parent);
		}
		
		string GetItemDescription (object item, object parent, string name)
		{
			return item.GetType().Name + " '" + name + "' in " + GetDescription (parent);
		}
	}
}

#endif
