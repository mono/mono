//
// mcs/class/System.Data/System.Data/XmlDataLoader.cs
//
// Purpose: Loads XmlDocument to DataSet 
//
// class: XmlDataLoader
// assembly: System.Data.dll
// namespace: System.Data
//
// Author:
//     Ville Palo <vi64pa@koti.soon.fi>
//
// (c)copyright 2002 Ville Palo
//
// XmlDataLoader is included within the Mono Class Library.
//

using System;
using System.Data;
using System.Xml;
using System.Xml.XPath;
using System.Collections;
using System.Globalization;

namespace System.Data {

        internal class XmlDataLoader
	{
	
	        private DataSet DSet;
		Hashtable DiffGrRows = new Hashtable ();

	        public XmlDataLoader (DataSet set) 
		{
			DSet = set;
		}

		public XmlReadMode LoadData (XmlReader reader, XmlReadMode mode)
		{
			XmlReadMode Result = XmlReadMode.Auto;

			switch (mode) {

			        case XmlReadMode.Fragment:
					break;
				case XmlReadMode.ReadSchema:
					Result = XmlReadMode.ReadSchema;
					ReadModeSchema (reader, false);
					break;
			        case XmlReadMode.IgnoreSchema:
					Result = XmlReadMode.IgnoreSchema;
					ReadModeSchema (reader, true);
					break;
			        case XmlReadMode.InferSchema:
					Result = XmlReadMode.InferSchema;
					ReadModeInferSchema (reader);
					break;
			        default:
					break;
			}

			return Result;
		}

		#region reading

		// XmlReadMode.InferSchema
		[MonoTODO]
		private void ReadModeInferSchema (XmlReader reader)
		{
			// root element is DataSets name
			reader.MoveToContent ();

			DSet.DataSetName = reader.LocalName;

			// And now comes tables
			while (reader.Read ()) {

				// skip possible inline-schema
				if (String.Compare (reader.LocalName, "schema", true) == 0 && reader.NodeType == XmlNodeType.Element) {
					while (reader.Read () && (reader.NodeType != XmlNodeType.EndElement 
								  || String.Compare (reader.LocalName, "schema", true) != 0));
				}


				if (reader.NodeType == XmlNodeType.Element) {
					
					string datatablename = reader.LocalName;
					DataTable table;
					bool NewTable = false;

					if (!DSet.Tables.Contains (datatablename)) {
						table = new DataTable (reader.LocalName);
						DSet.Tables.Add (table);
						NewTable = true;
					}
					else {
						table = DSet.Tables [datatablename];
					}

					Hashtable rowValue = new Hashtable ();

					while (reader.Read () && (reader.NodeType != XmlNodeType.EndElement 
								  || reader.LocalName != datatablename))
					{
						if (reader.NodeType == XmlNodeType.Element) {

							string dataColumnName = reader.LocalName;
							if (NewTable)
								table.Columns.Add (dataColumnName);

							// FIXME: exception?
							if (!reader.Read ())
								return;

							rowValue.Add (dataColumnName, reader.Value);
						}
					}
					
					DataRow row = table.NewRow ();
					
					IDictionaryEnumerator enumerator = rowValue.GetEnumerator ();
					while (enumerator.MoveNext ()) {
						row [enumerator.Key.ToString ()] = enumerator.Value.ToString ();
					}

					table.Rows.Add (row);
				}
			}			
		}

		// Read Xmldocument. XmlReadMode.ReadSchema and XmlReadMode.IgnoreSchema
		[MonoTODO]
		private void ReadModeSchema (XmlReader reader, bool IgnoreSchema)
		{
			/*\
			 *  Reads any inline schema, but an exception is thrown 
			 *  if any tables in the inline schema already exist in the DataSet.
			\*/

			reader.MoveToContent ();

			while (reader.Read ()) {

				// FIXME: possible inline-schema should be readed here
				if (String.Compare (reader.LocalName, "schema", true) == 0 && reader.NodeType == XmlNodeType.Element) {
					if (!IgnoreSchema)
						DSet.ReadXmlSchema (reader);
				}

				// find table
				if (reader.NodeType == XmlNodeType.Element && DSet.Tables.Contains (reader.LocalName)) {
					
					DataTable table = DSet.Tables [reader.LocalName];
					DataRow row = table.NewRow ();

					ReadColumns (reader, row, table, reader.LocalName);					

					table.Rows.Add (row);
				}
			}
		}

		#endregion // reading

		#region Private helper methods
		
		private void ReadColumns (XmlReader reader, DataRow row, DataTable table, string TableName)
		{
			do {
				if (reader.NodeType == XmlNodeType.Element && 
				    table.Columns.Contains (reader.LocalName)) {
					string columName = reader.LocalName;
					reader.Read ();
					row [columName] = reader.Value;
								}
				else {
					reader.Read ();
				}
				
			} while (table.TableName != reader.LocalName 
				 || reader.NodeType != XmlNodeType.EndElement);
		}

		#endregion // Private helper methods
	}
}
