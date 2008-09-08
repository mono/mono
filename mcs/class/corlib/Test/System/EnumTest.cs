// EnumTest.cs - NUnit Test Cases for the System.Enum class
//
// David Brandt (bucky@keystreams.com)
// Gert Driesen (drieseng@users.sourceforge.net)
//
// (C) Ximian, Inc.  http://www.ximian.com
// 

using System;
using System.IO;
using System.Reflection;

using NUnit.Framework;

namespace MonoTests.System
{
	[TestFixture]
	public class EnumTest
	{
		[Test]
		public void TestCompareTo ()
		{
			Enum e1 = new TestingEnum ();
			Enum e2 = new TestingEnum ();
			Enum e3 = new TestingEnum2 ();

			Assert.AreEqual (0, e1.CompareTo (e1), "#A1");
			Assert.AreEqual (0, e1.CompareTo (e2), "#A2");

			TestingEnum x = TestingEnum.This;
			TestingEnum y = TestingEnum.Is;
			Assert.AreEqual (0, x.CompareTo (x), "#B1");
			Assert.AreEqual (-1, x.CompareTo (y), "#B2");
			Assert.AreEqual (1, y.CompareTo (x), "#B3");

			try {
				e1.CompareTo (e3);
				Assert.Fail ("#C1");
			} catch (ArgumentException ex) {
				// Object must be the same type as the enum.
				// The type passed in was MonoTests.System.EnumTest+TestingEnum2;
				// the enum type was MonoTests.System.EnumTest+TestingEnum
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.IsTrue (ex.Message.IndexOf (typeof (TestingEnum).FullName) != -1, "#A5");
				Assert.IsTrue (ex.Message.IndexOf (typeof (TestingEnum2).FullName) != -1, "#A6");
				Assert.IsNull (ex.ParamName, "#A7");
			}

			try {
				((Enum) e1).CompareTo ((Enum) e3);
				Assert.Fail ("#D1");
			} catch (ArgumentException ex) {
				// Object must be the same type as the enum.
				// The type passed in was MonoTests.System.EnumTest+TestingEnum2;
				// the enum type was MonoTests.System.EnumTest+TestingEnum
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#D2");
				Assert.IsNull (ex.InnerException, "#D3");
				Assert.IsNotNull (ex.Message, "#D4");
				Assert.IsTrue (ex.Message.IndexOf (typeof (TestingEnum).FullName) != -1, "#D5");
				Assert.IsTrue (ex.Message.IndexOf (typeof (TestingEnum2).FullName) != -1, "#D6");
				Assert.IsNull (ex.ParamName, "#D7");
			}
		}

		[Test]
		public void TestEquals ()
		{
			Enum e1 = new TestingEnum ();
			Enum e2 = new TestingEnum ();
			Enum e3 = new TestingEnum2 ();

			Assert.IsTrue (e1.Equals (e1), "#1");
			Assert.IsTrue (e1.Equals (e2), "#2");
			Assert.IsFalse (e1.Equals (e3), "#3");
			Assert.IsFalse (e1.Equals (null), "#4");
		}

