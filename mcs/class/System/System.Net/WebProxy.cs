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
		private string [] bypassList;
		private ICredentials credentials;
		
		private Regex [] bypassRegexList;
	
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
			this.bypassList = bypassList;
			this.credentials = credentials;
			CreateBypassRegexList ();
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
				return new ArrayList (bypassList);
			}
		}
		
		public string[] BypassList {
			get { return bypassList; }
			set { 
				if (value == null)
					throw new ArgumentNullException ();
				bypassList = value; 
				CreateBypassRegexList ();
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
		
		[MonoTODO]
		public static WebProxy GetDefaultProxy ()
		{
			// for Mono we should probably read in these settings
			// from the global application configuration file
			
			// for now, return the empty WebProxy to indicate
			// no proxy is used
			// return GlobalProxySelection.GetEmptyWebProxy ();
			// ??
			
			throw new NotImplementedException ();
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
				
			string hostStr = host.Scheme + "://" + host.Authority;				
			for (int i = 0; i < bypassRegexList.Length; i++) 
				if (bypassRegexList [i].IsMatch (hostStr))
					return true;
			
			return false;
		}

		[MonoTODO]		
		void ISerializable.GetObjectData (SerializationInfo serializationInfo,
		                                  StreamingContext streamingContext)
		{
			throw new NotImplementedException ();
		}
		
		// Private Methods
		
		private void CreateBypassRegexList ()
		{			
			bypassRegexList	= new Regex [bypassList.Length];
			for (int i = 0; i < bypassList.Length; i++)
				bypassRegexList [i] = new Regex (bypassList [i], 
							// TODO: RegexOptions.Compiled |  // not implemented yet by Regex
							RegexOptions.IgnoreCase |
							RegexOptions.Singleline);
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