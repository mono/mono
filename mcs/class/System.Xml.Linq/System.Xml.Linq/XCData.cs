//
// Authors:
//   Atsushi Enomoto
//
// Copyright 2007 Novell (http://www.novell.com)
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
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace System.Xml.Linq
{
	public class XCData : XText
	{
		public XCData (string value)
			: base (value)
		{
		}

		public XCData (XCData other)
			: base (other)
		{
		}

		public override XmlNodeType NodeType {
			get { return XmlNodeType.CDATA; }
		}

		public override void WriteTo (XmlWriter writer)
		{
			int start = 0;
			StringBuilder sb = null;
			for (int i = 0; i < Value.Length - 2; i++) {
				if (Value [i] == ']' && Value [i + 1] == ']'
					&& Value [i + 2] == '>') {
					if (sb == null)
						sb = new StringBuilder ();
					sb.Append (Value, start, i - start);
					sb.Append ("]]&gt;");
					start = i + 3;
				}
			}
			if (start != 0 && start != Value.Length)
				sb.Append (Value, start, Value.Length - start);
			writer.WriteCData (sb == null ? Value : sb.ToString ());
		}
	}
}
