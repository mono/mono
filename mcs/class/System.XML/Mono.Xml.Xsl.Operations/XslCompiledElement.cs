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

namespace Mono.Xml.Xsl.Operations {

	internal abstract class XslCompiledElement : XslOperation {
		int lineNumber;
		int linePosition;
		XPathNodeType parentType;
		
		public XslCompiledElement (Compiler c) : this (c, XPathNodeType.Root)
		{
		}

		public XslCompiledElement (Compiler c, XPathNodeType parentType)
		{
			this.parentType = parentType;
			IXmlLineInfo li = c.Input as IXmlLineInfo;
			if (li != null) {
				lineNumber = li.LineNumber;
				linePosition = li.LinePosition;
			}
			this.Compile (c);
		}
		
		protected abstract void Compile (Compiler c);

		public int LineNumber {
			get { return lineNumber; }
		}

		public int LinePosition {
			get { return linePosition; }
		}

		public XPathNodeType ParentType {
			get { return parentType; }
		}
	}
}
