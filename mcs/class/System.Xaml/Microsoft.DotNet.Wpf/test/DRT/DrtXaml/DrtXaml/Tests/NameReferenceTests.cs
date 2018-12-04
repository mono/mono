using System;
using System.Collections.Generic;
using System.Xaml;
using DRT;
using DrtXaml.XamlTestFramework;
using Test.Elements;
using System.Xml;
using System.IO;
using System.Linq;
using DrtXaml.Tests;

namespace DrtXaml
{
    [TestClass]
    public sealed class NameReferenceTests : XamlTestSuite
    {
        private static XamlSchemaContext xsc = new XamlSchemaContext();

        public NameReferenceTests()
            : base("NameReferenceTests")
        {
        }

        public override DrtTest[] PrepareTests()
        {
            DrtTest[] tests = DrtTestFinder.FindTests(this);
            return tests;
        }

        [TestXaml, TestTreeValidator("Validator1")]
        const string Backward = @"
<Object10i xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses'
                     xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
<Object10i.Object0>
      <ColorElement x:Name='_redName' ColorName='Red' />
</Object10i.Object0>
<Object10i.Object1>
      <ColorElement x:Name='_blueName' ColorName='Blue' />
</Object10i.Object1>
<Object10i.Object2>
      <ColorElement x:Name='_greenName' ColorName='Green' />
</Object10i.Object2>
<Object10i.Object3>
      <ColorElement x:Name='_whiteName' ColorName='White' />
</Object10i.Object3>
<Object10i.Object4>
      <ColorElement x:Name='_blackName' ColorName='Black' />
</Object10i.Object4>

<!-- Test simple use of Backward ref in all four syntax -->
<Object10i.Object9>
  <Element10i Element0='{x:Reference _redName}'  Element1='{x:Reference Name=_blueName}'>
    <Element10i.Element2>
      <x:Reference Name='_greenName' />
    </Element10i.Element2>
  </Element10i>
</Object10i.Object9>
<Object10i.Object8>
  <ReferenceHolder Object='_whiteName' />
</Object10i.Object8>
<Object10i.Object7>
  <ColorNameRef>_blackName</ColorNameRef>
</Object10i.Object7>
</Object10i>";


        [TestXaml, TestTreeValidator("Validator1")]
        const string Forward = @"
<Object10i xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses'
                     xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
<!-- Test simple use of Forward ref in all five syntax  -->
<Object10i.Object9>
  <Element10i Element0='{x:Reference _redName}'  Element1='{x:Reference Name=_blueName}'>
    <Element10i.Element2>
      <x:Reference Name='_greenName' />
    </Element10i.Element2>
  </Element10i>
</Object10i.Object9>
<Object10i.Object8>
  <ReferenceHolder Object='_whiteName' />
</Object10i.Object8>
<Object10i.Object7>
  <ColorNameRef>_blackName</ColorNameRef>
</Object10i.Object7>


<Object10i.Object0>
      <ColorElement x:Name='_redName' ColorName='Red' />
</Object10i.Object0>
<Object10i.Object1>
      <ColorElement x:Name='_blueName' ColorName='Blue' />
</Object10i.Object1>
<Object10i.Object2>
      <ColorElement x:Name='_greenName' ColorName='Green' />
</Object10i.Object2>
<Object10i.Object3>
      <ColorElement x:Name='_whiteName' ColorName='White' />
</Object10i.Object3>
<Object10i.Object4>
      <ColorElement x:Name='_blackName' ColorName='Black' />
</Object10i.Object4>
</Object10i>";

        public void Validator1(object o)
        {
            var root = (Test.Elements.Object10i)o;
            var elems = (Test.Elements.Element10i)root.Object9;
            if (!root.HasEndInited || !elems.HasEndInited)
                throw new Exception("EndInit was not called on all objects.");

            Test.Elements.ColorElement colorElement;
            colorElement = (Test.Elements.ColorElement)elems.Element0;
            if (colorElement != root.Object0 || colorElement.ColorName != "Red")
                throw new Exception("Bad Reference,  not 'Red'");

            colorElement = (Test.Elements.ColorElement)elems.Element1;
            if (colorElement != root.Object1 || colorElement.ColorName != "Blue")
                throw new Exception("Bad Reference,  not 'Blue'");

            colorElement = (Test.Elements.ColorElement)elems.Element2;
            if (colorElement != root.Object2 || colorElement.ColorName != "Green")
                throw new Exception("Bad Reference,  not 'Green'");

            var refHolder = (Test.Elements.ReferenceHolder)root.Object8;
            colorElement = (Test.Elements.ColorElement)refHolder.Object;
            if (colorElement != root.Object3 || colorElement.ColorName != "White")
                throw new Exception("Bad Reference,  not 'White'");

            colorElement = (Test.Elements.ColorElement)root.Object7;
            if (colorElement != root.Object4 || colorElement.ColorName != "Black")
                throw new Exception("Bad Reference,  not 'Black'");

        }


