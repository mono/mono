// 
// System.Web.Services.Description.ServiceDescriptionReflector.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//   Lluis Sanchez Gual (lluis@ximian.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System.Web.Services;
using System.Xml.Serialization;
using System.Xml;
using System.Xml.Schema;
using System.Web.Services.Protocols;

namespace System.Web.Services.Description {
	public class ServiceDescriptionReflector {

		ProtocolReflector reflector;
		
		#region Constructors
	
		public ServiceDescriptionReflector ()
		{
			reflector = new SoapProtocolReflector ();
		}
		
		#endregion // Constructors

		#region Properties

		public XmlSchemas Schemas {
			get { return reflector.Schemas; }
		}

		public ServiceDescriptionCollection ServiceDescriptions {
			get { return reflector.ServiceDescriptions; }
		}


		#endregion // Properties

		#region Methods

		public void Reflect (Type type, string url)
		{
			reflector.Reflect (type, url);
		}
		
		
		#endregion
	}
}
