//
// System.Xml.XmlNotation.cs
//
// Author:
//	Duncan Mak  (duncan@ximian.com)
//
// (C) Ximian, Inc.
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
using System.Xml;

namespace System.Xml
{
	public class XmlNotation : XmlNode
	{
		#region Fields
		
		string localName;
		string publicId;
		string systemId;
		string prefix;
		
		#endregion
		
		#region Constructor
		
		internal XmlNotation (string localName, string prefix, string publicId,
				      string systemId, XmlDocument doc)
			: base (doc)
		{
			this.localName = doc.NameTable.Add (localName);
			this.prefix = doc.NameTable.Add (prefix);
			this.publicId = publicId;
			this.systemId = systemId;
		}

		#endregion

		#region Properties
		
		public override string InnerXml {
			get { return String.Empty; }
			set { throw new InvalidOperationException ("This operation is not allowed."); }
		}

		public override bool IsReadOnly {
			get { return true; } // Notation nodes are always read-only
		}

		public override string LocalName {
			get { return localName; }
		}

		public override string Name {
			get { return (prefix != String.Empty) ? (prefix + ":" + localName) : localName; }
		}

		public override XmlNodeType NodeType {
			get { return XmlNodeType.Notation; }
		}

		public override string OuterXml {
			get { return String.Empty; }
		}

		public string PublicId {
			get {
				if (publicId != null)
					return publicId;
				else
					return null;
			}
		}

		public string SystemId {
			get {
				if (systemId != null)
					return systemId;
				else
					return null;
			}
		}

		#endregion

		#region Methods
		
		public override XmlNode CloneNode (bool deep)
		{
			throw new InvalidOperationException ("This operation is not allowed.");
		}

		public override void WriteContentTo (XmlWriter w) {	} // has no effect.

		public override void WriteTo (XmlWriter w) {	} // has no effect.
		       
		#endregion
	}
}
