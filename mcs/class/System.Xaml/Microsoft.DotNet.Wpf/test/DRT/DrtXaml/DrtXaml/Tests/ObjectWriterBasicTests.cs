using System;
using DRT;
using System.IO;
using System.Xaml;
using System.Xml;
using DrtXaml.XamlTestFramework;
using Test;
using Test.Elements;
using System.Collections.Generic;
using Test.Collections;
using System.Collections;
using Test.Properties;
using System.Windows.Markup;
using System.Windows.Controls;

namespace DrtXaml.Tests
{
    [TestClass]
    sealed class ObjectWriterBasicTests : XamlTestSuite
    {
        public ObjectWriterBasicTests()
            : base("ObjectWriterBasicTests")
        {
        }

        public override DrtTest[] PrepareTests()
        {
            DrtTest[] tests = DrtTestFinder.FindTests(this);
            return tests;
        }

        // =============================================

        [TestMethod]
        public void WriteStringValueForPropertyOfTypeDouble()
        {
            XamlSchemaContext xsc = new XamlSchemaContext();
            XamlObjectWriter ow = new XamlObjectWriter(xsc);

            XamlType simpleElementXT = xsc.GetXamlType(typeof(ElementWithSimpleProperties));

            ow.WriteStartObject(simpleElementXT);
            ow.WriteStartMember(simpleElementXT.GetMember("Double"));
            ow.WriteValue("23.5");
            ow.WriteEndMember();
            ow.WriteEndObject();

            ElementWithSimpleProperties simpleElement = (ElementWithSimpleProperties)(ow.Result);
            if (23.5 != simpleElement.Double)
            {
                throw new Exception("simpleElement.Double==" + simpleElement.Double + " ; expected 23.5");
            }
        }

        [TestMethod]
        public void WriteDoubleValueForPropertyOfTypeDouble()
        {
            XamlSchemaContext xsc = new XamlSchemaContext();
            XamlObjectWriter ow = new XamlObjectWriter(xsc);

            XamlType simpleElementXT = xsc.GetXamlType(typeof(ElementWithSimpleProperties));

            ow.WriteStartObject(simpleElementXT);
            ow.WriteStartMember(simpleElementXT.GetMember("Double"));
            ow.WriteValue(23.5);
            ow.WriteEndMember();
            ow.WriteEndObject();

            ElementWithSimpleProperties simpleElement = (ElementWithSimpleProperties)(ow.Result);
            if (23.5 != simpleElement.Double)
            {
                throw new Exception("simpleElement.Double==" + simpleElement.Double + " ; expected 23.5");
            }
        }

        //MikeHill was questioning if this is a good thing to support.  It works because DoubleConverter supports converting from Int32.
        //He is questioning if we should use implicit converters from XAML.
        [TestMethod]
        public void WriteIntegerValueForPropertyOfTypeDouble()
        {
            XamlSchemaContext xsc = new XamlSchemaContext();
            XamlObjectWriter ow = new XamlObjectWriter(xsc);

            XamlType simpleElementXT = xsc.GetXamlType(typeof(ElementWithSimpleProperties));

            ow.WriteStartObject(simpleElementXT);
            ow.WriteStartMember(simpleElementXT.GetMember("Double"));
            ow.WriteValue(23);
            ow.WriteEndMember();
            ow.WriteEndObject();

            ElementWithSimpleProperties simpleElement = (ElementWithSimpleProperties)(ow.Result);
            if (23 != simpleElement.Double)
            {
                throw new Exception("simpleElement.Double==" + simpleElement.Double + " ; expected 23.  Int32 didn't work.");
            }
        }

        [TestMethod]
        public void WriteStringValueForPropertyOfTypeString()
        {
            XamlSchemaContext xsc = new XamlSchemaContext();
            XamlObjectWriter ow = new XamlObjectWriter(xsc);

            XamlType simpleElementXT = xsc.GetXamlType(typeof(ElementWithSimpleProperties));

            ow.WriteStartObject(simpleElementXT);
            ow.WriteStartMember(simpleElementXT.GetMember("String"));
            ow.WriteValue("23.5");
            ow.WriteEndMember();
            ow.WriteEndObject();

            ElementWithSimpleProperties simpleElement = (ElementWithSimpleProperties)(ow.Result);
            if ("23.5" != simpleElement.String)
            {
                throw new Exception("simpleElement.String==\"" + simpleElement.String + "\" ; expected \"23.5\"");
            }
        }

        [TestMethod]
        public void WriteDoubleValueForPropertyOfTypeString()
        {
            XamlSchemaContext xsc = new XamlSchemaContext();
            XamlObjectWriter ow = new XamlObjectWriter(xsc);

            XamlType simpleElementXT = xsc.GetXamlType(typeof(ElementWithSimpleProperties));

            ow.WriteStartObject(simpleElementXT);
            ow.WriteStartMember(simpleElementXT.GetMember("String"));
            bool didThrow = false;
            try
            {
                ow.WriteValue(23.5);
                ow.WriteEndMember();
                ow.WriteEndObject();
            }
            catch
            {
                didThrow = true;
            }

            if (!didThrow)
            {
                throw new Exception("writing a value of type double for a property of type string should have thrown.");
            }
        }

