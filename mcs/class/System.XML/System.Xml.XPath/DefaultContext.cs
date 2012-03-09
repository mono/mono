//
// System.Xml.XPath.DefaultContext & support classes
//
// Author:
//   Piers Haken (piersh@friskit.com)
//
// (C) 2002 Piers Haken
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
using System.Globalization;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;
using System.Text;

namespace System.Xml.XPath
{
	internal class XPathFunctions
	{
		public static bool ToBoolean (object arg)
		{
			if (arg == null)
				throw new ArgumentNullException ();
			if (arg is bool)
				return (bool) arg;
			if (arg is double)
			{
				double dArg = (double) arg;
				return (dArg != 0.0 && !double.IsNaN (dArg));
			}
			if (arg is string)
				return ((string) arg).Length != 0;
			if (arg is XPathNodeIterator)
			{
				XPathNodeIterator iter = (XPathNodeIterator) arg;
				return iter.MoveNext ();
			}
			if (arg is XPathNavigator)
			{
				return ToBoolean (((XPathNavigator) arg).SelectChildren (XPathNodeType.All));
			}
			throw new ArgumentException ();
		}

		public static bool ToBoolean (bool b)
		{
			return b;
		}

		public static bool ToBoolean (double d)
		{
			return d != 0.0 && !Double.IsNaN (d);
		}

		public static bool ToBoolean (string s)
		{
			return s != null && s.Length > 0;
		}

		public static bool ToBoolean (BaseIterator iter)
		{
			return iter != null && iter.MoveNext ();
		}

		public static string ToString (object arg)
		{
			if (arg == null)
				throw new ArgumentNullException ();
			if (arg is string)
				return (string) arg;
			if (arg is bool)
				return ((bool) arg) ? "true" : "false";
			if (arg is double)
				return ToString ((double) arg);
			if (arg is XPathNodeIterator)
			{
				XPathNodeIterator iter = (XPathNodeIterator) arg;
				if (!iter.MoveNext ())
					return "";
				return iter.Current.Value;
			}
			if (arg is XPathNavigator)
			{
				return ((XPathNavigator) arg).Value;
			}
			throw new ArgumentException ();
		}

		public static string ToString (double d)
		{
			// See XPath 1.0 section 4.2
			if (d == Double.NegativeInfinity)
				return "-Infinity";
			if (d == Double.PositiveInfinity)
				return "Infinity";
			return d.ToString ("R", NumberFormatInfo.InvariantInfo);
		}

		public static double ToNumber (object arg)
		{
			if (arg == null)
				throw new ArgumentNullException ();
			if (arg is BaseIterator || arg is XPathNavigator)
				arg = ToString (arg);	// follow on
			if (arg is string) {
				string s = arg as string;
				return ToNumber (s); // use explicit overload
			}
			if (arg is double)
				return (double) arg;
			if (arg is bool)
				return Convert.ToDouble ((bool) arg);
			throw new ArgumentException ();
		}
		
		public static double ToNumber (string arg)
		{
			if (arg == null)
				throw new ArgumentNullException ();
			string s = arg.Trim (XmlChar.WhitespaceChars);
			if (s.Length == 0)
				return double.NaN;
			try {
				// workaround for allowed ".xxx" style.
				if (s [0] == '.')
					s = '.' + s;
				return Double.Parse (s, NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite | NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, NumberFormatInfo.InvariantInfo);
			} catch (System.OverflowException) {
				return double.NaN;
			} catch (System.FormatException) {
				return double.NaN;
			}
		}
	}

	internal abstract class XPathFunction : Expression
	{
		public XPathFunction (FunctionArguments args) {}
	}


	internal class XPathFunctionLast : XPathFunction
	{
		public XPathFunctionLast (FunctionArguments args) : base (args)
		{
			if (args != null)
				throw new XPathException ("last takes 0 args");
		}
		
