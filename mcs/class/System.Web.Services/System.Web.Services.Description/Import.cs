// 
// System.Web.Services.Description.Import.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System.Xml.Serialization;

namespace System.Web.Services.Description {
	public sealed class Import : DocumentableItem {

		#region Fields

		string location;
		string ns;
		ServiceDescription serviceDescription;

		#endregion // Fields

		#region Constructors
		
		public Import ()
		{
			location = String.Empty;
			ns = String.Empty;
			serviceDescription = null;
		}
		
		#endregion // Constructors

		#region Properties

		[XmlAttribute ("location")]
		public string Location {
			get { return location; }
			set { location = value; }
		}

		[XmlAttribute ("namespace")]
		public string Namespace {
			get { return ns; }
			set { ns = value; }
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

		#endregion
	}
}
