// 
// System.Web.Services.Description.Port.cs
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
	public sealed class Port : DocumentableItem {

		#region Fields

		XmlQualifiedName binding;
		ServiceDescriptionFormatExtensionCollection extensions;
		string name;
		Service service;

		#endregion // Fields

		#region Constructors
		
		public Port ()
		{
			binding = null;
			extensions = new ServiceDescriptionFormatExtensionCollection (this);
			name = String.Empty;
			service = null;
		}
		
		#endregion // Constructors

		#region Properties

		[XmlAttribute ("binding")]
		public XmlQualifiedName Binding {
			get { return binding; }
			set { binding = value; }
		}

		[XmlIgnore]
		public ServiceDescriptionFormatExtensionCollection Extensions { 	
			get { return extensions; }
		}

		[XmlAttribute ("name", DataType = "NCName")]	
		public string Name {
			get { return name; }
			set { name = value; }
		}
	
		[XmlIgnore]
		public Service Service {
			get { return service; }
		}

		#endregion // Properties

		#region Methods

		internal void SetParent (Service service) 
		{
			this.service = service;
		}

		#endregion
	}
}
