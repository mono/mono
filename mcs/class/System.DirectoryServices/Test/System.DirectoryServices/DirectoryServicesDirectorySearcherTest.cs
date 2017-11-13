//
// DirectoryServicesDirectorySearcherTest.cs -
//	NUnit Test Cases for DirectoryServices.DirectorySearcher
//
// Author:
//	Boris Kirzner  <borisk@mainsoft.com>
//
using NUnit.Framework;
using System;
using System.DirectoryServices;
using System.Collections;

using System.Threading;

namespace MonoTests.System.DirectoryServices 
{
	[TestFixture]
	[Category ("InetAccess")]
	public class DirectoryServicesDirectorySearcherTest
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
			if (ds != null)
				ds.Dispose ();
			
			ds = null;
			
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
		public void DirectorySearcher_DirectorySearcher()
		{
			ds = new DirectorySearcher();

			Assert.AreEqual(ds.Filter,"(objectClass=*)");
			Assert.AreEqual(ds.PropertiesToLoad.Count,0);
			Assert.AreEqual(ds.SearchScope,SearchScope.Subtree);
			Assert.AreEqual(ds.CacheResults,true);
			Assert.AreEqual(ds.ClientTimeout,new TimeSpan(-10000000));
		}

		[Test]
		public void DirectorySearcher_DirectorySearcher_De()
		{
			de = new DirectoryEntry(configuration.ConnectionString,
									configuration.Username,
									configuration.Password,
									configuration.AuthenticationType);

			ds = new DirectorySearcher(de);

			Assert.AreEqual(ds.SearchRoot.Name,GetName (configuration.BaseDn));
			Assert.AreEqual(ds.Filter,"(objectClass=*)");
			Assert.AreEqual(ds.PropertiesToLoad.Count,0);
			Assert.AreEqual(ds.SearchScope,SearchScope.Subtree);
			Assert.AreEqual(ds.CacheResults,true);
			Assert.AreEqual(ds.ClientTimeout,new TimeSpan(-10000000));
		}

		[Test]
		public void DirectorySearcher_DirectorySearcher_Str()
		{
			ds = new DirectorySearcher("(objectClass=organizationalRole)");

			Assert.AreEqual(ds.Filter,"(objectClass=organizationalRole)");
			Assert.AreEqual(ds.PropertiesToLoad.Count,0);
			Assert.AreEqual(ds.SearchScope,SearchScope.Subtree);
			Assert.AreEqual(ds.CacheResults,true);
			Assert.AreEqual(ds.ClientTimeout,new TimeSpan(-10000000));
		}

		[Test]
		public void DirectorySearcher_DirectorySearcher_DeStr()
		{
			de = new DirectoryEntry(configuration.ConnectionString,
									configuration.Username,
									configuration.Password,
									configuration.AuthenticationType);

			ds = new DirectorySearcher(de,"(objectClass=organizationalRole)");

			Assert.AreEqual(ds.SearchRoot.Name,GetName (configuration.BaseDn));
			Assert.AreEqual(ds.Filter,"(objectClass=organizationalRole)");
			Assert.AreEqual(ds.PropertiesToLoad.Count,0);
			Assert.AreEqual(ds.SearchScope,SearchScope.Subtree);
			Assert.AreEqual(ds.CacheResults,true);
			Assert.AreEqual(ds.ClientTimeout,new TimeSpan(-10000000));
		}

		[Test]
		public void DirectorySearcher_DirectorySearcher_StrStrArr()
		{
			string[] properties = new string[] {"objectClass","ou","cn"};
			ds = new DirectorySearcher("(objectClass=organizationalRole)",properties);

			Assert.AreEqual(ds.Filter,"(objectClass=organizationalRole)");
			Assert.AreEqual(ds.PropertiesToLoad.Count,3);
			foreach(string s in properties) {
				Assert.IsTrue(ds.PropertiesToLoad.Contains(s));
			}
			Assert.AreEqual(ds.SearchScope,SearchScope.Subtree);
			Assert.AreEqual(ds.CacheResults,true);
			Assert.AreEqual(ds.ClientTimeout,new TimeSpan(-10000000));
		}

