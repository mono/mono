//
// System.Net.WebClient
//
// Authors:
// 	Lawrence Pit (loz@cable.a2000.nl)
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//	Atsushi Enomoto (atsushi@ximian.com)
//	Miguel de Icaza (miguel@ximian.com)
//	Stephane Delcroix (sdelcroix@novell.com)
//
// Copyright 2003 Ximian, Inc. (http://www.ximian.com)
// Copyright 2006, 2008 Novell, Inc. (http://www.novell.com)
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
//
// Notes on CancelAsync and Async methods:
//
//    WebClient.CancelAsync is implemented by calling Thread.Interrupt
//    in our helper thread.   The various async methods have to cancel
//    any ongoing requests by calling request.Abort () at that point.
//    In a few places (UploadDataCore, UploadValuesCore,
//    UploadFileCore) we catch the ThreadInterruptedException and
//    abort the request there.
//
//    Higher level routines (the async callbacks) also need to catch
//    the exception and raise the OnXXXXCompleted events there with
//    the "canceled" flag set to true. 
//
//    In a few other places where these helper routines are not used
//    (OpenReadAsync for example) catching the ThreadAbortException
//    also must abort the request.
//
//    The Async methods currently differ in their implementation from
//    the .NET implementation in that we manually catch any other
//    exceptions and correctly raise the OnXXXXCompleted passing the
//    Exception that caused the problem.   The .NET implementation
//    does not seem to have a mechanism to flag errors that happen
//    during downloads though.    We do this because we still need to
//    catch the exception on these helper threads, or we would
//    otherwise kill the application (on the 2.x profile, uncaught
//    exceptions in threads terminate the application).
//
using System;
using System.Security;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;

using Mono;

namespace System.Net 
{
#if NET_2_1
	// note: this type is effectively sealed to transparent code since it's default .ctor is marked with [SecuritySafeCritical]
	public class WebClient
	{
		private delegate void ProgressChangedDelegate (long read, long length, object state);

//		static readonly string urlEncodedCType = "application/x-www-form-urlencoded";
//		static byte [] hexBytes;
//		ICredentials credentials;
		WebHeaderCollection headers;
		WebHeaderCollection responseHeaders;
		Uri baseAddress;
		string baseString;
//		NameValueCollection queryString;
		bool is_busy, async;
		Thread async_thread;
		Encoding encoding = Encoding.UTF8;
		bool allow_read_buffering = true;
//		IWebProxy proxy;
//
//		// Constructors
//		static WebClient ()
//		{
//			hexBytes = new byte [16];
//			int index = 0;
//			for (int i = '0'; i <= '9'; i++, index++)
//				hexBytes [index] = (byte) i;
//
//			for (int i = 'A'; i <= 'F'; i++, index++)
//				hexBytes [index] = (byte) i;
//		}
//		
		public WebClient ()
		{
			// kind of calling NativeMethods.plugin_instance_get_source_location (PluginHost.Handle)
			// but without adding dependency on System.Windows.dll. GetData is [SecurityCritical]
			// this makes the default .ctor [SecuritySafeCritical] which would be a problem (inheritance)
			// but it happens that MS SL2 also has this default .ctor as SSC :-)
			baseAddress = new Uri (AppDomain.CurrentDomain.GetData ("xap_uri") as string);
		}
//		
//		// Properties
//		
		public string BaseAddress {
			get {
				if (baseString == null) {
					if (baseAddress == null)
						return String.Empty;
					else
						baseString = baseAddress.ToString ();
				}
				return baseString;
			}
			
			set {
				if (String.IsNullOrEmpty (value)) {
					baseAddress = null;
				} else {
					baseAddress = new Uri (value);
				}
			}
		}
//
//#if NET_2_0
//		static Exception GetMustImplement ()
//		{
//			return new NotImplementedException ();
//		}
//		
//		[MonoTODO]
//		public RequestCachePolicy CachePolicy
//		{
//			get {
//				throw GetMustImplement ();
//			}
//			set {
//				throw GetMustImplement ();
//			}
//		}
//
//		[MonoTODO]
//		public bool UseDefaultCredentials
//		{
//			get {
//				throw GetMustImplement ();
//			}
//			set {
//				throw GetMustImplement ();
//			}
//		}
//#endif
//		
//		public ICredentials Credentials {
//			get { return credentials; }
//			set { credentials = value; }
//		}
//
		// this is an unvalidated collection, HttpWebRequest is responsable to validate it
		public WebHeaderCollection Headers {
			get {
				if (headers == null)
					headers = new WebHeaderCollection ();

				return headers;
			}
			set { headers = value; }
		}
//		
//		public NameValueCollection QueryString {
//			get {
//				if (queryString == null)
//					queryString = new NameValueCollection ();
//
//				return queryString;
//			}
//			set { queryString = value; }
//		}
//		
		// note: it's public in SL3 beta 1 - but we need it right now
		internal WebHeaderCollection ResponseHeaders {
			get { return responseHeaders; }
		}
//
//#if NET_2_0
		public Encoding Encoding {
			get { return encoding; }
			set {
				if (value == null)
					throw new ArgumentNullException ("value");
				encoding = value;
			}
		}
//
//		public IWebProxy Proxy {
//			get { return proxy; }
//			set { proxy = value; }
//		}
//#endif
//
		public bool IsBusy {
			get { return is_busy; }
		}

		[MonoTODO ("value is unused")]
		public bool AllowReadStreamBuffering {
			get { return allow_read_buffering; }
			set { allow_read_buffering = value; }
		}

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
				is_busy = true;
			}
		}

