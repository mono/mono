//
// System.Xml.XmlEntity.cs
//
// Author:
// 	Duncan Mak  (duncan@ximian.com)
//
// (C) Ximian, Inc.
//

namespace System.Xml
{
	public class XmlEntity : XmlNode
	{
		#region Constructors

		internal XmlEntity (string name, string NDATA, string publicId, string systemId,
				    XmlDocument doc)
			: base (doc)
		{
			this.name = name;
			this.NDATA = NDATA;
			this.publicId = publicId;
			this.systemId = systemId;
			this.baseUri = doc.BaseURI;
		}

		#endregion
		
		#region Fields

		string name;
		string NDATA;
		string publicId;
		string systemId;
		string baseUri;
		XmlLinkedNode lastChild;

		#endregion

		#region Properties

		public override string BaseURI {
			get {  return baseUri; }
		}

		[MonoTODO]
		public override string InnerText {
			get { throw new NotImplementedException (); }
			set { throw new InvalidOperationException ("This operation is not supported."); }
		}

		public override string InnerXml {
			get { return String.Empty; }
			set { throw new InvalidOperationException ("This operation is not supported."); }
		}

		public override bool IsReadOnly {
			get { return true; } // always read-only.
		}

		internal override XmlLinkedNode LastLinkedChild {
			get { return lastChild; }

			set { lastChild = value; }
		}

		public override string LocalName {
			get { return name; }
		}

		public override string Name {
			get { return name; }
		}

		public override XmlNodeType NodeType {
			get { return XmlNodeType.Entity; }
		}

		public string NotationName {
			get {
				if (NDATA == null)
					return null;
				else
					return NDATA;
			}
		}

		public override string OuterXml {
			get { return String.Empty; }
		}

		public string PublicId {
			get {
				if (publicId == null)
					return null;
				else
					return publicId;
			}
		}

		public string SystemId {
			get {
				if (publicId == null)
					return null;
				else
					return systemId;
			}
		}
		#endregion

		#region Methods

		public override XmlNode CloneNode (bool deep)
		{
			throw new InvalidOperationException ("This operation is not supported.");
		}

		public override void WriteContentTo (XmlWriter w)
		{
			// No effect.
		}

		public override void WriteTo (XmlWriter w)
		{
			// No effect.
		}

		#endregion
	}
}
