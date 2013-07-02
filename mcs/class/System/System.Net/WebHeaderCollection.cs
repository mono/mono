//
// System.Net.WebHeaderCollection
//
// Authors:
// 	Lawrence Pit (loz@cable.a2000.nl)
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//      Miguel de Icaza (miguel@novell.com)
//	Marek Safar (marek.safar@gmail.com)
//
// Copyright 2003 Ximian, Inc. (http://www.ximian.com)
// Copyright 2007 Novell, Inc. (http://www.novell.com)
// Copyright (C) 2011 Xamarin Inc (http://www.xamarin.com)
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
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
    
// See RFC 2068 par 4.2 Message Headers
    
namespace System.Net 
{
	[Serializable]
	[ComVisible(true)]
	public class WebHeaderCollection : NameValueCollection, ISerializable {
		[Flags]
		internal enum HeaderInfo
		{
			Request = 1,
			Response = 1 << 1,
			MultiValue = 1 << 10
		}

		static readonly bool[] allowed_chars = {
			false, false, false, false, false, false, false, false, false, false, false, false, false, false,
			false, false, false, false, false, false, false, false, false, false, false, false, false, false,
			false, false, false, false, false, true, false, true, true, true, true, false, false, false, true,
			true, false, true, true, false, true, true, true, true, true, true, true, true, true, true, false,
			false, false, false, false, false, false, true, true, true, true, true, true, true, true, true,
			true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true,
			false, false, false, true, true, true, true, true, true, true, true, true, true, true, true, true,
			true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true,
			false, true, false
		};

		static readonly Dictionary<string, HeaderInfo> headers;
		HeaderInfo? headerRestriction;
		HeaderInfo? headerConsistency;
		
		static WebHeaderCollection () 
		{
			headers = new Dictionary<string, HeaderInfo> (StringComparer.OrdinalIgnoreCase) {
				{ "Allow", HeaderInfo.MultiValue },
				{ "Accept", HeaderInfo.Request | HeaderInfo.MultiValue },
				{ "Accept-Charset", HeaderInfo.MultiValue },
				{ "Accept-Encoding", HeaderInfo.MultiValue },
				{ "Accept-Language", HeaderInfo.MultiValue },
				{ "Accept-Ranges", HeaderInfo.MultiValue },
				{ "Authorization", HeaderInfo.MultiValue },
				{ "Cache-Control", HeaderInfo.MultiValue },
				{ "Cookie", HeaderInfo.MultiValue },
				{ "Connection", HeaderInfo.Request | HeaderInfo.MultiValue },
				{ "Content-Encoding", HeaderInfo.MultiValue },
				{ "Content-Length", HeaderInfo.Request | HeaderInfo.Response },
				{ "Content-Type", HeaderInfo.Request },
				{ "Content-Language", HeaderInfo.MultiValue },
				{ "Date", HeaderInfo.Request },
				{ "Expect", HeaderInfo.Request | HeaderInfo.MultiValue},
				{ "Host", HeaderInfo.Request },
				{ "If-Match", HeaderInfo.MultiValue },
				{ "If-Modified-Since", HeaderInfo.Request },
				{ "If-None-Match", HeaderInfo.MultiValue },
				{ "Keep-Alive", HeaderInfo.Response },
				{ "Pragma", HeaderInfo.MultiValue },
				{ "Proxy-Authenticate", HeaderInfo.MultiValue },
				{ "Proxy-Authorization", HeaderInfo.MultiValue },
				{ "Proxy-Connection", HeaderInfo.Request | HeaderInfo.MultiValue },
				{ "Range", HeaderInfo.Request | HeaderInfo.MultiValue },
				{ "Referer", HeaderInfo.Request },
				{ "Set-Cookie", HeaderInfo.MultiValue },
				{ "Set-Cookie2", HeaderInfo.MultiValue },
				{ "TE", HeaderInfo.MultiValue },
				{ "Trailer", HeaderInfo.MultiValue },
				{ "Transfer-Encoding", HeaderInfo.Request | HeaderInfo.Response | HeaderInfo.MultiValue },
				{ "Upgrade", HeaderInfo.MultiValue },
				{ "User-Agent", HeaderInfo.Request },
				{ "Vary", HeaderInfo.MultiValue },
				{ "Via", HeaderInfo.MultiValue },
				{ "Warning", HeaderInfo.MultiValue },
				{ "WWW-Authenticate", HeaderInfo.Response | HeaderInfo. MultiValue }
			};
		}
		