//		//   DownloadData
//
//		public byte [] DownloadData (string address)
//		{
//#if NET_2_0
//			if (address == null)
//				throw new ArgumentNullException ("address");
//#endif
//
//			return DownloadData (CreateUri (address));
//		}
//
//#if NET_2_0
//		public
//#endif
//		byte [] DownloadData (Uri address)
//		{
//#if NET_2_0
//			if (address == null)
//				throw new ArgumentNullException ("address");
//#endif
//
//			try {
//				SetBusy ();
//				async = false;
//				return DownloadDataCore (address, null);
//			} finally {
//				is_busy = false;
//			}
//		}
//
		Stream DownloadDataCore (Uri address, object userToken)
		{
			WebRequest request = null;

			try {
				request = SetupRequest (address, "GET");
				//WebResponse response = request.GetResponse ();
				IAsyncResult asyncresult = request.BeginGetResponse (null, userToken);
				asyncresult.AsyncWaitHandle.WaitOne ();
				WebResponse response = request.EndGetResponse (asyncresult);
				Stream st = ProcessResponse (response);
				return ReadAll (st, (int) st.Length, userToken);
			} catch (ThreadInterruptedException){
				if (request != null)
					request.Abort ();
				throw;
			} catch (Exception ex) {
				throw new WebException ("An error occurred " +
					"performing a WebClient request.", ex);
			}
		}

//		//   DownloadFile
//
//		public void DownloadFile (string address, string fileName)
//		{
//#if NET_2_0
//			if (address == null)
//				throw new ArgumentNullException ("address");
//#endif
//
//			DownloadFile (CreateUri (address), fileName);
//		}
//
//#if NET_2_0
//		public
//#endif
//		void DownloadFile (Uri address, string fileName)
//		{
//#if NET_2_0
//			if (address == null)
//				throw new ArgumentNullException ("address");
//			if (fileName == null)
//				throw new ArgumentNullException ("fileName");
//#endif
//
//			try {
//				SetBusy ();
//				async = false;
//				DownloadFileCore (address, fileName, null);
//			} catch (Exception ex) {
//				throw new WebException ("An error occurred " +
//					"performing a WebClient request.", ex);
//			} finally {
//				is_busy = false;
//			}
//		}
//
//		void DownloadFileCore (Uri address, string fileName, object userToken)
//		{
//			WebRequest request = null;
//			
//			using (FileStream f = new FileStream (fileName, FileMode.Create)) {
//				try {
//					request = SetupRequest (address);
//					WebResponse response = request.GetResponse ();
//					Stream st = ProcessResponse (response);
//					
//					int cLength = (int) response.ContentLength;
//					int length = (cLength <= -1 || cLength > 32*1024) ? 32*1024 : cLength;
//					byte [] buffer = new byte [length];
//					
//					int nread = 0;
//#if NET_2_0
//					long notify_total = 0;
//#endif					
//					while ((nread = st.Read (buffer, 0, length)) != 0){
//#if NET_2_0
//						if (async){
//							notify_total += nread;
//							OnDownloadProgressChanged (
//								new DownloadProgressChangedEventArgs (notify_total, response.ContentLength, userToken));
//												      
//						}
//#endif
//						f.Write (buffer, 0, nread);
//					}
//				} catch (ThreadInterruptedException){
//					if (request != null)
//						request.Abort ();
//					throw;
//				}
//			}
//		}
//
//		//   OpenRead
//
//		public Stream OpenRead (string address)
//		{
//#if NET_2_0
//			if (address == null)
//				throw new ArgumentNullException ("address");
//#endif
//
//			return OpenRead (CreateUri (address));
//		}
//
//#if NET_2_0
//		public
//#endif
//		Stream OpenRead (Uri address)
//		{
//#if NET_2_0
//			if (address == null)
//				throw new ArgumentNullException ("address");
//#endif
//
//			WebRequest request = null;
//			try {
//				SetBusy ();
//				async = false;
//				request = SetupRequest (address);
//				WebResponse response = request.GetResponse ();
//				return ProcessResponse (response);
//			} catch (Exception ex) {
//				throw new WebException ("An error occurred " +
//					"performing a WebClient request.", ex);
//			} finally {
//				is_busy = false;
//			}
//		}
//
//		//   OpenWrite
//
//		public Stream OpenWrite (string address)
//		{
//#if NET_2_0
//			if (address == null)
//				throw new ArgumentNullException ("address");
//#endif
//
//			return OpenWrite (CreateUri (address));
//		}
//		
//		public Stream OpenWrite (string address, string method)
//		{
//#if NET_2_0
//			if (address == null)
//				throw new ArgumentNullException ("address");
//#endif
//
//			return OpenWrite (CreateUri (address), method);
//		}
//
//#if NET_2_0
//		public
//#endif
//		Stream OpenWrite (Uri address)
//		{
//			return OpenWrite (address, (string) null);
//		}
//
//#if NET_2_0
//		public
//#endif
//		Stream OpenWrite (Uri address, string method)
//		{
//#if NET_2_0
//			if (address == null)
//				throw new ArgumentNullException ("address");
//#endif
//
//			try {
//				SetBusy ();
//				async = false;
//				WebRequest request = SetupRequest (address, method);
//				return request.GetRequestStream ();
//			} catch (Exception ex) {
//				throw new WebException ("An error occurred " +
//					"performing a WebClient request.", ex);
//			} finally {
//				is_busy = false;
//			}
//		}
//
		private string DetermineMethod (Uri address, string method)
		{
			if (method != null)
				return method;

			if (address.Scheme == Uri.UriSchemeFtp)
				return "RETR";
			return "POST";
		}

