//
// System.Net.GlobalProxySelection
//
// Author:
//   Lawrence Pit (loz@cable.a2000.nl)
//

using System;
using System.Collections;
using System.Configuration;
using System.IO;
using System.Runtime.Serialization;

namespace System.Net 
{
	public class GlobalProxySelection
	{
		private static IWebProxy proxy;
		
		// Constructors
		public GlobalProxySelection() { }
		
		// Properties
		
		static IWebProxy GetProxy ()
		{
			if (proxy != null)
				return proxy;

			lock (typeof (GlobalProxySelection)) {
				if (proxy != null)
					return proxy;

				object p = ConfigurationSettings.GetConfig ("system.net/defaultProxy");
				if (p == null)
					p = new EmptyWebProxy ();

				proxy = (IWebProxy) p;
			}

			return proxy;
		}
		
		public static IWebProxy Select {
			get { return GetProxy (); }
			set {
				if (value == null)
					throw new ArgumentNullException ("GlobalProxySelection.Select",
							"null IWebProxy not allowed. Use GetEmptyWebProxy ()");

				lock (typeof (GlobalProxySelection))
					proxy = value; 
			}
		}
		
		// Methods
		
		public static IWebProxy GetEmptyWebProxy()
		{
			// must return a new one each time, as the credentials
			// can be set
			return new EmptyWebProxy ();	
		}
		
		// Internal Classes
		
		internal class EmptyWebProxy : IWebProxy {
			private ICredentials credentials = null;
			
			internal EmptyWebProxy () { }
			
			public ICredentials Credentials {
				get { return credentials; } 
				set { credentials = value; }
			}

			public Uri GetProxy (Uri destination)
			{
				return destination;
			}

			public bool IsBypassed (Uri host)
			{
				return true; // pass directly to host
			}
		}
	}		
}
