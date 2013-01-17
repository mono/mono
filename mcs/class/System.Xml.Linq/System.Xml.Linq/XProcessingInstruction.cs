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
using System.Xml;

namespace System.Xml.Linq
{
	public class XProcessingInstruction : XNode
	{
		string name;
		string data;

		public XProcessingInstruction (string target, string data)
		{
			if (target == null)
				throw new ArgumentNullException ("target");
			if (data == null)
				throw new ArgumentNullException ("data");
			this.name = target;
			this.data = data;
		}

		public XProcessingInstruction (XProcessingInstruction other)
		{
			if (other == null)
				throw new ArgumentNullException ("other");
			this.name = other.name;
			this.data = other.data;
		}

		public string Data {
			get { return data; }
			set {
				if (value == null)
					throw new ArgumentNullException ("value");
				this.data = value;
			}
		}

		public override XmlNodeType NodeType {
			get { return XmlNodeType.ProcessingInstruction; }
		}

		public string Target {
			get { return name; }
			set {
				if (value == null)
					throw new ArgumentNullException ("value");
				name = value;
			}
		}

		public override void WriteTo (XmlWriter writer)
		{
			writer.WriteProcessingInstruction (name, data);
		}
	}
}
