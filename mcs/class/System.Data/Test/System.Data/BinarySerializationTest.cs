
using System;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;

using NUnit.Framework;

using MonoTests.Helpers;

namespace MonoTests.System.Data
{
[TestFixture]
public class BinarySerializationTest
{
	private CultureInfo originalCulture;

	[SetUp]
	public void SetUp ()
	{
		originalCulture = Thread.CurrentThread.CurrentCulture;
		Thread.CurrentThread.CurrentCulture = new CultureInfo ("en-US");
	}

	[TearDown]
	public void TearDown ()
	{
		Thread.CurrentThread.CurrentCulture = originalCulture;
	}

	[Test]
	public void RemotingFormatDataTableTest ()
	{
		DataTable dt = new DataTable ();
		//Test Default SerializationFormat
		Assert.AreEqual (SerializationFormat.Xml, dt.RemotingFormat, "#1 Default value for RemotingFormat of DataTable is Xml");
		//Test with Assigned value
		dt.RemotingFormat = SerializationFormat.Binary;
		Assert.AreEqual (SerializationFormat.Binary, dt.RemotingFormat, "#2 RemotingFormat Should be Binary");
	}	
	[Test]
	public void RemotingFormatDataSetTest ()
	{
		DataSet ds = new DataSet ();
		DataTable dt = new DataTable ();

		//Test Default SerializationFormat
		Assert.AreEqual (SerializationFormat.Xml, dt.RemotingFormat, "#1 Default value for RemotingFormat of DataTable is Xml");
		Assert.AreEqual (SerializationFormat.Xml, ds.RemotingFormat, "#2 Default value for RemotingFormat of DataSet is Xml");
		
		//Test with Assigned value
		ds.RemotingFormat = SerializationFormat.Binary;
		Assert.AreEqual (SerializationFormat.Binary, ds.RemotingFormat, "#3 RemotingFormat Should be Binary");

		//Test After adding a Table to DataSet
		ds.Tables.Add (dt);
		Assert.AreEqual (SerializationFormat.Binary, ds.RemotingFormat, "#4 RemotingFormat should be Binary for DataSet");
		Assert.AreEqual (SerializationFormat.Binary, dt.RemotingFormat, "#5 RemotingFormat should be Binary for DataTable");
		
		ds.RemotingFormat = SerializationFormat.Xml;
		Assert.AreEqual (SerializationFormat.Xml, ds.RemotingFormat, "#6 RemotingFormat should be Xml for DataSet");
		Assert.AreEqual (SerializationFormat.Xml, dt.RemotingFormat, "#7 RemotingFormat should be  Xml for DataTable");
		
		//Test for Exception when try t modify RemotingFormat of DataTable which belongs to a DataSet

		try {
			dt.RemotingFormat = SerializationFormat.Binary;
			Assert.Fail ("#8 It should throw an ArgumentException");
		} catch (ArgumentException e) {
			Assert.AreEqual (e.GetType (), typeof (ArgumentException), "#9 Invalid Exception");
		} catch (Exception e) {
			Assert.AreEqual (e.GetType (), typeof (ArgumentException), "#10 Invalid Exception");
		}
		
	}	
	[Test]
	public void DataTableSerializationTest1 ()
	{
		//Serialize Table
		DataTable tb1 = new DataTable ("Test");
		tb1.Columns.Add ("id", typeof (int));
		tb1.Columns.Add ("name", typeof (string));
		tb1.Rows.Add (new object[] {1, "A"});
		tb1.Rows.Add (new object[] {2, "B"});
		//Add Constraint
		UniqueConstraint uniqueConstraint = new UniqueConstraint (tb1.Columns ["id"]);
		tb1.Constraints.Add (uniqueConstraint);
		tb1.RemotingFormat = SerializationFormat.Binary;
		tb1.AcceptChanges();

		tb1.Rows[0][0] = 1;
		tb1.Rows[1].Delete();
		
		MemoryStream ms = new MemoryStream ();
		BinaryFormatter bf = new BinaryFormatter ();
		bf.Serialize (ms,tb1);
		byte [] serializedStream = ms.ToArray ();
		ms.Close ();
		//DserializeTable
		ms = new MemoryStream (serializedStream);
		DataTable dt = (DataTable)bf.Deserialize (ms);
		ms.Close ();
		
		//Test Properties of table
		//Assert.AreEqual (tb1.RemotingFormat, dt.RemotingFormat, "#1 RemotingFormat property is Different");
		Assert.AreEqual (tb1.Columns.Count, dt.Columns.Count, "#2 ColumnCount property is Different");
		Assert.AreEqual (tb1.TableName, dt.TableName, "#3 TableName property is Different");
		Assert.AreEqual (tb1.Prefix, dt.Prefix, "#4 Prefix propertyis Different");
		Assert.AreEqual (tb1.Namespace, dt.Namespace, "#5 NameSpace property is Different");
		Assert.AreEqual (tb1.CaseSensitive, dt.CaseSensitive, "#6 CaseSensitive property is Different");
		Assert.AreEqual (tb1.Locale.LCID, dt.Locale.LCID, "#7 LocaleLCID property is Different");
		Assert.AreEqual (tb1.MinimumCapacity, dt.MinimumCapacity, "#8 NameSpace property is Different");
		//Test Constraints
		Assert.AreEqual (tb1.Constraints.Count, dt.Constraints.Count, "#9 No. of Constraints is Different");
		for (int i = 0; i < tb1.Constraints.Count; i++)
			Assert.AreEqual (tb1.Constraints [i].GetType (), dt.Constraints [i].GetType (), "#10 Constraint : {0} is Different", tb1.Constraints [i]);
		//Test for Table Data 
		Assert.AreEqual (tb1.Rows.Count, dt.Rows.Count, "#11 RowCount propertyis Different");
		//RowStates
		for (int i = 0; i < tb1.Rows.Count; i++) {
			Assert.AreEqual (tb1.Rows [i].RowState, dt.Rows [i].RowState, "#12 RowState is Different");
		}
		//Table Data
		for (int i = 0; i < tb1.Rows.Count; i++) 
			for (int j = 0; j < tb1.Columns.Count; j++) {
			  if (tb1.Rows[i].RowState != DataRowState.Deleted)
				Assert.AreEqual (tb1.Rows [i][j], dt.Rows [i][j], "#13 Elements differ at Row :{0} Column :{1}", i, j);
			}
		dt.Rows[0].RejectChanges();
		dt.Rows[1].RejectChanges();
	}
	[Test]
	public void DataTableSerializationTest2 ()
	{
		//Serialize Table
		DataTable tb1 = new DataTable ("Test");
		tb1.Columns.Add ("id", typeof (int));
		tb1.Columns.Add ("name", typeof (string));
		tb1.Rows.Add (new object[] {1, "A"});
		tb1.Rows.Add (new object[] {2, "B"});
		//Add Constraint
		UniqueConstraint uniqueConstraint = new UniqueConstraint (tb1.Columns ["id"]);
		tb1.Constraints.Add (uniqueConstraint);
		tb1.RemotingFormat = SerializationFormat.Xml;
		tb1.AcceptChanges();

		tb1.Rows[0][0] = 1;
		tb1.Rows[1].Delete();
		
		BinaryFormatter bf = new BinaryFormatter ();
		FileStream fs = new FileStream (TestResourceHelper.GetFullPathOfResource ("Test/System.Data/binserialize/BS-tb1.bin"), FileMode.Open, FileAccess.Read);
		BinaryReader r = new BinaryReader (fs);
		byte [] serializedStream = r.ReadBytes ((int)fs.Length);
		r.Close ();
		fs.Close ();
		//DserializeTable
		MemoryStream ms = new MemoryStream (serializedStream);
		DataTable dt = (DataTable)bf.Deserialize (ms);
		ms.Close ();
		
		//Test Properties of table
		//Assert.AreEqual (tb1.RemotingFormat, dt.RemotingFormat, "#1 RemotingFormat property is Different");
		Assert.AreEqual (tb1.Columns.Count, dt.Columns.Count, "#2 ColumnCount property is Different");
		Assert.AreEqual (tb1.TableName, dt.TableName, "#3 TableName property is Different");
		Assert.AreEqual (tb1.Prefix, dt.Prefix, "#4 Prefix propertyis Different");
		Assert.AreEqual (tb1.Namespace, dt.Namespace, "#5 NameSpace property is Different");
		Assert.AreEqual (tb1.CaseSensitive, dt.CaseSensitive, "#6 CaseSensitive property is Different");
		Assert.AreEqual (tb1.Locale.LCID, dt.Locale.LCID, "#7 LocaleLCID property is Different");
		Assert.AreEqual (tb1.MinimumCapacity, dt.MinimumCapacity, "#8 NameSpace property is Different");
		//Test Constraints
		Assert.AreEqual (tb1.Constraints.Count, dt.Constraints.Count, "#9 No. of Constraints is Different");
		for (int i = 0; i < tb1.Constraints.Count; i++)
			Assert.AreEqual (tb1.Constraints [i].GetType (), dt.Constraints [i].GetType (), "#10 Constraint : {0} is Different", tb1.Constraints [i]);
		//Test for Table Data 
		Assert.AreEqual (tb1.Rows.Count, dt.Rows.Count, "#11 RowCount propertyis Different");
		//RowStates
		for (int i = 0; i < tb1.Rows.Count; i++) {
			Assert.AreEqual (tb1.Rows [i].RowState, dt.Rows [i].RowState, "#12 RowState is Different");
		}
		//Table Data
		for (int i = 0; i < tb1.Rows.Count; i++) 
			for (int j = 0; j < tb1.Columns.Count; j++) {
			  if (tb1.Rows[i].RowState != DataRowState.Deleted)
				Assert.AreEqual (tb1.Rows [i][j], dt.Rows [i][j], "#13 Elements differ at Row :{0} Column :{1}", i, j);
			}
		dt.Rows[0].RejectChanges();
		dt.Rows[1].RejectChanges();
	}

