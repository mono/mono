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
using System.Xml.Schema;
using System.Xml.XPath;
using Mono.Xml.XPath2;

namespace Mono.Xml.XQuery
{
	public class ElementConstructor : ExprSingle
	{
		public ElementConstructor (XmlQualifiedName name, AttributeConstructorList attributes, ExprSequence content)
		{
		}
	}

	public class AttributeConstructorList
	{
		ArrayList list = new ArrayList ();

		public AttributeConstructorList ()
		{
		}

		public void Add (AttributeConstructor item)
		{
			list.Add (item);
		}

		public void Insert (int pos, AttributeConstructor item)
		{
			list.Insert (pos, item);
		}
	}

	public class AttributeConstructor
	{
		public AttributeConstructor (XmlQualifiedName name, ExprSequence content)
		{
		}
	}

	public class XmlElemConstructor : ExprSingle
	{
		XmlQualifiedName name;
		ExprSequence nameExpr;
		ExprSequence content;

		public XmlElemConstructor (XmlQualifiedName name, ExprSequence content)
		{
		}

		public XmlElemConstructor (ExprSequence name, ExprSequence content)
		{
		}
	}

	public class XmlAttrConstructor : ExprSingle
	{
		XmlQualifiedName name;
		ExprSequence nameExpr;
		ExprSequence content;

		public XmlAttrConstructor (XmlQualifiedName name, ExprSequence content)
		{
		}

		public XmlAttrConstructor (ExprSequence name, ExprSequence content)
		{
		}
	}

	public class XmlNSConstructor : ExprSingle
	{
		public XmlNSConstructor (string prefix, ExprSequence content)
		{
		}
	}

	public class XmlDocConstructor : ExprSingle
	{
		public XmlDocConstructor (ExprSequence content)
		{
		}
	}

	public class XmlTextConstructor : ExprSingle
	{
		public XmlTextConstructor (string text)
		{
		}

		public XmlTextConstructor (ExprSequence content)
		{
		}
	}

	public class XmlCommentConstructor : ExprSingle
	{
		string contentLiteral;
		ExprSequence contentExpr;

		public XmlCommentConstructor (string content)
		{
			this.contentLiteral = content;
		}

		public XmlCommentConstructor (ExprSequence content)
		{
			this.contentExpr = content;
		}
	}

	public class XmlPIConstructor : ExprSingle
	{
		string name;
		ExprSequence nameExpr;

		string contentExpr;
		ExprSequence content;

		public XmlPIConstructor (string name, string content)
		{
		}

		public XmlPIConstructor (string name, ExprSequence content)
		{
		}

		public XmlPIConstructor (ExprSequence name, ExprSequence content)
		{
		}
	}
}

#endif
