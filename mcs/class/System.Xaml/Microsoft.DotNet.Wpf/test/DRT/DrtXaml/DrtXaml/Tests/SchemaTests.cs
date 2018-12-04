using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Reflection.Emit;
using DRT;
using System.Windows.Markup;
using System.Xaml;
using System.Xaml.Schema;
using DrtXaml.XamlTestFramework;
using Test.Elements;
using System.Xml;
using System.IO;

namespace DrtXaml.Tests
{
    [TestClass]
    class SchemaTests : XamlTestSuite
    {
        const string TestNamespace = "clr-namespace:Test.Elements;assembly=XamlTestClasses";

        public SchemaTests()
            : base("SchemaTests")
        {
            _schemaContext = new XamlSchemaContext();
        }

        private XamlSchemaContext _schemaContext;

        public override DrtTest[] PrepareTests()
        {
            DrtTest[] tests = DrtTestFinder.FindTests(this);
            return tests;
        }

        // =============================================

        private XamlSchemaContext SchemaContext
        {
            get { return _schemaContext; }
        }

        [TestMethod]
        public void GetAllAttachableMembers()
        {
            XamlType xt = _schemaContext.GetXamlType(typeof(HasAtt));

            Assert.AreEqual(1, xt.GetAllAttachableMembers().Count);
        }

        [TestMethod, TestExpectedException(typeof(XamlObjectWriterException))]
        public void LoadingUnreferencedAssembly()
        {
            var xsc = new XamlSchemaContext(new List<Assembly> { Assembly.GetExecutingAssembly() });
            string s = XamlServices.Save(new Element());
            using (XmlReader reader = XmlReader.Create(new StringReader(s)))
            {
                XamlXmlReader xamlReader = new XamlXmlReader(reader, xsc);
                XamlObjectWriter xamlWriter = new XamlObjectWriter(xamlReader.SchemaContext);
                XamlServices.Transform(xamlReader, xamlWriter);
            }
        }

        [TestMethod]
        public void Nullable()
        {
            XamlType element = SchemaContext.GetXamlType(TestNamespace, "Element");
            if (!element.IsNullable)
            {
                throw new InvalidOperationException("Test.Element should be nullable");
            }

            XamlType dbl = SchemaContext.GetXamlType(typeof(double));
            if (dbl.IsNullable)
            {
                throw new InvalidOperationException("'double' should not be nullable");
            }

            XamlType n_dbl = SchemaContext.GetXamlType(typeof(double?));
            if (!n_dbl.IsNullable)
            {
                throw new InvalidOperationException("'double?' should be nullable");
            }
        }

        private XamlType[] EnumOfXamlTypeToArray(IEnumerable<XamlType> ienum)
        {
            List<XamlType> list = new List<XamlType>();
            foreach (XamlType xt in ienum)
            {
                list.Add(xt);
            }
            return list.ToArray();
        }

        private bool CompareArraysOfXamlTypes_OneWay(XamlType[] aList, XamlType[] bList)
        {
            bool found;
            foreach (XamlType a in aList)
            {
                found = false;
                foreach (XamlType b in bList)
                {
                    if (a == b)
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    return false;
                }
            }
            return true;
        }

        private bool CompareArraysOfXamlTypes(XamlType[] aList, XamlType[] bList)
        {
            if (aList.Length != bList.Length)
            {
                return false;
            }
            if (!CompareArraysOfXamlTypes_OneWay(aList, bList))
            {
                return false;
            }
            if (!CompareArraysOfXamlTypes_OneWay(bList, aList))
            {
                return false;
            }
            return true;
        }

        [TestMethod]
        public void Dictionary_KeyAndCollectionTypes()
        {
            XamlType stringXt = SchemaContext.GetXamlType(typeof(string));
            XamlType intXt = SchemaContext.GetXamlType(typeof(int));
            XamlType doubleXt = SchemaContext.GetXamlType(typeof(double));

            // Not a collection, should fail gracefully
            XamlType element = SchemaContext.GetXamlType(TestNamespace, "Element");
            Assert.IsFalse(element.IsDictionary);
            Assert.IsNull(element.KeyType);
            Assert.IsNull(element.ItemType);
            Assert.IsNull(element.AllowedContentTypes);

            // -----------------------------------
            XamlType list1 = SchemaContext.GetXamlType(typeof(Dictionary<String, double>));
            Assert.IsTrue(list1.IsDictionary);
            Assert.AreEqual(stringXt, list1.KeyType);
            Assert.AreEqual(doubleXt, list1.ItemType);
            Assert.AreEqualUnordered(list1.AllowedContentTypes, list1.ItemType);

            // -----------------------------------
            XamlType list2 = SchemaContext.GetXamlType(TestNamespace, "DictionaryPlusAddExtraKeys");
            Assert.IsTrue(list2.IsDictionary);
            Assert.AreEqual(stringXt, list2.KeyType);
            Assert.AreEqual(element, list2.ItemType);
            Assert.AreEqualUnordered(list2.AllowedContentTypes, list2.ItemType);

            // -----------------------------------
            XamlType list = SchemaContext.GetXamlType(TestNamespace, "DictionaryPlusAddExtraValues");
            Assert.IsTrue(list.IsDictionary);
            Assert.AreEqual(stringXt, list.KeyType);
            Assert.AreEqual(stringXt, list.ItemType);
            Assert.AreEqualUnordered(list.AllowedContentTypes, list.ItemType);

            // -----------------------------------
            list = SchemaContext.GetXamlType(TestNamespace, "IDictionaryExplicitImpl");
            Assert.IsTrue(list.IsDictionary);
            Assert.AreEqual(stringXt, list.KeyType);
            Assert.AreEqual(stringXt, list.ItemType);
            Assert.AreEqualUnordered(list.AllowedContentTypes, list.ItemType);

            // -----------------------------------
            list = SchemaContext.GetXamlType(TestNamespace, "DictionaryPlusExplicitImpl");
            Assert.IsTrue(list.IsDictionary);
            Assert.AreEqual(stringXt, list.KeyType);
            Assert.AreEqual(stringXt, list.ItemType);
            Assert.AreEqualUnordered(list.AllowedContentTypes, list.ItemType);

            // -----------------------------------
            list = SchemaContext.GetXamlType(TestNamespace, "DictionaryIEnumerablePlusAdd");
            Assert.IsTrue(list.IsDictionary);
            Assert.AreEqual(doubleXt, list.KeyType);
            Assert.AreEqual(doubleXt, list.ItemType);
            Assert.AreEqualUnordered(list.AllowedContentTypes, list.ItemType);

            // -----------------------------------
            list = SchemaContext.GetXamlType(TestNamespace, "DictionaryGetEnumeratorPlusAdd");
            Assert.IsTrue(list.IsDictionary);
            Assert.AreEqual(doubleXt, list.KeyType);
            Assert.AreEqual(doubleXt, list.ItemType);
            Assert.AreEqualUnordered(list.AllowedContentTypes, list.ItemType);

            // -----------------------------------
            list = SchemaContext.GetXamlType(TestNamespace, "DictionaryPrivateGetEnumeratorPlusAdd");
            Assert.IsFalse(list.IsDictionary);
            Assert.IsNull(list.KeyType);
            Assert.IsNull(list.ItemType);
            Assert.IsNull(list.AllowedContentTypes);

            // -----------------------------------
            list = SchemaContext.GetXamlType(TestNamespace, "DictionaryAmbiguousInterface");
            XamlType itemType;
            Assert.IsTrue(list.IsDictionary);
            AssertException<XamlSchemaException>(() => itemType = list.ItemType);

            // -----------------------------------
            list = SchemaContext.GetXamlType(TestNamespace, "DictionaryAmbiguousMethods");
            Assert.IsTrue(list.IsDictionary);
            AssertException<XamlSchemaException>(() => itemType = list.ItemType);
        }

