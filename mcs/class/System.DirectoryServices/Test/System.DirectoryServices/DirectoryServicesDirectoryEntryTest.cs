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
			de = new DirectoryEntry(LDAPServerConnectionString,
									LDAPServerUsername,
									LDAPServerPassword,
									AuthenticationTypes.None);

			de.UsePropertyCache = true;
			Assert.AreEqual(de.UsePropertyCache,true);

			de.UsePropertyCache = false;
			Assert.AreEqual(de.UsePropertyCache,false);
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
		public void DirectoryEntry_Properties()
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
			de = new DirectoryEntry();
			DirectoryEntry schemaEntry = de.SchemaEntry;

			Assert.AreEqual(schemaEntry.Path,"LDAP://schema/domainDNS");
			Assert.AreEqual(schemaEntry.Name,"domainDNS");
			Assert.AreEqual(schemaEntry.Username,null);
			Assert.AreEqual(schemaEntry.Password,null);
			Assert.AreEqual(schemaEntry.UsePropertyCache,true);
			Assert.AreEqual(schemaEntry.SchemaClassName,"Class");
			Assert.AreEqual(schemaEntry.AuthenticationType,AuthenticationTypes.None);


			de = new DirectoryEntry(LDAPServerConnectionString);
			schemaEntry = de.SchemaEntry;

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
		public void DirectoryEntry_CommitChanges()
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

			de = new DirectoryEntry(barakTsabariDN,
									LDAPServerUsername,
									LDAPServerPassword,
									AuthenticationTypes.ServerBind);
			
			Assert.IsTrue(DirectoryEntry.Exists(barakTsabariDN));

			de.DeleteTree();
			de.CommitChanges();

			Assert.IsFalse(DirectoryEntry.Exists(barakTsabariDN));
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
