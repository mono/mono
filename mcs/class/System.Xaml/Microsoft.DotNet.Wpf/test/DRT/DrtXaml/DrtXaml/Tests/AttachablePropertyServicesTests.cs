namespace DrtXaml.Tests
{
    using System;
    using System.Xaml;
    using System.Text;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading;
    using System.Xml;
    using System.IO;
    using System.ComponentModel;
    using System.ComponentModel.Design.Serialization;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Xml.Linq;
    using DrtXaml.XamlTestFramework;
    using Test.Elements;
    using DRT;

    [TestClass]
    public class AttachablePropertyServicesTests : XamlTestSuite
    {
        public AttachablePropertyServicesTests()
            : base("AttachablePropertyServicesTests")
        {
        }

        public override DrtTest[] PrepareTests()
        {
            DrtTest[] tests = DrtTestFinder.FindTests(this);
            return tests;
        }

        static readonly AttachableMemberIdentifier propertyOne = new AttachableMemberIdentifier(typeof(AttachablePropertyServicesTests), "PropertyOne");
        static readonly AttachableMemberIdentifier propertyTwo = new AttachableMemberIdentifier(typeof(AttachablePropertyServicesTests), "PropertyTwo");

        static readonly AttachableMemberIdentifier barPropertyName = new AttachableMemberIdentifier(typeof(AttachablePropertyServicesTests), "Bar");
        static readonly AttachableMemberIdentifier complexPropertyName = new AttachableMemberIdentifier(typeof(AttachablePropertyServicesTests), "Complex");

        static readonly AttachableMemberIdentifier nonSerializedPropertyName = new AttachableMemberIdentifier(typeof(AttachablePropertyServicesTests), "NonSerialized");
        static readonly AttachableMemberIdentifier nonSerialized2PropertyName = new AttachableMemberIdentifier(typeof(AttachablePropertyServicesTests), "NonSerialized2");

        static readonly AttachableMemberIdentifier maybeSerializedPropertyName = new AttachableMemberIdentifier(typeof(AttachablePropertyServicesTests), "MaybeSerialized");
        static readonly AttachableMemberIdentifier valueWithDefaultName = new AttachableMemberIdentifier(typeof(AttachablePropertyServicesTests), "ValueWithDefault");

        public static int GetPropertyOne(object target)
        {
            int value;
            return AttachablePropertyServices.TryGetProperty(target, propertyOne, out value) ? value : 0;
        }

        public static void SetPropertyOne(object target, int value)
        {
            AttachablePropertyServices.SetProperty(target, propertyOne, value);
        }

        public static int GetPropertyTwo(object target)
        {
            int value;
            return AttachablePropertyServices.TryGetProperty(target, propertyTwo, out value) ? value : 0;
        }

        public static void SetPropertyTwo(object target, int value)
        {
            AttachablePropertyServices.SetProperty(target, propertyTwo, value);
        }

        public static int GetBar(object target)
        {
            int value;
            return AttachablePropertyServices.TryGetProperty(target, barPropertyName, out value) ? value : 0;
        }

        public static void SetBar(object target, int value)
        {
            AttachablePropertyServices.SetProperty(target, barPropertyName, value);
        }

        public static ComplexAttachedProperty GetComplex(object target)
        {
            ComplexAttachedProperty value;
            return AttachablePropertyServices.TryGetProperty(target, complexPropertyName, out value) ? value : null;
        }

        public static void SetComplex(object target, ComplexAttachedProperty value)
        {
            AttachablePropertyServices.SetProperty(target, complexPropertyName, value);
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public static int GetNonSerialized(object target)
        {
            int value;
            return AttachablePropertyServices.TryGetProperty(target, nonSerializedPropertyName, out value) ? value : 0;
        }

        public static void SetNonSerialized(object target, int value)
        {
            AttachablePropertyServices.SetProperty(target, nonSerializedPropertyName, value);
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public static string GetNonSerialized2(object target)
        {
            string value;
            return AttachablePropertyServices.TryGetProperty(target, nonSerialized2PropertyName, out value) ? value : String.Empty;
        }

        public static void SetNonSerialized2(object target, string value)
        {
            AttachablePropertyServices.SetProperty(target, nonSerialized2PropertyName, value);
        }

        public static string GetMaybeSerialized(object target)
        {
            string value;
            return AttachablePropertyServices.TryGetProperty(target, maybeSerializedPropertyName, out value) ? value : String.Empty;
        }

        public static void SetMaybeSerialized(object target, string value)
        {
            AttachablePropertyServices.SetProperty(target, maybeSerializedPropertyName, value);
        }

        public static bool ShouldSerializeMaybeSerialized(object target)
        {
            return target is FooWithBar && ((FooWithBar)target).Bar == 12;
        }

        [DefaultValue("Default string")]
        public static string GetValueWithDefault(object target)
        {
            string value;
            return AttachablePropertyServices.TryGetProperty(target, valueWithDefaultName, out value) ? value : String.Empty;
        }

        public static void SetValueWithDefault(object target, string value)
        {
            AttachablePropertyServices.SetProperty(target, valueWithDefaultName, value);
        }

        [TestMethod]
        public void BasicWithIAPSType()
        {
            ImplementsIAPS a = new ImplementsIAPS();

            object value;
            Assert.IsFalse(AttachablePropertyServices.TryGetProperty(a, propertyOne, out value), "We expect the store doesn't have propertyOne for 'a' yet");
            Assert.IsFalse(AttachablePropertyServices.TryGetProperty(a, propertyTwo, out value), "We expect the store doesn't have propertyTwo for 'a' yet");

            IAttachedPropertyStore aAsIAPS = a as IAttachedPropertyStore;
            Assert.IsNotNull(aAsIAPS);
            Assert.IsFalse(aAsIAPS.TryGetProperty(propertyOne, out value), "We expect the type does not have propertyOne yet");
            Assert.IsFalse(aAsIAPS.TryGetProperty(propertyTwo, out value), "We expect the type does not have propertyTwo yet");

            AttachablePropertyServices.SetProperty(a, propertyOne, "Foo Bar");
            Assert.IsTrue(AttachablePropertyServices.GetAttachedPropertyCount(a) == 1);
            var props = new KeyValuePair<AttachableMemberIdentifier, object>[1];
            AttachablePropertyServices.CopyPropertiesTo(a, props, 0);
            Assert.IsTrue((string)props[0].Value == "Foo Bar");
            Assert.IsTrue(AttachablePropertyServices.TryGetProperty(a, propertyOne, out value) && (string)value == "Foo Bar", "We should now find the property");
            Assert.IsTrue(aAsIAPS.TryGetProperty(propertyOne, out value) && (string)value == "Foo Bar", "And it's storage should be local on the instance");

            string valueAsString;
            Assert.IsTrue(AttachablePropertyServices.TryGetProperty(a, propertyOne, out valueAsString) && valueAsString == "Foo Bar");

            Assert.IsTrue(AttachablePropertyServices.RemoveProperty(a, propertyOne), "We should be able to remove the property");
            Assert.IsFalse(AttachablePropertyServices.TryGetProperty(a, propertyOne, out value), "We should not have the property anymore");
            Assert.IsFalse(aAsIAPS.TryGetProperty(propertyOne, out value), "Even the local copy should not have the property anymore");
            Assert.IsTrue(AttachablePropertyServices.GetAttachedPropertyCount(a) == 0);
        }

        [TestMethod]
        public void BasicWithNonIAPSType()
        {
            DoesntImplementIAPS a = new DoesntImplementIAPS();
            DoesntImplementIAPS b = new DoesntImplementIAPS();

            object value;
            Assert.IsFalse(AttachablePropertyServices.TryGetProperty(a, propertyOne, out value), "We expect the store doesn't have propertyOne for 'a' yet");
            Assert.IsFalse(AttachablePropertyServices.TryGetProperty(a, propertyTwo, out value), "We expect the store doesn't have propertyTwo for 'a' yet");
            Assert.IsFalse(AttachablePropertyServices.TryGetProperty(b, propertyOne, out value), "We expect the store doesn't have propertyOne for 'b' yet");
            Assert.IsFalse(AttachablePropertyServices.TryGetProperty(b, propertyTwo, out value), "We expect the store doesn't have propertyTwo for 'b' yet");

            AttachablePropertyServices.SetProperty(a, propertyOne, "Something");
            Assert.IsTrue(AttachablePropertyServices.GetAttachedPropertyCount(a) == 1);
            var props = new KeyValuePair<AttachableMemberIdentifier, object>[1];
            AttachablePropertyServices.CopyPropertiesTo(a, props, 0);
            Assert.IsTrue((string)props[0].Value == "Something");
            Assert.IsTrue(AttachablePropertyServices.GetAttachedPropertyCount(b) == 0);
            Assert.IsTrue(AttachablePropertyServices.TryGetProperty(a, propertyOne, out value) && (string)value == "Something", "We should now find the property");
            Assert.IsFalse(AttachablePropertyServices.TryGetProperty(b, propertyOne, out value), "We should not see the attached property added to 'a' on 'b'");

            string valueAsString;
            Assert.IsTrue(AttachablePropertyServices.TryGetProperty(a, propertyOne, out valueAsString) && valueAsString == "Something");

            Assert.IsTrue(AttachablePropertyServices.RemoveProperty(a, propertyOne), "We should be able to remove the property");
            Assert.IsFalse(AttachablePropertyServices.TryGetProperty(a, propertyOne, out value), "We should not have the property anymore");
            Assert.IsTrue(AttachablePropertyServices.GetAttachedPropertyCount(a) == 0);
        }

        [TestMethod, TestExpectedException(typeof(InvalidOperationException))]
        public void SerializeStringPropertyWithAttachedProperties()
        {
            var foo = new Simple { Prop = "asdf" };
            SetBar(foo.Prop, 98);
            XamlServices.Save(foo);
        }

        [TestMethod, TestExpectedException(typeof(InvalidOperationException))]
        public void SerializeStringConvertiblePropertyWithAttachedProperties()
        {
            var foo = new ClassWithTypeConverterContainer { Prop = new ClassWithTypeConverter(2) };
            SetBar(foo.Prop, 98);
            XamlServices.Save(foo);
        }

        [TestMethod, TestExpectedException(typeof(InvalidOperationException))]
        public void SerializeDictionaryKeyWithAttachedProperties()
        {
            var foo = new Dictionary<string, string>();
            string a = "troll";
            foo.Add(a, "pointless");
            SetBar(a, 98);
            XamlServices.Save(foo);
        }

        [TestMethod]
        [TestKnownFailure(Reason = "")]
        public void SerializeWithAttachedPropertyOnInstanceOfOwner()
        {
            AttachablePropertyServicesTests aps = new AttachablePropertyServicesTests();
            SetBar(aps, 198);

            Assert.IsTrue(GetBar(aps) == 198);
            Assert.IsTrue(AttachablePropertyServices.GetAttachedPropertyCount(aps) == 1);

            var expected = @"<AttachablePropertyServicesTests DRT=""{x:Null}"" Bar=""198"" xmlns=""clr-namespace:DrtXaml.Tests;assembly=DrtXaml"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"" />";
            AttachablePropertyServicesTests result = (AttachablePropertyServicesTests)RoundTrip(aps, expected);

            Assert.IsTrue(GetBar(result) == 198);
        }

        [TestMethod]
        public void SerializeWithAPSProperty()
        {
            FooWithBar f = new FooWithBar() { Bar = 12 };
            SetBar(f, 17);

            Assert.IsTrue(GetBar(f) == 17);
            Assert.IsTrue(AttachablePropertyServices.GetAttachedPropertyCount(f) == 1);

            var expected = @"<FooWithBar AttachablePropertyServicesTests.Bar=""17"" Bar=""12"" xmlns=""clr-namespace:DrtXaml.Tests;assembly=DrtXaml"" />";
            FooWithBar result = (FooWithBar)RoundTrip(f, expected);

            Assert.IsTrue(result.Bar == 12);
            Assert.IsTrue(GetBar(result) == 17);
        }

        [TestMethod]
        public void SerializeWithHiddenAPSProperties()
        {
            FooWithBar f = new FooWithBar() { Bar = 12 };
            SetNonSerialized(f, 17);
            SetNonSerialized2(f, "Something");

            Assert.IsTrue(GetNonSerialized(f) == 17);
            Assert.IsTrue(GetNonSerialized2(f) == "Something");
            Assert.IsTrue(AttachablePropertyServices.GetAttachedPropertyCount(f) == 2);

            var expected = @"<FooWithBar Bar=""12"" xmlns=""clr-namespace:DrtXaml.Tests;assembly=DrtXaml"" />";
            FooWithBar result = (FooWithBar)RoundTrip(f, expected);

            Assert.IsTrue(result.Bar == 12);
            Assert.IsTrue(GetNonSerialized(result) == 0);
            Assert.IsTrue(GetNonSerialized2(result) == String.Empty);
        }

        [TestMethod]
        public void SerializeWithAPSComplexProperty()
        {
            var ap = new ComplexAttachedProperty() { Bar = "something", Foo = 65 };
            FooWithBar f = new FooWithBar() { Bar = 12, };
            SetComplex(f, ap);

            Assert.IsTrue(GetComplex(f).Equals(ap));
            Assert.IsTrue(AttachablePropertyServices.GetAttachedPropertyCount(f) == 1);

            var expected = @"<FooWithBar Bar=""12"" xmlns=""clr-namespace:DrtXaml.Tests;assembly=DrtXaml"">
  <AttachablePropertyServicesTests.Complex>
    <ComplexAttachedProperty Bar=""something"" Foo=""65"" />
  </AttachablePropertyServicesTests.Complex>
</FooWithBar>";
            FooWithBar result = (FooWithBar)RoundTrip(f, expected);

            Assert.IsTrue(result.Bar == 12);
            Assert.IsTrue(GetComplex(result).Equals(ap));
        }

        [TestMethod]
        public void SerializeStringWithAttachedProperty()
        {
            string s = "Something";
            SetBar(s, 98);

            Assert.IsTrue(GetBar(s) == 98);
            Assert.IsTrue(AttachablePropertyServices.GetAttachedPropertyCount(s) == 1);

            var expected = @"<x:String AttachablePropertyServicesTests.Bar=""98"" xmlns=""clr-namespace:DrtXaml.Tests;assembly=DrtXaml"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">Something</x:String>";

            string generated = XamlServices.Save(s);
            Assert.AreEqual(expected, generated);

            //TODO, 549050
            //string result = (string)RoundTrip(s, expected);

            //Assert.IsTrue(result != null);
            //Assert.IsTrue(GetBar(result) == 98);
        }

        [TestMethod]
        public void SerializeStringWithComplexAttachedProperty()
        {
            var ap = new ComplexAttachedProperty() { Bar = "something", Foo = 124 };
            string s = "SomeComplexThing";
            SetComplex(s, ap);

            Assert.IsTrue(GetComplex(s).Equals(ap));
            Assert.IsTrue(AttachablePropertyServices.GetAttachedPropertyCount(s) == 1);

            var expected = @"<x:String xmlns=""clr-namespace:DrtXaml.Tests;assembly=DrtXaml"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">SomeComplexThing<AttachablePropertyServicesTests.Complex><ComplexAttachedProperty Bar=""something"" Foo=""124"" /></AttachablePropertyServicesTests.Complex></x:String>";

            string generated = XamlServices.Save(s);
            Assert.AreEqual(expected, generated);

            //TODO, 549050
            //string result = (string)RoundTrip(s, expected);

            //Assert.IsTrue(result != null);
            //Assert.IsTrue(GetComplex(result).Equals(ap));
        }

        [TestMethod]
        public void SerializeArrayWithAttachedProperty()
        {
            string[] strings = new string[3];
            strings[0] = "all";
            strings[1] = "strings";
            strings[2] = "rule!";

            SetBar(strings, 1023);

            Assert.IsTrue(GetBar(strings) == 1023);
            Assert.IsTrue(AttachablePropertyServices.GetAttachedPropertyCount(strings) == 1);

            var expected =
@"<x:Array AttachablePropertyServicesTests.Bar=""1023"" Type=""x:String"" xmlns=""clr-namespace:DrtXaml.Tests;assembly=DrtXaml"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
  <x:String>all</x:String>
  <x:String>strings</x:String>
  <x:String>rule!</x:String>
</x:Array>";

            string generated = XamlServices.Save(strings);
            Assert.AreEqual(expected, generated);

            //TODO, 549050
            //string[] result = (string[])RoundTrip(strings, expected);

            //Assert.IsTrue(GetBar(result) == GetBar(strings));
        }

        [TestMethod]
        public void SerializeArrayWithComplexAttachedProperty()
        {
            var ap = new ComplexAttachedProperty() { Bar = "AP", Foo = 34 };

            string[] strings = new string[3];
            strings[0] = "all";
            strings[1] = "strings";
            strings[2] = "rule!";

            SetComplex(strings, ap);

            Assert.IsTrue(GetComplex(strings).Equals(ap));
            Assert.IsTrue(AttachablePropertyServices.GetAttachedPropertyCount(strings) == 1);

            var expected = @"<x:Array Type=""x:String"" xmlns=""clr-namespace:DrtXaml.Tests;assembly=DrtXaml"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
  <AttachablePropertyServicesTests.Complex>
    <ComplexAttachedProperty Bar=""AP"" Foo=""34"" />
  </AttachablePropertyServicesTests.Complex>
  <x:String>all</x:String>
  <x:String>strings</x:String>
  <x:String>rule!</x:String>
</x:Array>";
            string generated = XamlServices.Save(strings);
            Assert.AreEqual(expected, generated);

            //TODO, 549050
            //string[] result = (string[])RoundTrip(strings, expected);

            //Assert.IsTrue(GetComplex(result).Equals(ap));
        }

        [TestMethod]
        public void SerializeCollectionWithAttachedProperty()
        {
            List<string> strings = new List<string>();
            strings.Add("Things");
            strings.Add("that");
            strings.Add("make");
            strings.Add("me go hmm...");

            SetBar(strings, 564);

            Assert.IsTrue(GetBar(strings) == 564);
            Assert.IsTrue(AttachablePropertyServices.GetAttachedPropertyCount(strings) == 1);

            var assemblyName = typeof(List<string>).GetAssemblyName();
            var expected = @"<List x:TypeArguments=""x:String"" dt:AttachablePropertyServicesTests.Bar=""564"" Capacity=""4"" xmlns=""clr-namespace:System.Collections.Generic;assembly={0}"" xmlns:dt=""clr-namespace:DrtXaml.Tests;assembly=DrtXaml"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
  <x:String>Things</x:String>
  <x:String>that</x:String>
  <x:String>make</x:String>
  <x:String>me go hmm...</x:String>
</List>";
            expected = string.Format(expected, assemblyName);

            List<string> result = (List<string>)RoundTrip(strings, expected);

            Assert.IsTrue(GetBar(result) == GetBar(strings));
        }

        [TestMethod]
        public void SerializeCollectionWithComplexAttachedProperty()
        {
            var ap = new ComplexAttachedProperty() { Bar = "AP", Foo = 52 };

            List<string> strings = new List<string>();
            strings.Add("Things");
            strings.Add("that");
            strings.Add("make");
            strings.Add("me go hmm...");

            SetComplex(strings, ap);

            Assert.IsTrue(GetComplex(strings).Equals(ap));
            Assert.IsTrue(AttachablePropertyServices.GetAttachedPropertyCount(strings) == 1);

            Type type = typeof(List<string>);
            var assemblyName = type.GetAssemblyName();

            var expected =
@"<List x:TypeArguments=""x:String"" Capacity=""4"" xmlns=""clr-namespace:System.Collections.Generic;assembly={0}"" xmlns:dt=""clr-namespace:DrtXaml.Tests;assembly=DrtXaml"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
  <dt:AttachablePropertyServicesTests.Complex>
    <dt:ComplexAttachedProperty Bar=""AP"" Foo=""52"" />
  </dt:AttachablePropertyServicesTests.Complex>
  <x:String>Things</x:String>
  <x:String>that</x:String>
  <x:String>make</x:String>
  <x:String>me go hmm...</x:String>
</List>";

            expected = string.Format(expected, assemblyName);

            List<string> result = (List<string>)RoundTrip(strings, expected);

            Assert.IsTrue(GetComplex(result).Equals(ap));
        }

        [TestMethod]
        public void SerializeDictionaryWithAttachedProperty()
        {
            Dictionary<string, int> priorities = new Dictionary<string, int>();
            priorities.Add("red", 10);

            SetBar(priorities, 21);

            Assert.IsTrue(GetBar(priorities) == 21);
            Assert.IsTrue(AttachablePropertyServices.GetAttachedPropertyCount(priorities) == 1);

            var assemblyName = typeof(Dictionary<string, int>).GetAssemblyName();
            var expected =
@"<Dictionary x:TypeArguments=""x:String, x:Int32"" dt:AttachablePropertyServicesTests.Bar=""21"" xmlns=""clr-namespace:System.Collections.Generic;assembly={0}"" xmlns:dt=""clr-namespace:DrtXaml.Tests;assembly=DrtXaml"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
  <x:Int32 x:Key=""red"">10</x:Int32>
</Dictionary>";
            expected = string.Format(expected, assemblyName);

            Dictionary<string, int> result = (Dictionary<string, int>)RoundTrip(priorities, expected);

            Assert.IsTrue(GetBar(result) == GetBar(priorities));
        }

        [TestMethod]
        public void SerializeDictionaryWithComplexAttachedProperty()
        {
            var ap = new ComplexAttachedProperty() { Bar = "WE", Foo = 890 };

            Dictionary<string, int> priorities = new Dictionary<string, int>();
            priorities.Add("red", 10);
            
            SetComplex(priorities, ap);

            Assert.IsTrue(GetComplex(priorities).Equals(ap));
            Assert.IsTrue(AttachablePropertyServices.GetAttachedPropertyCount(priorities) == 1);

            var expected =
@"<Dictionary x:TypeArguments=""x:String, x:Int32"" xmlns=""clr-namespace:System.Collections.Generic;assembly={0}"" xmlns:dt=""clr-namespace:DrtXaml.Tests;assembly=DrtXaml"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
  <dt:AttachablePropertyServicesTests.Complex>
    <dt:ComplexAttachedProperty Bar=""WE"" Foo=""890"" />
  </dt:AttachablePropertyServicesTests.Complex>
  <x:Int32 x:Key=""red"">10</x:Int32>
</Dictionary>";
            expected = string.Format(expected, typeof(Dictionary<string, int>).GetAssemblyName());

            Dictionary<string, int> result = (Dictionary<string, int>)RoundTrip(priorities, expected);

            Assert.IsTrue(GetComplex(result).Equals(ap));
        }

        [TestMethod]
        public void SerializeWithShouldSerializeMethod()
        {
            FooWithBar whatever = new FooWithBar { Bar = 12 };
            FooWithBar another = new FooWithBar { Bar = 13 };
            object list = new List<string>();

            SetMaybeSerialized(whatever, "Something1");
            SetMaybeSerialized(list, "Another thing1");
            SetMaybeSerialized(another, "Nagging1");

            object[] data = new object[] { whatever, another, list };

            var xaml =
@"<x:Array Type=""x:Object"" xmlns=""clr-namespace:DrtXaml.Tests;assembly=DrtXaml"" xmlns:scg=""clr-namespace:System.Collections.Generic;assembly={0}"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
  <FooWithBar Bar=""12"" AttachablePropertyServicesTests.MaybeSerialized=""Something1"" />
  <FooWithBar Bar=""13"" />
  <scg:List x:TypeArguments=""x:String"" Capacity=""0"" />
</x:Array>";
            xaml = string.Format(xaml, typeof(object[]).GetAssemblyName());

            string generated = XamlServices.Save(data);
            Assert.AreEqual(xaml, generated);

            //TODO, 549050
            //var result = RoundTrip(data, xaml);
            //Assert.IsNotNull(result);
        }

        [TestMethod]
        public void SerializeWithDefaultValue()
        {
            FooWithBar whatever = new FooWithBar { Bar = 12 };
            FooWithBar another = new FooWithBar { Bar = 13 };
            object list = new List<string>();

            SetValueWithDefault(whatever, "Something2");
            SetValueWithDefault(list, "Default string");
            SetValueWithDefault(another, "Nagging2");

            object[] data = new object[] { whatever, another, list };

            var xaml =
@"<x:Array Type=""x:Object"" xmlns=""clr-namespace:DrtXaml.Tests;assembly=DrtXaml"" xmlns:scg=""clr-namespace:System.Collections.Generic;assembly={0}"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
  <FooWithBar Bar=""12"" AttachablePropertyServicesTests.ValueWithDefault=""Something2"" />
  <FooWithBar Bar=""13"" AttachablePropertyServicesTests.ValueWithDefault=""Nagging2"" />
  <scg:List x:TypeArguments=""x:String"" Capacity=""0"" />
</x:Array>";
            xaml = string.Format(xaml, typeof(object[]).GetAssemblyName());

            string generated = XamlServices.Save(data);
            Assert.AreEqual(xaml, generated);

            //TODO, 549050
            //var result = RoundTrip(data, xaml);
            //Assert.IsNotNull(result);
        }

        object RoundTrip(object value, string expectedXaml)
        {
            StringWriter sw = new StringWriter();
            using (var xmlwriter = XmlWriter.Create(sw, new XmlWriterSettings() { Indent = true, OmitXmlDeclaration = true }))
            {
                XamlServices.Save(xmlwriter, value);
            }
            string actualXaml = sw.ToString();
            if (!String.Equals(expectedXaml, actualXaml))
            {
                char [] delimiters = new char[] {' '};
                string[] expected = expectedXaml.Split(delimiters);
                string[] actual = actualXaml.Split(delimiters);
                Array.Sort(expected);
                   Array.Sort(actual);
                if (!(expected.SequenceEqual(actual)))
                {
                    Assert.Fail("The result of serializing should match the expected Xaml");
                }
            }
            return XamlServices.Load(XmlReader.Create(new StringReader(actualXaml)));
        }

        object[] CreateArray(int size)
        {
            object[] array = new object[size];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = new object();
            }
            return array;
        }

        WeakReference[] CreateWeakReferences(object[] array)
        {
            WeakReference[] wr = new WeakReference[array.Length];
            for (int i = 0; i < array.Length; i++)
            {
                wr[i] = new WeakReference(array[i]);
            }
            return wr;
        }

        [TestMethod]
        public void WeakDictionaryTests()
        {
            const int numKeys = 10;

            object[] keys = CreateArray(numKeys);
            object[] values = CreateArray(numKeys);
            WeakReference[] wrKeys = CreateWeakReferences(keys);
            WeakReference[] wrValues = CreateWeakReferences(values);

            Assembly asm = typeof(AttachablePropertyServices).Assembly;
            string dictionaryName = "System.Xaml.AttachablePropertyServices+DefaultAttachedPropertyStore+WeakDictionary`2";

            object wd = null;
            try
            {
                wd = GetInstance(asm,
                    dictionaryName,
                    null,
                    new Type[] { typeof(object), typeof(object) });
            }
            catch
            {
                //
                // WeakDictionary is only in the codebase for 3.5 builds
                // If we can't find the type there is no need to continue with the test.
                return;
            }

            var add = (Action<object, object>)GetInstanceMethod<Action<object, object>>(wd, "Add");
            var tryGet = (TryGet)GetInstanceMethod<TryGet>(wd, "TryGetValue");
            var get_Count = (Func<int>)GetInstancePropertyGetter<Func<int>>(wd, "Count");
            var get_SyncObject = (Func<object>)GetInstancePropertyGetter<Func<object>>(wd, "SyncObject");

            lock (get_SyncObject())
            {
                for (int i = 0; i < keys.Length; i++)
                {
                    add(keys[i], values[i]);
                }

                Assert.IsTrue(get_Count() == numKeys, "There should be the expected amount of keys");

                for (int i = 0; i < keys.Length; i++)
                {
                    object key = wrKeys[i].Target;
                    if (key == null)
                    {
                        continue;
                    }

                    object value;
                    Assert.IsTrue(tryGet(key, out value) && value == values[i], "There should be an entry for " + i + " and it's value should be the expected value");
                }

                // null out a few of the keys... to verify that the dictionary drops them.

                keys[0] = null;
                keys[1] = null;
                keys[2] = null;

                // null out all the values... to verify that the dictionary holds onto them.

                for (int i = 0; i < values.Length; i++)
                {
                    values[i] = null;
                }
            }

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            GC.WaitForPendingFinalizers();

            Thread.Sleep(2000);

            lock (get_SyncObject())
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();

                Assert.IsTrue(get_Count() == numKeys - 3, "There should be the expected amount of keys");

                for (int i = 0; i < keys.Length; i++)
                {
                    object key = wrKeys[i].Target;
                    if (key == null)
                    {
                        // If the key is gone then 
                        Assert.IsFalse(wrValues[i].IsAlive, "If the key is gone then the value should also be gone");
                        continue;
                    }

                    object value;
                    Assert.IsTrue(wrValues[i].IsAlive, "If the key is still around we expect that the value should still be around");
                    Assert.IsTrue(tryGet(key, out value) && value == wrValues[i].Target, "There should be an entry for " + i + " and it's value should be the expected value");
                }
            }            
        }

        public static object GetInstance(Assembly assembly, string typeName, object[] constructorArgs, Type[] genericArgs)
        {
            Type type = assembly.GetType(typeName);
            if (genericArgs.Length > 0)
            {
                type = type.MakeGenericType(genericArgs);
            }
            return Activator.CreateInstance(type, constructorArgs);
        }

        public static Delegate GetInstanceMethod<T>(object instance, string methodName, params Type[] genericArgs)
        {
            MethodInfo method =
                (from m in instance.GetType().GetMethods(BindingFlags.Instance |
                     BindingFlags.NonPublic |
                     BindingFlags.Public |
                     BindingFlags.DeclaredOnly)
                 where
                     m.Name == methodName &&
                     m.GetGenericArguments().Length == genericArgs.Length
                 select m).First();

            if (genericArgs.Length > 0)
            {
                method = method.MakeGenericMethod(genericArgs);
            }

            return Delegate.CreateDelegate(typeof(T), instance, method);
        }

        public static Delegate GetInstancePropertyGetter<T>(object instance, string propertyName)
        {
            PropertyInfo property =
                (from p in instance.GetType().GetProperties(BindingFlags.Instance |
                    BindingFlags.NonPublic |
                    BindingFlags.Public |
                    BindingFlags.DeclaredOnly)
                 where p.Name == propertyName
                 select p).First();

            return Delegate.CreateDelegate(typeof(T), instance, property.GetGetMethod());
        }

        // Have to declare this explicitly b/c it has an out param...
        delegate bool TryGet(object key, out object value);

        class DoesntImplementIAPS
        {
        }

        class ImplementsIAPS : IAttachedPropertyStore
        {
            Dictionary<AttachableMemberIdentifier, object> attachedProperties = new Dictionary<AttachableMemberIdentifier, object>();

            int IAttachedPropertyStore.PropertyCount
            { get { return attachedProperties.Count; } }

            void IAttachedPropertyStore.CopyPropertiesTo(KeyValuePair<AttachableMemberIdentifier, object>[] array, int index)
            {
                ((ICollection<KeyValuePair<AttachableMemberIdentifier, object>>)attachedProperties).CopyTo(array, index);
            }

            bool IAttachedPropertyStore.RemoveProperty(AttachableMemberIdentifier name)
            {
                return attachedProperties.Remove(name);
            }

            void IAttachedPropertyStore.SetProperty(AttachableMemberIdentifier name, object value)
            {
                attachedProperties[name] = value;
            }

            bool IAttachedPropertyStore.TryGetProperty(AttachableMemberIdentifier name, out object value)
            {
                return attachedProperties.TryGetValue(name, out value);
            }
        }
    }

    public class FooSells
    {
        public int Bar
        { get; set; }
    }

    public class ComplexAttachedProperty
    {
        public int Foo
        { get; set; }
        public string Bar
        { get; set; }

        public override bool Equals(object obj)
        {
            ComplexAttachedProperty other = obj as ComplexAttachedProperty;
            if (other == null)
            {
                return false;
            }
            return this.Foo.Equals(other.Foo) && this.Bar.Equals(other.Bar);
        }

        public override int GetHashCode()
        {
            return 0;
        }
    }

    public class FooWithBar
    {
        public int Bar
        { get; set; }
    }

    public class ManyProperties
    {
        public ManyProperties()
        {
            Two = new List<int>();
            Three = new Dictionary<string, int>();
        }

        public object One
        { get; set; }
        public IList<int> Two
        { get; private set; }
        public IDictionary<string, int> Three
        { get; private set; }
    }
}
