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

		ICredentials credentials;
	
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
		
		public ICredentials Credentials {
			get { return credentials; }
			set { credentials = value; }
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
		
		public byte [] DownloadData (string address)
		{
			const int readSize = 8192;
			Stream networkStream = OpenRead (address);
			ArrayList chunks = new ArrayList ();
			byte[] buf = new byte [readSize];
			int size = 0;
			int total_size = 0;

			try {
				do {
					size = networkStream.Read (buf, 0, readSize);
					byte [] copy = new byte [size];
					Array.Copy (buf, 0, copy,0, size);
					chunks.Add (copy);
					total_size += size;
				} while (size != 0);
			} finally {
				networkStream.Close ();
			}
			
			byte [] result = new byte [total_size];
			int target = 0;
			foreach (byte [] block in chunks){
				int len = block.Length;
				Array.Copy (block, 0, result, target, len);
				target += len;
			}
			return result;
		}
		
		public void DownloadFile (string address, string fileName)
		{
			byte[] buf = DownloadData (address);
			using (FileStream f = new FileStream (fileName, FileMode.CreateNew)){
				f.Write (buf, 0, buf.Length);
			}
		}
		
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
