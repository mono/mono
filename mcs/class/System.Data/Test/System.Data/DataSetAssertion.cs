//
// DataSetAssertion.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
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


using System;
using System.Collections;
using System.IO;
using System.Data;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using NUnit.Framework;

namespace MonoTests.System.Data
{
	public class DataSetAssertion
	{
		public string GetNormalizedSchema (string source)
		{
/*
			// Due to the implementation difference, we must have
			// one more step to reorder attributes. Here, read
			// schema document into XmlSchema once, and compare
			// output string with those emission from Write().
			XmlSchema xs = XmlSchema.Read (new XmlTextReader (
				new StringReader (source)), null);
			StringWriter writer = new StringWriter ();
			xs.Write (writer);
			return writer.ToString ();
*/
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml (source);
			SortAttributes (doc.DocumentElement);
			StringWriter writer = new StringWriter ();
			doc.Save (writer);
			return writer.ToString ();
		}

		private void SortAttributes (XmlElement el)
		{
			SortAttributesAttributes (el);
			ArrayList al = new ArrayList ();
			foreach (XmlNode n in el.ChildNodes) {
				if (n.NodeType == XmlNodeType.Element)
					SortAttributes (n as XmlElement);
				if (n.NodeType == XmlNodeType.Comment)
					al.Add (n);
			}
			foreach (XmlNode n in al)
				el.RemoveChild (n);
		}

		private void SortAttributesAttributes (XmlElement el)
		{
			ArrayList al = new ArrayList ();
			foreach (XmlAttribute a in el.Attributes)
				al.Add (a.Name);
			al.Sort ();
			string [] names = (string []) al.ToArray (typeof (string));
			al.Clear ();
			foreach (string name in names)
				al.Add (el.RemoveAttributeNode (
					el.GetAttributeNode (name)));
			foreach (XmlAttribute a in al)
				// Exclude xmlns="" here.
				if (a.Name != "xmlns")// || a.Value != String.Empty)
					el.SetAttributeNode (a);
		}

		public void AssertDataSet (string label, DataSet ds, string name, int tableCount, int relCount)
		{
			Assert.AreEqual (name, ds.DataSetName, label + ".DataSetName");
			Assert.AreEqual (tableCount, ds.Tables.Count, label + ".TableCount");
			if (relCount >= 0)
				Assert.AreEqual (relCount, ds.Relations.Count, label + ".RelationCount");
		}

		public void AssertDataTable (string label, DataTable dt, string name, int columnCount, int rowCount, int parentRelationCount, int childRelationCount, int constraintCount, int primaryKeyLength)
		{
			Assert.AreEqual (name, dt.TableName, label + ".TableName");
			Assert.AreEqual (columnCount, dt.Columns.Count, label + ".ColumnCount");
			Assert.AreEqual (rowCount, dt.Rows.Count, label + ".RowCount");
			Assert.AreEqual (parentRelationCount, dt.ParentRelations.Count, label + ".ParentRelCount");
			Assert.AreEqual (childRelationCount, dt.ChildRelations.Count, label + ".ChildRelCount");
			Assert.AreEqual (constraintCount, dt.Constraints.Count, label + ".ConstraintCount");
			Assert.AreEqual (primaryKeyLength, dt.PrimaryKey.Length, label + ".PrimaryKeyLength");
		}

		public void AssertReadXml (DataSet ds, string label, string xml, XmlReadMode readMode, XmlReadMode resultMode, string datasetName, int tableCount)
		{
			AssertReadXml (ds, label, xml, readMode, resultMode, datasetName, tableCount, ReadState.EndOfFile, null, null);
		}

		public void AssertReadXml (DataSet ds, string label, string xml, XmlReadMode readMode, XmlReadMode resultMode, string datasetName, int tableCount, ReadState state)
		{
			AssertReadXml (ds, label, xml, readMode, resultMode, datasetName, tableCount, state, null, null);
		}

		// a bit detailed version
		public void AssertReadXml (DataSet ds, string label, string xml, XmlReadMode readMode, XmlReadMode resultMode, string datasetName, int tableCount, ReadState state, string readerLocalName, string readerNS)
		{
			XmlReader xtr = new XmlTextReader (xml, XmlNodeType.Element, null);
			Assert.AreEqual (resultMode, ds.ReadXml (xtr, readMode), label + ".return");
			AssertDataSet (label + ".dataset", ds, datasetName, tableCount, -1);
			Assert.AreEqual (state, xtr.ReadState, label + ".readstate");
			if (readerLocalName != null)
				Assert.AreEqual (readerLocalName, xtr.LocalName, label + ".reader-localName");
			if (readerNS != null)
				Assert.AreEqual (readerNS, xtr.NamespaceURI, label + ".reader-ns");
		}

