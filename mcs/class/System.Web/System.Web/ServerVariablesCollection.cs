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
using System.Web.Util;

namespace System.Web
{
	class ServerVariablesCollection : BaseParamsCollection
	{
		HttpRequest request;
		bool loaded;

		string QueryString {
			get {
				string qs = _request.QueryStringRaw;

				if (String.IsNullOrEmpty (qs))
					return qs;

				if (qs [0] == '?')
					return qs.Substring (1);

				return qs;
			}
		}
		
		public ServerVariablesCollection(HttpRequest request) : base(request)
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
				sb.Append (key.ToUpper (Helpers.InvariantCulture).Replace ('-', '_'));
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
						Add ("HTTP_" + hname.ToUpper (Helpers.InvariantCulture).Replace ('-', '_'), hvalue);
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
					Add ("HTTP_" + hname.ToUpper (Helpers.InvariantCulture).Replace ('-', '_'), hvalue);
				}
			}
		}

		void loadServerVariablesCollection()
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
			Add("QUERY_STRING", QueryString);
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

		protected override void InsertInfo ()
		{
			loadServerVariablesCollection ();
		}

		protected override string InternalGet (string name)
		{
			if ((name == null) || (this._request == null))
				return null;
			name = name.ToUpper (Helpers.InvariantCulture);
			switch (name) {
				case "AUTH_TYPE":
					if (null != _request.Context.User && _request.Context.User.Identity.IsAuthenticated)
						return _request.Context.User.Identity.AuthenticationType;
					else
						return string.Empty;
				case "AUTH_USER":
					if (null != _request.Context.User && _request.Context.User.Identity.IsAuthenticated)
						return _request.Context.User.Identity.Name;
					else
						return string.Empty;
				case "QUERY_STRING":
					return QueryString;
				case "PATH_INFO":
					return this._request.PathInfo;
				case "PATH_TRANSLATED":
					return this._request.PhysicalPath;
				case "REQUEST_METHOD":
					return this._request.HttpMethod;
				case "REMOTE_ADDR":
					return this._request.UserHostAddress;
				case "REMOTE_HOST":
					return this._request.UserHostName;
				case "REMOTE_ADDRESS":
					return this._request.UserHostAddress;
				case "SCRIPT_NAME":
					return this._request.FilePath;
				case "LOCAL_ADDR":
					return this._request.WorkerRequest.GetLocalAddress ();
				case "SERVER_PROTOCOL":
					return _request.WorkerRequest.GetHttpVersion ();
				case "CONTENT_TYPE":
					return _request.ContentType;
				case "REMOTE_PORT":
					return _request.WorkerRequest.GetRemotePort ().ToString ();
				case "SERVER_NAME":
					return _request.WorkerRequest.GetServerName ();
				case "SERVER_PORT":
					return _request.WorkerRequest.GetLocalPort ().ToString ();
				case "APPL_PHYSICAL_PATH":
					return _request.WorkerRequest.GetAppPathTranslated ();
				case "REMOTE_USER":
					return (_request.Context.User != null && _request.Context.User.Identity.IsAuthenticated) ?
						_request.Context.User.Identity.Name :
						String.Empty;
				case "URL":
					return _request.FilePath;
				case "SERVER_PORT_SECURE":
					return (_request.WorkerRequest.IsSecure ()) ? "1" : "0";
				case "ALL_HTTP":
					return Fill (_request.WorkerRequest, true);
				case "ALL_RAW":
					return Fill (_request.WorkerRequest, false);
				case "SERVER_SOFTWARE":
				case "APPL_MD_PATH":
				case "AUTH_PASSWORD":
				case "CERT_COOKIE":
				case "CERT_FLAGS":
				case "CERT_ISSUER":
				case "CERT_KEYSIZE":
				case "CERT_SECRETKEYSIZE":
				case "CERT_SERIALNUMBER":
				case "CERT_SERVER_ISSUER":
				case "CERT_SERVER_SUBJECT":
				case "GATEWAY_INTERFACE":
				case "HTTPS":
				case "HTTPS_KEYSIZE":
				case "HTTPS_SECRETKEYSIZE":
				case "HTTPS_SERVER_ISSUER":
				case "HTTPS_SERVER_SUBJECT":
				case "INSTANCE_ID":
				case "INSTANCE_META_PATH":
				case "LOGON_USER":
				case "HTTP_ACCEPT":
				case "HTTP_REFERER":
				case "HTTP_ACCEPT_LANGUAGE":
				case "HTTP_ACCEPT_ENCODING":
				case "HTTP_CONNECTION":
				case "HTTP_HOST":
				case "HTTP_USER_AGENT":
				case "HTTP_SOAPACTION":
					return _request.WorkerRequest.GetServerVariable (name);
				default:
					return null;
			}
		}
	}
}