		public override XPathResultType ReturnType { get { return XPathResultType.Number; }}

		internal override bool Peer {
			get { return true; }
		}

		public override object Evaluate (BaseIterator iter)
		{
			return (double) iter.Count;
		}

		public override string ToString ()
		{
			return "last()";
		}

		internal override bool IsPositional {
			get { return true; }
		}
	}


	internal class XPathFunctionPosition : XPathFunction
	{
		public XPathFunctionPosition (FunctionArguments args) : base (args)
		{
			if (args != null)
				throw new XPathException ("position takes 0 args");
		}
		
		public override XPathResultType ReturnType { get { return XPathResultType.Number; }}

		internal override bool Peer {
			get { return true; }
		}

		public override object Evaluate (BaseIterator iter)
		{
			return (double) iter.ComparablePosition;
		}

		public override string ToString ()
		{
			return "position()";
		}

		internal override bool IsPositional {
			get { return true; }
		}
	}


	internal class XPathFunctionCount : XPathFunction
	{
		Expression arg0;
		
		public XPathFunctionCount (FunctionArguments args) : base (args)
		{
			if (args == null || args.Tail != null)
				throw new XPathException ("count takes 1 arg");
			
			arg0 = args.Arg;
		}

		public override XPathResultType ReturnType { get { return XPathResultType.Number; }}

		internal override bool Peer {
			get { return arg0.Peer; }
		}

		public override object Evaluate (BaseIterator iter)
		{
			return (double) arg0.EvaluateNodeSet (iter).Count;
		}
		
		public override bool EvaluateBoolean (BaseIterator iter)
		{
			if (arg0.GetReturnType (iter) == XPathResultType.NodeSet)
				return arg0.EvaluateBoolean (iter);
			
			return arg0.EvaluateNodeSet (iter).MoveNext ();
		}

		public override string ToString ()
		{
			return "count(" + arg0.ToString () + ")";
		}
	}


	internal class XPathFunctionId : XPathFunction
	{
		Expression arg0;
		
		public XPathFunctionId (FunctionArguments args) : base (args)
		{
			if (args == null || args.Tail != null)
				throw new XPathException ("id takes 1 arg");
			
			arg0 = args.Arg;
		}

		public Expression Id { get { return arg0; } }
		
		private static char [] rgchWhitespace = {' ', '\t', '\r', '\n'};
		public override XPathResultType ReturnType { get { return XPathResultType.NodeSet; }}

		internal override bool Peer {
			get { return arg0.Peer; }
		}

		public override object Evaluate (BaseIterator iter)
		{
			String strArgs;
			object val = arg0.Evaluate (iter);
			
			XPathNodeIterator valItr = val as XPathNodeIterator;
			if (valItr != null)
			{
				strArgs = "";
				while (valItr.MoveNext ())
					strArgs += valItr.Current.Value + " ";
			}
			else
				strArgs = XPathFunctions.ToString (val);
			
			XPathNavigator n = iter.Current.Clone ();
			ArrayList rgNodes = new ArrayList ();
			string [] ids = strArgs.Split (rgchWhitespace);
			for (int i = 0; i < ids.Length; i++)
				if (n.MoveToId (ids [i]))
					rgNodes.Add (n.Clone ());

			rgNodes.Sort (XPathNavigatorComparer.Instance);
			return new ListIterator (iter, rgNodes);
		}

		public override string ToString ()
		{
			return "id(" + arg0.ToString () + ")";
		}
	}

	internal class XPathFunctionLocalName : XPathFunction
	{
		Expression arg0;
		
		public XPathFunctionLocalName (FunctionArguments args) : base (args)
		{
			if (args != null) {
				arg0 = args.Arg;
				if (args.Tail != null)
					throw new XPathException ("local-name takes 1 or zero args");
			}
		}
		
		public override XPathResultType ReturnType { get { return XPathResultType.String; }}

		internal override bool Peer {
			get { return arg0 != null ? arg0.Peer : true; }
		}
		
