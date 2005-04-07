//
// DirectoryServicesDirectoryEntryTest.cs -
//	NUnit Test Cases for DirectoryServices.DirectoryEntry
//
// Author:
//	Boris Kirzner  <borisk@mainsoft.com>
//

using NUnit.Framework;
using System;
using System.DirectoryServices;

namespace MonoTests.System.DirectoryServices 
{
	[TestFixture]
	[Category ("InetAccess")]
	public class DirectoryServicesDirectoryEntryTest
	{
		#region Fields

		static string LDAPServerRoot;
		static string LDAPServerConnectionString;
		static string LDAPServerUsername;
		static string LDAPServerPassword;
		static DirectoryEntry de;

		#endregion // Fields

		#region SetUp and TearDown

		[TestFixtureSetUp]
		public void TestFixtureSetUp()
		{
			de = null;
			string ldapServerName = Environment.GetEnvironmentVariable("MONO_LDAP_TEST_SERVER");
			Assert.IsFalse((ldapServerName == null || ldapServerName == String.Empty),"This test fixture requires environment variable MONO_LDAP_TEST_SERVER to be set up to LDAP server name.");
			LDAPServerRoot = "LDAP://" + ldapServerName + "/";
			LDAPServerConnectionString = LDAPServerRoot + "dc=myhosting,dc=example";
			LDAPServerUsername = "cn=Manager,dc=myhosting,dc=example";
			LDAPServerPassword = "secret";
		}


		[TestFixtureTearDown]
		public void TestFixtureTearDown()
		{
			de = null;
		}


		[SetUp]
		public void SetUp()
		{
			TearDown();

			#region Initialize basics

			DirectoryEntry root = new DirectoryEntry(	LDAPServerConnectionString,
														LDAPServerUsername,
														LDAPServerPassword,
														AuthenticationTypes.ServerBind);
			DirectoryEntry ouPeople = root.Children.Add("ou=people","Class");
			ouPeople.Properties["objectClass"].Value = "organizationalUnit";
			ouPeople.Properties["description"].Value = "All people in organisation";
			ouPeople.Properties["ou"].Value = "people";
			ouPeople.CommitChanges();

			#endregion // Initialize basics

			#region Human Resources
 
			DirectoryEntry ouHumanResources = ouPeople.Children.Add("ou=Human Resources","Class");
			ouHumanResources.Properties["objectClass"].Value = "organizationalUnit";
			ouHumanResources.Properties["ou"].Value = "Human Resources";
			ouHumanResources.CommitChanges();

			DirectoryEntry cnJohnSmith = ouHumanResources.Children.Add("cn=John Smith","Class");
			cnJohnSmith.Properties["objectClass"].Value = "organizationalRole";
			cnJohnSmith.Properties["cn"].Value = "John Smith";
			cnJohnSmith.Properties["description"].Value = "Very clever person";
			cnJohnSmith.Properties["ou"].Value = "Human Resources";
			cnJohnSmith.Properties["telephoneNumber"].Value = "1 801 555 1212";
			cnJohnSmith.CommitChanges();

			DirectoryEntry cnBarakTsabari = ouHumanResources.Children.Add("cn=Barak Tsabari","Class");
			((PropertyValueCollection)cnBarakTsabari.Properties["objectClass"]).Add("person");
			((PropertyValueCollection)cnBarakTsabari.Properties["objectClass"]).Add("organizationalPerson");
			cnBarakTsabari.Properties["cn"].Value = "Barak Tsabari";
			cnBarakTsabari.Properties["facsimileTelephoneNumber"].Value = "+1 906 777 8853";
			((PropertyValueCollection)cnBarakTsabari.Properties["ou"]).Add("Human Resources");
			((PropertyValueCollection)cnBarakTsabari.Properties["ou"]).Add("People");
			cnBarakTsabari.Properties["sn"].Value = "Tsabari";
			cnBarakTsabari.Properties["telephoneNumber"].Value = "+1 906 777 8854";
			cnBarakTsabari.CommitChanges();

			#endregion // Human Resources

			#region R&D

			DirectoryEntry ouRnD = ouPeople.Children.Add("ou=R&D","Class");
			ouRnD.Properties["objectClass"].Value = "organizationalUnit";
			ouRnD.Properties["ou"].Value = "R&D";
			ouRnD.CommitChanges();

			DirectoryEntry cnYossiCohen = ouRnD.Children.Add("cn=Yossi Cohen","Class");
			((PropertyValueCollection)cnYossiCohen.Properties["objectClass"]).Add("person");
			((PropertyValueCollection)cnYossiCohen.Properties["objectClass"]).Add("organizationalPerson");
			cnYossiCohen.Properties["cn"].Value = "Yossi Cohen";
			cnYossiCohen.Properties["facsimileTelephoneNumber"].Value = "+1 503 777 4498";
			((PropertyValueCollection)cnYossiCohen.Properties["ou"]).Add("R&D");
			((PropertyValueCollection)cnYossiCohen.Properties["ou"]).Add("People");
			cnYossiCohen.Properties["sn"].Value = "Cohen";
			cnYossiCohen.Properties["telephoneNumber"].Value = "+1 503 777 4499";
			cnYossiCohen.CommitChanges();

			DirectoryEntry cnUziCohen = ouRnD.Children.Add("cn=Uzi Cohen","Class");
			((PropertyValueCollection)cnUziCohen.Properties["objectClass"]).Add("person");
			((PropertyValueCollection)cnUziCohen.Properties["objectClass"]).Add("organizationalPerson");
			cnUziCohen.Properties["cn"].Value = "Uzi Cohen";
			cnUziCohen.Properties["facsimileTelephoneNumber"].Value = "+1 602 333 1234";
			((PropertyValueCollection)cnUziCohen.Properties["ou"]).Add("R&D");
			((PropertyValueCollection)cnUziCohen.Properties["ou"]).Add("People");
			cnUziCohen.Properties["sn"].Value = "Cohen";
			cnUziCohen.Properties["telephoneNumber"].Value = "+1 602 333 1233";
			cnUziCohen.CommitChanges();

			DirectoryEntry cnDanielCohen = ouRnD.Children.Add("cn=Daniel Cohen","Class");
			((PropertyValueCollection)cnDanielCohen.Properties["objectClass"]).Add("person");
			((PropertyValueCollection)cnDanielCohen.Properties["objectClass"]).Add("organizationalPerson");
			cnDanielCohen.Properties["cn"].Value = "Daniel Cohen";
			cnDanielCohen.Properties["facsimileTelephoneNumber"].Value = "+1 602 333 1235";
			((PropertyValueCollection)cnDanielCohen.Properties["ou"]).Add("R&D");
			((PropertyValueCollection)cnDanielCohen.Properties["ou"]).Add("People");
			cnDanielCohen.Properties["sn"].Value = "Cohen";
			cnDanielCohen.Properties["telephoneNumber"].Value = "+1 602 333 1236";
			cnDanielCohen.CommitChanges();

			DirectoryEntry cnSaraCohen = ouRnD.Children.Add("cn=Sara Cohen","Class");
			((PropertyValueCollection)cnSaraCohen.Properties["objectClass"]).Add("person");
			((PropertyValueCollection)cnSaraCohen.Properties["objectClass"]).Add("organizationalPerson");
			cnSaraCohen.Properties["cn"].Value = "Sara Cohen";
			cnSaraCohen.Properties["facsimileTelephoneNumber"].Value = "+1 602 333 1244";
			((PropertyValueCollection)cnSaraCohen.Properties["ou"]).Add("R&D");
			((PropertyValueCollection)cnSaraCohen.Properties["ou"]).Add("People");
			cnSaraCohen.Properties["sn"].Value = "Cohen";
			cnSaraCohen.Properties["telephoneNumber"].Value = "+1 602 333 1243";
			cnSaraCohen.CommitChanges();

			#endregion // R&D

			#region DevQA

			DirectoryEntry ouDevQA = ouPeople.Children.Add("ou=DevQA","Class");
			ouDevQA.Properties["objectClass"].Value = "organizationalUnit";
			ouDevQA.Properties["ou"].Value = "DevQA";
			ouDevQA.CommitChanges();

			DirectoryEntry cnDanielSmith = ouDevQA.Children.Add("cn=Daniel Smith","Class");
			((PropertyValueCollection)cnDanielSmith.Properties["objectClass"]).Add("person");
			((PropertyValueCollection)cnDanielSmith.Properties["objectClass"]).Add("organizationalPerson");
			cnDanielSmith.Properties["cn"].Value = "Daniel Smith";
			cnDanielSmith.Properties["facsimileTelephoneNumber"].Value = "+1 408 555 3372";
			cnDanielSmith.Properties["l"].Value = "Santa Clara";
			((PropertyValueCollection)cnDanielSmith.Properties["ou"]).Add("DevQA");
			((PropertyValueCollection)cnDanielSmith.Properties["ou"]).Add("People");
			cnDanielSmith.Properties["sn"].Value = "Smith";
			cnDanielSmith.Properties["telephoneNumber"].Value = "+1 408 555 9519";
			cnDanielSmith.CommitChanges();

			DirectoryEntry cnDanielMorgan = ouDevQA.Children.Add("cn=Daniel Morgan","Class");
			((PropertyValueCollection)cnDanielMorgan.Properties["objectClass"]).Add("person");
			((PropertyValueCollection)cnDanielMorgan.Properties["objectClass"]).Add("organizationalPerson");
			cnDanielMorgan.Properties["cn"].Value = "Daniel Morgan";
			cnDanielMorgan.Properties["facsimileTelephoneNumber"].Value = "+1 805 666 5645";
			((PropertyValueCollection)cnDanielMorgan.Properties["ou"]).Add("DevQA");
			((PropertyValueCollection)cnDanielMorgan.Properties["ou"]).Add("People");
			cnDanielMorgan.Properties["sn"].Value = "Morgan";
			cnDanielMorgan.Properties["telephoneNumber"].Value = "+1 805 666 5644";
			cnDanielMorgan.CommitChanges();

			#endregion // DevQA

			#region Manager

			DirectoryEntry cnManager = root.Children.Add("cn=Manager","Class");
			cnManager.Properties["objectClass"].Value = "organizationalRole";
			cnManager.Properties["cn"].Value = "Manager";
			cnManager.CommitChanges();

			DirectoryEntry cnUziCohen_ = cnManager.Children.Add("cn=Uzi Cohen","Class");
			((PropertyValueCollection)cnUziCohen_.Properties["objectClass"]).Add("person");
			((PropertyValueCollection)cnUziCohen_.Properties["objectClass"]).Add("organizationalPerson");
			cnUziCohen_.Properties["cn"].Value = "Uzi Cohen";
			cnUziCohen_.Properties["facsimileTelephoneNumber"].Value = "+1 602 333 1234";
			((PropertyValueCollection)cnUziCohen_.Properties["ou"]).Add("R&D");
			((PropertyValueCollection)cnUziCohen_.Properties["ou"]).Add("People");
			cnUziCohen_.Properties["sn"].Value = "Cohen";
			cnUziCohen_.Properties["telephoneNumber"].Value = "+1 602 333 1233";
			cnUziCohen_.CommitChanges();

			#endregion // Manager
						
			cnJohnSmith.Dispose();
			cnBarakTsabari.Dispose();
			ouHumanResources.Dispose();
			cnUziCohen.Dispose();
			cnYossiCohen.Dispose();
			cnDanielCohen.Dispose();
			cnSaraCohen.Dispose();
			ouRnD.Dispose();
			cnDanielSmith.Dispose();
			cnDanielMorgan.Dispose();
			ouDevQA.Dispose();
			cnUziCohen_.Dispose();
			cnManager.Dispose();
			ouPeople.Dispose();
			root.Dispose();
		}


