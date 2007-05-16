//
// System.Net.WebClient
//
// Authors:
// 	Lawrence Pit (loz@cable.a2000.nl)
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//	Atsushi Enomoto (atsushi@ximian.com)
//
// (c) 2003 Ximian, Inc. (http://www.ximian.com)
// (C) 2006 Novell, Inc. (http://www.novell.com)
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
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
#if NET_2_0
using System.Collections.Generic;
using System.Threading;
#endif

namespace System.Net 
{
	[ComVisible(true)]
	public
#if !NET_2_0
	sealed
#endif
	class WebClient : Component
	{
		static readonly string urlEncodedCType = "application/x-www-form-urlencoded";
		static byte [] hexBytes;
		ICredentials credentials;
		WebHeaderCollection headers;
		WebHeaderCollection responseHeaders;
		Uri baseAddress;
		string baseString;
		NameValueCollection queryString;
		bool isBusy;
#if NET_2_0
		Encoding encoding = Encoding.Default;
		IWebProxy proxy;
#endif

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

#if NET_2_0
		public Encoding Encoding {
			get { return encoding; }
			set {
				if (value == null)
					throw new ArgumentNullException ("value");
				encoding = value;
			}
		}

		public IWebProxy Proxy {
			get { return proxy; }
			set { proxy = value; }
		}
#endif

#if NET_2_0
		public bool IsBusy {
			get { return isBusy || wait_handles != null && wait_handles.Count > 0; }
		}
#else
		bool IsBusy {
			get { return isBusy; }
		}
#endif

		// Methods

		void CheckBusy ()
		{
			if (IsBusy)
				throw new NotSupportedException ("WebClient does not support conccurent I/O operations.");
		}

		void SetBusy ()
		{
			lock (this) {
				CheckBusy ();
				isBusy = true;
			}
		}

		//   DownloadData

		public byte [] DownloadData (string address)
		{
			return DownloadData (MakeUri (address));
		}

#if NET_2_0
		public
#endif
		byte [] DownloadData (Uri address)
		{
			try {
				SetBusy ();
				return DownloadDataCore (address);
			} finally {
				isBusy = false;
			}
		}

		byte [] DownloadDataCore (Uri address)
		{
			WebRequest request = SetupRequest (address, "GET");
			WebResponse response = request.GetResponse ();
			Stream st = ProcessResponse (response);
			return ReadAll (st, (int) response.ContentLength);
		}

		//   DownloadFile

		public void DownloadFile (string address, string fileName)
		{
			DownloadFile (MakeUri (address), fileName);
		}

#if NET_2_0
		public
#endif
		void DownloadFile (Uri address, string fileName)
		{
			try {
				SetBusy ();
				DownloadFileCore (address, fileName);
			} finally {
				isBusy = false;
			}
		}

		void DownloadFileCore (Uri address, string fileName)
		{
			WebRequest request = SetupRequest (address);
			WebResponse response = request.GetResponse ();
			Stream st = ProcessResponse (response);

			int cLength = (int) response.ContentLength;
			int length = (cLength <= -1 || cLength > 8192) ? 8192 : cLength;
			byte [] buffer = new byte [length];
			FileStream f = new FileStream (fileName, FileMode.Create);

			int nread = 0;
			while ((nread = st.Read (buffer, 0, length)) != 0)
				f.Write (buffer, 0, nread);

			f.Close ();
		}

		//   OpenRead

		public Stream OpenRead (string address)
		{
			return OpenRead (MakeUri (address));
		}

#if NET_2_0
		public
#endif
		Stream OpenRead (Uri address)
		{
			try {
				SetBusy ();
				WebRequest request = SetupRequest (address);
				WebResponse response = request.GetResponse ();
				return ProcessResponse (response);
			} finally {
				isBusy = false;
			}
		}

		//   OpenWrite

		public Stream OpenWrite (string address)
		{
			return OpenWrite (MakeUri (address));
		}
		
