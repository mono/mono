//
// System.ComponentModel.PropertyDescriptor test cases
//
// Authors:
//	Chris Toshok (toshok@ximian.com)
//	Gert Driesen (drieseng@users.sourceforge.net)
//
// (c) 2006 Novell, Inc. (http://www.novell.com/)
//

using System;
using System.Collections;
using System.ComponentModel;
using DescriptionAttribute = System.ComponentModel.DescriptionAttribute;
using System.ComponentModel.Design;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
#if !MOBILE
using System.Drawing.Design;
#endif
using NUnit.Framework;

namespace MonoTests.System.ComponentModel
{
	internal class MyVersionTypeConverter : TypeConverter
	{
	}

	class VirtPropParent
	{
		string _someProperty;

		public virtual string SomeProperty {
			get { return _someProperty; }
			set { _someProperty = value; }
		}
	}

	class VirtPropChildNoSetter : VirtPropParent
	{
		public override string SomeProperty {
			get { return base.SomeProperty + ": modified"; }
		}
	}

	class VirtPropChildNoGetter : VirtPropParent
	{
		public override string SomeProperty {
			get { return base.SomeProperty + ": modified"; }
		}
	}


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

			[ReadOnly (true)]
			[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
			public string Prop8 {
				get { return null; }
			}

			[ReadOnly (true)]
			[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
			public string Prop9 {
				get { return null; }
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

			public bool ShouldSerializeProp8 ()
			{
				return false;
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

		class Converter_test
		{
			public virtual Version NoConverter {
				get { return null; }
			}

			[TypeConverter (typeof(MyVersionTypeConverter))]
			public virtual Version WithConverter {
				get { return null; }
			}

			[TypeConverter ("MonoTests.System.ComponentModel.MyVersionTypeConverter")]
			public virtual Version WithConverterNamed {
				get { return null; }
			}

			[TypeConverter("System.ComponentModel.CharConverter, " + Consts.AssemblySystem)]
			public virtual Version WithConverterNamedAssmQuald {
				get { return null; }
			}

			public int WithDefaultConverter {
				get { return 0; }
			}
		}
                
		class ConverterSubclassNotOverridenProperties_test : Converter_test
		{ 
		}
		
		class ConverterSubclassOverridenProperties_test : Converter_test
		{
			public override Version WithConverter {
				get { return null; }
			}

			public override Version WithConverterNamed {
				get { return null; }
			}
		}

		class ConverterEmptyConvertersOnOveriddenProperties : Converter_test
		{
			[TypeConverter]
			public override Version WithConverter {
				get { return null; }
			}

			[TypeConverter]
			public override Version WithConverterNamed {
				get { return null; }
			}
		}
		
		private ArrayList _invokedHandlers;

		[SetUp]
		public void SetUp ()
		{
			_invokedHandlers = new ArrayList ();
		}

		void Reset ()
		{
			_invokedHandlers.Clear ();
		}

		[Test]
		public void Attributes ()
		{
			PropertyDescriptorCollection properties;
			PropertyDescriptor pd;

			properties = TypeDescriptor.GetProperties (typeof (TestBase));

			pd = properties ["PropBase3"];
			Assert.IsNull (FindAttribute (pd, typeof (DescriptionAttribute)), "#A1");
			Assert.IsNotNull (FindAttribute (pd, typeof (PropTestAttribute)), "#A2");

			pd = properties ["PropBase2"];
			Assert.IsNotNull (FindAttribute (pd, typeof (DescriptionAttribute)), "#B1");
			Assert.IsNotNull (FindAttribute (pd, typeof (PropTestAttribute)), "#B2");

			pd = properties ["PropBase1"];
			Assert.IsNull (FindAttribute (pd, typeof (DescriptionAttribute)), "#C1");
			Assert.IsNotNull (FindAttribute (pd, typeof (PropTestAttribute)), "#C2");

			properties = TypeDescriptor.GetProperties (typeof (TestSub));

			pd = properties ["PropBase3"];
			Assert.IsNull (FindAttribute (pd, typeof (DescriptionAttribute)), "#D1");
			Assert.IsNotNull (FindAttribute (pd, typeof (PropTestAttribute)), "#D2");

			pd = properties ["PropBase2"];
			Assert.IsNotNull (FindAttribute (pd, typeof (DescriptionAttribute)), "#E1");
			Assert.IsNotNull (FindAttribute (pd, typeof (PropTestAttribute)), "#E2");

			pd = properties ["PropBase1"];
			Assert.IsNull (FindAttribute (pd, typeof (DescriptionAttribute)), "#F1");
			Assert.IsNotNull (FindAttribute (pd, typeof (PropTestAttribute)), "#F2");
		}

		[Test]
		public void VirtualPropertyDontOverrideSetter ()
		{
			VirtPropChildNoSetter c = new VirtPropChildNoSetter ();
			PropertyDescriptorCollection pdc = TypeDescriptor.GetProperties (c);
			foreach (PropertyDescriptor pd in pdc) {
				if (pd.Name != "SomeProperty")
					continue;
				pd.SetValue (c, "testing2");
				pd.GetValue (c);
			}
		}

		[Test]
		public void VirtualPropertyDontOverrideGetter ()
		{
			VirtPropChildNoGetter c = new VirtPropChildNoGetter ();
			PropertyDescriptorCollection pdc = TypeDescriptor.GetProperties (c);
			foreach (PropertyDescriptor pd in pdc) {
				if (pd.Name != "SomeProperty")
					continue;
				pd.SetValue (c, "testing2");
				pd.GetValue (c);
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
		public void ConverterTest ()
		{
			Assert.AreEqual (typeof (TypeConverter), 
					 TypeDescriptor.GetProperties (typeof (Converter_test))["NoConverter"].Converter.GetType (), "#1");
			Assert.AreEqual (typeof (MyVersionTypeConverter), 
					 TypeDescriptor.GetProperties (typeof (Converter_test))["WithConverter"].Converter.GetType (), "#2");
			Assert.AreEqual (typeof (MyVersionTypeConverter), 
					 TypeDescriptor.GetProperties (typeof (Converter_test))["WithConverterNamed"].Converter.GetType (), "#3");
			Assert.AreEqual (typeof (CharConverter), 
					 TypeDescriptor.GetProperties (typeof (Converter_test))["WithConverterNamedAssmQuald"].Converter.GetType (), "#4");
			Assert.AreEqual (typeof (Int32Converter), 
					 TypeDescriptor.GetProperties (typeof (Converter_test))["WithDefaultConverter"].Converter.GetType (), "#5");

			Assert.AreEqual (typeof (TypeConverter), 
					 TypeDescriptor.GetProperties (typeof (ConverterSubclassNotOverridenProperties_test))["NoConverter"].Converter.GetType (), "#6");
			Assert.AreEqual (typeof (MyVersionTypeConverter), 
					 TypeDescriptor.GetProperties (typeof (ConverterSubclassNotOverridenProperties_test))["WithConverter"].Converter.GetType (), "#7");
			Assert.AreEqual (typeof (MyVersionTypeConverter), 
					 TypeDescriptor.GetProperties (typeof (ConverterSubclassNotOverridenProperties_test))["WithConverterNamed"].Converter.GetType (), "#8");
			Assert.AreEqual (typeof (CharConverter), 
					 TypeDescriptor.GetProperties (typeof (ConverterSubclassNotOverridenProperties_test))["WithConverterNamedAssmQuald"].Converter.GetType (), "#9");
			Assert.AreEqual (typeof (Int32Converter), 
					 TypeDescriptor.GetProperties (typeof (ConverterSubclassNotOverridenProperties_test))["WithDefaultConverter"].Converter.GetType (), "#10");

			Assert.AreEqual (typeof (TypeConverter), 
					 TypeDescriptor.GetProperties (typeof (ConverterSubclassOverridenProperties_test))["NoConverter"].Converter.GetType (), "#11");
			Assert.AreEqual (typeof (MyVersionTypeConverter), 
					 TypeDescriptor.GetProperties (typeof (ConverterSubclassOverridenProperties_test))["WithConverter"].Converter.GetType (), "#12");
			Assert.AreEqual (typeof (MyVersionTypeConverter), 
					 TypeDescriptor.GetProperties (typeof (ConverterSubclassOverridenProperties_test))["WithConverterNamed"].Converter.GetType (), "#13");
			Assert.AreEqual (typeof (CharConverter), 
					 TypeDescriptor.GetProperties (typeof (ConverterSubclassOverridenProperties_test))["WithConverterNamedAssmQuald"].Converter.GetType (), "#14");
			Assert.AreEqual (typeof (Int32Converter), 
					 TypeDescriptor.GetProperties (typeof (ConverterSubclassOverridenProperties_test))["WithDefaultConverter"].Converter.GetType (), "#15");

			Assert.AreEqual (typeof (TypeConverter), 
					 TypeDescriptor.GetProperties (typeof (ConverterEmptyConvertersOnOveriddenProperties))["NoConverter"].Converter.GetType (), "#116");
			Assert.AreEqual (typeof (TypeConverter), 
					 TypeDescriptor.GetProperties (typeof (ConverterEmptyConvertersOnOveriddenProperties))["WithConverter"].Converter.GetType (), "#17");
			Assert.AreEqual (typeof (TypeConverter), 
					 TypeDescriptor.GetProperties (typeof (ConverterEmptyConvertersOnOveriddenProperties))["WithConverterNamed"].Converter.GetType (), "#18");
			Assert.AreEqual (typeof (CharConverter), 
					 TypeDescriptor.GetProperties (typeof (ConverterEmptyConvertersOnOveriddenProperties))["WithConverterNamedAssmQuald"].Converter.GetType (), "#19");
			Assert.AreEqual (typeof (Int32Converter), 
					 TypeDescriptor.GetProperties (typeof (ConverterEmptyConvertersOnOveriddenProperties))["WithDefaultConverter"].Converter.GetType (), "#20");
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
			PropertyDescriptor prop8PD = properties ["Prop8"];
			PropertyDescriptor prop9PD = properties ["Prop9"];

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

			// has both DesignerSerializationVisibility.Content and ShouldSerialize { return false }
			Assert.IsFalse (prop8PD.ShouldSerializeValue (test), "#D1");
			// has DesignerSerializationVisibility.Content, no ShouldSerialize
			Assert.IsTrue (prop9PD.ShouldSerializeValue (test), "#D2");
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
		public void AddValueChanged ()
		{
			MockPropertyDescriptor pd = new MockPropertyDescriptor (
				"Name", new Attribute [0]);
			object compA = new object ();
			object compB = new object ();
			EventHandler handlerA = new EventHandler (ValueChanged1);
			EventHandler handlerB = new EventHandler (ValueChanged1);
			EventHandler handlerC = new EventHandler (ValueChanged2);

			pd.AddValueChanged (compA, handlerA);
			pd.AddValueChanged (compA, handlerC);
			pd.AddValueChanged (compA, handlerC);
			pd.AddValueChanged (compA, handlerB);

			pd.FireValueChanged (compA, new EventArgs ());
			Assert.AreEqual (4, _invokedHandlers.Count, "#A1");
			Assert.AreEqual ("ValueChanged1", _invokedHandlers [0], "#A1");
			Assert.AreEqual ("ValueChanged2", _invokedHandlers [1], "#A2");
			Assert.AreEqual ("ValueChanged2", _invokedHandlers [2], "#A3");
			Assert.AreEqual ("ValueChanged1", _invokedHandlers [3], "#A4");

			Reset ();

			pd.FireValueChanged (compB, new EventArgs ());
			Assert.AreEqual (0, _invokedHandlers.Count, "#B");
		}

		[Test]
		public void AddValueChanged_Component_Null ()
		{
			MockPropertyDescriptor pd = new MockPropertyDescriptor (
				"Name", new Attribute [0]);
			try {
				pd.AddValueChanged (null, new EventHandler (ValueChanged1));
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("component", ex.ParamName, "#6");
			}
		}

		[Test]
		public void AddValueChanged_Handler_Null ()
		{
			MockPropertyDescriptor pd = new MockPropertyDescriptor (
				"Name", new Attribute [0]);
			try {
				pd.AddValueChanged (new object (), (EventHandler) null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("handler", ex.ParamName, "#6");
			}
		}

#if NET_2_0
		[Test]
		public void GetInvocationTarget_Instance_Null ()
		{
			MockPropertyDescriptor pd = new MockPropertyDescriptor (
				"Name", new Attribute [0]);
			try {
				pd.GetInvocationTarget (typeof (int), null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("instance", ex.ParamName, "#6");
			}
		}

		[Test]
		public void GetInvocationTarget_Type_Null ()
		{
			MockPropertyDescriptor pd = new MockPropertyDescriptor (
				"Name", new Attribute [0]);
			try {
				pd.GetInvocationTarget ((Type) null, new object ());
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("type", ex.ParamName, "#6");
			}
		}

		[Test]
		public void GetValueChangedHandler ()
		{
			object compA = new object ();
			object compB = new object ();
			EventHandler handlerA = new EventHandler (ValueChanged1);
			EventHandler handlerB = new EventHandler (ValueChanged1);
			EventHandler handlerC = new EventHandler (ValueChanged2);

			MockPropertyDescriptor pd = new MockPropertyDescriptor (
				"Name", new Attribute [0]);
			Assert.IsNull (pd.GetValueChangedHandler (null), "#A1");
			Assert.IsNull (pd.GetValueChangedHandler (compA), "#A2");
			Assert.IsNull (pd.GetValueChangedHandler (compB), "#A3");

			pd.AddValueChanged (compA, handlerA);

			Assert.IsNull (pd.GetValueChangedHandler (null), "#B1");
			Assert.AreSame (handlerA, pd.GetValueChangedHandler (compA), "#B2");
			Assert.IsNull (pd.GetValueChangedHandler (compB), "#B3");

			pd.AddValueChanged (compA, handlerB);

			Assert.IsNull (pd.GetValueChangedHandler (null), "#C1");
			EventHandler handler = pd.GetValueChangedHandler (compA);
			Assert.AreEqual (2, handler.GetInvocationList ().Length, "#C2");
			Assert.AreEqual (handlerA, handler.GetInvocationList () [0], "#C3");
			Assert.AreEqual (handlerB, handler.GetInvocationList () [1], "#C4");
			Assert.IsNull (pd.GetValueChangedHandler (compB), "#C5");

			pd.AddValueChanged (compB, handlerA);

			Assert.IsNull (pd.GetValueChangedHandler (null), "#D1");
			handler = pd.GetValueChangedHandler (compA);
			Assert.AreEqual (2, handler.GetInvocationList ().Length, "#D2");
			Assert.AreSame (handlerA, pd.GetValueChangedHandler (compB), "#D3");

			pd.RemoveValueChanged (compB, handlerB);

			Assert.IsNull (pd.GetValueChangedHandler (null), "#E1");
			handler = pd.GetValueChangedHandler (compA);
			Assert.AreEqual (2, handler.GetInvocationList ().Length, "#E2");
			Assert.IsNull (pd.GetValueChangedHandler (compB), "#E3");

			pd.RemoveValueChanged (compB, handlerB);

			Assert.IsNull (pd.GetValueChangedHandler (null), "#F1");
			handler = pd.GetValueChangedHandler (compA);
			Assert.AreEqual (2, handler.GetInvocationList ().Length, "#F2");
			Assert.IsNull (pd.GetValueChangedHandler (compB), "#F3");

			pd.RemoveValueChanged (compA, handlerC);

			Assert.IsNull (pd.GetValueChangedHandler (null), "#G1");
			handler = pd.GetValueChangedHandler (compA);
			Assert.AreEqual (2, handler.GetInvocationList ().Length, "#G2");
			Assert.IsNull (pd.GetValueChangedHandler (compB), "#G3");

			pd.AddValueChanged (compA, handlerC);

			Assert.IsNull (pd.GetValueChangedHandler (null), "#H1");
			handler = pd.GetValueChangedHandler (compA);
			Assert.AreEqual (3, handler.GetInvocationList ().Length, "#H2");
			Assert.IsNull (pd.GetValueChangedHandler (compB), "#H3");

			pd.RemoveValueChanged (compA, handlerB);

			Assert.IsNull (pd.GetValueChangedHandler (null), "#I1");
			handler = pd.GetValueChangedHandler (compA);
			Assert.AreEqual (2, handler.GetInvocationList ().Length, "#I2");
			Assert.AreEqual (handlerA, handler.GetInvocationList () [0], "#I3");
			Assert.AreEqual (handlerC, handler.GetInvocationList () [1], "#I4");
			Assert.IsNull (pd.GetValueChangedHandler (compB), "#I5");
		}
#endif

		[Test]
		public void RemoveValueChanged ()
		{
			MockPropertyDescriptor pd = new MockPropertyDescriptor (
				"Name", new Attribute [0]);
			object compA = new object ();
			object compB = new object ();
			EventHandler handlerA = new EventHandler (ValueChanged1);
			EventHandler handlerB = new EventHandler (ValueChanged1);
			EventHandler handlerC = new EventHandler (ValueChanged2);

			pd.AddValueChanged (compA, handlerA);
			pd.AddValueChanged (compA, handlerC);
			pd.AddValueChanged (compA, handlerC);
			pd.AddValueChanged (compA, handlerB);
			pd.AddValueChanged (compB, handlerC);

			pd.FireValueChanged (compA, new EventArgs ());
			Assert.AreEqual (4, _invokedHandlers.Count, "#A1");
			pd.RemoveValueChanged (new object (), handlerC);
			pd.FireValueChanged (compA, new EventArgs ());
			Assert.AreEqual (8, _invokedHandlers.Count, "#A2");

			Reset ();
			pd.RemoveValueChanged (compA, handlerC);

			pd.FireValueChanged (compA, new EventArgs ());
			Assert.AreEqual (3, _invokedHandlers.Count, "#B1");
			Assert.AreEqual ("ValueChanged1", _invokedHandlers [0], "#B2");
			Assert.AreEqual ("ValueChanged2", _invokedHandlers [1], "#B3");
			Assert.AreEqual ("ValueChanged1", _invokedHandlers [2], "#B4");

			Reset ();

			pd.FireValueChanged (compB, new EventArgs ());
			Assert.AreEqual (1, _invokedHandlers.Count, "#C1");
			Assert.AreEqual ("ValueChanged2", _invokedHandlers [0], "#C2");

			Reset ();
			pd.RemoveValueChanged (compB, handlerC);

			pd.FireValueChanged (compB, new EventArgs ());
			Assert.AreEqual (0, _invokedHandlers.Count, "#D");
		}

		[Test]
		public void RemoveValueChanged_Component_Null ()
		{
			MockPropertyDescriptor pd = new MockPropertyDescriptor (
				"Name", new Attribute [0]);
			try {
				pd.RemoveValueChanged (null, new EventHandler (ValueChanged1));
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("component", ex.ParamName, "#6");
			}
		}

		[Test]
		public void RemoveValueChanged_Handler_Null ()
		{
			MockPropertyDescriptor pd = new MockPropertyDescriptor (
				"Name", new Attribute [0]);
			try {
				pd.RemoveValueChanged (new object (), (EventHandler) null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("handler", ex.ParamName, "#6");
			}
		}

		void ValueChanged1 (object sender, EventArgs e)
		{
			_invokedHandlers.Add ("ValueChanged1");
		}

		void ValueChanged2 (object sender, EventArgs e)
		{
			_invokedHandlers.Add ("ValueChanged2");
		}

		static Attribute FindAttribute (PropertyDescriptor pd, Type type)
		{
			foreach (Attribute attr in pd.Attributes)
				if (attr.GetType () == type)
					return attr;
			return null;
		}
#if !MOBILE
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
#endif

		class MockPropertyDescriptor : PropertyDescriptor
		{
			public MockPropertyDescriptor (MemberDescriptor reference)
				: base (reference)
			{
			}

			public MockPropertyDescriptor (MemberDescriptor reference, Attribute [] attrs)
				: base (reference, attrs)
			{
			}

			public MockPropertyDescriptor (string name, Attribute [] attrs)
				: base (name, attrs)
			{
			}

			public override Type ComponentType {
				get { return typeof (int); }
			}

			public override bool IsReadOnly {
				get { return false; }
			}

			public override Type PropertyType{
				get { return typeof (DateTime); }
			}

			public override object GetValue (object component)
			{
				return null;
			}

			public override void SetValue (object component, object value)
			{
			}

			public override void ResetValue (object component)
			{
			}

			public override bool CanResetValue (object component)
			{
				return true;
			}

			public override bool ShouldSerializeValue (object component)
			{
				return true;
			}

			public void FireValueChanged (object component, EventArgs e)
			{
				base.OnValueChanged (component, e);
			}

#if NET_2_0
			public new object GetInvocationTarget (Type type, object instance)
			{
				return base.GetInvocationTarget (type, instance);
			}

			public new EventHandler GetValueChangedHandler (object component)
			{
				return base.GetValueChangedHandler (component);
			}
#endif
		}

		[AttributeUsage (AttributeTargets.Field | AttributeTargets.Property)]
		public class PropTestAttribute : Attribute
		{
			public PropTestAttribute ()
			{
			}
		}

		public class TestBase
		{
			[PropTest]
			public int PropBase1
			{
				get { return 0; }
				set { }
			}

			[PropTest]
			[Description ("whatever")]
			public string PropBase2
			{
				get { return ""; }
				set { }
			}

			[PropTest]
			public virtual string PropBase3
			{
				get { return ""; }
				set { }
			}
		}

		public class TestSub : TestBase
		{
			[PropTest]
			public int PropSub1
			{
				get { return 0; }
				set { }
			}

			[PropTest]
			public string PropSub2
			{
				get { return ""; }
				set { }
			}

			public override string PropBase3
			{
				get { return ""; }
				set { }
			}
		}
	}
}