		public void AssertDataRelation (string label, DataRelation rel, string name, bool nested,
			string [] parentColNames, string [] childColNames,
			bool existsUK, bool existsFK)
		{
			Assert.AreEqual (name, rel.RelationName, label + ".Name");
			Assert.AreEqual (nested, rel.Nested, label + ".Nested");
			for (int i = 0; i < parentColNames.Length; i++)
				Assert.AreEqual (parentColNames [i], rel.ParentColumns [i].ColumnName, label + ".parentColumn_" + i);
			Assert.AreEqual (parentColNames.Length, rel.ParentColumns.Length, label + ".ParentColCount");
			for (int i = 0; i < childColNames.Length; i++)
				Assert.AreEqual (childColNames [i], rel.ChildColumns [i].ColumnName, label + ".childColumn_" + i);
			Assert.AreEqual (childColNames.Length, rel.ChildColumns.Length, label + ".ChildColCount");
			if (existsUK)
				Assert.IsNotNull (rel.ParentKeyConstraint, label + ".uniqKeyExists");
			else
				Assert.IsNull (rel.ParentKeyConstraint, label + ".uniqKeyNotExists");
			if (existsFK)
				Assert.IsNotNull (rel.ChildKeyConstraint, label + ".fkExists");
			else
				Assert.IsNull (rel.ChildKeyConstraint, label + ".fkNotExists");
		}

		public void AssertUniqueConstraint (string label, UniqueConstraint uc, 
			string name, bool isPrimaryKey, string [] colNames)
		{
			Assert.AreEqual (name, uc.ConstraintName, label + ".name");
			Assert.AreEqual (isPrimaryKey, uc.IsPrimaryKey, label + ".pkey");
			for (int i = 0; i < colNames.Length; i++)
				Assert.AreEqual (colNames [i], uc.Columns [i].ColumnName, label + ".column_" + i);
			Assert.AreEqual (colNames.Length, uc.Columns.Length, label + ".colCount");
		}

		public void AssertForeignKeyConstraint (string label,
			ForeignKeyConstraint fk, string name, 
			AcceptRejectRule acceptRejectRule, Rule delRule, Rule updateRule,
			string [] colNames, string [] relColNames)
		{
			Assert.AreEqual (name, fk.ConstraintName, label + ".name");
			Assert.AreEqual (acceptRejectRule, fk.AcceptRejectRule, label + ".acceptRejectRule");
			Assert.AreEqual (delRule, fk.DeleteRule, label + ".delRule");
			Assert.AreEqual (updateRule, fk.UpdateRule, label + ".updateRule");
			for (int i = 0; i < colNames.Length; i++)
				Assert.AreEqual (colNames [i], fk.Columns [i].ColumnName, label + ".column_" + i);
			Assert.AreEqual (colNames.Length, fk.Columns.Length, label + ".colCount");
			for (int i = 0; i < relColNames.Length; i++)
				Assert.AreEqual (relColNames [i], fk.RelatedColumns [i].ColumnName, label + ".relatedColumn_" + i);
			Assert.AreEqual (relColNames.Length, fk.RelatedColumns.Length, label + ".relatedColCount");
		}

		public void AssertDataColumn (string label, DataColumn col, 
			string colName, bool allowDBNull, 
			bool autoIncr, int autoIncrSeed, int autoIncrStep, 
			string caption, MappingType colMap, 
			Type type, object defaultValue, string expression, 
			int maxLength, string ns, int ordinal, string prefix, 
			bool readOnly, bool unique)
		{
			Assert.AreEqual (colName, col.ColumnName, label + "ColumnName: " );
			Assert.AreEqual (allowDBNull, col.AllowDBNull, label + "AllowDBNull? " );
			Assert.AreEqual (autoIncr, col.AutoIncrement, label + "AutoIncrement? " );
			Assert.AreEqual (autoIncrSeed, col.AutoIncrementSeed, label + "  Seed: " );
			Assert.AreEqual (autoIncrStep, col.AutoIncrementStep, label + "  Step: " );
			Assert.AreEqual (caption, col.Caption, label + "Caption " );
			Assert.AreEqual (colMap, col.ColumnMapping, label + "Mapping: " );
			Assert.AreEqual (type, col.DataType, label + "Type: " );
			Assert.AreEqual (defaultValue, col.DefaultValue, label + "DefaultValue: " );
			Assert.AreEqual (expression, col.Expression, label + "Expression: " );
			Assert.AreEqual (maxLength, col.MaxLength, label + "MaxLength: " );
			Assert.AreEqual (ns, col.Namespace, label + "Namespace: " );
			if (ordinal >= 0)
				Assert.AreEqual (ordinal, col.Ordinal, label + "Ordinal: " );
			Assert.AreEqual (prefix, col.Prefix, label + "Prefix: " );
			Assert.AreEqual (readOnly, col.ReadOnly, label + "ReadOnly: " );
			Assert.AreEqual (unique, col.Unique, label + "Unique: " );
		}
	}
}

