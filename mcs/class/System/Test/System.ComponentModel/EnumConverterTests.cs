//
// System.ComponentModel.EnumConverter test cases
//
// Authors:
// 	Gert Driesen (drieseng@users.sourceforge.net)
//
// (c) 2007 Gert Driesen
//

using System;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Globalization;
using System.Reflection;

using NUnit.Framework;

namespace MonoTests.System.ComponentModel
{
	[TestFixture]
	public class EnumConverterTests
	{
		[Test]
		public void CanConvertFrom ()
		{
			EnumConverter converter = new EnumConverter (typeof (E));
			Assert.IsTrue (converter.CanConvertFrom (typeof (string)), "#A1");
			Assert.IsFalse (converter.CanConvertFrom (typeof (Enum)), "#A2");
			Assert.IsFalse (converter.CanConvertFrom (typeof (object)), "#A3");
			Assert.IsFalse (converter.CanConvertFrom (typeof (int)), "#A4");
			Assert.IsTrue (converter.CanConvertFrom (typeof (InstanceDescriptor)), "#A5");
			Assert.IsFalse (converter.CanConvertFrom (typeof (string [])), "#A6");
#if NET_2_0
			Assert.IsTrue (converter.CanConvertFrom (typeof (Enum [])), "#A7");
#else
			Assert.IsFalse (converter.CanConvertFrom (typeof (Enum [])), "#A7");
#endif

			converter = new EnumConverter (typeof (E2));
			Assert.IsTrue (converter.CanConvertFrom (typeof (string)), "#B1");
			Assert.IsFalse (converter.CanConvertFrom (typeof (Enum)), "#B2");
			Assert.IsFalse (converter.CanConvertFrom (typeof (object)), "#B3");
			Assert.IsFalse (converter.CanConvertFrom (typeof (int)), "#B4");
			Assert.IsTrue (converter.CanConvertFrom (typeof (InstanceDescriptor)), "#B5");
			Assert.IsFalse (converter.CanConvertFrom (typeof (string [])), "#B6");
#if NET_2_0
			Assert.IsTrue (converter.CanConvertFrom (typeof (Enum [])), "#B7");
#else
			Assert.IsFalse (converter.CanConvertFrom (typeof (Enum [])), "#B7");
#endif
		}

		[Test]
		public void CanConvertTo ()
		{
			EnumConverter converter = new EnumConverter (typeof (E));
			Assert.IsTrue (converter.CanConvertTo (typeof (string)), "#A1");
			Assert.IsFalse (converter.CanConvertTo (typeof (Enum)), "#A2");
			Assert.IsFalse (converter.CanConvertTo (typeof (object)), "#A3");
			Assert.IsFalse (converter.CanConvertTo (typeof (int)), "#A4");
			Assert.IsTrue (converter.CanConvertTo (typeof (InstanceDescriptor)), "#A5");
			Assert.IsFalse (converter.CanConvertTo (typeof (string [])), "#A6");
#if NET_2_0
			Assert.IsTrue (converter.CanConvertTo (typeof (Enum [])), "#A7");
#else
			Assert.IsFalse (converter.CanConvertTo (typeof (Enum [])), "#A7");
#endif

			converter = new EnumConverter (typeof (E2));
			Assert.IsTrue (converter.CanConvertTo (typeof (string)), "#B1");
			Assert.IsFalse (converter.CanConvertTo (typeof (Enum)), "#B2");
			Assert.IsFalse (converter.CanConvertTo (typeof (object)), "#B3");
			Assert.IsFalse (converter.CanConvertTo (typeof (object)), "#B4");
			Assert.IsTrue (converter.CanConvertTo (typeof (InstanceDescriptor)), "#B5");
			Assert.IsFalse (converter.CanConvertTo (typeof (string [])), "#B6");
#if NET_2_0
			Assert.IsTrue (converter.CanConvertTo (typeof (Enum [])), "#B7");
#else
			Assert.IsFalse (converter.CanConvertTo (typeof (Enum [])), "#B7");
#endif
		}

		[Test]
		public void ConvertFrom_Null ()
		{
			EnumConverter converter = new EnumConverter (typeof (E));
			try {
				converter.ConvertFrom (null, CultureInfo.InvariantCulture, null);
				Assert.Fail ("#1");
			} catch (NotSupportedException ex) {
				// EnumConverter cannot convert from (null)
				Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsTrue (ex.Message.IndexOf (typeof (EnumConverter).Name) != -1, "#5");
				Assert.IsTrue (ex.Message.IndexOf ("(null)") != -1, "#6");
			}
		}

#if NET_2_0
		[Test]
		public void ConvertFrom_EnumArray ()
		{
			EnumConverter converter = new EnumConverter (typeof (E));
			Assert.AreEqual (E.Aa, converter.ConvertFrom (null,
				CultureInfo.InvariantCulture,
				(Enum []) new Enum [] { E.Aa }), "#A1");
			Assert.AreEqual ((E) 8, converter.ConvertFrom (null,
				CultureInfo.InvariantCulture,
				(Enum []) new Enum [] { E.Aa, E2.Dd }), "#A2");
			Assert.AreEqual ((E) 958, converter.ConvertFrom (null,
				CultureInfo.InvariantCulture,
				(Enum []) new Enum [] { (E2) 444, (E) 666 }), "#A3");
			Assert.AreEqual ((E) 0, converter.ConvertFrom (null,
				CultureInfo.InvariantCulture,
				(Enum []) new Enum [0]), "#A4");

			converter = new EnumConverter (typeof (E2));
			Assert.AreEqual ((E2) 0, converter.ConvertFrom (null,
				CultureInfo.InvariantCulture,
				(Enum []) new Enum [] { E.Aa }), "#B1");
			Assert.AreEqual (E2.Dd, converter.ConvertFrom (null,
				CultureInfo.InvariantCulture,
				(Enum []) new Enum [] { E.Aa, E2.Dd }), "#B2");
			Assert.AreEqual ((E2) 958, converter.ConvertFrom (null,
				CultureInfo.InvariantCulture,
				(Enum []) new Enum [] { (E2) 444, (E) 666 }), "#B3");
			Assert.AreEqual ((E2) 0, converter.ConvertFrom (null,
				CultureInfo.InvariantCulture,
				(Enum []) new Enum [0]), "#B4");
			Assert.AreEqual (E2.Bb | E2.Dd, converter.ConvertFrom (null,
				CultureInfo.InvariantCulture,
				(Enum []) new Enum [] { E2.Bb, E2.Dd }), "#B5");
		}
#endif

