//
// System.Net.WebProxy.cs
//
// Authors:
//   Lawrence Pit (loz@cable.a2000.nl)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
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
using System.Globalization;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

namespace System.Net 
{
	[Serializable]
	public class WebProxy : IWebProxy, ISerializable
	{
		Uri address;
		bool bypassOnLocal;
		ArrayList bypassList;
		ICredentials credentials;
#if NET_2_0
		bool useDefaultCredentials;
#endif

		// Constructors

		public WebProxy ()
			: this ((Uri) null, false, null, null) {}

		public WebProxy (string address)
			: this (ToUri (address), false, null, null) {}

		public WebProxy (Uri address) 
			: this (address, false, null, null) {}

		public WebProxy (string address, bool bypassOnLocal) 
			: this (ToUri (address), bypassOnLocal, null, null) {}

		public WebProxy (string host, int port)
			: this (new Uri ("http://" + host + ":" + port)) {}

		public WebProxy (Uri address, bool bypassOnLocal)
			: this (address, bypassOnLocal, null, null) {}

		public WebProxy (string address, bool bypassOnLocal, string [] bypassList)
			: this (ToUri (address), bypassOnLocal, bypassList, null) {}

		public WebProxy (Uri address, bool bypassOnLocal, string [] bypassList)
			: this (address, bypassOnLocal, bypassList, null) {}

		public WebProxy (string address, bool bypassOnLocal, string [] bypassList,
				ICredentials credentials)
			: this (ToUri (address), bypassOnLocal, bypassList, credentials) {}

		public WebProxy (Uri address, bool bypassOnLocal, 
				 string[] bypassList, ICredentials credentials)
		{
			this.address = address;
			this.bypassOnLocal = bypassOnLocal;
			if (bypassList != null)
				this.bypassList = new ArrayList (bypassList);
			this.credentials = credentials;
			CheckBypassList ();
		}

		protected WebProxy (SerializationInfo serializationInfo, StreamingContext streamingContext) 
		{
			this.address = (Uri) serializationInfo.GetValue ("_ProxyAddress", typeof (Uri));
			this.bypassOnLocal = serializationInfo.GetBoolean ("_BypassOnLocal");
			this.bypassList = (ArrayList) serializationInfo.GetValue ("_BypassList", typeof (ArrayList));
#if NET_2_0
			this.useDefaultCredentials =  serializationInfo.GetBoolean ("_UseDefaultCredentials");
#endif
			this.credentials = null;
			CheckBypassList ();
		}

		// Properties
		public Uri Address {
			get { return address; }
			set { address = value; }
		}

		public ArrayList BypassArrayList {
			get {
				if (bypassList == null)
					bypassList = new ArrayList ();
				return bypassList;
			}
		}

		public string [] BypassList {
			get { return (string []) BypassArrayList.ToArray (typeof (string)); }
			set { 
				if (value == null)
					throw new ArgumentNullException ();
				bypassList = new ArrayList (value); 
				CheckBypassList ();
			}
		}

		public bool BypassProxyOnLocal {
			get { return bypassOnLocal; }
			set { bypassOnLocal = value; }
		}

		public ICredentials Credentials {
			get { return credentials; }
			set { credentials = value; }
		}

#if NET_2_0
		[MonoTODO ("Does not affect Credentials, since CredentialCache.DefaultCredentials is not implemented.")]
		public bool UseDefaultCredentials {
			get { return useDefaultCredentials; }
			set { useDefaultCredentials = value; }
		}
#endif

		// Methods
#if NET_2_0
		[Obsolete ("This method has been deprecated", false)]
#endif
		[MonoTODO("Can we get this info under windows from the system?")]
		public static WebProxy GetDefaultProxy ()
		{
			// Select gets a WebProxy from config files, if available.
			IWebProxy p = GlobalProxySelection.Select;
			if (p is WebProxy)
				return (WebProxy) p;

			return new WebProxy ();
		}

		public Uri GetProxy (Uri destination)
		{
			if (IsBypassed (destination))
				return destination;

			return address;
		}

		public bool IsBypassed (Uri host)
		{
#if NET_2_0
			if (host == null)
				throw new ArgumentNullException ("host");
#endif

			if (host.IsLoopback && bypassOnLocal)
				return true;

			if (address == null)
				return true;

			string server = host.Host;
			if (bypassOnLocal && server.IndexOf ('.') == -1)
				return true;

			// LAMESPEC
			if (!bypassOnLocal) {
				if (String.Compare (server, "localhost", true, CultureInfo.InvariantCulture) == 0)
					return true;
				if (String.Compare (server, "loopback", true, CultureInfo.InvariantCulture) == 0)
					return true;

				IPAddress addr = null;
				if (IPAddress.TryParse (server, out addr) && IPAddress.IsLoopback (addr))
					return true;
			}

			if (bypassList == null || bypassList.Count == 0)
				return false;

			try {
				string hostStr = host.Scheme + "://" + host.Authority;
				int i = 0;
				for (; i < bypassList.Count; i++) {
					Regex regex = new Regex ((string) bypassList [i], 
						// TODO: RegexOptions.Compiled |  // not implemented yet by Regex
						RegexOptions.IgnoreCase |
						RegexOptions.Singleline);

					if (regex.IsMatch (hostStr))
						break;
				}

				if (i == bypassList.Count)
					return false;

				// continue checking correctness of regular expressions..
				// will throw expression when an invalid one is found
				for (; i < bypassList.Count; i++)
					new Regex ((string) bypassList [i]);

				return true;
			} catch (ArgumentException) {
				return false;
			}
		}

#if NET_2_0
		protected virtual 
#endif
		void GetObjectData (SerializationInfo serializationInfo, StreamingContext streamingContext)
		{
			serializationInfo.AddValue ("_BypassOnLocal", bypassOnLocal);
			serializationInfo.AddValue ("_ProxyAddress", address);
			serializationInfo.AddValue ("_BypassList", bypassList);
#if NET_2_0
			serializationInfo.AddValue ("_UseDefaultCredentials", UseDefaultCredentials);
#endif
		}

		void ISerializable.GetObjectData (SerializationInfo serializationInfo,
		                                  StreamingContext streamingContext)
		{
			GetObjectData (serializationInfo, streamingContext);
		}

		// Private Methods
		// this compiles the regular expressions, and will throw
		// an exception when an invalid one is found.
		void CheckBypassList ()
		{
			if (bypassList == null)
				return;
			for (int i = 0; i < bypassList.Count; i++)
				new Regex ((string) bypassList [i]);
		}

		static Uri ToUri (string address)
		{
			if (address == null)
				return null;
				
			if (address.IndexOf ("://", StringComparison.Ordinal) == -1) 
				address = "http://" + address;

			return new Uri (address);
		}
	}
}