		[Test]
		public void DirectorySearcher_DirectorySearcher_DeStrStrArr()
		{
			string[] properties = new string[] {"objectClass","ou","cn"};
			de = new DirectoryEntry(configuration.ConnectionString,
									configuration.Username,
									configuration.Password,
									configuration.AuthenticationType);

			ds = new DirectorySearcher(de,"(objectClass=organizationalRole)",properties);

			Assert.AreEqual(ds.SearchRoot.Name,GetName (configuration.BaseDn));
			Assert.AreEqual(ds.Filter,"(objectClass=organizationalRole)");
			Assert.AreEqual(ds.PropertiesToLoad.Count,3);
			foreach(string s in properties) {
				Assert.IsTrue(ds.PropertiesToLoad.Contains(s));
			}
			Assert.AreEqual(ds.SearchScope,SearchScope.Subtree);
			Assert.AreEqual(ds.CacheResults,true);
			Assert.AreEqual(ds.ClientTimeout,new TimeSpan(-10000000));
		}

		[Test]
		public void DirectorySearcher_DirectorySearcher_StrStrArrScp()
		{
			string[] properties = new string[] {"objectClass","ou","cn"};
			ds = new DirectorySearcher("(objectClass=organizationalRole)",properties,SearchScope.Base);

			Assert.AreEqual(ds.Filter,"(objectClass=organizationalRole)");
			Assert.AreEqual(ds.PropertiesToLoad.Count,3);
			foreach(string s in properties) {
				Assert.IsTrue(ds.PropertiesToLoad.Contains(s));
			}
			Assert.AreEqual(ds.SearchScope,SearchScope.Base);
			Assert.AreEqual(ds.CacheResults,true);
			Assert.AreEqual(ds.ClientTimeout,new TimeSpan(-10000000));
		}

		[Test]
		public void DirectorySearcher_DirectorySearcher_DeStrStrArrScp()
		{
			string[] properties = new string[] {"objectClass","ou","cn"};
			de = new DirectoryEntry(configuration.ConnectionString,
									configuration.Username,
									configuration.Password,
									configuration.AuthenticationType);

			ds = new DirectorySearcher(de,"(objectClass=organizationalRole)",properties,SearchScope.Base);

			Assert.AreEqual(ds.SearchRoot.Name,GetName (configuration.BaseDn));
			Assert.AreEqual(ds.Filter,"(objectClass=organizationalRole)");
			Assert.AreEqual(ds.PropertiesToLoad.Count,3);
			foreach(string s in properties) {
				Assert.IsTrue(ds.PropertiesToLoad.Contains(s));
			}
			Assert.AreEqual(ds.SearchScope,SearchScope.Base);
			Assert.AreEqual(ds.CacheResults,true);
			Assert.AreEqual(ds.ClientTimeout,new TimeSpan(-10000000));
		}

