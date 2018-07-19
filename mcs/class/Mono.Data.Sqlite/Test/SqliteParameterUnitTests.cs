// SqliteParameterUnitTests.cs - NUnit Test Cases for Mono.Data.Sqlite.SqliteParameter
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
	public class SqliteParameterUnitTests
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
		[Test]
		[Category ("NotWorking")]
		// fails randomly :)
		public void InsertRandomValuesWithParameter()
		{
			SqliteParameter textP = new SqliteParameter();
			textP.ParameterName = "textP";
			textP.SourceColumn = "t";
		
			SqliteParameter floatP = new SqliteParameter();
			floatP.ParameterName = "floatP";
			floatP.SourceColumn = "nu";
		
			SqliteParameter integerP = new SqliteParameter();
			integerP.ParameterName ="integerP";
			integerP.SourceColumn = "i";

			SqliteParameter blobP = new SqliteParameter();
			blobP.ParameterName = "blobP";
			blobP.SourceColumn = "b";

			Random random = new Random();
			StringBuilder builder = new StringBuilder();
			for (int k=0; k < random.Next(0,100); k++)
			{
				builder.Append((char)random.Next(65536));
			}
			
			SqliteCommand insertCmd = new SqliteCommand("DELETE FROM t1; INSERT INTO t1  (t, f, i, b ) VALUES(:textP,:floatP,:integerP,:blobP)",_conn);
			
			insertCmd.Parameters.Add(textP);
			insertCmd.Parameters.Add(floatP);
			insertCmd.Parameters.Add(blobP);
			insertCmd.Parameters.Add(integerP);
			
			textP.Value=builder.ToString();
			floatP.Value=Convert.ToInt64(random.Next(999));
			integerP.Value=random.Next(999);
			blobP.Value=global::System.Text.Encoding.UTF8.GetBytes("\u05D0\u05D1\u05D2" + builder.ToString());
			
			SqliteCommand selectCmd = new SqliteCommand("SELECT * from t1", _conn);

			using(_conn)
			{
				_conn.Open();
				int res = insertCmd.ExecuteNonQuery();
				Assert.AreEqual(res,1);
				
				using (IDataReader reader = selectCmd.ExecuteReader()) {
					Assert.AreEqual(reader.Read(), true);
					Assert.AreEqual(reader["t"], textP.Value);
					Assert.AreEqual(reader["f"], floatP.Value);
					Assert.AreEqual(reader["i"], integerP.Value);
					
					object compareValue;
					if (blobP.Value is byte[])
						compareValue = global::System.Text.Encoding.UTF8.GetString ((byte[])blobP.Value);
					else
						compareValue = blobP.Value;
					Assert.AreEqual(reader["b"], compareValue);
					Assert.AreEqual(reader.Read(), false);
				}
			}
		}
	}
}
