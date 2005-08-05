//
// Mono.Xml.XPath.IdPattern
//
// Author:
//	Ben Maurer (bmaurer@users.sourceforge.net)
//
// (C) 2003 Ben Maurer
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
using System.IO;
using System.Xml;
using System.Xml.Schema;
using System.Xml.XPath;
using System.Xml.Xsl;
using Mono.Xml.Xsl;

namespace Mono.Xml.XPath {
	internal class IdPattern : LocationPathPattern {

		string [] ids;
		
		public IdPattern (string arg0)
			: base ((NodeTest) null)
		{
			ids = arg0.Split (XmlChar.WhitespaceChars);
		}

		public override XPathNodeType EvaluatedNodeType {
			get { return XPathNodeType.Element; }
		}

		public override bool Matches (XPathNavigator node, XsltContext ctx)
		{
			XPathNavigator tmp = ((XsltCompiledContext) ctx).GetNavCache (this, node);
			for (int i = 0; i < ids.Length; i++)
				if (tmp.MoveToId (ids [i]) && tmp.IsSamePosition (node))
					return true;
			return false;
		}

		public override double DefaultPriority { get { return 0.5; } }
	}
}