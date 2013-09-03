//
// ParameterInfoTest - NUnit Test Cases for the ParameterInfo class
//
// Zoltan Varga (vargaz@freemail.hu)
//
// (C) Ximian, Inc.  http://www.ximian.com
// Copyright 2011 Xamarin Inc (http://www.xamarin.com).
//
//

using System;
using System.Threading;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Collections.Generic;

using NUnit.Framework;

namespace MonoTests.System.Reflection
{
	public class Marshal1 : ICustomMarshaler
	{
		public static ICustomMarshaler GetInstance (string s)
		{
			return new Marshal1 ();
		}

		public void CleanUpManagedData (object managedObj)
		{
		}

		public void CleanUpNativeData (IntPtr pNativeData)
		{
		}

		public int GetNativeDataSize ()
		{
			return 4;
		}

		public IntPtr MarshalManagedToNative (object managedObj)
		{
			return IntPtr.Zero;
	 	}

		public object MarshalNativeToManaged (IntPtr pNativeData)
		{
			return null;
		}
	}

	[TestFixture]
	public class ParameterInfoTest
	{
		[Test]
		public void IsDefined_AttributeType_Null ()
		{
			MethodInfo mi = typeof (object).GetMethod ("Equals", 
				new Type [1] { typeof (object) });
			ParameterInfo pi = mi.GetParameters () [0];

			try {
				pi.IsDefined ((Type) null, false);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("attributeType", ex.ParamName, "#6");
			}
		}

#if NET_2_0 && !NET_2_1
		public enum ParamEnum {
			None = 0,
			Foo = 1,
			Bar = 2
		};

		public static void paramMethod (int i, [In] int j, [Out] int k, [Optional] int l, [In,Out] int m, ParamEnum n = ParamEnum.Foo)
		{
		}

#if !TARGET_JVM // No support for extern methods in TARGET_JVM
		[DllImport ("foo")]
		public extern static void marshalAsMethod (
			[MarshalAs(UnmanagedType.Bool)]int p0, 
			[MarshalAs(UnmanagedType.LPArray, ArraySubType=UnmanagedType.LPStr)] string [] p1,
			[MarshalAs( UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof (Marshal1), MarshalCookie = "5")] object p2);
#endif
		[Test]
		public void DefaultValueEnum () {
			ParameterInfo[] info = typeof (ParameterInfoTest).GetMethod ("paramMethod").GetParameters ();

			Assert.AreEqual (typeof (ParamEnum), info [5].DefaultValue.GetType (), "#1");
			Assert.AreEqual (ParamEnum.Foo, info [5].DefaultValue, "#2");
		}

		public static void Sample2 ([DecimalConstantAttribute(2,2,2,2,2)] decimal a, [DateTimeConstantAttribute(123456)] DateTime b) {}

		[Test]
		public void DefaultValuesFromCustomAttr () {
			ParameterInfo[] info = typeof (ParameterInfoTest).GetMethod ("Sample2").GetParameters ();

			Assert.AreEqual (typeof (Decimal), info [0].DefaultValue.GetType (), "#1");
			Assert.AreEqual (typeof (DateTime), info [1].DefaultValue.GetType (), "#2");
		}

		[Test] // bug #339013
		public void TestDefaultValues ()
		{
			ParameterInfo [] pi = typeof (ParameterInfoTest).GetMethod ("Sample").GetParameters ();

			Assert.AreEqual (pi [0].DefaultValue.GetType (), typeof (DBNull), "#1");
			Assert.AreEqual (pi [1].DefaultValue.GetType (), typeof (Missing), "#2");
		}

		public void Sample (int a, [Optional] int b)
		{
		}

