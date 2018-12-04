using System;
using System.Collections.Generic;
using System.Text;
using DRT;
using System.Xaml;
using DrtXaml.XamlTestFramework;
using System.Xml;
using System.IO;
using System.Security.Permissions;
using System.Reflection;
using Test.Elements;

namespace DrtXaml.Tests
{
    [TestClass]
    sealed class BasicXamlTests : XamlTestSuite
    {
        public BasicXamlTests()
            : base("BasicXamlTests")
        {
        }

        public override DrtTest[] PrepareTests()
        {
            DrtTest[] tests = DrtTestFinder.FindTests(this);
            return tests;
        }

        // =============================================
        [TestXaml]
        const string Lists0 =
@"<Object10i
    xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses'
    xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>

    <!-- List in a Content Property -->
    <Object10i.Object0>
        <RwParentWithCP>
          <Kid Name='One' />
          <Kid Name='Two' />
          <Kid Name='Three' />
          <Kid Name='Four' />
        </RwParentWithCP>
    </Object10i.Object0>

    <Object10i.Object1>
    <RoParentWithOutCP>
    <!-- List in a ReadOnly collection -->
        <RoParentWithOutCP.RoKids>
          <Kid Name='One' />
          <Kid Name='Two' />
          <Kid Name='Three' />
          <Kid Name='Four' />
        </RoParentWithOutCP.RoKids>
      </RoParentWithOutCP>
    </Object10i.Object1>

    <Object10i.Object2>
      <RoParentWithOutCP>
        <!-- List in a ReadOnly collection w/ Null elements-->
        <RoParentWithOutCP.RoKids>
          <x:Null />
          <Kid Name='One' />
          <Kid Name='Two' />
          <x:Null />
          <Kid Name='Three' />
          <Kid Name='Four' />
          <x:Null />
        </RoParentWithOutCP.RoKids>
      </RoParentWithOutCP>
    </Object10i.Object2>

    <Object10i.Object3>
    </Object10i.Object3>
</Object10i>";

        [TestXaml]
        const string NonIListLists =
@"<Object10i
    xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses'
    xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>

    <Object10i.Object0>
      <EnumPlusAddHolder>
       <!-- Collection == IEnumerable + a couple of Add() methods -->
        <EnumPlusAddHolder.Content>
          <Element/>
          <ElementWithTitle Title='Two' />
          <ElementWithTitle Title='Four' />
        </EnumPlusAddHolder.Content>
      </EnumPlusAddHolder>
    </Object10i.Object0>

    <Object10i.Object1>
      <ElementCollectionHolder>
      <!-- Collection == ICollection<Element> not an 'IList' -->
        <ElementCollectionHolder.Content>
          <Element/>
          <ElementWithTitle Title='Two' />
          <ElementWithTitle Title='Four' />
        </ElementCollectionHolder.Content>
      </ElementCollectionHolder>
    </Object10i.Object1>
</Object10i>";

        [TestXaml]
        const string Dictionary0_XAML =
@"<RoDictionaryProvider
	xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'
    xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses'>
  <RoDictionaryProvider.Dict>
    <Resource SomeData='string1' x:Key='_1' />
    <Resource SomeData='string2' x:Key='_2' />
    <ResourceWithImplictKey SomeData='string3' MyKey='_3' />
    <ResourceWithImplictKey SomeData='string4' MyKey='_4' />
    <ResourceWithRoImplictKey SomeData='string5' />
    <ResourceWithRoImplictKey SomeData='string6' /> 
  </RoDictionaryProvider.Dict>
</RoDictionaryProvider>";

        [TestXaml, TestTreeValidator("Dictionary1_TreeValidator")]
        const string Dictionary1_XAML =
@"<RoDictionaryProvider
	xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'
    xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses'>
  <RoDictionaryProvider.Dict>
    <Resource SomeData='string1' x:Key='_1' />
    <Resource SomeData='string2' x:Key='{SimpleMe _2}' />
    <ColorElement x:Key='_3' ColorName='Purple' />
    <ColorElement x:Key='_4'>Orange</ColorElement>
  </RoDictionaryProvider.Dict>
</RoDictionaryProvider>";

