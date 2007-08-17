//
// FieldInfoTest - NUnit Test Cases for the FieldInfo class
//
// Authors:
//	Zoltan Varga (vargaz@freemail.hu)
//	Gert Driesen (drieseng@users.sourceforge.net)
//
// (c) 2003 Ximian, Inc. (http://www.ximian.com)
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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
using System.Threading;
using System.Reflection;
#if !TARGET_JVM
using System.Reflection.Emit;
#endif // TARGET_JVM
using System.Runtime.InteropServices;

using NUnit.Framework;

namespace MonoTests.System.Reflection
{
	[StructLayout(LayoutKind.Explicit, Pack = 4, Size = 64)]
	public class Class1
	{
		[FieldOffset (32)]
		public int i;
	}

	[StructLayout(LayoutKind.Sequential)]
	public class Class2
	{
		[MarshalAsAttribute(UnmanagedType.Bool)]
		public int f0;

		[MarshalAs(UnmanagedType.LPArray, ArraySubType=UnmanagedType.LPStr)]
		public string[] f1;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst=100)]
		public string f2;

		[MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef=typeof (Marshal1), MarshalCookie="5")]
		public int f3;

		[MarshalAs (UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof (Marshal1), MarshalCookie = "5")]
		public object f4;

		[Obsolete]
		public int f5;
	}

	public class Class3 : Class2
	{
	}

	[TestFixture]
	public class FieldInfoTest
	{
		[NonSerialized]
		public int i;

		[Test]
		public void IsDefined_AttributeType_Null ()
		{
			Type type = typeof (FieldInfoTest);
			FieldInfo field = type.GetField ("i");

			try {
				field.IsDefined ((Type) null, false);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("attributeType", ex.ParamName, "#6");
			}
		}

		[Test]
		public void GetCustomAttributes ()
		{
			object [] attrs;
			FieldInfo fi;

			fi = typeof (Class2).GetField ("f5");

			attrs = fi.GetCustomAttributes (false);
			Assert.AreEqual (1, attrs.Length, "#B1");
			Assert.AreEqual (typeof (ObsoleteAttribute), attrs [0].GetType (), "#B2");
			attrs = fi.GetCustomAttributes (true);
			Assert.AreEqual (1, attrs.Length, "#B3");
			Assert.AreEqual (typeof (ObsoleteAttribute), attrs [0].GetType (), "#B4");
			attrs = fi.GetCustomAttributes (typeof (MarshalAsAttribute), false);
			Assert.AreEqual (0, attrs.Length, "#B5");
			attrs = fi.GetCustomAttributes (typeof (MarshalAsAttribute), true);
			Assert.AreEqual (0, attrs.Length, "#B6");
			attrs = fi.GetCustomAttributes (typeof (ObsoleteAttribute), false);
			Assert.AreEqual (1, attrs.Length, "#B7");
			Assert.AreEqual (typeof (ObsoleteAttribute), attrs [0].GetType (), "#B8");
			attrs = fi.GetCustomAttributes (typeof (ObsoleteAttribute), true);
			Assert.AreEqual (1, attrs.Length, "#B9");
			Assert.AreEqual (typeof (ObsoleteAttribute), attrs [0].GetType (), "#B10");

			fi = typeof (Class3).GetField ("f5");

			attrs = fi.GetCustomAttributes (false);
			Assert.AreEqual (1, attrs.Length, "#D1");
			Assert.AreEqual (typeof (ObsoleteAttribute), attrs [0].GetType (), "#D2");
			attrs = fi.GetCustomAttributes (true);
			Assert.AreEqual (1, attrs.Length, "#D3");
			Assert.AreEqual (typeof (ObsoleteAttribute), attrs [0].GetType (), "#D4");
			attrs = fi.GetCustomAttributes (typeof (MarshalAsAttribute), false);
			Assert.AreEqual (0, attrs.Length, "#D5");
			attrs = fi.GetCustomAttributes (typeof (MarshalAsAttribute), true);
			Assert.AreEqual (0, attrs.Length, "#D6");
			attrs = fi.GetCustomAttributes (typeof (ObsoleteAttribute), false);
			Assert.AreEqual (1, attrs.Length, "#D7");
			Assert.AreEqual (typeof (ObsoleteAttribute), attrs [0].GetType (), "#D8");
			attrs = fi.GetCustomAttributes (typeof (ObsoleteAttribute), true);
			Assert.AreEqual (1, attrs.Length, "#D9");
			Assert.AreEqual (typeof (ObsoleteAttribute), attrs [0].GetType (), "#D10");
		}

		[Test]
		public void PseudoCustomAttributes ()
		{
			object [] attrs;
			Type t = typeof (FieldInfoTest);

#if NET_2_0
			Assert.AreEqual (1, t.GetField ("i").GetCustomAttributes (typeof (NonSerializedAttribute), true).Length);
#else
			Assert.AreEqual (0, t.GetField ("i").GetCustomAttributes (typeof (NonSerializedAttribute), true).Length);
#endif

			attrs = typeof (Class1).GetField ("i").GetCustomAttributes (true);
#if NET_2_0
			Assert.AreEqual (1, attrs.Length, "#B1");
			FieldOffsetAttribute field_attr = (FieldOffsetAttribute) attrs [0];
			Assert.AreEqual (32, field_attr.Value, "#B2");
#else
			Assert.AreEqual (0, attrs.Length, "#B1");
#endif

			MarshalAsAttribute attr;

			attrs = typeof (Class2).GetField ("f0").GetCustomAttributes (true);
#if NET_2_0
			Assert.AreEqual (1, attrs.Length, "#C1");
			attr = (MarshalAsAttribute) attrs [0];
			Assert.AreEqual (UnmanagedType.Bool, attr.Value, "#C2");
#else
			Assert.AreEqual (0, attrs.Length, "#C1");
#endif

			attrs = typeof (Class2).GetField ("f1").GetCustomAttributes (true);
#if NET_2_0
			Assert.AreEqual (1, attrs.Length, "#D1");
			attr = (MarshalAsAttribute) attrs [0];
			Assert.AreEqual (UnmanagedType.LPArray, attr.Value, "#D2");
			Assert.AreEqual (UnmanagedType.LPStr, attr.ArraySubType, "#D3");
#else
			Assert.AreEqual (0, attrs.Length, "#D1");
#endif

			attrs = typeof (Class2).GetField ("f2").GetCustomAttributes (true);
#if NET_2_0
			Assert.AreEqual (1, attrs.Length, "#E1");
			attr = (MarshalAsAttribute) attrs [0];
			Assert.AreEqual (UnmanagedType.ByValTStr, attr.Value, "#E2");
			Assert.AreEqual (100, attr.SizeConst, "#E3");
#else
			Assert.AreEqual (0, attrs.Length, "#E1");
#endif

			attrs = typeof (Class2).GetField ("f3").GetCustomAttributes (true);
#if NET_2_0
			Assert.AreEqual (1, attrs.Length, "#F1");
			attr = (MarshalAsAttribute) attrs [0];
			Assert.AreEqual (UnmanagedType.CustomMarshaler, attr.Value, "#F2");
			Assert.AreEqual ("5", attr.MarshalCookie, "#F3");
			Assert.AreEqual (typeof (Marshal1), Type.GetType (attr.MarshalType), "#F4");
#else
			Assert.AreEqual (0, attrs.Length, "#F1");
#endif

			attrs = typeof (Class3).GetField ("f3").GetCustomAttributes (false);
#if NET_2_0
			Assert.AreEqual (1, attrs.Length, "#G1");
			attr = (MarshalAsAttribute) attrs [0];
			Assert.AreEqual (UnmanagedType.CustomMarshaler, attr.Value, "#G2");
			Assert.AreEqual ("5", attr.MarshalCookie, "#G3");
			Assert.AreEqual (typeof (Marshal1), Type.GetType (attr.MarshalType), "#G4");
#else
			Assert.AreEqual (0, attrs.Length, "#G1");
#endif

			attrs = typeof (Class3).GetField ("f3").GetCustomAttributes (true);
#if NET_2_0
			Assert.AreEqual (1, attrs.Length, "#H1");
			attr = (MarshalAsAttribute) attrs [0];
			Assert.AreEqual (UnmanagedType.CustomMarshaler, attr.Value, "#H2");
			Assert.AreEqual ("5", attr.MarshalCookie, "#H3");
			Assert.AreEqual (typeof (Marshal1), Type.GetType (attr.MarshalType), "#H4");
#else
			Assert.AreEqual (0, attrs.Length, "#H1");
#endif

			// bug #82465
			attrs = typeof (Class2).GetField ("f3").GetCustomAttributes (true);
#if NET_2_0
			Assert.AreEqual (1, attrs.Length, "#I1");
			attr = (MarshalAsAttribute) attrs [0];
			Assert.AreEqual (UnmanagedType.CustomMarshaler, attr.Value, "#I2");
			Assert.AreEqual ("5", attr.MarshalCookie, "#I3");
			Assert.AreEqual (typeof (Marshal1), Type.GetType (attr.MarshalType), "#I4");
#else
			Assert.AreEqual (0, attrs.Length, "#I1");
#endif
		}

#if NET_2_0
#if !TARGET_JVM // ReflectionOnlyLoad not supported for TARGET_JVM
		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void GetValueOnRefOnlyAssembly ()
		{
			Assembly assembly = Assembly.ReflectionOnlyLoad (typeof (FieldInfoTest).Assembly.FullName);
			Type t = assembly.GetType (typeof (RefOnlyFieldClass).FullName);
			FieldInfo f = t.GetField ("RefOnlyField", BindingFlags.Static | BindingFlags.NonPublic);
			f.GetValue (null);
		}
	
		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void SetValueOnRefOnlyAssembly ()
		{
			Assembly assembly = Assembly.ReflectionOnlyLoad (typeof (FieldInfoTest).Assembly.FullName);
			Type t = assembly.GetType (typeof (RefOnlyFieldClass).FullName);
			FieldInfo f = t.GetField ("RefOnlyField", BindingFlags.Static | BindingFlags.NonPublic);
			f.SetValue (null, 8);
		}
#endif // TARGET_JVM

		const int literal = 42;

		[Test]
		[ExpectedException (typeof (FieldAccessException))]
		public void SetValueOnLiteralField ()
		{
			FieldInfo f = typeof (FieldInfoTest).GetField ("literal", BindingFlags.Static | BindingFlags.NonPublic);
			f.SetValue (null, 0);
		}

		public int? nullable_field;

		public static int? static_nullable_field;

		[Test]
		public void NullableTests ()
		{
			FieldInfoTest t = new FieldInfoTest ();

			FieldInfo fi = typeof (FieldInfoTest).GetField ("nullable_field");

			fi.SetValue (t, 101);
			Assert.AreEqual (101, fi.GetValue (t));
			fi.SetValue (t, null);
			Assert.AreEqual (null, fi.GetValue (t));

			FieldInfo fi2 = typeof (FieldInfoTest).GetField ("static_nullable_field");

			fi2.SetValue (t, 101);
			Assert.AreEqual (101, fi2.GetValue (t));
			fi2.SetValue (t, null);
			Assert.AreEqual (null, fi2.GetValue (t));
		}
	
#if !TARGET_JVM // TypeBuilder not supported for TARGET_JVM
		[Test]
		public void NonPublicTests ()
		{
			Assembly assembly = Assembly.ReflectionOnlyLoad (typeof (FieldInfoTest).Assembly.FullName);
		
			Type t = assembly.GetType (typeof (NonPublicFieldClass).FullName);

			// try to get non-public field
			FieldInfo fi = t.GetField ("protectedField");
			Assert.IsNull (fi);
			// get it for real
			fi = t.GetField ("protectedField", BindingFlags.NonPublic | BindingFlags.Instance);
			Assert.IsNotNull (fi);
			// get via typebuilder
			FieldInfo f = TypeBuilder.GetField (t, fi);
			Assert.IsNotNull (f);
		}
#endif // TARGET_JVM
	
#endif
	}

#if NET_2_0
	// Helper classes
	class RefOnlyFieldClass 
	{
		// Helper property
		static int RefOnlyField;
	}

	class NonPublicFieldClass
	{
		protected int protectedField;
	}
#endif
}
