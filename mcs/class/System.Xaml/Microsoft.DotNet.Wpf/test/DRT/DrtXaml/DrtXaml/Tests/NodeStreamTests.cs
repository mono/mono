using System;
using DRT;
using DrtXaml.XamlTestFramework;
using Test.NodeStream;
using System.Xaml;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Text;
using System.Windows.Controls;
using System.Reflection;

namespace DrtXaml.Tests
{
    [TestClass]
    public sealed class NodeStreamTests : XamlTestSuite
    {
        private static XamlSchemaContext xsc = new XamlSchemaContext();

        public NodeStreamTests()
            : base("NodeStreamTests")
        {
        }

        public override DrtTest[] PrepareTests()
        {
            DrtTest[] tests = DrtTestFinder.FindTests(this);
            return tests;
        }

        [TestMethod]
        public void UnknownSO_UnknownContent_V()
        {
            Node[] n = new Node[] {
                new NS(new NamespaceDeclaration("unknown", "")),
                new SO(new XamlType("unknown", "Foo", null, xsc)),
                    new SM(XamlLanguage.UnknownContent),
                        new V("text"),
                    new EM(),
                new EO()
            };
            string x = @"<?xml version=""1.0"" encoding=""utf-16""?>
<Foo xmlns=""unknown"">text</Foo>";
            CheckXmlToNodes(x, n);
            CheckNodesToXml(n, x);
        }

        [TestMethod]
        public void UnknownSO_UnknownContent_O()
        {
            Node[] n = new Node[] {
                new NS(new NamespaceDeclaration("unknown", "")),
                new SO(new XamlType("unknown", "Foo", null, xsc)),
                    new SM(XamlLanguage.UnknownContent),
                        new SO(new XamlType("unknown", "Bar", null, xsc)),
                        new EO(),
                    new EM(),
                new EO()
            };
            string x = @"<?xml version=""1.0"" encoding=""utf-16""?>
<Foo xmlns=""unknown"">
  <Bar />
</Foo>";
            CheckXmlToNodes(x, n);
            CheckNodesToXml(n, x);
        }

        [TestMethod]
        public void UnknownSO_UnknownContent_M()
        {
            Node[] n = new Node[] {
                new NS(new NamespaceDeclaration("unknown", "")),
                new SO(new XamlType("unknown", "Foo", null, xsc)),
                    new SM(new XamlMember("Baz", new XamlType("unknown", "Foo", null, xsc), false)),
                        new SO(new XamlType("unknown", "Bar", null, xsc)),
                        new EO(),
                    new EM(),
                new EO()
            };
            string x = @"<?xml version=""1.0"" encoding=""utf-16""?>
<Foo xmlns=""unknown"">
  <Foo.Baz>
    <Bar />
  </Foo.Baz>
</Foo>";
            CheckXmlToNodes(x, n);
            CheckNodesToXml(n, x);
        }

        [TestMethod]
        public void UnknownSO_UnknownContent_O_V_O()
        {
            Node[] n = new Node[] {
                new NS(new NamespaceDeclaration("unknown", "")),
                new SO(new XamlType("unknown", "Foo", null, xsc)),
                    new SM(XamlLanguage.UnknownContent),
                        new SO(new XamlType("unknown", "Bar", null, xsc)),
                        new EO(),
                        new V("text"),
                        new SO(new XamlType("unknown", "Bar", null, xsc)),
                        new EO(),
                    new EM(),
                new EO()
            };
            string x = @"<?xml version=""1.0"" encoding=""utf-16""?>
<Foo xmlns=""unknown"">
  <Bar />text<Bar /></Foo>";
            CheckXmlToNodes(x, n);
            CheckNodesToXml(n, x);
        }

        [TestMethod]
        public void UnknownSO_UnknownContent_V_O_V()
        {
            Node[] n = new Node[] {
                new NS(new NamespaceDeclaration("unknown", "")),
                new SO(new XamlType("unknown", "Foo", null, xsc)),
                    new SM(XamlLanguage.UnknownContent),
                        new V("text"),
                        new SO(new XamlType("unknown", "Bar", null, xsc)),
                        new EO(),
                        new V("text"),
                    new EM(),
                new EO()
            };
            string x = @"<?xml version=""1.0"" encoding=""utf-16""?>
<Foo xmlns=""unknown"">text<Bar />text</Foo>";
            CheckXmlToNodes(x, n);
            CheckNodesToXml(n, x);
        }

