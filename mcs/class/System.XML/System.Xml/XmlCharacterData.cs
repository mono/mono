//
// System.Xml.XmlCharacterData.cs
//
// Author:
//   Jason Diamond <jason@injektilo.org>
//
// (C) 2002 Jason Diamond  http://injektilo.org/
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
			this.data = data;
		}

		#endregion
		
		#region Properties

		public virtual string Data {
			get { return data; }
			
			set { data = value; }
		}

		[MonoTODO]
		public override string InnerText {
			get {
				throw new NotImplementedException ();
			}

			set {
				throw new NotImplementedException ();
			}
		}

		public int Length {
			get {
				return data != null ? data.Length : 0;
			}
		}

		public override string Value {
			get { return data; }

			set { data = value; }
		}

		#endregion

		#region Methods

		[MonoTODO]
		public virtual void AppendData (string strData)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void DeleteData (int offset, int count)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void InsertData (int offset, string strData)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void ReplaceData (int offset, int count, string strData)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		public virtual string Substring(int offset, int count)
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}
