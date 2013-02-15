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
#if !TARGET_JVM && !MONOTOUCH
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
	public unsafe class FieldInfoTest
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

		[Test] // GetFieldFromHandle (RuntimeFieldHandle)
		public void GetFieldFromHandle1_Handle_Zero ()
		{
			RuntimeFieldHandle fh = new RuntimeFieldHandle ();

			try {
				FieldInfo.GetFieldFromHandle (fh);
				Assert.Fail ("#1");
			} catch (ArgumentException ex) {
				// Handle is not initialized
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNull (ex.ParamName, "#5");
			}
		}

#if NET_2_0
		[Test] // GetFieldFromHandle (RuntimeFieldHandle, RuntimeTypeHandle)
		public void GetFieldFromHandle2_DeclaringType_Zero ()
		{
			RuntimeTypeHandle th = new RuntimeTypeHandle ();
			FieldInfo fi1 = typeof (Class2).GetField ("f5");
			RuntimeFieldHandle fh = fi1.FieldHandle;

			FieldInfo fi2 = FieldInfo.GetFieldFromHandle (fh, th);
			Assert.IsNotNull (fi2, "#1");
			Assert.AreSame (fi1.DeclaringType, fi2.DeclaringType, "#2");
			Assert.AreEqual (fi1.FieldType, fi2.FieldType, "#3");
			Assert.AreEqual (fi1.Name, fi2.Name, "#4");
		}

		[Test] // GetFieldFromHandle (RuntimeFieldHandle, RuntimeTypeHandle)
		public void GetFieldFromHandle2_Handle_Generic ()
		{
			FieldInfoTest<string> instance = new FieldInfoTest<string> ();
			Type t = instance.GetType ();

			FieldInfo fi1 = t.GetField ("TestField");
			RuntimeFieldHandle fh = fi1.FieldHandle;
			RuntimeTypeHandle th = t.TypeHandle;

			FieldInfo fi2 = FieldInfo.GetFieldFromHandle (fh, th);
			Assert.IsNotNull (fi2, "#1");
			Assert.AreSame (t, fi2.DeclaringType, "#2");
			Assert.AreEqual (typeof (string), fi2.FieldType, "#3");
			Assert.AreEqual ("TestField", fi2.Name, "#4");
		}

		[Test] // GetFieldFromHandle (RuntimeFieldHandle, RuntimeTypeHandle)
		[Category ("NotWorking")]
		[Category ("NotDotNet")] // https://connect.microsoft.com/VisualStudio/feedback/ViewFeedback.aspx?FeedbackID=343449
		public void GetFieldFromHandle2_Handle_GenericDefinition ()
		{
			Type t1 = typeof (FieldInfoTest<>);
			FieldInfo fi1 = t1.GetField ("TestField");
			RuntimeFieldHandle fh = fi1.FieldHandle;

			FieldInfoTest<string> instance = new FieldInfoTest<string> ();
			Type t2 = instance.GetType ();
			RuntimeTypeHandle th = t2.TypeHandle;

			FieldInfo fi2 = FieldInfo.GetFieldFromHandle (fh, th);
			Assert.IsNotNull (fi2, "#1");
			Assert.AreSame (t2, fi2.DeclaringType, "#2");
			Assert.AreEqual (typeof (string), fi2.FieldType, "#3");
			Assert.AreEqual ("TestField", fi2.Name, "#4");
		}

		[Test] // GetFieldFromHandle (RuntimeFieldHandle, RuntimeTypeHandle)
		public void GetFieldFromHandle2_Handle_Zero ()
		{
			object instance = new Class2 ();
			RuntimeTypeHandle th = Type.GetTypeHandle (instance);
			RuntimeFieldHandle fh = new RuntimeFieldHandle ();

			try {
				FieldInfo.GetFieldFromHandle (fh, th);
				Assert.Fail ("#1");
			} catch (ArgumentException ex) {
				// Handle is not initialized
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNull (ex.ParamName, "#5");
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void GetFieldFromHandle2_Incompatible ()
		{
			RuntimeFieldHandle fh = typeof (FieldInfoTest<int>).GetField ("TestField").FieldHandle;

			FieldInfoTest<string> instance = new FieldInfoTest<string> ();
			Type t2 = instance.GetType ();
			RuntimeTypeHandle th = t2.TypeHandle;

			FieldInfo fi2 = FieldInfo.GetFieldFromHandle (fh, th);
		}
#endif

		[Test]
		public void PseudoCustomAttributes ()
		{
			object [] attrs;
			Type t = typeof (FieldInfoTest);

			Assert.AreEqual (1, t.GetField ("i").GetCustomAttributes (typeof (NonSerializedAttribute), true).Length);

			attrs = typeof (Class1).GetField ("i").GetCustomAttributes (true);
			Assert.AreEqual (1, attrs.Length, "#B1");
			FieldOffsetAttribute field_attr = (FieldOffsetAttribute) attrs [0];
			Assert.AreEqual (32, field_attr.Value, "#B2");

			MarshalAsAttribute attr;

			attrs = typeof (Class2).GetField ("f0").GetCustomAttributes (true);
			Assert.AreEqual (1, attrs.Length, "#C1");
			attr = (MarshalAsAttribute) attrs [0];
			Assert.AreEqual (UnmanagedType.Bool, attr.Value, "#C2");

			attrs = typeof (Class2).GetField ("f1").GetCustomAttributes (true);
			Assert.AreEqual (1, attrs.Length, "#D1");
			attr = (MarshalAsAttribute) attrs [0];
			Assert.AreEqual (UnmanagedType.LPArray, attr.Value, "#D2");
			Assert.AreEqual (UnmanagedType.LPStr, attr.ArraySubType, "#D3");

			attrs = typeof (Class2).GetField ("f2").GetCustomAttributes (true);
			Assert.AreEqual (1, attrs.Length, "#E1");
			attr = (MarshalAsAttribute) attrs [0];
			Assert.AreEqual (UnmanagedType.ByValTStr, attr.Value, "#E2");
			Assert.AreEqual (100, attr.SizeConst, "#E3");

			attrs = typeof (Class2).GetField ("f3").GetCustomAttributes (true);
			Assert.AreEqual (1, attrs.Length, "#F1");
			attr = (MarshalAsAttribute) attrs [0];
			Assert.AreEqual (UnmanagedType.CustomMarshaler, attr.Value, "#F2");
			Assert.AreEqual ("5", attr.MarshalCookie, "#F3");
			Assert.AreEqual (typeof (Marshal1), Type.GetType (attr.MarshalType), "#F4");

			attrs = typeof (Class3).GetField ("f3").GetCustomAttributes (false);
			Assert.AreEqual (1, attrs.Length, "#G1");
			attr = (MarshalAsAttribute) attrs [0];
			Assert.AreEqual (UnmanagedType.CustomMarshaler, attr.Value, "#G2");
			Assert.AreEqual ("5", attr.MarshalCookie, "#G3");
			Assert.AreEqual (typeof (Marshal1), Type.GetType (attr.MarshalType), "#G4");

			attrs = typeof (Class3).GetField ("f3").GetCustomAttributes (true);
			Assert.AreEqual (1, attrs.Length, "#H1");
			attr = (MarshalAsAttribute) attrs [0];
			Assert.AreEqual (UnmanagedType.CustomMarshaler, attr.Value, "#H2");
			Assert.AreEqual ("5", attr.MarshalCookie, "#H3");
			Assert.AreEqual (typeof (Marshal1), Type.GetType (attr.MarshalType), "#H4");

			// bug #82465
			attrs = typeof (Class2).GetField ("f3").GetCustomAttributes (true);
			Assert.AreEqual (1, attrs.Length, "#I1");
			attr = (MarshalAsAttribute) attrs [0];
			Assert.AreEqual (UnmanagedType.CustomMarshaler, attr.Value, "#I2");
			Assert.AreEqual ("5", attr.MarshalCookie, "#I3");
			Assert.AreEqual (typeof (Marshal1), Type.GetType (attr.MarshalType), "#I4");
		}

		class Foo {
			public static int static_field;
			public int field;
		}

		[ExpectedException (typeof (ArgumentException))]
		public void GetValueWrongObject ()
		{
			Foo f = new Foo ();

			typeof (Foo).GetField ("field").GetValue (typeof (int));
		}

		public void GetValueWrongObjectStatic ()
		{
			Foo f = new Foo ();

			// This is allowed in MS.NET
			typeof (Foo).GetField ("static_field").GetValue (typeof (int));
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
		}
#endif // TARGET_JVM

		[Test]
		public void GetRawDefaultValue ()
		{
			Assert.AreEqual (5, typeof (FieldInfoTest).GetField ("int_field").GetRawConstantValue ());
			Assert.AreEqual (Int64.MaxValue, typeof (FieldInfoTest).GetField ("long_field").GetRawConstantValue ());
			Assert.AreEqual (2, typeof (FieldInfoTest).GetField ("int_enum_field").GetRawConstantValue ());
			Assert.AreEqual (typeof (int), typeof (FieldInfoTest).GetField ("int_enum_field").GetRawConstantValue ().GetType ());
			Assert.AreEqual (2, typeof (FieldInfoTest).GetField ("long_enum_field").GetRawConstantValue ());
			Assert.AreEqual (typeof (long), typeof (FieldInfoTest).GetField ("long_enum_field").GetRawConstantValue ().GetType ());
			Assert.AreEqual ("Hello", typeof (FieldInfoTest).GetField ("string_field").GetRawConstantValue ());
			Assert.AreEqual (null, typeof (FieldInfoTest).GetField ("object_field").GetRawConstantValue ());
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void GetRawDefaultValueNoDefault ()
		{
			typeof (FieldInfoTest).GetField ("non_const_field").GetRawConstantValue ();
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void GetValueOpenGeneric ()
		{
			typeof(Foo<>).GetField ("field").GetValue (null);
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void SetValueOpenGeneric ()
		{
			typeof(Foo<>).GetField ("field").SetValue (null, 0);
		}

		[Test]
		public void GetValueOnConstantOfOpenGeneric ()
		{
			Assert.AreEqual (10, typeof(Foo<>).GetField ("constant").GetValue (null), "#1");
			Assert.AreEqual ("waa", typeof(Foo<>).GetField ("sconstant").GetValue (null), "#2");
			Assert.AreEqual (IntEnum.Third, typeof(Foo<>).GetField ("econstant").GetValue (null), "#3");
		}

		public static unsafe void* ip;

		[Test]
		public unsafe void GetSetValuePointers ()
		{
			int i = 5;
			void *p = &i;
			typeof (FieldInfoTest).GetField ("ip").SetValue (null, (IntPtr)p);
			Pointer p2 = (Pointer)typeof (FieldInfoTest).GetField ("ip").GetValue (null);

			int *pi = (int*)Pointer.Unbox (p2);
			Assert.AreEqual (5, *pi);
		}

		public class Foo<T>
		{
			 /*
			The whole point of this field is to make sure we don't create the vtable layout
			when loading the value of constants for Foo<>. See bug #594942.

			*/
			public T dummy;
			public static int field;
			public const int constant = 10;
			public const string sconstant = "waa";
			public const IntEnum econstant = IntEnum.Third;
		}

		public enum IntEnum {
			First = 1,
			Second = 2,
			Third = 3
		}

		public enum LongEnum : long {
			First = 1,
			Second = 2,
			Third = 3
		}

		public const int int_field = 5;
		public const long long_field = Int64.MaxValue;
		public const IntEnum int_enum_field = IntEnum.Second;
		public const LongEnum long_enum_field = LongEnum.Second;
		public const string string_field = "Hello";
		public const FieldInfoTest object_field = null;
		public int non_const_field;
	
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

	public class FieldInfoTest<T>
	{
		public T TestField;
	}
#endif
}
