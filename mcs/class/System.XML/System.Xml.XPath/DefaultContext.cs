//
// System.Xml.XPath.DefaultContext & support classes
//
// Author:
//   Piers Haken (piersh@friskit.com)
//
// (C) 2002 Piers Haken
//
using System;
using System.Collections;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;

namespace System.Xml.XPath
{
	/// <summary>
	/// Summary description for DefaultContext.
	/// </summary>
	internal class DefaultContext : XsltContext
	{
		protected static Hashtable _htFunctions = new Hashtable ();

		static DefaultContext()
		{
			Add (new XPathFunctionLast ());
			Add (new XPathFunctionPosition ());
			Add (new XPathFunctionCount ());
			Add (new XPathFunctionId ());
			Add (new XPathFunctionLocalName ());
			Add (new XPathFunctionNamespaceUri ());
			Add (new XPathFunctionName ());
			Add (new XPathFunctionString ());
			Add (new XPathFunctionConcat ());
			Add (new XPathFunctionStartsWith ());
			Add (new XPathFunctionContains ());
			Add (new XPathFunctionSubstringBefore ());
			Add (new XPathFunctionSubstringAfter ());
			Add (new XPathFunctionSubstring ());
			Add (new XPathFunctionStringLength ());
			Add (new XPathFunctionNormalizeSpace ());
			Add (new XPathFunctionTranslate ());
			Add (new XPathFunctionBoolean ());
			Add (new XPathFunctionNot ());
			Add (new XPathFunctionTrue ());
			Add (new XPathFunctionFalse ());
			Add (new XPathFunctionLang ());
			Add (new XPathFunctionNumber ());
			Add (new XPathFunctionSum ());
			Add (new XPathFunctionFloor ());
			Add (new XPathFunctionCeil ());
			Add (new XPathFunctionRound ());
		}

