//
// System.Net.WebHeaderCollection
//
// Authors:
// 	Lawrence Pit (loz@cable.a2000.nl)
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (c) 2003 Ximian, Inc. (http://www.ximian.com)
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
using System.Collections.Specialized;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
    
// See RFC 2068 par 4.2 Message Headers
    
namespace System.Net 
{
	[Serializable]
	[ComVisible(true)]
	public class WebHeaderCollection : NameValueCollection, ISerializable
	{
		private static readonly Hashtable restricted;
		private static readonly Hashtable multiValue;
		private bool internallyCreated = false;
		
		// Static Initializer
		
		static WebHeaderCollection () 
		{
			// the list of restricted header names as defined 
			// by the ms.net spec
			restricted = new Hashtable (CaseInsensitiveHashCodeProvider.Default,
						    CaseInsensitiveComparer.Default);

			restricted.Add ("accept", true);
			restricted.Add ("connection", true);
			restricted.Add ("content-length", true);
			restricted.Add ("content-type", true);
			restricted.Add ("date", true);
			restricted.Add ("expect", true);
			restricted.Add ("host", true);
			restricted.Add ("range", true);
			restricted.Add ("referer", true);
			restricted.Add ("transfer-encoding", true);
			restricted.Add ("user-agent", true);			
			
			// see par 14 of RFC 2068 to see which header names
			// accept multiple values each separated by a comma
			multiValue = new Hashtable (CaseInsensitiveHashCodeProvider.Default,
						    CaseInsensitiveComparer.Default);

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

			// Extra
			multiValue.Add ("set-cookie", true);
			multiValue.Add ("set-cookie2", true);
		}
		
		// Constructors
		
		public WebHeaderCollection () {	}	
		
		protected WebHeaderCollection (SerializationInfo serializationInfo, 
					       StreamingContext streamingContext)
		{
			// TODO: test for compatibility with ms.net
			int count = serializationInfo.GetInt32("count");
			for (int i = 0; i < count; i++) 
				this.Add (serializationInfo.GetString ("k" + i),
					  serializationInfo.GetString ("v" + i));
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
				throw new ArgumentException ("restricted header");
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

			return values;
		}

		public static bool IsRestricted (string headerName)
		{
			if (headerName == null)
				throw new ArgumentNullException ("headerName");

			if (headerName == "") // MS throw nullexception here!
				throw new ArgumentException ("empty string", "headerName");

			return restricted.ContainsKey (headerName);
		}

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
		
		void ISerializable.GetObjectData (SerializationInfo serializationInfo,
		   				  StreamingContext streamingContext)
		{
			int count = base.Count;
			serializationInfo.AddValue ("count", count);
			for (int i = 0; i < count ; i++) {
				serializationInfo.AddValue ("k" + i, GetKey (i));
				serializationInfo.AddValue ("v" + i, Get (i));
			}
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
			// token          = 1*<any CHAR except CTLs or tspecials>
			// tspecials      = "(" | ")" | "<" | ">" | "@"
			//                | "," | ";" | ":" | "\" | <">
			//                | "/" | "[" | "]" | "?" | "="
			//                | "{" | "}" | SP | HT
			
			if (name == null || name.Length == 0)
				return false;

			int len = name.Length;
			for (int i = 0; i < len; i++) {			
				char c = name [i];
				if (c < 0x20 || c >= 0x7f)
					return false;
			}
			
			return name.IndexOfAny (tspecials) == -1;
		}

		private static char [] tspecials = 
				new char [] {'(', ')', '<', '>', '@',
					     ',', ';', ':', '\\', '"',
					     '/', '[', ']', '?', '=',
					     '{', '}', ' ', '\t'};
							
	}
}
