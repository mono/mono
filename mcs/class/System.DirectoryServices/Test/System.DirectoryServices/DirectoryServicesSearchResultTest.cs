//
// DirectoryServicesSearchResultTest.cs -
//	NUnit Test Cases for DirectoryServices.SearchResult
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
	public class DirectoryServicesSearchResultTest
	{
		#region Fields

		static TestConfiguration configuration;
		static DirectoryEntry de;
		static DirectorySearcher ds;

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
			if (ds != null)
				ds.Dispose ();
			
			ds = null;
			
			if (de != null)
				de.Dispose ();

			de = null;

			using (DirectoryEntry root = new DirectoryEntry(	configuration.ConnectionString,
														configuration.Username,
														configuration.Password,
														configuration.AuthenticationType)){
			
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
		public void SearchResult_Path()
		{
			de = new DirectoryEntry(configuration.ConnectionString,
									configuration.Username,
									configuration.Password,
									configuration.AuthenticationType);

			ds = new DirectorySearcher(de);

			SearchResultCollection results = ds.FindAll();

			// MS works only with "LDAP" while RFC2255 states "ldap"
			Assert.AreEqual(results[0].Path.ToLower(),(configuration.ServerRoot + configuration.BaseDn).ToLower());
			Assert.AreEqual(results[0].Path,results[0].GetDirectoryEntry().Path);

			// MS works only with "LDAP" while RFC2255 states "ldap"
			Assert.AreEqual(results[1].Path.ToLower(),(configuration.ServerRoot + "ou=people" + ((configuration.BaseDn.Length == 0) ? String.Empty : ("," + configuration.BaseDn))).ToLower());
			Assert.AreEqual(results[1].Path,results[1].GetDirectoryEntry().Path);

			// MS works only with "LDAP" while RFC2255 states "ldap"
			Assert.AreEqual(results[2].Path.ToLower(),(configuration.ServerRoot + "ou=Human Resources,ou=people" + ((configuration.BaseDn.Length == 0) ? String.Empty : ("," + configuration.BaseDn))).ToLower());
			Assert.AreEqual(results[2].Path,results[2].GetDirectoryEntry().Path);

			// MS works only with "LDAP" while RFC2255 states "ldap"
			Assert.AreEqual(results[3].Path.ToLower(),(configuration.ServerRoot + "cn=John Smith,ou=Human Resources,ou=people" + ((configuration.BaseDn.Length == 0) ? String.Empty : ("," + configuration.BaseDn))).ToLower());
			Assert.AreEqual(results[3].Path,results[3].GetDirectoryEntry().Path);
		}

		[Test]
		public void SearchResult_Properties()
		{
			de = new DirectoryEntry(configuration.ConnectionString,
									configuration.Username,
									configuration.Password,
									configuration.AuthenticationType);
			ds = new DirectorySearcher(de);

			ds.PropertiesToLoad.Add("cn");
			ds.PropertiesToLoad.Add("ADsPath");
			ds.PropertiesToLoad.Add("objectClass");

			Assert.AreEqual(ds.PropertiesToLoad.Count,3);
			Assert.IsTrue(ds.PropertiesToLoad.Contains("cn"));
			Assert.IsTrue(ds.PropertiesToLoad.Contains("ADsPath"));
			Assert.IsTrue(ds.PropertiesToLoad.Contains("objectClass"));

			ds.Filter = "(objectClass=person)";
			SearchResult result = ds.FindOne();

			Assert.AreEqual(result.Properties.Count,3);
			Assert.IsTrue(result.Properties.Contains("cn"));
			Assert.IsTrue(result.Properties.Contains("objectClass"));
			Assert.IsTrue(result.Properties.Contains("ADsPath"));

			ds.PropertiesToLoad.Clear();

			ds.PropertiesToLoad.Add("cn");
			ds.PropertiesToLoad.Add("objectClass");
			ds.PropertiesToLoad.Add("missingProperty");

			Assert.AreEqual(ds.PropertiesToLoad.Count,3);
			Assert.IsTrue(ds.PropertiesToLoad.Contains("cn"));
			Assert.IsTrue(ds.PropertiesToLoad.Contains("objectClass"));
			Assert.IsTrue(ds.PropertiesToLoad.Contains("missingProperty"));

			ds.Filter = "(objectClass=person)";
			result = ds.FindOne();

			// Properties that do not exist are not loaded
			Assert.AreEqual(result.Properties.Count,3);
			Assert.IsTrue(result.Properties.Contains("cn"));
			Assert.IsTrue(result.Properties.Contains("objectClass"));
			Assert.IsTrue(result.Properties.Contains("ADsPath"));

			Assert.AreEqual(((ResultPropertyValueCollection)result.Properties["cn"])[0],"Barak Tsabari");
			Assert.AreEqual(((ResultPropertyValueCollection)result.Properties["objectClass"])[0],"person");
			// MS works only with "LDAP" while RFC2255 states "ldap"
			Assert.AreEqual(((string)((ResultPropertyValueCollection)result.Properties["AdsPath"])[0]).ToLower(),(configuration.ServerRoot + "cn=Barak Tsabari,ou=Human Resources,ou=people" + ((configuration.BaseDn.Length == 0) ? String.Empty : ("," + configuration.BaseDn))).ToLower());
		}

		#endregion Tests
	}
}