        public void Dictionary1_TreeValidator(object o)
        {
            var root = (Test.Elements.RoDictionaryProvider)o;
            if (root == null)
                throw new NullReferenceException("Load returned null Root");

            // check the content
            var ce3 = (Test.Elements.ColorElement)root.Dict["_3"];
            if (String.Compare(ce3.ColorName, "Purple") != 0)
                throw new Exception("Item with x:Key='_3' is incorrect");

            var ce4 = (Test.Elements.ColorElement)root.Dict["_4"];
            if (String.Compare(ce4.ColorName, "Orange") != 0)
                throw new Exception("Item with x:Key='_4' is incorrect");
        }

        [TestXaml]
        const string SimpleNullable0_XAML =
@"<ElementWithNullableDouble
    xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses'
    NullableDouble='20.02'  />";

        [TestXaml, TestTreeValidator("ContentProperty0_TreeValidator")]
        const string ContentProperty0_XAML =
@"<HoldsOneElement
    xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses' >
<HoldsOneElement.Element>
  <HasTextCpa>
    TextPropertyValue
  </HasTextCpa>
</HoldsOneElement.Element>
</HoldsOneElement>";

        public void ContentProperty0_TreeValidator(object o)
        {
            var root = (Test.Elements.HoldsOneElement)o;
            if (root == null)
                throw new NullReferenceException("Load returned null Root");
            if (((Test.Elements.HasTextCpa)root.Element).Text != "TextPropertyValue")
                throw new Exception("CPA text value compare failed");
        }

        [TestXaml, TestTreeValidator("ContentProperty1_TreeValidator")]
        const string ContentProperty1_XAML =
@"<HoldsOneElement
    xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses' >
<HoldsOneElement.Element>
  <HasTitleCpa>
    This is the Title
  </HasTitleCpa>
</HoldsOneElement.Element>
</HoldsOneElement>";

        public void ContentProperty1_TreeValidator(object o)
        {
            var root = (Test.Elements.HoldsOneElement)o;
            if (root == null)
                throw new NullReferenceException("Load returned null Root");
            if (((Test.Elements.HasTitleCpa)root.Element).Title != "This is the Title")
                throw new Exception("CPA Title value compare failed");
        }

        [TestXaml, TestTreeValidator("ContentProperty2_TreeValidator")]
        const string ContentProperty2_XAML =
@"<HoldsOneElement
    xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses' >
<HoldsOneElement.Element>
  <InheritesTitleCP>
    This is the Title
  </InheritesTitleCP>
</HoldsOneElement.Element>
</HoldsOneElement>";

        public void ContentProperty2_TreeValidator(object o)
        {
            var root = (Test.Elements.HoldsOneElement)o;
            if (root == null)
                throw new NullReferenceException("Load returned null Root");
            if (((Test.Elements.InheritesTitleCP)root.Element).Text != "This is the Title")
                throw new Exception("CPA text value compare failed");
        }

        [TestXaml, TestAlternateXamlLoader("ContentProperty3_MustFail")]
        const string ContentProperty3_XAML =
@"<HoldsOneElement
    xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses' >
<HoldsOneElement.Element>
  <TurnsOffInheritedCPA>
    This is the Title
  </TurnsOffInheritedCPA>
</HoldsOneElement.Element>
</HoldsOneElement>";

        public object ContentProperty3_MustFail(string xamlString)
        {
            bool caughtException = false;
            try
            {
                XamlServices.Parse(xamlString);
            }
            catch (Exception)
            {
                caughtException = true;
            }
            if (!caughtException)
            {
                throw new Exception("Test should throw a missing CPA Exception");
            }
            return new object();  // alternate Loaders must not return null.
        }

        [TestXaml, TestAlternateXamlLoader("ContentProperty4_MustFail")]
        const string ContentProperty4_XAML =
@"<HoldsOneElement
    xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses' >
<HoldsOneElement.Element>
  <TurnsOffInheritedCPAwNull>
    This is the Title
  </TurnsOffInheritedCPAwNull>
</HoldsOneElement.Element>
</HoldsOneElement>";