		[Test]
		public void ConvertFrom_String ()
		{
			EnumConverter converter = new EnumConverter (typeof (E));
			Assert.AreEqual (E.Bb, converter.ConvertFrom (null, CultureInfo.InvariantCulture, "Bb"), "#A1");
			Assert.AreEqual (E.Cc, converter.ConvertFrom (null, CultureInfo.InvariantCulture, "2"), "#A2");
			Assert.AreEqual (E.Bb, converter.ConvertFrom (null, CultureInfo.InvariantCulture, " Bb "), "#A3");
			Assert.AreEqual (E.Dd, converter.ConvertFrom (null, CultureInfo.InvariantCulture, " 3 "), "#A4");
			Assert.AreEqual ((E) 666, converter.ConvertFrom (null, CultureInfo.InvariantCulture, "666"), "#A5");
			Assert.AreEqual (E.Dd, converter.ConvertFrom (null, CultureInfo.InvariantCulture, "Bb,Dd"), "#A6");
			Assert.AreEqual (E.Dd, converter.ConvertFrom (null, CultureInfo.InvariantCulture, "Dd,Bb"), "#A7");
			Assert.AreEqual (E.Bb, converter.ConvertFrom (null, CultureInfo.InvariantCulture, "Aa,Bb"), "#A8");

			try {
				converter.ConvertFrom (null, CultureInfo.InvariantCulture, string.Empty);
				Assert.Fail ("#B1");
#if NET_2_0
			} catch (FormatException ex) {
				//  is not a valid value for E
				Assert.AreEqual (typeof (FormatException), ex.GetType (), "#B2");
				Assert.IsNotNull (ex.InnerException, "#B3");
				Assert.AreEqual (typeof (ArgumentException), ex.InnerException.GetType (), "#B4");
				Assert.IsNotNull (ex.Message, "#B5");
				Assert.IsTrue (ex.Message.IndexOf ("E") != -1, "#B6");

				// Must specify valid information for parsing in the string
				ArgumentException inner = (ArgumentException) ex.InnerException;
				Assert.IsNull (inner.InnerException, "#B7");
				Assert.IsNotNull (inner.Message, "#B8");
			}
#else
			} catch (ArgumentException ex) {
				// Must specify valid information for parsing in the string
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
			}
#endif

			try {
				converter.ConvertFrom (null, CultureInfo.InvariantCulture, "YY");
				Assert.Fail ("#C1");
#if NET_2_0
			} catch (FormatException ex) {
				// YY is not a valid value for E
				Assert.AreEqual (typeof (FormatException), ex.GetType (), "#C2");
				Assert.IsNotNull (ex.InnerException, "#C3");
				Assert.AreEqual (typeof (ArgumentException), ex.InnerException.GetType (), "#C4");
				Assert.IsNotNull (ex.Message, "#C5");
				Assert.IsTrue (ex.Message.IndexOf ("YY") != -1, "#C6");
				Assert.IsTrue (ex.Message.IndexOf ("E") != -1, "#C7");

				// Requested value YY was not found
				ArgumentException inner = (ArgumentException) ex.InnerException;
				//Assert.IsNull (inner.InnerException, "#C8");
				Assert.IsNotNull (inner.Message, "#C9");
				Assert.IsTrue (inner.Message.IndexOf ("YY") != -1, "#C10");
				Assert.IsNull (inner.ParamName, "#C11");
			}
#else
			} catch (ArgumentException ex) {
				// Requested value YY was not found
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#C2");
				//Assert.IsNull (ex.InnerException, "#C3");
				Assert.IsNotNull (ex.Message, "#C4");
				Assert.IsTrue (ex.Message.IndexOf ("YY") != -1, "#C5");
				Assert.IsNull (ex.ParamName, "#C6");
			}
#endif
		}

		[Test]
#if TARGET_JVM
		[NUnit.Framework.Category("NotWorking")]
