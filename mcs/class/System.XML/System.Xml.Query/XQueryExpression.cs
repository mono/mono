//
// XQueryExpression.cs - abstract syntax tree for XQuery 1.0
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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
#if NET_2_0
using System;
using System.Collections;
using System.Xml;
using System.Xml.Query;
using System.Xml.Schema;
using System.Xml.XPath;
using Mono.Xml.XPath2;

namespace Mono.Xml.XQuery
{
	internal abstract class XmlConstructorExpr : ExprSingle
	{
		public XmlConstructorExpr (ExprSequence content)
		{
			this.content = content;
		}

		ExprSequence content;

		public ExprSequence Content {
			get { return content; }
		}

#region CompileAndEvaluate
		internal override ExprSingle CompileCore (XQueryASTCompiler compiler)
		{
			if (Content != null)
				for (int i = 0; i < Content.Count; i++)
					Content [i] = Content [i].Compile (compiler);
			return this;
		}

		public void SerializeContent (XPathSequence iter)
		{
			if (Content != null)
				foreach (ExprSingle expr in Content)
					expr.Serialize (iter);
		}

		internal IXmlNamespaceResolver GetNSResolver (XPathSequence iter)
		{
			// FIXME: IXmlNamespaceResolver must be constructed
			// considering 1)static context and 2)in-scope element
			// construction.
			return iter.Context;
		}

		public XPathSequence EvaluateNode (XPathSequence iter)
		{
			return EvaluateNode (iter, XPathNodeType.All);
		}
		
		public XPathSequence EvaluateNode (XPathSequence iter, XPathNodeType moveAfterCreation)
		{
			XPathDocument doc = new XPathDocument ();
			XmlWriter w = iter.Context.Writer;
			try {
				iter.Context.Writer = doc.CreateEditor ().AppendChild ();
				Serialize (iter);
				iter.Context.Writer.Close ();
			} finally {
				iter.Context.Writer = w;
			}
			XPathNavigator nav = doc.CreateNavigator ();
			switch (moveAfterCreation) {
			case XPathNodeType.Attribute:
				nav.MoveToFirstAttribute ();
				break;
			case XPathNodeType.Root:
				break;
			default:
				nav.MoveToFirstChild ();
				break;
			}
			return new SingleItemIterator (nav, iter.Context);
		}
#endregion
	}

	internal class XmlAttrConstructorList : CollectionBase
	{
		public XmlAttrConstructorList ()
		{
		}

		public void Add (XmlAttrConstructor item)
		{
			List.Add (item);
		}

		public void Insert (int pos, XmlAttrConstructor item)
		{
			List.Insert (pos, item);
		}
	}

	internal class XmlElemConstructor : XmlConstructorExpr
	{
		XmlQualifiedName name;
		ExprSequence nameExpr;

		public XmlElemConstructor (XmlQualifiedName name, ExprSequence content)
			: base (content)
		{
			this.name = name;
		}

		public XmlElemConstructor (ExprSequence name, ExprSequence content)
			: base (content)
		{
			this.name = XmlQualifiedName.Empty;
			this.nameExpr = name;
		}

		public XmlQualifiedName Name {
			get { return name; }
		}
		public ExprSequence NameExpr {
			get { return nameExpr; }
		}

		internal override void CheckReference (XQueryASTCompiler compiler)
		{
			if (nameExpr != null)
				nameExpr.CheckReference (compiler);
			if (Content != null)
				Content.CheckReference (compiler);
		}

#region CompileAndEvaluate
		internal override ExprSingle CompileCore (XQueryASTCompiler compiler)
		{
			if (NameExpr != null)
				for (int i = 0; i < NameExpr.Count; i++)
					NameExpr [i] = NameExpr [i].Compile (compiler);
			if (Content != null)
				for (int i = 0; i < Content.Count; i++)
					Content [i] = Content [i].Compile (compiler);
			return this;
		}

		// FIXME: can be optimized by checking all items in Expr
		public override SequenceType StaticType {
			get { return SequenceType.Element; }
		}

		public override void Serialize (XPathSequence iter)
		{
			XmlQualifiedName name = EvaluateName (iter);
			XmlWriter w = iter.Context.Writer;
			w.WriteStartElement (iter.Context.LookupPrefix (name.Namespace), name.Name, name.Namespace);
			SerializeContent (iter);
			w.WriteEndElement ();
		}

		public override XPathSequence Evaluate (XPathSequence iter)
		{
			return EvaluateNode (iter);
		}

