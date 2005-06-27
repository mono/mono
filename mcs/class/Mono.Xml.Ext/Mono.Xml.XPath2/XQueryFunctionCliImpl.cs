//
// XQueryFunctionCliImple.cs
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
// Runtime-level (native) implementation of XQuery 1.0 and XPath 2.0 
// Functions implementation. XQueryCliFunction
// See XQuery 1.0 and XPath 2.0 Functions and Operators.
//
#if NET_2_0
using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Query;
using System.Xml.Schema;
using System.Xml.XPath;
using Mono.Xml;

namespace Mono.Xml.XPath2
{
	public class XQueryFunctionCliImpl
	{
		internal static XmlSchemaType XmlTypeFromCliType (Type cliType)
		{
			switch (Type.GetTypeCode (cliType)) {
			case TypeCode.Int32:
				return InternalPool.XsInt;
			case TypeCode.Decimal:
				return InternalPool.XsDecimal;
			case TypeCode.Double:
				return InternalPool.XsDouble;
			case TypeCode.Single:
				return InternalPool.XsFloat;
			case TypeCode.Int64:
				return InternalPool.XsLong;
			case TypeCode.Int16:
				return InternalPool.XsShort;
			case TypeCode.UInt16:
				return InternalPool.XsUnsignedShort;
			case TypeCode.UInt32:
				return InternalPool.XsUnsignedInt;
			case TypeCode.String:
				return InternalPool.XsString;
			case TypeCode.DateTime:
				return InternalPool.XsDateTime;
			case TypeCode.Boolean:
				return InternalPool.XsBoolean;
			}
			if (cliType == typeof (XmlQualifiedName))
				return InternalPool.XsQName;
			return null;
		}

		private static XPathItem ToItem (object arg)
		{
			if (arg == null)
				return null;
			XPathItem item = arg as XPathItem;
			if (item != null)
				return item;
			XPathSequence seq = arg as XPathSequence;
			if (seq != null)
				return seq.MoveNext () ? seq.Current : null;
			return new XPathAtomicValue (arg, XmlTypeFromCliType (arg.GetType ()));
		}

		// Accessors

		public static XmlQualifiedName FnNodeName (XPathNavigator arg)
		{
			if (arg == null)
				return null;

			return arg.LocalName == String.Empty ?
				XmlQualifiedName.Empty :
				new XmlQualifiedName (arg.LocalName, arg.NamespaceURI);
		}

		public static bool FnNilled (XPathNavigator arg)
		{
			if (arg == null)
				throw new XmlQueryException ("Function nilled() does not allow empty sequence parameter.");

			IXmlSchemaInfo info = arg.NodeType == XPathNodeType.Element ? arg.SchemaInfo : null;
			return info != null && info.IsNil;
		}

		public static string FnString (XQueryContext context)
		{
			XPathItem item = context.CurrentItem;
			if (item == null)
				throw new ArgumentException ("FONC0001: undefined context item");
			return FnString (item);
		}

		[MonoTODO]
		public static string FnString (object arg)
		{
			if (arg == null)
				return String.Empty;
			XPathNavigator nav = arg as XPathNavigator;
			if (nav != null)
				return nav.Value;
			// FIXME: it should be exactly the same as "arg cast as xs:string"
			XPathItem item = ToItem (arg);
			return item != null ? XQueryConvert.ItemToString (item) : null;
		}

		[MonoTODO]
		public static XPathAtomicValue FnData (object arg)
		{
			// FIXME: parameter should be object []
			XPathNavigator nav = arg as XPathNavigator;
			if (nav != null) {
				XmlSchemaType st = nav.SchemaInfo != null ? nav.SchemaInfo.SchemaType : null;
				return new XPathAtomicValue (nav.TypedValue, st != null ? st : InternalPool.XsAnyType);
			}
			else
				return (XPathAtomicValue) arg;
		}

		public static string FnBaseUri (XPathNavigator nav)
		{
			return nav != null ? nav.BaseURI : null;
		}

		public static string FnDocumentUri (XPathNavigator nav)
		{
			if (nav == null)
				return null;
			XPathNavigator root = nav.Clone ();
			root.MoveToRoot ();
			return root.BaseURI;
		}

