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
using Mono.Xml.XPath;

using QName = System.Xml.XmlQualifiedName;

namespace Mono.Xml.Xsl {
	public class XslModedTemplateTable {
		
		class TemplateWithPriority : IComparable {
			public readonly double Priority;
			public readonly XslTemplate Template;
			public readonly Pattern Pattern;
			public readonly int TemplateID;
			
			public TemplateWithPriority (XslTemplate t, Pattern p)
			{
				Template = t;
				Pattern = p;
				Priority = p.DefaultPriority;
				TemplateID = t.Id;
			}
			
			public TemplateWithPriority (XslTemplate t, double p)
			{
				Template = t;
				Pattern = t.Match;
				Priority = p;
				TemplateID = t.Id;
			}
			
			public int CompareTo (object o)
			{
				TemplateWithPriority a = this,
					b = (TemplateWithPriority)o;
				
				//Debug.WriteLine (a.Pattern.ToString () + " ? " + b.Pattern.ToString ());
				//Debug.WriteLine (a.Priority + "   " + b.Priority);
				
				int r0 = a.Priority.CompareTo (b.Priority);
				//Debug.WriteLine (r0);
				if (r0 != 0) return r0;
				
				int r1 = a.TemplateID.CompareTo (b.TemplateID);
				//Debug.WriteLine (r1);
				return r1;
			}
			
			public bool Matches (XPathNavigator n, XslTransformProcessor p)
			{
				//Debug.WriteLine (Pattern.ToString ());
				return p.Matches (Pattern, n);
			}
		}
		
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

		public void Add (XslTemplate t)
		{
			if (!double.IsNaN (t.Priority))
				unnamedTemplates.Add (new TemplateWithPriority (t, t.Priority));
			else
				Add (t, t.Match);
		}
		
		public void Add (XslTemplate t, Pattern p)
		{
			if (p is UnionPattern) {
				Add (t, ((UnionPattern)p).p0);
				Add (t, ((UnionPattern)p).p1);
				return;
			}
			
			unnamedTemplates.Add (new TemplateWithPriority (t, p));
		}
		
		bool sorted = false;
		
		public XslTemplate FindMatch (XPathNavigator node, XslTransformProcessor p)
		{
			//Debug.WriteLine ("...");
			if (!sorted) {
				unnamedTemplates.Sort ();
				unnamedTemplates.Reverse ();
				
				sorted = true;
			}
			
			foreach (TemplateWithPriority t in this.unnamedTemplates)
				if (t.Matches (node, p))
					return t.Template;

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
		
		public XslTemplate FindMatch (XPathNavigator node, XmlQualifiedName mode, XslTransformProcessor p)
		{	
			XslTemplate ret;
			
			if (this [mode] != null)
			{
				ret =  this [mode].FindMatch (node, p);
				if (ret != null) return ret;
			}
			
			for (int i = parent.Imports.Count - 1; i >= 0; i--)
			{
				XslStylesheet s = (XslStylesheet)parent.Imports [i];
				ret = s.Templates.FindMatch (node, mode, p);
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
		Pattern match;
		XmlQualifiedName mode;
		double priority = double.NaN;
		ArrayList parameters;
		XslOperation content;
		
		static int nextId = 0;
		public readonly int Id = nextId ++;

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
				this.mode = QName.Empty;
			} else {
				this.name = c.ParseQNameAttribute ("name");
				this.match = c.CompilePattern (c.GetAttribute ("match"));
				this.mode = c.ParseQNameAttribute ("mode");
				
				string pri = c.GetAttribute ("priority");
				if (pri != null) {
					try {
						this.priority = double.Parse (pri);
					} catch (FormatException ex) {
						throw new XsltException ("Invalid priority number format.", ex, c.Input);
					}
				}
				Parse (c);
			}
			
			stackSize = c.PopScope ().VariableHighTide;
			
		}

		public XmlQualifiedName Name {
			get { return name; }
		}

		public Pattern Match {
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
						this.parameters = new ArrayList ();
					
					parameters.Add (new XslLocalParam (c));
					
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
					int len = parameters.Count;
					for (int i = 0; i < len; i++) {
						XslLocalParam param = (XslLocalParam)parameters [i];
						param.Evaluate (p);
					}
				} else {
					int len = parameters.Count;
					for (int i = 0; i < len; i++) {
						XslLocalParam param = (XslLocalParam)parameters [i];
						object o = withParams [param.Name];
						if (o != null)
							param.Override (p, o);
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
			if (p.CurrentNode.NodeType == XPathNodeType.Whitespace && !p.PreserveWhitespace ())
				return;
			p.Out.WriteString (p.CurrentNode.Value);
		}
	}
}