//		//   UploadData
//
//		public byte [] UploadData (string address, byte [] data)
//		{
//#if NET_2_0
//			if (address == null)
//				throw new ArgumentNullException ("address");
//#endif
//
//			return UploadData (CreateUri (address), data);
//		}
//		
//		public byte [] UploadData (string address, string method, byte [] data)
//		{
//#if NET_2_0
//			if (address == null)
//				throw new ArgumentNullException ("address");
//#endif
//
//			return UploadData (CreateUri (address), method, data);
//		}
//
//#if NET_2_0
//		public
//#endif
//		byte [] UploadData (Uri address, byte [] data)
//		{
//			return UploadData (address, (string) null, data);
//		}
//
//#if NET_2_0
//		public
//#endif
//		byte [] UploadData (Uri address, string method, byte [] data)
//		{
//#if NET_2_0
//			if (address == null)
//				throw new ArgumentNullException ("address");
//			if (data == null)
//				throw new ArgumentNullException ("data");
//#endif
//
//			try {
//				SetBusy ();
//				async = false;
//				return UploadDataCore (address, method, data, null);
//			} catch (Exception ex) {
//				throw new WebException ("An error occurred " +
//					"performing a WebClient request.", ex);
//			} finally {
//				is_busy = false;
//			}
//		}
//
//		byte [] UploadDataCore (Uri address, string method, byte [] data, object userToken)
//		{
//#if ONLY_1_1
//			if (address == null)
//				throw new ArgumentNullException ("address");
//			if (data == null)
//				throw new ArgumentNullException ("data");
//#endif
//
//			WebRequest request = SetupRequest (address, method);
//			try {
//				int contentLength = data.Length;
//				request.ContentLength = contentLength;
//				using (Stream stream = request.GetRequestStream ()) {
//					stream.Write (data, 0, contentLength);
//				}
//				
//				WebResponse response = request.GetResponse ();
//				Stream st = ProcessResponse (response);
//				return ReadAll (st, (int) response.ContentLength, userToken);
//			} catch (ThreadInterruptedException){
//				if (request != null)
//					request.Abort ();
//				throw;
//			}
//		}
//
//		//   UploadFile
//
//		public byte [] UploadFile (string address, string fileName)
//		{
//#if NET_2_0
//			if (address == null)
//				throw new ArgumentNullException ("address");
//#endif
//
//			return UploadFile (CreateUri (address), fileName);
//		}
//
//#if NET_2_0
//		public
//#endif
//		byte [] UploadFile (Uri address, string fileName)
//		{
//			return UploadFile (address, (string) null, fileName);
//		}
//		
//		public byte [] UploadFile (string address, string method, string fileName)
//		{
//			return UploadFile (CreateUri (address), method, fileName);
//		}
//
//#if NET_2_0
//		public
//#endif
//		byte [] UploadFile (Uri address, string method, string fileName)
//		{
//#if NET_2_0
//			if (address == null)
//				throw new ArgumentNullException ("address");
//			if (fileName == null)
//				throw new ArgumentNullException ("fileName");
//#endif
//
//			try {
//				SetBusy ();
//				async = false;
//				return UploadFileCore (address, method, fileName, null);
//			} catch (Exception ex) {
//				throw new WebException ("An error occurred " +
//					"performing a WebClient request.", ex);
//			} finally {
//				is_busy = false;
//			}
//		}
//
//		byte [] UploadFileCore (Uri address, string method, string fileName, object userToken)
//		{
//#if ONLY_1_1
//			if (address == null)
//				throw new ArgumentNullException ("address");
//#endif
//
//			string fileCType = Headers ["Content-Type"];
//			if (fileCType != null) {
//				string lower = fileCType.ToLower ();
//				if (lower.StartsWith ("multipart/"))
//					throw new WebException ("Content-Type cannot be set to a multipart" +
//								" type for this request.");
//			} else {
//				fileCType = "application/octet-stream";
//			}
//
//			string boundary = "------------" + DateTime.Now.Ticks.ToString ("x");
//			Headers ["Content-Type"] = String.Format ("multipart/form-data; boundary={0}", boundary);
//			Stream reqStream = null;
//			Stream fStream = null;
//			byte [] resultBytes = null;
//
//			fileName = Path.GetFullPath (fileName);
//
//			WebRequest request = null;
//			try {
//				fStream = File.OpenRead (fileName);
//				request = SetupRequest (address, method);
//				reqStream = request.GetRequestStream ();
//				byte [] realBoundary = Encoding.ASCII.GetBytes ("--" + boundary + "\r\n");
//				reqStream.Write (realBoundary, 0, realBoundary.Length);
//				string partHeaders = String.Format ("Content-Disposition: form-data; " +
//								    "name=\"file\"; filename=\"{0}\"\r\n" +
//								    "Content-Type: {1}\r\n\r\n",
//								    Path.GetFileName (fileName), fileCType);
//
//				byte [] partHeadersBytes = Encoding.UTF8.GetBytes (partHeaders);
//				reqStream.Write (partHeadersBytes, 0, partHeadersBytes.Length);
//				int nread;
//				byte [] buffer = new byte [4096];
//				while ((nread = fStream.Read (buffer, 0, 4096)) != 0)
//					reqStream.Write (buffer, 0, nread);
//
//				reqStream.WriteByte ((byte) '\r');
//				reqStream.WriteByte ((byte) '\n');
//				reqStream.Write (realBoundary, 0, realBoundary.Length);
//				reqStream.Close ();
//				reqStream = null;
//				WebResponse response = request.GetResponse ();
//				Stream st = ProcessResponse (response);
//				resultBytes = ReadAll (st, (int) response.ContentLength, userToken);
//			} catch (ThreadInterruptedException){
//				if (request != null)
//					request.Abort ();
//				throw;
//			} finally {
//				if (fStream != null)
//					fStream.Close ();
//
//				if (reqStream != null)
//					reqStream.Close ();
//			}
//			
//			return resultBytes;
//		}
//		
//		public byte[] UploadValues (string address, NameValueCollection data)
//		{
//#if NET_2_0
//			if (address == null)
//				throw new ArgumentNullException ("address");
//#endif
//
//			return UploadValues (CreateUri (address), data);
//		}
//		
//		public byte[] UploadValues (string address, string method, NameValueCollection data)
//		{
//#if NET_2_0
//			if (address == null)
//				throw new ArgumentNullException ("address");
//#endif
//
//			return UploadValues (CreateUri (address), method, data);
//		}
//
//#if NET_2_0
//		public
//#endif
//		byte[] UploadValues (Uri address, NameValueCollection data)
//		{
//			return UploadValues (address, (string) null, data);
//		}
//
//#if NET_2_0
//		public
//#endif
//		byte[] UploadValues (Uri address, string method, NameValueCollection data)
//		{
//#if NET_2_0
//			if (address == null)
//				throw new ArgumentNullException ("address");
//			if (data == null)
//				throw new ArgumentNullException ("data");
//#endif
//
//			try {
//				SetBusy ();
//				async = false;
//				return UploadValuesCore (address, method, data, null);
//			} catch (Exception ex) {
//				throw new WebException ("An error occurred " +
//					"performing a WebClient request.", ex);
//			} finally {
//				is_busy = false;
//			}
//		}
//
//		byte[] UploadValuesCore (Uri uri, string method, NameValueCollection data, object userToken)
//		{
//#if ONLY_1_1
//			if (data == null)
//				throw new ArgumentNullException ("data");
//#endif
//
//			string cType = Headers ["Content-Type"];
//			if (cType != null && String.Compare (cType, urlEncodedCType, true) != 0)
//				throw new WebException ("Content-Type header cannot be changed from its default " +
//							"value for this request.");
//
//			Headers ["Content-Type"] = urlEncodedCType;
//			WebRequest request = SetupRequest (uri, method);
//			try {
//				Stream rqStream = request.GetRequestStream ();
//				MemoryStream tmpStream = new MemoryStream ();
//				foreach (string key in data) {
//					byte [] bytes = Encoding.ASCII.GetBytes (key);
//					UrlEncodeAndWrite (tmpStream, bytes);
//					tmpStream.WriteByte ((byte) '=');
//					bytes = Encoding.ASCII.GetBytes (data [key]);
//					UrlEncodeAndWrite (tmpStream, bytes);
//					tmpStream.WriteByte ((byte) '&');
//				}
//				
//				int length = (int) tmpStream.Length;
//				if (length > 0)
//					tmpStream.SetLength (--length); // remove trailing '&'
//				
//				byte [] buf = tmpStream.GetBuffer ();
//				rqStream.Write (buf, 0, length);
//				rqStream.Close ();
//				tmpStream.Close ();
//				
//				WebResponse response = request.GetResponse ();
//				Stream st = ProcessResponse (response);
//				return ReadAll (st, (int) response.ContentLength, userToken);
//			} catch (ThreadInterruptedException) {
//				request.Abort ();
//				throw;
//			}
//		}
//
//#if NET_2_0
//		public string DownloadString (string address)
//		{
//			return encoding.GetString (DownloadData (address));
//		}
//
//		public string DownloadString (Uri address)
//		{
//			return encoding.GetString (DownloadData (address));
//		}
//
//		public string UploadString (string address, string data)
//		{
//			if (address == null)
//				throw new ArgumentNullException ("address");
//			if (data == null)
//				throw new ArgumentNullException ("data");
//
//			byte [] resp = UploadData (address, encoding.GetBytes (data));
//			return encoding.GetString (resp);
//		}
//
//		public string UploadString (string address, string method, string data)
//		{
//			if (address == null)
//				throw new ArgumentNullException ("address");
//			if (data == null)
//				throw new ArgumentNullException ("data");
//
//			byte [] resp = UploadData (address, method, encoding.GetBytes (data));
//			return encoding.GetString (resp);
//		}
//
//		public string UploadString (Uri address, string data)
//		{
//			if (address == null)
//				throw new ArgumentNullException ("address");
//			if (data == null)
//				throw new ArgumentNullException ("data");
//
//			byte [] resp = UploadData (address, encoding.GetBytes (data));
//			return encoding.GetString (resp);
//		}
//
//		public string UploadString (Uri address, string method, string data)
//		{
//			if (address == null)
//				throw new ArgumentNullException ("address");
//			if (data == null)
//				throw new ArgumentNullException ("data");
//
//			byte [] resp = UploadData (address, method, encoding.GetBytes (data));
//			return encoding.GetString (resp);
//		}
//
//		public event DownloadDataCompletedEventHandler DownloadDataCompleted;
//		public event AsyncCompletedEventHandler DownloadFileCompleted;
		public event DownloadProgressChangedEventHandler DownloadProgressChanged;
		public event DownloadStringCompletedEventHandler DownloadStringCompleted;
		public event OpenReadCompletedEventHandler OpenReadCompleted;
		public event OpenWriteCompletedEventHandler OpenWriteCompleted;