		[Test]
		public void DirectorySearcher_CacheResults()
		{
			de = new DirectoryEntry(configuration.ConnectionString,
									configuration.Username,
									configuration.Password,
									configuration.AuthenticationType);

			ds = new DirectorySearcher(de,"(cn=Barak Tsabari)");
			ds.CacheResults = true;

			string oldValue;
			string newValue = "New Description";

			SearchResult result = ds.FindOne();
			SearchResult secondResult;
			using (DirectoryEntry resultDE = result.GetDirectoryEntry()){

			oldValue = (string)((PropertyValueCollection)resultDE.Properties["description"]).Value;
			((PropertyValueCollection)resultDE.Properties["description"]).Value = newValue;
			Assert.AreEqual(((PropertyValueCollection)resultDE.Properties["description"]).Value,newValue);

			using (DirectorySearcher secondDs = new DirectorySearcher(de,"(cn=Barak Tsabari)")){
			secondResult = secondDs.FindOne();
			using (DirectoryEntry secondResultDE = secondResult.GetDirectoryEntry()){

			Assert.AreEqual(((PropertyValueCollection)secondResultDE.Properties["description"]).Value,oldValue);

			((PropertyValueCollection)resultDE.Properties["description"]).Value = oldValue;
			}
			}
			}
			
			ds = new DirectorySearcher(de,"(cn=Barak Tsabari)");
			ds.CacheResults = false;
			result = ds.FindOne();
			using (DirectoryEntry resultDE = result.GetDirectoryEntry()){

			((PropertyValueCollection)resultDE.Properties["description"]).Value = newValue;
			Assert.AreEqual(((PropertyValueCollection)resultDE.Properties["description"]).Value,newValue);

			using (DirectorySearcher secondDs = new DirectorySearcher(de,"(cn=Barak Tsabari)")){
			secondResult = secondDs.FindOne();
			using (DirectoryEntry secondResultDE = secondResult.GetDirectoryEntry()){

			// LAMESPEC : according to documentation, the value retrieved should be the new one,
			// but actually it is an old one
			Assert.AreEqual(((PropertyValueCollection)secondResultDE.Properties["description"]).Value,oldValue);

			((PropertyValueCollection)resultDE.Properties["description"]).Value = oldValue;	
			}
			}
			}
		}

	
		[Test]
		public void DirectorySearcher_ClientTimeout()
		{
			ds = new DirectorySearcher();

			Assert.AreEqual(ds.ClientTimeout,new TimeSpan(-10000000));

			ds.ClientTimeout = new TimeSpan(500000000);
			Assert.AreEqual(ds.ClientTimeout,new TimeSpan(500000000));

			ds.ClientTimeout = TimeSpan.MaxValue;
			Assert.AreEqual(ds.ClientTimeout,TimeSpan.MaxValue);

			ds.ClientTimeout = TimeSpan.MinValue;
			Assert.AreEqual(ds.ClientTimeout,TimeSpan.MinValue);
		}

		[Test]
		public void DirectorySearcher_Filter1()
		{
			de = new DirectoryEntry(configuration.ConnectionString,
									configuration.Username,
									configuration.Password,
									configuration.AuthenticationType);
			ds = new DirectorySearcher(de);
			
			ds.Filter = "(objectClass=person)";
			SearchResultCollection results = ds.FindAll();
			Assert.AreEqual(results.Count,8);

			ds.Filter = "(|(objectClass=person)(objectClass=organizationalUnit))";
			results = ds.FindAll();
			Assert.AreEqual(results.Count,12);

			ds.Filter = "(&(objectClass=person)(objectClass=organizationalUnit))";
			results = ds.FindAll();
			Assert.AreEqual(results.Count,0);
		}

		[Test]
		[Category("NotWorking")]
		public void DirectorySearcher_Filter2()
		{
			de = new DirectoryEntry(configuration.ConnectionString,
									configuration.Username,
									configuration.Password,
									configuration.AuthenticationType);
			ds = new DirectorySearcher(de);
			
			ds.Filter = "((objectClass=person))";
			SearchResultCollection results = ds.FindAll();
			Assert.AreEqual(results.Count,8);

			ds.Filter = "(|(objectClass=person)((objectClass=organizationalUnit)))";
			results = ds.FindAll();
			Assert.AreEqual(results.Count,12);

			ds.Filter = "(&((objectClass=person))(objectClass=organizationalUnit))";
			results = ds.FindAll();
			Assert.AreEqual(results.Count,0);
		}


		[Test]
		public void DirectorySearcher_PageSize()
		{
			de = new DirectoryEntry(configuration.ConnectionString,
									configuration.Username,
									configuration.Password,
									configuration.AuthenticationType);
			ds = new DirectorySearcher(de);

			Assert.AreEqual(ds.PageSize,0);
			
			ds.Filter = "(|(objectClass=person)(objectClass=organizationalUnit))";
			SearchResultCollection results = ds.FindAll();
			Assert.AreEqual(results.Count,12);

			ds.PageSize = 3;
			Assert.AreEqual(ds.PageSize,3);

			ds.Filter = "(|(objectClass=person)(objectClass=organizationalUnit))";
			results = ds.FindAll();
			// LAMESPEC : according to documentation there should be only 3 results !!!
			Assert.AreEqual(results.Count,12);

		}