		[TearDown]
		public void TearDown()
		{
			de = null;

			DirectoryEntry root = new DirectoryEntry(	LDAPServerConnectionString,
														LDAPServerUsername,
														LDAPServerPassword,
														AuthenticationTypes.ServerBind);
			
			foreach(DirectoryEntry child in root.Children) {
				DeleteTree_DFS(child);
			}	
		}

		private void DeleteTree_DFS(DirectoryEntry de)
		{
			foreach(DirectoryEntry child in de.Children) {
				DeleteTree_DFS(child);
			}
			de.DeleteTree();
			de.CommitChanges();
		}

		#endregion //SetUp and TearDown

		#region Tests

		[Test]
		public void DirectoryEntry_DirectoryEntry()
		{
			de = new DirectoryEntry();

			Assert.AreEqual(de.AuthenticationType,AuthenticationTypes.None);
			Assert.AreEqual(de.Password,null);
			Assert.AreEqual(de.Path,String.Empty);
			Assert.AreEqual(de.SchemaClassName,"domainDNS");
			Assert.AreEqual(de.UsePropertyCache,true);
			Assert.AreEqual(de.Username,null);		
		}


		[Test]
		public void DirectoryEntry_DirectoryEntry_Str()
		{
			DirectoryEntry de = new DirectoryEntry(LDAPServerConnectionString);
			
			Assert.AreEqual(de.AuthenticationType,AuthenticationTypes.None);
			Assert.AreEqual(de.Name,"dc=myhosting");
			Assert.AreEqual(de.Password,null);
			Assert.AreEqual(de.Path,LDAPServerConnectionString);
			Assert.AreEqual(de.SchemaClassName,"organization");
			Assert.AreEqual(de.UsePropertyCache,true);
			Assert.AreEqual(de.Username,null);
		}


		[Test]
		public void DirectoryEntry_DirectoryEntry_StrStrStrAuth()
		{

			#region AuthenticationTypes.Anonymous

			DirectoryEntry de = new DirectoryEntry(	LDAPServerConnectionString,
													LDAPServerUsername,
													LDAPServerPassword,
													AuthenticationTypes.Anonymous);
			
			Assert.AreEqual(de.AuthenticationType,AuthenticationTypes.Anonymous);
			//Assert.AreEqual(de.Guid,new Guid("0b045012-1d97-4f94-9d47-87cbf6dada46"));
			Assert.AreEqual(de.Name,"dc=myhosting");
			//Assert.AreEqual(de.NativeGuid,null);
			Assert.AreEqual(de.Password,LDAPServerPassword);
			Assert.AreEqual(de.Path,LDAPServerConnectionString);
			Assert.AreEqual(de.SchemaClassName,"organization");
			Assert.AreEqual(de.UsePropertyCache,true);
			Assert.AreEqual(de.Username,LDAPServerUsername);

			#endregion //AuthenticationTypes.Anonymous

			#region AuthenticationTypes.Delegation

			de = new DirectoryEntry(LDAPServerConnectionString,
									LDAPServerUsername,
									LDAPServerPassword,
									AuthenticationTypes.Delegation);
			
			Assert.AreEqual(de.AuthenticationType,AuthenticationTypes.Delegation);
			//Assert.AreEqual(de.Guid,new Guid("0b045012-1d97-4f94-9d47-87cbf6dada46"));
			Assert.AreEqual(de.Name,"dc=myhosting");
			//Assert.AreEqual(de.NativeGuid,null);
			Assert.AreEqual(de.Password,LDAPServerPassword);
			Assert.AreEqual(de.Path,LDAPServerConnectionString);
			Assert.AreEqual(de.SchemaClassName,"organization");
			Assert.AreEqual(de.UsePropertyCache,true);
			Assert.AreEqual(de.Username,LDAPServerUsername);

			#endregion //AuthenticationTypes.Delegation

			#region AuthenticationTypes.Encryption

			//			de = new DirectoryEntry(	LDAPServerConnectionString,
			//													LDAPServerUsername,
			//													LDAPServerPassword,
			//													AuthenticationTypes.Encryption);
			//			
			//			Assert.AreEqual(de.AuthenticationType,AuthenticationTypes.Encryption);
			//			//Assert.AreEqual(de.Guid,new Guid("0b045012-1d97-4f94-9d47-87cbf6dada46"));
			//			Assert.AreEqual(de.Name,"dc=myhosting");
			//			//Assert.AreEqual(de.NativeGuid,null);
			//			Assert.AreEqual(de.Password,LDAPServerPassword);
			//			Assert.AreEqual(de.Path,LDAPServerConnectionString);
			//			Assert.AreEqual(de.SchemaClassName,"organization");
			//			Assert.AreEqual(de.UsePropertyCache,true);
			//			Assert.AreEqual(de.Username,LDAPServerUsername);

			#endregion //AuthenticationTypes.Encryption

			#region AuthenticationTypes.FastBind

			de = new DirectoryEntry(LDAPServerConnectionString,
									LDAPServerUsername,
									LDAPServerPassword,
									AuthenticationTypes.FastBind);
			
			Assert.AreEqual(de.AuthenticationType,AuthenticationTypes.FastBind);
			//Assert.AreEqual(de.Guid,new Guid("0b045012-1d97-4f94-9d47-87cbf6dada46"));
			Assert.AreEqual(de.Name,"dc=myhosting");
			//Assert.AreEqual(de.NativeGuid,null);
			Assert.AreEqual(de.Password,LDAPServerPassword);
			Assert.AreEqual(de.Path,LDAPServerConnectionString);
			Assert.AreEqual(de.SchemaClassName,"organization");
			Assert.AreEqual(de.UsePropertyCache,true);
			Assert.AreEqual(de.Username,LDAPServerUsername);

			#endregion //AuthenticationTypes.FastBind

			#region AuthenticationTypes.None

			de = new DirectoryEntry(LDAPServerConnectionString,
									LDAPServerUsername,
									LDAPServerPassword,
									AuthenticationTypes.None);
			
			Assert.AreEqual(de.AuthenticationType,AuthenticationTypes.None);
			//Assert.AreEqual(de.Guid,new Guid("0b045012-1d97-4f94-9d47-87cbf6dada46"));
			Assert.AreEqual(de.Name,"dc=myhosting");
			//Assert.AreEqual(de.NativeGuid,null);
			Assert.AreEqual(de.Password,LDAPServerPassword);
			Assert.AreEqual(de.Path,LDAPServerConnectionString);
			Assert.AreEqual(de.SchemaClassName,"organization");
			Assert.AreEqual(de.UsePropertyCache,true);
			Assert.AreEqual(de.Username,LDAPServerUsername);

			#endregion //AuthenticationTypes.None

			#region AuthenticationTypes.ReadonlyServer

			de = new DirectoryEntry(LDAPServerConnectionString,
									LDAPServerUsername,
									LDAPServerPassword,
									AuthenticationTypes.ReadonlyServer);
			
			Assert.AreEqual(de.AuthenticationType,AuthenticationTypes.ReadonlyServer);
			//Assert.AreEqual(de.Guid,new Guid("0b045012-1d97-4f94-9d47-87cbf6dada46"));
			Assert.AreEqual(de.Name,"dc=myhosting");
			//Assert.AreEqual(de.NativeGuid,null);
			Assert.AreEqual(de.Password,LDAPServerPassword);
			Assert.AreEqual(de.Path,LDAPServerConnectionString);
			Assert.AreEqual(de.SchemaClassName,"organization");
			Assert.AreEqual(de.UsePropertyCache,true);
			Assert.AreEqual(de.Username,LDAPServerUsername);

			#endregion //AuthenticationTypes.ReadonlyServer

			#region AuthenticationTypes.Sealing

			de = new DirectoryEntry(LDAPServerConnectionString,
									LDAPServerUsername,
									LDAPServerPassword,
									AuthenticationTypes.Sealing);
			
			Assert.AreEqual(de.AuthenticationType,AuthenticationTypes.Sealing);
			//Assert.AreEqual(de.Guid,new Guid("0b045012-1d97-4f94-9d47-87cbf6dada46"));
			Assert.AreEqual(de.Name,"dc=myhosting");
			//Assert.AreEqual(de.NativeGuid,null);
			Assert.AreEqual(de.Password,LDAPServerPassword);
			Assert.AreEqual(de.Path,LDAPServerConnectionString);
			Assert.AreEqual(de.SchemaClassName,"organization");
			Assert.AreEqual(de.UsePropertyCache,true);
			Assert.AreEqual(de.Username,LDAPServerUsername);

			#endregion //AuthenticationTypes.Sealing

			#region AuthenticationTypes.Secure

			//			de = new DirectoryEntry(LDAPServerConnectionString,
			//									LDAPServerUsername,
			//									LDAPServerPassword,
			//									AuthenticationTypes.Secure);
			//			
			//			Assert.AreEqual(de.AuthenticationType,AuthenticationTypes.Secure);
			//			//Assert.AreEqual(de.Guid,new Guid("0b045012-1d97-4f94-9d47-87cbf6dada46"));
			//			Assert.AreEqual(de.Name,"dc=myhosting");
			//			//Assert.AreEqual(de.NativeGuid,null);
			//			Assert.AreEqual(de.Password,LDAPServerPassword);
			//			Assert.AreEqual(de.Path,LDAPServerConnectionString);
			//			Assert.AreEqual(de.SchemaClassName,"organization");
			//			Assert.AreEqual(de.UsePropertyCache,true);
			//			Assert.AreEqual(de.Username,LDAPServerUsername);

			#endregion //AuthenticationTypes.Secure

			#region AuthenticationTypes.SecureSocketsLayer

			//			de = new DirectoryEntry(LDAPServerConnectionString,
			//									LDAPServerUsername,
			//									LDAPServerPassword,
			//									AuthenticationTypes.SecureSocketsLayer);
			//			
			//			Assert.AreEqual(de.AuthenticationType,AuthenticationTypes.SecureSocketsLayer);
			//			//Assert.AreEqual(de.Guid,new Guid("0b045012-1d97-4f94-9d47-87cbf6dada46"));
			//			Assert.AreEqual(de.Name,"dc=myhosting");
			//			//Assert.AreEqual(de.NativeGuid,null);
			//			Assert.AreEqual(de.Password,LDAPServerPassword);
			//			Assert.AreEqual(de.Path,LDAPServerConnectionString);
			//			Assert.AreEqual(de.SchemaClassName,"organization");
			//			Assert.AreEqual(de.UsePropertyCache,true);
			//			Assert.AreEqual(de.Username,LDAPServerUsername);

			#endregion //AuthenticationTypes.SecureSocketsLayer

			#region AuthenticationTypes.ServerBind

			de = new DirectoryEntry(LDAPServerConnectionString,
									LDAPServerUsername,
									LDAPServerPassword,
									AuthenticationTypes.ServerBind);
			
			Assert.AreEqual(de.AuthenticationType,AuthenticationTypes.ServerBind);
			//Assert.AreEqual(de.Guid,new Guid("0b045012-1d97-4f94-9d47-87cbf6dada46"));
			Assert.AreEqual(de.Name,"dc=myhosting");
			//Assert.AreEqual(de.NativeGuid,null);
			Assert.AreEqual(de.Password,LDAPServerPassword);
			Assert.AreEqual(de.Path,LDAPServerConnectionString);
			Assert.AreEqual(de.SchemaClassName,"organization");
			Assert.AreEqual(de.UsePropertyCache,true);
			Assert.AreEqual(de.Username,LDAPServerUsername);

			#endregion //AuthenticationTypes.ServerBind

			#region AuthenticationTypes.Signing

			de = new DirectoryEntry(LDAPServerConnectionString,
									LDAPServerUsername,
									LDAPServerPassword,
									AuthenticationTypes.Signing);
			
			Assert.AreEqual(de.AuthenticationType,AuthenticationTypes.Signing);
			//Assert.AreEqual(de.Guid,new Guid("0b045012-1d97-4f94-9d47-87cbf6dada46"));
			Assert.AreEqual(de.Name,"dc=myhosting");
			//Assert.AreEqual(de.NativeGuid,null);
			Assert.AreEqual(de.Password,LDAPServerPassword);
			Assert.AreEqual(de.Path,LDAPServerConnectionString);
			Assert.AreEqual(de.SchemaClassName,"organization");
			Assert.AreEqual(de.UsePropertyCache,true);
			Assert.AreEqual(de.Username,LDAPServerUsername);

			#endregion //AuthenticationTypes.Signing

		}