//		public event UploadDataCompletedEventHandler UploadDataCompleted;
//		public event UploadFileCompletedEventHandler UploadFileCompleted;
		public event UploadProgressChangedEventHandler UploadProgressChanged;
		public event UploadStringCompletedEventHandler UploadStringCompleted;
//		public event UploadValuesCompletedEventHandler UploadValuesCompleted;
		public event WriteStreamClosedEventHandler WriteStreamClosed;
//#endif
//
//		Uri CreateUri (string address)
//		{
//#if ONLY_1_1
//			try {
//				return MakeUri (address);
//			} catch (Exception ex) {
//				throw new WebException ("An error occurred " +
//					"performing a WebClient request.", ex);
//			}
//#else
//			return MakeUri (address);
//#endif
//		}
//
//		Uri MakeUri (string path)
//		{
//			string query = null;
//			if (queryString != null && queryString.Count != 0) {
//				// This is not the same as UploadValues, because these 'keys' are not
//				// urlencoded here.
//				StringBuilder sb = new StringBuilder ();
//				sb.Append ('?');
//				foreach (string key in queryString)
//					sb.AppendFormat ("{0}={1}&", key, UrlEncode (queryString [key]));
//
//				if (sb.Length != 0) {
//					sb.Length--; // remove trailing '&'
//					query = sb.ToString ();
//				}
//			}
//
//			if (baseAddress == null && query == null) {
//				try {
//					return new Uri (path);
//#if NET_2_0
//				} catch (ArgumentNullException) {
//					path = Path.GetFullPath (path);
//					return new Uri ("file://" + path);
//#endif
//				} catch (UriFormatException) {
//					path = Path.GetFullPath (path);
//					return new Uri ("file://" + path);
//				}
//			}
//
//			if (baseAddress == null)
//				return new Uri (path + query, (query != null));
//
//			if (query == null)
//				return new Uri (baseAddress, path);
//
//			return new Uri (baseAddress, path + query, (query != null));
//		}
//		
//		WebRequest SetupRequest (Uri uri)
//		{
//			WebRequest request = WebRequest.CreateInternal (baseAddress != null ? new Uri (baseAddress, uri) : uri, true);
//			if (Proxy != null)
//				request.Proxy = Proxy;
//			request.Credentials = credentials;

			// Special headers. These are properties of HttpWebRequest.
			// What do we do with other requests differnt from HttpWebRequest?
