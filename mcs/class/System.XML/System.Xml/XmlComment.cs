//
// System.Xml.XmlComment
//
// Author:
//   Kral Ferch <kral_ferch@hotmail.com>
//
// (C) 2002 Kral Ferch
//

using System;
using System.Xml.XPath;

namespace System.Xml
{
	public class XmlComment : XmlCharacterData
	{
		#region Constructors

		protected internal XmlComment (string comment, XmlDocument doc)
			: base (comment, doc)
		{
		}

		#endregion

		#region Properties

		public override string LocalName {
			get { return "#comment"; }
		}

		public override string Name {
			get { return "#comment"; }
		}

		public override XmlNodeType NodeType {
			get { return XmlNodeType.Comment; }
		}
		
		internal protected override XPathNodeType XPathNodeType {
			get {
				return XPathNodeType.Comment;
			}
		}

		#endregion

		#region Methods

		public override XmlNode CloneNode (bool deep)
		{
			// discard deep because Comments have no children.
			return new XmlComment(Value, OwnerDocument); 
		}

		public override void WriteContentTo (XmlWriter w) { }

		public override void WriteTo (XmlWriter w)
		{
			w.WriteComment (Data);
		}

		#endregion
	}
}
