//
// System.Net.WebProxy
//
// Author:
//   Lawrence Pit (loz@cable.a2000.nl)
//

using System;
using System.Collections;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

namespace System.Net 
{
	[Serializable]
	public class WebProxy : IWebProxy, ISerializable
	{		
		private Uri address;
		private bool bypassOnLocal;
		private ArrayList bypassList;
		private ICredentials credentials;
	
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
		
		public WebProxy (string address, bool bypassOnLocal,
				 string[] bypassList, ICredentials credentials)
			: this (ToUri (address), bypassOnLocal, bypassList, null) {}

		public WebProxy (Uri address, bool bypassOnLocal, 
				 string[] bypassList, ICredentials credentials)
		{
			this.address = address;
			this.bypassOnLocal = bypassOnLocal;
			if (bypassList == null)
				bypassList = new string [] {};
			this.bypassList = new ArrayList (bypassList);
			this.credentials = credentials;
			CheckBypassList ();
		}
		
		[MonoTODO]
		protected WebProxy (SerializationInfo serializationInfo, StreamingContext streamingContext) 
		{
			throw new NotImplementedException ();
		}
		
		// Properties
		
		public Uri Address {
			get { return address; }
			set { address = value; }
		}
		
		public ArrayList BypassArrayList {
			get { 
				return bypassList;
			}
		}
		
		public string [] BypassList {
			get { return (string []) bypassList.ToArray (typeof (string)); }
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
		
		// Methods
		
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
			if (address == null)
				return true;
			
			if (host.IsLoopback)
				return true;
				
			if (bypassOnLocal && host.Host.IndexOf ('.') == -1)
				return true;
				
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

		[MonoTODO]		
		void ISerializable.GetObjectData (SerializationInfo serializationInfo,
		                                  StreamingContext streamingContext)
		{
			throw new NotImplementedException ();
		}
		
		// Private Methods
		
		// this compiles the regular expressions, and will throw
		// an exception when an invalid one is found.
		private void CheckBypassList ()
		{			
			for (int i = 0; i < bypassList.Count; i++)
				new Regex ((string) bypassList [i]);
		}
		
		private static Uri ToUri (string address)
		{
			if (address == null)
				return null;
				
			if (address.IndexOf (':') == -1) 
				address = "http://" + address;
			
			return new Uri (address);
		}
	}
}
