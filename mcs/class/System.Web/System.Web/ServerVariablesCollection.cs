//
// System.Web.ServerVariablesCollection
//
// Authors:
//   	Alon Gazit (along@mainsoft.com)
//   	Miguel de Icaza (miguel@novell.com)
//   	Gonzalo Paniagua Javier (gonzalo@novell.com)
//
// (c) 2004 Mainsoft, Inc. (http://www.mainsoft.com)
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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

using System.Text;
using System.Collections;
using System.Collections.Specialized;
using System.Runtime.Serialization;
using System.Globalization;
using System.Security.Permissions;

namespace System.Web
{
	class ServerVariablesCollection : NameValueCollection
	{
		HttpRequest request;
		bool loaded;

		public ServerVariablesCollection(HttpRequest request)
		{
			IsReadOnly = true;
			this.request = request;
		}

		void AppendKeyValue (StringBuilder sb, string key, string value, bool standard)
		{
			//
			// Standard has HTTP_ prefix, everything is uppercase, has no space
			// after colon, - is changed to _
			//
			// Raw is header, colon, space, values, raw.
			//
			if (standard){
				sb.Append ("HTTP_");
				sb.Append (key.ToUpper ().Replace ("-", "_"));
				sb.Append (":");
			} else {
				sb.Append (key);
				sb.Append (": ");
			}
			sb.Append (value);
			sb.Append ("\r\n");
		}
				     
		string Fill (HttpWorkerRequest wr, bool standard)
		{
			StringBuilder sb = new StringBuilder ();
			
			for (int i = 0; i < HttpWorkerRequest.RequestHeaderMaximum; i++){
				string val = wr.GetKnownRequestHeader (i);
				if (val == null || val == "")
					continue;
				string key = HttpWorkerRequest.GetKnownRequestHeaderName (i);
				AppendKeyValue (sb, key, val, standard);
			}
			string [][] other = wr.GetUnknownRequestHeaders ();
			if (other == null)
				return sb.ToString ();

			for (int i = other.Length; i > 0; ){
				i--;
				AppendKeyValue (sb, other [i][0], other [i][1], standard);
			}

			return sb.ToString ();
		}

		void AddHeaderVariables (HttpWorkerRequest wr)
		{
			string hname;
			string hvalue;

			// Add all known headers
			for (int i = 0; i < HttpWorkerRequest.RequestHeaderMaximum; i++) {
				hvalue = wr.GetKnownRequestHeader (i);
				if (null != hvalue && hvalue.Length > 0) {
					hname = HttpWorkerRequest.GetKnownRequestHeaderName (i);
					if (null != hname && hname.Length > 0)
						Add ("HTTP_" + hname.ToUpper ().Replace ('-', '_'), hvalue);
				}
			}

			// Get all other headers
			string [][] unknown = wr.GetUnknownRequestHeaders ();
			if (null != unknown) {
				for (int i = 0; i < unknown.Length; i++) {
					hname = unknown [i][0];
					if (hname == null)
						continue;
					hvalue = unknown [i][1];
					Add ("HTTP_" + hname.ToUpper ().Replace ('-', '_'), hvalue);
				}
			}
		}

		private void loadServerVariablesCollection()
		{
			HttpWorkerRequest wr = request.WorkerRequest;
			if (loaded || (wr == null))
				return;

			IsReadOnly = false;
		
			Add("ALL_HTTP", Fill (wr, true));
			Add("ALL_RAW",  Fill (wr, false));
			    
			Add("APPL_MD_PATH", wr.GetServerVariable("APPL_MD_PATH"));
			Add("APPL_PHYSICAL_PATH", wr.GetServerVariable("APPL_PHYSICAL_PATH"));

			if (null != request.Context.User && request.Context.User.Identity.IsAuthenticated) {
				Add ("AUTH_TYPE", request.Context.User.Identity.AuthenticationType);
				Add ("AUTH_USER", request.Context.User.Identity.Name);
			} else {
				Add ("AUTH_TYPE", "");
				Add ("AUTH_USER", "");
			}

			Add("AUTH_PASSWORD", wr.GetServerVariable("AUTH_PASSWORD"));
			Add ("LOGON_USER", wr.GetServerVariable("LOGON_USER"));
			Add ("REMOTE_USER", wr.GetServerVariable("REMOTE_USER"));
			Add("CERT_COOKIE", wr.GetServerVariable("CERT_COOKIE"));
			Add("CERT_FLAGS", wr.GetServerVariable("CERT_FLAGS"));
			Add("CERT_ISSUER", wr.GetServerVariable("CERT_ISSUER"));
			Add("CERT_KEYSIZE", wr.GetServerVariable("CERT_KEYSIZE"));
			Add("CERT_SECRETKEYSIZE", wr.GetServerVariable("CERT_SECRETKEYSIZE"));
			Add("CERT_SERIALNUMBER", wr.GetServerVariable("CERT_SERIALNUMBER"));
			Add("CERT_SERVER_ISSUER", wr.GetServerVariable("CERT_SERVER_ISSUER"));
			Add("CERT_SERVER_SUBJECT", wr.GetServerVariable("CERT_SERVER_SUBJECT"));
			Add("CERT_SUBJECT", wr.GetServerVariable("CERT_SUBJECT"));

			string sTmp = wr.GetKnownRequestHeader(HttpWorkerRequest.HeaderContentLength);
			if (null != sTmp)
				Add ("CONTENT_LENGTH", sTmp);
			Add ("CONTENT_TYPE", request.ContentType);

			Add("GATEWAY_INTERFACE", wr.GetServerVariable("GATEWAY_INTERFACE"));
			Add("HTTPS", wr.GetServerVariable("HTTPS"));
			Add("HTTPS_KEYSIZE", wr.GetServerVariable("HTTPS_KEYSIZE"));
			Add("HTTPS_SECRETKEYSIZE", wr.GetServerVariable("HTTPS_SECRETKEYSIZE"));
			Add("HTTPS_SERVER_ISSUER", wr.GetServerVariable("HTTPS_SERVER_ISSUER"));
			Add("HTTPS_SERVER_SUBJECT", wr.GetServerVariable("HTTPS_SERVER_SUBJECT"));
			Add("INSTANCE_ID", wr.GetServerVariable("INSTANCE_ID"));
			Add("INSTANCE_META_PATH", wr.GetServerVariable("INSTANCE_META_PATH"));
			Add("LOCAL_ADDR", wr.GetLocalAddress());
			Add("PATH_INFO", request.PathInfo);
			Add("PATH_TRANSLATED", request.PhysicalPath);
			Add("QUERY_STRING", request.QueryStringRaw);
			Add("REMOTE_ADDR", request.UserHostAddress);
			Add("REMOTE_HOST", request.UserHostName);
			Add("REMOTE_PORT", wr.GetRemotePort ().ToString ());
			Add("REQUEST_METHOD", request.HttpMethod);
			Add("SCRIPT_NAME", request.FilePath);
			Add("SERVER_NAME", wr.GetServerName());
			Add("SERVER_PORT", wr.GetLocalPort().ToString());
			if (wr.IsSecure()) 
				Add("SERVER_PORT_SECURE", "1");
			else
				Add("SERVER_PORT_SECURE", "0");
			Add("SERVER_PROTOCOL", wr.GetHttpVersion());
			Add("SERVER_SOFTWARE", wr.GetServerVariable("SERVER_SOFTWARE"));
			Add ("URL", request.FilePath);

			AddHeaderVariables (wr);

			IsReadOnly = true;
			loaded = true;
		}

 
		public override string Get(int index)
		{
			loadServerVariablesCollection();
			return base.Get(index); 
		}

