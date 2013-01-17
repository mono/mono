
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

using System;
using System.IO;
using System.Data;
using System.Xml;
using System.Xml.Serialization;


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

		public static void ReadXml (
			DataSet dataset, XmlReader reader, XmlReadMode mode)
		{
			new XmlDataReader (dataset, reader, mode).Process ();
		}

		DataSet dataset;
		XmlReader reader;
		XmlReadMode mode;

		public XmlDataReader (DataSet ds, XmlReader xr, XmlReadMode m)
		{
			dataset = ds;
			reader =xr;
			mode = m;
		}

		private void Process ()
		{
			bool savedEnforceConstraints =
				dataset.EnforceConstraints;
			try {
			dataset.EnforceConstraints = false;
			reader.MoveToContent ();

			if (mode == XmlReadMode.Fragment) {
					while (reader.NodeType == XmlNodeType.Element && !reader.EOF) {
						ReadTopLevelElement ();
					}
				}
				else
					ReadTopLevelElement ();
			} finally {
				dataset.EnforceConstraints = 
					savedEnforceConstraints;
			}
		}

		private bool IsTopLevelDataSet ()
		{
			string local = XmlHelper.Decode (reader.LocalName);

			// No need to check DataSetName. In fact, it is ignored.

			DataTable dt = dataset.Tables [local];
			if (dt == null)
				return true;

			XmlDocument doc = new XmlDocument ();
			XmlElement el = (XmlElement) doc.ReadNode (reader);
			doc.AppendChild (el);
			reader = new XmlNodeReader (el);
			reader.MoveToContent ();

			return !XmlDataInferenceLoader.IsDocumentElementTable (
				el, null);
		}

		private void ReadTopLevelElement ()
		{
			if (mode == XmlReadMode.Fragment &&
				(XmlHelper.Decode (reader.LocalName) !=
				dataset.DataSetName ||
				reader.NamespaceURI != dataset.Namespace))
				reader.Skip ();
			else if (mode == XmlReadMode.Fragment ||
				IsTopLevelDataSet ()) {
			int depth = reader.Depth;
			reader.Read ();
			reader.MoveToContent ();
			do {
				ReadDataSetContent ();
			} while (reader.Depth > depth && !reader.EOF);
			if (reader.NodeType == XmlNodeType.EndElement)
				reader.ReadEndElement ();
			reader.MoveToContent ();
		}
			else
				ReadDataSetContent ();
		}

		private void ReadDataSetContent ()
		{
			DataTable table = dataset.Tables [XmlHelper.Decode (reader.LocalName)];
			if (table == null || table.Namespace != reader.NamespaceURI) {
				reader.Skip ();
				reader.MoveToContent ();
				return; // skip if there is no matching table
			}

			// skip if namespace does not match.
			if (table.Namespace != reader.NamespaceURI) {
				reader.Skip ();
				reader.MoveToContent ();
				return; // skip if there is no matching table
			}

			DataRow row = table.NewRow ();
			ReadElement (row);
			table.Rows.Add (row);
		}

		private void ReadElement (DataRow row)
		{
			// Consume attributes
			if (reader.MoveToFirstAttribute ()) {
				do {
					if (reader.NamespaceURI == XmlConstants.XmlnsNS
#if NET_2_0
					|| reader.NamespaceURI == XmlConstants.XmlNS
#endif
						)
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
				if (reader.IsEmptyElement)
					reader.Read ();
				if (reader.NodeType == XmlNodeType.EndElement)
					reader.ReadEndElement ();
				reader.MoveToContent ();
			}
		}

		private void ReadElementAttribute (DataRow row)
		{
			DataColumn col = row.Table.Columns [XmlHelper.Decode (reader.LocalName)];
			if (col == null || col.Namespace != reader.NamespaceURI)
				return;
			row [col] = StringToObject (col.DataType, reader.Value);
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
					row [simple] = StringToObject (simple.DataType, s);
#else
// But it does not mean we support "123<!-- comment -->456". just allowed for string
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
			DataColumn col = row.Table.Columns [XmlHelper.Decode (reader.LocalName)];
			if (col == null ||  col.Namespace != reader.NamespaceURI)
				col = null;

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

				if (typeof (IXmlSerializable).IsAssignableFrom (col.DataType)) {
#if NET_2_0
					try {
						// NOTE: ReadElementString works fine with proper XML with CDATA etc,
						// however doesn't behave well with XMLs like the one in 
						// https://bugzilla.novell.com/show_bug.cgi?id=377146 which is 
						// apparently supported by MS.NET - to maintain compatibility,
						// If the obj implements IXmlSerializable, let obj's ReadXml do the reading
						IXmlSerializable obj = (IXmlSerializable) Activator.CreateInstance (col.DataType, new object [0]);
						if (!reader.IsEmptyElement) {
							obj.ReadXml (reader);
   							reader.ReadEndElement ();
						} else {
   							reader.Skip ();
						}						
						row [col] = obj;
					} catch (XmlException) {
#endif
						// XML is not in accordance to expected standards, try reading the content as an xml doc
						row [col] = reader.ReadInnerXml ();
#if NET_2_0
					} catch (InvalidOperationException) {

						row [col] = reader.ReadInnerXml ();
					}
#endif
				} else {
					row [col] = StringToObject (col.DataType, reader.ReadElementString ());
				}
					
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
				DataTable ct = rel.ChildTable;
				if (ct.TableName != XmlHelper.Decode (reader.LocalName) || ct.Namespace != reader.NamespaceURI)
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

		internal static object StringToObject (Type type, string value)
		{
			if (type == null) return value;

			switch (Type.GetTypeCode (type)) {
				case TypeCode.Boolean: return XmlConvert.ToBoolean (value);
				case TypeCode.Byte: return XmlConvert.ToByte (value);
				case TypeCode.Char: return (char)XmlConvert.ToInt32 (value);
#if NET_2_0
				case TypeCode.DateTime: return XmlConvert.ToDateTime (value, XmlDateTimeSerializationMode.Unspecified);
#else
				case TypeCode.DateTime: return XmlConvert.ToDateTime (value);
#endif
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
			if (type == typeof (Guid)) return XmlConvert.ToGuid (value);
			if (type == typeof (byte[])) return Convert.FromBase64String (value);
			if (type == typeof (System.Type)) return System.Type.GetType (value);

			return Convert.ChangeType (value, type);
		}
	}
}