        [TestMethod]
        public void UnknownSO_UnknownContent_V_O_M_O_V()
        {
            Node[] n = new Node[] {
                new NS(new NamespaceDeclaration("unknown", "")),
                new SO(new XamlType("unknown", "Foo", null, xsc)),
                    new SM(XamlLanguage.UnknownContent),
                        new V("text"),
                        new SO(new XamlType("unknown", "Bar", null, xsc)),
                        new EO(),
                    new EM(),
                    new SM(new XamlMember("Baz", new XamlType("unknown", "Foo", null, xsc), false)),
                        new SO(new XamlType("unknown", "Bar", null, xsc)),
                        new EO(),
                    new EM(),
                    new SM(XamlLanguage.UnknownContent),
                        new SO(new XamlType("unknown", "Bar", null, xsc)),
                        new EO(),
                        new V("text"),
                    new EM(),
                new EO()
            };
            string x = @"<?xml version=""1.0"" encoding=""utf-16""?>
<Foo xmlns=""unknown"">text<Bar /><Foo.Baz><Bar /></Foo.Baz><Bar />text</Foo>";
            CheckXmlToNodes(x, n);
            CheckBadNodesToXml(n, x);
        }

        [TestMethod]
        public void UnknownSO_UnknownContent_V_O_M_O_V_Nested()
        {
            Node[] n = new Node[] {
                new NS(new NamespaceDeclaration("unknown", "")),
                new SO(new XamlType("unknown", "Foo", null, xsc)),
                    new SM(XamlLanguage.UnknownContent),
                        new V("text"),
                        new SO(new XamlType("unknown", "Bar", null, xsc)),
                        new EO(),
                    new EM(),
                    new SM(new XamlMember("Baz", new XamlType("unknown", "Foo", null, xsc), false)),
                        new SO(new XamlType("unknown", "Foo", null, xsc)),
                            new SM(XamlLanguage.UnknownContent),
                                new V("text"),
                                new SO(new XamlType("unknown", "Bar", null, xsc)),
                                new EO(),
                            new EM(),
                        new EO(),
                    new EM(),
                    new SM(XamlLanguage.UnknownContent),
                        new SO(new XamlType("unknown", "Bar", null, xsc)),
                        new EO(),
                        new V("text"),
                    new EM(),
                new EO()
            };
            string x = @"<?xml version=""1.0"" encoding=""utf-16""?>
<Foo xmlns=""unknown"">text<Bar /><Foo.Baz><Foo>text<Bar /></Foo></Foo.Baz><Bar />text</Foo>";
            CheckXmlToNodes(x, n);
            CheckBadNodesToXml(n, x);
        }

        [TestMethod]
        public void UnknownSO_UnknownContent_M_V_O_M()
        {
            Node[] n = new Node[] {
                new NS(new NamespaceDeclaration("unknown", "")),
                new SO(new XamlType("unknown", "Foo", null, xsc)),
                    new SM(new XamlMember("Baz", new XamlType("unknown", "Foo", null, xsc), false)),
                        new SO(new XamlType("unknown", "Bar", null, xsc)),
                        new EO(),
                    new EM(),
                    new SM(XamlLanguage.UnknownContent),
                        new V("text"),
                        new SO(new XamlType("unknown", "Bar", null, xsc)),
                        new EO(),
                    new EM(),
                    new SM(new XamlMember("Buz", new XamlType("unknown", "Foo", null, xsc), false)),
                        new SO(new XamlType("unknown", "Bar", null, xsc)),
                        new EO(),
                    new EM(),
                new EO()
            };
            string x = @"<?xml version=""1.0"" encoding=""utf-16""?>
<Foo xmlns=""unknown"">
  <Foo.Baz>
    <Bar />
  </Foo.Baz>text<Bar /><Foo.Buz><Bar /></Foo.Buz></Foo>";
            CheckXmlToNodes(x, n);
            CheckNodesToXml(n, x);
        }

