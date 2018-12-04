using System;
using System.Collections.Generic;
using System.Text;
using DRT;
using System.IO;
using System.Linq;
using System.Xaml;
using System.Xml;
using DrtXaml.XamlTestFramework;
using Test.Collections;

namespace DrtXaml.Tests
{
    [TestClass]
    class CollectionTests : XamlTestSuite
    {
        public CollectionTests()
            : base("CollectionTests")
        {
        }

        public override DrtTest[] PrepareTests()
        {
            DrtTest[] tests = DrtTestFinder.FindTests(this);
            return tests;
        }

        // =============================================

        [TestXaml, TestExpectedException(typeof(XamlDuplicateMemberException))]
        const string CollectionProperties1_XAML =
@"<DoubleCollection 
    xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses'
    xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml' >
    <DoubleCollection.Flavor>Chocloate</DoubleCollection.Flavor>
    <DoubleCollection.Flavor>Strawberry</DoubleCollection.Flavor>    
</DoubleCollection>";

        [TestXaml]
        const string CollectionProperties2_XAML =
@"<DoubleCollection 
    xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses'
    xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml' >
    <x:Double>3.5</x:Double>
</DoubleCollection>";

        [TestXaml]
        const string CollectionProperties3_XAML =
@"<DoubleCollection 
    xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses'
    xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml' >
    <x:Double>3.5</x:Double>
    <DoubleCollection.Flavor>Chocloate</DoubleCollection.Flavor>
</DoubleCollection>";

        [TestXaml, TestExpectedException(typeof(XamlDuplicateMemberException))]
        const string CollectionProperties4_XAML =
@"<DoubleCollection 
    xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses'
    xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml' >
    <x:Double>3.5</x:Double>
    <DoubleCollection.Flavor>Chocloate</DoubleCollection.Flavor>
    <x:Double>3.5</x:Double>
    <DoubleCollection.Flavor>Strawberry</DoubleCollection.Flavor>    
</DoubleCollection>";

        [TestXaml]
        const string CollectionProperties5_XAML =
@"<DoubleCollection 
    xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses'
    xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml' >
    <DoubleCollection.Flavor>Chocloate</DoubleCollection.Flavor>
    <x:Double>3.5</x:Double>
</DoubleCollection>";

        [TestXaml, TestExpectedException(typeof(XamlDuplicateMemberException))]
        const string CollectionProperties6_XAML =
@"<DoubleCollection 
    xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses'
    xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml' >
    <x:Double>3.5</x:Double>
    <x:Key>key</x:Key>
    <x:Double>5.5</x:Double>
</DoubleCollection>";

        [TestXaml]
        const string ImplicitArray_XAML = @"<t:MyContentType 
xmlns:t='clr-namespace:Test.Elements;assembly=XamlTestClasses'
xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
<x:String>Hello</x:String>
<x:String>Hello</x:String>
</t:MyContentType >";

        [TestXaml]
        const string ExplicitArray_XAML = @"<t:MyContentType 
xmlns:t='clr-namespace:Test.Elements;assembly=XamlTestClasses'
xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
<t:MyContentType.Content>
<x:Array Type='x:String'>
<x:String>Hello</x:String>
<x:String>Hello</x:String>
</x:Array>
</t:MyContentType.Content>
</t:MyContentType >";

        [TestXaml]
        const string ImplicitArray2_XAML = @"<t:MyContentType 
xmlns:t='clr-namespace:Test.Elements;assembly=XamlTestClasses'
xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
<t:MyContentType.Content>
<x:String>Hello</x:String>
<x:String>Hello</x:String>
</t:MyContentType.Content>
</t:MyContentType >";

        [TestXaml]
        public const string Key_IDictionary_NoTC = @"<Dictionaries x:TypeArguments='x:Int32, x:String'
    xmlns='clr-namespace:Test.Collections;assembly=XamlTestClasses'
    xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
  <Dictionaries.IDictionary>
    <x:String>
      <x:Key>
        <x:Int32>1</x:Int32>
      </x:Key>
      2
    </x:String>
  </Dictionaries.IDictionary>
</Dictionaries>";

        [TestXaml, TestExpectedException(typeof(XamlObjectWriterException), typeof(ArgumentException))]
        public const string Key_IDictionary_TC = @"<Dictionaries x:TypeArguments='x:Int32, x:String'
    xmlns='clr-namespace:Test.Collections;assembly=XamlTestClasses'
    xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
  <Dictionaries.IDictionary>
    <x:String x:Key='1'>2</x:String>
  </Dictionaries.IDictionary>
</Dictionaries>";

        [TestXaml]
        public const string Key_GenericIDictionary_NoTC = @"<Dictionaries x:TypeArguments='x:Int32, x:String'
    xmlns='clr-namespace:Test.Collections;assembly=XamlTestClasses'
    xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
  <Dictionaries.GenericIDictionary>
    <x:String>
      <x:Key>
        <x:Int32>1</x:Int32>
      </x:Key>
      2
    </x:String>
  </Dictionaries.GenericIDictionary>
</Dictionaries>";