		[Test]
		public void DirectoryEntry_Dispose()
		{
			DirectoryEntry root = new DirectoryEntry(	LDAPServerConnectionString,
														LDAPServerUsername,
														LDAPServerPassword,
														AuthenticationTypes.ServerBind);

			DirectoryEntry ouPeople = root.Children.Add("ou=printers","Class");
			ouPeople.Properties["objectClass"].Value = "organizationalUnit";
			ouPeople.Properties["description"].Value = "All printers in organisation";
			ouPeople.Properties["ou"].Value = "printers";
			ouPeople.CommitChanges();

			//root.Dispose();

			ouPeople.Rename("ou=anotherPrinters");
			ouPeople.CommitChanges();

			Assert.IsTrue(DirectoryEntry.Exists(LDAPServerRoot + "ou=anotherPrinters,dc=myhosting,dc=example"));
		}


		[Test]
		public void DirectoryEntry_AuthenticationType()
		{
			de = new DirectoryEntry();

			de.AuthenticationType = AuthenticationTypes.Anonymous;
			Assert.AreEqual(de.AuthenticationType,AuthenticationTypes.Anonymous);

			de.AuthenticationType = AuthenticationTypes.Delegation;
			Assert.AreEqual(de.AuthenticationType,AuthenticationTypes.Delegation);

			de.AuthenticationType = AuthenticationTypes.Encryption;
			Assert.AreEqual(de.AuthenticationType,AuthenticationTypes.Encryption);

			de.AuthenticationType = AuthenticationTypes.FastBind;
			Assert.AreEqual(de.AuthenticationType,AuthenticationTypes.FastBind);

			de.AuthenticationType = AuthenticationTypes.None;
			Assert.AreEqual(de.AuthenticationType,AuthenticationTypes.None);

			de.AuthenticationType = AuthenticationTypes.ReadonlyServer;
			Assert.AreEqual(de.AuthenticationType,AuthenticationTypes.ReadonlyServer);

			de.AuthenticationType = AuthenticationTypes.Sealing;
			Assert.AreEqual(de.AuthenticationType,AuthenticationTypes.Sealing);

			de.AuthenticationType = AuthenticationTypes.Secure;
			Assert.AreEqual(de.AuthenticationType,AuthenticationTypes.Secure);

			de.AuthenticationType = AuthenticationTypes.SecureSocketsLayer;
			Assert.AreEqual(de.AuthenticationType,AuthenticationTypes.SecureSocketsLayer);

			de.AuthenticationType = AuthenticationTypes.ServerBind;
			Assert.AreEqual(de.AuthenticationType,AuthenticationTypes.ServerBind);

			de.AuthenticationType = AuthenticationTypes.Signing;
			Assert.AreEqual(de.AuthenticationType,AuthenticationTypes.Signing);


			de = new DirectoryEntry(LDAPServerConnectionString);

			de.AuthenticationType = AuthenticationTypes.Anonymous;
			Assert.AreEqual(de.AuthenticationType,AuthenticationTypes.Anonymous);

			de.AuthenticationType = AuthenticationTypes.Delegation;
			Assert.AreEqual(de.AuthenticationType,AuthenticationTypes.Delegation);

			de.AuthenticationType = AuthenticationTypes.Encryption;
			Assert.AreEqual(de.AuthenticationType,AuthenticationTypes.Encryption);

			de.AuthenticationType = AuthenticationTypes.FastBind;
			Assert.AreEqual(de.AuthenticationType,AuthenticationTypes.FastBind);

			de.AuthenticationType = AuthenticationTypes.None;
			Assert.AreEqual(de.AuthenticationType,AuthenticationTypes.None);

			de.AuthenticationType = AuthenticationTypes.ReadonlyServer;
			Assert.AreEqual(de.AuthenticationType,AuthenticationTypes.ReadonlyServer);

			de.AuthenticationType = AuthenticationTypes.Sealing;
			Assert.AreEqual(de.AuthenticationType,AuthenticationTypes.Sealing);

			de.AuthenticationType = AuthenticationTypes.Secure;
			Assert.AreEqual(de.AuthenticationType,AuthenticationTypes.Secure);

			de.AuthenticationType = AuthenticationTypes.SecureSocketsLayer;
			Assert.AreEqual(de.AuthenticationType,AuthenticationTypes.SecureSocketsLayer);

			de.AuthenticationType = AuthenticationTypes.ServerBind;
			Assert.AreEqual(de.AuthenticationType,AuthenticationTypes.ServerBind);

			de.AuthenticationType = AuthenticationTypes.Signing;
			Assert.AreEqual(de.AuthenticationType,AuthenticationTypes.Signing);

			de = new DirectoryEntry(LDAPServerConnectionString,
									LDAPServerUsername,
									LDAPServerPassword,
									AuthenticationTypes.None);

			de.AuthenticationType = AuthenticationTypes.Anonymous;
			Assert.AreEqual(de.AuthenticationType,AuthenticationTypes.Anonymous);

			de.AuthenticationType = AuthenticationTypes.Delegation;
			Assert.AreEqual(de.AuthenticationType,AuthenticationTypes.Delegation);

			de.AuthenticationType = AuthenticationTypes.Encryption;
			Assert.AreEqual(de.AuthenticationType,AuthenticationTypes.Encryption);

			de.AuthenticationType = AuthenticationTypes.FastBind;
			Assert.AreEqual(de.AuthenticationType,AuthenticationTypes.FastBind);

			de.AuthenticationType = AuthenticationTypes.None;
			Assert.AreEqual(de.AuthenticationType,AuthenticationTypes.None);

			de.AuthenticationType = AuthenticationTypes.ReadonlyServer;
			Assert.AreEqual(de.AuthenticationType,AuthenticationTypes.ReadonlyServer);

			de.AuthenticationType = AuthenticationTypes.Sealing;
			Assert.AreEqual(de.AuthenticationType,AuthenticationTypes.Sealing);

			de.AuthenticationType = AuthenticationTypes.Secure;
			Assert.AreEqual(de.AuthenticationType,AuthenticationTypes.Secure);

			de.AuthenticationType = AuthenticationTypes.SecureSocketsLayer;
			Assert.AreEqual(de.AuthenticationType,AuthenticationTypes.SecureSocketsLayer);

			de.AuthenticationType = AuthenticationTypes.ServerBind;
			Assert.AreEqual(de.AuthenticationType,AuthenticationTypes.ServerBind);

			de.AuthenticationType = AuthenticationTypes.Signing;
			Assert.AreEqual(de.AuthenticationType,AuthenticationTypes.Signing);
		}

		
		[Test]
		public void DirectoryEntry_UsePropertyCache()
		{
			string barakTsabariDN = LDAPServerRoot + "cn=Barak Tsabari,ou=Human Resources,ou=people,dc=myhosting,dc=example";
			de = new DirectoryEntry(barakTsabariDN,
									LDAPServerUsername,
									LDAPServerPassword,
									AuthenticationTypes.ServerBind);

			// UsePropertyCache = true
			de.UsePropertyCache = true;
			Assert.AreEqual(de.UsePropertyCache,true);

			#region Check Properties

			// Properties changes are cached
			string oldTelephoneNumber = (string)de.Properties["telephoneNumber"].Value;
			string newTelephoneNumber = "+972-3-6572345";

			de.Properties["telephoneNumber"].Value = newTelephoneNumber;
			DirectoryEntry barakTsabariDE = new DirectoryEntry(	barakTsabariDN,
																LDAPServerUsername,
																LDAPServerPassword,
																AuthenticationTypes.ServerBind);

			Assert.AreEqual(barakTsabariDE.Properties["telephoneNumber"].Value,oldTelephoneNumber);
			de.CommitChanges();
			barakTsabariDE = new DirectoryEntry(barakTsabariDN,
												LDAPServerUsername,
												LDAPServerPassword,
												AuthenticationTypes.ServerBind);
			Assert.AreEqual(barakTsabariDE.Properties["telephoneNumber"].Value,newTelephoneNumber);

			// restore object state
			de.Properties["telephoneNumber"].Value = oldTelephoneNumber;
			de.CommitChanges();

			#endregion // Check Properties

			#region Check DeleteTree

			// DeleteTree is not cached
			de.DeleteTree();
			try {
				barakTsabariDE = new DirectoryEntry(barakTsabariDN,
													LDAPServerUsername,
													LDAPServerPassword,
													AuthenticationTypes.ServerBind);
				barakTsabariDE.Properties["telephoneNumber"].Value = newTelephoneNumber;
				barakTsabariDE.CommitChanges();
				Assert.Fail("Object " + barakTsabariDN + " was not deleted from server.");
			}
			catch(AssertionException ae) {
				throw ae;
			}
			catch (Exception e) {
				// do nothing
			}

			// restore object state
			DirectoryEntry ouHumanResources = new DirectoryEntry(	LDAPServerRoot + "ou=Human Resources,ou=people,dc=myhosting,dc=example",
																	LDAPServerUsername,
																	LDAPServerPassword,
																	AuthenticationTypes.ServerBind);
			DirectoryEntry cnBarakTsabari = ouHumanResources.Children.Add("cn=Barak Tsabari","Class");
			((PropertyValueCollection)cnBarakTsabari.Properties["objectClass"]).Add("person");
			((PropertyValueCollection)cnBarakTsabari.Properties["objectClass"]).Add("organizationalPerson");
			cnBarakTsabari.Properties["cn"].Value = "Barak Tsabari";
			cnBarakTsabari.Properties["facsimileTelephoneNumber"].Value = "+1 906 777 8853";
			((PropertyValueCollection)cnBarakTsabari.Properties["ou"]).Add("Human Resources");
			((PropertyValueCollection)cnBarakTsabari.Properties["ou"]).Add("People");
			cnBarakTsabari.Properties["sn"].Value = "Tsabari";
			cnBarakTsabari.Properties["telephoneNumber"].Value = "+1 906 777 8854";
			cnBarakTsabari.CommitChanges();

			#endregion // Check DeleteTree

			#region Check MoveTo

			// Move to is not cached
			de = new DirectoryEntry(barakTsabariDN,
									LDAPServerUsername,
									LDAPServerPassword,
									AuthenticationTypes.ServerBind);

			DirectoryEntry ouRnD = new DirectoryEntry(	LDAPServerRoot + "ou=R&D,ou=people,dc=myhosting,dc=example",
														LDAPServerUsername,
														LDAPServerPassword,
														AuthenticationTypes.ServerBind);
			de.MoveTo(ouRnD);
			try {
				barakTsabariDE = new DirectoryEntry(barakTsabariDN,
													LDAPServerUsername,
													LDAPServerPassword,
													AuthenticationTypes.ServerBind);
				barakTsabariDE.Properties["telephoneNumber"].Value = newTelephoneNumber;
				barakTsabariDE.CommitChanges();
				Assert.Fail("Object " + barakTsabariDN + " was not moved from old location on the server.");
			}
			catch(AssertionException ae) {
				throw ae;
			}
			catch (Exception e) {
				// do nothing
			}


			barakTsabariDE = new DirectoryEntry(LDAPServerRoot + "cn=Barak Tsabari,ou=R&D,ou=people,dc=myhosting,dc=example",
												LDAPServerUsername,
												LDAPServerPassword,
												AuthenticationTypes.ServerBind);
			Assert.AreEqual(barakTsabariDE.Properties["telephoneNumber"].Value,oldTelephoneNumber);
			

			// restore object state
			ouHumanResources = new DirectoryEntry(	LDAPServerRoot + "ou=Human Resources,ou=people,dc=myhosting,dc=example",
																	LDAPServerUsername,
																	LDAPServerPassword,
																	AuthenticationTypes.ServerBind);
			barakTsabariDE = new DirectoryEntry(LDAPServerRoot + "cn=Barak Tsabari,ou=R&D,ou=people,dc=myhosting,dc=example",
												LDAPServerUsername,
												LDAPServerPassword,
												AuthenticationTypes.ServerBind);
			barakTsabariDE.MoveTo(ouHumanResources);
			barakTsabariDE.CommitChanges();

			#endregion // Check MoveTo

			#region Check Rename

			// Rename not chached
			de = new DirectoryEntry(barakTsabariDN,
									LDAPServerUsername,
									LDAPServerPassword,
									AuthenticationTypes.ServerBind);

			de.Rename("cn=MyUser");

			try {
				barakTsabariDE = new DirectoryEntry(barakTsabariDN,
													LDAPServerUsername,
													LDAPServerPassword,
													AuthenticationTypes.ServerBind);
				barakTsabariDE.Properties["telephoneNumber"].Value = newTelephoneNumber;
				barakTsabariDE.CommitChanges();
				Assert.Fail("Object " + barakTsabariDN + " was not renamed on the server.");
			}
			catch(AssertionException ae) {
				throw ae;
			}
			catch (Exception e) {
				// do nothing
			}

			barakTsabariDE = new DirectoryEntry(LDAPServerRoot + "cn=MyUser,ou=Human Resources,ou=people,dc=myhosting,dc=example",
												LDAPServerUsername,
												LDAPServerPassword,
												AuthenticationTypes.ServerBind);
			Assert.AreEqual(barakTsabariDE.Properties["telephoneNumber"].Value,oldTelephoneNumber);

			// restore object state
			barakTsabariDE = new DirectoryEntry(LDAPServerRoot + "cn=MyUser,ou=Human Resources,ou=people,dc=myhosting,dc=example",
												LDAPServerUsername,
												LDAPServerPassword,
												AuthenticationTypes.ServerBind);
			barakTsabariDE.Rename("cn=Barak Tsabari");
			barakTsabariDE.CommitChanges();

			#endregion // Check Rename

			// UsePropertyCache = false	
			de = new DirectoryEntry(barakTsabariDN,
									LDAPServerUsername,
									LDAPServerPassword,
									AuthenticationTypes.ServerBind);
			de.UsePropertyCache = false;
			Assert.AreEqual(de.UsePropertyCache,false);

			#region Check Properties

			// Properties changes not cached
			de.Properties["telephoneNumber"].Value = newTelephoneNumber;
			barakTsabariDE = new DirectoryEntry(barakTsabariDN,
												LDAPServerUsername,
												LDAPServerPassword,
												AuthenticationTypes.ServerBind);

			Assert.AreEqual(barakTsabariDE.Properties["telephoneNumber"].Value,newTelephoneNumber);

			#endregion // Check Properties

			#region Check DeleteTree

			// DeleteTree is not cached
			de.DeleteTree();
			try {
				barakTsabariDE = new DirectoryEntry(barakTsabariDN,
													LDAPServerUsername,
													LDAPServerPassword,
													AuthenticationTypes.ServerBind);
				barakTsabariDE.Properties["telephoneNumber"].Value = newTelephoneNumber;
				barakTsabariDE.CommitChanges();
				Assert.Fail("Object " + barakTsabariDN + " was not deleted from server.");
			}
			catch(AssertionException ae) {
				throw ae;
			}
			catch (Exception e) {
				// do nothing
			}

			// restore object state
			ouHumanResources = new DirectoryEntry(	LDAPServerRoot + "ou=Human Resources,ou=people,dc=myhosting,dc=example",
																	LDAPServerUsername,
																	LDAPServerPassword,
																	AuthenticationTypes.ServerBind);
			cnBarakTsabari = ouHumanResources.Children.Add("cn=Barak Tsabari","Class");
			((PropertyValueCollection)cnBarakTsabari.Properties["objectClass"]).Add("person");
			((PropertyValueCollection)cnBarakTsabari.Properties["objectClass"]).Add("organizationalPerson");
			cnBarakTsabari.Properties["cn"].Value = "Barak Tsabari";
			cnBarakTsabari.Properties["facsimileTelephoneNumber"].Value = "+1 906 777 8853";
			((PropertyValueCollection)cnBarakTsabari.Properties["ou"]).Add("Human Resources");
			((PropertyValueCollection)cnBarakTsabari.Properties["ou"]).Add("People");
			cnBarakTsabari.Properties["sn"].Value = "Tsabari";
			cnBarakTsabari.Properties["telephoneNumber"].Value = "+1 906 777 8854";
			cnBarakTsabari.CommitChanges();

			#endregion // Check DeleteTree

			#region Check MoveTo

			// Move to is not cached
			de = new DirectoryEntry(barakTsabariDN,
									LDAPServerUsername,
									LDAPServerPassword,
									AuthenticationTypes.ServerBind);

			ouRnD = new DirectoryEntry(	LDAPServerRoot + "ou=R&D,ou=people,dc=myhosting,dc=example",
										LDAPServerUsername,
										LDAPServerPassword,
										AuthenticationTypes.ServerBind);
			de.MoveTo(ouRnD);
			try {
				barakTsabariDE = new DirectoryEntry(barakTsabariDN,
													LDAPServerUsername,
													LDAPServerPassword,
													AuthenticationTypes.ServerBind);
				barakTsabariDE.Properties["telephoneNumber"].Value = newTelephoneNumber;
				barakTsabariDE.CommitChanges();
				Assert.Fail("Object " + barakTsabariDN + " was not moved from old location on the server.");
			}
			catch(AssertionException ae) {
				throw ae;
			}
			catch (Exception e) {
				// do nothing
			}


			barakTsabariDE = new DirectoryEntry(LDAPServerRoot + "cn=Barak Tsabari,ou=R&D,ou=people,dc=myhosting,dc=example",
												LDAPServerUsername,
												LDAPServerPassword,
												AuthenticationTypes.ServerBind);
			Assert.AreEqual(barakTsabariDE.Properties["telephoneNumber"].Value,oldTelephoneNumber);
			

			// restore object state
			ouHumanResources = new DirectoryEntry(	LDAPServerRoot + "ou=Human Resources,ou=people,dc=myhosting,dc=example",
																	LDAPServerUsername,
																	LDAPServerPassword,
																	AuthenticationTypes.ServerBind);
			barakTsabariDE = new DirectoryEntry(LDAPServerRoot + "cn=Barak Tsabari,ou=R&D,ou=people,dc=myhosting,dc=example",
												LDAPServerUsername,
												LDAPServerPassword,
												AuthenticationTypes.ServerBind);
			barakTsabariDE.MoveTo(ouHumanResources);
			barakTsabariDE.CommitChanges();

			#endregion // Check MoveTo

			#region Check Rename

			// Rename not chached
			de = new DirectoryEntry(barakTsabariDN,
									LDAPServerUsername,
									LDAPServerPassword,
									AuthenticationTypes.ServerBind);

			de.Rename("cn=MyUser");

			try {
				barakTsabariDE = new DirectoryEntry(barakTsabariDN,
													LDAPServerUsername,
													LDAPServerPassword,
													AuthenticationTypes.ServerBind);
				barakTsabariDE.Properties["telephoneNumber"].Value = newTelephoneNumber;
				barakTsabariDE.CommitChanges();
				Assert.Fail("Object " + barakTsabariDN + " was not renamed on the server.");
			}
			catch(AssertionException ae) {
				throw ae;
			}
			catch (Exception e) {
				// do nothing
			}

			barakTsabariDE = new DirectoryEntry(LDAPServerRoot + "cn=MyUser,ou=Human Resources,ou=people,dc=myhosting,dc=example",
												LDAPServerUsername,
												LDAPServerPassword,
												AuthenticationTypes.ServerBind);
			Assert.AreEqual(barakTsabariDE.Properties["telephoneNumber"].Value,oldTelephoneNumber);

			// restore object state
			barakTsabariDE = new DirectoryEntry(LDAPServerRoot + "cn=MyUser,ou=Human Resources,ou=people,dc=myhosting,dc=example",
												LDAPServerUsername,
												LDAPServerPassword,
												AuthenticationTypes.ServerBind);
			barakTsabariDE.Rename("cn=Barak Tsabari");
			barakTsabariDE.CommitChanges();

			#endregion // Check Rename
		}