		// Error

		[MonoTODO]
		public static void FnError (object arg)
		{
			throw new NotImplementedException ();
		}

		// Trace

		[MonoTODO]
		public static object FnTrace (XQueryContext ctx, object value, string label)
		{
			if (value == null)
				return new XPathEmptySequence (ctx);
			XPathSequence seq = value as XPathSequence;
			if (seq == null) {
				XPathAtomicValue av = value as XPathAtomicValue;
				if (av == null)
					av = new XPathAtomicValue (value,
						InternalPool.GetBuiltInType (
							InternalPool.XmlTypeCodeFromRuntimeType (
								value.GetType (), true)));
				seq = new SingleItemIterator (av, ctx);
			}
			return new TracingIterator (seq, label);
		}

		// Numeric Operation

		[MonoTODO]
		public static object FnAbs (object arg)
		{
			if (arg is int)
				return System.Math.Abs ((int) arg);
			if (arg is long)
				return System.Math.Abs ((long) arg);
			else if (arg is decimal)
				return System.Math.Abs ((decimal) arg);
			else if (arg is double)
				return System.Math.Abs ((double) arg);
			else if (arg is float)
				return System.Math.Abs ((float) arg);
			else if (arg is short)
				return System.Math.Abs ((short) arg);
			else if (arg is uint || arg is ulong || arg is ushort)
				return arg;
			return null;
		}

		[MonoTODO]
		public static object FnCeiling (object arg)
		{
			if (arg is decimal) {
				decimal d = (decimal) arg;
				decimal d2 = Decimal.Floor (d);
				return d2 != d ? d2 + 1 : d2;
			}
			else if (arg is double || arg is float)
				return System.Math.Ceiling ((double) arg);
			else if (arg is int || arg is long || arg is short || arg is uint || arg is ulong || arg is ushort)
				return arg;
			return null;
		}

		[MonoTODO]
		public static object FnFloor (object arg)
		{
			if (arg is decimal)
				return Decimal.Floor ((decimal) arg);
			else if (arg is double || arg is float)
				return System.Math.Floor ((double) arg);
			else if (arg is int || arg is long || arg is short || arg is uint || arg is ulong || arg is ushort)
				return arg;
			return null;
		}

		[MonoTODO]
		public static object FnRound (object arg)
		{
			if (arg is decimal)
				return Decimal.Round ((decimal) arg, 0);
			else if (arg is double || arg is float)
				return System.Math.Round ((double) arg);
			else if (arg is int || arg is long || arg is short || arg is uint || arg is ulong || arg is ushort)
				return arg;
			return null;
		}

		[MonoTODO]
		public static object FnRoundHalfToEven (object arg)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static string FnCodepointsToString (int [] arg)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static int [] FnStringToCodepoints (string arg)
		{
			throw new NotImplementedException ();
		}

		public static int FnCompare (XQueryContext ctx, string s1, string s2)
		{
			return FnCompare (s1, s2, ctx.DefaultCollation);
		}

		public static int FnCompare (XQueryContext ctx, string s1, string s2, string collation)
		{
			return FnCompare (s1, s2, ctx.GetCulture (collation));
		}

		private static int FnCompare (string s1, string s2, CultureInfo ci)
		{
			return ci.CompareInfo.Compare (s1, s2);
		}

		public static string FnConcat (object o1, object o2)
		{
			return String.Concat (o1, o2);
		}

		public static string FnStringJoin (string [] strings, string separator)
		{
			return String.Join (separator, strings);
		}

		public static string FnSubstring (string src, double loc)
		{
			return src.Substring ((int) loc);
		}

		public static string FnSubstring (string src, double loc, double length)
		{
			return src.Substring ((int) loc, (int) length);
		}

		public static int FnStringLength (XQueryContext ctx)
		{
			return FnString (ctx).Length;
		}

		public static int FnStringLength (string s)
		{
			return s.Length;
		}

		public static string FnNormalizeSpace (XQueryContext ctx)
		{
			return FnNormalizeSpace (FnString (ctx));
		}

