// 
// System.Web.Services.Description.MessagePart.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System.Xml;
using System.Xml.Serialization;

namespace System.Web.Services.Description {
	public sealed class MessagePart : DocumentableItem {

		#region Fields

		XmlQualifiedName element;
		Message message;
		string name;
		XmlQualifiedName type;

		#endregion // Fields

		#region Constructors
		
		public MessagePart ()
		{
			element = XmlQualifiedName.Empty;
			message = null;
			name = String.Empty;
			type = XmlQualifiedName.Empty;
		}
		
		#endregion // Constructors

		#region Properties

		[XmlAttribute ("element")]
		public XmlQualifiedName Element {
			get { return element; }
			set { element = value; }
		}
		
		[XmlIgnore]
		public Message Message {
			get { return message; }
		}
	
		[XmlAttribute ("name", DataType = "NMTOKEN")]
		public string Name {
			get { return name; }
			set { name = value; }
		}

		[XmlAttribute ("type")]
		public XmlQualifiedName Type {
			get { return type; }
			set { type = value; }
		}

		#endregion // Properties

		#region Methods

		internal void SetParent (Message message)
		{
			this.message = message; 
		}

		#endregion // Methods

	}
}