        [TestXaml, TestTreeValidator("Validator2")]
        const string ForwardReparseTCAttributeForm = @"
<Object10i xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses'
                     xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
<Object10i.Object6>
  <Number Value='first + second' />
</Object10i.Object6>

<Object10i.Object7>
  <Number x:Name='first' Value='42' />
</Object10i.Object7>

<Object10i.Object8>
  <Number x:Name='second' Value='18' />
</Object10i.Object8>

</Object10i>
";

        [TestXaml, TestTreeValidator("Validator2")]
        const string ForwardReparseMEAttributeForm = @"
<Object10i xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses'
                     xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
<Object10i.Object6>
  <Number Value='{Math Expression=first + second}' />
</Object10i.Object6>

<Object10i.Object7>
  <Number x:Name='first' Value='42' />
</Object10i.Object7>

<Object10i.Object8>
  <Number x:Name='second' Value='18' />
</Object10i.Object8>

</Object10i>
";

        public void Validator2(object o)
        {
            Number n = (Number)((Object10i)o).Object6;
            Assert.AreEqual(60.0, n.Value);
        }


        [TestXaml, TestTreeValidator("Validator3")]
        const string Reference2_BackwardWithNestedScope = @"
<ElementHolderWithNameScope xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses'
                     xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>

<!-- Test simple use of Backward ref when name ref is deeper in the tree -->
<ElementHolderWithNameScope.Element>
  <Element10>
    <Element10.Element0>
      <ElementHolderWithNameScope>  <!-- Name Scope should mask this element -->
        <ElementHolderWithNameScope.Element>
          <Element x:Name='_title2Element' />
        </ElementHolderWithNameScope.Element>
      </ElementHolderWithNameScope>
    </Element10.Element0>
    <Element10.Element2>
      <HoldsOneElement x:Name='_nest1'>
        <HoldsOneElement x:Name='_nest2'>
          <HoldsOneElement x:Name='_nest3'>
            <Element10>
              <Element10.Element0>
                <ElementWithTitle Title='The Title' x:Name='_titleElement' />
              </Element10.Element0>
              <Element10.Element1>
                <ElementWithTitle Title='The 2nd Title' x:Name='_title2Element' />
              </Element10.Element1>
            </Element10>
          </HoldsOneElement>
        </HoldsOneElement>
      </HoldsOneElement>
    </Element10.Element2>
    <Element10.Element3>
    
    </Element10.Element3>

    <Element10.Element4>
      <HoldsOneElement x:Name='_refUser' Element='{x:Reference _title2Element}' />
    </Element10.Element4>
  </Element10>
</ElementHolderWithNameScope.Element>
</ElementHolderWithNameScope>";

        public void Validator3(object o)
        {
            var root = (Test.Elements.ElementHolderWithNameScope)o;
            var nameScope = o as System.Windows.Markup.INameScope;
            var holder = nameScope.FindName("_refUser");
            if (holder == null)
                throw new NullReferenceException("Object10.Object0 should not be null");

            var top10 = (Test.Elements.Element10)root.Element;
            var holder0 = (Test.Elements.HoldsOneElement)top10.Element0;
            var title21 = (Test.Elements.Element)holder0.Element;

            var holder2 = (Test.Elements.HoldsOneElement)top10.Element2;
            var holder3 = (Test.Elements.HoldsOneElement)holder2.Element;
            var holder4 = (Test.Elements.HoldsOneElement)holder3.Element;
            var lower10 = (Test.Elements.Element10)holder4.Element;
            var title1 = lower10.Element0;
            var title22 = lower10.Element1;
            var title23 = top10.Element4;

            var holderLast = (Test.Elements.HoldsOneElement)top10.Element4;
            if (holderLast.Element == null)
                throw new NullReferenceException("Object10.Object1 should not be null");

            if (holderLast.Element != title22)
                throw new InvalidProgramException("Reference didn't return the right value");
        }

        // Tests complex use of a user written Name Reference Converter.
        // Multiple backward references.
        [TestXaml]
        const string MathBackward = @"
<Object10i xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses'
           xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
  <Object10i.Object4>
              <Number x:Name='first' Value='42' />
  </Object10i.Object4>

  <Object10i.Object7>
              <Number x:Name='second' Value='18' />
  </Object10i.Object7>

  <Object10i.Object6>
              <Number x:Name='final'>60 == first + second</Number>
  </Object10i.Object6>
</Object10i>
";