		// Constructors
		
		public WebHeaderCollection ()
		{
		}
		
		protected WebHeaderCollection (SerializationInfo serializationInfo, 
					       StreamingContext streamingContext)
		{
			int count;

			try {
				count = serializationInfo.GetInt32("Count");
				for (int i = 0; i < count; i++) 
					this.Add (serializationInfo.GetString (i.ToString ()),
						  serializationInfo.GetString ((count + i).ToString ()));
			} catch (SerializationException){
				count = serializationInfo.GetInt32("count");
				for (int i = 0; i < count; i++) 
					this.Add (serializationInfo.GetString ("k" + i),
						  serializationInfo.GetString ("v" + i));
			}
			
		}

		internal WebHeaderCollection (HeaderInfo headerRestriction)
		{
			this.headerRestriction = headerRestriction;
		}		
		
		// Methods
		
		public void Add (string header)
		{
			if (header == null)
				throw new ArgumentNullException ("header");
			int pos = header.IndexOf (':');
			if (pos == -1)
				throw new ArgumentException ("no colon found", "header");

			this.Add (header.Substring (0, pos), header.Substring (pos + 1));
		}
		
		public override void Add (string name, string value)
		{
			if (name == null)
				throw new ArgumentNullException ("name");

			CheckRestrictedHeader (name);
			this.AddWithoutValidate (name, value);
		}

		protected void AddWithoutValidate (string headerName, string headerValue)
		{
			if (!IsHeaderName (headerName))
				throw new ArgumentException ("invalid header name: " + headerName, "headerName");
			if (headerValue == null)
				headerValue = String.Empty;
			else
				headerValue = headerValue.Trim ();
			if (!IsHeaderValue (headerValue))
				throw new ArgumentException ("invalid header value: " + headerValue, "headerValue");
			
			AddValue (headerName, headerValue);
		}
			
		internal void AddValue (string headerName, string headerValue)
		{
			base.Add (headerName, headerValue);			
		}

		internal string [] GetValues_internal (string header, bool split)
		{
			if (header == null)
				throw new ArgumentNullException ("header");

			string [] values = base.GetValues (header);
			if (values == null || values.Length == 0)
				return null;

			if (split && IsMultiValue (header)) {
				List<string> separated = null;
				foreach (var value in values) {
					if (value.IndexOf (',') < 0)
						continue;

					if (separated == null) {
						separated = new List<string> (values.Length + 1);
						foreach (var v in values) {
							if (v == value)
								break;

							separated.Add (v);
						}
					}

					var slices = value.Split (',');
					var slices_length = slices.Length;
					if (value[value.Length - 1] == ',')
						--slices_length;

					for (int i = 0; i < slices_length; ++i ) {
						separated.Add (slices[i].Trim ());
					}
				}

				if (separated != null)
					return separated.ToArray ();
			}

			return values;
		}

		public override string [] GetValues (string header)
		{
			return GetValues_internal (header, true);
		}

		public override string[] GetValues (int index)
		{
			string[] values = base.GetValues (index);

			if (values == null || values.Length == 0) {
				return null;
			}
			
			return values;
		}

		public static bool IsRestricted (string headerName)
		{
			return IsRestricted (headerName, false);
		}

		public static bool IsRestricted (string headerName, bool response)
		{
			if (headerName == null)
				throw new ArgumentNullException ("headerName");

			if (headerName.Length == 0)
				throw new ArgumentException ("empty string", "headerName");

			if (!IsHeaderName (headerName))
				throw new ArgumentException ("Invalid character in header");

			HeaderInfo info;
			if (!headers.TryGetValue (headerName, out info))
				return false;

			var flag = response ? HeaderInfo.Response : HeaderInfo.Request;
			return (info & flag) != 0;
		}

