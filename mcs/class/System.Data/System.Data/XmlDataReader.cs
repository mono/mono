using System;
using System.IO;
using System.Data;
using System.Xml;


namespace System.Data
{
#if STANDALONE_DRIVER_TEST
	public class Driver
	{
		public static void Main (string [] args)
		{
			if (args.Length == 0) {
				Console.WriteLine ("usage: mono xmldatareader.exe filename");
				return;
			}

			Console.WriteLine ("Target file: " + args [0]);

			DataSet ds = new DataSet ();
//			ds.InferXmlSchema (args [0], null);

			try {
				ds.ReadXml (args [0]);
			} catch (Exception ex) {
				Console.WriteLine ("ReadXml() borked: " + ex.Message);
				return;
			}
			Console.WriteLine ("---- DataSet ----------------");
			StringWriter sw = new StringWriter ();
			PrintDataSet (ds, sw);
			PrintDataSet (ds, Console.Out);

			ds = new DataSet ();
			ds.InferXmlSchema (args [0], null);
			XmlDataReader.ReadXml (ds, new XmlTextReader (args [0]));
			Console.WriteLine ("---- XmlDataReader ----------------");
			StringWriter sw2 = new StringWriter ();
			PrintDataSet (ds, sw2);

			if (sw.ToString () == sw2.ToString ())
				Console.WriteLine ("Successful.");
			else
				Console.WriteLine ("Different *************************************************\n" + sw2);
		}

		private static void PrintDataSet (DataSet ds, TextWriter tw)
		{
			tw.WriteLine ("DS::" + ds.DataSetName + ", " + ds.Tables.Count + ", " + ds.Relations.Count);
			foreach (DataTable dt in ds.Tables)
				tw.WriteLine ("DT:" + dt.TableName + ", " + dt.Columns.Count + ", " + dt.Rows.Count);

			ds.WriteXml (tw);
			tw.WriteLine ();
		}
	}
#endif

	internal class XmlDataReader
	{
		const string xmlnsNS = "http://www.w3.org/2000/xmlns/";

		public static void ReadXml (DataSet dataset, XmlReader reader)
		{
			new XmlDataReader (dataset, reader).Process ();
		}

		DataSet dataset;
		XmlReader reader;

		public XmlDataReader (DataSet ds, XmlReader xr)
		{
			dataset = ds;
			reader =xr;
		}

		private void Process ()
		{
			// set EnforceConstraint to false during load time.
			bool savedEnforceConstraints =
				dataset.EnforceConstraints;
			dataset.EnforceConstraints = false;

			reader.MoveToContent ();

			if (reader.LocalName == dataset.DataSetName) {
				int depth = reader.Depth;
				reader.Read ();
				reader.MoveToContent ();
				do {
					ReadDataSetContent ();
				} while (reader.Depth > depth && !reader.EOF);
			}
			else
				ReadDataSetContent ();

			dataset.EnforceConstraints = savedEnforceConstraints;
		}

		private void ReadDataSetContent ()
		{
			DataTable table = dataset.Tables [reader.LocalName];
			if (table == null) {
				reader.Skip ();
				reader.MoveToContent ();
				return; // skip if there is no matching table
			}

			// skip if namespace does not match.
			// TODO: This part is suspicious for MS compatibility
			// (test required)
			if (table.Namespace != reader.NamespaceURI)
				return; 

			DataRow row = table.NewRow ();
			ReadElement (row);
			table.Rows.Add (row);
		}

		private void ReadElement (DataRow row)
		{
			// Consume attributes
			if (reader.MoveToFirstAttribute ()) {
				do {
					if (reader.NamespaceURI == xmlnsNS)
						continue;
					ReadElementAttribute (row);
				} while (reader.MoveToNextAttribute ());
				reader.MoveToElement ();
			}

			// If not empty element, read content.
			if (reader.IsEmptyElement) {
				reader.Skip ();
				reader.MoveToContent ();
			} else {
				int depth = reader.Depth;
				reader.Read ();
				reader.MoveToContent ();
				do {
					ReadElementContent (row);
				} while (reader.Depth > depth && !reader.EOF);
				reader.ReadEndElement ();
				reader.MoveToContent ();
			}
		}

		private void ReadElementAttribute (DataRow row)
		{
			DataColumn col = row.Table.Columns [reader.LocalName];
			if (col == null)
				return;
			row [col] = reader.Value;
		}

		private void ReadElementContent (DataRow row)
		{
			switch (reader.NodeType) {

			case XmlNodeType.EndElement:
				// This happens when the content was only whitespace (and skipped by MoveToContent()).
				return;

			case XmlNodeType.Element:
				ReadElementElement (row);
				break;

			case XmlNodeType.Text:
			case XmlNodeType.CDATA:
			case XmlNodeType.SignificantWhitespace:
				DataColumn simple = null;
				DataColumnCollection cols = row.Table.Columns;
				for (int i = 0; i < cols.Count; i++) {
					DataColumn col = cols [i];
					if (col.ColumnMapping ==
						MappingType.SimpleContent) {
						simple = col;
						break;
					}
				}
				string s = reader.ReadString ();
				reader.MoveToContent ();
#if SILLY_MS_COMPATIBLE
// As to MS, "test string" and "test <!-- comment -->string" are different :P
				if (simple != null && row.IsNull (simple))
					row [simple] = s;
#else
				if (simple != null)
					row [simple] += s;
#endif
				break;
			case XmlNodeType.Whitespace:
				reader.ReadString ();
				break;
			}
		}

		private void ReadElementElement (DataRow row)
		{
			// This child element (for row) might be either simple
			// content element, or child element

			// MS.NET crashes here... but it seems just a bug.
//			DataColumn col = row.Table.Columns [reader.LocalName];
			DataColumn col = null;
			DataColumnCollection cols = row.Table.Columns;
			for (int i = 0; i < cols.Count; i++) {
				if (cols [i].ColumnName == reader.LocalName) {
					col = cols [i];
					break;
				}
			}


			// if col exists, then it should be MappingType.Element
			if (col != null
				&& col.ColumnMapping == MappingType.Element) {

				// TODO: This part is suspicious for
				// MS compatibility (test required)
				if (col.Namespace != reader.NamespaceURI) {
					reader.Skip ();
					return;
				}

				bool wasEmpty = reader.IsEmptyElement;
				int depth = reader.Depth;
				row [col] = reader.ReadElementString ();
				if (!wasEmpty && reader.Depth > depth) {
				// This means, instance does not match with
				// the schema (because the instance element
				// contains complex content, while specified as
				// simple), so just skip to the end of the
				// element.
					while (reader.Depth > depth)
						reader.Read ();
					reader.Read ();
				}
				reader.MoveToContent ();
				return;
			} else if (col != null) {
				// Mismatch column type. Just skip
				reader.Skip ();
				reader.MoveToContent ();
				return;
			}

			// Otherwise, it might be child table element
			DataRelationCollection rels = row.Table.ChildRelations;
			for (int i = 0; i < rels.Count; i++) {
				DataRelation rel = rels [i];
				if (!rel.Nested)
					continue;
				if (rel.ChildTable.TableName != reader.LocalName)
					continue;

				DataRow childRow = rel.ChildTable.NewRow ();
				ReadElement (childRow);

				for (int c = 0; c < rel.ChildColumns.Length; c++) {
					childRow [rel.ChildColumns [c]]
						= row [rel.ParentColumns [c]];
				}
				rel.ChildTable.Rows.Add (childRow);
				return;
			}

			// Matched neither of the above: just skip
			reader.Skip ();
			reader.MoveToContent ();
		}
	}
}