        [TestXaml]
        const string MathBackwardMe = @"
<Object10i xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses'
           xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
  <Object10i.Object4>
              <Number x:Name='first' Value='42' />
  </Object10i.Object4>

  <Object10i.Object7>
              <Number x:Name='second' Value='18' />
  </Object10i.Object7>

  <Object10i.Object5>
              <Number x:Name='result' Value='{Math Expression=first + second}' />
  </Object10i.Object5>

  <Object10i.Object6>
              <Number x:Name='final'>60 == result</Number>
  </Object10i.Object6>
</Object10i>
";

        [TestXaml]
        const string MathBackwardMeTc = @"
<Object10i xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses'
           xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
  <Object10i.Object4>
              <Number x:Name='first' Value='42' />
  </Object10i.Object4>

  <Object10i.Object7>
              <Number x:Name='second' Value='18' />
  </Object10i.Object7>

  <Object10i.Object5>
              <Number x:Name='result' Value='{MathTc Expression=first + second}' />
  </Object10i.Object5>

  <Object10i.Object6>
              <Number x:Name='final'>60 == result</Number>
  </Object10i.Object6>
</Object10i>
";

        [TestXaml]
        const string MathBackwardTopDown = @"
<Object10i xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses'
           xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
  <Object10i.Object4>
              <NumberTopDown x:Name='first' Value='42' />
  </Object10i.Object4>

  <Object10i.Object7>
              <NumberTopDown x:Name='second' Value='18' />
  </Object10i.Object7>

  <Object10i.Object6>
              <NumberTopDown x:Name='final'>60 == first + second</NumberTopDown>
  </Object10i.Object6>
</Object10i>
";

        // Multiple backward references.
        [TestXaml]
        const string MathBackwardList = @"
<Object10i xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses'
           xmlns:g='clr-namespace:System.Collections.Generic;assembly=mscorlib'
           xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
  <Object10i.Object1>
      <g:List x:TypeArguments='Number'>
              <Number x:Name='first' Value='42' />
              <Number x:Name='second' Value='18' />
              <Number x:Name='final'>60 == first + second</Number>
      </g:List>
  </Object10i.Object1>
</Object10i>
";

        [TestXaml]
        const string MathBackwardListTopDown = @"
<Object10i xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses'
           xmlns:g='clr-namespace:System.Collections.Generic;assembly=mscorlib'
           xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
  <Object10i.Object1>
      <g:List x:TypeArguments='Number'>
              <NumberTopDown x:Name='first' Value='42' />
              <NumberTopDown x:Name='second' Value='18' />
              <NumberTopDown x:Name='final'>60 == first + second</NumberTopDown>
      </g:List>
  </Object10i.Object1>
</Object10i>
";

        [TestXaml]
        const string MathForward = @"
<Object10i xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses'
           xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
  <Object10i.Object6>
              <Number x:Name='final'>60 == first + second</Number>
  </Object10i.Object6>

  <Object10i.Object4>
              <Number x:Name='first' Value='42' />
  </Object10i.Object4>

  <Object10i.Object7>
              <Number x:Name='second' Value='18' />
  </Object10i.Object7>
</Object10i>
";

        [TestXaml]
        const string MathForwardMe = @"
<Object10i xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses'
           xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
  <Object10i.Object5>
              <Number x:Name='result' Value='{Math Expression=first + second}' />
  </Object10i.Object5>

  <Object10i.Object4>
              <Number x:Name='first' Value='42' />
  </Object10i.Object4>

  <Object10i.Object7>
              <Number x:Name='second' Value='18' />
  </Object10i.Object7>

  <Object10i.Object6>
              <Number x:Name='final'>60 == result</Number>
  </Object10i.Object6>
</Object10i>
";

        [TestXaml]
        const string MathForwardMeTc = @"
<Object10i xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses'
           xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
  <Object10i.Object5>
              <Number x:Name='result' Value='{MathTc Expression=first + second}' />
  </Object10i.Object5>

  <Object10i.Object4>
              <Number x:Name='first' Value='42' />
  </Object10i.Object4>

  <Object10i.Object7>
              <Number x:Name='second' Value='18' />
  </Object10i.Object7>

  <Object10i.Object6>
              <Number x:Name='final'>60 == result</Number>
  </Object10i.Object6>
</Object10i>
";

        [TestXaml]
        const string MathForwardTopDown = @"
<Object10i xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses'
           xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
  <Object10i.Object6>
              <NumberTopDown x:Name='final'>60 == first + second</NumberTopDown>
  </Object10i.Object6>

  <Object10i.Object4>
              <NumberTopDown x:Name='first' Value='42' />
  </Object10i.Object4>

