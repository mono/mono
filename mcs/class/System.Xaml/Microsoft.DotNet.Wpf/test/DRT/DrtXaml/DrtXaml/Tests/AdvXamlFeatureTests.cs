using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using DRT;
using System.Xaml;
using XAML3 = System.Windows.Markup;
using System.Reflection;
using System.IO;
using System.Xml;
using DrtXaml.XamlTestFramework;
using Test.Elements;
using System.Linq;

namespace DrtXaml.Tests
{
    [TestClass]
    sealed class AdvXamlFeatureTests : XamlTestSuite
    {
        public AdvXamlFeatureTests()
            : base("AdvXamlFeatureTests")
        {
        }

        public override DrtTest[] PrepareTests()
        {
            DrtTest[] tests = DrtTestFinder.FindTests(this);
            return tests;
        }

        // =============================================

        [TestXaml]
        const string FooAmbient =
@"<FooAmbient xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses'>
                <FooAmbient.AmbientTC>
                    test
                </FooAmbient.AmbientTC>
</FooAmbient>";

        [TestXaml, TestExpectedException(typeof(XamlObjectWriterException), typeof(ArgumentNullException))]
        const string AmbientMe = @"<AmbientMe xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses'>1</AmbientMe>";

        [TestXaml]
        const string Initialization0_XAML =
@"<InitializableElementHolder
    xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses'>
    <InitializableElementHolder.TopDown>
        <TopDown Prop1='99' Prop2='42'/>
    </InitializableElementHolder.TopDown>
    <InitializableElementHolder.TopDownTurnedOff>
        <TopDownTurnedOff Prop1='99' Prop2='42'/>
    </InitializableElementHolder.TopDownTurnedOff>
</InitializableElementHolder>";

        [TestXaml]
        const string Ambient0_XAML =
@"<HoldsOneElement
    xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses'>
    <HoldsOneElement.Element>
        <HasAmbientLabel Label='some text' />
    </HoldsOneElement.Element>
</HoldsOneElement>";

        [TestXaml, TestTreeValidator("Ambient1_Validator")]
        const string Ambient1_XAML =
@"<ElementListHolder   xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses'>
    <ElementListHolder.Elements>
        <HasAmbientLabel Label='thelabel'>
             <ColorElement>Red</ColorElement>
        </HasAmbientLabel>
    </ElementListHolder.Elements>
</ElementListHolder>";

        public void Ambient1_Validator(object o)
        {
            Test.Elements.ElementListHolder root = (Test.Elements.ElementListHolder)o;
            Test.Elements.HasAmbientLabel hal = (Test.Elements.HasAmbientLabel)root.Elements[0];
            Test.Elements.ColorElement ce = (Test.Elements.ColorElement)hal.Child;
            if (ce.ColorName != "Red-thelabel")
            {
                throw new Exception("TypeConverter use of Ambient property failed");
            }
        }

        [TestXaml, TestTreeValidator("Ambient2_Validator")]
        const string Ambient2_XAML =
@"<ElementResourceHolder   xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses'
                         xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'
                         xmlns:sys='clr-namespace:System;assembly=mscorlib' >
    <ElementResourceHolder.Resources>
        <HasAmbientLabel x:Key='_key' Label='streak'>
             <ColorElement>Blue</ColorElement>
        </HasAmbientLabel>
    </ElementResourceHolder.Resources>
    <HoldsOneElement  Element='{ResourceLookup _key}' />
</ElementResourceHolder>";

        public void Ambient2_Validator(object o)
        {
            Test.Elements.ElementResourceHolder root = (Test.Elements.ElementResourceHolder)o;
            Test.Elements.HoldsOneElement hoe = (Test.Elements.HoldsOneElement)root.Children[0];
            Test.Elements.HasAmbientLabel hal = (Test.Elements.HasAmbientLabel)hoe.Element;
            Test.Elements.ColorElement ce = (Test.Elements.ColorElement)hal.Child;
            if (ce.ColorName != "Blue-streak")
            {
                throw new Exception("TypeConverter use of Ambient property failed");
            }
        }

        [TestXaml, TestTreeValidator("Ambient3_Validator")]
        const string Ambient3_XAML =
@"<ElementResourceHolder   xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses'
                         xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'
                         xmlns:sys='clr-namespace:System;assembly=mscorlib' >
    <ElementResourceHolder.Resources>
        <sys:String x:Key='_str0'>The Zero String</sys:String>
    </ElementResourceHolder.Resources>
    <ElementResourceHolder>
      <ElementResourceHolder.Resources>
        <sys:String x:Key='_str1'>The String One</sys:String>
        <sys:String x:Key='_str2'>The String Two</sys:String>
      </ElementResourceHolder.Resources>
    <ElementWithTitle  Title='{ResourceLookup _str0}' />
  </ElementResourceHolder>
</ElementResourceHolder>";

        public void Ambient3_Validator(object o)
        {
            Test.Elements.ElementResourceHolder root = (Test.Elements.ElementResourceHolder)o;
            Test.Elements.ElementResourceHolder erh = (Test.Elements.ElementResourceHolder)root.Children[0];
            Test.Elements.ElementWithTitle ewt = (Test.Elements.ElementWithTitle)erh.Children[0];
            if (ewt.Title != "The Zero String")
            {
                throw new Exception("TypeConverter use of Ambient property failed");
            }
        }

        [TestXaml, TestTreeValidator("Ambient4_Validator")]
        const string Ambient4_XAML =
@"<ElementListHolder   xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses'>
    <ElementListHolder.Elements>
        <DerivedFromHasAmbientLabel Label='thelabel' Num='55'>
             <ColorElement2>Red</ColorElement2>
        </DerivedFromHasAmbientLabel>
    </ElementListHolder.Elements>
</ElementListHolder>";

        public void Ambient4_Validator(object o)
        {
            Test.Elements.ElementListHolder root = (Test.Elements.ElementListHolder)o;
            Test.Elements.DerivedFromHasAmbientLabel dfhal = (Test.Elements.DerivedFromHasAmbientLabel)root.Elements[0];
            Test.Elements.ColorElement ce = (Test.Elements.ColorElement)dfhal.Child;
            if (ce.ColorName != "Red-55")
            {
                throw new Exception("TypeConverter use of Ambient property failed");
            }
        }

        [TestXaml, TestTreeValidator("Ambient5_Validator")]
        const string Ambient5_XAML =
@"<AmbientType   xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses' Prop1='hello' Prop2='{AmbientTypeME}'/>";

        public void Ambient5_Validator(object o)
        {
            Test.Elements.AmbientType at = (Test.Elements.AmbientType)o;
            if (at.Prop1 != (String)at.Prop2)
            {
                throw new Exception("Type based lookup of Ambient Failed");
            }
        }

        [TestXaml]
        const string XData0_XAML =
@"<HoldsOneElement
    xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses'
    xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
    <HoldsOneElement.Element>
        <ElementWithTitle>
        <ElementWithTitle.Title>
          <x:XData>
 <Once xmlns=''>
    <Upon A='Time'>
    <There.Lived/>
    A Princess
    </Upon>
 </Once>
          </x:XData>
        </ElementWithTitle.Title>
        </ElementWithTitle>
    </HoldsOneElement.Element>
</HoldsOneElement>";

        [TestXaml]
        const string XData1_XAML =
@"<ElementCollectionHolder
    xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses'
    xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
     <XmlDataHolderElement>
        <XmlDataHolderElement.XmlData>
          <x:XData>
 <Once xmlns=''>
    <Upon A='Time'>
    <There.Lived/>
    A Princess
    </Upon>
 </Once>
          </x:XData>
        </XmlDataHolderElement.XmlData>
     </XmlDataHolderElement>