		[Test]
		public void PseudoCustomAttributes () {
			ParameterInfo[] info = typeof (ParameterInfoTest).GetMethod ("paramMethod").GetParameters ();
			Assert.AreEqual (0, info[0].GetCustomAttributes (true).Length, "#A1");
			Assert.AreEqual (1, info[1].GetCustomAttributes (typeof (InAttribute), true).Length, "#A2");
			Assert.AreEqual (1, info[2].GetCustomAttributes (typeof (OutAttribute), true).Length, "#A3");
			Assert.AreEqual (1, info[3].GetCustomAttributes (typeof (OptionalAttribute), true).Length, "#A4");
			Assert.AreEqual (2, info[4].GetCustomAttributes (true).Length, "#A5");

#if !TARGET_JVM // No support for extern methods in TARGET_JVM
			ParameterInfo[] pi = typeof (ParameterInfoTest).GetMethod ("marshalAsMethod").GetParameters ();
			MarshalAsAttribute attr;

			attr = (MarshalAsAttribute)(pi [0].GetCustomAttributes (true) [0]);
			Assert.AreEqual (UnmanagedType.Bool, attr.Value, "#B");

			attr = (MarshalAsAttribute)(pi [1].GetCustomAttributes (true) [0]);
			Assert.AreEqual (UnmanagedType.LPArray, attr.Value, "#C1");
			Assert.AreEqual (UnmanagedType.LPStr, attr.ArraySubType, "#C2");

			attr = (MarshalAsAttribute)(pi [2].GetCustomAttributes (true) [0]);
			Assert.AreEqual (UnmanagedType.CustomMarshaler, attr.Value, "#D1");
			Assert.AreEqual ("5", attr.MarshalCookie, "#D2");
			Assert.AreEqual (typeof (Marshal1), Type.GetType (attr.MarshalType), "#D3");
#endif
		}

		[Test] // bug #342536
		public void Generics_Name ()
		{
			MethodInfo mi;
			Type type;
			ParameterInfo [] info;
		
			type = typeof (BaseType<string>);

			mi = type.GetMethod ("GetItems");
			Assert.IsNotNull (mi, "#A1");
			info = mi.GetParameters ();
			Assert.AreEqual (1, info.Length, "#A2");
			Assert.AreEqual ("count", info [0].Name, "#A3");

			mi = type.GetMethod ("Add");
			Assert.IsNotNull (mi, "#B1");
			info = mi.GetParameters ();
			Assert.AreEqual (2, info.Length, "#B2");
			Assert.AreEqual ("item", info [0].Name, "#B3");
			Assert.AreEqual ("index", info [1].Name, "#B4");

			mi = type.GetMethod ("Create");
			Assert.IsNotNull (mi, "#C1");
			info = mi.GetParameters ();
			Assert.AreEqual (2, info.Length, "#C2");
			Assert.AreEqual ("x", info [0].Name, "#C3");
			Assert.AreEqual ("item", info [1].Name, "#C4");
		}

		public class BaseType <T>
		{
			public void GetItems (int count)
			{
			}

			public void Add (T item, int index)
			{
			}

			public V Create <V> (int x, T item)
			{
				return default (V);
			}
		}
#endif

		[Test]
		public void Member () {
			ParameterInfo parm = typeof (Derived).GetMethod ("SomeMethod").GetParameters()[0];
			Assert.AreEqual (typeof (Derived), parm.Member.ReflectedType);
			Assert.AreEqual (typeof (Base), parm.Member.DeclaringType);
		}

		[Test]
		public void ArrayMethodParameters ()
		{
			var matrix_int_get = typeof (int[,,]).GetMethod ("Get");
			var parameters = matrix_int_get.GetParameters ();

			Assert.AreEqual (3, parameters.Length);
			Assert.AreEqual (0, parameters [0].GetCustomAttributes (false).Length);
			Assert.AreEqual (0, parameters [1].GetCustomAttributes (false).Length);
			Assert.AreEqual (0, parameters [2].GetCustomAttributes (false).Length);
		}

		class Base
		{
			public void SomeMethod( int x )
			{
			}
		}

		class Derived : Base
		{
		}

#if NET_4_0
		public static void TestC (decimal u = decimal.MaxValue) {
		}

		[Test]
		public void DefaultValueDecimal () {
			var info = typeof (ParameterInfoTest).GetMethod ("TestC").GetParameters ();
			Assert.AreEqual (decimal.MaxValue, info [0].DefaultValue);
		}

		class MyParameterInfo2 : ParameterInfo
		{
			public ParameterAttributes MyAttrsImpl;

			public override ParameterAttributes Attributes {
				get {return MyAttrsImpl;}
			}

			public IList<CustomAttributeData> myList = new List<CustomAttributeData> ();

			public override IList<CustomAttributeData> GetCustomAttributesData () {
				return myList;
			}
		}

		class MyParameterInfo : ParameterInfo
		{
			public void SetClassImpl (Type t)
			{
				ClassImpl = t;
			}

			public void SetDefaultValueImpl (object o)
			{
				DefaultValueImpl = o;
			}

			public void SetMemberImpl (MemberInfo o)
			{
				MemberImpl = o;
			}

			public void SetNameImpl (string s)
			{
				NameImpl = s;
			}

			public void SetPositionImpl (int i)
			{
				PositionImpl = i;
			}

			public void SetAttrsImpl (ParameterAttributes a)
			{
				AttrsImpl = a;
			}