		[Test]
		public void DirectoryEntry_Children()
		{
			de = new DirectoryEntry();
			DirectoryEntries children = de.Children;
			Assert.AreEqual(children.SchemaFilter.Count,0);	


			de = new DirectoryEntry(LDAPServerConnectionString);
			children = de.Children;

			Assert.AreEqual(children.SchemaFilter.Count,0);

			int childrenCount = 0;
			foreach(DirectoryEntry childDe in children) {
				childrenCount++;
			}
			Assert.AreEqual(childrenCount,2);
			Assert.AreEqual(children.Find("ou=people").Name,"ou=people");
			Assert.AreEqual(children.Find("cn=Manager").Name,"cn=Manager");


			de = new DirectoryEntry(LDAPServerConnectionString,
									LDAPServerUsername,
									LDAPServerPassword,
									AuthenticationTypes.ServerBind);
			children = de.Children;

			Assert.AreEqual(children.SchemaFilter.Count,0);

			childrenCount = 0;
			foreach(DirectoryEntry childDe in children) {
				childrenCount++;
			}
			Assert.AreEqual(childrenCount,2);
			Assert.AreEqual(children.Find("ou=people").Name,"ou=people");
			Assert.AreEqual(children.Find("cn=Manager").Name,"cn=Manager");

			de = new DirectoryEntry(LDAPServerRoot + "ou=Human Resources,ou=people,dc=myhosting,dc=example" ,
									LDAPServerUsername,
									LDAPServerPassword,
									AuthenticationTypes.ServerBind);
			children = de.Children;

			Assert.AreEqual(children.Find("cn=Barak Tsabari").Name,"cn=Barak Tsabari");
			Assert.AreEqual(children.Find("cn=John Smith").Name,"cn=John Smith");
		}

