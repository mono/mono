//
// Basic Authentication implementation
//
// Authors:
//	Greg Reinacker (gregr@rassoc.com)
//	Sebastien Pouliot (spouliot@motus.com)
//
// Copyright 2002-2003 Greg Reinacker, Reinacker & Associates, Inc. All rights reserved.
// Portions (C) 2003 Motus Technologies Inc. (http://www.motus.com)
//
// Based on "DigestAuthenticationModule.cs". Original source code available at
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
using System.Configuration;
using System.IO;
using System.Security.Principal;
using System.Text;
using System.Web;
using System.Xml;

namespace Mono.Http.Modules
{
	public class BasicAuthenticationModule : AuthenticationModule
	{
		static char[] separator = {':'};

		public BasicAuthenticationModule () : base ("Basic") {}

		protected override bool AcceptCredentials (HttpApplication app, string authentication) 
		{
			byte[] userpass = Convert.FromBase64String (authentication);
			string[] up = Encoding.UTF8.GetString (userpass).Split (separator);
			string username = up [0];
			string password = up [1];

			string userFileName = app.Request.MapPath (ConfigurationSettings.AppSettings ["Basic.Users"]);
			if (userFileName == null || !File.Exists (userFileName))
				return false;

			XmlDocument userDoc = new XmlDocument ();
			userDoc.Load (userFileName);

			string xPath = String.Format ("/users/user[@name='{0}']", username);
			XmlNode user = userDoc.SelectSingleNode (xPath);

			if (user == null)
				return false;

			XmlAttribute att = user.Attributes ["password"];
			if (att == null || password != att.Value)
				return false;

			XmlNodeList roleNodes = user.SelectNodes ("role");
			string[] roles = new string [roleNodes.Count];
			int i = 0;
			foreach (XmlNode xn in roleNodes) {
				XmlAttribute rolename = xn.Attributes ["name"];
				if (rolename == null)
					continue;

				roles [i++] = rolename.Value;
			}
			app.Context.User = new GenericPrincipal (new GenericIdentity (username, AuthenticationMethod), roles);
			return true;
		}

		#region Event Handlers

		// We add the WWW-Authenticate header here, so if an authorization 
		// fails elsewhere than in this module, we can still request authentication 
		// from the client.
		public override void OnEndRequest (object source, EventArgs eventArgs)
		{
			HttpApplication app = (HttpApplication) source;
			if (app.Response.StatusCode != 401 || !AuthenticationRequired)
				return;

			string realm = ConfigurationSettings.AppSettings ["Basic.Realm"];
			string challenge = String.Format ("{0} realm=\"{1}\"", AuthenticationMethod, realm);
			app.Response.AppendHeader ("WWW-Authenticate", challenge);
		}

		#endregion
	}
}
