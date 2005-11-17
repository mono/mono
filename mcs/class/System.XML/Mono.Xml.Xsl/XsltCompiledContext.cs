//
// XsltCompiledContext.cs
//
// Authors:
//	Ben Maurer (bmaurer@users.sourceforge.net)
//	Atsushi Enomoto (atsushi@ximian.com)
// (C) 2003 Ben Maurer
// (C) 2004 Novell Inc.
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

using System;
using System.Collections;
using System.Collections.Specialized;
using System.Xml;
using System.Xml.Schema;
using System.Xml.XPath;
using System.Xml.Xsl;
using System.Text;
using System.IO;
using Mono.Xml.Xsl.Operations;
using System.Reflection;

using BF = System.Reflection.BindingFlags;
using QName = System.Xml.XmlQualifiedName;


namespace Mono.Xml.Xsl 
{
	internal class XsltCompiledContext : XsltContext 
	{
		class XsltContextInfo
		{
			public bool IsCData;
			public bool PreserveWhitespace = true;
			public string ElementPrefix;
			public string ElementNamespace;

			public void Clear ()
			{
				IsCData = false;
				PreserveWhitespace = true;
				ElementPrefix = ElementNamespace = null;
			}
		}

		Hashtable keyNameCache = new Hashtable ();
		Hashtable keyIndexTables = new Hashtable ();
		Hashtable patternNavCaches = new Hashtable ();

		XslTransformProcessor p;
		XsltContextInfo [] scopes;
		int scopeAt = 0;
		

		public XslTransformProcessor Processor { get { return p; }}
			
		public XsltCompiledContext (XslTransformProcessor p) : base (new NameTable ())
		{
			this.p = p;
			scopes = new XsltContextInfo [10];
			for (int i = 0; i < 10; i++)
				scopes [i] = new XsltContextInfo ();
		}

		public override string DefaultNamespace { get { return String.Empty; }}

		public XPathNavigator GetNavCache (Mono.Xml.XPath.Pattern p, XPathNavigator node)
		{
			XPathNavigator nav =
				patternNavCaches [p] as XPathNavigator;
			if (nav == null || !nav.MoveTo (node)) {
				nav = node.Clone ();
				patternNavCaches [p] = nav;
			}
			return nav;
		}

		public object EvaluateKey (IStaticXsltContext staticContext,
			BaseIterator iter,
			Expression nameExpr, Expression valueExpr)
		{
			QName name = GetKeyName (staticContext, iter, nameExpr);
			KeyIndexTable table = GetIndexTable (name);
			return table.Evaluate (iter, valueExpr);
		}

		public bool MatchesKey (XPathNavigator nav,
			IStaticXsltContext staticContext,
			string name, string value)
		{
			QName qname = XslNameUtil.FromString (name, staticContext);
			KeyIndexTable table = GetIndexTable (qname);
			return table.Matches (nav, value, this);
		}

		private QName GetKeyName (IStaticXsltContext staticContext,
			BaseIterator iter, Expression nameExpr)
		{
			QName name = null;
			if (nameExpr.HasStaticValue) {
				name = (QName) keyNameCache [nameExpr];
				if (name == null) {
					name = XslNameUtil.FromString (
						nameExpr.EvaluateString (iter),
						staticContext);
					keyNameCache [nameExpr] = name;
				}
			}
			else
				name = XslNameUtil.FromString (
					nameExpr.EvaluateString (iter), this);
			return name;
		}

		private KeyIndexTable GetIndexTable (QName name)
		{
			KeyIndexTable table =
				keyIndexTables [name] as KeyIndexTable;
			if (table == null) {
				table = new KeyIndexTable (this, p.CompiledStyle.ResolveKey (name));
				keyIndexTables [name] = table;
			}
			return table;
		}

		public override string LookupNamespace (string prefix)
		{
			throw new InvalidOperationException ("we should never get here");
		}
		
		internal override IXsltContextFunction ResolveFunction (XmlQualifiedName name, XPathResultType [] argTypes)
		{
			string ns = name.Namespace;

			if (ns == null) return null;

			object extension = null;
			
			if (p.Arguments != null)
				extension = p.Arguments.GetExtensionObject (ns);
			
			bool isScript = false;
			if (extension == null) {
				extension = p.ScriptManager.GetExtensionObject (ns);
				if (extension == null)
					return null;

				isScript = true;
			}
			
			
			MethodInfo method = FindBestMethod (extension.GetType (), name.Name, argTypes, isScript);
			
			if (method != null) 
				return new XsltExtensionFunction (extension, method, p.CurrentNode);
			return null;
		}
		