  <Object10i.Object7>
              <NumberTopDown x:Name='second' Value='18' />
  </Object10i.Object7>
</Object10i>
";
        [TestXaml]
        const string MathForwardList = @"
<Object10i xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses'
           xmlns:g='clr-namespace:System.Collections.Generic;assembly=mscorlib'
           xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
  <Object10i.Object1>
      <g:List x:TypeArguments='Number'>
              <Number x:Name='final'>60 == first + second</Number>
              <Number x:Name='first' Value='42' />
              <Number x:Name='second' Value='18' />
      </g:List>
  </Object10i.Object1>
</Object10i>
";

        [TestXaml]
        const string MathForwardListTopDown = @"
<Object10i xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses'
           xmlns:g='clr-namespace:System.Collections.Generic;assembly=mscorlib'
           xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
  <Object10i.Object1>
       <g:List x:TypeArguments='Number'>
              <NumberTopDown x:Name='final'>60 == first + second</NumberTopDown>
              <NumberTopDown x:Name='first' Value='42' />
              <NumberTopDown x:Name='second' Value='18' />
      </g:List>
  </Object10i.Object1>
</Object10i>
";

        // Multiple forward and backward references to top-down objects.
        [TestXaml]
        const string MathMixedListTopDown = @"
<Object10i xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses'
           xmlns:g='clr-namespace:System.Collections.Generic;assembly=mscorlib'
           xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
  <Object10i.Object1>
       <g:List x:TypeArguments='Number'>
             <NumberTopDown x:Name='first' Value='42' />
              <NumberTopDown x:Name='final'>60 == first + second</NumberTopDown>
              <NumberTopDown x:Name='second' Value='18' />
      </g:List>
  </Object10i.Object1>
</Object10i>
";

        [TestKnownFailure("bchapman, Backward references to forward references")]
        [TestXaml]
        const string MathBackwardRefToUnresolvedForwardRef = @"
<Object10i xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses'
           xmlns:g='clr-namespace:System.Collections.Generic;assembly=mscorlib'
           xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
  <Object10i.Object1>
      <g:List x:TypeArguments='Number'>
              <Number x:Name='first' Value='42' />
              <Number x:Name='final'>first + second</Number>
              <Number Value='final == 60' />
              <Number x:Name='second' Value='18' />
      </g:List>
  </Object10i.Object1>
</Object10i>
";

        [TestXaml]
        const string MathForwardRefToUnresolvedForwardRef = @"
<Object10i xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses'
           xmlns:g='clr-namespace:System.Collections.Generic;assembly=mscorlib'
           xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
  <Object10i.Object1>
      <g:List x:TypeArguments='Number'>
              <Number x:Name='A' Value='B' />
              <Number x:Name='B' Value='C' />
              <Number x:Name='C' Value='5' />
              <Number Value='A == 5' />
      </g:List>
  </Object10i.Object1>
</Object10i>
";

        [TestXaml, TestTreeValidator("Validator4")]
        [TestAlternateXamlLoader("LoadWithProvidedNameScope")]
        const string ToProvidedName = @"
<HoldsOneElement xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses'
               xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
  <x:Reference>InjectedName</x:Reference>
</HoldsOneElement>";

        [TestXaml, TestTreeValidator("Validator4")]
        [TestAlternateXamlLoader("LoadWithProvidedNameScope")]
        const string ToProvidedNameWithRootNameScope = @"
<ElementHolderWithNameScope xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses'
               xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
  <x:Reference>InjectedName</x:Reference>
</ElementHolderWithNameScope>";

        public object LoadWithProvidedNameScope(string xaml)
        {
            Element elem = new ColorElement { ColorName = "Red" };
            return LoadWithInjectedName(xaml, "InjectedName", elem);
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

        public void Validator4(object o)
        {
            HoldsOneElement holder = (HoldsOneElement)o;
            ColorElement color = (ColorElement)holder.Element;
            Assert.AreEqual("Red", color.ColorName);
        }

        [TestXaml, TestTreeValidator("ListValidator1")]
        const string List01 = @"
<Object10i xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses'
           xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'
           xmlns:scg='clr-namespace:System.Collections.Generic;assembly=mscorlib'>
<Object10i.Object1>
  <scg:List x:TypeArguments='Element'>
    <ColorElement x:Name='_redName' ColorName='Red' />
    <!-- Basic fwd ref -->
    <x:Reference Name='_greenName' />
    <ColorElement x:Name='_blueName' ColorName='Blue' />
    <!-- Another fwd ref in the same list -->
    <x:Reference Name='_whiteName' />
    <!-- Duplicate fwd ref in the same list -->
    <x:Reference Name='_greenName' />
    <!-- Fwd ref to an item within the list -->
    <x:Reference Name='_blackName' />
    <ColorElement x:Name='_blackName' ColorName='Black' />
  </scg:List>
</Object10i.Object1>
<Object10i.Object2>
      <ColorElement x:Name='_greenName' ColorName='Green' />
</Object10i.Object2>
<Object10i.Object3>
      <ColorElement x:Name='_whiteName' ColorName='White' />
</Object10i.Object3>
</Object10i>";

