//
// DataSetAssertion.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// (C)2004 Novell Inc.
//

using System;
using System.IO;
using System.Data;
using System.Text;
using System.Xml;
using NUnit.Framework;

namespace MonoTests.System.Data
{
	public class DataSetAssertion : Assertion
	{
		public void AssertDataSet (string label, DataSet ds, string name, int tableCount)
		{
			AssertEquals (label + ".DataSetName", name, ds.DataSetName);
			AssertEquals (label + ".TableCount", tableCount, ds.Tables.Count);
		}

		public void AssertDataSet (string label, DataSet ds, string name, int tableCount, int relCount)
		{
			AssertEquals (label + ".DataSetName", name, ds.DataSetName);
			AssertEquals (label + ".TableCount", tableCount, ds.Tables.Count);
			AssertEquals (label + ".RelationCount", relCount, ds.Relations.Count);
		}

		public void AssertDataTable (string label, DataTable dt, string name, int columnCount, int rowCount)
		{
			AssertEquals (label + ".TableName", name, dt.TableName);
			AssertEquals (label + ".ColumnCount", columnCount, dt.Columns.Count);
			AssertEquals (label + ".RowCount", rowCount, dt.Rows.Count);
		}

		public void AssertReadXml (DataSet ds, string label, string xml, XmlReadMode readMode, XmlReadMode resultMode, string datasetName, int tableCount)
		{
			AssertReadXml (ds, label, xml, readMode, resultMode, datasetName, tableCount, ReadState.EndOfFile);
		}

		// a bit detailed version
		public void AssertReadXml (DataSet ds, string label, string xml, XmlReadMode readMode, XmlReadMode resultMode, string datasetName, int tableCount, ReadState state)
		{
			XmlReader xtr = new XmlTextReader (xml, XmlNodeType.Element, null);
			AssertEquals (label + ".return", resultMode, ds.ReadXml (xtr, readMode));
			AssertDataSet (label + ".dataset", ds, datasetName, tableCount);
			AssertEquals (label + ".readstate", state, xtr.ReadState);
		}

		public void AssertDataRelation (string label, DataRelation rel, string name,
			string [] parentColNames, string [] childColNames,
			bool existsUK, bool existsFK)
		{
			AssertEquals (label + ".Name", name, rel.RelationName);
			for (int i = 0; i < parentColNames.Length; i++)
				AssertEquals (label + ".parentColumn_" + i, parentColNames [i], rel.ParentColumns [i].ColumnName);
			AssertEquals (label + ".ParentColCount", parentColNames.Length, rel.ParentColumns.Length);
			for (int i = 0; i < childColNames.Length; i++)
				AssertEquals (label + ".childColumn_" + i, childColNames [i], rel.ChildColumns [i].ColumnName);
			AssertEquals (label + ".ChildColCount", childColNames.Length, rel.ChildColumns.Length);
			if (existsUK)
				AssertNotNull (label + ".uniqKeyExists", rel.ParentKeyConstraint);
			else
				AssertNull (label + ".uniqKeyNotExists", rel.ParentKeyConstraint);
			if (existsFK)
				AssertNotNull (label + ".fkExists", rel.ChildKeyConstraint);
			else
				AssertNull (label + ".fkNotExists", rel.ChildKeyConstraint);
		}

		public void AssertUniqueConstraint (string label, UniqueConstraint uc, 
			string name, bool isPrimaryKey, string [] colNames)
		{
			AssertEquals (label + ".name", name, uc.ConstraintName);
			AssertEquals (label + ".pkey", isPrimaryKey, uc.IsPrimaryKey);
			for (int i = 0; i < colNames.Length; i++)
				AssertEquals (label + ".column_" + i, colNames [i], uc.Columns [i].ColumnName);
			AssertEquals (label + ".colCount", colNames.Length, uc.Columns.Length);
		}

		public void AssertForeignKeyConstraint (string label,
			ForeignKeyConstraint fk, string name, 
			AcceptRejectRule acceptRejectRule, Rule delRule, Rule updateRule,
			string [] colNames, string [] relColNames)
		{
			AssertEquals (label + ".name", name, fk.ConstraintName);
			AssertEquals (label + ".acceptRejectRule", acceptRejectRule, fk.AcceptRejectRule);
			AssertEquals (label + ".delRule", delRule, fk.DeleteRule);
			AssertEquals (label + ".updateRule", updateRule, fk.UpdateRule);
			for (int i = 0; i < colNames.Length; i++)
				AssertEquals (label + ".column_" + i, colNames [i], fk.Columns [i].ColumnName);
			AssertEquals (label + ".colCount", colNames.Length, fk.Columns.Length);
			for (int i = 0; i < relColNames.Length; i++)
				AssertEquals (label + ".relatedColumn_" + i, relColNames [i], fk.RelatedColumns [i].ColumnName);
			AssertEquals (label + ".relatedColCount", relColNames.Length, fk.RelatedColumns.Length);
		}

		public void AssertDataColumn (string label, DataColumn col, 
			string colName, bool allowDBNull, 
			bool autoIncr, int autoIncrSeed, int autoIncrStep, 
			string caption, MappingType colMap, 
			Type type, object defaultValue, string expression, 
			int maxLength, string ns, int ordinal, string prefix, 
			bool readOnly, bool unique)
		{
			AssertEquals (label + "ColumnName: " , colName, col.ColumnName);
			AssertEquals (label + "AllowDBNull? " , allowDBNull, col.AllowDBNull);
			AssertEquals (label + "AutoIncrement? " , autoIncr, col.AutoIncrement);
			AssertEquals (label + "  Seed: " , autoIncrSeed, col.AutoIncrementSeed);
			AssertEquals (label + "  Step: " , autoIncrStep, col.AutoIncrementStep);
			AssertEquals (label + "Caption " , caption, col.Caption);
			AssertEquals (label + "Mapping: " , colMap, col.ColumnMapping);
			AssertEquals (label + "Type: " , type, col.DataType);
			AssertEquals (label + "DefaultValue: " , defaultValue, col.DefaultValue);
			AssertEquals (label + "Expression: " , expression, col.Expression);
			AssertEquals (label + "MaxLength: " , maxLength, col.MaxLength);
			AssertEquals (label + "Namespace: " , ns, col.Namespace);
			AssertEquals (label + "Ordinal: " , ordinal, col.Ordinal);
			AssertEquals (label + "Prefix: " , prefix, col.Prefix);
			AssertEquals (label + "ReadOnly: " , readOnly, col.ReadOnly);
			AssertEquals (label + "Unique: " , unique, col.Unique);
		}
	}
}

