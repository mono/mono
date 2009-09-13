#define EMBEDDED_IN_1_0

//
// System.Net.WebHeaderCollection
//
// Authors:
// 	Lawrence Pit (loz@cable.a2000.nl)
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//      Miguel de Icaza (miguel@novell.com)
//
// Copyright 2003 Ximian, Inc. (http://www.ximian.com)
// Copyright 2007 Novell, Inc. (http://www.novell.com)
//
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
#if EMBEDDED_IN_1_0
using System.Collections.Generic;
#endif
using System.Collections.Specialized;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
    
// See RFC 2068 par 4.2 Message Headers
    
using System; using System.Net; namespace MonoHttp 
{
	[Serializable]
	[ComVisible(true)]
	internal class WebHeaderCollection : NameValueCollection, ISerializable
	{
		private static readonly Hashtable restricted;
		private static readonly Hashtable multiValue;
		private bool internallyCreated = false;
		
		// Static Initializer
		
		static WebHeaderCollection () 
		{
			// the list of restricted header names as defined 
			// by the ms.net spec
			restricted = new Hashtable (CaseInsensitiveHashCodeProvider.DefaultInvariant,
						    CaseInsensitiveComparer.DefaultInvariant);

			restricted.Add ("accept", true);
			restricted.Add ("connection", true);
			restricted.Add ("content-length", true);
			restricted.Add ("content-type", true);
			restricted.Add ("date", true);
			restricted.Add ("expect", true);
			restricted.Add ("host", true);
			restricted.Add ("if-modified-since", true);
			restricted.Add ("range", true);
			restricted.Add ("referer", true);
			restricted.Add ("transfer-encoding", true);
			restricted.Add ("user-agent", true);			
			restricted.Add ("proxy-connection", true);			

			//
			// see par 14 of RFC 2068 to see which header names
			// accept multiple values each separated by a comma
			multiValue = new Hashtable (CaseInsensitiveHashCodeProvider.DefaultInvariant,
						    CaseInsensitiveComparer.DefaultInvariant);

			multiValue.Add ("accept", true);
			multiValue.Add ("accept-charset", true);
			multiValue.Add ("accept-encoding", true);
			multiValue.Add ("accept-language", true);
			multiValue.Add ("accept-ranges", true);
			multiValue.Add ("allow", true);
			multiValue.Add ("authorization", true);
			multiValue.Add ("cache-control", true);
			multiValue.Add ("connection", true);
			multiValue.Add ("content-encoding", true);
			multiValue.Add ("content-language", true);			
			multiValue.Add ("expect", true);		
			multiValue.Add ("if-match", true);
			multiValue.Add ("if-none-match", true);
			multiValue.Add ("proxy-authenticate", true);
			multiValue.Add ("public", true);			
			multiValue.Add ("range", true);
			multiValue.Add ("transfer-encoding", true);
			multiValue.Add ("upgrade", true);
			multiValue.Add ("vary", true);
			multiValue.Add ("via", true);
			multiValue.Add ("warning", true);
			multiValue.Add ("www-authenticate", true);

			// Extra
			multiValue.Add ("set-cookie", true);
			multiValue.Add ("set-cookie2", true);
		}
		
		// Constructors
		
		public WebHeaderCollection () {	}	
		
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
		
		internal WebHeaderCollection (bool internallyCreated)
		{	
			this.internallyCreated = internallyCreated;
		}		
		
		// Methods
		
		public void Add (string header)
		{
			if (header == null)
				throw new ArgumentNullException ("header");
			int pos = header.IndexOf (':');
			if (pos == -1)
				throw new ArgumentException ("no colon found", "header");				
			this.Add (header.Substring (0, pos), 
				  header.Substring (pos + 1));
		}
		
		public override void Add (string name, string value)
		{
			if (name == null)
				throw new ArgumentNullException ("name");
			if (internallyCreated && IsRestricted (name))
				throw new ArgumentException ("This header must be modified with the appropiate property.");
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
			base.Add (headerName, headerValue);			
		}

		public override string [] GetValues (string header)
		{
			if (header == null)
				throw new ArgumentNullException ("header");

			string [] values = base.GetValues (header);
			if (values == null || values.Length == 0)
				return null;

			/*
			if (IsMultiValue (header)) {
				values = GetMultipleValues (values);
			}
			*/

			return values;
		}

		public override string[] GetValues (int index)
		{
			string[] values = base.GetValues (index);
			if (values == null || values.Length == 0) {
				return(null);
			}
			
			return(values);
		}

		/* Now i wonder why this is here...
		static string [] GetMultipleValues (string [] values)
		{
			ArrayList mvalues = new ArrayList (values.Length);
			StringBuilder sb = null;
			for (int i = 0; i < values.Length; ++i) {
				string val = values [i];
				if (val.IndexOf (',') == -1) {
					mvalues.Add (val);
					continue;
				}

				if (sb == null)
					sb = new StringBuilder ();

				bool quote = false;
				for (int k = 0; k < val.Length; k++) {
					char c = val [k];
					if (c == '"') {
						quote = !quote;
					} else if (!quote && c == ',') {
						mvalues.Add (sb.ToString ().Trim ());
						sb.Length = 0;
						continue;
					}
					sb.Append (c);
				}

				if (sb.Length > 0) {
					mvalues.Add (sb.ToString ().Trim ());
					sb.Length = 0;
				}
			}

			return (string []) mvalues.ToArray (typeof (string));
		}
		*/

		public static bool IsRestricted (string headerName)
		{
			if (headerName == null)
				throw new ArgumentNullException ("headerName");

			if (headerName == "") // MS throw nullexception here!
				throw new ArgumentException ("empty string", "headerName");

			if (!IsHeaderName (headerName))
				throw new ArgumentException ("Invalid character in header");

			return restricted.ContainsKey (headerName);
		}

#if EMBEDDED_IN_1_0
		public static bool IsRestricted (string headerName, bool response)
		{
			if (headerName == null || headerName.Length == 0)
				throw new ArgumentNullException ("headerName");

			if (!IsHeaderName (headerName))
				throw new ArgumentException ("Invalid character in header");


			if (response)
				return false;
			return restricted.ContainsKey (headerName);
		}
#endif

		public override void OnDeserialization (object sender)
		{
		}

		public override void Remove (string name)
		{
			if (name == null)
				throw new ArgumentNullException ("name");
			if (internallyCreated && IsRestricted (name))
				throw new ArgumentException ("restricted header");
			base.Remove (name);
		}

		public override void Set (string name, string value)
		{
			if (name == null)
				throw new ArgumentNullException ("name");
			if (internallyCreated && IsRestricted (name))
				throw new ArgumentException ("restricted header");
			if (!IsHeaderName (name))
				throw new ArgumentException ("invalid header name");
			if (value == null)
				value = String.Empty;
			else
				value = value.Trim ();
			if (!IsHeaderValue (value))
				throw new ArgumentException ("invalid header value");
			base.Set (name, value);			
		}

		public byte[] ToByteArray ()
		{
			return Encoding.UTF8.GetBytes(ToString ());
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

		public override string[] AllKeys
		{
			get {
				return(base.AllKeys);
			}
		}
		
		public override int Count 
		{
			get {
				return(base.Count);
			}
		}

		public override KeysCollection Keys
		{
			get {
				return(base.Keys);
			}
		}

		public override string Get (int index)
		{
			return(base.Get (index));
		}
		
		public override string Get (string name)
		{
			return(base.Get (name));
		}
		
		public override string GetKey (int index)
		{
			return(base.GetKey (index));
		}

#if EMBEDDED_IN_1_0
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

		string RequestHeaderToString (HttpRequestHeader value)
		{
			switch (value){
			case HttpRequestHeader.CacheControl:
				return "cache-control";
			case HttpRequestHeader.Connection:
				return "connection";
			case HttpRequestHeader.Date:
				return "date";
			case HttpRequestHeader.KeepAlive:
				return "keep-alive";
			case HttpRequestHeader.Pragma:
				return "pragma";
			case HttpRequestHeader.Trailer:
				return "trailer";
			case HttpRequestHeader.TransferEncoding:
				return "transfer-encoding";
			case HttpRequestHeader.Upgrade:
				return "upgrade";
			case HttpRequestHeader.Via:
				return "via";
			case HttpRequestHeader.Warning:
				return "warning";
			case HttpRequestHeader.Allow:
				return "allow";
			case HttpRequestHeader.ContentLength:
				return "content-length";
			case HttpRequestHeader.ContentType:
				return "content-type";
			case HttpRequestHeader.ContentEncoding:
				return "content-encoding";
			case HttpRequestHeader.ContentLanguage:
				return "content-language";
			case HttpRequestHeader.ContentLocation:
				return "content-location";
			case HttpRequestHeader.ContentMd5:
				return "content-md5";
			case HttpRequestHeader.ContentRange:
				return "content-range";
			case HttpRequestHeader.Expires:
				return "expires";
			case HttpRequestHeader.LastModified:
				return "last-modified";
			case HttpRequestHeader.Accept:
				return "accept";
			case HttpRequestHeader.AcceptCharset:
				return "accept-charset";
			case HttpRequestHeader.AcceptEncoding:
				return "accept-encoding";
			case HttpRequestHeader.AcceptLanguage:
				return "accept-language";
			case HttpRequestHeader.Authorization:
				return "authorization";
			case HttpRequestHeader.Cookie:
				return "cookie";
			case HttpRequestHeader.Expect:
				return "expect";
			case HttpRequestHeader.From:
				return "from";
			case HttpRequestHeader.Host:
				return "host";
			case HttpRequestHeader.IfMatch:
				return "if-match";
			case HttpRequestHeader.IfModifiedSince:
				return "if-modified-since";
			case HttpRequestHeader.IfNoneMatch:
				return "if-none-match";
			case HttpRequestHeader.IfRange:
				return "if-range";
			case HttpRequestHeader.IfUnmodifiedSince:
				return "if-unmodified-since";
			case HttpRequestHeader.MaxForwards:
				return "max-forwards";
			case HttpRequestHeader.ProxyAuthorization:
				return "proxy-authorization";
			case HttpRequestHeader.Referer:
				return "referer";
			case HttpRequestHeader.Range:
				return "range";
			case HttpRequestHeader.Te:
				return "te";
			case HttpRequestHeader.Translate:
				return "translate";
			case HttpRequestHeader.UserAgent:
				return "user-agent";
			default:
				throw new InvalidOperationException ();
			}
		}
		
		
		public string this[HttpRequestHeader hrh]
		{
			get {
				return Get (RequestHeaderToString (hrh));
			}
			
			set {
				Add (RequestHeaderToString (hrh), value);
			}
		}

		string ResponseHeaderToString (HttpResponseHeader value)
		{
			switch (value){
			case HttpResponseHeader.CacheControl:
				return "cache-control";
			case HttpResponseHeader.Connection:
				return "connection";
			case HttpResponseHeader.Date:
				return "date";
			case HttpResponseHeader.KeepAlive:
				return "keep-alive";
			case HttpResponseHeader.Pragma:
				return "pragma";
			case HttpResponseHeader.Trailer:
				return "trailer";
			case HttpResponseHeader.TransferEncoding:
				return "transfer-encoding";
			case HttpResponseHeader.Upgrade:
				return "upgrade";
			case HttpResponseHeader.Via:
				return "via";
			case HttpResponseHeader.Warning:
				return "warning";
			case HttpResponseHeader.Allow:
				return "allow";
			case HttpResponseHeader.ContentLength:
				return "content-length";
			case HttpResponseHeader.ContentType:
				return "content-type";
			case HttpResponseHeader.ContentEncoding:
				return "content-encoding";
			case HttpResponseHeader.ContentLanguage:
				return "content-language";
			case HttpResponseHeader.ContentLocation:
				return "content-location";
			case HttpResponseHeader.ContentMd5:
				return "content-md5";
			case HttpResponseHeader.ContentRange:
				return "content-range";
			case HttpResponseHeader.Expires:
				return "expires";
			case HttpResponseHeader.LastModified:
				return "last-modified";
			case HttpResponseHeader.AcceptRanges:
				return "accept-ranges";
			case HttpResponseHeader.Age:
				return "age";
			case HttpResponseHeader.ETag:
				return "etag";
			case HttpResponseHeader.Location:
				return "location";
			case HttpResponseHeader.ProxyAuthenticate:
				return "proxy-authenticate";
			case HttpResponseHeader.RetryAfter:
				return "RetryAfter";
			case HttpResponseHeader.Server:
				return "server";
			case HttpResponseHeader.SetCookie:
				return "set-cookie";
			case HttpResponseHeader.Vary:
				return "vary";
			case HttpResponseHeader.WwwAuthenticate:
				return "www-authenticate";
			default:
				throw new InvalidOperationException ();
			}
		}
		public string this[HttpResponseHeader hrh]
		{
			get
			{
				return Get (ResponseHeaderToString (hrh));
			}

			set
			{
				Add (ResponseHeaderToString (hrh), value);
			}
		}

#if !EMBEDDED_IN_1_0
		public override void Clear ()
		{
			base.Clear ();
		}


		public override IEnumerator GetEnumerator ()
		{
			return(base.GetEnumerator ());
		}
#endif
#endif

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
		
		internal static bool IsMultiValue (string headerName)
		{
			if (headerName == null || headerName == "")
				return false;

			return multiValue.ContainsKey (headerName);
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
				if (c > 126 || !allowed_chars [(int) c])
					return false;
			}
			
			return true;
		}

		static bool [] allowed_chars = new bool [126] {
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
	}
}