		private XmlQualifiedName EvaluateName (XPathSequence iter)
		{
			XmlQualifiedName name = Name;
			if (NameExpr != null) {
				XPathAtomicValue value = Atomize (new ExprSequenceIterator (iter, NameExpr));
				IXmlNamespaceResolver res = iter.Context.NSResolver;

				switch (value.XmlType.TypeCode) {
				case XmlTypeCode.QName:
					name = (XmlQualifiedName) value.ValueAs (typeof (XmlQualifiedName), res);
					break;
				case XmlTypeCode.String:
					try {
						name = XmlQualifiedName.Parse (value.Value, res);
					} catch (ArgumentException ex) {
						// FIXME: add more info
						throw new XmlQueryException (String.Format ("The evaluation result of the name expression could not be resolved as a valid QName. Evaluation result string is '{0}'.", value.Value));
					}
					break;
				default:
					// FIXME: add more info
					throw new XmlQueryException ("A name of an element constructor must be resolved to either a QName or string.");
				}
			}
			return name;
		}
#endregion
	}

	internal class XmlAttrConstructor : XmlConstructorExpr
	{
		XmlQualifiedName name;
		ExprSequence nameExpr;

		public XmlAttrConstructor (XmlQualifiedName name, ExprSequence content)
			: base (content)
		{
			this.name = name;
		}

		public XmlAttrConstructor (ExprSequence name, ExprSequence content)
			: base (content)
		{
			this.nameExpr = name;
		}

		internal override void CheckReference (XQueryASTCompiler compiler)
		{
			if (nameExpr != null)
				nameExpr.CheckReference (compiler);
			if (Content != null)
				Content.CheckReference (compiler);
		}

#region CompileAndEvaluate
		internal override ExprSingle CompileCore (XQueryASTCompiler compiler)
		{
			if (NameExpr != null)
				for (int i = 0; i < NameExpr.Count; i++)
					NameExpr [i] = NameExpr [i].Compile (compiler);
			if (Content != null)
				for (int i = 0; i < Content.Count; i++)
					Content [i] = Content [i].Compile (compiler);
			return this;
		}

		public XmlQualifiedName Name {
			get { return name; }
		}
		public ExprSequence NameExpr {
			get { return nameExpr; }
		}

		// FIXME: can be optimized by checking all items in Expr
		public override SequenceType StaticType {
			get { return SequenceType.Attribute; }
		}

		public override void Serialize (XPathSequence iter)
		{
			XmlQualifiedName name = EvaluateName (iter);
			XmlWriter w = iter.Context.Writer;
			w.WriteStartAttribute (GetNSResolver (iter).LookupPrefix (name.Namespace), name.Name, name.Namespace);
			SerializeContent (iter);
			w.WriteEndAttribute ();
		}

		public override XPathSequence Evaluate (XPathSequence iter)
		{
			return EvaluateNode (iter, XPathNodeType.Attribute);
		}

		private XmlQualifiedName EvaluateName (XPathSequence iter)
		{
			XmlQualifiedName name = Name;
			if (NameExpr != null) {
				XPathAtomicValue value = Atomize (new ExprSequenceIterator (iter, NameExpr));
				IXmlNamespaceResolver res = GetNSResolver (iter);

				switch (value.XmlType.TypeCode) {
				case XmlTypeCode.QName:
					name = (XmlQualifiedName) value.ValueAs (typeof (XmlQualifiedName), res);
					break;
				case XmlTypeCode.String:
					try {
						// nonprefixed attribute name == element's local namespace
						if (value.Value.IndexOf (':') < 0)
							name = new XmlQualifiedName (value.Value);
						else
							name = XmlQualifiedName.Parse (value.Value, res);
					} catch (ArgumentException ex) {
						// FIXME: add more info
						throw new XmlQueryException (String.Format ("The evaluation result of the name expression could not be resolved as a valid QName. Evaluation result string is '{0}'.", value.Value));
					}
					break;
				default:
					// FIXME: add more info
					throw new XmlQueryException ("A name of an attribute constructor must be resolved to either a QName or string.");
				}
			}
			return name;
		}
#endregion
	}

	internal class XmlNSConstructor : XmlConstructorExpr
	{
		public XmlNSConstructor (string prefix, ExprSequence content)
			: base (content)
		{
		}

		internal override void CheckReference (XQueryASTCompiler compiler)
		{
			Content.CheckReference (compiler);
		}

#region CompileAndEvaluate
		internal override ExprSingle CompileCore (XQueryASTCompiler compiler)
		{
			if (Content != null)
				for (int i = 0; i < Content.Count; i++)
					Content [i] = Content [i].Compile (compiler);
			return this;
		}

		// FIXME: can be optimized by checking all items in Expr
		public override SequenceType StaticType {
			get { return SequenceType.Namespace; }
		}

		public override void Serialize (XPathSequence iter)
		{
			// TBD
			throw new NotImplementedException ();
		}

		public override XPathSequence Evaluate (XPathSequence iter)
		{
			// TBD
			throw new NotImplementedException ();
		}
#endregion
	}

	internal class XmlDocConstructor : XmlConstructorExpr
	{
		public XmlDocConstructor (ExprSequence content)
			: base (content)
		{
		}

		internal override void CheckReference (XQueryASTCompiler compiler)
		{
			if (Content != null)
				Content.CheckReference (compiler);
		}

#region CompileAndEvaluate
		internal override ExprSingle CompileCore (XQueryASTCompiler compiler)
		{
			if (Content != null)
				for (int i = 0; i < Content.Count; i++)
					Content [i] = Content [i].Compile (compiler);
			return this;
		}

