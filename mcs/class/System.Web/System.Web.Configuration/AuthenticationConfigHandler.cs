//
// System.Web.Configuration.AuthenticationSectionHandler
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
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
using System.Configuration;
using System.Xml;

namespace System.Web.Configuration
{
	class AuthenticationConfigHandler : IConfigurationSectionHandler
	{
		public object Create (object parent, object context, XmlNode section)
		{
			AuthConfig config = new AuthConfig (parent);

			string mode = AttValue ("mode", section);
			if (mode != null)
				config.SetMode (mode);
			
			if (section.Attributes != null && section.Attributes.Count != 0)
				ThrowException ("Unrecognized attribute", section);

			XmlNodeList authNodes = section.ChildNodes;
			foreach (XmlNode child in authNodes) {
				XmlNodeType ntype = child.NodeType;
				if (ntype != XmlNodeType.Element)
					continue;
				
				if (child.Name == "forms") {
					config.CookieName = AttValue ("name", child);
					config.CookiePath = AttValue ("path", child);
					config.LoginUrl = AttValue ("loginUrl", child);
					config.SetProtection (AttValue ("protection", child));
					config.SetTimeout (AttValue ("timeout", child));
#if NET_1_1
					string att = AttValue ("requireSSL", child);
					if (att != null) {
						if (att == "true") {
							config.RequireSSL = true;
						} else if (att == "false") {
							config.RequireSSL = false;
						} else {
							HandlersUtil.ThrowException
								("Invalid value for RequireSSL", child);
						}
					}

					att = AttValue ("slidingExpiration", child);
					if (att != null) {
						if (att == "true") {
							config.SlidingExpiration = true;
						} else if (att == "false") {
							config.SlidingExpiration = false;
						} else {
							HandlersUtil.ThrowException
								("Invalid value for SlidingExpiration", child);
						}
					}
#endif

					ReadCredentials (child.ChildNodes, config);
					continue;
				}

				if (child.Name == "passport") {
					continue;
				}

				HandlersUtil.ThrowException ("Unexpected element", child);
			}

			return config;
		}

		static void ReadCredentials (XmlNodeList nodes, AuthConfig config)
		{
			foreach (XmlNode child in nodes) {
				XmlNodeType ntype = child.NodeType;
				if (ntype != XmlNodeType.Element)
					continue;

				if (child.Name != "credentials")
					HandlersUtil.ThrowException ("Unexpected element", child);

				config.SetPasswordFormat (AttValue ("passwordFormat", child));
				ReadUsers (child.ChildNodes, config.CredentialUsers);
			}
		}

		static void ReadUsers (XmlNodeList nodes, Hashtable users)
		{
			foreach (XmlNode child in nodes) {
				XmlNodeType ntype = child.NodeType;
				if (ntype != XmlNodeType.Element)
					continue;

				if (child.Name != "user")
					HandlersUtil.ThrowException ("Unexpected element", child);

				string name = AttValue ("name", child, false);
				string password = AttValue ("password", child);
				if (users.ContainsKey (name))
					ThrowException ("User '" + name + "' already added.", child);

				users [name] = password;
				if (child.HasChildNodes)
					ThrowException ("Child nodes not allowed here", child.FirstChild);
			}
		}
		// A few methods to save some typing
		static string AttValue (string name, XmlNode node, bool optional)
		{
			return HandlersUtil.ExtractAttributeValue (name, node, optional);
		}

		static string AttValue (string name, XmlNode node)
		{
			return HandlersUtil.ExtractAttributeValue (name, node, true);
		}

		static void ThrowException (string message, XmlNode node)
		{
			HandlersUtil.ThrowException (message, node);
		}
		//

	}
}

