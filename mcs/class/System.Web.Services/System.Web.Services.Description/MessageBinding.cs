// 
// System.Web.Services.Description.MessageBinding.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System.Xml.Serialization;

namespace System.Web.Services.Description {
	public abstract class MessageBinding : DocumentableItem {

		#region Fields

		string name;
		OperationBinding operationBinding;

		#endregion // Fields

		#region Constructors
		
		protected MessageBinding ()
		{
			name = String.Empty;
			operationBinding = new OperationBinding ();
		}
		
		#endregion // Constructors

		#region Properties

		[XmlIgnore]
		public abstract ServiceDescriptionFormatExtensionCollection Extensions { 	
			get;
		}

		[XmlAttribute ("name", DataType = "NMTOKEN")]	
		public string Name {
			get { return name; }
			set { name = value; }
		}
	
		[XmlIgnore]
		public OperationBinding OperationBinding {
			get { return operationBinding; }
		}

		#endregion // Properties
	}
}
