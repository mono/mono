//
// Commons.Xml.Relaxng.Derivative.Util.cs
//
// Author:
//	Atsushi Enomoto <ginga@kit.hi-ho.ne.jp>
//
// 2003 Atsushi Enomoto "No rights reserved."
//
// Copyright (c) 2004 Novell Inc.
// All rights reserved
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

using QName = System.Xml.XmlQualifiedName;

namespace Commons.Xml.Relaxng.Derivative
{
	public class RdpUtil
	{
		public static char[] WhitespaceChars = " \t\r\n".ToCharArray ();

		internal static RdpBinaryFunction AfterFunction =
			new RdpBinaryFunction (After);
		internal static RdpBinaryFunction GroupFunction =
			new RdpBinaryFunction (Group);
		internal static RdpBinaryFunction InterleaveFunction =
			new RdpBinaryFunction (Interleave);

		// Generating simplified pattern status (similar to XML representation).
		#region Debug
		private static string DebugNameClass (RdpNameClass n)
		{
			switch (n.NameClassType) {
			case RdpNameClassType.Name:
				RdpName nm = (RdpName)n;
				return "<name ns='" + nm.NamespaceURI + "'>"
					+ nm.LocalName + "</name>\n";
			case RdpNameClassType.NsName:
				return "<nsName ns='" + ((RdpNsName)n).NamespaceURI + "'/>\n";
			case RdpNameClassType.NameClassChoice:
				RdpNameClassChoice nc = (RdpNameClassChoice) n;
				return "<choice>" + DebugNameClass (nc.LValue) + DebugNameClass (nc.RValue) + "</choice>";
			default:
				return "<" + n.NameClassType.ToString () + "/>\n";
			}
		}

		internal static string DebugRdpPattern (RdpPattern p, Hashtable visitedPattern)
		{
			if (p is RdpText)
				return "<text/>\n";
			if (p is RdpEmpty)
				return "<empty/>\n";
			if (p is RdpNotAllowed)
				return "<notAllowed/>\n";

			if (visitedPattern.Contains (p))
				return "<" + p.PatternType + " ref='" + p.GetHashCode () + "'/>";
			visitedPattern.Add (p, p);
			string intl = "(id=" + p.GetHashCode () + ") ";

			RdpAbstractSingleContent s = p as RdpAbstractSingleContent;
			if (s != null)
				intl = DebugRdpPattern (s.Child, visitedPattern);
			RdpAbstractBinary b = p as RdpAbstractBinary;
			if (b != null)
				intl = DebugRdpPattern (b.LValue, visitedPattern) +
					DebugRdpPattern (b.RValue, visitedPattern);

			RdpData data = p as RdpData;
			if (data != null)
				intl = String.Format ("name={0},ns={1},type={2} {3}",
					data.Datatype.LocalName,
					data.Datatype.NamespaceURI,
					data.Datatype.GetType (),
					data is RdpDataExcept ? DebugRdpPattern (((RdpDataExcept) data).Except, visitedPattern) : String.Empty);

			RdpValue value = p as RdpValue;
			if (value != null)
				intl = String.Format ("name={0},ns={1},value={2} type={3}",
					value.Datatype.LocalName,
					value.Datatype.NamespaceURI,
					value.Value,
					value.Datatype.GetType ());

			RdpElement el = p as RdpElement;
			if (el != null)
				intl = DebugNameClass (el.NameClass) +
					DebugRdpPattern (el.Children, visitedPattern);

			RdpAttribute at = p as RdpAttribute;
			if (at != null)
				intl = DebugNameClass (at.NameClass) +
					DebugRdpPattern (at.Children, visitedPattern);

			string str = String.Format ("<{0} id='id{1}'>\n{2}\n</{0}>",
				p.PatternType.ToString (),
				p.GetHashCode (),
				intl);

			return str;
		}
		#endregion

		// contains :: NameClass -> QName -> Bool
		internal static bool Contains (RdpNameClass nc, string name, string ns)
		{
			return nc.Contains (name, ns);
		}

		// nullable :: Pattern -> Bool
		internal static bool Nullable (RdpPattern p)
		{
			return p.Nullable;
		}