		[Test]
		public void DirectorySearcher_PropertiesToLoad()
		{
			de = new DirectoryEntry(configuration.ConnectionString,
									configuration.Username,
									configuration.Password,
									configuration.AuthenticationType);
			ds = new DirectorySearcher(de);

			Assert.AreEqual(ds.PropertiesToLoad.Count,0);

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
			Assert.AreEqual(ds.PropertiesToLoad.Count,0);

			ds.PropertiesToLoad.Add("cn");
			ds.PropertiesToLoad.Add("objectClass");

			Assert.AreEqual(ds.PropertiesToLoad.Count,2);
			Assert.IsTrue(ds.PropertiesToLoad.Contains("cn"));
			Assert.IsTrue(ds.PropertiesToLoad.Contains("objectClass"));

			ds.Filter = "(objectClass=person)";
			result = ds.FindOne();

			Assert.AreEqual(result.Properties.Count,3);
			Assert.IsTrue(result.Properties.Contains("cn"));
			Assert.IsTrue(result.Properties.Contains("objectClass"));
			Assert.IsTrue(result.Properties.Contains("ADsPath"));

						
			ds.PropertiesToLoad.Clear();
			Assert.AreEqual(ds.PropertiesToLoad.Count,0);

			ds.PropertiesToLoad.Add("cn");
			ds.PropertiesToLoad.Add("dn");
			ds.PropertiesToLoad.Add("objectClass");

			Assert.AreEqual(ds.PropertiesToLoad.Count,3);
			Assert.IsTrue(ds.PropertiesToLoad.Contains("cn"));
			Assert.IsTrue(ds.PropertiesToLoad.Contains("dn"));
			Assert.IsTrue(ds.PropertiesToLoad.Contains("objectClass"));

			ds.Filter = "(objectClass=person)";
			result = ds.FindOne();

			Assert.AreEqual(result.Properties.Count,3);
			Assert.IsTrue(result.Properties.Contains("cn"));
			Assert.IsTrue(result.Properties.Contains("objectClass"));
			// FIXME : .NET framework threats "dn" as "ADsPath"
			// More on http://www.rlmueller.net/Name_Attributes.htm
			Assert.IsTrue(result.Properties.Contains("ADsPath"));
		}

		[Test]
		public void DirectorySearcher_PropertyNamesOnly()
		{
			de = new DirectoryEntry(configuration.ConnectionString,
									configuration.Username,
									configuration.Password,
									configuration.AuthenticationType);
			ds = new DirectorySearcher(de);

			Assert.AreEqual(ds.PropertyNamesOnly,false);

			// All rpoperties are loaded without values, except "ADsPath"
			ds.PropertyNamesOnly = true;

			ds.Filter = "(objectClass=person)";
			SearchResult result = ds.FindOne();

			foreach(DictionaryEntry en in result.Properties) {
				if(String.Compare((string)en.Key,"adspath",true) != 0) {
					Assert.AreEqual(((ResultPropertyValueCollection)en.Value).Count,0);
				}
				else {
					Assert.AreEqual(((ResultPropertyValueCollection)en.Value).Count,1);
				}				
			}


			// all properties are loaded including values
			ds.PropertyNamesOnly = false;

			ds.Filter = "(objectClass=person)";
			result = ds.FindOne();

			foreach(DictionaryEntry en in result.Properties) {
				Assert.IsTrue(((ResultPropertyValueCollection)en.Value).Count > 0);
			}
		}

		[Test]
		public void DirectorySearcher_ReferralChasing()
		{
			de = new DirectoryEntry(configuration.ConnectionString,
									configuration.Username,
									configuration.Password,
									configuration.AuthenticationType);
			ds = new DirectorySearcher(de);

			Assert.AreEqual(ds.ReferralChasing,ReferralChasingOption.External);

			ds.ReferralChasing = ReferralChasingOption.All;
			Assert.AreEqual(ds.ReferralChasing,ReferralChasingOption.All);

			ds.ReferralChasing = ReferralChasingOption.External;
			Assert.AreEqual(ds.ReferralChasing,ReferralChasingOption.External);

			ds.ReferralChasing = ReferralChasingOption.None;
			Assert.AreEqual(ds.ReferralChasing,ReferralChasingOption.None);

			ds.ReferralChasing = ReferralChasingOption.Subordinate;
			Assert.AreEqual(ds.ReferralChasing,ReferralChasingOption.Subordinate);

			// FIXME : currently we do not have an infrastucture for good testing of this feature
		}

