//
// System.Xml.XmlCharacterData.cs
//
// Authors:
//   Jason Diamond <jason@injektilo.org>
//   Kral Ferch <kral_ferch@hotmail.com>
//
// (C) 2002 Jason Diamond, Kral Ferch
//

using System;

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
				OwnerDocument.onNodeChanging (this, this.ParentNode);

				if (IsReadOnly)
					throw new ArgumentException ("Node is read-only.");

				data = value;

				OwnerDocument.onNodeChanged (this, this.ParentNode);
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

		#endregion

		#region Methods

		public virtual void AppendData (string strData)
		{
			OwnerDocument.onNodeChanging (this, this.ParentNode);
			data += strData;
			OwnerDocument.onNodeChanged (this, this.ParentNode);
		}

		public virtual void DeleteData (int offset, int count)
		{
			OwnerDocument.onNodeChanging (this, this.ParentNode);

			if (offset < 0)
				throw new ArgumentOutOfRangeException ("offset", "Must be non-negative and must not be greater than the length of this instance.");

			int newCount = data.Length - offset;

			if ((offset + count) < data.Length)
				newCount = count;

			data = data.Remove (offset, newCount);
			
			OwnerDocument.onNodeChanged (this, this.ParentNode);
		}

		public virtual void InsertData (int offset, string strData)
		{
			OwnerDocument.onNodeChanging (this, this.ParentNode);

			if ((offset < 0) || (offset > data.Length))
				throw new ArgumentOutOfRangeException ("offset", "Must be non-negative and must not be greater than the length of this instance.");

			data = data.Insert(offset, strData);
			
			OwnerDocument.onNodeChanged (this, this.ParentNode);
		}

		public virtual void ReplaceData (int offset, int count, string strData)
		{
			OwnerDocument.onNodeChanging (this, this.ParentNode);

			if ((offset < 0) || (offset > data.Length))
				throw new ArgumentOutOfRangeException ("offset", "Must be non-negative and must not be greater than the length of this instance.");

			if (strData == null)
				throw new ArgumentNullException ("strData", "Must be non-null.");

			string newData = data.Substring (0, offset) + strData;
			
			if ((offset + count) < data.Length)
				newData += data.Substring (offset + count);

			data = newData;

			OwnerDocument.onNodeChanged (this, this.ParentNode);
		}

		public virtual string Substring (int offset, int count)
		{
			return data.Substring (offset, count);
		}

		#endregion
	}
}
