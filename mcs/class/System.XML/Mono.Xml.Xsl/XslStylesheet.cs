//
// XslStylesheet.cs
//
// Authors:
//	Ben Maurer (bmaurer@users.sourceforge.net)
//	Atsushi Enomoto (ginga@kit.hi-ho.ne.jp)
//	
// (C) 2003 Ben Maurer
// (C) 2003 Atsushi Enomoto
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
using System.IO;

using Mono.Xml.Xsl.Operations;

using QName = System.Xml.XmlQualifiedName;

namespace Mono.Xml.Xsl {

	internal class XslStylesheet {
		public const string XsltNamespace = "http://www.w3.org/1999/XSL/Transform";
		public const string MSXsltNamespace = "urn:schemas-microsoft-com:xslt";
		
		Compiler c;

		XslStylesheet importer;
		// Top-level elements
		ArrayList imports = new ArrayList ();
		// [QName]=>XmlSpace
		Hashtable spaceControls = new Hashtable ();
		// [string stylesheet-prefix]=>string result-prefix
		NameValueCollection namespaceAliases = new NameValueCollection ();
		// [QName]=>XmlSpace
		Hashtable parameters = new Hashtable ();
		// [QName]=>XslKey
		Hashtable keys = new Hashtable();

		XslTemplateTable templates;

		// stylesheet attributes
		string version;
		XmlQualifiedName [] extensionElementPrefixes;
		XmlQualifiedName [] excludeResultPrefixes;
		ArrayList stylesheetNamespaces = new ArrayList ();

		// below are newly introduced in XSLT 2.0
		//  elements::
		// xsl:import-schema should be interpreted into it.
		XmlSchemaCollection schemas = new XmlSchemaCollection ();
		// [QName]=>XslCharacterMap
		Hashtable characterMap = new Hashtable ();
		// [QName]=>XslDateFormat
		Hashtable dateFormats = new Hashtable ();
		// [QName]=>XslFunction
		Hashtable functions = new Hashtable ();
		// [QName]=>XslSortKey
		Hashtable sortKeys = new Hashtable ();

		public string BaseUri {
			get { return c.Input.BaseURI; }
		}

		public XmlQualifiedName [] ExtensionElementPrefixes {
			get { return extensionElementPrefixes; }
		}

		public XmlQualifiedName [] ExcludeResultPrefixes {
			get { return excludeResultPrefixes; }
		}

		public ArrayList StylesheetNamespaces {
			get { return stylesheetNamespaces; }
		}

		public ArrayList Imports {
			get { return imports; }
		}

		public Hashtable SpaceControls {
			get { return spaceControls; }
		}

		public NameValueCollection NamespaceAliases {
			get { return namespaceAliases; }
		}

		public Hashtable Parameters {
			get { return parameters; }
		}

		public XPathNavigator StyleDocument {
			get { return c.Input; }
		}

		public XslTemplateTable Templates {
			get { return templates; }
		}

		public Hashtable Keys {
			get { return keys; }
		}

		public string Version {
			get { return version; }
		}

		public XslStylesheet (Compiler c)
		{
			this.c = c;
			c.PushStylesheet (this);
			
			templates = new XslTemplateTable (this);

			// move to root element
			while (c.Input.NodeType != XPathNodeType.Element)
				if (!c.Input.MoveToNext ())
					throw new XsltCompileException ("Stylesheet root element must be either \"stylesheet\" or \"transform\" or any literal element.", null, c.Input);

			if (c.Input.NamespaceURI != XsltNamespace) {
				if (c.Input.GetAttribute ("version", XsltNamespace) == null)
					throw new XsltCompileException ("Mandatory global attribute version is missing.", null, c.Input);
				// then it is simplified stylesheet.
				Templates.Add (new XslTemplate (c));
			} else {
				if (c.Input.LocalName != "stylesheet" &&
					c.Input.LocalName != "transform")
					throw new XsltCompileException ("Stylesheet root element must be either \"stylesheet\" or \"transform\" or any literal element.", null, c.Input);

				version = c.Input.GetAttribute ("version", "");
				if (version == null)
					throw new XsltCompileException ("Mandatory attribute version is missing.", null, c.Input);

				extensionElementPrefixes = c.ParseQNameListAttribute ("extension-element-prefixes");
				excludeResultPrefixes = c.ParseQNameListAttribute ("exclude-result-prefixes");
				if (c.Input.MoveToFirstNamespace (XPathNamespaceScope.Local)) {
					do {
						if (c.Input.Value == XsltNamespace)
							continue;
						this.stylesheetNamespaces.Insert (0, new QName (c.Input.Name, c.Input.Value));
					} while (c.Input.MoveToNextNamespace (XPathNamespaceScope.Local));
					c.Input.MoveToParent ();
				}
				ProcessTopLevelElements ();
			}
			
			c.PopStylesheet ();
		}
		
