//
// AcceptEncodingSectionHandler.cs
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (c) 2003 Ximian, Inc (http://www.ximian.com)
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
using System.Xml;

namespace Mono.Http.Configuration
{
	public class AcceptEncodingSectionHandler : IConfigurationSectionHandler
	{
		public object Create (object parent, object configContext, XmlNode section)
		{
			AcceptEncodingConfig cfg = new AcceptEncodingConfig (parent as AcceptEncodingConfig);
			
			if (section.Attributes != null && section.Attributes.Count != 0)
				ThrowException ("Unrecognized attribute", section);

			XmlNodeList nodes = section.ChildNodes;
			foreach (XmlNode child in nodes) {
				XmlNodeType ntype = child.NodeType;
				if (ntype == XmlNodeType.Whitespace || ntype == XmlNodeType.Comment)
					continue;

				if (ntype != XmlNodeType.Element)
					ThrowException ("Only elements allowed", child);
				
				string name = child.Name;
				if (name == "clear") {
					if (child.Attributes != null && child.Attributes.Count != 0)
						ThrowException ("Unrecognized attribute", child);

					cfg.Clear ();
					continue;
				}
					
				if (name != "add")
					ThrowException ("Unexpected element", child);

				string encoding = ExtractAttributeValue ("encoding", child, false);
				string type = ExtractAttributeValue ("type", child, false);
				string disabled = ExtractAttributeValue ("disabled", child, true);
				if (disabled != null && disabled == "yes")
					continue;

				if (child.Attributes != null && child.Attributes.Count != 0)
					ThrowException ("Unrecognized attribute", child);

				cfg.Add (encoding, type);
			}

			return cfg;
		}

		static void ThrowException (string msg, XmlNode node)
		{
			if (node != null && node.Name != String.Empty)
				msg = msg + " (node name: " + node.Name + ") ";

			throw new ConfigurationException (msg, node);
		}

		static string ExtractAttributeValue (string attKey, XmlNode node, bool optional)
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
	}
}

