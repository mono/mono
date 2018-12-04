using System;
using System.Collections.Generic;
using System.Text;
using DRT;
using System.Xaml;
using DrtXaml.XamlTestFramework;
using System.Xml;
using System.IO;
using System.Windows.Controls;
using System.Windows;
using Test.Elements;
using System.Windows.Markup;
using System.Windows.Documents;
using BamlTestClasses40;
using System.Windows.Media;
using System.Windows.Shapes;

namespace DrtXaml.Tests
{
    [TestClass]
    sealed class WpfUsageTests : XamlTestSuite
    {
        public WpfUsageTests()
            : base("WpfUsageTests")
        {
        }

        public override DrtTest[] PrepareTests()
        {
            DrtTest[] tests = DrtTestFinder.FindTests(this);
            return tests;
        }

        public override object StandardXamlLoader(string xamlString)
        {
            XamlSchemaContext xsc = System.Windows.Markup.XamlReader.GetWpfSchemaContext();
            XamlNodeList xamlNodeList = new XamlNodeList(xsc);
            XamlServices.Transform(new XamlXmlReader(XmlReader.Create(new StringReader(xamlString)), xsc), xamlNodeList.Writer);

            NodeListValidator.Validate(xamlNodeList);

            System.Xaml.XamlReader reader = xamlNodeList.GetReader();
            object root = System.Windows.Markup.XamlReader.Load(reader);
            if (root == null)
                throw new NullReferenceException("Load returned null Root");
            return root;
        }

        // =============================================

        [TestXaml]
        const string HelloWorld_XAML =
@"<Button
    xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'
   Content='Hello World' />";

        [TestXaml]
        const string Smoke_XAML =
@"<StackPanel Margin='50,50,50,50'
        xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'
        xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'
        xmlns:sys='clr-namespace:System;assembly=mscorlib'
        xmlns:foo='foobar:spam'
        xmlns:nc='notevenacolon'>
    <FrameworkElement.Resources>
      <Brush x:Key='_redBrush'>  Red</Brush>
      <Button x:Key='_btn' Content='Test' />
    </FrameworkElement.Resources>
    <Button  Width='  200  ' Height='50 '>
       <sys:String x:Name='_okText'>   OK   There  </sys:String>
       <Button.Background>
           <SolidColorBrush Color='   Cyan  '/>
       </Button.Background>
    </Button>
</StackPanel>";

        [TestXaml]
        const string x_SetterWithSpace =
@"<StackPanel Margin='50,50,50,50'
        xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'
        xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml' >
<StackPanel.Resources>
    <Style TargetType='Button'>
        <Setter Property='Background ' Value='Green'/>
    </Style>
</StackPanel.Resources>
    <Button Height='70' Width='200' Foreground='White'>
        <Button.Background>Red</Button.Background>
        Press Me
    </Button>
</StackPanel>";

        [TestXaml]
        const string x_NameSimple_XAML =
@"<StackPanel
       xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'
       xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
    <Button x:Name='_btn' Content='Press'/>
  </StackPanel>";

        [TestXaml]
        const string ResourceDictionary_XAML =
@"<StackPanel
    xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'
    xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
    <StackPanel.Resources>
        <Button x:Key='_rButton1' Content='r1'/>
        <Brush x:Key='_rbrush'>Blue</Brush>
    </StackPanel.Resources>
    <Button x:Name='_btn' Content='Press'/>
  </StackPanel>";

        [TestXaml]
        const string SimpleMarkupExtension_XAML =
@"<Page
	xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'
	xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml' >
<Button Height='40' Width='300' Content='Press'>
  <Button.Background>
 	<SolidColorBrush Color='{x:Static Colors.Pink}'/>
  </Button.Background>
</Button>
</Page>";

        [TestXaml]
        const string ExplicitBrush_XAML =
@"<Page Margin='50,50,50,50'
        xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' >
    <Button Content='OK' Width='200' Height='50'>
    <Button.Background><SolidColorBrush>Cyan</SolidColorBrush></Button.Background>
  </Button>
</Page>";