			public void TestMethod (int a) {}
			public int this[int x, int y] {
				get { return 0; }
				set { }
			}

		}

		[Test]
		public void SubClassWithNoOverrides ()
		{
			var p = new MyParameterInfo ();
			Assert.IsFalse (p.IsDefined (typeof (FlagsAttribute), false), "#1");
			Assert.AreEqual (0, p.GetCustomAttributes (false).Length, "#2");
			Assert.AreEqual (0, p.GetCustomAttributes (typeof (FlagsAttribute), false).Length, "#3");
			Assert.AreEqual (0, p.GetOptionalCustomModifiers ().Length, "#4");
			Assert.AreEqual (0, p.GetRequiredCustomModifiers ().Length, "#5");
#if NET_4_5
			try {
				var ign = p.HasDefaultValue;
				Assert.Fail ("#6");
			} catch (NotImplementedException) {
			}
#endif
			Assert.IsFalse (p.IsIn, "#7");
			Assert.IsFalse (p.IsLcid, "#8");
			Assert.IsFalse (p.IsOptional, "#9");
			Assert.IsFalse (p.IsOut, "#10");
			Assert.IsFalse (p.IsRetval, "#10");
#if NET_4_5
			try {
				var ign = p.CustomAttributes;
				Assert.Fail ("#11");
			} catch (NotImplementedException) {
			}
#endif
			try {
				p.GetCustomAttributesData ();
				Assert.Fail ("#12");
			} catch (NotImplementedException) {
			}

			Assert.AreEqual (0x8000000, p.MetadataToken, "#13");
			Assert.AreEqual (0, p.Position, "#14");
			try {
				var ign = p.DefaultValue;
				Assert.Fail ("#15");
			} catch (NotImplementedException) {
			}
			try {
				var ign = p.RawDefaultValue;
				Assert.Fail ("#16");
			} catch (NotImplementedException) {
			}
			Assert.IsNull (p.Member, "#17");
			Assert.AreEqual (ParameterAttributes.None, p.Attributes, "#18");
			Assert.IsNull (p.Name, "#19");
			Assert.IsNull (p.ParameterType, "#20");
		}

		[Test]
		public void SubClassWithValuesSet ()
		{
			var p = new MyParameterInfo ();
			p.SetClassImpl (typeof (Decimal));
			Assert.AreEqual (typeof (Decimal), p.ParameterType, "#1");
			p.SetClassImpl (null);

			p.SetDefaultValueImpl ("foo");
			try {
				var ign = p.DefaultValue;
				Assert.Fail ("#2");
			} catch (NotImplementedException) {
			}
			p.SetDefaultValueImpl (null);

			var obj = typeof (object);
			p.SetMemberImpl (obj);
			Assert.AreEqual (obj, p.Member, "#3");
			Assert.AreEqual (0x8000000, p.MetadataToken, "#4");
			p.SetMemberImpl (null);

			var method = typeof (MyParameterInfo).GetMethod ("TestMethod");
			p.SetMemberImpl (method);
			Assert.IsNotNull (method, "#5");
			Assert.AreEqual (method, p.Member, "#6");
			Assert.AreEqual (0x8000000, p.MetadataToken, "#7");
			p.SetMemberImpl (null);

			var property = typeof (MyParameterInfo).GetProperty ("Item");
			p.SetMemberImpl (property);
			Assert.IsNotNull (property, "#8");
			Assert.AreEqual (property, p.Member, "#9");
			Assert.AreEqual (0x8000000, p.MetadataToken, "#10");
			p.SetMemberImpl (null);

			p.SetNameImpl ("foo");
			Assert.AreEqual ("foo", p.Name, "#11");
			p.SetNameImpl (null);

			p.SetPositionImpl (99);
			Assert.AreEqual (p.Position, 99, "#12");
			Assert.AreEqual (p.MetadataToken, 0x8000000, "#13");
			p.SetPositionImpl (0);

			Assert.IsFalse (p.IsIn, "#14");
			p.SetAttrsImpl (ParameterAttributes.In);
			Assert.IsTrue (p.IsIn, "#15");
		}

		[Test]
		public void SubClassWithOverrides()
		{
			var p2 = new MyParameterInfo2 ();
			Assert.IsFalse (p2.IsIn, "#1");
			p2.MyAttrsImpl = ParameterAttributes.In;
			Assert.IsTrue (p2.IsIn, "#2");
#if NET_4_5
			Assert.AreEqual (p2.myList, p2.CustomAttributes, "#3");
#endif
		}
#endif
	}
}
