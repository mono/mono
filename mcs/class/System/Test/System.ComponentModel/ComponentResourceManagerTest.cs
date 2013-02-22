//
// System.ComponentModel.ComponentResourceManager test cases
//
// Authors:
// 	Gert Driesen (drieseng@users.sourceforge.net)
//
// (c) 2007 Gert Driesen
//

#if !MOBILE

using System;
using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Resources;

using NUnit.Framework;

namespace MonoTests.System.ComponentModel
{
	[TestFixture]
	public class ComponentResourceManagerTest
	{
		[Test]
		public void Constructor0 ()
		{
			MockComponentResourceManager crm = new MockComponentResourceManager ();
			Assert.IsNull (crm.BaseName, "#1");
			Assert.IsNull (crm.BaseNameField, "#2");
			Assert.IsFalse (crm.IgnoreCase, "#3");
			Assert.IsNull (crm.MainAssembly, "#4");
			Assert.IsNull (crm.ResourceSets, "#5");
			Assert.IsNotNull (crm.ResourceSetType, "#6");
			Assert.IsTrue (typeof (ResourceSet).IsAssignableFrom (crm.ResourceSetType), "#7");
			//Assert.IsFalse (typeof (ResourceSet) == crm.ResourceSetType, "#7");
		}

		[Test]
		public void Constructor1 ()
		{
			MockComponentResourceManager crm = new MockComponentResourceManager (
				typeof (Component));
			Assert.IsNotNull (crm.BaseName, "#1");
			Assert.AreEqual ("Component", crm.BaseName, "#2");
			Assert.IsNotNull (crm.BaseNameField, "#3");
			Assert.AreEqual ("Component", crm.BaseNameField, "#4");
			Assert.IsFalse (crm.IgnoreCase, "#5");
			Assert.IsNotNull (crm.MainAssembly, "#6");
			Assert.AreEqual (typeof (Component).Assembly, crm.MainAssembly, "#7");
			Assert.IsNotNull (crm.ResourceSets, "#8");
			Assert.AreEqual (0, crm.ResourceSets.Count, "#9");
			Assert.IsNotNull (crm.ResourceSetType, "#10");
			Assert.IsTrue (typeof (ResourceSet).IsAssignableFrom (crm.ResourceSetType), "#11");
			//Assert.IsFalse (typeof (ResourceSet) == crm.ResourceSetType, "#12");
		}

		[Test]
		public void Constructor1_ResourceSource_Null ()
		{
			try {
				new ComponentResourceManager ((Type) null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("resourceSource", ex.ParamName, "#6");
			}
		}

		[Test]
		public void ApplyResources_ObjectName_Null ()
		{
			ComponentResourceManager crm = new ComponentResourceManager ();
			try {
				crm.ApplyResources (new object (), (string) null);
				Assert.Fail ("#A1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.IsNotNull (ex.ParamName, "#A5");
				Assert.AreEqual ("objectName", ex.ParamName, "#A6");
			}

			try {
				crm.ApplyResources (new object (), (string) null, CultureInfo.InvariantCulture);
				Assert.Fail ("#B1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
				Assert.IsNotNull (ex.ParamName, "#B5");
				Assert.AreEqual ("objectName", ex.ParamName, "#B6");
			}
		}

		[Test]
		public void ApplyResources_Value_Null ()
		{
			ComponentResourceManager crm = new ComponentResourceManager ();
			try {
				crm.ApplyResources (null, "$this");
				Assert.Fail ("#A1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.IsNotNull (ex.ParamName, "#A5");
				Assert.AreEqual ("value", ex.ParamName, "#A6");
			}

			try {
				crm.ApplyResources (null, "$this", CultureInfo.InvariantCulture);
				Assert.Fail ("#B1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
				Assert.IsNotNull (ex.ParamName, "#B5");
				Assert.AreEqual ("value", ex.ParamName, "#B6");
			}
		}

		[Test]
		public void IgnoreCase ()
		{
			ComponentResourceManager crm = new ComponentResourceManager ();
			Assert.IsFalse (crm.IgnoreCase, "#1");
			crm.IgnoreCase = true;
			Assert.IsTrue (crm.IgnoreCase, "#2");
		}

		class MockComponentResourceManager : ComponentResourceManager
		{
			public MockComponentResourceManager ()
			{
			}

			public MockComponentResourceManager (Type resourceSource)
				: base (resourceSource)
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
		}
	}
}

#endif