     <!-- Check the content property case -->
     <XmlDataHolderElement>
          <x:XData>
 <Once xmlns=''>
    <Upon A='Time'>
    <There.Lived/>
    A Princess
    </Upon>
 </Once>
          </x:XData>
     </XmlDataHolderElement>
</ElementCollectionHolder>";

        [TestXaml]
        const string XData2_XAML =
@"<HoldsOneElement
    xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses'
    xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
    <HoldsOneElement.Element>
        <ElementWithTitle>
        <ElementWithTitle.Title>
          <x:XData/>
        </ElementWithTitle.Title>
        </ElementWithTitle>
    </HoldsOneElement.Element>
</HoldsOneElement>";

        [TestXaml]
        const string XData3_XAML =
@"<HoldsOneElement
    xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses'
    xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
    <HoldsOneElement.Element>
        <ElementWithTitle>
        <ElementWithTitle.Title>
          <x:XData></x:XData>
        </ElementWithTitle.Title>
        </ElementWithTitle>
    </HoldsOneElement.Element>
</HoldsOneElement>";

        [TestXaml]
        const string XData2NoSpaceAfterCloseXData_XAML =
@"<Page
     xmlns = 'http://schemas.microsoft.com/winfx/2006/xaml/presentation'
     xmlns:x = 'http://schemas.microsoft.com/winfx/2006/xaml'>
 
     <Page.Resources>
         <XmlDataProvider x:Key='DSO1'>
         <x:XData>
<Root/>
         </x:XData></XmlDataProvider>
     </Page.Resources>
</Page>";

        private void WhiteSpaceChecker(string s, bool shouldLead, bool shouldTrail, string text)
        {
            if (shouldLead && !Char.IsWhiteSpace(s[0]))
            {
                throw new Exception(String.Format("'{0}': Missing Leading whitespace {1}", s, text));
            }
            if (!shouldLead && Char.IsWhiteSpace(s[0]))
            {
                throw new Exception(String.Format("'{0}': Extra leading whitespace {1}", s, text));
            }
            if (shouldTrail && !Char.IsWhiteSpace(s[s.Length - 1]))
            {
                throw new Exception(String.Format("'{0}': Missing trailing whitespace {1}", s, text));
            }
            if (!shouldTrail && Char.IsWhiteSpace(s[s.Length - 1]))
            {
                throw new Exception(String.Format("'{0}': Extra trailing whitespace {1}", s, text));
            }
        }

        [TestXaml, TestTreeValidator("Whitespace0_Validator")]
        const string Whitespace0_XAML =
@"<ObjectListHolder
    xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses'>
    <ObjectListHolder.StringList>
        text01
<!-- comment -->
        text02
        text03
    </ObjectListHolder.StringList>
</ObjectListHolder>";

        public void Whitespace0_Validator(object o)
        {
            Test.Elements.ObjectListHolder holder = (Test.Elements.ObjectListHolder)o;
            string s;

            const string ShouldBe = "text01 text02 text03";
            s = holder.StringList[0];
            WhiteSpaceChecker(s, false, false, "First String");
            if (holder.StringList.Count != 1)
            {
                throw new Exception("Text broken into more than one piece");
            }
            if (s != ShouldBe)
            {
                throw new Exception(String.Format("'{0}' is not '{1}'", s, ShouldBe));
            }
        }

        [TestXaml, TestTreeValidator("Whitespace1_Validator")]
        const string Whitespace1_XAML = @"
<ObjectListHolder
    xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'
    xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses' >
<!-- Case 1 should be ' Hi There ' -->
    <ObjectListHolder>
      <ObjectListHolder.ObjectList>
        <Element/>
        Hi There 
        <Element />
      </ObjectListHolder.ObjectList>
    </ObjectListHolder>

<!-- Case 2 should be ' Hi There ' -->
    <ObjectListHolder>
        <Element/>
        Hi There 
        <Element />
    </ObjectListHolder>

<!-- Case 3 should be 'Hi There ' -->
    <ObjectListHolder>
        <ObjectListHolder.Tag>
            <Element/>
        </ObjectListHolder.Tag>
        Hi There 
        <Element />
    </ObjectListHolder>

</ObjectListHolder>
";

        public void Whitespace1_Validator(object o)
        {
            Test.Elements.ObjectListHolder holder = (Test.Elements.ObjectListHolder)o;

            string s;

            Test.Elements.ObjectListHolder holder0 = (Test.Elements.ObjectListHolder)holder.ObjectList[0];
            s = (string)holder0.ObjectList[1];
            WhiteSpaceChecker(s, true, true, "Text in First Case");

            Test.Elements.ObjectListHolder holder1 = (Test.Elements.ObjectListHolder)holder.ObjectList[1];
            s = (string)holder1.ObjectList[1];
            WhiteSpaceChecker(s, true, true, "Text in Second Case");

            Test.Elements.ObjectListHolder holder2 = (Test.Elements.ObjectListHolder)holder.ObjectList[2];
            s = (string)holder2.ObjectList[0];
            WhiteSpaceChecker(s, false, true, "Text in Third Case");
        }

        [TestXaml, TestTreeValidator("Whitespace2_Validator")]
        const string Whitespace2_XAML = @"
<Object10i xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'
           xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses' xml:space='preserve'>

<Object10i.Object0>
  <ColorElement>   </ColorElement>
</Object10i.Object0>
<Object10i.Object1>
  <ColorElement>
     <ColorElement.ColorName>   </ColorElement.ColorName>
  </ColorElement>
</Object10i.Object1>
<Object10i.Object2>
  <ColorHolder>
     <ColorHolder.Color>   </ColorHolder.Color>
  </ColorHolder>
</Object10i.Object2>
<Object10i.Object3>
  <HasTitleCpa>   </HasTitleCpa>
</Object10i.Object3>
<Object10i.Object4>
  <HasTitleCpa>   <x:String>   </x:String></HasTitleCpa>
</Object10i.Object4>
<Object10i.Object5>
  <ListOfStrings>  <x:String>   </x:String>    </ListOfStrings>
</Object10i.Object5>
<Object10i.Object6>
  <ColorHolder>
     <ColorHolder.Color>  <ColorElement> Red </ColorElement>  </ColorHolder.Color>
  </ColorHolder>
</Object10i.Object6>
<!--  WSSC with a leading property element -->
<Object10i.Object7>
  <ListOfStrings>    
       <ListOfStrings.Text>  text </ListOfStrings.Text>
        <x:String>   </x:String> 
   </ListOfStrings>
</Object10i.Object7>
<!--  WSSC with a trailing property element -->
<Object10i.Object8>
  <ListOfStrings>    
        <x:String>   </x:String> 
       <ListOfStrings.Title>  title  </ListOfStrings.Title>
   </ListOfStrings>
</Object10i.Object8>
<!--  WSSC with a leading and trailing property element -->
<Object10i.Object9>
  <ListOfStrings>    
       <ListOfStrings.Text>  text </ListOfStrings.Text>
        <x:String>   </x:String> 
       <ListOfStrings.Title>  title  </ListOfStrings.Title>
   </ListOfStrings>
</Object10i.Object9>
</Object10i>
";