		public override object Evaluate (BaseIterator iter)
		{
			if (arg0 == null)
				return iter.Current.LocalName;
			
			BaseIterator argNs = arg0.EvaluateNodeSet (iter);
			if (argNs == null || !argNs.MoveNext ())
				return "";
			return argNs.Current.LocalName;
		}

		public override string ToString ()
		{
			return "local-name(" + arg0.ToString () + ")";
		}
	}


	internal class XPathFunctionNamespaceUri : XPathFunction
	{
		Expression arg0;
		
		public XPathFunctionNamespaceUri (FunctionArguments args) : base (args)
		{
			if (args != null) {
				arg0 = args.Arg;
				if (args.Tail != null)
					throw new XPathException ("namespace-uri takes 1 or zero args");
			}
		}

		internal override bool Peer {
			get { return arg0 != null ? arg0.Peer : true; }
		}
		
		public override XPathResultType ReturnType { get { return XPathResultType.String; }}
		
		public override object Evaluate (BaseIterator iter)
		{
			if (arg0 == null)
				return iter.Current.NamespaceURI;
			
			BaseIterator argNs = arg0.EvaluateNodeSet (iter);
			if (argNs == null || !argNs.MoveNext ())
				return "";
			return argNs.Current.NamespaceURI;
		}

		public override string ToString ()
		{
			return "namespace-uri(" + arg0.ToString () + ")";
		}
	}


	internal class XPathFunctionName : XPathFunction
	{
		Expression arg0;
		
		public XPathFunctionName (FunctionArguments args) : base (args)
		{
			if (args != null) {
				arg0 = args.Arg;
				if (args.Tail != null)
					throw new XPathException ("name takes 1 or zero args");
			}
		}
		
		public override XPathResultType ReturnType { get { return XPathResultType.String; }}

		internal override bool Peer {
			get { return arg0 != null ? arg0.Peer : true; }
		}
		
		public override object Evaluate (BaseIterator iter)
		{
			if (arg0 == null)
				return iter.Current.Name;
			
			BaseIterator argNs = arg0.EvaluateNodeSet (iter);
			if (argNs == null || !argNs.MoveNext ())
				return "";
			return argNs.Current.Name;
		}

		public override string ToString ()
		{
			return String.Concat ("name(",
				 arg0 != null ? arg0.ToString () : String.Empty,
				 ")");
		}
	}


	internal class XPathFunctionString : XPathFunction
	{
		Expression arg0;
		
		public XPathFunctionString (FunctionArguments args) : base (args)
		{
			if (args != null) {
				arg0 = args.Arg;
				if (args.Tail != null)
					throw new XPathException ("string takes 1 or zero args");
			}
		}
		
		public override XPathResultType ReturnType { get { return XPathResultType.String; }}

		internal override bool Peer {
			get { return arg0 != null ? arg0.Peer : true; }
		}

		public override object Evaluate (BaseIterator iter)
		{
			if (arg0 == null)
				return iter.Current.Value;
			return arg0.EvaluateString (iter);
		}

		public override string ToString ()
		{
			return "string(" + arg0.ToString () + ")";
		}
	}


	internal class XPathFunctionConcat : XPathFunction
	{
		ArrayList rgs;
		
		public XPathFunctionConcat (FunctionArguments args) : base (args)
		{
			if (args == null || args.Tail == null)
				throw new XPathException ("concat takes 2 or more args");
			
			args.ToArrayList (rgs = new ArrayList ());
		}
		
		public override XPathResultType ReturnType { get { return XPathResultType.String; }}

		internal override bool Peer {
			get {
				for (int i = 0; i < rgs.Count; i++)
					if (!((Expression) rgs [i]).Peer)
						return false;
				return true;
			}
		}
		