        [TestXaml]
        const string NestedBindingMarkupExtension_XAML =
@"<StackPanel
      xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'
      xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
        <Button Background='Red' Content='{Binding RelativeSource={RelativeSource self}, Path=Background}'/>
  </StackPanel>";

        [TestXaml]
        const string AttachablePropertyFromBaseClass_XAML =
@"<StackPanel Margin='50,50,50,50'
        xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'
      xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'
>
  <StackPanel.Children>
    <Button Canvas.ZIndex='2' Background='{x:Static Brushes.CadetBlue}'
                       Content='OK' Width='200' Height='50'/>
    <Button Width='200' Height='40'>
        Button          Text
    </Button>
  </StackPanel.Children>
</StackPanel>";

        [TestXaml]
        const string AttachableDp0_XAML = @"
<StackPanel Margin='50,50,50,50'
        xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'
        xmlns:elem='clr-namespace:Test.Elements;assembly=XamlTestClasses' >
    <Button Width='300' Height='40'>
        <elem:HasAttachableDp.MyDp>Anything Type Object</elem:HasAttachableDp.MyDp>
    </Button>
</StackPanel>";

        [TestXaml]
        const string AttachableDp1_XAML = @"
<StackPanel
    xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'
    xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'
    xmlns:elem='clr-namespace:Test.Elements;assembly=XamlTestClasses'
    Width='400' Height='300'>
    <Grid x:Name='LayoutRoot' Background='White'>
        <TextBlock Text='1234567...' elem:HasAttachableDp.MyDp='123'/>
    </Grid>
</StackPanel>";

        [TestXaml]
        const string AttachablePreCtorBinding = @"
<Grid Background='Cyan' Name='topGrid'
        xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'
        xmlns:sys='clr-namespace:System;assembly=mscorlib'
        xmlns:tst='clr-namespace:Test.Elements;assembly=XamlTestClasses'
        xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml' >


<Grid.Tag> <sys:Int32>2</sys:Int32></Grid.Tag>
<Grid.RowDefinitions>
<RowDefinition/>
<RowDefinition/>
<RowDefinition/>
</Grid.RowDefinitions>
    <Button Grid.Row='0' Height='70' Width='200' Foreground='White'>
        <Button.Background>Red</Button.Background>
        Press Me
    </Button>
    <Button  Grid.Row='{Binding ElementName=topGrid, Path=Tag }'
            Height='70' Width='200' Foreground='Yellow'>
        <Button.Background>Brown</Button.Background>
       button 2
    </Button>
</Grid>
";

        [TestXaml]
        const string MarkupExtensionEscapesWPF = @"
<Page
    xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'
    xmlns:sys='clr-namespace:System;assembly=mscorlib'
    xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'>
<StackPanel Width='200' >
  <StackPanel.Resources>
    <sys:String x:Key='greenString'>green</sys:String>
    <SolidColorBrush x:Key='gray'>gray</SolidColorBrush>
    <SolidColorBrush x:Key='green'>Green</SolidColorBrush>
    <SolidColorBrush x:Key='{}{red}'>red</SolidColorBrush>
    <SolidColorBrush x:Key='{}{blue}{blue}'>blue</SolidColorBrush>
  </StackPanel.Resources>

<!-- Simple name  -->
  <Button Content='Gray1' Background='{StaticResource gray}' />
  <Button Content='Gray2' Background='{StaticResource ResourceKey=gray}' />

<!-- Simple name in quotes -->
  <Button Content='Gray3' Background='{StaticResource &apos;gray&apos;}' />
  <Button Content='Gray4' Background='{StaticResource ResourceKey=&apos;gray&apos;}' />

<Button Content='-- space --'/>

<!-- Name that looks like a ME; w/ escaped braces -->
  <Button Content='Red1' Background='{StaticResource \{red\}}' />
  <Button Content='Red2' Background='{StaticResource ResourceKey=\{red\}}' />

<!-- Name that looks like a ME; w/ escaped braces -->
<!-- inside of quotes-->
  <Button Content='Red3' Background='{StaticResource &apos;\{red\}&apos;}' />
  <Button Content='Red4' Background='{StaticResource ResourceKey=&apos;\{red\}&apos;}' />

<!-- Name that looks like a ME; w/ escaped braces -->
<!-- inside of quotes only esc the first one -->
  <Button Content='Red5' Background='{StaticResource &apos;\{red}&apos;}' />
  <Button Content='Red6' Background='{StaticResource ResourceKey=&apos;\{red}&apos;}' />

<!-- THESE DON'T WORK in 3.5 -->
<!--
  <Button Content='Red7' Background='{StaticResource {}{red}}' />
  <Button Content='Red4' Background='{StaticResource ResourceKey={}{red} }' />
  <Button Content='Red7' Background='{StaticResource &apos;{}{red}&apos; }' />
  <Button Content='Red4' Background='{StaticResource ResourceKey=&apos;{}{red}&apos; }' />
-->