		[MonoTODO]
		public override IXsltContextFunction ResolveFunction (string prefix, string name, XPathResultType[] ArgTypes)
		{
			// match the prefix
			if (prefix != null && prefix != "")	// TODO: should we allow some namespaces here?
				return null;

			// match the function name
			XPathFunction fn = (XPathFunction) _htFunctions [name];
			if (fn == null)
				return null;

			// check the number of arguments
			int cArgs = ArgTypes.Length;
			if (cArgs < fn.Minargs || cArgs > fn.Maxargs)
				return null;

			// check the types of the arguments
			XPathResultType [] rgTypes = fn.ArgTypes;
			if (rgTypes == null)
			{
				if (cArgs != 0)
					return null;
			}
			else
			{
				int cTypes = rgTypes.Length;
				XPathResultType [] rgTypesRequested = ArgTypes;
				for (int iArg = 0; iArg < cArgs; iArg ++)
				{
					XPathResultType typeRequested = rgTypesRequested [iArg];
					XPathResultType typeDefined = (iArg >= cTypes) ? rgTypes [cTypes - 1] : rgTypes [iArg];
					if (typeRequested != XPathResultType.NodeSet &&
						typeDefined != XPathResultType.Any &&
						typeDefined != typeRequested)
					{
						return null;
					}
				}
			}
			return fn;
		}
		public override IXsltContextVariable ResolveVariable (string prefix, string name)
		{
			return null;
		}
		[MonoTODO]
		public override int CompareDocument (string baseUri, string nextBaseUri) { throw new NotImplementedException (); }
		[MonoTODO]
		public override bool PreserveWhitespace (XPathNavigator nav) { throw new NotImplementedException (); }
		[MonoTODO]
		public override bool Whitespace { get { throw new NotImplementedException (); }}
		protected static void Add (XPathFunction fn)
		{
			_htFunctions.Add (fn.Name, fn);
		}
	}


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
			if (arg is BaseIterator)
			{
				BaseIterator iter = (BaseIterator) arg;
				return iter.MoveNext ();
			}
			throw new ArgumentException ();
		}
		[MonoTODO]
		public static string ToString (object arg)
		{
			if (arg == null)
				throw new ArgumentNullException ();
			if (arg is string)
				return (string) arg;
			if (arg is bool)
				return ((bool) arg) ? "true" : "false";
			if (arg is double)
				return ((double) arg).ToString ("R", System.Globalization.NumberFormatInfo.InvariantInfo);
			if (arg is BaseIterator)
			{
				BaseIterator iter = (BaseIterator) arg;
				if (!iter.MoveNext ())
					return "";
				return iter.Current.Value;
			}
			throw new ArgumentException ();
		}
		[MonoTODO]
		public static double ToNumber (object arg)
		{
			if (arg == null)
				throw new ArgumentNullException ();
			if (arg is BaseIterator)
				arg = ToString (arg);	// follow on
			if (arg is string)
				return XmlConvert.ToDouble ((string) arg);	// TODO: spec? convert string to number
			if (arg is double)
				return (double) arg;
			if (arg is bool)
				return Convert.ToDouble ((bool) arg);
			throw new ArgumentException ();
		}
	}

	internal abstract class XPathFunction : IXsltContextFunction
	{
		public abstract XPathResultType ReturnType { get; }
		public abstract int Minargs { get; }
		public abstract int Maxargs { get; }
		public abstract XPathResultType [] ArgTypes { get; }
		public object Invoke (XsltContext xsltContext, object[] args, XPathNavigator docContext)
		{
			return TypesafeInvoke (xsltContext, args, docContext);
		}

		public abstract string Name { get; }
		public abstract object TypesafeInvoke (XsltContext xsltContext, object[] args, XPathNavigator docContext);
	}


	internal class XPathFunctionLast : XPathFunction
	{
		public override XPathResultType ReturnType { get { return XPathResultType.Number; }}
		public override int Minargs { get { return 0; }}
		public override int Maxargs { get { return 0; }}
		public override XPathResultType [] ArgTypes { get { return null; }}
		public override object TypesafeInvoke (XsltContext xsltContext, object[] args, XPathNavigator docContext)
		{
			throw new NotImplementedException ();	// special-cased
		}
		public override string Name { get { return "last"; }}
	}


	internal class XPathFunctionPosition : XPathFunction
	{
		public override XPathResultType ReturnType { get { return XPathResultType.Number; }}
		public override int Minargs { get { return 0; }}
		public override int Maxargs { get { return 0; }}
		public override XPathResultType [] ArgTypes { get { return null; }}
		public override object TypesafeInvoke (XsltContext xsltContext, object[] args, XPathNavigator docContext)
		{
			throw new NotImplementedException ();	// special-cased
		}
		public override string Name { get { return "position"; }}
	}


	internal class XPathFunctionCount : XPathFunction
	{
		public override XPathResultType ReturnType { get { return XPathResultType.Number; }}
		public override int Minargs { get { return 1; }}
		public override int Maxargs { get { return 1; }}
		public override XPathResultType [] ArgTypes { get { return new XPathResultType [] { XPathResultType.NodeSet }; }}
		public override object TypesafeInvoke (XsltContext xsltContext, object[] args, XPathNavigator docContext)
		{
			return ((BaseIterator) args [0]).Count;
		}
		public override string Name { get { return "count"; }}
	}


	internal class XPathFunctionId : XPathFunction
	{
		private static char [] rgchWhitespace = {' ', '\t', '\r', '\n'};
		public override XPathResultType ReturnType { get { return XPathResultType.NodeSet; }}
		public override int Minargs { get { return 1; }}
		public override int Maxargs { get { return 1; }}
		public override XPathResultType [] ArgTypes { get { return new XPathResultType [] { XPathResultType.Any }; }}
		[MonoTODO]
		public override object TypesafeInvoke (XsltContext xsltContext, object[] args, XPathNavigator docContext)
		{
			String strArgs;
			BaseIterator iter = args [0] as BaseIterator;
			if (iter != null)
			{
				strArgs = "";
				while (!iter.MoveNext ())
					strArgs += iter.Current.Value + " ";
			}
			else
				strArgs = XPathFunctions.ToString (args [0]);
			string [] rgstrArgs = strArgs.Split (rgchWhitespace);
			ArrayList rgNodes = new ArrayList ();
			foreach (string strArg in rgstrArgs)
			{
				if (docContext.MoveToId (strArg))
					rgNodes.Add (docContext.Clone ());
			}
			return new ArrayListIterator (iter, rgNodes);
		}
		public override string Name { get { return "id"; }}
	}


	internal class XPathFunctionLocalName : XPathFunction
	{
		public override XPathResultType ReturnType { get { return XPathResultType.NodeSet; }}
		public override int Minargs { get { return 0; }}
		public override int Maxargs { get { return 1; }}
		public override XPathResultType [] ArgTypes { get { return new XPathResultType [] { XPathResultType.NodeSet }; }}
		public override object TypesafeInvoke (XsltContext xsltContext, object[] args, XPathNavigator docContext)
		{
			BaseIterator iter = (args.Length == 1) ? ((BaseIterator) args [0]) : new SelfIterator (docContext, xsltContext);
			if (iter == null || !iter.MoveNext ())
				return "";
			return iter.Current.LocalName;
		}
		public override string Name { get { return "local-name"; }}
	}


	internal class XPathFunctionNamespaceUri : XPathFunction
	{
		public override XPathResultType ReturnType { get { return XPathResultType.String; }}
		public override int Minargs { get { return 0; }}
		public override int Maxargs { get { return 1; }}
		public override XPathResultType [] ArgTypes { get { return new XPathResultType [] { XPathResultType.NodeSet }; }}
		[MonoTODO]
		public override object TypesafeInvoke (XsltContext xsltContext, object[] args, XPathNavigator docContext)
		{
			BaseIterator iter = (args.Length == 1) ? ((BaseIterator) args [0]) : new SelfIterator (docContext, xsltContext);
			if (iter == null || !iter.MoveNext ())
				return "";
			return iter.Current.NamespaceURI;	// TODO: should the namespace be expanded wrt. the given context?
		}
		public override string Name { get { return "namespace-uri"; }}
	}


	internal class XPathFunctionName : XPathFunction
	{
		public override XPathResultType ReturnType { get { return XPathResultType.String; }}
		public override int Minargs { get { return 0; }}
		public override int Maxargs { get { return 1; }}
		public override XPathResultType [] ArgTypes { get { return new XPathResultType [] { XPathResultType.NodeSet }; }}
		[MonoTODO]
		public override object TypesafeInvoke (XsltContext xsltContext, object[] args, XPathNavigator docContext)
		{
			BaseIterator iter = (args.Length == 1) ? ((BaseIterator) args [0]) : new SelfIterator (docContext, xsltContext);
			if (iter == null || !iter.MoveNext ())
				return "";
			return iter.Current.Name;
		}
		public override string Name { get { return "name"; }}
	}


	internal class XPathFunctionString : XPathFunction
	{
		public override XPathResultType ReturnType { get { return XPathResultType.String; }}
		public override int Minargs { get { return 0; }}
		public override int Maxargs { get { return 1; }}
		public override XPathResultType [] ArgTypes { get { return new XPathResultType [] { XPathResultType.Any }; }}
		public override object TypesafeInvoke (XsltContext xsltContext, object[] args, XPathNavigator docContext)
		{
			return XPathFunctions.ToString (args [0]);
		}
		public override string Name { get { return "string"; }}
	}


	internal class XPathFunctionConcat : XPathFunction
	{
		public override XPathResultType ReturnType { get { return XPathResultType.String; }}
		public override int Minargs { get { return 2; }}
		public override int Maxargs { get { return int.MaxValue; }}
		public override XPathResultType [] ArgTypes { get { return new XPathResultType [] { XPathResultType.String, XPathResultType.String, XPathResultType.String }; }}
		public override object TypesafeInvoke (XsltContext xsltContext, object[] args, XPathNavigator docContext)
		{
			String str = "";
			foreach (string strArg in args)
				str += strArg;
			return str;
		}
		public override string Name { get { return "concat"; }}
	}


	internal class XPathFunctionStartsWith : XPathFunction
	{
		public override XPathResultType ReturnType { get { return XPathResultType.Boolean; }}
		public override int Minargs { get { return 2; }}
		public override int Maxargs { get { return 2; }}
		public override XPathResultType [] ArgTypes { get { return new XPathResultType [] { XPathResultType.String, XPathResultType.String }; }}
		public override object TypesafeInvoke (XsltContext xsltContext, object[] args, XPathNavigator docContext)
		{
			string str1 = (string) args [0];
			string str2 = (string) args [1];
			return str1.StartsWith (str2);
		}
		public override string Name { get { return "starts-with"; }}
	}


	internal class XPathFunctionContains : XPathFunction
	{
		public override XPathResultType ReturnType { get { return XPathResultType.Boolean; }}
		public override int Minargs { get { return 2; }}
		public override int Maxargs { get { return 2; }}
		public override XPathResultType [] ArgTypes { get { return new XPathResultType [] { XPathResultType.String, XPathResultType.String }; }}
		public override object TypesafeInvoke (XsltContext xsltContext, object[] args, XPathNavigator docContext)
		{
			string str1 = (string) args [0];
			string str2 = (string) args [1];
			return str1.IndexOf (str2) != -1;
		}
		public override string Name { get { return "contains"; }}
	}


	internal class XPathFunctionSubstringBefore : XPathFunction
	{
		public override XPathResultType ReturnType { get { return XPathResultType.String; }}
		public override int Minargs { get { return 2; }}
		public override int Maxargs { get { return 2; }}
		public override XPathResultType [] ArgTypes { get { return new XPathResultType [] { XPathResultType.String, XPathResultType.String }; }}
		public override object TypesafeInvoke (XsltContext xsltContext, object[] args, XPathNavigator docContext)
		{
			string str1 = (string) args [0];
			string str2 = (string) args [1];
			int ich = str1.IndexOf (str2);
			if (ich <= 0)
				return "";
			return str1.Substring (0, ich);
		}
		public override string Name { get { return "substring-before"; }}
	}


	internal class XPathFunctionSubstringAfter : XPathFunction
	{
		public override XPathResultType ReturnType { get { return XPathResultType.String; }}
		public override int Minargs { get { return 2; }}
		public override int Maxargs { get { return 2; }}
		public override XPathResultType [] ArgTypes { get { return new XPathResultType [] { XPathResultType.String, XPathResultType.String }; }}
		public override object TypesafeInvoke (XsltContext xsltContext, object[] args, XPathNavigator docContext)
		{
			string str1 = (string) args [0];
			string str2 = (string) args [1];
			int ich = str1.IndexOf (str2);
			if (ich <= 0)
				return "";
			return str1.Substring (ich + str2.Length);
		}
		public override string Name { get { return "substring-after"; }}
	}


	internal class XPathFunctionSubstring : XPathFunction
	{
		public override XPathResultType ReturnType { get { return XPathResultType.String; }}
		public override int Minargs { get { return 2; }}
		public override int Maxargs { get { return 3; }}
		public override XPathResultType [] ArgTypes { get { return new XPathResultType [] { XPathResultType.String, XPathResultType.Number, XPathResultType.Number }; }}
		[MonoTODO]
		public override object TypesafeInvoke (XsltContext xsltContext, object[] args, XPathNavigator docContext)
		{
			// TODO: check this, what the hell were they smoking?
			string str = (string) args [0];
			double ich = Math.Round ((double) args [1]) - 1;
			if (Double.IsNaN (ich) || ich >= (double) str.Length)
				return "";

			if (args.Length == 2)
			{
				if (ich < 0)
					ich = 0.0;
				return str.Substring ((int) ich);
			}
			else
			{
				double cch = Math.Round ((double) args [2]);
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
		public override string Name { get { return "substring"; }}
	}


	internal class XPathFunctionStringLength : XPathFunction
	{
		public override XPathResultType ReturnType { get { return XPathResultType.Number; }}
		public override int Minargs { get { return 0; }}
		public override int Maxargs { get { return 1; }}
		public override XPathResultType [] ArgTypes { get { return new XPathResultType [] { XPathResultType.String }; }}
		public override object TypesafeInvoke (XsltContext xsltContext, object[] args, XPathNavigator docContext)
		{
			string str;
			if (args.Length == 1)
				str = (string) args [0];
			else
				str = docContext.Value;
			return (double) str.Length;
		}
		public override string Name { get { return "string-length"; }}
	}


	internal class XPathFunctionNormalizeSpace : XPathFunction
	{
		public override XPathResultType ReturnType { get { return XPathResultType.String; }}
		public override int Minargs { get { return 0; }}
		public override int Maxargs { get { return 1; }}
		public override XPathResultType [] ArgTypes { get { return new XPathResultType [] { XPathResultType.String }; }}
		[MonoTODO]
		public override object TypesafeInvoke (XsltContext xsltContext, object[] args, XPathNavigator docContext)
		{
			string str;
			if (args.Length == 1)
				str = (string) args [0];
			else
				str = docContext.Value;
			System.Text.StringBuilder sb = new System.Text.StringBuilder ();
			bool fSpace = false;
			foreach (char ch in str)
			{
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
		public override string Name { get { return "normalize-space"; }}
	}


	internal class XPathFunctionTranslate : XPathFunction
	{
		public override XPathResultType ReturnType { get { return XPathResultType.String; }}
		public override int Minargs { get { return 3; }}
		public override int Maxargs { get { return 3; }}
		public override XPathResultType [] ArgTypes { get { return new XPathResultType [] { XPathResultType.String, XPathResultType.String, XPathResultType.String }; }}
		[MonoTODO]
		public override object TypesafeInvoke (XsltContext xsltContext, object[] args, XPathNavigator docContext)
		{
			throw new NotImplementedException ();
		}
		public override string Name { get { return "translate"; }}
	}


	internal class XPathFunctionBoolean : XPathFunction
	{
		public override XPathResultType ReturnType { get { return XPathResultType.Boolean; }}
		public override int Minargs { get { return 1; }}
		public override int Maxargs { get { return 1; }}
		public override XPathResultType [] ArgTypes { get { return new XPathResultType [] { XPathResultType.Any }; }}
		public override object TypesafeInvoke (XsltContext xsltContext, object[] args, XPathNavigator docContext)
		{
			return XPathFunctions.ToBoolean (args [0]);
		}
		public override string Name { get { return "boolean"; }}
	}


	internal class XPathFunctionNot : XPathFunction
	{
		public override XPathResultType ReturnType { get { return XPathResultType.Boolean; }}
		public override int Minargs { get { return 1; }}
		public override int Maxargs { get { return 1; }}
		public override XPathResultType [] ArgTypes { get { return new XPathResultType [] { XPathResultType.Any }; }}
		public override object TypesafeInvoke (XsltContext xsltContext, object[] args, XPathNavigator docContext)
		{
			return !(XPathFunctions.ToBoolean (args [0]));
		}
		public override string Name { get { return "not"; }}
	}


	internal class XPathFunctionTrue : XPathFunction
	{
		public override XPathResultType ReturnType { get { return XPathResultType.Boolean; }}
		public override int Minargs { get { return 0; }}
		public override int Maxargs { get { return 0; }}
		public override XPathResultType [] ArgTypes { get { return null; }}
		public override object TypesafeInvoke (XsltContext xsltContext, object[] args, XPathNavigator docContext)
		{
			return true;
		}
		public override string Name { get { return "true"; }}
	}


	internal class XPathFunctionFalse : XPathFunction
	{
		public override XPathResultType ReturnType { get { return XPathResultType.Boolean; }}
		public override int Minargs { get { return 0; }}
		public override int Maxargs { get { return 0; }}
		public override XPathResultType [] ArgTypes { get { return null; }}
		public override object TypesafeInvoke (XsltContext xsltContext, object[] args, XPathNavigator docContext)
		{
			return false;
		}
		public override string Name { get { return "false"; }}
	}


	internal class XPathFunctionLang : XPathFunction
	{
		public override XPathResultType ReturnType { get { return XPathResultType.Boolean; }}
		public override int Minargs { get { return 1; }}
		public override int Maxargs { get { return 1; }}
		public override XPathResultType [] ArgTypes { get { return new XPathResultType [] { XPathResultType.String }; }}
		public override object TypesafeInvoke (XsltContext xsltContext, object[] args, XPathNavigator docContext)
		{
			string lang = ((string)args[0]).ToLower ();
			string actualLang = docContext.XmlLang.ToLower ();
			
			return lang == actualLang || lang == (actualLang.Split ('-')[0]);
		}
		public override string Name { get { return "lang"; }}
	}


	internal class XPathFunctionNumber : XPathFunction
	{
		public override XPathResultType ReturnType { get { return XPathResultType.Number; }}
		public override int Minargs { get { return 0; }}
		public override int Maxargs { get { return 1; }}
		public override XPathResultType [] ArgTypes { get { return new XPathResultType [] { XPathResultType.Any }; }}
		public override object TypesafeInvoke (XsltContext xsltContext, object[] args, XPathNavigator docContext)
		{
			return XPathFunctions.ToNumber (args [0]);
		}
		public override string Name { get { return "number"; }}
	}


	internal class XPathFunctionSum : XPathFunction
	{
		public override XPathResultType ReturnType { get { return XPathResultType.Number; }}
		public override int Minargs { get { return 1; }}
		public override int Maxargs { get { return 1; }}
		public override XPathResultType [] ArgTypes { get { return new XPathResultType [] { XPathResultType.NodeSet }; }}
		[MonoTODO]
		public override object TypesafeInvoke (XsltContext xsltContext, object[] args, XPathNavigator docContext)
		{
			throw new NotImplementedException ();
		}
		public override string Name { get { return "sum"; }}
	}


	internal class XPathFunctionFloor : XPathFunction
	{
		public override XPathResultType ReturnType { get { return XPathResultType.Number; }}
		public override int Minargs { get { return 1; }}
		public override int Maxargs { get { return 1; }}
		public override XPathResultType [] ArgTypes { get { return new XPathResultType [] { XPathResultType.Number }; }}
		public override object TypesafeInvoke (XsltContext xsltContext, object[] args, XPathNavigator docContext)
		{
			return Math.Floor ((double) args [0]);
		}
		public override string Name { get { return "floor"; }}
	}


	internal class XPathFunctionCeil : XPathFunction
	{
		public override XPathResultType ReturnType { get { return XPathResultType.Number; }}
		public override int Minargs { get { return 1; }}
		public override int Maxargs { get { return 1; }}
		public override XPathResultType [] ArgTypes { get { return new XPathResultType [] { XPathResultType.Number }; }}
		public override object TypesafeInvoke (XsltContext xsltContext, object[] args, XPathNavigator docContext)
		{
			return Math.Ceiling ((double) args [0]);
		}
		public override string Name { get { return "ceil"; }}
	}


	internal class XPathFunctionRound : XPathFunction
	{
		public override XPathResultType ReturnType { get { return XPathResultType.Number; }}
		public override int Minargs { get { return 1; }}
		public override int Maxargs { get { return 1; }}
		public override XPathResultType [] ArgTypes { get { return new XPathResultType [] { XPathResultType.Number }; }}
		public override object TypesafeInvoke (XsltContext xsltContext, object[] args, XPathNavigator docContext)
		{
			double arg = (double) args [0];
			if (arg < -0.5 || arg > 0)
				return Math.Floor (arg + 0.5);
			return Math.Round (arg);
		}
		public override string Name { get { return "round"; }}
	}
}