		public override void OnDeserialization (object sender)
		{
		}

		public override void Remove (string name)
		{
			if (name == null)
				throw new ArgumentNullException ("name");

			CheckRestrictedHeader (name);
			base.Remove (name);
		}

		public override void Set (string name, string value)
		{
			if (name == null)
				throw new ArgumentNullException ("name");
			if (!IsHeaderName (name))
				throw new ArgumentException ("invalid header name");
			if (value == null)
				value = String.Empty;
			else
				value = value.Trim ();
			if (!IsHeaderValue (value))
				throw new ArgumentException ("invalid header value");

			CheckRestrictedHeader (name);
			base.Set (name, value);			
		}

		public byte[] ToByteArray ()
		{
			return Encoding.UTF8.GetBytes(ToString ());
		}

		internal string ToStringMultiValue ()
		{
			StringBuilder sb = new StringBuilder();

			int count = base.Count;
			for (int i = 0; i < count ; i++) {
				string key = GetKey (i);
				if (IsMultiValue (key)) {
					foreach (string v in GetValues (i)) {
						sb.Append (key)
						  .Append (": ")
						  .Append (v)
						  .Append ("\r\n");
					}
				} else {
					sb.Append (key)
					  .Append (": ")
					  .Append (Get (i))
					  .Append ("\r\n");
				}
			 }
			return sb.Append("\r\n").ToString();
		}

		public override string ToString ()
		{
			StringBuilder sb = new StringBuilder();

			int count = base.Count;
			for (int i = 0; i < count ; i++)
				sb.Append (GetKey (i))
				  .Append (": ")
				  .Append (Get (i))
				  .Append ("\r\n");

			return sb.Append("\r\n").ToString();
		}
#if !TARGET_JVM
		void ISerializable.GetObjectData (SerializationInfo serializationInfo,
						  StreamingContext streamingContext)
		{
			GetObjectData (serializationInfo, streamingContext);
		}
#endif
		public override void GetObjectData (SerializationInfo serializationInfo, StreamingContext streamingContext)
		{
			int count = base.Count;
			serializationInfo.AddValue ("Count", count);
			for (int i = 0; i < count; i++) {
				serializationInfo.AddValue (i.ToString (), GetKey (i));
				serializationInfo.AddValue ((count + i).ToString (), Get (i));
			}
		}

		public override string[] AllKeys {
			get {
				return base.AllKeys;
			}
		}
		
		public override int Count {
			get {
				return base.Count;
			}
		}

		public override KeysCollection Keys {
			get {
				return base.Keys;
			}
		}

		public override string Get (int index)
		{
			return base.Get (index);
		}
		
		public override string Get (string name)
		{
			return base.Get (name);
		}
		
		public override string GetKey (int index)
		{
			return base.GetKey (index);
		}

		public void Add (HttpRequestHeader header, string value)
		{
			Add (RequestHeaderToString (header), value);
		}

		public void Remove (HttpRequestHeader header)
		{
			Remove (RequestHeaderToString (header));
		}

		public void Set (HttpRequestHeader header, string value)
		{
			Set (RequestHeaderToString (header), value);
		}

		public void Add (HttpResponseHeader header, string value)
		{
			Add (ResponseHeaderToString (header), value);
		}

		public void Remove (HttpResponseHeader header)
		{
			Remove (ResponseHeaderToString (header));
		}

		public void Set (HttpResponseHeader header, string value)
		{
			Set (ResponseHeaderToString (header), value);
		}

		public string this [HttpRequestHeader header] {
			get {
				return Get (RequestHeaderToString (header));
			}
			
			set {
				Set (header, value);
			}
		}

		public string this [HttpResponseHeader header] {
			get {
				return Get (ResponseHeaderToString (header));
			}

			set {
				Set (header, value);
			}
		}

		public override void Clear ()
		{
			base.Clear ();
		}

		public override IEnumerator GetEnumerator ()
		{
			return base.GetEnumerator ();
		}

		// Internal Methods
		
