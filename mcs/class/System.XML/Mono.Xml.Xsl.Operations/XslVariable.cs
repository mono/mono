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
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;
using Mono.Xml.XPath;

using QName = System.Xml.XmlQualifiedName;

namespace Mono.Xml.Xsl.Operations {
	
	internal class XslVariableInformation
	{
		QName name;
		XPathExpression select;
		XslOperation content;
		
		public XslVariableInformation (Compiler c)
		{

			c.CheckExtraAttributes (c.Input.LocalName, "name", "select");

			c.AssertAttribute ("name");

			name = c.ParseQNameAttribute ("name");
			try {
				XmlConvert.VerifyName (name.Name);
			} catch (XmlException ex) {
				throw new XsltCompileException ("Variable name is not qualified name", ex, c.Input);
			}

			string sel = c.GetAttribute ("select");
			if (sel != null && sel != "" ) {
				select = c.CompileExpression (c.GetAttribute ("select"));
				// TODO assert empty
			} else  if (c.Input.MoveToFirstChild ()) {
				content = c.CompileTemplateContent ();
				c.Input.MoveToParent ();
			}
		}
		
		public object Evaluate (XslTransformProcessor p)
		{
			if (select != null) {
				object o = p.Evaluate (select);
				// To resolve variable references correctly, we
				// have to collect all the target nodes here.
				// (otherwise, variables might be resolved with
				// different level of variable stack in
				// XslTransformProcessor).
				if (o is XPathNodeIterator) {
					ArrayList al = new ArrayList ();
					XPathNodeIterator iter = (XPathNodeIterator) o;
					while (iter.MoveNext ())
						al.Add (iter.Current.Clone ());
					o = new ListIterator (al, p.XPathContext);
				}
				return o;
			} else if (content != null) {
				DTMXPathDocumentWriter2 w = new DTMXPathDocumentWriter2 (p.Root.NameTable, 200);
				Outputter outputter = new GenericOutputter(w, p.Outputs, null, true);
				p.PushOutput (outputter);
				if (p.CurrentNodeset.CurrentPosition == 0)
					p.NodesetMoveNext ();
				content.Evaluate (p);
				p.PopOutput ();
				return w.CreateDocument ().CreateNavigator ();
			} else {
				return "";
			}
		}
		
		public QName Name { get { return name; }}

		internal XPathExpression Select {
			get { return select; }
		}

		internal XslOperation Content {
			get { return content; }
		}
	}
	
	internal abstract class XslGeneralVariable : XslCompiledElement, IXsltContextVariable {
		protected XslVariableInformation var;	
		
		public XslGeneralVariable (Compiler c) : base (c) {}
		
		protected override void Compile (Compiler c)
		{
			if (c.Debugger != null)
				c.Debugger.DebugCompile (this.DebugInput);

			this.var = new XslVariableInformation (c);
		}
		
		public override abstract void Evaluate (XslTransformProcessor p);
		protected abstract object GetValue (XslTransformProcessor p);
		
		
		public object Evaluate (XsltContext xsltContext)
		{	
			object value = GetValue (((XsltCompiledContext)xsltContext).Processor);
			
			if (value is XPathNodeIterator)
				return new WrapperIterator (((XPathNodeIterator)value).Clone (), xsltContext);
			
			return value;
		}

		public QName Name {get {return  var.Name;}}
		public XPathResultType VariableType { get {return XPathResultType.Any;}}
		public abstract bool IsLocal { get; }
		public abstract bool IsParam { get; }
	}
	
	internal class XslGlobalVariable : XslGeneralVariable {
		public XslGlobalVariable (Compiler c) : base (c) {}
		static object busyObject = new Object ();
		
			
		public override void Evaluate (XslTransformProcessor p)
		{
			if (p.Debugger != null)
				p.Debugger.DebugExecute (p, this.DebugInput);

			Hashtable varInfo = p.globalVariableTable;
			
			if (varInfo.Contains (this)) {
				if (varInfo [this] == busyObject)
					throw new XsltException ("Circular dependency was detected", null, p.CurrentNode);
				return;
			}
			
			varInfo [this] = busyObject;
			varInfo [this] = var.Evaluate (p);
			
		}
		
		protected override object GetValue (XslTransformProcessor p)
		{
			Evaluate (p);
			return p.globalVariableTable [this];
		}
			
		public override bool IsLocal { get { return false; }}
		public override bool IsParam { get { return false; }}
	}
	
	internal class XslGlobalParam : XslGlobalVariable {
		
		public XslGlobalParam (Compiler c) : base (c) {}
			
		public void Override (XslTransformProcessor p, object paramVal)
		{
			Debug.Assert (!p.globalVariableTable.Contains (this), "Shouldn't have been evaluated by this point");
			
			p.globalVariableTable [this] = paramVal;
		}
		
		public override bool IsParam { get { return true; }}
	}
	
	internal class XslLocalVariable : XslGeneralVariable {
		protected int slot;
				
		public XslLocalVariable (Compiler c) : base (c)
		{
			slot = c.AddVariable (this);
		}
		
		public override void Evaluate (XslTransformProcessor p)
		{	
			if (p.Debugger != null)
				p.Debugger.DebugExecute (p, this.DebugInput);

			p.SetStackItem (slot, var.Evaluate (p));
		}
		
		protected override object GetValue (XslTransformProcessor p)
		{
			return p.GetStackItem (slot);
		}
		
		public bool IsEvaluated (XslTransformProcessor p)
		{
			return p.GetStackItem (slot) != null;
		}
		
		public override bool IsLocal { get { return true; }}
		public override bool IsParam { get { return false; }}
	}
	
	internal class XslLocalParam : XslLocalVariable {
		
		public XslLocalParam (Compiler c) : base (c) {}
		
		public override void Evaluate (XslTransformProcessor p)
		{
			if (p.Debugger != null)
				p.Debugger.DebugExecute (p, this.DebugInput);

			if (p.GetStackItem (slot) != null)
				return; // evaluated already

			if (p.Arguments != null &&
				var.Select == null &&
				var.Content == null) {
				object val = p.Arguments.GetParam (Name.Name,
					Name.Namespace);
				if (val != null) {
					Override (p, val);
					return;
				}
			}

			base.Evaluate (p);
		}
		
		public void Override (XslTransformProcessor p, object paramVal)
		{
			p.SetStackItem (slot, paramVal);
		}
		
		public override bool IsParam { get { return true; }}
	}
	
	internal class XPathVariableBinding : Expression {
		XslGeneralVariable v;
		public XPathVariableBinding (XslGeneralVariable v)
		{
			this.v = v;
		}
		public override String ToString () { return "$" + v.Name.ToString (); }
		public override XPathResultType ReturnType { get { return XPathResultType.Any; }}
		public override XPathResultType GetReturnType (BaseIterator iter)
		{
			return XPathResultType.Any;
		}
		
		public override object Evaluate (BaseIterator iter)
		{
			return v.Evaluate (iter.NamespaceManager as XsltContext);
		}
	}
}
