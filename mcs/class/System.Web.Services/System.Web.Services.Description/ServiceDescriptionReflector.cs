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
		ServiceDescriptionCollection serviceDescriptions;
		Types types;

		#region Constructors
	
		public ServiceDescriptionReflector ()
		{
			reflector = new SoapProtocolReflector ();
			types = new Types ();
			serviceDescriptions = new ServiceDescriptionCollection ();
		}
		
		#endregion // Constructors

		#region Properties

		public XmlSchemas Schemas {
			get { return types.Schemas; }
		}

		public ServiceDescriptionCollection ServiceDescriptions {
			get { return serviceDescriptions; }
		}


		#endregion // Properties

		#region Methods

		public void Reflect (Type type, string url)
		{
			reflector.Reflect (this, type, url);
			
			if (serviceDescriptions.Count == 1)
				serviceDescriptions[0].Types = types;
			else
			{
				foreach (ServiceDescription d in serviceDescriptions)
				{
					d.Types = new Types();
					for (int n=0; n<types.Schemas.Count; n++)
						ProtocolReflector.AddImport (d, types.Schemas[n].TargetNamespace, GetSchemaUrl (url, n));
				}
			}
		}
		
		string GetSchemaUrl (string baseUrl, int id)
		{
			return baseUrl + "?schema=" + id;
		}
		
		
		#endregion
	}
}
