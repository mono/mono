//
// Commons.Xml.Relaxng.Derivative.Util.cs
//
// Author:
//	Atsushi Enomoto <ginga@kit.hi-ho.ne.jp>
//
// 2003 Atsushi Enomoto "No rights reserved."
//

using System;
using System.Collections;
using System.Xml;

namespace Commons.Xml.Relaxng.Derivative
{
	public class RdpUtil
	{
		public static char[] WhitespaceChars = " \t\r\n".ToCharArray ();

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
			default:
				return "<" + n.NameClassType.ToString () + "/>";
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

		// childDeriv :: Context -> Pattern -> ChildNode -> Pattern
		internal static RdpPattern ChildDeriv (RdpContext ctx, RdpPattern p, RdpChildNode child)
		{
			return p.ChildDeriv (child);
		}

		// textDeriv :: Context -> Pattern -> String -> Pattern
		internal static RdpPattern TextDeriv (RdpContext ctx, RdpPattern p, string s)
		{
			return p.TextDeriv (s);
		}

		// listDeriv :: Context -> Pattern -> [String] -> Pattern
		internal static RdpPattern ListDeriv (RdpContext ctx, RdpPattern p, string [] list)
		{
			return p.ListDeriv (list);
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

		internal static bool DatatypeAllows (RdpDatatype dt, RdpParamList pl, string value, RdpContext ctx)
		{
			return dt.IsAllowed (pl, value);
		}

		// datatypeEqual :: Datatype -> String -> Context -> String -> Context -> Bool
		internal static bool DatatypeEqual (RdpDatatype dt, string value1, RdpContext ctx1, string value2, RdpContext ctx2)
		{
			return dt.IsTypeEqual (value1, ctx1, value2);
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

		// attsDeriv :: Context -> Pattern -> [AttributeNode] -> Pattern
		// [implemented in RdpPattern]
		internal static RdpPattern AttsDeriv (RdpPattern p, RdpAttributes attributes)
		{
			return p.AttsDeriv (attributes);
		}

		// attDeriv :: Context -> Pattern -> AttributeNode -> Pattern
		// [all implemented]
		internal static RdpPattern AttDeriv (RdpContext ctx,
			RdpPattern p, string name, string ns, string value)
		{
			return p.AttDeriv (name, ns, value);
		}

		// valueMatch :: Context -> Pattern -> String -> Bool
		// [implemented in RdpPattern]
		internal static bool ValueMatch (RdpPattern p, string s)
		{
			return p.ValueMatch (s);
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

		// childrenDeriv :: Context -> Pattern -> [ChildNode] -> Pattern
		// [implemented in RdpPattern. also stripChildrenDeriv is.]
		internal static RdpPattern ChildrenDeriv (RdpContext ctx, 
			RdpPattern p, RdpChildNodes children)
		{
			return p.ChildrenDeriv (children);
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

