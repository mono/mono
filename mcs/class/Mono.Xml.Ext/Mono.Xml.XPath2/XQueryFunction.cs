//
// XQueryFunction.cs - XQuery 1.0 and XPath 2.0 Functions implementation
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
using Mono.Xml;

namespace Mono.Xml.XPath2
{
	// It is instantiated per function definition. (e.g. fn:string(), fn:data(), fn:contat()).
	public abstract class XQueryFunction
	{
		public const string Namespace = "http://www.w3.org/2004/07/xpath-functions";

		static XQueryFunctionTable defaultFunctions = new XQueryFunctionTable ();

		static XQueryFunction ()
		{
			defaultFunctions.Add (FromName ("node-name", "FnNodeName"));
			defaultFunctions.Add (FromName ("nilled", "FnNilled"));
			defaultFunctions.Add (FromName ("string", "FnString"));
			defaultFunctions.Add (FromName ("data", "FnData"));
			defaultFunctions.Add (FromName ("base-uri", "FnBaseUri"));
			defaultFunctions.Add (FromName ("document-uri", "FnDocumentUri"));
			defaultFunctions.Add (FromName ("error", "FnError"));
			defaultFunctions.Add (FromName ("trace", "FnTrace"));
			defaultFunctions.Add (FromName ("abs", "FnAbs"));
			defaultFunctions.Add (FromName ("ceiling", "FnCeiling"));
			defaultFunctions.Add (FromName ("floor", "FnFloor"));
			defaultFunctions.Add (FromName ("round", "FnRound"));
			defaultFunctions.Add (FromName ("round-half-to-even", "FnRoundHalfToEven"));
			defaultFunctions.Add (FromName ("codepoints-to-string", "FnCodepointsToString"));
			defaultFunctions.Add (FromName ("string-to-codepoints", "FnStringToCodepoints"));
			defaultFunctions.Add (FromName ("compare", "FnCompare"));
			defaultFunctions.Add (FromName ("concat", "FnConcat"));
			defaultFunctions.Add (FromName ("string-join", "FnStringJoin"));
			defaultFunctions.Add (FromName ("substring", "FnSubstring"));
			defaultFunctions.Add (FromName ("string-length", "FnStringLength"));
			defaultFunctions.Add (FromName ("normaize-space", "FnNormalizeSpace"));
			defaultFunctions.Add (FromName ("normalize-unicode", "FnNormalizeUnicode"));
			defaultFunctions.Add (FromName ("uppercase", "FnUpperCase"));
			defaultFunctions.Add (FromName ("lowercase", "FnLowerCase"));
			defaultFunctions.Add (FromName ("translate", "FnTranslate"));
			defaultFunctions.Add (FromName ("escape-uri", "FnEscapeUri"));
			defaultFunctions.Add (FromName ("contains", "FnContains"));
			defaultFunctions.Add (FromName ("starts-with", "FnStartsWith"));
			defaultFunctions.Add (FromName ("ends-with", "FnEndsWith"));
			defaultFunctions.Add (FromName ("substring-before", "FnSubstringBefore"));
			defaultFunctions.Add (FromName ("substring-after", "FnSubstringAfter"));
			defaultFunctions.Add (FromName ("matches", "FnMatches"));
			defaultFunctions.Add (FromName ("replace", "FnReplace"));
			defaultFunctions.Add (FromName ("tokenize", "FnTokenize"));
			defaultFunctions.Add (FromName ("resolve-uri", "FnResolveUri"));
			defaultFunctions.Add (FromName ("true", "FnTrue"));
			defaultFunctions.Add (FromName ("false", "FnFalse"));
			defaultFunctions.Add (FromName ("not", "FnNot"));

			defaultFunctions.Add (FromName ("resolve-qname", "FnResolveQName"));
			defaultFunctions.Add (FromName ("expand-qname", "FnExpandQName"));
			defaultFunctions.Add (FromName ("namespace-uri-for-prefix", "FnNamespaceUriForPrefix"));
			defaultFunctions.Add (FromName ("in-scope-prefixes", "FnInScopePrefixes"));
			defaultFunctions.Add (FromName ("name", "FnName"));
			defaultFunctions.Add (FromName ("local-name", "FnLocalName"));
			defaultFunctions.Add (FromName ("namespace-uri", "FnNamespaceUri"));
			defaultFunctions.Add (FromName ("number", "FnNumber"));
			defaultFunctions.Add (FromName ("lang", "FnLang"));
			defaultFunctions.Add (FromName ("root", "FnRoot"));
			defaultFunctions.Add (FromName ("boolean", "FnBoolean"));
			defaultFunctions.Add (FromName ("indexof", "FnIndexOf"));
			defaultFunctions.Add (FromName ("empty", "FnEmpty"));
			defaultFunctions.Add (FromName ("exists", "FnExists"));
			defaultFunctions.Add (FromName ("distinct-values", "FnDistinctValues"));
			defaultFunctions.Add (FromName ("insert-before", "FnInsertBefore"));
			defaultFunctions.Add (FromName ("remove", "FnRemove"));
			defaultFunctions.Add (FromName ("reverse", "FnReverse"));
			defaultFunctions.Add (FromName ("subsequence", "FnSubsequence"));
			defaultFunctions.Add (FromName ("unordered", "FnUnordered"));
			defaultFunctions.Add (FromName ("zero-or-one", "FnZeroOrOne"));
			defaultFunctions.Add (FromName ("one-or-more", "FnOneOrMore"));
			defaultFunctions.Add (FromName ("exactly-one", "FnExactlyOne"));
			defaultFunctions.Add (FromName ("deep-equal", "FnDeepEqual"));
			defaultFunctions.Add (FromName ("count", "FnCount"));
			defaultFunctions.Add (FromName ("avg", "FnAvg"));
			defaultFunctions.Add (FromName ("max", "FnMax"));
			defaultFunctions.Add (FromName ("min", "FnMin"));
			defaultFunctions.Add (FromName ("sum", "FnSum"));
			defaultFunctions.Add (FromName ("id", "FnId"));
			defaultFunctions.Add (FromName ("idref", "FnIdRef"));
			defaultFunctions.Add (FromName ("doc", "FnDoc"));
			defaultFunctions.Add (FromName ("collection", "FnCollection"));
			defaultFunctions.Add (FromName ("position", "FnPosition"));
			defaultFunctions.Add (FromName ("last", "FnLast"));
			defaultFunctions.Add (FromName ("current-datetime", "FnCurrentDateTime"));
			defaultFunctions.Add (FromName ("current-date", "FnCurrentDate"));
			defaultFunctions.Add (FromName ("current-time", "FnCurrentTime"));
			defaultFunctions.Add (FromName ("default-collation", "FnDefaultCollation"));
			defaultFunctions.Add (FromName ("implicit-timezone", "FnImplicitTimeZone"));
//			defaultFunctions.Add (FromName ("years-from-duration", "FnYearsFromDuration"));
/*
			fnAtomicConstructor,
			fnYearsFromDuration, fnMonthsFromDuration,
			fnDaysFromDuration, fnHoursFromDuration,
			fnMinutesFromDuration, fnSecondsFromDuration,
			fnYearFromDateTime, fnMonthFromDateTime,
			fnDayFromDateTime, fnHoursFromDateTime,
			fnMinutesFromDateTime, fnSecondsFromDateTime,
			fnTimeZoneFromDateTime, fnYearFromDate, fnMonthFromDate,
			fnDayFromDate, fnTimeZoneFromDate, fnHoursFromTime,
			fnMinutesFromTime, fnSecondsFromTime,
			fnTimeZoneFromTime, fnAdjustDateTimeToTimeZone,
			fnAdjustDateToTimeZone, fnAdjustTimeToTimeZone,
			fnSubtractDateTimesYieldingYearMonthDuration,
			fnSubtractDateTimesYieldingDayTimeDuration,
			fnSubtractDatesYieldingYearMonthDuration,
			fnSubtractDatesYieldingDayTimeDuration,
			fnSubtractTimes,
*/
		}

