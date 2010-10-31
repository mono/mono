//
// System.Net.BasicClient
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2003 Ximian, Inc (http://www.ximian.com)
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
			if (header.ToLower ().IndexOf ("basic", StringComparison.Ordinal) == -1)
				return null;

			return InternalAuthenticate (webRequest, credentials);
		}

		static byte [] GetBytes (string str)
		{
			int i = str.Length;
			byte [] result = new byte [i];
			for (--i; i >= 0; i--)
				result [i] = (byte) str [i];

			return result;
		}

		static Authorization InternalAuthenticate (WebRequest webRequest, ICredentials credentials)
		{
			HttpWebRequest request = webRequest as HttpWebRequest;
			if (request == null || credentials == null)
				return null;

			NetworkCredential cred = credentials.GetCredential (request.AuthUri, "basic");
			if (cred == null)
				return null;

			string userName = cred.UserName;
			if (userName == null || userName == "")
				return null;

			string password = cred.Password;
			string domain = cred.Domain;
			byte [] bytes;

			// If domain is set, MS sends "domain\user:password". 
			if (domain == null || domain == "" || domain.Trim () == "")
				bytes = GetBytes (userName + ":" + password);
			else
				bytes = GetBytes (domain + "\\" + userName + ":" + password);

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

