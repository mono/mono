using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Xaml;
using System.Xml;
using DrtXaml.XamlTestFramework;
using Cdf.Test.Elements;
using DRT;

namespace DrtXaml.Tests
{

    [TestClass]
    class XamlTemplateTests : XamlTestSuite
    {
        public XamlTemplateTests()
            : base("XamlTemplateTests")
        {
        }

        public override DrtTest[] PrepareTests()
        {
            DrtTest[] tests = DrtTestFinder.FindTests(this);
            return tests;
        }

        static XmlTextWriter CreateXmlWriter(TextWriter writer)
        {
            var t = new XmlTextWriter(writer);
            t.Indentation = 2;
            t.IndentChar = ' ';
            t.Formatting = Formatting.Indented;
            t.QuoteChar = '\'';
            return t;
        }

        static string SaveXaml(object value)
        {
            StringWriter sw = new StringWriter();
            using (var writer = CreateXmlWriter(sw))
            {
                XamlServices.Save(writer, value);
            }
            return sw.ToString();
        }

        [TestMethod]
        public void ExpressionTemplateTest()
        {
            var m = new ExprMarkupFactory<object>( () => new TypeWithTemplateProperty() { Name = "First" });
            var t = (TypeWithTemplateProperty)m.Evaluate();
            Assert.AreEqual("First", t.Name);
            t.Name = "Second";
            Assert.AreEqual("Second", t.Name);
            t = (TypeWithTemplateProperty)m.Evaluate();
            Assert.AreEqual("First", t.Name);
        }

        // Do we want this type of "LoadAsFactory" support???
        //
        //        [TestMethod]
        //        public void MarkupTemplateTest()
        //        {
        //            string xaml = @"
        //<TypeWithTemplateProperty 
        //  xmlns='clr-namespace:DrtXaml.Tests;assembly=CDF.CIT.Scenarios.Xaml.XamlTest'
        //  xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'
        //  Name='First' />
        //";
        //            var reader = new XmlTextReader(new StringReader(xaml));
        //            var xr = XamlReader.Create(reader);
        //            var m = new XamlReaderMarkupFactory<object, object>(xr);

        //            var t = (TypeWithTemplateProperty)m.Eval(null);
        //            Assert.AreEqual("First", t.Name);
        //            t.Name = "Second";
        //            Assert.AreEqual("Second", t.Name);
        //            t = (TypeWithTemplateProperty)m.Eval(null);
        //            Assert.AreEqual("First", t.Name);
        //        }

        [TestXaml, TestTreeValidator("SimpleLoad_Validator")]
        const string SimpleLoad = @"
<TypeWithTemplateProperty 
  xmlns='clr-namespace:Cdf.Test.Elements;assembly=XamlTestClasses'
  xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'
  Name='Outer'>
    <TypeWithTemplateProperty.Template>
        <TypeWithTemplateProperty Name='Inner' />
    </TypeWithTemplateProperty.Template>
</TypeWithTemplateProperty>
";
        public void SimpleLoad_Validator(object o)
        {
            Assert.IsInstanceOfType(typeof(TypeWithTemplateProperty), o, "Should have deserialized to correct type");
            var twtp = (TypeWithTemplateProperty)o;
            Assert.AreEqual("Outer", twtp.Name);
            twtp = (TypeWithTemplateProperty)twtp.Template.Evaluate();
            Assert.AreEqual("Inner", twtp.Name);
        }

        [TestDisabled]   // Unable to serialize type Cdf.Test.Elements.TestFactory`1[System.Object]
        [TestXaml, TestTreeValidator("SimpleSave_Validator")]
        const string SimpleSave = @"
<TypeWithTemplateProperty Name='Outer'
       xmlns='clr-namespace:Cdf.Test.Elements;assembly=XamlTestClasses'>
  <TypeWithTemplateProperty.Template>
    <TypeWithTemplateProperty Name='Inner' />
  </TypeWithTemplateProperty.Template>
</TypeWithTemplateProperty>
";
        public void SimpleSave_Validator(object o)
        {
            Assert.IsInstanceOfType(typeof(TypeWithTemplateProperty), o, "Should have deserialized to correct type");
            var twtp = (TypeWithTemplateProperty)o;
            Assert.AreEqual("Outer", twtp.Name);
            twtp = (TypeWithTemplateProperty)twtp.Template.Evaluate();
            Assert.AreEqual("Inner", twtp.Name);

            var comp = SaveXaml(o);

            Assert.AreEqual(SimpleSave, comp);
        }

