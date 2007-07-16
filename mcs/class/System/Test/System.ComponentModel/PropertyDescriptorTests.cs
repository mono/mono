//
// System.ComponentModel.PropertyDescriptor test cases
//
// Authors:
// 	Chris Toshok (toshok@ximian.com)
//
// (c) 2006 Novell, Inc. (http://www.novell.com/)
//

using System;
using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Drawing.Design;
using System.ComponentModel.Design;

using NUnit.Framework;

namespace MonoTests.System.ComponentModel
{
	[TestFixture]
	public class PropertyDescriptorTests
	{
		class MissingConverterType_test
		{
			public class NestedClass { }

			[TypeConverter ("missing-type-name")]
			public NestedClass Prop {
				get { return null; }
			}

			[TypeConverter ("missing-type-name")]
			public int IntProp {
				get { return 5; }
			}

			[TypeConverter ("missing-type-name")]
			public string StringProp {
				get { return ""; }
			}
		}

		class ReadOnlyProperty_test
		{
			public int Prop {
				get { return 5; }
			}
		}

		class ReadOnlyAttribute_test
		{
			[ReadOnly (true)]
			public int Prop {
				get { return 5; }
				set { }
			}
		}

		class ConflictingReadOnly_test
		{
			[ReadOnly (false)]
			public int Prop {
				get { return 5; }
			}
		}

		class ShouldSerialize_public_test
		{
			public int Prop {
				get { return 5; }
			}

			public bool ShouldSerializeProp()
			{
				return false;
			}
		}

		class ShouldSerialize_protected_test
		{
			public int Prop {
				get { return 5; }
			}

			protected bool ShouldSerializeProp()
			{
				return false;
			}
		}

		class ShouldSerialize_private_test
		{
			public int Prop {
				get { return 5; }
			}

			private bool ShouldSerializeProp()
			{
				return false;
			}
		}

		class ShouldSerializeFalseEffectOnCanReset_test
		{
			public int Prop {
				get { return 5; }
				set { }
			}

			public bool ShouldSerializeProp()
			{
				return false;
			}

			public void ResetProp()
			{
			}
		}

		class ShouldSerialize_Null_Default
		{
			[DefaultValue (null)]
			public string Prop {
				get { return _prop; }
				set { _prop = value; }
			}

			public bool SerializeProp {
				get { return _serializeProp; }
				set { _serializeProp = value; }
			}

			public bool ShouldSerializeProp ()
			{
				return _serializeProp;
			}

			private string _prop;
			private bool _serializeProp;
		}

		class ShouldSerialize_No_Default
		{
			public string Prop {
				get { return _prop; }
				set { _prop = value; }
			}

			private string _prop;
		}

		class ShouldSerialize_ReadOnly
		{
			[ReadOnly (true)]
			[DefaultValue ("ok")]
			public string Prop1 {
				get { return _prop1; }
				set { _prop1 = value; }
			}

			[ReadOnly (false)]
			public string Prop2 {
				get { return _prop2; }
				set { _prop2 = value; }
			}

			[ReadOnly (true)]
			public string Prop3 {
				get { return _prop3; }
				set { _prop3 = value; }
			}

			[ReadOnly (false)]
			public string Prop4 {
				get { return _prop4; }
				set { _prop4 = value; }
			}

			public string Prop5 {
				get { return _prop5; }
			}

			[DefaultValue ("bad")]
			public string Prop6 {
				get { return _prop6; }
			}

			[ReadOnly (true)]
			[DefaultValue ("good")]
			public string Prop7 {
				get { return _prop7; }
				set { _prop7 = value; }
			}

			public bool SerializeProp3 {
				get { return _serializeProp3; }
				set { _serializeProp3 = value; }
			}

			public bool SerializeProp4 {
				get { return _serializeProp4; }
				set { _serializeProp4 = value; }
			}

			public bool SerializeProp5 {
				get { return _serializeProp5; }
				set { _serializeProp5 = value; }
			}

			public bool SerializeProp6 {
				get { return _serializeProp6; }
				set { _serializeProp6 = value; }
			}

			public bool SerializeProp7 {
				get { return _serializeProp7; }
				set { _serializeProp7 = value; }
			}

			public bool ShouldSerializeProp3 ()
			{
				return _serializeProp3;
			}

			public bool ShouldSerializeProp4 ()
			{
				return _serializeProp4;
			}