//			if (headers != null && headers.Count != 0 && (request is HttpWebRequest)) {
//				HttpWebRequest req = (HttpWebRequest) request;
//				string expect = headers ["Expect"];
//				string contentType = headers ["Content-Type"];
//				string accept = headers ["Accept"];
//				string connection = headers ["Connection"];
//				string userAgent = headers ["User-Agent"];
//				string referer = headers ["Referer"];
//				headers.RemoveInternal ("Expect");
//				headers.RemoveInternal ("Content-Type");
//				headers.RemoveInternal ("Accept");
//				headers.RemoveInternal ("Connection");
//				headers.RemoveInternal ("Referer");
//				headers.RemoveInternal ("User-Agent");
//				request.Headers = headers;
//
//				if (expect != null && expect.Length > 0)
//					req.Expect = expect;
//
//				if (accept != null && accept.Length > 0)
//					req.Accept = accept;
//
//				if (contentType != null && contentType.Length > 0)
//					req.ContentType = contentType;
//
//				if (connection != null && connection.Length > 0)
//					req.Connection = connection;
//
//				if (userAgent != null && userAgent.Length > 0)
//					req.UserAgent = userAgent;
//
//				if (referer != null && referer.Length > 0)
//					req.Referer = referer;
//			}
//
//			responseHeaders = null;
//			return request;
//		}

		WebRequest SetupRequest (Uri uri, string method)
		{
			WebRequest request = GetWebRequest (uri);
			request.Method = DetermineMethod (uri, method);
			return request;
		}

		Stream ProcessResponse (WebResponse response)
		{
			responseHeaders = response.Headers;
			HttpWebResponse hwr = (response as HttpWebResponse);
			if (hwr == null)
				throw new NotSupportedException ();

			HttpStatusCode status_code = HttpStatusCode.NotFound;
			Stream s = null;
			try {
				status_code = hwr.StatusCode;
				if (status_code == HttpStatusCode.OK)
					s = response.GetResponseStream ();
			}
			catch (Exception e) {
				throw new WebException ("NotFound", e, WebExceptionStatus.UnknownError, response);
			}
			finally {
				if (status_code != HttpStatusCode.OK)
					throw new WebException ("NotFound", null, WebExceptionStatus.UnknownError, response);
			}
			return s;
		}

		Stream ReadAll (Stream stream, int length, object userToken)
		{
			MemoryStream ms = null;

			bool nolength = (length == -1);
			int size = ((nolength) ? 8192 : length);
			ms = new MemoryStream (size);

			int nread;
			byte [] buffer = new byte [size];
			while ((nread = stream.Read (buffer, 0, buffer.Length)) != 0) {
				ms.Write (buffer, 0, nread);
				if (async){
					OnDownloadProgressChanged (new DownloadProgressChangedEventArgs (nread, length, userToken));
				}
				if (!nolength && ms.Length == length) {
					break;
				}
			}

			ms.Position = 0;
			return ms;
		}

//		string UrlEncode (string str)
//		{
//			StringBuilder result = new StringBuilder ();
//
//			int len = str.Length;
//			for (int i = 0; i < len; i++) {
//				char c = str [i];
//				if (c == ' ')
//					result.Append ('+');
//				else if ((c < '0' && c != '-' && c != '.') ||
//					 (c < 'A' && c > '9') ||
//					 (c > 'Z' && c < 'a' && c != '_') ||
//					 (c > 'z')) {
//					result.Append ('%');
//					int idx = ((int) c) >> 4;
//					result.Append ((char) hexBytes [idx]);
//					idx = ((int) c) & 0x0F;
//					result.Append ((char) hexBytes [idx]);
//				} else {
//					result.Append (c);
//				}
//			}
//
//			return result.ToString ();
//		}
//
//		static void UrlEncodeAndWrite (Stream stream, byte [] bytes)
//		{
//			if (bytes == null)
//				return;
//
//			int len = bytes.Length;
//			if (len == 0)
//				return;
//
//			for (int i = 0; i < len; i++) {
//				char c = (char) bytes [i];
//				if (c == ' ')
//					stream.WriteByte ((byte) '+');
//				else if ((c < '0' && c != '-' && c != '.') ||
//					 (c < 'A' && c > '9') ||
//					 (c > 'Z' && c < 'a' && c != '_') ||
//					 (c > 'z')) {
//					stream.WriteByte ((byte) '%');
//					int idx = ((int) c) >> 4;
//					stream.WriteByte (hexBytes [idx]);
//					idx = ((int) c) & 0x0F;
//					stream.WriteByte (hexBytes [idx]);
//				} else {
//					stream.WriteByte ((byte) c);
//				}
//			}
//		}
//
//#if NET_2_0
		[SecuritySafeCritical]
		public void CancelAsync ()
		{
			lock (this){
				if (async_thread == null)
					return;

				//
				// We first flag things as done, in case the Interrupt hangs
				// or the thread decides to hang in some other way inside the
				// event handlers, or if we are stuck somewhere else.  This
				// ensures that the WebClient object is reusable immediately
				//
				Thread t = async_thread;
				CompleteAsync ();
				t.Interrupt ();
			}
		}

		void CompleteAsync ()
		{
			lock (this){
				is_busy = false;
				async_thread = null;
			}
		}

