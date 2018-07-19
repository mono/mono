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

		static TestConfiguration configuration;
		static DirectoryEntry de;

		#endregion // Fields

		#region SetUp and TearDown

		[TestFixtureSetUp]
		public void TestFixtureSetUp()
		{
			de = null;
			configuration = new TestConfiguration ();

			if (String.IsNullOrEmpty (configuration.ConnectionString))
				Assert.Ignore ("No configuration");
		}


		[TestFixtureTearDown]
		public void TestFixtureTearDown()
		{
			if (de != null)
				de.Dispose ();
			de = null;
		}


		[SetUp]
		public void SetUp()
		{
			TearDown();

			#region Initialize basics

			DirectoryEntry root = new DirectoryEntry(	configuration.ConnectionString,
														configuration.Username,
														configuration.Password,
														configuration.AuthenticationType);
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
			cnManager.Properties["facsimileTelephoneNumber"].Value = "+1 602 333 1238";
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
			if (de != null)
				de.Dispose ();

			de = null;

			using (DirectoryEntry root = new DirectoryEntry(	configuration.ConnectionString,
														configuration.Username,
														configuration.Password,
														configuration.AuthenticationType)) {
			
			foreach(DirectoryEntry child in root.Children) {
				DeleteTree_DFS(child);
			}
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
			Assert.AreEqual(de.Path,String.Empty);
			Assert.AreEqual(de.UsePropertyCache,true);
			Assert.AreEqual(de.Username,null);		
		}


		[Test]
		public void DirectoryEntry_DirectoryEntry_Str()
		{
			using (DirectoryEntry de = new DirectoryEntry(configuration.ConnectionString)) {
			
			Assert.AreEqual(de.AuthenticationType,AuthenticationTypes.None);
			Assert.AreEqual(de.Name,GetName (configuration.BaseDn));
			Assert.AreEqual(de.Path,configuration.ConnectionString);
			Assert.AreEqual(de.SchemaClassName,"organization");
			Assert.AreEqual(de.UsePropertyCache,true);
			Assert.AreEqual(de.Username,null);
			}
		}


		[Test]
		public void DirectoryEntry_DirectoryEntry_StrStrStrAuth()
		{
			if ((configuration.AuthenticationType != AuthenticationTypes.ServerBind) && 
				(configuration.AuthenticationType != AuthenticationTypes.None) && 
				(configuration.AuthenticationType != AuthenticationTypes.Anonymous))
				return;

			#region AuthenticationTypes.Anonymous

			using (DirectoryEntry de = new DirectoryEntry(	configuration.ConnectionString,
													configuration.Username,
													configuration.Password,
													AuthenticationTypes.Anonymous)){
			
			Assert.AreEqual(de.AuthenticationType,AuthenticationTypes.Anonymous);
			//Assert.AreEqual(de.Guid,new Guid("0b045012-1d97-4f94-9d47-87cbf6dada46"));
			Assert.AreEqual(de.Name,GetName (configuration.BaseDn));
			//Assert.AreEqual(de.NativeGuid,null);
			Assert.AreEqual(de.Path,configuration.ConnectionString);
			Assert.AreEqual(de.SchemaClassName,"organization");
			Assert.AreEqual(de.UsePropertyCache,true);
			Assert.AreEqual(de.Username,configuration.Username);
			}

			#endregion //AuthenticationTypes.Anonymous

			#region AuthenticationTypes.Delegation

			using (DirectoryEntry de = new DirectoryEntry(configuration.ConnectionString,
									configuration.Username,
									configuration.Password,
									AuthenticationTypes.Delegation)){
			
			Assert.AreEqual(de.AuthenticationType,AuthenticationTypes.Delegation);
			//Assert.AreEqual(de.Guid,new Guid("0b045012-1d97-4f94-9d47-87cbf6dada46"));
			Assert.AreEqual(de.Name,GetName (configuration.BaseDn));
			//Assert.AreEqual(de.NativeGuid,null);
			Assert.AreEqual(de.Path,configuration.ConnectionString);
			Assert.AreEqual(de.SchemaClassName,"organization");
			Assert.AreEqual(de.UsePropertyCache,true);
			Assert.AreEqual(de.Username,configuration.Username);
			}

			#endregion //AuthenticationTypes.Delegation

			#region AuthenticationTypes.Encryption

			//			de = new DirectoryEntry(	configuration.ConnectionString,
			//													configuration.Username,
			//													configuration.Password,
			//													AuthenticationTypes.Encryption);
			//			
			//			Assert.AreEqual(de.AuthenticationType,AuthenticationTypes.Encryption);
			//			//Assert.AreEqual(de.Guid,new Guid("0b045012-1d97-4f94-9d47-87cbf6dada46"));
			//			Assert.AreEqual(de.Name,"dc=myhosting");
			//			//Assert.AreEqual(de.NativeGuid,null);
			//			Assert.AreEqual(de.Password,configuration.Password);
			//			Assert.AreEqual(de.Path,configuration.ConnectionString);
			//			Assert.AreEqual(de.SchemaClassName,"organization");
			//			Assert.AreEqual(de.UsePropertyCache,true);
			//			Assert.AreEqual(de.Username,configuration.Username);

			#endregion //AuthenticationTypes.Encryption

			#region AuthenticationTypes.FastBind

			using (DirectoryEntry de = new DirectoryEntry(configuration.ConnectionString,
									configuration.Username,
									configuration.Password,
									AuthenticationTypes.FastBind)){
			
			Assert.AreEqual(de.AuthenticationType,AuthenticationTypes.FastBind);
			//Assert.AreEqual(de.Guid,new Guid("0b045012-1d97-4f94-9d47-87cbf6dada46"));
			Assert.AreEqual(de.Name,GetName (configuration.BaseDn));
			//Assert.AreEqual(de.NativeGuid,null);
			Assert.AreEqual(de.Path,configuration.ConnectionString);
			Assert.AreEqual(de.SchemaClassName,"organization");
			Assert.AreEqual(de.UsePropertyCache,true);
			Assert.AreEqual(de.Username,configuration.Username);
			}

			#endregion //AuthenticationTypes.FastBind

			#region AuthenticationTypes.None

			using (DirectoryEntry de = new DirectoryEntry(configuration.ConnectionString,
									configuration.Username,
									configuration.Password,
									AuthenticationTypes.None)){
			
			Assert.AreEqual(de.AuthenticationType,AuthenticationTypes.None);
			//Assert.AreEqual(de.Guid,new Guid("0b045012-1d97-4f94-9d47-87cbf6dada46"));
			Assert.AreEqual(de.Name,GetName (configuration.BaseDn));
			//Assert.AreEqual(de.NativeGuid,null);
			Assert.AreEqual(de.Path,configuration.ConnectionString);
			Assert.AreEqual(de.SchemaClassName,"organization");
			Assert.AreEqual(de.UsePropertyCache,true);
			Assert.AreEqual(de.Username,configuration.Username);
			}

			#endregion //AuthenticationTypes.None

			#region AuthenticationTypes.ReadonlyServer

			using (DirectoryEntry de = new DirectoryEntry(configuration.ConnectionString,
									configuration.Username,
									configuration.Password,
									AuthenticationTypes.ReadonlyServer)){
			
			Assert.AreEqual(de.AuthenticationType,AuthenticationTypes.ReadonlyServer);
			//Assert.AreEqual(de.Guid,new Guid("0b045012-1d97-4f94-9d47-87cbf6dada46"));
			Assert.AreEqual(de.Name,GetName (configuration.BaseDn));
			//Assert.AreEqual(de.NativeGuid,null);
			Assert.AreEqual(de.Path,configuration.ConnectionString);
			Assert.AreEqual(de.SchemaClassName,"organization");
			Assert.AreEqual(de.UsePropertyCache,true);
			Assert.AreEqual(de.Username,configuration.Username);
			}

			#endregion //AuthenticationTypes.ReadonlyServer

			#region AuthenticationTypes.Sealing

			using (DirectoryEntry de = new DirectoryEntry(configuration.ConnectionString,
									configuration.Username,
									configuration.Password,
									AuthenticationTypes.Sealing)){
			
			Assert.AreEqual(de.AuthenticationType,AuthenticationTypes.Sealing);
			//Assert.AreEqual(de.Guid,new Guid("0b045012-1d97-4f94-9d47-87cbf6dada46"));
			Assert.AreEqual(de.Name,GetName (configuration.BaseDn));
			//Assert.AreEqual(de.NativeGuid,null);
			Assert.AreEqual(de.Path,configuration.ConnectionString);
			Assert.AreEqual(de.SchemaClassName,"organization");
			Assert.AreEqual(de.UsePropertyCache,true);
			Assert.AreEqual(de.Username,configuration.Username);
			}

			#endregion //AuthenticationTypes.Sealing

			#region AuthenticationTypes.Secure

			//			de = new DirectoryEntry(configuration.ConnectionString,
			//									configuration.Username,
			//									configuration.Password,
			//									AuthenticationTypes.Secure);
			//			
			//			Assert.AreEqual(de.AuthenticationType,AuthenticationTypes.Secure);
			//			//Assert.AreEqual(de.Guid,new Guid("0b045012-1d97-4f94-9d47-87cbf6dada46"));
			//			Assert.AreEqual(de.Name,"dc=myhosting");
			//			//Assert.AreEqual(de.NativeGuid,null);
			//			Assert.AreEqual(de.Password,configuration.Password);
			//			Assert.AreEqual(de.Path,configuration.ConnectionString);
			//			Assert.AreEqual(de.SchemaClassName,"organization");
			//			Assert.AreEqual(de.UsePropertyCache,true);
			//			Assert.AreEqual(de.Username,configuration.Username);

			#endregion //AuthenticationTypes.Secure

			#region AuthenticationTypes.SecureSocketsLayer

			//			de = new DirectoryEntry(configuration.ConnectionString,
			//									configuration.Username,
			//									configuration.Password,
			//									AuthenticationTypes.SecureSocketsLayer);
			//			
			//			Assert.AreEqual(de.AuthenticationType,AuthenticationTypes.SecureSocketsLayer);
			//			//Assert.AreEqual(de.Guid,new Guid("0b045012-1d97-4f94-9d47-87cbf6dada46"));
			//			Assert.AreEqual(de.Name,"dc=myhosting");
			//			//Assert.AreEqual(de.NativeGuid,null);
			//			Assert.AreEqual(de.Password,configuration.Password);
			//			Assert.AreEqual(de.Path,configuration.ConnectionString);
			//			Assert.AreEqual(de.SchemaClassName,"organization");
			//			Assert.AreEqual(de.UsePropertyCache,true);
			//			Assert.AreEqual(de.Username,configuration.Username);

			#endregion //AuthenticationTypes.SecureSocketsLayer

			#region AuthenticationTypes.ServerBind

			using (DirectoryEntry de = new DirectoryEntry(configuration.ConnectionString,
									configuration.Username,
									configuration.Password,
									AuthenticationTypes.ServerBind)){
			
			Assert.AreEqual(de.AuthenticationType,AuthenticationTypes.ServerBind);
			//Assert.AreEqual(de.Guid,new Guid("0b045012-1d97-4f94-9d47-87cbf6dada46"));
			Assert.AreEqual(de.Name,GetName (configuration.BaseDn));
			//Assert.AreEqual(de.NativeGuid,null);
			Assert.AreEqual(de.Path,configuration.ConnectionString);
			Assert.AreEqual(de.SchemaClassName,"organization");
			Assert.AreEqual(de.UsePropertyCache,true);
			Assert.AreEqual(de.Username,configuration.Username);
			}

			#endregion //AuthenticationTypes.ServerBind

			#region AuthenticationTypes.Signing

			using (DirectoryEntry de = new DirectoryEntry(configuration.ConnectionString,
									configuration.Username,
									configuration.Password,
									AuthenticationTypes.Signing)){
			
			Assert.AreEqual(de.AuthenticationType,AuthenticationTypes.Signing);
			//Assert.AreEqual(de.Guid,new Guid("0b045012-1d97-4f94-9d47-87cbf6dada46"));
			Assert.AreEqual(de.Name,GetName (configuration.BaseDn));
			//Assert.AreEqual(de.NativeGuid,null);
			Assert.AreEqual(de.Path,configuration.ConnectionString);
			Assert.AreEqual(de.SchemaClassName,"organization");
			Assert.AreEqual(de.UsePropertyCache,true);
			Assert.AreEqual(de.Username,configuration.Username);
			}

			#endregion //AuthenticationTypes.Signing
		}

		[Test]
		public void DirectoryEntry_Dispose()
		{
			using (DirectoryEntry root = new DirectoryEntry(	configuration.ConnectionString,
														configuration.Username,
														configuration.Password,
														configuration.AuthenticationType)){

			DirectoryEntry ouPeople = root.Children.Add("ou=printers","Class");
			ouPeople.Properties["objectClass"].Value = "organizationalUnit";
			ouPeople.Properties["description"].Value = "All printers in organisation";
			ouPeople.Properties["ou"].Value = "printers";
			ouPeople.CommitChanges();

			ouPeople.Rename("ou=anotherPrinters");
			ouPeople.CommitChanges();

			Assert.IsTrue(DirectoryEntry.Exists(configuration.ServerRoot + "ou=anotherPrinters" + ((configuration.BaseDn.Length == 0) ? String.Empty : ("," + configuration.BaseDn))));
			}
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

			de = new DirectoryEntry(configuration.ConnectionString);

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

			de = new DirectoryEntry(configuration.ConnectionString,
									configuration.Username,
									configuration.Password,
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
			string barakTsabariDN = configuration.ServerRoot + "cn=Barak Tsabari,ou=Human Resources,ou=people" + ((configuration.BaseDn.Length == 0) ? String.Empty : ("," + configuration.BaseDn));
			de = new DirectoryEntry(barakTsabariDN,
									configuration.Username,
									configuration.Password,
									configuration.AuthenticationType);

			// UsePropertyCache = true
			de.UsePropertyCache = true;
			Assert.AreEqual(de.UsePropertyCache,true);

			#region Check Properties

			// Properties changes are cached
			string oldTelephoneNumber = (string)de.Properties["telephoneNumber"].Value;
			string newTelephoneNumber = "+972-3-6572345";

			de.Properties["telephoneNumber"].Value = newTelephoneNumber;
			using (DirectoryEntry barakTsabariDE = new DirectoryEntry(	barakTsabariDN,
																configuration.Username,
																configuration.Password,
																configuration.AuthenticationType)){

			Assert.AreEqual(barakTsabariDE.Properties["telephoneNumber"].Value,oldTelephoneNumber);
			de.CommitChanges();
			}
			using (DirectoryEntry barakTsabariDE = new DirectoryEntry(barakTsabariDN,
												configuration.Username,
												configuration.Password,
												configuration.AuthenticationType)){
			Assert.AreEqual(barakTsabariDE.Properties["telephoneNumber"].Value,newTelephoneNumber);

			// restore object state
			de.Properties["telephoneNumber"].Value = oldTelephoneNumber;
			de.CommitChanges();
			}

			#endregion // Check Properties

			#region Check DeleteTree

			// DeleteTree is not cached
			de.DeleteTree();
			try {
				using (DirectoryEntry barakTsabariDE = new DirectoryEntry(barakTsabariDN,
													configuration.Username,
													configuration.Password,
													configuration.AuthenticationType)){
				barakTsabariDE.Properties["telephoneNumber"].Value = newTelephoneNumber;
				barakTsabariDE.CommitChanges();
				Assert.Fail("Object " + barakTsabariDN + " was not deleted from server.");
				}
			}
			catch(AssertionException ae) {
				throw ae;
			}
			catch (Exception e) {
				// do nothing
			}

			// restore object state
			using (DirectoryEntry ouHumanResources = new DirectoryEntry(	configuration.ServerRoot + "ou=Human Resources,ou=people" + ((configuration.BaseDn.Length == 0) ? String.Empty : ("," + configuration.BaseDn)),
																	configuration.Username,
																	configuration.Password,
																	configuration.AuthenticationType)){
			using (DirectoryEntry cnBarakTsabari = ouHumanResources.Children.Add("cn=Barak Tsabari","Class")){
			((PropertyValueCollection)cnBarakTsabari.Properties["objectClass"]).Add("person");
			((PropertyValueCollection)cnBarakTsabari.Properties["objectClass"]).Add("organizationalPerson");
			cnBarakTsabari.Properties["cn"].Value = "Barak Tsabari";
			cnBarakTsabari.Properties["facsimileTelephoneNumber"].Value = "+1 906 777 8853";
			((PropertyValueCollection)cnBarakTsabari.Properties["ou"]).Add("Human Resources");
			((PropertyValueCollection)cnBarakTsabari.Properties["ou"]).Add("People");
			cnBarakTsabari.Properties["sn"].Value = "Tsabari";
			cnBarakTsabari.Properties["telephoneNumber"].Value = "+1 906 777 8854";
			cnBarakTsabari.CommitChanges();
			}
			}

			#endregion // Check DeleteTree

			#region Check MoveTo

			// Move to is not cached
			de = new DirectoryEntry(barakTsabariDN,
									configuration.Username,
									configuration.Password,
									configuration.AuthenticationType);

			using (DirectoryEntry ouRnD = new DirectoryEntry(	configuration.ServerRoot + "ou=R&D,ou=people" + ((configuration.BaseDn.Length == 0) ? String.Empty : ("," + configuration.BaseDn)),
														configuration.Username,
														configuration.Password,
														configuration.AuthenticationType)){
			de.MoveTo(ouRnD);
			try {
				using (DirectoryEntry barakTsabariDE = new DirectoryEntry(barakTsabariDN,
													configuration.Username,
													configuration.Password,
													configuration.AuthenticationType)){
				barakTsabariDE.Properties["telephoneNumber"].Value = newTelephoneNumber;
				barakTsabariDE.CommitChanges();
				Assert.Fail("Object " + barakTsabariDN + " was not moved from old location on the server.");
				}
			}
			catch(AssertionException ae) {
				throw ae;
			}
			catch (Exception e) {
				// do nothing
			}
			}

			using (DirectoryEntry barakTsabariDE = new DirectoryEntry(configuration.ServerRoot + "cn=Barak Tsabari,ou=R&D,ou=people" + ((configuration.BaseDn.Length == 0) ? String.Empty : ("," + configuration.BaseDn)),
												configuration.Username,
												configuration.Password,
												configuration.AuthenticationType)){
			Assert.AreEqual(barakTsabariDE.Properties["telephoneNumber"].Value,oldTelephoneNumber);
			}
			

			// restore object state
			using (DirectoryEntry ouHumanResources = new DirectoryEntry(	configuration.ServerRoot + "ou=Human Resources,ou=people" + ((configuration.BaseDn.Length == 0) ? String.Empty : ("," + configuration.BaseDn)),
																	configuration.Username,
																	configuration.Password,
																	configuration.AuthenticationType)){
			using (DirectoryEntry barakTsabariDE = new DirectoryEntry(configuration.ServerRoot + "cn=Barak Tsabari,ou=R&D,ou=people" + ((configuration.BaseDn.Length == 0) ? String.Empty : ("," + configuration.BaseDn)),
												configuration.Username,
												configuration.Password,
												configuration.AuthenticationType)){
			barakTsabariDE.MoveTo(ouHumanResources);
			barakTsabariDE.CommitChanges();
			}
			}

			#endregion // Check MoveTo

			#region Check Rename

			// Rename not chached
			de = new DirectoryEntry(barakTsabariDN,
									configuration.Username,
									configuration.Password,
									configuration.AuthenticationType);

			de.Rename("cn=MyUser");

			try {
				using (DirectoryEntry barakTsabariDE = new DirectoryEntry(barakTsabariDN,
													configuration.Username,
													configuration.Password,
													configuration.AuthenticationType)){
				barakTsabariDE.Properties["telephoneNumber"].Value = newTelephoneNumber;
				barakTsabariDE.CommitChanges();
				Assert.Fail("Object " + barakTsabariDN + " was not renamed on the server.");
				}
			}
			catch(AssertionException ae) {
				throw ae;
			}
			catch (Exception e) {
				// do nothing
			}

			using (DirectoryEntry barakTsabariDE = new DirectoryEntry(configuration.ServerRoot + "cn=MyUser,ou=Human Resources,ou=people" + ((configuration.BaseDn.Length == 0) ? String.Empty : ("," + configuration.BaseDn)),
												configuration.Username,
												configuration.Password,
												configuration.AuthenticationType)){
			Assert.AreEqual(barakTsabariDE.Properties["telephoneNumber"].Value,oldTelephoneNumber);
			}

			// restore object state
			using (DirectoryEntry barakTsabariDE = new DirectoryEntry(configuration.ServerRoot + "cn=MyUser,ou=Human Resources,ou=people" + ((configuration.BaseDn.Length == 0) ? String.Empty : ("," + configuration.BaseDn)),
												configuration.Username,
												configuration.Password,
												configuration.AuthenticationType)){
			barakTsabariDE.Rename("cn=Barak Tsabari");
			barakTsabariDE.CommitChanges();
			}

			#endregion // Check Rename

			// UsePropertyCache = false	
			de = new DirectoryEntry(barakTsabariDN,
									configuration.Username,
									configuration.Password,
									configuration.AuthenticationType);
			de.UsePropertyCache = false;
			Assert.AreEqual(de.UsePropertyCache,false);

			#region Check Properties

			// Properties changes not cached
			de.Properties["telephoneNumber"].Value = newTelephoneNumber;
			using (DirectoryEntry barakTsabariDE = new DirectoryEntry(barakTsabariDN,
												configuration.Username,
												configuration.Password,
												configuration.AuthenticationType)){
			}

			//Assert.AreEqual(barakTsabariDE.Properties["telephoneNumber"].Value,newTelephoneNumber);

			#endregion // Check Properties

			#region Check DeleteTree

			// DeleteTree is not cached
			de.DeleteTree();
			try {
				using (DirectoryEntry barakTsabariDE = new DirectoryEntry(barakTsabariDN,
													configuration.Username,
													configuration.Password,
													configuration.AuthenticationType)){
				barakTsabariDE.Properties["telephoneNumber"].Value = newTelephoneNumber;
				barakTsabariDE.CommitChanges();
				Assert.Fail("Object " + barakTsabariDN + " was not deleted from server.");
				}
			}
			catch(AssertionException ae) {
				throw ae;
			}
			catch (Exception e) {
				// do nothing
			}

			// restore object state
			using (DirectoryEntry ouHumanResources = new DirectoryEntry(	configuration.ServerRoot + "ou=Human Resources,ou=people" + ((configuration.BaseDn.Length == 0) ? String.Empty : ("," + configuration.BaseDn)),
																	configuration.Username,
																	configuration.Password,
																	configuration.AuthenticationType)){
			using (DirectoryEntry cnBarakTsabari = ouHumanResources.Children.Add("cn=Barak Tsabari","Class")){
			((PropertyValueCollection)cnBarakTsabari.Properties["objectClass"]).Add("person");
			((PropertyValueCollection)cnBarakTsabari.Properties["objectClass"]).Add("organizationalPerson");
			cnBarakTsabari.Properties["cn"].Value = "Barak Tsabari";
			cnBarakTsabari.Properties["facsimileTelephoneNumber"].Value = "+1 906 777 8853";
			((PropertyValueCollection)cnBarakTsabari.Properties["ou"]).Add("Human Resources");
			((PropertyValueCollection)cnBarakTsabari.Properties["ou"]).Add("People");
			cnBarakTsabari.Properties["sn"].Value = "Tsabari";
			cnBarakTsabari.Properties["telephoneNumber"].Value = "+1 906 777 8854";
			cnBarakTsabari.CommitChanges();
			}
			}

			#endregion // Check DeleteTree

			#region Check MoveTo

			// Move to is not cached
			de = new DirectoryEntry(barakTsabariDN,
									configuration.Username,
									configuration.Password,
									configuration.AuthenticationType);

			using (DirectoryEntry ouRnD = new DirectoryEntry(	configuration.ServerRoot + "ou=R&D,ou=people" + ((configuration.BaseDn.Length == 0) ? String.Empty : ("," + configuration.BaseDn)),
										configuration.Username,
										configuration.Password,
										configuration.AuthenticationType)){
			de.MoveTo(ouRnD);
			}
			try {
				using (DirectoryEntry barakTsabariDE = new DirectoryEntry(barakTsabariDN,
													configuration.Username,
													configuration.Password,
													configuration.AuthenticationType)){
				barakTsabariDE.Properties["telephoneNumber"].Value = newTelephoneNumber;
				barakTsabariDE.CommitChanges();
				Assert.Fail("Object " + barakTsabariDN + " was not moved from old location on the server.");
				}
			}
			catch(AssertionException ae) {
				throw ae;
			}
			catch (Exception e) {
				// do nothing
			}


			using (DirectoryEntry barakTsabariDE = new DirectoryEntry(configuration.ServerRoot + "cn=Barak Tsabari,ou=R&D,ou=people" + ((configuration.BaseDn.Length == 0) ? String.Empty : ("," + configuration.BaseDn)),
												configuration.Username,
												configuration.Password,
												configuration.AuthenticationType)){
			Assert.AreEqual(barakTsabariDE.Properties["telephoneNumber"].Value,oldTelephoneNumber);
			}
			

			// restore object state
			using (DirectoryEntry ouHumanResources = new DirectoryEntry(	configuration.ServerRoot + "ou=Human Resources,ou=people" + ((configuration.BaseDn.Length == 0) ? String.Empty : ("," + configuration.BaseDn)),
																	configuration.Username,
																	configuration.Password,
																	configuration.AuthenticationType)){
			using (DirectoryEntry barakTsabariDE = new DirectoryEntry(configuration.ServerRoot + "cn=Barak Tsabari,ou=R&D,ou=people" + ((configuration.BaseDn.Length == 0) ? String.Empty : ("," + configuration.BaseDn)),
												configuration.Username,
												configuration.Password,
												configuration.AuthenticationType)){
			barakTsabariDE.MoveTo(ouHumanResources);
			barakTsabariDE.CommitChanges();
			}
			}

			#endregion // Check MoveTo

			#region Check Rename

			// Rename not chached
			de = new DirectoryEntry(barakTsabariDN,
									configuration.Username,
									configuration.Password,
									configuration.AuthenticationType);

			de.Rename("cn=MyUser");

			try {
				using (DirectoryEntry barakTsabariDE = new DirectoryEntry(barakTsabariDN,
													configuration.Username,
													configuration.Password,
													configuration.AuthenticationType)){
				barakTsabariDE.Properties["telephoneNumber"].Value = newTelephoneNumber;
				barakTsabariDE.CommitChanges();
				Assert.Fail("Object " + barakTsabariDN + " was not renamed on the server.");
				}
			}
			catch(AssertionException ae) {
				throw ae;
			}
			catch (Exception e) {
				// do nothing
			}

			using (DirectoryEntry barakTsabariDE = new DirectoryEntry(configuration.ServerRoot + "cn=MyUser,ou=Human Resources,ou=people" + ((configuration.BaseDn.Length == 0) ? String.Empty : ("," + configuration.BaseDn)),
												configuration.Username,
												configuration.Password,
												configuration.AuthenticationType)){
			Assert.AreEqual(barakTsabariDE.Properties["telephoneNumber"].Value,oldTelephoneNumber);
			}

			// restore object state
			using (DirectoryEntry barakTsabariDE = new DirectoryEntry(configuration.ServerRoot + "cn=MyUser,ou=Human Resources,ou=people" + ((configuration.BaseDn.Length == 0) ? String.Empty : ("," + configuration.BaseDn)),
												configuration.Username,
												configuration.Password,
												configuration.AuthenticationType)){
			barakTsabariDE.Rename("cn=Barak Tsabari");
			barakTsabariDE.CommitChanges();
			}

			#endregion // Check Rename
		}

		[Test]
		[Category("NotWorking")]
		public void DirectoryEntry_Children()
		{
			de = new DirectoryEntry();
			DirectoryEntries children = de.Children;
			//Assert.AreEqual(children.SchemaFilter.Count,0);	


			de = new DirectoryEntry(configuration.ConnectionString);
			children = de.Children;

			//Assert.AreEqual(children.SchemaFilter.Count,0);

			int childrenCount = 0;
			foreach(DirectoryEntry childDe in children) {
				childrenCount++;
			}
			Assert.AreEqual(childrenCount,2);
			Assert.AreEqual(children.Find("ou=people").Name,"ou=people");
			Assert.AreEqual(children.Find("cn=Manager").Name,"cn=Manager");


			de = new DirectoryEntry(configuration.ConnectionString,
									configuration.Username,
									configuration.Password,
									configuration.AuthenticationType);
			children = de.Children;

			//Assert.AreEqual(children.SchemaFilter.Count,0);

			childrenCount = 0;
			foreach(DirectoryEntry childDe in children) {
				childrenCount++;
			}
			Assert.AreEqual(childrenCount,2);
			Assert.AreEqual(children.Find("ou=people").Name,"ou=people");
			Assert.AreEqual(children.Find("cn=Manager").Name,"cn=Manager");

			de = new DirectoryEntry(configuration.ServerRoot + "ou=Human Resources,ou=people" + ((configuration.BaseDn.Length == 0) ? String.Empty : ("," + configuration.BaseDn)) ,
									configuration.Username,
									configuration.Password,
									configuration.AuthenticationType);
			children = de.Children;

			Assert.AreEqual(children.Find("cn=Barak Tsabari").Name,"cn=Barak Tsabari");
			Assert.AreEqual(children.Find("cn=John Smith").Name,"cn=John Smith");
		}

		[Test]
		public void DirectoryEntry_Name()
		{
			de = new DirectoryEntry(configuration.ConnectionString);

			de = new DirectoryEntry(configuration.ConnectionString,
									configuration.Username,
									configuration.Password,
									configuration.AuthenticationType);

			de = new DirectoryEntry(configuration.ServerRoot + "ou=Human Resources,ou=people" + ((configuration.BaseDn.Length == 0) ? String.Empty : ("," + configuration.BaseDn)) ,
									configuration.Username,
									configuration.Password,
									configuration.AuthenticationType);
			Assert.AreEqual(de.Name,"ou=Human Resources");

			de = new DirectoryEntry(configuration.ServerRoot + "cn=Barak Tsabari,ou=Human Resources,ou=people" + ((configuration.BaseDn.Length == 0) ? String.Empty : ("," + configuration.BaseDn)) ,
									configuration.Username,
									configuration.Password,
									configuration.AuthenticationType);
			Assert.AreEqual(de.Name,"cn=Barak Tsabari");
		}
		

		[Test]
		public void DirectoryEntry_Parent()
		{
			de = new DirectoryEntry(configuration.ConnectionString);

			// MS works only with "LDAP" while RFC2255 states "ldap"
			Assert.AreEqual(de.Parent.Path.ToLower(),(configuration.ServerRoot + GetParentDN (configuration.BaseDn)).ToLower());

			de = new DirectoryEntry(configuration.ConnectionString,
									configuration.Username,
									configuration.Password,
									configuration.AuthenticationType);

			// MS works only with "LDAP" while RFC2255 states "ldap"
			Assert.AreEqual(de.Parent.Path.ToLower(),(configuration.ServerRoot + GetParentDN (configuration.BaseDn)).ToLower());

			de = new DirectoryEntry(configuration.ServerRoot + "cn=Barak Tsabari,ou=Human Resources,ou=people" + ((configuration.BaseDn.Length == 0) ? String.Empty : ("," + configuration.BaseDn)) ,
									configuration.Username,
									configuration.Password,
									configuration.AuthenticationType);

			// MS works only with "LDAP" while RFC2255 states "ldap"
			Assert.AreEqual(de.Parent.Path.ToLower(),(configuration.ServerRoot + "ou=Human Resources,ou=people" + ((configuration.BaseDn.Length == 0) ? String.Empty : ("," + configuration.BaseDn))).ToLower());
		}


		[Test]
		public void DirectoryEntry_Path()
		{
			string wrongPath = "something that is not LDAP path";

			de = new DirectoryEntry();

			Assert.AreEqual(de.Path,String.Empty);

			de.Path = configuration.ConnectionString;
			Assert.AreEqual(de.Path,configuration.ConnectionString);

			de.Path = "";
			Assert.AreEqual(de.Path,String.Empty);
			
			de.Path = wrongPath;
			Assert.AreEqual(de.Path,wrongPath);


			de = new DirectoryEntry(configuration.ConnectionString);

			de.Path = configuration.ConnectionString;
			Assert.AreEqual(de.Path,configuration.ConnectionString);

			de.Path = "";
			Assert.AreEqual(de.Path,String.Empty);

			de.Path = wrongPath;
			Assert.AreEqual(de.Path,wrongPath);

			
			de = new DirectoryEntry(configuration.ConnectionString,
									configuration.Username,
									configuration.Password,
									configuration.AuthenticationType);

			de.Path = configuration.ConnectionString;
			Assert.AreEqual(de.Path,configuration.ConnectionString);

			de.Path = "";
			Assert.AreEqual(de.Path,String.Empty);

			de.Path = wrongPath;
			Assert.AreEqual(de.Path,wrongPath);

			de = new DirectoryEntry("ldap://myhost:389/ou=people",null,null,AuthenticationTypes.None);
			Assert.AreEqual(de.Path,"ldap://myhost:389/ou=people");

			de.Path = null;
			Assert.AreEqual(de.Path,String.Empty);
		}


		[Test]
		public void DirectoryEntry_Properties1()
		{
			de = new DirectoryEntry(configuration.ConnectionString);

			Assert.AreEqual(de.Properties.Count,3);
			Assert.AreEqual(((PropertyValueCollection)de.Properties["dc"]).Value,"example");
			Assert.AreEqual(((PropertyValueCollection)de.Properties["description"]).Value,null);

			
			de = new DirectoryEntry(configuration.ConnectionString,
									configuration.Username,
									configuration.Password,
									configuration.AuthenticationType);

			Assert.AreEqual(de.Properties.Count,3);
			Assert.AreEqual(((PropertyValueCollection)de.Properties["dc"]).Value,"example");
			Assert.AreEqual(((PropertyValueCollection)de.Properties["description"]).Value,null);

			// ensure that properties are not accessible after removing an entry from the server
			string barakTsabariDN = configuration.ServerRoot + "cn=Barak Tsabari,ou=Human Resources,ou=people" + ((configuration.BaseDn.Length == 0) ? String.Empty : ("," + configuration.BaseDn));
			de = new DirectoryEntry(barakTsabariDN,
									configuration.Username,
									configuration.Password,
									configuration.AuthenticationType);
			
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
			string barakTsabariDN = configuration.ServerRoot + "cn=Barak Tsabari,ou=Human Resources,ou=people" + ((configuration.BaseDn.Length == 0) ? String.Empty : ("," + configuration.BaseDn));
			de = new DirectoryEntry(barakTsabariDN,
									configuration.Username,
									configuration.Password,
									configuration.AuthenticationType);

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
			using (DirectoryEntry ouHumanResources = new DirectoryEntry(	configuration.ServerRoot + "ou=Human Resources,ou=people" + ((configuration.BaseDn.Length == 0) ? String.Empty : ("," + configuration.BaseDn)),
																	configuration.Username,
																	configuration.Password,
																	configuration.AuthenticationType)){
			using (DirectoryEntry cnBarakTsabari = ouHumanResources.Children.Add("cn=Barak Tsabari","Class")){
			((PropertyValueCollection)cnBarakTsabari.Properties["objectClass"]).Add("person");
			((PropertyValueCollection)cnBarakTsabari.Properties["objectClass"]).Add("organizationalPerson");
			cnBarakTsabari.Properties["cn"].Value = "Barak Tsabari";
			cnBarakTsabari.Properties["facsimileTelephoneNumber"].Value = "+1 906 777 8853";
			((PropertyValueCollection)cnBarakTsabari.Properties["ou"]).Add("Human Resources");
			((PropertyValueCollection)cnBarakTsabari.Properties["ou"]).Add("People");
			cnBarakTsabari.Properties["sn"].Value = "Tsabari";
			cnBarakTsabari.Properties["telephoneNumber"].Value = "+1 906 777 8854";
			cnBarakTsabari.CommitChanges();
			}
			}
			
			// the local property chache is still accessible
			Assert.AreEqual(((PropertyValueCollection)de.Properties["sn"]).Value,"Barbari");

			// Refresh from server
			de.RefreshCache();
			// ensure the properties of an entry are still accessible through the old object
			Assert.AreEqual(((PropertyValueCollection)de.Properties["sn"]).Value,"Tsabari");

		}


		[Test]
		[Category("NotWorking")]
		public void DirectoryEntry_SchemaClassName()
		{
			de = new DirectoryEntry();
			Assert.AreEqual(de.SchemaClassName,"domainDNS");


			de = new DirectoryEntry(configuration.ConnectionString);
			Assert.AreEqual(de.SchemaClassName,"organization");


			de = new DirectoryEntry(configuration.ConnectionString,
									configuration.Username,
									configuration.Password,
									configuration.AuthenticationType);
			Assert.AreEqual(de.SchemaClassName,"organization");

			DirectoryEntry de2 = de.Children.Add("ou=My Child","Class");
			Assert.AreEqual(de2.SchemaClassName,"Class");
			Assert.AreEqual(((PropertyValueCollection)de2.Properties["structuralObjectClass"]).Value,null);
		}

		[Test]
		[Category("NotWorking")]
		public void DirectoryEntry_SchemaEntry()
		{
			de = new DirectoryEntry();
			DirectoryEntry schemaEntry = de.SchemaEntry;

			// MS works only with "LDAP" while RFC2255 states "ldap"
			Assert.AreEqual(schemaEntry.Path.ToLower(),"LDAP://schema/domainDNS".ToLower());
			Assert.AreEqual(schemaEntry.Name,"domainDNS");
			Assert.AreEqual(schemaEntry.Username,null);
			Assert.AreEqual(schemaEntry.UsePropertyCache,true);
			Assert.AreEqual(schemaEntry.SchemaClassName,"Class");
			Assert.AreEqual(schemaEntry.AuthenticationType,AuthenticationTypes.None);


			de = new DirectoryEntry(configuration.ConnectionString);
			schemaEntry = de.SchemaEntry;

			Assert.AreEqual(schemaEntry.Path,configuration.ServerRoot + "schema/organization");
			Assert.AreEqual(schemaEntry.Name,"organization");
			Assert.AreEqual(schemaEntry.Username,null);
			Assert.AreEqual(schemaEntry.UsePropertyCache,true);
			Assert.AreEqual(schemaEntry.SchemaClassName,"Class");
			Assert.AreEqual(schemaEntry.AuthenticationType,AuthenticationTypes.None);


			de = new DirectoryEntry(configuration.ConnectionString,
									configuration.Username,
									configuration.Password,
									configuration.AuthenticationType);
			schemaEntry = de.SchemaEntry;

			Assert.AreEqual(schemaEntry.Path,configuration.ServerRoot + "schema/organization");
			Assert.AreEqual(schemaEntry.Name,"organization");
			Assert.AreEqual(schemaEntry.Username,configuration.Username);
			Assert.AreEqual(schemaEntry.UsePropertyCache,true);
			Assert.AreEqual(schemaEntry.SchemaClassName,"Class");
			Assert.AreEqual(schemaEntry.AuthenticationType,configuration.AuthenticationType);
		}		


		[Test]
		public void DirectoryEntry_Username()
		{
			string wrongUsername = "some wrong username";

			de = new DirectoryEntry();

			Assert.AreEqual(de.Username,null);

			de.Username = configuration.Username;
			Assert.AreEqual(de.Username,configuration.Username);

			de.Username = "";
			Assert.AreEqual(de.Username,String.Empty);
			
			de.Username = wrongUsername;
			Assert.AreEqual(de.Username,wrongUsername);


			de = new DirectoryEntry(configuration.ConnectionString);

			de.Username = configuration.Username;
			Assert.AreEqual(de.Username,configuration.Username);

			de.Username = "";
			Assert.AreEqual(de.Username,String.Empty);

			de.Username = wrongUsername;
			Assert.AreEqual(de.Username,wrongUsername);

			
			de = new DirectoryEntry(configuration.ConnectionString,
									configuration.Username,
									configuration.Password,
									configuration.AuthenticationType);

			de.Username = configuration.Username;
			Assert.AreEqual(de.Username,configuration.Username);

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

			de = new DirectoryEntry(configuration.ConnectionString);
			de.Close();
	
			de = new DirectoryEntry(configuration.ConnectionString,
									configuration.Username,
									configuration.Password,
									configuration.AuthenticationType);
			de.Close();
		}


		[Test]
		public void DirectoryEntry_CommitChanges1()
		{
			string humanResourcesDN = configuration.ServerRoot + "ou=Human Resources,ou=people" + ((configuration.BaseDn.Length == 0) ? String.Empty : ("," + configuration.BaseDn));
			using (DirectoryEntry ouHumanResources = new DirectoryEntry(	humanResourcesDN,
																	configuration.Username,
																	configuration.Password,
																	configuration.AuthenticationType)){

			// new entry
			string newEmployeeDN = configuration.ServerRoot + "cn=New Employee,ou=Human Resources,ou=people" + ((configuration.BaseDn.Length == 0) ? String.Empty : ("," + configuration.BaseDn));
			de = ouHumanResources.Children.Add("cn=New Employee","Class");
			Assert.IsFalse(DirectoryEntry.Exists(newEmployeeDN));

			de.Properties["objectClass"].Value = "organizationalRole";
			de.Properties["cn"].Value = "New Employee";
			Assert.IsFalse(DirectoryEntry.Exists(newEmployeeDN));
			
			de.CommitChanges();
			Assert.IsTrue(DirectoryEntry.Exists(newEmployeeDN));
			}

			// existing entry
			string barakTsabariDN = configuration.ServerRoot + "cn=Barak Tsabari,ou=Human Resources,ou=people" + ((configuration.BaseDn.Length == 0) ? String.Empty : ("," + configuration.BaseDn));
			de = new DirectoryEntry(barakTsabariDN,
									configuration.Username,
									configuration.Password,
									configuration.AuthenticationType);

			string oldTelephone = (string)((PropertyValueCollection)de.Properties["telephoneNumber"]).Value;
			string newTelephone = "+972 3 6078596";

			// UsePropertyCache - true
			de.UsePropertyCache = true;
			((PropertyValueCollection)de.Properties["telephoneNumber"]).Value = newTelephone;
			Assert.AreEqual(((PropertyValueCollection)de.Properties["telephoneNumber"]).Value,newTelephone);

			using (DirectoryEntry cnBarakTsabari = new DirectoryEntry(	barakTsabariDN,
																configuration.Username,
																configuration.Password,
																configuration.AuthenticationType)){

			//check that on server there is still an old value
			Assert.AreEqual(((PropertyValueCollection)cnBarakTsabari.Properties["telephoneNumber"]).Value,oldTelephone);

			de.CommitChanges();
			}

			using (DirectoryEntry cnBarakTsabari = new DirectoryEntry(barakTsabariDN,
												configuration.Username,
												configuration.Password,
												configuration.AuthenticationType)){

			// check that new value is updated on the server
			Assert.AreEqual(((PropertyValueCollection)cnBarakTsabari.Properties["telephoneNumber"]).Value,newTelephone);
			}

			// UsePropertyCache - false
			de = new DirectoryEntry(barakTsabariDN,
									configuration.Username,
									configuration.Password,
									configuration.AuthenticationType);
			de.UsePropertyCache = false;
			Assert.AreEqual(((PropertyValueCollection)de.Properties["telephoneNumber"]).Value,newTelephone);
			((PropertyValueCollection)de.Properties["telephoneNumber"]).Value = oldTelephone;
			Assert.AreEqual(((PropertyValueCollection)de.Properties["telephoneNumber"]).Value,oldTelephone);

			using (DirectoryEntry cnBarakTsabari = new DirectoryEntry(barakTsabariDN,
												configuration.Username,
												configuration.Password,
												configuration.AuthenticationType)){

			// check that new value is updated on the server
			//Assert.AreEqual(((PropertyValueCollection)cnBarakTsabari.Properties["telephoneNumber"]).Value,oldTelephone);
			}

			de.CommitChanges(); // this should do nothing
		}

		[Test]
		public void DirectoryEntry_CommitChanges2()
		{
			string barakTsabariDN = configuration.ServerRoot + "cn=Barak Tsabari,ou=Human Resources,ou=people" + ((configuration.BaseDn.Length == 0) ? String.Empty : ("," + configuration.BaseDn));
			using (DirectoryEntry barakTsabariDE1 = new DirectoryEntry(barakTsabariDN,
																configuration.Username,
																configuration.Password,
																configuration.AuthenticationType)){
			barakTsabariDE1.UsePropertyCache = true;

			using (DirectoryEntry barakTsabariDE2 = new DirectoryEntry(barakTsabariDN,
																configuration.Username,
																configuration.Password,
																configuration.AuthenticationType)){
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
									configuration.Username,
									configuration.Password,
									configuration.AuthenticationType);
			Assert.AreEqual(de.Properties["telephoneNumber"].Value,newTelephone);
			Assert.AreEqual(de.Properties["facsimileTelephoneNumber"].Value,newFacsimilieTelephoneNumber1);

			barakTsabariDE2.CommitChanges();
			de = new DirectoryEntry(barakTsabariDN,
									configuration.Username,
									configuration.Password,
									configuration.AuthenticationType);
			Assert.AreEqual(de.Properties["telephoneNumber"].Value,newTelephone);
			Assert.AreEqual(de.Properties["facsimileTelephoneNumber"].Value,newFacsimilieTelephoneNumber2);
			}
			}
		}


		[Test]
		[ExpectedException(typeof(NotImplementedException))]
		public void DirectoryEntry_CopyTo()
		{
			string barakTsabariDN = configuration.ServerRoot + "cn=Barak Tsabari,ou=Human Resources,ou=people" + ((configuration.BaseDn.Length == 0) ? String.Empty : ("," + configuration.BaseDn));
			de = new DirectoryEntry(configuration.ConnectionString,
									configuration.Username,
									configuration.Password,
									configuration.AuthenticationType);

			using (DirectoryEntry cnBarakTsabari = new DirectoryEntry(	barakTsabariDN,
																configuration.Username,
																configuration.Password,
																configuration.AuthenticationType)){

			cnBarakTsabari.CopyTo(de);
			}
		}


		[Test]
		public void DirectoryEntry_DeleteTree()
		{
			string barakTsabariDN = configuration.ServerRoot + "cn=Barak Tsabari,ou=Human Resources,ou=people" + ((configuration.BaseDn.Length == 0) ? String.Empty : ("," + configuration.BaseDn));

			Assert.IsTrue(DirectoryEntry.Exists(barakTsabariDN));
			de = new DirectoryEntry(barakTsabariDN,
									configuration.Username,
									configuration.Password,
									configuration.AuthenticationType);						

			// no properties changed
			de.DeleteTree();
			de.CommitChanges();

			Assert.IsFalse(DirectoryEntry.Exists(barakTsabariDN));

			string johnSmithDN = configuration.ServerRoot + "cn=John Smith,ou=Human Resources,ou=people" + ((configuration.BaseDn.Length == 0) ? String.Empty : ("," + configuration.BaseDn));

			Assert.IsTrue(DirectoryEntry.Exists(johnSmithDN));
			de = new DirectoryEntry(johnSmithDN,
									configuration.Username,
									configuration.Password,
									configuration.AuthenticationType);

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
				// do nothing
			}
		}

		[Test]
		public void DirectoryEntry_DeleteTree2()
		{
			string johnSmithDN = configuration.ServerRoot + "cn=John Smith,ou=Human Resources,ou=people" + ((configuration.BaseDn.Length == 0) ? String.Empty : ("," + configuration.BaseDn));

			Assert.IsTrue(DirectoryEntry.Exists(johnSmithDN));
			// two objects refer to the same entry
			de = new DirectoryEntry(johnSmithDN,
									configuration.Username,
									configuration.Password,
									configuration.AuthenticationType);

			using (DirectoryEntry johnSmithDE = new DirectoryEntry(johnSmithDN,
															configuration.Username,
															configuration.Password,
															configuration.AuthenticationType)){

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
		}


		[Test]
		public void DirectoryEntry_Exists()
		{
			string barakTsabariDN = configuration.ServerRoot + "cn=Barak Tsabari,ou=Human Resources,ou=people" + ((configuration.BaseDn.Length == 0) ? String.Empty : ("," + configuration.BaseDn));
			string johnSmithDN = configuration.ServerRoot + "cn=John Smith,ou=Human Resources,ou=people" + ((configuration.BaseDn.Length == 0) ? String.Empty : ("," + configuration.BaseDn));
			string humanResourcesOU = configuration.ServerRoot + "ou=Human Resources,ou=people" + ((configuration.BaseDn.Length == 0) ? String.Empty : ("," + configuration.BaseDn));

			Assert.IsTrue(DirectoryEntry.Exists(barakTsabariDN));
			Assert.IsTrue(DirectoryEntry.Exists(johnSmithDN));
			Assert.IsTrue(DirectoryEntry.Exists(humanResourcesOU));

			Assert.IsFalse(DirectoryEntry.Exists(barakTsabariDN + ",dc=mono"));
		}


		[Test]
		public void DirectoryEntry_MoveTo_De()
		{
			string barakTsabariHumanResourcesDN = configuration.ServerRoot + "cn=Barak Tsabari,ou=Human Resources,ou=people" + ((configuration.BaseDn.Length == 0) ? String.Empty : ("," + configuration.BaseDn));
			string barakTsabariDevQaDN = configuration.ServerRoot + "cn=Barak Tsabari,ou=DevQA,ou=people" + ((configuration.BaseDn.Length == 0) ? String.Empty : ("," + configuration.BaseDn));

			using (DirectoryEntry barakTsabariDE = new DirectoryEntry(	barakTsabariHumanResourcesDN,
																configuration.Username,
																configuration.Password,
																configuration.AuthenticationType)){

			string devQaOU = configuration.ServerRoot + "ou=DevQA,ou=people" + ((configuration.BaseDn.Length == 0) ? String.Empty : ("," + configuration.BaseDn));

			using (DirectoryEntry devQaDE = new DirectoryEntry(devQaOU,
														configuration.Username,
														configuration.Password,
														configuration.AuthenticationType)){

			barakTsabariDE.MoveTo(devQaDE);
			barakTsabariDE.CommitChanges();

			Assert.IsTrue(DirectoryEntry.Exists(barakTsabariDevQaDN));

			string humanRwsourcesOU = configuration.ServerRoot + "ou=Human Resources,ou=people" + ((configuration.BaseDn.Length == 0) ? String.Empty : ("," + configuration.BaseDn));

			using (DirectoryEntry humanResourcesDE = new DirectoryEntry(	humanRwsourcesOU,
																	configuration.Username,
																	configuration.Password,
																	configuration.AuthenticationType)){

			barakTsabariDE.MoveTo(humanResourcesDE);
			barakTsabariDE.CommitChanges();

			Assert.IsTrue(DirectoryEntry.Exists(barakTsabariHumanResourcesDN));
			}
			}
			}
		}


		[Test]
		public void DirectoryEntry_MoveTo_DeStr()
		{
			string barakTsabariHumanResourcesDN = configuration.ServerRoot + "cn=Barak Tsabari,ou=Human Resources,ou=people" + ((configuration.BaseDn.Length == 0) ? String.Empty : ("," + configuration.BaseDn));
			string barakTsabariDevQaDN = configuration.ServerRoot + "cn=My Name,ou=DevQA,ou=people" + ((configuration.BaseDn.Length == 0) ? String.Empty : ("," + configuration.BaseDn));

			using (DirectoryEntry barakTsabariDE = new DirectoryEntry(	barakTsabariHumanResourcesDN,
																configuration.Username,
																configuration.Password,
																configuration.AuthenticationType)){

			string devQaOU = configuration.ServerRoot + "ou=DevQA,ou=people" + ((configuration.BaseDn.Length == 0) ? String.Empty : ("," + configuration.BaseDn));

			using (DirectoryEntry devQaDE = new DirectoryEntry(devQaOU,
														configuration.Username,
														configuration.Password,
														configuration.AuthenticationType)){

			barakTsabariDE.MoveTo(devQaDE,"cn=My Name");
			barakTsabariDE.CommitChanges();

			Assert.IsTrue(DirectoryEntry.Exists(barakTsabariDevQaDN));

			string humanRwsourcesOU = configuration.ServerRoot + "ou=Human Resources,ou=people" + ((configuration.BaseDn.Length == 0) ? String.Empty : ("," + configuration.BaseDn));

			using (DirectoryEntry humanResourcesDE = new DirectoryEntry(	humanRwsourcesOU,
																	configuration.Username,
																	configuration.Password,
																	configuration.AuthenticationType)){

			barakTsabariDE.MoveTo(humanResourcesDE,"cn=Barak Tsabari");
			barakTsabariDE.CommitChanges();

			Assert.IsTrue(DirectoryEntry.Exists(barakTsabariHumanResourcesDN));
			}
			}
			}
		}

		[Test]
		public void DirectoryEntry_RefreshCache()
		{
			de = new DirectoryEntry(configuration.ConnectionString);
			de.UsePropertyCache = true;
			
			string newValue = "Just a company";
			string oldValue = (string)((PropertyValueCollection)de.Properties["description"]).Value;
			((PropertyValueCollection)de.Properties["description"]).Value = newValue;
			
			Assert.AreEqual(((PropertyValueCollection)de.Properties["description"]).Value,newValue);

			de.RefreshCache();

			Assert.AreEqual(((PropertyValueCollection)de.Properties["description"]).Value,oldValue);
			
			// call RefeshCache on new entry prior to submitting it to the server shoud fail
			string newEmployeeDN = configuration.ServerRoot + "cn=New Employee,ou=Human Resources,ou=people" + ((configuration.BaseDn.Length == 0) ? String.Empty : ("," + configuration.BaseDn));
			string humanResourcesDN = configuration.ServerRoot + "ou=Human Resources,ou=people" + ((configuration.BaseDn.Length == 0) ? String.Empty : ("," + configuration.BaseDn));

			using (DirectoryEntry humanResourcesDE = new DirectoryEntry(	humanResourcesDN,
																	configuration.Username,
																	configuration.Password,
																	configuration.AuthenticationType)){

			using (DirectoryEntry newEmployeeDE = humanResourcesDE.Children.Add("cn=New Employee","Class")){
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
			}
		}

		[Test]
		public void DirectoryEntry_RefreshCache_StrArr()
		{			
			de = new DirectoryEntry(configuration.ServerRoot + "cn=Uzi Cohen,cn=Manager" + ((configuration.BaseDn.Length == 0) ? String.Empty : ("," + configuration.BaseDn)));
			de.UsePropertyCache = true;
			
			string[] newValues = new string [] { "Just a manager", "Levi" };
			string[] oldValues = new string [2];
			oldValues [0] = (string)((PropertyValueCollection)de.Properties["description"]).Value;
			oldValues [1] = (string)((PropertyValueCollection)de.Properties["sn"]).Value;
			
			((PropertyValueCollection)de.Properties["description"]).Value = newValues [0];
			((PropertyValueCollection)de.Properties["sn"]).Value = newValues [1];
			
			Assert.AreEqual(((PropertyValueCollection)de.Properties["description"]).Value,newValues [0]);
			Assert.AreEqual(((PropertyValueCollection)de.Properties["sn"]).Value,newValues [1]);

			de.RefreshCache(new string[] {"cn"});

			Assert.AreEqual(((PropertyValueCollection)de.Properties["description"]).Value,newValues [0]);
			Assert.AreEqual(((PropertyValueCollection)de.Properties["sn"]).Value,newValues [1]);

			de.RefreshCache(new string[] {"description","sn"});

			Assert.AreEqual(((PropertyValueCollection)de.Properties["description"]).Value,newValues [0]);
			Assert.AreEqual(((PropertyValueCollection)de.Properties["sn"]).Value,oldValues [1]);	
		}

		[Test]
		public void DirectoryEntry_Rename()
		{
			string barakTsabariOldDN = configuration.ServerRoot + "cn=Barak Tsabari,ou=Human Resources,ou=people" + ((configuration.BaseDn.Length == 0) ? String.Empty : ("," + configuration.BaseDn));
			string barakTsabariNewDN = configuration.ServerRoot + "cn=My Name,ou=Human Resources,ou=people" + ((configuration.BaseDn.Length == 0) ? String.Empty : ("," + configuration.BaseDn));

			using (DirectoryEntry barakTsabariDE = new DirectoryEntry(	barakTsabariOldDN,
																configuration.Username,
																configuration.Password,
																configuration.AuthenticationType)){

			barakTsabariDE.Rename("cn=My Name");
			barakTsabariDE.CommitChanges();

			Assert.IsTrue(DirectoryEntry.Exists(barakTsabariNewDN));

			barakTsabariDE.Rename("cn=Barak Tsabari");
			barakTsabariDE.CommitChanges();

			Assert.IsTrue(DirectoryEntry.Exists(barakTsabariOldDN));
			}
		}

		#endregion Tests

		private static string GetName (string baseDn)
		{
			if (baseDn == null || baseDn.Length == 0 || baseDn.IndexOf (',') == -1)
				return baseDn;

			int index = baseDn.IndexOf (',');

			return baseDn.Substring (0, index);
		}

		private static string GetParentDN (string baseDn)
		{
			if (baseDn == null || baseDn.Length == 0 || baseDn.IndexOf (',') == -1)
				return String.Empty;

			int index = baseDn.IndexOf (',');

			return baseDn.Substring (index + 1,baseDn.Length - index - 1);
		}
	}
}