        public void Whitespace2_Validator(object o)
        {
            var o10 = (Object10i)o;

            var ce = (ColorElement)o10.Object0;
            Assert.AreEqual("   ", ce.ColorName);
            ce = (ColorElement)o10.Object1;
            Assert.AreEqual("   ", ce.ColorName);
            ce = ((ColorHolder)o10.Object2).Color;
            Assert.AreEqual("   ", ce.ColorName);
            ce = ((ColorHolder)o10.Object6).Color;
            Assert.AreEqual(" Red ", ce.ColorName);

            var ht = (HasTitleCpa)o10.Object3;
            Assert.AreEqual("   ", ht.Title);
            ht = (HasTitleCpa)o10.Object4;
            Assert.AreEqual("   ", ht.Title);

            var ls = (ListOfStrings)o10.Object5;
            Assert.AreEqualOrdered(ls, "  ", "   ", "    ");
            ls = (ListOfStrings)o10.Object7;
            Assert.AreEqualOrdered(ls, "\n        ", "   ", " \n   ");
            Assert.AreEqual("  text ", ls.Text);
            ls = (ListOfStrings)o10.Object8;
            Assert.AreEqualOrdered(ls, "    \n        ", "   ", " \n       ");
            Assert.AreEqual("  title  ", ls.Title);
            ls = (ListOfStrings)o10.Object9;
            Assert.AreEqualOrdered(ls, "\n        ", "   ", " \n       ");
            Assert.AreEqual("  text ", ls.Text);
            Assert.AreEqual("  title  ", ls.Title);
        }

        [TestXaml]
        [TestDisabled]
        const string RootNameScope0_XAML =
        @"<HoldsOneElement
            xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'
            xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses'>
           <HoldsOneElement.Element>
              <Element x:Name='ElementName' />
           </HoldsOneElement.Element>
        </HoldsOneElement>";

        [TestMethod]
        public void RootNameScopeTest()
        {
            XamlSchemaContext xsc = new XamlSchemaContext();
            XamlReader reader = new XamlXmlReader(XmlReader.Create(new StringReader(RootNameScope0_XAML)), xsc);
            XamlObjectWriter writer = new XamlObjectWriter(xsc);
            XamlServices.Transform(reader, writer);
            Element testElement = (Element)writer.RootNameScope.FindName("ElementName");
            if (testElement == null)
            {
                throw new Exception("Getting root namescope not working correctly");
            }
        }

        [TestXaml, TestTreeValidator("RootNameSpace0_Validator")]
        const string RootNameSpace0_XAML =
@"<ElementWithNSProperty
        xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses'
        xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
    <ElementListHolder.Elements>
        <Element x:Name='_elm1'/>
        <Element x:Name='_elm2'/>
        <Element x:Name='_elm3'/>
    </ElementListHolder.Elements>
</ElementWithNSProperty>";

        public void RootNameSpace0_Validator(object o)
        {
            Test.Elements.ElementWithNSProperty root = (Test.Elements.ElementWithNSProperty)o;
            XAML3.INameScope nameScope = root.TheNameScope;
            Test.Elements.Element elm = (Test.Elements.Element)nameScope.FindName("_elm2");
            Assert.IsNotNull(elm);
        }

        [TestXaml, TestTreeValidator("RootNameSpace0_Validator")]
        [TestAlternateXamlLoader("RootNameScope0_LoadWithProvidedNameScope")]
        const string RootNameScope0_XAML_WithProvidedNameScope = RootNameSpace0_XAML;

        public object RootNameScope0_LoadWithProvidedNameScope(string xaml)
        {
            return LoadWithInjectedName(xaml, "_elm2", new object());
        }

        object LoadWithInjectedName(string xaml, string name, object value)
        {
            XamlObjectWriterSettings settings = new XamlObjectWriterSettings()
            {
                ExternalNameScope = new ElementHolderWithNameScope()
            };
            settings.ExternalNameScope.RegisterName(name, value);
            XamlXmlReader reader = new XamlXmlReader(XmlReader.Create(new StringReader(xaml)));
            XamlObjectWriter writer = new XamlObjectWriter(reader.SchemaContext, settings);
            XamlServices.Transform(reader, writer);
            return writer.Result;
        }

        [TestXaml, TestTreeValidator("RootNameSpace1_Validator")]
        const string RootNameSpace1_XAML =
@"<ElementWithAttachedNSProperty
        xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses'
        xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
    <ElementListHolder.Elements>
        <Element x:Name='_elm1'/>
        <Element x:Name='_elm2'/>
        <Element x:Name='_elm3'/>
    </ElementListHolder.Elements>
</ElementWithAttachedNSProperty>";

        public void RootNameSpace1_Validator(object o)
        {
            Test.Elements.ElementWithAttachedNSProperty root = (Test.Elements.ElementWithAttachedNSProperty)o;
            XAML3.INameScope nameScope = Test.Elements.AttachableNameScope.GetAttachableNameScope(root);
            Test.Elements.Element elm = (Test.Elements.Element)nameScope.FindName("_elm2");
        }

        [TestXaml, TestTreeValidator("Template0_Validator")]
        const string Template0_XAML = @"
        <TemplateClass1 xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses'>
          <TemplateClass1.Template>
             <ElementWithTitle Title='This is the title string'/>
          </TemplateClass1.Template>
        </TemplateClass1>";

        public void Template0_Validator(object o)
        {
            Test.Elements.TemplateClass1 to = (Test.Elements.TemplateClass1)o;
            Test.Elements.TestTemplate template = to.Template;
            IXamlObjectWriterFactory factory = template.XamlObjectWriterFactory;
            XamlObjectWriter writer2 = factory.GetXamlObjectWriter(null);
            XamlReader reader = template.GetXamlReader();
            XamlServices.Transform(reader, writer2);
            Test.Elements.ElementWithTitle eWithTitle = (Test.Elements.ElementWithTitle)writer2.Result;
        }

        // Simple Basic use of a Template property.
        [TestXaml, TestTreeValidator("Template1_Validator")]
        const string Template1_XAML = @"
<!-- Test when the template property is NOT the first property set. -->
        <TemplateClass1 Text='set this first' xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses'>
          <TemplateClass1.Template>
             <ElementWithTitle Title='This is the title string'/>
          </TemplateClass1.Template>
        </TemplateClass1>";

        public void Template1_Validator(object o)
        {
            Test.Elements.TemplateClass1 to = (Test.Elements.TemplateClass1)o;
            Test.Elements.TestTemplate template = to.Template;
            IXamlObjectWriterFactory factory = template.XamlObjectWriterFactory;
            XamlObjectWriter writer2 = factory.GetXamlObjectWriter(null);
            XamlReader reader = template.GetXamlReader();
            XamlServices.Transform(reader, writer2);
            Test.Elements.ElementWithTitle eWithTitle = (Test.Elements.ElementWithTitle)writer2.Result;
        }

        [TestXaml, TestTreeValidator("TemplateAndAmbient0_Validator")]
        const string TemplateAndAmbient0_XAML = @"
      <HasAmbientLabel Label='thelabel' xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses'>
        <TemplateClass1 xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses'>
          <TemplateClass1.Template>
             <ColorElement>Red</ColorElement>
          </TemplateClass1.Template>
        </TemplateClass1>
      </HasAmbientLabel>";

        public void TemplateAndAmbient0_Validator(object o)
        {
            Test.Elements.HasAmbientLabel root = (Test.Elements.HasAmbientLabel)o;
            Test.Elements.TemplateClass1 to = (Test.Elements.TemplateClass1)root.Child;

            Test.Elements.TestTemplate template = to.Template;
            IXamlObjectWriterFactory factory = template.XamlObjectWriterFactory;
            XamlObjectWriter writer2 = factory.GetXamlObjectWriter(null);
            XamlReader reader = template.GetXamlReader();
            XamlServices.Transform(reader, writer2);
            Test.Elements.ColorElement colorElement = (Test.Elements.ColorElement)writer2.Result;

            if (colorElement.ColorName != "Red-thelabel")
            {
                throw new Exception("TypeConverter use of Ambient property failed");
            }
        }