//		//    DownloadDataAsync
//
//		public void DownloadDataAsync (Uri address)
//		{
//			DownloadDataAsync (address, null);
//		}
//
//		public void DownloadDataAsync (Uri address, object userToken)
//		{
//			if (address == null)
//				throw new ArgumentNullException ("address");
//			
//			lock (this) {
//				SetBusy ();
//				async = true;
//				
//				async_thread = new Thread (delegate (object state) {
//					object [] args = (object []) state;
//					try {
//						byte [] data = DownloadDataCore ((Uri) args [0], args [1]);
//						OnDownloadDataCompleted (
//							new DownloadDataCompletedEventArgs (data, null, false, args [1]));
//					} catch (ThreadInterruptedException){
//						OnDownloadDataCompleted (
//							new DownloadDataCompletedEventArgs (null, null, true, args [1]));
//						throw;
//					} catch (Exception e){
//						OnDownloadDataCompleted (
//							new DownloadDataCompletedEventArgs (null, e, false, args [1]));
//					}
//				});
//				object [] cb_args = new object [] {address, userToken};
//				async_thread.Start (cb_args);
//			}
//		}
//
//		//    DownloadFileAsync
//
//		public void DownloadFileAsync (Uri address, string fileName)
//		{
//			DownloadFileAsync (address, fileName, null);
//		}
//
//		public void DownloadFileAsync (Uri address, string fileName, object userToken)
//		{
//			if (address == null)
//				throw new ArgumentNullException ("address");
//			if (fileName == null)
//				throw new ArgumentNullException ("fileName");
//			
//			lock (this) {
//				SetBusy ();
//				async = true;
//
//				async_thread = new Thread (delegate (object state) {
//					object [] args = (object []) state;
//					try {
//						DownloadFileCore ((Uri) args [0], (string) args [1], args [2]);
//						OnDownloadFileCompleted (
//							new AsyncCompletedEventArgs (null, false, args [2]));
//					} catch (ThreadInterruptedException){
//						OnDownloadFileCompleted (
//							new AsyncCompletedEventArgs (null, true, args [2]));
//					} catch (Exception e){
//						OnDownloadFileCompleted (
//							new AsyncCompletedEventArgs (e, false, args [2]));
//					}});
//				object [] cb_args = new object [] {address, fileName, userToken};
//				async_thread.Start (cb_args);
//			}
//		}
//
//		//    DownloadStringAsync
//
		public void DownloadStringAsync (Uri address)
		{
			DownloadStringAsync (address, null);
		}

		public void DownloadStringAsync (Uri address, object userToken)
		{
			if (address == null)
				throw new ArgumentNullException ("address");

			lock (this) {
				SetBusy ();
				async = true;

				async_thread = new Thread (delegate (object state) {
					object [] args = (object []) state;
					try {
						Stream bdata = DownloadDataCore ((Uri) args [0], args [1]);
						string data;
						using (StreamReader stream = new StreamReader (bdata, encoding, true))
							data = stream.ReadToEnd ();

						OnDownloadStringCompleted (
							new DownloadStringCompletedEventArgs (data, null, false, args [1]));
					} catch (ThreadInterruptedException){
						OnDownloadStringCompleted (
							new DownloadStringCompletedEventArgs (null, null, true, args [1]));
					} catch (Exception e){
						OnDownloadStringCompleted (
							new DownloadStringCompletedEventArgs (null, e, false, args [1]));
					}});
				object [] cb_args = new object [] {address, userToken};
				async_thread.Start (cb_args);
			}
		}

		//    OpenReadAsync

		public void OpenReadAsync (Uri address)
		{
			OpenReadAsync (address, null, false);
		}

		public void OpenReadAsync (Uri address, object userToken)
		{
			OpenReadAsync (address, userToken, false);
		}

		internal void OpenPolicyReadAsync (Uri address, object userToken)
		{
			switch (address.LocalPath) {
			case "/clientaccesspolicy.xml":
			case "/crossdomain.xml":
				OpenReadAsync (address, userToken, true);
				break;
			default:
				throw new SecurityException ();
			}
		}

		private void OpenReadAsync (Uri address, object userToken, bool policy)
		{
			if (address == null)
				throw new ArgumentNullException ("address");

			lock (this) {
				SetBusy ();
				async = true;

				async_thread = new Thread (delegate (object state) {
					object [] args = (object []) state;
					WebRequest request = null;
					try {
						request = SetupRequest (address, "GET");
						IAsyncResult asyncresult = request.BeginGetResponse (null, userToken, policy);
						asyncresult.AsyncWaitHandle.WaitOne ();
						WebResponse response = request.EndGetResponse (asyncresult);
						Stream stream = ProcessResponse (response);
						OnOpenReadCompleted (
							new OpenReadCompletedEventArgs (stream, address, null, false, args [1]));
					} catch (ThreadInterruptedException){
						if (request != null)
							request.Abort ();

						OnOpenReadCompleted (new OpenReadCompletedEventArgs (null, null, null, true, args [1]));
					} catch (Exception e){
						OnOpenReadCompleted (new OpenReadCompletedEventArgs (null, null, e, false, args [1]));
					} });
				object [] cb_args = new object [] {address, userToken};
				async_thread.Start (cb_args);
			}
		}