#endif
		public void ConvertFrom_String_Flags ()
		{
			EnumConverter converter = new EnumConverter (typeof (E2));
			Assert.AreEqual (E2.Cc, converter.ConvertFrom (null, CultureInfo.InvariantCulture, "Cc"), "#B1");
			Assert.AreEqual (E2.Dd, converter.ConvertFrom (null, CultureInfo.InvariantCulture, "8"), "#B2");
			Assert.AreEqual (E2.Cc | E2.Dd, converter.ConvertFrom (null, CultureInfo.InvariantCulture, "Cc,Dd"), "#B3");
			Assert.AreEqual (E2.Aa | E2.Bb, converter.ConvertFrom (null, CultureInfo.InvariantCulture, "3"), "#B4");
			Assert.AreEqual (E2.Bb | E2.Cc, converter.ConvertFrom (null, CultureInfo.InvariantCulture, "2,4"), "#B5");
			Assert.AreEqual (E2.Aa | E2.Dd, converter.ConvertFrom (null, CultureInfo.InvariantCulture, " 1 , 8 "), "#B5");
			Assert.AreEqual ((E2) 666, converter.ConvertFrom (null, CultureInfo.InvariantCulture, "666"), "#B6");
			Assert.AreEqual (E2.Cc | E2.Dd, converter.ConvertFrom (null, CultureInfo.InvariantCulture, " Dd , Cc "), "#B7");
			Assert.AreEqual (E2.Bb, converter.ConvertFrom (null, CultureInfo.InvariantCulture, " Bb "), "#B8");
			Assert.AreEqual (E2.Aa | E2.Bb, converter.ConvertFrom (null, CultureInfo.InvariantCulture, " 3 "), "#B9");

			try {
				converter.ConvertFrom (null, CultureInfo.InvariantCulture, string.Empty);
				Assert.Fail ("#B1");
#if NET_2_0
			} catch (FormatException ex) {
				//  is not a valid value for E2
				Assert.AreEqual (typeof (FormatException), ex.GetType (), "#B2");
				Assert.IsNotNull (ex.InnerException, "#B3");
				Assert.AreEqual (typeof (ArgumentException), ex.InnerException.GetType (), "#B4");
				Assert.IsNotNull (ex.Message, "#B5");
				Assert.IsTrue (ex.Message.IndexOf ("E2") != -1, "#B6");

				// Must specify valid information for parsing in the string
				ArgumentException inner = (ArgumentException) ex.InnerException;
				Assert.IsNull (inner.InnerException, "#B7");
				Assert.IsNotNull (inner.Message, "#B8");
			}
#else
			} catch (ArgumentException ex) {
				// Must specify valid information for parsing in the string
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
			}
#endif

			try {
				converter.ConvertFrom (null, CultureInfo.InvariantCulture, "Aa Bb");
				Assert.Fail ("#C1");
#if NET_2_0
			} catch (FormatException ex) {
				// Aa Bb is not a valid value for E2
				Assert.AreEqual (typeof (FormatException), ex.GetType (), "#C2");
				Assert.IsNotNull (ex.InnerException, "#C3");
				Assert.AreEqual (typeof (ArgumentException), ex.InnerException.GetType (), "#C4");
				Assert.IsNotNull (ex.Message, "#C5");
				Assert.IsTrue (ex.Message.IndexOf ("Aa Bb") != -1, "#C6");
				Assert.IsTrue (ex.Message.IndexOf ("E2") != -1, "#C7");

				// Requested value Aa Bb was not found
				ArgumentException inner = (ArgumentException) ex.InnerException;
				Assert.IsNotNull (inner.Message, "#C9");
				Assert.IsTrue (inner.Message.IndexOf ("Aa Bb") != -1, "#C10");
				Assert.IsNull (inner.ParamName, "#C11");
			}
#else
			} catch (ArgumentException ex) {
				// Requested value Aa Bb was not found
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#C2");
				Assert.IsNotNull (ex.Message, "#C3");
				Assert.IsTrue (ex.Message.IndexOf ("Aa Bb") != -1, "#C4");
				Assert.IsNull (ex.ParamName, "#C5");
			}
#endif

			try {
				converter.ConvertFrom (null, CultureInfo.InvariantCulture, "2,");
				Assert.Fail ("#D1");
#if NET_2_0
			} catch (FormatException ex) {
				// 2, is not a valid value for E2
				Assert.AreEqual (typeof (FormatException), ex.GetType (), "#D2");
				Assert.IsNotNull (ex.InnerException, "#D3");
				Assert.AreEqual (typeof (ArgumentException), ex.InnerException.GetType (), "#D4");
				Assert.IsNotNull (ex.Message, "#D5");
				Assert.IsTrue (ex.Message.IndexOf ("2,") != -1, "#D6");
				Assert.IsTrue (ex.Message.IndexOf ("E2") != -1, "#D7");

				// Must specify valid information for parsing in the string
				ArgumentException inner = (ArgumentException) ex.InnerException;
				Assert.IsNull (inner.InnerException, "#D8");
				Assert.IsNotNull (inner.Message, "#D9");
				Assert.IsFalse (inner.Message.IndexOf ("2,") != -1, "#D10");
			}
#else
			} catch (ArgumentException ex) {
				// Must specify valid information for parsing in the string
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#D2");
				Assert.IsNull (ex.InnerException, "#D3");
				Assert.IsNotNull (ex.Message, "#D4");
				Assert.IsFalse (ex.Message.IndexOf ("2,") != -1, "#D5");
			}
#endif
		}