        [TestXaml, TestTreeValidator("Template_RootNamescopeTest")]
        const string Template_RootNamescopeTest_XAML = @"
        <TemplateClass1 xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses'
            xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
        <TemplateClass1.Template>
             <ElementWithTitle Title='This is the title string' x:Name='ElementName' />
          </TemplateClass1.Template>
        </TemplateClass1>";

        public void Template_RootNamescopeTest(object o)
        {
            Test.Elements.TemplateClass1 to = (Test.Elements.TemplateClass1)o;
            Test.Elements.TestTemplate template = to.Template;
            IXamlObjectWriterFactory factory = template.XamlObjectWriterFactory;

            XamlObjectWriterSettings settings = new XamlObjectWriterSettings();
            settings.ExternalNameScope = new System.Windows.NameScope();
            settings.RegisterNamesOnExternalNamescope = false;
            XamlObjectWriter writer2 = factory.GetXamlObjectWriter(settings);
            XamlReader reader2 = template.GetXamlReader();
            XamlServices.Transform(reader2, writer2);
            Test.Elements.ElementWithTitle eWithTitle = (Test.Elements.ElementWithTitle)writer2.Result;

            ElementWithTitle testElementFromRootNamescope = (ElementWithTitle)writer2.RootNameScope.FindName("ElementName");
            Assert.IsNotNull(testElementFromRootNamescope, "Namescope did not get registered correctly on root namescope");
            ElementWithTitle testElementFromExternalNamescope = (ElementWithTitle)settings.ExternalNameScope.FindName("ElementName");
            Assert.IsNull(testElementFromExternalNamescope, "Namescope got registered on external namescope when it should not.");
        }

        [TestXaml, TestTreeValidator("TemplateUsingNameScopeEventSimple_Validator")]
        const string TemplateUsingNameScopeEventSimple_XAML = @"
       <HoldsTwoElements xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses' xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
        <HoldsTwoElements.One>
          <HoldsOneElement x:Name='Foo'>
            <TemplateClassWithNameResolver>
              <TemplateClassWithNameResolver.Template>
                <HoldsTwoElements One='{x:Reference Foo}' Two='{x:Reference Bar}' />
              </TemplateClassWithNameResolver.Template>
            </TemplateClassWithNameResolver>
          </HoldsOneElement>
        </HoldsTwoElements.One>
        <HoldsTwoElements.Two>
          <ElementWithTitle x:Name='Bar' Title='This is the title string'/>
        </HoldsTwoElements.Two>
      </HoldsTwoElements>";

        public void TemplateUsingNameScopeEventSimple_Validator(object o)
        {
            HoldsTwoElements root = (HoldsTwoElements)o;
            TemplateClassWithNameResolver to = (TemplateClassWithNameResolver)((HoldsOneElement)root.One).Element;

            TemplateWithNameResolver template = to.Template;

            IXamlNameResolver resolver = template.Resolver;
            var foo = resolver.Resolve("Foo") as HoldsOneElement;
            var bar = resolver.Resolve("Bar") as ElementWithTitle;

            Assert.IsNotNull(foo);
            Assert.IsNotNull(bar);

            var pairs = resolver.GetAllNamesAndValuesInScope();

            Assert.AreEqual(2, pairs.Count<KeyValuePair<string, object>>());
        }

        [TestXaml, TestTreeValidator("MultipleTemplatesUsingNameScopeEvent_Validator")]
        const string MultipleTemplatesUsingNameScopeEvent_XAML = @"
       <HoldsTwoElements xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses' xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
        <HoldsTwoElements.One>
          <HoldsOneElement x:Name='Foo'>
            <TemplateClassWithNameResolver>
              <TemplateClassWithNameResolver.Template>
                <HoldsTwoElements One='{x:Reference Foo}' Two='{x:Reference Bar}' />
              </TemplateClassWithNameResolver.Template>
            </TemplateClassWithNameResolver>
          </HoldsOneElement>
        </HoldsTwoElements.One>
        <HoldsTwoElements.Two>
          <NameScopeElement x:Name='Bar'>
            <NameScopeElement.StartNode>
              <HoldsOneElement x:Name='Baz'>
                <TemplateClassWithNameResolver>
                  <TemplateClassWithNameResolver.Template>
                    <HoldsTwoElements One='{x:Reference Foo}' Two='{x:Reference Bar}' />
                  </TemplateClassWithNameResolver.Template>
                </TemplateClassWithNameResolver>
              </HoldsOneElement>
            </NameScopeElement.StartNode>
          </NameScopeElement>
        </HoldsTwoElements.Two>
      </HoldsTwoElements>";

        public void MultipleTemplatesUsingNameScopeEvent_Validator(object o)
        {
            HoldsTwoElements root = (HoldsTwoElements)o;
            TemplateClassWithNameResolver to1 = (TemplateClassWithNameResolver)((HoldsOneElement)root.One).Element;
            TemplateWithNameResolver template1 = to1.Template;
            IXamlNameResolver resolver1 = template1.Resolver;

            Assert.AreEqual(2, resolver1.GetAllNamesAndValuesInScope().Count());

            TemplateClassWithNameResolver to2 = (TemplateClassWithNameResolver)((HoldsOneElement)((NameScopeElement)root.Two).StartNode).Element;
            TemplateWithNameResolver template2 = to2.Template;
            IXamlNameResolver resolver2 = template2.Resolver;

            var names = resolver2.GetAllNamesAndValuesInScope().Select(kvp => kvp.Key).ToList();
            Assert.AreEqualUnordered(names, "Bar", "Baz", "Foo");
        }

        [TestXaml, TestTreeValidator("MultipleTemplatesUsingNameScopeEventWithShadowedNames_Validator")]
        const string MultipleTemplatesUsingNameScopeEventWithShadowedNames_XAML = @"
       <HoldsTwoElements x:Name='Foo' xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses' xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
        <HoldsTwoElements.One>
          <HoldsOneElement x:Name='Baz'>
            <TemplateClassWithNameResolver>
              <TemplateClassWithNameResolver.Template>
                <HoldsTwoElements One='{x:Reference Foo}' Two='{x:Reference Bar}' />
              </TemplateClassWithNameResolver.Template>
            </TemplateClassWithNameResolver>
          </HoldsOneElement>
        </HoldsTwoElements.One>
        <HoldsTwoElements.Two>
          <NameScopeElement x:Name='Bar'>
            <NameScopeElement.StartNode>
              <HoldsOneElement x:Name='Foo'>
                <TemplateClassWithNameResolver>
                  <TemplateClassWithNameResolver.Template>
                    <HoldsTwoElements One='{x:Reference Foo}' Two='{x:Reference Bar}' />
                  </TemplateClassWithNameResolver.Template>
                </TemplateClassWithNameResolver>
              </HoldsOneElement>
            </NameScopeElement.StartNode>
          </NameScopeElement>
        </HoldsTwoElements.Two>
      </HoldsTwoElements>";

        public void MultipleTemplatesUsingNameScopeEventWithShadowedNames_Validator(object o)
        {
            HoldsTwoElements root = (HoldsTwoElements)o;
            TemplateClassWithNameResolver to1 = (TemplateClassWithNameResolver)((HoldsOneElement)root.One).Element;
            TemplateWithNameResolver template1 = to1.Template;
            IXamlNameResolver resolver1 = template1.Resolver;

            var namesAndValues = resolver1.GetAllNamesAndValuesInScope().ToList();

            Assert.AreEqual(3, namesAndValues.Count);

            TemplateClassWithNameResolver to2 = (TemplateClassWithNameResolver)((HoldsOneElement)((NameScopeElement)root.Two).StartNode).Element;
            TemplateWithNameResolver template2 = to2.Template;
            IXamlNameResolver resolver2 = template2.Resolver;

            namesAndValues = resolver2.GetAllNamesAndValuesInScope().ToList();
            var names = namesAndValues.Select(kvp => kvp.Key).ToList();
            Assert.AreEqualUnordered(names, "Bar", "Foo", "Baz");

            var foo = namesAndValues.Where(kvp => kvp.Key == "Foo").Single();
            Assert.IsTrue(foo.Value is HoldsOneElement);

        }