        bool tapEventFired;
        [TestMethod]
        public void WriteValueOfEventAsDelegate()
        {
            XamlSchemaContext xsc = new XamlSchemaContext();
            XamlObjectWriter ow = new XamlObjectWriter(xsc);

            XamlType eventElementXT = xsc.GetXamlType(typeof(EventElement));

            ow.WriteStartObject(eventElementXT);
            ow.WriteStartMember(eventElementXT.GetMember("Tap"));
            EventElement.TapDelegate tapDelegate = new EventElement.TapDelegate(methodToCall);
            ow.WriteValue(tapDelegate);
            ow.WriteEndMember();
            ow.WriteEndObject();

            EventElement eventElement = ((EventElement)ow.Result);
            tapEventFired = false;
            eventElement.RaiseTapEvent();
            if (!tapEventFired)
            {
                throw new Exception("Tap event did not fire");
            }
        }
        private void methodToCall(object source, EventArgs args)
        {
            tapEventFired = true;
        }

        [TestXaml, TestTreeValidator("SetEventToMarkupExtensionWhichReturnsAMatchingDelegate_TreeValidator")]
        const string SetEventToMarkupExtensionWhichReturnsAMatchingDelegate_XAML =
@"<EventElement
    xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses'
    Tap='{DelegateCreatingME}' />";

        public void SetEventToMarkupExtensionWhichReturnsAMatchingDelegate_TreeValidator(object o)
        {
            EventElement eventElement = ((EventElement)o);
            int fireCount = eventElement.TapEventCount;
            eventElement.RaiseTapEvent();
            if (eventElement.TapEventCount != fireCount + 1)
            {
                throw new Exception("Tap event did not fire");
            }
        }

        [TestMethod]
        public void WriteEventWithAMarkupExtensionReturingADelegate()
        {
            XamlSchemaContext xsc = new XamlSchemaContext();
            XamlObjectWriter ow = new XamlObjectWriter(xsc);

            XamlType eventElementXT = xsc.GetXamlType(typeof(EventElement));
            XamlType delegateCreatingMeXT = xsc.GetXamlType(typeof(DelegateCreatingME));

            ow.WriteStartObject(eventElementXT);
            ow.WriteStartMember(eventElementXT.GetMember("Tap"));
            ow.WriteStartObject(delegateCreatingMeXT);
            ow.WriteEndObject();
            ow.WriteEndMember();
            ow.WriteEndObject();

            EventElement eventElement = ((EventElement)ow.Result);
            int fireCount = eventElement.TapEventCount;
            eventElement.RaiseTapEvent();
            if (eventElement.TapEventCount != fireCount + 1)
            {
                throw new Exception("Tap event did not fire");
            }
        }

        [TestXaml, TestTreeValidator("SetAttachableEvent_TreeValidator")]
        const string SetAttachableEvent_XAML =
@"<EventElement
    xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses'
    AttachedEventHolder.TapEvent='{DelegateCreatingME}' />";

        public void SetAttachableEvent_TreeValidator(object o)
        {
            EventElement eventElement = ((EventElement)o);
            int fireCount = eventElement.TapEventCount;
            AttachedEventHolder.RaiseTapEvent(eventElement);
            if (eventElement.TapEventCount != fireCount + 1)
            {
                throw new Exception("Tap event did not fire");
            }
        }

        [TestMethod]
        public void WriteAttachableEvent()
        {
            XamlSchemaContext xsc = new XamlSchemaContext();
            XamlObjectWriter ow = new XamlObjectWriter(xsc);

            XamlType eventElementXT = xsc.GetXamlType(typeof(EventElement));
            XamlType eventHolderXT = xsc.GetXamlType(typeof(AttachedEventHolder));
            XamlType delegateCreatingMeXT = xsc.GetXamlType(typeof(DelegateCreatingME));

            ow.WriteStartObject(eventElementXT);
            ow.WriteStartMember(eventHolderXT.GetAttachableMember("TapEvent"));
            ow.WriteStartObject(delegateCreatingMeXT);
            ow.WriteEndObject();
            ow.WriteEndMember();
            ow.WriteEndObject();

            EventElement eventElement = ((EventElement)ow.Result);
            int fireCount = eventElement.TapEventCount;
            AttachedEventHolder.RaiseTapEvent(eventElement);
            if (eventElement.TapEventCount != fireCount + 1)
            {
                throw new Exception("Tap event did not fire");
            }
        }

        [TestMethod]
        public void WriteValuesIntoACollection()
        {
            XamlSchemaContext xsc = new XamlSchemaContext();
            XamlObjectWriter ow = new XamlObjectWriter(xsc);

            XamlType roParentWithCpaXT = xsc.GetXamlType(typeof(RoParentWithCP));

            ow.WriteStartObject(roParentWithCpaXT);
            XamlMember contentProperty = roParentWithCpaXT.ContentProperty;
            ow.WriteStartMember(contentProperty);
            ow.WriteGetObject();
            ow.WriteStartMember(XamlLanguage.Items);
            ow.WriteValue(new Kid());
            ow.WriteValue(new Kid());
            ow.WriteValue(new Kid());
            ow.WriteEndMember();
            ow.WriteEndObject();
            ow.WriteEndMember();
            ow.WriteEndObject();

            RoParentWithCP roParentWithCpa = (RoParentWithCP)(ow.Result);
            if (roParentWithCpa.RoKids.Count != 3)
            {
                throw new Exception("WriteValue of multiple items aren't being added into a collection by XamlObjectWriter");
            }
        }