		[Test]
		public void DirectoryEntry_Name()
		{
			de = new DirectoryEntry(LDAPServerConnectionString);
			Assert.AreEqual(de.Name,"dc=myhosting");

			de = new DirectoryEntry(LDAPServerConnectionString,
									LDAPServerUsername,
									LDAPServerPassword,
									AuthenticationTypes.ServerBind);
			Assert.AreEqual(de.Name,"dc=myhosting");

			de = new DirectoryEntry(LDAPServerRoot + "ou=Human Resources,ou=people,dc=myhosting,dc=example",
									LDAPServerUsername,
									LDAPServerPassword,
									AuthenticationTypes.ServerBind);
			Assert.AreEqual(de.Name,"ou=Human Resources");

			de = new DirectoryEntry(LDAPServerRoot + "cn=Barak Tsabari,ou=Human Resources,ou=people,dc=myhosting,dc=example" ,
									LDAPServerUsername,
									LDAPServerPassword,
									AuthenticationTypes.ServerBind);
			Assert.AreEqual(de.Name,"cn=Barak Tsabari");
		}
		

		[Test]
		public void DirectoryEntry_Parent()
		{
			de = new DirectoryEntry(LDAPServerConnectionString);

			Assert.AreEqual(de.Parent.Path,LDAPServerRoot + "dc=example");

			de = new DirectoryEntry(LDAPServerConnectionString,
									LDAPServerUsername,
									LDAPServerPassword,
									AuthenticationTypes.ServerBind);

			Assert.AreEqual(de.Parent.Path,LDAPServerRoot + "dc=example");

			de = new DirectoryEntry(LDAPServerRoot + "cn=Barak Tsabari,ou=Human Resources,ou=people,dc=myhosting,dc=example" ,
									LDAPServerUsername,
									LDAPServerPassword,
									AuthenticationTypes.ServerBind);

			Assert.AreEqual(de.Parent.Path,LDAPServerRoot + "ou=Human Resources,ou=people,dc=myhosting,dc=example");
		}


		[Test]
		public void DirectoryEntry_Password()
		{
			string wrongPassword = "some wrong password";

			de = new DirectoryEntry();

			Assert.AreEqual(de.Password,null);

			de.Password = LDAPServerPassword;
			Assert.AreEqual(de.Password,LDAPServerPassword);

			de.Password = "";
			Assert.AreEqual(de.Password,String.Empty);
			
			de.Password = wrongPassword;
			Assert.AreEqual(de.Password,wrongPassword);


			de = new DirectoryEntry(LDAPServerConnectionString);

			de.Password = LDAPServerPassword;
			Assert.AreEqual(de.Password,LDAPServerPassword);

			de.Password = "";
			Assert.AreEqual(de.Password,String.Empty);

			de.Password = wrongPassword;
			Assert.AreEqual(de.Password,wrongPassword);

			
			de = new DirectoryEntry(LDAPServerConnectionString,
									LDAPServerUsername,
									LDAPServerPassword,
									AuthenticationTypes.ServerBind);

			de.Password = LDAPServerPassword;
			Assert.AreEqual(de.Password,LDAPServerPassword);

			de.Password = "";
			Assert.AreEqual(de.Password,String.Empty);

			de.Password = wrongPassword;
			Assert.AreEqual(de.Password,wrongPassword);
		}