        private Assembly FindLoadedAssembly(string asmName)
        {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly asm in assemblies)
            {
                if (asm.FullName.Contains(asmName))
                    return asm;
            }
            return null;
        }

        [TestDisabled]  // Needs setup in the TestMethod below. (fails as independent text)
        [TestXaml]
        const string LocalAssembly0_XAML = @"
        <HoldsOneElement
            xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses'>
           <HoldsOneElement.Element>
              <InternalElement PublicNameOfInternalType='this is the title' />
           </HoldsOneElement.Element>
        </HoldsOneElement>";

        [TestMethod]  // continue to run this as a test method.
        public void LocalAssembly0()
        {
            XamlXmlReaderSettings settings = new XamlXmlReaderSettings();
            settings.LocalAssembly = FindLoadedAssembly("XamlTestClasses");

            TextReader textReader = new StringReader(LocalAssembly0_XAML);
            XmlReader xmlReader = XmlReader.Create(textReader);
            XamlXmlReader reader = new XamlXmlReader(xmlReader, settings);

            XamlNodeList xamlNodeList = new XamlNodeList(reader.SchemaContext);
            XamlWriter xamlWriter = xamlNodeList.Writer;
            XamlServices.Transform(reader, xamlWriter);

            XamlReader xamlReader = xamlNodeList.GetReader();
            XamlObjectWriter objWriter = new XamlObjectWriter(xamlReader.SchemaContext);
            XamlServices.Transform(xamlReader, objWriter);
            object o = objWriter.Result;
            Test.Elements.HoldsOneElement root = (Test.Elements.HoldsOneElement)o;
        }

        [TestDisabled]  // Needs setup in the TestMethod below. (fails as independent text)
        [TestXaml]
        const string LocalAssembly1_XAML = @"
        <HoldsOneElement
            xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses'>
           <HoldsOneElement.Element>
              <InternalElement InternalProperty='internal on internal' />
           </HoldsOneElement.Element>
        </HoldsOneElement>";

        [TestMethod]  // continue to run this as a test method.
        public void LocalAssembly1()
        {
            XamlXmlReaderSettings settings = new XamlXmlReaderSettings();
            settings.LocalAssembly = FindLoadedAssembly("XamlTestClasses");

            TextReader textReader = new StringReader(LocalAssembly1_XAML);
            XmlReader xmlReader = XmlReader.Create(textReader);
            XamlXmlReader reader = new XamlXmlReader(xmlReader, settings);

            XamlNodeList xamlNodeList = new XamlNodeList(reader.SchemaContext);
            XamlWriter xamlWriter = xamlNodeList.Writer;
            XamlServices.Transform(reader, xamlWriter);

            XamlReader xamlReader = xamlNodeList.GetReader();
            XamlObjectWriter objWriter = new XamlObjectWriter(xamlReader.SchemaContext);
            XamlServices.Transform(xamlReader, objWriter);
            object o = objWriter.Result;
            Test.Elements.HoldsOneElement root = (Test.Elements.HoldsOneElement)o;
        }

        [TestDisabled]  // Needs setup in the TestMethod below. (fails as independent text)
        [TestXaml]
        const string LocalAssembly2_XAML = @"
        <HoldsOneElement
            xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses'>
           <HoldsOneElement.Element>
              <ElementWithInternalProperty InternalProperty='internal on a public' />
           </HoldsOneElement.Element>
        </HoldsOneElement>";

        [TestMethod]  // continue to run this as a test method.
        public void LocalAssembly2()
        {
            XamlXmlReaderSettings settings = new XamlXmlReaderSettings();
            settings.LocalAssembly = FindLoadedAssembly("XamlTestClasses");

            TextReader textReader = new StringReader(LocalAssembly2_XAML);
            XmlReader xmlReader = XmlReader.Create(textReader);
            XamlXmlReader reader = new XamlXmlReader(xmlReader, settings);

            XamlNodeList xamlNodeList = new XamlNodeList(reader.SchemaContext);
            XamlWriter xamlWriter = xamlNodeList.Writer;
            XamlServices.Transform(reader, xamlWriter);

            XamlReader xamlReader = xamlNodeList.GetReader();
            XamlObjectWriter objWriter = new XamlObjectWriter(xamlReader.SchemaContext);
            XamlServices.Transform(xamlReader, objWriter);
            object o = objWriter.Result;
            Test.Elements.HoldsOneElement root = (Test.Elements.HoldsOneElement)o;
        }

        [TestMethod, TestExpectedException(typeof(XamlObjectWriterException))]
        public void LocalAssembly2_Negative()
        {
            XamlXmlReaderSettings settings = new XamlXmlReaderSettings();
            settings.LocalAssembly = FindLoadedAssembly("DrtXaml");

            TextReader textReader = new StringReader(LocalAssembly2_XAML);
            XmlReader xmlReader = XmlReader.Create(textReader);
            XamlXmlReader reader = new XamlXmlReader(xmlReader, settings);

            XamlNodeList xamlNodeList = new XamlNodeList(reader.SchemaContext);
            XamlWriter xamlWriter = xamlNodeList.Writer;
            XamlServices.Transform(reader, xamlWriter);

            XamlReader xamlReader = xamlNodeList.GetReader();
            XamlObjectWriter objWriter = new XamlObjectWriter(xamlReader.SchemaContext);
            XamlServices.Transform(xamlReader, objWriter);
            object o = objWriter.Result;
            Test.Elements.HoldsOneElement root = (Test.Elements.HoldsOneElement)o;
        }

        [TestDisabled]  // Needs setup in the TestMethod below. (fails as independent text)
        [TestXaml]
        const string LocalAssembly3_XAML = @"
        <HoldsOneElement
            xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses'>
           <HoldsOneElement.Element>
              <InternalElement/>
           </HoldsOneElement.Element>
        </HoldsOneElement>";

        [TestDisabled]  // this fails because the Generic<> stuff broke LoadAssemby.
        [TestMethod]
        public void LocalAssembly3()
        {
            XamlXmlReaderSettings settings = new XamlXmlReaderSettings();
            // settings.LocalAssembly = FindLoadedAssembly("XamlTestClasses");   No LocalAssembly

            TextReader textReader = new StringReader(LocalAssembly3_XAML);
            XmlReader xmlReader = XmlReader.Create(textReader);
            XamlXmlReader reader = new XamlXmlReader(xmlReader, settings);

            XamlNodeList xamlNodeList = new XamlNodeList(reader.SchemaContext);
            XamlWriter xamlWriter = xamlNodeList.Writer;
            XamlServices.Transform(reader, xamlWriter);

            XamlReader xamlReader = xamlNodeList.GetReader();
            XamlObjectWriter objWriter = new XamlObjectWriter(xamlReader.SchemaContext);
            bool didThrow = false;
            try
            {
                XamlServices.Transform(xamlReader, objWriter);
            }
            catch
            {
                didThrow = true;
            }
            if (!didThrow)
            {
                throw new Exception("Accessing an internal type should have thrown");
            }
            object o = objWriter.Result;
            Test.Elements.HoldsOneElement root = (Test.Elements.HoldsOneElement)o;
        }

        [TestDisabled]  // Needs setup in the TestMethod below. (fails as independent text)
        [TestXaml]
        const string LocalAssembly4_XAML = @"
        <HoldsOneElement
            xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses'>
           <HoldsOneElement.Element>
              <ElementWithInternalProperty InternalProperty='internal on a public' />
           </HoldsOneElement.Element>
        </HoldsOneElement>";

