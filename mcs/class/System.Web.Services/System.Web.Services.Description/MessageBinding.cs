// 
// System.Web.Services.Description.MessageBinding.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

namespace System.Web.Services.Description {
	public abstract class MessageBinding : DocumentableItem {

		#region Fields

		string name;
		OperationBinding operationBinding;

		#endregion // Fields

		#region Constructors
		
		public MessageBinding ()
		{
			name = String.Empty;
			operationBinding = new OperationBinding ();
		}
		
		#endregion // Constructors

		#region Properties

		public abstract ServiceDescriptionFormatExtensionCollection Extensions { 	
			get;
		}
	
		public string Name {
			get { return name; }
			set { name = value; }
		}
	
		public OperationBinding OperationBinding {
			get { return operationBinding; }
		}

		#endregion // Properties
	}
}
