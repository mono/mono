//
// System.Net.Configuration.DefaultProxyHandler
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2003 Ximian, Inc (http://www.ximian.com)
//

using System.Collections;
using System.Configuration;
using System.Xml;

namespace System.Net.Configuration
{
	class DefaultProxyHandler : IConfigurationSectionHandler
	{
		public virtual object Create (object parent, object configContext, XmlNode section)
		{
			IWebProxy result = parent as IWebProxy;
			
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
					string deflt = HandlersUtil.ExtractAttributeValue ("usesystemdefault", child, true);
					string bypass = HandlersUtil.ExtractAttributeValue ("bypassonlocal", child, true);
					string address = HandlersUtil.ExtractAttributeValue ("proxyaddress", child, true);
					if (child.Attributes != null && child.Attributes.Count != 0) {
						foreach (XmlAttribute n in child.Attributes)
							Console.WriteLine ("att: {0}", n.Name);
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
					if (address == null)
						continue;

					try {
						((WebProxy) result).Address = new Uri (address);
					} catch (Exception) {
						HandlersUtil.ThrowException ("invalid uri", child);
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
					Console.WriteLine ("WARNING: module not implemented yet");
					continue;
				}

				HandlersUtil.ThrowException ("Unexpected element", child);
			}

			return result;
		}

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
						Console.WriteLine ("added: {0}", address);
						bypass.Add (address);
					}
					continue;
				}

				if (name == "remove") {
					string address = HandlersUtil.ExtractAttributeValue ("address", child);
					bypass.Remove (address);
						Console.WriteLine ("removed: {0}", address);
					continue;
				}

				if (name == "clear") {
					if (node.Attributes != null && node.Attributes.Count != 0)
						HandlersUtil.ThrowException ("Unrecognized attribute", node);
					
					bypass.Clear ();
						Console.WriteLine ("clear");
					continue;
				}

				HandlersUtil.ThrowException ("Unexpected element", child);
			}

			proxy.BypassList = (string []) bypass.ToArray (typeof (string));
		}
	}
}

