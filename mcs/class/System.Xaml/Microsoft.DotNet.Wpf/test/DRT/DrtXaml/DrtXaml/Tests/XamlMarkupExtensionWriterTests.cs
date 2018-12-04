namespace DrtXaml.Tests
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design.Serialization;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Text;
    using System.Windows.Markup;
    using System.Xaml;
    using System.Xml;
    using System.Collections.Generic;
    using DrtXaml.XamlTestFramework;
    using Test.Elements;
    using DRT;
    using Test.Elements.AnotherNamespace;

    [TestClass]
    class XamlMarkupExtensionWriterTests : XamlTestSuite
    {
        public XamlMarkupExtensionWriterTests()
            : base("XamlMarkupExtensionWriterTests")
        {
        }

        public override DrtTest[] PrepareTests()
        {
            DrtTest[] tests = DrtTestFinder.FindTests(this);
            return tests;
        }

        string SaveToString(object o)
        {
            var sw = new StringWriter();
            using (var xw = XmlWriter.Create(sw, new XmlWriterSettings { Indent = true, OmitXmlDeclaration = true }))
            {
                XamlServices.Save(xw, o);
            }
            return sw.ToString();
        }

        object RoundTrip(object o)
        {
            string s = SaveToString(o);
            return XamlServices.Parse(s);
        }

        [TestMethod]
        public void SimpleAttributableMETest()
        {
            SimpleAttributableMEContainer obj = new SimpleAttributableMEContainer();
            string generated = SaveToString(obj);

            var expected = @"<SimpleAttributableMEContainer InstanceOfClassWithMEConverter=""{ClassWithME IntData=1, StringData=hello}"" xmlns=""clr-namespace:Test.Elements;assembly=XamlTestClasses"" />";

            Assert.AreEqual(expected, generated);
        }

        [TestMethod]
        public void SimpleNonAttributableMETest()
        {
            SimpleNonAttributableMEContainer obj = new SimpleNonAttributableMEContainer();
            string generated = SaveToString(obj);

            var expected = @"<SimpleNonAttributableMEContainer xmlns=""clr-namespace:Test.Elements;assembly=XamlTestClasses"">
  <SimpleNonAttributableMEContainer.InstanceOfClassWithMEConverter>
    <ClassWithNonAttributableME IntData=""1"" StringData=""hello"">
      <ClassWithNonAttributableME.ComplexTypeInstance>
        <ComplexType IntData=""2"" />
      </ClassWithNonAttributableME.ComplexTypeInstance>
    </ClassWithNonAttributableME>
  </SimpleNonAttributableMEContainer.InstanceOfClassWithMEConverter>
</SimpleNonAttributableMEContainer>";

            Assert.AreEqual(expected, generated);
        }

        [TestMethod]
        public void NestedMETest()
        {
            NestedMEContainer obj = new NestedMEContainer();
            var generated = SaveToString(obj);

            var expected = @"<NestedMEContainer InstanceOfClassWithMEConverter=""{ClassWithNestedME ComplexTypeInstance={ComplexType IntData=100, StringData=complex}, IntData=1, StringData=hello}"" xmlns=""clr-namespace:Test.Elements;assembly=XamlTestClasses"" />";

            Assert.AreEqual(expected, generated);
        }

        [TestMethod]
        public void MEWithStringConvertersTest()
        {
            MEWithStringConvertersContainer obj = new MEWithStringConvertersContainer();
            var generated = SaveToString(obj);

            var expected = @"<MEWithStringConvertersContainer InstanceOfClassWithMEConverter=""{ClassWithNestedMEExtension2 ComplexTypeInstance={ComplexTypeExtension2 InstanceOfClassWithStringConverter=&quot;(1,2)&quot;, IntData=100}, InstanceOfClassWithStringConverter=&quot;(1,2)&quot;, IntData=1}"" InstanceOfClassWithStringConverter=""(1,2)"" xmlns=""clr-namespace:Test.Elements;assembly=XamlTestClasses"" />";

            Assert.AreEqual(expected, generated);
        }

        [TestMethod]
        public void MEWithCollectionTest()
        {
            MEWithCollectionContainer obj = new MEWithCollectionContainer();
            var generated = SaveToString(obj);

            var expected = @"<MEWithCollectionContainer InstanceOfClassWithMEWithCollectionConverter=""{CollectionME IntData=2}"" xmlns=""clr-namespace:Test.Elements;assembly=XamlTestClasses"" />";

            Assert.AreEqual(expected, generated);
        }

        [TestMethod]
        public void SimpleMEWithCstrArgsTest()
        {
            SimpleMEWithCstrArgsContainer obj = new SimpleMEWithCstrArgsContainer();
            var generated = SaveToString(obj);
            var expected = @"<SimpleMEWithCstrArgsContainer InstanceOfClassWithMEConverter=""{ClassWithMEWithCstrArgs 1, hello}"" xmlns=""clr-namespace:Test.Elements;assembly=XamlTestClasses"" />";

            Assert.AreEqual(expected, generated);
        }

        [TestMethod]
        public void MEWithParamCstrArgsTest()
        {
            MEWithParamCstrArgsContainer obj = new MEWithParamCstrArgsContainer();
            var generated = SaveToString(obj);
            var expected =
@"<MEWithParamCstrArgsContainer xmlns=""clr-namespace:Test.Elements;assembly=XamlTestClasses"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
  <MEWithParamCstrArgsContainer.InstanceOfClassWithMEConverter>
    <ClassWithMEWithParamCstrArgs>
      <x:Arguments>
        <x:Array Type=""x:Int32"">
          <x:Int32>1</x:Int32>
          <x:Int32>2</x:Int32>
          <x:Int32>3</x:Int32>
        </x:Array>
      </x:Arguments>
    </ClassWithMEWithParamCstrArgs>
  </MEWithParamCstrArgsContainer.InstanceOfClassWithMEConverter>
</MEWithParamCstrArgsContainer>";

            Assert.AreEqual(expected, generated);
        }

        [TestMethod]
        public void NestedMEWithCstrArgsTest1()
        {
            NestedMEWithCstrArgsContainer obj = new NestedMEWithCstrArgsContainer();
            var generated = SaveToString(obj);

            var expected = 
@"<NestedMEWithCstrArgsContainer InstanceOfClassWithMEConverter=""{ClassWithNestedMEWithCstrArgs ComplexTypeInstance={ComplexTypeExtension3 IntData=100, StringData=complex}, ComplexTypeInstanceForCstrArgs={ComplexTypeExtension3 IntData=100, StringData=complex}, IntData=1, StringData=hello}"" xmlns=""clr-namespace:Test.Elements;assembly=XamlTestClasses"" />";

            Assert.AreEqual(expected, generated);
        }

        [TestMethod]
        public void NestedMEWithCstrArgsTest2()
        {
            var o = new ClassWithNestedMEWithCstrArgsContainer2 { property = new ClassWithNestedMEWithCstrArgsExtension2(123, new ComplexTypeExtension3()) };
            var generated = SaveToString(o);

            var expected =
@"<ClassWithNestedMEWithCstrArgsContainer2 property=""{ClassWithNestedMEWithCstrArgsExtension2 123, {ComplexTypeExtension3 IntData=100, StringData=complex}, ComplexTypeInstance={ComplexTypeExtension3 IntData=100, StringData=complex}, StringData={x:Null}}"" xmlns=""clr-namespace:Test.Elements;assembly=XamlTestClasses"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"" />";

            Assert.AreEqual(expected, generated);
        }

        [TestMethod]
        public void ReferenceMETest()
        {
            ContainingRefClass obj = new ContainingRefClass();
            var generated = SaveToString(obj);
            var expected = 
@"<ContainingRefClass Be=""{MERefTest Value={x:Reference __ReferenceID0}}"" xmlns=""clr-namespace:Test.Elements;assembly=XamlTestClasses"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
  <ContainingRefClass.B>
    <MERefTest Reference=""{x:Reference __ReferenceID0}"" x:Name=""__ReferenceID0"" />
  </ContainingRefClass.B>
</ContainingRefClass>";

            Assert.AreEqual(expected, generated);
        }

        [TestMethod]
        public void SpecialCharactersTest()
        {
            SimpleMEContainer2 obj = new SimpleMEContainer2();
            var generated = SaveToString(obj);
            var expected = @"<SimpleMEContainer2 InstanceOfClassWithMEConverter=""{ClassWithMEExtension2 IntData=1, StringData1=&quot;Has a Space&quot;, StringData2=&quot;I'm \&quot;cool\&quot;, 1 = 2.  {Math is \\magic\\} &quot;}"" xmlns=""clr-namespace:Test.Elements;assembly=XamlTestClasses"" />";

            Assert.AreEqual(expected, generated);
        }

        [TestMethod, TestDisabled]
        public void RoundTripSpecialCharsTest()
        {
            AddressContainer obj = new AddressContainer() { AddressInst = new Address() { Street = @"He said, ""Kewl, isn't it?"" ", City = @"1 != 2, {3 + 4}", State = @"http:\\cool.com\index.html" } };

            AddressContainer objAfterRoundTrip = RoundTrip(obj) as AddressContainer;
            Assert.IsNotNull(objAfterRoundTrip);
            Assert.IsNotNull(objAfterRoundTrip.AddressInst);
            Assert.AreEqual(objAfterRoundTrip.AddressInst.Street, obj.AddressInst.Street);
            Assert.AreEqual(objAfterRoundTrip.AddressInst.State, obj.AddressInst.State);
            Assert.AreEqual(objAfterRoundTrip.AddressInst.City, obj.AddressInst.City);
        }

        [TestMethod]
        public void NullValueTest()
        {
            var generated = new StringBuilder();
            var xmlSettings = new XmlWriterSettings { Indent = true, OmitXmlDeclaration = true };
            var xamlSettings = new XamlXmlWriterSettings { CloseOutput = true};
            var xsc = new XamlSchemaContext();
            using (System.Xaml.XamlXmlWriter writer = new System.Xaml.XamlXmlWriter(XmlWriter.Create(new StringWriter(generated), xmlSettings), xsc, xamlSettings))
            {
                var developerType = xsc.GetXamlType(typeof(Developer));
                var developerExtensionType = xsc.GetXamlType(typeof(DeveloperExtension));
                writer.WriteNamespace(new NamespaceDeclaration(XamlLanguage.Xaml2006Namespace, "x"));
                writer.WriteStartObject(developerType);
                writer.WriteStartMember(developerType.GetMember("Name"));
                writer.WriteValue("Andrew");
                writer.WriteEndMember();

                writer.WriteStartMember(developerType.GetMember("Friend"));
                writer.WriteStartObject(developerExtensionType);
                writer.WriteStartMember(developerExtensionType.GetMember("Name"));
                writer.WriteValue(null);
                writer.WriteEndMember();
                writer.WriteEndObject();
                writer.WriteEndMember();

                writer.WriteEndObject();
            }

            var target = @"<Developer Name=""Andrew"" Friend=""{Developer Name={x:Null}}"" xmlns=""clr-namespace:Test.Elements;assembly=XamlTestClasses"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"" />";
            Assert.AreEqual(target, generated.ToString());
        }

        [TestMethod]
        public void EmptyStringTest()
        {
            var generated = new StringBuilder();
            var xmlSettings = new XmlWriterSettings { Indent = true, OmitXmlDeclaration = true };
            var xamlSettings = new XamlXmlWriterSettings { CloseOutput = true };
            var xsc = new XamlSchemaContext();
            using (System.Xaml.XamlXmlWriter writer = new System.Xaml.XamlXmlWriter(XmlWriter.Create(new StringWriter(generated), xmlSettings), xsc, xamlSettings))
            {
                var developerType = xsc.GetXamlType(typeof(Developer));
                var developerExtensionType = xsc.GetXamlType(typeof(DeveloperExtension));
                writer.WriteNamespace(new NamespaceDeclaration(XamlLanguage.Xaml2006Namespace, "x"));
                writer.WriteStartObject(developerType);
                writer.WriteStartMember(developerType.GetMember("Name"));
                writer.WriteValue("Andrew");
                writer.WriteEndMember();

                writer.WriteStartMember(developerType.GetMember("Friend"));
                writer.WriteStartObject(developerExtensionType);
                writer.WriteStartMember(developerExtensionType.GetMember("Name"));
                writer.WriteValue("");
                writer.WriteEndMember();
                writer.WriteEndObject();
                writer.WriteEndMember();

                writer.WriteEndObject();
            }

            var target = @"<Developer Name=""Andrew"" Friend=""{Developer Name=&quot;&quot;}"" xmlns=""clr-namespace:Test.Elements;assembly=XamlTestClasses"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"" />";
            Assert.AreEqual(target, generated.ToString());
        }

        [TestMethod]
        public void AttachedPropertyTest()
        {
            var generated = new StringBuilder();
            var xmlSettings = new XmlWriterSettings { Indent = true, OmitXmlDeclaration = true };
            var xamlSettings = new XamlXmlWriterSettings { CloseOutput = true };
            var xsc = new XamlSchemaContext();
            using (System.Xaml.XamlXmlWriter writer = new System.Xaml.XamlXmlWriter(XmlWriter.Create(new StringWriter(generated), xmlSettings), xsc, xamlSettings))
            {
                XamlType mc = xsc.GetXamlType(typeof(MediumContainer));
                XamlMember thing = mc.GetAttachableMember("Thing");
                var developerType = xsc.GetXamlType(typeof(Developer));
                var developerExtensionType = xsc.GetXamlType(typeof(DeveloperExtension));
                string ns = "clr-namespace:Test.Elements.AnotherNamespace;assembly=XamlTestClasses";
                writer.WriteNamespace(new NamespaceDeclaration(ns, "PreferredPrefix"));
                writer.WriteStartObject(developerType);
                writer.WriteStartMember(developerType.GetMember("Name"));
                writer.WriteValue("Andrew");
                writer.WriteEndMember();

                writer.WriteStartMember(developerType.GetMember("Friend"));
                writer.WriteStartObject(developerExtensionType);
                writer.WriteStartMember(developerExtensionType.GetMember("Name"));
                writer.WriteValue("");
                writer.WriteEndMember();
                writer.WriteEndObject();
                writer.WriteEndMember();

                writer.WriteStartMember(thing);
                writer.WriteValue("attached");
                writer.WriteEndMember();

                writer.WriteEndObject();
            }

            var target = @"<Developer Name=""Andrew"" Friend=""{Developer Name=&quot;&quot;}"" PreferredPrefix:MediumContainer.Thing=""attached"" xmlns=""clr-namespace:Test.Elements;assembly=XamlTestClasses"" xmlns:PreferredPrefix=""clr-namespace:Test.Elements.AnotherNamespace;assembly=XamlTestClasses"" />";
            Assert.AreEqual(target, generated.ToString());
        }

        [TestMethod]
        public void EscapeCurlies()
        {
            var generated = new StringBuilder();
            var xmlSettings = new XmlWriterSettings { Indent = true, OmitXmlDeclaration = true };
            var xamlSettings = new XamlXmlWriterSettings { CloseOutput = true };
            var xsc = new XamlSchemaContext();
            using (System.Xaml.XamlXmlWriter writer = new System.Xaml.XamlXmlWriter(XmlWriter.Create(new StringWriter(generated), xmlSettings), xsc, xamlSettings))
            {
                var developerType = xsc.GetXamlType(typeof(Developer));
                var developerExtensionType = xsc.GetXamlType(typeof(DeveloperExtension));
                writer.WriteNamespace(new NamespaceDeclaration(XamlLanguage.Xaml2006Namespace, "x"));
                writer.WriteStartObject(developerType);
                writer.WriteStartMember(developerType.GetMember("Name"));
                writer.WriteValue("{Andrew}");
                writer.WriteEndMember();
            }
            string target = @"<Developer Name=""{}{Andrew}"" xmlns=""clr-namespace:Test.Elements;assembly=XamlTestClasses"" />";
            Assert.AreEqual(target, generated.ToString());
        }

        [TestMethod]
        public void MEWithTypeWithComplexTypeAsCstrArgument()
        {
            var obj = new ObjectContainer() { O = new CustomMEXargsClass() { Content = "Hello" } };
            string generated = SaveToString(obj);
            string expected = 
@"<ObjectContainer xmlns=""clr-namespace:Test.Elements;assembly=XamlTestClasses"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
  <ObjectContainer.O>
    <CustomMEWithXArgs>
      <x:Arguments>
        <x:Null />
      </x:Arguments>
    </CustomMEWithXArgs>
  </ObjectContainer.O>
</ObjectContainer>";
            Assert.AreEqual(expected, generated);
        }
        /*

        [TestMethod]
        public void CloseTest()
        {
            var generated = new StringBuilder();
            var xmlSettings = new XmlWriterSettings { Indent = true, OmitXmlDeclaration = true };
            var xamlSettings = new XamlWriterSettings { CloseOutput = true };
            using (System.Xaml.XamlXmlWriter writer = new System.Xaml.XamlXmlWriter(XmlWriter.Create(new StringWriter(generated), xmlSettings), xamlSettings))
            {
                writer.WriteStartRecord(XName.Get("{CSD}Developer"));
                writer.WriteStartMember("Name");
                writer.WriteAtom("Andrew");
                writer.WriteEndMember();

                writer.WriteStartMember("Friend");

                writer.WriteStartRecordAsMarkupExtension(XName.Get("{DevDiv}Developer"));

                writer.WriteStartMember("Arguments", XamlServices.DirectiveTypeName2008);

                writer.WriteStartRecord(XName.Get("string", XamlServices.NamespaceBuiltInTypes.NamespaceName));
                writer.WriteStartMember(null);
                writer.WriteAtom("Steven");
                writer.WriteEndMember();
                writer.WriteEndRecord();

                writer.WriteStartRecordAsMarkupExtension(XName.Get("{Confidential}Info"));
                writer.WriteStartMember("Hobby");
                writer.WriteAtom("Camping");
            }

            var target = @"<p:Developer Name=""Andrew"" Friend=""{p1:Developer Steven, {p2:Info Hobby=Camping}}"" xmlns:p=""CSD"" xmlns:p1=""DevDiv"" xmlns:p2=""Confidential"" />";
            Assert.AreEqual(target, generated.ToString());
        }
        */
    }
}
