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

namespace System.Data.Common
{
	public class DbProviderFactoriesConfigurationHandler : IConfigurationSectionHandler
	{
		#region Constructors

		public DbProviderFactoriesConfigurationHandler ()
		{
		}

		#endregion // Constructors

		#region Methods

		public virtual object Create (object parent, object configContext, XmlNode section)
		{
			DataSet ds = parent as DataSet ?? CreateDataSet ();
			FillDataTables (ds, section);
			return ds;
		}

		DataSet CreateDataSet ()
		{
			DataSet ds = new DataSet (DbProviderFactories.CONFIG_SECTION_NAME);
			DataTable dt = ds.Tables.Add (DbProviderFactories.CONFIG_SEC_TABLE_NAME);
			DataColumn [] columns = new DataColumn [4];
			columns [0] = new DataColumn ("Name", typeof (string));
			columns [1] = new DataColumn ("Description", typeof (string));
			columns [2] = new DataColumn ("InvariantName", typeof (string));
			columns [3] = new DataColumn ("AssemblyQualifiedName", typeof (string));
			dt.Columns.AddRange (columns);
			dt.PrimaryKey = new DataColumn [] { columns [2] };
			return ds;
		}

		void FillDataTables (DataSet ds, XmlNode section)
		{
			DataTable dt = ds.Tables [0];
			foreach (XmlNode node in section.ChildNodes) {
				if (node.NodeType != XmlNodeType.Element)
					continue;

				if (node.Name == DbProviderFactories.CONFIG_SEC_TABLE_NAME) {
					foreach (XmlNode factoryNode in node.ChildNodes) {
						if (factoryNode.NodeType != XmlNodeType.Element)
							continue;

						switch (factoryNode.Name) {
						case "add":
							AddRow (dt, factoryNode);
							break;
						case "clear":
							dt.Rows.Clear ();
							break;
						case "remove":
							RemoveRow (dt, factoryNode);
							break;
						default:
							throw new ConfigurationErrorsException (
								"Unrecognized element.", factoryNode);
						}
					}
				}
			}
		}

		string GetAttributeValue (XmlNode node, string name, bool required)
		{
			XmlAttribute attr = node.Attributes [name];
			if (attr == null) {
				if (!required)
					return null;
				throw new ConfigurationErrorsException ("Required Attribute '" 
					+ name + "' is  missing!", node);
			}
			string value = attr.Value;
			if (value == "")
				throw new ConfigurationException ("Attribute '" + name 
					+ "' cannot be empty!", node);
			return value;
		}

		void AddRow (DataTable dt, XmlNode addNode)
		{
			string name = GetAttributeValue (addNode, "name", true);
			string description = GetAttributeValue (addNode, "description", true);
			string invariant = GetAttributeValue (addNode, "invariant", true);
			string type = GetAttributeValue (addNode, "type", true);

			// FIXME: throw ConfigurationErrorsException for unrecognized
			// attributes. Consider "supports" valid although we're not using
			// it

			DataRow row = dt.NewRow ();
			row [0] = name;
			row [1] = description;
			row [2] = invariant;
			row [3] = type;

			dt.Rows.Add (row);
		}

		void RemoveRow (DataTable dt, XmlNode removeNode)
		{
			// FIXME: throw ConfigurationErrorsException for unrecognized
			// attributes.

			string invariant = GetAttributeValue (removeNode, "invariant", true);
			DataRow row = dt.Rows.Find (invariant);
			if (row != null)
				row.Delete ();
		}

		#endregion // Methods
	}
}

#endif // NET_2_0