		public Stream OpenWrite (string address, string method)
		{
			return OpenWrite (MakeUri (address), method);
		}

#if NET_2_0
		public
#endif
		Stream OpenWrite (Uri address)
		{
			return OpenWrite (address, DetermineMethod (address));
		}

#if NET_2_0
		public
#endif
		Stream OpenWrite (Uri address, string method)
		{
			try {
				SetBusy ();
				WebRequest request = SetupRequest (address, method);
				return request.GetRequestStream ();
			} finally {
				isBusy = false;
			}
		}

		private string DetermineMethod (Uri address)
		{
			if (address == null)
				throw new ArgumentNullException ("address");
#if NET_2_0
			if (address.Scheme == Uri.UriSchemeFtp)
				return "RETR";
#endif
			return "POST";
		}

		//   UploadData

		public byte [] UploadData (string address, byte [] data)
		{
			return UploadData (MakeUri (address), data);
		}
		
		public byte [] UploadData (string address, string method, byte [] data)
		{
			return UploadData (MakeUri (address), method, data);
		}

#if NET_2_0
		public
#endif
		byte [] UploadData (Uri address, byte [] data)
		{
			return UploadData (address, DetermineMethod (address), data);
		}

#if NET_2_0
		public
#endif
		byte [] UploadData (Uri address, string method, byte [] data)
		{
			try {
				SetBusy ();
				return UploadDataCore (address, method, data);
			} finally {
				isBusy = false;
			}
		}

		byte [] UploadDataCore (Uri address, string method, byte [] data)
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

		//   UploadFile

		public byte [] UploadFile (string address, string fileName)
		{
			return UploadFile (MakeUri (address), fileName);
		}

#if NET_2_0
		public
#endif
		byte [] UploadFile (Uri address, string fileName)
		{
			return UploadFile (address, DetermineMethod (address), fileName);
		}
		
		public byte [] UploadFile (string address, string method, string fileName)
		{
			return UploadFile (MakeUri (address), method, fileName);
		}

#if NET_2_0
		public
#endif
		byte [] UploadFile (Uri address, string method, string fileName)
		{
			try {
				SetBusy ();
				return UploadFileCore (address, method, fileName);
			} finally {
				isBusy = false;
			}
		}

		byte [] UploadFileCore (Uri address, string method, string fileName)
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
			return UploadValues (MakeUri (address), data);
		}
		
		public byte[] UploadValues (string address, string method, NameValueCollection data)
		{
			return UploadValues (MakeUri (address), method, data);
		}

#if NET_2_0
		public
#endif
		byte[] UploadValues (Uri address, NameValueCollection data)
		{
			return UploadValues (address, DetermineMethod (address), data);
		}

#if NET_2_0
		public
#endif
		byte[] UploadValues (Uri uri, string method, NameValueCollection data)
		{
			try {
				SetBusy ();
				return UploadValuesCore (uri, method, data);
			} finally {
				isBusy = false;
			}
		}