        [TestMethod]
        public void WriteValueIntoASingularProperty()
        {
            XamlSchemaContext xsc = new XamlSchemaContext();
            XamlObjectWriter ow = new XamlObjectWriter(xsc);

            XamlType holdsOneElementXT = xsc.GetXamlType(typeof(HoldsOneElement));

            ow.WriteStartObject(holdsOneElementXT);
            XamlMember contentProperty = holdsOneElementXT.ContentProperty;
            ow.WriteStartMember(contentProperty);
            Element element = new Element();
            ow.WriteValue(element);
            ow.WriteEndMember();
            ow.WriteEndObject();

            HoldsOneElement holdsOneElement = (HoldsOneElement)(ow.Result);
            if (holdsOneElement.Element != element)
            {
                throw new Exception("WriteValue of an instance by XamlObjectWriter can't be set to a singular property value");
            }
        }

        //This sounds a bit odd, but we should support it.
        [TestMethod]
        public void WriteNullIntoACollection()
        {
            XamlSchemaContext xsc = new XamlSchemaContext();
            XamlObjectWriter ow = new XamlObjectWriter(xsc);

            XamlType roParentWithCpaXT = xsc.GetXamlType(typeof(RoParentWithCP));

            ow.WriteStartObject(roParentWithCpaXT);
            XamlMember contentProperty = roParentWithCpaXT.ContentProperty;
            ow.WriteStartMember(contentProperty);
            ow.WriteGetObject();
            ow.WriteStartMember(XamlLanguage.Items);
            ow.WriteValue(null);
            ow.WriteValue(null);
            ow.WriteEndMember();
            ow.WriteEndObject();
            ow.WriteEndMember();
            ow.WriteEndObject();

            RoParentWithCP roParentWithCP = (RoParentWithCP)(ow.Result);
            List<Kid> kids = roParentWithCP.RoKids;
            if (kids.Count != 2 || kids[0] != null || kids[1] != null)
            {
                throw new Exception("Calling WriteValue twice with a null value is malfunctioning.");
            }
        }

        [TestMethod]
        public void WriteNullIntoASingularProperty()
        {
            XamlSchemaContext xsc = new XamlSchemaContext();
            XamlObjectWriter ow = new XamlObjectWriter(xsc);

            XamlType holdsOneElementXT = xsc.GetXamlType(typeof(HoldsOneElement));

            ow.WriteStartObject(holdsOneElementXT);
            XamlMember contentProperty = holdsOneElementXT.ContentProperty;
            ow.WriteStartMember(contentProperty);
            ow.WriteValue(null);
            ow.WriteEndMember();
            ow.WriteEndObject();

            HoldsOneElement holdsOneElement = (HoldsOneElement)(ow.Result);
            if (holdsOneElement.Element != null)
            {
                throw new Exception("WriteValue of null by XamlObjectWriter can't be set to a singular property value");
            }
        }

        [TestMethod]
        public void NamespacePrefixValidationTest()
        {
            string xaml = @"
<NamespacePrefixValidation xmlns=""clr-namespace:Test.Elements;assembly=XamlTestClasses"" xmlns:s=""clr-namespace:System;assembly=mscorlib"">
    <NamespacePrefixValidation.BarContainer>
        <BarContainer xmlns:st=""clr-namespace:System.Threading;assembly=System"" xmlns:scg=""clr-namespace:System.Collection.Generic;assembly=System"" >
            <BarContainer.Bar>1</BarContainer.Bar>
        </BarContainer>
    </NamespacePrefixValidation.BarContainer>
    <NamespacePrefixValidation.FooContainer>
        <FooContainer xmlns:sx=""clr-namespace:System.Xaml;assembly=System.Xaml"" xmlns:s=""clr-namespace:System.Collections.Generic;assembly=System"">
            <FooContainer.Foo>4</FooContainer.Foo>
        </FooContainer>
    </NamespacePrefixValidation.FooContainer>
    <NamespacePrefixValidation.FooContainerTemplate>
        <FooContainer xmlns:sx=""clr-namespace:System.Xaml;assembly=System.Xaml"" xmlns:st=""clr-namespace:System.Threading;assembly=System"" xmlns:scg=""clr-namespace:System.Collection.Generic;assembly=System"">
            <FooContainer.Foo>6</FooContainer.Foo>
        </FooContainer>
    </NamespacePrefixValidation.FooContainerTemplate>
</NamespacePrefixValidation>
";
            NamespacePrefixValidation npv = XamlServices.Parse(xaml) as NamespacePrefixValidation;

            FooContainer container1 = npv.FooContainerTemplate();
            FooContainer container2 = npv.FooContainerTemplate();
        }


        [TestMethod]
        public void IDestinationTypeProviderTestAttributes()
        {
            string xaml = @"
<DestinationTypeProviderTestContainer DestinationType=""DT(int)"" DestinationProperty=""List(string)""  xmlns=""clr-namespace:Test.Elements;assembly=XamlTestClasses"" />
";
            DestinationTypeProviderTestContainer dest = XamlServices.Parse(xaml) as DestinationTypeProviderTestContainer;
        }