		public override object Evaluate (BaseIterator iter)
		{
			StringBuilder sb = new StringBuilder ();
			
			int len = rgs.Count;
			for (int i = 0; i < len; i++)
				sb.Append (((Expression)rgs[i]).EvaluateString (iter));
			
			return sb.ToString ();
		}

		public override string ToString ()
		{
			StringBuilder sb = new StringBuilder ();
			sb.Append ("concat(");
			for (int i = 0; i < rgs.Count - 1; i++) {
				sb.AppendFormat (CultureInfo.InvariantCulture, "{0}", rgs [i].ToString ());
				sb.Append (',');
			}
			sb.AppendFormat (CultureInfo.InvariantCulture, "{0}", rgs [rgs.Count - 1].ToString ());
			sb.Append (')');
			return sb.ToString ();
		}
	}


	internal class XPathFunctionStartsWith : XPathFunction
	{
		Expression arg0, arg1;
		
		public XPathFunctionStartsWith (FunctionArguments args) : base (args)
		{
			if (args == null || args.Tail == null || args.Tail.Tail != null)
				throw new XPathException ("starts-with takes 2 args");
			
			arg0 = args.Arg;
			arg1 = args.Tail.Arg;
		}
		
		public override XPathResultType ReturnType { get { return XPathResultType.Boolean; }}

		internal override bool Peer {
			get { return arg0.Peer && arg1.Peer; }
		}
		
		public override object Evaluate (BaseIterator iter)
		{
			return arg0.EvaluateString (iter).StartsWith (arg1.EvaluateString (iter));
		}

		public override string ToString ()
		{
			return String.Concat ("starts-with(", arg0.ToString (), ",", arg1.ToString (), ")");
		}
	}

	internal class XPathFunctionContains : XPathFunction
	{
		Expression arg0, arg1;
		
		public XPathFunctionContains (FunctionArguments args) : base (args)
		{
			if (args == null || args.Tail == null || args.Tail.Tail != null)
				throw new XPathException ("contains takes 2 args");
			
			arg0 = args.Arg;
			arg1 = args.Tail.Arg;
		}
		
		public override XPathResultType ReturnType { get { return XPathResultType.Boolean; }}

		internal override bool Peer {
			get { return arg0.Peer && arg1.Peer; }
		}
		
		public override object Evaluate (BaseIterator iter)
		{
			return arg0.EvaluateString (iter).IndexOf (arg1.EvaluateString (iter)) != -1;
		}

		public override string ToString ()
		{
			return String.Concat ("contains(", arg0.ToString (), ",", arg1.ToString (), ")");
		}
	}


	internal class XPathFunctionSubstringBefore : XPathFunction
	{
		Expression arg0, arg1;
		
		public XPathFunctionSubstringBefore (FunctionArguments args) : base (args)
		{
			if (args == null || args.Tail == null || args.Tail.Tail != null)
				throw new XPathException ("substring-before takes 2 args");
			
			arg0 = args.Arg;
			arg1 = args.Tail.Arg;
		}
		
		public override XPathResultType ReturnType { get { return XPathResultType.String; }}

		internal override bool Peer {
			get { return arg0.Peer && arg1.Peer; }
		}
		
		public override object Evaluate (BaseIterator iter)
		{
			string str1 = arg0.EvaluateString (iter);
			string str2 = arg1.EvaluateString (iter);
			int ich = str1.IndexOf (str2);
			if (ich <= 0)
				return "";
			return str1.Substring (0, ich);
		}

		public override string ToString ()
		{
			return String.Concat ("substring-before(", arg0.ToString (), ",", arg1.ToString (), ")");
		}
	}


	internal class XPathFunctionSubstringAfter : XPathFunction
	{
		Expression arg0, arg1;
		
		public XPathFunctionSubstringAfter (FunctionArguments args) : base (args)
		{
			if (args == null || args.Tail == null || args.Tail.Tail != null)
				throw new XPathException ("substring-after takes 2 args");
			
			arg0 = args.Arg;
			arg1 = args.Tail.Arg;
		}
		
