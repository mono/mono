//
// System.Net.WebClient
//
// Authors:
// 	Lawrence Pit (loz@cable.a2000.nl)
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (c) 2003 Ximian, Inc. (http://www.ximian.com)
//

using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;

namespace System.Net 
{
	[ComVisible(true)]
	public sealed class WebClient : Component
	{
		static readonly string urlEncodedCType = "application/x-www-form-urlencoded";
		static byte [] hexBytes;
		ICredentials credentials;
		WebHeaderCollection headers;
		WebHeaderCollection responseHeaders;
		Uri baseAddress;
		string baseString;
		NameValueCollection queryString;
	
		// Constructors
		static WebClient ()
		{
			hexBytes = new byte [16];
			int index = 0;
			for (int i = '0'; i <= '9'; i++, index++)
				hexBytes [index] = (byte) i;

			for (int i = 'A'; i <= 'F'; i++, index++)
				hexBytes [index] = (byte) i;
		}
		
		public WebClient ()
		{
		}
		
		// Properties
		
		public string BaseAddress {
			get {
				if (baseString == null) {
					if (baseAddress == null)
						return "";
				}

				baseString = baseAddress.ToString ();
				return baseString;
			}
			
			set {
				if (value == null || value == "") {
					baseAddress = null;
				} else {
					baseAddress = new Uri (value);
				}
			}
		}
		
		public ICredentials Credentials {
			get { return credentials; }
			set { credentials = value; }
		}
		
		public WebHeaderCollection Headers {
			get {
				if (headers == null)
					headers = new WebHeaderCollection ();

				return headers;
			}
			set { headers = value; }
		}
		
		public NameValueCollection QueryString {
			get {
				if (queryString == null)
					queryString = new NameValueCollection ();

				return queryString;
			}
			set { queryString = value; }
		}
		
		public WebHeaderCollection ResponseHeaders {
			get { return responseHeaders; }
		}

		// Methods
		
		public byte [] DownloadData (string address)
		{
			WebRequest request = SetupRequest (address);
			WebResponse response = request.GetResponse ();
			Stream st = ProcessResponse (response);
			return ReadAll (st, (int) response.ContentLength);
		}
		
		public void DownloadFile (string address, string fileName)
		{
			WebRequest request = SetupRequest (address);
			WebResponse response = request.GetResponse ();
			Stream st = ProcessResponse (response);

			int cLength = (int) response.ContentLength;
			int length = (cLength <= -1 || cLength > 8192) ? 8192 : cLength;
			byte [] buffer = new byte [length];
			FileStream f = new FileStream (fileName, FileMode.CreateNew);

			int nread = 0;
			while ((nread = st.Read (buffer, 0, length)) != 0)
				f.Write (buffer, 0, nread);

			f.Close ();
		}
		
		public Stream OpenRead (string address)
		{
			WebRequest request = SetupRequest (address);
			WebResponse response = request.GetResponse ();
			return ProcessResponse (response);
		}
		
		public Stream OpenWrite (string address)
		{
			return OpenWrite (address, "POST");
		}
		
		public Stream OpenWrite (string address, string method)
		{
			WebRequest request = SetupRequest (address, method);
			return request.GetRequestStream ();
		}
				
		public byte [] UploadData (string address, byte [] data)
		{
			return UploadData (address, "POST", data);
		}
		
		public byte [] UploadData (string address, string method, byte [] data)
		{
			if (data == null)
				throw new ArgumentNullException ("data");

			int contentLength = data.Length;
			WebRequest request = SetupRequest (address, method, contentLength);
			using (Stream stream = request.GetRequestStream ()) {
				stream.Write (data, 0, contentLength);
			}

			WebResponse response = request.GetResponse ();
			Stream st = ProcessResponse (response);
			return ReadAll (st, (int) response.ContentLength);
		}
		
		public byte [] UploadFile (string address, string fileName)
		{
			return UploadFile (address, "POST", fileName);
		}
		
		public byte [] UploadFile (string address, string method, string fileName)
		{
			string fileCType = Headers ["Content-Type"];
			if (fileCType != null) {
				string lower = fileCType.ToLower ();
				if (lower.StartsWith ("multipart/"))
					throw new WebException ("Content-Type cannot be set to a multipart" +
								" type for this request.");
			} else {
				fileCType = "application/octet-stream";
			}

			string boundary = "------------" + DateTime.Now.Ticks.ToString ("x");
			Headers ["Content-Type"] = String.Format ("multipart/form-data; boundary={0}", boundary);
			WebRequest request = SetupRequest (address, method);
			Stream reqStream = null;
			Stream fStream = null;
			byte [] resultBytes = null;

			try {
				fStream = File.OpenRead (fileName);
				reqStream = request.GetRequestStream ();
				byte [] realBoundary = Encoding.ASCII.GetBytes ("--" + boundary + "\r\n");
				reqStream.Write (realBoundary, 0, realBoundary.Length);
				string partHeaders = String.Format ("Content-Disposition: form-data; " +
								    "name=\"file\"; filename=\"{0}\"\r\n" +
								    "Content-Type: {1}\r\n\r\n",
								    Path.GetFileName (fileName), fileCType);

				byte [] partHeadersBytes = Encoding.UTF8.GetBytes (partHeaders);
				reqStream.Write (partHeadersBytes, 0, partHeadersBytes.Length);
				int nread;
				byte [] buffer = new byte [4096];
				while ((nread = fStream.Read (buffer, 0, 4096)) != 0)
					reqStream.Write (buffer, 0, nread);

				reqStream.WriteByte ((byte) '\r');
				reqStream.WriteByte ((byte) '\n');
				reqStream.Write (realBoundary, 0, realBoundary.Length);
				reqStream.Close ();
				reqStream = null;
				WebResponse response = request.GetResponse ();
				Stream st = ProcessResponse (response);
				resultBytes = ReadAll (st, (int) response.ContentLength);
			} catch (WebException) {
				throw;
			} catch (Exception e) {
				throw new WebException ("Error uploading file.", e);
			} finally {
				if (fStream != null)
					fStream.Close ();

				if (reqStream != null)
					reqStream.Close ();
			}
			
			return resultBytes;	
		}
		
		public byte[] UploadValues (string address, NameValueCollection data)
		{
			return UploadValues (address, "POST", data);
		}
		
		public byte[] UploadValues (string address, string method, NameValueCollection data)
		{
			if (data == null)
				throw new ArgumentNullException ("data"); // MS throws a nullref

			string cType = Headers ["Content-Type"];
			if (cType != null && String.Compare (cType, urlEncodedCType, true) != 0)
				throw new WebException ("Content-Type header cannot be changed from its default " +
							"value for this request.");

			Headers ["Content-Type"] = urlEncodedCType;
			WebRequest request = SetupRequest (address, method);
			Stream rqStream = request.GetRequestStream ();
			MemoryStream tmpStream = new MemoryStream ();
			foreach (string key in data) {
				byte [] bytes = Encoding.ASCII.GetBytes (key);
				UrlEncodeAndWrite (tmpStream, bytes);
				tmpStream.WriteByte ((byte) '=');
				bytes = Encoding.ASCII.GetBytes (data [key]);
				UrlEncodeAndWrite (tmpStream, bytes);
				tmpStream.WriteByte ((byte) '&');
			}

			int length = (int) tmpStream.Length;
			if (length > 0)
				tmpStream.SetLength (--length); // remove trailing '&'

			tmpStream.WriteByte ((byte) '\r');
			tmpStream.WriteByte ((byte) '\n');

			byte [] buf = tmpStream.GetBuffer ();
			rqStream.Write (buf, 0, length + 2);
			rqStream.Close ();
			tmpStream.Close ();

			WebResponse response = request.GetResponse ();
			Stream st = ProcessResponse (response);
			return ReadAll (st, (int) response.ContentLength);
		}

		Uri MakeUri (string path)
		{
			string query = null;
			if (queryString != null && queryString.Count != 0) {
				// This is not the same as UploadValues, because these 'keys' are not
				// urlencoded here.
				StringBuilder sb = new StringBuilder ();
				sb.Append ('?');
				foreach (string key in queryString)
					sb.AppendFormat ("{0}={1}&", key, queryString [key]);

				if (sb.Length != 0) {
					sb.Length--; // remove trailing '&'
					query = sb.ToString ();
				}
			}
			

			if (baseAddress == null && query == null)
				return new Uri (path);

			if (baseAddress == null)
				return new Uri (path + query);

			if (query == null)
				return new Uri (baseAddress, path);

			return new Uri (baseAddress, path + query);
		}
		
		WebRequest SetupRequest (string address)
		{
			Uri uri = MakeUri (address);
			WebRequest request = WebRequest.Create (uri);
			request.Credentials = credentials;

			// Special headers. These are properties of HttpWebRequest.
			// What do we do with other requests differnt from HttpWebRequest?
			if (headers != null && headers.Count != 0 && (request is HttpWebRequest)) {
				HttpWebRequest req = (HttpWebRequest) request;
				string expect = headers ["Expect"];
				string contentType = headers ["Content-Type"];
				string accept = headers ["Accept"];
				string connection = headers ["Connection"];
				string userAgent = headers ["User-Agent"];
				string referer = headers ["Referer"];
				headers.RemoveInternal ("Expect");
				headers.RemoveInternal ("Content-Type");
				headers.RemoveInternal ("Accept");
				headers.RemoveInternal ("Connection");
				headers.RemoveInternal ("Referer");
				headers.RemoveInternal ("User-Agent");
				request.Headers = headers;

				if (expect != null && expect != "")
					req.Expect = expect;

				if (accept != null && accept != "")
					req.Accept = accept;

				if (contentType != null && contentType != "")
					req.ContentType = contentType;

				if (connection != null && connection != "")
					req.Connection = connection;

				if (userAgent != null && userAgent != "")
					req.UserAgent = userAgent;

				if (referer != null && referer != "")
					req.Referer = referer;
			}

			responseHeaders = null;
			return request;
		}

		WebRequest SetupRequest (string address, string method)
		{
			WebRequest request = SetupRequest (address);
			request.Method = method;
			return request;
		}

		WebRequest SetupRequest (string address, string method, int contentLength)
		{
			WebRequest request = SetupRequest (address, method);
			request.ContentLength = contentLength;
			return request;
		}

		Stream ProcessResponse (WebResponse response)
		{
			responseHeaders = response.Headers;
			return response.GetResponseStream ();
		}

		static byte [] ReadAll (Stream stream, int length)
		{
			MemoryStream ms = null;
			
			bool nolength = (length == -1);
			int size = ((nolength) ? 8192 : length);
			if (nolength)
				ms = new MemoryStream ();

			int nread = 0;
			int offset = 0;
			byte [] buffer = new byte [size];
			while ((nread = stream.Read (buffer, offset, size)) != 0) {
				if (nolength) {
					ms.Write (buffer, 0, nread);
				} else {
					offset += nread;
					size -= nread;
				}
			}

			if (nolength)
				return ms.ToArray ();

			return buffer;
		}

		static void UrlEncodeAndWrite (Stream stream, byte [] bytes)
		{
			if (bytes == null)
				return;

			int len = bytes.Length;
			if (len == 0)
				return;

			for (int i = 0; i < len; i++) {
				char c = (char) bytes [i];
				if (c == ' ')
					stream.WriteByte ((byte) '+');
				else if ((c < '0' && c != '-' && c != '.') ||
					 (c < 'A' && c > '9') ||
					 (c > 'Z' && c < 'a' && c != '_') ||
					 (c > 'z')) {
					stream.WriteByte ((byte) '%');
					int idx = ((int) c) >> 4;
					stream.WriteByte (hexBytes [idx]);
					idx = ((int) c) & 0x0F;
					stream.WriteByte (hexBytes [idx]);
				} else {
					stream.WriteByte ((byte) c);
				}
			}
		}
	}
}

