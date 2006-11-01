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

namespace Mono.Xml.Xsl {
	internal class Debug {
		[System.Diagnostics.Conditional("_DEBUG")]
		internal static void TraceContext(XPathNavigator context) {
			string output = "(null)";
			
			if (context != null) {
				context = context.Clone ();
				switch (context.NodeType) {
					case XPathNodeType.Element:
						output = string.Format("<{0}:{1}", context.Prefix, context.LocalName);
						for (bool attr = context.MoveToFirstAttribute(); attr; attr = context.MoveToNextAttribute()) {
							output += string.Format(CultureInfo.InvariantCulture, " {0}:{1}={2}", context.Prefix, context.LocalName, context.Value);
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
				throw new XsltException (message, null);
		}

		[System.Diagnostics.Conditional("_DEBUG")]
		internal static void WriteLine (object value)
		{
			Console.Error.WriteLine (value);
		}

		[System.Diagnostics.Conditional("_DEBUG")]
		internal static void WriteLine (string message)
		{
			Console.Error.WriteLine (message);
		}
		
		//static Stack eleStack = new Stack ();
		
		[System.Diagnostics.Conditional("DEBUG")]
		internal static void EnterNavigator (Compiler c)
		{
			//eleStack.Push (c.Input.Clone ());
		}
		
		[System.Diagnostics.Conditional("DEBUG")]
		internal static void ExitNavigator (Compiler c)
		{
			//XPathNavigator x = (XPathNavigator)eleStack.Pop();
			//if (!x.IsSamePosition (c.Input))
			//	throw new Exception ("Position must be the same on enter/exit. Enter node: " + x.Name + " exit node " + c.Input.Name);
			
		}
	}
}