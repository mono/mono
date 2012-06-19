//
// StronglyTypedResourceBuilderResourceNameTests.cs - tests validation 
// of the Resource Names passed in the resourceList param of the main 
// Create overload and the unmatatchables returned
// 
// Author:
//  	Gary Barnett (2012)
// 
// Copyright (C) Gary Barnett (2012)
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
#if NET_2_0

using NUnit.Framework;
using System;
using System.Resources.Tools;
using System.CodeDom;
using Microsoft.CSharp;
using System.Collections.Generic;

namespace MonoTests.System.Resources.Tools {
	[TestFixture]
	public class StronglyTypedResourceBuilderResourceNameTests	{
		CSharpCodeProvider provider = new CSharpCodeProvider ();
		
		[Test, ExpectedException (typeof (ArgumentException))]
		public void ResourceNamesCaseSensitiveDupes ()
		{
			// passing in case insensitive dupes of resourcenames throws exception in .NET framework
			Dictionary<string, object> testResources = new Dictionary<string, object> ();
			string [] unmatchables;

			testResources.Add ("FortyTwo", String.Empty);
			testResources.Add ("fortytwo", String.Empty);      		 
			
			StronglyTypedResourceBuilder.Create (testResources,
								"TestClass",
								"TestNamespace",
								"TestResourcesNameSpace",
								provider,
								true,
								out unmatchables);
		}

		[Test, ExpectedException (typeof (ArgumentException))]
		public void ResourceNamesCaseSensitiveDupesIgnored ()
		{
			// passing in case insensitive dupes of resourcenames to be ignored still raises exception
			Dictionary<string, object> testResources = new Dictionary<string, object> ();
			string [] unmatchables;

			testResources.Add (">>FortyTwo", String.Empty);
			testResources.Add (">>fortytwo", String.Empty);      

			StronglyTypedResourceBuilder.Create (testResources,
								"TestClass",
								"TestNamespace",
								"TestResourcesNameSpace",
								provider,
								true,
								out unmatchables);
		}

		[Test, ExpectedException (typeof (ArgumentException))]
		public void ResourceNamesCaseSensitiveDupesInvalid ()
		{
			// passing in case insensitive dupes of invalid resourcenames still raises exception
			Dictionary<string, object> testResources = new Dictionary<string, object> ();
			string [] unmatchables;

			testResources.Add ("Fo$rtyTwo", String.Empty);
			testResources.Add ("fo$rtytwo", String.Empty);      		 
			
			StronglyTypedResourceBuilder.Create (testResources,
								"TestClass",
								"TestNamespace",
								"TestResourcesNameSpace",
								provider,
								true,
								out unmatchables);
		}

		[Test]
		public void ResourceNamesCaseSensitiveDupesWithSpecialChars ()
		{
			// resourcenames with special chars that would result in case insentive dupes go to unmatchables

			Dictionary<string, object> testResources = new Dictionary<string, object> ();
			string [] unmatchables;

			testResources.Add ("Fo&rtyTwo", String.Empty);
			testResources.Add ("fo_rtytwo", String.Empty);      		 
			
			StronglyTypedResourceBuilder.Create (testResources,
								"TestClass",
								"TestNamespace",
								"TestResourcesNameSpace",
								provider,
								true,
								out unmatchables);
			
			Assert.AreEqual (2,unmatchables.Length);
		}

		[Test]
		public void ResourceNamesDuplicate_NETBUG ()
		{
			/* 	
			 * DUPES CHECK HAPPENS AFTER VerifyResourceName called which changed eg.
			 *   language keywords have _ appended to start
			 *   string.emtpy converted to _
			 *   various chars replaced
			 * .net inconsistency:
			 * if keys contain multiple single char names made up of invalid chars
			 * which are converted to _, sometimes none are used and all are returned 
			 * in unmatchables, while other times one is used, and other times 
			 * none are used but one is still missing from unmatchables like in this case
			*/
			Dictionary<string, object> testResources = new Dictionary<string, object> ();
			string [] unmatchables;
			
			testResources.Add ("for", "1");
			testResources.Add ("_for", "2"); 
			testResources.Add (String.Empty, String.Empty);
			testResources.Add ("*", String.Empty);
			testResources.Add ("_", String.Empty);
			testResources.Add (".", String.Empty);	
			testResources.Add ("/", String.Empty);	
			testResources.Add ("\\", String.Empty);	
			testResources.Add ("imok", "2");
			
			CodeCompileUnit ccu = StronglyTypedResourceBuilder.Create (testResources,
										"TestRes",
										"TestNamespace",
										"TestResourcesNameSpace",
										provider,
										true,
										out unmatchables);
							
			int matchedResources = testResources.Count - unmatchables.Length;
			int membersExpected = matchedResources + 5; // 5 standard members
			
			Assert.AreEqual (membersExpected,ccu.Namespaces [0].Types [0].Members.Count);	
		}
		
