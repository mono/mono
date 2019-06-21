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
using System.Globalization;
using System.Threading;
using System.Reflection;
#if !MONOTOUCH && !FULL_AOT_RUNTIME
using System.Reflection.Emit;
#endif
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
#if FEATURE_COMINTEROP
		[MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef=typeof (Marshal1), MarshalCookie="5")]
		public int f3;

		[MarshalAs (UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof (Marshal1), MarshalCookie = "5")]
		public object f4;
#endif
		[Obsolete]
		public int f5;
	}

	public class Class3 : Class2
	{
	}

	// Disable this warning, as the purpose of this struct is to poke at the internal via reflection
	#pragma warning disable 649
	class FieldInvokeMatrix
	{
		public Byte field_Byte;
		public SByte field_SByte;
		public Boolean field_Boolean;
		public Char field_Char;
		public Int16 field_Int16;
		public UInt16 field_UInt16;
		public Int32 field_Int32;
		public UInt32 field_UInt32;
		public Int64 field_Int64;
		public UInt64 field_UInt64;
		public Single field_Single;
		public Double field_Double;
		public IntPtr field_IntPtr;
		public UIntPtr field_UIntPtr;
		public Decimal field_Decimal;
		public DateTime field_DateTime;
		public String field_String;

		public ByteEnum field_ByteEnum;
		public SByteEnum field_SByteEnum;
		public Int16Enum field_Int16Enum;
		public UInt16Enum field_UInt16Enum;
		public Int32Enum field_Int32Enum;
		public UInt32Enum field_UInt32Enum;
		public Int64Enum field_Int64Enum;
		public UInt64Enum field_UInt64Enum;
	}
	#pragma warning restore 649

	public enum ByteEnum : byte
	{
		MaxValue = Byte.MaxValue
	}

	public enum SByteEnum : sbyte
	{
		MaxValue = SByte.MaxValue
	}

	public enum Int16Enum : short
	{
		MaxValue = Int16.MaxValue
	}

	public enum UInt16Enum : ushort
	{
		MaxValue = UInt16.MaxValue
	}

	public enum Int32Enum : int
	{
		MaxValue = Int32.MaxValue
	}

	public enum UInt32Enum: uint
	{
		MaxValue= UInt32.MaxValue
	}

	public enum Int64Enum : long
	{
		MaxValue = Int64.MaxValue
	}

	public enum UInt64Enum: ulong
	{
		MaxValue = UInt64.MaxValue
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
		public void FieldInfoModule ()
		{
			Type type = typeof (FieldInfoTest);
			FieldInfo field = type.GetField ("i");

			Assert.AreEqual (type.Module, field.Module);
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
		public void MetadataToken ()
		{
			Type type = typeof (FieldInfoTest);
			FieldInfo field = type.GetField ("i");
			Assert.IsTrue ((int)field.MetadataToken > 0);
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

#if FEATURE_COMINTEROP
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
#endif
		}

		// Disable "field not used warning", this is intended.
#pragma warning disable 649
		class Foo {
			public static int static_field;
			public int field;
		}
#pragma warning restore 649

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
			Pointer p0 = (Pointer)typeof (FieldInfoTest).GetField ("ip").GetValue (null);
			int *p0i = (int*)Pointer.Unbox (p0);
			Assert.AreEqual (IntPtr.Zero, new IntPtr (p0i));

			int i = 5;
			void *p = &i;
			typeof (FieldInfoTest).GetField ("ip").SetValue (null, (IntPtr)p);
			Pointer p2 = (Pointer)typeof (FieldInfoTest).GetField ("ip").GetValue (null);

			int *pi = (int*)Pointer.Unbox (p2);
			Assert.AreEqual (5, *pi);

			typeof (FieldInfoTest).GetField ("ip").SetValue (null, (UIntPtr)p);
			p2 = (Pointer)typeof (FieldInfoTest).GetField ("ip").GetValue (null);

			pi = (int*)Pointer.Unbox (p2);
			Assert.AreEqual (5, *pi);
		}

		[Test]
		public void SetValuePrimitiveConversions ()
		{
			FieldInfo field;
			var instance = new FieldInvokeMatrix ();
			var fh = typeof (FieldInvokeMatrix);

			field = fh.GetField ("field_Byte");			
			field.SetValue (instance, Byte.MaxValue);
			Assert.AreEqual (Byte.MaxValue, instance.field_Byte);
			Throws (field, instance, SByte.MaxValue);
			Throws (field, instance, true);
			Throws (field, instance, Char.MaxValue);
			Throws (field, instance, Int16.MaxValue);
			Throws (field, instance, UInt16.MaxValue);
			Throws (field, instance, Int32.MaxValue);
			Throws (field, instance, UInt32.MaxValue);
			Throws (field, instance, Int64.MaxValue);
			Throws (field, instance, UInt64.MaxValue);
			Throws (field, instance, Single.MaxValue);
			Throws (field, instance, Double.MaxValue);
			Throws (field, instance, IntPtr.Zero);
			Throws (field, instance, UIntPtr.Zero);
			Throws (field, instance, Decimal.MaxValue);
			Throws (field, instance, DateTime.MaxValue);
			field.SetValue (instance, ByteEnum.MaxValue);
			Assert.AreEqual (Byte.MaxValue, instance.field_Byte);
			Throws (field, instance, SByteEnum.MaxValue);
			Throws (field, instance, Int16Enum.MaxValue);
			Throws (field, instance, UInt16Enum.MaxValue);
			Throws (field, instance, Int32Enum.MaxValue);
			Throws (field, instance, UInt32Enum.MaxValue);
			Throws (field, instance, Int64Enum.MaxValue);
			Throws (field, instance, UInt64Enum.MaxValue);
			field = fh.GetField ("field_SByte");
			Throws (field, instance, Byte.MaxValue);
			field.SetValue (instance, SByte.MaxValue);
			Assert.AreEqual (SByte.MaxValue, instance.field_SByte);
			Throws (field, instance, true);
			Throws (field, instance, Char.MaxValue);
			Throws (field, instance, Int16.MaxValue);
			Throws (field, instance, UInt16.MaxValue);
			Throws (field, instance, Int32.MaxValue);
			Throws (field, instance, UInt32.MaxValue);
			Throws (field, instance, Int64.MaxValue);
			Throws (field, instance, UInt64.MaxValue);
			Throws (field, instance, Single.MaxValue);
			Throws (field, instance, Double.MaxValue);
			Throws (field, instance, IntPtr.Zero);
			Throws (field, instance, UIntPtr.Zero);
			Throws (field, instance, Decimal.MaxValue);
			Throws (field, instance, DateTime.MaxValue);
			Throws (field, instance, ByteEnum.MaxValue);
			field.SetValue (instance, SByteEnum.MaxValue);
			Assert.AreEqual (SByte.MaxValue, instance.field_SByte);
			Throws (field, instance, Int16Enum.MaxValue);
			Throws (field, instance, UInt16Enum.MaxValue);
			Throws (field, instance, Int32Enum.MaxValue);
			Throws (field, instance, UInt32Enum.MaxValue);
			Throws (field, instance, Int64Enum.MaxValue);
			Throws (field, instance, UInt64Enum.MaxValue);
			field = fh.GetField ("field_Boolean");
			Throws (field, instance, Byte.MaxValue);
			Throws (field, instance, SByte.MaxValue);
			field.SetValue (instance, true);
			Assert.AreEqual (true, instance.field_Boolean);
			Throws (field, instance, Char.MaxValue);
			Throws (field, instance, Int16.MaxValue);
			Throws (field, instance, UInt16.MaxValue);
			Throws (field, instance, Int32.MaxValue);
			Throws (field, instance, UInt32.MaxValue);
			Throws (field, instance, Int64.MaxValue);
			Throws (field, instance, UInt64.MaxValue);
			Throws (field, instance, Single.MaxValue);
			Throws (field, instance, Double.MaxValue);
			Throws (field, instance, IntPtr.Zero);
			Throws (field, instance, UIntPtr.Zero);
			Throws (field, instance, Decimal.MaxValue);
			Throws (field, instance, DateTime.MaxValue);
			Throws (field, instance, ByteEnum.MaxValue);
			Throws (field, instance, SByteEnum.MaxValue);
			Throws (field, instance, Int16Enum.MaxValue);
			Throws (field, instance, UInt16Enum.MaxValue);
			Throws (field, instance, Int32Enum.MaxValue);
			Throws (field, instance, UInt32Enum.MaxValue);
			Throws (field, instance, Int64Enum.MaxValue);
			Throws (field, instance, UInt64Enum.MaxValue);
			field = fh.GetField ("field_Char");
			field.SetValue (instance, Byte.MaxValue);
			Assert.AreEqual (Byte.MaxValue, instance.field_Char);
			Throws (field, instance, SByte.MaxValue);
			Throws (field, instance, true);
			field.SetValue (instance, Char.MaxValue);
			Assert.AreEqual (Char.MaxValue, instance.field_Char);
			Throws (field, instance, Int16.MaxValue);
			field.SetValue (instance, UInt16.MaxValue);
			Assert.AreEqual (UInt16.MaxValue, instance.field_Char);
			Throws (field, instance, Int32.MaxValue);
			Throws (field, instance, UInt32.MaxValue);
			Throws (field, instance, Int64.MaxValue);
			Throws (field, instance, UInt64.MaxValue);
			Throws (field, instance, Single.MaxValue);
			Throws (field, instance, Double.MaxValue);
			Throws (field, instance, IntPtr.Zero);
			Throws (field, instance, UIntPtr.Zero);
			Throws (field, instance, Decimal.MaxValue);
			Throws (field, instance, DateTime.MaxValue);
			field.SetValue (instance, ByteEnum.MaxValue);
			Assert.AreEqual (Byte.MaxValue, instance.field_Char);
			Throws (field, instance, SByteEnum.MaxValue);
			Throws (field, instance, Int16Enum.MaxValue);
			field.SetValue (instance, UInt16Enum.MaxValue);
			Assert.AreEqual (UInt16.MaxValue, instance.field_Char);
			Throws (field, instance, Int32Enum.MaxValue);
			Throws (field, instance, UInt32Enum.MaxValue);
			Throws (field, instance, Int64Enum.MaxValue);
			Throws (field, instance, UInt64Enum.MaxValue);
			field = fh.GetField ("field_Int16");
			field.SetValue (instance, Byte.MaxValue);
			Assert.AreEqual (Byte.MaxValue, instance.field_Int16);
			field.SetValue (instance, SByte.MaxValue);
			Assert.AreEqual (SByte.MaxValue, instance.field_Int16);
			Throws (field, instance, true);
			Throws (field, instance, Char.MaxValue);
			field.SetValue (instance, Int16.MaxValue);
			Assert.AreEqual (Int16.MaxValue, instance.field_Int16);
			Throws (field, instance, UInt16.MaxValue);
			Throws (field, instance, Int32.MaxValue);
			Throws (field, instance, UInt32.MaxValue);
			Throws (field, instance, Int64.MaxValue);
			Throws (field, instance, UInt64.MaxValue);
			Throws (field, instance, Single.MaxValue);
			Throws (field, instance, Double.MaxValue);
			Throws (field, instance, IntPtr.Zero);
			Throws (field, instance, UIntPtr.Zero);
			Throws (field, instance, Decimal.MaxValue);
			Throws (field, instance, DateTime.MaxValue);
			field.SetValue (instance, ByteEnum.MaxValue);
			Assert.AreEqual (Byte.MaxValue, instance.field_Int16);
			field.SetValue (instance, SByteEnum.MaxValue);
			Assert.AreEqual (SByte.MaxValue, instance.field_Int16);
			field.SetValue (instance, Int16Enum.MaxValue);
			Assert.AreEqual (Int16.MaxValue, instance.field_Int16);
			Throws (field, instance, UInt16Enum.MaxValue);
			Throws (field, instance, Int32Enum.MaxValue);
			Throws (field, instance, UInt32Enum.MaxValue);
			Throws (field, instance, Int64Enum.MaxValue);
			Throws (field, instance, UInt64Enum.MaxValue);
			field = fh.GetField ("field_UInt16");
			field.SetValue (instance, Byte.MaxValue);
			Assert.AreEqual (Byte.MaxValue, instance.field_UInt16);
			Throws (field, instance, SByte.MaxValue);
			Throws (field, instance, true);
			field.SetValue (instance, Char.MaxValue);
			Assert.AreEqual (Char.MaxValue, instance.field_UInt16);
			Throws (field, instance, Int16.MaxValue);
			field.SetValue (instance, UInt16.MaxValue);
			Assert.AreEqual (UInt16.MaxValue, instance.field_UInt16);
			Throws (field, instance, Int32.MaxValue);
			Throws (field, instance, UInt32.MaxValue);
			Throws (field, instance, Int64.MaxValue);
			Throws (field, instance, UInt64.MaxValue);
			Throws (field, instance, Single.MaxValue);
			Throws (field, instance, Double.MaxValue);
			Throws (field, instance, IntPtr.Zero);
			Throws (field, instance, UIntPtr.Zero);
			Throws (field, instance, Decimal.MaxValue);
			Throws (field, instance, DateTime.MaxValue);
			field.SetValue (instance, ByteEnum.MaxValue);
			Assert.AreEqual (Byte.MaxValue, instance.field_UInt16);
			Throws (field, instance, SByteEnum.MaxValue);
			Throws (field, instance, Int16Enum.MaxValue);
			field.SetValue (instance, UInt16Enum.MaxValue);
			Assert.AreEqual (UInt16.MaxValue, instance.field_UInt16);
			Throws (field, instance, Int32Enum.MaxValue);
			Throws (field, instance, UInt32Enum.MaxValue);
			Throws (field, instance, Int64Enum.MaxValue);
			Throws (field, instance, UInt64Enum.MaxValue);
			field = fh.GetField ("field_Int32");
			field.SetValue (instance, Byte.MaxValue);
			Assert.AreEqual (Byte.MaxValue, instance.field_Int32);
			field.SetValue (instance, SByte.MaxValue);
			Assert.AreEqual (SByte.MaxValue, instance.field_Int32);
			Throws (field, instance, true);
			field.SetValue (instance, Char.MaxValue);
			Assert.AreEqual (Char.MaxValue, instance.field_Int32);
			field.SetValue (instance, Int16.MaxValue);
			Assert.AreEqual (Int16.MaxValue, instance.field_Int32);
			field.SetValue (instance, UInt16.MaxValue);
			Assert.AreEqual (UInt16.MaxValue, instance.field_Int32);
			field.SetValue (instance, Int32.MaxValue);
			Assert.AreEqual (Int32.MaxValue, instance.field_Int32);
			Throws (field, instance, UInt32.MaxValue);
			Throws (field, instance, Int64.MaxValue);
			Throws (field, instance, UInt64.MaxValue);
			Throws (field, instance, Single.MaxValue);
			Throws (field, instance, Double.MaxValue);
			Throws (field, instance, IntPtr.Zero);
			Throws (field, instance, UIntPtr.Zero);
			Throws (field, instance, Decimal.MaxValue);
			Throws (field, instance, DateTime.MaxValue);
			field.SetValue (instance, ByteEnum.MaxValue);
			Assert.AreEqual (Byte.MaxValue, instance.field_Int32);
			field.SetValue (instance, SByteEnum.MaxValue);
			Assert.AreEqual (SByte.MaxValue, instance.field_Int32);
			field.SetValue (instance, Int16Enum.MaxValue);
			Assert.AreEqual (Int16.MaxValue, instance.field_Int32);
			field.SetValue (instance, UInt16Enum.MaxValue);
			Assert.AreEqual (UInt16.MaxValue, instance.field_Int32);
			field.SetValue (instance, Int32Enum.MaxValue);
			Assert.AreEqual (Int32.MaxValue, instance.field_Int32);
			Throws (field, instance, UInt32Enum.MaxValue);
			Throws (field, instance, Int64Enum.MaxValue);
			Throws (field, instance, UInt64Enum.MaxValue);
			field = fh.GetField ("field_UInt32");
			field.SetValue (instance, Byte.MaxValue);
			Assert.AreEqual (Byte.MaxValue, instance.field_UInt32);
			Throws (field, instance, SByte.MaxValue);
			Throws (field, instance, true);
			field.SetValue (instance, Char.MaxValue);
			Assert.AreEqual (Char.MaxValue, instance.field_UInt32);
			Throws (field, instance, Int16.MaxValue);
			field.SetValue (instance, UInt16.MaxValue);
			Assert.AreEqual (UInt16.MaxValue, instance.field_UInt32);
			Throws (field, instance, Int32.MaxValue);
			field.SetValue (instance, UInt32.MaxValue);
			Assert.AreEqual (UInt32.MaxValue, instance.field_UInt32);
			Throws (field, instance, Int64.MaxValue);
			Throws (field, instance, UInt64.MaxValue);
			Throws (field, instance, Single.MaxValue);
			Throws (field, instance, Double.MaxValue);
			Throws (field, instance, IntPtr.Zero);
			Throws (field, instance, UIntPtr.Zero);
			Throws (field, instance, Decimal.MaxValue);
			Throws (field, instance, DateTime.MaxValue);
			field.SetValue (instance, ByteEnum.MaxValue);
			Assert.AreEqual (Byte.MaxValue, instance.field_UInt32);
			Throws (field, instance, SByteEnum.MaxValue);
			Throws (field, instance, Int16Enum.MaxValue);
			field.SetValue (instance, UInt16Enum.MaxValue);
			Assert.AreEqual (UInt16.MaxValue, instance.field_UInt32);
			Throws (field, instance, Int32Enum.MaxValue);
			field.SetValue (instance, UInt32Enum.MaxValue);
			Assert.AreEqual (UInt32.MaxValue, instance.field_UInt32);
			Throws (field, instance, Int64Enum.MaxValue);
			Throws (field, instance, UInt64Enum.MaxValue);
			field = fh.GetField ("field_Int64");
			field.SetValue (instance, Byte.MaxValue);
			Assert.AreEqual (Byte.MaxValue, instance.field_Int64);
			field.SetValue (instance, SByte.MaxValue);
			Assert.AreEqual (SByte.MaxValue, instance.field_Int64);
			Throws (field, instance, true);
			field.SetValue (instance, Char.MaxValue);
			Assert.AreEqual (Char.MaxValue, instance.field_Int64);
			field.SetValue (instance, Int16.MaxValue);
			Assert.AreEqual (Int16.MaxValue, instance.field_Int64);
			field.SetValue (instance, UInt16.MaxValue);
			Assert.AreEqual (UInt16.MaxValue, instance.field_Int64);
			field.SetValue (instance, Int32.MaxValue);
			Assert.AreEqual (Int32.MaxValue, instance.field_Int64);
			field.SetValue (instance, UInt32.MaxValue);
			Assert.AreEqual (UInt32.MaxValue, instance.field_Int64);
			field.SetValue (instance, Int64.MaxValue);
			Assert.AreEqual (Int64.MaxValue, instance.field_Int64);
			Throws (field, instance, UInt64.MaxValue);
			Throws (field, instance, Single.MaxValue);
			Throws (field, instance, Double.MaxValue);
			Throws (field, instance, IntPtr.Zero);
			Throws (field, instance, UIntPtr.Zero);
			Throws (field, instance, Decimal.MaxValue);
			Throws (field, instance, DateTime.MaxValue);
			field.SetValue (instance, ByteEnum.MaxValue);
			Assert.AreEqual (Byte.MaxValue, instance.field_Int64);
			field.SetValue (instance, SByteEnum.MaxValue);
			Assert.AreEqual (SByte.MaxValue, instance.field_Int64);
			field.SetValue (instance, Int16Enum.MaxValue);
			Assert.AreEqual (Int16.MaxValue, instance.field_Int64);
			field.SetValue (instance, UInt16Enum.MaxValue);
			Assert.AreEqual (UInt16.MaxValue, instance.field_Int64);
			field.SetValue (instance, Int32Enum.MaxValue);
			Assert.AreEqual (Int32.MaxValue, instance.field_Int64);
			field.SetValue (instance, UInt32Enum.MaxValue);
			Assert.AreEqual (UInt32.MaxValue, instance.field_Int64);
			field.SetValue (instance, Int64Enum.MaxValue);
			Assert.AreEqual (Int64.MaxValue, instance.field_Int64);
			Throws (field, instance, UInt64Enum.MaxValue);
			field = fh.GetField ("field_UInt64");
			field.SetValue (instance, Byte.MaxValue);
			Assert.AreEqual (Byte.MaxValue, instance.field_UInt64);
			Throws (field, instance, SByte.MaxValue);
			Throws (field, instance, true);
			field.SetValue (instance, Char.MaxValue);
			Assert.AreEqual (Char.MaxValue, instance.field_UInt64);
			Throws (field, instance, Int16.MaxValue);
			field.SetValue (instance, UInt16.MaxValue);
			Assert.AreEqual (UInt16.MaxValue, instance.field_UInt64);
			Throws (field, instance, Int32.MaxValue);
			field.SetValue (instance, UInt32.MaxValue);
			Assert.AreEqual (UInt32.MaxValue, instance.field_UInt64);
			Throws (field, instance, Int64.MaxValue);
			field.SetValue (instance, UInt64.MaxValue);
			Assert.AreEqual (UInt64.MaxValue, instance.field_UInt64);
			Throws (field, instance, Single.MaxValue);
			Throws (field, instance, Double.MaxValue);
			Throws (field, instance, IntPtr.Zero);
			Throws (field, instance, UIntPtr.Zero);
			Throws (field, instance, Decimal.MaxValue);
			Throws (field, instance, DateTime.MaxValue);
			field.SetValue (instance, ByteEnum.MaxValue);
			Assert.AreEqual (Byte.MaxValue, instance.field_UInt64);
			Throws (field, instance, SByteEnum.MaxValue);
			Throws (field, instance, Int16Enum.MaxValue);
			field.SetValue (instance, UInt16Enum.MaxValue);
			Assert.AreEqual (UInt16.MaxValue, instance.field_UInt64);
			Throws (field, instance, Int32Enum.MaxValue);
			field.SetValue (instance, UInt32Enum.MaxValue);
			Assert.AreEqual (UInt32.MaxValue, instance.field_UInt64);
			Throws (field, instance, Int64Enum.MaxValue);
			field.SetValue (instance, UInt64Enum.MaxValue);
			Assert.AreEqual (UInt64.MaxValue, instance.field_UInt64);
			field = fh.GetField ("field_Single");
			field.SetValue (instance, Byte.MaxValue);
			Assert.AreEqual (Byte.MaxValue, instance.field_Single);
			field.SetValue (instance, SByte.MaxValue);
			Assert.AreEqual (SByte.MaxValue, instance.field_Single);
			Throws (field, instance, true);
			field.SetValue (instance, Char.MaxValue);
			Assert.AreEqual ((Single) Char.MaxValue, instance.field_Single);
			field.SetValue (instance, Int16.MaxValue);
			Assert.AreEqual (Int16.MaxValue, instance.field_Single);
			field.SetValue (instance, UInt16.MaxValue);
			Assert.AreEqual (UInt16.MaxValue, instance.field_Single);
			field.SetValue (instance, Int32.MaxValue);
			Assert.AreEqual ((Single)Int32.MaxValue, instance.field_Single);
			field.SetValue (instance, UInt32.MaxValue);
			Assert.AreEqual ((Single) UInt32.MaxValue, instance.field_Single);
			field.SetValue (instance, Int64.MaxValue);
			Assert.AreEqual (Int64.MaxValue, instance.field_Single);
			field.SetValue (instance, UInt64.MaxValue);
			Assert.AreEqual (UInt64.MaxValue, instance.field_Single);
			field.SetValue (instance, Single.MaxValue);
			Assert.AreEqual (Single.MaxValue, instance.field_Single);
			Throws (field, instance, Double.MaxValue);
			Throws (field, instance, IntPtr.Zero);
			Throws (field, instance, UIntPtr.Zero);
			Throws (field, instance, Decimal.MaxValue);
			Throws (field, instance, DateTime.MaxValue);
			field.SetValue (instance, ByteEnum.MaxValue);
			Assert.AreEqual (Byte.MaxValue, instance.field_Single);
			field.SetValue (instance, SByteEnum.MaxValue);
			Assert.AreEqual (SByte.MaxValue, instance.field_Single);
			field.SetValue (instance, Int16Enum.MaxValue);
			Assert.AreEqual (Int16.MaxValue, instance.field_Single);
			field.SetValue (instance, UInt16Enum.MaxValue);
			Assert.AreEqual (UInt16.MaxValue, instance.field_Single);
			field.SetValue (instance, Int32Enum.MaxValue);
			Assert.AreEqual ((Single) Int32.MaxValue, instance.field_Single);
			field.SetValue (instance, UInt32Enum.MaxValue);
			Assert.AreEqual ((Single) UInt32.MaxValue, instance.field_Single);
			field.SetValue (instance, Int64Enum.MaxValue);
			Assert.AreEqual (Int64.MaxValue, instance.field_Single);
			field.SetValue (instance, UInt64Enum.MaxValue);
			Assert.AreEqual (UInt64.MaxValue, instance.field_Single);
			field = fh.GetField ("field_Double");
			field.SetValue (instance, Byte.MaxValue);
			Assert.AreEqual (Byte.MaxValue, instance.field_Double);
			field.SetValue (instance, SByte.MaxValue);
			Assert.AreEqual (SByte.MaxValue, instance.field_Double);
			Throws (field, instance, true);
			field.SetValue (instance, Char.MaxValue);
			Assert.AreEqual ((Double) Char.MaxValue, instance.field_Double);
			field.SetValue (instance, Int16.MaxValue);
			Assert.AreEqual (Int16.MaxValue, instance.field_Double);
			field.SetValue (instance, UInt16.MaxValue);
			Assert.AreEqual (UInt16.MaxValue, instance.field_Double);
			field.SetValue (instance, Int32.MaxValue);
			Assert.AreEqual (Int32.MaxValue, instance.field_Double);
			field.SetValue (instance, UInt32.MaxValue);
			Assert.AreEqual (UInt32.MaxValue, instance.field_Double);
			field.SetValue (instance, Int64.MaxValue);
			Assert.AreEqual (Int64.MaxValue, instance.field_Double);
			field.SetValue (instance, UInt64.MaxValue);
			Assert.AreEqual (UInt64.MaxValue, instance.field_Double);
			field.SetValue (instance, Single.MaxValue);
			Assert.AreEqual (Single.MaxValue, instance.field_Double);
			field.SetValue (instance, Double.MaxValue);
			Assert.AreEqual (Double.MaxValue, instance.field_Double);
			Throws (field, instance, IntPtr.Zero);
			Throws (field, instance, UIntPtr.Zero);
			Throws (field, instance, Decimal.MaxValue);
			Throws (field, instance, DateTime.MaxValue);
			field.SetValue (instance, ByteEnum.MaxValue);
			Assert.AreEqual (Byte.MaxValue, instance.field_Double);
			field.SetValue (instance, SByteEnum.MaxValue);
			Assert.AreEqual (SByte.MaxValue, instance.field_Double);
			field.SetValue (instance, Int16Enum.MaxValue);
			Assert.AreEqual (Int16.MaxValue, instance.field_Double);
			field.SetValue (instance, UInt16Enum.MaxValue);
			Assert.AreEqual (UInt16.MaxValue, instance.field_Double);
			field.SetValue (instance, Int32Enum.MaxValue);
			Assert.AreEqual (Int32.MaxValue, instance.field_Double);
			field.SetValue (instance, UInt32Enum.MaxValue);
			Assert.AreEqual (UInt32.MaxValue, instance.field_Double);
			field.SetValue (instance, Int64Enum.MaxValue);
			Assert.AreEqual (Int64.MaxValue, instance.field_Double);
			field.SetValue (instance, UInt64Enum.MaxValue);
			Assert.AreEqual (UInt64.MaxValue, instance.field_Double);
			field = fh.GetField ("field_IntPtr");
			Throws (field, instance, Byte.MaxValue);
			Throws (field, instance, SByte.MaxValue);
			Throws (field, instance, true);
			Throws (field, instance, Char.MaxValue);
			Throws (field, instance, Int16.MaxValue);
			Throws (field, instance, UInt16.MaxValue);
			Throws (field, instance, Int32.MaxValue);
			Throws (field, instance, UInt32.MaxValue);
			Throws (field, instance, Int64.MaxValue);
			Throws (field, instance, UInt64.MaxValue);
			Throws (field, instance, Single.MaxValue);
			Throws (field, instance, Double.MaxValue);
			field.SetValue (instance, IntPtr.Zero);
			Assert.AreEqual (IntPtr.Zero, instance.field_IntPtr);
			Throws (field, instance, UIntPtr.Zero);
			Throws (field, instance, Decimal.MaxValue);
			Throws (field, instance, DateTime.MaxValue);
			Throws (field, instance, ByteEnum.MaxValue);
			Throws (field, instance, SByteEnum.MaxValue);
			Throws (field, instance, Int16Enum.MaxValue);
			Throws (field, instance, UInt16Enum.MaxValue);
			Throws (field, instance, Int32Enum.MaxValue);
			Throws (field, instance, UInt32Enum.MaxValue);
			Throws (field, instance, Int64Enum.MaxValue);
			Throws (field, instance, UInt64Enum.MaxValue);
			field = fh.GetField ("field_UIntPtr");
			Throws (field, instance, Byte.MaxValue);
			Throws (field, instance, SByte.MaxValue);
			Throws (field, instance, true);
			Throws (field, instance, Char.MaxValue);
			Throws (field, instance, Int16.MaxValue);
			Throws (field, instance, UInt16.MaxValue);
			Throws (field, instance, Int32.MaxValue);
			Throws (field, instance, UInt32.MaxValue);
			Throws (field, instance, Int64.MaxValue);
			Throws (field, instance, UInt64.MaxValue);
			Throws (field, instance, Single.MaxValue);
			Throws (field, instance, Double.MaxValue);
			Throws (field, instance, IntPtr.Zero);
			field.SetValue (instance, UIntPtr.Zero);
			Assert.AreEqual (UIntPtr.Zero, instance.field_UIntPtr);
			Throws (field, instance, Decimal.MaxValue);
			Throws (field, instance, DateTime.MaxValue);
			Throws (field, instance, ByteEnum.MaxValue);
			Throws (field, instance, SByteEnum.MaxValue);
			Throws (field, instance, Int16Enum.MaxValue);
			Throws (field, instance, UInt16Enum.MaxValue);
			Throws (field, instance, Int32Enum.MaxValue);
			Throws (field, instance, UInt32Enum.MaxValue);
			Throws (field, instance, Int64Enum.MaxValue);
			Throws (field, instance, UInt64Enum.MaxValue);
			field = fh.GetField ("field_Decimal");
			Throws (field, instance, Byte.MaxValue);
			Throws (field, instance, SByte.MaxValue);
			Throws (field, instance, true);
			Throws (field, instance, Char.MaxValue);
			Throws (field, instance, Int16.MaxValue);
			Throws (field, instance, UInt16.MaxValue);
			Throws (field, instance, Int32.MaxValue);
			Throws (field, instance, UInt32.MaxValue);
			Throws (field, instance, Int64.MaxValue);
			Throws (field, instance, UInt64.MaxValue);
			Throws (field, instance, Single.MaxValue);
			Throws (field, instance, Double.MaxValue);
			Throws (field, instance, IntPtr.Zero);
			Throws (field, instance, UIntPtr.Zero);
			field.SetValue (instance, Decimal.MaxValue);
			Assert.AreEqual (Decimal.MaxValue, instance.field_Decimal);
			Throws (field, instance, DateTime.MaxValue);
			Throws (field, instance, ByteEnum.MaxValue);
			Throws (field, instance, SByteEnum.MaxValue);
			Throws (field, instance, Int16Enum.MaxValue);
			Throws (field, instance, UInt16Enum.MaxValue);
			Throws (field, instance, Int32Enum.MaxValue);
			Throws (field, instance, UInt32Enum.MaxValue);
			Throws (field, instance, Int64Enum.MaxValue);
			Throws (field, instance, UInt64Enum.MaxValue);
			field = fh.GetField ("field_DateTime");
			Throws (field, instance, Byte.MaxValue);
			Throws (field, instance, SByte.MaxValue);
			Throws (field, instance, true);
			Throws (field, instance, Char.MaxValue);
			Throws (field, instance, Int16.MaxValue);
			Throws (field, instance, UInt16.MaxValue);
			Throws (field, instance, Int32.MaxValue);
			Throws (field, instance, UInt32.MaxValue);
			Throws (field, instance, Int64.MaxValue);
			Throws (field, instance, UInt64.MaxValue);
			Throws (field, instance, Single.MaxValue);
			Throws (field, instance, Double.MaxValue);
			Throws (field, instance, IntPtr.Zero);
			Throws (field, instance, UIntPtr.Zero);
			Throws (field, instance, Decimal.MaxValue);
			field.SetValue (instance, DateTime.MaxValue);
			Assert.AreEqual (DateTime.MaxValue, instance.field_DateTime);
			Throws (field, instance, ByteEnum.MaxValue);
			Throws (field, instance, SByteEnum.MaxValue);
			Throws (field, instance, Int16Enum.MaxValue);
			Throws (field, instance, UInt16Enum.MaxValue);
			Throws (field, instance, Int32Enum.MaxValue);
			Throws (field, instance, UInt32Enum.MaxValue);
			Throws (field, instance, Int64Enum.MaxValue);
			Throws (field, instance, UInt64Enum.MaxValue);
			field = fh.GetField ("field_ByteEnum");
			field.SetValue (instance, Byte.MaxValue);
			Assert.AreEqual (ByteEnum.MaxValue, instance.field_ByteEnum);
			Throws (field, instance, SByte.MaxValue);
			Throws (field, instance, true);
			Throws (field, instance, Char.MaxValue);
			Throws (field, instance, Int16.MaxValue);
			Throws (field, instance, UInt16.MaxValue);
			Throws (field, instance, Int32.MaxValue);
			Throws (field, instance, UInt32.MaxValue);
			Throws (field, instance, Int64.MaxValue);
			Throws (field, instance, UInt64.MaxValue);
			Throws (field, instance, Single.MaxValue);
			Throws (field, instance, Double.MaxValue);
			Throws (field, instance, IntPtr.Zero);
			Throws (field, instance, UIntPtr.Zero);
			Throws (field, instance, Decimal.MaxValue);
			Throws (field, instance, DateTime.MaxValue);
			field.SetValue (instance, ByteEnum.MaxValue);
			Assert.AreEqual (ByteEnum.MaxValue, instance.field_ByteEnum);
			Throws (field, instance, SByteEnum.MaxValue);
			Throws (field, instance, Int16Enum.MaxValue);
			Throws (field, instance, UInt16Enum.MaxValue);
			Throws (field, instance, Int32Enum.MaxValue);
			Throws (field, instance, UInt32Enum.MaxValue);
			Throws (field, instance, Int64Enum.MaxValue);
			Throws (field, instance, UInt64Enum.MaxValue);
			field = fh.GetField ("field_SByteEnum");
			Throws (field, instance, Byte.MaxValue);
			field.SetValue (instance, SByte.MaxValue);
			Assert.AreEqual (SByteEnum.MaxValue, instance.field_SByteEnum);
			Throws (field, instance, true);
			Throws (field, instance, Char.MaxValue);
			Throws (field, instance, Int16.MaxValue);
			Throws (field, instance, UInt16.MaxValue);
			Throws (field, instance, Int32.MaxValue);
			Throws (field, instance, UInt32.MaxValue);
			Throws (field, instance, Int64.MaxValue);
			Throws (field, instance, UInt64.MaxValue);
			Throws (field, instance, Single.MaxValue);
			Throws (field, instance, Double.MaxValue);
			Throws (field, instance, IntPtr.Zero);
			Throws (field, instance, UIntPtr.Zero);
			Throws (field, instance, Decimal.MaxValue);
			Throws (field, instance, DateTime.MaxValue);
			Throws (field, instance, ByteEnum.MaxValue);
			field.SetValue (instance, SByteEnum.MaxValue);
			Assert.AreEqual (SByteEnum.MaxValue, instance.field_SByteEnum);
			Throws (field, instance, Int16Enum.MaxValue);
			Throws (field, instance, UInt16Enum.MaxValue);
			Throws (field, instance, Int32Enum.MaxValue);
			Throws (field, instance, UInt32Enum.MaxValue);
			Throws (field, instance, Int64Enum.MaxValue);
			Throws (field, instance, UInt64Enum.MaxValue);
			field = fh.GetField ("field_Int16Enum");
			field.SetValue (instance, Byte.MaxValue);
			Assert.AreEqual (Byte.MaxValue, (byte) instance.field_Int16Enum);
			field.SetValue (instance, SByte.MaxValue);
			Assert.AreEqual (SByte.MaxValue, (sbyte) instance.field_Int16Enum);
			Throws (field, instance, true);
			Throws (field, instance, Char.MaxValue);
			field.SetValue (instance, Int16.MaxValue);
			Assert.AreEqual (Int16Enum.MaxValue, instance.field_Int16Enum);
			Throws (field, instance, UInt16.MaxValue);
			Throws (field, instance, Int32.MaxValue);
			Throws (field, instance, UInt32.MaxValue);
			Throws (field, instance, Int64.MaxValue);
			Throws (field, instance, UInt64.MaxValue);
			Throws (field, instance, Single.MaxValue);
			Throws (field, instance, Double.MaxValue);
			Throws (field, instance, IntPtr.Zero);
			Throws (field, instance, UIntPtr.Zero);
			Throws (field, instance, Decimal.MaxValue);
			Throws (field, instance, DateTime.MaxValue);
			field.SetValue (instance, ByteEnum.MaxValue);
			Assert.AreEqual (ByteEnum.MaxValue, (ByteEnum) instance.field_Int16Enum);
			field.SetValue (instance, SByteEnum.MaxValue);
			Assert.AreEqual (SByteEnum.MaxValue, (SByteEnum) instance.field_Int16Enum);
			field.SetValue (instance, Int16Enum.MaxValue);
			Assert.AreEqual (Int16Enum.MaxValue, instance.field_Int16Enum);
			Throws (field, instance, UInt16Enum.MaxValue);
			Throws (field, instance, Int32Enum.MaxValue);
			Throws (field, instance, UInt32Enum.MaxValue);
			Throws (field, instance, Int64Enum.MaxValue);
			Throws (field, instance, UInt64Enum.MaxValue);
			field = fh.GetField ("field_UInt16Enum");
			field.SetValue (instance, Byte.MaxValue);
			Assert.AreEqual (Byte.MaxValue, (byte) instance.field_UInt16Enum);
			Throws (field, instance, SByte.MaxValue);
			Throws (field, instance, true);
			field.SetValue (instance, Char.MaxValue);
			Assert.AreEqual (Char.MaxValue, (char) instance.field_UInt16Enum);
			Throws (field, instance, Int16.MaxValue);
			field.SetValue (instance, UInt16.MaxValue);
			Assert.AreEqual (UInt16.MaxValue, (UInt16) instance.field_UInt16Enum);
			Throws (field, instance, Int32.MaxValue);
			Throws (field, instance, UInt32.MaxValue);
			Throws (field, instance, Int64.MaxValue);
			Throws (field, instance, UInt64.MaxValue);
			Throws (field, instance, Single.MaxValue);
			Throws (field, instance, Double.MaxValue);
			Throws (field, instance, IntPtr.Zero);
			Throws (field, instance, UIntPtr.Zero);
			Throws (field, instance, Decimal.MaxValue);
			Throws (field, instance, DateTime.MaxValue);
			field.SetValue (instance, ByteEnum.MaxValue);
			Assert.AreEqual (ByteEnum.MaxValue, (ByteEnum) instance.field_UInt16Enum);
			Throws (field, instance, SByteEnum.MaxValue);
			Throws (field, instance, Int16Enum.MaxValue);
			field.SetValue (instance, UInt16Enum.MaxValue);
			Assert.AreEqual (UInt16Enum.MaxValue, instance.field_UInt16Enum);
			Throws (field, instance, Int32Enum.MaxValue);
			Throws (field, instance, UInt32Enum.MaxValue);
			Throws (field, instance, Int64Enum.MaxValue);
			Throws (field, instance, UInt64Enum.MaxValue);
			field = fh.GetField ("field_Int32Enum");
			field.SetValue (instance, Byte.MaxValue);
			Assert.AreEqual (Byte.MaxValue, (byte) instance.field_Int32Enum);
			field.SetValue (instance, SByte.MaxValue);
			Assert.AreEqual (SByte.MaxValue, (sbyte) instance.field_Int32Enum);
			Throws (field, instance, true);
			field.SetValue (instance, Char.MaxValue);
			Assert.AreEqual (Char.MaxValue, (char) instance.field_Int32Enum);
			field.SetValue (instance, Int16.MaxValue);
			Assert.AreEqual (Int16.MaxValue, (Int16) instance.field_Int32Enum);
			field.SetValue (instance, UInt16.MaxValue);
			Assert.AreEqual (UInt16.MaxValue, (UInt16) instance.field_Int32Enum);
			field.SetValue (instance, Int32.MaxValue);
			Assert.AreEqual (Int32.MaxValue, (Int32) instance.field_Int32Enum);
			Throws (field, instance, UInt32.MaxValue);
			Throws (field, instance, Int64.MaxValue);
			Throws (field, instance, UInt64.MaxValue);
			Throws (field, instance, Single.MaxValue);
			Throws (field, instance, Double.MaxValue);
			Throws (field, instance, IntPtr.Zero);
			Throws (field, instance, UIntPtr.Zero);
			Throws (field, instance, Decimal.MaxValue);
			Throws (field, instance, DateTime.MaxValue);
			field.SetValue (instance, ByteEnum.MaxValue);
			Assert.AreEqual (ByteEnum.MaxValue, (ByteEnum) instance.field_Int32Enum);
			field.SetValue (instance, SByteEnum.MaxValue);
			Assert.AreEqual (SByteEnum.MaxValue, (SByteEnum) instance.field_Int32Enum);
			field.SetValue (instance, Int16Enum.MaxValue);
			Assert.AreEqual (Int16Enum.MaxValue, (Int16Enum) instance.field_Int32Enum);
			field.SetValue (instance, UInt16Enum.MaxValue);
			Assert.AreEqual (UInt16Enum.MaxValue, (UInt16Enum) instance.field_Int32Enum);
			field.SetValue (instance, Int32Enum.MaxValue);
			Assert.AreEqual (Int32Enum.MaxValue, instance.field_Int32Enum);
			Throws (field, instance, UInt32Enum.MaxValue);
			Throws (field, instance, Int64Enum.MaxValue);
			Throws (field, instance, UInt64Enum.MaxValue);
			field = fh.GetField ("field_UInt32Enum");
			field.SetValue (instance, Byte.MaxValue);
			Assert.AreEqual (Byte.MaxValue, (byte) instance.field_UInt32Enum);
			Throws (field, instance, SByte.MaxValue);
			Throws (field, instance, true);
			field.SetValue (instance, Char.MaxValue);
			Assert.AreEqual (Char.MaxValue, (char) instance.field_UInt32Enum);
			Throws (field, instance, Int16.MaxValue);
			field.SetValue (instance, UInt16.MaxValue);
			Assert.AreEqual (UInt16.MaxValue, (UInt16) instance.field_UInt32Enum);
			Throws (field, instance, Int32.MaxValue);
			field.SetValue (instance, UInt32.MaxValue);
			Assert.AreEqual (UInt32.MaxValue, (UInt32) instance.field_UInt32Enum);
			Throws (field, instance, Int64.MaxValue);
			Throws (field, instance, UInt64.MaxValue);
			Throws (field, instance, Single.MaxValue);
			Throws (field, instance, Double.MaxValue);
			Throws (field, instance, IntPtr.Zero);
			Throws (field, instance, UIntPtr.Zero);
			Throws (field, instance, Decimal.MaxValue);
			Throws (field, instance, DateTime.MaxValue);
			field.SetValue (instance, ByteEnum.MaxValue);
			Assert.AreEqual (ByteEnum.MaxValue, (ByteEnum) instance.field_UInt32Enum);
			Throws (field, instance, SByteEnum.MaxValue);
			Throws (field, instance, Int16Enum.MaxValue);
			field.SetValue (instance, UInt16Enum.MaxValue);
			Assert.AreEqual (UInt16Enum.MaxValue, (UInt16Enum) instance.field_UInt32Enum);
			Throws (field, instance, Int32Enum.MaxValue);
			field.SetValue (instance, UInt32Enum.MaxValue);
			Assert.AreEqual (UInt32Enum.MaxValue, instance.field_UInt32Enum);
			Throws (field, instance, Int64Enum.MaxValue);
			Throws (field, instance, UInt64Enum.MaxValue);
			field = fh.GetField ("field_Int64Enum");
			field.SetValue (instance, Byte.MaxValue);
			Assert.AreEqual (Byte.MaxValue, (byte) instance.field_Int64Enum);
			field.SetValue (instance, SByte.MaxValue);
			Assert.AreEqual (SByte.MaxValue, (sbyte) instance.field_Int64Enum);
			Throws (field, instance, true);
			field.SetValue (instance, Char.MaxValue);
			Assert.AreEqual (Char.MaxValue, (char) instance.field_Int64Enum);
			field.SetValue (instance, Int16.MaxValue);
			Assert.AreEqual (Int16.MaxValue, (Int16) instance.field_Int64Enum);
			field.SetValue (instance, UInt16.MaxValue);
			Assert.AreEqual (UInt16.MaxValue, (UInt16) instance.field_Int64Enum);
			field.SetValue (instance, Int32.MaxValue);
			Assert.AreEqual (Int32.MaxValue, (Int32) instance.field_Int64Enum);
			field.SetValue (instance, UInt32.MaxValue);
			Assert.AreEqual (UInt32.MaxValue, (UInt32) instance.field_Int64Enum);
			field.SetValue (instance, Int64.MaxValue);
			Assert.AreEqual (Int64.MaxValue, (Int64) instance.field_Int64Enum);
			Throws (field, instance, UInt64.MaxValue);
			Throws (field, instance, Single.MaxValue);
			Throws (field, instance, Double.MaxValue);
			Throws (field, instance, IntPtr.Zero);
			Throws (field, instance, UIntPtr.Zero);
			Throws (field, instance, Decimal.MaxValue);
			Throws (field, instance, DateTime.MaxValue);
			field.SetValue (instance, ByteEnum.MaxValue);
			Assert.AreEqual (ByteEnum.MaxValue, (ByteEnum) instance.field_Int64Enum);
			field.SetValue (instance, SByteEnum.MaxValue);
			Assert.AreEqual (SByteEnum.MaxValue, (SByteEnum) instance.field_Int64Enum);
			field.SetValue (instance, Int16Enum.MaxValue);
			Assert.AreEqual (Int16Enum.MaxValue, (Int16Enum) instance.field_Int64Enum);
			field.SetValue (instance, UInt16Enum.MaxValue);
			Assert.AreEqual (UInt16Enum.MaxValue, (UInt16Enum) instance.field_Int64Enum);
			field.SetValue (instance, Int32Enum.MaxValue);
			Assert.AreEqual (Int32Enum.MaxValue, (Int32Enum) instance.field_Int64Enum);
			field.SetValue (instance, UInt32Enum.MaxValue);
			Assert.AreEqual (UInt32Enum.MaxValue, (UInt32Enum) instance.field_Int64Enum);
			field.SetValue (instance, Int64Enum.MaxValue);
			Assert.AreEqual (Int64Enum.MaxValue, instance.field_Int64Enum);
			Throws (field, instance, UInt64Enum.MaxValue);
			field = fh.GetField ("field_UInt64Enum");
			field.SetValue (instance, Byte.MaxValue);
			Assert.AreEqual (Byte.MaxValue, (byte) instance.field_UInt64Enum);
			Throws (field, instance, SByte.MaxValue);
			Throws (field, instance, true);
			field.SetValue (instance, Char.MaxValue);
			Assert.AreEqual (Char.MaxValue, (char) instance.field_UInt64Enum);
			Throws (field, instance, Int16.MaxValue);
			field.SetValue (instance, UInt16.MaxValue);
			Assert.AreEqual (UInt16.MaxValue, (UInt16) instance.field_UInt64Enum);
			Throws (field, instance, Int32.MaxValue);
			field.SetValue (instance, UInt32.MaxValue);
			Assert.AreEqual (UInt32.MaxValue, (UInt32) instance.field_UInt64Enum);
			Throws (field, instance, Int64.MaxValue);
			field.SetValue (instance, UInt64.MaxValue);
			Assert.AreEqual (UInt64.MaxValue, (UInt64) instance.field_UInt64Enum);
			Throws (field, instance, Single.MaxValue);
			Throws (field, instance, Double.MaxValue);
			Throws (field, instance, IntPtr.Zero);
			Throws (field, instance, UIntPtr.Zero);
			Throws (field, instance, Decimal.MaxValue);
			Throws (field, instance, DateTime.MaxValue);
			field.SetValue (instance, ByteEnum.MaxValue);
			Assert.AreEqual (ByteEnum.MaxValue, (ByteEnum) instance.field_UInt64Enum);
			Throws (field, instance, SByteEnum.MaxValue);
			Throws (field, instance, Int16Enum.MaxValue);
			field.SetValue (instance, UInt16Enum.MaxValue);
			Assert.AreEqual (UInt16Enum.MaxValue, (UInt16Enum) instance.field_UInt64Enum);
			Throws (field, instance, Int32Enum.MaxValue);
			field.SetValue (instance, UInt32Enum.MaxValue);
			Assert.AreEqual (UInt32Enum.MaxValue, (UInt32Enum) instance.field_UInt64Enum);
			Throws (field, instance, Int64Enum.MaxValue);
			field.SetValue (instance, UInt64Enum.MaxValue);
			Assert.AreEqual (UInt64Enum.MaxValue, instance.field_UInt64Enum);

		}

		static void Throws (FieldInfo field, object instance, object value)
		{
			try {
				field.SetValue (instance, value);
				Assert.Fail ("ArgumentException expected");
			} catch (ArgumentException ex) {
			}
		}

		public object[] ObjectArrayField;

		[Test]
		public void TestSetValueArray ()
		{
			var field = typeof (FieldInfoTest).GetField ("ObjectArrayField");
			var instance = new FieldInfoTest ();
			field.SetValue (instance, new string[] { "3" });
			field.SetValue (instance, null);

			Throws (field, instance, new int[] { 3 });
		}

		struct TestFields {
			public int MaxValue;
			public string str;
		}

		[Test]
		public void SetValueDirect ()
		{
			TestFields fields = new TestFields { MaxValue = 1234, str = "A" };

			FieldInfo info = fields.GetType ().GetField ("MaxValue");
			TypedReference reference = __makeref(fields);
			info.SetValueDirect (reference, 4096);
			Assert.AreEqual (4096, fields.MaxValue);

			info = fields.GetType ().GetField ("str");
			reference = __makeref(fields);
			info.SetValueDirect (reference, "B");
			Assert.AreEqual ("B", fields.str);
		}

#if !DISABLE_REMOTING
		[Test]
		[Category ("Remoting")]
		public void GetValueContextBoundObject ()
		{
			var instance = new CBOTest ();

			var field1 = typeof (CBOTest).GetField ("d1");
			var d1 = field1.GetValue (instance);
			Assert.AreEqual ((double)d1, 14.0, "d1");

			var field2 = typeof (CBOTest).GetField ("d2");
			var d2 = field2.GetValue (instance);
			Assert.AreEqual ((double)d2, -20, "d2");

			var field3 = typeof (CBOTest).GetField ("s1");
			var s1 = field3.GetValue (instance);
			Assert.AreEqual (s1, "abcd", "s1");

			var field4 = typeof (CBOTest).GetField ("s2");
			var s2 = field4.GetValue (instance);
			Assert.AreEqual (s2, "hijkl", "s2");
		}

		[Test]
		[Category ("Remoting")]
		public void SetValueContextBoundObject ()
		{
			var instance = new CBOTest ();

			var field1 = typeof (CBOTest).GetField ("d1");
			field1.SetValue (instance, 90.3);
			var d1 = field1.GetValue (instance);
			Assert.AreEqual ((double)d1, 90.3, "d1");

			var field2 = typeof (CBOTest).GetField ("d2");
			field2.SetValue (instance, 1);
			var d2 = field2.GetValue (instance);
			Assert.AreEqual ((double)d2, 1, "d2");

			var field3 = typeof (CBOTest).GetField ("s1");
			field3.SetValue (instance, "//////");
			var s1 = field3.GetValue (instance);
			Assert.AreEqual (s1, "//////", "s1");

			var field4 = typeof (CBOTest).GetField ("s2");
			field4.SetValue (instance, "This is a string");
			var s2 = field4.GetValue (instance);
			Assert.AreEqual (s2, "This is a string", "s2");

		}
#endif

		class CBOTest : ContextBoundObject {
			public double d1 = 14.0;
			public double d2 = -20.0;
			public string s1 = "abcd";
			public string s2 = "hijkl";
		}


		public IntEnum PPP;

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

		class FieldInfoWrapper : FieldInfo
		{
			private FieldInfo fieldInfo;

			public FieldInfoWrapper (FieldInfo fieldInfo)
			{
				this.fieldInfo = fieldInfo;
			}

			public override FieldAttributes Attributes => fieldInfo.Attributes;
			public override Type DeclaringType => fieldInfo.DeclaringType;
			public override RuntimeFieldHandle FieldHandle => fieldInfo.FieldHandle;
			public override Type FieldType => fieldInfo.FieldType;
			public override string Name => fieldInfo.Name;
			public override Type ReflectedType => fieldInfo.ReflectedType;
			
			public override object[] GetCustomAttributes (bool inherit) => fieldInfo.GetCustomAttributes (inherit); 
			public override object[] GetCustomAttributes (Type attributeType, bool inherit) => fieldInfo.GetCustomAttributes (attributeType, inherit);
			public override object GetValue (object obj) => fieldInfo.GetValue (obj);
			public override bool IsDefined (Type attributeType, bool inherit) => fieldInfo.IsDefined (attributeType, inherit);
			public override void SetValue (object obj, object value, BindingFlags invokeAttr, Binder binder, CultureInfo culture) =>
				fieldInfo.SetValue (obj, value, invokeAttr, binder, culture);
		}

		[Test]
		public void CustomFieldInfo () 
		{
			var fieldInfoWrapper = new FieldInfoWrapper (GetType ().GetField (nameof (non_const_field)));
			MethodInfo method = typeof (FieldInfoWrapper).GetMethod ("GetFieldOffset", BindingFlags.NonPublic | BindingFlags.Instance);
			Assert.IsNotNull (method);
			Assert.IsTrue (method.IsVirtual);

			var ex = Assert.Catch<Exception> (() => method.Invoke (fieldInfoWrapper, new object[] {}));
			Assert.IsTrue (ex.InnerException is SystemException);
		}
	}

	// We do not refernece the field, that is expected
#pragma warning disable 169
	// Helper classes
	class RefOnlyFieldClass 
	{
		// Helper property
		static int RefOnlyField;
	}
#pragma warning restore 169
	
	class NonPublicFieldClass
	{
		protected int protectedField;

		public void Dummy ()
		{
			protectedField = 1;
		}
	}

	public class FieldInfoTest<T>
	{
		public T TestField;
	}
}
