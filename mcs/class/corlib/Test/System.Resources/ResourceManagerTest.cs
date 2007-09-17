// 
// ResourceManager.cs:
//     NUnit Test Cases for System.Resources.ResourceManager
//
// Authors:
//     Robert Jordan (robertj@gmx.net)
//     Gert Driesen (drieseng@users.sourceforge.net)
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
using System.Reflection;
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
		private CultureInfo _orgUICulture;

		[SetUp]
		public void SetUp ()
		{
			_orgUICulture = Thread.CurrentThread.CurrentUICulture;
		}

		[TearDown]
		public void TearDown ()
		{
			Thread.CurrentThread.CurrentUICulture = _orgUICulture;
		}

		[Test]
		public void Constructor0 ()
		{
			MockResourceManager rm = new MockResourceManager ();
			Assert.IsNull (rm.BaseName, "#1");
			Assert.IsNull (rm.BaseNameField, "#2");
			Assert.IsFalse (rm.IgnoreCase, "#3");
			Assert.IsNull (rm.MainAssembly, "#4");
			Assert.IsNull (rm.ResourceSets, "#5");
			Assert.IsNotNull (rm.ResourceSetType, "#6");
			Assert.IsTrue (typeof (ResourceSet).IsAssignableFrom (rm.ResourceSetType), "#7");
			//Assert.IsFalse (typeof (ResourceSet) == rm.ResourceSetType, "#8");
		}

		[Test]
		public void Constructor1 ()
		{
			MockResourceManager rm = new MockResourceManager (typeof (string));
			Assert.IsNotNull (rm.BaseName, "#1");
			Assert.AreEqual ("String", rm.BaseName, "#2");
			Assert.IsNotNull (rm.BaseNameField, "#3");
			Assert.AreEqual ("String", rm.BaseNameField, "#4");
			Assert.IsFalse (rm.IgnoreCase, "#5");
			Assert.IsNotNull (rm.MainAssembly, "#6");
			Assert.AreEqual (typeof (String).Assembly, rm.MainAssembly, "#7");
			Assert.IsNotNull (rm.ResourceSets, "#8");
			Assert.AreEqual (0, rm.ResourceSets.Count, "#9");
			Assert.IsNotNull (rm.ResourceSetType, "#10");
			Assert.IsTrue (typeof (ResourceSet).IsAssignableFrom (rm.ResourceSetType), "#11");
			//Assert.IsFalse (typeof (ResourceSet) == rm.ResourceSetType, "#12");
		}

		[Test]
		public void Constructor2 ()
		{
			MockResourceManager rm = null;
			Assembly assembly = null;

			assembly = Assembly.GetExecutingAssembly ();
			rm = new MockResourceManager ("mono", assembly);
			Assert.IsNotNull (rm.BaseName, "#A1");
			Assert.AreEqual ("mono", rm.BaseName, "#A2");
			Assert.IsNotNull (rm.BaseNameField, "#A3");
			Assert.AreEqual ("mono", rm.BaseNameField, "#A4");
			Assert.IsFalse (rm.IgnoreCase, "#A5");
			Assert.IsNotNull (rm.MainAssembly, "#A6");
			Assert.AreEqual (assembly, rm.MainAssembly, "#A7");
			Assert.IsNotNull (rm.ResourceSets, "#A8");
			Assert.AreEqual (0, rm.ResourceSets.Count, "#A9");
			Assert.IsNotNull (rm.ResourceSetType, "#A10");
			Assert.IsTrue (typeof (ResourceSet).IsAssignableFrom (rm.ResourceSetType), "#A11");
			//Assert.IsFalse (typeof (ResourceSet) == rm.ResourceSetType, "#A12");

			assembly = typeof (int).Assembly;
			rm = new MockResourceManager (string.Empty, assembly);
			Assert.IsNotNull (rm.BaseName, "#B1");
			Assert.AreEqual (string.Empty, rm.BaseName, "#B2");
			Assert.IsNotNull (rm.BaseNameField, "#B3");
			Assert.AreEqual (string.Empty, rm.BaseNameField, "#B4");
			Assert.IsFalse (rm.IgnoreCase, "#B5");
			Assert.IsNotNull (rm.MainAssembly, "#B6");
			Assert.AreEqual (assembly, rm.MainAssembly, "#B7");
			Assert.IsNotNull (rm.ResourceSets, "#B8");
			Assert.AreEqual (0, rm.ResourceSets.Count, "#B9");
			Assert.IsNotNull (rm.ResourceSetType, "#B10");
			Assert.IsTrue (typeof (ResourceSet).IsAssignableFrom (rm.ResourceSetType), "#B11");
			//Assert.IsFalse (typeof (ResourceSet) == rm.ResourceSetType, "#B12");
		}

		[Test]
		public void Constructor2_BaseName_Null ()
		{
			try {
				new ResourceManager ((string) null, Assembly.
					GetExecutingAssembly ());
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("baseName", ex.ParamName, "#6");
			}
		}

		[Test]
		public void Constructor2_BaseName_Resources ()
		{
#if NET_2_0
			MockResourceManager rm = new MockResourceManager (
				"mono.resources",
				Assembly.GetExecutingAssembly ());
			Assert.IsNotNull (rm.BaseName, "#1");
			Assert.AreEqual ("mono.resources", rm.BaseName, "#2");
			Assert.IsNotNull (rm.BaseNameField, "#3");
			Assert.AreEqual ("mono.resources", rm.BaseNameField, "#4");
			Assert.IsFalse (rm.IgnoreCase, "#5");
			Assert.IsNotNull (rm.MainAssembly, "#6");
			Assert.AreEqual (Assembly.GetExecutingAssembly (), rm.MainAssembly, "#7");
			Assert.IsNotNull (rm.ResourceSets, "#8");
			Assert.AreEqual (0, rm.ResourceSets.Count, "#9");
			Assert.IsNotNull (rm.ResourceSetType, "#10");
			Assert.IsTrue (typeof (ResourceSet).IsAssignableFrom (rm.ResourceSetType), "#11");
			//Assert.IsFalse (typeof (ResourceSet) == rm.ResourceSetType, "#12");
#else
			try {
				new ResourceManager ("mono.resources",
					Assembly.GetExecutingAssembly ());
				Assert.Fail ("#1");
			} catch (ArgumentException ex) {
				// ResourceManager base name should not end in
				// .resources. It should be similar to MyResources,
				// which the ResourceManager can convert into
				// MyResources.<culture>.resources; for example,
				// MyResources.en-US.resources
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsTrue (ex.Message.IndexOf (" .resources") != -1, "#5");
				Assert.IsNull (ex.ParamName, "#6");
			}
#endif
		}

		[Test]
		public void Constructor2_Assembly_Null ()
		{
			try {
				new ResourceManager (string.Empty, (Assembly) null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("assembly", ex.ParamName, "#6");
			}
		}

		[Test]
		public void Constructor3 ()
		{
			MockResourceManager rm = null;
			Assembly assembly = null;

			assembly = Assembly.GetExecutingAssembly ();
			rm = new MockResourceManager ("mono", assembly,
				typeof (ResourceSet));
			Assert.IsNotNull (rm.BaseName, "#A1");
			Assert.AreEqual ("mono", rm.BaseName, "#A2");
			Assert.IsNotNull (rm.BaseNameField, "#A3");
			Assert.AreEqual ("mono", rm.BaseNameField, "#A4");
			Assert.IsFalse (rm.IgnoreCase, "#A5");
			Assert.IsNotNull (rm.MainAssembly, "#A6");
			Assert.AreEqual (assembly, rm.MainAssembly, "#A7");
			Assert.IsNotNull (rm.ResourceSets, "#A8");
			Assert.AreEqual (0, rm.ResourceSets.Count, "#A9");
			Assert.IsNotNull (rm.ResourceSetType, "#A10");
			Assert.AreEqual (typeof (ResourceSet), rm.ResourceSetType, "#A11");

			assembly = typeof (int).Assembly;
			rm = new MockResourceManager ("mono", assembly,
				typeof (MockResourceSet));
			Assert.IsNotNull (rm.BaseName, "#B1");
			Assert.AreEqual ("mono", rm.BaseName, "#B2");
			Assert.IsNotNull (rm.BaseNameField, "#B3");
			Assert.AreEqual ("mono", rm.BaseNameField, "#B4");
			Assert.IsFalse (rm.IgnoreCase, "#B5");
			Assert.IsNotNull (rm.MainAssembly, "#B6");
			Assert.AreEqual (assembly, rm.MainAssembly, "#B7");
			Assert.IsNotNull (rm.ResourceSets, "#B8");
			Assert.AreEqual (0, rm.ResourceSets.Count, "#B9");
			Assert.IsNotNull (rm.ResourceSetType, "#B10");
			Assert.AreEqual (typeof (MockResourceSet), rm.ResourceSetType, "#B11");
		}

		[Test]
		public void Constructor3_BaseName_Null ()
		{
			try {
				new ResourceManager ((string) null, Assembly.
					GetExecutingAssembly (),
					typeof (ResourceSet));
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("baseName", ex.ParamName, "#6");
			}
		}

		[Test]
		public void Constructor3_BaseName_Resources ()
		{
#if NET_2_0
			MockResourceManager rm = new MockResourceManager (
				"mono.resources",
				Assembly.GetExecutingAssembly (),
				typeof (ResourceSet));
			Assert.IsNotNull (rm.BaseName, "#1");
			Assert.AreEqual ("mono.resources", rm.BaseName, "#2");
			Assert.IsNotNull (rm.BaseNameField, "#3");
			Assert.AreEqual ("mono.resources", rm.BaseNameField, "#4");
			Assert.IsFalse (rm.IgnoreCase, "#5");
			Assert.IsNotNull (rm.MainAssembly, "#6");
			Assert.AreEqual (Assembly.GetExecutingAssembly (), rm.MainAssembly, "#7");
			Assert.IsNotNull (rm.ResourceSets, "#8");
			Assert.AreEqual (0, rm.ResourceSets.Count, "#9");
			Assert.IsNotNull (rm.ResourceSetType, "#10");
			Assert.AreEqual (typeof (ResourceSet), rm.ResourceSetType, "#11");
#else
			try {
				new ResourceManager ("mono.resources", 
					Assembly.GetExecutingAssembly (),
					typeof (ResourceSet));
				Assert.Fail ("#1");
			} catch (ArgumentException ex) {
				// ResourceManager base name should not end in
				// .resources. It should be similar to MyResources,
				// which the ResourceManager can convert into
				// MyResources.<culture>.resources; for example,
				// MyResources.en-US.resources
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsTrue (ex.Message.IndexOf (" .resources") != -1, "#5");
				Assert.IsNull (ex.ParamName, "#6");
			}
#endif
		}

		[Test]
		public void Constructor3_Assembly_Null ()
		{
			try {
				new ResourceManager (string.Empty, (Assembly) null,
					typeof (ResourceSet));
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("assembly", ex.ParamName, "#6");
			}
		}

		[Test]
		public void Constructor3_UsingResourceSet_Invalid ()
		{
			try {
				new ResourceManager ("mono", typeof (int).Assembly,
					typeof (string));
				Assert.Fail ("#1");
			} catch (ArgumentException ex) {
				// Type parameter must refer to a subclass of
				// ResourceSet
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsTrue (ex.Message.IndexOf ("ResourceSet") != -1, "#5");
				Assert.IsNotNull (ex.ParamName, "#6");
				Assert.AreEqual ("usingResourceSet", ex.ParamName, "#7");
			}
		}

		[Test]
		public void Constructor3_UsingResourceSet_Null ()
		{
			MockResourceManager rm = new MockResourceManager (
				string.Empty, Assembly.GetExecutingAssembly (),
				(Type) null);
			Assert.IsNotNull (rm.BaseName, "#1");
			Assert.AreEqual (string.Empty, rm.BaseName, "#2");
			Assert.IsNotNull (rm.BaseNameField, "#3");
			Assert.AreEqual (string.Empty, rm.BaseNameField, "#4");
			Assert.IsFalse (rm.IgnoreCase, "#5");
			Assert.IsNotNull (rm.MainAssembly, "#6");
			Assert.AreEqual (Assembly.GetExecutingAssembly (), rm.MainAssembly, "#7");
			Assert.IsNotNull (rm.ResourceSets, "#8");
			Assert.AreEqual (0, rm.ResourceSets.Count, "#9");
			Assert.IsNotNull (rm.ResourceSetType, "#10");
			Assert.IsTrue (typeof (ResourceSet).IsAssignableFrom (rm.ResourceSetType), "#11");
			//Assert.IsFalse (typeof (ResourceSet) == rm.ResourceSetType, "#12");
		}

		[Test]
		public void CreateFileBasedResourceManager_BaseName_Null ()
		{
			try {
				ResourceManager.CreateFileBasedResourceManager (
					(string) null, AppDomain.CurrentDomain.BaseDirectory,
					typeof (ResourceSet));
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("baseName", ex.ParamName, "#6");
			}
		}

		[Test]
#if NET_2_0
		[Category ("NotWorking")]
#endif
		public void CreateFileBasedResourceManager_BaseName_Resources ()
		{
#if NET_2_0
			ResourceManager rm = ResourceManager.CreateFileBasedResourceManager (
				"MyResources.resources", "Test/resources", null);
			try {
				rm.GetResourceSet (CultureInfo.InvariantCulture, true, true);
				Assert.Fail ("#1");
			} catch (MissingManifestResourceException ex) {
				// Could not find any resources appropriate for
				// the specified culture (or the neutral culture)
				//on disk
				Assert.AreEqual (typeof (MissingManifestResourceException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
			}
#else
			try {
				ResourceManager.CreateFileBasedResourceManager (
					"MyResources.resources", "Test/resources",
					null);
				Assert.Fail ("#1");
			} catch (ArgumentException ex) {
				// ResourceManager base name should not end in
				// .resources. It should be similar to MyResources,
				// which the ResourceManager can convert into
				// MyResources.<culture>.resources; for example,
				// MyResources.en-US.resources
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsTrue (ex.Message.IndexOf (" .resources") != -1, "#5");
				Assert.IsNull (ex.ParamName, "#6");
			}
#endif
		}

		[Test]
		public void CreateFileBasedResourceManager_ResourceDir_Null ()
		{
			try {
				ResourceManager.CreateFileBasedResourceManager (
					"whatever", (string) null,
					typeof (ResourceSet));
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("resourceDir", ex.ParamName, "#6");
			}
		}

		[Test]
		public void CreateFileBasedResourceManager_UsingResourceSet_Invalid ()
		{
			ResourceManager rm = ResourceManager.CreateFileBasedResourceManager (
				"MyResources", "Test/resources", typeof (string));
			Assert.IsNotNull (rm.BaseName, "#1");
			Assert.AreEqual ("MyResources", rm.BaseName, "#2");
			Assert.IsFalse (rm.IgnoreCase, "#3");
			Assert.IsNotNull (rm.ResourceSetType, "#4");
			Assert.AreEqual (typeof (string), rm.ResourceSetType, "#5");
		}

		[Test]
		public void CreateFileBasedResourceManager_UsingResourceSet_Null ()
		{
			ResourceManager rm = ResourceManager.CreateFileBasedResourceManager (
				"MyResources", "Test/resources", (Type) null);
			Assert.IsNotNull (rm.BaseName, "#1");
			Assert.AreEqual ("MyResources", rm.BaseName, "#2");
			Assert.IsFalse (rm.IgnoreCase, "#3");
			Assert.IsNotNull (rm.ResourceSetType, "#4");
			Assert.IsTrue (typeof (ResourceSet).IsAssignableFrom (rm.ResourceSetType), "#5");
			//Assert.IsFalse (typeof (ResourceSet) == rm.ResourceSetType, "#6");
		}

		[Test]
		public void GetObject ()
		{
			ResourceManager rm = ResourceManager.
				CreateFileBasedResourceManager ("MyResources", "Test/resources", null);

			Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
			Assert.AreEqual ("Hello World", rm.GetObject ("HelloWorld"), "#A1");
			Assert.AreEqual ("Hello World", rm.GetObject ("HelloWorld", new CultureInfo ("de")), "#A2");
			Assert.AreEqual ("Hello World", rm.GetObject ("HelloWorld", (CultureInfo) null), "#A3");
			Assert.IsNull (rm.GetObject ("deHelloWorld"), "#A4");
			Assert.AreEqual ("Hallo Welt", rm.GetObject ("deHelloWorld", new CultureInfo ("de")), "#A5");
			Assert.IsNull (rm.GetObject ("deHelloWorld", (CultureInfo) null), "#A6");

			Thread.CurrentThread.CurrentUICulture = new CultureInfo ("de");
			Assert.AreEqual ("Hello World", rm.GetObject ("HelloWorld"), "#B1");
			Assert.AreEqual ("Hello World", rm.GetObject ("HelloWorld", (CultureInfo) null), "#B2");
			Assert.AreEqual ("Hallo Welt", rm.GetObject ("deHelloWorld"), "#B3");
			Assert.IsNull (rm.GetObject ("deHelloWorld", new CultureInfo ("nl-BE")), "#B4");
			Assert.AreEqual ("Hallo Welt", rm.GetObject ("deHelloWorld", (CultureInfo) null), "#B5");

			Thread.CurrentThread.CurrentUICulture = new CultureInfo ("nl-BE");
			Assert.AreEqual ("Hello World", rm.GetObject ("HelloWorld"), "#C1");
			Assert.AreEqual ("Hello World", rm.GetObject ("HelloWorld", new CultureInfo ("de")), "#C2");
			Assert.AreEqual ("Hello World", rm.GetObject ("HelloWorld", (CultureInfo) null), "#C3");
			Assert.IsNull (rm.GetObject ("deHelloWorld"), "#C4");
			Assert.AreEqual ("Hallo Welt", rm.GetObject ("deHelloWorld", new CultureInfo ("de")), "#C5");
			Assert.IsNull (rm.GetObject ("deHelloWorld", (CultureInfo) null), "#C6");

			rm.ReleaseAllResources ();
		}

		[Test]
		public void GetObject_Name_Null ()
		{
			ResourceManager rm = new ResourceManager (typeof (string));
			try {
				rm.GetObject ((string) null);
				Assert.Fail ("#A1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.IsNotNull (ex.ParamName, "#A5");
				Assert.AreEqual ("name", ex.ParamName, "#A6");
			}

			try {
				rm.GetObject ((string) null, CultureInfo.InvariantCulture);
				Assert.Fail ("#B1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
				Assert.IsNotNull (ex.ParamName, "#B5");
				Assert.AreEqual ("name", ex.ParamName, "#B6");
			}
			rm.ReleaseAllResources ();
		}

		[Test]
		public void GetResourceFileName ()
		{
			MockResourceManager rm = new MockResourceManager ();
			Assert.AreEqual (".nl-BE.resources",
				rm.GetResourceFileName (new CultureInfo ("nl-BE")), "#A1");
			Assert.AreEqual (".fr.resources",
				rm.GetResourceFileName (new CultureInfo ("fr")), "#A2");
			Assert.AreEqual (".resources",
				rm.GetResourceFileName (CultureInfo.InvariantCulture), "#A3");

			rm = new MockResourceManager (typeof (string));
			Assert.AreEqual ("String.nl-BE.resources",
				rm.GetResourceFileName (new CultureInfo ("nl-BE")), "#B1");
			Assert.AreEqual ("String.fr.resources",
				rm.GetResourceFileName (new CultureInfo ("fr")), "#B2");
			Assert.AreEqual ("String.resources",
				rm.GetResourceFileName (CultureInfo.InvariantCulture), "#B3");
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void GetResourceFileName_Culture_Null ()
		{
			MockResourceManager rm = new MockResourceManager ();
			rm.GetResourceFileName ((CultureInfo) null);
		}

		[Test]
		public void GetResourceSet_Culture_Null ()
		{
			ResourceManager rm = new ResourceManager (typeof (string));
			try {
				rm.GetResourceSet ((CultureInfo) null, false, false);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("culture", ex.ParamName, "#6");
			}
			rm.ReleaseAllResources ();
		}

		[Test]
		public void GetString ()
		{
			ResourceManager rm = ResourceManager.
				CreateFileBasedResourceManager ("MyResources", "Test/resources", null);

			Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
			Assert.AreEqual ("Hello World", rm.GetString ("HelloWorld"), "#A1");
			Assert.AreEqual ("Hello World", rm.GetString ("HelloWorld", new CultureInfo ("de")), "#A2");
			Assert.AreEqual ("Hello World", rm.GetString ("HelloWorld", (CultureInfo) null), "#A3");
			Assert.IsNull (rm.GetString ("deHelloWorld"), "#A4");
			Assert.AreEqual ("Hallo Welt", rm.GetString ("deHelloWorld", new CultureInfo ("de")), "#A5");
			Assert.IsNull (rm.GetString ("deHelloWorld", (CultureInfo) null), "#A6");

			Thread.CurrentThread.CurrentUICulture = new CultureInfo ("de");
			Assert.AreEqual ("Hello World", rm.GetString ("HelloWorld"), "#B1");
			Assert.AreEqual ("Hello World", rm.GetString ("HelloWorld", (CultureInfo) null), "#B2");
			Assert.AreEqual ("Hallo Welt", rm.GetString ("deHelloWorld"), "#B3");
			Assert.IsNull (rm.GetString ("deHelloWorld", new CultureInfo ("nl-BE")), "#B4");
			Assert.AreEqual ("Hallo Welt", rm.GetString ("deHelloWorld", (CultureInfo) null), "#B5");

			Thread.CurrentThread.CurrentUICulture = new CultureInfo ("nl-BE");
			Assert.AreEqual ("Hello World", rm.GetString ("HelloWorld"), "#C1");
			Assert.AreEqual ("Hello World", rm.GetString ("HelloWorld", new CultureInfo ("de")), "#C2");
			Assert.AreEqual ("Hello World", rm.GetString ("HelloWorld", (CultureInfo) null), "#C3");
			Assert.IsNull (rm.GetString ("deHelloWorld"), "#C4");
			Assert.AreEqual ("Hallo Welt", rm.GetString ("deHelloWorld", new CultureInfo ("de")), "#C5");
			Assert.IsNull (rm.GetString ("deHelloWorld", (CultureInfo) null), "#C6");

			rm.ReleaseAllResources ();
		}

		[Test]
		public void GetString_Name_Null ()
		{
			ResourceManager rm = new ResourceManager (typeof (string));
			try {
				rm.GetString ((string) null);
				Assert.Fail ("#A1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.IsNotNull (ex.ParamName, "#A5");
				Assert.AreEqual ("name", ex.ParamName, "#A6");
			}

			try {
				rm.GetString ((string) null, CultureInfo.InvariantCulture);
				Assert.Fail ("#B1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
				Assert.IsNotNull (ex.ParamName, "#B5");
				Assert.AreEqual ("name", ex.ParamName, "#B6");
			}
			rm.ReleaseAllResources ();
		}

#if NET_2_0
		[Test]
		public void GetStream ()
		{
			Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
			ResourceManager rm = ResourceManager.
				CreateFileBasedResourceManager ("StreamTest", "Test/resources", null);
			UnmanagedMemoryStream s = rm.GetStream ("test");
			Assert.AreEqual (22, s.Length, "#A1");
			Assert.AreEqual ("veritas vos liberabit\n", new StreamReader (s).ReadToEnd (), "#A2");
			s.Close ();

			Thread.CurrentThread.CurrentUICulture = new CultureInfo ("ja-JP");
			s = rm.GetStream ("test");
			Assert.AreEqual (22, s.Length, "#B1");
			Assert.AreEqual ("Veritas Vos Liberabit\n", new StreamReader (s).ReadToEnd (), "#B2");
			s.Close ();

			Thread.CurrentThread.CurrentUICulture = new CultureInfo ("nl-BE");
			s = rm.GetStream ("test");
			Assert.AreEqual (22, s.Length, "#C1");
			Assert.AreEqual ("veritas vos liberabit\n", new StreamReader (s).ReadToEnd (), "#C2");
			rm.ReleaseAllResources ();
		}

		[Test]
		public void GetStream_Culture ()
		{
			Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
			ResourceManager rm = ResourceManager.
				CreateFileBasedResourceManager ("StreamTest", "Test/resources", null);
			UnmanagedMemoryStream s = rm.GetStream ("test", new CultureInfo ("ja-JP"));
			Assert.AreEqual (22, s.Length, "#1");
			Assert.AreEqual ("Veritas Vos Liberabit\n", new StreamReader (s).ReadToEnd (), "#2");
			s.Close ();

			Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
			s = rm.GetStream ("test", null);
			Assert.AreEqual (22, s.Length, "#1");
			Assert.AreEqual ("veritas vos liberabit\n", new StreamReader (s).ReadToEnd (), "#2");
			s.Close ();

			Thread.CurrentThread.CurrentUICulture = new CultureInfo ("ja-JP");
			s = rm.GetStream ("test", null);
			Assert.AreEqual (22, s.Length, "#1");
			Assert.AreEqual ("Veritas Vos Liberabit\n", new StreamReader (s).ReadToEnd (), "#2");
			s.Close ();
			rm.ReleaseAllResources ();
		}

		[Test]
		public void GetStream_Name_Null ()
		{
			ResourceManager rm = new ResourceManager (typeof (string));
			try {
				rm.GetStream ((string) null);
				Assert.Fail ("#A1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.IsNotNull (ex.ParamName, "#A5");
				Assert.AreEqual ("name", ex.ParamName, "#A6");
			}

			try {
				rm.GetStream ((string) null, CultureInfo.InvariantCulture);
				Assert.Fail ("#A1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.IsNotNull (ex.ParamName, "#A5");
				Assert.AreEqual ("name", ex.ParamName, "#A6");
			}
			rm.ReleaseAllResources ();
		}

		[Test]
		public void GetStream_Resource_DoesNotExist ()
		{
			ResourceManager rm = ResourceManager.
				CreateFileBasedResourceManager ("StreamTest", "Test/resources", null);
			Assert.IsNull (rm.GetStream ("HelloWorld"));
			rm.ReleaseAllResources ();
		}

		[Test]
		public void GetStream_Resource_NonStream ()
		{
			ResourceManager rm = ResourceManager.
				CreateFileBasedResourceManager ("MyResources", "Test/resources", null);

			try {
				rm.GetStream ("HelloWorld", CultureInfo.InvariantCulture);
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
#endif

		[Test]
		public void IgnoreCase ()
		{
			ResourceManager rm = ResourceManager.
				CreateFileBasedResourceManager ("MyResources", "Test/resources", null);

			Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
			Assert.IsFalse (rm.IgnoreCase, "#A1");
			Assert.IsNull (rm.GetString ("helloWORLD"), "#A2");
			rm.IgnoreCase = true;
			Assert.IsTrue (rm.IgnoreCase, "#B1");
			Assert.AreEqual ("Hello World", rm.GetString ("HelloWorld"), "#B2");
			rm.ReleaseAllResources ();
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void TestResourceManagerGetResourceSetEmpty ()
		{
			Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
			ResourceManagerPoker rm = new ResourceManagerPoker ();
			rm.GetResourceSet (CultureInfo.InvariantCulture, true, true);
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

		class MockResourceManager : ResourceManager
		{
			public MockResourceManager ()
			{
			}

			public MockResourceManager (Type resourceSource) : base (resourceSource)
			{
			}

			public MockResourceManager (string baseName, Assembly assembly)
				: base (baseName, assembly)
			{
			}

			public MockResourceManager (string baseName, Assembly assembly, Type usingResourceSet)
				: base (baseName, assembly, usingResourceSet)
			{
			}

			public new string BaseNameField {
				get { return base.BaseNameField; }
			}

			public new Assembly MainAssembly {
				get { return base.MainAssembly; }
			}

			public new Hashtable ResourceSets {
				get { return base.ResourceSets; }
			}

			public new string GetResourceFileName (CultureInfo culture)
			{
				return base.GetResourceFileName (culture);
			}
		}

		class MockResourceSet : ResourceSet
		{
		}
	}
}
