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
using System.Xml;
using System.Xml.Schema;
using System.Xml.XPath;
using MS.Internal.Xml;

namespace Mono.Xml.XPath2
{
	public class XQueryFunctionCliImpl
	{
		internal static XmlSchemaType XmlTypeFromCliType (Type cliType)
		{
			switch (Type.GetTypeCode (cliType)) {
			case TypeCode.Int32:
				return XmlSchemaSimpleType.XsInt;
			case TypeCode.Decimal:
				return XmlSchemaSimpleType.XsDecimal;
			case TypeCode.Double:
				return XmlSchemaSimpleType.XsDouble;
			case TypeCode.Single:
				return XmlSchemaSimpleType.XsFloat;
			case TypeCode.Int64:
				return XmlSchemaSimpleType.XsLong;
			case TypeCode.Int16:
				return XmlSchemaSimpleType.XsShort;
			case TypeCode.UInt16:
				return XmlSchemaSimpleType.XsUnsignedShort;
			case TypeCode.UInt32:
				return XmlSchemaSimpleType.XsUnsignedInt;
			case TypeCode.String:
				return XmlSchemaSimpleType.XsString;
			case TypeCode.DateTime:
				return XmlSchemaSimpleType.XsDateTime;
			case TypeCode.Boolean:
				return XmlSchemaSimpleType.XsBoolean;
			}
			if (cliType == typeof (XmlQualifiedName))
				return XmlSchemaSimpleType.XsQName;
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
				return XmlQualifiedName.Empty;

			return arg.LocalName == String.Empty ?
				XmlQualifiedName.Empty :
				new XmlQualifiedName (arg.LocalName, arg.NamespaceURI);
		}

