//
// System.Net.Configuration.ConnectionManagementHandler
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
	class ConnectionManagementData
	{
		Hashtable data; // key -> address, value -> maxconnections
		
		public ConnectionManagementData (object parent)
		{
			data = new Hashtable ();
			if (parent != null && parent is ConnectionManagementData) {
				ConnectionManagementData p = (ConnectionManagementData) parent;
				foreach (string k in p.data.Keys)
					data [k] = p.data [k];	
			}
		}

		public void Add (string address, string nconns)
		{
			if (nconns == null || nconns == "")
				nconns = "2";
			// Adding duplicates works fine under MS, so...
			data [address] = UInt32.Parse (nconns);
		}

		public void Remove (string address)
		{
			// Removing non-existent address is fine.
			data.Remove (address);
		}

		public void Clear ()
		{
			data.Clear ();
		}

		public Hashtable Data {
			get { return data; }
		}
	}

	class ConnectionManagementHandler : IConfigurationSectionHandler
	{
		public virtual object Create (object parent, object configContext, XmlNode section)
		{
			ConnectionManagementData cmd = new ConnectionManagementData (parent);
			
			if (section.Attributes != null && section.Attributes.Count != 0)
				HandlersUtil.ThrowException ("Unrecognized attribute", section);

			XmlNodeList httpHandlers = section.ChildNodes;
			foreach (XmlNode child in httpHandlers) {
				XmlNodeType ntype = child.NodeType;
				if (ntype == XmlNodeType.Whitespace || ntype == XmlNodeType.Comment)
					continue;

				if (ntype != XmlNodeType.Element)
					HandlersUtil.ThrowException ("Only elements allowed", child);
				
				string name = child.Name;
				if (name == "clear") {
					if (child.Attributes != null && child.Attributes.Count != 0)
						HandlersUtil.ThrowException ("Unrecognized attribute", child);

					cmd.Clear ();
					continue;
				}

				//LAMESPEC: the MS doc says that <remove name="..."/> but they throw an exception
				// if you use that. "address" is correct.

				string address = HandlersUtil.ExtractAttributeValue ("address", child);
				if (name == "add") {
					string maxcnc = HandlersUtil.ExtractAttributeValue ("maxconnection", child, true);
					if (child.Attributes != null && child.Attributes.Count != 0)
						HandlersUtil.ThrowException ("Unrecognized attribute", child);

					cmd.Add (address, maxcnc);
					continue;
				}

				if (name == "remove") {
					if (child.Attributes != null && child.Attributes.Count != 0)
						HandlersUtil.ThrowException ("Unrecognized attribute", child);

					cmd.Remove (address);
					continue;
				}

				HandlersUtil.ThrowException ("Unexpected element", child);
			}

			return cmd;
		}
	}

	internal class HandlersUtil
	{
		private HandlersUtil ()
		{
		}

		static internal string ExtractAttributeValue (string attKey, XmlNode node)
		{
			return ExtractAttributeValue (attKey, node, false);
		}
			
		static internal string ExtractAttributeValue (string attKey, XmlNode node, bool optional)
		{
			if (node.Attributes == null) {
				if (optional)
					return null;

				ThrowException ("Required attribute not found: " + attKey, node);
			}

			XmlNode att = node.Attributes.RemoveNamedItem (attKey);
			if (att == null) {
				if (optional)
					return null;
				ThrowException ("Required attribute not found: " + attKey, node);
			}

			string value = att.Value;
			if (value == String.Empty) {
				string opt = optional ? "Optional" : "Required";
				ThrowException (opt + " attribute is empty: " + attKey, node);
			}

			return value;
		}

		static internal void ThrowException (string msg, XmlNode node)
		{
			if (node != null && node.Name != String.Empty)
				msg = msg + " (node name: " + node.Name + ") ";
			throw new ConfigurationException (msg, node);
		}
	}
}

