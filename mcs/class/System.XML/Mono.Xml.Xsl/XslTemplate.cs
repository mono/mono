//
// XslTemplate.cs
//
// Authors:
//	Ben Maurer (bmaurer@users.sourceforge.net)
//	Atsushi Enomoto (ginga@kit.hi-ho.ne.jp)
//	
// (C) 2003 Ben Maurer
// (C) 2003 Atsushi Enomoto
//

using System;
using System.Collections;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;
using Mono.Xml.Xsl.Operations;

using QName = System.Xml.XmlQualifiedName;

namespace Mono.Xml.Xsl {
	public class XslModedTemplateTable {
		// [QName name]=>XslTemplate
		
		ArrayList unnamedTemplates = new ArrayList ();
		
		XmlQualifiedName mode;

		public XslModedTemplateTable (XmlQualifiedName mode)
		{
			if (mode == null)
				throw new InvalidOperationException ();
			this.mode = mode;
		}

		public XmlQualifiedName Mode {
			get { return mode; }
		}

		public void Add (XslTemplate template)
		{
			unnamedTemplates.Add (template);
		}
		
		public XslTemplate FindMatch (XPathNavigator node)
		{
			foreach (XslTemplate t in this.unnamedTemplates)
				if (node.Matches (t.Match)) return t;
					
			return null;
		}
	}

	public class XslTemplateTable {
		// [QName mode]=>XslTemplateTable
		Hashtable templateTables = new Hashtable ();
		Hashtable namedTemplates = new Hashtable ();
		XslStylesheet parent;
		
		public XslTemplateTable (XslStylesheet parent)
		{
			this.parent = parent;
		}
		
		public Hashtable TemplateTables {
			get { return templateTables; }
		}

		public XslModedTemplateTable this [XmlQualifiedName mode] {
			get {
				return templateTables [mode] as XslModedTemplateTable;
			}
		}

		public void Add (XslTemplate template)
		{
			if (template.Name != XmlQualifiedName.Empty) {
				if (namedTemplates [template.Name] != null)
					throw new InvalidOperationException ("Named template " + template.Name + " is already registered.");
				
				namedTemplates [template.Name] = template;
			}
			
			if (template.Match == null) return;
			
			XslModedTemplateTable tbl = this [template.Mode];
			if (tbl == null) {
				tbl = new XslModedTemplateTable (template.Mode);
				Add (tbl);
			}

			tbl.Add (template);
		}

		public void Add (XslModedTemplateTable table)
		{
			if (this [table.Mode] != null)
				throw new InvalidOperationException ("Mode " + table.Mode + " is already registered.");
			templateTables.Add (table.Mode, table);
		}
		
		public XslTemplate FindMatch (XPathNavigator node, XmlQualifiedName mode)
		{	
			XslTemplate ret;
			
			if (this [mode] != null)
			{
				ret =  this [mode].FindMatch (node);
				if (ret != null) return ret;
			}
			
			for (int i = parent.Imports.Count - 1; i >= 0; i--)
			{
				XslStylesheet s = (XslStylesheet)parent.Imports [i];
				ret = s.Templates.FindMatch (node, mode);
				if (ret != null)
					return ret;
			}
			
			return null;
		}
		
		public XslTemplate FindTemplate (XmlQualifiedName name)
		{
			XslTemplate ret = (XslTemplate)namedTemplates [name];
			
			if (ret != null) return ret;
				
			for (int i = parent.Imports.Count - 1; i >= 0; i--) {
				XslStylesheet s = (XslStylesheet)parent.Imports [i];
				ret = s.Templates.FindTemplate (name);
				if (ret != null)
					return ret;
			}
			
			return null;
		}
	}

	public class XslTemplate
	{
		XmlQualifiedName name;
		XPathExpression match;
		XmlQualifiedName mode;
		double priority;
		Hashtable parameters;
		XslOperation content;