  <Button Content='{}{This is a spacer}'/>
  <Button Content='Blue' Background='{StaticResource \{blue\}\{blue\}}' />
  <Button Content='\{this is a spacer\}'/>

<!-- Nested ME  -->
  <Button Content='Green1' Background='{StaticResource {StaticResource greenString} }' />
  <Button Content='Green2' Background='{StaticResource ResourceKey={StaticResource greenString} }' />

<!-- Nested ME, inside of quotes  -->
  <Button Content='Green3' Background='{StaticResource &apos;{StaticResource greenString}&apos; }' />
  <Button Content='Green4' Background='{StaticResource ResourceKey=&apos;{StaticResource greenString}&apos; }' />

<!-- THESE DON'T WORK in 3.5 -->
<!--
  <Button Content='Green5' Background='{StaticResource &apos;{StaticResource &apos;greenString&apos;}&apos; }' />
  <Button Content='Green6' Background='{StaticResource ResourceKey=&apos;{StaticResource &apos;greenString&apos;}&apos; }' />
-->

</StackPanel>
</Page>
";

        [TestXaml]
        [TestExpectedException(typeof(System.Windows.Markup.XamlParseException))]
        const string nonAttachableProperty_Fail = @"
<Canvas xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'>
    <Border Page.Foreground='Green' />
</Canvas>
";

        [TestXaml]
        const string dualModeProperty = @"
<Canvas xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'>
    <TextBlock FontWeight='Bold'>this is text</TextBlock>
    <Border TextBlock.FontWeight='Bold'/>
</Canvas>
";

        [TestMethod]
        public void DependencyPropertyWithNoClrProperty()
        {
            XamlSchemaContext xsc = System.Windows.Markup.XamlReader.GetWpfSchemaContext();
            XamlType xType = xsc.GetXamlType(typeof(XamlTestClasses.BamlTestType));
            XamlMember xMember = xType.GetMember("DpWithNoClrProperty");
            Assert.IsNotNull(xMember);
            Assert.AreEqual(xMember.Type, xType);
        }

        [TestMethod]
        public void RoutedEventWithNoRoutedEventProperty()
        {
            XamlSchemaContext xsc = System.Windows.Markup.XamlReader.GetWpfSchemaContext();
            XamlType xType = xsc.GetXamlType(typeof(XamlTestClasses.BamlTestType));
            XamlMember xMember = xType.GetMember("RoutedEventWithNoClrEvent");
            Assert.IsNotNull(xMember);
            Assert.IsTrue(xMember.IsEvent);
            Assert.AreEqual(xMember.TypeConverter.Name, "EventConverter");
        }

        [TestMethod]
        public void DependencyPropertyTypeMistmatch()
        {
            XamlSchemaContext xsc = System.Windows.Markup.XamlReader.GetWpfSchemaContext();
            XamlType xType = xsc.GetXamlType(typeof(XamlTestClasses.BamlTestType));
            XamlMember xMember = xType.GetMember("TypeMismatch");
            Assert.IsNotNull(xMember);
            Assert.AreEqual(xMember.TypeConverter, XamlLanguage.Int32.TypeConverter);
        }