		/*
		// childDeriv :: Context -> Pattern -> ChildNode -> Pattern
		internal static RdpPattern ChildDeriv (RdpContext ctx, RdpPattern p, RdpChildNode child)
		{
			return p.ChildDeriv (child);
		}
		*/

		// textDeriv :: Context -> Pattern -> String -> Pattern
		internal static RdpPattern TextDeriv (XmlReader reader, RdpPattern p, string s)
		{
			return p.TextDeriv (s, reader);
		}

		// listDeriv :: Context -> Pattern -> [String] -> Pattern
		internal static RdpPattern ListDeriv (XmlReader reader, RdpPattern p, string [] list)
		{
			return p.ListDeriv (list, 0, reader);
		}

		// choice :: Pattern -> Pattern -> Pattern
		internal static RdpPattern Choice (RdpPattern p1, RdpPattern p2)
		{
			return p1.Choice (p2);
		}

		// group :: Pattern -> Pattern -> Pattern
		internal static RdpPattern Group (RdpPattern p1, RdpPattern p2)
		{
			return p1.Group (p2);
		}

		// interleave :: Pattern -> Pattern -> Pattern
		internal static RdpPattern Interleave (RdpPattern p1, RdpPattern p2)
		{
			return p1.Interleave (p2);
		}

		// after :: Pattern -> Pattern -> Pattern
		internal static RdpPattern After (RdpPattern p1, RdpPattern p2)
		{
			return p1.After (p2);
		}

		// datatypeAllows :: Datatype -> ParamList -> String -> Context -> Bool

		internal static bool DatatypeAllows (RdpDatatype dt, string value, XmlReader reader)
		{
			return dt.IsAllowed (value, reader);
		}

		// datatypeEqual :: Datatype -> String -> Context -> String -> Context -> Bool
		internal static bool DatatypeEqual (RdpDatatype dt, string value1, string value2, XmlReader reader)
		{
			return dt.IsTypeEqual (value1, value2, reader);
		}

#if false
		// normalizeWhitespace :: String -> String
		internal static string NormalizeString (string s)
		{
			throw new NotImplementedException ();
		}
#endif

#if false
		// applyAfter :: (Pattern -> Pattern) -> Pattern -> Pattern
		internal static RdpPattern ApplyAfter (RdpApplyAfterHandler h, RdpPattern p)
		{
			if (p is RdpAfter)
				return After (p.LValue, h (p.RValue));
		}
#endif

		#region Validation Core
		// startTagOpenDeriv :: Pattern -> QName -> Pattern
		// TODO remains: Interleave, OneOrMore, Group, After
		internal static RdpPattern StartTagOpenDeriv (RdpPattern pattern, string name, string ns)
		{
			return pattern.StartTagOpenDeriv (name, ns);
		}

		/*
		// attsDeriv :: Context -> Pattern -> [AttributeNode] -> Pattern
		// [implemented in RdpPattern]
		internal static RdpPattern AttsDeriv (RdpPattern p, RdpAttributes attributes)
		{
			return p.AttsDeriv (attributes);
		}
		*/

		// attDeriv :: Context -> Pattern -> AttributeNode -> Pattern
		// [all implemented]
		internal static RdpPattern AttDeriv (XmlReader reader,
			RdpPattern p, string name, string ns, string value)
		{
			return p.AttDeriv (name, ns, value, reader);
		}

		// valueMatch :: Context -> Pattern -> String -> Bool
		// [implemented in RdpPattern]
		internal static bool ValueMatch (RdpPattern p, string s, XmlReader reader)
		{
			return p.ValueMatch (s, reader);
		}

		// startTagCloseDeriv :: Pattern -> Pattern
		// [implemented]
		internal static RdpPattern StartTagCloseDeriv (RdpPattern p)
		{
			return p.StartTagCloseDeriv ();
		}

		// oneOrMore :: Pattern -> Pattern
		// [implemented in RdpPattern]
		internal static RdpPattern OneOrMore (RdpPattern p)
		{
			return p.OneOrMore ();
		}

		// writespace :: String -> Bool
		// [implemented here]
		internal static bool Whitespace (string s)
		{
			return s.Trim (WhitespaceChars).Length == 0;
		}

		// endTagDeriv :: Pattern -> Pattern
		// [implemented]
		internal static RdpPattern EndTagDeriv (RdpPattern p)
		{
			return p.EndTagDeriv ();
		}