        [TestMethod]
        public void IEnumeratorPlusAdd_CollectionTypes()
        {
            XamlType stringXt = SchemaContext.GetXamlType(typeof(string));
            XamlType objectXt = SchemaContext.GetXamlType(typeof(object));
            XamlType kidXt = SchemaContext.GetXamlType(TestNamespace, "Kid");
            XamlType elementXt = SchemaContext.GetXamlType(TestNamespace, "Element");

            // Not a collection, should fail gracefully
            Assert.IsFalse(elementXt.IsCollection);
            Assert.IsNull(elementXt.ItemType);
            Assert.IsNull(elementXt.AllowedContentTypes);

            // -----------------------------------
            XamlType list1 = SchemaContext.GetXamlType(typeof(List<string>));
            Assert.IsTrue(list1.IsCollection);
            Assert.AreEqual(stringXt, list1.ItemType);
            Assert.AreEqualUnordered(list1.AllowedContentTypes, stringXt);

            // -----------------------------------
            XamlType kidList = SchemaContext.GetXamlType(TestNamespace, "KidList");
            Assert.IsTrue(kidList.IsCollection);
            Assert.AreEqual(kidXt, kidList.ItemType);
            Assert.AreEqualUnordered(kidList.AllowedContentTypes, kidXt);

            // -----------------------------------
            XamlType enumPlusAdd = SchemaContext.GetXamlType(TestNamespace, "EnumPlusAdd");
            Assert.IsTrue(enumPlusAdd.IsCollection);
            Assert.AreEqual(elementXt, enumPlusAdd.ItemType);
            Assert.AreEqualUnordered(enumPlusAdd.AllowedContentTypes, elementXt);

            // -----------------------------------
            XamlType testType = SchemaContext.GetXamlType(TestNamespace, "GetEnumPlusAdd");
            Assert.IsTrue(testType.IsCollection);
            Assert.AreEqual(elementXt, testType.ItemType);
            Assert.AreEqualUnordered(testType.AllowedContentTypes, elementXt);

            // -----------------------------------
            testType = SchemaContext.GetXamlType(TestNamespace, "EnumPlusContentWrapper");
            Assert.IsTrue(testType.IsCollection);
            Assert.AreEqual(objectXt, testType.ItemType);
            Assert.AreEqualUnordered(testType.AllowedContentTypes, objectXt, elementXt, kidXt);

            // -----------------------------------
            testType = SchemaContext.GetXamlType(TestNamespace, "ExplicitIList");
            Assert.IsTrue(testType.IsCollection);
            Assert.AreEqual(objectXt, testType.ItemType);
            Assert.AreEqualUnordered(testType.AllowedContentTypes, objectXt);

            // -----------------------------------
            testType = SchemaContext.GetXamlType(TestNamespace, "ExplicitICollectionOfString");
            Assert.IsTrue(testType.IsCollection);
            Assert.AreEqual(stringXt, testType.ItemType);
            Assert.AreEqualUnordered(testType.AllowedContentTypes, stringXt);

            // -----------------------------------
            testType = SchemaContext.GetXamlType(TestNamespace, "ImplicitAndExplicitCollection");
            Assert.IsTrue(testType.IsCollection);
            Assert.AreEqual(stringXt, testType.ItemType);
            Assert.AreEqualUnordered(testType.AllowedContentTypes, stringXt);

            // -----------------------------------
            testType = SchemaContext.GetXamlType(TestNamespace, "AmbiguousCollectionInterface");
            XamlType itemType;
            Assert.IsTrue(testType.IsCollection);
            AssertException<XamlSchemaException>(() => itemType = testType.ItemType);

            // -----------------------------------
            testType = SchemaContext.GetXamlType(TestNamespace, "AmbiguousCollectionMethods");
            Assert.IsTrue(testType.IsCollection);
            AssertException<XamlSchemaException>(() => itemType = testType.ItemType);

            // -----------------------------------
            testType = SchemaContext.GetXamlType(TestNamespace, "CollectionWithTwoParameterAdd");
            Assert.IsTrue(testType.IsCollection);
            Assert.IsFalse(testType.IsDictionary);
            Assert.AreEqual(XamlLanguage.String, testType.ItemType);
            Assert.IsNull(testType.KeyType);
        }

        [TestMethod]
        public void NamespaceOrdering()
        {
            XamlType type = SchemaContext.GetXamlType("http://test.xaml/ns1/2005", "Foo");
            IList<string> namespaces = type.GetXamlNamespaces();
            Assert.AreEqualOrdered(namespaces,
                "http://test.xaml/ns1/2008",
                "http://test.xaml/ns1/2005",
                "clr-namespace:XmlNsClasses.Ns1;assembly=XamlTestClasses");
        }

        [TestMethod]
        public void BuiltInTypeNamespaces()
        {
            XamlType type = SchemaContext.GetXamlType(typeof(string));
            IList<string> namespaces = type.GetXamlNamespaces();
            Assert.AreEqualOrdered(namespaces,
                "http://schemas.microsoft.com/winfx/2006/xaml",
                string.Format("clr-namespace:System;assembly={0}", typeof(IList<string>).GetAssemblyName()));
        }

        [TestMethod]
        public void PreferredPrefix()
        {
            string prefix = SchemaContext.GetPreferredPrefix("http://test.xaml/ns1/2005");
            Assert.AreEqual("ns1", prefix);
            prefix = SchemaContext.GetPreferredPrefix("http://test.xaml/ns1/2008");
            Assert.AreEqual("ns1", prefix);
            prefix = SchemaContext.GetPreferredPrefix("http://schemas.microsoft.com/winfx/2006/xaml");
            Assert.AreEqual("x", prefix);
            prefix = SchemaContext.GetPreferredPrefix("http://random");
            Assert.AreEqual("p", prefix);
            prefix = SchemaContext.GetPreferredPrefix("clr-namespace:XmlNsClasses.Ns1;assembly=XamlTestClasses");
            Assert.AreEqual("xn", prefix);
            prefix = SchemaContext.GetPreferredPrefix("clr-namespace:XmlNsClasses;assembly=XamlTestClasses");
            Assert.AreEqual("p", prefix);
            prefix = SchemaContext.GetPreferredPrefix("clr-namespace:XmlNsClasses");
            Assert.AreEqual("local", prefix);

            XamlSchemaContext schemaContext = new XamlSchemaContext(new Assembly[0]);
            prefix = SchemaContext.GetPreferredPrefix("http://schemas.microsoft.com/winfx/2006/xaml");
            Assert.AreEqual("x", prefix);
        }

        [TestMethod]
        public void GetAllXamlNamespaces()
        {
            Assembly asmWindowsBase = typeof(System.Windows.Markup.DesignerSerializationOptions).Assembly;
            Assembly asmSysXaml = typeof(System.Xaml.XamlType).Assembly;
            Assembly asmTestClasses = Assembly.Load("XamlTestClasses");
            Assembly[] assemblies = new Assembly[] { asmWindowsBase, asmSysXaml, asmTestClasses };
            IEnumerable<string> namespaces = new XamlSchemaContext(assemblies).GetAllXamlNamespaces();
            Assert.AreEqualUnordered(new List<string>(namespaces),
                "http://schemas.microsoft.com/winfx/2006/xaml/presentation",
                "http://schemas.microsoft.com/netfx/2007/xaml/presentation",
                "http://schemas.microsoft.com/netfx/2009/xaml/presentation",
                "http://schemas.microsoft.com/xps/2005/06",
                "http://schemas.microsoft.com/winfx/2006/xaml/composite-font",
                "http://schemas.microsoft.com/winfx/2006/xaml",
                "http://test.xaml/ns1/2008",
                "http://test.xaml/ns1/2005",
                "http://foo",
                "http://bar",
                "http://testroot"
                );
        }

        Dictionary<string, Action<XamlTypeName>[]> validXamlTypeNames =
            new Dictionary<string, Action<XamlTypeName>[]>()
            {
                { "Foo", new Action<XamlTypeName>[] {
                    typeName => Assert.AreEqual("default", typeName.Namespace),
                    typeName => Assert.AreEqual("Foo", typeName.Name) }
                },
                { "a:Foo", new Action<XamlTypeName>[] {
                    typeName => Assert.AreEqual("a_namespace", typeName.Namespace),
                    typeName => Assert.AreEqual("Foo", typeName.Name) }
                },
                { "a:Foo(Bar)", new Action<XamlTypeName>[] {
                    typeName => Assert.AreEqual("a_namespace", typeName.Namespace),
                    typeName => Assert.AreEqual("Foo", typeName.Name),
                    typeName => Assert.AreEqual(1, typeName.TypeArguments.Count),
                    typeName => Assert.AreEqual("default", typeName.TypeArguments[0].Namespace),
                    typeName => Assert.AreEqual("Bar", typeName.TypeArguments[0].Name) }
                },
                { "a:Foo(Bar, b:Baz)", new Action<XamlTypeName>[] {
                    typeName => Assert.AreEqual("a_namespace", typeName.Namespace),
                    typeName => Assert.AreEqual("Foo", typeName.Name),
                    typeName => Assert.AreEqual(2, typeName.TypeArguments.Count),
                    typeName => Assert.AreEqual("default", typeName.TypeArguments[0].Namespace),
                    typeName => Assert.AreEqual("Bar", typeName.TypeArguments[0].Name),
                    typeName => Assert.AreEqual("b_namespace", typeName.TypeArguments[1].Namespace),
                    typeName => Assert.AreEqual("Baz", typeName.TypeArguments[1].Name) }
                }
            };

        [TestMethod]
        public void XamlTypeName_Positive()
        {
            IXamlNamespaceResolver resolver = new SimpleNamespaceResolver();
            foreach (string validString in validXamlTypeNames.Keys)
            {
                Action<XamlTypeName>[] validators = validXamlTypeNames[validString];

                XamlTypeName result = XamlTypeName.Parse(validString, resolver);
                RunActions(result, validators);

                Assert.IsTrue(XamlTypeName.TryParse(validString, resolver, out result));
                RunActions(result, validators);

                string normalizedString = NormalizeCommas(validString);
                string roundTrippedString = result.ToString(new SimplePrefixLookup());
                Assert.AreEqual(normalizedString, roundTrippedString);
            }
        }

        [TestMethod]
        public void XamlTypeNameList_Positive()
        {
            IXamlNamespaceResolver resolver = new SimpleNamespaceResolver();
            foreach (string[] typeNames in CombinationsOf(validXamlTypeNames.Keys))
            {
                string typeListString = string.Join(",", typeNames);

                IList<XamlTypeName> result = XamlTypeName.ParseList(typeListString, resolver);
                Assert.AreEqual(typeNames.Length, result.Count);
                for (int i = 0; i < result.Count; i++)
                {
                    Action<XamlTypeName>[] validators = validXamlTypeNames[typeNames[i]];
                    RunActions(result[i], validators);
                }

                Assert.IsTrue(XamlTypeName.TryParseList(typeListString, resolver, out result));
                Assert.AreEqual(typeNames.Length, result.Count);
                for (int i = 0; i < result.Count; i++)
                {
                    Action<XamlTypeName>[] validators = validXamlTypeNames[typeNames[i]];
                    RunActions(result[i], validators);
                }

                string normalizedString = NormalizeCommas(typeListString);
                string roundTrippedString = XamlTypeName.ToString(result, new SimplePrefixLookup());
                Assert.AreEqual(normalizedString, roundTrippedString);
            }
        }