        [TestMethod]
        public void UnknownSO_UnknownContent_V_NS_O_M_V_NS_O()
        {
            Node[] n = new Node[] {
                new NS(new NamespaceDeclaration("unknown", "")),
                new SO(new XamlType("unknown", "Foo", null, xsc)),
                    new SM(XamlLanguage.UnknownContent),
                        new V("text"),
                        new NS(new NamespaceDeclaration("buz", "b")),
                        new SO(new XamlType("unknown", "Bar", null, xsc)),
                        new EO(),
                    new EM(),
                    new SM(new XamlMember("Baz", new XamlType("unknown", "Foo", null, xsc), false)),
                        new SO(new XamlType("unknown", "Bar", null, xsc)),
                        new EO(),
                    new EM(),
                    new SM(XamlLanguage.UnknownContent),
                        new V("text"),
                        new NS(new NamespaceDeclaration("buz", "b")),
                        new SO(new XamlType("unknown", "Bar", null, xsc)),
                        new EO(),
                    new EM(),
                new EO()
            };
            string x = @"<?xml version=""1.0"" encoding=""utf-16""?>
<Foo xmlns=""unknown"">text<Bar xmlns:b=""buz"" />
  <Foo.Baz>
    <Bar />
  </Foo.Baz>text<Bar xmlns:b=""buz"" /></Foo>";
            CheckXmlToNodes(x, n);
            CheckBadNodesToXml(n, x);
        }

        private Node[] Arguments_Constructor1_N = new Node[] {
            new NS(new NamespaceDeclaration("clr-namespace:Test.NodeStream;assembly=XamlTestClasses", "")),
            new NS(new NamespaceDeclaration("http://schemas.microsoft.com/winfx/2006/xaml", "x")),
            new SO(new XamlType(typeof(ConstructorArguments1), xsc)),
                new SM(XamlLanguage.Arguments),
                    new SO(new XamlType(typeof(string), xsc)),
                        new SM(XamlLanguage.Initialization),
                            new V("foo"),
                        new EM(),
                    new EO(),
                new EM(),
            new EO()
        };

        private string Arguments_Constructor1_X = @"<?xml version=""1.0"" encoding=""utf-16""?>
<ConstructorArguments1 xmlns=""clr-namespace:Test.NodeStream;assembly=XamlTestClasses"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
  <x:Arguments>
    <x:String>foo</x:String>
  </x:Arguments>
</ConstructorArguments1>";

        private ConstructorArguments1 Arguments_Constructor1_O = new ConstructorArguments1("foo");

        [TestMethod]
        public void Arguments_Constructor1_X2N()
        {
            CheckXmlToNodes(Arguments_Constructor1_X, Arguments_Constructor1_N);
        }

        [TestMethod]
        public void Arguments_Constructor1_N2X()
        {
            CheckNodesToXml(Arguments_Constructor1_N, Arguments_Constructor1_X);
        }

        [TestMethod]
        public void Arguments_Constructor1_O2N()
        {
            CheckObjectToNodes(Arguments_Constructor1_O, Arguments_Constructor1_N);
        }

        [TestMethod]
        public void Arguments_Constructor1_N2O()
        {
            CheckNodesToObject(Arguments_Constructor1_N, Arguments_Constructor1_O);
        }

        private Node[] Arguments_Constructor2_N = new Node[] {
            new NS(new NamespaceDeclaration("clr-namespace:Test.NodeStream;assembly=XamlTestClasses", "")),
            new NS(new NamespaceDeclaration("http://schemas.microsoft.com/winfx/2006/xaml", "x")),
            new SO(new XamlType(typeof(ConstructorArguments2), xsc)),
                new SM(XamlLanguage.Arguments),
                    new SO(new XamlType(typeof(string), xsc)),
                        new SM(XamlLanguage.Initialization),
                            new V("foo"),
                        new EM(),
                    new EO(),
                    new SO(new XamlType(typeof(string), xsc)),
                        new SM(XamlLanguage.Initialization),
                            new V("bar"),
                        new EM(),
                    new EO(),
                new EM(),
            new EO()
        };

        private string Arguments_Constructor2_X = @"<?xml version=""1.0"" encoding=""utf-16""?>
<ConstructorArguments2 xmlns=""clr-namespace:Test.NodeStream;assembly=XamlTestClasses"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
  <x:Arguments>
    <x:String>foo</x:String>
    <x:String>bar</x:String>
  </x:Arguments>
</ConstructorArguments2>";

        private ConstructorArguments2 Arguments_Constructor2_O = new ConstructorArguments2("foo", "bar");

