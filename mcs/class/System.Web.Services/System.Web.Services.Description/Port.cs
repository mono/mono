// 
// System.Web.Services.Description.Port.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System.Xml;

namespace System.Web.Services.Description {
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

		public XmlQualifiedName Binding {
			get { return binding; }
			set { binding = value; }
		}

		public ServiceDescriptionFormatExtensionCollection Extensions { 	
			get { return extensions; }
		}
	
		public string Name {
			get { return name; }
			set { name = value; }
		}
	
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