        public object ContentProperty4_MustFail(string xamlString)
        {
            bool caughtException = false;
            try
            {
                XamlServices.Parse(xamlString);
            }
            catch (XamlException)
            {
                caughtException = true;
            }
            if (!caughtException)
            {
                throw new Exception("Test should throw a missing CPA Exception");
            }
            return new object();  // alternate Loaders must not return null.
        }

        [TestXaml, TestTreeValidator("ContentProperty5_TreeValidator")]
        const string ContentProperty5_XAML =
@"<HoldsOneElement
    xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses' >
<HoldsOneElement.Element>
  <ChangesInheritedCPAToText>
    This is the Text
  </ChangesInheritedCPAToText>
</HoldsOneElement.Element>
</HoldsOneElement>";

        public void ContentProperty5_TreeValidator(object o)
        {
            var root = (Test.Elements.HoldsOneElement)o;
            if (root == null)
                throw new NullReferenceException("Load returned null Root");
            if (((Test.Elements.ChangesInheritedCPAToText)root.Element).Text != "This is the Text")
                throw new Exception("CPA text value compare failed");
        }

        [TestXaml, TestTreeValidator("ContentProperty6_TreeValidator")]
        const string ContentProperty6_XAML =
@"<ElementListHolder     xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses' >
  <ElementListHolder.Elements>
    <InheritedContentType1 > text </InheritedContentType1 >
    <InheritedContentType2 > text </InheritedContentType2 >
    <InheritedContentType3 > This is the Text </InheritedContentType3 >
  </ElementListHolder.Elements>
</ElementListHolder>";

        public void ContentProperty6_TreeValidator(object o)
        {
            var root = (Test.Elements.ElementListHolder)o;
            if (root == null)
                throw new NullReferenceException("Load returned null Root");
            var es = root.Elements[2];
            if (((Test.Elements.InheritedContentType3)es).Content != "This is the Text")
                throw new Exception("CPA Content value compare failed");
        }

        [TestXaml]
        const string GetDirectiveValue_XAML =
@"<RoDictionaryProvider
	xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'
        xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses'>
  <RoDictionaryProvider.Dict>
    <BuggyHashCodeResource SomeData='string1' Value='5' x:Key='_1' />
    <BuggyHashCodeResource SomeData='string2' x:Key='_2' />
  </RoDictionaryProvider.Dict>
</RoDictionaryProvider>";

        [TestXaml]
        const string MEInACollection_XAML =
@"<ElementResourceHolder   xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses'
                         xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'
                         xmlns:sys='clr-namespace:System;assembly=mscorlib' >
  <ElementResourceHolder.Resources>
    <ElementResourceDictionary x:Key='_dct'>
      <ElementWithTitle x:Key='elm' Title='Hello World' />
    </ElementResourceDictionary>
  </ElementResourceHolder.Resources>

  <RWElementResourceHolder>
    <RWElementResourceHolder.Resources>
      <ResourceLookupExtension ResourceKey='_dct'/>
    </RWElementResourceHolder.Resources>

    <HoldsOneElement  Element='{ResourceLookup elm}' />
  
  </RWElementResourceHolder>
</ElementResourceHolder>";

        //        [TestXaml]
        //        const string GrandParentProperty_XAML =
        //        @"<g:List xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'
        //                  xmlns:g='clr-namespace:System.Collections.Generic;assembly=mscorlib'
        //                  x:TypeArguments='x:String'>
        //                <x:String>Hello</x:String>
        //        </g:List>";

        //        [TestXaml]
        //        const string MEInACollection2_XAML = @"<RWElementResourceHolder   xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses'
        //                         xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'
        //                         xmlns:sys='clr-namespace:System;assembly=mscorlib' >
        //    <RWElementResourceHolder.Resources>
        //      <IntResourceExtension x:Key='int'/>
        //    </RWElementResourceHolder.Resources>
        //</RWElementResourceHolder>";

        [TestXaml]
        const string AMEET_XAML = @"<MySetter   
                         xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses'
                         xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'
                         xmlns:sys='clr-namespace:System;assembly=mscorlib'
                         Value='{MyBinding}'/>";

