// 
// System.Web.Services.Description.ServiceDescriptionReflector.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System.Web.Services;
using System.Xml.Serialization;

namespace System.Web.Services.Description {
	public class ServiceDescriptionReflector {

		#region Fields

		XmlSchemas schemas;
		ServiceDescriptionCollection serviceDescriptions;

		#endregion // Fields

		#region Constructors
	
		public ServiceDescriptionReflector ()
		{
			schemas = new XmlSchemas ();
			serviceDescriptions = new ServiceDescriptionCollection ();
		}
		
		#endregion // Constructors

		#region Properties

		public XmlSchemas Schemas {
			get { return schemas; }
		}

		public ServiceDescriptionCollection ServiceDescriptions {
			get { return serviceDescriptions; }
		}

	
		#endregion // Properties

		#region Methods

		[MonoTODO]
		public void Reflect (Type type, string url)
		{
			throw new NotImplementedException ();
		}

		#endregion
	}
}