		[MonoTODO]
		public static string FnNormalizeSpace (string s)
		{
			throw new NotImplementedException ();
		}

		public static string FnNormalizeUnicode (string arg)
		{
			return FnNormalizeUnicode (arg, "NFC");
		}

		[MonoTODO]
		public static string FnNormalizeUnicode (string arg, string normalizationForm)
		{
			throw new NotImplementedException ();
		}

		public static string FnUpperCase (string arg)
		{
			// FIXME: supply culture
			return arg.ToUpper ();
		}

		public static string FnLowerCase (string arg)
		{
			// FIXME: supply culture
			return arg.ToLower ();
		}

		public static string FnTranslate (string arg, string mapString, string transString)
		{
			return arg == null ? null : arg.Replace (mapString, transString);
		}

		[MonoTODO]
		public static string FnEscapeUri (string uriPart, bool escapeReserved)
		{
			throw new NotImplementedException ();
		}

		public static bool FnContains (XQueryContext ctx, string arg1, string arg2)
		{
			return FnContains (arg1, arg2, ctx.DefaultCollation);
		}

		public static bool FnContains (XQueryContext ctx, string arg1, string arg2, string collation)
		{
			return FnContains (arg1, arg2, ctx.GetCulture (collation));
		}

		private static bool FnContains (string arg1, string arg2, CultureInfo ci)
		{
			if (arg1 == null)
				arg1 = String.Empty;
			if (arg2 == null)
				arg2 = String.Empty;
			if (arg2 == String.Empty)
				return true;
			return ci.CompareInfo.IndexOf (arg1, arg2) >= 0;
		}

		public static bool FnStartsWith (XQueryContext ctx, string arg1, string arg2)
		{
			return FnStartsWith (arg1, arg2, ctx.DefaultCollation);
		}

		public static bool FnStartsWith (XQueryContext ctx, string arg1, string arg2, string collation)
		{
			return FnStartsWith (arg1, arg2, ctx.GetCulture (collation));
		}

		private static bool FnStartsWith (string arg1, string arg2, CultureInfo ci)
		{
			return ci.CompareInfo.IsPrefix (arg1, arg2);
		}

		public static bool FnEndsWith (XQueryContext ctx, string arg1, string arg2)
		{
			return FnEndsWith (arg1, arg2, ctx.DefaultCollation);
		}

		public static bool FnEndsWith (XQueryContext ctx, string arg1, string arg2, string collation)
		{
			return FnEndsWith (arg1, arg2, ctx.GetCulture (collation));
		}

		private static bool FnEndsWith (string arg1, string arg2, CultureInfo ci)
		{
			return ci.CompareInfo.IsSuffix (arg1, arg2);
		}

		public static string FnSubstringBefore (XQueryContext ctx, string arg1, string arg2)
		{
			return FnSubstringBefore (arg1, arg2, ctx.DefaultCollation);
		}

		public static string FnSubstringBefore (XQueryContext ctx, string arg1, string arg2, string collation)
		{
			return FnSubstringBefore (arg1, arg2, ctx.GetCulture (collation));
		}

		private static string FnSubstringBefore (string arg1, string arg2, CultureInfo ci)
		{
			int index = ci.CompareInfo.IndexOf (arg1, arg2);
			return arg1.Substring (0, index);
		}

		public static string FnSubstringAfter (XQueryContext ctx, string arg1, string arg2)
		{
			return FnSubstringAfter (arg1, arg2, ctx.DefaultCollation);
		}

		public static string FnSubstringAfter (XQueryContext ctx, string arg1, string arg2, string collation)
		{
			return FnSubstringAfter (arg1, arg2, ctx.GetCulture (collation));
		}

		private static string FnSubstringAfter (string arg1, string arg2, CultureInfo ci)
		{
			int index = ci.CompareInfo.IndexOf (arg1, arg2);
			return arg1.Substring (index);
		}

		public static bool FnMatches (string input, string pattern)
		{
			return new Regex (pattern).IsMatch (input);
		}

		[MonoTODO]
		public static bool FnMatches (string input, string pattern, string flags)
		{
			throw new NotImplementedException ();
		}

