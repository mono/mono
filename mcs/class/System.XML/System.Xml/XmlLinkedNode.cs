using System;

namespace System.Xml
{
	public abstract class XmlLinkedNode : XmlNode
	{
		#region Fields
		XmlLinkedNode nextSibling;

		#endregion

		#region Constructors
		protected internal XmlLinkedNode(XmlDocument doc) : base(doc) { }

		#endregion

		#region Properties		
		public override XmlNode NextSibling
		{
			get {
				if (Object.ReferenceEquals(nextSibling, ParentNode.LastLinkedChild.NextLinkedSibling) == false) {
					return nextSibling;
				}
				else {
					return null;
				}
			}
		}

		internal XmlLinkedNode NextLinkedSibling
		{
			get { return nextSibling; }
			set { nextSibling = value; }
		}

		[MonoTODO]
		public override XmlNode PreviousSibling
		{
			get {
				throw new NotImplementedException ();
			}
		}

		#endregion
	}
}
