// 
// System.Web.Services.Description.MessagePart.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System.Xml;

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

		public XmlQualifiedName Element {
			get { return element; }
			set { element = value; }
		}
		
		public Message Message {
			get { return message; }
		}
		
		public string Name {
			get { return name; }
			set { name = value; }
		}

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