        [TestMethod]
        public void SimpleSerializeNullTemplateProperty()
        {
            TypeWithTemplateProperty t = new TypeWithTemplateProperty();

            SaveXaml(t);
        }


        [TestXaml, TestTreeValidator("SimpleInterfaceLoad_Validator")]
        const string SimpleInterfaceLoad = @"
<TypeWithTemplateInterfaceProperty 
       xmlns='clr-namespace:Cdf.Test.Elements;assembly=XamlTestClasses'
       xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'
       Name='Outer'>
    <TypeWithTemplateInterfaceProperty.Template>
        <TypeWithTemplateInterfaceProperty Name='Inner' />
    </TypeWithTemplateInterfaceProperty.Template>
</TypeWithTemplateInterfaceProperty>
";
        public void SimpleInterfaceLoad_Validator(Object o)
        {
            Assert.IsInstanceOfType(typeof(TypeWithTemplateInterfaceProperty), o, "Should have deserialized to correct type");
            var twtp = (TypeWithTemplateInterfaceProperty)o;
            Assert.AreEqual("Outer", twtp.Name);
            twtp = (TypeWithTemplateInterfaceProperty)twtp.Template.Evaluate();
            Assert.AreEqual("Inner", twtp.Name);
        }

        [TestXaml, TestTreeValidator("SimpleInterfaceSave_Validator")]

        // This String needs very exact formating because
        // it will be String Compared against the Serializer.
        const string SimpleInterfaceSave =
@"<TypeWithTemplateInterfaceProperty Name=""Outer"" xmlns=""clr-namespace:Cdf.Test.Elements;assembly=XamlTestClasses"">
  <TypeWithTemplateInterfaceProperty.Template>
    <TypeWithTemplateInterfaceProperty Name=""Inner"" />
  </TypeWithTemplateInterfaceProperty.Template>
</TypeWithTemplateInterfaceProperty>";


        public void SimpleInterfaceSave_Validator(Object o)
        {
            Assert.IsInstanceOfType(typeof(TypeWithTemplateInterfaceProperty), o, "Should have deserialized to correct type");
            var twtp = (TypeWithTemplateInterfaceProperty)o;
            Assert.AreEqual("Outer", twtp.Name);
            twtp = (TypeWithTemplateInterfaceProperty)twtp.Template.Evaluate();
            Assert.AreEqual("Inner", twtp.Name);

            var comp = XamlServices.Save(o);

            Assert.AreEqual(SimpleInterfaceSave, comp);
        }

        [TestXaml, TestTreeValidator("BindingLoad_Validator")]
        const string BindingLoad = @"
<TypeWithTemplateProperty 
         xmlns='clr-namespace:Cdf.Test.Elements;assembly=XamlTestClasses'
         xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'
         Name='Outer'>
    <TypeWithTemplateProperty.Template>
        <TypeWithTemplateProperty>
            <TypeWithTemplateProperty.Name>
                <TemplateBindingExtension Property='Name' />
            </TypeWithTemplateProperty.Name>
        </TypeWithTemplateProperty>

    </TypeWithTemplateProperty.Template>
</TypeWithTemplateProperty>
";
        public void BindingLoad_Validator(object o)
        {
            Assert.IsInstanceOfType(typeof(TypeWithTemplateProperty), o, "Should have deserialized to correct type");
            var twtp = (TypeWithTemplateProperty)o;
            Assert.AreEqual("Outer", twtp.Name);
            twtp = (TypeWithTemplateProperty)twtp.Template.Evaluate();
            Assert.AreEqual("Outer", twtp.Name, "Names are not Equal");
        }

