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
using System.Net.Configuration;

namespace System.Net 
{
	[ObsoleteAttribute("Use WebRequest.DefaultProxy instead")]
	public class GlobalProxySelection
	{
		// Constructors
		public GlobalProxySelection() { }
		
		// Properties

		public static IWebProxy Select {
			get { return WebRequest.DefaultWebProxy; }
			set { WebRequest.DefaultWebProxy = value; }
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
