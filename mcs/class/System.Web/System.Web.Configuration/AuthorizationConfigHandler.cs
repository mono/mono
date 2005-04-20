//
// System.Web.Configuration.AuthorizationConfigHandler
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

using System;
using System.Collections;
using System.Configuration;
using System.Xml;

namespace System.Web.Configuration
{
	class AuthorizationConfigHandler : IConfigurationSectionHandler
	{
		public object Create (object parent, object context, XmlNode section)
		{
			AuthorizationConfig config = new AuthorizationConfig (parent);

			if (section.Attributes != null && section.Attributes.Count != 0)
				ThrowException ("Unrecognized attribute", section);

			XmlNodeList authNodes = section.ChildNodes;
			foreach (XmlNode child in authNodes) {
				XmlNodeType ntype = child.NodeType;
				if (ntype != XmlNodeType.Element)
					continue;
				
				string childName = child.Name;
				bool allow = (childName == "allow");
				bool deny = (childName == "deny");
				if (!allow && !deny)
					ThrowException ("Element name must be 'allow' or 'deny'.", child);

				string users = AttValue ("users", child);
				string roles = AttValue ("roles", child);
				if (users == null && roles == null)
					ThrowException ("At least 'users' or 'roles' must be present.", child);

				string verbs = AttValue ("verbs", child);
				if (child.Attributes != null && child.Attributes.Count != 0)
					ThrowException ("Unrecognized attribute.", child);

				bool added;
				if (allow)
					added = config.Allow (users, roles, verbs);
				else
					added = config.Deny (users, roles, verbs);

				if (!added)
					ThrowException ("User and role names cannot contain '?' or '*'.", child);
			}

			return config;
		}

		// A few methods to save some typing
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