		public override XPathResultType ReturnType { get { return XPathResultType.String; }}

		internal override bool Peer {
			get { return arg0.Peer && arg1.Peer; }
		}
		
		public override object Evaluate (BaseIterator iter)
		{
			string str1 = arg0.EvaluateString (iter);
			string str2 = arg1.EvaluateString (iter);
			int ich = str1.IndexOf (str2);
			if (ich < 0)
				return "";
			return str1.Substring (ich + str2.Length);
		}

		public override string ToString ()
		{
			return String.Concat ("substring-after(", arg0.ToString (), ",", arg1.ToString (), ")");
		}
	}


	internal class XPathFunctionSubstring : XPathFunction
	{
		Expression arg0, arg1, arg2;
		
		public XPathFunctionSubstring (FunctionArguments args) : base (args)
		{
			if (args == null || args.Tail == null || (args.Tail.Tail != null && args.Tail.Tail.Tail != null))
				throw new XPathException ("substring takes 2 or 3 args");
			
			arg0 = args.Arg;
			arg1 = args.Tail.Arg;
			if (args.Tail.Tail != null)
				arg2= args.Tail.Tail.Arg;
		}
		
		public override XPathResultType ReturnType { get { return XPathResultType.String; }}

		internal override bool Peer {
			get { return arg0.Peer && arg1.Peer && (arg2 != null ? arg2.Peer : true); }
		}
		
		public override object Evaluate (BaseIterator iter)
		{
			string str = arg0.EvaluateString (iter);
			double ich = Math.Round (arg1.EvaluateNumber (iter)) - 1;
			if (Double.IsNaN (ich) ||
				Double.IsNegativeInfinity (ich) ||
				ich >= (double) str.Length)
				return "";

			if (arg2 == null)
			{
				if (ich < 0)
					ich = 0.0;
				return str.Substring ((int) ich);
			}
			else
			{
				double cch = Math.Round (arg2.EvaluateNumber (iter));
				if (Double.IsNaN (cch))
					return "";
				if (ich < 0.0 || cch < 0.0) 
				{
					cch = ich + cch;
					if (cch <= 0.0)
						return "";
					ich = 0.0;
				}
				double cchMax = (double) str.Length - ich;
				if (cch > cchMax)
					cch = cchMax;
				return str.Substring ((int) ich, (int) cch);
			}
		}

		public override string ToString ()
		{
			return String.Concat (new string [] {
				"substring(", arg0.ToString (), ",", arg1.ToString (), ",", arg2.ToString (), ")"});
		}
	}


	internal class XPathFunctionStringLength : XPathFunction
	{
		Expression arg0;
		
		public XPathFunctionStringLength (FunctionArguments args) : base (args)
		{
			if (args != null) {
				arg0 = args.Arg;
				if (args.Tail != null)
					throw new XPathException ("string-length takes 1 or zero args");
			}
		}
		
		public override XPathResultType ReturnType { get { return XPathResultType.Number; }}

		internal override bool Peer {
			get { return arg0 != null ? arg0.Peer : true; }
		}
		
		public override object Evaluate (BaseIterator iter)
		{
			string str;
			if (arg0 != null)
				str = arg0.EvaluateString (iter);
			else
				str = iter.Current.Value;
			return (double) str.Length;
		}

		public override string ToString ()
		{
			return String.Concat (new string [] {
				"string-length(", arg0.ToString (), ")"});
		}
	}


	internal class XPathFunctionNormalizeSpace : XPathFunction
	{
		Expression arg0;
		
		public XPathFunctionNormalizeSpace (FunctionArguments args) : base (args)
		{
			if (args != null) {
				arg0 = args.Arg;
				if (args.Tail != null)
					throw new XPathException ("normalize-space takes 1 or zero args");
			}
		}
		
		public override XPathResultType ReturnType { get { return XPathResultType.String; }}