		[Test]
		public void DirectoryEntry_Path()
		{
			string wrongPath = "something that is not LDAP path";

			de = new DirectoryEntry();

			Assert.AreEqual(de.Path,String.Empty);

			de.Path = LDAPServerConnectionString;
			Assert.AreEqual(de.Path,LDAPServerConnectionString);

			de.Path = "";
			Assert.AreEqual(de.Path,String.Empty);
			
			de.Path = wrongPath;
			Assert.AreEqual(de.Path,wrongPath);


			de = new DirectoryEntry(LDAPServerConnectionString);

			de.Path = LDAPServerConnectionString;
			Assert.AreEqual(de.Path,LDAPServerConnectionString);

			de.Path = "";
			Assert.AreEqual(de.Path,String.Empty);

			de.Path = wrongPath;
			Assert.AreEqual(de.Path,wrongPath);

			
			de = new DirectoryEntry(LDAPServerConnectionString,
									LDAPServerUsername,
									LDAPServerPassword,
									AuthenticationTypes.ServerBind);

			de.Path = LDAPServerConnectionString;
			Assert.AreEqual(de.Path,LDAPServerConnectionString);

			de.Path = "";
			Assert.AreEqual(de.Path,String.Empty);

			de.Path = wrongPath;
			Assert.AreEqual(de.Path,wrongPath);
		}


		[Test]
		public void DirectoryEntry_Properties1()
		{
			de = new DirectoryEntry(LDAPServerConnectionString);

			Assert.AreEqual(de.Properties.Count,4);
			Assert.AreEqual(((PropertyValueCollection)de.Properties["dc"]).Value,"myhosting");
			Assert.AreEqual(((PropertyValueCollection)de.Properties["description"]).Value,"My wonderful company as much text as you want to place in this line up to 32Kcontinuation data for the line above must have <CR> or <CR><LF> i.e. ENTER works on both Windows and *nix system - new line MUST begin with ONE SPACE");
			Assert.AreEqual(((PropertyValueCollection)de.Properties["o"]).Value,"Example, Inc.");

			
			de = new DirectoryEntry(LDAPServerConnectionString,
									LDAPServerUsername,
									LDAPServerPassword,
									AuthenticationTypes.ServerBind);

			Assert.AreEqual(de.Properties.Count,4);
			Assert.AreEqual(((PropertyValueCollection)de.Properties["dc"]).Value,"myhosting");
			Assert.AreEqual(((PropertyValueCollection)de.Properties["description"]).Value,"My wonderful company as much text as you want to place in this line up to 32Kcontinuation data for the line above must have <CR> or <CR><LF> i.e. ENTER works on both Windows and *nix system - new line MUST begin with ONE SPACE");
			Assert.AreEqual(((PropertyValueCollection)de.Properties["o"]).Value,"Example, Inc.");

			// ensure that properties are not accessible after removing an entry from the server
			string barakTsabariDN = LDAPServerRoot + "cn=Barak Tsabari,ou=Human Resources,ou=people,dc=myhosting,dc=example";
			de = new DirectoryEntry(barakTsabariDN,
									LDAPServerUsername,
									LDAPServerPassword,
									AuthenticationTypes.ServerBind);
			
			de.DeleteTree();
			
			try {
				int i = de.Properties.Count;
				Assert.Fail("Properties should not be accessible after deleting an entry from the server");
			}
			catch(AssertionException ae) {
				throw ae;
			}
			catch(Exception e) {
				// supress exception
			}

			try {
				string s = (string)((PropertyValueCollection)de.Properties["dc"]).Value;
				Assert.Fail("Properties should not be accessible after deleting an entry from the server");
			}
			catch(AssertionException ae) {
				throw ae;
			}
			catch(Exception e) {
				// supress exception
			}
		}

		[Test]
		public void DirectoryEntry_Properties2()
		{
			// delete entry, create a new one (the same) and access properties of the old object
			string barakTsabariDN = LDAPServerRoot + "cn=Barak Tsabari,ou=Human Resources,ou=people,dc=myhosting,dc=example";
			de = new DirectoryEntry(barakTsabariDN,
									LDAPServerUsername,
									LDAPServerPassword,
									AuthenticationTypes.ServerBind);

			// cause to properties loading
			Assert.AreEqual(de.Properties.Count,6);
			Assert.AreEqual(((PropertyValueCollection)de.Properties["sn"]).Value,"Tsabari");

			// delete entry
			de.DeleteTree();

			// the local property chache is still accessible
			Assert.AreEqual(de.Properties.Count,6);
			Assert.AreEqual(((PropertyValueCollection)de.Properties["sn"]).Value,"Tsabari");

			de.CommitChanges();

			// the local property chache is still accessible
			((PropertyValueCollection)de.Properties["sn"]).Value = "Barbari";

			// create the entry back again
			DirectoryEntry ouHumanResources = new DirectoryEntry(	LDAPServerRoot + "ou=Human Resources,ou=people,dc=myhosting,dc=example",
																	LDAPServerUsername,
																	LDAPServerPassword,
																	AuthenticationTypes.ServerBind);
			DirectoryEntry cnBarakTsabari = ouHumanResources.Children.Add("cn=Barak Tsabari","Class");
			((PropertyValueCollection)cnBarakTsabari.Properties["objectClass"]).Add("person");
			((PropertyValueCollection)cnBarakTsabari.Properties["objectClass"]).Add("organizationalPerson");
			cnBarakTsabari.Properties["cn"].Value = "Barak Tsabari";
			cnBarakTsabari.Properties["facsimileTelephoneNumber"].Value = "+1 906 777 8853";
			((PropertyValueCollection)cnBarakTsabari.Properties["ou"]).Add("Human Resources");
			((PropertyValueCollection)cnBarakTsabari.Properties["ou"]).Add("People");
			cnBarakTsabari.Properties["sn"].Value = "Tsabari";
			cnBarakTsabari.Properties["telephoneNumber"].Value = "+1 906 777 8854";
			cnBarakTsabari.CommitChanges();
			
			// the local property chache is still accessible
			Assert.AreEqual(((PropertyValueCollection)de.Properties["sn"]).Value,"Barbari");

			// Refresh from server
			de.RefreshCache();
			// ensure the properties of an entry are still accessible through the old object
			Assert.AreEqual(((PropertyValueCollection)de.Properties["sn"]).Value,"Tsabari");

		}


		[Test]
		public void DirectoryEntry_SchemaClassName()
		{
			de = new DirectoryEntry();
			Assert.AreEqual(de.SchemaClassName,"domainDNS");


			de = new DirectoryEntry(LDAPServerConnectionString);
			Assert.AreEqual(de.SchemaClassName,"organization");


			de = new DirectoryEntry(LDAPServerConnectionString,
									LDAPServerUsername,
									LDAPServerPassword,
									AuthenticationTypes.ServerBind);
			Assert.AreEqual(de.SchemaClassName,"organization");

			DirectoryEntry de2 = de.Children.Add("ou=My Child","Class");
			Assert.AreEqual(de2.SchemaClassName,"Class");
			Assert.AreEqual(((PropertyValueCollection)de2.Properties["structuralObjectClass"]).Value,null);
		}

		[Test]
		public void DirectoryEntry_SchemaEntry()
		{
			//			de = new DirectoryEntry();
			//			DirectoryEntry schemaEntry = de.SchemaEntry;
			//
			//			Assert.AreEqual(schemaEntry.Path,"LDAP://schema/domainDNS");
			//			Assert.AreEqual(schemaEntry.Name,"domainDNS");
			//			Assert.AreEqual(schemaEntry.Username,null);
			//			Assert.AreEqual(schemaEntry.Password,null);
			//			Assert.AreEqual(schemaEntry.UsePropertyCache,true);
			//			Assert.AreEqual(schemaEntry.SchemaClassName,"Class");
			//			Assert.AreEqual(schemaEntry.AuthenticationType,AuthenticationTypes.None);


			de = new DirectoryEntry(LDAPServerConnectionString);
			DirectoryEntry schemaEntry = de.SchemaEntry;

			Assert.AreEqual(schemaEntry.Path,LDAPServerRoot + "schema/organization");
			Assert.AreEqual(schemaEntry.Name,"organization");
			Assert.AreEqual(schemaEntry.Username,null);
			Assert.AreEqual(schemaEntry.Password,null);
			Assert.AreEqual(schemaEntry.UsePropertyCache,true);
			Assert.AreEqual(schemaEntry.SchemaClassName,"Class");
			Assert.AreEqual(schemaEntry.AuthenticationType,AuthenticationTypes.None);


			de = new DirectoryEntry(LDAPServerConnectionString,
									LDAPServerUsername,
									LDAPServerPassword,
									AuthenticationTypes.ServerBind);
			schemaEntry = de.SchemaEntry;

			Assert.AreEqual(schemaEntry.Path,LDAPServerRoot + "schema/organization");
			Assert.AreEqual(schemaEntry.Name,"organization");
			Assert.AreEqual(schemaEntry.Username,LDAPServerUsername);
			Assert.AreEqual(schemaEntry.Password,LDAPServerPassword);
			Assert.AreEqual(schemaEntry.UsePropertyCache,true);
			Assert.AreEqual(schemaEntry.SchemaClassName,"Class");
			Assert.AreEqual(schemaEntry.AuthenticationType,AuthenticationTypes.ServerBind);
		}		


