//
// System.Web.Configuration.AuthenticationSectionHandler
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
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
			//TODO: context?
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
					ReadCredentials (child.ChildNodes, config);
					continue;
				}

				if (child.Name == "passport") {
					Console.WriteLine ("**WARNING**: Passport not supported! Ignoring section.");
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

