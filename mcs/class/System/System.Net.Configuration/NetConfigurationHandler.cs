//
// System.Net.Configuration.NetConfigurationHandler.cs
//
// Authors:
//	Jerome Laban (jlaban@wanadoo.fr)
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2003 Ximian, Inc (http://www.ximian.com)
// (c) 2004 Novell, Inc. (http://www.novell.com)
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

using System.Collections;
using System.Configuration;
#if (XML_DEP)
using System.Xml;
#else
using XmlNode = System.Object;
#endif

namespace System.Net.Configuration
{
	class NetConfigurationHandler : IConfigurationSectionHandler
	{
		public virtual object Create (object parent, object configContext, XmlNode section)
		{
			NetConfig config = new NetConfig ();
#if (XML_DEP)
			if (section.Attributes != null && section.Attributes.Count != 0)
				HandlersUtil.ThrowException ("Unrecognized attribute", section);

			XmlNodeList reqHandlers = section.ChildNodes;
			foreach (XmlNode child in reqHandlers) {
				XmlNodeType ntype = child.NodeType;
				if (ntype == XmlNodeType.Whitespace || ntype == XmlNodeType.Comment)
					continue;

				if (ntype != XmlNodeType.Element)
					HandlersUtil.ThrowException ("Only elements allowed", child);
				
				string name = child.Name;
				if (name == "ipv6") {
					string enabled = HandlersUtil.ExtractAttributeValue ("enabled", child, false);
					if (child.Attributes != null && child.Attributes.Count != 0)
						HandlersUtil.ThrowException ("Unrecognized attribute", child);

					if (enabled == "true")
						config.ipv6Enabled = true;
					else if (enabled != "false")
						HandlersUtil.ThrowException ("Invalid boolean value", child);
						
					continue;
				}

				if (name == "httpWebRequest") {
					string max = HandlersUtil.ExtractAttributeValue
								("maximumResponseHeadersLength", child, true);

					// this one is just ignored
					HandlersUtil.ExtractAttributeValue ("useUnsafeHeaderParsing", child, true);

					if (child.Attributes != null && child.Attributes.Count != 0)
						HandlersUtil.ThrowException ("Unrecognized attribute", child);

					try {
						if (max != null) {
							int val = Int32.Parse (max.Trim ());
							if (val < -1)
								HandlersUtil.ThrowException ("Must be -1 or >= 0", child);

							config.MaxResponseHeadersLength = val;
						}
					} catch {
						HandlersUtil.ThrowException ("Invalid int value", child);
					}

					continue;
				}

				HandlersUtil.ThrowException ("Unexpected element", child);
			}
#endif			

			return config;
		}
	}
}