        [TestXaml, TestExpectedException(typeof(XamlDuplicateMemberException))]
        const string DuplicateTemplateSet_XAML =
@"<TemplateClass2
    xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses'>
    <TemplateClass2.Template>
        <TemplateClass2/>
    </TemplateClass2.Template>
    <TemplateClass2.Template>
        <TemplateClass2/>
    </TemplateClass2.Template>
  </TemplateClass2>";

        [TestXaml]
        const string DottedPrefix_XAML = @"<s.s:String xmlns:s.s='clr-namespace:System;assembly=mscorlib'>Hello</s.s:String>";

        [TestXaml]
        const string xKeyInit_XAML = @"<x:String xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
                                    <x:Key>Key2</x:Key>
                                    RedBrush
                                    </x:String>";

        // Tests to write TypeConverter for MyButton, and MyButton with Content property and Prop property
        //        [TestXaml]
        //        const string xKeyInit_XAML2 = @"<m:MyButton
        //                                         Prop='A'
        //                                        xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
        //                                    <x:Key>Key2</x:Key>
        //                                    Content
        //                                    </m:MyButton>";

        //        [TestXaml]
        //        const string xKeyInit_XAML3 = @"<m:MyButton
        //                                        xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
        //                                    <x:Key>Key2</x:Key>
        //                                    Init text
        //                                    </m:MyButton>";

        [TestXaml, TestTreeValidator("AttachedPropOnCtor_TreeValidator")]
        const string AttachedPropsOnTypeConvertedType = @"
<Element10
    xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses'
    xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml' >
   <Element10.Element0>
     <ColorElement APP.Foo='FooString'> Blue </ColorElement>
   </Element10.Element0>

   <Element10.Element1>
     <ColorElement x:Name='_redElement' APP.Foo='RedElement'> Red </ColorElement>
   </Element10.Element1>

</Element10>
";

        [TestXaml]
        const string CreateValueTypeElement = @"
<Object10
    xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses'
    xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml' >

   <Object10.Object0>
     <ColorStruct>
       <ColorStruct.Color>
            Orange 
       </ColorStruct.Color>
     </ColorStruct>
   </Object10.Object0>
</Object10>
";


        public void AttachedPropOnCtor_TreeValidator(object o)
        {
            Element10 root = (Element10)o;
            ColorElement color;
            string attachedString;

            color = (ColorElement)root.Element0;
            Assert.AreEqual("Blue", color.ColorName);
            attachedString = APP.GetFoo(color);
            Assert.AreEqual("FooString", attachedString);

            color = (ColorElement)root.Element1;
            Assert.AreEqual("Red", color.ColorName);
            attachedString = APP.GetFoo(color);
            Assert.AreEqual("RedElement", attachedString);
        }

        [TestXaml, TestTreeValidator("TypeConverterAndStringCPA_TreeValidator")]
        const string TypeConverterAndStringCPA = @"
<Element10
    xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses'
    xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml' >

   <Element10.Element0>
     <ColorElementDuel> Orange-Duel </ColorElementDuel>
   </Element10.Element0>

</Element10>
";
        public void TypeConverterAndStringCPA_TreeValidator(object o)
        {
            Element10 root = (Element10)o;

            ColorElementDuel duel = (ColorElementDuel)root.Element0;
            string valueFromTypeConverter = duel.ColorName;
            string valueFromContentProperty = duel.ColorNameCPA;

            // We should prefer string CPA to TypeConverter.
            Assert.IsNull(valueFromTypeConverter);
            Assert.AreEqual("Orange-Duel", valueFromContentProperty);
        }

        [TestXaml, TestTreeValidator("TypeConverterAndStringList_TreeValidator")]
        const string TypeConverterAndStringList = @"
<Object10i
    xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses'
    xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml' >

