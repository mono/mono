//
// XQueryDefaultFunctionCall.cs
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

//
// XQuery 1.0 and XPath 2.0 Functions implementation as XPathItemExpression.
// See XQuery 1.0 and XPath 2.0 Functions and Operators.
//
#if NET_2_0
using System;
using System.Collections;
using System.Reflection;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Query;
using System.Xml.XPath;
using System.Xml.Xsl;

namespace Mono.Xml.XPath2
{
	internal abstract class DefaultFunctionCall : FunctionCallExprBase
	{
		static Hashtable qnameTable = new Hashtable ();
		static XmlQualifiedName GetName (string name)
		{
			XmlQualifiedName qname = qnameTable [name] as XmlQualifiedName;
			if (qname == null) {
				qname = new XmlQualifiedName (name, XQueryFunction.Namespace);
				qnameTable.Add (name, qname);
			}
			return qname;
		}

		public DefaultFunctionCall (XQueryStaticContext ctx, string name, int minArgs, int maxArgs, SequenceType type, ExprSequence args)
			: base (GetName (name), args)
		{
			this.type = type;
			this.minArgs = minArgs;
			this.maxArgs = maxArgs;
		}

		SequenceType type;
		int minArgs;
		int maxArgs;

		public override int MinArgs { get { return minArgs; } }
		public override int MaxArgs { get { return maxArgs; } }

		public override SequenceType StaticType {
			get { return type; }
		}
	}

	// Accessors

	// 2.1 fn:node-name ($arg as node ()?) as xs:QName?
	internal class FnNodeNameCall : DefaultFunctionCall
	{
		public FnNodeNameCall (XQueryStaticContext ctx, ExprSequence args)
			: base (ctx, "node-name", 1, 1, SequenceType.Create (XmlSchemaSimpleType.XsQName, Occurence.Optional), args)
		{
		}

		public override XPathSequence Evaluate (XPathSequence iter)
		{
			XPathSequence res = Args [0].Evaluate (iter);
			if (!res.MoveNext ())
				return new XPathEmptySequence (iter.Context);
			// FIXME: what happens if context item is not a node.
			XPathNavigator nav = res.Current as XPathNavigator;
			if (nav == null || nav.LocalName == String.Empty)
				return new XPathEmptySequence (iter.Context);
			return new SingleItemIterator (new XPathAtomicValue (new XmlQualifiedName (nav.LocalName, nav.NamespaceURI), XmlSchemaSimpleType.XsQName), iter);
		}
	}

	// 2.2 fn:nilled ($arg as node()) as xs:boolean?
	internal class FnNilledCall : DefaultFunctionCall
	{
		public FnNilledCall (XQueryStaticContext ctx, XPathItemExpression [] args)
			: base (ctx, "nilled", 1, 1, SequenceType.Create (XmlSchemaSimpleType.XsBoolean, Occurence.One), args)
		{
		}

		public override XPathSequence Evaluate (XPathSequence iter)
		{
			XPathSequence res = Args [0].Evaluate (iter);
			if (!res.MoveNext ())
				return new XPathEmptySequence (iter.Context);
			XPathNavigator nav = res.Current as XPathNavigator;
			IXmlSchemaInfo info = nav.NodeType == XPathNodeType.Element ? nav.SchemaInfo : null;
			if (info != null)
				return new SingleItemIterator (new XPathAtomicValue (info.IsNil, null), iter);
			else
				return new XPathEmptySequence (iter.Context);
		}
	}

	// 2.3 fn:string ($arg as item()?) as xs:string
	internal class FnStringCall : DefaultFunctionCall
	{
		public FnStringCall (XQueryStaticContext ctx, XPathItemExpression [] args)
			: base (ctx, "string", 0, 1, SequenceType.Create (XmlSchemaSimpleType.XsString, Occurence.Optional), args)
		{
		}

		public override XPathSequence Evaluate (XPathSequence iter)
		{
			XPathItem item = null;
			if (Args.Length == 0)
				item = iter.Context.CurrentItem;
			else {
				XPathSequence res = Args [0].Evaluate (iter);
				if (!res.MoveNext ())
					return new XPathEmptySequence (iter.Context);
				item = res.Current;
			}
			return new SingleItemIterator (new XPathAtomicValue (Core (item), null), iter);
		}

		private string Core (XPathItem item)
		{
			XPathNavigator nav = item as XPathNavigator;
			
			return nav != null ? nav.Value : XQueryConvert.ItemToString (item);
		}
	}

