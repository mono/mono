//
// System.Xml.XmlProcessingInstruction
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
	public class XmlProcessingInstruction : XmlLinkedNode
	{
		string target;
		string data;

		#region Constructors

		protected internal XmlProcessingInstruction (string target, string data, XmlDocument doc) : base(doc)
		{
			if (data == null)
				data = String.Empty;

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

		public override string InnerText
		{
			get { return Data; }
			set { data = value; }
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

		internal override XPathNodeType XPathNodeType {
			get {
				return XPathNodeType.ProcessingInstruction;
			}
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

		public override void WriteContentTo (XmlWriter w) { }

		public override void WriteTo (XmlWriter w)
		{
			w.WriteProcessingInstruction (target, data);
		}

		#endregion
	}
}