        public void ListValidator1(object o)
        {
            var root = (Object10i)o;
            var list = (List<Element>)root.Object1;
            var nameList = list.Cast<ColorElement>().Select(ce => ce.ColorName).ToList();
            Assert.AreEqualOrdered(nameList, "Red", "Green", "Blue", "White", "Green", "Black", "Black");
        }

        [TestXaml, TestTreeValidator("ListValidator2")]
        const string List02 = @"
<!-- All references resolved within the list -->
<Object10i xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses'
           xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'
           xmlns:scg='clr-namespace:System.Collections.Generic;assembly=mscorlib'>
<Object10i.Object1>
  <scg:List x:TypeArguments='Element'>
    <x:Reference Name='_greenName' />
    <x:Reference Name='_whiteName' />
    <HoldsOneElement>
      <ColorElement x:Name='_greenName' ColorName='Green' />
    </HoldsOneElement>
    <ColorElement x:Name='_whiteName' ColorName='White' />
  </scg:List>
</Object10i.Object1>
</Object10i>";

        public void ListValidator2(object o)
        {
            var root = (Object10i)o;
            var list = (List<Element>)root.Object1;
            Assert.AreEqual(4, list.Count);
            Assert.AreEqual(list[1], list[3]);
            Element containedElement = ((HoldsOneElement)list[2]).Element;
            Assert.AreEqual(containedElement, list[0]);
        }

        [TestXaml, TestTreeValidator("DictValidator")]
        const string Dict = @"
<Object10i xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses'
           xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'
           xmlns:sys='clr-namespace:System;assembly=mscorlib'
           xmlns:scg='clr-namespace:System.Collections.Generic;assembly=mscorlib'>
<Object10i.Object1>
  <OrderedDictionary x:TypeArguments='sys:Object, Resource'>
    <Resource SomeData='Foo' x:Key='FooKey' />
    <!-- Fwd ref outside list, with explicit key -->
    <x:Reference Name='Bar' x:Key='BarKey' />
    <!-- Fwd ref outside list, with implicit key -->
    <x:Reference Name='Baz' />
    <!-- Duplicate item, with explicit key -->
    <x:Reference Name='Baz' x:Key='BazKey2' />
    <!-- Fwd ref inside list, with implicit key -->
    <x:Reference Name='Qux' />
    <ResourceWithImplictKey x:Name='Qux' SomeData='Qux' MyKey='QuxKey' x:Key='QuxKey2' />
    <ResourceWithImplictKey SomeData='Quux' MyKey='QuuxKey' />
  </OrderedDictionary>
</Object10i.Object1>
<Object10i.Object2>
      <ResourceWithImplictKey x:Name='Bar' SomeData='Bar' MyKey='BarKey2' />
</Object10i.Object2>
<Object10i.Object3>
      <ResourceWithImplictKey x:Name='Baz' SomeData='Baz' MyKey='BazKey' />
</Object10i.Object3>
</Object10i>";

        [TestXaml, TestTreeValidator("DictValidator")]
        const string DictWithKeyRefs = @"
<Object10i xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses'
           xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'
           xmlns:sys='clr-namespace:System;assembly=mscorlib'
           xmlns:scg='clr-namespace:System.Collections.Generic;assembly=mscorlib'>
<Object10i.Object1>
  <!-- Same as previous, but with all keys converted to refs -->
  <OrderedDictionary x:TypeArguments='sys:Object, Resource'>
    <Resource SomeData='Foo' x:Key='{x:Reference FooKey}' />
    <!-- Fwd ref outside list, with explicit key -->
    <x:Reference Name='Bar' x:Key='{x:Reference BarKey}' />
    <!-- Fwd ref outside list, with implicit key -->
    <x:Reference Name='Baz' />
    <!-- Duplicate item, with explicit key -->
    <x:Reference Name='Baz' x:Key='{x:Reference BazKey2}' />
    <!-- Fwd ref inside list, with implicit key -->
    <x:Reference Name='Qux' />
    <ResourceWithImplictKey x:Name='Qux' SomeData='Qux' MyKey='QuxKey' x:Key='{x:Reference QuxKey2}' />
    <ResourceWithImplictKey SomeData='Quux' MyKey='QuuxKey' />
  </OrderedDictionary>
</Object10i.Object1>

<Object10i.Object2>
      <ResourceWithImplictKey x:Name='Bar' SomeData='Bar' MyKey='BarKey2' />
</Object10i.Object2>
<Object10i.Object3>
      <ResourceWithImplictKey x:Name='Baz' SomeData='Baz' MyKey='BazKey' />
</Object10i.Object3>
<Object10i.Object4>
      <StringKey x:Name='FooKey'>FooKey</StringKey>
</Object10i.Object4>
<Object10i.Object5>
      <StringKey x:Name='BarKey'>BarKey</StringKey>
</Object10i.Object5>
<Object10i.Object6>
      <StringKey x:Name='BazKey2'>BazKey2</StringKey>
</Object10i.Object6>
<Object10i.Object7>
      <StringKey x:Name='QuxKey2'>QuxKey2</StringKey>
</Object10i.Object7>
<Object10i.Object8>
      <StringKey x:Name='QuxKey'>QuxKey</StringKey>
</Object10i.Object8>
<Object10i.Object9>
      <StringKey x:Name='QuuxKey'>QuuxKey</StringKey>
</Object10i.Object9>
</Object10i>";

