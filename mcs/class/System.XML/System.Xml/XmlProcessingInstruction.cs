//
// System.Xml.XmlProcessingInstruction
//
// Author:
//   Kral Ferch <kral_ferch@hotmail.com>
//
// (C) 2002 Kral Ferch
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
using System.Xml.XPath;

namespace System.Xml
{
	public class XmlProcessingInstruction : XmlLinkedNode
	{
		string target;
		string data;

		#region Constructors

		protected internal XmlProcessingInstruction (string target, string data, XmlDocument doc) : base(doc)
		{
			XmlConvert.VerifyName (target);
			if (data == null)
				data = String.Empty;

			this.target = target;
			this.data = data;
		}

		#endregion

		#region Properties

		public string Data
		{
			get { return data; }

			set { data = value; }
		}

		public override string InnerText
		{
			get { return Data; }
			set { data = value; }
		}

		public override string LocalName
		{
			get { return target; }
		}

		public override string Name
		{
			get { return target; }
		}

		public override XmlNodeType NodeType
		{
			get { return XmlNodeType.ProcessingInstruction; }
		}

		internal override XPathNodeType XPathNodeType {
			get {
				return XPathNodeType.ProcessingInstruction;
			}
		}
		
		public string Target
		{
			get { return target; }
		}

		public override string Value
		{
			get { return data; }
			set {
				if (this.IsReadOnly)
					throw new ArgumentException ("This node is read-only.");
				else
					data = value;
			}
		}

		#endregion

		#region Methods

		public override XmlNode CloneNode (bool deep)
		{
			XmlNode n = new XmlProcessingInstruction (target, data, OwnerDocument);
			return n;
		}

		public override void WriteContentTo (XmlWriter w) { }

		public override void WriteTo (XmlWriter w)
		{
			w.WriteProcessingInstruction (target, data);
		}

		#endregion
	}
}
