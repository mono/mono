//
// System.Xml.XmlProcessingInstruction
//
// Author:
//   Kral Ferch <kral_ferch@hotmail.com>
//
// (C) 2002 Kral Ferch
//

using System;

namespace System.Xml
{
	public class XmlProcessingInstruction : XmlLinkedNode
	{
		string target;
		string data;

		#region Constructors

		protected internal XmlProcessingInstruction (string target, string data, XmlDocument doc)
			: base(doc)
		{
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

		[MonoTODO]
		public override string InnerText
		{
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
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
			return new XmlProcessingInstruction (target, data, OwnerDocument);
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
