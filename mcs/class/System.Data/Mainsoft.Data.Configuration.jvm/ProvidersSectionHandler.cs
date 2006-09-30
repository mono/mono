//
// System.Data.OleDb.OleDbConnection
//
// Authors:
//	Konstantin Triger <kostat@mainsoft.com>
//	Boris Kirzner <borisk@mainsoft.com>
//	
// (C) 2006 Mainsoft Corporation (http://www.mainsoft.com)
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
using System.Xml;
using System.Collections;
using System.Collections.Specialized;

namespace Mainsoft.Data.Configuration
{
	class ProvidersSectionHandler : IConfigurationSectionHandler
	{
		public virtual object Create (object parent, object configContext, XmlNode section) {
			if (section.Attributes != null && section.Attributes.Count != 0)
				ThrowException ("Unrecognized attribute", section);

			ArrayList providers = new ArrayList();
			
			XmlNodeList providerElements = section.SelectNodes ("provider");
			foreach (XmlNode child in providerElements) {
				XmlNodeType ntype = child.NodeType;
				if (ntype == XmlNodeType.Whitespace || ntype == XmlNodeType.Comment)
					continue;

				if (ntype != XmlNodeType.Element)
					ThrowException ("Only elements allowed", child);

				object parentProvider = null;
				XmlAttribute extends = child.Attributes["parent"];
				if (extends != null) {
					string super = extends.Value;
					string sectionName = super.Substring(0, super.IndexOf('#'));
					string id = super.Substring(sectionName.Length+1);
					IList supers = (IList) ConfigurationSettings.GetConfig(sectionName);
					for (int i = 0; i < supers.Count; i++) {
						Hashtable ht = (Hashtable)supers[i];
						string curId = (string)ht["id"];
						if (String.CompareOrdinal(id,curId) == 0) {
							parentProvider = ht;
							break;
						}
					}
					child.Attributes.Remove(extends);
				}

				DictionarySectionHandler handler = new DictionarySectionHandler();
				Hashtable col = (Hashtable) handler.Create (parentProvider, null, child);
				providers.Add(col);
			}	
			if (parent != null && parent is IList) {
				IList parentList = (IList)parent;
				for (int i = 0; i < parentList.Count; i++) {
					Hashtable htParent = (Hashtable)parentList[i];
					string parentId = (string)htParent["id"];
					bool overriden = false;
					for (int y = 0; y < providers.Count; y++) {
						Hashtable htMe = (Hashtable)providers[y];
						string myId = (string)htMe["id"];
						if (String.CompareOrdinal(parentId,myId) == 0) {
							overriden = true;
							foreach (object key in htParent.Keys)
								if (!htMe.ContainsKey(key))
									htMe[key] = htParent[key];
							break;
						}
					}

					if (!overriden)
						providers.Add(htParent);
				}
			}

			return providers;
		}
		
		static internal void ThrowException (string msg, XmlNode node) 
		{
			if (node != null && node.Name != String.Empty)
				msg = msg + " (node name: " + node.Name + ") ";
			throw new ConfigurationException (msg, node);
		}
	}
}
