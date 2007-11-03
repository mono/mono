//
// System.ComponentModel.InvalidEnumArgumentException test cases
//
// Authors:
// 	Gert Driesen (drieseng@users.sourceforge.net)
//
// (c) 2007 Gert Driesen
//

using System;
using System.ComponentModel;
using System.Globalization;

using NUnit.Framework;

namespace MonoTests.System.ComponentModel
{
	[TestFixture]
	public class InvalidEnumArgumentExceptionTest
	{
		[Test] // ctor ()
		public void Constructor0 ()
		{
			InvalidEnumArgumentException iea = new InvalidEnumArgumentException ();
			Assert.IsNull (iea.InnerException, "#1");
			Assert.IsNotNull (iea.Message, "#2");
			Assert.IsTrue (iea.Message.IndexOf (typeof (InvalidEnumArgumentException).FullName) != -1, "#3");
			Assert.IsNull (iea.ParamName, "#4");
		}

		[Test] // ctor (string)
		public void Constructor1 ()
		{
			InvalidEnumArgumentException iea;

			iea = new InvalidEnumArgumentException ("msg");
			Assert.IsNull (iea.InnerException, "#A1");
			Assert.IsNotNull (iea.Message, "#A2");
			Assert.AreEqual ("msg", iea.Message, "#A3");
			Assert.IsNull (iea.ParamName, "#A4");

			iea = new InvalidEnumArgumentException (string.Empty);
			Assert.IsNull (iea.InnerException, "#B1");
			Assert.IsNotNull (iea.Message, "#B2");
			Assert.AreEqual (string.Empty, iea.Message, "#B3");
			Assert.IsNull (iea.ParamName, "#B4");

			iea = new InvalidEnumArgumentException ((string) null);
			Assert.IsNull (iea.InnerException, "#C1");
			Assert.IsNotNull (iea.Message, "#C2");
			Assert.IsTrue (iea.Message.IndexOf (typeof (InvalidEnumArgumentException).FullName) != -1, "#C3");
			Assert.IsNull (iea.ParamName, "#C4");
		}

		// TODO: ctor (SerializationInfo, StreamingContext)

#if NET_2_0
		[Test] // ctor (string, Exception)
		public void Constructor3 ()
		{
			InvalidEnumArgumentException iea;
			Exception inner = new Exception ("INNER");

			iea = new InvalidEnumArgumentException ("msg", (Exception) null);
			Assert.IsNull (iea.InnerException, "#A1");
			Assert.IsNotNull (iea.Message, "#A2");
			Assert.AreEqual ("msg", iea.Message, "#A3");
			Assert.IsNull (iea.ParamName, "#A4");

			iea = new InvalidEnumArgumentException (string.Empty, inner);
			Assert.AreSame (inner, iea.InnerException, "#B1");
			Assert.IsNotNull (iea.Message, "#B2");
			Assert.AreEqual (string.Empty, iea.Message, "#B3");
			Assert.IsNull (iea.ParamName, "#B4");

			iea = new InvalidEnumArgumentException ((string) null, inner);
			Assert.AreSame (inner, iea.InnerException, "#C1");
			Assert.IsNotNull (iea.Message, "#C2");
			Assert.IsTrue (iea.Message.IndexOf (typeof (InvalidEnumArgumentException).FullName) != -1, "#C3");
			Assert.IsNull (iea.ParamName, "#C4");
		}
#endif

