// 
// System.Web.Services.Description.Binding.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System.Web.Services.Configuration;
using System.Xml;
using System.Xml.Serialization;

namespace System.Web.Services.Description {
	[XmlFormatExtensionPoint ("Extensions")]
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

		[XmlIgnore]
		public ServiceDescriptionFormatExtensionCollection Extensions { 	
			get { return extensions; }
		}

		[XmlAttribute ("name", DataType = "NCName")]	
		public string Name {
			get { return name; }
			set { name = value; }
		}

		[XmlElement ("operation")]
		public OperationBindingCollection Operations {
			get { return operations; }
		}

		[XmlIgnore]
		public ServiceDescription ServiceDescription {
			get { return serviceDescription; }
		}

		[XmlAttribute ("type")]	
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