		[Test]
		public void DirectoryEntry_Username()
		{
			string wrongUsername = "some wrong username";

			de = new DirectoryEntry();

			Assert.AreEqual(de.Username,null);

			de.Username = LDAPServerUsername;
			Assert.AreEqual(de.Username,LDAPServerUsername);

			de.Username = "";
			Assert.AreEqual(de.Username,String.Empty);
			
			de.Username = wrongUsername;
			Assert.AreEqual(de.Username,wrongUsername);


			de = new DirectoryEntry(LDAPServerConnectionString);

			de.Username = LDAPServerUsername;
			Assert.AreEqual(de.Username,LDAPServerUsername);

			de.Username = "";
			Assert.AreEqual(de.Username,String.Empty);

			de.Username = wrongUsername;
			Assert.AreEqual(de.Username,wrongUsername);

			
			de = new DirectoryEntry(LDAPServerConnectionString,
									LDAPServerUsername,
									LDAPServerPassword,
									AuthenticationTypes.ServerBind);

			de.Username = LDAPServerUsername;
			Assert.AreEqual(de.Username,LDAPServerUsername);

			de.Username = "";
			Assert.AreEqual(de.Username,String.Empty);

			de.Username = wrongUsername;
			Assert.AreEqual(de.Username,wrongUsername);
		}


		[Test]
		public void DirectoryEntry_Close()
		{
			de = new DirectoryEntry();
			de.Close();

			de = new DirectoryEntry(LDAPServerConnectionString);
			de.Close();
	
			de = new DirectoryEntry(LDAPServerConnectionString,
									LDAPServerUsername,
									LDAPServerPassword,
									AuthenticationTypes.ServerBind);
			de.Close();
		}


		[Test]
		public void DirectoryEntry_CommitChanges1()
		{
			string humanResourcesDN = LDAPServerRoot + "ou=Human Resources,ou=people,dc=myhosting,dc=example";
			DirectoryEntry ouHumanResources = new DirectoryEntry(	humanResourcesDN,
																	LDAPServerUsername,
																	LDAPServerPassword,
																	AuthenticationTypes.ServerBind);

			// new entry
			string newEmployeeDN = LDAPServerRoot + "cn=New Employee,ou=Human Resources,ou=people,dc=myhosting,dc=example";
			de = ouHumanResources.Children.Add("cn=New Employee","Class");
			Assert.IsFalse(DirectoryEntry.Exists(newEmployeeDN));

			de.Properties["objectClass"].Value = "organizationalRole";
			de.Properties["cn"].Value = "New Employee";
			Assert.IsFalse(DirectoryEntry.Exists(newEmployeeDN));
			
			de.CommitChanges();
			Assert.IsTrue(DirectoryEntry.Exists(newEmployeeDN));

			// existing entry
			string barakTsabariDN = LDAPServerRoot + "cn=Barak Tsabari,ou=Human Resources,ou=people,dc=myhosting,dc=example";
			de = new DirectoryEntry(barakTsabariDN,
									LDAPServerUsername,
									LDAPServerPassword,
									AuthenticationTypes.ServerBind);

			string oldTelephone = (string)((PropertyValueCollection)de.Properties["telephoneNumber"]).Value;
			string newTelephone = "+972 3 6078596";

			// UsePropertyCache - true
			de.UsePropertyCache = true;
			((PropertyValueCollection)de.Properties["telephoneNumber"]).Value = newTelephone;
			Assert.AreEqual(((PropertyValueCollection)de.Properties["telephoneNumber"]).Value,newTelephone);

			DirectoryEntry cnBarakTsabari = new DirectoryEntry(	barakTsabariDN,
																LDAPServerUsername,
																LDAPServerPassword,
																AuthenticationTypes.ServerBind);

			//check that on server there is still an old value
			Assert.AreEqual(((PropertyValueCollection)cnBarakTsabari.Properties["telephoneNumber"]).Value,oldTelephone);

			de.CommitChanges();

			cnBarakTsabari = new DirectoryEntry(barakTsabariDN,
												LDAPServerUsername,
												LDAPServerPassword,
												AuthenticationTypes.ServerBind);

			// check that new value is updated on the server
			Assert.AreEqual(((PropertyValueCollection)cnBarakTsabari.Properties["telephoneNumber"]).Value,newTelephone);

			// UsePropertyCache - false
			de = new DirectoryEntry(barakTsabariDN,
									LDAPServerUsername,
									LDAPServerPassword,
									AuthenticationTypes.ServerBind);
			de.UsePropertyCache = false;
			Assert.AreEqual(((PropertyValueCollection)de.Properties["telephoneNumber"]).Value,newTelephone);
			((PropertyValueCollection)de.Properties["telephoneNumber"]).Value = oldTelephone;
			Assert.AreEqual(((PropertyValueCollection)de.Properties["telephoneNumber"]).Value,oldTelephone);

			cnBarakTsabari = new DirectoryEntry(barakTsabariDN,
												LDAPServerUsername,
												LDAPServerPassword,
												AuthenticationTypes.ServerBind);

			// check that new value is updated on the server
			Assert.AreEqual(((PropertyValueCollection)cnBarakTsabari.Properties["telephoneNumber"]).Value,oldTelephone);

			de.CommitChanges(); // this should do nothing
		}

		[Test]
		public void DirectoryEntry_CommitChanges2()
		{
			string barakTsabariDN = LDAPServerRoot + "cn=Barak Tsabari,ou=Human Resources,ou=people,dc=myhosting,dc=example";
			DirectoryEntry barakTsabariDE1 = new DirectoryEntry(barakTsabariDN,
																LDAPServerUsername,
																LDAPServerPassword,
																AuthenticationTypes.ServerBind);
			barakTsabariDE1.UsePropertyCache = true;

			DirectoryEntry barakTsabariDE2 = new DirectoryEntry(barakTsabariDN,
																LDAPServerUsername,
																LDAPServerPassword,
																AuthenticationTypes.ServerBind);
			barakTsabariDE2.UsePropertyCache = true;

			string oldTelephone = (string)((PropertyValueCollection)barakTsabariDE1.Properties["telephoneNumber"]).Value;
			string newTelephone = "+972 3 6078596";
			string oldFacsimilieTelephoneNumber = (string)((PropertyValueCollection)barakTsabariDE1.Properties["facsimileTelephoneNumber"]).Value;
			string newFacsimilieTelephoneNumber1 = "+972-3-9872365";
			string newFacsimilieTelephoneNumber2 = "+972-3-9999999";

			barakTsabariDE1.Properties["telephoneNumber"].Value = newTelephone;
			barakTsabariDE1.Properties["facsimileTelephoneNumber"].Value = newFacsimilieTelephoneNumber1;

			barakTsabariDE2.Properties["facsimileTelephoneNumber"].Value = newFacsimilieTelephoneNumber2;

			// only the changed properties of each object are set

			barakTsabariDE1.CommitChanges();
			de = new DirectoryEntry(barakTsabariDN,
									LDAPServerUsername,
									LDAPServerPassword,
									AuthenticationTypes.ServerBind);
			Assert.AreEqual(de.Properties["telephoneNumber"].Value,newTelephone);
			Assert.AreEqual(de.Properties["facsimileTelephoneNumber"].Value,newFacsimilieTelephoneNumber1);

			barakTsabariDE2.CommitChanges();
			de = new DirectoryEntry(barakTsabariDN,
									LDAPServerUsername,
									LDAPServerPassword,
									AuthenticationTypes.ServerBind);
			Assert.AreEqual(de.Properties["telephoneNumber"].Value,newTelephone);
			Assert.AreEqual(de.Properties["facsimileTelephoneNumber"].Value,newFacsimilieTelephoneNumber2);
		}


		[Test]
		[ExpectedException(typeof(NotImplementedException))]
		public void DirectoryEntry_CopyTo()
		{
			string barakTsabariDN = LDAPServerRoot + "cn=Barak Tsabari,ou=Human Resources,ou=people,dc=myhosting,dc=example";
			de = new DirectoryEntry(LDAPServerConnectionString,
									LDAPServerUsername,
									LDAPServerPassword,
									AuthenticationTypes.ServerBind);

			DirectoryEntry cnBarakTsabari = new DirectoryEntry(	barakTsabariDN,
																LDAPServerUsername,
																LDAPServerPassword,
																AuthenticationTypes.ServerBind);

			cnBarakTsabari.CopyTo(de);
		}


		[Test]
		public void DirectoryEntry_DeleteTree()
		{
			string barakTsabariDN = LDAPServerRoot + "cn=Barak Tsabari,ou=Human Resources,ou=people,dc=myhosting,dc=example";

			Assert.IsTrue(DirectoryEntry.Exists(barakTsabariDN));
			de = new DirectoryEntry(barakTsabariDN,
									LDAPServerUsername,
									LDAPServerPassword,
									AuthenticationTypes.ServerBind);						

			// no properties changed
			de.DeleteTree();
			de.CommitChanges();

			Assert.IsFalse(DirectoryEntry.Exists(barakTsabariDN));

			string johnSmithDN = LDAPServerRoot + "cn=John Smith,ou=Human Resources,ou=people,dc=myhosting,dc=example";

			Assert.IsTrue(DirectoryEntry.Exists(johnSmithDN));
			de = new DirectoryEntry(johnSmithDN,
									LDAPServerUsername,
									LDAPServerPassword,
									AuthenticationTypes.ServerBind);

			de.Properties["telephoneNumber"].Value = "+972 3 9999999";

			// some properties changed
			de.DeleteTree();
			try {
				de.CommitChanges();					
				Assert.Fail("Object " + johnSmithDN + " was not deleted from server");
			}
			catch(AssertionException ae) {
				throw ae;
			}
			catch(Exception e) {
				Console.WriteLine(e.StackTrace);
				// do nothing
			}
		}