		private static XQueryCliFunction FromName (string xname, string cliname)
		{
			return XQueryCliFunction.CreateFromMethodInfo (
				new XmlQualifiedName (xname, XQueryFunction.Namespace),
				FindNamedMethods (typeof (XQueryFunctionCliImpl), cliname)
				);
		}

		internal static XQueryCliFunction FromQName (XmlQualifiedName qname)
		{
			return XQueryCliFunction.CreateFromMethodInfo (
				qname, FindNamedMethods (Type.GetType (qname.Namespace), qname.Name));
		}

		private static bool FilterImpl (MemberInfo m, object filterCriteria)
		{
			return m.Name == filterCriteria.ToString ();
		}

		private static MemberFilter memberFilter = new MemberFilter (FilterImpl);

		private static MethodInfo [] FindNamedMethods (Type type, string name)
		{
			ArrayList al = new ArrayList (
				type.FindMembers (
					MemberTypes.Method,
					BindingFlags.Default | BindingFlags.Public | BindingFlags.Static,
					// FIXME: wait for anonymous method support
//					delegate (MemberInfo m, object filterCriteria) {
//						return m.Name == filterCriteria.ToString ();
//					},
					memberFilter,
					name));
			return al.ToArray (typeof (MethodInfo)) as MethodInfo [];
		}

