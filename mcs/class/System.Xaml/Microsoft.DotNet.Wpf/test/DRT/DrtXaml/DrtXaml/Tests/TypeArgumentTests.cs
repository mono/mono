using System;
using System.Collections.Generic;
using System.Text;
using DRT;
using System.Xaml;
using DrtXaml.XamlTestFramework;

namespace DrtXaml.Tests
{
    [TestClass]
    public sealed class TypeArgumentTests: XamlTestSuite
    {
        public TypeArgumentTests()
            : base("TypeArgumentTests")
        {
        }

        public override DrtTest[] PrepareTests()
        {
            DrtTest[] tests = DrtTestFinder.FindTests(this);
            return tests;
        }

        [TestXaml]
        public const string List = @"
<Object10
    xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses'
    xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'
    xmlns:s='clr-namespace:System;assembly=mscorlib'
    xmlns:g='clr-namespace:System.Collections.Generic;assembly=mscorlib'>

  <Object10.Object0>
    <g:List x:TypeArguments='s:String'/>
  </Object10.Object0>

  <Object10.Object1>
    <g:List x:TypeArguments='s:String'>
        <x:String>apple</x:String>
        <x:String>orange</x:String>
    </g:List>
  </Object10.Object1>

  <Object10.Object2>
    <g:List x:TypeArguments='x:String'>
        <s:String>bicycle</s:String>
        <s:String>fish</s:String>
    </g:List>
  </Object10.Object2>
</Object10>
";

        [TestXaml]
        public const string TypeArgsAtTheRoot = @"
<g:List xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'
    xmlns:g='clr-namespace:System.Collections.Generic;assembly=mscorlib'
    xmlns:s='clr-namespace:System;assembly=mscorlib'
    x:TypeArguments='x:String'>
        <x:String>a</x:String>
        <x:String>a</x:String>
</g:List>
";

        [TestXaml]
        public const string TypeArgsOrder = @"
<Object10
    xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses'
    xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'
    xmlns:s='clr-namespace:System;assembly=mscorlib'
>

  <Object10.Object0>
        <!-- Type arguments before the namespace -->
    <g:List x:TypeArguments='s:String' 
            xmlns:g='clr-namespace:System.Collections.Generic;assembly=mscorlib' />
  </Object10.Object0>

  <Object10.Object1>
        <!-- Type arguments after the namespace -->
    <g:List 
            xmlns:g='clr-namespace:System.Collections.Generic;assembly=mscorlib'
            x:TypeArguments='s:String'>
        <x:String>apple</x:String>
        <x:String>orange</x:String>
    </g:List>
  </Object10.Object1>

  <Object10.Object2>
        <!-- Type arguments before the namespace -->
    <g:List
            x:TypeArguments='x:String'
            xmlns:g='clr-namespace:System.Collections.Generic;assembly=mscorlib'>
        <s:String>bicycle</s:String>
        <s:String>fish</s:String>
    </g:List>
  </Object10.Object2>
</Object10>
";

        [TestXaml]
        public const string Dictionary = @"
<Object10
        xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses'
        xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'
        xmlns:g='clr-namespace:System.Collections.Generic;assembly=mscorlib'
        xmlns:s='clr-namespace:System;assembly=mscorlib'>

  <Object10.Object0>
    <g:Dictionary x:TypeArguments='s:String, x:String'>
        <x:String x:Key='hello'>world</x:String>
        <s:String x:Key='hello2'>world2</s:String>
    </g:Dictionary>
  </Object10.Object0>

  <Object10.Object1>
    <g:Dictionary x:TypeArguments='x:String, s:Object'/>
  </Object10.Object1>
</Object10>
";

        [TestXaml]
        public const string Nested = @"
<Object10
        xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses'
        xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'
        xmlns:g='clr-namespace:System.Collections.Generic;assembly=mscorlib'
        xmlns:s='clr-namespace:System;assembly=mscorlib'>

  <Object10.Object0>
    <g:List x:TypeArguments='g:List(s:String)'>
       <g:List x:TypeArguments='s:String'>
          <s:String>Foo</s:String>
          <s:String>Bar</s:String>
          <s:String>Spam</s:String>
       </g:List>
       <g:List x:TypeArguments='s:String'>
          <s:String>Apple</s:String>
          <s:String>Orange</s:String>
          <s:String>Banana</s:String>
       </g:List>
    </g:List>
  </Object10.Object0>

</Object10>
";

        [TestXaml]
        public const string PropertiesOnGenerics = @"
<Element10 
        xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses'>
 <Element10.Element0>
   <GenericElement  x:TypeArguments='x:String'  Color='Red'  Content='Stuff'
        xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses'
        xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml' >
   </GenericElement>
 </Element10.Element0>

<Element10.Element1>
   <GenericElement  x:TypeArguments='x:String'
        xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses'
        xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml' >
     <GenericElement.Color> Green </GenericElement.Color>
     <GenericElement.Content> Junk </GenericElement.Content>
   </GenericElement>
 </Element10.Element1>

 <Element10.Element2>
   <GenericElement  x:TypeArguments='x:String'  GenericElement.Color='Blue' 
        xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses'
        xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml' >
     This is stuff goes in the Generic Content property.
   </GenericElement>
 </Element10.Element2>
</Element10>
";      

        [TestXaml, TestExpectedException(typeof(XamlParseException))]
        public const string BadTypeArg = @"<g:List 
	                                    xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'
	                                    xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'
	                                    xmlns:g='clr-namespace:System.Collections.Generic;assembly=mscorlib'
	                                    xmlns:s='clr-namespace:System;assembly=mscorlib'
                                        x:TypeArguments='{Binding}'/>";

        [TestXaml, TestExpectedException(typeof(XamlParseException))]
        public const string BadTypeArg2 = @"<g:List xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'
                                        xmlns:g='clr-namespace:System.Collections.Generic;assembly=mscorlib'
                                        xmlns:s='clr-namespace:System;assembly=mscorlib'
                                        x:TypeArguments='s:Int, {Binding}'>
                                        </g:List>";

        [TestXaml, TestExpectedException(typeof(XamlParseException))]
        public const string BadTypeArg3 = @"<g:List xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'
                                            xmlns:g='clr-namespace:System.Collections.Generic;assembly=mscorlib'
                                            xmlns:s='clr-namespace:System;assembly=mscorlib'
                                            x:TypeArguments='s:Int(g:Bool'>
                                            </g:List>";

        [TestXaml]
        public const string GenericArray = @"<ArrayHolder xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses'
xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'
xmlns:s='clr-namespace:System.Collections.Generic;assembly=mscorlib'>
<ArrayHolder.MyArray>
<s:List x:TypeArguments='x:Int32'>
<x:Int32>42</x:Int32>
</s:List>
</ArrayHolder.MyArray>
</ArrayHolder>";
    }
}
