// 
// System.Web.Services.Protocols.XmlReturnReader.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System.IO;
using System.Net;
using System.Xml.Serialization;
using System.Web.Services;

namespace System.Web.Services.Protocols {
	public class XmlReturnReader : MimeReturnReader {

		XmlSerializer serializer;
		
		#region Constructors

		public XmlReturnReader () 
		{
		}
		
		#endregion // Constructors

		#region Methods

		public override object GetInitializer (LogicalMethodInfo methodInfo)
		{
			string namesp = WebServiceHelper.GetServiceNamespace (methodInfo.DeclaringType);
			return new XmlSerializer (methodInfo.ReturnType, namesp);
		}

		public override object[] GetInitializers (LogicalMethodInfo[] methodInfos)
		{
			XmlReflectionImporter importer = new XmlReflectionImporter ();
			XmlMapping[] sers = new XmlMapping [methodInfos.Length];
			for (int n=0; n<sers.Length; n++)
			{
				LogicalMethodInfo metinfo = methodInfos[n];
				if (metinfo.IsVoid) 
					sers[n] = null;
				else
				{
					string namesp = WebServiceHelper.GetServiceNamespace (metinfo.DeclaringType);
					sers[n] = importer.ImportTypeMapping (methodInfos[n].ReturnType, namesp);
				}
			}
			return XmlSerializer.FromMappings (sers);
		}
		
		public override void Initialize (object o)
		{
			serializer = (XmlSerializer)o;
		}

		public override object Read (WebResponse response, Stream responseStream)
		{
			object result = null;
			if (serializer != null)
			{
				if (response.ContentType.IndexOf ("text/xml") == -1)
					throw new InvalidOperationException ("Result was not XML");
				
				result = serializer.Deserialize (responseStream);
			}
			responseStream.Close ();
			return result;
		}

		#endregion // Methods
	}
}
