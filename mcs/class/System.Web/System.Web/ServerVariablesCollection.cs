//
// System.Web.ServerVariablesCollection
//
// Authors:
//   	Alon Gazit (along@mainsoft.com)
//
// (c) 2004 Mainsoft, Inc. (http://www.mainsoft.com)
//
using System;
using System.Collections;
using System.Runtime.Serialization;
using System.Globalization;

namespace System.Web
{

	internal class ServerVariablesCollection:HttpValueCollection
	{
		private HttpRequest _request;
		private bool _loaded = false;
		public ServerVariablesCollection(HttpRequest request):base()
		{
			_request = request;
		}

		private void loadServerVariablesCollection()
		{
			if (_loaded)
				return;
			MakeReadWrite();
			Add("ALL_HTTP", _request.GetAllHeaders(false));
			Add("ALL_RAW", _request.GetAllHeaders(true));
			Add("APPL_MD_PATH", _request.WorkerRequest.GetServerVariable("APPL_MD_PATH"));
			Add("AUTH_PASSWORD", _request.WorkerRequest.GetServerVariable("AUTH_PASSWORD"));
			Add("CERT_COOKIE", _request.WorkerRequest.GetServerVariable("CERT_COOKIE"));
			Add("CERT_FLAGS", _request.WorkerRequest.GetServerVariable("CERT_FLAGS"));
			Add("CERT_ISSUER", _request.WorkerRequest.GetServerVariable("CERT_ISSUER"));
			Add("CERT_KEYSIZE", _request.WorkerRequest.GetServerVariable("CERT_KEYSIZE"));
			Add("CERT_SECRETKEYSIZE", _request.WorkerRequest.GetServerVariable("CERT_SECRETKEYSIZE"));
			Add("CERT_SERIALNUMBER", _request.WorkerRequest.GetServerVariable("CERT_SERIALNUMBER"));
			Add("CERT_SERVER_ISSUER", _request.WorkerRequest.GetServerVariable("CERT_SERVER_ISSUER"));
			Add("CERT_SERVER_SUBJECT", _request.WorkerRequest.GetServerVariable("CERT_SERVER_SUBJECT"));
			Add("CERT_SUBJECT", _request.WorkerRequest.GetServerVariable("CERT_SUBJECT"));
			Add("GATEWAY_INTERFACE", _request.WorkerRequest.GetServerVariable("GATEWAY_INTERFACE"));
			Add("HTTPS", _request.WorkerRequest.GetServerVariable("HTTPS"));
			Add("HTTPS_KEYSIZE", _request.WorkerRequest.GetServerVariable("HTTPS_KEYSIZE"));
			Add("HTTPS_SECRETKEYSIZE", _request.WorkerRequest.GetServerVariable("HTTPS_SECRETKEYSIZE"));
			Add("CONTENT_TYPE", _request.ContentType);
			Add("HTTPS_SERVER_ISSUER", _request.WorkerRequest.GetServerVariable("HTTPS_SERVER_ISSUER"));
			Add("HTTPS_SERVER_SUBJECT", _request.WorkerRequest.GetServerVariable("HTTPS_SERVER_SUBJECT"));
			Add("INSTANCE_ID", _request.WorkerRequest.GetServerVariable("INSTANCE_ID"));
			Add("INSTANCE_META_PATH", _request.WorkerRequest.GetServerVariable("INSTANCE_META_PATH"));
			Add("LOCAL_ADDR", _request.WorkerRequest.GetLocalAddress());
			Add("REMOTE_ADDR", _request.UserHostAddress);
			Add("REMOTE_HOST", _request.UserHostName);
			Add("REMOTE_PORT", _request.WorkerRequest.GetRemotePort ().ToString ());
			Add("REQUEST_METHOD", _request.HttpMethod);
			Add("SERVER_NAME", _request.WorkerRequest.GetServerName());
			Add("SERVER_PORT", _request.WorkerRequest.GetLocalPort().ToString());
			Add("SERVER_PROTOCOL", _request.WorkerRequest.GetHttpVersion());
			Add("SERVER_SOFTWARE", _request.WorkerRequest.GetServerVariable("SERVER_SOFTWARE"));
			if (_request.WorkerRequest.IsSecure()) 
				Add("SERVER_PORT_SECURE", "1");
			else 
				Add("SERVER_PORT_SECURE", "0");

			string sTmp = _request.WorkerRequest.GetKnownRequestHeader(HttpWorkerRequest.HeaderContentLength);
			if (null != sTmp) 
				Add("CONTENT_LENGTH", sTmp);
			// TODO: Should be dynamic
			if (null != _request.Context.User && _request.Context.User.Identity.IsAuthenticated) 
			{
				Add("AUTH_TYPE", _request.Context.User.Identity.AuthenticationType);
				Add("AUTH_USER", _request.Context.User.Identity.Name);
			} 
			else 
			{
				Add("AUTH_TYPE", "");
				Add("AUTH_USER", "");
			}

			Add("PATH_INFO", _request.PathInfo);
			Add("PATH_TRANSLATED", _request.PhysicalPath);
			Add("QUERY_STRING", _request.QueryStringRaw);
			Add("SCRIPT_NAME", _request.FilePath);
			MakeReadOnly();
			_loaded = true;
		}

 
		public override string Get(int index)
		{
			loadServerVariablesCollection();
			return base.Get(index); 
		}