   <Object10i.Object0>
     <ColorListTC> Orange </ColorListTC>
   </Object10i.Object0>

</Object10i>
";
        public void TypeConverterAndStringList_TreeValidator(object o)
        {
            Object10i root = (Object10i)o;
            ColorListTC listTC = (ColorListTC)root.Object0;

            // We should prefer TypeConverter to string list
            Assert.AreEqual(0, listTC.Count);
            Assert.AreEqual("Orange", listTC.MainColor);
        }

        [TestXaml, TestTreeValidator("ContentPropertyAndStringList_TreeValidator")]
        const string ContentPropertyAndStringList = @"
<Object10i
    xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses'
    xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml' >

   <Object10i.Object0>
     <ColorListCPA> Orange </ColorListCPA>
   </Object10i.Object0>

</Object10i>
";
        public void ContentPropertyAndStringList_TreeValidator(object o)
        {
            Object10i root = (Object10i)o;
            ColorListCPA listCPA = (ColorListCPA)root.Object0;

            // We should prefer content property to string list
            Assert.AreEqual(0, listCPA.Count);
            Assert.AreEqual("Orange", listCPA.MainColor);
        }

        [TestXaml, TestTreeValidator("ContentPropertyWithTypeConverter_TreeValidator")]
        const string ContentPropertyWithTypeConverter = @"
<Element10
    xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses'
    xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml' >

   <Element10.Element0>
     <ColorElementCPA> Orange </ColorElementCPA>
   </Element10.Element0>

</Element10>
";
        public void ContentPropertyWithTypeConverter_TreeValidator(object o)
        {
            Element10 root = (Element10)o;
            ColorElementCPA colorHolder = (ColorElementCPA)root.Element0;
            Assert.IsNotNull(colorHolder.Color);
            Assert.AreEqual("Orange", colorHolder.Color.ColorName);
        }

        [TestXaml, TestExpectedException(typeof(XamlDuplicateMemberException))]
        const string CollectionContentSplitByProperty_XAML =
@"<DoubleCollection 
    xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses'
    xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml' >
    <x:Double>3.5</x:Double>
    <x:Key>Value2</x:Key>
    <x:Double>0.10</x:Double>
</DoubleCollection>";

        [TestXaml, TestExpectedException(typeof(XamlObjectWriterException))]
        const string BasicUnknownTest =
            @"<StackPanel xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'>
<abc />
</StackPanel>";

        [TestXaml, TestKnownFailure("BChapman Requires XML Compat fixes")]
        const string MarkupCompatibilityXmlns = @"
<StackPanel Name='CustomStackPanel0' xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' 
xmlns:v1='http://schemas.microsoft.com/winfx/2006/xaml/presentation'
xmlns:mc='http://schemas.openxmlformats.org/markup-compatibility/2006'
mc:Ignorable='v1'>
  <mc:AlternateContent Name='AlternateContent' xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
    <mc:Choice Requires='v1'>
      <v1:Button Background='{x:Static Brushes.Red}'>
          This is a v2 button
      </v1:Button>
    </mc:Choice>
    <mc:Fallback>
    </mc:Fallback>
  </mc:AlternateContent>
</StackPanel>";

        [TestXaml]
        const string xTypeTypeName = @"<Style xmlns = 'http://schemas.microsoft.com/winfx/2006/xaml/presentation'
xmlns:x = 'http://schemas.microsoft.com/winfx/2006/xaml'>
<Style.TargetType>
<x:Type TypeName='ListBox'/>
</Style.TargetType>
</Style>";

        [TestXaml]
        const string CollectionInit = @"<Canvas xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'>
<Polygon Name='Polygon'><Polygon.Points>
        15,10 50,30 50,25 45,20
        45,15 50,10 55,10 60,15
        60,20 55,25 55,30 60,30
        75,45 60,55 72,45 60,35
        55,60 70,95 53.5,65 
        35,95 50,60 45,35 15,10</Polygon.Points>
</Polygon>
</Canvas>";

        // Need to set XamlSchemaContextSettings.SupportMarkupExtensionsWithDuplicateArity = true
        [TestXaml, TestAlternateXamlLoader(@"DuplicateArityLoader")]
        const string DuplicateArity = @"<TextBlock xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'
xmlns:t='clr-namespace:Test.Elements;assembly=XamlTestClasses'
Text='{t:DuplicateArity 1}'/>";

