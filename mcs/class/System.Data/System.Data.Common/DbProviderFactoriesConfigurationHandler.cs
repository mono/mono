//
// System.Data.Common.DbProviderFactoriesConfigurationHandler.cs
//
// Author:
//   Sureshkumar T (tsureshkumar@novell.com)
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2003
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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
using System.Globalization;
using System.Configuration;

namespace System.Data.Common {
	public class DbProviderFactoriesConfigurationHandler : IConfigurationSectionHandler
	{
		#region Constructors

		[MonoTODO]
		public DbProviderFactoriesConfigurationHandler ()
		{
		}

		#endregion // Constructors

		#region Methods

		[MonoTODO]
		public virtual object Create (object parent, object configContext, XmlNode section)
		{
                        DataSet ds = new DataSet ("DbProviderFactories");
                        CreateDataTables (ds, section);
                        return ds;
		}

                internal virtual void CreateDataTables (DataSet ds, XmlNode section) 
                {                        
                        DataTable dt = ds.Tables.Add ("DbProviderFactories");
                        DataColumn [] columns = new DataColumn [5];
                        columns [0] = new DataColumn ("Name", typeof (string));
                        columns [1] = new DataColumn ("Description", typeof (string));
                        columns [2] = new DataColumn ("InvariantName", typeof (string));
                        columns [3] = new DataColumn ("AssemblyQualifiedName", typeof (string));
                        columns [4] = new DataColumn ("SupportedClasses", typeof (int));
                        dt.Columns.AddRange (columns);
                        dt.PrimaryKey = new DataColumn [] {columns [2]};
                                
                        foreach (XmlNode node in section.SelectNodes (".//DbProviderFactories")) {
                                AddRows (dt, node);
                                RemoveRows (dt, node);
                       }
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

                internal virtual void AddRows (DataTable dt, XmlNode factoriesNode)
                {
                        foreach (XmlNode addNode in factoriesNode.SelectNodes (".//add")) {
                                if (addNode.NodeType != XmlNodeType.Element)
                                        continue;
                                
                                string name = "";
                                string description = "";
                                string invariant = "";
                                string type = "";
                                string supportedClasses = "";
                                int support;

                                name            = GetAttributeValue (addNode, "name");
                                description     = GetAttributeValue (addNode, "description");
                                invariant       = GetAttributeValue (addNode, "invariant");
                                type            = GetAttributeValue (addNode, "type");
                                supportedClasses = GetAttributeValue (addNode, "support");
                                
                                support = int.Parse (supportedClasses, NumberStyles.HexNumber);
                                        
                                DataRow row = dt.NewRow ();
                                row [0] = name;
                                row [1] = description;
                                row [2] = invariant;
                                row [3] = type;
                                row [4] = support;
                                        
                                dt.Rows.Add (row);
                        }        
                }

                internal virtual void RemoveRows (DataTable dt, XmlNode removeNode)
                {
                        foreach (XmlNode node in removeNode.SelectNodes (".//remove")) {
                                if (node.NodeType != XmlNodeType.Element)
                                        continue;
                                
                                string invariant = GetAttributeValue (node, "invariant");
                                DataRow row = dt.Rows.Find (invariant);
                                if (row != null)
                                        row.Delete ();
                        }
                }
                

		#endregion // Methods
	}
}

#endif // NET_2_0