			public bool ShouldSerializeProp5 ()
			{
				return _serializeProp5;
			}

			public bool ShouldSerializeProp6 ()
			{
				return _serializeProp6;
			}

			public bool ShouldSerializeProp7 ()
			{
				return _serializeProp7;
			}

			private string _prop1;
			private string _prop2;
			private string _prop3;
			private string _prop4;
			private string _prop5 = "good";
			private string _prop6 = "bad";
			private string _prop7;
			private bool _serializeProp3;
			private bool _serializeProp4;
			private bool _serializeProp5;
			private bool _serializeProp6;
			private bool _serializeProp7;
		}

		class NoSerializeOrResetProp_test
		{
			public int Prop {
				get { return 5; }
			}
		}

		class CanReset_public_test
		{
			int prop = 5;
			public int Prop {
				get { return prop; }
				set { prop = value; }
			}

			public void ResetProp()
			{
				prop = 10;
			}
		}

		class CanReset_protected_test
		{
			int prop = 5;
			public int Prop {
				get { return prop; }
				set { prop = value; }
			}

			protected void ResetProp()
			{
				prop = 10;
			}
		}

		class CanReset_private_test
		{
			int prop = 5;
			public int Prop {
				get { return prop; }
				set { prop = value; }
			}

			private void ResetProp()
			{
				prop = 10;
			}
		}

		class CanResetNoSetter_test
		{
			int prop = 5;
			public int Prop {
				get { return prop; }
			}

			private void ResetProp()
			{
				prop = 10;
			}
		}

		class DisplayName_test
		{
#if NET_2_0
			[DisplayName ("An explicit displayname")]
#endif
			public bool Explicit {
				get { return false; }
			}

			public bool Implicit {
				get { return false; }
			}
		}

		[Test]
		public void MissingTypeConverter ()
		{
			PropertyDescriptor p1 = TypeDescriptor.GetProperties (typeof (MissingConverterType_test))["Prop"];
			PropertyDescriptor p2 = TypeDescriptor.GetProperties (typeof (MissingConverterType_test))["IntProp"];
			PropertyDescriptor p3 = TypeDescriptor.GetProperties (typeof (MissingConverterType_test))["StringProp"];

			Assert.AreEqual (typeof (TypeConverter), p1.Converter.GetType (), "1");
			Assert.AreEqual (typeof (Int32Converter), p2.Converter.GetType (), "2");
			Assert.AreEqual (typeof (StringConverter), p3.Converter.GetType (), "3");
		}

		[Test]
		public void ShouldSerializeTest_public ()
		{
			PropertyDescriptor p = TypeDescriptor.GetProperties (typeof (ShouldSerialize_public_test))["Prop"];
			ShouldSerialize_public_test test = new ShouldSerialize_public_test ();

			Assert.IsFalse (p.ShouldSerializeValue (test), "1");
		}

		[Test]
		public void ShouldSerializeTest_protected ()
		{
			PropertyDescriptor p = TypeDescriptor.GetProperties (typeof (ShouldSerialize_protected_test))["Prop"];
			ShouldSerialize_protected_test test = new ShouldSerialize_protected_test ();

			Assert.IsFalse (p.ShouldSerializeValue (test), "1");
		}

		[Test]
		public void ShouldSerializeTest_private ()
		{
			PropertyDescriptor p = TypeDescriptor.GetProperties (typeof (ShouldSerialize_protected_test))["Prop"];
			ShouldSerialize_protected_test test = new ShouldSerialize_protected_test ();

			Assert.IsFalse (p.ShouldSerializeValue (test), "1");
		}

		[Test]
		public void ShouldSerializeTest_No_Default ()
		{
			PropertyDescriptor p = TypeDescriptor.GetProperties (typeof (ShouldSerialize_No_Default)) ["Prop"];
			ShouldSerialize_No_Default test = new ShouldSerialize_No_Default ();

			Assert.IsTrue (p.ShouldSerializeValue (test), "#1");
			test.Prop = "whatever";
			Assert.IsTrue (p.ShouldSerializeValue (test), "#2");
		}