        public object DuplicateArityLoader(string xamlString)
        {
            var scSettings = new XamlSchemaContextSettings();
            scSettings.SupportMarkupExtensionsWithDuplicateArity = true;
            var xsc = new XamlSchemaContext(scSettings);
            var xmlReader = XmlReader.Create(new StringReader(xamlString));
            XamlReader reader = new XamlXmlReader(xmlReader, xsc);
            XamlObjectWriter objWriter = new XamlObjectWriter(reader.SchemaContext);
            XamlServices.Transform(reader, objWriter);
            object root = objWriter.Result;
            if (root == null)
                throw new NullReferenceException("Load returned null Root");
            return root;
        }

        // Need to set XamlReaderSettings.UidsOnPropertyElements = true
        [TestXaml, TestAlternateXamlLoader(@"xUidInPELoader")]
        const string xUidInPE = @"<Button xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'
xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
<Button.Content x:Uid='1'>
WPF!
</Button.Content>
</Button>";

        public object xUidInPELoader(string xamlString)
        {
            var xtrSettings = new XamlXmlReaderSettings();
            xtrSettings.IgnoreUidsOnPropertyElements = true;
            var xmlReader = XmlReader.Create(new StringReader(xamlString));
            XamlReader reader = new XamlXmlReader(xmlReader, xtrSettings);
            XamlObjectWriter objWriter = new XamlObjectWriter(reader.SchemaContext);
            XamlServices.Transform(reader, objWriter);
            object root = objWriter.Result;
            if (root == null)
                throw new NullReferenceException("Load returned null Root");
            return root;
        }

        [TestXaml, TestTreeValidator("ConverterWhichLiesAboutConvertability_TreeValidator")]
        const string ConverterWhichLiesAboutConvertabilityWithDefaultSettings_XAML =
@"<ConverterTestElement
    xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses' 
    String='original value' />";

        public void ConverterWhichLiesAboutConvertability_TreeValidator(object o)
        {
            ConverterTestElement converterTestElement = (ConverterTestElement)o;
            switch (converterTestElement.String)
            {
                case "original value":
                    //Great.  That is what it should be.
                    break;
                case "original value has been converted.":
                    throw new Exception("Type Converter was called, but it shouldn't have since it answers false for CanConvertFrom(typeof(string))");
                default:
                    throw new Exception("unexpected value for String property was '" + converterTestElement.String + "', but we expected 'original value'");
            }
        }

        [TestXaml, TestAlternateXamlLoader("UseObjectWriterWithIgnoreCanConvert_Loader"),
            TestTreeValidator("ConverterWhichLiesAboutConvertabilityWithIgnoreCanConvert_TreeValidator")]
        const string ConverterWhichLiesAboutConvertabilityWithIgnoreCanConvertSetting_XAML =
@"<ConverterTestElement
    xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses' 
    String='original value' />";

        public object UseObjectWriterWithIgnoreCanConvert_Loader(string xamlString)
        {
            XmlReader xmlReader = XmlReader.Create(new StringReader(xamlString));
            XamlXmlReader xtr = new XamlXmlReader(xmlReader);
            XamlObjectWriterSettings settings = new XamlObjectWriterSettings();
            settings.IgnoreCanConvert = true;
            XamlObjectWriter ow = new XamlObjectWriter(xtr.SchemaContext, settings);
            XamlServices.Transform(xtr, ow);
            return ow.Result;
        }

        public void ConverterWhichLiesAboutConvertabilityWithIgnoreCanConvert_TreeValidator(object o)
        {
            ConverterTestElement converterTestElement = (ConverterTestElement)o;
            switch (converterTestElement.String)
            {
                case "original value has been converted.":
                    //Great.  That is what it should be.
                    break;
                case "original value":
                    throw new Exception("Type Converter wasn't called, but it should have since we don't check CanConvertFrom(typeof(string))");
                default:
                    throw new Exception("unexpected value for String property was '" + converterTestElement.String + "', but we expected 'original value'");
            }
        }