        [TestMethod]
        public void IDestinationTypeProviderTestCollections()
        {
            string xaml = @"
<DestinationTypeProviderTestContainer xmlns=""clr-namespace:Test.Elements;assembly=XamlTestClasses"" xmlns:s=""clr-namespace:System;assembly=mscorlib"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
    <DestinationTypeProviderTestContainer.DestinationTypeList>
        <DestinationType x:TypeArguments=""s:Int32"">DestinationType</DestinationType>
    </DestinationTypeProviderTestContainer.DestinationTypeList>
</DestinationTypeProviderTestContainer>
";
            DestinationTypeProviderTestContainer dest = XamlServices.Parse(xaml) as DestinationTypeProviderTestContainer;
        }


        [TestMethod]
        [TestDisabled]
        public void IDestinationTypeProviderTestElements()
        {
            string xaml = @"
<DestinationTypeProviderTestContainer xmlns=""clr-namespace:Test.Elements;assembly=XamlTestClasses"" xmlns:s=""clr-namespace:System;assembly=mscorlib"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"" xmlns:scg=""clr-namespace:System.Collection.Generic;assembly=System"">    
    <DestinationTypeProviderTestContainer.DestinationType>
        <DestinationType x:TypeArguments=""s:Int32"">DestinationType(int)</DestinationType>
    </DestinationTypeProviderTestContainer.DestinationType>
    <DestinationTypeProviderTestContainer.DestinationType>
        <scg:List x:TypeArguments=""s:String"">List(string)</scg:List>
    </DestinationTypeProviderTestContainer.DestinationType>
</DestinationTypeProviderTestContainer>
";
            DestinationTypeProviderTestContainer dest = XamlServices.Parse(xaml) as DestinationTypeProviderTestContainer;
        }

        [TestMethod]
        public void GenericDictionaryKeyAttribute()
        {
            string xaml = @"
<Dictionary x:TypeArguments=""s:Int32, s:String"" xmlns=""clr-namespace:System.Collections.Generic;assembly=mscorlib"" xmlns:s=""clr-namespace:System;assembly=mscorlib"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
    <s:String x:Key=""1"">One</s:String>
</Dictionary>
";
            Dictionary<int, string> dictionary = (Dictionary<int, string>)XamlServices.Parse(xaml);
        }

        [TestMethod]
        public void GenericDictionaryKeyElement()
        {
            string xaml = @"
<Dictionary x:TypeArguments=""s:Int32, s:String"" xmlns=""clr-namespace:System.Collections.Generic;assembly=mscorlib"" xmlns:s=""clr-namespace:System;assembly=mscorlib"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
    <s:String><x:Key>1</x:Key>One</s:String>
</Dictionary>
";
            Dictionary<int, string> dictionary = (Dictionary<int, string>)XamlServices.Parse(xaml);
        }

        [TestMethod]
        public void GenericDictionaryKeyComplexElement()
        {
            string xaml = @"
<Dictionary x:TypeArguments=""te:Kid, s:String"" xmlns=""clr-namespace:System.Collections.Generic;assembly=mscorlib"" xmlns:s=""clr-namespace:System;assembly=mscorlib"" xmlns:te=""clr-namespace:Test.Elements;assembly=XamlTestClasses"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
    <s:String>
        <x:Key>
            <te:Kid>
                <te:Kid.Name>Barney</te:Kid.Name>
            </te:Kid>
        </x:Key>
        One
    </s:String>
</Dictionary>
";
            Dictionary<Kid, string> dictionary = (Dictionary<Kid, string>)XamlServices.Parse(xaml);
        }

        [TestMethod]
        public void ForwardReferenceInNestedNamescope()
        {
            var s = @"
<HoldsOneElement xmlns=""clr-namespace:Test.Elements;assembly=XamlTestClasses"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
  <NameScopeElement EndNode=""{x:Reference Name=__ReferenceID0}"">
    <NameScopeElement.StartNode>
      <Element x:Name=""__ReferenceID0"" />
    </NameScopeElement.StartNode>
  </NameScopeElement>
</HoldsOneElement>";
            var holder = (HoldsOneElement)XamlServices.Parse(s);
        }

        [TestXaml]
        [TestExpectedException(typeof(XamlObjectWriterException))]
        const string UnknownAssembly = @"
<local:TypeContainer 
 xmlns:local=""clr-namespace:Test.Elements;assembly=XamlTestClasses""
 xmlns:un=""clr-namespace:Test.Elements;assembly=Unknown""
 local:TypeContainer.Type=""un:TypeContainer"" >
</local:TypeContainer>
";

        [TestXaml]
        [TestExpectedException(typeof(XamlObjectWriterException))]
        const string PropertiesOnTypeConvertedInstance = @"
<HoldsOneElement
    xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses' >
   <HoldsOneElement.Element>
      <ColorElement>Red
         <ColorElement.ColorName>Blue</ColorElement.ColorName>
      </ColorElement>
   </HoldsOneElement.Element>
</HoldsOneElement>
";

        [TestXaml]
        [TestExpectedException(typeof(XamlObjectWriterException))]
        const string GetObjectPropertyNull = @"<RwNullParentWithCP xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses'><Kid Name='test'/></RwNullParentWithCP>";

        [TestXaml]
        const string NonGenericDictionary_NoKeyTypeConversion =
@"<ResourceDictionary
        xmlns='clr-namespace:System.Windows;assembly=PresentationFramework'
        xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
    <x:String x:Key='1'>2</x:String>
</ResourceDictionary>";

        [TestXaml]
        const string GenericDictionary_NoKeyTypeConversion =
@"<Dictionary x:TypeArguments='x:Int32, x:String'
        xmlns='clr-namespace:System.Collections.Generic;assembly=mscorlib'
        xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
    <x:String x:Key='1'>2</x:String>
</Dictionary>";