	[Test]
	public void TestDefaultValues ()
	{
	 	//Serialize Table
		DataTable tb1 = new DataTable ();
		tb1.Columns.Add ("id", typeof (int));
		tb1.Columns.Add ("Date", typeof (string));
		tb1.Columns["id"].DefaultValue = 10;
		tb1.Columns["Date"].DefaultValue = "9/15/2008";
		tb1.Rows.Add (tb1.NewRow());

		MemoryStream ms = new MemoryStream ();
		BinaryFormatter bf = new BinaryFormatter ();
		tb1.RemotingFormat = SerializationFormat.Binary;
		bf.Serialize (ms,tb1);
		byte [] serializedStream = ms.ToArray ();
		ms.Close ();
		//DserializeTable
		ms = new MemoryStream (serializedStream);
		DataTable dt = (DataTable)bf.Deserialize (ms);
		ms.Close ();

		//Table Data
		for (int i = 0; i < tb1.Rows.Count; i++) 
			for (int j = 0; j < tb1.Columns.Count; j++) {
				Assert.AreEqual (tb1.Columns[j].DefaultValue, dt.Rows [i][j], "#1 Element differs from DefaultValue at Row :{0} Column :{1}", i, j);
				Assert.AreEqual (tb1.Rows [i][j], dt.Rows [i][j], "#2 Elements differ at Row :{0} Column :{1}", i, j);
			}
	}

