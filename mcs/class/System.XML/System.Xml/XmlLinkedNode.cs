using System;

namespace System.Xml
{
	public abstract class XmlLinkedNode : XmlNode
	{
		#region Constructors

		protected internal XmlLinkedNode(XmlDocument doc) : base(doc) { }

		#endregion

		#region Properties

		[MonoTODO]
		public override XmlNode NextSibling
		{
			get {
				throw new NotImplementedException ();
			}
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