        [TestXaml, TestTreeValidator("DictValidator")]
        static string DictWithKeyRefs2 = DictWithKeyRefs.Replace("MyKey='QuuxKey'", "MyKey='{x:Reference QuuxKey}'");

        [TestXaml, TestTreeValidator("DictValidator")]
        static string DictWithKeyRefs3 = DictWithKeyRefs.Replace("MyKey='QuxKey'", "MyKey='{x:Reference QuxKey}'");

        public void DictValidator(object o)
        {
            var root = (Object10i)o;
            var dict = (OrderedDictionary<Object, Resource>)root.Object1;
            var keys = dict.Select(kvp => kvp.Key.ToString()).ToList();
            Assert.AreEqualOrdered(keys, "FooKey", "BarKey", "BazKey", "BazKey2", "QuxKey", "QuxKey2", "QuuxKey");
            var values = dict.Select(kvp => kvp.Value.SomeData).ToList();
            Assert.AreEqualOrdered(values, "Foo", "Bar", "Baz", "Baz", "Qux", "Qux", "Quux");
        }

        [TestXaml, TestTreeValidator("Validator5")]
        const string KeyRefWithinItem = @"
<OrderedDictionary x:TypeArguments='StringKey, ResourceWithNameableKey'
                   xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses'
                   xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'
                   xmlns:scg='clr-namespace:System.Collections.Generic;assembly=mscorlib'>
  <ResourceWithNameableKey x:Key='{x:Reference FooKey}'>
    <ResourceWithNameableKey.SomeData2>
      <StringKey x:Name='FooKey'>FooKey</StringKey>
    </ResourceWithNameableKey.SomeData2>
  </ResourceWithNameableKey>
</OrderedDictionary>";

        // TODO, 555215, enable this test once the XamlObjectWriter supports directives that come after other members
        [TestXaml, TestTreeValidator("Validator5")]
        [TestKnownFailure("dglick")]
        const string KeyRefInElementFormWithinItem = @"
<OrderedDictionary x:TypeArguments='StringKey, ResourceWithNameableKey'
                   xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses'
                   xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'
                   xmlns:scg='clr-namespace:System.Collections.Generic;assembly=mscorlib'>
  <ResourceWithNameableKey SomeData2='FooKey'>
    <x:Key>
      <x:Reference>FooKey</x:Reference>
    </x:Key>
    <ResourceWithNameableKey.MyKey>
      <StringKey x:Name='FooKey'>FooKey</StringKey>
    </ResourceWithNameableKey.MyKey>
  </ResourceWithNameableKey>
</OrderedDictionary>";

        public void Validator5(object o)
        {
            var dict = (OrderedDictionary<StringKey, ResourceWithNameableKey>)o;
            var keys = dict.Select(kvp => kvp.Key.ToString()).ToList();
            Assert.AreEqualOrdered(keys, "FooKey");
            var values = dict.Select(kvp => kvp.Value.SomeData2.ToString()).ToList();
            Assert.AreEqualOrdered(values, "FooKey");
        }

        [TestXaml]
        [TestExpectedException(typeof(XamlObjectWriterException))]
        const string UnresolvedSimple = @"
<Object10i xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses'
                     xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
<Object10i.Object1>
  <x:Reference>DoesNotExist</x:Reference>
</Object10i.Object1>

</Object10i>
";

        public object ExceptionUnwrappingLoader(string xamlString)
        {
            try
            {
                object result = XamlServices.Parse(xamlString);
                return result;
            }
            catch (Exception ex)
            {
                if (ex.InnerException == null)
                {
                    // Can't use Assert.Fail here because it throws InvalidOperationException,
                    // which might be what we're actually expecting
                    throw new Exception("Expected an InnerException");
                }
                throw ex.InnerException;
            }
        }

