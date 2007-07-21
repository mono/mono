//
// System.ComponentModel.DisplayNameAttribute test cases
//
// Authors:
//      Marek Habersack (grendello@gmail.com)
//      Gert Driesen (drieseng@users.sourceforge.net
//
// (c) 2006 Marek Habersack
//

#if NET_2_0
using System;
using System.ComponentModel;
using System.Reflection;

using NUnit.Framework;

namespace MonoTests.System.ComponentModel 
{
	[DisplayName ()]
	class TestClass1
	{
		[DisplayName ()]
		public string Property1
		{
			get { return String.Empty; }
		}

		[DisplayName ()]
		public string Method1 ()
		{
			return String.Empty;
		}
	}

	[DisplayName ("TestClassTwo")]
	class TestClass2
	{
		[DisplayName ("PropertyTwo")]
		public string Property2
		{
			get { return String.Empty; }
		}

		[DisplayName ("MethodTwo")]
		public string Method2 ()
		{
			return String.Empty;
		}
	}

	[DisplayName (null)]
	class TestClass3
	{
		[DisplayName (null)]
		public string Property3
		{
			get { return String.Empty; }
		}

		[DisplayName (null)]
		public string Method3 ()
		{
			return String.Empty;
		}
	}
	
	[TestFixture]
	public class DisplayNameAttributeTests
	{
		private TestClass1 tc1;
		private TestClass2 tc2;
		private TestClass3 tc3;

		DisplayNameAttribute GetDisplayNameAttribute (object[] attrs)
		{
			DisplayNameAttribute dn = null;
			foreach (object attr in attrs) {
				dn = attr as DisplayNameAttribute;
				if (dn != null)
					break;
			}
			return dn;
		}
		
		DisplayNameAttribute GetAttribute (Type type)
		{
			return GetDisplayNameAttribute (type.GetCustomAttributes (false));
		}

		DisplayNameAttribute GetAttribute (Type type, string memberName, MemberTypes expectedType)
		{
			MemberInfo[] mi = type.GetMember (memberName, expectedType, BindingFlags.Instance | BindingFlags.Public);
			return GetDisplayNameAttribute (mi[0].GetCustomAttributes (false));
		}

		[SetUp]
		public void FixtureSetUp ()
		{
			tc1 = new TestClass1 ();
			tc2 = new TestClass2 ();
			tc3 = new TestClass3 ();
		}
		
		[Test]
		public void Constructor0 ()
		{
			DisplayNameAttribute dn = new DisplayNameAttribute ();
			Assert.IsNotNull (dn.DisplayName, "#1");
			Assert.AreEqual (string.Empty, dn.DisplayName, "#2");
			Assert.IsTrue (dn.IsDefaultAttribute (), "#3");
		}

		[Test]
		public void Constructor1 ()
		{
			DisplayNameAttribute dn = new DisplayNameAttribute (string.Empty);
			Assert.IsNotNull (dn.DisplayName, "#A1");
			Assert.AreEqual (string.Empty, dn.DisplayName, "#A2");
			Assert.IsTrue (dn.IsDefaultAttribute (), "#A3");

			dn = new DisplayNameAttribute (null);
			Assert.IsNull (dn.DisplayName, "#B1");
			Assert.IsFalse (dn.IsDefaultAttribute (), "#B2");

			dn = new DisplayNameAttribute ("category");
			Assert.IsNotNull (dn.DisplayName, "#C1");
			Assert.AreEqual ("category", dn.DisplayName, "#C2");
			Assert.IsFalse (dn.IsDefaultAttribute (), "#C3");
		}

		[Test]
		public void Default ()
		{
			DisplayNameAttribute dn = DisplayNameAttribute.Default;
			Assert.IsNotNull (dn.DisplayName, "#1");
			Assert.AreEqual (string.Empty, dn.DisplayName, "#2");
			Assert.IsTrue (dn.IsDefaultAttribute (), "#3");
		}

		[Test]
		public void Equals ()
		{
			DisplayNameAttribute dn = new DisplayNameAttribute ();
			Assert.IsTrue (dn.Equals (DisplayNameAttribute.Default), "#A1");
			Assert.IsTrue (dn.Equals (new DisplayNameAttribute (string.Empty)), "#A2");
			Assert.IsFalse (dn.Equals (new DisplayNameAttribute ("category")), "#A3");
			Assert.IsFalse (dn.Equals (new DisplayNameAttribute (null)), "#A4");
			Assert.IsFalse (dn.Equals (null), "#A5");
			Assert.IsTrue (dn.Equals (dn), "#A6");
			Assert.IsFalse (dn.Equals (55), "#A7");

			dn = new DisplayNameAttribute ("category");
			Assert.IsFalse (dn.Equals (DisplayNameAttribute.Default), "#B1");
			Assert.IsFalse (dn.Equals (new DisplayNameAttribute (string.Empty)), "#B2");
			Assert.IsTrue (dn.Equals (new DisplayNameAttribute ("category")), "#B3");
			Assert.IsFalse (dn.Equals (new DisplayNameAttribute (null)), "#B4");
			Assert.IsFalse (dn.Equals (null), "#B5");
			Assert.IsTrue (dn.Equals (dn), "#B6");
			Assert.IsFalse (dn.Equals (55), "#B7");
		}