        [TestXaml, TestTreeValidator("UseGenericConverter_Validator")]
        const string UseGenericConverter =
@"<ClassWithGenericConverter
    xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses'>Dummy string</ClassWithGenericConverter>";

        public void UseGenericConverter_Validator(object o)
        {
            var obj = (ClassWithGenericConverter)o;
            Assert.AreEqual("Dummy string", obj.Value);
        }

        [TestXaml, TestTreeValidator("ArrayTypeName_Validator")]
        const string ArrayTypeName =
@"<Object10i
    xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses'
    xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
    <Object10i.Object0>
      <x:Type TypeName='Element[]' />
    </Object10i.Object0>
</Object10i>";

        public void ArrayTypeName_Validator(object o)
        {
            var obj = (Object10i)o;
            Assert.AreEqual(typeof(Element[]), obj.Object0);
        }

        // Tests that in curly form we resolve to ColorElementExtension before ColorElement
        [TestXaml, TestTreeValidator("MarkupExtensionPrecedence_Validator")]
        const string MarkupExtensionPrecedence =
@"<HoldsOneElement
    xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses'
    xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'
    Element='{ColorElement MEColorName=Red}' />";

        public void MarkupExtensionPrecedence_Validator(object o)
        {
            var obj = (HoldsOneElement)o;
            var color = (ColorElement)obj.Element;
            Assert.AreEqual("Red", color.ColorName);
        }

        [TestXaml, TestTreeValidator("NestedTypeName_Validator")]
        const string NestedTypeName =
@"<Object10i
    xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses'
    xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'
    Object0='{x:Type HasNested+NestedClass}' />";

        public void NestedTypeName_Validator(object o)
        {
            Object10i o10i = (Object10i)o;
            Type type = (Type)o10i.Object0;
            Assert.AreEqual(typeof(HasNested.NestedClass), type);
        }

        [TestXaml, TestTreeValidator("StaticExtensionUsingNestedTypeName_Validator")]
        const string StaticExtensionUsingNestedTypeName =
@"<Object10i
    xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses'
    xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'
    Object0='{x:Static HasNested.StaticProp}'
    Object1='{x:Static HasNested+NestedClass.StaticProp}'
    />";

        public void StaticExtensionUsingNestedTypeName_Validator(object o)
        {
            Object10i o10i = (Object10i)o;
            int int0 = (int)o10i.Object0;
            Assert.AreEqual(int0, 5);
            int int1 = (int)o10i.Object1;
            Assert.AreEqual(int1, 10);
        }

        //We currently get an XmlException here...should be compat with v3...need to test.
        [TestXaml, TestExpectedException(typeof(XmlException))]
        const string NestedTypesAsTagNamesShouldNotWork =
@"<HasNested+NestedClass
    xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses'
    xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'
    />";

        [TestXaml]
        const string EmptyPropertyElement =
@"<HoldsOneElement
    xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses'
    xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'
    >
    <HoldsOneElement.Element/>
</HoldsOneElement>";

        [TestXaml, TestExpectedException(typeof(XamlDuplicateMemberException))]
        const string DuplicateWithEmptyPropertyElement =
@"<HoldsOneElement
    xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses'
    xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'
    >
    <HoldsOneElement.Element/>
    <HoldsOneElement.Element>
        <ColorElement/>
    </HoldsOneElement.Element>
</HoldsOneElement>";

        [TestXaml, TestExpectedException(typeof(XamlDuplicateMemberException))]
        const string MultipleValuesSingularProperty = @"
<Button xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'>
  <Button.Background>
    <Brush>Red</Brush>
    <Brush>Green</Brush>
    <Brush>Cyan</Brush>
  </Button.Background>
</Button>
";

        [TestXaml]
        const string StaticExtensionTest0 = @"
<Object10i
        xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses'
        xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'
    Object0='{x:Static StaticColorElements.Red}'
    Object1='{x:Static StaticColorElements.Blue}' />
";

