//
// System.Runtime.Remoting.MetadataServices.ServiceType
//
// Authors:
//      Martin Willemoes Hansen (mwh@sysrq.dk)
//		Lluis Sanchez Gual (lluis@ximian.com)
//
// (C) 2003 Martin Willemoes Hansen
//

namespace System.Runtime.Remoting.MetadataServices
{
	public class ServiceType
	{
		Type _type;
		string _url;
		
		public ServiceType (Type type)
		{
			_type = type;
		}

		public ServiceType (Type type, string url)
		{
			_type = type;
			_url = url;
		}

		public Type ObjectType 
		{
			get { return _type; }
		}

		public string Url 
		{
			get { return _url; }
		}
	}
}