		internal override bool Peer {
			get { return arg0 !=null ? arg0.Peer : true; }
		}

		public override object Evaluate (BaseIterator iter)
		{
			string str;
			if (arg0 != null)
				str = arg0.EvaluateString (iter);
			else
				str = iter.Current.Value;
			System.Text.StringBuilder sb = new System.Text.StringBuilder ();
			bool fSpace = false;
			for (int i = 0; i < str.Length; i++) {
				char ch = str [i];
				if (ch == ' ' || ch == '\t' || ch == '\r' || ch == '\n')
				{
					fSpace = true;
				}
				else
				{
					if (fSpace)
					{
						fSpace = false;
						if (sb.Length > 0)
							sb.Append (' ');
					}
					sb.Append (ch);
				}
			}
			return sb.ToString ();
		}

		public override string ToString ()
		{
			return String.Concat (new string [] {
				"normalize-space(",
				arg0 != null ? arg0.ToString () : String.Empty,
				")"});
		}
	}


	internal class XPathFunctionTranslate : XPathFunction
	{
		Expression arg0, arg1, arg2;
		
		public XPathFunctionTranslate (FunctionArguments args) : base (args)
		{
			if (args == null || args.Tail == null || args.Tail.Tail == null || args.Tail.Tail.Tail != null)
				throw new XPathException ("translate takes 3 args");
			
			arg0 = args.Arg;
			arg1 = args.Tail.Arg;
			arg2= args.Tail.Tail.Arg;
		}
		
		public override XPathResultType ReturnType { get { return XPathResultType.String; }}

		internal override bool Peer {
			get { return arg0.Peer && arg1.Peer && arg2.Peer; }
		}
		
		public override object Evaluate (BaseIterator iter)
		{
			string s0 = arg0.EvaluateString (iter);
			string s1 = arg1.EvaluateString (iter);
			string s2 = arg2.EvaluateString (iter);
			
			StringBuilder ret = new StringBuilder (s0.Length);
				
			int pos = 0, len = s0.Length, s2len = s2.Length;
			
			while (pos < len) {
				int idx = s1.IndexOf (s0 [pos]);
				
				if (idx != -1) {
					if (idx < s2len)
						ret.Append (s2 [idx]);
				}
				else
					ret.Append (s0 [pos]);
				
				pos++;
			}
			
			return ret.ToString ();
		}

		public override string ToString ()
		{
			return String.Concat (new string [] {
				"string-length(",
				arg0.ToString (), ",",
				arg1.ToString (), ",",
				arg2.ToString (), ")"});
		}
	}

	internal abstract class XPathBooleanFunction : XPathFunction
	{
		public XPathBooleanFunction (FunctionArguments args) : base (args)
		{
		}

		public override XPathResultType ReturnType { get { return XPathResultType.Boolean; }}

		public override object StaticValue {
			get { return StaticValueAsBoolean; }
		}
	}

	internal class XPathFunctionBoolean : XPathBooleanFunction
	{
		Expression arg0;
		
		public XPathFunctionBoolean (FunctionArguments args) : base (args)
		{
			if (args != null) {
				arg0 = args.Arg;
				if (args.Tail != null)
					throw new XPathException ("boolean takes 1 or zero args");
			}
		}
		
		internal override bool Peer {
			get { return arg0 != null ? arg0.Peer : true; }
		}

		public override object Evaluate (BaseIterator iter)
		{
			if (arg0 == null)
				return XPathFunctions.ToBoolean (iter.Current.Value);
			return arg0.EvaluateBoolean (iter);
		}

		public override string ToString ()
		{
			return String.Concat (new string [] {"boolean(", arg0.ToString (), ")"});
		}
	}


	internal class XPathFunctionNot : XPathBooleanFunction
	{
		Expression arg0;
		
