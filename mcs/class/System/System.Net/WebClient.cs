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
		
		[MonoTODO("depends on OpenRead")]
		public byte [] DownloadData (string address)
		{
			const int readSize = 4096;
			Stream networkStream = OpenRead (address);
			MemoryStream ms = new MemoryStream ();
			byte[] buf = new byte [readSize];
			int size = 0;
			do {
				size = networkStream.Read (buf, 0, readSize);
				ms.Write (buf, 0, size);
			} while (size == readSize);
			networkStream.Close ();
			return ms.GetBuffer ();
		}
		
		[MonoTODO("depends on DownloadData")]
		public void DownloadFile (string address, string fileName)
		{
			byte[] buf = DownloadData (address);
			new FileStream (fileName, FileMode.CreateNew).Write (buf, 0, buf.Length);
		}
		
		[MonoTODO("some tests are required")]
		public Stream OpenRead (string address)
		{
			Uri uri = new Uri (address);
			WebRequest request = null;

			if (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)
				request = new HttpWebRequest (uri);
			else if(uri.Scheme == Uri.UriSchemeFile)
				request = new FileWebRequest (uri);
			else
				throw new NotImplementedException ();

			return request.GetResponse ().GetResponseStream ();
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