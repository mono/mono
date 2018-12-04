using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DrtXaml.XamlTestFramework;
using DRT;
using Test.Elements;
using System.Xaml;

namespace DrtXaml.Tests
{
    [TestClass]
    public sealed class FactoryArgumentTests : XamlTestSuite
    {
        public FactoryArgumentTests()
            : base("FactoryArgumentTests")
        {
        }

        public override DrtTest[] PrepareTests()
        {
            DrtTest[] tests = DrtTestFinder.FindTests(this);
            return tests;
        }

        [TestXaml]  // TODO should validate the tree to confirm the Ctor/Factor was called correctly
        public const string SimpleFactoryAttribute = @"
<t:FactoryMade    x:FactoryMethod='Create'
          xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'
          xmlns:t='clr-namespace:Test.Elements;assembly=XamlTestClasses'/>
";

        [TestXaml]  // TODO should validate the tree to confirm the Ctor/Factor was called correctly
        public const string SimpleCtorArgsPE = @"
<t:FactoryMade
          xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'
          xmlns:s='clr-namespace:System;assembly=mscorlib'
          xmlns:t='clr-namespace:Test.Elements;assembly=XamlTestClasses'>
   <x:Arguments>
      <s:String>hello</s:String>
   </x:Arguments>
</t:FactoryMade>
";

        [TestXaml, TestTreeValidator("Validate_NullCtorArg")] 
        public const string NullCtorArg = @"
<t:FactoryNullable
          xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'
          xmlns:s='clr-namespace:System;assembly=mscorlib'
          xmlns:t='clr-namespace:Test.Elements;assembly=XamlTestClasses'>
   <x:Arguments>
      <x:Null />
   </x:Arguments>
</t:FactoryNullable>
";

        public void Validate_NullCtorArg(object o)
        {
            var obj = (FactoryNullable)o;
            Assert.IsFalse(obj.Value.HasValue);
        }

        [TestXaml, TestTreeValidator("Validate_NullableCtorArg")]
        public const string NullableCtorArg = @"
<t:FactoryNullable
          xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'
          xmlns:s='clr-namespace:System;assembly=mscorlib'
          xmlns:t='clr-namespace:Test.Elements;assembly=XamlTestClasses'>
   <x:Arguments>
      <x:Int32>42</x:Int32>
   </x:Arguments>
</t:FactoryNullable>
";

        public void Validate_NullableCtorArg(object o)
        {
            var obj = (FactoryNullable)o;
            Assert.AreEqual(42, obj.Value.Value);
        }

        [TestXaml]  // TODO should validate the tree to confirm the Ctor/Factor was called correctly
        public const string SimpleFactoryAttrWithArgsPE = @"
<t:FactoryMade
         xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'
         xmlns:t='clr-namespace:Test.Elements;assembly=XamlTestClasses'
      x:FactoryMethod='Create'>
   <x:Arguments>
      <x:String>hello</x:String>
   </x:Arguments>
</t:FactoryMade>
";

        [TestXaml]  // TODO should validate the tree to confirm the Ctor/Factor was called correctly
        public const string SimpleFactoryPEAndArgsPE = @"
<t:FactoryMade
          xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'
          xmlns:t='clr-namespace:Test.Elements;assembly=XamlTestClasses'>
	<x:Arguments>
		<x:String>hello</x:String>
	</x:Arguments>
	<x:FactoryMethod>
		Create
	</x:FactoryMethod>
</t:FactoryMade>
";

        [TestXaml]  // TODO should validate the tree to confirm the Ctor/Factor was called correctly
        public const string AttachedCreate = @"
<FactoryMade   x:FactoryMethod='FactoryProvider.Create'
          xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'
          xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses'/>
";

        [TestXaml]  // TODO should validate the tree to confirm the Ctor/Factor was called correctly
        public const string OtherAttachedCreate = @"
<FactoryMade    x:FactoryMethod='c:OtherFactoryProvider.Create'
          xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses'
          xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'
          xmlns:c='clr-namespace:Test.Elements2;assembly=XamlTestClasses'/>
";

