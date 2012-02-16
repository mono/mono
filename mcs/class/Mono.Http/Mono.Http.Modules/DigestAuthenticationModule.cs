//
// Digest Authentication implementation
//
// Authors:
//	Greg Reinacker (gregr@rassoc.com)
//	Sebastien Pouliot (spouliot@motus.com)
//
// Copyright 2002-2003 Greg Reinacker, Reinacker & Associates, Inc. All rights reserved.
// Portions (C) 2003 Motus Technologies Inc. (http://www.motus.com)
//
// Original source code available at
// http://www.rassoc.com/gregr/weblog/stories/2002/07/09/webServicesSecurityHttpDigestAuthenticationWithoutActiveDirectory.html
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
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Web;
using System.Xml;

namespace Mono.Http.Modules
{
	public class DigestAuthenticationModule : AuthenticationModule
	{
		// TODO: Digest.Nonce.Lifetime="0"	Never expires
		static int nonceLifetime = 60;
		static char[] trim = {'='};

		public DigestAuthenticationModule () : base ("Digest") {}

		protected virtual bool IsValidNonce (string nonce) 
		{
			DateTime expireTime;

			// pad nonce on the right with '=' until length is a multiple of 4
			int numPadChars = nonce.Length % 4;
			if (numPadChars > 0)
				numPadChars = 4 - numPadChars;
			string newNonce = nonce.PadRight(nonce.Length + numPadChars, '=');

			try {
				byte[] decodedBytes = Convert.FromBase64String(newNonce);
				string expireStr = new ASCIIEncoding().GetString(decodedBytes);
				expireTime = DateTime.Parse(expireStr);
			}
			catch (FormatException) {
				return false;
			}

			return (DateTime.Now <= expireTime);
		}

		protected virtual bool GetUserByName (HttpApplication app, string username,
									   out string password, out string[] roles)
		{
			password = String.Empty;
			roles = new string[0];

			string userFileName = app.Request.MapPath (ConfigurationSettings.AppSettings ["Digest.Users"]);
			if (userFileName == null || !File.Exists (userFileName))
				return false;

			XmlDocument userDoc = new XmlDocument ();
			userDoc.Load (userFileName);

			string xPath = String.Format ("/users/user[@name='{0}']", username);
			XmlNode user = userDoc.SelectSingleNode (xPath);

			if (user == null)
				return false;

			password = user.Attributes ["password"].Value;

			XmlNodeList roleNodes = user.SelectNodes ("role");
			roles = new string [roleNodes.Count];
			int i = 0;
			foreach (XmlNode xn in roleNodes)
				roles [i++] = xn.Attributes ["name"].Value;

			return true;
		}

		protected override bool AcceptCredentials (HttpApplication app, string authentication) 
		{
			// digest
			ListDictionary reqInfo = new ListDictionary ();

			string[] elems = authentication.Split( new char[] {','});
			foreach (string elem in elems) {
				// form key="value"
				string[] parts = elem.Split (new char[] {'='}, 2);
				string key = parts [0].Trim (new char[] {' ','\"'});
				string val = parts [1].Trim (new char[] {' ','\"'});
				reqInfo.Add (key,val);
			}

			string username = (string) reqInfo ["username"];
			string password;
			string[] roles;

			if (!GetUserByName (app, username, out password, out roles))
				return false;

			string realm = ConfigurationSettings.AppSettings ["Digest.Realm"];

			// calculate the Digest hashes

			// A1 = unq(username-value) ":" unq(realm-value) ":" passwd
			string A1 = String.Format ("{0}:{1}:{2}", username, realm, password);

			// H(A1) = MD5(A1)
			string HA1 = GetMD5HashBinHex (A1);

			// A2 = Method ":" digest-uri-value
			string A2 = String.Format ("{0}:{1}", app.Request.HttpMethod, (string)reqInfo["uri"]);

			// H(A2)
			string HA2 = GetMD5HashBinHex(A2);

			// KD(secret, data) = H(concat(secret, ":", data))
			// if qop == auth:
			// request-digest  = <"> < KD ( H(A1),     unq(nonce-value)
			//                              ":" nc-value
			//                              ":" unq(cnonce-value)
			//                              ":" unq(qop-value)
			//                              ":" H(A2)
			//                            ) <">
			// if qop is missing,
			// request-digest  = <"> < KD ( H(A1), unq(nonce-value) ":" H(A2) ) > <">

			string unhashedDigest;
			if (reqInfo["qop"] != null) {
				unhashedDigest = String.Format("{0}:{1}:{2}:{3}:{4}:{5}",
					HA1,
					(string)reqInfo["nonce"],
					(string)reqInfo["nc"],
					(string)reqInfo["cnonce"],
					(string)reqInfo["qop"],
					HA2);
			}
			else {
				unhashedDigest = String.Format("{0}:{1}:{2}",
					HA1,
					(string)reqInfo["nonce"],
					HA2);
			}

			string hashedDigest = GetMD5HashBinHex (unhashedDigest);

			bool isNonceStale = !IsValidNonce((string)reqInfo["nonce"]);
			app.Context.Items["staleNonce"] = isNonceStale;

			bool result = (((string)reqInfo["response"] == hashedDigest) && (!isNonceStale));
			if (result) {
				IIdentity id = new GenericIdentity (username, AuthenticationMethod);
				app.Context.User = new GenericPrincipal (id, roles);
			}
			return result;
		}

		#region Event Handlers

		public override void OnEndRequest(object source, EventArgs eventArgs)
		{
			// We add the WWW-Authenticate header here, so if an authorization 
			// fails elsewhere than in this module, we can still request authentication 
			// from the client.

			HttpApplication app = (HttpApplication) source;
			if (app.Response.StatusCode != 401 || !AuthenticationRequired)
				return;
				
			string realm = ConfigurationSettings.AppSettings ["Digest.Realm"];
			string nonce = GetCurrentNonce ();
			bool isNonceStale = false;
			object staleObj = app.Context.Items ["staleNonce"];
			if (staleObj != null)
				isNonceStale = (bool)staleObj;

			StringBuilder challenge = new StringBuilder ("Digest realm=\"");
			challenge.Append(realm);
			challenge.Append("\"");
			challenge.Append(", nonce=\"");
			challenge.Append(nonce);
			challenge.Append("\"");
			challenge.Append(", opaque=\"0000000000000000\"");
			challenge.Append(", stale=");
			challenge.Append(isNonceStale ? "true" : "false");
			challenge.Append(", algorithm=MD5");
			challenge.Append(", qop=\"auth\"");

			app.Response.AppendHeader("WWW-Authenticate", challenge.ToString());
			app.Response.StatusCode = 401;
		}

		#endregion

		private string GetMD5HashBinHex (string toBeHashed)
		{
			MD5 hash = MD5.Create ();
			byte[] result = hash.ComputeHash (Encoding.ASCII.GetBytes (toBeHashed));

			StringBuilder sb = new StringBuilder ();
			foreach (byte b in result)
				sb.Append (b.ToString ("x2"));
			return sb.ToString ();
		}

		protected virtual string GetCurrentNonce ()
		{
			DateTime nonceTime = DateTime.Now.AddSeconds (nonceLifetime);
			byte[] expireBytes = Encoding.ASCII.GetBytes (nonceTime.ToString ("G"));
			string nonce = Convert.ToBase64String (expireBytes);
			// nonce can't end in '=', so trim them from the end
			nonce = nonce.TrimEnd (trim);
			return nonce;
		}
	}
}