        [TestXaml, TestAlternateXamlLoader("ExceptionUnwrappingLoader")]
        [TestExpectedException(typeof(InvalidOperationException))]
        const string UnresolvedReparse = @"
<Object10i xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses'
                     xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
<Object10i.Object6>
  <Number>first + second</Number>
</Object10i.Object6>

<Object10i.Object7>
  <Number x:Name='second' Value='18' />
</Object10i.Object7>

</Object10i>
";

        [TestXaml, TestTreeValidator("TransitiveEndInitValidator")]
        const string TransitiveEndInit = @"
<NestedNumber xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses'
              xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
  <NestedNumber.Number1>
    <NestedNumber>
       <NestedNumber.Number1>
         <NestedNumber>
           <NestedNumber.Number1>
             <Number Value='ref' />
           </NestedNumber.Number1>
         </NestedNumber>
       </NestedNumber.Number1>
    </NestedNumber>
  </NestedNumber.Number1>
  <NestedNumber.Number2>
    <Number x:Name='ref' Value='42' />
  </NestedNumber.Number2>
</NestedNumber>";

        public void TransitiveEndInitValidator(object o)
        {
            Assert.AreEqual(84, ((Number)o).Value);
        }

        [TestXaml, TestTreeValidator("DeferredProvideValueValidator")]
        const string DeferredProvideValue = @"
<NestedNumber xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses'
              xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
  <NestedNumber.Number1>
    <MathPlusExtension Expression='77'>
       <MathPlusExtension.AdditionalValue>
         <Number Value='ref' />
       </MathPlusExtension.AdditionalValue>
    </MathPlusExtension>
  </NestedNumber.Number1>
  <NestedNumber.Number2>
    <MathPlusExtension Expression='ref2'>
       <MathPlusExtension.AdditionalValue>
         <Number Value='ref' />
       </MathPlusExtension.AdditionalValue>
    </MathPlusExtension>
  </NestedNumber.Number2>
  <NestedNumber.Number3>
    <NestedNumber>
      <NestedNumber.Number1>
        <Number x:Name='ref' Value='68' />
      </NestedNumber.Number1>
      <NestedNumber.Number2>
        <Number x:Name='ref2' Value='18' />
      </NestedNumber.Number2>
    </NestedNumber>
  </NestedNumber.Number3>
</NestedNumber>";

        public void DeferredProvideValueValidator(object o)
        {
            Assert.AreEqual(317, ((Number)o).Value);
        }

        [TestXaml, TestTreeValidator("BackRefToFwdRefValidator")]
        const string BackRefToFwdRef = @"
<NestedNumber xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses'
              xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
  <NestedNumber.Number1>
    <Number x:Name='A' Value='B' />
  </NestedNumber.Number1>
  <NestedNumber.Number2>
    <Number Value='A' />
  </NestedNumber.Number2>
  <NestedNumber.Number3>
    <Number x:Name='B' Value='9' />
  </NestedNumber.Number3>
</NestedNumber>";

        public void BackRefToFwdRefValidator(object o)
        {
            Assert.AreEqual(27, ((Number)o).Value);
        }

        [TestXaml, TestTreeValidator("DeferredProvideValueAtRootValidator")]
        const string DeferredProvideValueAtRoot = @"
<GenericMarkup x:TypeArguments='Element'
               xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses'
               xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
  <GenericMarkup.Value>
    <Element10>
      <Element10.Element0>
        <HoldsOneElement x:Name='A'>
          <x:Reference>B</x:Reference>
        </HoldsOneElement>
      </Element10.Element0>
      <Element10.Element1>
        <HoldsOneElement x:Name='B'>
          <x:Reference>A</x:Reference>
        </HoldsOneElement>
      </Element10.Element1>
    </Element10>
  </GenericMarkup.Value>
</GenericMarkup>";

        public void DeferredProvideValueAtRootValidator(object o)
        {
            Element10 e10 = (Element10)o;
            Assert.AreEqual(e10.Element0, ((HoldsOneElement)e10.Element1).Element);
            Assert.AreEqual(e10.Element1, ((HoldsOneElement)e10.Element0).Element);
        }

        [TestXaml, TestExpectedException(typeof(XamlObjectWriterException))]
        const string ProvideValueCycle = @"
<Number x:Name='A'
        xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses'
        xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
  <Number.Value>
    <MathPlusExtension Expression='0'>
      <MathPlusExtension.AdditionalValue>
        <x:Reference>A</x:Reference>
      </MathPlusExtension.AdditionalValue>
    </MathPlusExtension>
  </Number.Value>
</Number>";