        [TestMethod]
        public void Arguments_Constructor2_X2N()
        {
            CheckXmlToNodes(Arguments_Constructor2_X, Arguments_Constructor2_N);
        }

        [TestMethod]
        public void Arguments_Constructor2_N2X()
        {
            CheckNodesToXml(Arguments_Constructor2_N, Arguments_Constructor2_X);
        }

        [TestMethod]
        public void Arguments_Constructor2_O2N()
        {
            CheckObjectToNodes(Arguments_Constructor2_O, Arguments_Constructor2_N);
        }

        [TestMethod]
        public void Arguments_Constructor2_N2O()
        {
            CheckNodesToObject(Arguments_Constructor2_N, Arguments_Constructor2_O);
        }

        private Node[] Arguments_Factory1_N = new Node[] {
            new NS(new NamespaceDeclaration("clr-namespace:Test.NodeStream;assembly=XamlTestClasses", "")),
            new NS(new NamespaceDeclaration("http://schemas.microsoft.com/winfx/2006/xaml", "x")),
            new SO(new XamlType(typeof(FactoryArguments1), xsc)),
                new SM(XamlLanguage.FactoryMethod),
                    new V("Factory"),
                new EM(),
                new SM(XamlLanguage.Arguments),
                    new SO(new XamlType(typeof(string), xsc)),
                        new SM(XamlLanguage.Initialization),
                            new V("foo"),
                        new EM(),
                    new EO(),
                new EM(),
            new EO()
        };

        private string Arguments_Factory1_X = @"<?xml version=""1.0"" encoding=""utf-16""?>
<FactoryArguments1 x:FactoryMethod=""Factory"" xmlns=""clr-namespace:Test.NodeStream;assembly=XamlTestClasses"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
  <x:Arguments>
    <x:String>foo</x:String>
  </x:Arguments>
</FactoryArguments1>";

        private FactoryArguments1 Arguments_Factory1_O = FactoryArguments1.Factory("foo");

        [TestMethod]
        public void Arguments_Factory1_X2N()
        {
            CheckXmlToNodes(Arguments_Factory1_X, Arguments_Factory1_N);
        }

        [TestMethod]
        public void Arguments_Factory1_N2X()
        {
            CheckNodesToXml(Arguments_Factory1_N, Arguments_Factory1_X);
        }

        [TestMethod]
        public void Arguments_Factory1_O2N()
        {
            CheckObjectToNodes(Arguments_Factory1_O, Arguments_Factory1_N);
        }

        [TestMethod]
        public void Arguments_Factory1_N2O()
        {
            CheckNodesToObject(Arguments_Factory1_N, Arguments_Factory1_O);
        }

        private Node[] Arguments_Factory2_N = new Node[] {
            new NS(new NamespaceDeclaration("clr-namespace:Test.NodeStream;assembly=XamlTestClasses", "")),
            new NS(new NamespaceDeclaration("http://schemas.microsoft.com/winfx/2006/xaml", "x")),
            new SO(new XamlType(typeof(FactoryArguments2), xsc)),
                new SM(XamlLanguage.FactoryMethod),
                    new V("Factory"),
                new EM(),
                new SM(XamlLanguage.Arguments),
                    new SO(new XamlType(typeof(string), xsc)),
                        new SM(XamlLanguage.Initialization),
                            new V("foo"),
                        new EM(),
                    new EO(),
                    new SO(new XamlType(typeof(string), xsc)),
                        new SM(XamlLanguage.Initialization),
                            new V("bar"),
                        new EM(),
                    new EO(),
                new EM(),
            new EO()
        };

        private string Arguments_Factory2_X = @"<?xml version=""1.0"" encoding=""utf-16""?>
<FactoryArguments2 x:FactoryMethod=""Factory"" xmlns=""clr-namespace:Test.NodeStream;assembly=XamlTestClasses"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
  <x:Arguments>
    <x:String>foo</x:String>
    <x:String>bar</x:String>
  </x:Arguments>
</FactoryArguments2>";

        private FactoryArguments2 Arguments_Factory2_O = FactoryArguments2.Factory("foo", "bar");

        [TestMethod]
        public void Arguments_Factory2_X2N()
        {
            CheckXmlToNodes(Arguments_Factory2_X, Arguments_Factory2_N);
        }