		public static string FnReplace (string input, string pattern, string replace)
		{
			return new Regex (pattern).Replace (input, replace);
		}

		[MonoTODO]
		public static string FnReplace (string input, string pattern, string replace, string flags)
		{
			throw new NotImplementedException ();
		}

		public static string [] FnTokenize (string input, string pattern)
		{
			return new Regex (pattern).Split (input);
		}

		[MonoTODO]
		public static string [] FnTokenize (string input, string pattern, string flags)
		{
			throw new NotImplementedException ();
		}

		public static string FnResolveUri (XQueryContext ctx, string relUri)
		{
			return new Uri (new Uri (ctx.StaticContext.BaseUri), relUri).ToString ();
		}

		public static string FnResolveUri (string relUri, string baseUri)
		{
			return new Uri (new Uri (baseUri), relUri).ToString ();
		}

		public static object FnTrue ()
		{
			return true;
		}

		public static object FnFalse ()
		{
			return false;
		}

		public static object FnNot (bool value)
		{
			return !value;
		}

		// FIXME: add a bunch of annoying datetime functions

		public static object FnResolveQName (string qname, XPathNavigator element)
		{
			if (qname == null)
				return null;

			int index = qname.IndexOf (':');
			string prefix = (index < 0) ? "" : qname.Substring (index);
			return new XmlQualifiedName (
				element.LookupNamespace (prefix),
				index < 0 ? qname : qname.Substring (index + 1));
		}

		public static object FnExpandQName (string ns, string local)
		{
			return new XmlQualifiedName (local, ns);
		}

		public static string FnLocalNameFromQName (XmlQualifiedName name)
		{
			return name != null ? name.Name : null;
		}

		public static object FnNamespaceUriFromQName (XmlQualifiedName name)
		{
			return name != null ? name.Namespace : null;
		}

		public static object FnNamespaceUriForPrefix (XQueryContext context, string prefix)
		{
			return prefix != null ? context.LookupNamespace (prefix) : null;
		}

		public static string [] FnInScopePrefixes (XQueryContext context)
		{
			IDictionary dict = context.GetNamespacesInScope (XmlNamespaceScope.ExcludeXml);
			ArrayList keys = new ArrayList (dict.Keys);
			return keys.ToArray (typeof (string)) as string [];
		}

		public static string FnName (XPathNavigator nav)
		{
			return nav != null ? nav.Name : null;
		}

		public static string FnLocalName (XPathNavigator nav)
		{
			return nav != null ? nav.LocalName : null;
		}

		public static string FnNamespaceUri (XPathNavigator nav)
		{
			return nav != null ? nav.NamespaceURI : null;
		}

		public static double FnNumber (XQueryContext ctx)
		{
			return FnNumber (ctx.CurrentItem);
		}

		public static double FnNumber (object arg)
		{
			if (arg == null)
				throw new XmlQueryException ("Context item could not be ndetermined during number() evaluation.");
			XPathItem item = ToItem (arg);
			return XQueryConvert.ItemToDouble (item);
		}

		public static bool FnLang (XQueryContext ctx, string testLang)
		{
			return FnLang (testLang, ctx.CurrentNode);
		}

		public static bool FnLang (string testLang, XPathNavigator node)
		{
			return testLang == node.XmlLang;
		}

		public static XPathNavigator FnRoot (XQueryContext ctx)
		{
			if (ctx.CurrentItem == null)
				throw new XmlQueryException ("FONC0001: Undefined context item.");
			if (ctx.CurrentNode == null)
				throw new XmlQueryException ("FOTY0011: Context item is not a node.");
			return FnRoot (ctx.CurrentNode);
		}

		public static XPathNavigator FnRoot (XPathNavigator node)
		{
			if (node == null)
				return null;
			XPathNavigator root = node.Clone ();
			root.MoveToRoot ();
			return root;
		}

		public static bool FnBoolean (IEnumerator e)
		{
			if (!e.MoveNext ())
				return false;
			XPathItem item = e.Current as XPathItem;
			if (e.MoveNext ())
				return true;
			return XQueryConvert.ItemToBoolean (item);
		}

