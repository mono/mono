using System;
using System.Windows;
using DRT;
using DrtXaml.XamlTestFramework;
using BamlTestClasses40;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Reflection;

namespace DrtXaml.Tests
{
    [TestClass]
    sealed class BamlTests : XamlTestSuite
    {
        public BamlTests() : base("BamlTests")
        {
        }

        public override DrtTest[] PrepareTests()
        {
            DrtTest[] tests = DrtTestFinder.FindTests(this);
            return tests;
        }

        [TestMethod]
        public void XmlSpaceDefault()
        {
            StackPanel s1 = new XmlSpace1();
            if (!XmlAttributeProperties.GetXmlSpace((DependencyObject)s1).Equals("default"))
            {
                throw new Exception("Expected the outer StackPanel's xml:space to be 'default'.");
            }
            StackPanel s2 = s1.Children[0] as StackPanel;
            if (!XmlAttributeProperties.GetXmlSpace((DependencyObject)s2).Equals("default"))
            {
                throw new Exception("Expected the inner StackPanel's xml:space to be 'default'.");
            }
        }

        [TestMethod]
        public void XmlSpacePreserve1()
        {
            StackPanel s1 = new XmlSpace2();
            if (!XmlAttributeProperties.GetXmlSpace((DependencyObject)s1).Equals("preserve"))
            {
                throw new Exception("Expected outer StackPanel's xml:space to be 'preserve'.");
            }
            StackPanel s2 = s1.Children[0] as StackPanel;
            if (!XmlAttributeProperties.GetXmlSpace((DependencyObject)s2).Equals("default"))
            {
                throw new Exception("Expected inner StackPanel's xml:space to be 'default'.");
            }
        }

        [TestMethod]
        public void XmlSpacePreserve2()
        {
            StackPanel s1 = new XmlSpace3();
            if (!XmlAttributeProperties.GetXmlSpace((DependencyObject)s1).Equals("preserve"))
            {
                throw new Exception("Expected outer StackPanel's xml:space to be 'preserve'.");
            }
            StackPanel s2 = s1.Children[0] as StackPanel;
            if (!XmlAttributeProperties.GetXmlSpace((DependencyObject)s2).Equals("preserve"))
            {
                throw new Exception("Expected inner StackPanel's xml:space to be 'preserve'.");
            }
        }

#if NET4X && false
        // Verifies correct BAML behavior for 3.5 and 4.0 assemblies regarding
        // MarkupExtension.ProvideValue in top-level deferrable content resources.
        [TestMethod]
        public void DeferredContentMarkupExtensionProvideValue()
        {
            object value;
            // 3.5
            Assert.IsTrue(Assembly.Load("BamlTestClasses35").ImageRuntimeVersion.StartsWith("v2"));
            // Test 1
            var stackPanel35 = new BamlTestClasses35.DeferContMEPV();
            // Test 2
            value = stackPanel35.Test2.Resources["markupextension"];
            Assert.IsTrue(value == null);
            // Test 3
            value = stackPanel35.Test3.Resources["markupextension1"];
            Assert.IsTrue(value == null);
            value = stackPanel35.Test3.Resources["markupextension2"];
            Assert.IsTrue(value == null);
            // Test 4
            value = stackPanel35.Test4.Resources["markupextension1"];
            Assert.IsTrue(value is NullExtension);
            value = stackPanel35.Test4.Resources["markupextension2"];
            Assert.IsTrue(value is NullExtension);
            // 4.0
            Assert.IsTrue(Assembly.Load("BamlTestClasses40").ImageRuntimeVersion.StartsWith("v4"));
            // Test 1
            var stackPanel40 = new DeferContMEPV();
            // Test 2
            value = stackPanel40.Test2.Resources["markupextension"];
            Assert.IsTrue(value == null);
            // Test 3
            value = stackPanel40.Test3.Resources["markupextension1"];
            Assert.IsTrue(value == null);
            value = stackPanel40.Test3.Resources["markupextension2"];
            Assert.IsTrue(value == null);
            // Test 4
            value = stackPanel40.Test4.Resources["markupextension1"];
            Assert.IsTrue(value == null);
            value = stackPanel40.Test4.Resources["markupextension2"];
            Assert.IsTrue(value == null);
        }
#endif
        // Make sure Bindings with non-resolvable types in the path fail silently like in v3.
        [TestMethod]
        public void TestInvalidBindingPathReference()
        {
            var mw = new BamlBindingPath();
        }

#if TESTBUILD_NET_ATLEAST_462
        [TestMethod]
        public void TestMarkupExtensionBracketCharacterAttributes()
        {
            BracketCharacterAttribute mw = new BracketCharacterAttribute();
            foreach (UIElement element in mw.Children)
            {
                TextBlock textBlock = element as TextBlock;
                if (textBlock != null)
                {
                    string actual = textBlock.Text;
                    string expected = textBlock.Tag as string;
                    if (actual != expected)
                    {
                        throw new Exception(string.Format("Mismatch in expected text of TextBlock. Actual : {0} , Expected : {1}", actual, expected));
                    }
                }
            }
        }
#endif
    }
}