        [TestMethod]
        public void XmlSpaceDefault()
        {
            string x = @"<StackPanel xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
    <StackPanel/>
</StackPanel>";
            var o = System.Windows.Markup.XamlReader.Load(new XmlTextReader(new StringReader(x)));
            StackPanel s1 = o as StackPanel;
            if (!XmlAttributeProperties.GetXmlSpace((DependencyObject)s1).Equals("default"))
            {
                throw new Exception(string.Format("Expected the outer StackPanel's xml:space to be 'default' in:\n\n{0}", x));
            }
            StackPanel s2 = s1.Children[0] as StackPanel;
            if (!XmlAttributeProperties.GetXmlSpace((DependencyObject)s2).Equals("default"))
            {
                throw new Exception(string.Format("Expected the inner StackPanel's xml:space to be 'default' in:\n\n{0}", x));
            }
        }

        [TestMethod]
        public void XmlSpacePreserve1()
        {
            string x = @"<StackPanel xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml' xml:space='preserve'>
    <StackPanel/>
</StackPanel>";
            var o = System.Windows.Markup.XamlReader.Load(new XmlTextReader(new StringReader(x)));
            StackPanel s1 = o as StackPanel;
            if (!XmlAttributeProperties.GetXmlSpace((DependencyObject)s1).Equals("preserve"))
            {
                throw new Exception(string.Format("Expected outer StackPanel's xml:space to be 'preserve' in:\n\n{0}", x));
            }
            StackPanel s2 = s1.Children[0] as StackPanel;
            if (!XmlAttributeProperties.GetXmlSpace((DependencyObject)s2).Equals("default"))
            {
                throw new Exception(string.Format("Expected inner StackPanel's xml:space to be 'default' in:\n\n{0}", x));
            }
        }

        [TestMethod]
        public void XmlSpacePreserve2()
        {
            string x = @"<StackPanel xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml' xml:space='preserve'>
    <StackPanel xml:space='preserve'/>
</StackPanel>";
            var o = System.Windows.Markup.XamlReader.Load(new XmlTextReader(new StringReader(x)));
            StackPanel s1 = o as StackPanel;
            if (!XmlAttributeProperties.GetXmlSpace((DependencyObject)s1).Equals("preserve"))
            {
                throw new Exception(string.Format("Expected outer StackPanel's xml:space to be 'preserve' in:\n\n{0}", x));
            }
            StackPanel s2 = s1.Children[0] as StackPanel;
            if (!XmlAttributeProperties.GetXmlSpace((DependencyObject)s2).Equals("preserve"))
            {
                throw new Exception(string.Format("Expected inner StackPanel's xml:space to be 'preserve' in:\n\n{0}", x));
            }
        }

        [TestMethod]
        public void PreferPublicAttachableSetter()
        {
            string xaml = @"<Element AOP.Foo='2' xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses' />";
            var o = XamlServices.Parse(xaml);
        }

        [TestMethod]
        public void AllAttachableMembersPreferPublics()
        {
            var xsc = new XamlSchemaContext();
            var AOP = xsc.GetXamlType(typeof(AOP));
            var members = AOP.GetAllAttachableMembers();
            Assert.AreEqual(1, members.Count);
            var enumerator = members.GetEnumerator();
            enumerator.MoveNext();
            XamlMember member = enumerator.Current;
            Assert.IsTrue(member.IsWriteOnly);
        }

        const string ElementXaml = "<Element xmlns='http://elements' />";