        [TestXaml]
        public const string Key_GenericIDictionary_TC = @"<Dictionaries x:TypeArguments='x:Int32, x:String'
    xmlns='clr-namespace:Test.Collections;assembly=XamlTestClasses'
    xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
  <Dictionaries.GenericIDictionary>
    <x:String x:Key='1'>2</x:String>
  </Dictionaries.GenericIDictionary>
</Dictionaries>";

        [TestXaml]
        public const string Key_Dictionary_NoTC = @"<Dictionaries x:TypeArguments='x:Int32, x:String'
    xmlns='clr-namespace:Test.Collections;assembly=XamlTestClasses'
    xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
  <Dictionaries.Dictionary>
    <x:String>
      <x:Key>
        <x:Int32>1</x:Int32>
      </x:Key>
      2
    </x:String>
  </Dictionaries.Dictionary>
</Dictionaries>";

        [TestXaml]
        public const string Key_Dictionary_TC = @"<Dictionaries x:TypeArguments='x:Int32, x:String'
    xmlns='clr-namespace:Test.Collections;assembly=XamlTestClasses'
    xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
  <Dictionaries.Dictionary>
    <x:String x:Key='1'>2</x:String>
  </Dictionaries.Dictionary>
</Dictionaries>";

        [TestXaml]
        public const string Key_MyDictionary_NoTC = @"<Dictionaries x:TypeArguments='x:Int32, x:String'
    xmlns='clr-namespace:Test.Collections;assembly=XamlTestClasses'
    xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
  <Dictionaries.MyDictionary>
    <x:String>
      <x:Key>
        <x:Int32>1</x:Int32>
      </x:Key>
      2
    </x:String>
  </Dictionaries.MyDictionary>
</Dictionaries>";

        [TestXaml]
        public const string Key_MyDictionary_TC = @"<Dictionaries x:TypeArguments='x:Int32, x:String'
    xmlns='clr-namespace:Test.Collections;assembly=XamlTestClasses'
    xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
  <Dictionaries.MyDictionary>
    <x:String x:Key='1'>2</x:String>
  </Dictionaries.MyDictionary>
</Dictionaries>";

        [TestXaml, TestExpectedException(typeof(XamlObjectWriterException), typeof(Exception))]
        public const string Key_CustomConvertingDictionary = @"<Dictionaries x:TypeArguments='x:Int32, x:String'
    xmlns='clr-namespace:Test.Collections;assembly=XamlTestClasses'
    xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
  <Dictionaries.CustomConvertingDictionary>
    <x:String x:Key='A'>2</x:String>
  </Dictionaries.CustomConvertingDictionary>
</Dictionaries>";

        [TestXaml, TestAlternateXamlLoader("PreferUnconvertedKeysLoader"), TestTreeValidator("CustomConversionApplied")]
        public const string Key_PreferUnconvertedKeys = @"<Dictionaries x:TypeArguments='x:Int32, x:String'
    xmlns='clr-namespace:Test.Collections;assembly=XamlTestClasses'
    xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
  <Dictionaries.MyDictionary>
    <x:String x:Key='1'>2</x:String>
  </Dictionaries.MyDictionary>
  <Dictionaries.CustomConvertingDictionary>
    <x:String x:Key='A'>2</x:String>
  </Dictionaries.CustomConvertingDictionary>
</Dictionaries>";

        [TestXaml, TestAlternateXamlLoader("PreferUnconvertedKeysLoader"), TestTreeValidator("DeferredAddValidator")]
        public const string Key_PreferUnconvertedKeysDeferredAdd = @"<Dictionaries x:TypeArguments='x:Int32, x:String'
    xmlns='clr-namespace:Test.Collections;assembly=XamlTestClasses'
    xmlns:e='clr-namespace:Test.Elements;assembly=XamlTestClasses'
    xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
  <Dictionaries.MyDictionary>
    <x:Reference Name='B' x:Key='1' />
    <x:String x:Key='2'>2</x:String>
  </Dictionaries.MyDictionary>
  <Dictionaries.CustomConvertingDictionary>
    <x:String x:Key='{x:Reference B}'>1</x:String>
    <x:String x:Key='A'>2</x:String>
  </Dictionaries.CustomConvertingDictionary>
  <e:APP.Foo>
    <x:String x:Name='B'>B</x:String>
  </e:APP.Foo>
</Dictionaries>";

        public object PreferUnconvertedKeysLoader(string xaml)
        {
            var settings = new XamlObjectWriterSettings { PreferUnconvertedDictionaryKeys = true };
            var reader = new XamlXmlReader(XmlReader.Create(new StringReader(xaml)));
            var writer = new XamlObjectWriter(reader.SchemaContext, settings);
            XamlServices.Transform(reader, writer);
            return writer.Result;
        }

        public void CustomConversionApplied(object root)
        {
            var dicts = (Dictionaries<int, string>)root;
            Assert.AreEqual(10, dicts.CustomConvertingDictionary.Keys.First());
        }

        public void DeferredAddValidator(object root)
        {
            var dicts = (Dictionaries<int, string>)root;
            Assert.AreEqual("B", dicts.MyDictionary[1]);
            Assert.AreEqual("2", dicts.MyDictionary[2]);
            Assert.AreEqual("1", dicts.CustomConvertingDictionary[11]);
            Assert.AreEqual("2", dicts.CustomConvertingDictionary[10]);
        }
    }
}