		public static bool FnNilled (XPathNavigator arg)
		{
			if (arg == null)
				return false;

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

		public static string FnString (object arg)
		{
			XPathNavigator nav = arg as XPathNavigator;
			if (nav != null)
				return nav.Value;
			XPathItem item = ToItem (arg);
			return item != null ? XQueryConvert.ItemToString (item) : null;
		}

		public static XPathAtomicValue FnData (object arg)
		{
			XPathNavigator nav = arg as XPathNavigator;
			if (nav != null) {
				XmlSchemaType st = nav.SchemaInfo != null ? nav.SchemaInfo.SchemaType : null;
				return new XPathAtomicValue (nav.TypedValue, st != null ? st : XmlSchemaComplexType.AnyType);
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

		public static void FnError (object arg)
		{
			throw new NotImplementedException ();
		}

		// Trace

		public static object FnTrace (object arg)
		{
			throw new NotImplementedException ();
		}

		// Numeric Operation

		public static object FnAbs (ValueType arg)
		{
			throw new NotImplementedException ();
		}

		public static object FnCeiling (ValueType arg)
		{
			throw new NotImplementedException ();
		}

		public static object FnFloor (ValueType arg)
		{
			throw new NotImplementedException ();
		}

		public static object FnRound (ValueType arg)
		{
			throw new NotImplementedException ();
		}

		public static object FnRoundHalfToEven (ValueType arg)
		{
			throw new NotImplementedException ();
		}

		public static string FnCodepointsToString (int [] arg)
		{
			throw new NotImplementedException ();
		}

		public static int [] FnStringToCodepoints (string arg)
		{
			throw new NotImplementedException ();
		}

		public static object FnCompare ()
		{
			throw new NotImplementedException ();
		}

		public static object FnConcat ()
		{
			throw new NotImplementedException ();
		}

		public static object FnStringJoin ()
		{
			throw new NotImplementedException ();
		}

		public static object FnSubstring ()
		{
			throw new NotImplementedException ();
		}

		public static object FnStringLength ()
		{
			throw new NotImplementedException ();
		}

		public static object FnNormalizeSpace ()
		{
			throw new NotImplementedException ();
		}

		public static object FnNormalizeUnicode ()
		{
			throw new NotImplementedException ();
		}

		public static object FnUpperCase ()
		{
			throw new NotImplementedException ();
		}

		public static object FnLowerCase ()
		{
			throw new NotImplementedException ();
		}

		public static object FnTranslate ()
		{
			throw new NotImplementedException ();
		}

		public static object FnEscapeUri ()
		{
			throw new NotImplementedException ();
		}

		public static object FnContains ()
		{
			throw new NotImplementedException ();
		}

		public static object FnStartsWith ()
		{
			throw new NotImplementedException ();
		}

		public static object FnEndsWith ()
		{
			throw new NotImplementedException ();
		}

		public static object FnSubstringBefore ()
		{
			throw new NotImplementedException ();
		}

		public static object FnSubstringAfter ()
		{
			throw new NotImplementedException ();
		}

		public static object FnMatches ()
		{
			throw new NotImplementedException ();
		}

		public static object FnReplace ()
		{
			throw new NotImplementedException ();
		}

		public static object FnTokenize ()
		{
			throw new NotImplementedException ();
		}

		public static object FnResolveUri ()
		{
			throw new NotImplementedException ();
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

		public static object FnResolveQName ()
		{
			throw new NotImplementedException ();
		}

		public static object FnExpandQName ()
		{
			throw new NotImplementedException ();
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

		public static object FnNumber ()
		{
			throw new NotImplementedException ();
		}

		public static object FnLang ()
		{
			throw new NotImplementedException ();
		}

		public static object FnRoot ()
		{
			throw new NotImplementedException ();
		}

		public static object FnBoolean ()
		{
			throw new NotImplementedException ();
		}

		public static object FnIndexOf ()
		{
			throw new NotImplementedException ();
		}

		public static object FnEmpty ()
		{
			throw new NotImplementedException ();
		}

		public static object FnExists ()
		{
			throw new NotImplementedException ();
		}

		public static object FnDistinctValues ()
		{
			throw new NotImplementedException ();
		}

		public static object FnInsertBefore ()
		{
			throw new NotImplementedException ();
		}

		public static object FnRemove ()
		{
			throw new NotImplementedException ();
		}

		public static object FnReverse ()
		{
			throw new NotImplementedException ();
		}

		public static object FnSubsequence ()
		{
			throw new NotImplementedException ();
		}

		public static object FnUnordered ()
		{
			throw new NotImplementedException ();
		}

		public static object FnZeroOrMore ()
		{
			throw new NotImplementedException ();
		}

		public static object FnOneOrMore ()
		{
			throw new NotImplementedException ();
		}

		public static object FnExactlyOne ()
		{
			throw new NotImplementedException ();
		}

		public static object FnDeepEqual ()
		{
			throw new NotImplementedException ();
		}

		public static object FnCount ()
		{
			throw new NotImplementedException ();
		}

		public static object FnAvg ()
		{
			throw new NotImplementedException ();
		}

		public static object FnMax ()
		{
			throw new NotImplementedException ();
		}

		public static object FnMin ()
		{
			throw new NotImplementedException ();
		}

		public static object FnSum ()
		{
			throw new NotImplementedException ();
		}

		public static object FnId ()
		{
			throw new NotImplementedException ();
		}

		public static object FnIdRef ()
		{
			throw new NotImplementedException ();
		}

		public static object FnDoc ()
		{
			throw new NotImplementedException ();
		}

		public static object FnCollection ()
		{
			throw new NotImplementedException ();
		}

		public static object FnPosition ()
		{
			throw new NotImplementedException ();
		}

		public static object FnLast ()
		{
			throw new NotImplementedException ();
		}

		public static object FnCurrentDateTime ()
		{
			throw new NotImplementedException ();
		}

		public static object FnCurrentDate ()
		{
			throw new NotImplementedException ();
		}

		public static object FnCurrentTime ()
		{
			throw new NotImplementedException ();
		}

		public static object FnDefaultCollation ()
		{
			throw new NotImplementedException ();
		}

		public static object FnImplicitTimeZone ()
		{
			throw new NotImplementedException ();
		}
	}
}
#endif