        [TestMethod]
        public void TypeMapperWithNamespaceEntry()
        {
            XamlTypeMapper typeMapper = new XamlTypeMapper(new string[0], new NamespaceMapEntry[] {
                new NamespaceMapEntry("http://elements", "XamlTestClasses", "Test.Elements")
            });
            ParserContext pc = new ParserContext { XamlTypeMapper = typeMapper };
            
            object result = System.Windows.Markup.XamlReader.Parse(ElementXaml, pc);
            Assert.IsInstanceOfType(typeof(Element), result);

            MemoryStream stream = new MemoryStream(Encoding.Default.GetBytes(ElementXaml));
            result = new System.Windows.Markup.XamlReader().LoadAsync(stream, pc);
            Assert.IsInstanceOfType(typeof(Element), result);
        }

        [TestMethod]
        public void TypeMapperWithMappingPI()
        {
            XamlTypeMapper typeMapper = new XamlTypeMapper(new string[0]);
            typeMapper.AddMappingProcessingInstruction("http://elements", "Test.Elements", "XamlTestClasses");
            ParserContext pc = new ParserContext { XamlTypeMapper = typeMapper };
            object result = System.Windows.Markup.XamlReader.Parse(ElementXaml, pc);
            Assert.IsInstanceOfType(typeof(Element), result);
        }

        const string InternalXaml = "<InternalElement xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses' />";

        [TestMethod]
        public void TypeMapperWithAllowInternal()
        {
            XamlTypeMapper typeMapper = new TypeMapperAllowingInternals();
            ParserContext pc = new ParserContext { XamlTypeMapper = typeMapper };
            object result = System.Windows.Markup.XamlReader.Parse(InternalXaml, pc);
            Assert.IsInstanceOfType(typeof(Element).Assembly.GetType("Test.Elements.InternalElement"), result);
        }

        private class TypeMapperAllowingInternals : XamlTypeMapper
        {
            public TypeMapperAllowingInternals()
                : base(new string[0])
            {
            }

            protected override bool AllowInternalType(Type type)
            {
                return true;
            }
        }

        [TestMethod]
        public void TestResourceDictionaryNameScope()
        {
            XamlSchemaContext xsc = System.Windows.Markup.XamlReader.GetWpfSchemaContext();
            Assert.IsFalse(xsc.GetXamlType(typeof(ResourceDictionary)).IsNameScope);
            Assert.IsFalse(xsc.GetXamlType(typeof(CustomResourceDictionary)).IsNameScope);
            Assert.IsFalse(xsc.GetXamlType(typeof(CustomResourceDictionaryWithNameScopeButNoRegisterName)).IsNameScope);
            Assert.IsTrue(xsc.GetXamlType(typeof(CustomResourceDictionaryWithExplicitNameScope)).IsNameScope);
            Assert.IsTrue(xsc.GetXamlType(typeof(CustomResourceDictionaryWithImplicitNameScope)).IsNameScope);
        }

        [TestMethod]
        public void SerializeWpfTypes()
        {
            string xaml = @"<DockPanel
 xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
 xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
    <TextBlock>
        <Bold />
        <Italic />
    </TextBlock>
</DockPanel>";
            var o = System.Windows.Markup.XamlReader.Parse(xaml);

            string generated = WpfSave(o);
            string expected = 
@"<DockPanel xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"">
  <TextBlock>
    <Bold /> <Italic /></TextBlock>
</DockPanel>";
            Assert.AreEqual(expected, generated);
        }

        [TestMethod]
        public void Stuff()
        {
            TextBlock Target = new TextBlock();
            Target.Inlines.Add("Hello");
            Target.Inlines.Add(new Bold());
            string s = WpfSave(Target);
        }

        public string WpfSave(object o)
        {
            var context = System.Windows.Markup.XamlReader.GetWpfSchemaContext();
            var sb = new StringBuilder();
            var reader = new XamlObjectReader(o, context, new XamlObjectReaderSettings { RequireExplicitContentVisibility = true });
            var writer = new XamlXmlWriter(XmlWriter.Create(sb, new XmlWriterSettings { Indent = true, OmitXmlDeclaration = true }), reader.SchemaContext);
            XamlServices.Transform(reader, writer);
            return sb.ToString();
        }