        [TestMethod]
        public void XamlTypeNameParse_ArgumentNull()
        {
            XamlTypeName result;
            IList<XamlTypeName> resultList;
            AssertException<ArgumentNullException>(() => XamlTypeName.Parse(null, new SimpleNamespaceResolver()));
            AssertException<ArgumentNullException>(() => XamlTypeName.TryParse("z:Foo", null, out result));
            AssertException<ArgumentNullException>(() => XamlTypeName.ParseList(null, new SimpleNamespaceResolver()));
            AssertException<ArgumentNullException>(() => XamlTypeName.TryParseList("z:Foo", null, out resultList));
        }

        [TestMethod]
        public void XamlTypeNameParse_Negative()
        {
            NegativeXamlTypeNameParse(string.Empty);
            NegativeXamlTypeNameParse("z:Foo");
            NegativeXamlTypeNameParse(";");
            NegativeXamlTypeNameParse("a:;");
            NegativeXamlTypeNameParse("a:Foo;");
            NegativeXamlTypeNameParse("a:Foo;");
            NegativeXamlTypeNameParse("a:Foo[");
            NegativeXamlTypeNameParse("a:Foo[],");
            NegativeXamlTypeNameParse("a:Foo[][");
            NegativeXamlTypeNameParse("a:Foo[*]");
            NegativeXamlTypeNameParse("[a:Foo");
            NegativeXamlTypeNameParse("a:Foo(");
            NegativeXamlTypeNameParse("a:Foo)");
            NegativeXamlTypeNameParse("a:Foo(;)");
            NegativeXamlTypeNameParse("a:Foo(b:)");
            NegativeXamlTypeNameParse("a:Foo(b:Bar");
            NegativeXamlTypeNameParse("a:Foo(b:Bar);)");
            NegativeXamlTypeNameParse("a:Foo(b:Bar[)");
            NegativeXamlTypeNameParse("a:Foo(b:Bar)[");
        }

        [TestMethod]
        public void XamlTypeNameListParse_Negative()
        {
            NegativeXamlTypeNameListParse(string.Empty);
            NegativeXamlTypeNameListParse(",a:Foo");
            NegativeXamlTypeNameListParse("a:Foo,");
            NegativeXamlTypeNameListParse("a:Foo,b:Bar;");
            NegativeXamlTypeNameListParse("a:Foo(,b:Bar");
        }

        [TestMethod]
        public void XamlTypeNameToString_ArgumentNull()
        {
            XamlTypeName typeName = new XamlTypeName();
            AssertException<ArgumentNullException>(() => XamlTypeName.ToString(null, new SimplePrefixLookup()));
            XamlTypeName[] typeNameList = new XamlTypeName[] { typeName };
            AssertException<ArgumentNullException>(() => XamlTypeName.ToString(typeNameList, null));
        }

        [TestMethod]
        public void XamlTypeNameToString_Negative()
        {
            SimplePrefixLookup lookup = new SimplePrefixLookup();

            XamlTypeName typeName = new XamlTypeName() { Name = "Foo" };
            AssertException<InvalidOperationException>(() => typeName.ToString(lookup));
            typeName.Namespace = "a_namespace";
            typeName.Name = null;
            AssertException<InvalidOperationException>(() => typeName.ToString(lookup));
            typeName.Name = string.Empty;
            AssertException<InvalidOperationException>(() => typeName.ToString(lookup));
            // this will return null from the prefix lookup
            typeName.Namespace = "Foo";
            typeName.Name = "Bar";
            Assert.AreEqual("{Foo}Bar", typeName.ToString());
            AssertException<InvalidOperationException>(() => typeName.ToString(lookup));
        }

        [TestMethod]
        public void DirectivesMatchStatics()
        {
            foreach (XamlMember directive in XamlLanguage.AllDirectives)
            {
                foreach (string ns in directive.GetXamlNamespaces())
                {
                    XamlMember matchingDirective = SchemaContext.GetXamlDirective(ns, directive.Name);
                    Assert.AreEqual(directive, matchingDirective);
                }
            }
        }

        [TestMethod]
        public void BuiltinTypesMatchStatics()
        {
            foreach (XamlType type in XamlLanguage.AllTypes)
            {
                foreach (string ns in type.GetXamlNamespaces())
                {
                    XamlType matchingType = SchemaContext.GetXamlType(new XamlTypeName(ns, type.Name));
                    Assert.AreEqual(type, matchingType);
                    matchingType = SchemaContext.GetXamlType(type.UnderlyingType);
                    Assert.AreEqual(type, matchingType);
                    if (type.Name.EndsWith("Extension"))
                    {
                        string name = type.Name.Substring(0, type.Name.Length - "Extension".Length);
                        matchingType = SchemaContext.GetXamlType(new XamlTypeName(ns, name));
                        Assert.AreEqual(type, matchingType);
                    }
                }
            }
        }

        [TestMethod]
        public void XamlTypeTypeConverter()
        {
            // Non-generic CLR type
            RoundtripXamlType(typeof(Test.Elements.Element));
            // Generic CLR type
            RoundtripXamlType(typeof(List<Test.Elements.Element>));
            // Non-generic unknown type
            RoundtripXamlType(new XamlType("dummyns", "dummyType", null, SchemaContext));
            // Generic unknown type with known typearg
            RoundtripXamlType(new XamlType("dummyns", "dummyType",
                new XamlType[] { SchemaContext.GetXamlType(typeof(Test.Elements.Element)) },
                SchemaContext));
            // Generic unknown type with unknown typearg
            string listNs = SchemaContext.GetXamlType(typeof(List<>)).PreferredXamlNamespace;
            RoundtripXamlType(new XamlType(listNs, "List",
                new XamlType[] { new XamlType("dummyns", "dummyType", null, SchemaContext) },
                SchemaContext));
            // Nested class
            RoundtripXamlType(typeof(HasNested.NestedClass));
        }

        [TestMethod]
        public void ShadowedProperty()
        {
            XamlType shadowedType = SchemaContext.GetXamlType(typeof(Shadowed));
            XamlMember shadowedProperty = shadowedType.GetMember("Value");
            Assert.AreEqual(SchemaContext.GetXamlType(typeof(object)), shadowedProperty.Type);
            XamlMember shadowedEvent = shadowedType.GetMember("Event");
            Assert.AreEqual(SchemaContext.GetXamlType(typeof(EventHandler)), shadowedEvent.Type);
            XamlMember shadowedContent = shadowedType.GetMember("Content");
            Assert.AreEqual(SchemaContext.GetXamlType(typeof(Shadowed)), shadowedEvent.DeclaringType);
            ICollection<XamlMember> shadowedMembers = shadowedType.GetAllMembers();
            Assert.AreEqualUnordered(shadowedMembers, shadowedProperty, shadowedEvent, shadowedContent);

            XamlType shadowerType = SchemaContext.GetXamlType(typeof(Shadower));
            XamlMember shadowerProperty = shadowerType.GetMember("Value");
            Assert.AreEqual(XamlLanguage.String, shadowerProperty.Type);
            XamlMember shadowerEvent = shadowerType.GetMember("Event");
            Assert.AreEqual(SchemaContext.GetXamlType(typeof(EventHandler<EventArgs>)), shadowerEvent.Type);
            XamlMember shadowerContent = shadowerType.GetMember("Content");
            Assert.AreEqual(SchemaContext.GetXamlType(typeof(Shadower)), shadowerEvent.DeclaringType);
            ICollection<XamlMember> shadowerMembers = shadowerType.GetAllMembers();
            Assert.AreEqualUnordered(shadowerMembers, shadowerProperty, shadowerEvent, shadowerContent);
        }

        [TestMethod]
        public void KnownTypeEquality()
        {
            XamlSchemaContext sc2 = new XamlSchemaContext();

            // Built-in types
            XamlType type1 = SchemaContext.GetXamlType(typeof(string));
            XamlType type2 = sc2.GetXamlType(typeof(string));
            Assert.AreEqual(type1, type2);

            // Non-generic types
            type1 = SchemaContext.GetXamlType(typeof(ColorElement));
            type2 = sc2.GetXamlType(typeof(ColorElement));
            Assert.AreEqual(type1, type2);

            // Closed Generic types
            type1 = SchemaContext.GetXamlType(typeof(List<Element>));
            type2 = sc2.GetXamlType(typeof(List<Element>));
            Assert.AreEqual(type1, type2);

            // Open generic types
            type1 = SchemaContext.GetXamlType(typeof(GenericElement<>));
            type2 = sc2.GetXamlType(typeof(GenericElement<>));
            Assert.AreEqual(type1, type2);

            // Base and derived type
            type1 = SchemaContext.GetXamlType(typeof(Element));
            type2 = sc2.GetXamlType(typeof(ColorElement));
            Assert.AreNotEqual(type1, type2);

            // Open generic and its closure
            type1 = SchemaContext.GetXamlType(typeof(GenericElement<>));
            type2 = sc2.GetXamlType(typeof(GenericElement<ColorElement>));
            Assert.AreNotEqual(type1, type2);

            // Different closures of same generic
            type1 = SchemaContext.GetXamlType(typeof(GenericElement<Element>));
            type2 = sc2.GetXamlType(typeof(GenericElement<ColorElement>));
            Assert.AreNotEqual(type1, type2);
        }

