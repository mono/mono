//
// System.Net.BasicClient
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2003 Ximian, Inc (http://www.ximian.com)
//

using System.Text;
namespace System.Net
{
	class BasicClient : IAuthenticationModule
	{
		public Authorization Authenticate (string challenge, WebRequest webRequest, ICredentials credentials)
		{
			if (credentials == null || challenge == null)
				return null;

			string header = challenge.Trim ();
			if (!header.ToLower ().StartsWith ("basic "))
				return null;

			return InternalAuthenticate (webRequest, credentials);
		}

		static Authorization InternalAuthenticate (WebRequest webRequest, ICredentials credentials)
		{
			HttpWebRequest request = webRequest as HttpWebRequest;
			if (request == null)
				return null;

			NetworkCredential cred = credentials.GetCredential (request.AuthUri, "basic");
			string userName = cred.UserName;
			if (userName == null || userName == "")
				return null;

			string password = cred.Password;
			string domain = cred.Domain;
			byte [] bytes;

			// If domain is set, MS sends "domain\user:password". 
			if (domain == null || domain == "" || domain.Trim () == "")
				bytes = Encoding.Default.GetBytes (userName + ":" + password);
			else
				bytes = Encoding.Default.GetBytes (domain + "\\" + userName + ":" + password);

			string auth = "Basic " + Convert.ToBase64String (bytes);
			return new Authorization (auth);
		}

		public Authorization PreAuthenticate (WebRequest webRequest, ICredentials credentials)
		{
			return InternalAuthenticate ( webRequest, credentials);
		}

		public string AuthenticationType {
			get { return "Basic"; }
		}

		public bool CanPreAuthenticate {
			get { return true; }
		}
	}
}