        [TestXaml]
        const string GenericDictionary_KeyTypeConversion =
@"<RWGenericDictionaryProvider x:TypeArguments='x:Int32, x:String'
        xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses'
        xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
    <RWGenericDictionaryProvider.Dict>
        <x:String x:Key='1'>2</x:String>
    </RWGenericDictionaryProvider.Dict>
</RWGenericDictionaryProvider>";

        [TestXaml]
        const string DerivedGenericDictionary_KeyTypeConversion =
@"<RWDerivedGenericDictionaryProvider
        xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses'
        xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
    <RWDerivedGenericDictionaryProvider.Dict>
        <x:String x:Key='1'>2</x:String>
    </RWDerivedGenericDictionaryProvider.Dict>
</RWDerivedGenericDictionaryProvider>";

        [TestXaml, TestAlternateXamlLoader("SkipDuplicatePropertyCheck_Loader")]
        const string SkipDuplicatePropertyCheck =
@"<HoldsOneElement xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses'>
    <HoldsOneElement.Element>
        <Element />
    </HoldsOneElement.Element>
    <HoldsOneElement.Element>
        <Element />
    </HoldsOneElement.Element>
</HoldsOneElement>";

        public object SkipDuplicatePropertyCheck_Loader(string xaml)
        {
            XamlObjectWriterSettings settings = new XamlObjectWriterSettings
            {
                SkipDuplicatePropertyCheck = true
            };
            XamlXmlReader reader = new XamlXmlReader(XmlReader.Create(new StringReader(xaml)));
            XamlObjectWriter writer = new XamlObjectWriter(reader.SchemaContext, settings);
            XamlServices.Transform(reader, writer);
            return writer.Result;
        }

        [TestXaml]
        [TestExpectedException(typeof(XamlObjectWriterException))]
        public const string ArgumentsOutOfOrder = @"
<StringBuilder
    xmlns='clr-namespace:System.Text;assembly=mscorlib'
    xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
    <StringBuilder.Length>50</StringBuilder.Length>
    <x:Arguments>
        <x:String>foo</x:String>
    </x:Arguments>
</StringBuilder>
";

        [TestXaml, TestExpectedException(typeof(XamlDuplicateMemberException))]
        const string DirectiveAliasedProperties_Name1 = @"
<HoldsOneElement  xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'
                  xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses' >
    <HoldsOneElement.Element>
        <ElementWithDirectiveAliasedProperties x:Name='first' RuntimeName='second'/>
    </HoldsOneElement.Element>
</HoldsOneElement>
";

        [TestXaml, TestExpectedException(typeof(XamlDuplicateMemberException))]
        const string DirectiveAliasedProperties_Name2 = @"
<HoldsOneElement  xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'
                  xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses' >
    <HoldsOneElement.Element>
        <ElementWithDirectiveAliasedProperties>
            <ElementWithDirectiveAliasedProperties.RuntimeName>
                first
            </ElementWithDirectiveAliasedProperties.RuntimeName>
            <x:Name>
                second
            </x:Name>
        </ElementWithDirectiveAliasedProperties>
    </HoldsOneElement.Element>
</HoldsOneElement>
";

        [TestXaml, TestExpectedException(typeof(XamlDuplicateMemberException))]
        const string DirectiveAliasedProperties_Name3 = @"
<HoldsOneElement  xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'
                  xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses' >
    <HoldsOneElement.Element>
        <ElementWithDirectiveAliasedProperties x:Name='first'>
            <ElementWithDirectiveAliasedProperties.RuntimeName>
                second
            </ElementWithDirectiveAliasedProperties.RuntimeName>
        </ElementWithDirectiveAliasedProperties>
    </HoldsOneElement.Element>
</HoldsOneElement>
";

        [TestXaml, TestExpectedException(typeof(XamlDuplicateMemberException))]
        const string DirectiveAliasedProperties_Name4 = @"
<HoldsOneElement  xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'
                  xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses' >
    <HoldsOneElement.Element>
        <ElementWithDirectiveAliasedProperties RuntimeName='first'>
            <x:Name>
                second
            </x:Name>
        </ElementWithDirectiveAliasedProperties>
    </HoldsOneElement.Element>
</HoldsOneElement>
";

        [TestXaml, TestExpectedException(typeof(XamlDuplicateMemberException))]
        const string DirectiveAliasedProperties_xmlLang = @"
<HoldsOneElement  xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'
                  xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses' >
    <HoldsOneElement.Element>
            <ElementWithDirectiveAliasedProperties xml:lang='en' XmlLang='en-us'/>
    </HoldsOneElement.Element>
</HoldsOneElement>
";

        [TestXaml, TestExpectedException(typeof(XamlDuplicateMemberException))]
        const string DirectiveAliasedProperties_Uid = @"
<HoldsOneElement  xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'
                  xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses' >
    <HoldsOneElement.Element>
            <ElementWithDirectiveAliasedProperties x:Uid='012345-56789' Uid='123456-7890'/>
    </HoldsOneElement.Element>
</HoldsOneElement>
";

        [TestXaml, TestExpectedException(typeof(XamlObjectWriterException))]
        const string DictItemValueNodeString = @"
<Dictionary x:TypeArguments='x:String, x:Object'
xmlns='clr-namespace:System.Collections.Generic;assembly=mscorlib'
xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
test
</Dictionary>
";