		[Test]
		public void TestFormat_Args ()
		{
			try {
				TestingEnum x = TestingEnum.Test;
				Enum.Format (null, x, "G");
				Assert.Fail ("#A1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.IsNotNull (ex.ParamName, "#A5");
				Assert.AreEqual ("enumType", ex.ParamName, "#A6");
			}

			try {
				Enum.Format (typeof (TestingEnum), null, "G");
				Assert.Fail ("#B1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
				Assert.IsNotNull (ex.ParamName, "#B5");
				Assert.AreEqual ("value", ex.ParamName, "#B6");
			}

			try {
				TestingEnum x = TestingEnum.Test;
				Enum.Format (x.GetType (), x, null);
				Assert.Fail ("#C1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#C2");
				Assert.IsNull (ex.InnerException, "#C3");
				Assert.IsNotNull (ex.Message, "#C4");
				Assert.IsNotNull (ex.ParamName, "#C5");
				Assert.AreEqual ("format", ex.ParamName, "#C6");
			}

			try {
				TestingEnum x = TestingEnum.Test;
				Enum.Format (typeof (string), x, "G");
				Assert.Fail ("#D1");
			} catch (ArgumentException ex) {
				// Type provided must be an Enum
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.IsNotNull (ex.ParamName, "#A5");
				Assert.AreEqual ("enumType", ex.ParamName, "#A6");
			}

			try {
				TestingEnum x = TestingEnum.Test;
				TestingEnum2 y = TestingEnum2.Test;
				Enum.Format (y.GetType (), x, "G");
				Assert.Fail ("#E1");
			} catch (ArgumentException ex) {
				// Object must be the same type as the enum. The type passed in was
				// MonoTests.System.EnumTest.TestingEnum2; the enum type was
				// MonoTests.System.EnumTest.TestingEnum
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#E2");
				Assert.IsNull (ex.InnerException, "#E3");
				Assert.IsNotNull (ex.Message, "#E4");
				Assert.IsTrue (ex.Message.IndexOf (typeof (TestingEnum2).FullName) != -1, "#E5");
				Assert.IsTrue (ex.Message.IndexOf (typeof (TestingEnum).FullName) != -1, "#E6");
				Assert.IsNull (ex.ParamName, "#A7");
			}

			try {
				String bad = "huh?";
				TestingEnum x = TestingEnum.Test;
				Enum.Format (x.GetType (), bad, "G");
				Assert.Fail ("#F1");
			} catch (ArgumentException ex) {
				// Enum underlying type and the object must be the same type or
				// object. Type passed in was String.String; the enum underlying
				// was System.Int32
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#F2");
				Assert.IsNull (ex.InnerException, "#F3");
				Assert.IsNotNull (ex.Message, "#F4");
				Assert.IsTrue (ex.Message.IndexOf (typeof (string).FullName) != -1, "#F5");
				Assert.IsTrue (ex.Message.IndexOf (typeof (int).FullName) != -1, "#F6");
				Assert.IsNull (ex.ParamName, "#F7");
			}

			string [] codes = {"a", "b", "c", "ad", "e", "af", "ag", "h", 
				  "i", "j", "k", "l", "m", "n", "o", "p", 
				  "q", "r", "s", "t", "u", "v", "w", "ax", 
				  "y", "z"};
			foreach (string code in codes) {
				try {
					TestingEnum x = TestingEnum.Test;
					Enum.Format (x.GetType (), x, code);
					Assert.Fail ("#G1:" + code);
				} catch (FormatException ex) {
					// Format String can be only "G","g","X","x","F","f","D" or "d"
					Assert.AreEqual (typeof (FormatException), ex.GetType (), "#G2");
					Assert.IsNull (ex.InnerException, "#G3");
					Assert.IsNotNull (ex.Message, "#G4");
				}
			}

			TestingEnum test = TestingEnum.Test;
			Assert.AreEqual ("3", Enum.Format (test.GetType (), test, "d"), "#H1");
			Assert.AreEqual ("18446744073709551615", Enum.Format (typeof (TestingEnum3), TestingEnum3.Test, "d"), "#H2");
			Assert.AreEqual ("Test", Enum.Format (test.GetType (), test, "g"), "#H3");
			Assert.AreEqual ("00000003", Enum.Format (test.GetType (), test, "x"), "#H4");
			Assert.AreEqual ("Test", Enum.Format (test.GetType (), test, "f"), "#H5");
		}

		public void TestFormat_FormatSpecifier ()
		{
			ParameterAttributes pa =
				ParameterAttributes.In | ParameterAttributes.HasDefault;
			const string fFormatOutput = "In, HasDefault";
			const string xFormatOutput = "00001001";
			string fOutput = Enum.Format (pa.GetType (), pa, "f");
			Assert.AreEqual (fFormatOutput, fOutput, "#A1");
			string xOutput = Enum.Format (pa.GetType (), pa, "x");
			Assert.AreEqual (xFormatOutput, xOutput, "#A2");

			Assert.AreEqual ("00", TestingEnum4.This.ToString ("x"), "#B1");
			Assert.AreEqual ("00", TestingEnum4.This.ToString ("X"), "#B2");
#if !TARGET_JVM // This appears not to work under .Net
			Assert.AreEqual ("ff", TestingEnum4.Test.ToString ("x"), "#B3");
#endif // TARGET_JVM
			Assert.AreEqual ("FF", TestingEnum4.Test.ToString ("X"), "#B4");

			Assert.AreEqual ("0000", TestingEnum5.This.ToString ("x"), "#C1");
			Assert.AreEqual ("0000", TestingEnum5.This.ToString ("X"), "#C2");
#if !TARGET_JVM // This appears not to work under .Net
			Assert.AreEqual ("7fff", TestingEnum5.Test.ToString ("x"), "#C3");
#endif // TARGET_JVM
			Assert.AreEqual ("7FFF", TestingEnum5.Test.ToString ("X"), "#C4");

			Assert.AreEqual ("00000000", TestingEnum6.This.ToString ("x"), "#D1");
			Assert.AreEqual ("00000000", TestingEnum6.This.ToString ("X"), "#D2");
#if !TARGET_JVM // This appears not to work under .Net
			Assert.AreEqual ("7fffffff", TestingEnum6.Test.ToString ("x"), "#D3");
#endif // TARGET_JVM
			Assert.AreEqual ("7FFFFFFF", TestingEnum6.Test.ToString ("X"), "#D4");

			Assert.AreEqual ("0000000000000000", TestingEnum3.This.ToString ("x"), "#E1");
			Assert.AreEqual ("0000000000000000", TestingEnum3.This.ToString ("X"), "#E2");
#if !TARGET_JVM // This appears not to work under .Net
			Assert.AreEqual ("ffffffffffffffff", TestingEnum3.Test.ToString ("x"), "#E3");
#endif // TARGET_JVM
			Assert.AreEqual ("FFFFFFFFFFFFFFFF", TestingEnum3.Test.ToString ("X"), "#E4");
		}

		[Test]
		public void TestGetHashCode ()
		{
			Enum e1 = new TestingEnum ();
			Enum e2 = new TestingEnum2 ();

			Assert.AreEqual (e1.GetHashCode (), e1.GetHashCode ());
		}

		[Test]
		public void GetName ()
		{
			try {
				TestingEnum x = TestingEnum.Test;
				Enum.GetName (null, x);
				Assert.Fail ("#A1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.IsNotNull (ex.ParamName, "#A5");
				Assert.AreEqual ("enumType", ex.ParamName, "#A6");
			}

			try {
				TestingEnum x = TestingEnum.Test;
				Enum.GetName (x.GetType (), null);
				Assert.Fail ("#B1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
				Assert.IsNotNull (ex.ParamName, "#B5");
				Assert.AreEqual ("value", ex.ParamName, "#B6");
			}

			try {
				String bad = "huh?";
				TestingEnum x = TestingEnum.Test;
				Enum.GetName (bad.GetType (), x);
				Assert.Fail ("#C1");
			} catch (ArgumentException ex) {
				// Type provided must be an Enum
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#C2");
				Assert.IsNull (ex.InnerException, "#C3");
				Assert.IsNotNull (ex.Message, "#C4");
				Assert.IsNotNull (ex.ParamName, "#C5");
				Assert.AreEqual ("enumType", ex.ParamName, "#C6");
			}

			try {
				String bad = "huh?";
				TestingEnum x = TestingEnum.Test;
				Enum.GetName (x.GetType (), bad);
				Assert.Fail ("#D1");
			} catch (ArgumentException ex) {
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#D2");
				Assert.IsNull (ex.InnerException, "#D3");
				Assert.IsNotNull (ex.Message, "#D4");
				Assert.IsNotNull (ex.ParamName, "#D5");
				Assert.AreEqual ("value", ex.ParamName, "#D6");
			}

			TestingEnum a = TestingEnum.This;
			TestingEnum b = TestingEnum.Is;
			TestingEnum c = TestingEnum.A;
			TestingEnum2 d = TestingEnum2.Test;

			Assert.AreEqual ("This", Enum.GetName (a.GetType (), a), "#E1");
			Assert.AreEqual ("Is", Enum.GetName (b.GetType (), b), "#E2");
			Assert.AreEqual ("A", Enum.GetName (c.GetType (), c), "#E3");
			Assert.AreEqual ("Test", Enum.GetName (c.GetType (), d), "#E4");
		}

		[Test]
		public void TestGetNames ()
		{
			try {
				Enum.GetNames (null);
				Assert.Fail ("#A1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.IsNotNull (ex.ParamName, "#A5");
				Assert.AreEqual ("enumType", ex.ParamName, "#A6");
			}

			TestingEnum x = TestingEnum.This;
			string [] match = { "This", "Is", "A", "Test" };
			string [] names = Enum.GetNames (x.GetType ());
			Assert.IsNotNull (names, "#B1");
			Assert.AreEqual (match.Length, names.Length, "#B2");
			for (int i = 0; i < names.Length; i++)
				Assert.AreEqual (match [i], names [i], "#B3");
		}

		[Test]
		public void TestGetTypeCode ()
		{
			TestingEnum x = TestingEnum.This;
			TestingEnum y = new TestingEnum ();
			Assert.AreEqual (TypeCode.Int32, x.GetTypeCode (), "#1");
			Assert.AreEqual (TypeCode.Int32, y.GetTypeCode (), "#2");
		}

		[Test]
		public void TestGetUnderlyingType ()
		{
			try {
				Enum.GetUnderlyingType (null);
				Assert.Fail ("#A1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.IsNotNull (ex.ParamName, "#A5");
				Assert.AreEqual ("enumType", ex.ParamName, "#A6");
			}

			try {
				String bad = "huh?";
				Enum.GetUnderlyingType (bad.GetType ());
				Assert.Fail ("#B1");
			} catch (ArgumentException ex) {
				// Type provided must be an Enum
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
				Assert.IsNotNull (ex.ParamName, "#B5");
				Assert.AreEqual ("enumType", ex.ParamName, "#B6");
			}

			short sh = 5;
			int i = 5;
			Enum t1 = new TestingEnum ();
			Enum t2 = new TestShortEnum ();
			Assert.AreEqual (i.GetType (), Enum.GetUnderlyingType (t1.GetType ()), "#C1");
			Assert.AreEqual (sh.GetType (), Enum.GetUnderlyingType (t2.GetType ()), "#C2");
		}

		[Test]
		public void TestGetValues ()
		{
			try {
				Enum.GetValues (null);
				Assert.Fail ("#A1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.IsNotNull (ex.ParamName, "#A5");
				Assert.AreEqual ("enumType", ex.ParamName, "#A6");
			}

			try {
				String bad = "huh?";
				Enum.GetValues (bad.GetType ());
				Assert.Fail ("#B1");
			} catch (ArgumentException ex) {
				// Type provided must be an Enum
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
				Assert.IsNotNull (ex.ParamName, "#B5");
				Assert.AreEqual ("enumType", ex.ParamName, "#B6");
			}

			Enum t1 = new TestingEnum ();
			Array a1 = Enum.GetValues (t1.GetType ());
			for (int i = 0; i < a1.Length; i++)
				Assert.AreEqual ((TestingEnum) i, a1.GetValue (i), "#C1");

			Enum t2 = new TestShortEnum ();
			Array a2 = Enum.GetValues (t2.GetType ());
			for (short i = 0; i < a1.Length; i++)
				Assert.AreEqual ((TestShortEnum) i, a2.GetValue (i), "#C2");
		}

		[Test]
		public void TestIsDefined ()
		{
			try {
				Enum.IsDefined (null, 1);
				Assert.Fail ("#A1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.IsNotNull (ex.ParamName, "#A5");
				Assert.AreEqual ("enumType", ex.ParamName, "#A6");
			}

			try {
				TestingEnum x = TestingEnum.Test;
				Enum.IsDefined (x.GetType (), null);
				Assert.Fail ("#B1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
				Assert.IsNotNull (ex.ParamName, "#B5");
				Assert.AreEqual ("value", ex.ParamName, "#B6");
			}

			try {
				String bad = "huh?";
				int i = 4;
				Enum.IsDefined (bad.GetType (), i);
				Assert.Fail ("#C1");
			} catch (ArgumentException ex) {
				// Type provided must be an Enum
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#C2");
				Assert.IsNull (ex.InnerException, "#C3");
				Assert.IsNotNull (ex.Message, "#C4");
				Assert.IsNotNull (ex.ParamName, "#C5");
				Assert.AreEqual ("enumType", ex.ParamName, "#C6");
			}

			try {
				TestingEnum x = TestingEnum.Test;
				short i = 4;
				Enum.IsDefined (x.GetType (), i);
				Assert.Fail ("#D1");
			} catch (ArgumentException ex) {
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#D2");
				Assert.IsNull (ex.InnerException, "#D3");
				Assert.IsNotNull (ex.Message, "#D4");
				Assert.IsNull (ex.ParamName, "#D5");
			}

			Enum t1 = new TestingEnum ();
			int valCount = Enum.GetValues (t1.GetType ()).Length;
			for (int i = 0; i < valCount; i++)
				Assert.IsTrue (Enum.IsDefined (t1.GetType (), i), "#F1:" + i);
			Assert.IsFalse (Enum.IsDefined (t1.GetType (), valCount), "#F2");
			Assert.IsFalse (Enum.IsDefined (typeof (TestingEnum), "huh?"), "#F3");
		}

		[Test]
		public void TestParse1 ()
		{
			try {
				String name = "huh?";
				Enum.Parse (null, name);
				Assert.Fail ("#A1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.IsNotNull (ex.ParamName, "#A5");
				Assert.AreEqual ("enumType", ex.ParamName, "#A6");
			}

			try {
				TestingEnum x = TestingEnum.Test;
				Enum.Parse (x.GetType (), null);
				Assert.Fail ("#B1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
				Assert.IsNotNull (ex.ParamName, "#B5");
				Assert.AreEqual ("value", ex.ParamName, "#B6");
			}

			try {
				String bad = "huh?";
				Enum.Parse (bad.GetType (), bad);
				Assert.Fail ("#C1");
			} catch (ArgumentException ex) {
				// Type provided must be an Enum
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#C2");
				Assert.IsNull (ex.InnerException, "#C3");
				Assert.IsNotNull (ex.Message, "#C4");
				Assert.IsNotNull (ex.ParamName, "#C5");
				Assert.AreEqual ("enumType", ex.ParamName, "#C6");
			}

			try {
				TestingEnum x = TestingEnum.Test;
				String bad = "";
				Enum.Parse (x.GetType (), bad);
				Assert.Fail ("#D1");
			} catch (ArgumentException ex) {
				// Must specify valid information for parsing
				// in the string
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#D2");
				Assert.IsNull (ex.InnerException, "#D3");
				Assert.IsNotNull (ex.Message, "#D4");
				Assert.IsNull (ex.ParamName, "#D5");
			}

			try {
				TestingEnum x = TestingEnum.Test;
				String bad = " ";
				Enum.Parse (x.GetType (), bad);
				Assert.Fail ("#E1");
			} catch (ArgumentException ex) {
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#E2");
				Assert.IsNull (ex.InnerException, "#E3");
				Assert.IsNotNull (ex.Message, "#E4");
				Assert.IsNull (ex.ParamName, "#E5");
			}

			try {
				String bad = "huh?";
				TestingEnum x = TestingEnum.Test;
				Enum.Parse (x.GetType (), bad);
				Assert.Fail ("#F1");
			} catch (ArgumentException ex) {
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#F2");
				Assert.IsNull (ex.InnerException, "#F3");
				Assert.IsNotNull (ex.Message, "#F4");
				Assert.IsNull (ex.ParamName, "#F5");
			}

			TestingEnum t1 = new TestingEnum ();
			Assert.AreEqual (TestingEnum.This, Enum.Parse (t1.GetType (), "This"), "#G1");
			Assert.AreEqual (TestingEnum.Is, Enum.Parse (t1.GetType (), "Is"), "#G2");
			Assert.AreEqual (TestingEnum.A, Enum.Parse (t1.GetType (), "A"), "#G3");
			Assert.AreEqual (TestingEnum.Test, Enum.Parse (t1.GetType (), "Test"), "#G4");
			Assert.AreEqual (TestingEnum.Test, Enum.Parse (t1.GetType (), "    \n\nTest\t"), "#G5");
			Assert.AreEqual (TestingEnum.Is, Enum.Parse (t1.GetType (), "This,Is"), "#G6");
			Assert.AreEqual (TestingEnum.Test, Enum.Parse (t1.GetType (), "This,Test"), "#G7");
			Assert.AreEqual (TestingEnum.Test, Enum.Parse (t1.GetType (), "This,Is,A"), "#G8");
			Assert.AreEqual (TestingEnum.Test, Enum.Parse (t1.GetType (), "   \n\tThis \t\n,    Is,A \n"), "#G9");
		}

		[Test]
		public void TestParse2 ()
		{
			try {
				String name = "huh?";
				Enum.Parse (null, name, true);
				Assert.Fail ("#A1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.IsNotNull (ex.ParamName, "#A5");
				Assert.AreEqual ("enumType", ex.ParamName, "#A6");
			}

			try {
				TestingEnum x = TestingEnum.Test;
				Enum.Parse (x.GetType (), null, true);
				Assert.Fail ("#B1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
				Assert.IsNotNull (ex.ParamName, "#B5");
				Assert.AreEqual ("value", ex.ParamName, "#B6");
			}

			try {
				String bad = "huh?";
				Enum.Parse (bad.GetType (), bad, true);
				Assert.Fail ("#C1");
			} catch (ArgumentException ex) {
				// Type provided must be an Enum
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#D2");
				Assert.IsNull (ex.InnerException, "#D3");
				Assert.IsNotNull (ex.Message, "#D4");
				Assert.IsNotNull (ex.ParamName, "#D5");
				Assert.AreEqual ("enumType", ex.ParamName, "#D6");
			}

			try {
				TestingEnum x = TestingEnum.Test;
				String bad = "";
				Enum.Parse (x.GetType (), bad, true);
				Assert.Fail ("#E1");
			} catch (ArgumentException ex) {
				// Must specify valid information for parsing
				// in the string
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#E2");
				Assert.IsNull (ex.InnerException, "#E3");
				Assert.IsNotNull (ex.Message, "#E4");
				Assert.IsNull (ex.ParamName, "#E5");
			}

			try {
				TestingEnum x = TestingEnum.Test;
				String bad = " ";
				Enum.Parse (x.GetType (), bad, true);
				Assert.Fail ("#F1");
			} catch (ArgumentException ex) {
				// Must specify valid information for parsing
				// in the string
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#F2");
				Assert.IsNull (ex.InnerException, "#F3");
				Assert.IsNotNull (ex.Message, "#F4");
				Assert.IsFalse (ex.Message.IndexOf ("' '") != -1, "#F5");
				Assert.IsNull (ex.ParamName, "#F6");
			}

			try {
				String bad = "huh?";
				TestingEnum x = TestingEnum.Test;
				Enum.Parse (x.GetType (), bad, true);
				Assert.Fail ("#G1");
			} catch (ArgumentException ex) {
				// Requested value 'huh?' was not found
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#G2");
				Assert.IsNull (ex.InnerException, "#G3");
				Assert.IsNotNull (ex.Message, "#G4");
#if NET_2_0
				Assert.IsTrue (ex.Message.IndexOf ("'huh?'") != -1, "#G5");
#else
				Assert.IsTrue (ex.Message.IndexOf ("huh?") != -1, "#G5");
#endif
				Assert.IsNull (ex.ParamName, "#G6");
			}

			try {
				String bad = "test";
				TestingEnum x = TestingEnum.Test;
				Enum.Parse (x.GetType (), bad, false);
				Assert.Fail ("#H1");
			} catch (ArgumentException ex) {
				// Requested value 'test' was not found
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#H2");
				Assert.IsNull (ex.InnerException, "#H3");
				Assert.IsNotNull (ex.Message, "#H4");
#if NET_2_0
				Assert.IsTrue (ex.Message.IndexOf ("'test'") != -1, "#H5");
#else
				Assert.IsTrue (ex.Message.IndexOf ("test") != -1, "#H5");
#endif
				Assert.IsNull (ex.ParamName, "#H6");
			}

			TestingEnum t1 = new TestingEnum ();
			Assert.AreEqual (TestingEnum.This, Enum.Parse (t1.GetType (), "this", true), "#I1");
			Assert.AreEqual (TestingEnum.Is, Enum.Parse (t1.GetType (), "is", true), "#I2");
			Assert.AreEqual (TestingEnum.A, Enum.Parse (t1.GetType (), "a", true), "#I3");
			Assert.AreEqual (TestingEnum.Test, Enum.Parse (t1.GetType (), "test", true), "#I4");
			Assert.AreEqual (TestingEnum.Test, Enum.Parse (t1.GetType (), "    \n\ntest\t", true), "#I5");

			Assert.AreEqual (TestingEnum.Is, Enum.Parse (t1.GetType (), "This,is", true), "#J1");
			Assert.AreEqual (TestingEnum.Test, Enum.Parse (t1.GetType (), "This,test", true), "#J2");
			Assert.AreEqual (TestingEnum.Test, Enum.Parse (t1.GetType (), "This,is,A", true), "#J3");
			Assert.AreEqual (TestingEnum.Test, Enum.Parse (t1.GetType (), "   \n\tThis \t\n,    is,a \n", true), "#J4");
		}

		[Test]
		public void ParseValue ()
		{
			TestingEnum3 t1 = new TestingEnum3 ();
			Assert.AreEqual (TestingEnum3.Test, Enum.Parse (t1.GetType (), "18446744073709551615", false));
		}

		[Test]
		public void ToObject_EnumType_Int32 ()
		{
			object value = Enum.ToObject (typeof (TestingEnum), 0);
			Assert.AreEqual (TestingEnum.This, value, "#1");
			value = Enum.ToObject (typeof (TestingEnum), 2);
			Assert.AreEqual (TestingEnum.A, value, "#2");
		}

		[Test]
#if ONLY_1_1
		[Category ("NotDotNet")]
#endif
		public void ToObject_EnumType_UInt64 ()
		{
			object value = Enum.ToObject (typeof (TestingEnum3), 0);
			Assert.AreEqual (TestingEnum3.This, value, "#1");
			value = Enum.ToObject (typeof (TestingEnum3), 1);
			Assert.AreEqual (TestingEnum3.Is, value, "#2");
			value = Enum.ToObject (typeof (TestingEnum3), ulong.MaxValue);
			Assert.AreEqual (TestingEnum3.Test, value, "#3");
		}

		[Test]
		public void ToObject_EnumType_Byte ()
		{
			object value = Enum.ToObject (typeof (TestingEnum4), 0);
			Assert.AreEqual (TestingEnum4.This, value, "#1");
			value = Enum.ToObject (typeof (TestingEnum4), byte.MaxValue);
			Assert.AreEqual (TestingEnum4.Test, value, "#2");
		}

		[Test]
		public void ToObject_EnumType_Int16 ()
		{
			object value = Enum.ToObject (typeof (TestingEnum5), 0);
			Assert.AreEqual (TestingEnum5.This, value, "#1");
			value = Enum.ToObject (typeof (TestingEnum5), short.MaxValue);
			Assert.AreEqual (TestingEnum5.Test, value, "#2");
		}

		[Test]
		public void ToObject_EnumType_Invalid ()
		{
			try {
				Enum.ToObject (typeof (string), 1);
				Assert.Fail ("#1");
			} catch (ArgumentException ex) {
				// Type provided must be an Enum
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("enumType", ex.ParamName, "#6");
			}
		}

		[Test]
		public void ToObject_EnumType_Null ()
		{
			try {
				Enum.ToObject (null, 1);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("enumType", ex.ParamName, "#6");
			}
		}

		[Test]
		public void ToObject_Value_Null ()
		{
			try {
				Enum.ToObject (typeof (TestingEnum), (object) null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("value", ex.ParamName, "#6");
			}
		}

		[Test]
		public void ToObject_Value_Invalid ()
		{
			try {
				Enum.ToObject (typeof (TestingEnum), "This");
				Assert.Fail ("#1");
			} catch (ArgumentException ex) {
				// The value passed in must be an enum base or
				// an underlying type for an enum, such as an
				// Int32
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("value", ex.ParamName, "#6");
			}
		}

		[Test]
		public void TestToString ()
		{
			Assert.AreEqual ("This", TestingEnum.This.ToString (), "#A1");
			Assert.AreEqual ("Is", TestingEnum.Is.ToString (), "#A2");
			Assert.AreEqual ("A", TestingEnum.A.ToString (), "#A3");
			Assert.AreEqual ("Test", TestingEnum.Test.ToString (), "#A4");

			Enum is1 = TestingEnum.Is;

			Assert.AreEqual ("1", is1.ToString ("d"), "#B1");
			Assert.AreEqual ("Is", is1.ToString ("g"), "#B2");
			Assert.AreEqual ("00000001", is1.ToString ("x"), "#B3");
			Assert.AreEqual ("Is", is1.ToString ("f"), "#B4");

			Assert.AreEqual ("b, c", ((SomeEnum) 3).ToString ("f"), "#C1");
			Assert.AreEqual ("b, c", ((SomeByteEnum) 3).ToString ("f"), "#C2");
			Assert.AreEqual ("b, c", ((SomeInt64Enum) 3).ToString ("f"), "#C3");

			Assert.AreEqual ("12", ((SomeEnum) 12).ToString ("f"), "#D1");
			Assert.AreEqual ("12", ((SomeByteEnum) 12).ToString ("f"), "#D2");
			Assert.AreEqual ("12", ((SomeInt64Enum) 12).ToString ("f"), "#D3");
		}

		[Test]
		public void FlagTest ()
		{
			int [] evalues = new int [4] { 0, 1, 2, 3 };
			E [] e = new E [4] { E.Aa, E.Bb, E.Cc, E.Dd };

			for (int i = 0; i < 4; ++i)
				Assert.AreEqual (e [i].ToString (),
					Enum.Format (typeof (E), evalues [i], "f"),
					"#1" + i);

			int invalidValue = 1000;

			Assert.AreEqual (invalidValue.ToString (),
				Enum.Format (typeof (E2), invalidValue, "g"),
				"#2");
		}

		[Test]
		public void FlagTest_Negative ()
		{
			FlagsNegativeTestEnum t;

			t = FlagsNegativeTestEnum.None;
			Assertion.AssertEquals ("#01", "None", t.ToString ());
			t = FlagsNegativeTestEnum.One;
			Assertion.AssertEquals ("#02", "One", t.ToString ());
		}

		[Test]
		public void AnotherFormatBugPinned ()
		{
			Assert.AreEqual ("100", Enum.Format (typeof (E3), 100, "f"));
		}

		[Test]
		public void LogicBugPinned ()
		{
			string format = null;
			string [] names = new string [] { "A", "B", "C", "D", };
			string [] fmtSpl = null;
			UE ue = UE.A | UE.B | UE.C | UE.D;

			//all flags must be in format return
			format = Enum.Format (typeof (UE), ue, "f");
			fmtSpl = format.Split (',');
			for (int i = 0; i < fmtSpl.Length; ++i)
				fmtSpl [i] = fmtSpl [i].Trim ();

			foreach (string nval in fmtSpl)
				Assert.IsTrue (Array.IndexOf (names, nval) >= 0, "#1:" + nval);

			foreach (string nval in names)
				Assert.IsTrue (Array.IndexOf (fmtSpl, nval) >= 0, "#2:" + nval);
		}
		// TODO - ToString with IFormatProviders

		[Test]
		public void GetHashCode_ShouldBeEqualToUnderlyingType ()
		{
			Assert.AreEqual (EnInt8.A.GetHashCode(), SByte.MinValue, "i8#0");
			Assert.AreEqual (EnInt8.B.GetHashCode(), 44, "i8#1");
			Assert.AreEqual (EnInt8.C.GetHashCode(), SByte.MaxValue, "i8#2");
	
			Assert.AreEqual (EnUInt8.A.GetHashCode(), Byte.MinValue, "u8#0");
			Assert.AreEqual (EnUInt8.B.GetHashCode(), 44, "u8#1");
			Assert.AreEqual (EnUInt8.C.GetHashCode(), Byte.MaxValue, "u8#2");
	
			Assert.AreEqual (EnInt16.A.GetHashCode(), Int16.MinValue, "i16#0");
			Assert.AreEqual (EnInt16.B.GetHashCode(), 44, "i16#1");
			Assert.AreEqual (EnInt16.C.GetHashCode(), Int16.MaxValue, "i16#2");
	
			Assert.AreEqual (EnUInt16.A.GetHashCode(), UInt16.MinValue, "u16#0");
			Assert.AreEqual (EnUInt16.B.GetHashCode(), 44, "u16#1");
			Assert.AreEqual (EnUInt16.C.GetHashCode(), UInt16.MaxValue, "u16#2");
	
			Assert.AreEqual (EnInt32.A.GetHashCode(), Int32.MinValue, "i32#0");
			Assert.AreEqual (EnInt32.B.GetHashCode(), 44, "i32#1");
			Assert.AreEqual (EnInt32.C.GetHashCode(), Int32.MaxValue, "i32#2");
	
			Assert.AreEqual (EnUInt32.A.GetHashCode(), UInt32.MinValue, "u32#0");
			Assert.AreEqual (EnUInt32.B.GetHashCode(), 44, "u32#1");
			Assert.AreEqual (EnUInt32.C.GetHashCode(), UInt32.MaxValue.GetHashCode (), "u32#2");
	
			Assert.AreEqual (EnInt64.A.GetHashCode(), Int64.MinValue.GetHashCode (), "i64#0");
			Assert.AreEqual (EnInt64.B.GetHashCode(), 3488924689489L.GetHashCode (), "i64#1");
			Assert.AreEqual (EnInt64.C.GetHashCode(), Int64.MaxValue.GetHashCode (), "i64#2");
	
			Assert.AreEqual (EnUInt64.A.GetHashCode(), UInt64.MinValue.GetHashCode (), "u64#0");
			Assert.AreEqual (EnUInt64.B.GetHashCode(), 3488924689489L.GetHashCode (), "u64#1");
			Assert.AreEqual (EnUInt64.C.GetHashCode(), UInt64.MaxValue.GetHashCode (), "u64#2");
		}

		[Flags]
		enum SomeEnum
		{
			a,
			b,
			c
		}

		[Flags]
		enum SomeByteEnum : byte
		{
			a,
			b,
			c
		}

		[Flags]
		enum SomeInt64Enum : long
		{
			a,
			b,
			c
		}

		enum TestShortEnum : short
		{
			zero,
			one,
			two,
			three,
			four,
			five,
			six
		}

		enum E
		{
			Aa = 0,
			Bb = 1,
			Cc = 2,
			Dd = 3,
		}

		[Flags]
		enum E2
		{
			Aa,
			Bb,
			Cc,
			Dd,
		}

		[Flags]
		enum FlagsNegativeTestEnum
		{
			None = 0,
			One = 1,
			Two = 2,
			Negative = unchecked ((int) 0xFFFF0000)
		}

		enum TestingEnum
		{
			This,
			Is,
			A,
			Test
		}

		enum TestingEnum2
		{
			This,
			Is,
			A,
			Test
		}

		enum TestingEnum3 : ulong
		{
			This,
			Is,
			A,
			Test = ulong.MaxValue
		}

		enum TestingEnum4 : byte
		{
			This,
			Is,
			A,
			Test = byte.MaxValue
		}

		enum TestingEnum5 : short
		{
			This,
			Is,
			A,
			Test = short.MaxValue
		}

		enum TestingEnum6
		{
			This,
			Is,
			A,
			Test = int.MaxValue
		}

		enum E3
		{
			A = 0,
			B = 1,
			C = 2,
			D = 3
		}

		enum UE : ulong
		{
			A = 1,
			B = 2,
			C = 4,
			D = 8
		}

		enum EA
		{
			A = 0,
			B = 2,
			C = 3,
			D = 4
		}
		
		enum EnInt8 : sbyte{
			A = SByte.MinValue,
			B = 44,
			C = SByte.MaxValue,
		}
		
		enum EnUInt8 : byte {
			A = Byte.MinValue,
			B = 44,
			C = Byte.MaxValue,
		}
		
		enum EnInt16 : short{
			A = Int16.MinValue,
			B = 44,
			C = Int16.MaxValue,
		}
		
		enum EnUInt16 : ushort {
			A = UInt16.MinValue,
			B = 44,
			C = UInt16.MaxValue,
		}
		
		enum EnInt32 : int{
			A = Int32.MinValue,
			B = 44,
			C = Int32.MaxValue,
		}
		
		enum EnUInt32 : uint {
			A = UInt32.MinValue,
			B = 44,
			C = UInt32.MaxValue,
		}
		
		enum EnInt64 : long{
			A = Int64.MinValue,
			B = 3488924689489L,
			C = Int64.MaxValue,
		}
		
		enum EnUInt64 : ulong {
			A = UInt64.MinValue,
			B = 3488924689489L,
			C = UInt64.MaxValue,
		}
	}
}
