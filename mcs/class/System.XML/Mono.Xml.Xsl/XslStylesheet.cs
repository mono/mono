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

		// Top-level elements
		ArrayList imports = new ArrayList ();
		// [QName]=>XmlSpace
		Hashtable spaceControls = new Hashtable ();
		// [string stylesheet-prefix]=>string result-prefix
		NameValueCollection namespaceAliases = new NameValueCollection ();
		// [QName]=>XmlSpace
		Hashtable parameters = new Hashtable ();
		// [QName]=>ArrayList of XslKey
		Hashtable keys = new Hashtable();
		// [QName]=>XslVariable
		Hashtable variables = new Hashtable ();

		XslTemplateTable templates;

		string baseURI;

		// stylesheet attributes
		string version;
		XmlQualifiedName [] extensionElementPrefixes;
		XmlQualifiedName [] excludeResultPrefixes;
		ArrayList stylesheetNamespaces = new ArrayList ();

		// in-process includes. They must be first parsed as
		// XPathNavigator, collected imports, and then processed
		// other content.
		Hashtable inProcessIncludes = new Hashtable ();

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

		public XslTemplateTable Templates {
			get { return templates; }
		}

		public string BaseURI {
			get { return baseURI; }
		}

		public string Version {
			get { return version; }
		}

		public XslStylesheet ()
		{
		}

		internal void Compile (Compiler c)
		{
			c.PushStylesheet (this);
			
			templates = new XslTemplateTable (this);
			baseURI = c.Input.BaseURI;

			// move to root element
			while (c.Input.NodeType != XPathNodeType.Element)
				if (!c.Input.MoveToNext ())
					throw new XsltCompileException ("Stylesheet root element must be either \"stylesheet\" or \"transform\" or any literal element", null, c.Input);

			if (c.Input.NamespaceURI != XsltNamespace) {
				if (c.Input.GetAttribute ("version", XsltNamespace) == String.Empty)
					throw new XsltCompileException ("Mandatory global attribute version is missing", null, c.Input);
				// then it is simplified stylesheet.
				templates.Add (new XslTemplate (c));
			} else {
				if (c.Input.LocalName != "stylesheet" &&
					c.Input.LocalName != "transform")
					throw new XsltCompileException ("Stylesheet root element must be either \"stylesheet\" or \"transform\" or any literal element", null, c.Input);

				version = c.Input.GetAttribute ("version", "");
				if (version == String.Empty)
					throw new XsltCompileException ("Mandatory attribute version is missing", null, c.Input);

				extensionElementPrefixes = ParseMappedPrefixes (c.GetAttribute ("extension-element-prefixes"), c.Input);
				excludeResultPrefixes = ParseMappedPrefixes (c.GetAttribute ("exclude-result-prefixes"), c.Input);
				if (c.Input.MoveToFirstNamespace (XPathNamespaceScope.Local)) {
					do {
						if (c.Input.Value == XsltNamespace)
							continue;
						this.stylesheetNamespaces.Insert (0, new QName (c.Input.Name, c.Input.Value));
					} while (c.Input.MoveToNextNamespace (XPathNamespaceScope.Local));
					c.Input.MoveToParent ();
				}
				ProcessTopLevelElements (c);
			}

			foreach (XslGlobalVariable v in variables.Values)
				c.AddGlobalVariable (v);
			foreach (ArrayList al in keys.Values)
				for (int i = 0; i < al.Count; i++)
					c.AddKey ((XslKey) al[i]);

			c.PopStylesheet ();
			inProcessIncludes = null;
		}

		private QName [] ParseMappedPrefixes (string list, XPathNavigator nav)
		{
			if (list == null)
				return null;
			ArrayList al = new ArrayList ();
			foreach (string entry in list.Split (XmlChar.WhitespaceChars)) {
				if (entry.Length == 0)
					continue;
				if (entry == "#default")
					al.Add (new QName (String.Empty, String.Empty));
				else {
					string entryNS = nav.GetNamespace (entry);
					if (entryNS != String.Empty)
						al.Add (new QName (entry, entryNS));
				}
			}
			return (QName []) al.ToArray (typeof (QName));
		}

		bool countedSpaceControlExistence;
		bool cachedHasSpaceControls;
		static readonly QName allMatchName = new QName ("*");

		public bool HasSpaceControls {
			get {
				if (!countedSpaceControlExistence) {
					countedSpaceControlExistence = true;
					cachedHasSpaceControls =
						ComputeHasSpaceControls ();
				}
				return cachedHasSpaceControls;
			}
		}

		private bool ComputeHasSpaceControls ()
		{
			if (this.spaceControls.Count > 0
				&& HasStripSpace (spaceControls))
				return true;

			if (imports.Count == 0)
				return false;

			for (int i = 0; i < imports.Count; i++) {
				XslStylesheet s = (XslStylesheet) imports [i];
				if (s.spaceControls.Count > 0 &&
					HasStripSpace (s.spaceControls))
					return true;
			}
			return false;
		}

		private bool HasStripSpace (IDictionary table)
		{
			foreach (XmlSpace space in table.Values)
				if (space == XmlSpace.Default)
					return true;
			return false;
		}

		public bool GetPreserveWhitespace (XPathNavigator nav)
		{
			if (!HasSpaceControls)
				return true;
			nav = nav.Clone ();

			if (!nav.MoveToParent () || nav.NodeType != XPathNodeType.Element) {
				object def = GetDefaultXmlSpace ();
				return def == null || (XmlSpace) def == XmlSpace.Preserve;
			}

			string localName = nav.LocalName;
			string ns = nav.NamespaceURI;

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

			if (o == null)
				o = GetDefaultXmlSpace ();

			if (o != null) {
				switch ((XmlSpace) o) {
				case XmlSpace.Preserve:
					return true;
				case XmlSpace.Default:
					return false;
				}
			}
			throw new SystemException ("Mono BUG: should not reach here");
		}

		object GetDefaultXmlSpace ()
		{
			object o = spaceControls [allMatchName];
			if (o == null) {
				for (int i = 0; i < imports.Count; i++) {
					o = ((XslStylesheet) imports [i]).SpaceControls [allMatchName];
					if (o != null)
						break;
				}
			}
			return o;
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

		private void StoreInclude (Compiler c)
		{
			XPathNavigator including = c.Input.Clone ();
			c.PushInputDocument (c.Input.GetAttribute ("href", String.Empty));
			inProcessIncludes [including] = c.Input;

			HandleImportsInInclude (c);
			c.PopInputDocument ();
		}

		private void HandleImportsInInclude (Compiler c)
		{
			if (c.Input.NamespaceURI != XsltNamespace) {
				if (c.Input.GetAttribute ("version",
					XsltNamespace) == String.Empty)
					throw new XsltCompileException ("Mandatory global attribute version is missing", null, c.Input);
				// simplified style == never imports.
				// Keep this position
				return;
			}

			if (!c.Input.MoveToFirstChild ()) {
				c.Input.MoveToRoot ();
				return;
			}

			HandleIncludesImports (c);
		}

		private void HandleInclude (Compiler c)
		{
			XPathNavigator included = null;
			foreach (XPathNavigator inc in inProcessIncludes.Keys) {
				if (inc.IsSamePosition (c.Input)) {
					included = (XPathNavigator) inProcessIncludes [inc];
					break;
				}
			}
			if (included == null)
				throw new Exception ("Should not happen. Current input is " + c.Input.BaseURI + " / " + c.Input.Name + ", " + inProcessIncludes.Count);

			if (included.NodeType == XPathNodeType.Root)
				return; // Already done.

			c.PushInputDocument (included);
			included.MoveToRoot ();
			included.MoveToFirstChild ();

			while (c.Input.NodeType != XPathNodeType.Element)
				if (!c.Input.MoveToNext ())
					break;

			if (c.Input.NamespaceURI != XsltNamespace &&
				c.Input.NodeType == XPathNodeType.Element) {
				// then it is simplified stylesheet.
				templates.Add (new XslTemplate (c));
			}
			else {
				c.Input.MoveToFirstChild ();
				do {
					if (c.Input.NodeType != XPathNodeType.Element || c.Input.LocalName == "import" && c.Input.NamespaceURI == XsltNamespace)
						continue;
					Debug.EnterNavigator (c);
					HandleTopLevelElement (c);
					Debug.ExitNavigator (c);
				} while (c.Input.MoveToNext ());
			}

			c.Input.MoveToParent ();
			c.PopInputDocument ();
		}
		
		private void HandleImport (Compiler c, string href)
		{
			c.PushInputDocument (href);
			XslStylesheet imported = new XslStylesheet ();
			imported.Compile (c);
			imports.Add (imported);
			c.PopInputDocument ();
		}
		
		private void HandleTopLevelElement (Compiler c)
		{
			XPathNavigator n = c.Input;
			switch (n.NamespaceURI)
			{
			case XsltNamespace:
				switch (n.LocalName)
				{
				case "import":
					throw new XsltCompileException ("Invalid occurence of import element after other top-level content", null, c.Input);
				case "include":
					HandleInclude (c);
					break;
				case "preserve-space":
					AddSpaceControls (c.ParseQNameListAttribute ("elements"), XmlSpace.Preserve, n);
					break;
				
				case "strip-space":
					AddSpaceControls (c.ParseQNameListAttribute ("elements"), XmlSpace.Default, n);
					break;
				case "namespace-alias":
					// do nothing. It is handled in prior.
					break;
				
				case "attribute-set":
					c.AddAttributeSet (new XslAttributeSet (c));
					break;

				case "key":
					XslKey key = new XslKey (c);
					if (keys [key.Name] == null)
						keys [key.Name] = new ArrayList ();
					((ArrayList) keys [key.Name]).Add (key);
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
					XslGlobalVariable gvar = new XslGlobalVariable (c);
					variables [gvar.Name] = gvar;
					break;
				case "param":
					XslGlobalParam gpar = new XslGlobalParam (c);
					variables [gpar.Name] = gpar;
					break;
				default:
					if (version == "1.0")
						throw new XsltCompileException ("Unrecognized top level element after imports", null, c.Input);
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

		private XPathNavigator HandleIncludesImports (Compiler c)
		{
			// process imports. They must precede to other
			// top level elements by schema.
			do {
				if (c.Input.NodeType != XPathNodeType.Element)
					continue;
				if (c.Input.LocalName != "import" ||
					c.Input.NamespaceURI != XsltNamespace)
					break;
				Debug.EnterNavigator (c);
				HandleImport (c, c.GetAttribute ("href"));
				Debug.ExitNavigator (c);
			} while (c.Input.MoveToNext ());

			XPathNavigator saved = c.Input.Clone ();

			// process includes to handle nested imports. They must precede to other
			// top level elements by schema.
			do {
				if (c.Input.NodeType != XPathNodeType.Element ||
					c.Input.LocalName != "include" ||
					c.Input.NamespaceURI != XsltNamespace)
					continue;
				Debug.EnterNavigator (c);
				StoreInclude (c);
				Debug.ExitNavigator (c);
			} while (c.Input.MoveToNext ());

			c.Input.MoveTo (saved);

			return saved;
		}

		private void ProcessTopLevelElements (Compiler c)
		{
			if (!c.Input.MoveToFirstChild ())
				return;

			XPathNavigator saved = HandleIncludesImports (c);

			do {
				// Collect namespace aliases first.
				if (c.Input.NodeType != XPathNodeType.Element ||
					c.Input.LocalName != "namespace-alias" ||
					c.Input.NamespaceURI != XsltNamespace)
					continue;
				string sprefix = (string) c.GetAttribute ("stylesheet-prefix", "");
				if (sprefix == "#default")
					sprefix = String.Empty;
				string rprefix= (string) c.GetAttribute ("result-prefix", "");
				if (rprefix == "#default")
					rprefix = String.Empty;
				namespaceAliases.Set (sprefix, rprefix);
			} while (c.Input.MoveToNext ());

			c.Input.MoveTo (saved);
			do {
				if (c.Input.NodeType != XPathNodeType.Element)
					continue;
				Debug.EnterNavigator (c);
				this.HandleTopLevelElement (c);
				Debug.ExitNavigator (c);
			} while (c.Input.MoveToNext ());
			
			c.Input.MoveToParent ();
		}

		private void AddSpaceControls (QName [] names, XmlSpace result,	XPathNavigator styleElem)
		{
			// XSLT 3.4 - This implementation recovers from errors.
			foreach (QName name in names)
				spaceControls [name] = result;
		}
	}
}
