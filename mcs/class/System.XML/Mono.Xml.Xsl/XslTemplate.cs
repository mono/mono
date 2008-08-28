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
using System.Globalization;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;
using Mono.Xml.Xsl.Operations;
using Mono.Xml.XPath;

using QName = System.Xml.XmlQualifiedName;

namespace Mono.Xml.Xsl {
	internal class XslModedTemplateTable {
		
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
			
			for (int i = 0; i < unnamedTemplates.Count; i++) {
				TemplateWithPriority t = (TemplateWithPriority) unnamedTemplates [i];
				if (t.Matches (node, p))
					return t.Template;
			}

			return null;
		}
	}

	internal class XslTemplateTable {
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

	internal class XslTemplate
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

			if (c.Input.Name == "template" &&
			    c.Input.NamespaceURI == Compiler.XsltNamespace &&
			    c.Input.MoveToAttribute ("mode", String.Empty)) {
				c.Input.MoveToParent ();
				if (!c.Input.MoveToAttribute ("match", String.Empty))
					throw new XsltCompileException ("XSLT 'template' element must not have 'mode' attribute when it does not have 'match' attribute", null, c.Input);
				c.Input.MoveToParent ();
			}

			if (c.Input.NamespaceURI != Compiler.XsltNamespace) {
				this.name = QName.Empty;
				this.match = c.CompilePattern ("/", c.Input);
				this.mode = QName.Empty;
			} else {
				this.name = c.ParseQNameAttribute ("name");
				this.match = c.CompilePattern (c.GetAttribute ("match"), c.Input);
				this.mode = c.ParseQNameAttribute ("mode");
				
				string pri = c.GetAttribute ("priority");
				if (pri != null) {
					try {
						this.priority = double.Parse (pri, CultureInfo.InvariantCulture);
					} catch (FormatException ex) {
						throw new XsltException ("Invalid priority number format", ex, c.Input);
					}
				}
			}
			Parse (c);
			
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
			if (c.Input.NamespaceURI != Compiler.XsltNamespace) {
				content = c.CompileTemplateContent ();
				return;
			}

			if (c.Input.MoveToFirstChild ()) {
				bool alldone = true;
				XPathNavigator contentStart = c.Input.Clone ();
				bool shouldMove = false;
				do {
					if (shouldMove) {
						shouldMove = false;
						contentStart.MoveTo (c.Input);
					}
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
					shouldMove = true;
				} while (c.Input.MoveToNext ());
				if (!alldone) {
					c.Input.MoveTo (contentStart);
					content = c.CompileTemplateContent ();
				}
				c.Input.MoveToParent ();
			}
		}

		string LocationMessage {
			get {
				XslCompiledElementBase op = (XslCompiledElementBase) content;
				return String.Format (" from\nxsl:template {0} at {1} ({2},{3})", Match, style.BaseURI, op.LineNumber, op.LinePosition);
			}
		}

		void AppendTemplateFrame (XsltException ex)
		{
			ex.AddTemplateFrame (LocationMessage);
		}

		public virtual void Evaluate (XslTransformProcessor p, Hashtable withParams)
		{
			if (XslTransform.TemplateStackFrameError) {
				try {
					EvaluateCore (p, withParams);
				} catch (XsltException ex) {
					AppendTemplateFrame (ex);
					throw ex;
				} catch (Exception) {
					// Note that this catch causes different
					// type of error to be thrown (esp.
					// this causes NUnit test regression).
					XsltException e = new XsltException ("Error during XSLT processing: ", null, p.CurrentNode);
					AppendTemplateFrame (e);
					throw e;
				}
			}
			else
				EvaluateCore (p, withParams);
		}

		void EvaluateCore (XslTransformProcessor p, Hashtable withParams)
		{
			if (XslTransform.TemplateStackFrameOutput != null)
				XslTransform.TemplateStackFrameOutput.WriteLine (LocationMessage);

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
	
	internal class XslDefaultNodeTemplate : XslTemplate {
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
	
	internal class XslEmptyTemplate : XslTemplate {

		static XslEmptyTemplate instance = new XslEmptyTemplate ();
		XslEmptyTemplate () : base (null) {}
			
		public static XslTemplate Instance {
			get { return instance; }
		}
		
		public override void Evaluate (XslTransformProcessor p, Hashtable withParams)
		{
		}
	}
	
	internal class XslDefaultTextTemplate: XslTemplate {

		static XslDefaultTextTemplate instance = new XslDefaultTextTemplate ();
		XslDefaultTextTemplate () : base (null) {}
			
		public static XslTemplate Instance {
			get { return instance; }
		}
		
		public override void Evaluate (XslTransformProcessor p, Hashtable withParams)
		{
			if (p.CurrentNode.NodeType == XPathNodeType.Whitespace) {
				if (p.PreserveOutputWhitespace)
					p.Out.WriteWhitespace (p.CurrentNode.Value);
			}
			else
				p.Out.WriteString (p.CurrentNode.Value);
		}
	}
}
