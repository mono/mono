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
	public class FbDatabaseInfoTest : BaseTest 
	{
		public FbDatabaseInfoTest() : base(false)
		{		
		}

		[Test]
		public void DatabaseInfoTest()
		{
			FbDatabaseInfo dbInfo = new FbDatabaseInfo(Connection);
			
			Console.WriteLine("Server Version: {0}", dbInfo.ServerVersion);
			Console.WriteLine("ISC Version : {0}", dbInfo.IscVersion);
			Console.WriteLine("Server Class : {0}", dbInfo.ServerClass);
			Console.WriteLine("Max memory : {0}", dbInfo.MaxMemory);
			Console.WriteLine("Current memory : {0}", dbInfo.CurrentMemory);
			Console.WriteLine("Page size : {0}", dbInfo.PageSize);
			Console.WriteLine("ODS Mayor version : {0}", dbInfo.OdsVersion);
			Console.WriteLine("ODS Minor version : {0}", dbInfo.OdsMinorVersion);
			Console.WriteLine("Allocation pages: {0}", dbInfo.AllocationPages);
			Console.WriteLine("Base level: {0}", dbInfo.BaseLevel);
			Console.WriteLine("Database id: {0}", dbInfo.DbId);
			Console.WriteLine("Database implementation: {0}", dbInfo.Implementation);
			Console.WriteLine("No reserve: {0}", dbInfo.NoReserve);
			Console.WriteLine("Forced writes: {0}", dbInfo.ForcedWrites);
			Console.WriteLine("Sweep interval: {0}", dbInfo.SweepInterval);
			Console.WriteLine("Number of page fetches: {0}", dbInfo.Fetches);
			Console.WriteLine("Number of page marks: {0}", dbInfo.Marks);
			Console.WriteLine("Number of page reads: {0}", dbInfo.Reads);
			Console.WriteLine("Number of page writes: {0}", dbInfo.Writes);
			Console.WriteLine("Removals of a version of a record: {0}", dbInfo.BackoutCount);
			Console.WriteLine("Number of database deletes: {0}", dbInfo.DeleteCount);
			Console.WriteLine("Number of removals of a record and all of its ancestors: {0}", dbInfo.ExpungeCount);
			Console.WriteLine("Number of inserts: {0}", dbInfo.InsertCount);
			Console.WriteLine("Number of removals of old versions of fully mature records: {0}", dbInfo.PurgeCount);
			Console.WriteLine("Number of reads done via an index: {0}", dbInfo.ReadIdxCount);
			Console.WriteLine("Number of sequential sequential table scans: {0}", dbInfo.ReadSeqCount);
			Console.WriteLine("Number of database updates: {0}", dbInfo.UpdateCount);
			Console.WriteLine("Database size in pages: {0}", dbInfo.DatabaseSizeInPages);
			Console.WriteLine("Number of the oldest transaction: {0}", dbInfo.OldestTransaction);
			Console.WriteLine("Number of the oldest active transaction: {0}", dbInfo.OldestActiveTransaction);
			Console.WriteLine("Number of the oldest active snapshot: {0}", dbInfo.OldestActiveSnapshot);
			Console.WriteLine("Number of the next transaction: {0}", dbInfo.NextTransaction);
			Console.WriteLine("Number of active transactions: {0}", dbInfo.ActiveTransactions);
		}		
	}
}
