//
// System.Data.Common.ConnectionStringsSectionHandler.cs
//
// Author:
//   Sureshkumar T (tsureshkumar@novell.com)
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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

#if NET_2_0

using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Globalization;
using System.Configuration;

namespace System.Data.Common {

        /// <summary>
        /// This is a handler class for the configuration section <b>connectionStrings</b>.
        /// This handler is a temporary solution till the new 2.0 configuration API is supported
        /// in mono.
        /// </summary>
	public class ConnectionStringsSectionHandler : IConfigurationSectionHandler
	{
		#region Constructors

		public ConnectionStringsSectionHandler ()
		{
		}

		#endregion // Constructors

		#region Methods

		public virtual object Create (object parent, object configContext, XmlNode section)
		{
                        ConnectionStringsSection csSection  = new ConnectionStringsSection ();
                        ConnectionStringSettingsCollection csList = csSection.ConnectionStrings;
                        foreach (XmlNode addNode in section.SelectNodes (".//add")) {
                                if (addNode.NodeType != XmlNodeType.Element)
                                        continue;
                                string name;
                                string providerName;
                                string connectionString;
                                
                                name = GetAttributeValue (addNode, "name");
                                providerName = GetAttributeValue (addNode, "providerName");
                                connectionString = GetAttributeValue (addNode, "connectionString");
                                
                                ConnectionStringSettings cs = new ConnectionStringSettings (name, connectionString, providerName);
                                csList.Add (cs);
                        }

                        return csSection;
                        
		}

                internal string GetAttributeValue (XmlNode node, string name)
                {
                        XmlAttribute attr = node.Attributes[name];
                        if (attr == null)
                                throw new ConfigurationException ("Required Attribute '" + name +
                                                                  "' is  missing!", node);
                        string value = attr.Value;
                        if (value == "")
                                throw new ConfigurationException ("Attribute '" + name + "' cannot be empty!",
                                                                  node);
                        return value;
                }


		#endregion // Methods
	}
}

#endif // NET_2_0
