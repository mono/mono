//
// XsltCompiledContext.cs
//
// Authors:
//	Ben Maurer (bmaurer@users.sourceforge.net)
//	Atsushi Enomoto (atsushi@ximian.com)
// (C) 2003 Ben Maurer
// (C) 2004 Novell Inc.
//

using System;
using System.CodeDom;
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
		XslTransformProcessor p;
			
		public XslTransformProcessor Processor { get { return p; }}
			
		public XsltCompiledContext (XslTransformProcessor p) : base (new NameTable ())
		{
			this.p = p;
		}

		public override string DefaultNamespace { get { return String.Empty; }}


		public override string LookupNamespace (string prefix)
		{
			throw new Exception ("we should never get here");
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
				return new XsltExtensionFunction (extension, method);
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
					
					XPathResultType actual = XPFuncImpl.GetXPathType (pi [par].ParameterType);
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
			throw new Exception ("shouldn't get here");
		}
		
		public override IXsltContextFunction ResolveFunction (string prefix, string name, XPathResultType [] ArgTypes)
		{
			throw new Exception ("shouldn't get here");
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
			return p.CompiledStyle.Style.GetPreserveWhitespace (nav.LocalName, nav.NamespaceURI);
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

		struct XsltContextInfo
		{
			public bool IsCData;
			public bool PreserveWhitespace;
		}
		
		XsltContextInfo [] scopes = new XsltContextInfo [40];
		int scopeAt = 0;
		
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
		}
	}
}