		public static XPathSequence FnIndexOf (XQueryContext ctx, XPathSequence items, XPathItem item)
		{
			return FnIndexOf (ctx, items, item, ctx.DefaultCollation);
		}

		public static XPathSequence FnIndexOf (XQueryContext ctx, XPathSequence items, XPathItem item, CultureInfo ci)
		{
			ArrayList al = new ArrayList ();
			IEnumerator e = items.GetEnumerator ();
			for (int i = 0; e.MoveNext (); i++) {
				XPathItem iter = e.Current as XPathItem;
				if (iter.XmlType.TypeCode == XmlTypeCode.String) {
					if (ci.CompareInfo.Compare (iter.Value, item.Value) == 0)
						al.Add (i);
				}
				else {
					IComparable ic = (IComparable) iter.TypedValue;
					if (ic.CompareTo ((IComparable) item.TypedValue) == 0)
						al.Add (i);
				}
			}
			return new ListIterator (ctx, al);
		}

		public static bool FnEmpty (XPathSequence e)
		{
			if (e is XPathEmptySequence)
				return true;
			return !e.GetEnumerator ().MoveNext ();
		}

		public static bool FnExists (XPathSequence e)
		{
			if (e is XPathEmptySequence)
				return false;
			return e.MoveNext ();
		}

		public static XPathSequence FnDistinctValues (XQueryContext ctx, XPathSequence items)
		{
			return FnDistinctValuesImpl (ctx, items, ctx.DefaultCollation);
		}

		public static XPathSequence FnDistinctValues (XQueryContext ctx, XPathSequence items, string collation)
		{
			return FnDistinctValuesImpl (ctx, items, ctx.GetCulture (collation));
		}

		private static XPathSequence FnDistinctValuesImpl (XQueryContext ctx, XPathSequence items, CultureInfo collation)
		{
			return new DistinctValueIterator (ctx, items, collation);
		}

		public static XPathSequence FnInsertBefore (XPathSequence target, int position, XPathSequence inserts)
		{
			if (position < 1)
				position = 1;
			return new InsertingIterator (target, position, inserts);
		}

		public static XPathSequence FnRemove (XPathSequence target, int position)
		{
			if (position < 1)
				return target;
			return new RemovalIterator (target, position);
		}

		[MonoTODO ("optimize")]
		public static XPathSequence FnReverse (XPathSequence arg)
		{
			ArrayList al = new ArrayList ();
			while (arg.MoveNext ())
				al.Add (arg.Current);
			al.Reverse ();
			return new ListIterator (arg.Context, al);
		}

		public static object FnSubsequence (XPathSequence sourceSeq, double startingLoc)
		{
			return FnSubsequence (sourceSeq, startingLoc, double.MaxValue);
		}

		[MonoTODO]
		public static object FnSubsequence (XPathSequence sourceSeq, double startingLoc, double length)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		// Basically it should be optimized by XQueryASTCompiler
		public static XPathSequence FnUnordered (XPathSequence e)
		{
			return e;
		}

		public static XPathItem FnZeroOrOne (XPathSequence e)
		{
			if (!e.MoveNext ())
				return null;
			XPathItem item = e.Current;
			if (e.MoveNext ())
				throw new XmlQueryException ("zero-or-one() function detected that the argument sequence contains two or more items.");
			return item;
		}

		public static object FnOneOrMore (XPathSequence e)
		{
			if (!e.Clone ().MoveNext ())
				throw new XmlQueryException ("one-or-more() function detected that the argument sequence contains no items.");
			return e;
		}

		public static XPathItem FnExactlyOne (XPathSequence e)
		{
			if (!e.MoveNext ())
				throw new XmlQueryException ("exactly-one() function detected that the argument sequence contains no items.");
			XPathItem item = e.Current;
			if (e.MoveNext ())
				throw new XmlQueryException ("exactly-one() function detected that the argument sequence contains two or more items.");
			return item;
		}

		public static object FnDeepEqual (XQueryContext ctx, XPathSequence p1, XPathSequence p2)
		{
			return FnDeepEqualImpl (p1, p2, ctx.DefaultCollation);
		}