		[Test]
		public void ShouldSerializeTest_Null_Default ()
		{
			PropertyDescriptor p = TypeDescriptor.GetProperties (typeof (ShouldSerialize_Null_Default)) ["Prop"];
			ShouldSerialize_Null_Default test = new ShouldSerialize_Null_Default ();

			Assert.IsFalse (p.ShouldSerializeValue (test), "#1");
			test.SerializeProp = true;
			Assert.IsFalse (p.ShouldSerializeValue (test), "#2");
			test.Prop = "whatever";
			Assert.IsTrue (p.ShouldSerializeValue (test), "#3");
			test.SerializeProp = false;
			Assert.IsTrue (p.ShouldSerializeValue (test), "#4");
		}

		[Test]
		public void ShouldSerializeTest_ReadOnly ()
		{
			PropertyDescriptorCollection properties = TypeDescriptor.GetProperties (
				typeof (ShouldSerialize_ReadOnly));
			ShouldSerialize_ReadOnly test = new ShouldSerialize_ReadOnly ();

			PropertyDescriptor prop1PD = properties ["Prop1"];
			PropertyDescriptor prop2PD = properties ["Prop2"];
			PropertyDescriptor prop3PD = properties ["Prop3"];
			PropertyDescriptor prop4PD = properties ["Prop4"];
			PropertyDescriptor prop5PD = properties ["Prop5"];
			PropertyDescriptor prop6PD = properties ["Prop6"];
			PropertyDescriptor prop7PD = properties ["Prop7"];

			Assert.IsFalse (prop1PD.ShouldSerializeValue (test), "#A1");
			Assert.IsTrue (prop2PD.ShouldSerializeValue (test), "#A2");
			Assert.IsFalse (prop3PD.ShouldSerializeValue (test), "#A3");
			Assert.IsFalse (prop4PD.ShouldSerializeValue (test), "#A4");
			Assert.IsFalse (prop5PD.ShouldSerializeValue (test), "#A5");
			Assert.IsFalse (prop6PD.ShouldSerializeValue (test), "#A6");
			Assert.IsFalse (prop7PD.ShouldSerializeValue (test), "#A7");

			test.Prop1 = "whatever";
			Assert.IsFalse (prop1PD.ShouldSerializeValue (test), "#B1");
			test.Prop2 = "whatever";
			Assert.IsTrue (prop2PD.ShouldSerializeValue (test), "#B2");
			test.Prop3 = "whatever";
			Assert.IsFalse (prop3PD.ShouldSerializeValue (test), "#B3");
			test.Prop4 = "whatever";
			Assert.IsFalse (prop4PD.ShouldSerializeValue (test), "#B4");
			test.Prop7 = "whatever";
			Assert.IsFalse (prop7PD.ShouldSerializeValue (test), "#B5");

			test.Prop1 = "ok";
			Assert.IsFalse (prop1PD.ShouldSerializeValue (test), "#C1");
			test.SerializeProp3 = true;
			Assert.IsTrue (prop3PD.ShouldSerializeValue (test), "#C2");
			test.SerializeProp4 = true;
			Assert.IsTrue (prop4PD.ShouldSerializeValue (test), "#C3");
			test.SerializeProp5 = true;
			Assert.IsTrue (prop5PD.ShouldSerializeValue (test), "#C4");
			test.SerializeProp6 = true;
			Assert.IsTrue (prop6PD.ShouldSerializeValue (test), "#C5");
			test.Prop7 = "good";
			Assert.IsFalse (prop7PD.ShouldSerializeValue (test), "#C6");
			test.SerializeProp7 = true;
			Assert.IsTrue (prop7PD.ShouldSerializeValue (test), "#C7");
			test.Prop7 = "good";
			Assert.IsTrue (prop7PD.ShouldSerializeValue (test), "#C8");
		}

		[Test]
		public void CanResetTest_public ()
		{
			PropertyDescriptor p = TypeDescriptor.GetProperties (typeof (CanReset_public_test))["Prop"];
			CanReset_public_test test = new CanReset_public_test ();

			Assert.IsTrue (p.CanResetValue (test), "1");
			Assert.AreEqual (5, test.Prop, "2");
			p.ResetValue (test);
			Assert.AreEqual (10, test.Prop, "3");
		}

		[Test]
		public void CanResetTest_protected ()
		{
			PropertyDescriptor p = TypeDescriptor.GetProperties (typeof (CanReset_protected_test))["Prop"];
			CanReset_protected_test test = new CanReset_protected_test ();

			Assert.IsTrue (p.CanResetValue (test), "1");
			Assert.AreEqual (5, test.Prop, "2");
			p.ResetValue (test);
			Assert.AreEqual (10, test.Prop, "3");
		}