        [TestXaml, TestTreeValidator("BindingLoadCompactSyntax_Validator")]
        const string BindingLoadCompactSyntax = @"
<TypeWithTemplateProperty 
         xmlns='clr-namespace:Cdf.Test.Elements;assembly=XamlTestClasses'
         xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'
         Name='Outer'>
    <TypeWithTemplateProperty.Template>
        <TypeWithTemplateProperty Name='{TemplateBindingExtension Name}' />
    </TypeWithTemplateProperty.Template>
</TypeWithTemplateProperty>
";
        public void BindingLoadCompactSyntax_Validator(object o)
        {
            Assert.IsInstanceOfType(typeof(TypeWithTemplateProperty), o, "Should have deserialized to correct type");
            var twtp = (TypeWithTemplateProperty)o;
            Assert.AreEqual("Outer", twtp.Name);
            twtp = (TypeWithTemplateProperty)twtp.Template.Evaluate();
            Assert.AreEqual("Outer", twtp.Name);
        }

        // This isn't an actual validated case in ObjectWriter; TargetException is just what happens
        // to get thrown. It would be nice to validate this and throw DuplicateMemberException instead.
        [TestXaml, TestExpectedException(typeof(XamlObjectWriterException), typeof(System.Reflection.TargetException))]
        const string DictionaryTemplateLoad = @"
<TypeWithDictionaryTemplateProperty 
         xmlns='clr-namespace:Cdf.Test.Elements;assembly=XamlTestClasses'
         xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'
         Name='Outer'>
    <TypeWithDictionaryTemplateProperty.Template>
        <TypeWithTemplateProperty x:Key='one' Name='one' />
        <TypeWithTemplateProperty x:Key='two' Name='two' />
    </TypeWithDictionaryTemplateProperty.Template>
</TypeWithDictionaryTemplateProperty>
";

        // This isn't an actual validated case in ObjectWriter; TargetException is just what happens
        // to get thrown. It would be nice to validate this and throw DuplicateMemberException instead.
        [TestXaml, TestExpectedException(typeof(XamlObjectWriterException), typeof(System.Reflection.TargetException))]
        const string DictionaryTemplateLoadNoKeys = @"
<TypeWithDictionaryTemplateProperty 
         xmlns='clr-namespace:Cdf.Test.Elements;assembly=XamlTestClasses'
         xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'
         Name='Outer'>
    <TypeWithDictionaryTemplateProperty.Template>
        <TypeWithTemplateProperty Name='one' />
        <TypeWithTemplateProperty Name='two' />
    </TypeWithDictionaryTemplateProperty.Template>
</TypeWithDictionaryTemplateProperty>
";

        // This isn't an actual validated case in ObjectWriter; TargetException is just what happens
        // to get thrown. It would be nice to validate this and throw DuplicateMemberException instead.
        [TestXaml, TestExpectedException(typeof(XamlObjectWriterException), typeof(System.Reflection.TargetException))]
        const string ListTemplateLoad = @"
<TypeWithListTemplateProperty 
         xmlns='clr-namespace:Cdf.Test.Elements;assembly=XamlTestClasses'
         xmlns:g='clr-namespace:System.Collections.Generic;assembly=mscorlib'
         xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'
         Name='Outer'>
    <TypeWithListTemplateProperty.Template>
        <TypeWithTemplateProperty Name='one' />
        <TypeWithTemplateProperty Name='two' />
    </TypeWithListTemplateProperty.Template>
</TypeWithListTemplateProperty>
";

