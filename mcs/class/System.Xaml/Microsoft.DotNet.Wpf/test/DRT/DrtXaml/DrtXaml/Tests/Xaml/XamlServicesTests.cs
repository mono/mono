namespace DrtXaml.Tests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.Text;
    using System.Xaml;
    using System.Xml;
    using System.Xml.Linq;
    using DrtXaml.XamlTestFramework;
    using Test.Elements;
    using DRT;

    [TestClass]
    class XamlServicesTests : XamlTestSuite
    {
        public XamlServicesTests()
            : base("XamlServicesTests")
        {
        }

        public override DrtTest[] PrepareTests()
        {
            DrtTest[] tests = DrtTestFinder.FindTests(this);
            return tests;
        }

        const string Simple_XAML = @"<HoldsOneElement xmlns=""clr-namespace:Test.Elements;assembly=XamlTestClasses"">
    <HoldsOneElement.Element>
        <Element />
    </HoldsOneElement.Element>
</HoldsOneElement>
";

        [TestMethod]
        public void Parse()
        {
            Assert.IsTrue(XamlServices.Parse(Simple_XAML) is HoldsOneElement, 
                "Parse should return a HoldsOneElement");
        }

        [TestMethod]
        public void LoadFile()
        {
        }

        [TestMethod]
        public void LoadStream()
        {
            UTF8Encoding uniEncoding = new UTF8Encoding();

            Assert.IsTrue(XamlServices.Load(new MemoryStream(uniEncoding.GetBytes(Simple_XAML))) is HoldsOneElement, 
                "Load should return a HoldsOneElement");
        }

        [TestMethod]
        public void LoadTextReader()
        {
            Assert.IsTrue(XamlServices.Load(new StringReader(Simple_XAML)) is HoldsOneElement, 
                "Load should return a HoldsOneElement");
        }

        [TestMethod]
        public void LoadXmlReader()
        {
            Assert.IsTrue(XamlServices.Load(XmlReader.Create(new StringReader(Simple_XAML))) is HoldsOneElement, 
                "Load should return a HoldsOneElement");
        }

        [TestMethod]
        public void Transform()
        {
            Assert.IsTrue(XamlServices.Load(new XamlXmlReader(XmlReader.Create(new StringReader(Simple_XAML)))) is HoldsOneElement, 
                "Load should return a HoldsOneElement");
        }

        [TestMethod]
        public void TransformWithWriter()
        {
            XamlObjectWriter xamlWriter = new XamlObjectWriter(new XamlSchemaContext());
            XamlXmlReader xamlReader = new XamlXmlReader(XmlReader.Create(new StringReader(Simple_XAML)), xamlWriter.SchemaContext);
            XamlServices.Transform(xamlReader, xamlWriter);
            Assert.IsTrue(xamlWriter.Result is HoldsOneElement,
                "Load should return a HoldsOneElement");
        }

        [TestMethod]
        public void TransformWithWriterCloseWriterTrue()
        {
            XamlObjectWriter xamlWriter = new XamlObjectWriter(new XamlSchemaContext());
            XamlXmlReader xamlReader = new XamlXmlReader(XmlReader.Create(new StringReader(Simple_XAML)), xamlWriter.SchemaContext);
            XamlServices.Transform(xamlReader, xamlWriter, true);
            Assert.IsTrue(xamlWriter.Result is HoldsOneElement,
                "Load should return a HoldsOneElement");
        }

        [TestMethod]
        public void TransformWithWriterCloseWriterFalse()
        {
            XamlObjectWriter xamlWriter = new XamlObjectWriter(new XamlSchemaContext());
            XamlXmlReader xamlReader = new XamlXmlReader(XmlReader.Create(new StringReader(Simple_XAML)), xamlWriter.SchemaContext);
            XamlServices.Transform(xamlReader, xamlWriter, false);
            Assert.IsTrue(xamlWriter.Result is HoldsOneElement,
                "Load should return a HoldsOneElement");
        }
        //
        // Null parameter tests
        // 
        [TestMethod]
        [TestExpectedException(typeof(ArgumentNullException))]
        public void LoadStreamFailNull()
        {
            Stream s = null;
            XamlServices.Load(s);
        }

        [TestMethod]
        [TestExpectedException(typeof(ArgumentNullException))]
        public void LoadTextReaderFailNull()
        {
            TextReader tr = null;
            XamlServices.Load(tr);
        }

        [TestMethod]
        [TestExpectedException(typeof(ArgumentNullException))]
        public void LoadXamlReaderFailNull()
        {
            XamlReader xr = null;
            XamlServices.Load(xr);
        }
        [TestMethod]
        [TestExpectedException(typeof(ArgumentNullException))]
        public void LoadXamlReaderFailNullWriter()
        {
            XamlXmlReader xr = new XamlXmlReader(XmlReader.Create(new StringReader(@"<Foo xmlns=""http://foo""/>")));
            XamlServices.Transform(xr, null);
        }

        [TestMethod]
        [TestExpectedException(typeof(ArgumentNullException))]
        public void LoadXmlReaderFailNull()
        {
            XmlReader xr = null;
            XamlServices.Load(xr);
        }        

        [TestMethod]
        [TestExpectedException(typeof(ArgumentNullException))]
        public void ParseFailNull()
        {
            String s = null;
            XamlServices.Parse(s);
        }

        //[TestMethod]
        //public void SaveStream()
        //{
        //    Stream s = new MemoryStream();
        //    Foo f = new Foo();
        //    XamlServices.Save(s, f);
        //}
        //[TestMethod]
        //[TestExpectedException(typeof(ArgumentNullException))]
        //public void SaveStreamFailNull()
        //{
        //    Stream s = null;
        //    Foo f = new Foo();
        //    XamlServices.Save(s, f);
        //}
        //[TestMethod]
        //public void SaveTextWriter()
        //{
        //    StringBuilder sb = new StringBuilder();
        //    TextWriter tw = new StringWriter(sb);
        //    Foo f = new Foo();
        //    XamlServices.Save(tw, f);
        //}
        //[TestMethod]
        //[TestExpectedException(typeof(ArgumentNullException))]
        //public void SaveTextWriterFailNull()
        //{
        //    TextWriter tw = null;
        //    Foo f = new Foo();
        //    XamlServices.Save(tw, f);
        //}
        //[TestMethod]
        //[TestExpectedException(typeof(ArgumentNullException))]
        //public void SaveXamlWriterFailNull()
        //{
        //    XamlXmlWriter xw = null;
        //    Foo f = new Foo();
        //    XamlServices.Save(xw, f);
        //}

        //[TestMethod]
        //[TestExpectedException(typeof(ArgumentNullException))]
        //public void SaveXmlWriterFailNull()
        //{
        //    XmlWriter xw = null;
        //    Foo f = new Foo();
        //    XamlServices.Save(xw, f);
        //}

        void RoundTrip(string xaml)
        {
            object obj = XamlServices.Parse(xaml);
            var sb = new StringBuilder();
            XamlServices.Save(new StringWriter(sb), obj);

            Assert.AreEqual(xaml, sb.ToString());
        }
    }
}