		[Test]
		public void DirectorySearcher_SearchRoot()
		{
			de = new DirectoryEntry(configuration.ConnectionString,
									configuration.Username,
									configuration.Password,
									configuration.AuthenticationType);
			ds = new DirectorySearcher();
			ds.SearchRoot = de;

			Assert.AreEqual(ds.SearchRoot.Name,GetName (configuration.BaseDn));

			ds.Filter = "(objectClass=person)";
			SearchResultCollection results = ds.FindAll();
			Assert.AreEqual(results.Count,8);

			de = new DirectoryEntry(configuration.ServerRoot + "ou=people" + ((configuration.BaseDn.Length == 0) ? String.Empty : ("," + configuration.BaseDn)),
									configuration.Username,
									configuration.Password,
									configuration.AuthenticationType);
			ds.SearchRoot = de;
			Assert.AreEqual(ds.SearchRoot.Name,"ou=people");

			results = ds.FindAll();
			Assert.AreEqual(results.Count,7);

			de = new DirectoryEntry(configuration.ServerRoot + "ou=Human Resources,ou=people" + ((configuration.BaseDn.Length == 0) ? String.Empty : ("," + configuration.BaseDn)),
									configuration.Username,
									configuration.Password,
									configuration.AuthenticationType);
			ds.SearchRoot = de;
			Assert.AreEqual(ds.SearchRoot.Name,"ou=Human Resources");

			results = ds.FindAll();
			Assert.AreEqual(results.Count,1);
		}

		[Test]
		public void DirectorySearcher_SearchScope()
		{
			de = new DirectoryEntry(configuration.ConnectionString,
									configuration.Username,
									configuration.Password,
									configuration.AuthenticationType);
			ds = new DirectorySearcher(de);
			
			Assert.AreEqual(ds.SearchScope,SearchScope.Subtree);

			ds.SearchScope = SearchScope.Base;
			Assert.AreEqual(ds.SearchScope,SearchScope.Base);

			ds.Filter = "(objectClass=organizationalUnit)";
			SearchResultCollection results = ds.FindAll();
			Assert.AreEqual(results.Count,0);

			ds.SearchScope = SearchScope.OneLevel;
			Assert.AreEqual(ds.SearchScope,SearchScope.OneLevel);

			results = ds.FindAll();
			Assert.AreEqual(results.Count,1);

			ds.SearchScope = SearchScope.Subtree;
			Assert.AreEqual(ds.SearchScope,SearchScope.Subtree);

			results = ds.FindAll();
			Assert.AreEqual(results.Count,4);
		}

		[Test]
		public void DirectorySearcher_ServerPageTimeLimit()
		{
			de = new DirectoryEntry(configuration.ConnectionString,
									configuration.Username,
									configuration.Password,
									configuration.AuthenticationType);
			ds = new DirectorySearcher(de);

			Assert.AreEqual(ds.ServerPageTimeLimit,new TimeSpan(-10000000));

			// According to spec PageSize should be set to a value that is not the default of -1
			ds.PageSize = 5;
			ds.ServerPageTimeLimit = new TimeSpan(500000000);
			Assert.AreEqual(ds.ServerPageTimeLimit,new TimeSpan(500000000));

			ds.ServerPageTimeLimit = TimeSpan.MaxValue;
			Assert.AreEqual(ds.ServerPageTimeLimit,TimeSpan.MaxValue);

			ds.ServerPageTimeLimit = TimeSpan.MinValue;
			Assert.AreEqual(ds.ServerPageTimeLimit,TimeSpan.MinValue);

			// FIXME : currently we do not have an infrastucture for good testing of this feature
		}

