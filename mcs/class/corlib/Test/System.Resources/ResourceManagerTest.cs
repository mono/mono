// 
// ResourceManager.cs:
//     NUnit Test Cases for System.Resources.ResourceManager
//
// Authors:
//     Robert Jordan (robertj@gmx.net)
//
// Copyright (C) 2005 Novell, Inc. (http://www.novell.com)
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

using System;
using System.Collections;
using System.Globalization;
using System.Resources;
using System.Threading;
using System.IO;

using NUnit.Framework;

namespace MonoTests.System.Resources
{
	class ResourceManagerPoker : ResourceManager
	{
		public ResourceManagerPoker ()
		{
			BaseNameField = String.Format ("Test{0}resources{0}MyResources", Path.DirectorySeparatorChar);
		}

		public Hashtable GetResourceSets ()
		{
			return base.ResourceSets;
		}

		public void InitResourceSets ()
		{
			base.ResourceSets = new Hashtable ();
		}
	}

	[TestFixture]
	public class ResourceManagerTest
	{
		[Test]
		public void TestInvariantCulture ()
		{
			Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
			ResourceManager rm = ResourceManager.
				CreateFileBasedResourceManager ("MyResources", "Test/resources", null);
			Assert.AreEqual ("Hello World", rm.GetString ("HelloWorld"), "#01");
			Assert.AreEqual ("Hello World", rm.GetObject ("HelloWorld"), "#02");
			rm.ReleaseAllResources ();
		}

#if NET_2_0
		[Test]
		public void GetStreamOverNonStream ()
		{
			Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
			ResourceManager rm = ResourceManager.
				CreateFileBasedResourceManager ("MyResources", "Test/resources", null);

			try {
				rm.GetStream ("HelloWorld");
				Assert.Fail ("#1");
			} catch (InvalidOperationException ex) {
				// Resource 'HelloWorld' was not a Stream - call
				// GetObject instead
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
			} finally {
				rm.ReleaseAllResources ();
			}
		}

		[Test]
		public void TestInvariantCultureStreamMissing ()
		{
			Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
			ResourceManager rm = ResourceManager.
				CreateFileBasedResourceManager ("StreamTest", "Test/resources", null);
			Assert.IsNull (rm.GetStream ("HelloWorld")); // no such resource
			rm.ReleaseAllResources ();
		}

		[Test]
		public void TestInvariantCultureStream ()
		{
			Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
			ResourceManager rm = ResourceManager.
				CreateFileBasedResourceManager ("StreamTest", "Test/resources", null);
			UnmanagedMemoryStream s = rm.GetStream ("test");
			Assert.AreEqual (22, s.Length, "#1");
			Assert.AreEqual ("veritas vos liberabit\n", new StreamReader (s).ReadToEnd (), "#2");
			s.Close ();
			rm.ReleaseAllResources ();
		}

		[Test]
		public void TestCustomCultureStream ()
		{
			Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
			ResourceManager rm = ResourceManager.
				CreateFileBasedResourceManager ("StreamTest", "Test/resources", null);
			UnmanagedMemoryStream s = rm.GetStream ("test", new CultureInfo ("ja-JP"));
			Assert.AreEqual (22, s.Length, "#1");
			Assert.AreEqual ("Veritas Vos Liberabit\n", new StreamReader (s).ReadToEnd (), "#2");
			s.Close ();
			rm.ReleaseAllResources ();
		}
#endif

		[Test]
		public void TestGermanCulture ()
		{
			Thread.CurrentThread.CurrentUICulture = new CultureInfo ("de-DE");
			ResourceManager rm = ResourceManager.
				CreateFileBasedResourceManager ("MyResources", "Test/resources", null);
			Assert.AreEqual ("Hello World", rm.GetString ("HelloWorld"), "#01");
			Assert.AreEqual ("Hello World", rm.GetObject ("HelloWorld"), "#02");
			Assert.AreEqual ("Hallo Welt", rm.GetString ("deHelloWorld"), "#03");
			Assert.AreEqual ("Hallo Welt", rm.GetObject ("deHelloWorld"), "#04");
			rm.ReleaseAllResources ();
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void TestResourceManagerGetResourceSetEmpty ()
		{
			Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
			ResourceManagerPoker rm = new ResourceManagerPoker ();
			ResourceSet rs = rm.GetResourceSet (CultureInfo.InvariantCulture,
							    true, true);
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void TestResourceManagerReleaseAllResourcesEmpty ()
		{
			Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
			ResourceManagerPoker rm = new ResourceManagerPoker ();
			rm.ReleaseAllResources ();
		}

		[Test]
		public void TestResourceManagerReleaseAllResources ()
		{
			Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
			ResourceManagerPoker rm = new ResourceManagerPoker ();
			rm.InitResourceSets ();
			rm.ReleaseAllResources ();
		}

		[Test]
		public void TestResourceManagerResourceSets ()
		{
			Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
			ResourceManagerPoker rm = new ResourceManagerPoker ();

			rm.InitResourceSets ();

			ResourceSet rs = rm.GetResourceSet (CultureInfo.InvariantCulture,
							    true, true);

			Assert.AreEqual (1, rm.GetResourceSets().Keys.Count, "#01");

			rs.Close ();

			Assert.AreEqual (1, rm.GetResourceSets().Keys.Count, "#02");
			
			rs = rm.GetResourceSet (CultureInfo.InvariantCulture,
						true, true);
			
			Assert.AreEqual (1, rm.GetResourceSets().Keys.Count, "#03");

			rm.ReleaseAllResources ();
		}
		
		[Test]
		public void TestResourceManagerResourceSetClosedException ()
		{
			Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
			ResourceManagerPoker rm = new ResourceManagerPoker ();
			
			rm.InitResourceSets ();
			
			ResourceSet rs = rm.GetResourceSet (CultureInfo.InvariantCulture,
							    true, true);
			rs.Close ();
			rs = rm.GetResourceSet (CultureInfo.InvariantCulture,
						true, true);

			try {
				rm.GetString ("HelloWorld");
				Assert.Fail ("#1");
			} catch (InvalidOperationException ex) {
				// ResourceSet is closed
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
			} finally {
				rm.ReleaseAllResources ();
			}
		}
	}
}