        [TestDisabled]  //  This test expects Types in a list of templates to be converted into templates.
        [TestXaml, TestTreeValidator("TemplateListRoundtrip_Validator")]
        const string TemplateListRoundtrip = @"
<TypeWithTemplateListProperty  Name='Outer'
         xmlns='clr-namespace:Cdf.Test.Elements;assembly=XamlTestClasses'>
  <TypeWithTemplateListProperty.Templates>
    <TypeWithTemplateProperty Name='one' />
    <TypeWithTemplateProperty Name='two' />
  </TypeWithTemplateListProperty.Templates>
</TypeWithTemplateListProperty>
";
        public void TemplateListRoundtrip_Validator(object o)
        {
            Assert.IsInstanceOfType(typeof(TypeWithTemplateListProperty), o, "Should have deserialized to correct type");
            var t = (TypeWithTemplateListProperty)o;
            Assert.AreEqual("Outer", t.Name);
            Assert.AreEqual(2, t.Templates.Count);
            var one = t.Templates[0].Evaluate() as TypeWithTemplateProperty;
            Assert.AreEqual("one", one.Name);
            one.Name = "three";
            Assert.AreEqual("three", one.Name);
            one = t.Templates[0].Evaluate() as TypeWithTemplateProperty;
            Assert.AreEqual("one", one.Name);
            var two = t.Templates[1].Evaluate() as TypeWithTemplateProperty;
            Assert.AreEqual("two", two.Name);

            var sw = new StringWriter();
            using (var xw = XmlWriter.Create(sw, new XmlWriterSettings { Indent = true, OmitXmlDeclaration = true }))
            {
                XamlServices.Save(xw, o);
            }

            Assert.AreEqual(TemplateListRoundtrip, sw.ToString());
        }

        [TestDisabled]  //  This test expects Types in a list of templates to be converted into templates.
        [TestXaml, TestTreeValidator("TemplateDictionaryRoundtrip_Validator")]
        const string TemplateDictionaryRoundtrip = @"
<TypeWithTemplateDictionaryProperty Name='Outer'
         xmlns='clr-namespace:Cdf.Test.Elements;assembly=XamlTestClasses'
       xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
  <TypeWithTemplateDictionaryProperty.Templates>
    <TypeWithTemplateProperty x:Key='one' Name='one' />
    <TypeWithTemplateProperty x:Key='two' Name='two' />
  </TypeWithTemplateDictionaryProperty.Templates>
</TypeWithTemplateDictionaryProperty>
";
        public void TemplateDictionaryRoundtrip_Validator(object o)
        {
            Assert.IsInstanceOfType(typeof(TypeWithTemplateDictionaryProperty), o, "Should have deserialized to correct type");
            var t = (TypeWithTemplateDictionaryProperty)o;
            Assert.AreEqual("Outer", t.Name);
            Assert.AreEqual(2, t.Templates.Count);
            var one = t.Templates["one"].Evaluate() as TypeWithTemplateProperty;
            Assert.AreEqual("one", one.Name);
            one.Name = "three";
            Assert.AreEqual("three", one.Name);
            one = t.Templates["one"].Evaluate() as TypeWithTemplateProperty;
            Assert.AreEqual("one", one.Name);
            var two = t.Templates["two"].Evaluate() as TypeWithTemplateProperty;
            Assert.AreEqual("two", two.Name);

            var sw = new StringWriter();
            using (var xw = XmlWriter.Create(sw, new XmlWriterSettings { Indent = true, OmitXmlDeclaration = true}))
            {
                XamlServices.Save(xw, o);
            }

            Assert.AreEqual(TemplateDictionaryRoundtrip, sw.ToString());
        }

        [TestXaml, TestTreeValidator("FakeWPFTemplateImplementation_Validator")]
        const string FakeWPFTemplateImplementation = @"
<FakeButton 
         xmlns='clr-namespace:Cdf.Test.Elements;assembly=XamlTestClasses'
  xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'
  Content='Hello World'
  >
  <FakeButton.Template>
    <FakeControlTemplate>
      <FakeBorder>
        <FakeContentPresenter>
          <FakeContentPresenter.Content>
            <TemplateBindingExtension Property='Content' />
          </FakeContentPresenter.Content>
        </FakeContentPresenter>
      </FakeBorder>
    </FakeControlTemplate>
  </FakeButton.Template>
</FakeButton>
";
        public void FakeWPFTemplateImplementation_Validator(object o)
        {
            var fakeButton = o as FakeButton;
            Assert.AreEqual("Hello World", fakeButton.Content);
            var border = (FakeBorder)fakeButton.Visuals;
            var cp = (FakeContentPresenter)border.Child;
            Assert.AreEqual("Hello World", cp.Content);

            // This String needs very exact formating because
            // it will be String Compared against the Serializer.
            string compXaml =
@"<FakeButton xmlns=""clr-namespace:Cdf.Test.Elements;assembly=XamlTestClasses"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
  <FakeButton.Template>
    <FakeControlTemplate TargetType=""{x:Null}"">
      <FakeBorder>
        <FakeContentPresenter Content=""{TemplateBinding Property=Content}"" />
      </FakeBorder>
    </FakeControlTemplate>
  </FakeButton.Template>Hello World</FakeButton>";

            var persistXaml = XamlServices.Save(fakeButton);

            Assert.AreEqual(compXaml, persistXaml);
        }