        [TestMethod]
        public void XmlNamespaceMaps()
        {
            const string xaml =
@"<Window xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'
   xmlns:test='http://test'
   test:HasAttachableDp.MyDp='Bar'
   Content='{Binding (test:HasAttachableDp.MyProperty), RelativeSource={RelativeSource Self}}' />";
            XamlTypeMapper mapper = new XamlTypeMapper(new string[0], new NamespaceMapEntry[]
            {
                new NamespaceMapEntry
                {
                    XmlNamespace = "http://test",
                    ClrNamespace = "Test.Elements",
                    AssemblyName = "XamlTestClasses"
                }
            });
            ParserContext pc = new ParserContext { XamlTypeMapper = mapper };
            Window window = (Window)System.Windows.Markup.XamlReader.Parse(xaml, pc);
            window.Show();
            object content = window.Content;
            window.Close();
            Assert.AreEqual("Bar", content);
        }
        
        [TestMethod]
        public void StaticResource_Style_ResourceOnLocal_NoBase()
        {
            string xaml = @"
<Border xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
    <Border.Resources>
        <Style x:Key='b' TargetType='Button'>
            <Style.Resources>
                <x:String x:Key='s'>test</x:String>
            </Style.Resources>
            <Setter Property='Content' Value='{StaticResource s}'/>
        </Style>
    </Border.Resources>
    <Button Style='{StaticResource b}'/>
</Border>";
            Border border = (Border)XamlServices.Parse(xaml);
            Button button = (Button)border.Child;
            string content = (string)button.Content;
            Assert.AreEqual(content, "test");
        }

        [TestMethod]
        public void StaticResource_Style_ResourceOnLocal_WithBase()
        {
            string xaml = @"
<Border xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
    <Border.Resources>
        <Style x:Key='a'>
            <Style.Resources>
                <x:String x:Key='s'>test1</x:String>
            </Style.Resources>
        </Style>
        <Style x:Key='b' TargetType='Button' BasedOn='{StaticResource a}'>
            <Style.Resources>
                <x:String x:Key='s'>test2</x:String>
            </Style.Resources>
            <Setter Property='Content' Value='{StaticResource s}'/>
        </Style>
    </Border.Resources>
    <Button Style='{StaticResource b}'/>
</Border>";
            Border border = (Border)XamlServices.Parse(xaml);
            Button button = (Button)border.Child;
            string content = (string)button.Content;
            Assert.AreEqual(content, "test2");
        }

        [TestMethod]
        public void StaticResource_Style_ResourceOnBase_NoLocal()
        {
            string xaml = @"
<Border xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
    <Border.Resources>
        <Style x:Key='a'>
            <Style.Resources>
                <x:String x:Key='s'>test</x:String>
            </Style.Resources>
        </Style>
        <Style x:Key='b' TargetType='Button' BasedOn='{StaticResource a}'>
            <Setter Property='Content' Value='{StaticResource s}'/>
        </Style>
    </Border.Resources>
    <Button Style='{StaticResource b}'/>
</Border>";
            Border border = (Border)XamlServices.Parse(xaml);
            Button button = (Button)border.Child;
            string content = (string)button.Content;
            Assert.AreEqual(content, "test");
        }

        [TestMethod]
        public void StaticResource_Style_ResourceOnBase_OneBase()
        {
            string xaml = @"
<Border xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
    <Border.Resources>
        <Style x:Key='a'>
            <Style.Resources>
                <x:String x:Key='s'>test1</x:String>
            </Style.Resources>
        </Style>
        <Style x:Key='b' TargetType='Button' BasedOn='{StaticResource a}'>
            <Style.Resources>
                <x:String x:Key='t'>test2</x:String>
            </Style.Resources>
            <Setter Property='Content' Value='{StaticResource s}'/>
        </Style>
    </Border.Resources>
    <Button Style='{StaticResource b}'/>
</Border>";
            Border border = (Border)XamlServices.Parse(xaml);
            Button button = (Button)border.Child;
            string content = (string)button.Content;
            Assert.AreEqual(content, "test1");
        }