//		//    OpenWriteAsync
//
		public void OpenWriteAsync (Uri address)
		{
			OpenWriteAsync (address, null);
		}

		public void OpenWriteAsync (Uri address, string method)
		{
			OpenWriteAsync (address, method, null);
		}

		public void OpenWriteAsync (Uri address, string method, object userToken)
		{
			if (address == null)
				throw new ArgumentNullException ("address");

			throw new NotImplementedException ();
//
//			lock (this) {
//				SetBusy ();
//				async = true;
//
//				async_thread = new Thread (delegate (object state) {
//					object [] args = (object []) state;
//					WebRequest request = null;
//					try {
//						request = SetupRequest ((Uri) args [0], (string) args [1]);
//						Stream stream = request.GetRequestStream ();
//						OnOpenWriteCompleted (
//							new OpenWriteCompletedEventArgs (stream, null, false, args [2]));
//					} catch (ThreadInterruptedException){
//						if (request != null)
//							request.Abort ();
//						OnOpenWriteCompleted (
//							new OpenWriteCompletedEventArgs (null, null, true, args [2]));
//					} catch (Exception e){
//						OnOpenWriteCompleted (
//							new OpenWriteCompletedEventArgs (null, e, false, args [2]));
//					}});
//				object [] cb_args = new object [] {address, method, userToken};
//				async_thread.Start (cb_args);
//			}
		}
//
//		//    UploadDataAsync
//
//		public void UploadDataAsync (Uri address, byte [] data)
//		{
//			UploadDataAsync (address, null, data);
//		}
//
//		public void UploadDataAsync (Uri address, string method, byte [] data)
//		{
//			UploadDataAsync (address, method, data, null);
//		}
//
//		public void UploadDataAsync (Uri address, string method, byte [] data, object userToken)
//		{
//			if (address == null)
//				throw new ArgumentNullException ("address");
//			if (data == null)
//				throw new ArgumentNullException ("data");
//			
//			lock (this) {
//				SetBusy ();
//				async = true;
//
//				async_thread = new Thread (delegate (object state) {
//					object [] args = (object []) state;
//					byte [] data2;
//
//					try {
//						data2 = UploadDataCore ((Uri) args [0], (string) args [1], (byte []) args [2], args [3]);
//					
//						OnUploadDataCompleted (
//							new UploadDataCompletedEventArgs (data2, null, false, args [3]));
//					} catch (ThreadInterruptedException){
//						OnUploadDataCompleted (
//							new UploadDataCompletedEventArgs (null, null, true, args [3]));
//					} catch (Exception e){
//						OnUploadDataCompleted (
//							new UploadDataCompletedEventArgs (null, e, false, args [3]));
//					}});
//				object [] cb_args = new object [] {address, method, data,  userToken};
//				async_thread.Start (cb_args);
//			}
//		}
//
//		//    UploadFileAsync
//
//		public void UploadFileAsync (Uri address, string fileName)
//		{
//			UploadFileAsync (address, null, fileName);
//		}
//
//		public void UploadFileAsync (Uri address, string method, string fileName)
//		{
//			UploadFileAsync (address, method, fileName, null);
//		}
//
//		public void UploadFileAsync (Uri address, string method, string fileName, object userToken)
//		{
//			if (address == null)
//				throw new ArgumentNullException ("address");
//			if (fileName == null)
//				throw new ArgumentNullException ("fileName");
//
//			lock (this) {
//				SetBusy ();
//				async = true;
//
//				async_thread = new Thread (delegate (object state) {
//					object [] args = (object []) state;
//					byte [] data;
//
//					try {
//						data = UploadFileCore ((Uri) args [0], (string) args [1], (string) args [2], args [3]);
//						OnUploadFileCompleted (
//							new UploadFileCompletedEventArgs (data, null, false, args [3]));
//					} catch (ThreadInterruptedException){
//						OnUploadFileCompleted (
//							new UploadFileCompletedEventArgs (null, null, true, args [3]));
//					} catch (Exception e){
//						OnUploadFileCompleted (
//							new UploadFileCompletedEventArgs (null, e, false, args [3]));
//					}});
//				object [] cb_args = new object [] {address, method, fileName,  userToken};
//				async_thread.Start (cb_args);
//			}
//		}
//
//		//    UploadStringAsync
//
		public void UploadStringAsync (Uri address, string data)
		{
			UploadStringAsync (address, null, data);
		}

		public void UploadStringAsync (Uri address, string method, string data)
		{
			UploadStringAsync (address, method, data, null);
		}

		public void UploadStringAsync (Uri address, string method, string data, object userToken)
		{
			if (address == null)
				throw new ArgumentNullException ("address");
			if (data == null)
				throw new ArgumentNullException ("data");

			throw new NotImplementedException ();
//			lock (this) {
//				SetBusy ();
//				async = true;
//				
//				async_thread = new Thread (delegate (object state) {
//					object [] args = (object []) state;
//
//					try {
//						string data2 = UploadString ((Uri) args [0], (string) args [1], (string) args [2]);
//						OnUploadStringCompleted (
//							new UploadStringCompletedEventArgs (data2, null, false, args [3]));
//					} catch (ThreadInterruptedException){
//						OnUploadStringCompleted (
//							new UploadStringCompletedEventArgs (null, null, true, args [3]));
//					} catch (Exception e){
//						OnUploadStringCompleted (
//							new UploadStringCompletedEventArgs (null, e, false, args [3]));
//					}});
//				object [] cb_args = new object [] {address, method, data, userToken};
//				async_thread.Start (cb_args);
//			}
		}
