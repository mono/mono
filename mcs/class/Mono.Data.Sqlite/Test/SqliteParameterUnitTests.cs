// SqliteParameterUnitTests.cs - NUnit Test Cases for Mono.Data.Sqlite.SqliteParameter
//
// Author(s):	Thomas Zoechling <thomas.zoechling@gmx.at>


using System;
using System.Data;
using System.IO;
using System.Text;
using Mono.Data.Sqlite;
#if USE_MSUNITTEST
#if WINDOWS_PHONE || NETFX_CORE
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using TestFixtureAttribute = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestClassAttribute;
using SetUpAttribute = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestInitializeAttribute;
using TearDownAttribute = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestCleanupAttribute;
using TestAttribute = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestMethodAttribute;
using CategoryAttribute = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestCategoryAttribute;
#else // !WINDOWS_PHONE && !NETFX_CORE
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestFixtureAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute;
using SetUpAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestInitializeAttribute;
using TearDownAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestCleanupAttribute;
using TestAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute;
using CategoryAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute;
#endif // WINDOWS_PHONE || NETFX_CORE
#else // !USE_MSUNITTEST
using NUnit.Framework;
#endif // USE_MSUNITTEST

namespace MonoTests.Mono.Data.Sqlite
{
	[TestFixture]
	public class SqliteParameterUnitTests : SqliteUnitTestsBaseWithT1
	{
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
			floatP.Value=(double)random.Next(999);
			integerP.Value=(long)random.Next(999);
			blobP.Value=System.Text.Encoding.UTF8.GetBytes("\u05D0\u05D1\u05D2" + builder.ToString());
			
			SqliteCommand selectCmd = new SqliteCommand("SELECT * from t1", _conn);

			using(_conn)
			{
				_conn.Open();
				int res = insertCmd.ExecuteNonQuery();
				Assert.AreEqual(2, res); // delete + insert = 2 changes
				
				using (IDataReader reader = selectCmd.ExecuteReader()) {
					Assert.AreEqual(reader.Read(), true);
					Assert.AreEqual(reader["t"], textP.Value);
					Assert.AreEqual(reader["f"], floatP.Value);
					Assert.AreEqual(reader["i"], integerP.Value);
					
					object compareValue;
#if NET_2_0
					if (blobP.Value is byte[])
						compareValue = System.Text.Encoding.UTF8.GetString ((byte[])blobP.Value, 0, ((byte[])blobP.Value).Length);
					else
#endif
						compareValue = blobP.Value;
					Assert.AreEqual(reader["b"], compareValue);
					Assert.AreEqual(reader.Read(), false);
				}
				insertCmd.Dispose();
				selectCmd.Dispose();
			}
		}
	}
}