		public XslKey FindKey (QName name)
		{
			XslKey key = Keys [name] as XslKey;
			if (key != null)
				return key;
			for (int i = Imports.Count - 1; i >= 0; i--) {
				key = ((XslStylesheet) Imports [i]).FindKey (name);
				if (key != null)
					return key;
			}
			return null;
		}

		bool countedSpaceControlExistence;
		bool cachedHasSpaceControls;
		public bool HasSpaceControls {
			get {
				if (!countedSpaceControlExistence) {
					countedSpaceControlExistence = true;
					if (this.spaceControls.Count > 0)
						cachedHasSpaceControls = true;
					else if (imports.Count == 0)
						cachedHasSpaceControls = false;
					else {
						for (int i = 0; i < imports.Count; i++)
							if (((XslStylesheet) imports [i]).spaceControls.Count > 0)
								countedSpaceControlExistence = true;
						cachedHasSpaceControls = false;
					}
				}
				return cachedHasSpaceControls;
			}
		}

		public bool GetPreserveWhitespace (string localName, string ns)
		{
			if (!HasSpaceControls)
				return true;

			XmlQualifiedName qname = new XmlQualifiedName (localName, ns);
			object o = spaceControls [qname];
			if (o == null) {

				for (int i = 0; i < imports.Count; i++) {
					o = ((XslStylesheet) imports [i]).SpaceControls [qname];
					if (o != null)
						break;
				}
			}

			if (o == null) {
				qname = new XmlQualifiedName ("*", ns);
				o = spaceControls [qname];
				if (o == null) {
					for (int i = 0; i < imports.Count; i++) {
						o = ((XslStylesheet) imports [i]).SpaceControls [qname];
						if (o != null)
							break;
					}
				}
			}

			if (o == null) {
				qname = new XmlQualifiedName ("*", String.Empty);
				o = spaceControls [qname];
				if (o == null) {
					for (int i = 0; i < imports.Count; i++) {
						o = ((XslStylesheet) imports [i]).SpaceControls [qname];
						if (o != null)
							break;
					}
				}
			}

			if (o != null) {
				XmlSpace space = (XmlSpace) o;
				switch ((XmlSpace) o) {
				case XmlSpace.Preserve:
					return true;
				case XmlSpace.Default:
					return false;
				}
			}
			return true;
		}

		bool countedNamespaceAliases;
		bool cachedHasNamespaceAliases;
		public bool HasNamespaceAliases {
			get {
				if (!countedNamespaceAliases) {
					countedNamespaceAliases = true;
					if (namespaceAliases.Count > 0)
						cachedHasNamespaceAliases = true;
					else if (imports.Count == 0)
						cachedHasNamespaceAliases = false;
					else {
						for (int i = 0; i < imports.Count; i++)
							if (((XslStylesheet) imports [i]).namespaceAliases.Count > 0)
								countedNamespaceAliases = true;
						cachedHasNamespaceAliases = false;
					}
				}
				return cachedHasNamespaceAliases;
			}
		}

		public string GetActualPrefix (string prefix)
		{
			if (!HasNamespaceAliases)
				return prefix;

			string result = namespaceAliases [prefix];
			if (result == null) {
				for (int i = 0; i < imports.Count; i++) {
					result = ((XslStylesheet) imports [i]).namespaceAliases [prefix];
					if (result != null)
						break;
				}
			}

			return result != null ? result : prefix;
		}