        [TestMethod]
        public void Arguments_Factory2_N2X()
        {
            CheckNodesToXml(Arguments_Factory2_N, Arguments_Factory2_X);
        }

        [TestMethod]
        public void Arguments_Factory2_O2N()
        {
            CheckObjectToNodes(Arguments_Factory2_O, Arguments_Factory2_N);
        }

        [TestMethod]
        public void Arguments_Factory2_N2O()
        {
            CheckNodesToObject(Arguments_Factory2_N, Arguments_Factory2_O);
        }

        [TestMethod]
        public void KnownDirectiveNotFromXAML()
        {
            Node[] n = new Node[] {
                new NS(new NamespaceDeclaration("http://schemas.microsoft.com/winfx/2006/xaml", "")),
                new SO(new XamlType(typeof(Button), xsc)),
                    new SM(new XamlDirective(XamlLanguage.Arguments.GetXamlNamespaces(),
                        "Foo",
                        new XamlType(typeof(Button), xsc),
                        XamlLanguage.Arguments.TypeConverter,
                        XamlLanguage.Arguments.AllowedLocation)),
                        new SO(new XamlType(typeof(Button), xsc)),
                        new EO(),
                        new V("test"),
                    new EM(),
                new EO()
            };
            NodesToXml(n);
        }

        private string Protected_CP_Xml = @"<?xml version=""1.0"" encoding=""utf-16""?>
<ProtectedRecursive xmlns=""clr-namespace:Test.Properties;assembly=XamlTestClasses"">
    <ProtectedRecursive>
        <ProtectedRecursive/>
    </ProtectedRecursive>
</ProtectedRecursive>";

        [TestMethod]
        public void Protected_CP_FullTrust_DontAllowProtectedMembersOnRoot()
        {
            XamlType typeOfProtectedRecursive = xsc.GetXamlType(typeof(Test.Properties.ProtectedRecursive));
            XamlMember unknownContentProp = new XamlMember("Recurse", typeOfProtectedRecursive, false);
            Node[] nodes = new Node[] {
                new NS(new NamespaceDeclaration("clr-namespace:Test.Properties;assembly=XamlTestClasses", "")),
                new SO(typeOfProtectedRecursive),
                    new SM(unknownContentProp),
                        new SO(typeOfProtectedRecursive),
                            new SM(unknownContentProp),
                                new SO(typeOfProtectedRecursive),
                                new EO(),
                            new EM(),
                        new EO(),
                    new EM(),
                new EO()
            };
            CheckXmlToNodes(Protected_CP_Xml, nodes);
        }

        [TestMethod]
        public void Protected_CP_FullTrust_AllowProtectedMembersOnRoot()
        {
            XamlType typeOfProtectedRecursive = xsc.GetXamlType(typeof(Test.Properties.ProtectedRecursive));
            XamlMember unknownContentProp = new XamlMember("Recurse", typeOfProtectedRecursive, false);
            BindingFlags recurseBindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetProperty;
            PropertyInfo recursePropertyInfo = typeof(Test.Properties.ProtectedRecursive).GetProperty("Recurse", recurseBindingFlags);
            Node[] nodes = new Node[] {
                new NS(new NamespaceDeclaration("clr-namespace:Test.Properties;assembly=XamlTestClasses", "")),
                new SO(typeOfProtectedRecursive),
                    new SM(new XamlMember(recursePropertyInfo, xsc)),
                        new SO(typeOfProtectedRecursive),
                            new SM(unknownContentProp),
                                new SO(typeOfProtectedRecursive),
                                new EO(),
                            new EM(),
                        new EO(),
                    new EM(),
                new EO()
            };
            CheckXmlToNodes(Protected_CP_Xml, nodes, new XamlXmlReaderSettings { AllowProtectedMembersOnRoot = true });
        }

        private string Internal_CP_Xml = @"<?xml version=""1.0"" encoding=""utf-16""?>
<InternalRecursive xmlns=""clr-namespace:Test.Properties;assembly=XamlTestClasses"">
    <InternalRecursive>
        <InternalRecursive/>
    </InternalRecursive>
</InternalRecursive>";

