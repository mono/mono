// 
// System.Web.Services.Description.Binding.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System.Web.Services;
using System.Xml;

namespace System.Web.Services.Description {
	public sealed class Binding : DocumentableItem {

		#region Fields

		ServiceDescriptionFormatExtensionCollection extensions;
		string name;
		OperationBindingCollection operations;
		ServiceDescription serviceDescription;
		XmlQualifiedName type;

		#endregion // Fields

		#region Constructors

		public Binding ()
		{
			extensions = new ServiceDescriptionFormatExtensionCollection (this);
			name = String.Empty;
			operations = new OperationBindingCollection (this);
			serviceDescription = null;
			type = XmlQualifiedName.Empty;
		}
		
		#endregion // Constructors

		#region Properties

		public ServiceDescriptionFormatExtensionCollection Extensions { 	
			get { return extensions; }
		}
	
		public string Name {
			get { return name; }
			set { name = value; }
		}
	
		public OperationBindingCollection Operations {
			get { return operations; }
		}

		public ServiceDescription ServiceDescription {
			get { return serviceDescription; }
		}

		public XmlQualifiedName Type {
			get { return type; }
			set { type = value; }
		}

		#endregion // Properties

		#region Methods

		internal void SetParent (ServiceDescription serviceDescription)
		{
			this.serviceDescription = serviceDescription;
		}

		#endregion // Methods
	}
}