        [TestXaml] // TODO should validate the tree to confirm the Ctor/Factor was called correctly
        public const string OutOfOrderTest = @"
<FactoryMade IntProp='55'
          xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses'
          xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
  <x:FactoryMethod>Create</x:FactoryMethod>
</FactoryMade>
";

        [TestXaml] // TODO should validate the tree to confirm the Ctor/Factor was called correctly
        public const string OutOfOrderNestedTest = @"
<ElementListHolder
          xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses'
          xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
  <ElementListHolder.Elements>
    <FactoryMade IntProp='55'>
      <x:FactoryMethod>Create</x:FactoryMethod>
    </FactoryMade>

    <FactoryMade IntProp='55'>
      <x:Arguments>
         <FactoryMade IntProp='77'>
            <x:FactoryMethod>Create</x:FactoryMethod>
         </FactoryMade>
      </x:Arguments>
    </FactoryMade>

  </ElementListHolder.Elements>
</ElementListHolder>
";

        [TestXaml]
        [TestTreeValidator("Validate_FactoryMade")]
        public const string LargeListOfFactoryTests = @"
<Element10 Mask='0x1F'
          xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses'
          xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
    <Element10.Element0>
    <FactoryMade>
      <x:FactoryMethod>Create</x:FactoryMethod>
      <x:Arguments>
        <x:String>element0</x:String>
      </x:Arguments>
    </FactoryMade>
    </Element10.Element0>

    <Element10.Element1>
    <FactoryMade IntProp='55'>
      <x:Arguments>
        <x:String>element1</x:String>
      </x:Arguments>
      <x:FactoryMethod>Create</x:FactoryMethod>
    </FactoryMade>
    </Element10.Element1>

    <Element10.Element2>
    <FactoryMade >
      <x:Arguments>
        <x:String>element2</x:String>
      </x:Arguments>
      <x:FactoryMethod>Create</x:FactoryMethod>
      <FactoryMade.IntProp>123</FactoryMade.IntProp>
    </FactoryMade>
    </Element10.Element2>

    <Element10.Element3>
    <FactoryMade Title='The Title'>
      <x:Arguments>
        <x:String>element3</x:String>
      </x:Arguments>
      <x:FactoryMethod>Create</x:FactoryMethod>
      <FactoryMade.IntProp>123</FactoryMade.IntProp>
    </FactoryMade>
    </Element10.Element3>

    <Element10.Element4>
    <FactoryMade  x:FactoryMethod='FactoryProvider.Create' IntProp='42'>
      <x:Arguments>
        <x:String>element4</x:String>
      </x:Arguments>
    </FactoryMade>
    </Element10.Element4>
</Element10>
";

        [TestXaml]
        [TestTreeValidator("Validate_FactoryMade")]
        public const string MarkupExtension = @"
<Element10
            xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses'
            xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'
    Mask='0x0123'
    Element0='{FactoryMarkup x:FactoryMethod=Create}'
    Element1='{FactoryMarkup element1, x:FactoryMethod=Create}' >

    <Element10.Element5>
        <FactoryMarkup x:FactoryMethod='Create'>
            <x:Arguments>
                <x:String>element5</x:String>
            </x:Arguments>
        </FactoryMarkup>
    </Element10.Element5>

</Element10>
";

        public void Validate_FactoryMade(object o)
        {
            Element10 element10 = o as Element10;
            Assert.IsNotNull(element10, "Validator expected an object of type 'Element10'");

            int allSetValues = element10.Mask & 0xFF;
            int defaultValues = (element10.Mask >> 8) & 0xFF;

            for (int i = 0; i < 8; i++)
            {
                bool isDefault = (0 != ((1 <<  i) & defaultValues));
                bool isSet = (0 != ((1 << i) & allSetValues));
                if (isSet)
                {
                    switch (i)
                    {
                        case 0: CheckFactoryValue(element10.Element0, i, isDefault); break;
                        case 1: CheckFactoryValue(element10.Element1, i, isDefault); break;
                        case 2: CheckFactoryValue(element10.Element2, i, isDefault); break;
                        case 3: CheckFactoryValue(element10.Element3, i, isDefault); break;
                        case 4: CheckFactoryValue(element10.Element4, i, isDefault); break;
                        case 5: CheckFactoryValue(element10.Element5, i, isDefault); break;
                        case 6: CheckFactoryValue(element10.Element6, i, isDefault); break;
                        case 7: CheckFactoryValue(element10.Element7, i, isDefault); break;
                    }
                }
            }
        }