        [TestMethod]
        public void UnknownTypeEquality()
        {
            XamlSchemaContext sc2 = new XamlSchemaContext();

            // Non-generic types
            XamlType type1 = GetXamlType(SchemaContext, "a:Foo");
            XamlType type2 = GetXamlType(SchemaContext, "a:Foo");
            Assert.AreEqual(type1, type2);
            // Types from different SchemaContexts
            type2 = GetXamlType(sc2, "a:Foo");
            Assert.AreEqual(type1, type2);

            // Generic types with unknown type args
            type1 = GetXamlType(SchemaContext, "a:Foo(b:Bar)");
            type2 = GetXamlType(SchemaContext, "a:Foo(b:Bar)");
            Assert.AreEqual(type1, type2);

            // Generic types with known type args
            type1 = new XamlType("a_namespace", "Foo",
                new XamlType[] { SchemaContext.GetXamlType(typeof(ColorElement)) }, SchemaContext);
            type2 = new XamlType("a_namespace", "Foo",
                new XamlType[] { sc2.GetXamlType(typeof(ColorElement)) }, sc2);
            Assert.AreEqual(type1, type2);

            // Same name, diff namespace
            type1 = GetXamlType(SchemaContext, "a:Foo");
            type2 = GetXamlType(SchemaContext, "b:Foo");
            Assert.AreNotEqual(type1, type2);

            // Same namespace, diff name
            type1 = GetXamlType(SchemaContext, "a:Foo");
            type2 = GetXamlType(SchemaContext, "a:Bar");
            Assert.AreNotEqual(type1, type2);

            // Type args with same name, diff namespace
            type1 = GetXamlType(SchemaContext, "a:Foo(b:Bar)");
            type2 = GetXamlType(SchemaContext, "a:Foo(a:Bar)");
            Assert.AreNotEqual(type1, type2);

            // Generic types with different known type args
            type1 = new XamlType("a_namespace", "Foo",
                new XamlType[] { SchemaContext.GetXamlType(typeof(ColorElement)) }, SchemaContext);
            type2 = new XamlType("a_namespace", "Foo",
                new XamlType[] { SchemaContext.GetXamlType(typeof(Element)) }, SchemaContext);
            Assert.AreNotEqual(type1, type2);
        }

        [TestMethod]
        public void KnownTypeMemberEquality()
        {
            XamlSchemaContext sc2 = new XamlSchemaContext();
            XamlType type1 = SchemaContext.GetXamlType(typeof(ColorElementDuel));
            XamlType type2 = sc2.GetXamlType(typeof(ColorElementDuel));

            // Same property
            XamlMember member1 = type1.GetMember("ColorName");
            XamlMember member2 = type2.GetMember("ColorName");
            Assert.AreEqual(member1, member2);
            // Manually created XamlMember with same PI
            member2 = new XamlMember(typeof(ColorElementDuel).GetProperty("ColorName"), SchemaContext);
            Assert.AreEqual(member1, member2);
            // XamlMember from base type
            member2 = SchemaContext.GetXamlType(typeof(ColorElement)).GetMember("ColorName");
            Assert.AreEqual(member1, member2);
            // XamlMember from base type from different schema context
            member2 = sc2.GetXamlType(typeof(ColorElement)).GetMember("ColorName");
            Assert.AreEqual(member1, member2);

            // Different properties
            member1 = type1.GetMember("ColorName");
            member2 = type1.GetMember("ColorNameCPA");
            Assert.AreNotEqual(member1, member2);
            // Different proprties from different schemacontexts
            member2 = type2.GetMember("ColorNameCPA");
            Assert.AreNotEqual(member1, member2);
            // Unknown member != known member with the same name
            member2 = new XamlMember("ColorName", type1, false /*isAttachable*/);
            Assert.AreNotEqual(member1, member2);

            // Unknown property on same known type
            member1 = new XamlMember("ColorName", type1, false /*isAttachable*/);
            member2 = new XamlMember("ColorName", type1, false /*isAttachable*/);
            Assert.AreEqual(member1, member2);
            // Unkonwn property on equal known type
            member1 = new XamlMember("ColorName", type2, false /*isAttachable*/);
            Assert.AreEqual(member1, member2);
        }

        [TestMethod]
        public void KnownTypeAttachableMemberEquality()
        {
            XamlSchemaContext sc2 = new XamlSchemaContext();
            XamlType type1 = SchemaContext.GetXamlType(typeof(APP));
            XamlType type2 = sc2.GetXamlType(typeof(APP));

            // Same property
            XamlMember member1 = type1.GetAttachableMember("Foo");
            XamlMember member2 = type2.GetAttachableMember("Foo");
            Assert.AreEqual(member1, member2);
            // Manually created XamlMember with same MIs
            member2 = new XamlMember("Foo", typeof(APP).GetMethod("GetFoo"), typeof(APP).GetMethod("SetFoo"), SchemaContext);
            Assert.AreEqual(member1, member2);

            // The following two should be true because we only compare Name and declaring type,
            // we no longer compare the underlying members
            // Manually created XamlMember with only getter
            member2 = new XamlMember("Foo", typeof(APP).GetMethod("GetFoo"), null, SchemaContext);
            Assert.AreEqual(member1, member2);
            // Manually created XamlMember with only setter
            member2 = new XamlMember("Foo", null, typeof(APP).GetMethod("SetFoo"), SchemaContext);
            Assert.AreEqual(member1, member2);

            // Different properties
            member1 = type1.GetAttachableMember("Foo");
            member2 = type1.GetAttachableMember("Bar");
            Assert.AreNotEqual(member1, member2);
            // Different proprties from different schemacontexts
            member2 = type2.GetAttachableMember("Bar");
            Assert.AreNotEqual(member1, member2);
            // Unknown member != known member with the same name
            member2 = new XamlMember("Foo", type1, true /*isAttachable*/);
            Assert.AreNotEqual(member1, member2);

            // Unknown property on same known type
            member1 = new XamlMember("Baz", type1, true /*isAttachable*/);
            member2 = new XamlMember("Baz", type1, true /*isAttachable*/);
            Assert.AreEqual(member1, member2);
            // Unkonwn property on equal known type
            member1 = new XamlMember("Baz", type2, true /*isAttachable*/);
            Assert.AreEqual(member1, member2);

            // Attachable and non-attachable with same name
            type1 = SchemaContext.GetXamlType(typeof(AttachableAndNonAtttachable));
            member1 = type1.GetAttachableMember("Foo");
            XamlMember nonAttachable = type1.GetMember("Foo");
            Assert.AreNotEqual(member1, nonAttachable);
        }

        [TestMethod]
        public void UnknownTypeMemberEquality()
        {
            // Same property
            XamlType type1 = GetXamlType(SchemaContext, "a:Foo");
            XamlMember member1 = new XamlMember("Bar", type1, false /*isAttachable*/);
            XamlMember member2 = new XamlMember("Bar", type1, false /*isAttachable*/);
            Assert.AreEqual(member1, member2);
            // Same property, equal type
            XamlSchemaContext sc2 = new XamlSchemaContext();
            XamlType type2 = GetXamlType(SchemaContext, "a:Foo");
            member2 = new XamlMember("Bar", type2, false /*isAttachable*/);
            Assert.AreEqual(member1, member2);

            // Different properties
            member1 = new XamlMember("Bar", type1, false /*isAttachable*/);
            member2 = new XamlMember("Bar2", type1, false /*isAttachable*/);
            Assert.AreNotEqual(member1, member2);
            // Same property on different types
            type2 = GetXamlType(SchemaContext, "a:Baz");
            member2 = new XamlMember("Bar", type2, false /*isAttachable*/);
            Assert.AreNotEqual(member1, member2);
        }

        [TestMethod]
        public void UnknownTypeAttachableMemberEquality()
        {
            // Same property
            XamlType type1 = GetXamlType(SchemaContext, "a:Foo");
            XamlMember member1 = new XamlMember("Bar", type1, true /*isAttachable*/);
            XamlMember member2 = new XamlMember("Bar", type1, true /*isAttachable*/);
            Assert.AreEqual(member1, member2);
            // Same property, equal type
            XamlSchemaContext sc2 = new XamlSchemaContext();
            XamlType type2 = GetXamlType(SchemaContext, "a:Foo");
            member2 = new XamlMember("Bar", type2, true /*isAttachable*/);
            Assert.AreEqual(member1, member2);

            // Different properties
            member1 = new XamlMember("Bar", type1, true /*isAttachable*/);
            member2 = new XamlMember("Bar2", type1, true /*isAttachable*/);
            Assert.AreNotEqual(member1, member2);
            // Same property on different types
            type2 = GetXamlType(SchemaContext, "a:Baz");
            member2 = new XamlMember("Bar", type2, true /*isAttachable*/);
            Assert.AreNotEqual(member1, member2);

            // Attachable member != regular member from same type
            member1 = new XamlMember("Bar", type1, true /*isAttachable*/);
            XamlMember nonAttachable = new XamlMember("Bar", type1, false /*isAttachable*/);
            Assert.AreNotEqual(member1, member2);
        }

        [TestMethod]
        public void DirectiveEquality()
        {
            // Known == unknown, for directives
            XamlDirective directive1 = new XamlDirective(new string[] { XamlLanguage.Xaml2006Namespace }, "Foo",
                XamlLanguage.String, XamlLanguage.String.TypeConverter, AllowedMemberLocations.Any);
            XamlDirective directive2 = new XamlDirective(XamlLanguage.Xaml2006Namespace, "Foo");
            Assert.AreEqual(directive1, directive2);

            // Known == known if namespaces match
            directive1 = new XamlDirective(new string[] { XamlLanguage.Xaml2006Namespace, XamlLanguage.Xml1998Namespace },
                "Foo", XamlLanguage.String, XamlLanguage.String.TypeConverter, AllowedMemberLocations.Any);
            directive2 = new XamlDirective(new string[] { XamlLanguage.Xaml2006Namespace, XamlLanguage.Xml1998Namespace },
                "Foo", XamlLanguage.Int32, XamlLanguage.Int32.TypeConverter, AllowedMemberLocations.Any);
            Assert.AreEqual(directive1, directive2);
            // Known != known if namespaces don't match
            directive2 = new XamlDirective(new string[] { XamlLanguage.Xml1998Namespace, XamlLanguage.Xaml2006Namespace},
                "Foo", XamlLanguage.Int32, XamlLanguage.Int32.TypeConverter, AllowedMemberLocations.Any);
            Assert.AreNotEqual(directive1, directive2);
        }

