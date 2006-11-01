//
// System.Net.GlobalProxySelection
//
// Author:
//   Lawrence Pit (loz@cable.a2000.nl)
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

using System;
using System.Collections;
using System.Configuration;
using System.IO;
using System.Runtime.Serialization;
#if NET_2_0
using System.Net.Configuration;
#endif

namespace System.Net 
{
#if NET_2_0
	[ObsoleteAttribute("Use WebRequest.DefaultProxy instead")]
#endif
	public class GlobalProxySelection
	{
		// Constructors
		public GlobalProxySelection() { }
		
		// Properties

#if !NET_2_0
		volatile static IWebProxy proxy;
		static readonly object lockobj = new object ();
		
		static IWebProxy GetProxy ()
		{
			lock (lockobj) {
				if (proxy != null)
					return proxy;

				object p = ConfigurationSettings.GetConfig ("system.net/defaultProxy");
				if (p == null)
					p = new EmptyWebProxy ();
				proxy = (IWebProxy) p;
			}

			return proxy;
		}
#endif
		
		public static IWebProxy Select {
#if NET_2_0
			get { return WebRequest.DefaultWebProxy; }
			set { WebRequest.DefaultWebProxy = value; }
#else
			get { return GetProxy (); }
			set {
				if (value == null)
					throw new ArgumentNullException ("GlobalProxySelection.Select",
							"null IWebProxy not allowed. Use GetEmptyWebProxy ()");
				proxy = value; 
			}
#endif
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
