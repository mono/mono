//
// XslCompiledElement.cs
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
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;

using QName = System.Xml.XmlQualifiedName;

namespace Mono.Xml.Xsl.Operations
{
	internal abstract class XslCompiledElementBase : XslOperation
	{
		int lineNumber;
		int linePosition;
		XPathNavigator debugInput;

		// It does not automatically invoke Compile() in .ctor()
		public XslCompiledElementBase (Compiler c)
		{
			IXmlLineInfo li = c.Input as IXmlLineInfo;
			if (li != null) {
				lineNumber = li.LineNumber;
				linePosition = li.LinePosition;
			}
			// store input only when the debugger is enabled
			if (c.Debugger != null)
				debugInput = c.Input.Clone ();
		}

		public XPathNavigator DebugInput {
			get { return debugInput; }
		}

		public int LineNumber {
			get { return lineNumber; }
		}

		public int LinePosition {
			get { return linePosition; }
		}

		protected abstract void Compile (Compiler c);
	}

	internal abstract class XslCompiledElement : XslCompiledElementBase
	{
		public XslCompiledElement (Compiler c)
			: base (c)
		{
			this.Compile (c);
		}
	}
}