	// 2.4 fn:data ($arg as item()*) as xdt:anyAtomicType*
	internal class FnDataCall : DefaultFunctionCall
	{
		public FnDataCall (XQueryStaticContext ctx, XPathItemExpression [] args)
			: base (ctx, "data", 1, 1, SequenceType.Create (XmlSchemaComplexType.AnyType, Occurence.ZeroOrMore), args)
		{
		}

		public override XPathSequence Evaluate (XPathSequence iter)
		{
			return new AtomizingIterator (Args [0].Evaluate (iter));
		}
	}

	// 2.5 fn:base-uri ($arg as node()?) as xs:anyURI?
	internal class FnBaseUriCall : DefaultFunctionCall
	{
		public FnBaseUriCall (XQueryStaticContext ctx, XPathItemExpression [] args)
			: base (ctx, "base-uri", 1, 1, SequenceType.Create (XmlSchemaSimpleType.XsAnyUri, Occurence.Optional), args)
		{
		}

		public override XPathSequence Evaluate (XPathSequence iter)
		{
			XPathSequence res = Args [0].Evaluate (iter);
			if (res.MoveNext ())
				return new XPathEmptySequence (iter.Context);
			XPathNavigator nav = res.Current as XPathNavigator;
			if (nav == null)
				return new XPathEmptySequence (iter.Context);
			else
				return new SingleItemIterator (new XPathAtomicValue (nav.BaseURI, XmlSchemaSimpleType.XsString), iter);
		}
	}

	// 2.6 fn:document-uri ($arg as node()?) as xs:anyURI?
	internal class FnDocumentUriCall : DefaultFunctionCall
	{
		public FnDocumentUriCall (XQueryStaticContext ctx, XPathItemExpression [] args)
			: base (ctx, "document-uri", 1, 1, SequenceType.Create (XmlSchemaSimpleType.XsAnyUri, Occurence.Optional), args)
		{
		}

		public override XPathSequence Evaluate (XPathSequence iter)
		{
			XPathSequence res = Args [0].Evaluate (iter);
			if (res.MoveNext ())
				return new XPathEmptySequence (iter.Context);
			XPathNavigator nav = res.Current as XPathNavigator;
			if (nav == null)
				return new XPathEmptySequence (iter.Context);
			nav = nav.Clone ();
			nav.MoveToRoot ();
			return new SingleItemIterator (new XPathAtomicValue (nav.BaseURI, null), iter);
		}
	}

	// 3 fn:error ()
	//   fn:error ($error as xs:QName)
	//   fn:error ($error as xs:QName, $description as xs:string)
	//   fn:error ($error as xs:QName, $description as xs:string, $error-object as item()*)
	internal class FnErrorCall : DefaultFunctionCall
	{
		public FnErrorCall (XQueryStaticContext ctx, XPathItemExpression [] args)
			// FIXME: return type is actually none
			: base (ctx, "error", 0, 3, SequenceType.AnyType, args)
		{
		}

		public override XPathSequence Evaluate (XPathSequence iter)
		{
			// error name
			XPathSequence errorNameIter = Args.Length > 0 ? Args [0].Evaluate (iter) : null;
			XmlQualifiedName errorType = XmlQualifiedName.Empty;
			if (errorNameIter != null && errorNameIter.MoveNext ())
				errorType = XQueryConvert.ItemToQName (errorNameIter.Current);

			// description
			string description = Args.Length > 1 ? Args [1].EvaluateAsString (iter) : String.Empty;

			// error-object
			XPathSequence errorObjIter = Args.Length > 2 ? Args [2].Evaluate (iter) : null;

			// FIXME: add error-object information
			throw new XmlQueryException (errorType + description);
		}
	}

	// 4 trace ($value as item()*, $label as xs:string) as item()*
	internal class FnTraceCall : DefaultFunctionCall
	{
		public FnTraceCall (XQueryStaticContext ctx, XPathItemExpression [] args)
			: base (ctx, "trace", 2, 2, SequenceType.Create (XmlSchemaComplexType.AnyType, Occurence.ZeroOrMore), args)
		{
		}

		public override XPathSequence Evaluate (XPathSequence  iter)
		{
			return new TracingIterator (Args [0].Evaluate (iter), Args [1].EvaluateAsString (iter));
		}
	}

	// 5 constructor functions
	internal class AtomicConstructorCall : DefaultFunctionCall
	{
		// FIXME: use IXmlNamespaceResolver.LookupPrefix() in ctx
		public AtomicConstructorCall (XQueryStaticContext ctx, SequenceType type, XPathItemExpression [] args)
			: base (ctx, type.SchemaType.QualifiedName.Name, 1, 1, type, args)
		{
		}

		public override XPathSequence Evaluate (XPathSequence iter)
		{
			return new SingleItemIterator (XQueryConvert.ItemToItem (Atomize (Args [0].Evaluate (iter)), null), iter);
		}
	}