        [TestMethod]
        public void Internal_CP_FullTrust_DontSetLocalAssembly()
        {
            XamlType typeOfInternalRecursive = xsc.GetXamlType(typeof(Test.Properties.InternalRecursive));
            XamlMember unknownContentProp = new XamlMember("Recurse", typeOfInternalRecursive, false);
            Node[] nodes = new Node[] {
                new NS(new NamespaceDeclaration("clr-namespace:Test.Properties;assembly=XamlTestClasses", "")),
                new SO(typeOfInternalRecursive),
                    new SM(unknownContentProp),
                        new SO(typeOfInternalRecursive),
                            new SM(unknownContentProp),
                                new SO(typeOfInternalRecursive),
                                new EO(),
                            new EM(),
                        new EO(),
                    new EM(),
                new EO()
            };
            CheckXmlToNodes(Internal_CP_Xml, nodes);
        }

        [TestMethod]
        public void Internal_CP_FullTrust_SetLocalAssembly()
        {
            BindingFlags recurseBindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetProperty;
            PropertyInfo recursePropertyInfo = typeof(Test.Properties.InternalRecursive).GetProperty("Recurse", recurseBindingFlags);
            Node[] nodes = new Node[] {
                new NS(new NamespaceDeclaration("clr-namespace:Test.Properties;assembly=XamlTestClasses", "")),
                new SO(new XamlType(typeof(Test.Properties.InternalRecursive), xsc)),
                    new SM(new XamlMember(recursePropertyInfo, xsc)),
                        new SO(new XamlType(typeof(Test.Properties.InternalRecursive), xsc)),
                            new SM(new XamlMember(recursePropertyInfo, xsc)),
                                new SO(new XamlType(typeof(Test.Properties.InternalRecursive), xsc)),
                                new EO(),
                            new EM(),
                        new EO(),
                    new EM(),
                new EO()
            };
            CheckXmlToNodes(Internal_CP_Xml, nodes, new XamlXmlReaderSettings { LocalAssembly = typeof(Test.Properties.ProtectedRecursive).Assembly });
        }

        private string NodesToXml(IEnumerable<Node> ns)
        {
            StringWriter s = new StringWriter();

            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.Encoding = Encoding.Unicode;
            XmlWriter xmlWriter = XmlWriter.Create(s, settings);

            XamlXmlWriter w = new XamlXmlWriter(xmlWriter, xsc);
            foreach (Node n in ns)
            {
                if (n is EM)
                {
                    w.WriteEndMember();
                }
                else if (n is EO)
                {
                    w.WriteEndObject();
                }
                else if (n is GO)
                {
                    w.WriteGetObject();
                }
                else if (n is NS)
                {
                    w.WriteNamespace((n as NS).Value);
                }
                else if (n is SM)
                {
                    w.WriteStartMember((n as SM).Value);
                }
                else if (n is SO)
                {
                    w.WriteStartObject((n as SO).Value);
                }
                else if (n is V)
                {
                    w.WriteValue((n as V).Value);
                }
            }
            w.Close();
            return s.ToString();
        }

        private IEnumerable<Node> XmlToNodes(string x)
        {
            return XmlToNodes(x, new XamlXmlReaderSettings());
        }

        private IEnumerable<Node> XmlToNodes(string x, XamlXmlReaderSettings xamlXmlReaderSettings)
        {
            XamlXmlReader r = new XamlXmlReader(XmlReader.Create(new StringReader(x)), xamlXmlReaderSettings);
            while (r.Read())
            {
                switch (r.NodeType)
                {
                    case XamlNodeType.EndMember:
                        yield return new EM();
                        break;
                    case XamlNodeType.EndObject:
                        yield return new EO();
                        break;
                    case XamlNodeType.GetObject:
                        yield return new GO();
                        break;
                    case XamlNodeType.NamespaceDeclaration:
                        yield return new NS(r.Namespace);
                        break;
                    case XamlNodeType.StartMember:
                        yield return new SM(r.Member);
                        break;
                    case XamlNodeType.StartObject:
                        yield return new SO(r.Type);
                        break;
                    case XamlNodeType.Value:
                        yield return new V(r.Value);
                        break;
                }
            }
        }

