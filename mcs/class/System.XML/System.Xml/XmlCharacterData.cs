//
// System.Xml.XmlCharacterData.cs
//
// Authors:
//   Jason Diamond <jason@injektilo.org>
//   Kral Ferch <kral_ferch@hotmail.com>
//
// (C) 2002 Jason Diamond, Kral Ferch
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
	public abstract class XmlCharacterData : XmlLinkedNode
	{
		private string data;

		#region Constructor

		protected internal XmlCharacterData (string data, XmlDocument doc)
			: base (doc)
		{
			if (data == null)
				data = String.Empty;

			this.data = data;
		}

		#endregion
		
		#region Properties

		public virtual string Data {
			get { return data; }
			
			set {
				string old = data;
				OwnerDocument.onNodeChanging (this, this.ParentNode, old, value);

				data = value;

				OwnerDocument.onNodeChanged (this, this.ParentNode, old, value);
			}
		}

		public override string InnerText {
			get { return data; }

			set { Data = value; }	// invokes events
		}

		public virtual int Length {
			get { return data != null ? data.Length : 0; }
		}

		public override string Value {
			get { return data; }

			set {
				Data = value;
			}
		}

		internal override XPathNodeType XPathNodeType {
			get { return XPathNodeType.Text; }
		}

		#endregion

		#region Methods

		public virtual void AppendData (string strData)
		{
			string oldData = data;
			string newData = data += strData;
			OwnerDocument.onNodeChanging (this, this.ParentNode, oldData, newData);
			data = newData;
			OwnerDocument.onNodeChanged (this, this.ParentNode, oldData, newData);
		}

		public virtual void DeleteData (int offset, int count)
		{
			if (offset < 0)
				throw new ArgumentOutOfRangeException ("offset", "Must be non-negative and must not be greater than the length of this instance.");

			int newCount = data.Length - offset;

			if ((offset + count) < data.Length)
				newCount = count;

			string oldValue = data;
			string newValue = data.Remove (offset, newCount);

			OwnerDocument.onNodeChanging (this, this.ParentNode, oldValue, newValue);

			data = newValue;
			
			OwnerDocument.onNodeChanged (this, this.ParentNode, oldValue, newValue);
		}

		public virtual void InsertData (int offset, string strData)
		{
			if ((offset < 0) || (offset > data.Length))
				throw new ArgumentOutOfRangeException ("offset", "Must be non-negative and must not be greater than the length of this instance.");

			string oldData = data;
			string newData = data.Insert(offset, strData);
			
			OwnerDocument.onNodeChanging (this, this.ParentNode, oldData, newData);

			data = newData;

			OwnerDocument.onNodeChanged (this, this.ParentNode, oldData, newData);
		}

		public virtual void ReplaceData (int offset, int count, string strData)
		{
			if ((offset < 0) || (offset > data.Length))
				throw new ArgumentOutOfRangeException ("offset", "Must be non-negative and must not be greater than the length of this instance.");

			if (count < 0)
				throw new ArgumentOutOfRangeException ("count", "Must be non-negative.");

			if (strData == null)
				throw new ArgumentNullException ("strData", "Must be non-null.");

			string oldData = data;
			string newData = data.Substring (0, offset) + strData;
			
			if ((offset + count) < data.Length)
				newData += data.Substring (offset + count);

			OwnerDocument.onNodeChanging (this, this.ParentNode, oldData, newData);

			data = newData;

			OwnerDocument.onNodeChanged (this, this.ParentNode, oldData, newData);
		}

		public virtual string Substring (int offset, int count)
		{
			if (data.Length < offset + count)
				return data.Substring (offset);
			else
				return data.Substring (offset, count);
		}
		#endregion
	}
}