		private XslStylesheet (Compiler c, XslStylesheet importer) : this (c)
		{
			this.importer = importer;
		}
		
		private void HandleInclude (string href)
		{
			c.PushInputDocument (href);
			ProcessTopLevelElements ();
			c.PopInputDocument ();
		}
		
		private void HandleImport (string href)
		{
			c.PushInputDocument (href);
			imports.Add (new XslStylesheet (c, this));
			c.PopInputDocument ();
		}
		
		private void HandleTopLevelElement ()
		{
			XPathNavigator n = c.Input;
			switch (n.NamespaceURI)
			{
			case XsltNamespace:
				
				switch (n.LocalName)
				{
				case "include":
					HandleInclude (c.GetAttribute ("href"));
					break;
				case "import":
					HandleImport (c.GetAttribute ("href"));
					break;
				case "preserve-space":
					AddSpaceControls (c.ParseQNameListAttribute ("elements"), XmlSpace.Preserve, n);
					break;
				
				case "strip-space":
					AddSpaceControls (c.ParseQNameListAttribute ("elements"), XmlSpace.Default, n);
					break;
				
				case "namespace-alias":
					namespaceAliases.Add ((string) c.GetAttribute ("stylesheet-prefix", ""), (string) c.GetAttribute ("result-prefix", ""));
					break;
				
				case "attribute-set":
					c.AddAttributeSet (new XslAttributeSet (c));
					break;

				case "key":
					keys.Add (c.ParseQNameAttribute ("name"), new XslKey (c));
					break;
					
				case "output":
					c.CompileOutput ();
					break;
				
				case "decimal-format":
					c.CompileDecimalFormat ();
					break;
					
				case "template":
					templates.Add (new XslTemplate (c));	
					break;
				case "variable":
					c.AddGlobalVariable (new XslGlobalVariable (c));
					break;
				case "param":
					c.AddGlobalVariable (new XslGlobalParam (c));
					break;
				default:
					if (version == "1.0")
						throw new XsltCompileException ("Unrecognized top level element.", null, c.Input);
					break;
				}
				break;
			case MSXsltNamespace:
				switch (n.LocalName)
				{
				case "script":
					c.ScriptManager.AddScript (c);
					break;
				}
				break;
			}
		}
		
		private void ProcessTopLevelElements ()
		{
			if (c.Input.MoveToFirstChild ()) {
				do {
					if (c.Input.NodeType == XPathNodeType.Element) {					
						Debug.EnterNavigator (c);
						this.HandleTopLevelElement();
						Debug.ExitNavigator (c);
					}
				} while (c.Input.MoveToNext ());
				
				c.Input.MoveToParent ();
			}
		}

		private void AddSpaceControls (QName [] names, XmlSpace result,	XPathNavigator styleElem)
		{
			// XSLT 3.4 - This implementation recovers from errors.
			foreach (QName name in names)
				spaceControls [name] = result;
		}

		public string PrefixInEffect (string prefix, ArrayList additionalExcluded)
		{
			if (additionalExcluded != null && additionalExcluded.Contains (prefix == String.Empty ? "#default" : prefix))
				return null;
			if (prefix == "#default")
				prefix = String.Empty;

			if (ExcludeResultPrefixes != null) {
				bool exclude = false;
				foreach (XmlQualifiedName exc in ExcludeResultPrefixes)
					if (exc.Name == "#default" && prefix == String.Empty || exc.Name == prefix) {
						exclude = true;
						break;
					}
				if (exclude)
					return null;
			}

			if (ExtensionElementPrefixes != null) {
				bool exclude = false;
				foreach (XmlQualifiedName exc in ExtensionElementPrefixes)
					if (exc.Name == "#default" && prefix == String.Empty || exc.Name == prefix) {
						exclude = true;
						break;
					}
				if (exclude)
					return null;
			}

			return GetActualPrefix (prefix);
		}
	}

	
	internal enum XslDefaultValidation
	{
		Strict,
		Lax,
		Preserve,
		Strip
	}
}