		// With this we don't check for invalid characters in header. See bug #55994.
		internal void SetInternal (string header)
		{
			int pos = header.IndexOf (':');
			if (pos == -1)
				throw new ArgumentException ("no colon found", "header");				

			SetInternal (header.Substring (0, pos), header.Substring (pos + 1));
		}

		internal void SetInternal (string name, string value)
		{
			if (value == null)
				value = String.Empty;
			else
				value = value.Trim ();
			if (!IsHeaderValue (value))
				throw new ArgumentException ("invalid header value");

			if (IsMultiValue (name)) {
				base.Add (name, value);
			} else {
				base.Remove (name);
				base.Set (name, value);	
			}
		}

		internal void RemoveAndAdd (string name, string value)
		{
			if (value == null)
				value = String.Empty;
			else
				value = value.Trim ();

			base.Remove (name);
			base.Set (name, value);
		}

		internal void RemoveInternal (string name)
		{
			if (name == null)
				throw new ArgumentNullException ("name");
			base.Remove (name);
		}		
		
		// Private Methods

		string RequestHeaderToString (HttpRequestHeader value)
		{
			CheckHeaderConsistency (HeaderInfo.Request);

			switch (value) {
			case HttpRequestHeader.CacheControl:
				return "Cache-Control";
			case HttpRequestHeader.Connection:
				return "Connection";
			case HttpRequestHeader.Date:
				return "Date";
			case HttpRequestHeader.KeepAlive:
				return "Keep-Alive";
			case HttpRequestHeader.Pragma:
				return "Pragma";
			case HttpRequestHeader.Trailer:
				return "Trailer";
			case HttpRequestHeader.TransferEncoding:
				return "Transfer-Encoding";
			case HttpRequestHeader.Upgrade:
				return "Upgrade";
			case HttpRequestHeader.Via:
				return "Via";
			case HttpRequestHeader.Warning:
				return "Warning";
			case HttpRequestHeader.Allow:
				return "Allow";
			case HttpRequestHeader.ContentLength:
				return "Content-Length";
			case HttpRequestHeader.ContentType:
				return "Content-Type";
			case HttpRequestHeader.ContentEncoding:
				return "Content-Encoding";
			case HttpRequestHeader.ContentLanguage:
				return "Content-Language";
			case HttpRequestHeader.ContentLocation:
				return "Content-Location";
			case HttpRequestHeader.ContentMd5:
				return "Content-MD5";
			case HttpRequestHeader.ContentRange:
				return "Content-Range";
			case HttpRequestHeader.Expires:
				return "Expires";
			case HttpRequestHeader.LastModified:
				return "Last-Modified";
			case HttpRequestHeader.Accept:
				return "Accept";
			case HttpRequestHeader.AcceptCharset:
				return "Accept-Charset";
			case HttpRequestHeader.AcceptEncoding:
				return "Accept-Encoding";
			case HttpRequestHeader.AcceptLanguage:
				return "accept-language";
			case HttpRequestHeader.Authorization:
				return "Authorization";
			case HttpRequestHeader.Cookie:
				return "Cookie";
			case HttpRequestHeader.Expect:
				return "Expect";
			case HttpRequestHeader.From:
				return "From";
			case HttpRequestHeader.Host:
				return "Host";
			case HttpRequestHeader.IfMatch:
				return "If-Match";
			case HttpRequestHeader.IfModifiedSince:
				return "If-Modified-Since";
			case HttpRequestHeader.IfNoneMatch:
				return "If-None-Match";
			case HttpRequestHeader.IfRange:
				return "If-Range";
			case HttpRequestHeader.IfUnmodifiedSince:
				return "If-Unmodified-Since";
			case HttpRequestHeader.MaxForwards:
				return "Max-Forwards";
			case HttpRequestHeader.ProxyAuthorization:
				return "Proxy-Authorization";
			case HttpRequestHeader.Referer:
				return "Referer";
			case HttpRequestHeader.Range:
				return "Range";
			case HttpRequestHeader.Te:
				return "TE";
			case HttpRequestHeader.Translate:
				return "Translate";
			case HttpRequestHeader.UserAgent:
				return "User-Agent";
			default:
				throw new InvalidOperationException ();
			}
		}