        private object NodesToObject(IEnumerable<Node> ns)
        {
            XamlObjectWriter w = new XamlObjectWriter(xsc);
            foreach (Node n in ns)
            {
                if (n is EM)
                {
                    w.WriteEndMember();
                }
                else if (n is EO)
                {
                    w.WriteEndObject();
                }
                else if (n is GO)
                {
                    w.WriteGetObject();
                }
                else if (n is NS)
                {
                    w.WriteNamespace((n as NS).Value);
                }
                else if (n is SM)
                {
                    w.WriteStartMember((n as SM).Value);
                }
                else if (n is SO)
                {
                    w.WriteStartObject((n as SO).Value);
                }
                else if (n is V)
                {
                    w.WriteValue((n as V).Value);
                }
            }
            w.Close();
            return w.Result;
        }

        private IEnumerable<Node> ObjectToNodes(object o)
        {
            XamlObjectReader r = new XamlObjectReader(o);
            while (r.Read())
            {
                switch (r.NodeType)
                {
                    case XamlNodeType.EndMember:
                        yield return new EM();
                        break;
                    case XamlNodeType.EndObject:
                        yield return new EO();
                        break;
                    case XamlNodeType.GetObject:
                        yield return new GO();
                        break;
                    case XamlNodeType.NamespaceDeclaration:
                        yield return new NS(r.Namespace);
                        break;
                    case XamlNodeType.StartMember:
                        yield return new SM(r.Member);
                        break;
                    case XamlNodeType.StartObject:
                        yield return new SO(r.Type);
                        break;
                    case XamlNodeType.Value:
                        yield return new V(r.Value);
                        break;
                }
            }
        }

        private void CheckNodesToXml(IEnumerable<Node> actual, string expected)
        {
            string actualXml = NodesToXml(actual);
            if (!expected.Equals(actualXml))
            {
                throw new Exception(string.Format(
                    "Expected and actual XML mismatch.\n\nExpected XML:\n\n{0}\nActual XML:\n\n{1}",
                    expected,
                    actualXml));
            }
        }

        private void CheckBadNodesToXml(IEnumerable<Node> actual, string expected)
        {
            try
            {
                CheckNodesToXml(actual, expected);
            }
            catch (XamlXmlWriterException)
            {
                return;
            }
            catch (Exception e)
            {
                throw e;
            }
            throw new Exception("XamlXmlWriter accepted an invalid node stream:\n\n" + NodesToString(actual));
        }

        private void CheckXmlToNodes(string actual, IEnumerable<Node> expected)
        {
            NodesEqual(XmlToNodes(actual), expected);
        }

        private void CheckXmlToNodes(string actual, IEnumerable<Node> expected, XamlXmlReaderSettings xamlXmlReaderSettings)
        {
            NodesEqual(XmlToNodes(actual, xamlXmlReaderSettings), expected);
        }

        private void CheckNodesToObject(IEnumerable<Node> actual, object expected)
        {
            if (!expected.Equals(NodesToObject(actual)))
            {
                throw new Exception(string.Format(
                    "Expected and actual object mismatch. Expected object: {0}. Actual object: {1}.",
                    expected.ToString(),
                    actual.ToString()));
            }
        }

        private void CheckObjectToNodes(object actual, IEnumerable<Node> expected)
        {
            NodesEqual(ObjectToNodes(actual), expected);
        }

        private string NodesToString(IEnumerable<Node> ns)
        {
            StringBuilder b = new StringBuilder();
            foreach (Node n in ns)
            {
                if (b.Length > 0)
                {
                    b.Append("\n");
                }
                b.Append(n.ToString());
            }
            return b.ToString();
        }

        private void NodesEqual(IEnumerable<Node> actual, IEnumerable<Node> expected)
        {
            using (IEnumerator<Node> expectedEnumerator = expected.GetEnumerator(), actualEnumerator = actual.GetEnumerator())
            {
                bool expectedCheck, actualCheck;
                do
                {
                    expectedCheck = expectedEnumerator.MoveNext();
                    actualCheck = actualEnumerator.MoveNext();
                    if ((expectedCheck != actualCheck) || (expectedCheck && actualCheck && !expectedEnumerator.Current.Equals(actualEnumerator.Current)))
                    {
                        throw new Exception(string.Format(
                            "Expected and actual node stream mismatch.\n\nExpected node stream:\n\n{0}\n\nActual node stream:\n\n{1}",
                            NodesToString(expected),
                            NodesToString(actual)));
                    }
                } while (expectedCheck && actualCheck);
            }
        }
    }
}