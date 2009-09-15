//
// System.Net.Configuration.DefaultProxyHandler
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

using System.Collections;
using System.Configuration;
#if (XML_DEP)
using System.Xml;
#else
using XmlNode = System.Object;
#endif

namespace System.Net.Configuration
{
	class DefaultProxyHandler : IConfigurationSectionHandler
	{
		public virtual object Create (object parent, object configContext, XmlNode section)
		{
			IWebProxy result = parent as IWebProxy;
#if (XML_DEP)
			if (section.Attributes != null && section.Attributes.Count != 0)
				HandlersUtil.ThrowException ("Unrecognized attribute", section);

			XmlNodeList nodes = section.ChildNodes;
			foreach (XmlNode child in nodes) {
				XmlNodeType ntype = child.NodeType;
				if (ntype == XmlNodeType.Whitespace || ntype == XmlNodeType.Comment)
					continue;

				if (ntype != XmlNodeType.Element)
					HandlersUtil.ThrowException ("Only elements allowed", child);
				
				string name = child.Name;
				if (name == "proxy") {
					string sysdefault = HandlersUtil.ExtractAttributeValue ("usesystemdefault", child, true);
					string bypass = HandlersUtil.ExtractAttributeValue ("bypassonlocal", child, true);
					string address = HandlersUtil.ExtractAttributeValue ("proxyaddress", child, true);
					if (child.Attributes != null && child.Attributes.Count != 0) {
						HandlersUtil.ThrowException ("Unrecognized attribute", child);
					}

					result = new WebProxy ();
					bool bp = (bypass != null && String.Compare (bypass, "true", true) == 0);
					if (bp == false) {
						if (bypass != null && String.Compare (bypass, "false", true) != 0)
							HandlersUtil.ThrowException ("Invalid boolean value", child);
					}

					if (!(result is WebProxy))
						continue;

					((WebProxy) result).BypassProxyOnLocal = bp;
					
					if (address != null)
						try {
							((WebProxy) result).Address = new Uri (address);
							continue;
						} catch (UriFormatException) {} //MS: ignore bad URIs, fall through to default
					
					//MS: presence of valid address URI takes precedence over usesystemdefault
					if (sysdefault != null && String.Compare (sysdefault, "true", true) == 0) {
						address = Environment.GetEnvironmentVariable ("http_proxy");
						if (address == null)
							address = Environment.GetEnvironmentVariable ("HTTP_PROXY");

						if (address != null) {
							try {
								Uri uri = new Uri (address);
								IPAddress ip;
								if (IPAddress.TryParse (uri.Host, out ip)) {
									if (IPAddress.Any.Equals (ip)) {
										UriBuilder builder = new UriBuilder (uri);
										builder.Host = "127.0.0.1";
										uri = builder.Uri;
									} else if (IPAddress.IPv6Any.Equals (ip)) {
										UriBuilder builder = new UriBuilder (uri);
										builder.Host = "[::1]";
										uri = builder.Uri;
									}
								}
								((WebProxy) result).Address = uri;
							} catch (UriFormatException) { }
						}
					}
					
					continue;
				}

				if (name == "bypasslist") {
					if (!(result is WebProxy))
						continue;

					FillByPassList (child, (WebProxy) result);
					continue;
				}

				if (name == "module") {
					HandlersUtil.ThrowException ("WARNING: module not implemented yet", child);
				}

				HandlersUtil.ThrowException ("Unexpected element", child);
			}
#endif
			return result;
		}

#if (XML_DEP)
		static void FillByPassList (XmlNode node, WebProxy proxy)
		{
			ArrayList bypass = new ArrayList (proxy.BypassArrayList);
			if (node.Attributes != null && node.Attributes.Count != 0)
				HandlersUtil.ThrowException ("Unrecognized attribute", node);

			XmlNodeList nodes = node.ChildNodes;
			foreach (XmlNode child in nodes) {
				XmlNodeType ntype = child.NodeType;
				if (ntype == XmlNodeType.Whitespace || ntype == XmlNodeType.Comment)
					continue;

				if (ntype != XmlNodeType.Element)
					HandlersUtil.ThrowException ("Only elements allowed", child);
				
				string name = child.Name;
				if (name == "add") {
					string address = HandlersUtil.ExtractAttributeValue ("address", child);
					if (!bypass.Contains (address)) {
						bypass.Add (address);
					}
					continue;
				}

				if (name == "remove") {
					string address = HandlersUtil.ExtractAttributeValue ("address", child);
					bypass.Remove (address);
					continue;
				}

				if (name == "clear") {
					if (node.Attributes != null && node.Attributes.Count != 0)
						HandlersUtil.ThrowException ("Unrecognized attribute", node);
					
					bypass.Clear ();
					continue;
				}

				HandlersUtil.ThrowException ("Unexpected element", child);
			}

			proxy.BypassList = (string []) bypass.ToArray (typeof (string));
		}
#endif
	}
}