		internal static XQueryFunction FindKnownFunction (
			XmlQualifiedName name)
		{
			switch (name.Namespace) {
			case XQueryFunction.Namespace:
				return defaultFunctions [name];
			case InternalPool.XdtNamespace:
			case XmlSchema.Namespace:
				XmlSchemaType type = XmlSchemaType.GetBuiltInSimpleType (name);
				if (type != null)
					return null; // FIXME: atomic constructor
				type = XmlSchemaType.GetBuiltInComplexType (name);
				if (type == null)
					return null; // FIXME: atomic constructor?
				return null;
			default:
				return null;
			}
		}

		// Constructor

		internal XQueryFunction (XmlQualifiedName name, XQueryFunctionArgument [] args, SequenceType returnType)
		{
			this.name = name;
			this.args = args;
			this.returnType = returnType;
		}

		// Fields

		XmlQualifiedName name;
		XQueryFunctionArgument [] args;
		SequenceType returnType;

		// Properties

		public XmlQualifiedName Name {
			get { return name; }
		}

		public abstract int MinArgs { get; }
		public abstract int MaxArgs { get; }

		public XQueryFunctionArgument [] Args {
			get { return args; }
		}

		public SequenceType ReturnType {
			get { return returnType; }
		}

		public abstract object Invoke (XPathSequence current, object [] args);

		public virtual XPathSequence Evaluate (XPathSequence iter, ExprSequence args)
		{
			object [] instParams = new object [args.Count];
			for (int i = 0; i < args.Count; i++) {
				XPathSequence val = args [i].Evaluate (iter);
				instParams [i] = Args [i].Type.ToRuntimeType (val);
			}
			object o = Invoke (iter, instParams);
			if (o == null)
				return new XPathEmptySequence (iter.Context);
			if (o is XPathSequence)
				return (XPathSequence) o;
			XPathItem item = o as XPathItem;
			if (item == null)
				item = new XPathAtomicValue (o, ReturnType.SchemaType);
			return new SingleItemIterator (item, iter.Context);
		}
	}

	public class XQueryFunctionArgument
	{
		public XQueryFunctionArgument (XmlQualifiedName name, SequenceType type)
		{
			this.name = name;
			this.type = type;
		}

		XmlQualifiedName name;
		SequenceType type;

		public XmlQualifiedName Name {
			get { return name; }
		}

		public SequenceType Type {
			get { return type; }
		}
	}

	public class XQueryUserFunction : XQueryFunction
	{
		ExprSequence expr;

		internal XQueryUserFunction (XmlQualifiedName name,
			XQueryFunctionArgument [] parameters,
			ExprSequence expr,
			SequenceType returnType)
			: base (name, parameters, returnType)
		{
			this.expr = expr;
		}

		public override int MinArgs {
			get { return Args.Length; }
		}

		public override int MaxArgs {
			get { return Args.Length; }
		}

		public override object Invoke (XPathSequence current, object [] args)
		{
			throw new SystemException ("XQuery internal error: should not happen.");
		}

		public override XPathSequence Evaluate (XPathSequence iter, ExprSequence args)
		{
			for (int i = 0; i < Args.Length; i++)
				iter.Context.PushVariable (Args [i].Name, args [i].Evaluate (iter));
			XPathSequence seq = new ExprSequenceIterator (iter, expr);
			for (int i = 0; i < Args.Length; i++)
				iter.Context.PopVariable ();
			return seq;
		}
	}
}
#endif
