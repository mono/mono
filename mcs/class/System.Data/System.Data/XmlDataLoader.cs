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
			reader.ReadStartElement ();
			reader.MoveToContent ();

			while (reader.NodeType != XmlNodeType.EndElement) 
			{
				if (reader.NodeType == XmlNodeType.Element)
				{
					// FIXME: possible inline-schema should be readed here
					if (String.Compare (reader.LocalName, "schema", true) == 0) 
					{
						if (!IgnoreSchema)
							DSet.ReadXmlSchema (reader);
					}

					// find table
					if (DSet.Tables.Contains (reader.LocalName)) 
					{
						DataTable table = DSet.Tables [reader.LocalName];
						DataRow row = table.NewRow ();

						reader.ReadStartElement ();
						ReadColumns (reader, row, table, reader.LocalName);					
						reader.ReadEndElement ();

						table.Rows.Add (row);
					}
				}
				reader.MoveToContent ();
			}
		}

		#endregion // reading

		#region Private helper methods
		
		private void ReadColumns (XmlReader reader, DataRow row, DataTable table, string TableName)
		{
			do {
				if (reader.NodeType == XmlNodeType.Element) {
					DataColumn col = table.Columns [reader.LocalName];
					if (col != null) {
						reader.Read ();
						row [col] = StringToObject (col.DataType, reader.Value);
					}
				}
				else {
					reader.Read ();
				}
				
			} while (table.TableName != reader.LocalName 
				 || reader.NodeType != XmlNodeType.EndElement);
		}

		internal static object StringToObject (Type type, string value)
		{
			if (type == null) return value;

			switch (Type.GetTypeCode (type))
			{
				case TypeCode.Boolean: return XmlConvert.ToBoolean (value);
				case TypeCode.Byte: return XmlConvert.ToByte (value);
				case TypeCode.Char: return (char)XmlConvert.ToInt32 (value);
				case TypeCode.DateTime: return XmlConvert.ToDateTime (value);
				case TypeCode.Decimal: return XmlConvert.ToDecimal (value);
				case TypeCode.Double: return XmlConvert.ToDouble (value);
				case TypeCode.Int16: return XmlConvert.ToInt16 (value);
				case TypeCode.Int32: return XmlConvert.ToInt32 (value);
				case TypeCode.Int64: return XmlConvert.ToInt64 (value);
				case TypeCode.SByte: return XmlConvert.ToSByte (value);
				case TypeCode.Single: return XmlConvert.ToSingle (value);
				case TypeCode.UInt16: return XmlConvert.ToUInt16 (value);
				case TypeCode.UInt32: return XmlConvert.ToUInt32 (value);
				case TypeCode.UInt64: return XmlConvert.ToUInt64 (value);
			}

			if (type == typeof (TimeSpan)) return XmlConvert.ToTimeSpan (value);
			if (type == typeof (byte[])) return Convert.FromBase64String (value);

			return Convert.ChangeType (value, type);
		}

		#endregion // Private helper methods
	}
}