#if NET_2_0
		[Test]
		public void ConvertTo_EnumArray ()
		{
			Enum [] enums;
			EnumConverter converter = new EnumConverter (typeof (E));

			enums = converter.ConvertTo (null, CultureInfo.InvariantCulture,
				E.Bb, typeof (Enum [])) as Enum [];
			Assert.IsNotNull (enums, "#A1");
			Assert.AreEqual (1, enums.Length, "#A2");
			Assert.AreEqual (typeof (E), enums [0].GetType (), "#A3");
			Assert.AreEqual (E.Bb, enums [0], "#A4");

			enums = converter.ConvertTo (null, CultureInfo.InvariantCulture,
				2, typeof (Enum [])) as Enum [];
			Assert.IsNotNull (enums, "#B1");
			Assert.AreEqual (1, enums.Length, "#B2");
			Assert.AreEqual (typeof (E), enums [0].GetType (), "#B3");
			Assert.AreEqual (E.Cc, enums [0], "#B4");

			try {
				enums = converter.ConvertTo (null, CultureInfo.InvariantCulture,
					"2", typeof (Enum [])) as Enum [];
				Assert.Fail ("#C1");
			} catch (ArgumentException ex) {
				// The value passed in must be an enum base or an
				// underlying type for an enum, such as an Int32
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#C2");
				Assert.IsNull (ex.InnerException, "#C3");
				Assert.IsNotNull (ex.Message, "#C4");
				Assert.IsNotNull (ex.ParamName, "#C5");
				Assert.AreEqual ("value", ex.ParamName, "#C6");
			}

			try {
				converter.ConvertTo (null, CultureInfo.InvariantCulture,
					null, typeof (Enum []));
				Assert.Fail ("#D1");
			} catch (NotSupportedException ex) {
				// 'EnumConverter' is unable to convert '(null)'
				// to 'System.Enum[]'
				Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#D2");
				Assert.IsNull (ex.InnerException, "#D3");
				Assert.IsNotNull (ex.Message, "#D4");
				Assert.IsTrue (ex.Message.IndexOf ("'" + typeof (EnumConverter).Name + "'") != -1, "#D5");
				Assert.IsTrue (ex.Message.IndexOf ("'(null)'") != -1, "#D6");
				Assert.IsTrue (ex.Message.IndexOf ("'" + typeof (Enum []).FullName + "'") != -1, "#D7");
			}

			try {
				converter.ConvertTo (null, CultureInfo.InvariantCulture,
					"Cc", typeof (Enum []));
				Assert.Fail ("#E1");
			} catch (ArgumentException ex) {
				// The value passed in must be an enum base or an
				// underlying type for an enum, such as an Int32
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#E2");
				Assert.IsNull (ex.InnerException, "#E3");
				Assert.IsNotNull (ex.Message, "#E4");
				Assert.IsNotNull (ex.ParamName, "#E5");
				Assert.AreEqual ("value", ex.ParamName, "#E6");
			}
		}

		[Test]
		public void ConvertTo_EnumArray_Flags ()
		{
			Enum [] enums;
			EnumConverter converter = new EnumConverter (typeof (E2));

			enums = converter.ConvertTo (null, CultureInfo.InvariantCulture,
				E.Bb, typeof (Enum [])) as Enum [];
			Assert.IsNotNull (enums, "#A1");
			Assert.AreEqual (1, enums.Length, "#A2");
			Assert.AreEqual (typeof (E2), enums [0].GetType (), "#A3");
			Assert.AreEqual (E2.Aa, enums [0], "#A4");

			enums = converter.ConvertTo (null, CultureInfo.InvariantCulture,
				E2.Bb, typeof (Enum [])) as Enum [];
			Assert.IsNotNull (enums, "#B1");
			Assert.AreEqual (1, enums.Length, "#B2");
			Assert.AreEqual (typeof (E2), enums [0].GetType (), "#B3");
			Assert.AreEqual (E2.Bb, enums [0], "#B4");

			enums = converter.ConvertTo (null, CultureInfo.InvariantCulture,
				(E2) 0, typeof (Enum [])) as Enum [];
			Assert.IsNotNull (enums, "#C1");
			Assert.AreEqual (0, enums.Length, "#C2");

			enums = converter.ConvertTo (null, CultureInfo.InvariantCulture,
				(E2) 18, typeof (Enum [])) as Enum [];
			Assert.IsNotNull (enums, "#D1");
			Assert.AreEqual (2, enums.Length, "#D2");
			Assert.AreEqual (typeof (E2), enums [0].GetType (), "#D3");
			Assert.AreEqual (E2.Bb, enums [0], "#D4");
			Assert.AreEqual (typeof (E2), enums [1].GetType (), "#D5");
			Assert.AreEqual ((E2) 16, enums [1], "#D6");

			try {
				enums = converter.ConvertTo (null, CultureInfo.InvariantCulture,
					5, typeof (Enum [])) as Enum [];
				Assert.Fail ("#E1");
			} catch (InvalidCastException ex) {
				// Unable to cast object of type 'System.Int32'
				// to type 'System.Enum'
				Assert.AreEqual (typeof (InvalidCastException), ex.GetType (), "#E2");
				Assert.IsNull (ex.InnerException, "#E3");
				Assert.IsNotNull (ex.Message, "#E4");
			}

			try {
				enums = converter.ConvertTo (null, CultureInfo.InvariantCulture,
					"2", typeof (Enum [])) as Enum [];
				Assert.Fail ("#F1");
			} catch (InvalidCastException ex) {
				// Unable to cast object of type 'System.String'
				// to type 'System.Enum'
				Assert.AreEqual (typeof (InvalidCastException), ex.GetType (), "#F2");
				Assert.IsNull (ex.InnerException, "#F3");
				Assert.IsNotNull (ex.Message, "#F4");
			}

			try {
				converter.ConvertTo (null, CultureInfo.InvariantCulture,
					null, typeof (Enum []));
				Assert.Fail ("#G1");
			} catch (NotSupportedException ex) {
				// 'EnumConverter' is unable to convert '(null)'
				// to 'System.Enum[]'
				Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#G2");
				Assert.IsNull (ex.InnerException, "#G3");
				Assert.IsNotNull (ex.Message, "#G4");
				Assert.IsTrue (ex.Message.IndexOf ("'" + typeof (EnumConverter).Name + "'") != -1, "#G5");
				Assert.IsTrue (ex.Message.IndexOf ("'(null)'") != -1, "#G6");
				Assert.IsTrue (ex.Message.IndexOf ("'" + typeof (Enum []).FullName + "'") != -1, "#G7");
			}

			try {
				converter.ConvertTo (null, CultureInfo.InvariantCulture,
					"Bb,Cc", typeof (Enum []));
				Assert.Fail ("#H1");
			} catch (InvalidCastException ex) {
				// Unable to cast object of type 'System.String'
				// to type 'System.Enum'
				Assert.AreEqual (typeof (InvalidCastException), ex.GetType (), "#H2");
				Assert.IsNull (ex.InnerException, "#H3");
				Assert.IsNotNull (ex.Message, "#H4");
			}

			try {
				converter.ConvertTo (null, CultureInfo.InvariantCulture,
					"2,4", typeof (Enum []));
				Assert.Fail ("#I1");
			} catch (InvalidCastException ex) {
				// Unable to cast object of type 'System.String'
				// to type 'System.Enum'
				Assert.AreEqual (typeof (InvalidCastException), ex.GetType (), "#I2");
				Assert.IsNull (ex.InnerException, "#I3");
				Assert.IsNotNull (ex.Message, "#I4");
			}

			converter = new EnumConverter (typeof (F2));

			enums = converter.ConvertTo (null, CultureInfo.InvariantCulture,
				(F2) 15, typeof (Enum [])) as Enum [];
			Assert.IsNotNull (enums, "#J1");
			Assert.AreEqual (3, enums.Length, "#J2");
			Assert.AreEqual (typeof (F2), enums [0].GetType (), "#J3");
			Assert.AreEqual (F2.Bb, enums [0], "#J4");
			Assert.AreEqual (typeof (F2), enums [1].GetType (), "#J5");
			Assert.AreEqual (F2.Dd, enums [1], "#J6");
			Assert.AreEqual (typeof (F2), enums [2].GetType (), "#J5");
			Assert.AreEqual ((F2) 5, enums [2], "#J6");

			// Test Flags conversion of enum value 0
			converter = new EnumConverter (typeof (E3));
			enums = converter.ConvertTo (null, CultureInfo.InvariantCulture,
				E3.Aa, typeof (Enum [])) as Enum [];
			Assert.AreEqual (1, enums.Length, "#H1");
			Assert.AreEqual (typeof (E3), enums [0].GetType (), "#H2");
			Assert.AreEqual (E3.Aa, enums[0], "#H3");
		}