		[Test]
		public void GetHashCodeTest ()
		{
			DisplayNameAttribute dn = new DisplayNameAttribute ();
			Assert.AreEqual (string.Empty.GetHashCode (), dn.GetHashCode (), "#A1");
			dn = new DisplayNameAttribute ("A");
			Assert.AreEqual ("A".GetHashCode (), dn.GetHashCode (), "#A2");

			// https://connect.microsoft.com/VisualStudio/feedback/ViewFeedback.aspx?FeedbackID=288534
			dn = new DisplayNameAttribute (null);
			try {
				dn.GetHashCode ();
				Assert.Fail ("#B1");
			} catch (NullReferenceException) {
			}
		}

		[Test]
		public void TestEmptyName ()
		{
			Type tc1t = tc1.GetType ();
			DisplayNameAttribute dn = GetAttribute (tc1t);
			Assert.IsNotNull (dn, "#1_1");
			Assert.IsFalse (dn.DisplayName == null, "#1_2");
			Assert.AreEqual (dn.DisplayName, "", "#1_3");

			dn = GetAttribute (tc1t, "Property1", MemberTypes.Property);
			Assert.IsNotNull (dn, "#2_1");
			Assert.IsFalse (dn.DisplayName == null, "#2_2");
			Assert.AreEqual (dn.DisplayName, "", "#2_3");
			
			dn = GetAttribute (tc1t, "Method1", MemberTypes.Method);
			Assert.IsNotNull (dn, "#3_1");
			Assert.IsFalse (dn.DisplayName == null, "#3_2");
			Assert.AreEqual (dn.DisplayName, "", "#3_3");
		}

		[Test]
		public void TestNonEmptyName ()
		{
			Type tc2t = tc2.GetType ();
			DisplayNameAttribute dn = GetAttribute (tc2t);
			Assert.IsNotNull (dn, "#1_1");
			Assert.IsFalse (dn.DisplayName == null, "#1_2");
			Assert.AreEqual (dn.DisplayName, "TestClassTwo", "#1_3");

			dn = GetAttribute (tc2t, "Property2", MemberTypes.Property);
			Assert.IsNotNull (dn, "#2_1");
			Assert.IsFalse (dn.DisplayName == null, "#2_2");
			Assert.AreEqual (dn.DisplayName, "PropertyTwo", "#2_3");
			
			dn = GetAttribute (tc2t, "Method2", MemberTypes.Method);
			Assert.IsNotNull (dn, "#3_1");
			Assert.IsFalse (dn.DisplayName == null, "#3_2");
			Assert.AreEqual (dn.DisplayName, "MethodTwo", "#3_3");
		}

		[Test]
		public void TestNullName ()
		{
			Type tc3t = tc3.GetType ();
			DisplayNameAttribute dn = GetAttribute (tc3t);
			Assert.IsNotNull (dn, "#1_1");
			Assert.IsNull (dn.DisplayName, "#1_2");
			
			dn = GetAttribute (tc3t, "Property3", MemberTypes.Property);
			Assert.IsNotNull (dn, "#2_1");
			Assert.IsNull (dn.DisplayName, "#2_2");
			
			dn = GetAttribute (tc3t, "Method3", MemberTypes.Method);
			Assert.IsNotNull (dn, "#3_1");
			Assert.IsNull (dn.DisplayName, "#3_2");
		}
		
		[Test]
		public void TestDefaultAttribute ()
		{
			Type tc1t = tc1.GetType ();
			DisplayNameAttribute dn = GetAttribute (tc1t);
			Assert.IsNotNull (dn, "#1_1");
			Assert.IsTrue (dn.IsDefaultAttribute (), "#1_2");

			dn = GetAttribute (tc1t, "Property1", MemberTypes.Property);
			Assert.IsNotNull (dn, "#1_3");
			Assert.IsTrue (dn.IsDefaultAttribute (), "#1_4");

			dn = GetAttribute (tc1t, "Method1", MemberTypes.Method);
			Assert.IsNotNull (dn, "#1_5");
			Assert.IsTrue (dn.IsDefaultAttribute (), "#1_6");

			Type tc2t = tc2.GetType ();
			dn = GetAttribute (tc2t);
			Assert.IsNotNull (dn, "#2_1");
			Assert.IsFalse (dn.IsDefaultAttribute (), "#2_2");

			dn = GetAttribute (tc2t, "Property2", MemberTypes.Property);
			Assert.IsNotNull (dn, "#2_3");
			Assert.IsFalse (dn.IsDefaultAttribute (), "#2_4");

			dn = GetAttribute (tc2t, "Method2", MemberTypes.Method);
			Assert.IsNotNull (dn, "#2_5");
			Assert.IsFalse (dn.IsDefaultAttribute (), "#2_6");

			Type tc3t = tc3.GetType ();
			dn = GetAttribute (tc3t);
			Assert.IsNotNull (dn, "#3_1");
			Assert.IsFalse (dn.IsDefaultAttribute (), "#3_2");

			dn = GetAttribute (tc3t, "Property3", MemberTypes.Property);
			Assert.IsNotNull (dn, "#3_3");
			Assert.IsFalse (dn.IsDefaultAttribute (), "#3_4");

			dn = GetAttribute (tc3t, "Method3", MemberTypes.Method);
			Assert.IsNotNull (dn, "#3_5");
			Assert.IsFalse (dn.IsDefaultAttribute (), "#3_6");
		}
	}
}
#endif