		public override string Get(string name)
		{
			string text1;
			if (!loaded) {
				text1 = GetServerVar(name);
				if (text1 != null)				
					return text1; 				
				loadServerVariablesCollection(); 
			}
			return base.Get(name); 

			
		}

		private string GetServerVar(string name)
		{
			if (((name == null) || (name.Length <= 8)) || (this.request == null))
				return null;
			
			if (string.Compare(name, "AUTH_TYPE", true, CultureInfo.InvariantCulture) == 0) {
				if (null != request.Context.User && request.Context.User.Identity.IsAuthenticated) 
					return request.Context.User.Identity.AuthenticationType;
				else
					return string.Empty;
			} else if (string.Compare(name, "AUTH_USER",true, CultureInfo.InvariantCulture) == 0) {
				if (null != request.Context.User && request.Context.User.Identity.IsAuthenticated) 
					return request.Context.User.Identity.Name;
				else
					return string.Empty;
			} else if (string.Compare(name, "QUERY_STRING", true, CultureInfo.InvariantCulture) == 0)				
				return this.request.QueryStringRaw; 
			else if (string.Compare(name, "PATH_INFO", true, CultureInfo.InvariantCulture) == 0)				
				return this.request.PathInfo; 
			else if (string.Compare(name, "PATH_TRANSLATED", true, CultureInfo.InvariantCulture) == 0)
				return this.request.PhysicalPath; 			
			else if (string.Compare(name, "REQUEST_METHOD", true, CultureInfo.InvariantCulture) == 0)				
				return this.request.HttpMethod;
			else if (string.Compare(name, "REMOTE_ADDR", true, CultureInfo.InvariantCulture) == 0)			
				return this.request.UserHostAddress; 			
			else if (string.Compare(name, "REMOTE_HOST", true, CultureInfo.InvariantCulture) == 0)			
				return this.request.UserHostName; 			
			else if (string.Compare(name, "REMOTE_ADDRESS", true, CultureInfo.InvariantCulture) == 0)
				return this.request.UserHostAddress; 
			else if (string.Compare(name, "SCRIPT_NAME", true, CultureInfo.InvariantCulture) == 0)				
				return this.request.FilePath;
			else if (string.Compare(name, "LOCAL_ADDR", true, CultureInfo.InvariantCulture) == 0)				
				return this.request.WorkerRequest.GetLocalAddress();
			else if (string.Compare(name, "SERVER_PROTOCOL", true, CultureInfo.InvariantCulture) == 0)
				return request.WorkerRequest.GetHttpVersion();
			else if (string.Compare(name, "SERVER_SOFTWARE", true, CultureInfo.InvariantCulture) == 0)
				return request.WorkerRequest.GetServerVariable("SERVER_SOFTWARE");
			return null; 
		}
 
		public override string GetKey(int index)
		{
			loadServerVariablesCollection();
			return base.GetKey(index); 
		}
 
		public override string[] GetValues(int index)
		{
			string text1;
			string[] array1;

			text1 = Get(index);
			if (text1 == null) 
				return null;
			
			array1 = new string[1];
			array1[0] = text1;

			return array1; 
		}
 
		public override string[] GetValues(string name)
		{
			string text1;
			string[] array1;

			text1 = Get(name);
			if (text1 == null)
				return null; 
			array1 = new string[1];
			array1[0] = text1;
			
			return array1; 
		}
 
		// not really useful except for not triggering Gendarme warnings
		[SecurityPermission (SecurityAction.Demand, SerializationFormatter = true)]
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			throw new SerializationException(); 
		}

		public override string[] AllKeys 
		{
			get {
				loadServerVariablesCollection ();
				return base.AllKeys;
			}
		}

		public override int Count 
		{
			get {
				loadServerVariablesCollection ();
				return base.Count;
			}
		} 
	}
}