        private void CheckFactoryValue(Element e, int i, bool isDefault)
        {
            string val = "element" + i.ToString();
            Assert.IsNotNull(e, String.Format("Value for {0} should not be null", val));
            FactoryMade fm = e as FactoryMade;
            Assert.IsNotNull(fm, String.Format("Value for {0} should be of type FactoryMade", val));
            if (isDefault)
            {
                val = "default";
            }
            if (fm.Label != val)
            {
                Assert.Fail(String.Format("'{0}' and '{1}' are not equal", fm.Label, val));
            }
        }

        [TestXaml]
        [TestExpectedException(typeof(XamlObjectWriterException))]
        public const string AmbiguousMatch = @"
<FactoryMade xmlns=""clr-namespace:Test.Elements;assembly=XamlTestClasses"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
    <x:FactoryMethod>Create</x:FactoryMethod>
    <x:Arguments>
        <x:Null/>
    </x:Arguments>
</FactoryMade>
";

        [TestXaml]
        [TestExpectedException(typeof(XamlObjectWriterException))]
        public const string MissingMethod = @"
<FactoryMade xmlns=""clr-namespace:Test.Elements;assembly=XamlTestClasses"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
    <x:FactoryMethod>Create</x:FactoryMethod>
    <x:Arguments>
        <x:Int32>0</x:Int32>
    </x:Arguments>
</FactoryMade>
";

        [TestXaml]
        [TestExpectedException(typeof(XamlObjectWriterException))]
        public const string MethodInvocation = @"
<FactoryMade xmlns=""clr-namespace:Test.Elements;assembly=XamlTestClasses"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
    <x:FactoryMethod>CreateE</x:FactoryMethod>
</FactoryMade>
";

        [TestXaml]
        [TestExpectedException(typeof(XamlObjectWriterException))]
        public const string NotAssignableFrom = @"
<FactoryMade xmlns=""clr-namespace:Test.Elements;assembly=XamlTestClasses"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
    <x:FactoryMethod>CreateI</x:FactoryMethod>
    <x:Arguments>
        <x:Boolean>True</x:Boolean>
    </x:Arguments>
</FactoryMade>
";

        [TestXaml]
        [TestExpectedException(typeof(XamlObjectWriterException))]
        public const string ReturnNull = @"
<FactoryMade xmlns=""clr-namespace:Test.Elements;assembly=XamlTestClasses"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
    <x:FactoryMethod>CreateI</x:FactoryMethod>
    <x:Arguments>
        <x:Null/>
    </x:Arguments>
</FactoryMade>
";

        [TestXaml]
        public const string NullArgMethodInvocation = @"
<FactoryMade xmlns=""clr-namespace:Test.Elements;assembly=XamlTestClasses"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
    <x:FactoryMethod>Create</x:FactoryMethod>
    <x:Arguments>
        <x:String>test</x:String>        
        <x:Null/>
    </x:Arguments>
</FactoryMade>
";

        [TestXaml]
        [TestExpectedException(typeof(XamlObjectWriterException))]
        public const string FactoryMethodOutOfOrder = @"
<FactoryMade xmlns=""clr-namespace:Test.Elements;assembly=XamlTestClasses"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
    <x:Arguments>
        <x:String>hello</x:String>        
    </x:Arguments>
    <FactoryMade.Title>TooEarly</FactoryMade.Title>
    <x:FactoryMethod>Create</x:FactoryMethod>
</FactoryMade>
";
    }
}