		[Test]
		public void ResourceNamesDuplicate ()
		{
			/* 	
			 * DUPES CHECK HAPPENS AFTER VerifyResourceName called which changed eg.
			 *   language keywords have _ appended to start
			 *   string.emtpy converted to _
			 *   various chars replaced
			*/
			Dictionary<string, object> testResources = new Dictionary<string, object> ();
			string [] unmatchables;
			
			testResources.Add ("for", "1");
			testResources.Add ("_for", "2"); 
			testResources.Add ("&", String.Empty);     		
			testResources.Add ("_", String.Empty);
			testResources.Add ("imok", "2");
			
			CodeCompileUnit ccu = StronglyTypedResourceBuilder.Create (testResources,
										"TestRes",
										"TestNamespace",
										"TestResourcesNameSpace",
										provider,
										true,
										out unmatchables);
			
			int matchedResources = testResources.Count - unmatchables.Length;
			int membersExpected = matchedResources + 5; // 5 standard members
			Assert.AreEqual (membersExpected,ccu.Namespaces [0].Types [0].Members.Count);	
		}
		
		[Test]
		public void ResourceNamesIgnored ()
		{
			// names beginning with the chars "$" and ">>" ignored and not put in unmatchables
			Dictionary<string, object> testResources = new Dictionary<string, object>();
			string [] unmatchables;
			
			testResources.Add ("$test1", String.Empty);
			testResources.Add ("$test2", String.Empty); 
			testResources.Add (">>test1", String.Empty);     		
			testResources.Add (">>test2", String.Empty);     		 
			testResources.Add ("$", String.Empty);
			testResources.Add (">>", String.Empty);	
			testResources.Add (">", String.Empty);	
			testResources.Add (">test1", String.Empty);
			testResources.Add ("test>>", String.Empty);
			// resource name with $ somwhere else goes into unmatchables as invalid with csharpprovider
			
			CodeCompileUnit ccu = StronglyTypedResourceBuilder.Create (testResources,
											"TestRes",
											"TestNamespace",
											"TestResourcesNameSpace",
											provider,
											true,
											out unmatchables);
				
			Assert.AreEqual(0,unmatchables.Length);
			
			Assert.AreEqual (8,ccu.Namespaces [0].Types [0].Members.Count);	// 3 valid + 5 standard
			Assert.IsNotNull (StronglyTypedResourceBuilderCodeDomTest.Get<CodeMemberProperty> (
													"_", ccu));
			Assert.IsNotNull (StronglyTypedResourceBuilderCodeDomTest.Get<CodeMemberProperty> (
													"_test1", ccu));
			Assert.IsNotNull (StronglyTypedResourceBuilderCodeDomTest.Get<CodeMemberProperty> (
													"test__", ccu));
		}
		
		[Test]
		public void ResourceNamesInvalid ()
		{
			// resources named Culture and ResourceManager go into unmatchables (case sensitive)
			// if there is a $ in resource name after char 0 it goes into unmatchables with csharpprovider
			Dictionary<string, object> testResources = new Dictionary<string, object> ();
			string [] unmatchables;
			
			testResources.Add ("ResourceManager", String.Empty);
			testResources.Add ("Culture", String.Empty);
			testResources.Add ("t$est", String.Empty);

			CodeCompileUnit ccu = StronglyTypedResourceBuilder.Create (testResources,
										"TestRes",
										"TestNamespace",
										"TestResourcesNameSpace",
										provider,
										true,
										out unmatchables);
			
			Assert.AreEqual (3,unmatchables.Length);
			Assert.AreEqual (5,ccu.Namespaces [0].Types [0].Members.Count);	// 5 standard
		}

		[Test]
		public void ResourceNamesInvalidDifferentCase ()
		{
			// resources named Culture and ResourceManager go into unmatchables (case sensitive)
			Dictionary<string, object> testResources = new Dictionary<string, object> ();
			string [] unmatchables;
			
			testResources.Add ("resourceManager", String.Empty);
			testResources.Add ("culture", String.Empty);
			testResources.Add ("t$est", String.Empty);

			CodeCompileUnit ccu = StronglyTypedResourceBuilder.Create (testResources,
										"TestRes",
										"TestNamespace",
										"TestResourcesNameSpace",
										provider,
										true,
										out unmatchables);
			
			Assert.AreEqual (1,unmatchables.Length);
			Assert.AreEqual (7,ccu.Namespaces [0].Types [0].Members.Count);	// 5 standard +2
		}

		[Test, ExpectedException (typeof (InvalidCastException))]
		public void ResourceNamesNotString ()
		{
			// throws InvalidCastException in .net framework
			Dictionary<object, object> testResources = new Dictionary<object, object> ();
			string [] unmatchables;
			
			testResources.Add (DateTime.MinValue, "1");
			
			StronglyTypedResourceBuilder.Create (testResources,
								"TestRes",
								"TestNamespace",
								"TestResourcesNameSpace",
								provider,
								true,
								out unmatchables);
			
		}
		
	}
}

#endif