        [TestMethod]
        public void DictItemValueNodeDKP()
        {
            XamlSchemaContext c = new XamlSchemaContext();
            XamlObjectWriter w = new XamlObjectWriter(c);
            w.WriteNamespace(new NamespaceDeclaration("http://schemas.microsoft.com/winfx/2006/xaml", "x"));
            w.WriteNamespace(new NamespaceDeclaration("clr-namespace:System.Collections.Generic;assembly=mscorlib", "g"));
            w.WriteNamespace(new NamespaceDeclaration("clr-namespace:Test;assembly=XamlTestClasses", "t"));
            w.WriteStartObject(c.GetXamlType(typeof(Dictionary<string, object>)));
            w.WriteStartMember(XamlLanguage.Items);
            w.WriteValue(new DKPClass());
            w.WriteEndMember();
            w.WriteEndObject();
        }

        [TestMethod, TestExpectedException(typeof(XamlObjectWriterException))]
        public void DictItemValueNodeNull()
        {
            XamlSchemaContext c = new XamlSchemaContext();
            XamlObjectWriter w = new XamlObjectWriter(c);
            w.WriteNamespace(new NamespaceDeclaration("http://schemas.microsoft.com/winfx/2006/xaml", "x"));
            w.WriteNamespace(new NamespaceDeclaration("clr-namespace:System.Collections.Generic;assembly=mscorlib", "g"));
            w.WriteStartObject(c.GetXamlType(typeof(Dictionary<string, object>)));
            w.WriteStartMember(XamlLanguage.Items);
            w.WriteValue(null);
            w.WriteEndMember();
            w.WriteEndObject();
        }

