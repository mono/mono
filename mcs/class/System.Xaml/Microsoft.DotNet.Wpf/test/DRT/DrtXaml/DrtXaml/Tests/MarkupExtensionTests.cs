using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xaml;
using System.Xml;
using DrtXaml.XamlTestFramework;
using DRT;
using Test.Elements;

namespace DrtXaml.Tests
{
    [TestClass]
    sealed class MarkupExtensionTests : XamlTestSuite
    {
        public MarkupExtensionTests()
            : base("MarkupExtensionTests")
        {
        }

        public override DrtTest[] PrepareTests()
        {
            DrtTest[] tests = DrtTestFinder.FindTests(this);
            return tests;
        }

        // =============================================

        //        [TestXaml]
        //        const string AMEET_XAML =
        //@"<InitializableElementHolder
        //    xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses'>
        //    <InitializableElementHolder.TopDown>
        //        <TopDown Prop1='99' Prop2='42'/>
        //    </InitializableElementHolder.TopDown>
        //    <InitializableElementHolder.TopDownTurnedOff>
        //        <TopDownTurnedOff Prop1='99' Prop2='42'/>
        //    </InitializableElementHolder.TopDownTurnedOff>
        //</InitializableElementHolder>";

        [TestXaml]
        const string MarkupExtension0_XAML =
@"<ElementWithTitle
    xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses'
    Title='{SimpleMe}' />";

        [TestXaml]
        const string MarkupExtension1_XAML =
@"<ElementWithTitle
    xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses'
    Title='{SimpleMe FirstParam}' />";

        [TestXaml]
        const string MarkupExtension2_XAML =
@"<ElementWithTitle
    xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses'
    Title='{SimpleMe SecondString, FirstParam}' />";

        [TestXaml]
        const string MarkupExtension3_XAML =
@"<ElementWithTitle
    xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses'
    Title='{SimpleMe SecondString, FirstParam, Display=disp}' />";

        [TestXaml]
        const string MarkupExtension4_XAML =
@"<ElementWithTitle
    xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses'
    Title='{SimpleMe Display=disp }' />";

        [TestXaml]
        const string MarkupExtension5_XAML =
@"<ElementWithTitle
    xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses'
    Title='{SimpleMe SecondString, Display= foo  }' />";

        [TestXaml]
        const string MarkupExtension6_XAML =
@"<ElementWithTitle
    xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses'
    Title='{SimpleMe {SimpleMe foobar}, Display={SimpleMe {SimpleMe Nested} }  }' />";

        [TestXaml, TestTreeValidator("MarkupExtensionEscapes_TreeValidator")]
        const string MarkupExtensionEscapes = @"
<ElementResourceHolder
      xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses'
      xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
  <ElementResourceHolder.Resources>
    <ColorElement ColorName='gray'   x:Key='gray' />
    <ColorElement ColorName='red'    x:Key='{}{red}' />
    <ColorElement ColorName='blue'   x:Key='{}{blue}{blue}' />
    <x:String x:Key='greenString'>green</x:String>
    <ColorElement ColorName='green'  x:Key='green' />
  </ElementResourceHolder.Resources>

  <ElementResourceHolder.Children>
<!-- Simple name  -->
  <HoldsOneElement Element='{ResourceLookup gray}' />
  <HoldsOneElement Element='{ResourceLookup ResourceKey=gray}' />

<!-- Simple name in quotes -->
  <HoldsOneElement Element='{ResourceLookup &apos;gray&apos;}' />
  <HoldsOneElement Element='{ResourceLookup ResourceKey=&apos;gray&apos;}' />

  <ElementWithTitle Title='-- space --'/>

<!-- Name that looks like a ME; w/ escaped braces -->
  <HoldsOneElement Element='{ResourceLookup \{red\}}' />
  <HoldsOneElement Element='{ResourceLookup ResourceKey=\{red\}}' />

<!-- Name that looks like a ME; w/ escaped braces -->
<!-- inside of quotes-->
  <HoldsOneElement Element='{ResourceLookup &apos;\{red\}&apos;}' />
  <HoldsOneElement Element='{ResourceLookup ResourceKey=&apos;\{red\}&apos;}' />

<!-- Name that looks like a ME; w/ escaped braces -->
<!-- inside of quotes only esc the first one -->
  <HoldsOneElement Element='{ResourceLookup &apos;\{red}&apos;}' />
  <HoldsOneElement Element='{ResourceLookup ResourceKey=&apos;\{red}&apos;}' />

