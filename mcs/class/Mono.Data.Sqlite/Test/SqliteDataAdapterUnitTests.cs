// SqliteDataAdapterUnitTests.cs - NUnit Test Cases for Mono.Data.Sqlite.SqliteDataAdapter
//
// Author(s):	Thomas Zoechling <thomas.zoechling@gmx.at>


using System;
using System.Data;
using System.IO;
using System.Text;
using Mono.Data.Sqlite;
using NUnit.Framework;

namespace MonoTests.Mono.Data.Sqlite
{
	[TestFixture]
	public class SqliteDataAdapterUnitTests
	{
		string _uri;
		string _connectionString;
		SqliteConnection _conn;
        
        	[SetUp]
        	public void SetUp ()
        	{
			_uri = Path.GetTempFileName ();
			_connectionString = "URI=file://" + _uri + ", version=3";
			_conn = new SqliteConnection (_connectionString);
        	}

        	[TearDown]
        	public void TearDown ()
        	{
        		if (File.Exists (_uri))
        			File.Delete (_uri);
        	}

		SqliteDataAdapter PrepareDataAdapter()
		{
			SqliteCommand select  = new SqliteCommand("SELECT t, f, i, b FROM t1",_conn);
			SqliteCommand update = new SqliteCommand("UPDATE t1 SET t = :textP, f = :floatP, i = :integerP, n=:blobP WHERE t = :textP ");
			update.Connection=_conn;
			SqliteCommand delete = new SqliteCommand("DELETE FROM t1 WHERE t = :textP");
			delete.Connection=_conn;
			SqliteCommand insert = new SqliteCommand("INSERT INTO t1  (t, f, i, b ) VALUES(:textP,:floatP,:integerP,:blobP)");
			insert.Connection=_conn;
			SqliteDataAdapter custDA = new SqliteDataAdapter(select);
		
			SqliteParameter textP = new SqliteParameter();
			textP.ParameterName = "textP";
			textP.SourceColumn = "t";
		
			SqliteParameter floatP = new SqliteParameter();
			floatP.ParameterName = "floatP";
			floatP.SourceColumn = "f";
		
			SqliteParameter integerP = new SqliteParameter();
			integerP.ParameterName ="integerP";
			integerP.SourceColumn = "i";

			SqliteParameter blobP = new SqliteParameter();
			blobP.ParameterName = "blobP";
			blobP.SourceColumn = "b";
		
			update.Parameters.Add(textP);
			update.Parameters.Add(floatP);
			update.Parameters.Add(integerP);
			update.Parameters.Add(blobP);
		
			delete.Parameters.Add(textP);
		
			insert.Parameters.Add(textP);
			insert.Parameters.Add(floatP);
			insert.Parameters.Add(integerP);
			insert.Parameters.Add(blobP);
		
			custDA.UpdateCommand = update;
			custDA.DeleteCommand = delete;
			custDA.InsertCommand = insert;
		
			return custDA;
		}

		[Test]
		[Category ("NotWorking")]
		public void GetSchemaTable()
		{
			_conn.ConnectionString = _connectionString;
			SqliteDataReader reader = null;
			using (_conn) 
			{
				_conn.Open ();
				SqliteCommand cmd = (SqliteCommand) _conn.CreateCommand ();
				cmd.CommandText = "select * from t1";
				reader = cmd.ExecuteReader ();
				try 
				{
					DataTable dt = reader.GetSchemaTable ();
					Assert.IsNotNull (dt, "#GS1 should return valid table");
					Assert.IsTrue (dt.Rows.Count > 0, "#GS2 should return with rows ;-)");
				}
				finally 
				{
					if (reader != null && !reader.IsClosed)
						reader.Close ();
					_conn.Close ();
				}
			}
		}

		[Test]
		[Category ("NotWorking")]
		public void DataAdapterRandomValues()
		{
			SqliteDataAdapter da = PrepareDataAdapter();
			DataSet ds = new DataSet();
			int i = 0;
			Random random = new Random();
			using(_conn)
			{
				_conn.Open();
				da.Fill(ds);
				for(; i<300;i++)
				{
					DataRow dr = ds.Tables[0].NewRow();
					
					foreach(DataColumn dc in ds.Tables[0].Columns)
					{
						switch(dc.DataType.Name)
						{
							case "String":
							{
								int ml=0;
								if(dc.MaxLength!=-1)
								{
									ml=dc.MaxLength;
								}
								else
								{
									ml=256;
								}
								StringBuilder builder = new StringBuilder(ml);
								for (int k=0; k < random.Next(ml); k++)
								{
									builder.Append((char)random.Next(65536));
						
								}
								string curs = builder.ToString();
								dr[dc]=curs;
								break;
							}

							case "Int32":
							{
								dr[dc]=random.Next(65536);
								break;
							}

							case "Int64":
							{
								dr[dc]=Convert.ToInt64(random.Next(65536));
								break;
							}
						}
					}
					ds.Tables[0].Rows.Add(dr);
				}
				int res = da.Update(ds);
				Assert.AreEqual(i,res);
			}
		}
	}
}
