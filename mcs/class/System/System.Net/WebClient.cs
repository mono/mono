//
// System.Net.WebClient
//
// Author:
//   Lawrence Pit (loz@cable.a2000.nl)
//

using System;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace System.Net 
{
	[ComVisible(true)]
	public sealed class WebClient : Component
	{		
	
		// Constructors
		
		public WebClient ()
		{
		}
		
		// Properties
		
		[MonoTODO]
		public string BaseAddress {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
		
		[MonoTODO]
		public ICredentials Credentials {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
		
		[MonoTODO]
		public WebHeaderCollection Headers {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
		
		[MonoTODO]
		public NameValueCollection QueryString {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
		
		[MonoTODO]
		public WebHeaderCollection ResponseHeaders {
			get { throw new NotImplementedException (); }
		}

		// Methods
		
		[MonoTODO]
		public byte [] DownloadData (string address)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public void DownloadFile (string address, string fileName)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public Stream OpenRead (string address)
		{
			throw new NotImplementedException ();
		}
		
		public Stream OpenWrite (string address)
		{
			return OpenWrite (address, "POST");
		}
		
		[MonoTODO]
		public Stream OpenWrite (string address, string method)
		{
			throw new NotImplementedException ();
		}
				
		public byte [] UploadData (string address, byte [] data)
		{
			return UploadData (address, "POST", data);
		}
		
		[MonoTODO]
		public byte [] UploadData (string address, string method, byte [] data)
		{
			throw new NotImplementedException ();
		}
		
		public byte [] UploadFile (string address, string fileName)
		{
			return UploadFile (address, "POST", fileName);
		}
		
		[MonoTODO]
		public byte[] UploadFile (string address, string method, string fileName)
		{
			throw new NotImplementedException ();
		}
		
		public byte[] UploadValues (string address, NameValueCollection data)
		{
			return UploadValues (address, "POST", data);
		}
		
		[MonoTODO]
		public byte[] UploadValues (string address, string method, NameValueCollection data)
		{
			throw new NotImplementedException ();
		}
	}
}