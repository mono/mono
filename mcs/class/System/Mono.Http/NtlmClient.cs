//
// Mono.Http.NtlmClient
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (c) 2003 Novell, Inc. (http://www.novell.com)
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

#if SECURITY_DEP
using System;
using System.Collections;
using System.Net;
using Mono.Security.Protocol.Ntlm;

namespace Mono.Http
{
	class NtlmSession
	{
		MessageBase message;

		public NtlmSession () 
		{
		}

		public Authorization Authenticate (string challenge, WebRequest webRequest, ICredentials credentials) 
		{
			HttpWebRequest request = webRequest as HttpWebRequest;
			if (request == null)
				return null;
	
			NetworkCredential cred = credentials.GetCredential (request.RequestUri, "NTLM");
			if (cred == null)
				return null;

			string userName = cred.UserName;
			string domain = cred.Domain;
			string password = cred.Password;
			if (userName == null || userName == "")
				return null;
			domain = domain != null && domain.Length > 0 ? domain : request.Headers ["Host"];

			bool completed = false;
			if (message == null) {
				Type1Message type1 = new Type1Message ();
				type1.Domain = domain;
				message = type1;
			} else if (message.Type == 1) {
				// Should I check the credentials?
				if (challenge == null) {
					message = null;
					return null;
				}

				Type2Message type2 = new Type2Message (Convert.FromBase64String (challenge));
				if (password == null)
					password = "";

				Type3Message type3 = new Type3Message ();
				type3.Domain = domain;
				type3.Username = userName;
				type3.Challenge = type2.Nonce;
				type3.Password = password;
				message = type3;
				completed = true;
			} else {
				// Should I check the credentials?
				// type must be 3 here
				if (challenge == null || challenge == String.Empty) {
					Type1Message type1 = new Type1Message ();
					type1.Domain = domain;
					message = type1;
				} else {
					completed = true;
				}
			}
			
			string token = "NTLM " + Convert.ToBase64String (message.GetBytes ());
			return new Authorization (token, completed);
		}
	}

	class NtlmClient : IAuthenticationModule
	{
		static Hashtable cache;

		static NtlmClient () 
		{
			cache = new Hashtable ();
		}
	
		public NtlmClient () {}
	
		public Authorization Authenticate (string challenge, WebRequest webRequest, ICredentials credentials) 
		{
			if (credentials == null || challenge == null)
				return null;
	
			string header = challenge.Trim ();
			int idx = header.ToLower ().IndexOf ("ntlm");
			if (idx == -1)
				return null;

			idx = header.IndexOfAny (new char [] {' ', '\t'});
			if (idx != -1) {
				header = header.Substring (idx).Trim ();
			} else {
				header = null;
			}

			HttpWebRequest request = webRequest as HttpWebRequest;
			if (request == null)
				return null;

			lock (cache) {
				NtlmSession ds = (NtlmSession) cache [request];
				if (ds == null) {
					ds = new NtlmSession ();
					cache.Add (request, ds);
				}

				return ds.Authenticate (header, webRequest, credentials);
			}
		}

		public Authorization PreAuthenticate (WebRequest webRequest, ICredentials credentials) 
		{
			return null;
		}
	
		public string AuthenticationType { 
			get { return "NTLM"; }
		}
	
		public bool CanPreAuthenticate { 
			get { return false; }
		}
	}
}
#endif
