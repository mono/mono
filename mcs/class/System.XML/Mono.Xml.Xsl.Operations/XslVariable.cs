//
// XslVariable.cs
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

using QName = System.Xml.XmlQualifiedName;

namespace Mono.Xml.Xsl.Operations {
	public abstract class XslGeneralVariable : XslCompiledElement, IXsltContextVariable {
		protected QName name;
		protected XPathExpression select;
		protected XslOperation content;
		protected object value;
		protected bool busy, evaluated;
		
		
		public XslGeneralVariable (Compiler c) : base (c) {}
		
		protected override void Compile (Compiler c)
		{
			c.AssertAttribute ("name");
			name = c.ParseQNameAttribute ("name");
			
			string sel = c.GetAttribute ("select");
			if (sel != null && sel != "" ) {
				select = c.CompileExpression (c.GetAttribute ("select"));
				// TODO assert empty
			} else  if (c.Input.MoveToFirstChild ()) {
				content = c.CompileTemplateContent ();
				c.Input.MoveToParent ();
			}
		}
		
		public override void Evaluate (XslTransformProcessor p) {
			Evaluate (p, false);
		}
		
		protected virtual void Evaluate (XslTransformProcessor p, bool isFromXPath)
		{
			if (!evaluated) {
				if (busy) throw new Exception ("Circular dependency");
				busy = true;
				
				if (select != null) {
					value = p.Evaluate (select);
				} else if (content != null) {
					XmlNodeWriter w = new XmlNodeWriter ();
					p.PushOutput (w);
					content.Evaluate (p);
					p.PopOutput ();
					value = w.Document.CreateNavigator ().SelectChildren (XPathNodeType.All);
				} else {
					value = "";
				}
				
				evaluated = true;
				busy = false;
			}
			
		}
		
		public QName Name {get {return this.name;}}
		
		public XPathResultType VariableType { 
			get {
				if (value != null) return Functions.XPFuncImpl.GetXPathType (value.GetType ());
				return XPathResultType.Any;
			}
		}
		
		public object Evaluate (XsltContext xsltContext)
		{
			if (!evaluated)
				Evaluate (((XsltCompiledContext)xsltContext).Processor, true);
			
			if (value is XPathNodeIterator)			
				return ((XPathNodeIterator)value).Clone ();
			
			return value;
			
		}
		
				
		public virtual void Clear ()
		{
			evaluated = false;
			value = null;
		}
		
		public bool Evaluated { get { return evaluated; }}
		public abstract bool IsLocal { get; }
		public abstract bool IsParam { get; }
	}
	
	public class XslGlobalVariable : XslGeneralVariable {
		public XslGlobalVariable (Compiler c) : base (c) {}
		public override bool IsLocal { get { return false; }}
		public override bool IsParam { get { return false; }}
	}
	
	public class XslGlobalParam : XslGlobalVariable {
		bool overriden;
		object paramVal;
		
		public XslGlobalParam (Compiler c) : base (c) {}
			
		public void Override (object paramVal)
		{
			if (evaluated)
				throw new Exception ("why was i called again?");
			evaluated = true;
			
			this.value = paramVal;
		}
		public override bool IsParam { get { return true; }}
	}
	
	public class XslLocalVariable : XslGeneralVariable {
		protected Stack values;
			
		public XslLocalVariable (Compiler c) : base (c)
		{
			c.AddVariable (this);
		}
		public override bool IsLocal { get { return true; }}
		public override bool IsParam { get { return false; }}
		
		protected override void Evaluate (XslTransformProcessor p, bool isFromXPath)
		{
			if (!evaluated && isFromXPath)
				throw new Exception ("Variable used before decleration");
			
			if (evaluated && !isFromXPath) {
				if (values == null)
					values = new Stack ();
				values.Push (value);
			}
			
			base.Evaluate (p, isFromXPath);
		}
		
		public override void Clear ()
		{
			base.Clear ();
			if (values != null && values.Count != 0) {
				value = values.Pop ();
				evaluated = true;
			}
		}
	}
	
	public class XslLocalParam : XslLocalVariable {
		bool overriden;
		object paramVal;
		
		public XslLocalParam (Compiler c) : base (c) {}
			
		public void Override (object paramVal)
		{
			if (evaluated) {
				if (values == null)
					values = new Stack ();
				values.Push (value);
			}
			evaluated = true;
			
			this.value = paramVal;
		}
		public override bool IsParam { get { return true; }}
	}
	
	public class XslWithParam : XslGeneralVariable {
		public XslWithParam (Compiler c) : base (c) {}
		public override bool IsLocal { get { return false; }}
		public override bool IsParam { get { return false; }}
		
		public object Value {
			get {
				if (!evaluated) throw new Exception ("why wasn't I evaluated");
				if (value is XPathNodeIterator) return ((XPathNodeIterator)value).Clone ();
				return value;
			}
		}
	}
}
