// 
// System.Web.Services.Description.Service.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System.Xml.Serialization;

namespace System.Web.Services.Description {
	public sealed class Service : DocumentableItem {

		#region Fields

		ServiceDescriptionFormatExtensionCollection extensions;
		string name;
		PortCollection ports;
		ServiceDescription serviceDescription;

		#endregion // Fields

		#region Constructors
	
		public Service ()
		{
			extensions = new ServiceDescriptionFormatExtensionCollection (this);
			name = String.Empty;
			ports = new PortCollection (this);
			serviceDescription = null;
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

		[XmlElement ("port")]	
		public PortCollection Ports {
			get { return ports; }
		}

		[XmlIgnore]
		public ServiceDescription ServiceDescription {
			get { return serviceDescription; }
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