        [TestXaml, TestTreeValidator("FakeWFTemplateMarkup_Validator")]
        const string FakeWFTemplateMarkup = @"
<FakeForEach 
         xmlns='clr-namespace:Cdf.Test.Elements;assembly=XamlTestClasses'
         xmlns:sys='clr-namespace:System;assembly=mscorlib'
         xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'
         x:TypeArguments='sys:Int32'
  >
    <FakeForEach.LoopVariable>
        <FakeVariable x:TypeArguments='sys:Int32' x:Name='i' />
    </FakeForEach.LoopVariable>
    <FakeForEach.ItemsSource>
        <x:Array Type='sys:Int32'>
            <sys:Int32>1</sys:Int32>
            <sys:Int32>2</sys:Int32>
            <sys:Int32>3</sys:Int32>
        </x:Array>
    </FakeForEach.ItemsSource>
    <FakeForEach.Body>
        <FakeAppendIntToString
            Target='{VariableExtension global}'
            Content='{x:Reference i}' />
    </FakeForEach.Body>
</FakeForEach>
";
        public void FakeWFTemplateMarkup_Validator(object o)
        {

            var loop = o as FakeForEach<int>;
            var global = new FakeVariable<string>() { Name = "global" };
            global.SetValue(null, "Hello World");
            var environment = new FakeEnvironment();
            environment.Variables[global.Name] = global;

            loop.Invoke(environment);

            Assert.AreEqual("Hello World123", global.GetValue(null));
        }

        [TestMethod]
        public void FakeWFTemplateImperative1()
        {
            // This is the semantically equivalent code to the markup example.
            // Any conditional logical in the construction of the activity
            // tree is not representable in xaml.
            //
            var iVar = new FakeVariable<int>() { Name = "i" };
            var loop = new FakeForEach<int>()
                {
                    LoopVariable = iVar,
                    ItemsSource = new int[] { 1, 2, 3 },
                    Body = () =>
                    new FakeAppendIntToString()
                    {
                        Target = new FakeLookupVariable<string>("global"),
                        Content = iVar
                    }
                };

            var global = new FakeVariable<string>() { Name = "global" };
            global.SetValue(null, "Hello World");
            var environment = new FakeEnvironment();
            environment.Variables[global.Name] = global;

            loop.Invoke(environment);

            string globalFinalValue = global.GetValue(null);
            Assert.AreEqual("Hello World123", globalFinalValue);
        }

        [TestMethod]
        public void FakeWFTemplateImperative2()
        {
            // This is the more natural C# developer intuition.
            // Notice that the string LoopVariableName is effectively
            // ignored.
            //
            var loop = new FakeForEach<int>()
                {
                    LoopVariable = new FakeVariable<int>() { Name = "i" },
                    ItemsSource = new int[] { 1, 2, 3 },
                    Body = () =>
                    new FakeAppendIntToString()
                    {
                        Target = new FakeLookupVariable<string>("global"),
                        Content = new FakeLookupVariable<int>("i")
                    }
                };

            var global = new FakeVariable<string>() { Name = "global" };
            global.SetValue(null, "Hello World");
            var environment = new FakeEnvironment();
            environment.Variables[global.Name] = global;

            loop.Invoke(environment);

            string globalFinalValue = global.GetValue(null);
            Assert.AreEqual("Hello World123", globalFinalValue);
        }