		string ResponseHeaderToString (HttpResponseHeader value)
		{
			CheckHeaderConsistency (HeaderInfo.Response);

			switch (value) {
			case HttpResponseHeader.CacheControl:
				return "Cache-Control";
			case HttpResponseHeader.Connection:
				return "Connection";
			case HttpResponseHeader.Date:
				return "Date";
			case HttpResponseHeader.KeepAlive:
				return "Keep-Alive";
			case HttpResponseHeader.Pragma:
				return "Pragma";
			case HttpResponseHeader.Trailer:
				return "Trailer";
			case HttpResponseHeader.TransferEncoding:
				return "Transfer-Encoding";
			case HttpResponseHeader.Upgrade:
				return "Upgrade";
			case HttpResponseHeader.Via:
				return "Via";
			case HttpResponseHeader.Warning:
				return "Warning";
			case HttpResponseHeader.Allow:
				return "Allow";
			case HttpResponseHeader.ContentLength:
				return "Content-Length";
			case HttpResponseHeader.ContentType:
				return "Content-Type";
			case HttpResponseHeader.ContentEncoding:
				return "Content-Encoding";
			case HttpResponseHeader.ContentLanguage:
				return "Content-Language";
			case HttpResponseHeader.ContentLocation:
				return "Content-Location";
			case HttpResponseHeader.ContentMd5:
				return "Content-MD5";
			case HttpResponseHeader.ContentRange:
				return "Content-Range";
			case HttpResponseHeader.Expires:
				return "Expires";
			case HttpResponseHeader.LastModified:
				return "Last-Modified";
			case HttpResponseHeader.AcceptRanges:
				return "Accept-Ranges";
			case HttpResponseHeader.Age:
				return "Age";
			case HttpResponseHeader.ETag:
				return "ETag";
			case HttpResponseHeader.Location:
				return "Location";
			case HttpResponseHeader.ProxyAuthenticate:
				return "Proxy-Authenticate";
			case HttpResponseHeader.RetryAfter:
				return "Retry-After";
			case HttpResponseHeader.Server:
				return "Server";
			case HttpResponseHeader.SetCookie:
				return "Set-Cookie";
			case HttpResponseHeader.Vary:
				return "Vary";
			case HttpResponseHeader.WwwAuthenticate:
				return "WWW-Authenticate";
			default:
				throw new InvalidOperationException ();
			}
		}

		void CheckRestrictedHeader (string headerName)
		{
			if (!headerRestriction.HasValue)
				return;

			HeaderInfo info;
			if (!headers.TryGetValue (headerName, out info))
				return;

			if ((info & headerRestriction.Value) != 0)
				throw new ArgumentException ("This header must be modified with the appropiate property.");
		}

		void CheckHeaderConsistency (HeaderInfo value)
		{
			if (!headerConsistency.HasValue) {
				headerConsistency = value;
				return;
			}

			if ((headerConsistency & value) == 0)
				throw new InvalidOperationException ();
		}
		
		internal static bool IsMultiValue (string headerName)
		{
			if (headerName == null)
				return false;

			HeaderInfo info;
			return headers.TryGetValue (headerName, out info) && (info & HeaderInfo.MultiValue) != 0;
		}		
		
		internal static bool IsHeaderValue (string value)
		{
			// TEXT any 8 bit value except CTL's (0-31 and 127)
			//      but including \r\n space and \t
			//      after a newline at least one space or \t must follow
			//      certain header fields allow comments ()
				
			int len = value.Length;
			for (int i = 0; i < len; i++) {			
				char c = value [i];
				if (c == 127)
					return false;
				if (c < 0x20 && (c != '\r' && c != '\n' && c != '\t'))
					return false;
				if (c == '\n' && ++i < len) {
					c = value [i];
					if (c != ' ' && c != '\t')
						return false;
				}
			}
			
			return true;
		}
		
		internal static bool IsHeaderName (string name)
		{
			if (name == null || name.Length == 0)
				return false;

			int len = name.Length;
			for (int i = 0; i < len; i++) {			
				char c = name [i];
				if (c > 126 || !allowed_chars [c])
					return false;
			}
			
			return true;
		}
	}
}
