// 
// System.Web.Services.Description.ServiceDescriptionReflector.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//   Lluis Sanchez Gual (lluis@ximian.com)
//
// Copyright (C) Tim Coleman, 2002
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System.Web.Services;
using System.Xml.Serialization;
using System.Xml;
using System.Xml.Schema;
using System.Web.Services.Protocols;
using System.Web.Services.Configuration;

namespace System.Web.Services.Description {
	public class ServiceDescriptionReflector 
	{
		ServiceDescriptionCollection serviceDescriptions;
		Types types;

		#region Constructors
	
		public ServiceDescriptionReflector ()
		{
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
			XmlSchemaExporter schemaExporter = new XmlSchemaExporter (Schemas);
			SoapSchemaExporter soapSchemaExporter = new SoapSchemaExporter (Schemas);
			
			new SoapProtocolReflector ().Reflect (this, type, url, schemaExporter, soapSchemaExporter);
			
			if (WSConfig.IsSupported (WSProtocol.HttpGet))
				new HttpGetProtocolReflector ().Reflect (this, type, url, schemaExporter, soapSchemaExporter);
			
			if (WSConfig.IsSupported (WSProtocol.HttpPost))
				new HttpPostProtocolReflector ().Reflect (this, type, url, schemaExporter, soapSchemaExporter);
				
			int i=0;
			while (i < types.Schemas.Count) {
				if (types.Schemas[i].Items.Count == 0) types.Schemas.RemoveAt (i);
				else i++;
			}
			
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
