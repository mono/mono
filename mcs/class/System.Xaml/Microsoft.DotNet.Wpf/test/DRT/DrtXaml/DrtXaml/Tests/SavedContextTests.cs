using System;
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

namespace DrtXaml.Tests
{
    [TestClass]
    sealed class SavedContextTests : XamlTestSuite
    {
        public SavedContextTests()
            : base("SavedContextTests")
        {
        }

        public override DrtTest[] PrepareTests()
        {
            DrtTest[] tests = DrtTestFinder.FindTests(this);
            return tests;
        }

        // =============================================

        [TestXaml, TestTreeValidator("ChainedSavedContext_Validator")]
        const string ChainedSavedContext_XAML =
@"<TemplateClass2 Suffix='.foo'
    xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses'>
    <TemplateClass2.Template>
        <TemplateClass2>
            <TemplateClass2.Template>
                <TemplateClass2 AppendWithSuffix='bar'/>
            </TemplateClass2.Template>
        </TemplateClass2>
    </TemplateClass2.Template>
  </TemplateClass2>";

        public void ChainedSavedContext_Validator(object o)
        {
            TemplateClass2 foo = (TemplateClass2)o;

            TemplateClass2 foo2 = (TemplateClass2)foo.Template.LoadTemplate(null);

            TemplateClass2 foo3 = (TemplateClass2)foo2.Template.LoadTemplate(null);

            if (foo3.AppendWithSuffix != "bar.foo")
            {
                throw new Exception("\"bar.foo\" expected, actually: \"" + foo3.AppendWithSuffix + "\"");
            }
        }

        [TestXaml, TestTreeValidator("ChainedSavedContext_Validator2")]
        const string ChainedSavedContext_XAML2 =
@"<TemplateClass2 Suffix='.foo'
    xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses'>
    <TemplateClass2.Template>
        <TemplateClass2 Suffix='.baz'>
            <TemplateClass2.Template>
                <TemplateClass2 AppendWithSuffix='bar'/>
            </TemplateClass2.Template>
        </TemplateClass2>
    </TemplateClass2.Template>
  </TemplateClass2>";

        public void ChainedSavedContext_Validator2(object o)
        {
            TemplateClass2 foo = (TemplateClass2)o;

            TemplateClass2 foo2 = (TemplateClass2)foo.Template.LoadTemplate(null);

            TemplateClass2 foo3 = (TemplateClass2)foo2.Template.LoadTemplate(null);

            if (foo3.AppendWithSuffix != "bar.baz.foo")
                throw new Exception("\"bar.baz.foo\" expected, actually: \"" + foo3.AppendWithSuffix + "\"");
        }
    }
}