		public override string Get(string name)
		{
			string text1;
			if (!_loaded)
			{
				text1 = GetServerVar(name);
				if (text1 != null)				
					return text1; 				
				loadServerVariablesCollection(); 
			}
			return base.Get(name); 

			
		}

		private string GetServerVar(string name)
		{
			if (((name == null) || (name.Length <= 8)) || (this._request == null))
				return null;
			if (string.Compare(name, "AUTH_TYPE", true, CultureInfo.InvariantCulture) == 0)
			{
				if (null != _request.Context.User && _request.Context.User.Identity.IsAuthenticated) 
					return _request.Context.User.Identity.AuthenticationType;
				else
					return string.Empty;
			}
			else if (string.Compare(name, "AUTH_USER",true, CultureInfo.InvariantCulture) == 0)
			{
				if (null != _request.Context.User && _request.Context.User.Identity.IsAuthenticated) 
					return _request.Context.User.Identity.Name;
				else
					return string.Empty;
			}
			else if (string.Compare(name, "QUERY_STRING", true, CultureInfo.InvariantCulture) == 0)				
				return this._request.QueryStringRaw; 
			else if (string.Compare(name, "PATH_INFO", true, CultureInfo.InvariantCulture) == 0)				
				return this._request.PathInfo; 
			else if (string.Compare(name, "PATH_TRANSLATED", true, CultureInfo.InvariantCulture) == 0)
				return this._request.PhysicalPath; 			
			else if (string.Compare(name, "REQUEST_METHOD", true, CultureInfo.InvariantCulture) == 0)				
				return this._request.HttpMethod;
			else if (string.Compare(name, "REMOTE_ADDR", true, CultureInfo.InvariantCulture) == 0)			
				return this._request.UserHostAddress; 			
			else if (string.Compare(name, "REMOTE_HOST", true, CultureInfo.InvariantCulture) == 0)			
				return this._request.UserHostName; 			
			else if (string.Compare(name, "REMOTE_ADDRESS", true, CultureInfo.InvariantCulture) == 0)
				return this._request.UserHostAddress; 
			else if (string.Compare(name, "SCRIPT_NAME", true, CultureInfo.InvariantCulture) == 0)				
				return this._request.FilePath;
			else if (string.Compare(name, "LOCAL_ADDR", true, CultureInfo.InvariantCulture) == 0)				
				return this._request.WorkerRequest.GetLocalAddress();
			else if (string.Compare(name, "SERVER_PROTOCOL", true, CultureInfo.InvariantCulture) == 0)
				return _request.WorkerRequest.GetHttpVersion();
			else if (string.Compare(name, "SERVER_SOFTWARE", true, CultureInfo.InvariantCulture) == 0)
				return _request.WorkerRequest.GetServerVariable("SERVER_SOFTWARE");
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
			{
				return null; 
			}
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
			{
				return null; 
			}
			array1 = new string[1];
			array1[0] = text1;
			return array1; 
		}
 
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			throw new SerializationException(); 
		}

		public override string[] AllKeys 
		{
			get 
			{
				loadServerVariablesCollection();
				return base.AllKeys;
			}
		}

		public override int Count 
		{
			get 
			{
				loadServerVariablesCollection();
				return base.Count;
			}
		} 
	}
}