		byte[] UploadValuesCore (Uri uri, string method, NameValueCollection data)
		{
			if (data == null)
				throw new ArgumentNullException ("data"); // MS throws a nullref

			string cType = Headers ["Content-Type"];
			if (cType != null && String.Compare (cType, urlEncodedCType, true) != 0)
				throw new WebException ("Content-Type header cannot be changed from its default " +
							"value for this request.");

			Headers ["Content-Type"] = urlEncodedCType;
			WebRequest request = SetupRequest (uri, method);
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

#if NET_2_0
		public string DownloadString (string address)
		{
			return encoding.GetString (DownloadData (address));
		}

		public string DownloadString (Uri address)
		{
			return encoding.GetString (DownloadData (address));
		}

		public string UploadString (string address, string data)
		{
			byte [] resp = UploadData (address, encoding.GetBytes (data));
			return encoding.GetString (resp);
		}

		public string UploadString (string address, string method, string data)
		{
			byte [] resp = UploadData (address, method, encoding.GetBytes (data));
			return encoding.GetString (resp);
		}

		public string UploadString (Uri address, string data)
		{
			byte [] resp = UploadData (address, encoding.GetBytes (data));
			return encoding.GetString (resp);
		}

		public string UploadString (Uri address, string method, string data)
		{
			byte [] resp = UploadData (address, method, encoding.GetBytes (data));
			return encoding.GetString (resp);
		}

		public event DownloadDataCompletedEventHandler DownloadDataCompleted;
		public event AsyncCompletedEventHandler DownloadFileCompleted;
		public event DownloadProgressChangedEventHandler DownloadProgressChanged;
		public event DownloadStringCompletedEventHandler DownloadStringCompleted;
		public event OpenReadCompletedEventHandler OpenReadCompleted;
		public event OpenWriteCompletedEventHandler OpenWriteCompleted;
		public event UploadDataCompletedEventHandler UploadDataCompleted;
		public event UploadFileCompletedEventHandler UploadFileCompleted;
		public event UploadProgressChangedEventHandler UploadProgressChanged;
		public event UploadStringCompletedEventHandler UploadStringCompleted;
		public event UploadValuesCompletedEventHandler UploadValuesCompleted;
#endif

		Uri MakeUri (string path)
		{
			string query = null;
			if (queryString != null && queryString.Count != 0) {
				// This is not the same as UploadValues, because these 'keys' are not
				// urlencoded here.
				StringBuilder sb = new StringBuilder ();
				sb.Append ('?');
				foreach (string key in queryString)
					sb.AppendFormat ("{0}={1}&", key, UrlEncode (queryString [key]));

				if (sb.Length != 0) {
					sb.Length--; // remove trailing '&'
					query = sb.ToString ();
				}
			}
			

			if (baseAddress == null && query == null) {
				try {
					return new Uri (path);
				}
				catch (System.UriFormatException) {
					if ((path[0] == Path.DirectorySeparatorChar) || (path[1] == ':' && Char.ToLower(path[0]) > 'a' && Char.ToLower(path[0]) < 'z')) {
						return new Uri ("file://" + path);
					}
					else {
						return new Uri ("file://" + Environment.CurrentDirectory + Path.DirectorySeparatorChar + path);
					}
				}
			}

			if (baseAddress == null)
				return new Uri (path + query, (query != null));

			if (query == null)
				return new Uri (baseAddress, path);

			return new Uri (baseAddress, path + query, (query != null));
		}
		
		WebRequest SetupRequest (Uri uri)
		{
			WebRequest request = WebRequest.Create (uri);
#if NET_2_0
			if (Proxy != null)
				request.Proxy = Proxy;
#endif
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

		WebRequest SetupRequest (Uri uri, string method)
		{
			WebRequest request = SetupRequest (uri);
			request.Method = method;
			return request;
		}

		WebRequest SetupRequest (Uri uri, string method, int contentLength)
		{
			WebRequest request = SetupRequest (uri, method);
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

		string UrlEncode (string str)
		{
			StringBuilder result = new StringBuilder ();

			int len = str.Length;
			for (int i = 0; i < len; i++) {
				char c = str [i];
				if (c == ' ')
					result.Append ('+');
				else if ((c < '0' && c != '-' && c != '.') ||
					 (c < 'A' && c > '9') ||
					 (c > 'Z' && c < 'a' && c != '_') ||
					 (c > 'z')) {
					result.Append ('%');
					int idx = ((int) c) >> 4;
					result.Append ((char) hexBytes [idx]);
					idx = ((int) c) & 0x0F;
					result.Append ((char) hexBytes [idx]);
				} else {
					result.Append (c);
				}
			}

			return result.ToString ();
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

#if NET_2_0
		List<RegisteredWaitHandle> wait_handles;

		List<RegisteredWaitHandle> WaitHandles {
			get {
				if (wait_handles == null)
					wait_handles = new List<RegisteredWaitHandle> ();
				return wait_handles;
			}
		}

		[MonoTODO ("Is it enough to just unregister wait handles from ThreadPool?")]
		public void CancelAsync ()
		{
			if (wait_handles == null)
				return;
			lock (wait_handles) {
				foreach (RegisteredWaitHandle handle in wait_handles)
					handle.Unregister (null);
				wait_handles.Clear ();
			}
		}

		//    DownloadDataAsync

		public void DownloadDataAsync (Uri uri)
		{
			DownloadDataAsync (uri, null);
		}

		public void DownloadDataAsync (Uri uri, object asyncState)
		{
			lock (this) {
				CheckBusy ();

				object [] cbArgs = new object [] {uri, asyncState};
				WaitOrTimerCallback cb = delegate (object state, bool timedOut) {
					object [] args = (object []) state;
					byte [] data = timedOut ? null : DownloadData ((Uri) args [0]);
					OnDownloadDataCompleted (
						new DownloadDataCompletedEventArgs (data, null, timedOut, args [1]));
					};
				AutoResetEvent ev = new AutoResetEvent (true);
				WaitHandles.Add (ThreadPool.RegisterWaitForSingleObject (ev, cb, cbArgs, -1, true));
			}
		}

		//    DownloadFileAsync

		public void DownloadFileAsync (Uri uri, string method)
		{
			DownloadFileAsync (uri, method, null);
		}

		public void DownloadFileAsync (Uri uri, string method, object asyncState)
		{
			lock (this) {
				CheckBusy ();

				object [] cbArgs = new object [] {uri, method, asyncState};
				WaitOrTimerCallback cb = delegate (object innerState, bool timedOut) {
					object [] args = (object []) innerState;
					if (!timedOut)
						DownloadFile ((Uri) args [0], (string) args [1]);
					OnDownloadFileCompleted (
						new AsyncCompletedEventArgs (null, timedOut, args [2]));
					};
				AutoResetEvent ev = new AutoResetEvent (true);
				WaitHandles.Add (ThreadPool.RegisterWaitForSingleObject (ev, cb, cbArgs, -1, true));
			}
		}

		//    DownloadStringAsync

		public void DownloadStringAsync (Uri uri)
		{
			DownloadStringAsync (uri, null);
		}

		public void DownloadStringAsync (Uri uri, object asyncState)
		{
			lock (this) {
				CheckBusy ();

				object [] cbArgs = new object [] {uri, asyncState};
				WaitOrTimerCallback cb = delegate (object innerState, bool timedOut) {
					object [] args = (object []) innerState;
					string data = timedOut ? null : DownloadString ((Uri) args [0]);
					OnDownloadStringCompleted (
						new DownloadStringCompletedEventArgs (data, null, timedOut, args [1]));
					};
				AutoResetEvent ev = new AutoResetEvent (true);
				WaitHandles.Add (ThreadPool.RegisterWaitForSingleObject (ev, cb, cbArgs, -1, true));
			}
		}

		//    OpenReadAsync

		public void OpenReadAsync (Uri uri)
		{
			OpenReadAsync (uri, null);
		}

		public void OpenReadAsync (Uri uri, object asyncState)
		{
			lock (this) {
				CheckBusy ();

				object [] cbArgs = new object [] {uri, asyncState};
				WaitOrTimerCallback cb = delegate (object innerState, bool timedOut) {
					object [] args = (object []) innerState;
					Stream stream = timedOut ? null : OpenRead ((Uri) args [0]);
					OnOpenReadCompleted (
						new OpenReadCompletedEventArgs (stream, null, timedOut, args [1]));
					};
				AutoResetEvent ev = new AutoResetEvent (true);
				WaitHandles.Add (ThreadPool.RegisterWaitForSingleObject (ev, cb, cbArgs, -1, true));
			}
		}

		//    OpenWriteAsync

		public void OpenWriteAsync (Uri uri)
		{
			OpenWriteAsync (uri, null);
		}

		public void OpenWriteAsync (Uri uri, string method)
		{
			OpenWriteAsync (uri, method, null);
		}

		public void OpenWriteAsync (Uri uri, string method, object asyncState)
		{
			lock (this) {
				CheckBusy ();

				object [] cbArgs = new object [] {uri, method, asyncState};
				WaitOrTimerCallback cb = delegate (object innerState, bool timedOut) {
					object [] args = (object []) innerState;
					Stream stream = timedOut ? null : OpenWrite ((Uri) args [0], (string) args [1]);
					OnOpenWriteCompleted (
						new OpenWriteCompletedEventArgs (stream, null, timedOut, args [2]));
					};
				AutoResetEvent ev = new AutoResetEvent (true);
				WaitHandles.Add (ThreadPool.RegisterWaitForSingleObject (ev, cb, cbArgs, -1, true));
			}
		}

		//    UploadDataAsync

		public void UploadDataAsync (Uri uri, byte [] data)
		{
			UploadDataAsync (uri, null, data);
		}

		public void UploadDataAsync (Uri uri, string method, byte [] data)
		{
			UploadDataAsync (uri, method, data, null);
		}

		public void UploadDataAsync (Uri uri, string method, byte [] data, object asyncState)
		{
			lock (this) {
				CheckBusy ();

				object [] cbArgs = new object [] {uri, method, data,  asyncState};
				WaitOrTimerCallback cb = delegate (object innerState, bool timedOut) {
					object [] args = (object []) innerState;
					byte [] data2 = timedOut ? null : UploadData ((Uri) args [0], (string) args [1], (byte []) args [2]);
					OnUploadDataCompleted (
						new UploadDataCompletedEventArgs (data2, null, timedOut, args [3]));
					};
				AutoResetEvent ev = new AutoResetEvent (true);
				WaitHandles.Add (ThreadPool.RegisterWaitForSingleObject (ev, cb, cbArgs, -1, true));
			}
		}

		//    UploadFileAsync

		public void UploadFileAsync (Uri uri, string file)
		{
			UploadFileAsync (uri, null, file);
		}

		public void UploadFileAsync (Uri uri, string method, string file)
		{
			UploadFileAsync (uri, method, file, null);
		}

		public void UploadFileAsync (Uri uri, string method, string file, object asyncState)
		{
			lock (this) {
				CheckBusy ();

				object [] cbArgs = new object [] {uri, method, file,  asyncState};
				WaitOrTimerCallback cb = delegate (object innerState, bool timedOut) {
					object [] args = (object []) innerState;
					byte [] data = timedOut ? null : UploadFile ((Uri) args [0], (string) args [1], (string) args [2]);
					OnUploadFileCompleted (
						new UploadFileCompletedEventArgs (data, null, timedOut, args [3]));
					};
				AutoResetEvent ev = new AutoResetEvent (true);
				WaitHandles.Add (ThreadPool.RegisterWaitForSingleObject (ev, cb, cbArgs, -1, true));
			}
		}

		//    UploadStringAsync

		public void UploadStringAsync (Uri uri, string data)
		{
			UploadStringAsync (uri, null, data);
		}

		public void UploadStringAsync (Uri uri, string method, string data)
		{
			UploadStringAsync (uri, method, data, null);
		}

		public void UploadStringAsync (Uri uri, string method, string data, object asyncState)
		{
			lock (this) {
				CheckBusy ();

				object [] cbArgs = new object [] {uri, method, data, asyncState};
				WaitOrTimerCallback cb = delegate (object innerState, bool timedOut) {
					object [] args = (object []) innerState;
					string data2 = timedOut ? null : UploadString ((Uri) args [0], (string) args [1], (string) args [2]);
					OnUploadStringCompleted (
						new UploadStringCompletedEventArgs (data2, null, timedOut, args [3]));
					};
				AutoResetEvent ev = new AutoResetEvent (true);
				WaitHandles.Add (ThreadPool.RegisterWaitForSingleObject (ev, cb, cbArgs, -1, true));
			}
		}

		//    UploadValuesAsync

		public void UploadValuesAsync (Uri uri, NameValueCollection values)
		{
			UploadValuesAsync (uri, null, values);
		}

		public void UploadValuesAsync (Uri uri, string method, NameValueCollection values)
		{
			UploadValuesAsync (uri, method, values, null);
		}

		public void UploadValuesAsync (Uri uri, string method, NameValueCollection values, object asyncState)
		{
			lock (this) {
				CheckBusy ();

				object [] cbArgs = new object [] {uri, method, values,  asyncState};
				WaitOrTimerCallback cb = delegate (object innerState, bool timedOut) {
					object [] args = (object []) innerState;
					byte [] data = timedOut ? null : UploadValues ((Uri) args [0], (string) args [1], (NameValueCollection) args [2]);
					OnUploadValuesCompleted (
						new UploadValuesCompletedEventArgs (data, null, timedOut, args [3]));
					};
				AutoResetEvent ev = new AutoResetEvent (true);
				WaitHandles.Add (ThreadPool.RegisterWaitForSingleObject (ev, cb, cbArgs, -1, true));
			}
		}

		protected virtual void OnDownloadDataCompleted (
			DownloadDataCompletedEventArgs args)
		{
			if (DownloadDataCompleted != null)
				DownloadDataCompleted (this, args);
		}

		protected virtual void OnDownloadFileCompleted (
			AsyncCompletedEventArgs args)
		{
			if (DownloadFileCompleted != null)
				DownloadFileCompleted (this, args);
		}

		protected virtual void OnDownloadProgressChanged (DownloadProgressChangedEventArgs e)
		{
			if (DownloadProgressChanged != null)
				DownloadProgressChanged (this, e);
		}

		protected virtual void OnDownloadStringCompleted (
			DownloadStringCompletedEventArgs args)
		{
			if (DownloadStringCompleted != null)
				DownloadStringCompleted (this, args);
		}

		protected virtual void OnOpenReadCompleted (
			OpenReadCompletedEventArgs args)
		{
			if (OpenReadCompleted != null)
				OpenReadCompleted (this, args);
		}

		protected virtual void OnOpenWriteCompleted (
			OpenWriteCompletedEventArgs args)
		{
			if (OpenWriteCompleted != null)
				OpenWriteCompleted (this, args);
		}

		protected virtual void OnUploadDataCompleted (
			UploadDataCompletedEventArgs args)
		{
			if (UploadDataCompleted != null)
				UploadDataCompleted (this, args);
		}

		protected virtual void OnUploadFileCompleted (
			UploadFileCompletedEventArgs args)
		{
			if (UploadFileCompleted != null)
				UploadFileCompleted (this, args);
		}

		protected virtual void OnUploadProgressChanged (UploadProgressChangedEventArgs e)
		{
			if (UploadProgressChanged != null)
				UploadProgressChanged (this, e);
		}

		protected virtual void OnUploadStringCompleted (
			UploadStringCompletedEventArgs args)
		{
			if (UploadStringCompleted != null)
				UploadStringCompleted (this, args);
		}

		protected virtual void OnUploadValuesCompleted (
			UploadValuesCompletedEventArgs args)
		{
			if (UploadValuesCompleted != null)
				UploadValuesCompleted (this, args);
		}

		[MonoNotSupported("")]
		protected virtual WebRequest GetWebRequest (Uri address)
		{
			throw new NotImplementedException ();
		}

		protected virtual WebResponse GetWebResponse (WebRequest request)
		{
			return request.GetResponse ();
		}

		protected virtual WebResponse GetWebResponse (WebRequest request, IAsyncResult result)
		{
			return request.EndGetResponse (result);
		}
#endif
	}
}