        [TestMethod]
        public void VisibilityTests()
        {
            const string elementsNs = "clr-namespace:Test.Elements;assembly=XamlTestClasses";

            // Public class, public property
            XamlType type = SchemaContext.GetXamlType(typeof(HoldsOneElement));
            Assert.IsTrue(type.IsPublic);
            XamlMember member = type.GetMember("Element");
            Assert.IsTrue(member.IsReadPublic);
            Assert.IsTrue(member.IsWritePublic);

            // Internal class
            type = SchemaContext.GetXamlType(elementsNs, "InternalElement");
            Assert.IsFalse(type.IsPublic);
            // Internal class, public property
            member = type.GetMember("PublicNameOfInternalType");
            Assert.IsFalse(member.IsReadPublic);
            Assert.IsFalse(member.IsWritePublic);
            // Internal class, internal property
            member = type.GetMember("InternalProperty");
            Assert.IsFalse(member.IsReadPublic);
            Assert.IsFalse(member.IsWritePublic);

            // Public class
            type = SchemaContext.GetXamlType(typeof(ElementWithInternalProperty));
            Assert.IsTrue(type.IsPublic);
            // Public class, internal properties
            member = type.GetMember("InternalProperty");
            Assert.IsFalse(member.IsReadPublic);
            Assert.IsFalse(member.IsWritePublic);
            member = type.GetMember("InternalReadProperty");
            Assert.IsFalse(member.IsReadPublic);
            Assert.IsTrue(member.IsWritePublic);
            member = type.GetMember("InternalWriteProperty");
            Assert.IsFalse(member.IsWritePublic);
            Assert.IsTrue(member.IsReadPublic);
            // Public class, protected and private properties
            member = type.GetMember("ProtectedProperty");
            Assert.IsFalse(member.IsReadPublic);
            Assert.IsFalse(member.IsWritePublic);
            member = type.GetMember("PrivateProperty");
            Assert.IsNull(member);

            // Nested classes aren't generally supported in XAML, but they can be passed to Schema,
            // so we'd like to make sure we support them correctly
            type = SchemaContext.GetXamlType(typeof(HasNested.NestedClass));
            Assert.IsTrue(type.IsPublic);
            type = SchemaContext.GetXamlType(elementsNs, "HasNested+InternalNestedClass");
            Assert.IsFalse(type.IsPublic);
            type = SchemaContext.GetXamlType(elementsNs, "HasNested+ProtectedNestedClass");
            Assert.IsFalse(type.IsPublic);
            type = SchemaContext.GetXamlType(elementsNs, "HasNested+PrivateNestedClass");
            Assert.IsNull(type);
        }

        [TestXaml, TestAlternateXamlLoader("LoadWithLoggingInvokers")]
        [TestTreeValidator("ValidateBasicCustomInvokerTest")]
        const string BasicCustomInvokerTest = @"
<DoubleCollection
    xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses'
    xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
  <DoubleCollection.Flavor>Raspberry</DoubleCollection.Flavor>
  <x:Double>42.5</x:Double>
  <x:Double>23.666</x:Double>
</DoubleCollection>";

        const string BasicCustomInvokerTest_ExpectedLog = @"
Create Test.Elements.DoubleCollection
Set Test.Elements.DoubleCollection.Flavor = Raspberry
Add(Test.Elements.DoubleCollection,42.5)
Add(Test.Elements.DoubleCollection,23.666)";

        public object LoadWithLoggingInvokers(string xaml)
        {
            XamlXmlReader reader = new XamlXmlReader(XmlReader.Create(new StringReader(xaml)));
            StringBuilder log = new StringBuilder();
            LoggingInvokerReader wrappingReader = new LoggingInvokerReader(reader, log);
            XamlServices.Load(wrappingReader);
            return log; // note, the point of this test is to validate the log, not the object tree
        }

        public void ValidateBasicCustomInvokerTest(object o)
        {
            Assert.AreEqual(BasicCustomInvokerTest_ExpectedLog.Trim(), o.ToString().Trim());
        }

        [TestMethod]
        public void XamlTypeExtensibility()
        {
            XamlType overriden = new DoubleCollectionXamlType(SchemaContext);

            // Basic override tests
            Assert.IsNull(overriden.KeyType);
            Assert.AreEqual(typeof(decimal), overriden.ItemType.UnderlyingType);
            Assert.AreEqual("Flavor", overriden.GetAliasedProperty(XamlLanguage.Name).Name);
            Assert.IsNull(overriden.GetMember("Capacity"));
            Assert.AreEqual(XamlLanguage.Double.TypeConverter.ConverterInstance.GetType(),
                overriden.TypeConverter.ConverterInstance.GetType());

            // Assignability override
            Assert.IsTrue(overriden.CanAssignTo(new XamlType(typeof(decimal), SchemaContext)));
            Assert.IsFalse(overriden.CanAssignTo(new XamlType(typeof(double), SchemaContext)));

            //Member lookup override tests
            var allMembers = overriden.GetAllMembers();
            Assert.IsFalse(allMembers.Any(m => m.Name == "Capacity"));
            // Duplicate that last test on a fresh instance that hasn't had member lookup done
            XamlType overriden2 = new DoubleCollectionXamlType(SchemaContext);
            Assert.IsFalse(overriden2.GetAllMembers().Any(m => m.Name == "Capacity"));
            // Make sure that there isn't a null hanging around in GetAllMembers from the negative member lookup
            Assert.IsFalse(allMembers.Contains(null));

            // Ensure idempotence in the face of bad lookup method
            Assert.IsNull(overriden.ContentProperty);
            Assert.IsNull(overriden.ContentProperty);

            // Make sure that namespaces are still looked up, even though we provided null in ctor
            XamlType original = SchemaContext.GetXamlType(typeof(DoubleCollection));
            Assert.AreEqualOrdered(overriden.GetXamlNamespaces(), original.GetXamlNamespaces().ToArray());

            // Equality should look at underlying type
            Assert.AreEqual(overriden, original);
        }

        [TestMethod]
        public void XamlTypeExtensibilityWithoutUnderlyingType()
        {
            XamlType baseType = SchemaContext.GetXamlType(typeof(DoubleCollection));
            XamlType derivedType = new DerivedType("MyDoubleCollection", baseType);
            Assert.AreEqual(baseType.IsCollection, derivedType.IsCollection);
            Assert.AreSame(baseType.ItemType, derivedType.ItemType);
            Assert.AreSame(baseType.TypeConverter, derivedType.TypeConverter);
            Assert.AreSame(baseType.GetMember("Flavor"), derivedType.GetMember("Flavor"));
            Assert.IsNull(derivedType.UnderlyingType);
            Assert.IsTrue(derivedType.CanAssignTo(baseType));
            Assert.AreEqualUnordered(derivedType.GetAllMembers(), baseType.GetAllMembers().ToArray());
            Assert.AreEqual(string.Empty, derivedType.PreferredXamlNamespace);
            Assert.AreNotEqual(derivedType, baseType);
        }

        [TestMethod]
        public void XamlMemberExtensibility()
        {
            XamlMember overriden = new ColorMember(SchemaContext);
            XamlMember original = SchemaContext.GetXamlType(typeof(ColorHolder)).GetMember("Color");

            // Attribute-based lookups should be the same
            Assert.AreSame(original.DeferringLoader, overriden.DeferringLoader);

            // Check basic flags
            Assert.IsFalse(overriden.IsAttachable);
            Assert.IsFalse(overriden.IsDirective);
            Assert.IsFalse(overriden.IsEvent);
            Assert.IsFalse(overriden.IsUnknown);

            // Check lazy member lookup and equality
            Assert.AreEqual(original.UnderlyingMember, overriden.UnderlyingMember);
            Assert.AreEqual(original.Invoker.UnderlyingGetter, overriden.Invoker.UnderlyingGetter);
            Assert.AreEqual(overriden, original);

            // Check overriden members
            Assert.IsFalse(overriden.IsReadPublic);
            Assert.IsNull(overriden.TypeConverter);

            // Make sure that invoker is using our overriden setter
            ColorHolder ch = new ColorHolder();
            overriden.Invoker.SetValue(ch, new ColorElement() { ColorName = "Red" });
            Assert.AreEqual("Redx", ch.Color.ColorName);
        }

        [TestMethod]
        public void XamlMemberExtensibilityWithoutUnderlyingMember()
        {
            XamlMember original = SchemaContext.GetXamlType(typeof(ColorHolder)).GetMember("Color");
            XamlMember overriden = new WrappingMember(original);

            // Check basic flags
            Assert.IsFalse(overriden.IsAttachable);
            Assert.IsFalse(overriden.IsDirective);
            Assert.IsFalse(overriden.IsEvent);
            Assert.IsFalse(overriden.IsUnknown);

            // Check that all the CLR info is null
            Assert.IsNull(overriden.UnderlyingMember);
            Assert.IsNull(overriden.Invoker.UnderlyingGetter);
            Assert.IsNull(overriden.Invoker.UnderlyingSetter);

            // But the members should still be equal based on declaring type and name
            Assert.AreEqual(overriden, original);

            // And TypeConverter lookup based on the property type should still work
            Assert.AreEqual(original.TypeConverter, overriden.TypeConverter);
        }