		[Test]
		public void DirectorySearcher_ServerTimeLimit()
		{
			de = new DirectoryEntry(configuration.ConnectionString,
									configuration.Username,
									configuration.Password,
									configuration.AuthenticationType);
			ds = new DirectorySearcher(de);

			Assert.AreEqual(ds.ServerTimeLimit,new TimeSpan(-10000000));

			// According to spec PageSize should be set to a value that is not the default of -1
			ds.PageSize = 5;
			ds.ServerTimeLimit = new TimeSpan(500000000);
			Assert.AreEqual(ds.ServerTimeLimit,new TimeSpan(500000000));

			ds.ServerTimeLimit = TimeSpan.MaxValue;
			Assert.AreEqual(ds.ServerTimeLimit,TimeSpan.MaxValue);

			ds.ServerTimeLimit = TimeSpan.MinValue;
			Assert.AreEqual(ds.ServerTimeLimit,TimeSpan.MinValue);

			// FIXME : currently we do not have an infrastucture for good testing of this feature
		}

		[Test]
		public void DirectorySearcher_SizeLimit()
		{
			de = new DirectoryEntry(configuration.ConnectionString,
									configuration.Username,
									configuration.Password,
									configuration.AuthenticationType);
			ds = new DirectorySearcher(de);

			Assert.AreEqual(ds.SizeLimit,0);
			
			ds.Filter = "(|(objectClass=person)(objectClass=organizationalUnit))";
			SearchResultCollection results = ds.FindAll();
			Assert.AreEqual(results.Count,12);

			ds.SizeLimit = 3;
			Assert.AreEqual(ds.SizeLimit,3);

			ds.Filter = "(|(objectClass=person)(objectClass=organizationalUnit))";
			results = ds.FindAll();
			Assert.AreEqual(results.Count,3);

			ds.SizeLimit = Int32.MaxValue;
			Assert.AreEqual(ds.SizeLimit,Int32.MaxValue);

			ds.Filter = "(|(objectClass=person)(objectClass=organizationalUnit))";
			results = ds.FindAll();
			Assert.AreEqual(results.Count,12);

		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void DirectorySearcher_SizeLimit_Neg()
		{
			de = new DirectoryEntry(configuration.ConnectionString,
									configuration.Username,
									configuration.Password,
									configuration.AuthenticationType);
			ds = new DirectorySearcher(de);
			ds.SizeLimit = -1;
			Assert.AreEqual(ds.SizeLimit,-1);

			SearchResultCollection results = ds.FindAll();
		}

		[Test]
		public void DirectorySearcher_Sort()
		{
			// FIXME : howto create good sorting
		}

		[Test]
		public void DirectorySearcher_FindAll()
		{
			de = new DirectoryEntry(configuration.ConnectionString,
									configuration.Username,
									configuration.Password,
									configuration.AuthenticationType);
			ds = new DirectorySearcher(de);

			ds.Filter = "(objectClass=person)";
			SearchResultCollection results = ds.FindAll();
			Assert.AreEqual(results.Count,8);

			de = new DirectoryEntry(configuration.ServerRoot + "ou=Human Resources,ou=people" + ((configuration.BaseDn.Length == 0) ? String.Empty : ("," + configuration.BaseDn)),
									configuration.Username,
									configuration.Password,
									configuration.AuthenticationType);
			ds = new DirectorySearcher(de);

			results = ds.FindAll();
			Assert.AreEqual(results.Count,3);
		}

		[Test]
		public void DirectorySearcher_FindOne()
		{
			de = new DirectoryEntry(configuration.ConnectionString,
									configuration.Username,
									configuration.Password,
									configuration.AuthenticationType);
			ds = new DirectorySearcher(de);

			ds.Filter = "(objectClass=person)";
			SearchResult result = ds.FindOne();
			Assert.IsNotNull(result);
			Assert.AreEqual(result.GetDirectoryEntry().Name,"cn=Barak Tsabari");
			

			de = new DirectoryEntry(configuration.ServerRoot + "ou=Human Resources,ou=people" + ((configuration.BaseDn.Length == 0) ? String.Empty : ("," + configuration.BaseDn)),
									configuration.Username,
									configuration.Password,
									configuration.AuthenticationType);
			ds = new DirectorySearcher(de);

			result = ds.FindOne();
			Assert.IsNotNull(result);
			Assert.AreEqual(result.GetDirectoryEntry().Name,"ou=Human Resources");

			ds.Filter = "(objectClass=Barak Tsabari)";
			result = ds.FindOne();
			Assert.IsNull(result);
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

