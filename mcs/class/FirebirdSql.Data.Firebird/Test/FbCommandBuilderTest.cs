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
 *  Copyright (c) 2002, 2004 Carlos Guzman Alvarez
 *  All Rights Reserved.
 */

using NUnit.Framework;
using System;
using System.Data;
using FirebirdSql.Data.Firebird;

namespace FirebirdSql.Data.Firebird.Tests
{
	[TestFixture]
	public class FbCommandBuilderTest : BaseTest
	{
		FbDataAdapter	adapter;
		
		public FbCommandBuilderTest() : base(false)
		{
		}
		
		[SetUp]
		public override void SetUp()
		{
			base.SetUp();
			adapter = new FbDataAdapter(new FbCommand("select * from TEST where VARCHAR_FIELD = ?", Connection));
		}
		
		[TearDown]
		public override void TearDown()
		{
			adapter.Dispose();
			base.TearDown();
		}
		
		[Test]
		public void GetInsertCommandTest()
		{
			FbCommandBuilder builder = new FbCommandBuilder(adapter);

			Console.WriteLine();
			Console.WriteLine("CommandBuilder - GetInsertCommand Method Test");
			
			Console.WriteLine( builder.GetInsertCommand().CommandText );

			builder.Dispose();
		}
		
		[Test]
		public void GetUpdateCommandTest()
		{
			FbCommandBuilder builder = new FbCommandBuilder(adapter);

			Console.WriteLine();
			Console.WriteLine("CommandBuilder - GetUpdateCommand Method Test");
			
			Console.WriteLine( builder.GetUpdateCommand().CommandText );

			builder.Dispose();
		}
		
		[Test]
		public void GetDeleteCommandTest()
		{
			FbCommandBuilder builder = new FbCommandBuilder(adapter);

			Console.WriteLine();
			Console.WriteLine("CommandBuilder - GetDeleteCommand Method Test");
			
			Console.WriteLine( builder.GetDeleteCommand().CommandText );

			builder.Dispose();
		}
		
		[Test]
		public void RefreshSchemaTest()
		{
			FbCommandBuilder builder = new FbCommandBuilder(adapter);
			
			Console.WriteLine();
			Console.WriteLine("CommandBuilder - RefreshSchema Method Test - Commands for original SQL statement: ");

			Console.WriteLine( builder.GetInsertCommand().CommandText );
			Console.WriteLine( builder.GetUpdateCommand().CommandText );
			Console.WriteLine( builder.GetDeleteCommand().CommandText );
			
			adapter.SelectCommand.CommandText = "select * from TEST where BIGINT_FIELD = ?";
			
			builder.RefreshSchema();
			
			Console.WriteLine();
			Console.WriteLine("CommandBuilder - RefreshSchema Method Test - Commands for new SQL statement: ");
						
			Console.WriteLine( builder.GetInsertCommand().CommandText );
			Console.WriteLine( builder.GetUpdateCommand().CommandText );
			Console.WriteLine( builder.GetDeleteCommand().CommandText );

			builder.Dispose();
		}
		
		[Test]
		public void CommandBuilderWithExpressionFieldTest()
		{
			adapter.SelectCommand.CommandText = "select TEST.*, 0 AS VALOR from TEST";

			FbCommandBuilder builder = new FbCommandBuilder(adapter);

			Console.WriteLine();
			Console.WriteLine("CommandBuilder - GetUpdateCommand Method Test");
			
			Console.WriteLine( builder.GetUpdateCommand().CommandText );

			builder.Dispose();
		}

		[Test]
		public void DeriveParameters()
		{	
			FbCommand command = new FbCommand("GETVARCHARFIELD", Connection);
			
			command.CommandType = CommandType.StoredProcedure;
						
			FbCommandBuilder.DeriveParameters(command);

            Assert.AreEqual(2, command.Parameters.Count);
        }

		[Test]
		public void DeriveParameters2()
		{
			FbTransaction transaction = Connection.BeginTransaction();

			FbCommand command = new FbCommand("GETVARCHARFIELD", Connection, transaction);
			
			command.CommandType = CommandType.StoredProcedure;
						
			FbCommandBuilder.DeriveParameters(command);

            Assert.AreEqual(2, command.Parameters.Count);

			transaction.Commit();
		}

		[Test]
		public void TestWithClosedConnection()
		{
			Connection.Close();

			FbCommandBuilder builder = new FbCommandBuilder(adapter);
			
			Console.WriteLine();
			Console.WriteLine("CommandBuilder - RefreshSchema Method Test - Commands for original SQL statement: ");

			Console.WriteLine( builder.GetInsertCommand().CommandText );
			Console.WriteLine( builder.GetUpdateCommand().CommandText );
			Console.WriteLine( builder.GetDeleteCommand().CommandText );
			
			adapter.SelectCommand.CommandText = "select * from TEST where BIGINT_FIELD = ?";
			
			builder.RefreshSchema();
			
			Console.WriteLine();
			Console.WriteLine("CommandBuilder - RefreshSchema Method Test - Commands for new SQL statement: ");
						
			Console.WriteLine( builder.GetInsertCommand().CommandText );
			Console.WriteLine( builder.GetUpdateCommand().CommandText );
			Console.WriteLine( builder.GetDeleteCommand().CommandText );

			builder.Dispose();
		}
	}
}
