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

using System;
using System.Configuration;
using System.IO;
using System.Data;
using System.Text;

using FirebirdSql.Data.Firebird;
using FirebirdSql.Data.Firebird.Services;

using NUnit.Framework;

namespace FirebirdSql.Data.Firebird.Tests
{
	[TestFixture]
	public class FbServicesTest : BaseTest 
	{	
		public FbServicesTest() : base(false)
		{
		}

		[SetUp]
		public override void SetUp()
		{
			base.SetUp();

			if (this.Connection != null && this.Connection.State == ConnectionState.Open)
			{
				this.Connection.Close();
			}
		}

		[Test]
		public void BackupTest()
		{
			FbBackup backupSvc = new FbBackup();
						
			backupSvc.ConnectionString = this.BuildServicesConnectionString();
			backupSvc.BackupFiles.Add(new FbBackupFile(@"c:\testdb.gbk", 2048));
			backupSvc.Verbose = true;
			
			backupSvc.Options = FbBackupFlags.IgnoreLimbo;

			backupSvc.ServiceOutput += new ServiceOutputEventHandler(ServiceOutput);
			
			backupSvc.Execute();
		}
		
		[Test]
		public void RestoreTest()
		{
			FbRestore restoreSvc = new FbRestore();

			restoreSvc.ConnectionString = this.BuildServicesConnectionString();
			restoreSvc.BackupFiles.Add(new FbBackupFile(@"c:\testdb.gbk", 2048));
			restoreSvc.Verbose	= true;
			restoreSvc.PageSize = 4096;
			restoreSvc.Options	= FbRestoreFlags.Create | FbRestoreFlags.Replace; 

			restoreSvc.ServiceOutput += new ServiceOutputEventHandler(ServiceOutput);

			restoreSvc.Execute();
		}

		[Test]
		public void ValidationTest()
		{
			FbValidation validationSvc = new FbValidation();

			validationSvc.ConnectionString = this.BuildServicesConnectionString();
			validationSvc.Options = FbValidationFlags.ValidateDatabase; 

			validationSvc.ServiceOutput += new ServiceOutputEventHandler(ServiceOutput);

			validationSvc.Execute();
		}

		[Test]
		public void SweepTest()
		{
			FbValidation validationSvc = new FbValidation();

			validationSvc.ConnectionString = this.BuildServicesConnectionString();
			validationSvc.Options = FbValidationFlags.SweepDatabase;

			validationSvc.ServiceOutput += new ServiceOutputEventHandler(ServiceOutput);

			validationSvc.Execute();
		}

		[Test]
		public void SetPropertiesTest()
		{
			FbConfiguration configurationSvc = new FbConfiguration();

			configurationSvc.ConnectionString = this.BuildServicesConnectionString();

			configurationSvc.SetSweepInterval(1000);
			configurationSvc.SetReserveSpace(true);
			configurationSvc.SetForcedWrites(true);
			configurationSvc.DatabaseShutdown(FbShutdownMode.Forced, 10);
			configurationSvc.DatabaseOnline();
		}

		[Test]
		public void StatisticsTest()
		{
			FbStatistical statisticalSvc = new FbStatistical();

			statisticalSvc.ConnectionString = this.BuildServicesConnectionString();
			statisticalSvc.Options			= FbStatisticalFlags.SystemTablesRelations;
						
			statisticalSvc.ServiceOutput += new ServiceOutputEventHandler(ServiceOutput);

			statisticalSvc.Execute();
		}
		
		[Test]
		public void FbLogTest()
		{
			FbLog logSvc = new FbLog();

			logSvc.ConnectionString = this.BuildServicesConnectionString(false);

			logSvc.ServiceOutput += new ServiceOutputEventHandler(ServiceOutput);
						
			logSvc.Execute();
		}

		[Test]
		public void AddUserTest()
		{
			FbSecurity securitySvc = new FbSecurity();

			securitySvc.ConnectionString = this.BuildServicesConnectionString(false);

			FbUserData user = new FbUserData();
			
			user.UserName 		= "new_user";
			user.UserPassword 	= "1";
			
			securitySvc.AddUser(user);
		}
		
		[Test]
		public void DeleteUser()
		{
			FbSecurity securitySvc = new FbSecurity();

			securitySvc.ConnectionString = this.BuildServicesConnectionString(false);

			FbUserData user = new FbUserData();
			
			user.UserName = "new_user";
						
			securitySvc.DeleteUser(user);
		}

		[Test]
		public void DisplayUser()
		{
			FbSecurity securitySvc = new FbSecurity();

			securitySvc.ConnectionString = this.BuildServicesConnectionString(false);

			FbUserData user = securitySvc.DisplayUser("SYSDBA");

			Console.WriteLine("User name {0}", user.UserName);
		}

		[Test]
		public void DisplayUsers()
		{
			FbSecurity securitySvc = new FbSecurity();

			securitySvc.ConnectionString = this.BuildServicesConnectionString(false);

			FbUserData[] users = securitySvc.DisplayUsers();

			Console.WriteLine("User List");

			for (int i = 0; i < users.Length; i++)
			{
				Console.WriteLine("User {0} name {1}", i, users[i].UserName);
			}
		}
		
		[Test]
		public void ServerPropertiesTest()
		{
			FbServerProperties serverProp = new FbServerProperties();

			serverProp.ConnectionString = this.BuildServicesConnectionString(false);

			FbServerConfig	serverConfig	= serverProp.ServerConfig;
			FbDatabasesInfo databasesInfo	= serverProp.DatabasesInfo;
			
			Console.WriteLine(serverProp.MessageFile);
			Console.WriteLine(serverProp.LockManager);
			Console.WriteLine(serverProp.RootDirectory);
			Console.WriteLine(serverProp.Implementation);
			Console.WriteLine(serverProp.ServerVersion);
			Console.WriteLine(serverProp.Version);
		}

		void ServiceOutput(object sender, ServiceOutputEventArgs e)
		{
			Console.WriteLine(e.Message);
		}
	}
}