//
// System.Net.GlobalProxySelection
//
// Author:
//   Lawrence Pit (loz@cable.a2000.nl)
//

using System;
using System.Collections;
using System.IO;
using System.Runtime.Serialization;

namespace System.Net 
{
	public class GlobalProxySelection
	{
		private static IWebProxy proxy;
		
		// Static Initializer
		
		static GlobalProxySelection ()
		{
			proxy = GetEmptyWebProxy ();
			
			// TODO: create proxy object based on information from
			//       the global or application configuration file.
		}
		
		// Constructors
		
		public GlobalProxySelection() { }
		
		// Properties
		
		public static IWebProxy Select {
			get { return proxy; }
			set { 
				proxy = (value == null) ? GetEmptyWebProxy () : value; 
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