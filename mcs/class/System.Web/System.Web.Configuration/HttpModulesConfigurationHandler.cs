//
// System.Web.Configuration.HttpModulesConfigurationHandler
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

using System.Configuration;
using System.Xml;
using System.Web.Security;

namespace System.Web.Configuration
{
	class HttpModulesConfigurationHandler : IConfigurationSectionHandler
	{
		public virtual object Create (object parent, object configContext, XmlNode section)
		{
			ModulesConfiguration mapper;
			
			if (parent is ModulesConfiguration)
				mapper = new ModulesConfiguration ((ModulesConfiguration) parent);
			else
				mapper = new ModulesConfiguration (null);
			
			if (section.Attributes != null && section.Attributes.Count != 0)
				HandlersUtil.ThrowException ("Unrecognized attribute", section);

			XmlNodeList httpModules = section.ChildNodes;

			foreach (XmlNode child in httpModules) {
				XmlNodeType ntype = child.NodeType;
				if (ntype == XmlNodeType.Whitespace || ntype == XmlNodeType.Comment)
					continue;

				if (ntype != XmlNodeType.Element)
					HandlersUtil.ThrowException ("Only elements allowed", child);

				string name = child.Name;
				if (name == "clear") {
					if (child.Attributes.Count != 0)
						HandlersUtil.ThrowException ("Unrecognized attribute", child);

					mapper.Clear ();
					continue;
				}

				string name_attr = HandlersUtil.ExtractAttributeValue ("name", child);
				if (name == "add") {
					string type = HandlersUtil.ExtractAttributeValue ("type", child);
					if (child.Attributes.Count != 0)
						HandlersUtil.ThrowException ("Unrecognized attribute", child);

					// FIXME: gotta remove this. Just here to make it work with my local config
					if (type.StartsWith ("System.Web.Mobile"))
						continue;

					mapper.Add (name_attr, type);
					continue;
				}

				if (name == "remove") {
					if (child.Attributes.Count != 0)
						HandlersUtil.ThrowException ("Unrecognized attribute", child);

					if (mapper.Remove (name_attr) == null)
						HandlersUtil.ThrowException ("Module not loaded", child);
					continue;
				}

				HandlersUtil.ThrowException ("Unrecognized element", child);
			}

			mapper.Add ("DefaultAuthentication", typeof (DefaultAuthenticationModule));
			
			return mapper;
		}
	}
}