		[Test]
		public void CanResetTest_private ()
		{
			PropertyDescriptor p = TypeDescriptor.GetProperties (typeof (CanReset_private_test))["Prop"];
			CanReset_private_test test = new CanReset_private_test ();

			Assert.IsTrue (p.CanResetValue (test), "1");
			Assert.AreEqual (5, test.Prop, "2");
			p.ResetValue (test);
			Assert.AreEqual (10, test.Prop, "3");
		}

		[Test]
		public void CanResetTestNoSetterTest ()
		{
			PropertyDescriptor p = TypeDescriptor.GetProperties (typeof (CanResetNoSetter_test))["Prop"];
			CanResetNoSetter_test test = new CanResetNoSetter_test ();

#if NET_2_0
			Assert.IsFalse (p.CanResetValue (test), "1");
#else
			Assert.IsTrue (p.CanResetValue (test), "1");
#endif
			Assert.AreEqual (5, test.Prop, "2");
			p.ResetValue (test);
			Assert.AreEqual (10, test.Prop, "3");
		}

		[Test]
		public void NoSerializeOrResetPropTest ()
		{
			PropertyDescriptor p = TypeDescriptor.GetProperties (typeof (NoSerializeOrResetProp_test))["Prop"];
			NoSerializeOrResetProp_test test = new NoSerializeOrResetProp_test ();

			Assert.IsFalse (p.CanResetValue (test), "1");
			Assert.IsFalse (p.ShouldSerializeValue (test), "2");
		}

		[Test]
		public void ShouldSerializeFalseEffectOnCanResetTest ()
		{
			PropertyDescriptor p = TypeDescriptor.GetProperties (typeof (ShouldSerializeFalseEffectOnCanReset_test))["Prop"];
			ShouldSerializeFalseEffectOnCanReset_test test = new ShouldSerializeFalseEffectOnCanReset_test ();

			Assert.IsFalse (p.ShouldSerializeValue (test), "1");
			Assert.IsFalse (p.CanResetValue (test), "2");
		}

		[Test]
		public void ReadOnlyPropertyTest ()
		{
			PropertyDescriptorCollection col = TypeDescriptor.GetProperties (typeof (ReadOnlyProperty_test));
			Assert.IsTrue (col["Prop"].IsReadOnly, "1");
		}

		[Test]
		public void ReadOnlyAttributeTest ()
		{
			PropertyDescriptorCollection col = TypeDescriptor.GetProperties (typeof (ReadOnlyAttribute_test));
			Assert.IsTrue (col["Prop"].IsReadOnly, "1");
		}

		[Test]
		public void ReadOnlyConflictingTest ()
		{
			PropertyDescriptorCollection col = TypeDescriptor.GetProperties (typeof (ConflictingReadOnly_test));
			Assert.IsTrue (col["Prop"].IsReadOnly, "1");
		}

		[Test] // bug #80292
		public void DisplayNameTest ()
		{
			PropertyDescriptor p1 = TypeDescriptor.GetProperties (typeof (DisplayName_test)) ["Explicit"];
			PropertyDescriptor p2 = TypeDescriptor.GetProperties (typeof (DisplayName_test)) ["Implicit"];

#if NET_2_0
			Assert.AreEqual ("An explicit displayname", p1.DisplayName, "#1");
#else
			Assert.AreEqual ("Explicit", p1.DisplayName, "#1");
#endif
			Assert.AreEqual ("Implicit", p2.DisplayName, "#2");
		}

		[Test]
		public void GetEditorTest ()
		{
			PropertyDescriptorCollection col;
			PropertyDescriptor pd;
			UITypeEditor ed;

			col = TypeDescriptor.GetProperties (typeof (GetEditor_test));
			pd = col [0];
			ed = pd.GetEditor (typeof (UITypeEditor)) as UITypeEditor;

			Assert.IsNotNull (ed, "#01");
			Assert.AreEqual (ed.GetType ().Name, "UIEditor", "#02");
		
		}

		class GetEditor_test 
		{
			[Editor (typeof (UIEditor), typeof (UITypeEditor))]
			public string Property {
				get { return "abc"; }
				set { }
			}
		}

		class UIEditor : UITypeEditor
		{
			
		}
	}
}