		public XPathFunctionNot (FunctionArguments args) : base (args)
		{
			if (args == null || args.Tail != null)
				throw new XPathException ("not takes one arg");
			arg0 = args.Arg;
		}
		
		internal override bool Peer {
			get { return arg0.Peer; }
		}

		public override object Evaluate (BaseIterator iter)
		{
			return !arg0.EvaluateBoolean (iter);
		}

		public override string ToString ()
		{
			return String.Concat (new string [] {"not(", arg0.ToString (), ")"});
		}
	}


	internal class XPathFunctionTrue : XPathBooleanFunction
	{
		public XPathFunctionTrue (FunctionArguments args) : base (args)
		{
			if (args != null)
				throw new XPathException ("true takes 0 args");
		}

		public override bool HasStaticValue {
			get { return true; }
		}

		public override bool StaticValueAsBoolean {
			get { return true; }
		}
		
		internal override bool Peer {
			get { return true; }
		}

		public override object Evaluate (BaseIterator iter)
		{
			return true;
		}

		public override string ToString ()
		{
			return "true()";
		}
	}


	internal class XPathFunctionFalse : XPathBooleanFunction
	{
		public XPathFunctionFalse (FunctionArguments args) : base (args)
		{
			if (args != null)
				throw new XPathException ("false takes 0 args");
		}

		public override bool HasStaticValue {
			get { return true; }
		}

		public override bool StaticValueAsBoolean {
			get { return false; }
		}

		internal override bool Peer {
			get { return true; }
		}

		public override object Evaluate (BaseIterator iter)
		{
			return false;
		}

		public override string ToString ()
		{
			return "false()";
		}
	}


	internal class XPathFunctionLang : XPathFunction
	{
		Expression arg0;
		
		public XPathFunctionLang (FunctionArguments args) : base (args)
		{
			if (args == null || args.Tail != null)
				throw new XPathException ("lang takes one arg");
			arg0 = args.Arg;
		}

		public override XPathResultType ReturnType { get { return XPathResultType.Boolean; }}

		internal override bool Peer {
			get { return arg0.Peer; }
		}

		public override object Evaluate (BaseIterator iter)
		{
			return EvaluateBoolean (iter);
		}

		public override bool EvaluateBoolean (BaseIterator iter)
		{
			string lang = arg0.EvaluateString (iter).ToLower (CultureInfo.InvariantCulture);
			string actualLang = iter.Current.XmlLang.ToLower (CultureInfo.InvariantCulture);
			
			return lang == actualLang || lang == (actualLang.Split ('-')[0]);
		}

		public override string ToString ()
		{
			return String.Concat (new string [] {"lang(", arg0.ToString (), ")"});
		}
	}

	internal abstract class XPathNumericFunction : XPathFunction
	{
		internal XPathNumericFunction (FunctionArguments args)
			: base (args)
		{
		}

		public override XPathResultType ReturnType { get { return XPathResultType.Number; }}

		public override object StaticValue {
			get { return StaticValueAsNumber; }
		}
	}

	internal class XPathFunctionNumber : XPathNumericFunction
	{
		Expression arg0;
		
		public XPathFunctionNumber (FunctionArguments args) : base (args)
		{
			if (args != null) {
				arg0 = args.Arg;
				if (args.Tail != null)
					throw new XPathException ("number takes 1 or zero args");
			}
		}

		public override Expression Optimize ()
		{
			if (arg0 == null)
				return this;
			arg0 = arg0.Optimize ();
			return !arg0.HasStaticValue ?
				(Expression) this :
				new ExprNumber (StaticValueAsNumber);
		}

		public override bool HasStaticValue {
			get { return arg0 != null && arg0.HasStaticValue; }
		}

		public override double StaticValueAsNumber {
			get { return arg0 != null ? arg0.StaticValueAsNumber : 0; }
		}

		internal override bool Peer {
			get { return arg0 != null ? arg0.Peer : true; }
		}