        [TestMethod]
        public void XamlAttachableEventExtensibility()
        {
            XamlMember original = SchemaContext.GetXamlType(typeof(AttachedEventHolder)).GetAttachableMember("TapEvent");
            XamlMember overriden = new WrappingAttachableEvent(original);

            // Check basic flags
            Assert.IsTrue(overriden.IsAttachable);
            Assert.IsFalse(overriden.IsDirective);
            Assert.IsTrue(overriden.IsEvent);
            Assert.IsFalse(overriden.IsUnknown);

            // Check that Readable/Writeable flags leverage the lazily provided CLR info
            Assert.IsTrue(overriden.IsWriteOnly);
            Assert.IsTrue(overriden.IsWritePublic);
            Assert.IsFalse(overriden.IsReadOnly);
            Assert.IsFalse(overriden.IsReadPublic);

            // Check that the invoker picks up the lazy CLR info
            EventElement eventElement = new EventElement();
            Delegate handler = Delegate.CreateDelegate(typeof(EventElement.TapDelegate), eventElement, "PrivateHandler");
            overriden.Invoker.SetValue(eventElement, handler);
            int fireCount = eventElement.TapEventCount;
            AttachedEventHolder.RaiseTapEvent(eventElement);
            Assert.AreEqual(fireCount + 1, eventElement.TapEventCount, "Tap event did not fire");

            // Members should be equal
            Assert.AreEqual(overriden, original);
        }

        [TestMethod]
        public void ValueSerializer()
        {
            // No ValueSerializer present
            XamlType xt = SchemaContext.GetXamlType(typeof(HoldsOneElement));
            Assert.IsNull(xt.ValueSerializer);
            XamlMember xm = xt.GetMember("Element");
            Assert.IsNull(xm.ValueSerializer);

            // Built-in ValueSerializer
            xt = XamlLanguage.String;
            Assert.AreEqual("StringValueSerializer", xt.ValueSerializer.ConverterType.Name);

            // ValueSerializer on type
            xt = SchemaContext.GetXamlType(typeof(Motorboat));
            Assert.AreEqual(typeof(MotorboatVS), xt.ValueSerializer.ConverterType);

            // ValueSerializer on base type propagates to derived
            XamlType derived = SchemaContext.GetXamlType(typeof(Cruiser));
            Assert.AreEqual(xt.ValueSerializer, derived.ValueSerializer);

            // ValueSerializer on property
            xt = SchemaContext.GetXamlType(typeof(VSContainer2));
            xm = xt.GetMember("Vehicle");
            Assert.AreEqual(typeof(VehicleVS), xm.ValueSerializer.ConverterType);

            // ValueSerializer on type propagates to property
            xt = SchemaContext.GetXamlType(typeof(ColorElement));
            xm = xt.GetMember("ColorName");
            Assert.AreEqual(XamlLanguage.String.ValueSerializer, xm.ValueSerializer);

            // ValueSerializer on property overrides one on type
            xt = SchemaContext.GetXamlType(typeof(VSOnStringPropertyContainer));
            xm = xt.GetMember("Prop");
            Assert.AreEqual(typeof(AlwaysThrowVS), xm.ValueSerializer.ConverterType);
        }

        [TestMethod]
        public void CompatWith()
        {
            const string ns2005 = "http://test.xaml/ns1/2005";
            const string ns2006 = "http://test.xaml/ns1/2006";
            const string ns2007 = "http://test.xaml/ns1/2007";
            const string ns2008 = "http://test.xaml/ns1/2008";

            string compatibleNs;

            Assert.IsTrue(SchemaContext.TryGetCompatibleXamlNamespace(ns2005, out compatibleNs));
            Assert.AreEqual(ns2008, compatibleNs);

            Assert.IsFalse(SchemaContext.TryGetCompatibleXamlNamespace(ns2006, out compatibleNs));
            Assert.IsNull(compatibleNs);

            Assert.IsTrue(SchemaContext.TryGetCompatibleXamlNamespace(ns2007, out compatibleNs));
            Assert.AreEqual(ns2008, compatibleNs);

            Assert.IsTrue(SchemaContext.TryGetCompatibleXamlNamespace(ns2008, out compatibleNs));
            Assert.AreEqual(ns2008, compatibleNs);
        }

        [TestMethod]
        public void SystemTypeSpoofing()
        {
            XamlSchemaContext xsc = new XamlSchemaContext();
            Type t = new EqualityLyingType();
            XamlType xt = xsc.GetXamlType(t);
            Assert.IsTrue(object.ReferenceEquals(t, xt.UnderlyingType));
            t = typeof(Element);
            xt = xsc.GetXamlType(t);
            Assert.IsTrue(object.ReferenceEquals(t, xt.UnderlyingType));
        }

        [TestMethod]
        public void UnknownMemberDefaults()
        {
            XamlMember xm = new XamlMember("Foo", SchemaContext.GetXamlType(typeof(Element)), false);
            Assert.AreEqual(SchemaContext.GetXamlType(typeof(Element)), xm.DeclaringType);
            Assert.IsNull(xm.DeferringLoader);
            Assert.IsFalse(xm.IsAttachable);
            Assert.IsFalse(xm.IsDirective);
            Assert.IsFalse(xm.IsEvent);
            Assert.IsFalse(xm.IsReadOnly);
            Assert.IsTrue(xm.IsReadPublic);
            Assert.IsTrue(xm.IsUnknown);
            Assert.IsFalse(xm.IsWriteOnly);
            Assert.IsTrue(xm.IsWritePublic);
            Assert.AreEqual(xm.DeclaringType, xm.TargetType);
            Assert.AreEqual(SchemaContext.GetXamlType(typeof(object)), xm.Type);
            Assert.IsNull(xm.TypeConverter);
            Assert.IsNull(xm.UnderlyingMember);
            Assert.IsNull(xm.ValueSerializer);
        }

        [TestMethod]
        public void FullyQualifyAssemblyNames()
        {
            // Verify short assembly name
            XamlType xt1 = SchemaContext.GetXamlType(typeof(Element));
            string shortNs = xt1.GetXamlNamespaces().Last();
            Assert.AreEqual("clr-namespace:Test.Elements;assembly=XamlTestClasses", shortNs);

            // Verify full assembly name
            XamlSchemaContext qualifiedXSC = new XamlSchemaContext(
                new XamlSchemaContextSettings { FullyQualifyAssemblyNamesInClrNamespaces = true });
            XamlType xt2 = qualifiedXSC.GetXamlType(typeof(Element));
            string longNs = xt2.GetXamlNamespaces().Last();
            Assert.AreEqual("clr-namespace:Test.Elements;assembly=" + typeof(Element).Assembly.FullName, longNs);

            // Make sure that either context can do lookups in either form
            XamlType xt3 = SchemaContext.GetXamlType(new XamlTypeName(longNs, "Element"));
            Assert.IsTrue(object.ReferenceEquals(xt1, xt3));
            xt3 = qualifiedXSC.GetXamlType(new XamlTypeName(shortNs, "Element"));
            Assert.IsTrue(object.ReferenceEquals(xt2, xt3));

            // Make sure that built-in types respect the setting
            shortNs = XamlLanguage.Array.GetXamlNamespaces().Last();
            longNs = qualifiedXSC.GetXamlType(new XamlTypeName(XamlLanguage.Xaml2006Namespace, "Array")).GetXamlNamespaces().Last();
            Assert.AreNotEqual(shortNs, longNs);
            Assert.IsTrue(longNs.StartsWith(shortNs));
        }

        [TestMethod]
        public void UnparsedDynamicAssemblyCollectible()
        {
            XamlSchemaContext schemaContext = new XamlSchemaContext();
            AssemblyBuilder.DefineDynamicAssembly(
                new AssemblyName("Foo, Version=1.0"), AssemblyBuilderAccess.RunAndCollect);
            Assert.IsNotNull(AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(
                a => a.GetName().Name == "Foo"));

            DRT.ResumeAt(VerifyUnparsedDynamicAssemblyCollectible);
        }

        void VerifyUnparsedDynamicAssemblyCollectible()
        {
            // With a single call to WaitForPendingFinalizers, this test failed intermittently.
            // The most likely reason is a linked chain of finalizable objects. Doing a few rounds
            // of GC in an attempt to make the test more reliable.
            bool assemblyIsAlive;
            int collectCount = 0;
            GC.Collect();
            do
            {
                GC.WaitForPendingFinalizers();
                GC.Collect();
                collectCount++;
                assemblyIsAlive = (AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(
                    a => a.GetName().Name == "Foo") != null);
            }
            while (assemblyIsAlive && collectCount < 10);
            Assert.IsFalse(assemblyIsAlive);
        }

        [TestMethod]
        public void ParsedDynamicAssemblyCollectible()
        {
            ParsedDynamicAssemblyCollectibleCore();
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            Assert.IsNull(AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(
                a => a.GetName().Name == "Foo2"));
        }

