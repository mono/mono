//
// System.Xml.XmlText
//
// Author:
//   Jason Diamond <jason@injektilo.org>
//
// (C) 2002 Jason Diamond  http://injektilo.org/
//

using System;

namespace System.Xml
{
	public class XmlText : XmlCharacterData
	{
		#region Constructor

		protected internal XmlText (string strData, XmlDocument doc) : base(strData, doc)
		{
		}

		#endregion

		#region Properties

		public override string LocalName 
		{
			get { return "#text"; }
		}

		public override string Name {
			get { return "#text"; }
		}

		public override XmlNodeType NodeType {
			get { return XmlNodeType.Text; }
		}

		#endregion

		#region Methods

		[MonoTODO]
		public override XmlNode CloneNode (bool deep)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual XmlText SplitText (int offset)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override void WriteContentTo (XmlWriter w)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override void WriteTo (XmlWriter w)
		{
			throw new NotImplementedException ();
		}

		#endregion
	}
}