#endif

		[Test]
		public void ConvertTo_InstanceDescriptor ()
		{
			InstanceDescriptor idesc;
			FieldInfo fi;
			EnumConverter converter = new EnumConverter (typeof (E));

			idesc = converter.ConvertTo (null, CultureInfo.InvariantCulture,
				E.Bb, typeof (InstanceDescriptor)) as InstanceDescriptor;
			Assert.IsNotNull (idesc, "#A1");
			Assert.IsNotNull (idesc.Arguments, "#A2");
			Assert.AreEqual (0, idesc.Arguments.Count, "#A3");
			Assert.AreEqual (typeof (object []), idesc.Arguments.GetType (), "#A4");
			Assert.IsTrue (idesc.IsComplete, "#A5");
			fi = idesc.MemberInfo as FieldInfo;
			Assert.IsNotNull (fi, "#A6");
			Assert.AreEqual (typeof (E), fi.DeclaringType, "#A7");
			Assert.AreEqual (typeof (E), fi.FieldType, "#A8");
			Assert.IsTrue (fi.IsStatic, "#A9");
			Assert.AreEqual ("Bb", fi.Name, "#A10");
			Assert.AreEqual (E.Bb, fi.GetValue (null), "#A11");


			idesc = converter.ConvertTo (null, CultureInfo.InvariantCulture,
				"2", typeof (InstanceDescriptor)) as InstanceDescriptor;
			Assert.IsNotNull (idesc, "#B1");
			Assert.IsNotNull (idesc.Arguments, "#B2");
			Assert.AreEqual (0, idesc.Arguments.Count, "#B3");
			Assert.AreEqual (typeof (object []), idesc.Arguments.GetType (), "#B4");
			Assert.IsTrue (idesc.IsComplete, "#B5");
			fi = idesc.MemberInfo as FieldInfo;
			Assert.IsNotNull (fi, "#B6");
			Assert.AreEqual (typeof (E), fi.DeclaringType, "#B7");
			Assert.AreEqual (typeof (E), fi.FieldType, "#B8");
			Assert.IsTrue (fi.IsStatic, "#B9");
			Assert.AreEqual ("Cc", fi.Name, "#B10");
			Assert.AreEqual (E.Cc, fi.GetValue (null), "#B11");


			idesc = converter.ConvertTo (null, CultureInfo.InvariantCulture,
				2, typeof (InstanceDescriptor)) as InstanceDescriptor;
			Assert.IsNotNull (idesc, "#C1");
			Assert.IsNotNull (idesc.Arguments, "#C2");
			Assert.AreEqual (0, idesc.Arguments.Count, "#C3");
			Assert.AreEqual (typeof (object []), idesc.Arguments.GetType (), "#C4");
			Assert.IsTrue (idesc.IsComplete, "#C5");
			fi = idesc.MemberInfo as FieldInfo;
			Assert.IsNotNull (fi, "#C6");
			Assert.AreEqual (typeof (E), fi.DeclaringType, "#C7");
			Assert.AreEqual (typeof (E), fi.FieldType, "#C8");
			Assert.IsTrue (fi.IsStatic, "#C9");
			Assert.AreEqual ("Cc", fi.Name, "#C10");
			Assert.AreEqual (E.Cc, fi.GetValue (null), "#C11");

			idesc = converter.ConvertTo (null, CultureInfo.InvariantCulture,
				(E) 2, typeof (InstanceDescriptor)) as InstanceDescriptor;
			Assert.IsNotNull (idesc, "#D1");
			Assert.IsNotNull (idesc.Arguments, "#D2");
			Assert.AreEqual (0, idesc.Arguments.Count, "#D3");
			Assert.AreEqual (typeof (object []), idesc.Arguments.GetType (), "#D4");
			Assert.IsTrue (idesc.IsComplete, "#D5");
			fi = idesc.MemberInfo as FieldInfo;
			Assert.IsNotNull (fi, "#D6");
			Assert.AreEqual (typeof (E), fi.DeclaringType, "#D7");
			Assert.AreEqual (typeof (E), fi.FieldType, "#D8");
			Assert.IsTrue (fi.IsStatic, "#D9");
			Assert.AreEqual ("Cc", fi.Name, "#D10");
			Assert.AreEqual (E.Cc, fi.GetValue (null), "#D11");

			try {
				converter.ConvertTo (null, CultureInfo.InvariantCulture,
					null, typeof (InstanceDescriptor));
				Assert.Fail ("#E1");
			} catch (NotSupportedException ex) {
				// 'EnumConverter' is unable to convert '(null)'
				// to 'System.ComponentModel.Design.Serialization.InstanceDescriptor'
				Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#E2");
				Assert.IsNull (ex.InnerException, "#E3");
				Assert.IsNotNull (ex.Message, "#E4");
				Assert.IsTrue (ex.Message.IndexOf ("'" + typeof (EnumConverter).Name + "'") != -1, "#E5");
				Assert.IsTrue (ex.Message.IndexOf ("'(null)'") != -1, "#E6");
				Assert.IsTrue (ex.Message.IndexOf ("'" + typeof (InstanceDescriptor).FullName + "'") != -1, "#E7");
			}

			try {
				converter.ConvertTo (null, CultureInfo.InvariantCulture,
					"5", typeof (InstanceDescriptor));
				Assert.Fail ("#F1");
			} catch (ArgumentException ex) {
				// The value '5' is not a valid value for the enum 'E'
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#F2");
				Assert.IsNull (ex.InnerException, "#F3");
				Assert.IsNotNull (ex.Message, "#F4");
				Assert.IsTrue (ex.Message.IndexOf ("'5'") != -1, "#F5");
				Assert.IsTrue (ex.Message.IndexOf ("'E'") != -1, "#F6");
				Assert.IsNull (ex.ParamName, "#F7");
			}

			try {
				converter.ConvertTo (null, CultureInfo.InvariantCulture,
					"Cc", typeof (InstanceDescriptor));
				Assert.Fail ("#G1");
			} catch (FormatException ex) {
				// Input string was not in a correct format
				Assert.AreEqual (typeof (FormatException), ex.GetType (), "#G2");
				Assert.IsNull (ex.InnerException, "#G3");
				Assert.IsNotNull (ex.Message, "#G4");
			}

			try {
				converter.ConvertTo (null, CultureInfo.InvariantCulture,
					(E) 666, typeof (InstanceDescriptor));
				Assert.Fail ("#F1");
			} catch (ArgumentException ex) {
				// The value '666' is not a valid value for the enum 'E'
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#F2");
				Assert.IsNull (ex.InnerException, "#F3");
				Assert.IsNotNull (ex.Message, "#F4");
				Assert.IsTrue (ex.Message.IndexOf ("'666'") != -1, "#F5");
				Assert.IsTrue (ex.Message.IndexOf ("'E'") != -1, "#F6");
				Assert.IsNull (ex.ParamName, "#F7");
			}
		}

		[Test]
		public void ConvertTo_InstanceDescriptor_Flags ()
		{
			InstanceDescriptor idesc;
			FieldInfo fi;
			MethodInfo mi;
			ParameterInfo [] parameters;
			object [] arguments;
			EnumConverter converter = new EnumConverter (typeof (E2));

			idesc = converter.ConvertTo (null, CultureInfo.InvariantCulture,
				E2.Bb, typeof (InstanceDescriptor)) as InstanceDescriptor;
			Assert.IsNotNull (idesc, "#A1");
			Assert.IsNotNull (idesc.Arguments, "#A2");
			Assert.AreEqual (0, idesc.Arguments.Count, "#A3");
			Assert.AreEqual (typeof (object []), idesc.Arguments.GetType (), "#A4");
			Assert.IsTrue (idesc.IsComplete, "#A5");
			fi = idesc.MemberInfo as FieldInfo;
			Assert.IsNotNull (fi, "#A6");
			Assert.AreEqual (typeof (E2), fi.DeclaringType, "#A7");
			Assert.AreEqual (typeof (E2), fi.FieldType, "#A8");
			Assert.IsTrue (fi.IsStatic, "#A9");
			Assert.AreEqual ("Bb", fi.Name, "#A10");
			Assert.AreEqual (E2.Bb, fi.GetValue (null), "#A11");

			idesc = converter.ConvertTo (null, CultureInfo.InvariantCulture,
				E.Bb, typeof (InstanceDescriptor)) as InstanceDescriptor;
			Assert.IsNotNull (idesc, "#B1");
			Assert.IsNotNull (idesc.Arguments, "#B2");
			Assert.AreEqual (0, idesc.Arguments.Count, "#B3");
			Assert.AreEqual (typeof (object []), idesc.Arguments.GetType (), "#B4");
			Assert.IsTrue (idesc.IsComplete, "#B5");
			fi = idesc.MemberInfo as FieldInfo;
			Assert.IsNotNull (fi, "#B6");
			Assert.AreEqual (typeof (E2), fi.DeclaringType, "#B7");
			Assert.AreEqual (typeof (E2), fi.FieldType, "#B8");
			Assert.IsTrue (fi.IsStatic, "#B9");
			Assert.AreEqual ("Aa", fi.Name, "#B10");
			Assert.AreEqual (E2.Aa, fi.GetValue (null), "#B11");

			idesc = converter.ConvertTo (null, CultureInfo.InvariantCulture,
				E2.Bb | E2.Dd, typeof (InstanceDescriptor)) 
				as InstanceDescriptor;
			Assert.IsNotNull (idesc, "#C1");
			Assert.IsNotNull (idesc.Arguments, "#C2");
			Assert.AreEqual (2, idesc.Arguments.Count, "#C3");
			Assert.AreEqual (typeof (object []), idesc.Arguments.GetType (), "#C4");
			arguments = (object []) idesc.Arguments;
			Assert.AreEqual (typeof (E2), arguments [0], "#C5");
			Assert.AreEqual (typeof (int), arguments [1].GetType (), "#C6");
			Assert.AreEqual (10, arguments [1], "#C7");
			Assert.IsTrue (idesc.IsComplete, "#C8");
			mi = idesc.MemberInfo as MethodInfo;
			Assert.IsNotNull (mi, "#C9");
			Assert.AreEqual ("ToObject", mi.Name, "#C10");
			Assert.AreEqual (typeof (Enum), mi.DeclaringType, "#C11");
			parameters = mi.GetParameters ();
			Assert.AreEqual (2, parameters.Length, "#C12");
			Assert.AreEqual (typeof (Type), parameters [0].ParameterType, "#C13");
			Assert.AreEqual (typeof (int), parameters [1].ParameterType, "#C14");

			idesc = converter.ConvertTo (null, CultureInfo.InvariantCulture,
				"5", typeof (InstanceDescriptor)) as InstanceDescriptor;
			Assert.IsNotNull (idesc, "#D1");
			Assert.IsNotNull (idesc.Arguments, "#D2");
			Assert.AreEqual (2, idesc.Arguments.Count, "#D3");
			Assert.AreEqual (typeof (object []), idesc.Arguments.GetType (), "#D4");
			arguments = (object []) idesc.Arguments;
			Assert.AreEqual (typeof (E2), arguments [0], "#D5");
			Assert.AreEqual (typeof (int), arguments [1].GetType (), "#D6");
			Assert.AreEqual (5, arguments [1], "#D7");
			Assert.IsTrue (idesc.IsComplete, "#D8");
			mi = idesc.MemberInfo as MethodInfo;
			Assert.IsNotNull (mi, "#D9");
			Assert.AreEqual ("ToObject", mi.Name, "#D10");
			Assert.AreEqual (typeof (Enum), mi.DeclaringType, "#D11");
			parameters = mi.GetParameters ();
			Assert.AreEqual (2, parameters.Length, "#D12");
			Assert.AreEqual (typeof (Type), parameters [0].ParameterType, "#D13");
			Assert.AreEqual (typeof (int), parameters [1].ParameterType, "#D14");

			idesc = converter.ConvertTo (null, CultureInfo.InvariantCulture,
				3, typeof (InstanceDescriptor)) as InstanceDescriptor;
			Assert.IsNotNull (idesc, "#E1");
			Assert.IsNotNull (idesc.Arguments, "#E2");
			Assert.AreEqual (2, idesc.Arguments.Count, "#E3");
			Assert.AreEqual (typeof (object []), idesc.Arguments.GetType (), "#E4");
			arguments = (object []) idesc.Arguments;
			Assert.AreEqual (typeof (E2), arguments [0], "#E5");
			Assert.AreEqual (typeof (int), arguments [1].GetType (), "#E6");
			Assert.AreEqual (3, arguments [1], "#E7");
			Assert.IsTrue (idesc.IsComplete, "#E8");
			mi = idesc.MemberInfo as MethodInfo;
			Assert.IsNotNull (mi, "#E9");
			Assert.AreEqual ("ToObject", mi.Name, "#E10");
			Assert.AreEqual (typeof (Enum), mi.DeclaringType, "#E11");
			parameters = mi.GetParameters ();
			Assert.AreEqual (2, parameters.Length, "#E12");
			Assert.AreEqual (typeof (Type), parameters [0].ParameterType, "#E13");
			Assert.AreEqual (typeof (int), parameters [1].ParameterType, "#E14");

			try {
				converter.ConvertTo (null, CultureInfo.InvariantCulture,
					null, typeof (InstanceDescriptor));
				Assert.Fail ("#F1");
			} catch (NotSupportedException ex) {
				// 'EnumConverter' is unable to convert '(null)'
				// to 'System.ComponentModel.Design.Serialization.InstanceDescriptor'
				Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#F2");
				Assert.IsNull (ex.InnerException, "#F3");
				Assert.IsNotNull (ex.Message, "#F4");
				Assert.IsTrue (ex.Message.IndexOf ("'" + typeof (EnumConverter).Name + "'") != -1, "#F5");
				Assert.IsTrue (ex.Message.IndexOf ("'(null)'") != -1, "#F6");
				Assert.IsTrue (ex.Message.IndexOf ("'" + typeof (InstanceDescriptor).FullName + "'") != -1, "#F7");
			}

			try {
				converter.ConvertTo (null, CultureInfo.InvariantCulture,
					"2,1", typeof (InstanceDescriptor));
				Assert.Fail ("#G1");
			} catch (FormatException ex) {
				// Input string was not in a correct format
				Assert.AreEqual (typeof (FormatException), ex.GetType (), "#G2");
				Assert.IsNull (ex.InnerException, "#G3");
				Assert.IsNotNull (ex.Message, "#G4");
			}

			try {
				converter.ConvertTo (null, CultureInfo.InvariantCulture,
					"Cc", typeof (InstanceDescriptor));
				Assert.Fail ("#H1");
			} catch (FormatException ex) {
				// Input string was not in a correct format
				Assert.AreEqual (typeof (FormatException), ex.GetType (), "#H2");
				Assert.IsNull (ex.InnerException, "#H3");
				Assert.IsNotNull (ex.Message, "#H4");
			}

			try {
				converter.ConvertTo (null, CultureInfo.InvariantCulture,
					(E2) 666 | (E2) 222, typeof (InstanceDescriptor));
				Assert.Fail ("#I1");
			} catch (NotSupportedException ex) {
				// 'EnumConverter' is unable to convert 'MonoTests.System.ComponentModel.EnumConverterTests+E2'
				// to 'System.ComponentModel.Design.Serialization.InstanceDescriptor'
				Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#I2");
				Assert.IsNull (ex.InnerException, "#I3");
				Assert.IsNotNull (ex.Message, "#I4");
				Assert.IsTrue (ex.Message.IndexOf ("'" + typeof (EnumConverter).Name + "'") != -1, "#I5");
				Assert.IsTrue (ex.Message.IndexOf ("'" + typeof (E2).FullName + "'") != -1, "#I6");
				Assert.IsTrue (ex.Message.IndexOf ("'" + typeof (InstanceDescriptor).FullName + "'") != -1, "#I7");
			}

			converter = new EnumConverter (typeof (F2));
			idesc = converter.ConvertTo (null, CultureInfo.InvariantCulture,
				F2.Bb | F2.Dd, typeof (InstanceDescriptor))
				as InstanceDescriptor;
			Assert.IsNotNull (idesc, "#J1");
			Assert.IsNotNull (idesc.Arguments, "#J2");
			Assert.AreEqual (2, idesc.Arguments.Count, "#J3");
			Assert.AreEqual (typeof (object []), idesc.Arguments.GetType (), "#J4");
			arguments = (object []) idesc.Arguments;
			Assert.AreEqual (typeof (F2), arguments [0], "#J5");
			Assert.AreEqual (typeof (byte), arguments [1].GetType (), "#J6");
			Assert.AreEqual (10, arguments [1], "#J7");
			Assert.IsTrue (idesc.IsComplete, "#J8");
			mi = idesc.MemberInfo as MethodInfo;
			Assert.IsNotNull (mi, "#J9");
			Assert.AreEqual ("ToObject", mi.Name, "#J10");
			Assert.AreEqual (typeof (Enum), mi.DeclaringType, "#J11");
			parameters = mi.GetParameters ();
			Assert.AreEqual (2, parameters.Length, "#J12");
			Assert.AreEqual (typeof (Type), parameters [0].ParameterType, "#J13");
			Assert.AreEqual (typeof (byte), parameters [1].ParameterType, "#J14");
		}

		[Test]
		public void ConvertTo_String ()
		{
			EnumConverter converter = new EnumConverter (typeof (E));

			Assert.AreEqual ("Bb", converter.ConvertTo (null,
				CultureInfo.InvariantCulture, E.Bb,
				typeof (string)), "#A1");
			Assert.AreEqual ("Dd", converter.ConvertTo (null,
				CultureInfo.InvariantCulture, 3,
				typeof (string)), "#A2");
			Assert.AreEqual (string.Empty, converter.ConvertTo (
				null, CultureInfo.InvariantCulture, null,
				typeof (string)), "#A3");
			Assert.AreEqual ("Cc", converter.ConvertTo (
				null, CultureInfo.InvariantCulture, (E) 2,
				typeof (string)), "#A4");
			Assert.AreEqual ("Cc", converter.ConvertTo (null,
				CultureInfo.InvariantCulture, E2.Bb,
				typeof (string)), "#A5");
			Assert.AreEqual ("Dd", converter.ConvertTo (null,
				CultureInfo.InvariantCulture, E.Bb | E.Dd,
				typeof (string)), "#A6");

			try {
				converter.ConvertTo (null, CultureInfo.InvariantCulture,
					(E) 666, typeof (string));
				Assert.Fail ("#B1");
			} catch (ArgumentException ex) {
				// The value '666' is not a valid value for the enum 'E'
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
				Assert.IsTrue (ex.Message.IndexOf ("'666'") != -1, "#B5");
				Assert.IsTrue (ex.Message.IndexOf ("'E'") != -1, "#B6");
				Assert.IsNull (ex.ParamName, "#B7");
			}

			try {
				converter.ConvertTo (null, CultureInfo.InvariantCulture,
					"Cc", typeof (string));
				Assert.Fail ("#C1");
			} catch (FormatException ex) {
				// Input string was not in a correct format
				Assert.AreEqual (typeof (FormatException), ex.GetType (), "#C2");
				Assert.IsNull (ex.InnerException, "#C3");
				Assert.IsNotNull (ex.Message, "#C4");
			}


			converter = new EnumConverter (typeof (E2));

			Assert.AreEqual ("Bb", converter.ConvertTo (null,
				CultureInfo.InvariantCulture, E2.Bb,
				typeof (string)), "#B1");
			Assert.AreEqual ("Aa, Bb", converter.ConvertTo (null,
				CultureInfo.InvariantCulture, 3,
				typeof (string)), "#B2");
			Assert.AreEqual (string.Empty, converter.ConvertTo (
				null, CultureInfo.InvariantCulture, null,
				typeof (string)), "#B3");
			Assert.AreEqual ("Bb", converter.ConvertTo (
				null, CultureInfo.InvariantCulture, (E2) 2,
				typeof (string)), "#B4");
			Assert.AreEqual ("Aa, Bb", converter.ConvertTo (
				null, CultureInfo.InvariantCulture, E.Dd,
				typeof (string)), "#B5");
			Assert.AreEqual ("Bb, Dd", converter.ConvertTo (null,
				CultureInfo.InvariantCulture, E2.Bb | E2.Dd,
				typeof (string)), "#B6");
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
			Aa = 1,
			Bb = 2,
			Cc = 4,
			Dd = 8,
		}


		[Flags]
		enum E3
		{
			Aa = 0,
			Bb = 1,
			Cc = 2,
			Dd = 4,
		}

		enum F : byte
		{
			Bb = 1,
			Dd = 3
		}

		[Flags]
		enum F2 : byte
		{
			Bb = 2,
			Dd = 8
		}
	}
}
