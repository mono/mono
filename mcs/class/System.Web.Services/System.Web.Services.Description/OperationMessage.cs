// 
// System.Web.Services.Description.OperationMessage.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System.Web.Services;
using System.Xml;
using System.Xml.Serialization;

namespace System.Web.Services.Description {
	public abstract class OperationMessage : DocumentableItem {

		#region Fields

		XmlQualifiedName message;
		string name;
		Operation operation;

		#endregion // Fields

		#region Constructors
		
		protected OperationMessage ()
		{
			message = null;
			name = String.Empty;
			operation = null;
		}
		
		#endregion // Constructors

		#region Properties

		[XmlAttribute ("message")]
		public XmlQualifiedName Message {
			get { return message; }
			set { message = value; }
		}

		[XmlAttribute ("name", DataType = "NMTOKEN")]
		public string Name {
			get { return name; }
			set { name = value; }
		}

		[XmlIgnore]
		public Operation Operation {
			get { return operation; }
		}

		#endregion // Properties

		#region Methods

		internal void SetParent (Operation operation)
		{
			this.operation = operation;
		}

		#endregion // Methods
	}
}