        [TestMethod]  // continue to run this as a test method.
        public void LocalAssembly4()
        {
            XamlXmlReaderSettings settings = new XamlXmlReaderSettings();
            // settings.LocalAssembly = FindLoadedAssembly("XamlTestClasses");  No localAssembly.

            TextReader textReader = new StringReader(LocalAssembly4_XAML);
            XmlReader xmlReader = XmlReader.Create(textReader);
            XamlXmlReader reader = new XamlXmlReader(xmlReader, settings);

            XamlNodeList xamlNodeList = new XamlNodeList(reader.SchemaContext);
            XamlWriter xamlWriter = xamlNodeList.Writer;
            XamlServices.Transform(reader, xamlWriter);

            XamlReader xamlReader = xamlNodeList.GetReader();
            XamlObjectWriter objWriter = new XamlObjectWriter(xamlReader.SchemaContext);
            bool didThrow = false;
            try
            {
                XamlServices.Transform(xamlReader, objWriter);
            }
            catch
            {
                didThrow = true;
            }
            if (!didThrow)
            {
                throw new Exception("Access to internal property should have thrown");
            }
            object o = objWriter.Result;
            Test.Elements.HoldsOneElement root = (Test.Elements.HoldsOneElement)o;
        }

        [TestDisabled]  // Needs setup in the TestMethod below. (fails as independent text)
        [TestXaml]
        const string LocalAssembly5_XAML = @"
        <HoldsOneElement
            xmlns='clr-namespace:Test.Elements'>
           <HoldsOneElement.Element>
              <InternalElement/>
           </HoldsOneElement.Element>
        </HoldsOneElement>";

        [TestMethod]  // continue to run this as a test method.
        public void LocalAssembly5()
        {
            XamlXmlReaderSettings settings = new XamlXmlReaderSettings();
            settings.LocalAssembly = FindLoadedAssembly("XamlTestClasses");

            TextReader textReader = new StringReader(LocalAssembly5_XAML);
            XmlReader xmlReader = XmlReader.Create(textReader);
            XamlXmlReader reader = new XamlXmlReader(xmlReader, settings);

            XamlNodeList xamlNodeList = new XamlNodeList(reader.SchemaContext);
            XamlWriter xamlWriter = xamlNodeList.Writer;
            XamlServices.Transform(reader, xamlWriter);

            XamlReader xamlReader = xamlNodeList.GetReader();
            XamlObjectWriter objWriter = new XamlObjectWriter(xamlReader.SchemaContext);
            XamlServices.Transform(xamlReader, objWriter);
            object o = objWriter.Result;
            Test.Elements.HoldsOneElement root = (Test.Elements.HoldsOneElement)o;
        }

        [TestDisabled]  // Needs setup in the TestMethod below. (fails as independent text)
        [TestXaml]
        const string LocalAssembly6_XAML = @"
        <Object10
            xmlns='clr-namespace:Test.Elements'>
           <Object10.Object0>
              <InternalObjectWithInternalDefaultCtor X='3'  Y='5'/>
           </Object10.Object0>
        </Object10>";

        [TestMethod]  // continue to run this as a test method.
        public void LocalAssembly6()
        {
            Assembly localAssembly = FindLoadedAssembly("XamlTestClasses");
            XamlXmlReaderSettings settings = new XamlXmlReaderSettings();
            settings.LocalAssembly = localAssembly;

            TextReader textReader = new StringReader(LocalAssembly6_XAML);
            XmlReader xmlReader = XmlReader.Create(textReader);
            XamlXmlReader reader = new XamlXmlReader(xmlReader, settings);

            XamlNodeList xamlNodeList = new XamlNodeList(reader.SchemaContext);
            XamlWriter xamlWriter = xamlNodeList.Writer;
            XamlServices.Transform(reader, xamlWriter);

            XamlReader xamlReader = xamlNodeList.GetReader();

            // Set local assembly on the XOW also (to access internal Ctors)
            XamlObjectWriterSettings xowSettings = new XamlObjectWriterSettings();
            xowSettings.AccessLevel = System.Xaml.Permissions.XamlAccessLevel.AssemblyAccessTo(localAssembly);

            XamlObjectWriter objWriter = new XamlObjectWriter(xamlReader.SchemaContext, xowSettings);
            XamlServices.Transform(xamlReader, objWriter);
            object o = objWriter.Result;
            Test.Elements.Object10 root = (Test.Elements.Object10)o;
        }

        const string LocalAssembly_FriendWithKey_XAML = @"
        <Object10i
            xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses'>
           <Object10i.Object0>
              <FriendWithKey xmlns='clr-namespace:Test.Elements.Friends;assembly=XamlTestClasses.FriendWithKey' />
           </Object10i.Object0>
        </Object10i>";

        [TestXaml, TestExpectedException(typeof(XamlObjectWriterException))]
        const string LocalAssembly_FriendWithKey_Negative = LocalAssembly_FriendWithKey_XAML;

        [TestXaml, TestAlternateXamlLoader("LocalAssembly_Loader")]
        [TestKnownFailure (Reason = "")]
        const string LocalAssembly_FriendWithKey_Positive = LocalAssembly_FriendWithKey_XAML;

        const string LocalAssembly_FriendWithoutKey_XAML = @"
        <Object10i
            xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses'>
           <Object10i.Object0>
              <FriendWithoutKey xmlns='clr-namespace:Test.Elements.Friends;assembly=XamlTestClasses.FriendWithoutKey' />
           </Object10i.Object0>
        </Object10i>";

        [TestXaml, TestExpectedException(typeof(XamlObjectWriterException))]
        const string LocalAssembly_FriendWithoutKey_Negative = LocalAssembly_FriendWithoutKey_XAML;

        [TestXaml, TestAlternateXamlLoader("LocalAssembly_Loader")]
        const string LocalAssembly_FriendWithoutKey_Positive = LocalAssembly_FriendWithoutKey_XAML;

        [TestXaml, TestAlternateXamlLoader("LocalAssembly_Loader")]
        const string LocalAssembly_InternalTypeArg = @"
        <Object10i
            xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses'
            xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'
            xmlns:scg='clr-namespace:System.Collections.Generic;assembly=mscorlib'>
           <Object10i.Object0>
                <scg:List x:TypeArguments='InternalElement'>
                 <InternalElement />
                 <InternalElement />
              </scg:List>
           </Object10i.Object0>
           <Object10i.Object1>
              <scg:Dictionary x:TypeArguments='x:String, InternalElement'>
                 <InternalElement x:Key='a' />
              </scg:Dictionary>
           </Object10i.Object1>
        </Object10i>";

        [TestXaml, TestAlternateXamlLoader("LocalAssembly_Loader"), TestExpectedException(typeof(XamlObjectWriterException))]
        const string LocalAssembly_MixedInternalTypeArgs = @"
        <scg:Dictionary
            xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses'
            xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'
            xmlns:s='clr-namespace:System;assembly=mscorlib'
            xmlns:scg='clr-namespace:System.Collections.Generic;assembly=mscorlib'
            x:TypeArguments='InternalElement,s:RuntimeType' />";

        [TestXaml, TestAlternateXamlLoader("LocalAssembly_Loader"), TestExpectedException(typeof(XamlObjectWriterException))]
        const string LocalAssembly_ForeignInternalTypeArg = @"
        <InternalList
            xmlns='clr-namespace:Test.Collections;assembly=XamlTestClasses'
            xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'
            xmlns:s='clr-namespace:System;assembly=mscorlib'
            xmlns:scg='clr-namespace:System.Collections.Generic;assembly=mscorlib'
            x:TypeArguments='s:RuntimeType' />";