  <ElementWithTitle Title='{}{This is a spacer}'/>

  <HoldsOneElement Element='{ResourceLookup \{blue\}\{blue\}}' />

  <ElementWithTitle Title='\{this is a spacer\}'/>

<!-- Nested ME  -->
  <HoldsOneElement Element='{ResourceLookup {ResourceLookup greenString} }' />
  <HoldsOneElement Element='{ResourceLookup ResourceKey={ResourceLookup greenString} }' />

<!-- Nested ME, inside of quotes  -->
  <HoldsOneElement Element='{ResourceLookup &apos;{ResourceLookup greenString}&apos; }' />
  <HoldsOneElement Element='{ResourceLookup ResourceKey=&apos;{ResourceLookup greenString}&apos; }' />

  </ElementResourceHolder.Children>
</ElementResourceHolder>
";
        public void MarkupExtensionEscapes_TreeValidator(object o)
        {
            var root = (ElementResourceHolder)o;
            var dict = (ElementResourceDictionary)root.Resources;
            var list = (ElementChildren)root.Children;

            ColorElement gray = (ColorElement)dict["gray"];
            Assert.IsNotNull(gray, "gray is null");

            ColorElement red = (ColorElement)dict["{red}"];
            Assert.IsNotNull(red, "red is null");

            ColorElement blue = (ColorElement)dict["{blue}{blue}"];
            Assert.IsNotNull(blue, "blue is null");

            ColorElement green = (ColorElement)dict["green"];
            Assert.IsNotNull(green, "green is null");

            if (list.Count != 18)
            {
                Assert.Fail("Element count is not 18");
            }
            ElementWithTitle ewt;
            HoldsOneElement hoe;
            for (int i = 0; i < 4; i++)
            {
                hoe = (HoldsOneElement)list[i];
                Assert.AreSame(hoe.Element, gray);
            }
            ewt = (ElementWithTitle)list[4];
            Assert.Equals(ewt.Title, "-- space --");

            for (int i = 5; i <11; i++)
            {
                hoe = (HoldsOneElement)list[i];
                Assert.AreSame(hoe.Element, red);
            }

            ewt = (ElementWithTitle)list[11];
            Assert.Equals(ewt.Title, "{This is a spacer}");

            hoe = (HoldsOneElement)list[12];
            Assert.AreSame(hoe.Element, blue);

            ewt = (ElementWithTitle)list[13];
            Assert.Equals(ewt.Title, "\\{This is a spacer\\}");

            for (int i = 14; i < 18; i++)
            {
                hoe = (HoldsOneElement)list[i];
                Assert.AreSame(hoe.Element, green);
            }

        }

        [TestXaml, TestTreeValidator("ValidatePEonGenericME")]
        const string PropertyElementOnGenericME =
@"<Element10 
      xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses'
      xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
  <Element10.Element0>
    <GenericMarkup x:TypeArguments='Element'>
      <GenericMarkup.Value>
        <Element />
      </GenericMarkup.Value>
    </GenericMarkup>
  </Element10.Element0>
  <Element10.Element1>
    <GenericMarkup x:TypeArguments='Element'>
      <GenericMarkupExtension.Value>
        <Element />
      </GenericMarkupExtension.Value>
    </GenericMarkup>
  </Element10.Element1>
  <Element10.Element2>
    <GenericMarkupExtension x:TypeArguments='Element'>
      <GenericMarkup.Value>
        <Element />
      </GenericMarkup.Value>
    </GenericMarkupExtension>
  </Element10.Element2>
  <Element10.Element3>
    <GenericMarkupExtension x:TypeArguments='Element'>
      <GenericMarkupExtension.Value>
        <Element />
      </GenericMarkupExtension.Value>
    </GenericMarkupExtension>
  </Element10.Element3>
  <Element10.Element4>
    <GenericMarkupExtension x:TypeArguments='Element'>
      <GenericMarkupExtension.AP>
         <Element />
      </GenericMarkupExtension.AP>
    </GenericMarkupExtension>
  </Element10.Element4>
</Element10>";