//
//		//    UploadValuesAsync
//
//		public void UploadValuesAsync (Uri address, NameValueCollection values)
//		{
//			UploadValuesAsync (address, null, values);
//		}
//
//		public void UploadValuesAsync (Uri address, string method, NameValueCollection values)
//		{
//			UploadValuesAsync (address, method, values, null);
//		}
//
//		public void UploadValuesAsync (Uri address, string method, NameValueCollection values, object userToken)
//		{
//			if (address == null)
//				throw new ArgumentNullException ("address");
//			if (values == null)
//				throw new ArgumentNullException ("values");
//
//			lock (this) {
//				CheckBusy ();
//				async = true;
//
//				async_thread = new Thread (delegate (object state) {
//					object [] args = (object []) state;
//					try {
//						byte [] data = UploadValuesCore ((Uri) args [0], (string) args [1], (NameValueCollection) args [2], args [3]);
//						OnUploadValuesCompleted (
//							new UploadValuesCompletedEventArgs (data, null, false, args [3]));
//					} catch (ThreadInterruptedException){
//						OnUploadValuesCompleted (
//							new UploadValuesCompletedEventArgs (null, null, true, args [3]));
//					} catch (Exception e){
//						OnUploadValuesCompleted (
//							new UploadValuesCompletedEventArgs (null, e, false, args [3]));
//					}});
//				object [] cb_args = new object [] {address, method, values,  userToken};
//				async_thread.Start (cb_args);
//			}
//		}
//
//		protected virtual void OnDownloadDataCompleted (DownloadDataCompletedEventArgs args)
//		{
//			CompleteAsync ();
//			if (DownloadDataCompleted != null)
//				DownloadDataCompleted (this, args);
//		}
//
//		protected virtual void OnDownloadFileCompleted (AsyncCompletedEventArgs args)
//		{
//			CompleteAsync ();
//			if (DownloadFileCompleted != null)
//				DownloadFileCompleted (this, args);
//		}
//
		protected virtual void OnDownloadProgressChanged (DownloadProgressChangedEventArgs e)
		{
			if (DownloadProgressChanged != null) {
				DownloadProgressChanged (this, e);
			}
		}
		
		private object callback_args;

		protected virtual void OnOpenReadCompleted (OpenReadCompletedEventArgs args)
		{
			CompleteAsync ();
			if (OpenReadCompleted != null) {
				ManualResetEvent wait_event = new ManualResetEvent (false);
				GSourceFunc callback = (GSourceFunc) delegate (IntPtr ctx)
				{
					try {
						OpenReadCompleted (this, (OpenReadCompletedEventArgs) callback_args);
					} catch (Exception ex) {
						try {
							Console.WriteLine ("Unhandled exception: {0}", ex);
						} catch {
						}
					}
					try {
						wait_event.Set ();
					} catch (Exception ex) {
						try {
							Console.WriteLine ("Unhandled exception: {0}", ex);
						} catch {
						}
					}
					return false;
				};
				callback_args = args;

				g_timeout_add (0, callback, IntPtr.Zero);

				wait_event.WaitOne ();
				GC.KeepAlive (callback);
			}
		}

		public delegate bool GSourceFunc (IntPtr data);

		[DllImport ("moon")]
		static extern uint g_timeout_add (uint delay, GSourceFunc callback, IntPtr data);

		protected virtual void OnDownloadStringCompleted (DownloadStringCompletedEventArgs args)
		{
			CompleteAsync ();
			if (DownloadStringCompleted != null) {
				ManualResetEvent wait_event = new ManualResetEvent (false);
				GSourceFunc callback = (GSourceFunc) delegate (IntPtr ctx) 
				{
					try {
						DownloadStringCompleted (this, (DownloadStringCompletedEventArgs) callback_args);
					} catch (Exception ex) {
						try {
							Console.WriteLine ("Unhandled exception: {0}", ex);
						} catch {
						}
					}
					try {
						wait_event.Set ();
					} catch (Exception ex) {
						try {
							Console.WriteLine ("Unhandled exception: {0}", ex);
						} catch {
						}
					}
					return false;
				};
				callback_args = args;

				g_timeout_add (0, callback, IntPtr.Zero);

				wait_event.WaitOne ();
				GC.KeepAlive (callback);
			}
		}


		protected virtual void OnOpenWriteCompleted (OpenWriteCompletedEventArgs args)
		{
			CompleteAsync ();
			if (OpenWriteCompleted != null)
				OpenWriteCompleted (this, args);
		}
//
//		protected virtual void OnUploadDataCompleted (UploadDataCompletedEventArgs args)
//		{
//			CompleteAsync ();
//			if (UploadDataCompleted != null)
//				UploadDataCompleted (this, args);
//		}
//
//		protected virtual void OnUploadFileCompleted (UploadFileCompletedEventArgs args)
//		{
//			CompleteAsync ();
//			if (UploadFileCompleted != null)
//				UploadFileCompleted (this, args);
//		}
//
		protected virtual void OnUploadProgressChanged (UploadProgressChangedEventArgs e)
		{
			if (UploadProgressChanged != null)
				UploadProgressChanged (this, e);
		}

		protected virtual void OnUploadStringCompleted (UploadStringCompletedEventArgs args)
		{
			CompleteAsync ();
			if (UploadStringCompleted != null)
				UploadStringCompleted (this, args);
		}
//
//		protected virtual void OnUploadValuesCompleted (UploadValuesCompletedEventArgs args)
//		{
//			CompleteAsync ();
//			if (UploadValuesCompleted != null)
//				UploadValuesCompleted (this, args);
//		}
//
		protected virtual void OnWriteStreamClosed (WriteStreamClosedEventArgs e)
		{
			throw new NotImplementedException ();
		}

		protected virtual WebRequest GetWebRequest (Uri address)
		{
			if (address == null)
				throw new ArgumentNullException ("address");

			// if the URI is relative then we use our base address URI to make an absolute one
			Uri uri = address.IsAbsoluteUri ? address : new Uri (baseAddress, address);

			WebRequest request = WebRequest.Create (uri);

			request.SetupProgressDelegate ((ProgressChangedDelegate) delegate (long read, long length, object state) {
				OnDownloadProgressChanged (new DownloadProgressChangedEventArgs (read, length, state));
			});
			return request;
		}
//
//		protected virtual WebResponse GetWebResponse (WebRequest request)
//		{
//			return request.GetResponse ();
//		}
//
		protected virtual WebResponse GetWebResponse (WebRequest request, IAsyncResult result)
		{
			return request.EndGetResponse (result);
		}
//#endif
#endif
	}
}


