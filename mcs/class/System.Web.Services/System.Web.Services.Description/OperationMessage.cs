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

namespace System.Web.Services.Description {
	public abstract class OperationMessage : DocumentableItem {

		#region Fields

		XmlQualifiedName message;
		string name;
		Operation operation;

		#endregion // Fields

		#region Constructors
		
		public OperationMessage ()
		{
			message = null;
			name = String.Empty;
			operation = null;
		}
		
		#endregion // Constructors

		#region Properties

		public XmlQualifiedName Message {
			get { return message; }
			set { message = value; }
		}

		public string Name {
			get { return name; }
			set { name = value; }
		}

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