		XslStylesheet style;
		int stackSize;
		
		
		public XslTemplate (Compiler c)
		{
			if (c == null) return; // built in template
			this.style = c.CurrentStylesheet;
			
			c.PushScope ();
			
			if (c.Input.NamespaceURI != Compiler.XsltNamespace) {
				this.name = QName.Empty;
				this.match = null;
				this.priority = 0; // ?
				this.mode = QName.Empty;
			} else {
				this.name = c.ParseQNameAttribute ("name");
				this.match = c.CompilePattern (c.GetAttribute ("match"));
				this.mode = c.ParseQNameAttribute ("mode");
				
				string pri = c.GetAttribute ("priority");
				if (pri != null && pri != "")
					this.priority = double.Parse (pri);
				Parse (c);
			}
			
			stackSize = c.PopScope ().VariableHighTide;
			
		}

		public XmlQualifiedName Name {
			get { return name; }
		}

		public XPathExpression Match {
			get {
				return match;
			}
		}

		public XmlQualifiedName Mode {
			get { return mode; }
		}

		public double Priority {
			get { return priority; }
		}

		public Hashtable Parameters {
			get { return parameters; }
		}
		
		public XslStylesheet Parent {
			get { return style; }
		}

		private void Parse (Compiler c) {
			if (c.Input.MoveToFirstChild ()) {
				bool alldone = true;
				do {
					if (c.Input.NodeType == XPathNodeType.Text)
						{ alldone = false; break; }
					
					if (c.Input.NodeType != XPathNodeType.Element)
						continue;
					if (c.Input.NamespaceURI != Compiler.XsltNamespace)
						{ alldone = false; break; }
					if (c.Input.LocalName != "param")
						{ alldone = false; break; }
					
					if (this.parameters == null)
						this.parameters = new Hashtable ();
					
					XslLocalParam p = new XslLocalParam (c);
					this.parameters [p.Name] = p;
					
				} while (c.Input.MoveToNext ());
				if (!alldone)
					content = c.CompileTemplateContent ();
				c.Input.MoveToParent ();
			}
		}
		
		public virtual void Evaluate (XslTransformProcessor p, Hashtable withParams)
		{
			p.PushStack (stackSize);

			if (parameters != null) {
				if (withParams == null) {
					foreach (XslLocalParam param in parameters.Values)
						param.Evaluate (p);
				} else {
					foreach (XslLocalParam param in parameters.Values)
					{
						if (withParams.Contains (param.Name))
							param.Override (p, withParams [param.Name]);
						else
							param.Evaluate (p);
					}
				}
			}
			
			if (content != null)
				content.Evaluate (p);

			p.PopStack ();
		}
		public void Evaluate (XslTransformProcessor p)
		{
			Evaluate (p, null);
		}
	}
	
	public class XslDefaultNodeTemplate : XslTemplate {
		QName mode;
		
		static XslDefaultNodeTemplate instance = new XslDefaultNodeTemplate (QName.Empty);
		public XslDefaultNodeTemplate (QName mode) : base (null)
		{
			this.mode = mode;
		}
			
		public static XslTemplate Instance {
			get { return instance; }
		}
		
		public override void Evaluate (XslTransformProcessor p, Hashtable withParams)
		{
			p.ApplyTemplates (p.CurrentNode.SelectChildren (XPathNodeType.All), mode, null);
		}
	}
	
	public class XslEmptyTemplate : XslTemplate {

		static XslEmptyTemplate instance = new XslEmptyTemplate ();
		XslEmptyTemplate () : base (null) {}
			
		public static XslTemplate Instance {
			get { return instance; }
		}
		
		public override void Evaluate (XslTransformProcessor p, Hashtable withParams)
		{
		}
	}
	
	public class XslDefaultTextTemplate: XslTemplate {

		static XslDefaultTextTemplate instance = new XslDefaultTextTemplate ();
		XslDefaultTextTemplate () : base (null) {}
			
		public static XslTemplate Instance {
			get { return instance; }
		}
		
		public override void Evaluate (XslTransformProcessor p, Hashtable withParams)
		{
			p.Out.WriteString (p.CurrentNode.Value);
		}
	}
}
