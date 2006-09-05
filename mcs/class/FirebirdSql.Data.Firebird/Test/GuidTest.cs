/*
 *  Firebird ADO.NET Data provider for .NET and Mono 
 * 
 *     The contents of this file are subject to the Initial 
 *     Developer's Public License Version 1.0 (the "License"); 
 *     you may not use this file except in compliance with the 
 *     License. You may obtain a copy of the License at 
 *     http://www.firebirdsql.org/index.php?op=doc&id=idpl
 *
 *     Software distributed under the License is distributed on 
 *     an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either 
 *     express or implied.  See the License for the specific 
 *     language governing rights and limitations under the License.
 * 
 *  Copyright (c) 2002, 2005 Carlos Guzman Alvarez
 *  All Rights Reserved.
 */

using System;
using System.Data;
using FirebirdSql.Data.Firebird;
using NUnit.Framework;

namespace FirebirdSql.Data.Firebird.Tests
{
	/// <summary>
	/// All the test in this TestFixture are using implicit transaction support.
	/// </summary>
	[TestFixture]
	public class GuidTest : BaseTest
	{
		public GuidTest() : base()
		{
		}

		[Test]
		public void InsertGuidTest()
		{
			FbCommand createTable = new FbCommand("CREATE TABLE GUID_TEST (GUID_FIELD CHAR(16) CHARACTER SET OCTETS)", Connection);
			createTable.ExecuteNonQuery();
			createTable.Dispose();

			Guid newGuid = Guid.Empty;
			Guid guidValue = Guid.NewGuid();

			// Insert the Guid
			FbCommand insert = new FbCommand("INSERT INTO GUID_TEST (GUID_FIELD) VALUES (@GuidValue)", Connection);
			insert.Parameters.Add("@GuidValue", FbDbType.Guid).Value = guidValue;
			insert.ExecuteNonQuery();
			insert.Dispose();

			// Select the value
			FbCommand select = new FbCommand("SELECT * FROM GUID_TEST", Connection);
			FbDataReader r = select.ExecuteReader();
			if (r.Read())
			{
				newGuid = r.GetGuid(0);
			}

			Assert.AreEqual(guidValue, newGuid);
		}

		[Test]
		public void InsertNullGuidTest()
		{
			FbCommand createTable = new FbCommand("CREATE TABLE GUID_TEST (INT_FIELD INTEGER, GUID_FIELD CHAR(16) CHARACTER SET OCTETS)", Connection);
			createTable.ExecuteNonQuery();
			createTable.Dispose();

			// Insert the Guid
			FbCommand insert = new FbCommand("INSERT INTO GUID_TEST (INT_FIELD, GUID_FIELD) VALUES (@IntField, @GuidValue)", Connection);
			insert.Parameters.Add("@IntField", FbDbType.Integer).Value = this.GetId();
			insert.Parameters.Add("@GuidValue", FbDbType.Guid).Value = DBNull.Value;
			insert.ExecuteNonQuery();
			insert.Dispose();

			// Select the value
			FbCommand select = new FbCommand("SELECT * FROM GUID_TEST", Connection);
			FbDataReader r = select.ExecuteReader();
			if (r.Read())
			{
				if (!r.IsDBNull(1))
				{
					throw new Exception();
				}
			}
		}
	}
}