        public object LocalAssembly_Loader(string xaml)
        {
            XamlXmlReaderSettings settings = new XamlXmlReaderSettings();
            settings.LocalAssembly = FindLoadedAssembly("XamlTestClasses");

            TextReader textReader = new StringReader(xaml);
            XmlReader xmlReader = XmlReader.Create(textReader);
            XamlXmlReader reader = new XamlXmlReader(xmlReader, settings);

            return XamlServices.Load(reader);
        }

        const string ProtectedProp_Attribute_XAML = @"
        <ElementWithInternalProperty ProtectedProperty='Secret'
          xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses' />";

        [TestXaml, TestAlternateXamlLoader("ProtectedPropLoader")]
        const string ProtectedProp_Attribute_Positive = ProtectedProp_Attribute_XAML;

        [TestXaml, TestExpectedException(typeof(XamlObjectWriterException))]
        const string ProtectedProp_Attribute_Negative = ProtectedProp_Attribute_XAML;

        const string ProtectedProp_Element_XAML = @"
        <ElementWithInternalProperty xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses'>
          <ElementWithInternalProperty.ProtectedProperty>Secret</ElementWithInternalProperty.ProtectedProperty>
        </ElementWithInternalProperty>";

        [TestXaml, TestAlternateXamlLoader("ProtectedPropLoader")]
        const string ProtectedProp_Element_Positive = ProtectedProp_Element_XAML;

        [TestXaml, TestExpectedException(typeof(XamlObjectWriterException))]
        const string ProtectedProp_Element_Negative = ProtectedProp_Element_XAML;

        [TestXaml, TestAlternateXamlLoader("ProtectedPropLoader"), TestExpectedException(typeof(XamlObjectWriterException))]
        const string ProtectedProp_NotAtRoot_XAML = @"
        <ElementWithInternalProperty xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses'>
          <ElementWithInternalProperty.NestedElement>
            <ElementWithInternalProperty ProtectedProperty='Secret' />
          </ElementWithInternalProperty.NestedElement>
        </ElementWithInternalProperty>";

        public object ProtectedPropLoader(string xaml)
        {
            XamlXmlReaderSettings settings = new XamlXmlReaderSettings();
            settings.AllowProtectedMembersOnRoot = true;

            TextReader textReader = new StringReader(xaml);
            XmlReader xmlReader = XmlReader.Create(textReader);
            XamlXmlReader reader = new XamlXmlReader(xmlReader, settings);
            IHaveNoPrivacy ihp = (IHaveNoPrivacy)XamlServices.Load(reader);
            Assert.AreEqual("Secret", ihp.GetValue("ProtectedProperty"));
            return ihp;
        }

        [TestXaml]
        const string BuiltInMeTest0 = @"
<Object10 xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'
                     xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses'
     Object1='{x:Type TypeName=x:String, x:Name=_typ1}'
     Object2='{x:Type x:Type.TypeName=x:String}' >
   <Object10.Object3>
      <x:Type TypeName='x:String' x:Name='_typ3'/>
   </Object10.Object3>
   <Object10.Object4>
      <x:Type x:Type.TypeName='x:String'/>
   </Object10.Object4>
   <Object10.Object5>
      <x:Type x:Name='_typ5'>
         <x:Type.TypeName>
            x:String
         </x:Type.TypeName>
      </x:Type>
   </Object10.Object5>
</Object10>";


        [TestMethod]
        public void ObjectWriterClear()
        {
            string xaml = @"<x:String xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>Hello</x:String>";

            XamlXmlReader reader = new XamlXmlReader(XmlReader.Create(new StringReader(xaml)));
            XamlObjectWriter writer = new XamlObjectWriter(reader.SchemaContext);

            reader = new XamlXmlReader(XmlReader.Create(new StringReader(xaml)), reader.SchemaContext);
            writer.Clear();
            XamlServices.Transform(reader, writer);
        }

        [TestMethod]
        public void NSBeforeGO()
        {
            XamlSchemaContext xsc = new XamlSchemaContext();
            XamlObjectWriter ow = new XamlObjectWriter(xsc);

            ow.WriteNamespace(new NamespaceDeclaration("http://schemas.microsoft.com/winfx/2006/xaml", "x"));
            ow.WriteNamespace(new NamespaceDeclaration("clr-namespace:Test.Elements;assembly=XamlTestClasses", string.Empty));

            XamlType nsgo = xsc.GetXamlType(typeof(NSGO));
            XamlMember stringList = nsgo.GetAttachableMember("StringList");
            XamlType stringType = xsc.GetXamlType(typeof(string));
            ow.WriteStartObject(nsgo);
              ow.WriteStartMember(stringList);
                ow.WriteNamespace(new NamespaceDeclaration("clr-namespace:System;assembly=mscorlib", "s"));
                ow.WriteGetObject();
                  ow.WriteStartMember(XamlLanguage.Items);
                    ow.WriteStartObject(stringType);
                      ow.WriteStartMember(XamlLanguage.Initialization);
                        ow.WriteValue("hello");
                      ow.WriteEndMember(); // _init
                    ow.WriteEndObject(); // string
                  ow.WriteEndMember(); // _items
                ow.WriteEndObject(); // GO
              ow.WriteEndMember(); // StringList
            ow.WriteEndObject(); // NSGO
        }

        [TestXaml]
        const string DictionaryDelayAdd =
@"<StackPanel
    xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'
    xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
    <StackPanel.Resources>
        <Button x:Key='_rButton1' Content='r1'/>
        <Brush x:Key='_rbrush'>Blue</Brush>
        <Button Content='hello'>
           <x:Key>button_key</x:Key>
       </Button>
    </StackPanel.Resources>
    <Button x:Name='_btn' Content='Press'/>
  </StackPanel>
";

        [TestXaml]
        const string CompatNS =
        @"<FixedPage xmlns='http://schemas.microsoft.com/xps/2005/06' 
xmlns:x='http://schemas.microsoft.com/xps/2005/06/resourcedictionary-key' 
xml:lang='en-us'>
  <FixedPage.Resources>
    <ResourceDictionary>
      <ImageBrush x:Key='b0'/>
    </ResourceDictionary>
  </FixedPage.Resources>
</FixedPage>
";

        const string PrefixToNamespaceDefinedInXmlContext1 = "<my:TypeElement TypeProperty='my:Element' />";

        [TestMethod]
        public void Load_PrefixToNamespaceDefinedInXmlContext1()
        {
            XmlNamespaceManager xmlnsMgr = new XmlNamespaceManager(new NameTable());
            xmlnsMgr.AddNamespace("my", "clr-namespace:Test.Elements;assembly=XamlTestClasses");
            XmlParserContext parserContext = new XmlParserContext(null, xmlnsMgr, null, XmlSpace.None);
            Stream stream = new MemoryStream(Encoding.Default.GetBytes(PrefixToNamespaceDefinedInXmlContext1));
            XmlTextReader xmlReader = new XmlTextReader(stream, XmlNodeType.Document, parserContext);
            XamlXmlReader xamlReader = new XamlXmlReader(xmlReader);
            TypeElement result = (TypeElement)XamlServices.Load(xamlReader);
            Assert.AreEqual(typeof(Element), result.TypeProperty);
        }