        [TestXaml, TestExpectedException(typeof(XamlObjectWriterException))]
        const string UnresolvedArgument = @"
<NestedNumber xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses'
              xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
  <NestedNumber.Number1>
    <Number>
      <x:Arguments>
        <NestedNumber>
          <NestedNumber.Number1>
            <Number>
              <x:Reference>ref</x:Reference>
            </Number>
          </NestedNumber.Number1>
        </NestedNumber>
      </x:Arguments>
    </Number>
  </NestedNumber.Number1>
  <NestedNumber.Number2>
    <Number x:Name='ref' Value='42' />
  </NestedNumber.Number2>
</NestedNumber>";

        [TestXaml, TestTreeValidator("ForwardRefToTCValidator")]
        const string ForwardRefToTC = @"
<NestedNumber xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses'
              xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
  <NestedNumber.Number1>
    <ConvertibleNumber>A</ConvertibleNumber>
  </NestedNumber.Number1>
  <NestedNumber.Number2>
    <ConvertibleNumber x:Name='A'>42</ConvertibleNumber>
  </NestedNumber.Number2>
</NestedNumber>";

        public void ForwardRefToTCValidator(object o)
        {
            Assert.AreEqual(84, ((Number)o).Value);
        }

        [TestXaml, TestExpectedException(typeof(XamlObjectWriterException))]
        const string AttachedPropOnForwardRef = @"
<NestedNumber xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses'
              xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
  <NestedNumber.Number1>
    <ConvertibleNumber APP.Foo='Foo'>A</ConvertibleNumber>
  </NestedNumber.Number1>
  <NestedNumber.Number2>
    <Number x:Name='A' Value='42' />
  </NestedNumber.Number2>
</NestedNumber>";

        [TestXaml, TestAlternateXamlLoader("ExceptionUnwrappingLoader")]
        [TestExpectedException(typeof(InvalidOperationException))]
        const string NotFullyInitialized =
@"<NestedNumber x:Name='A'
                xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses'
                xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
  <NestedNumber.Number1>
    <ConvertibleNumber>A</ConvertibleNumber>
  </NestedNumber.Number1>
</NestedNumber>";

        [TestXaml, TestTreeValidator("FwdRefsInRetrievedObjectsValidator")]
        const string FwdRefsInRetrievedObjects =
@"<Element10 xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses'
             xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
    <Element10.Element0>
      <ElementListHolder>
        <ElementListHolder.Elements>
          <Element />
          <!-- fwd ref within the collection -->
          <x:Reference>A</x:Reference>
          <ElementWithTitle Title='A' x:Name='A' />
        </ElementListHolder.Elements>
      </ElementListHolder>
    </Element10.Element0>
    <Element10.Element1>
      <ElementListHolder>
        <ElementListHolder.Elements>
          <Element />
          <!-- fwd ref outside the collection -->
          <x:Reference>C</x:Reference>
          <ElementWithTitle Title='B' x:Name='B' />
        </ElementListHolder.Elements>
      </ElementListHolder>
    </Element10.Element1>
    <Element10.Element2>
      <ElementWithTitle Title='C' x:Name='C' />
    </Element10.Element2>
  </Element10>";

        public void FwdRefsInRetrievedObjectsValidator(object o)
        {
            Element10 root = (Element10)o;
            ElementListHolder list = (ElementListHolder)root.Element0;
            Assert.AreEqual(list.Elements[1], list.Elements[2]);
            list = (ElementListHolder)root.Element1;
            Assert.AreEqual(list.Elements[1], root.Element2);
        }

        [TestXaml]
        const string CyclicalRefMultipleBranches =
@"<scg:List x:TypeArguments='x:Object'
            xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses'
            xmlns:scg='clr-namespace:System.Collections.Generic;assembly=mscorlib'
            xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
    <HoldsOneElement x:Name='__ReferenceID0'>
      <HoldsOneElement x:Name='__ReferenceID1'>
        <x:Reference>__ReferenceID0</x:Reference>
      </HoldsOneElement>
    </HoldsOneElement>
    <x:Reference>__ReferenceID1</x:Reference>
    <HoldsOneElement x:Name='__ReferenceID3'>
      <x:Reference>__ReferenceID1</x:Reference>
    </HoldsOneElement>
    <HoldsOneElement x:Name='__ReferenceID2'>
      <x:Reference>__ReferenceID1</x:Reference>
    </HoldsOneElement>
    <scg:Dictionary x:TypeArguments='x:Int32,Element'>
      <x:Reference>__ReferenceID3
        <x:Key>0</x:Key>
      </x:Reference>
    </scg:Dictionary>
</scg:List>
";
    }
}