        [TestMethod]
        public void StaticResource_Style_ResourceOnBase_MultipleBases()
        {
            string xaml = @"
<Border xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
    <Border.Resources>
        <Style x:Key='a'>
            <Style.Resources>
                <x:String x:Key='mexico'>test1</x:String>
            </Style.Resources>
        </Style>
        <Style x:Key='b' BasedOn='{StaticResource a}'/>
        <Style x:Key='c' TargetType='Button' BasedOn='{StaticResource b}'>
            <Style.Resources>
                <x:String x:Key='t'>test2</x:String>
            </Style.Resources>
            <Setter Property='Content' Value='{StaticResource mexico}'/>
        </Style>
    </Border.Resources>
    <Button Style='{StaticResource c}'/>
</Border>";
            Border border = (Border)XamlServices.Parse(xaml);
            Button button = (Button)border.Child;
            string content = (string)button.Content;
            Assert.AreEqual(content, "test1");
        }

        [TestMethod]
        public void ResourceResolutionForwardReference1()
        {
            bool caughtException = false;
            try
            {
                var rr = new ResourceResolutionForwardReference1();
            }
            catch (System.Windows.Markup.XamlParseException)
            {
                caughtException = true;
            }
            DRT.Assert(caughtException, "Static Resource forward reference did not throw");
        }

        [TestMethod]
        public void ResourceResolutionForwardReference2()
        {
            var rr = new ResourceResolutionForwardReference2();
            rr.Measure(new Size(900.0, 900.0));

            ContentControl contentControl;
            Rectangle rect;
            ContentControl innerContentControl;
            SolidColorBrush brush;

            contentControl = rr.FindName("_contentControl1") as ContentControl;
            rect = contentControl.Content as Rectangle;
            brush = rect.Fill as SolidColorBrush;
            DRT.AssertEqual(Brushes.Cyan.Color, brush.Color, "Incorrect resource resolution (likely forward)");

            contentControl = rr.FindName("_contentControl2") as ContentControl;
            innerContentControl = contentControl.Template.FindName("_innerContentControl2", contentControl) as ContentControl;
            rect = innerContentControl.Content as Rectangle;
            brush = rect.Fill as SolidColorBrush;
            DRT.AssertEqual(Brushes.Cyan.Color, brush.Color, "Incorrect resource resolution (likely forward)");
        }

        [TestMethod]
        public void ResourceResolutionWithDictionaryOverWriting()
        {
            var rr = new ResourceResolution_OverWrite();
            rr.Measure(new Size(900.0, 900.0));

            Button button = rr.FindName("_button0") as Button;
            DRT.AssertEqual("Test0", button.Content, "Resource overwritten");

            button = rr.FindName("_button1") as Button;
            TextBlock textBlock = button.Content as TextBlock;
            DRT.AssertEqual("Test1", textBlock.Text, "Resource overwritten");

            button = rr.FindName("_button2") as Button;
            Button button2 = button.Template.FindName("_templateButton2", button) as Button;
            DRT.AssertEqual("Test2", button2.Content, "Resource overwritten");

            button = rr.FindName("_button3") as Button;
            button2 = button.Template.FindName("_templateButton3", button) as Button;
            textBlock = button2.Content as TextBlock;
            DRT.AssertEqual("Test3", textBlock.Text, "Resource overwritten");

            button = rr.FindName("_button4") as Button;
            button2 = button.Template.FindName("_templateButton4", button) as Button;
            DRT.AssertEqual("Test4", button2.Content, "Resource overwritten");
        }