		[Test] // ctor (string, int, System.Type)
		public void Constructor4 ()
		{
			InvalidEnumArgumentException iea;
			Type enumClass = typeof (AttributeTargets);

			// The value of argument 'value' (667666) is invalid for
			// Enum type 'AttributeTargets'
			iea = new InvalidEnumArgumentException ("arg", 667666, enumClass);
			Assert.IsNull (iea.InnerException, "#A1");
			Assert.IsNotNull (iea.Message, "#A2");
#if NET_2_0
			Assert.IsTrue (iea.Message.IndexOf ("'arg'") != -1, "#A3");
			Assert.IsTrue (iea.Message.IndexOf ("(" + 667666.ToString (CultureInfo.CurrentCulture) + ")") != -1, "#A4");
			Assert.IsTrue (iea.Message.IndexOf ("'" + enumClass.Name + "'") != -1, "#A5");
#else
			Assert.IsTrue (iea.Message.IndexOf ("arg") != -1, "#A3");
			Assert.IsTrue (iea.Message.IndexOf (667666.ToString (CultureInfo.CurrentCulture)) != -1, "#A4");
			Assert.IsTrue (iea.Message.IndexOf (enumClass.Name) != -1, "#A5");
#endif
			Assert.IsNotNull (iea.ParamName, "#A6");
			Assert.AreEqual ("arg", iea.ParamName, "#A7");

			// The value of argument '' (0) is invalid for
			// Enum type 'AttributeTargets'
			iea = new InvalidEnumArgumentException (string.Empty, 0, enumClass);
			Assert.IsNull (iea.InnerException, "#B1");
			Assert.IsNotNull (iea.Message, "#B2");
#if NET_2_0
			Assert.IsTrue (iea.Message.IndexOf ("''") != -1, "#B3");
			Assert.IsTrue (iea.Message.IndexOf ("(" + 0.ToString (CultureInfo.CurrentCulture) + ")") != -1, "#B4");
			Assert.IsTrue (iea.Message.IndexOf ("'" + enumClass.Name + "'") != -1, "#B5");
#else
			Assert.IsTrue (iea.Message.IndexOf ("  ") != -1, "#B3");
			Assert.IsTrue (iea.Message.IndexOf (0.ToString (CultureInfo.CurrentCulture)) != -1, "#B4");
			Assert.IsTrue (iea.Message.IndexOf (enumClass.Name) != -1, "#B5");
#endif
			Assert.IsNotNull (iea.ParamName, "#B6");
			Assert.AreEqual (string.Empty, iea.ParamName, "#B7");

			// The value of argument '' (-56776) is invalid for Enum type
			// 'AttributeTargets'
			iea = new InvalidEnumArgumentException ((string) null, -56776, enumClass);
			Assert.IsNull (iea.InnerException, "#C1");
			Assert.IsNotNull (iea.Message, "#C2");
#if NET_2_0
			Assert.IsTrue (iea.Message.IndexOf ("''") != -1, "#C3");
			Assert.IsTrue (iea.Message.IndexOf ("(" + (-56776).ToString (CultureInfo.CurrentCulture) + ")") != -1, "#C4");
			Assert.IsTrue (iea.Message.IndexOf ("'" + enumClass.Name + "'") != -1, "#C5");
#else
			Assert.IsTrue (iea.Message.IndexOf ("  ") != -1, "#C3");
			Assert.IsTrue (iea.Message.IndexOf ((-56776).ToString (CultureInfo.CurrentCulture)) != -1, "#C4");
			Assert.IsTrue (iea.Message.IndexOf (enumClass.Name) != -1, "#C5");
#endif
			Assert.IsNull (iea.ParamName, "#C6");

			// The value of argument '' (0) is invalid for Enum type
			// 'AttributeTargets'
			iea = new InvalidEnumArgumentException ((string) null, 0, enumClass);
			Assert.IsNull (iea.InnerException, "#D1");
			Assert.IsNotNull (iea.Message, "#D2");
#if NET_2_0
			Assert.IsTrue (iea.Message.IndexOf ("''") != -1, "#D3");
			Assert.IsTrue (iea.Message.IndexOf ("(" + 0.ToString (CultureInfo.CurrentCulture) + ")") != -1, "#D4");
			Assert.IsTrue (iea.Message.IndexOf ("'" + enumClass.Name + "'") != -1, "#D5");
#else
			Assert.IsTrue (iea.Message.IndexOf ("  ") != -1, "#D3");
			Assert.IsTrue (iea.Message.IndexOf (0.ToString (CultureInfo.CurrentCulture)) != -1, "#D4");
			Assert.IsTrue (iea.Message.IndexOf (enumClass.Name) != -1, "#D5");
#endif
			Assert.IsNull (iea.ParamName, "#D6");
		}

		[Test] // ctor (string, int, System.Type)
		[ExpectedException (typeof (NullReferenceException))]
		public void Constructor4_EnumClass_Null ()
		{
			new InvalidEnumArgumentException ("param", 55, (Type) null);
		}
	}
}