        string PrefixToNamespaceDefinedInXmlContext2 = @"
<StackPanel>
    <Button x:Name='btn' Height='70' Width='200' Foreground='White'>
        <Button.Background>Red</Button.Background>
        Press Me
    </Button>
</StackPanel>
";
        [TestMethod]
        public void Load_PrefixToNamespaceDefinedInXmlContext2()
        {
            XmlNamespaceManager xmlnsMgr = new XmlNamespaceManager(new NameTable());
            xmlnsMgr.AddNamespace("x", "http://schemas.microsoft.com/winfx/2006/xaml");
            xmlnsMgr.AddNamespace("", "http://schemas.microsoft.com/winfx/2006/xaml/presentation");
            XmlParserContext parserContext = new XmlParserContext(null, xmlnsMgr, null, XmlSpace.None);
            Stream stream = new MemoryStream(Encoding.Default.GetBytes(PrefixToNamespaceDefinedInXmlContext2));
            XmlTextReader xmlReader = new XmlTextReader(stream, XmlNodeType.Document, parserContext);
            XamlXmlReader xamlReader = new XamlXmlReader(xmlReader);
            object result = XamlServices.Load(xamlReader);
        }

        public void CollectionWithCPwithTCTest()
        {
            TypeConverterContentProperty target = new TypeConverterContentProperty
            {
                ContentProperty = new AnimalList
                {
                    new AnimalInfo{ Name = "Cassowary", Number = int.MaxValue},
                    new AnimalInfo{ Name = "Emu", Number = int.MinValue},
                    new AnimalInfo{ Name = "Ostrich", Number = 0}
                }
            };
            string xaml = XamlServices.Save(target);
            TypeConverterContentProperty parsedObject = (TypeConverterContentProperty)XamlServices.Parse(xaml);

            Assert.IsNotNull(parsedObject);
            Assert.AreEqual(target.ContentProperty.Count, parsedObject.ContentProperty.Count);
            Assert.AreEqual(target.ContentProperty[0].Name, parsedObject.ContentProperty[0].Name);
            Assert.AreEqual(target.ContentProperty[1].Name, parsedObject.ContentProperty[1].Name);
            Assert.AreEqual(target.ContentProperty[2].Name, parsedObject.ContentProperty[2].Name);
        }

        const string XClassAtRoot_Xaml = @"
<Element x:Class='Test.Elements.HoldsOneElement' 
         xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses'
         xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml' />";

        [TestXaml, TestAlternateXamlLoader("RootObjectInstanceLoader")]
        const string XClassAtRoot_Positive = XClassAtRoot_Xaml;

        [TestXaml, TestExpectedException(typeof(XamlObjectWriterException))]
        const string XClassAtRoot_Negative = XClassAtRoot_Xaml;

        [TestXaml, TestExpectedException(typeof(XamlObjectWriterException))]
        const string XClassBelowRoot = @"
<HoldsOneElement 
         xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses'
         xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
  <Element x:Class='Test.Elements.HoldsOneElement' />
</HoldsOneElement>
";

        public object RootObjectInstanceLoader(string xaml)
        {
            XamlReader reader = new XamlXmlReader(XmlReader.Create(new StringReader(xaml)));
            XamlObjectWriterSettings ows = new XamlObjectWriterSettings { RootObjectInstance = new HoldsOneElement() };
            XamlObjectWriter xow = new XamlObjectWriter(reader.SchemaContext, ows);
            XamlServices.Transform(reader, xow);
            return xow.Result;
        }

        const string MEAsTemplateRoot =
@"<ElementResourceHolder xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses' xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
  <ElementResourceHolder.Resources>
    <ElementWithTitle x:Key='foo' Title='This is the title string'/>
  </ElementResourceHolder.Resources>
  <ElementResourceHolder.Children>
    <TemplateClass1>
      <TemplateClass1.Template>
        <ResourceLookupExtension ResourceKey='foo'/>
      </TemplateClass1.Template>
    </TemplateClass1>
  </ElementResourceHolder.Children>
</ElementResourceHolder>";

        object RunMEAsTemplateRoot(bool skip)
        {
            XamlSchemaContext c = new XamlSchemaContext();
            XamlXmlReader r = new XamlXmlReader(XmlReader.Create(new StringReader(MEAsTemplateRoot)), c);
            XamlObjectWriterSettings s = new XamlObjectWriterSettings();
            s.SkipProvideValueOnRoot = skip;
            XamlObjectWriter w = new XamlObjectWriter(c, s);
            XamlServices.Transform(r, w);
            TemplateClass1 holder = (TemplateClass1)((ElementResourceHolder)w.Result).Children[0];
            XamlObjectWriter w2 = holder.Template.XamlObjectWriterFactory.GetXamlObjectWriter(s);
            XamlServices.Transform(holder.Template.GetXamlReader(), w2);
            return w2.Result;
        }

        [TestMethod]
        public void ProvideValueOnMEAsTemplateRoot()
        {
            object o = RunMEAsTemplateRoot(false);
            if (!(o is ElementWithTitle))
            {
                throw new Exception(string.Format("Expected result of type {0}, not {1}.", typeof(ElementWithTitle), o.GetType()));
            }
        }

        [TestMethod]
        public void SkipProvideValueOnMEAsTemplateRoot()
        {
            object o = RunMEAsTemplateRoot(true);
            if (!(o is ResourceLookupExtension))
            {
                throw new Exception(string.Format("Expected result of type {0}, not {1}.", typeof(ResourceLookupExtension), o.GetType()));
            }
        }

        [TestMethod]
        public void IPVT_RegularProperty()
        {
            string xaml = @"
<IPVTContainer xmlns=""clr-namespace:Test.Elements;assembly=XamlTestClasses"">
    <IPVTContainer.Setter>
        <SetterExtension />
    </IPVTContainer.Setter>
</IPVTContainer>";

            IPVTContainer container = XamlServices.Load(XmlReader.Create(new StringReader(xaml))) as IPVTContainer;

            if (container.Setter != "Setter")
            {
                throw new Exception("IProvideValueTarget.TargetProperty did not return the Setter method");
            }
        }
        
        [TestMethod]
        public void IPVT_AttachedProperty()
        {
            string xaml = @"
<IPVTContainer xmlns=""clr-namespace:Test.Elements;assembly=XamlTestClasses"">
    <TargetPropertyHolder.Setter>
        <SetterExtension />
    </TargetPropertyHolder.Setter>
</IPVTContainer>";

            IPVTContainer container = XamlServices.Load(XmlReader.Create(new StringReader(xaml))) as IPVTContainer;

            if (TargetPropertyHolder.GetSetter(container) != "SetSetter")
            {
                throw new Exception("IProvideValueTarget.TargetProperty did not return the Setter method for an attached property");
            }
        }
        
        [TestMethod]
        public void IPVT_CollectionItem()
        {
            string xaml = @"
<IPVTContainer xmlns:sc=""clr-namespace:System.Collections;assembly=mscorlib"" xmlns=""clr-namespace:Test.Elements;assembly=XamlTestClasses"">
    <IPVTContainer.List>
        <sc:ArrayList>
                <SetterExtension />
        </sc:ArrayList>
    </IPVTContainer.List>
</IPVTContainer>";

            IPVTContainer container = XamlServices.Load(XmlReader.Create(new StringReader(xaml))) as IPVTContainer;

            if (container.List.Count != 1 || container.List[0].ToString() != "null")
            {
                throw new Exception("IProvideValueTarget.TargetProperty did not return null for an item in a collection");
            }
        }
        
        [TestMethod]
        public void IPVT_DictionaryItem()
        {
            string xaml = @"
<sc:Hashtable xmlns:sc=""clr-namespace:System.Collections;assembly=mscorlib"" xmlns=""clr-namespace:Test.Elements;assembly=XamlTestClasses"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
        <SetterExtension x:Key=""foo"" />
</sc:Hashtable>";

            Hashtable table = XamlServices.Load(XmlReader.Create(new StringReader(xaml))) as Hashtable as Hashtable;

            if (table.Count != 1 || table["foo"].ToString() != "null")
            {
                throw new Exception("IProvideValueTarget.TargetProperty did not return null for an item in a dictionary");
            }
        }
    }
}