		public static object FnDeepEqual (XQueryContext ctx, XPathSequence p1, XPathSequence p2, string collation)
		{
			return FnDeepEqualImpl (p1, p2, ctx.GetCulture (collation));
		}

		public static bool FnDeepEqualImpl (XPathSequence p1, XPathSequence p2, CultureInfo collation)
		{
			// FIXME: use collation
			while (p1.MoveNext ()) {
				if (!p2.MoveNext ())
					return false;
				if (!FnDeepEqualItem (p1.Current, p2.Current, collation))
					return false;
			}
			if (p2.MoveNext ())
				return false;
			return true;
		}

		// FIXME: Actually ValueEQ() should consider collation.
		[MonoTODO]
		private static bool FnDeepEqualItem (XPathItem i1, XPathItem i2, CultureInfo collation)
		{
			XPathAtomicValue av1 = i1 as XPathAtomicValue;
			XPathAtomicValue av2 = i1 as XPathAtomicValue;
			if (av1 != null && av2 != null) {
				try {
					return XQueryComparisonOperator.ValueEQ (av1, av2);
				} catch (XmlQueryException) {
					// not-allowed comparison never raises
					// an error here, just return false.
					return false;
				}
			}
			else if (av1 != null || av2 != null)
				return false;

			XPathNavigator n1 = i1 as XPathNavigator;
			XPathNavigator n2 = i2 as XPathNavigator;
			if (n1.NodeType != n2.NodeType)
				return false;
			switch (n1.NodeType) {
			case XPathNodeType.Root:
				throw new NotImplementedException ();
			case XPathNodeType.Element:
				throw new NotImplementedException ();
			case XPathNodeType.Attribute:
				return n1.Name == n2.Name && n1.TypedValue == n2.TypedValue;
			case XPathNodeType.ProcessingInstruction:
			case XPathNodeType.Namespace:
				return n1.Name == n2.Name && n1.Value == n2.Value;
			case XPathNodeType.Text:
			case XPathNodeType.Comment:
				return n1.Value == n2.Value;
			}
			return false;
		}

		public static int FnCount (XPathSequence e)
		{
			if (e == null)
				return 0;
			return e.Count;
		}

		[MonoTODO]
		public static object FnAvg (XPathSequence e)
		{
			if (!e.MoveNext ())
				return null;
			switch (e.Current.XmlType.TypeCode) {
			case XmlTypeCode.DayTimeDuration:
				return FnAvgDayTimeDuration (e);
			case XmlTypeCode.YearMonthDuration:
				return FnAvgYearMonthDuration (e);
			case XmlTypeCode.Decimal:
				return FnAvgDecimal (e);
			case XmlTypeCode.Integer:
				return FnAvgInteger (e);
			case XmlTypeCode.Float:
				return FnAvgFloat (e);
			case XmlTypeCode.UntypedAtomic:
			case XmlTypeCode.Double:
				return FnAvgDouble (e);
			}
			throw new XmlQueryException ("avg() function detected that the sequence contains an item whose type is neither of dayTimeDuration, yearMonthDuration, decimal, integer, float, double, nor untypedAtomic.");
		}

		private static TimeSpan FnAvgDayTimeDuration (XPathSequence e)
		{
			throw new NotImplementedException ();
		}

		private static TimeSpan FnAvgYearMonthDuration (XPathSequence e)
		{
			throw new NotImplementedException ();
		}

		private static TimeSpan FnAvgDecimal (XPathSequence e)
		{
			throw new NotImplementedException ();
		}

		private static TimeSpan FnAvgInteger (XPathSequence e)
		{
			throw new NotImplementedException ();
		}

		private static TimeSpan FnAvgFloat (XPathSequence e)
		{
			throw new NotImplementedException ();
		}

		private static TimeSpan FnAvgDouble (XPathSequence e)
		{
			throw new NotImplementedException ();
		}

		public static object FnMax (XQueryContext ctx, XPathSequence e)
		{
			return FnMaxImpl (e, ctx.DefaultCollation);
		}