        [TestMethod]
        public void NameScopeProperty1()
        {
            NameScopeArrayListHolder n = (NameScopeArrayListHolder)XamlServices.Parse(@"
<NameScopeArrayListHolder xmlns='clr-namespace:Test.Properties;assembly=XamlTestClasses' xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
  <NameScopeArrayListHolder.ArrayList>
    <x:Int32 x:Name='foo'/>
  </NameScopeArrayListHolder.ArrayList>
</NameScopeArrayListHolder>");
            Assert.AreEqual((n as INameScope).FindName("foo"), 0);
            Assert.IsNull((n.ArrayList as INameScope).FindName("foo"));
        }

        [TestMethod]
        public void NameScopeProperty2()
        {
            NameScopeArrayListHolder n = (NameScopeArrayListHolder)XamlServices.Parse(@"
<NameScopeArrayListHolder xmlns='clr-namespace:Test.Properties;assembly=XamlTestClasses' xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
  <NameScopeArrayListHolder.NameScopeArrayList>
    <x:Int32 x:Name='foo'/>
  </NameScopeArrayListHolder.NameScopeArrayList>
</NameScopeArrayListHolder>");
            Assert.IsNull((n as INameScope).FindName("foo"));
            Assert.AreEqual((n.NameScopeArrayList as INameScope).FindName("foo"), 0);
        }

        // Write GO as the first node in the NodeStream
        //
        [TestMethod]
        [TestExpectedException(typeof(XamlObjectWriterException))]
        public void TestGO_1()
        {
            XamlSchemaContext context = new XamlSchemaContext();
            XamlObjectWriter objWriter = new XamlObjectWriter(context);

            objWriter.WriteGetObject();

            objWriter.Clear();
            objWriter.Close();
        }

        // Write SO; GO (no current Property)
        //
        [TestMethod]
        [TestExpectedException(typeof(XamlObjectWriterException))]
        public void TestGO_2()
        {
            XamlSchemaContext context = new XamlSchemaContext();
            XamlObjectWriter objWriter = new XamlObjectWriter(context);

            XamlType type = context.GetXamlType(typeof(Element10));
            objWriter.WriteStartObject(type);

            objWriter.WriteGetObject();

            objWriter.Clear();
            objWriter.Close();
        }

        // Write SO; SO (no current Property)
        //
        [TestMethod]
        [TestExpectedException(typeof(XamlObjectWriterException))]
        public void TestSO_1()
        {
            XamlSchemaContext context = new XamlSchemaContext();
            XamlObjectWriter objWriter = new XamlObjectWriter(context);

            XamlType type = context.GetXamlType(typeof(Element10));
            objWriter.WriteStartObject(type);

            XamlType innerType = context.GetXamlType(typeof(Object10));

            /* should fail here */
            objWriter.WriteStartObject(innerType);

            objWriter.Clear();
            objWriter.Close();
        }

        // Write SO; NS (no current Property)
        //
        [TestMethod]
        [TestExpectedException(typeof(XamlObjectWriterException))]
        public void TestSO_2()
        {
            XamlSchemaContext context = new XamlSchemaContext();
            XamlObjectWriter objWriter = new XamlObjectWriter(context);

            XamlType type = context.GetXamlType(typeof(Element10));
            objWriter.WriteStartObject(type);

            var ns = new NamespaceDeclaration("http://schemas.microsoft.com/winfx/2006/xaml", "xx");

            /* should fail here */
            objWriter.WriteNamespace(ns);

            objWriter.Clear();
            objWriter.Close();
        }

        // Write EO at the first node in the stream
        //
        [TestMethod]
        [TestExpectedException(typeof(XamlObjectWriterException))]
        public void TestEO_1()
        {
            XamlSchemaContext context = new XamlSchemaContext();
            XamlObjectWriter objWriter = new XamlObjectWriter(context);

            /* should fail here */
            objWriter.WriteEndObject();

            objWriter.Clear();
            objWriter.Close();
        }

        // Write SO; SM; EO
        //
        [TestMethod]
        [TestExpectedException(typeof(XamlObjectWriterException))]
        public void TestEO_2()
        {
            XamlSchemaContext context = new XamlSchemaContext();
            XamlObjectWriter objWriter = new XamlObjectWriter(context);

            XamlType type = context.GetXamlType(typeof(Element10));
            objWriter.WriteStartObject(type);

            XamlMember member = type.GetMember("Element3");
            objWriter.WriteStartMember(member);

            /* should fail here */
            objWriter.WriteEndObject();

            objWriter.Clear();
            objWriter.Close();
        }

        // Write SM as the first node in the stream.
        //
        [TestMethod]
        [TestExpectedException(typeof(XamlObjectWriterException))]
        public void TestSM_1()
        {
            XamlSchemaContext context = new XamlSchemaContext();
            XamlObjectWriter objWriter = new XamlObjectWriter(context);

            XamlType type = context.GetXamlType(typeof(Element10));
            XamlMember member = type.GetMember("Element3");

            /* should fail here */
            objWriter.WriteStartMember(member);

            objWriter.Clear();
            objWriter.Close();
        }

        // Write SO; SM; SM
        //
        [TestMethod]
        [TestExpectedException(typeof(XamlObjectWriterException))]
        public void TestSM_2()
        {
            XamlSchemaContext context = new XamlSchemaContext();
            XamlObjectWriter objWriter = new XamlObjectWriter(context);

            XamlType type = context.GetXamlType(typeof(Element10));
            objWriter.WriteStartObject(type);

            XamlMember member = type.GetMember("Element3");
            objWriter.WriteStartMember(member);

            XamlMember member2 = type.GetMember("Element7");

            /* should fail here */
            objWriter.WriteStartMember(member2);

            objWriter.Clear();
            objWriter.Close();
        }

        // Write EM as the first node in the stream.
        //
        [TestMethod]
        [TestExpectedException(typeof(XamlObjectWriterException))]
        public void TestEM_1()
        {
            XamlSchemaContext context = new XamlSchemaContext();
            XamlObjectWriter objWriter = new XamlObjectWriter(context);

            /* should fail here */
            objWriter.WriteEndMember();

            objWriter.Clear();
            objWriter.Close();
        }

        // Write SO; EM
        //
        [TestMethod]
        [TestExpectedException(typeof(XamlObjectWriterException))]
        public void TestEM_2()
        {
            XamlSchemaContext context = new XamlSchemaContext();
            XamlObjectWriter objWriter = new XamlObjectWriter(context);

            XamlType type = context.GetXamlType(typeof(Element10));
            objWriter.WriteStartObject(type);

            /* should fail here */
            objWriter.WriteEndMember();

            objWriter.Clear();
            objWriter.Close();
        }

        // Write V as the first node in the stream.
        //
        [TestMethod]
        [TestExpectedException(typeof(XamlObjectWriterException))]
        public void TestV_1()
        {
            XamlSchemaContext context = new XamlSchemaContext();
            XamlObjectWriter objWriter = new XamlObjectWriter(context);

            /* should fail here */
            objWriter.WriteValue("XYZZY-42");

            objWriter.Clear();
            objWriter.Close();
        }

        // Write SO; V
        //
        [TestMethod]
        [TestExpectedException(typeof(XamlObjectWriterException))]
        public void TestV_2()
        {
            XamlSchemaContext context = new XamlSchemaContext();
            XamlObjectWriter objWriter = new XamlObjectWriter(context);

            XamlType type = context.GetXamlType(typeof(Element10));
            objWriter.WriteStartObject(type);

            /* should fail here */
            objWriter.WriteValue("XYZZY-42");

            objWriter.Clear();
            objWriter.Close();
        }

        // support method.
        static XamlObjectWriter Setup_SO_SM_V()
        {
            XamlSchemaContext context = new XamlSchemaContext();
            XamlObjectWriter objWriter = new XamlObjectWriter(context);

            XamlType type = context.GetXamlType(typeof(Object10));
            objWriter.WriteStartObject(type);

            XamlMember member = type.GetMember("Object0");
            objWriter.WriteStartMember(member);

            objWriter.WriteValue("XYZZY-42");

            return objWriter;
        }

        // Write SO; SM; V; SO
        //
        [TestMethod]
        [TestExpectedException(typeof(XamlObjectWriterException))]
        public void TestV_3()
        {
            XamlObjectWriter objWriter = Setup_SO_SM_V();

            XamlType innerType = objWriter.SchemaContext.GetXamlType(typeof(ColorElement));

            /* should fail here */
            objWriter.WriteStartObject(innerType);

            objWriter.Clear();
            objWriter.Close();
        }

        // Write SO; SM; V; SO
        //
        [TestMethod]
        [TestExpectedException(typeof(XamlObjectWriterException))]
        public void TestV_4()
        {
            XamlObjectWriter objWriter = Setup_SO_SM_V();

            XamlType innerType = objWriter.SchemaContext.GetXamlType(typeof(ColorElement));

            /* should fail here */
            objWriter.WriteStartObject(innerType);

            objWriter.Clear();
            objWriter.Close();
        }

        // Write SO; SM; V; GO
        //
        [TestMethod]
        [TestExpectedException(typeof(XamlObjectWriterException))]
        public void TestV_5()
        {
            XamlObjectWriter objWriter = Setup_SO_SM_V();

            /* should fail here */
            objWriter.WriteGetObject();

            objWriter.Clear();
            objWriter.Close();
        }

        // Write SO; SM; V; EO
        //
        [TestMethod]
        [TestExpectedException(typeof(XamlObjectWriterException))]
        public void TestV_6()
        {
            XamlObjectWriter objWriter = Setup_SO_SM_V();

            /* should fail here */
            objWriter.WriteEndObject();

            objWriter.Clear();
            objWriter.Close();
        }

        // Write SO; SM; V; SM
        //
        [TestMethod]
        [TestExpectedException(typeof(XamlObjectWriterException))]
        public void TestV_7()
        {
            XamlObjectWriter objWriter = Setup_SO_SM_V();

            XamlType type = objWriter.SchemaContext.GetXamlType(typeof(Element10));
            XamlMember member = type.GetMember("Element0");

            /* should fail here */
            objWriter.WriteStartMember(member);

            objWriter.Clear();
            objWriter.Close();
        }

        // Write SO; SM; V; NS
        //
        [TestMethod]
        [TestExpectedException(typeof(XamlObjectWriterException))]
        public void TestV_8()
        {
            XamlObjectWriter objWriter = Setup_SO_SM_V();

            var ns = new NamespaceDeclaration("http://schemas.microsoft.com/winfx/2006/xaml", "xx");

            /* should fail here */
            objWriter.WriteNamespace(ns);

            objWriter.Clear();
            objWriter.Close();
        }

        [TestXaml, TestExpectedException(typeof(XamlObjectWriterException))]
        const string DuplicateBaseUri = @"
<c:ArrayList xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'
xmlns:s='clr-namespace:System;assembly=mscorlib'
xmlns:c='clr-namespace:System.Collections;assembly=mscorlib'>
<s:String xml:base='foo'>Hello</s:String>
</c:ArrayList>";

        [TestMethod]
        public void TestBaseUri()
        {
            string xaml = @"
<c:ArrayList xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'
xmlns:s='clr-namespace:System;assembly=mscorlib'
xmlns:c='clr-namespace:System.Collections;assembly=mscorlib'>
<s:String>Hello</s:String>
</c:ArrayList>";
            MemoryStream ms = new MemoryStream();
            using (StreamWriter sw = new StreamWriter(ms))
            {
                sw.Write(xaml);
                sw.Flush();
                ms.Position = 0;
                XamlXmlReaderSettings settings = new XamlXmlReaderSettings();
                settings.BaseUri = new Uri("http://schemas.microsoft.com/winfx/2006/xaml");
                XamlXmlReader xxr = new XamlXmlReader(ms, settings);
                XamlObjectWriter objWriter = new XamlObjectWriter(xxr.SchemaContext);

                while (xxr.Read())
                {
                    objWriter.WriteNode(xxr);
                }
                
                objWriter.Clear();
                objWriter.Close();
            }
        }

        [TestMethod]
        public void TestBaseUri2()
        {
            string xaml = @"
<c:ArrayList xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'
xmlns:s='clr-namespace:System;assembly=mscorlib'
xmlns:c='clr-namespace:System.Collections;assembly=mscorlib' xml:base='http://schemas.microsoft.com/winfx/2006/xaml'>
<s:String>Hello</s:String>
</c:ArrayList>";
            MemoryStream ms = new MemoryStream();
            using (StreamWriter sw = new StreamWriter(ms))
            {
                sw.Write(xaml);
                sw.Flush();
                ms.Position = 0;
                XamlXmlReaderSettings settings = new XamlXmlReaderSettings();
                XamlXmlReader xxr = new XamlXmlReader(ms, settings);
                XamlObjectWriter objWriter = new XamlObjectWriter(xxr.SchemaContext);

                while (xxr.Read())
                {
                    objWriter.WriteNode(xxr);
                }

                objWriter.Clear();
                objWriter.Close();
            }
        }

        [TestMethod]
        public void TestBaseUri3()
        {
            string xaml = @"
<c:ArrayList xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'
xmlns:s='clr-namespace:System;assembly=mscorlib'
xmlns:c='clr-namespace:System.Collections;assembly=mscorlib' xml:base='http://schemas.microsoft.com/winfx/2006/xaml'>
<s:String>Hello</s:String>
</c:ArrayList>";
            TextReader textReader = new StringReader(xaml);
            XamlXmlReaderSettings settings = new XamlXmlReaderSettings();
            XamlXmlReader xxr = new XamlXmlReader(textReader, settings);
            XamlObjectWriter objWriter = new XamlObjectWriter(xxr.SchemaContext);

            while (xxr.Read())
            {
                objWriter.WriteNode(xxr);
            }
            objWriter.Clear();
            objWriter.Close();
        }

        [TestMethod]
        public void EndMemberCheck()
        {
            var c = new XamlSchemaContext();
            var w = new XamlObjectWriter(c);
            w.WriteStartObject(new XamlType(typeof(Button), c));
            w.WriteStartMember(new XamlMember(typeof(Button).GetProperty("Content"), c));
            w.WriteValue(0);
            w.Clear();
            w.WriteStartObject(new XamlType(typeof(Button), c));
        }
    }
}