         [TestMethod]
#if NETCOREAPP3_X
         [TestKnownFailure(Reason = ".NET Core does not support Assembly.ReflectionOnlyLoad. Contact wpfdev for additional details")]
#endif
        public void ReflectionOnlyTypes()
        {
            XamlSchemaContext oldXsc = _schemaContext;
            Assembly rolAssembly = Assembly.ReflectionOnlyLoad(typeof(Element).Assembly.FullName);
            _schemaContext = new XamlSchemaContext(new Assembly[] { rolAssembly });
            ResolveEventHandler resolveHandler =
                (object sender, ResolveEventArgs e) => Assembly.ReflectionOnlyLoad(e.Name);
            AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += resolveHandler;
            try
            {
                XamlType rolType;

                // Test assignability to built-in type
                rolType = GetRolType(typeof(NameScopeElement));
                Assert.IsTrue(rolType.IsNameScope);

                // Test collection evaluation
                rolType = GetRolType(typeof(ElementCollection));
                Assert.IsTrue(rolType.IsCollection);
                Assert.AreEqual(GetRolType(typeof(Element)), rolType.ItemType);

                // Test dictionary evaluation
                rolType = GetRolType(typeof(IDictionaryExplicitImpl));
                Assert.IsTrue(rolType.IsDictionary);
                Assert.AreEqual(XamlLanguage.String, rolType.KeyType);
                Assert.AreEqual(XamlLanguage.String, rolType.ItemType);

                // Test attribute lookup on types
                rolType = GetRolType(typeof(HoldsOneElement));
                Assert.AreEqual(rolType.GetMember("Element"), rolType.ContentProperty);

                // Test attribute lookup on properties (also, attribute type not defined in System.Xaml)
                rolType = GetRolType(typeof(BaseWithVirtualProperties));
                XamlMember rolMember = rolType.GetMember("HasVisibility");
                Assert.AreEqual(DesignerSerializationVisibility.Hidden, rolMember.SerializationVisibility);

                // Test attribute lookup on assembly
                rolType = SchemaContext.GetXamlType("http://test.xaml/ns1/2005", "Foo");
                Assert.AreEqual(GetRolType(typeof(XmlNsClasses.Ns1.Foo)), rolType);
            }
            finally
            {
                _schemaContext = oldXsc;
                AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve -= resolveHandler;
            }

        }

        private XamlType GetRolType(Type type)
        {
            Assembly asm = AppDomain.CurrentDomain.ReflectionOnlyGetAssemblies().First(a => a.FullName == type.Assembly.FullName);
            return SchemaContext.GetXamlType(asm.GetType(type.FullName));
        }

        // Do the setup in a separate method so the variable has a change to go out of scope
        private void ParsedDynamicAssemblyCollectibleCore()
        {
            XamlSchemaContext schemaContext = new XamlSchemaContext();
            var ab = AssemblyBuilder.DefineDynamicAssembly(
                new AssemblyName("Foo2, Version=1.0"), AssemblyBuilderAccess.RunAndCollect);
            Assert.IsNotNull(AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(
                a => a.GetName().Name == "Foo2"));

            var cab = new CustomAttributeBuilder(
                typeof(XmlnsDefinitionAttribute).GetConstructor(new Type[] { typeof(string), typeof(string) }),
                new object[] { "http://Foo", "Foo" });
            ab.SetCustomAttribute(cab);
            Assert.IsNull(schemaContext.GetXamlType(new XamlTypeName("http://Foo", "Bar")));
        }

        private static XamlType GetXamlType(XamlSchemaContext schemaContext, string typeName)
        {
            XamlTypeName xamlTypeName = XamlTypeName.Parse(typeName, new SimpleNamespaceResolver());
            return GetXamlType(schemaContext, xamlTypeName);
        }

        private static XamlType GetXamlType(XamlSchemaContext schemaContext, XamlTypeName typeName)
        {
            List<XamlType> typeArgs = null;
            if (typeName.TypeArguments.Count > 0)
            {
                typeArgs = new List<XamlType>();
                foreach (XamlTypeName typeArgName in typeName.TypeArguments)
                {
                    typeArgs.Add(GetXamlType(schemaContext, typeArgName));
                }
            }
            return new XamlType(typeName.Namespace, typeName.Name, typeArgs, schemaContext);
        }

        private void RoundtripXamlType(Type type)
        {
            RoundtripXamlType(SchemaContext.GetXamlType(type));
        }

        private void RoundtripXamlType(XamlType xamlType)
        {
            PropertyDefinition property = new PropertyDefinition() { Type = xamlType };
            PropertyDefinition roundTripped = (PropertyDefinition)XamlServices.Parse(XamlServices.Save(property));
            XamlType roundTrippedType = roundTripped.Type;
            Assert.IsTrue(xamlType.CanAssignTo(roundTrippedType));
            Assert.IsTrue(roundTrippedType.CanAssignTo(xamlType));
        }

        void AssertException<E>(Action action) where E : Exception
        {
            try
            {
                action.Invoke();
                Assert.Fail("Expected " + typeof(E).Name);
            }
            catch (E)
            {
                //swallow expected
            }
        }

        void NegativeXamlTypeNameParse(string input)
        {
            XamlTypeName result;
            IXamlNamespaceResolver resolver = new SimpleNamespaceResolver();
            AssertException<FormatException>(() => XamlTypeName.Parse(input, resolver));
            Assert.IsFalse(XamlTypeName.TryParse(input, resolver, out result));
            Assert.IsNull(result);
        }

        void NegativeXamlTypeNameListParse(string input)
        {
            IList<XamlTypeName> result;
            IXamlNamespaceResolver resolver = new SimpleNamespaceResolver();
            AssertException<FormatException>(() => XamlTypeName.ParseList(input, resolver));
            Assert.IsFalse(XamlTypeName.TryParseList(input, resolver, out result));
            Assert.IsNull(result);
        }

        IEnumerable<T[]> CombinationsOf<T>(IEnumerable<T> values)
        {
            List<T> inputValues = new List<T>(values);
            for (int i = 1; i < (1 << inputValues.Count); i++)
            {
                List<T> result = new List<T>();
                for (int j = 0; j < inputValues.Count; j++)
                {
                    if (((i >> j) % 2) == 1)
                    {
                        result.Add(inputValues[j]);
                    }
                }
                yield return result.ToArray();
            }
        }

        string GetArrayStrings(object[] array)
        {
            string[] result = new string[array.Length];
            for (int i = 0; i < array.Length; i++)
            {
                result[i] = array[i].ToString();
            }
            return string.Join(", ", result);
        }

        void RunActions<T>(T value, IEnumerable<Action<T>> actions)
        {
            foreach (Action<T> action in actions)
            {
                action.Invoke(value);
            }
        }

        string NormalizeCommas(string input)
        {
            return Regex.Replace(input, @",\s*", ", ");
        }

        class SimpleNamespaceResolver : IXamlNamespaceResolver
        {
            public string GetNamespace(string prefix)
            {
                if (prefix == null)
                {
                    throw new ArgumentNullException();
                }
                if (prefix == string.Empty)
                {
                    return "default";
                }
                if (prefix == "z")
                {
                    return null;
                }
                return prefix + "_namespace";
            }

            public IEnumerable<NamespaceDeclaration> GetNamespacePrefixes()
            {
                return null;
            }
        }

        class SimplePrefixLookup : INamespacePrefixLookup
        {
            public string LookupPrefix(string ns)
            {
                if (ns == null)
                {
                    throw new ArgumentNullException();
                }
                if (ns == "default")
                {
                    return string.Empty;
                }
                if (ns.EndsWith("_namespace"))
                {
                    return ns.Substring(0, ns.Length - "_namespace".Length);
                }
                return null;
            }
        }

        class LoggingTypeInvoker : XamlTypeInvoker
        {
            public XamlType Type { get; private set; }

            public LoggingTypeInvoker(XamlType type, StringBuilder log)
                :base(type)
            {
                Type = type;
                Log = log;
            }

            public StringBuilder Log { get; set; }

            public override void AddToCollection(object instance, object item)
            {
                Log.AppendFormat(XamlDrt.InvariantEnglishUS, "Add({0},{1})\r\n", instance, item);
                base.AddToCollection(instance, item);
            }

            public override void AddToDictionary(object instance, object key, object item)
            {
                Log.AppendFormat(XamlDrt.InvariantEnglishUS, "Add({0},{1},{2})\r\n", instance, key, item);
                base.AddToDictionary(instance, key, item);
            }

            public override object CreateInstance(object[] arguments)
            {
                Log.AppendFormat(XamlDrt.InvariantEnglishUS, "Create {0}\r\n", Type);
                return base.CreateInstance(arguments);
            }
        }

        class LoggingMemberInvoker : XamlMemberInvoker
        {
            public XamlMember Member { get; private set; }

            public LoggingMemberInvoker(XamlMember member, StringBuilder log)
                : base(member)
            {
                Member = member;
                Log = log;
            }

            public StringBuilder Log { get; set; }

            public override object GetValue(object instance)
            {
                Log.AppendFormat(XamlDrt.InvariantEnglishUS, "Get {0}\r\n", Member);
                return base.GetValue(instance);
            }

            public override void SetValue(object instance, object value)
            {
                Log.AppendFormat(XamlDrt.InvariantEnglishUS, "Set {0} = {1}\r\n", Member, value);
                base.SetValue(instance, value);
            }
        }

        // For now, we plug in the logging invokers via a wrapping reader, because member lookup
        // isn't overridable in schema yet
        class LoggingInvokerReader : System.Xaml.XamlReader
        {
            System.Xaml.XamlReader _underlyingReader;
            XamlMember _member;
            XamlType _type;
            StringBuilder _log;

            public LoggingInvokerReader(System.Xaml.XamlReader underlyingReader, StringBuilder log)
            {
                _underlyingReader = underlyingReader;
                _log = log;
            }

            public override bool IsEof
            {
                get { return _underlyingReader.IsEof; }
            }

            public override XamlMember Member
            {
                get { return _member; }
            }

            public override NamespaceDeclaration Namespace
            {
                get { return _underlyingReader.Namespace; }
            }

            public override XamlNodeType NodeType
            {
                get { return _underlyingReader.NodeType; }
            }