		// Name class analysis
		internal static bool NamesOverlap (RdpPattern p1,
			RdpPattern p2, bool checkElements)
		{
			if (p1 == p2)
				return true;
			RdpAbstractBinary bp1 = p1 as RdpAbstractBinary;
			if (bp1 != null)
				return NamesOverlap (bp1.LValue, p2, checkElements)
					|| NamesOverlap (bp1.RValue, p2, checkElements);
			RdpOneOrMore rp1 = p1 as RdpOneOrMore;
			if (rp1 != null)
				return NamesOverlap (rp1.Child, p2, checkElements);
			RdpAttribute ap1 = p1 as RdpAttribute;
			if (ap1 != null)
				return NamesOverlap (p2, ap1.NameClass, checkElements);

			if (!checkElements)
				return false;

			RdpElement ep1 = p1 as RdpElement;
			if (ep1 != null)
				return NamesOverlap (p2, ep1.NameClass, checkElements);

			return false;
		}

		// Name class analysis
		static bool NamesOverlap (RdpPattern p1,
			RdpNameClass n, bool checkElements)
		{
			RdpAbstractBinary bp1 = p1 as RdpAbstractBinary;
			if (bp1 != null)
				return NamesOverlap (bp1.LValue, n, checkElements)
					|| NamesOverlap (bp1.RValue, n, checkElements);
			RdpOneOrMore rp1 = p1 as RdpOneOrMore;
			if (rp1 != null)
				return NamesOverlap (rp1.Child, n, checkElements);
			RdpAttribute ap1 = p1 as RdpAttribute;
			if (ap1 != null)
				return NameClassOverlap (ap1.NameClass, n);

			if (!checkElements)
				return false;

			RdpElement ep1 = p1 as RdpElement;
			if (ep1 != null)
				return NameClassOverlap (ep1.NameClass, n);

			return false;
		}

		internal static bool NameClassOverlap (RdpNameClass n1, RdpNameClass n2)
		{
			Hashtable names = new Hashtable ();
			GetNameClassRepresentatives (n1, names);
			GetNameClassRepresentatives (n2, names);
			foreach (QName qn in names.Keys)
				if (NameClassBothContain (n1, n2, qn))
					return true;
			return false;
		}

		static QName illegalQName = new QName ("", "\x1");

		static void GetNameClassRepresentatives (
			RdpNameClass n, Hashtable names)
		{
			if (n is RdpAnyName) {
				names [illegalQName] = illegalQName;
				return;
			}
			RdpAnyNameExcept exc = n as RdpAnyNameExcept;
			if (exc != null) {
				GetNameClassRepresentatives (exc.ExceptNameClass, names);
				names [illegalQName] = illegalQName;
				return;
			}
			RdpNsName ns = n as RdpNsName;
			if (ns != null) {
				QName nn = new QName (String.Empty, ns.NamespaceURI);
				names [nn] = nn;
				return;
			}
			RdpNsNameExcept nse = n as RdpNsNameExcept;
			if (nse != null) {
				GetNameClassRepresentatives (nse.ExceptNameClass, names);
				QName nn = new QName (String.Empty, nse.NamespaceURI);
				names [nn] = nn;
				return;
			}
			RdpName name = n as RdpName;
			if (name != null) {
				QName qname = new QName (name.LocalName, name.NamespaceURI);
				names [qname] = qname;
				return;
			}
			else {
				RdpNameClassChoice c = (RdpNameClassChoice) n;
				GetNameClassRepresentatives (c.LValue, names);
				GetNameClassRepresentatives (c.RValue, names);
				return;
			}
		}

		static bool NameClassBothContain (
			RdpNameClass n1, RdpNameClass n2, QName qn)
		{
			return Contains (n1, qn.Name, qn.Namespace) &&
				Contains (n2, qn.Name, qn.Namespace);
		}

		#endregion
	}

	public delegate RdpPattern RdpBinaryFunction (RdpPattern p1, RdpPattern p2);

	class RdpFlip
	{
		RdpBinaryFunction func;
		RdpPattern arg;

		public RdpFlip (RdpBinaryFunction func, RdpPattern p)
		{
			this.func = func;
			this.arg = p;
		}
		
		public RdpPattern Apply (RdpPattern p)
		{
			return func (p, arg);
		}
	}
}

