//
// System.Net.WebHeaderCollection
//
// Author:
//   Lawrence Pit (loz@cable.a2000.nl)
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
		private static readonly int [] restricted;
		private static readonly int [] multiValue;
		private bool internallyCreated = false;
		
		// Static Initializer
		
		static WebHeaderCollection () 
		{
			// For performance reasons we initialize the following
			// tables by taking the hashcode of header names.
			// When you add a header make sure all characters are in 
			// lowercase.
			
			// the list of restricted header names as defined 
			// by the ms.net spec
			ArrayList a = new ArrayList ();
			a.Add ("accept".GetHashCode ());
			a.Add ("connection".GetHashCode ());
			a.Add ("content-length".GetHashCode ());
			a.Add ("content-type".GetHashCode ());
			a.Add ("date".GetHashCode ());
			a.Add ("expect".GetHashCode ());    // ??? What is this anyway?
			a.Add ("host".GetHashCode ());
			a.Add ("range".GetHashCode ());
			a.Add ("referer".GetHashCode ());
			a.Add ("transfer-encoding".GetHashCode ());
			a.Add ("user-agent".GetHashCode ());			
			restricted = (int []) a.ToArray (typeof (int));
			
			// see par 14 of RFC 2068 to see which header names
			// accept multiple values each separated by a comma
			a = new ArrayList ();
			a.Add ("accept".GetHashCode ());
			a.Add ("accept-charset".GetHashCode ());
			a.Add ("accept-encoding".GetHashCode ());
			a.Add ("accept-language".GetHashCode ());
			a.Add ("accept-ranges".GetHashCode ());
			a.Add ("allow".GetHashCode ());
			a.Add ("authorization".GetHashCode ());
			a.Add ("cache-control".GetHashCode ());
			a.Add ("connection".GetHashCode ());
			a.Add ("content-encoding".GetHashCode ());
			a.Add ("content-language".GetHashCode ());			
			a.Add ("expect".GetHashCode ());		
			a.Add ("if-match".GetHashCode ());
			a.Add ("if-none-match".GetHashCode ());
			a.Add ("proxy-authenticate".GetHashCode ());
			a.Add ("public".GetHashCode ());			
			a.Add ("range".GetHashCode ());
			a.Add ("transfer-encoding".GetHashCode ());
			a.Add ("upgrade".GetHashCode ());
			a.Add ("vary".GetHashCode ());
			a.Add ("via".GetHashCode ());
			a.Add ("warning".GetHashCode ());
			multiValue = (int []) a.ToArray (typeof (int));
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
		
		internal WebHeaderCollection (bool dummy) : base ()
		{	
			this.internallyCreated = true;
		}		
		
		// Methods
		
		public void Add (string header)
		{
			if (header == null)
				throw new ArgumentNullException ("header");
			int pos = header.IndexOf (':');
			if (pos == -1)
				throw new ArgumentException ("no colon found");				
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
				throw new ArgumentException ("invalid header name");
			if (headerValue == null)
				headerValue = String.Empty;
			else
				headerValue = headerValue.Trim ();
			if (!IsHeaderValue (headerValue))
				throw new ArgumentException ("invalid header value");
			base.Add (headerName, headerValue);			
		}
		
		public override string [] GetValues (string header)
		{
			if (header == null)
				throw new ArgumentNullException ("header");
			string [] values = base.GetValues (header);
			if (values == null || values.Length == 0) 
				return null;
			if (!IsMultiValue (header))
				return values;
			StringCollection col = new StringCollection ();
			for (int i = 0; i < values.Length; i++) {
				string [] s = values [i].Split (new char [] {','});
				for (int j = 0; j < s.Length; j++) 
					s [j] = s [j].Trim ();
				col.AddRange (s);
			}
			values = new string [col.Count];
			col.CopyTo (values, 0);
			return values;
		}

		public static bool IsRestricted (string headerName)
		{
			int hashCode = headerName.ToLower ().GetHashCode ();
			for (int i = 0; i < restricted.Length; i++) 
				if (restricted [i] == hashCode)
					return true;
			return false;
		}

		[MonoTODO]
		public override void OnDeserialization (object sender)
		{
			// no idea what to do here... spec doesn't say much
			throw new NotImplementedException ();
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
		
		internal void SetInternal (string name, string value)
		{
			if (value == null)
				value = String.Empty;
			else
				value = value.Trim ();
			if (!IsHeaderValue (value))
				throw new ArgumentException ("invalid header value");
			base.Set (name, value);	
		}
		
		internal void RemoveInternal (string name)
		{
			if (name == null)
				throw new ArgumentNullException ("name");
			base.Remove (name);
		}		
		
		// Private Methods
		
		private static bool IsMultiValue (string headerName)
		{
			int hashCode = headerName.ToLower ().GetHashCode ();
			for (int i = 0; i < multiValue.Length; i++) 
				if (multiValue [i] == hashCode)
					return true;
			return false;
		}		
		
		private bool IsHeaderValue (string value)
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
		
		private bool IsHeaderName (string name)
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