        [TestXaml, TestAlternateXamlLoader("LocalAssemblyLoader"), TestTreeValidator("ValidatePEonGenericME")]
        static readonly string PropertyElementOnGenericME_LocalAssembly =
            PropertyElementOnGenericME.Replace(";assembly=XamlTestClasses", "");

        public object LocalAssemblyLoader(string xaml)
        {
            XamlXmlReaderSettings settings = new XamlXmlReaderSettings { LocalAssembly = typeof(Element).Assembly };
            XamlXmlReader reader = new XamlXmlReader(XmlReader.Create(new StringReader(xaml)), settings);
            return XamlServices.Load(reader);
        }

        public object WpfAssemblyLoader(string xaml)
        {
            return System.Windows.Markup.XamlReader.Load(XmlReader.Create(new StringReader(xaml)));
        }
        
        public void ValidatePEonGenericME(object o)
        {
            Element10 e10 = (Element10)o;
            Assert.IsInstanceOfType(typeof(Element), e10.Element0);
            Assert.IsInstanceOfType(typeof(Element), e10.Element1);
            Assert.IsInstanceOfType(typeof(Element), e10.Element2);
            Assert.IsInstanceOfType(typeof(Element), e10.Element3);
            Assert.IsNull(e10.Element4);
        }

        [TestXaml]
        const string BalancedBracesME = @"<StackPanel
        xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'
        xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml' >
    <StackPanel.Resources>
<Button x:Key='[{}]'/>
</StackPanel.Resources>
<ContentControl Content='{StaticResource [{}]}'/>
</StackPanel>";

        [TestXaml]
        const string BalancedBracesME2 = @"<StackPanel
        xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'
        xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml' >
    <StackPanel.Resources>
<Button x:Key='[{{}}]'/>
</StackPanel.Resources>
<ContentControl Content='{StaticResource [{{}}]}'/>
</StackPanel>";

        [TestXaml, TestExpectedException(typeof(XamlParseException))]
        const string UnbalancedBracesME = @"<StackPanel
        xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'
        xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml' >
    <StackPanel.Resources>
<Button x:Key='[{]'/>
</StackPanel.Resources>
<ContentControl Content='{StaticResource [{]}'/>
</StackPanel>";

        [TestXaml, TestExpectedException(typeof(XamlParseException))]
        const string UnbalancedBracesME2 = @"<StackPanel
        xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'
        xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml' >
    <StackPanel.Resources>
<Button x:Key='[}]'/>
</StackPanel.Resources>
<ContentControl Content='{StaticResource [}]}'/>
</StackPanel>";

        [TestXaml, TestAlternateXamlLoader("WpfAssemblyLoader")]
        const string NestedMEEscapeString1 = @"
<StackPanel  xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'
             xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
  <TextBox Name='Title' Text='Hello'/>
  <TextBlock Grid.Column='2' Background='red' Text='{Binding ElementName=Title, Path=Text, StringFormat={}{0} test(s)}' />
</StackPanel>";

        [TestXaml, TestAlternateXamlLoader("WpfAssemblyLoader")]
        const string NestedMEEscapeString2 = @"
<StackPanel  xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'
             xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
  <TextBox Name='Title' Text='Hello'/>
  <TextBlock Grid.Column='2' Background='red' Text='{Binding ElementName=Title, StringFormat={}{0} test(s), Path=Text}' />
</StackPanel>";

        [TestXaml, TestAlternateXamlLoader("WpfAssemblyLoader")]
        string NestedMEEscapeString3 = @"
<StackPanel  xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'
             xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
  <TextBox Name='Title' Text='Hello'/>
  <TextBlock Grid.Column='2' Background='red' Text='{Binding ElementName=Title, Path=Text, StringFormat=@{}{0} test(s)@}' />
</StackPanel>".Replace('@', '\"');

        [TestXaml, TestAlternateXamlLoader("WpfAssemblyLoader")]
        string NestedMEEscapeString4 = @"
<StackPanel  xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'
             xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
  <TextBox Name='Title' Text='Hello'/>
  <TextBlock Grid.Column='2' Background='red' Text='{Binding ElementName=Title, StringFormat=@{}{0} test(s)@, Path=Text}' />
</StackPanel>".Replace('@', '\"');

    }
}