		// FIXME: can be optimized by checking all items in Expr
		public override SequenceType StaticType {
			get { return SequenceType.Document; }
		}

		public override void Serialize (XPathSequence iter)
		{
			XmlWriter w = iter.Context.Writer;
			w.WriteStartDocument ();
			SerializeContent (iter);
			w.WriteEndDocument ();
		}

		public override XPathSequence Evaluate (XPathSequence iter)
		{
			return EvaluateNode (iter, XPathNodeType.Root);
		}
#endregion
	}

	internal class XmlTextConstructor : XmlConstructorExpr
	{
		public XmlTextConstructor (string text)
			: base (null)
		{
			this.text = text;
		}

		public XmlTextConstructor (ExprSequence content)
			: base (content)
		{
		}

		string text;

		public string LiteralText {
			get { return text; }
		}

		internal override void CheckReference (XQueryASTCompiler compiler)
		{
			if (Content != null)
				Content.CheckReference (compiler);
		}

#region CompileAndEvaluate
		internal override ExprSingle CompileCore (XQueryASTCompiler compiler)
		{
			if (Content != null)
				for (int i = 0; i < Content.Count; i++)
					Content [i] = Content [i].Compile (compiler);
			return this;
		}

		public override SequenceType StaticType {
			get { return SequenceType.Text; }
		}

		public override void Serialize (XPathSequence iter)
		{
			if (Content != null)
				iter.Context.Writer.WriteString (Atomize (new ExprSequenceIterator (iter, Content)).Value);
			else
				iter.Context.Writer.WriteString (LiteralText);
		}

		public override XPathSequence Evaluate (XPathSequence iter)
		{
			return EvaluateNode (iter);
		}
#endregion
	}

	internal class XmlCommentConstructor : XmlConstructorExpr
	{
		string contentLiteral;

		public XmlCommentConstructor (string content)
			: base (null)
		{
			this.contentLiteral = content;
		}

		public XmlCommentConstructor (ExprSequence content)
			: base (content)
		{
		}

		internal override void CheckReference (XQueryASTCompiler compiler)
		{
			if (Content != null)
				Content.CheckReference (compiler);
		}

#region CompileAndEvaluate
		internal override ExprSingle CompileCore (XQueryASTCompiler compiler)
		{
			if (Content != null)
				for (int i = 0; i < Content.Count; i++)
					Content [i] = Content [i].Compile (compiler);
			return this;
		}

		// FIXME: can be optimized by checking all items in Expr
		public override SequenceType StaticType {
			get { return SequenceType.Comment; }
		}

		public override void Serialize (XPathSequence iter)
		{
			iter.Context.Writer.WriteComment (Atomize (new ExprSequenceIterator (iter, Content)).Value);
		}

		public override XPathSequence Evaluate (XPathSequence iter)
		{
			return EvaluateNode (iter);
		}
#endregion
	}

	internal class XmlPIConstructor : XmlConstructorExpr
	{
		string name;
		ExprSequence nameExpr;

		string contentLiteral;

		public XmlPIConstructor (string name, string content)
			: base (null)
		{
			this.name = name;
			this.contentLiteral = content;
		}

		public XmlPIConstructor (string name, ExprSequence content)
			: base (content)
		{
			this.name = name;
		}

		public XmlPIConstructor (ExprSequence name, ExprSequence content)
			: base (content)
		{
			this.nameExpr = name;
		}

		internal override void CheckReference (XQueryASTCompiler compiler)
		{
			if (nameExpr != null)
				nameExpr.CheckReference (compiler);
			if (Content != null)
				Content.CheckReference (compiler);
		}

#region CompileAndEvaluate
		internal override ExprSingle CompileCore (XQueryASTCompiler compiler)
		{
			if (NameExpr != null)
				for (int i = 0; i < NameExpr.Count; i++)
					NameExpr [i] = NameExpr [i].Compile (compiler);
			if (Content != null)
				for (int i = 0; i < Content.Count; i++)
					Content [i] = Content [i].Compile (compiler);
			return this;
		}

		public string Name {
			get { throw new NotImplementedException (); }
		}
		public ExprSequence NameExpr {
			get { throw new NotImplementedException (); }
		}

		// FIXME: can be optimized by checking all items in Expr
		public override SequenceType StaticType {
			get { return SequenceType.XmlPI; }
		}

		public override void Serialize (XPathSequence iter)
		{
			iter.Context.Writer.WriteProcessingInstruction (
				GetName (iter),
				Atomize (new ExprSequenceIterator (iter, Content)).Value);
		}

		public override XPathSequence Evaluate (XPathSequence iter)
		{
			return EvaluateNode (iter);
		}

		private string GetName (XPathSequence iter)
		{
			if (Name != String.Empty)
				return Name;
			return Atomize (new ExprSequenceIterator (iter, NameExpr)).Value;
		}
#endregion
	}
}

#endif