	// 6 functions on numerics (operators are not defined here)

	// 6.4.1 fn:abs ($arg as numeric?) as numeric?
	internal class FnAbsCall : DefaultFunctionCall
	{
		public FnAbsCall (XQueryStaticContext ctx, XPathItemExpression [] args)
			: base (ctx, "abs", 1, 1, args [0].StaticType, args)
		{
		}

		public override XPathSequence Evaluate (XPathSequence iter)
		{
			XPathSequence arg = Args [0].Evaluate (iter);
			if (!arg.MoveNext ())
				return new XPathEmptySequence (iter.Context);
			XPathAtomicValue a = null;
			// FIXME: use schema type IsDerivedFrom()
			switch (Type.GetTypeCode (arg.Current.ValueType)) {
			case TypeCode.Int64:
				return new SingleItemIterator (new XPathAtomicValue (System.Math.Abs (arg.Current.ValueAsInt64), arg.Current.XmlType), iter);
			case TypeCode.Int32:
				return new SingleItemIterator (new XPathAtomicValue (System.Math.Abs (arg.Current.ValueAsInt32), arg.Current.XmlType), iter);
			case TypeCode.Double:
				return new SingleItemIterator (new XPathAtomicValue (System.Math.Abs (arg.Current.ValueAsDouble), arg.Current.XmlType), iter);
			case TypeCode.Decimal:
				return new SingleItemIterator (new XPathAtomicValue (System.Math.Abs (arg.Current.ValueAsDecimal), arg.Current.XmlType), iter);
			case TypeCode.Single:
				return new SingleItemIterator (new XPathAtomicValue (System.Math.Abs (arg.Current.ValueAsSingle), arg.Current.XmlType), iter);
			}
			return new XPathEmptySequence (iter.Context);
		}
	}

	// 6.4.2 fn:ceiling ($arg as numeric?) as numeric?
	internal class FnCeilingCall : DefaultFunctionCall
	{
		public FnCeilingCall (XQueryStaticContext ctx, XPathItemExpression [] args)
			: base (ctx, "ceiling", 1, 1, args [0].StaticType, args)
		{
		}

		public override XPathSequence Evaluate (XPathSequence iter)
		{
			throw new NotImplementedException ();
		}
	}

	// 6.4.3 fn:floor ($arg as numeric?) as numeric?
	internal class FnFloorCall : DefaultFunctionCall
	{
		public FnFloorCall (XQueryStaticContext ctx, XPathItemExpression [] args)
			: base (ctx, "floor", 1, 1, args [0].StaticType, args)
		{
		}

		public override XPathSequence Evaluate (XPathSequence iter)
		{
			throw new NotImplementedException ();
		}
	}

	// 6.4.4 fn:round ($arg as numeric?) as numeric?
	internal class FnRoundCall : DefaultFunctionCall
	{
		public FnRoundCall (XQueryStaticContext ctx, XPathItemExpression [] args)
			: base (ctx, "round", 1, 1, args [0].StaticType, args)
		{
		}

		public override XPathSequence Evaluate (XPathSequence iter)
		{
			throw new NotImplementedException ();
		}
	}

	// 6.4.5 fn:round-half-to-even ($arg as numeric?) as numeric?
	internal class FnRoundHalfToEvenCall : DefaultFunctionCall
	{
		public FnRoundHalfToEvenCall (XQueryStaticContext ctx, XPathItemExpression [] args)
			: base (ctx, "round-half-to-even", 1, 2, args [0].StaticType, args)
		{
		}

		public override XPathSequence Evaluate (XPathSequence iter)
		{
			throw new NotImplementedException ();
		}
	}

	// 7.2.1 fn:codepoints-to-string ($arg as xs:integer*) as xs:string
	internal class FnCodepointsToStringCall : DefaultFunctionCall
	{
		public FnCodepointsToStringCall (XQueryStaticContext ctx, XPathItemExpression [] args)
			: base (ctx, "codepoints-to-string", 1, 1, SequenceType.IntegerList, args)
		{
		}

		public override XPathSequence Evaluate (XPathSequence iter)
		{
			throw new NotImplementedException ();
		}
	}

	internal class FnStringCallToCodepointsCall : DefaultFunctionCall
	{
		public FnStringCallToCodepointsCall (XQueryStaticContext ctx, XPathItemExpression [] args)
			: base (ctx, "string-to-codepoints", 1, 1, SequenceType.Create (XmlSchemaSimpleType.XsString, Occurence.Optional), args)
		{
		}

		public override XPathSequence Evaluate (XPathSequence iter)
		{
			throw new NotImplementedException ();
		}
	}
}
#endif