		public static object FnMax (XQueryContext ctx, XPathSequence e, string collation)
		{
			return FnMaxImpl (e, ctx.GetCulture (collation));
		}

		private static object FnMaxImpl (XPathSequence e, CultureInfo collation)
		{
			if (!e.MoveNext ())
				return null;
			switch (e.Current.XmlType.TypeCode) {
			case XmlTypeCode.DayTimeDuration:
				return FnMaxDayTimeDuration (e);
			case XmlTypeCode.YearMonthDuration:
				return FnMaxYearMonthDuration (e);
			case XmlTypeCode.Decimal:
				return FnMaxDecimal (e);
			case XmlTypeCode.Integer:
				return FnMaxInteger (e);
			case XmlTypeCode.Float:
				return FnMaxFloat (e);
			case XmlTypeCode.UntypedAtomic:
			case XmlTypeCode.Double:
				return FnMaxDouble (e);
			}
			throw new XmlQueryException ("avg() function detected that the sequence contains an item whose type is neither of dayTimeDuration, yearMonthDuration, decimal, integer, float, double, nor untypedAtomic.");
		}

		private static TimeSpan FnMaxDayTimeDuration (XPathSequence e)
		{
			// FIXME: reject yMD (but is it possible...?)
			TimeSpan ret = TimeSpan.Zero;
			do {
				TimeSpan ts = (TimeSpan) e.Current.TypedValue;
				if (ts > ret)
					ret = ts;
			} while (e.MoveNext ());
			return ret;
		}

		private static TimeSpan FnMaxYearMonthDuration (XPathSequence e)
		{
			// FIXME: reject dTD (but is it possible...?)
			TimeSpan ret = TimeSpan.Zero;
			do {
				TimeSpan ts = (TimeSpan) e.Current.TypedValue;
				if (ts > ret)
					ret = ts;
			} while (e.MoveNext ());
			return ret;
		}

		private static decimal FnMaxDecimal (XPathSequence e)
		{
			decimal ret = decimal.MinValue;
			do {
				ret = System.Math.Max (e.Current.ValueAsDecimal, ret);
			} while (e.MoveNext ());
			return ret;
		}

		private static int FnMaxInteger (XPathSequence e)
		{
			int ret = int.MinValue;
			do {
				ret = System.Math.Max (e.Current.ValueAsInt32, ret);
			} while (e.MoveNext ());
			return ret;
		}

		private static float FnMaxFloat (XPathSequence e)
		{
			float ret = float.MinValue;
			do {
				ret = System.Math.Max (e.Current.ValueAsSingle, ret);
			} while (e.MoveNext ());
			return ret;
		}

		private static double FnMaxDouble (XPathSequence e)
		{
			double ret = double.MinValue;
			do {
				ret = System.Math.Max (e.Current.ValueAsDouble, ret);
			} while (e.MoveNext ());
			return ret;
		}

		public static object FnMin (XQueryContext ctx, XPathSequence e)
		{
			return FnMinImpl (e, ctx.DefaultCollation);
		}

		public static object FnMin (XQueryContext ctx, XPathSequence e, string collation)
		{
			return FnMinImpl (e, ctx.GetCulture (collation));
		}

		private static object FnMinImpl (XPathSequence e, CultureInfo collation)
		{
			if (!e.MoveNext ())
				return null;
			switch (e.Current.XmlType.TypeCode) {
			case XmlTypeCode.DayTimeDuration:
				return FnMinDayTimeDuration (e);
			case XmlTypeCode.YearMonthDuration:
				return FnMinYearMonthDuration (e);
			case XmlTypeCode.Decimal:
				return FnMinDecimal (e);
			case XmlTypeCode.Integer:
				return FnMinInteger (e);
			case XmlTypeCode.Float:
				return FnMinFloat (e);
			case XmlTypeCode.UntypedAtomic:
			case XmlTypeCode.Double:
				return FnMinDouble (e);
			}
			throw new XmlQueryException ("avg() function detected that the sequence contains an item whose type is neither of dayTimeDuration, yearMonthDuration, decimal, integer, float, double, nor untypedAtomic.");
		}