	[Test]
	public void TestEmptyTable ()
	{
	 	//Serialize Table
		DataTable tb1 = new DataTable ();
		tb1.Columns.Add ("id", typeof (int));
		tb1.Columns.Add ("Date", typeof (string));

		MemoryStream ms = new MemoryStream ();
		BinaryFormatter bf = new BinaryFormatter ();
		tb1.RemotingFormat = SerializationFormat.Binary;
		bf.Serialize (ms,tb1);
		byte [] serializedStream = ms.ToArray ();
		ms.Close ();
		//DserializeTable
		ms = new MemoryStream (serializedStream);
		DataTable dt = (DataTable)bf.Deserialize (ms);
		ms.Close ();

		Assert.AreEqual(tb1.Rows.Count, dt.Rows.Count);
	}

	[Test]
	public void Test_With_Null_Values1 ()
	{
	 	//Serialize Table
		DataTable tb1 = new DataTable ();
		tb1.Columns.Add ("id", typeof (int));
		tb1.Columns.Add ("Date", typeof (string));
		tb1.Rows.Add (new object[] {1, "A"});
		tb1.Rows.Add (new object[] {2, null});
		tb1.Rows.Add (new object[] {null, "B"});
		tb1.Rows.Add (new object[] {null, null});

		MemoryStream ms = new MemoryStream ();
		BinaryFormatter bf = new BinaryFormatter ();
		tb1.RemotingFormat = SerializationFormat.Binary;
		bf.Serialize (ms,tb1);
		byte [] serializedStream = ms.ToArray ();
		ms.Close ();
		//DserializeTable
		ms = new MemoryStream (serializedStream);
		DataTable dt = (DataTable)bf.Deserialize (ms);
		ms.Close ();
		//Table Data
		for (int i = 0; i < tb1.Rows.Count; i++) 
			for (int j = 0; j < tb1.Columns.Count; j++) {
				Assert.AreEqual (tb1.Rows [i][j], dt.Rows [i][j], "#1 Elements differ at Row :{0} Column :{1}", i, j);
			}
			
	}
	[Test]
	public void Test_With_Null_Values2 ()
	{
	 	//Serialize Table
		DataTable tb1 = new DataTable ();
		tb1.Columns.Add ("id", typeof (int));
		tb1.Columns.Add ("Date", typeof (string));
		tb1.Rows.Add (new object[] {1, "A"});
		tb1.Rows.Add (new object[] {2, null});
		tb1.Rows.Add (new object[] {null, "B"});
		tb1.Rows.Add (new object[] {null, null});

		BinaryFormatter bf = new BinaryFormatter ();
		tb1.RemotingFormat = SerializationFormat.Xml;
		FileStream fs = new FileStream (TestResourceHelper.GetFullPathOfResource ("Test/System.Data/binserialize/BS-tb2.bin"), FileMode.Open, FileAccess.Read);
		BinaryReader r = new BinaryReader (fs);
		byte [] serializedStream = r.ReadBytes ((int)fs.Length);
		r.Close ();
		fs.Close ();
		//DserializeTable
		MemoryStream ms = new MemoryStream (serializedStream);
		DataTable dt = (DataTable)bf.Deserialize (ms);
		ms.Close ();
		//Table Data
		for (int i = 0; i < tb1.Rows.Count; i++) 
			for (int j = 0; j < tb1.Columns.Count; j++) {
				Assert.AreEqual (tb1.Rows [i][j], dt.Rows [i][j], "#1 Elements differ at Row :{0} Column :{1}", i, j);
			}
			
	}
	[Test]
	public void Test_With_DateTime_Values1 ()
	{
	 	//Serialize Table
		DataTable tb1 = new DataTable ();
		DateTime dateTime = DateTime.UtcNow;
		tb1.Columns.Add ("id", typeof (int));
		tb1.Columns.Add ("Date", typeof (DateTime));
		tb1.Rows.Add (new object[] {1, "12-09-07"});
		tb1.Rows.Add (new object[] {2, "12-09-06"});
		//tb1.Rows.Add (new object[] {3, dateTime});	
		MemoryStream ms = new MemoryStream ();
		BinaryFormatter bf = new BinaryFormatter ();
		tb1.RemotingFormat = SerializationFormat.Binary;
		bf.Serialize (ms,tb1);
		byte [] serializedStream = ms.ToArray ();
		ms.Close ();
		//DserializeTable
		ms = new MemoryStream (serializedStream);
		DataTable dt = (DataTable)bf.Deserialize (ms);
		ms.Close ();
		//Table Data
		for (int i = 0; i < tb1.Rows.Count; i++) 
			for (int j = 0; j < tb1.Columns.Count; j++) {
				Assert.AreEqual (tb1.Rows [i][j], dt.Rows [i][j], "#1 Elements differ at Row :{0} Column :{1}", i, j);
			}
	}
	[Test]
	[Category ("NotWorking")]
	public void Test_With_DateTime_Values2 ()
	{
	 	//Serialize Table
		DataTable tb1 = new DataTable ();
		DateTime dateTime = DateTime.UtcNow;
		tb1.Columns.Add ("id", typeof (int));
		tb1.Columns.Add ("Date", typeof (DateTime));
		tb1.Rows.Add (new object[] {1, "12-09-07"});
		tb1.Rows.Add (new object[] {2, "12-09-06"});
		//tb1.Rows.Add (new object[] {3, dateTime});	
		BinaryFormatter bf = new BinaryFormatter ();
		tb1.RemotingFormat = SerializationFormat.Binary;
		FileStream fs = new FileStream (TestResourceHelper.GetFullPathOfResource ("Test/System.Data/binserialize/BS-tb3.bin"), FileMode.Open, FileAccess.Read);
		BinaryReader r = new BinaryReader (fs);
		byte [] serializedStream = r.ReadBytes ((int) fs.Length);
		r.Close ();
		fs.Close ();
		//DserializeTable
		MemoryStream ms = new MemoryStream (serializedStream);
		DataTable dt = (DataTable)bf.Deserialize (ms);
		ms.Close ();
		//Table Data
		for (int i = 0; i < tb1.Rows.Count; i++) 
			for (int j = 0; j < tb1.Columns.Count; j++) {
				Assert.AreEqual (tb1.Rows [i][j], dt.Rows [i][j], "#1 Elements differ at Row :{0} Column :{1}", i, j);
			}
	}
	[Test]
	public void DataSetSerializationTest1 ()
	{
		DataSet ds = new DataSet ();
		//Table1
		DataTable tb1 = new DataTable ();
		tb1.Columns.Add ("id", typeof (int));
		tb1.Columns.Add ("name", typeof (string));
		tb1.Rows.Add (new object[] {1, "A"});
		tb1.Rows.Add (new object[] {2, "B"});
		ds.Tables.Add (tb1);
		//Table2
		DataTable tb2 = new DataTable ();
		tb2.Columns.Add ("RollNO", typeof (int));
		tb2.Columns.Add ("Name", typeof (string));
		tb2.Rows.Add (new object[] {1, "A"});
		tb2.Rows.Add (new object[] {2, "B"});
		ds.Tables.Add (tb2);
		//Constraints and relations
		ForeignKeyConstraint fKey = new ForeignKeyConstraint (tb2.Columns ["RollNO"], 
								      tb1.Columns ["id"]);
		tb1.Constraints.Add (fKey);
		DataRelation rel = new DataRelation ("Relation1", tb1.Columns ["name"], 
						    tb2.Columns ["Name"]);
		ds.Relations.Add (rel);
		//SerializeDataSet
		MemoryStream ms = new MemoryStream ();
		BinaryFormatter bf = new BinaryFormatter ();
		ds.RemotingFormat = SerializationFormat.Binary;
		bf.Serialize (ms,ds);
		byte [] serializedStream = ms.ToArray ();
		ms.Close ();
		//DserializeDataSet
		ms = new MemoryStream (serializedStream);
		DataSet ds1 = (DataSet)bf.Deserialize (ms);
		ms.Close ();
		//Test DataSet Properties
		//Assert.AreEqual (ds.RemotingFormat, ds1.RemotingFormat, "#1 RemotingFormat is different");
		Assert.AreEqual (ds.DataSetName, ds1.DataSetName, "#2 DataSetName is different");
		Assert.AreEqual (ds.Namespace, ds1.Namespace, "#3 Namespace is different");
		Assert.AreEqual (ds.Prefix, ds1.Prefix, "#4 Prefix is different");
		Assert.AreEqual (ds.CaseSensitive, ds1.CaseSensitive, "#5 CaseSensitive property value is different");
		Assert.AreEqual (ds.Locale.LCID, ds1.Locale.LCID, "#6 DataSet LocaleLCID is different");
		Assert.AreEqual (ds.EnforceConstraints, ds1.EnforceConstraints, "#7 EnforceConstraints property value is different");
		Assert.AreEqual (ds.Tables.Count, ds1.Tables.Count, "#7 Table Count is different");

		//Test Constraints & relations

		//Table1
		Assert.AreEqual (ds.Tables [0].Constraints.Count, ds1.Tables [0].Constraints.Count, "#8 Number of constraint is different for Table :{0}", ds.Tables [0].TableName);

		for (int i = 0; i < ds.Tables [0].Constraints.Count; i++)
			Assert.AreEqual (ds.Tables [0].Constraints [i].GetType (), 
					 ds1.Tables [0].Constraints [i].GetType (), 
					 "#9 Constraint : {0} is Different", ds.Tables [0].Constraints [i]);

		//Table2 
		Assert.AreEqual (ds.Tables [1].Constraints.Count, ds1.Tables [1].Constraints.Count, "#10 Number of constraint is different for Table :{0}", ds.Tables [1].TableName);

		for (int i = 0; i < ds.Tables [1].Constraints.Count; i++)
			Assert.AreEqual (ds.Tables [1].Constraints [i].GetType (), 
					 ds1.Tables [1].Constraints [i].GetType (), 
					 "#11 Constraint : {0} is Different", ds.Tables [1].Constraints [i]);
		//Relations
		Assert.AreEqual (ds.Relations.Count, ds1.Relations.Count, "#12 Relation count is different");
		for (int i = 0; i < ds.Relations.Count; i++)
			Assert.AreEqual (ds.Relations [i].RelationName, ds1.Relations [i].RelationName, "#13 Relation Name is different for relation :{0}", ds.Relations [i].RelationName);

		for (int i = 0; i < ds.Relations.Count; i++)
			Assert.AreEqual (ds.Relations [i].ParentTable.TableName, 
					 ds1.Relations[i].ParentTable.TableName, "#14 Relation Name is different for relation :{0}", ds.Relations [i].ParentTable.TableName);	
		
		for (int i = 0; i < ds.Relations.Count; i++)
			Assert.AreEqual (ds.Relations [i].ChildTable.TableName, 
					 ds1.Relations[i].ChildTable.TableName, "#15 Relation Name is different for relation :{0}", ds.Relations [i].ChildTable.TableName);	
		
		//Table Data
		//Table1
		for (int i = 0; i < ds.Tables [0].Rows.Count; i++) 
			for (int j = 0; j < ds.Tables [0].Columns.Count; j++) {
				Assert.AreEqual (ds.Tables [0].Rows [i][j], ds1.Tables [0].Rows [i][j], 
						 "#16 Elements differ at Row :{0} Column :{1}", i, j);
			}
		//Table2
		for (int i = 0; i < ds.Tables [0].Rows.Count; i++) 
			for (int j = 0; j < ds.Tables [1].Columns.Count; j++) {
				Assert.AreEqual (ds.Tables [1].Rows [i][j], ds1.Tables [1].Rows [i][j], 
						 "#17 Elements differ at Row :{0} Column :{1}", i, j);
			}
		
	}
	[Test]
	[Category("NotWorking")] // Ordering issue
	public void DataSetSerializationTest2 ()
	{
		DataSet ds = new DataSet ();
		//Table1
		DataTable tb1 = new DataTable ();
		tb1.Columns.Add ("id", typeof (int));
		tb1.Columns.Add ("name", typeof (string));
		tb1.Rows.Add (new object[] {1, "A"});
		tb1.Rows.Add (new object[] {2, "B"});
		ds.Tables.Add (tb1);
		//Table2
		DataTable tb2 = new DataTable ();
		tb2.Columns.Add ("RollNO", typeof (int));
		tb2.Columns.Add ("Name", typeof (string));
		tb2.Rows.Add (new object[] {1, "A"});
		tb2.Rows.Add (new object[] {2, "B"});
		ds.Tables.Add (tb2);
		//Constraints and relations
		ForeignKeyConstraint fKey = new ForeignKeyConstraint (tb2.Columns ["RollNO"], 
								      tb1.Columns ["id"]);
		tb1.Constraints.Add (fKey);
		DataRelation rel = new DataRelation ("Relation1", tb1.Columns ["name"], 
						    tb2.Columns ["Name"]);
		ds.Relations.Add (rel);
		//SerializeDataSet
		BinaryFormatter bf = new BinaryFormatter ();
		ds.RemotingFormat = SerializationFormat.Xml;
		FileStream fs = new FileStream (TestResourceHelper.GetFullPathOfResource ("Test/System.Data/binserialize/BS-tb4.bin"), FileMode.Open, FileAccess.Read);
		BinaryReader r = new BinaryReader (fs);
		byte [] serializedStream = r.ReadBytes ((int) fs.Length);
		r.Close ();
		fs.Close ();
		//DserializeDataSet
		MemoryStream ms = new MemoryStream (serializedStream);
		DataSet ds1 = (DataSet)bf.Deserialize (ms);
		ms.Close ();
		//Test DataSet Properties
		//Assert.AreEqual (ds.RemotingFormat, ds1.RemotingFormat, "#1 RemotingFormat is different");
		Assert.AreEqual (ds.DataSetName, ds1.DataSetName, "#2 DataSetName is different");
		Assert.AreEqual (ds.Namespace, ds1.Namespace, "#3 Namespace is different");
		Assert.AreEqual (ds.Prefix, ds1.Prefix, "#4 Prefix is different");
		Assert.AreEqual (ds.CaseSensitive, ds1.CaseSensitive, "#5 CaseSensitive property value is different");
		Assert.AreEqual (ds.Locale.LCID, ds1.Locale.LCID, "#6 DataSet LocaleLCID is different");
		Assert.AreEqual (ds.EnforceConstraints, ds1.EnforceConstraints, "#7 EnforceConstraints property value is different");
		Assert.AreEqual (ds.Tables.Count, ds1.Tables.Count, "#7 Table Count is different");

		//Test Constraints & relations

		//Table1
		Assert.AreEqual (ds.Tables [0].Constraints.Count, ds1.Tables [0].Constraints.Count, "#8 Number of constraint is different for Table :{0}", ds.Tables [0].TableName);

		for (int i = 0; i < ds.Tables [0].Constraints.Count; i++)
			Assert.AreEqual (ds.Tables [0].Constraints [i].GetType (), 
					 ds1.Tables [0].Constraints [i].GetType (), 
					 "#9 Constraint : {0} is Different", ds.Tables [0].Constraints [i]);

		//Table2 
		Assert.AreEqual (ds.Tables [1].Constraints.Count, ds1.Tables [1].Constraints.Count, "#10 Number of constraint is different for Table :{0}", ds.Tables [1].TableName);

		for (int i = 0; i < ds.Tables [1].Constraints.Count; i++)
			Assert.AreEqual (ds.Tables [1].Constraints [i].GetType (), 
					 ds1.Tables [1].Constraints [i].GetType (), 
					 "#11 Constraint : {0} is Different", ds.Tables [1].Constraints [i]);
		//Relations
		Assert.AreEqual (ds.Relations.Count, ds1.Relations.Count, "#12 Relation count is different");
		for (int i = 0; i < ds.Relations.Count; i++)
			Assert.AreEqual (ds.Relations [i].RelationName, ds1.Relations [i].RelationName, "#13 Relation Name is different for relation :{0}", ds.Relations [i].RelationName);

		for (int i = 0; i < ds.Relations.Count; i++)
			Assert.AreEqual (ds.Relations [i].ParentTable.TableName, 
					 ds1.Relations[i].ParentTable.TableName, "#14 Relation Name is different for relation :{0}", ds.Relations [i].ParentTable.TableName);	
		
		for (int i = 0; i < ds.Relations.Count; i++)
			Assert.AreEqual (ds.Relations [i].ChildTable.TableName, 
					 ds1.Relations[i].ChildTable.TableName, "#15 Relation Name is different for relation :{0}", ds.Relations [i].ChildTable.TableName);	
		
		//Table Data
		//Table1
		for (int i = 0; i < ds.Tables [0].Rows.Count; i++) 
			for (int j = 0; j < ds.Tables [0].Columns.Count; j++) {
				Assert.AreEqual (ds.Tables [0].Rows [i][j], ds1.Tables [0].Rows [i][j], 
						 "#16 Elements differ at Row :{0} Column :{1}", i, j);
			}
		//Table2
		for (int i = 0; i < ds.Tables [0].Rows.Count; i++) 
			for (int j = 0; j < ds.Tables [1].Columns.Count; j++) {
				Assert.AreEqual (ds.Tables [1].Rows [i][j], ds1.Tables [1].Rows [i][j], 
						 "#17 Elements differ at Row :{0} Column :{1}", i, j);
			}
		
	}
	[Test]
	public void Constraint_Relations_Test1 ()
	{
		//Serialize DataSet
		DataSet ds = new DataSet ();
		
		DataTable tb1 = new DataTable ();
		tb1.Columns.Add ("id", typeof (int));
		tb1.Columns.Add ("name", typeof (string));
		tb1.Rows.Add (new object[] {1, "A"});
		tb1.Rows.Add (new object[] {2, "B"});
		ds.Tables.Add (tb1);
		//Table2
		DataTable tb2 = new DataTable ();
		tb2.Columns.Add ("eid", typeof (int));
		tb2.Columns.Add ("SSN", typeof (int));
		tb2.Columns.Add ("DOJ", typeof (DateTime));
		tb2.Rows.Add (new object[] {1, 111, "07-25-06"});
		tb2.Rows.Add (new object[] {2, 112, "07-19-06"});
		tb2.Rows.Add (new object[] {3, 113, "07-22-06"});
		ds.Tables.Add (tb2);
		//Table3
		DataTable tb3 = new DataTable ();
		tb3.Columns.Add ("eid", typeof (int));
		tb3.Columns.Add ("Salary", typeof (long));
		tb3.Rows.Add (new object[] {1, 20000});
		tb3.Rows.Add (new object[] {2, 30000});
		ds.Tables.Add (tb3);
		//Table4
		DataTable tb4 = new DataTable ();
		tb4.Columns.Add ("ssn", typeof (int));
		tb4.Columns.Add ("Name", typeof (string));
		tb4.Columns.Add ("DOB", typeof (DateTime));
		tb4.Rows.Add (new object[] {112, "A", "09-12-81"});
		tb4.Rows.Add (new object[] {113, "B", "09-12-82"});
		ds.Tables.Add (tb4);
		
		//Constraints
		UniqueConstraint uKey = new UniqueConstraint (tb1.Columns ["id"]);
		/*
		DataColumn[] parentColumns = new DataColumn[2];
		parentColumns[0] = tb1.Columns["id"];
		parentColumns[1] = tb1.Columns["name"];
		DataColumn[] childColumns = new DataColumn[2];
		childColumns[0] = tb4.Columns["ssn"];
		childColumns[1] = tb4.Columns["Name"];
		ForeignKeyConstraint fKey1 = new ForeignKeyConstraint(childColumns,
								      parentColumns);
		*/
		ForeignKeyConstraint fKey1 = new ForeignKeyConstraint (tb2.Columns ["eid"], 
									tb1.Columns ["id"]);
		ForeignKeyConstraint fKey2 = new ForeignKeyConstraint (tb2.Columns ["eid"], 
									tb3.Columns ["eid"]);
		DataRelation rel = new DataRelation ("Relation1", tb1.Columns ["name"], tb4.Columns ["Name"]);
		DataRelation rel1 = new DataRelation ("Relation2", tb2.Columns ["SSN"], tb4.Columns ["ssn"]);
		tb1.Constraints.Add (uKey);	
		tb1.Constraints.Add (fKey1);
		tb3.Constraints.Add (fKey2);
		ds.Relations.Add (rel);
		ds.Relations.Add (rel1);
		ds.AcceptChanges();

		//SerializeDataSet
		MemoryStream ms = new MemoryStream ();
		BinaryFormatter bf = new BinaryFormatter ();
		ds.RemotingFormat = SerializationFormat.Binary;
		bf.Serialize (ms,ds);
		byte [] serializedStream = ms.ToArray ();
		ms.Close();
		//DserializeDataSet
		ms = new MemoryStream (serializedStream);
		DataSet ds1 = (DataSet)bf.Deserialize (ms);
		ms.Close ();
		Assert.AreEqual (ds.Tables.Count, ds1.Tables.Count, "#1 Number of Table differs");
		//Test Constraints
		//Table1
		Assert.AreEqual (ds.Tables [0].Constraints.Count, ds1.Tables [0].Constraints.Count, "#2 Number of Constraints differs in Table :{0}", ds.Tables [0]);
		for (int i = 0; i < ds.Tables [0].Constraints.Count; i++)
			Assert.AreEqual (ds.Tables [0].Constraints [i].GetType (), 
					 ds1.Tables [0].Constraints [i].GetType (), 
					 "#3 Constraint : {0} is Different", ds.Tables [0].Constraints [i]);
		//Table2
		Assert.AreEqual (ds.Tables [1].Constraints.Count, ds1.Tables [1].Constraints.Count, "#4 Number of Constraints differs in Table :{0}", ds.Tables [1]);
		for (int i = 0; i < ds.Tables [1].Constraints.Count; i++)
			Assert.AreEqual (ds.Tables [1].Constraints [i].GetType (), 
					 ds1.Tables [1].Constraints [i].GetType (), 
					 "#5 Constraint : {0} is Different", ds.Tables [1].Constraints [i]);
		//Table3
		Assert.AreEqual (ds.Tables [2].Constraints.Count, ds1.Tables [2].Constraints.Count, "#5 Number of Constraints differs in Table :{0}", ds.Tables [2]);
		for (int i = 0; i < ds.Tables [2].Constraints.Count; i++)
			Assert.AreEqual (ds.Tables [2].Constraints [i].GetType (), 
					 ds1.Tables [2].Constraints [i].GetType (), 
					 "#6 Constraint : {0} is Different", ds.Tables [2].Constraints [i]);
		//Table4
		Assert.AreEqual (ds.Tables [3].Constraints.Count, ds1.Tables [3].Constraints.Count, "#7 Number of Constraints differs in Table :{0}", ds.Tables [3]);
		for (int i = 0; i < ds.Tables [3].Constraints.Count; i++) 
			Assert.AreEqual (ds.Tables [3].Constraints [i].GetType (), 
					 ds1.Tables [3].Constraints [i].GetType (), 
					 "#8 Constraint : {0} is Different", ds.Tables [3].Constraints [i]);
		//Relations
		Assert.AreEqual (ds.Relations.Count, ds1.Relations.Count, "#8 Number of realtions differ");
		for (int i = 0; i < ds.Relations.Count; i++)
			Assert.AreEqual (ds.Relations [i].RelationName, ds.Relations [i].RelationName, "#9 Relation : {0} differs", ds.Relations [i]);
	}
	[Test]
	[Category("NotWorking")] // Ordering issue
	public void Constraint_Relations_Test2 ()
	{
		//Serialize DataSet
		DataSet ds = new DataSet ();
		
		DataTable tb1 = new DataTable ();
		tb1.Columns.Add ("id", typeof (int));
		tb1.Columns.Add ("name", typeof (string));
		tb1.Rows.Add (new object[] {1, "A"});
		tb1.Rows.Add (new object[] {2, "B"});
		ds.Tables.Add (tb1);
		//Table2
		DataTable tb2 = new DataTable ();
		tb2.Columns.Add ("eid", typeof (int));
		tb2.Columns.Add ("SSN", typeof (int));
		tb2.Columns.Add ("DOJ", typeof (DateTime));
		tb2.Rows.Add (new object[] {1, 111, "07-25-06"});
		tb2.Rows.Add (new object[] {2, 112, "07-19-06"});
		tb2.Rows.Add (new object[] {3, 113, "07-22-06"});
		ds.Tables.Add (tb2);
		//Table3
		DataTable tb3 = new DataTable ();
		tb3.Columns.Add ("eid", typeof (int));
		tb3.Columns.Add ("Salary", typeof (long));
		tb3.Rows.Add (new object[] {1, 20000});
		tb3.Rows.Add (new object[] {2, 30000});
		ds.Tables.Add (tb3);
		//Table4
		DataTable tb4 = new DataTable ();
		tb4.Columns.Add ("ssn", typeof (int));
		tb4.Columns.Add ("Name", typeof (string));
		tb4.Columns.Add ("DOB", typeof (DateTime));
		tb4.Rows.Add (new object[] {112, "A", "09-12-81"});
		tb4.Rows.Add (new object[] {113, "B", "09-12-82"});
		ds.Tables.Add (tb4);
		
		//Constraints
		UniqueConstraint uKey = new UniqueConstraint (tb1.Columns ["id"]);
		/*
		DataColumn[] parentColumns = new DataColumn[2];
		parentColumns[0] = tb1.Columns["id"];
		parentColumns[1] = tb1.Columns["name"];
		DataColumn[] childColumns = new DataColumn[2];
		childColumns[0] = tb4.Columns["ssn"];
		childColumns[1] = tb4.Columns["Name"];
		ForeignKeyConstraint fKey1 = new ForeignKeyConstraint(childColumns,
								      parentColumns);
		*/
		ForeignKeyConstraint fKey1 = new ForeignKeyConstraint (tb2.Columns ["eid"], 
									tb1.Columns ["id"]);
		ForeignKeyConstraint fKey2 = new ForeignKeyConstraint (tb2.Columns ["eid"], 
									tb3.Columns ["eid"]);
		DataRelation rel = new DataRelation ("Relation1", tb1.Columns ["name"], tb4.Columns ["Name"]);
		DataRelation rel1 = new DataRelation ("Relation2", tb2.Columns ["SSN"], tb4.Columns ["ssn"]);
		tb1.Constraints.Add (uKey);	
		tb1.Constraints.Add (fKey1);
		tb3.Constraints.Add (fKey2);
		ds.Relations.Add (rel);
		ds.Relations.Add (rel1);
		ds.AcceptChanges();

		//SerializeDataSet
		BinaryFormatter bf = new BinaryFormatter ();
		ds.RemotingFormat = SerializationFormat.Xml;
		FileStream fs = new FileStream (TestResourceHelper.GetFullPathOfResource ("Test/System.Data/binserialize/BS-tb5.bin"), FileMode.Open, FileAccess.Read);
		BinaryReader r = new BinaryReader (fs);
		byte [] serializedStream = r.ReadBytes ((int)fs.Length);
		r.Close ();
		fs.Close ();
		//DserializeDataSet
		MemoryStream ms = new MemoryStream (serializedStream);
		DataSet ds1 = (DataSet)bf.Deserialize (ms);
		ms.Close ();
		Assert.AreEqual (ds.Tables.Count, ds1.Tables.Count, "#1 Number of Table differs");
		//Test Constraints
		//Table1
		Assert.AreEqual (ds.Tables [0].Constraints.Count, ds1.Tables [0].Constraints.Count, "#2 Number of Constraints differs in Table :{0}", ds.Tables [0]);
		for (int i = 0; i < ds.Tables [0].Constraints.Count; i++)
			Assert.AreEqual (ds.Tables [0].Constraints [i].GetType (), 
					 ds1.Tables [0].Constraints [i].GetType (), 
					 "#3 Constraint : {0} is Different", ds.Tables [0].Constraints [i]);
		//Table2
		Assert.AreEqual (ds.Tables [1].Constraints.Count, ds1.Tables [1].Constraints.Count, "#4 Number of Constraints differs in Table :{0}", ds.Tables [1]);
		for (int i = 0; i < ds.Tables [1].Constraints.Count; i++)
			Assert.AreEqual (ds.Tables [1].Constraints [i].GetType (), 
					 ds1.Tables [1].Constraints [i].GetType (), 
					 "#5 Constraint : {0} is Different", ds.Tables [1].Constraints [i]);
		//Table3
		Assert.AreEqual (ds.Tables [2].Constraints.Count, ds1.Tables [2].Constraints.Count, "#5 Number of Constraints differs in Table :{0}", ds.Tables [2]);
		for (int i = 0; i < ds.Tables [2].Constraints.Count; i++)
			Assert.AreEqual (ds.Tables [2].Constraints [i].GetType (), 
					 ds1.Tables [2].Constraints [i].GetType (), 
					 "#6 Constraint : {0} is Different", ds.Tables [2].Constraints [i]);
		//Table4
		Assert.AreEqual (ds.Tables [3].Constraints.Count, ds1.Tables [3].Constraints.Count, "#7 Number of Constraints differs in Table :{0}", ds.Tables [3]);
		for (int i = 0; i < ds.Tables [3].Constraints.Count; i++) 
			Assert.AreEqual (ds.Tables [3].Constraints [i].GetType (), 
					 ds1.Tables [3].Constraints [i].GetType (), 
					 "#8 Constraint : {0} is Different", ds.Tables [3].Constraints [i]);
		//Relations
		Assert.AreEqual (ds.Relations.Count, ds1.Relations.Count, "#8 Number of realtions differ");
		for (int i = 0; i < ds.Relations.Count; i++)
			Assert.AreEqual (ds.Relations [i].RelationName, ds.Relations [i].RelationName, "#9 Relation : {0} differs", ds.Relations [i]);
	}
}

}
