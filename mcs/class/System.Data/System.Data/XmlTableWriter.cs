// 
// System.Data/XmlTableWriter.cs
//
// Author:
//   Patrick Earl <mono@patearl.net>
//
// Copyright (c) 2006, Patrick Earl
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

#if NET_2_0
using System;
using System.Collections.Generic;
using System.Data;
using System.Xml;

internal class XmlTableWriter {
	// This method is modelled after the DataSet's WriteXml functionality.
	internal static void WriteTables(XmlWriter writer,
		                 XmlWriteMode mode,
				 List<DataTable> tables,
				 List<DataRelation> relations,
				 string mainDataTable,
				 string dataSetName)
	{
		if (mode == XmlWriteMode.DiffGram) {
			foreach (DataTable table in tables)
				table.SetRowsID();
			DataSet.WriteDiffGramElement(writer);
		}

		bool shouldOutputContent = (mode != XmlWriteMode.DiffGram);
		for (int n = 0; n < tables.Count && !shouldOutputContent; n++)
			shouldOutputContent = tables[n].Rows.Count > 0;

		if (shouldOutputContent) {
			// We assume that tables[0] is the main table being written.
			// We happen to know that the code above us does things that way.
			DataSet.WriteStartElement(writer, mode, tables[0].Namespace, tables[0].Prefix, XmlHelper.Encode(dataSetName));

			if (mode == XmlWriteMode.WriteSchema) {
				DataTable [] _tables = new DataTable[tables.Count];
				tables.CopyTo(_tables);
				DataRelation[] _relations = new DataRelation[relations.Count];
				relations.CopyTo(_relations);
				DataTable dt = _tables [0];
				new XmlSchemaWriter(writer,
					_tables,
					_relations,
					mainDataTable,
					dataSetName,
					dt.LocaleSpecified ? dt.Locale : null
				).WriteSchema();
			}

			WriteTableList (writer, mode, tables, DataRowVersion.Default);

			writer.WriteEndElement();
		}

		if (mode == XmlWriteMode.DiffGram) {
			List<DataTable> changedTables = new List<DataTable>();
			foreach (DataTable table in tables) {
				DataTable changed = table.GetChanges(DataRowState.Modified | DataRowState.Deleted);
				if (changed != null && changed.Rows.Count > 0) {
					changedTables.Add(changed);
				}
			}
			if (changedTables.Count > 0) {
				DataSet.WriteStartElement(writer, XmlWriteMode.DiffGram, XmlConstants.DiffgrNamespace, XmlConstants.DiffgrPrefix, "before");
				WriteTableList (writer, mode, changedTables, DataRowVersion.Original);
				writer.WriteEndElement();
			}
		
			writer.WriteEndElement(); // diffgr:diffgram
		}

		writer.Flush();
	}

	internal static void WriteTableList(XmlWriter writer, XmlWriteMode mode, List<DataTable> tables, DataRowVersion version)
	{
		foreach (DataTable table in tables)
			DataSet.WriteTable(writer, table, mode, version);
	}
}
#endif