		MethodInfo FindBestMethod (Type t, string name, XPathResultType [] argTypes, bool isScript)
		{
			int free, length;
			
			MethodInfo [] mi = t.GetMethods ((isScript ? BF.Public | BF.NonPublic : BF.Public) | BF.Instance | BF.Static);
			if (mi.Length == 0)
				return null;
			
			if (argTypes == null)
				return mi [0]; // if we dont have info on the arg types, nothing we can do


			free = 0;
			// filter on name + num args
			int numArgs = argTypes.Length;
			for (int i = 0; i < mi.Length; i ++) {
				if (mi [i].Name == name && mi [i].GetParameters ().Length == numArgs) 
					mi [free++] = mi [i];
			}
			length = free;
			
			// No method
			if (length == 0)
				return null;
			
			// Thats it!
			if (length == 1)
				return mi [0];
			
			free = 0;
			for (int i = 0; i < length; i ++) {
				bool match = true;
				ParameterInfo [] pi = mi [i].GetParameters ();
				
				for (int par = 0; par < pi.Length; par++) {
					XPathResultType required = argTypes [par];
					if (required == XPathResultType.Any)
						continue; // dunno what it is
					
					XPathResultType actual = XPFuncImpl.GetXPathType (pi [par].ParameterType, p.CurrentNode);
					if (actual != required && actual != XPathResultType.Any) {
						match = false;
						break;
					}
					
					if (actual == XPathResultType.Any) {
						// try to get a stronger gind
						if (required != XPathResultType.NodeSet && !(pi [par].ParameterType == typeof (object)))
						{
							match = false;
							break;
						}
					}
				}
				if (match) return mi [i]; // TODO look for exact match
			}
			return null;
		}
			
		public override IXsltContextVariable ResolveVariable (string prefix, string name)
		{
			throw new InvalidOperationException ("shouldn't get here");
		}
		
		public override IXsltContextFunction ResolveFunction (string prefix, string name, XPathResultType [] ArgTypes)
		{
			throw new InvalidOperationException ("XsltCompiledContext exception: shouldn't get here.");
		}
		
		internal override System.Xml.Xsl.IXsltContextVariable ResolveVariable(QName q)
		{
			return p.CompiledStyle.ResolveVariable (q);
		}

		public override int CompareDocument (string baseUri, string nextBaseUri) 
		{
			// it is implementation specific
			return baseUri.GetHashCode ().CompareTo (nextBaseUri.GetHashCode ());
		}

		public override bool PreserveWhitespace (XPathNavigator nav) 
		{
			return p.CompiledStyle.Style.GetPreserveWhitespace (nav);
		}

		public override bool Whitespace { get { return WhitespaceHandling; } }

		// Below are mimicking XmlNamespaceManager ;-)
		public bool IsCData {
			get { return scopes [scopeAt].IsCData; }
			set { scopes [scopeAt].IsCData = value; }
		}
		public bool WhitespaceHandling {
			get { return scopes [scopeAt].PreserveWhitespace; }
			set { scopes [scopeAt].PreserveWhitespace = value; }
		}
		public string ElementPrefix {
			get { return scopes [scopeAt].ElementPrefix; }
			set { scopes [scopeAt].ElementPrefix = value; }
		}
		public string ElementNamespace {
			get { return scopes [scopeAt].ElementNamespace; }
			set { scopes [scopeAt].ElementNamespace = value; }
		}
		
		// precondition scopeAt == scopes.Length
		void ExtendScope ()
		{
			XsltContextInfo [] old = scopes;
			scopes = new XsltContextInfo [scopeAt * 2 + 1];
			if (scopeAt > 0)
				Array.Copy (old, 0, scopes, 0, scopeAt);
		}

		public override bool PopScope ()
		{
			base.PopScope ();

			if (scopeAt == -1)
				return false;
			scopeAt--;
			return true;
		}

		public override void PushScope ()
		{
			base.PushScope ();

			scopeAt++;
			if (scopeAt == scopes.Length)
				ExtendScope ();
			if (scopes [scopeAt] == null)
				scopes [scopeAt] = new XsltContextInfo ();
			else
				scopes [scopeAt].Clear ();
		}
	}
}
