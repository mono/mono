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

		public XmlReadMode LoadData (XmlReader r)
		{
			// FIXME: somekinda exception?
			if (!r.Read ())
				return XmlReadMode.Auto; // FIXME

			/*\
			 *  If document is diffgram we will use diffgram
			\*/
			if (r.LocalName == "diffgram")
				return LoadData (r, XmlReadMode.DiffGram);

			/*\
			 *  If we already have a schema, or the document 
			 *  contains an in-line schema, sets XmlReadMode to ReadSchema.
		        \*/

			// FIXME: is this always true: "if we have tables we have to have schema also"
			if (DSet.Tables.Count > 0)				
				return LoadData (r, XmlReadMode.ReadSchema);

			/*\
			 *  If we dont have a schema yet and document 
			 *  contains no inline-schema  mode is XmlReadMode.InferSchema
			\*/

			return LoadData (r, XmlReadMode.InferSchema);
		}

		public XmlReadMode LoadData (XmlReader reader, XmlReadMode mode)
		{
			XmlReadMode Result = XmlReadMode.Auto;

			switch (mode) {

			        case XmlReadMode.DiffGram:
					Result = XmlReadMode.DiffGram;
					ReadModeDiffGram (reader);
					break;
			        case XmlReadMode.Auto:
					LoadData (reader);
					break;
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

		// These methods should be in their own class for example XmlDiffLoader. But for now, let them be here
		#region diffgram-methods

		// Reads diffgr:before -values from diffgram
		private void ReadModeDiffGramBefore (XmlReader reader)
		{
			while (reader.Read ()) {

				if (String.Compare (reader.LocalName, "before", true) == 0) {

					while (reader.Read ()) {

						if (reader.NodeType == XmlNodeType.Element && DSet.Tables.Contains (reader.LocalName)) {
							
							string id = reader ["diffgr:id"];
							string TableName = reader.LocalName;
							DataTable table = DSet.Tables [TableName];
							DataRow row = table.NewRow ();

							ReadColumns (reader, row, table, TableName);

							table.Rows.Add (row);
							DiffGrRows.Add (id, row);
							row.AcceptChanges ();
						} 
						else if (reader.NodeType == XmlNodeType.Element) {
							throw new DataException (Locale.GetText ("Cannot load diffGram. Table '" + reader.LocalName + "' is missing in the destination dataset"));
						}
					}
				}
			}
		}

		// Reader current values from diffgram
		private void ReadModeDiffGramCurrent (XmlReader reader)
		{
			while (reader.Read ()) {

				if (reader.NodeType == XmlNodeType.Element) {

					if (DSet.Tables.Contains (reader.LocalName)) {
						
						string TableName = reader.LocalName;
						bool NewRow = false;
						DataTable table = DSet.Tables [TableName];
						DataRow row; 

						if (DiffGrRows.Contains (reader ["diffgr:id"])) {
							row = (DataRow)DiffGrRows [reader ["diffgr:id"]];
						} 
						else {
							row = table.NewRow ();
							NewRow = true;
						}

						ReadColumns (reader, row, table, TableName);

						if (NewRow)
							table.Rows.Add (row);
					}
					else if (String.Compare (reader.LocalName, "before", true) == 0) {
						break;
					}
					else {
						throw new DataException (Locale.GetText ("Cannot load diffGram. Table '" + reader.LocalName + "' is missing in the destination dataset"));
					}
				}
			}
		}

		// XmlReadMode.DiffGram
		private void ReadModeDiffGram (XmlReader reader)
		{
			reader.MoveToContent ();
			string Prefix = reader.Prefix;
			reader.Read ();
			XmlTextReader TempReader = new XmlTextReader (reader.BaseURI);
			ReadModeDiffGramBefore (TempReader);
			TempReader.Close ();
						
			ReadModeDiffGramCurrent (reader);
		}

		#endregion // diffgram-methods

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
					bool addTable = true;
					DataTable table;

					if (!DSet.Tables.Contains (datatablename)) {
						table = new DataTable (reader.LocalName);
					}
					else {
						table = DSet.Tables [datatablename];
						addTable = false;
					}

					Hashtable rowValue = new Hashtable ();

					while (reader.Read () && (reader.NodeType != XmlNodeType.EndElement 
								  || reader.LocalName != datatablename))
					{
						if (reader.NodeType == XmlNodeType.Element) {

							string dataColumnName = reader.LocalName;
							if (addTable)
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
					
					if (addTable)
						DSet.Tables.Add (table);
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