		[Test]
		public void DirectoryEntry_DeleteTree2()
		{
			string johnSmithDN = LDAPServerRoot + "cn=John Smith,ou=Human Resources,ou=people,dc=myhosting,dc=example";

			Assert.IsTrue(DirectoryEntry.Exists(johnSmithDN));
			// two objects refer to the same entry
			de = new DirectoryEntry(johnSmithDN,
									LDAPServerUsername,
									LDAPServerPassword,
									AuthenticationTypes.ServerBind);

			DirectoryEntry johnSmithDE = new DirectoryEntry(johnSmithDN,
															LDAPServerUsername,
															LDAPServerPassword,
															AuthenticationTypes.ServerBind);

			johnSmithDE.Properties["telephoneNumber"].Value = "+972 3 9999999";

			// check that the second entry is not accessible after the first is deleted
			de.DeleteTree();
			de.CommitChanges();

			try {
				johnSmithDE.CommitChanges();					
				Assert.Fail("Object " + johnSmithDN + " should not be accessible");
			}
			catch(AssertionException ae) {
				throw ae;
			}
			catch(Exception e) {
				// do nothing
			}
		}


		[Test]
		public void DirectoryEntry_Exists()
		{
			string barakTsabariDN = LDAPServerRoot + "cn=Barak Tsabari,ou=Human Resources,ou=people,dc=myhosting,dc=example";
			string johnSmithDN = LDAPServerRoot + "cn=John Smith,ou=Human Resources,ou=people,dc=myhosting,dc=example";
			string humanResourcesOU = LDAPServerRoot + "ou=Human Resources,ou=people,dc=myhosting,dc=example";

			Assert.IsTrue(DirectoryEntry.Exists(barakTsabariDN));
			Assert.IsTrue(DirectoryEntry.Exists(johnSmithDN));
			Assert.IsTrue(DirectoryEntry.Exists(humanResourcesOU));

			Assert.IsFalse(DirectoryEntry.Exists(barakTsabariDN + ",dc=mono"));
		}


		[Test]
		public void DirectoryEntry_MoveTo_De()
		{
			string barakTsabariHumanResourcesDN = LDAPServerRoot + "cn=Barak Tsabari,ou=Human Resources,ou=people,dc=myhosting,dc=example";
			string barakTsabariDevQaDN = LDAPServerRoot + "cn=Barak Tsabari,ou=DevQA,ou=people,dc=myhosting,dc=example";

			DirectoryEntry barakTsabariDE = new DirectoryEntry(	barakTsabariHumanResourcesDN,
																LDAPServerUsername,
																LDAPServerPassword,
																AuthenticationTypes.ServerBind);

			string devQaOU = LDAPServerRoot + "ou=DevQA,ou=people,dc=myhosting,dc=example";

			DirectoryEntry devQaDE = new DirectoryEntry(devQaOU,
														LDAPServerUsername,
														LDAPServerPassword,
														AuthenticationTypes.ServerBind);

			barakTsabariDE.MoveTo(devQaDE);
			barakTsabariDE.CommitChanges();

			Assert.IsTrue(DirectoryEntry.Exists(barakTsabariDevQaDN));

			string humanRwsourcesOU = LDAPServerRoot + "ou=Human Resources,ou=people,dc=myhosting,dc=example";

			DirectoryEntry humanResourcesDE = new DirectoryEntry(	humanRwsourcesOU,
																	LDAPServerUsername,
																	LDAPServerPassword,
																	AuthenticationTypes.ServerBind);

			barakTsabariDE.MoveTo(humanResourcesDE);
			barakTsabariDE.CommitChanges();

			Assert.IsTrue(DirectoryEntry.Exists(barakTsabariHumanResourcesDN));
		}


		[Test]
		public void DirectoryEntry_MoveTo_DeStr()
		{
			string barakTsabariHumanResourcesDN = LDAPServerRoot + "cn=Barak Tsabari,ou=Human Resources,ou=people,dc=myhosting,dc=example";
			string barakTsabariDevQaDN = LDAPServerRoot + "cn=My Name,ou=DevQA,ou=people,dc=myhosting,dc=example";

			DirectoryEntry barakTsabariDE = new DirectoryEntry(	barakTsabariHumanResourcesDN,
																LDAPServerUsername,
																LDAPServerPassword,
																AuthenticationTypes.ServerBind);

			string devQaOU = LDAPServerRoot + "ou=DevQA,ou=people,dc=myhosting,dc=example";

			DirectoryEntry devQaDE = new DirectoryEntry(devQaOU,
														LDAPServerUsername,
														LDAPServerPassword,
														AuthenticationTypes.ServerBind);

			barakTsabariDE.MoveTo(devQaDE,"cn=My Name");
			barakTsabariDE.CommitChanges();

			Assert.IsTrue(DirectoryEntry.Exists(barakTsabariDevQaDN));

			string humanRwsourcesOU = LDAPServerRoot + "ou=Human Resources,ou=people,dc=myhosting,dc=example";

			DirectoryEntry humanResourcesDE = new DirectoryEntry(	humanRwsourcesOU,
																	LDAPServerUsername,
																	LDAPServerPassword,
																	AuthenticationTypes.ServerBind);

			barakTsabariDE.MoveTo(humanResourcesDE,"cn=Barak Tsabari");
			barakTsabariDE.CommitChanges();

			Assert.IsTrue(DirectoryEntry.Exists(barakTsabariHumanResourcesDN));
		}

		[Test]
		public void DirectoryEntry_RefreshCache()
		{
			de = new DirectoryEntry(LDAPServerConnectionString);
			de.UsePropertyCache = true;
			
			string newValue = "Just a company";
			string oldValue = (string)((PropertyValueCollection)de.Properties["description"]).Value;
			((PropertyValueCollection)de.Properties["description"]).Value = newValue;
			
			Assert.AreEqual(((PropertyValueCollection)de.Properties["description"]).Value,newValue);

			de.RefreshCache();

			Assert.AreEqual(((PropertyValueCollection)de.Properties["description"]).Value,oldValue);
			
			// call RefeshCache on new entry prior to submitting it to the server shoud fail
			string newEmployeeDN = LDAPServerRoot + "cn=New Employee,ou=Human Resources,ou=people,dc=myhosting,dc=example";
			string humanResourcesDN = LDAPServerRoot + "ou=Human Resources,ou=people,dc=myhosting,dc=example";

			DirectoryEntry humanResourcesDE = new DirectoryEntry(	humanResourcesDN,
																	LDAPServerUsername,
																	LDAPServerPassword,
																	AuthenticationTypes.ServerBind);

			DirectoryEntry newEmployeeDE = humanResourcesDE.Children.Add("cn=New Employee","Class");
			Assert.AreEqual(newEmployeeDE.Properties["cn"].Value,null);

			((PropertyValueCollection)newEmployeeDE.Properties["objectClass"]).Add("person");
			((PropertyValueCollection)newEmployeeDE.Properties["objectClass"]).Add("organizationalPerson");
			newEmployeeDE.Properties["cn"].Value = "New Employee";
			newEmployeeDE.Properties["sn"].Value = "Employee";
			newEmployeeDE.Properties["ou"].Value = "Human Resources";

			Assert.AreEqual(newEmployeeDE.Properties["cn"].Value,"New Employee");

			try {
				newEmployeeDE.RefreshCache();
				Assert.Fail("Call to RefreshCache did not fail");
			}
			catch(AssertionException ae) {
				throw ae;
			}
			catch (Exception e) {
				// supress exception
			}

			Assert.AreEqual(newEmployeeDE.Properties["cn"].Value,"New Employee");

			newEmployeeDE.CommitChanges();

			// now it should work without any problem
			newEmployeeDE.RefreshCache();

			Assert.AreEqual(newEmployeeDE.Properties["cn"].Value,"New Employee");
		}

		[Test]
		public void DirectoryEntry_RefreshCache_StrArr()
		{
			de = new DirectoryEntry(LDAPServerConnectionString);
			de.UsePropertyCache = true;
			
			string newValue = "Just a company";
			string oldValue = (string)((PropertyValueCollection)de.Properties["description"]).Value;
			((PropertyValueCollection)de.Properties["description"]).Value = newValue;
			
			Assert.AreEqual(((PropertyValueCollection)de.Properties["description"]).Value,newValue);

			de.RefreshCache(new string[] {"cn"});

			Assert.AreEqual(((PropertyValueCollection)de.Properties["description"]).Value,newValue);

			de.RefreshCache(new string[] {"description"});

			Assert.AreEqual(((PropertyValueCollection)de.Properties["description"]).Value,oldValue);				
		}

		[Test]
		public void DirectoryEntry_Rename()
		{
			string barakTsabariOldDN = LDAPServerRoot + "cn=Barak Tsabari,ou=Human Resources,ou=people,dc=myhosting,dc=example";
			string barakTsabariNewDN = LDAPServerRoot + "cn=My Name,ou=Human Resources,ou=people,dc=myhosting,dc=example";

			DirectoryEntry barakTsabariDE = new DirectoryEntry(	barakTsabariOldDN,
																LDAPServerUsername,
																LDAPServerPassword,
																AuthenticationTypes.ServerBind);

			barakTsabariDE.Rename("cn=My Name");
			barakTsabariDE.CommitChanges();

			Assert.IsTrue(DirectoryEntry.Exists(barakTsabariNewDN));

			barakTsabariDE.Rename("cn=Barak Tsabari");
			barakTsabariDE.CommitChanges();

			Assert.IsTrue(DirectoryEntry.Exists(barakTsabariOldDN));
		}

		#endregion Tests
	}
}
