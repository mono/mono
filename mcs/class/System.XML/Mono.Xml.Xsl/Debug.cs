//
// Debug.cs
//
// Authors:
//	Ben Maurer (bmaurer@users.sourceforge.net)
//	Atsushi Enomoto (ginga@kit.hi-ho.ne.jp)
//	
// (C) 2003 Ben Maurer
// (C) 2003 Atsushi Enomoto
//

using System;
using System.Collections;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;

namespace Mono.Xml.Xsl {
	internal class Debug {
		[System.Diagnostics.Conditional("DEBUG")]
		internal static void TraceContext(XPathNavigator context) {
			string output = "(null)";
			
			if (context != null) {
				context = context.Clone ();
				switch (context.NodeType) {
					case XPathNodeType.Element:
						output = string.Format("<{0}:{1}", context.Prefix, context.LocalName);
						for (bool attr = context.MoveToFirstAttribute(); attr; attr = context.MoveToNextAttribute()) {
							output += string.Format(" {0}:{1}={2}", context.Prefix, context.LocalName, context.Value);
						}
						 output += ">";
						break;
					default:
						break;
				}
			}
	
			WriteLine(output);
		}

		[System.Diagnostics.Conditional("DEBUG")]
		internal static void Assert (bool condition, string message)
		{
			if (!condition)
				throw new Exception (message);
		}

		[System.Diagnostics.Conditional("DEBUG")]
		internal static void WriteLine (object value)
		{
			Console.Error.WriteLine (value);
		}

		[System.Diagnostics.Conditional("DEBUG")]
		internal static void WriteLine (string message)
		{
			Console.Error.WriteLine (message);
		}
		
		static Stack eleStack = new Stack ();
		
		[System.Diagnostics.Conditional("DEBUG")]
		internal static void EnterNavigator (Compiler c)
		{
			eleStack.Push (c.Input.Clone ());
		}
		
		[System.Diagnostics.Conditional("DEBUG")]
		internal static void ExitNavigator (Compiler c)
		{
			XPathNavigator x = (XPathNavigator)eleStack.Pop();
			if (!x.IsSamePosition (c.Input))
				throw new Exception ("Position must be the same on enter/exit. Enter node: " + x.Name + " exit node " + c.Input.Name);
			
		}
	}
}