        [TestMethod]
        public void ResourceResolutionWithDictionaryInsertions()
        {
            var rr = new ResourceResolution_Insert();
            rr.Measure(new Size(900.0, 900.0));

            Button button = rr.FindName("_button0") as Button;
            DRT.AssertEqual("Test0", button.Content, "Inserted Resource interferred with correct resource lookup");

            button = rr.FindName("_button1") as Button;
            TextBlock textBlock = button.Content as TextBlock;
            DRT.AssertEqual("Test1", textBlock.Text, "Inserted Resource interferred with correct resource lookup");

            button = rr.FindName("_button2") as Button;
            Button button2 = button.Template.FindName("_templateButton2", button) as Button;
            DRT.AssertEqual("Test2", button2.Content, "Inserted Resource interferred with correct resource lookup");

            button = rr.FindName("_button3") as Button;
            button2 = button.Template.FindName("_templateButton3", button) as Button;
            textBlock = button2.Content as TextBlock;
            DRT.AssertEqual("Test3", textBlock.Text, "Inserted Resource interferred with correct resource lookup");

            button = rr.FindName("_button4") as Button;
            button2 = button.Template.FindName("_templateButton4", button) as Button;
            DRT.AssertEqual("Test4", button2.Content, "Inserted Resource interferred with correct resource lookup");
        }

        [TestMethod]
        public void TemplateResourceResolution()
        {
            var rr = new ResourceResolution_DeferredDictionary();
            rr.Measure(new Size(900.0, 900.0));

            SolidColorBrush brush;

            brush = FindTemplatedFillBrush(rr, "_templateParent1", "_colorRectangle1");
            DRT.AssertEqual(Brushes.Cyan.Color, brush.Color, "Resource inside the template was not found");

            brush = FindTemplatedFillBrush(rr, "_templateParent2", "_colorRectangle2");
            DRT.AssertEqual(Brushes.Cyan.Color, brush.Color, "Resource inside the template was not found");

            brush = FindTemplatedFillBrush(rr, "_templateParent3", "_colorRectangle3");
            DRT.AssertEqual(Brushes.Cyan.Color, brush.Color, "Resource inside the template was not found");

            brush = FindTemplatedFillBrush(rr, "_templateParent4", "_colorRectangle4");
            DRT.AssertEqual(Brushes.Cyan.Color, brush.Color, "Resource inside the template was not found");
        }

        private SolidColorBrush FindTemplatedFillBrush(FrameworkElement element, string parentName, string templateElementName)
        {
            ContentControl templateParent = element.FindName(parentName) as ContentControl;
            ControlTemplate controlTemplate = templateParent.Template;
            ContentControl rectContentControl = controlTemplate.FindName(templateElementName, templateParent) as ContentControl;
            Rectangle rect = rectContentControl.Content as Rectangle;
            return (SolidColorBrush)rect.Fill;
        }

        [TestMethod]
        public void TemplateDPSetOrder()
        {
            var tempDPSetOrder1 = new TempDPSetOrder1();
            tempDPSetOrder1.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
            var order = new List<Property>() {
                Property.IsSynchronizedWithCurrentItem,
                Property.SelectedIndex,
                Property.ItemsSource,
                Property.IsSynchronizedWithCurrentItem,
                Property.SelectedIndex,
                Property.ItemsSource
            };
            Assert.IsTrue(UsefulEquals(order, TempDPSetOrder1ListBox.Order));
        }

        public static bool UsefulEquals(List<Property> list1, List<Property> list2)
        {
            using (IEnumerator<Property> iEnumerator1 = list1.GetEnumerator())
            using (IEnumerator<Property> iEnumerator2 = list2.GetEnumerator())
            {
                bool moveNext1, moveNext2;
                do
                {
                    moveNext1 = iEnumerator1.MoveNext();
                    moveNext2 = iEnumerator2.MoveNext();
                    if (moveNext1 != moveNext2)
                    {
                        return false;
                    }
                    if (!moveNext1)
                    {
                        return true;
                    }
                    if (!iEnumerator1.Current.Equals(iEnumerator2.Current))
                    {
                        return false;
                    }
                }
                while (true);
            }
        }

        [TestMethod]
        public void WpfObfuscation()
        {
            var test = new WpfObfus2();
            test.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
        }
    }
}