		public override object Evaluate (BaseIterator iter)
		{
			if (arg0 == null)
				return XPathFunctions.ToNumber (iter.Current.Value);
			return arg0.EvaluateNumber (iter);
		}

		public override string ToString ()
		{
			return String.Concat (new string [] {"number(", arg0.ToString (), ")"});
		}
	}


	internal class XPathFunctionSum : XPathNumericFunction
	{
		Expression arg0;
		
		public XPathFunctionSum (FunctionArguments args) : base (args)
		{
			if (args == null || args.Tail != null)
				throw new XPathException ("sum takes one arg");
			arg0 = args.Arg;
		}
		
		internal override bool Peer {
			get { return arg0.Peer; }
		}

		public override object Evaluate (BaseIterator iter)
		{
			XPathNodeIterator itr = arg0.EvaluateNodeSet (iter);
			
			double sum = 0;
			while (itr.MoveNext ())
				sum += XPathFunctions.ToNumber (itr.Current.Value);
			
			return sum;
		}

		public override string ToString ()
		{
			return String.Concat (new string [] {"sum(", arg0.ToString (), ")"});
		}
	}


	internal class XPathFunctionFloor : XPathNumericFunction
	{
		Expression arg0;
		
		public XPathFunctionFloor (FunctionArguments args) : base (args)
		{
			if (args == null || args.Tail != null)
				throw new XPathException ("floor takes one arg");
			arg0 = args.Arg;
		}

		public override bool HasStaticValue {
			get { return arg0.HasStaticValue; }
		}

		public override double StaticValueAsNumber {
			get { return HasStaticValue ? Math.Floor (arg0.StaticValueAsNumber) : 0; }
		}
		
		internal override bool Peer {
			get { return arg0.Peer; }
		}

		public override object Evaluate (BaseIterator iter)
		{
			return Math.Floor (arg0.EvaluateNumber (iter));
		}

		public override string ToString ()
		{
			return String.Concat (new string [] {"floor(", arg0.ToString (), ")"});
		}
	}


	internal class XPathFunctionCeil : XPathNumericFunction
	{
		Expression arg0;
		
		public XPathFunctionCeil (FunctionArguments args) : base (args)
		{
			if (args == null || args.Tail != null)
				throw new XPathException ("ceil takes one arg");
			arg0 = args.Arg;
		}

		public override bool HasStaticValue {
			get { return arg0.HasStaticValue; }
		}

		public override double StaticValueAsNumber {
			get { return HasStaticValue ? Math.Ceiling (arg0.StaticValueAsNumber) : 0; }
		}
		
		internal override bool Peer {
			get { return arg0.Peer; }
		}

		public override object Evaluate (BaseIterator iter)
		{
			return Math.Ceiling (arg0.EvaluateNumber (iter));
		}

		public override string ToString ()
		{
			return String.Concat (new string [] {"ceil(", arg0.ToString (), ")"});
		}
	}


	internal class XPathFunctionRound : XPathNumericFunction
	{
		Expression arg0;
		
		public XPathFunctionRound (FunctionArguments args) : base (args)
		{
			if (args == null || args.Tail != null)
				throw new XPathException ("round takes one arg");
			arg0 = args.Arg;
		}
		
		public override bool HasStaticValue {
			get { return arg0.HasStaticValue; }
		}

		public override double StaticValueAsNumber {
			get { return HasStaticValue ? Round (arg0.StaticValueAsNumber) : 0; }
		}

		internal override bool Peer {
			get { return arg0.Peer; }
		}

		public override object Evaluate (BaseIterator iter)
		{
			return Round (arg0.EvaluateNumber (iter));
		}

		private double Round (double arg)
		{
			if (arg < -0.5 || arg > 0)
				return Math.Floor (arg + 0.5);
			return Math.Round (arg);
		}

		public override string ToString ()
		{
			return String.Concat (new string [] {"round(", arg0.ToString (), ")"});
		}
	}
}