            public override bool Read()
            {
                _member = null;
                _type = null;
                if (!_underlyingReader.Read())
                {
                    return false;
                }
                if (_underlyingReader.Type != null)
                {
                    _type = new XamlType(
                        _underlyingReader.Type.UnderlyingType,
                        _underlyingReader.SchemaContext,
                        new LoggingTypeInvoker(_underlyingReader.Type, _log));
                }
                else if (_underlyingReader.Member != null)
                {
                    _member = _underlyingReader.Member;
                    if (!_member.IsDirective)
                    {
                        LoggingMemberInvoker invoker = new LoggingMemberInvoker(_member, _log);
                        if (_member.IsAttachable)
                        {
                            if (_member.IsEvent)
                            {
                                _member = new XamlMember(_member.Name, (MethodInfo)_member.UnderlyingMember,
                                    _underlyingReader.SchemaContext, invoker);
                            }
                            else
                            {
                                _member = new XamlMember(_member.Name, _member.Invoker.UnderlyingGetter,
                                    _member.Invoker.UnderlyingSetter, _underlyingReader.SchemaContext, invoker);
                            }
                        }
                        else
                        {
                            if (_member.IsEvent)
                            {
                                _member = new XamlMember((EventInfo)_member.UnderlyingMember,
                                    _underlyingReader.SchemaContext, invoker);
                            }
                            else
                            {
                                _member = new XamlMember((PropertyInfo)_member.UnderlyingMember,
                                    _underlyingReader.SchemaContext, invoker);
                            }
                        }
                    }
                }
                return true;
            }

            public override XamlSchemaContext SchemaContext
            {
                get { return _underlyingReader.SchemaContext; }
            }

            public override XamlType Type
            {
                get { return _type; }
            }

            public override object Value
            {
                get { return _underlyingReader.Value; }
            }
        }

        class DoubleCollectionXamlType : XamlType
        {
            bool contentPropertyLookedUp;

            public DoubleCollectionXamlType(XamlSchemaContext xsc)
                : base("DoubleCollection", null, xsc)
            {
            }

            protected override XamlType LookupKeyType()
            {
                // This should never get called, we're not a dictionary;
                return SchemaContext.GetXamlType(typeof(int));
            }

            protected override XamlMember LookupAliasedProperty(XamlDirective directive)
            {
                if (directive == XamlLanguage.Name)
                {
                    return GetMember("Flavor");
                }
                return null;
            }

            protected override XamlType LookupItemType()
            {
                return SchemaContext.GetXamlType(typeof(decimal));
            }

            protected override IEnumerable<XamlMember> LookupAllMembers()
            {
                var result = base.LookupAllMembers();
                return result.Where(m => m.Name != "Capacity");
            }

            protected override XamlMember LookupMember(string name, bool skipReadOnlyCheck)
            {
                if (name == "Capacity")
                {
                    return null;
                }
                return base.LookupMember(name, skipReadOnlyCheck);
            }

            protected override XamlMember LookupContentProperty()
            {
                if (!contentPropertyLookedUp)
                {
                    contentPropertyLookedUp = true;
                    return null;
                }
                // We should only ever get called once (on a given thread)
                return GetMember("Flavor");
            }

            protected override XamlValueConverter<TypeConverter> LookupTypeConverter()
            {
                return XamlLanguage.Double.TypeConverter;
            }

            protected override Type LookupUnderlyingType()
            {
                return typeof(DoubleCollection);
            }

            public override bool CanAssignTo(XamlType xamlType)
            {
                if (xamlType.UnderlyingType == typeof(Decimal))
                {
                    return true;
                }
                return base.CanAssignTo(xamlType);
            }
        }

        class DerivedType : XamlType
        {
            XamlType _baseType;

            public DerivedType(string name, XamlType baseType)
                : base(name, null, baseType.SchemaContext)
            {
                _baseType = baseType;
            }

            protected override XamlType LookupBaseType()
            {
                return _baseType;
            }

            protected override bool LookupIsUnknown()
            {
                return false;
            }
        }

        class ColorMember : XamlMember
        {
            public ColorMember(XamlSchemaContext xsc)
                : base("Color", xsc.GetXamlType(typeof(ColorHolder)), false)
            {
            }

            public static void SetValue(ColorHolder target, ColorElement value)
            {
                target.Color = new ColorElement() { ColorName = value.ColorName + "x" };
            }

            protected override bool LookupIsReadPublic()
            {
                return false;
            }

            protected override XamlValueConverter<TypeConverter> LookupTypeConverter()
            {
                return null;
            }

            protected override MemberInfo LookupUnderlyingMember()
            {
                return DeclaringType.UnderlyingType.GetProperty(Name);
            }

            protected override MethodInfo LookupUnderlyingSetter()
            {
                return typeof(ColorHolder).GetMethod("SetColor");
            }
        }

        class WrappingMember : XamlMember
        {
            XamlMember _wrappedMember;

            public WrappingMember(XamlMember wrappedMember)
                :base(wrappedMember.Name, wrappedMember.DeclaringType, wrappedMember.IsAttachable)
            {
                _wrappedMember = wrappedMember;
            }

            protected override bool LookupIsUnknown()
            {
                return false;
            }

            protected override XamlType LookupType()
            {
                return _wrappedMember.Type;
            }
        }

        class WrappingAttachableEvent : XamlMember
        {
            XamlMember _wrappedMember;

            public WrappingAttachableEvent(XamlMember wrappedMember)
                :base(wrappedMember.Name, wrappedMember.DeclaringType, true)
            {
                _wrappedMember = wrappedMember;
            }

            protected override bool LookupIsEvent()
            {
                return true;
            }

            protected override MemberInfo LookupUnderlyingMember()
            {
                return _wrappedMember.UnderlyingMember;
            }

            protected override MethodInfo LookupUnderlyingSetter()
            {
                return (MethodInfo)UnderlyingMember;
            }
        }

        class EqualityLyingType : Type
        {
            public override string Name
            {
                get { return "EqualityLyingType"; }
            }

            public override bool Equals(Type o)
            {
                return (o == typeof(Element));
            }

            public override int GetHashCode()
            {
                return typeof(Element).GetHashCode();
            }

            public override Assembly Assembly
            {
                get { throw new NotImplementedException(); }
            }

            public override string AssemblyQualifiedName
            {
                get { throw new NotImplementedException(); }
            }

            public override Type BaseType
            {
                get { throw new NotImplementedException(); }
            }

            public override string FullName
            {
                get { throw new NotImplementedException(); }
            }

            public override Guid GUID
            {
                get { throw new NotImplementedException(); }
            }

            protected override TypeAttributes GetAttributeFlagsImpl()
            {
                throw new NotImplementedException();
            }

            protected override ConstructorInfo GetConstructorImpl(BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers)
            {
                throw new NotImplementedException();
            }

            public override ConstructorInfo[] GetConstructors(BindingFlags bindingAttr)
            {
                throw new NotImplementedException();
            }

            public override Type GetElementType()
            {
                throw new NotImplementedException();
            }

            public override EventInfo GetEvent(string name, BindingFlags bindingAttr)
            {
                throw new NotImplementedException();
            }

            public override EventInfo[] GetEvents(BindingFlags bindingAttr)
            {
                throw new NotImplementedException();
            }

            public override FieldInfo GetField(string name, BindingFlags bindingAttr)
            {
                throw new NotImplementedException();
            }

            public override FieldInfo[] GetFields(BindingFlags bindingAttr)
            {
                throw new NotImplementedException();
            }

            public override Type GetInterface(string name, bool ignoreCase)
            {
                throw new NotImplementedException();
            }

            public override Type[] GetInterfaces()
            {
                throw new NotImplementedException();
            }

            public override MemberInfo[] GetMembers(BindingFlags bindingAttr)
            {
                throw new NotImplementedException();
            }

            protected override MethodInfo GetMethodImpl(string name, BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers)
            {
                throw new NotImplementedException();
            }

            public override MethodInfo[] GetMethods(BindingFlags bindingAttr)
            {
                throw new NotImplementedException();
            }

            public override Type GetNestedType(string name, BindingFlags bindingAttr)
            {
                throw new NotImplementedException();
            }

            public override Type[] GetNestedTypes(BindingFlags bindingAttr)
            {
                throw new NotImplementedException();
            }

            public override PropertyInfo[] GetProperties(BindingFlags bindingAttr)
            {
                throw new NotImplementedException();
            }

            protected override PropertyInfo GetPropertyImpl(string name, BindingFlags bindingAttr, Binder binder, Type returnType, Type[] types, ParameterModifier[] modifiers)
            {
                throw new NotImplementedException();
            }

            protected override bool HasElementTypeImpl()
            {
                throw new NotImplementedException();
            }

            public override object InvokeMember(string name, BindingFlags invokeAttr, Binder binder, object target, object[] args, ParameterModifier[] modifiers, System.Globalization.CultureInfo culture, string[] namedParameters)
            {
                throw new NotImplementedException();
            }

            protected override bool IsArrayImpl()
            {
                return false;
            }

            protected override bool IsByRefImpl()
            {
                throw new NotImplementedException();
            }

            protected override bool IsCOMObjectImpl()
            {
                throw new NotImplementedException();
            }

            protected override bool IsPointerImpl()
            {
                throw new NotImplementedException();
            }

            protected override bool IsPrimitiveImpl()
            {
                throw new NotImplementedException();
            }

            public override Module Module
            {
                get { throw new NotImplementedException(); }
            }

            public override string Namespace
            {
                get { throw new NotImplementedException(); }
            }

            public override Type UnderlyingSystemType
            {
                get { throw new NotImplementedException(); }
            }

            public override object[] GetCustomAttributes(Type attributeType, bool inherit)
            {
                throw new NotImplementedException();
            }

            public override object[] GetCustomAttributes(bool inherit)
            {
                throw new NotImplementedException();
            }

            public override bool IsDefined(Type attributeType, bool inherit)
            {
                throw new NotImplementedException();
            }
        }
    }
}
