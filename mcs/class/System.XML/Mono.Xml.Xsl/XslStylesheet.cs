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

using System;
using System.CodeDom;
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

	public class XslStylesheet {
		public const string XsltNamespace = "http://www.w3.org/1999/XSL/Transform";
		public const string MSXsltNamespace = "urn:schemas-microsoft-com:xslt::script";
		
		Compiler c;

		XslStylesheet importer;
		// Top-level elements
		ArrayList imports = new ArrayList ();
		// [QName]=>XmlSpace
		Hashtable spaceControls = new Hashtable ();
		// [string stylesheet-prefix]=>string result-prefix
		Hashtable namespaceAliases = new Hashtable ();
		// [QName]=>XmlSpace
		Hashtable parameters = new Hashtable ();
		// [QName]=>XslKey
		Hashtable keys = new Hashtable();

		MSXslScriptManager msScripts = new MSXslScriptManager ();
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
		//  attributes::
		string xpathDefaultNamespace = "";
		XslDefaultValidation defaultValidation = XslDefaultValidation.Lax;

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

		public Hashtable NamespaceAliases {
			get { return namespaceAliases; }
		}

		public Hashtable Parameters {
			get { return parameters; }
		}

		public MSXslScriptManager ScriptManager{
			get { return msScripts; }
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

		public XslStylesheet (Compiler c)
		{
			this.c = c;
			c.PushStylesheet (this);
			
			templates = new XslTemplateTable (this);
			if (c.Input.NamespaceURI != XsltNamespace) {
				// then it is simplified stylesheet.
				Templates.Add (new XslTemplate (c));
			} else {
				version = c.Input.GetAttribute ("version", "");
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
					namespaceAliases.Add (c.GetAttribute ("stylesheet-prefix", ""), c.GetAttribute ("result-prefix", ""));
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
				}
				break;
			case MSXsltNamespace:
				switch (n.LocalName)
				{
				case "script":
					msScripts.AddScript (n);
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
			foreach (QName name in names)
				spaceControls.Add (name, result);
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

			string alias = NamespaceAliases [prefix] as string;
			if (alias != null)
				return alias;

			return prefix;
		}
	}

	
	public enum XslDefaultValidation
	{
		Strict,
		Lax,
		Preserve,
		Strip
	}
}
