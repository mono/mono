//
// System.Web.Configuration.HttpHandlersSectionHandler
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

using System.Collections;
using System.Configuration;
using System.Xml;
using System.Globalization;

namespace System.Web.Configuration
{
	class HttpHandlersSectionHandler : IConfigurationSectionHandler
	{
		public virtual object Create (object parent, object configContext, XmlNode section)
		{
			HandlerFactoryConfiguration mapper;
			
			if (parent is HandlerFactoryConfiguration)
				mapper = new HandlerFactoryConfiguration ((HandlerFactoryConfiguration) parent);
			else
				mapper = new HandlerFactoryConfiguration (null);
			
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

					mapper.Clear ();
					continue;
				}
					
				string verb = HandlersUtil.ExtractAttributeValue ("verb", child);
				string path = HandlersUtil.ExtractAttributeValue ("path", child);
				string validateStr = HandlersUtil.ExtractAttributeValue ("validate", child, true);
				bool validate;
				if (validateStr == null) {
					validate = true;
				} else {
#if NET_2_0
					validate = "true" == validateStr.ToLower (CultureInfo.InvariantCulture);
					if (!validate && "false" != validateStr.ToLower (CultureInfo.InvariantCulture))
#else
					validate = validateStr == "true";
					if (!validate && validateStr != "false")
#endif
						HandlersUtil.ThrowException (
								"Invalid value for validate attribute.", child);
				}

				if (name == "add") {
					string type = HandlersUtil.ExtractAttributeValue ("type", child);
					if (child.Attributes != null && child.Attributes.Count != 0)
						HandlersUtil.ThrowException ("Unrecognized attribute", child);

					mapper.Add (verb, path, type, validate);
					continue;
				}

				if (name == "remove") {
					if (child.Attributes != null && child.Attributes.Count != 0)
						HandlersUtil.ThrowException ("Unrecognized attribute", child);

					if (validate && false == mapper.Remove (verb, path))
						HandlersUtil.ThrowException ("There's no mapping to remove", child);
					
					continue;
				}
				HandlersUtil.ThrowException ("Unexpected element", child);
			}

			return mapper;
		}
	}

	internal class HandlersUtil
	{
		HandlersUtil ()
		{
		}

		static internal string ExtractAttributeValue (string attKey, XmlNode node)
		{
			return ExtractAttributeValue (attKey, node, false);
		}
			
		static internal string ExtractAttributeValue (string attKey, XmlNode node, bool optional)
		{
			return ExtractAttributeValue (attKey, node, optional, false);
		}
		
		static internal string ExtractAttributeValue (string attKey, XmlNode node, bool optional,
							      bool allowEmpty)
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
			if (!allowEmpty && value == String.Empty) {
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