        [TestXaml]
        const string DictWithMeWithKey0 = @"
<g:Dictionary x:TypeArguments='x:String, Element'
        xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses'
        xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'
	    xmlns:g='clr-namespace:System.Collections.Generic;assembly=mscorlib'>
    <x:Static Member='StaticColorElements.Red' x:Key='red'/>
    <x:Static Member='StaticColorElements.Blue' x:Key='blue'/>
</g:Dictionary>
";

        [TestXaml]
        const string DictWithMeWithKey1 = @"
<ElementDictionaryHolder
        xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses'
        xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'
	    xmlns:g='clr-namespace:System.Collections.Generic;assembly=mscorlib'>
    <ElementDictionaryHolder.ElementDictionary>
        <x:StaticExtension Member='StaticColorElements.Red' x:Key='red'/>
        <x:StaticExtension Member='StaticColorElements.Blue' x:Key='blue'/>
    </ElementDictionaryHolder.ElementDictionary>
</ElementDictionaryHolder>
";

        [TestXaml]
        const string DictWithMeWithNoKey = @"
<ElementDictionaryHolder
        xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses'
        xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'
	    xmlns:g='clr-namespace:System.Collections.Generic;assembly=mscorlib'>
    <ElementDictionaryHolder.ElementDictionary>
        <x:StaticExtension Member='StaticColorElements.ElementDictionary'/>
    </ElementDictionaryHolder.ElementDictionary>
</ElementDictionaryHolder>
";

        [TestXaml]
        const string EmptyElementXmlnsDef = @"<List x:TypeArguments='s:Object' Capacity='4' 
xmlns='clr-namespace:System.Collections.Generic;assembly=mscorlib' 
xmlns:s='clr-namespace:System;assembly=mscorlib'  
xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
  <Object xmlns='clr-namespace:System;assembly=mscorlib'/>
  <List x:TypeArguments='s:Object' Capacity='4'>
    <x:String>blah</x:String>
  </List>
</List>";

        [TestXaml]
        const string EmptyStringDefaultConstructable =
        @"<Button xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'></Button>";

        [TestXaml, TestExpectedException(typeof(XamlObjectWriterException))]
        const string EmptyStringTypeConvertable =
        @"<Brush xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'></Brush>";

        [TestXaml]
        const string PointWithPropertyAndEmptyContent =
        @"<Point X='1' xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'></Point>";

        [TestXaml]
        const string StringWithEmptyContent =
        @"<x:String xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'></x:String>";

        // Upgrade test infrastructure to allow unknown type and property tests
        [TestXaml, TestKnownFailure("jezhan")]
        const string UnknownProp =
        @"<Foo xmlns='Unknown' xml:space='preserve'><Foo.Bar> <Baz/></Foo.Bar></Foo>";

        [TestXaml, TestTreeValidator("ValidateShadowedProperty")]
        const string ShadowedContentProperty =
        @"<Shadower xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses'>Hello!</Shadower>";

        public void ValidateShadowedProperty(object o)
        {
            Shadower shadower = (Shadower)o;
            Assert.AreEqual("Hello!", shadower.Content);
            Shadowed shadowed = (Shadowed)o;
            Assert.IsNull(shadowed.Content);
        }

        [TestXaml, TestTreeValidator("ValidateAPPFoo")]
        const string AttachedPropOnDerivedClass =
        @"<InheritsAPP xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses' APP.Foo='Hello' />";

        public void ValidateAPPFoo(object o)
        {
            Assert.AreEqual("Hello", APP.GetFoo(o));
        }

        [TestXaml, TestTreeValidator("ValidateWriteOnly")]
        const string WriteOnlyProperty =
        @"<ElementWithWriteOnly xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses' WriteOnly='Hello' />";

        public void ValidateWriteOnly(object o)
        {
            ElementWithWriteOnly ewo = (ElementWithWriteOnly)o;
            Assert.AreEqual("Hello", ewo.ReadOnly);
        }

        [TestXaml]
        const string ClassInRootNamespace1 =
        @"<ClassInRootNamespace xmlns='http://testroot' />";

        [TestXaml]
        const string ClassInRootNamespace2 =
        @"<ClassInRootNamespace xmlns='clr-namespace:;assembly=XamlTestClasses' />";

    }
}