        [TestMethod]
        public void FakeWFTemplateImperative3()
        {
            // This is identical to #2, however using a factory method
            // to get better type infrencing. C# 3.0 doesn't do inferencing
            // based on the LHS, so you have to type all the expr variables.
            //
            var loop = new FakeForEach<int>()
                {
                    LoopVariable = new FakeVariable<int>() { Name = "i" },
                    ItemsSource = new int[] { 1, 2, 3 },
                    Body = () =>
                    (FakeActivity)new FakeAppendIntToString()
                    {
                        Target = new FakeLookupVariable<string>("global"),
                        Content = new FakeLookupVariable<int>("i")
                    }
                };

            var global = new FakeVariable<string>() { Name = "global" };
            global.SetValue(null, "Hello World");
            var environment = new FakeEnvironment();
            environment.Variables[global.Name] = global;

            loop.Invoke(environment);

            string globalFinalValue = global.GetValue(null);
            Assert.AreEqual("Hello World123", globalFinalValue);
        }

        [TestMethod]
        public void FakeXadImperative()
        {
            var s =
                new FakeSwitch<int, string>()
                {
                    Cases = {
                        new FakeCase<int, string>()
                        {
                            Condition = 0,
                            Body = () => "Hello"
                        },
                        new FakeCase<int, string>()
                        {
                            Condition = 1,
                            Body = () => "Goodbye"
                        }
                    }
                };

            Assert.AreEqual("Hello", s.Content);
            s.Value = 1;
            Assert.AreEqual("Goodbye", s.Content);
        }

        [TestXaml, TestTreeValidator("FakeXadMarkup_Validator")]
        const string FakeXadMarkup = @"
<FakeSwitch
         xmlns='clr-namespace:Cdf.Test.Elements;assembly=XamlTestClasses'
         xmlns:sys='clr-namespace:System;assembly=mscorlib'
         xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'
         x:TypeArguments='sys:Int32, sys:String'
  >

  <FakeCase Condition='0' x:TypeArguments='sys:Int32, sys:String'>
    <sys:String>Hello</sys:String>
  </FakeCase>

  <FakeCase Condition='1' x:TypeArguments='sys:Int32, sys:String'>
    <sys:String>Goodbye</sys:String>
  </FakeCase>

</FakeSwitch>
";
        public void FakeXadMarkup_Validator(object o)
        {
            var s = o as FakeSwitch<int, string>;
            Assert.AreEqual("Hello", s.Content);
            s.Value = 1;
            Assert.AreEqual("Goodbye", s.Content);
        }

        [TestDisabled] // The 'FakeBinding' test object doesn't work
        [TestXaml, TestTreeValidator("FakeXadIntegrationWithBinding_Validator")]
        const string FakeXadIntegrationWithBinding = @"
<FakeButton 
    xmlns='clr-namespace:Cdf.Test.Elements;assembly=XamlTestClasses'
    xmlns:sys='clr-namespace:System;assembly=mscorlib'
    xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'
    Content='{FakeBinding Path=Content}'>

    <FakeButton.DataContext>
        <FakeSwitch
          
          x:TypeArguments='sys:Int32, sys:String'
          Value='0'
          >

          <FakeCase Condition='0' x:TypeArguments='sys:Int32, sys:String'>
            <sys:String>Hello</sys:String>
          </FakeCase>

          <FakeCase Condition='1' x:TypeArguments='sys:Int32, sys:String'>
            <sys:String>Goodbye</sys:String>
          </FakeCase>

        </FakeSwitch>
    </FakeButton.DataContext>
</FakeButton>
";
        public void FakeXadIntegrationWithBinding_Validator(object o)
        {
            var fButton = o as FakeButton;

            var fSwitch = (FakeSwitch<int, string>)fButton.DataContext;

            fSwitch.Value = 0;
            string str0 = (string)fButton.Content;
            Assert.AreEqual("Hello", str0);

            fSwitch.Value = 1;
            string str1 = (string)fButton.Content;
            Assert.AreEqual("Goodbye", str1);
        }

    }
}