		private static TimeSpan FnMinDayTimeDuration (XPathSequence e)
		{
			// FIXME: reject yMD (but is it possible...?)
			TimeSpan ret = TimeSpan.Zero;
			do {
				TimeSpan ts = (TimeSpan) e.Current.TypedValue;
				if (ts > ret)
					ret = ts;
			} while (e.MoveNext ());
			return ret;
		}

		private static TimeSpan FnMinYearMonthDuration (XPathSequence e)
		{
			// FIXME: reject dTD (but is it possible...?)
			TimeSpan ret = TimeSpan.Zero;
			do {
				TimeSpan ts = (TimeSpan) e.Current.TypedValue;
				if (ts > ret)
					ret = ts;
			} while (e.MoveNext ());
			return ret;
		}

		private static decimal FnMinDecimal (XPathSequence e)
		{
			decimal ret = decimal.MaxValue;
			do {
				ret = System.Math.Min (e.Current.ValueAsDecimal, ret);
			} while (e.MoveNext ());
			return ret;
		}

		private static int FnMinInteger (XPathSequence e)
		{
			int ret = int.MaxValue;
			do {
				ret = System.Math.Min (e.Current.ValueAsInt32, ret);
			} while (e.MoveNext ());
			return ret;
		}

		private static float FnMinFloat (XPathSequence e)
		{
			float ret = float.MaxValue;
			do {
				ret = System.Math.Min (e.Current.ValueAsSingle, ret);
			} while (e.MoveNext ());
			return ret;
		}

		private static double FnMinDouble (XPathSequence e)
		{
			double ret = double.MaxValue;
			do {
				ret = System.Math.Min (e.Current.ValueAsDouble, ret);
			} while (e.MoveNext ());
			return ret;
		}

		[MonoTODO]
		public static object FnSum (XPathSequence e)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static object FnSum (XPathSequence e, XPathItem zero)
		{
			throw new NotImplementedException ();
		}

		public static XPathNavigator FnId (XQueryContext ctx, string id)
		{
			return FnId (id, ctx.CurrentNode);
		}

		public static XPathNavigator FnId (string id, XPathNavigator nav)
		{
			XPathNavigator node = nav.Clone ();
			return node.MoveToId (id) ? node : null;
		}

		[MonoTODO]
		public static object FnIdRef (XQueryContext ctx, string arg)
		{
			return FnIdRef (arg, ctx.CurrentNode);
		}

		[MonoTODO]
		public static object FnIdRef (string arg, XPathNavigator node)
		{
			throw new NotImplementedException ();
		}

		public static XPathNavigator FnDoc (XQueryContext ctx, string uri)
		{
			XmlResolver res = ctx.ContextManager.ExtDocResolver;
			string baseUriString = ctx.StaticContext.BaseUri;
			Uri baseUri = null;
			if (baseUriString != null && baseUriString != String.Empty)
				baseUri = new Uri (baseUriString);
			Uri relUri = res.ResolveUri (baseUri, uri);
			Stream s = res.GetEntity (relUri, null, typeof (Stream)) as Stream;
			try {
				XPathDocument doc = new XPathDocument (new XmlValidatingReader (new XmlTextReader (s)), XmlSpace.Preserve);
				return doc.CreateNavigator ();
			} finally {
				s.Close ();
			}
		}

		public static XPathSequence FnCollection (XQueryContext ctx, string name)
		{
			return ctx.ResolveCollection (name);
		}

		[XQueryFunctionContext]
		public static int FnPosition (XPathSequence current)
		{
			return current.Position;
		}

		[XQueryFunctionContext]
		public static int FnLast (XPathSequence current)
		{
			return current.Count;
		}

		public static DateTime FnCurrentDateTime ()
		{
			return DateTime.Now;
		}

		public static DateTime FnCurrentDate ()
		{
			return DateTime.Today;
		}

		public static DateTime FnCurrentTime ()
		{
			return new DateTime (DateTime.Now.TimeOfDay.Ticks);
		}

		[MonoTODO]
		public static object FnDefaultCollation ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static object FnImplicitTimeZone ()
		{
			throw new NotImplementedException ();
		}
	}
}
#endif
