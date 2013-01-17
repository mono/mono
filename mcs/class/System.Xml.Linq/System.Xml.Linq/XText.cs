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
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace System.Xml.Linq
{
	public class XText : XNode
	{
		string value;

		public XText (string value)
		{
			this.value = value;
		}

		public XText (XText other)
		{
			value = other.value;
		}

		public override XmlNodeType NodeType {
			get { return XmlNodeType.Text; }
		}

		public string Value {
			get { return value; }
			set {
				if (value == null)
					throw new ArgumentNullException ("value");
				this.value = value;
			}
		}

		public override void WriteTo (XmlWriter writer)
		{
			if (Value.Length > 0 && Value.All (c => c == ' ' || c == '\t' || c == '\r' || c == '\n'))
				writer.WriteWhitespace (value);
			else
				writer.WriteString (value);
		}
	}
}
