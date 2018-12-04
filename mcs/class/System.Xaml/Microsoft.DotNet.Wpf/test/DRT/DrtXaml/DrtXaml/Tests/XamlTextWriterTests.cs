namespace DrtXaml.Tests
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using System.Xaml;
    using System.Xml;
    using System.Xml.Linq;
    using System.Collections.Generic;
    using System.Windows.Documents;
    using DrtXaml.XamlTestFramework;
    using Test.Elements;
    using Test.Elements.AnotherNamespace;
    using Test.Elements.OneMoreNamespace;
    using DRT;

    [TestClass]
    class XamlXmlWriterTests : XamlTestSuite
    {
        public XamlXmlWriterTests()
            : base("XamlXmlWriterTests")
        {
        }

        public override DrtTest[] PrepareTests()
        {
            DrtTest[] tests = DrtTestFinder.FindTests(this);
            return tests;
        }

        [TestMethod]
        [TestExpectedException(typeof(XamlXmlWriterException))]
        public void WritingWhitespaceInPE()
        {
            XamlSchemaContext xsc = new XamlSchemaContext();

            var generated = new StringBuilder();
            var xmlSettings = new XmlWriterSettings { OmitXmlDeclaration = true };
            var xamlSettings = new XamlXmlWriterSettings { CloseOutput = true };
            using (XamlXmlWriter writer = new XamlXmlWriter(XmlWriter.Create(new StringWriter(generated), xmlSettings), xsc, xamlSettings))
            {
                XamlType bc = xsc.GetXamlType(typeof(BigContainer));
                writer.WriteStartObject(bc);
                writer.WriteStartMember(bc.GetMember("Content"));
                writer.WriteValue("Hello World");
                writer.WriteEndMember();
                writer.WriteStartMember(bc.GetMember("Chars"));
                writer.WriteValue("   ");
                writer.WriteEndMember();
                writer.WriteEndObject();
            }

            var target = @"<BigContainer xmlns=""clr-namespace:Test.Elements;assembly=XamlTestClasses"">Hello World</BigContainer>";
            Assert.AreEqual(target, generated.ToString());
        }

        [TestMethod]
        public void XmlLangSimple()
        {
            StringWriter stringWriter = new StringWriter();
            XamlWriter xamlWriter = new XamlXmlWriter(stringWriter, new XamlSchemaContext());

            xamlWriter.WriteStartObject(XamlLanguage.String);
            xamlWriter.WriteStartMember(XamlLanguage.Lang);
            xamlWriter.WriteValue("en-US");
            xamlWriter.WriteEndMember();
            xamlWriter.WriteStartMember(XamlLanguage.Initialization);
            xamlWriter.WriteValue("Text");
            xamlWriter.WriteEndMember();
            xamlWriter.WriteEndObject();
            xamlWriter.Close();

            string generated = stringWriter.GetStringBuilder().ToString();
            string expected = @"<?xml version=""1.0"" encoding=""utf-16""?><String xml:lang=""en-US"" xmlns=""http://schemas.microsoft.com/winfx/2006/xaml"">Text</String>";
            Assert.AreEqual(generated, expected);
        }

        [TestMethod]
        public void XmlAsPrefixInClrNamespace()
        {
            XamlSchemaContext xsc = new XamlSchemaContext();
            var generated = new StringBuilder();
            var xmlSettings = new XmlWriterSettings { Indent = true, OmitXmlDeclaration = true };

            using (XamlXmlWriter writer = new XamlXmlWriter(XmlWriter.Create(new StringWriter(generated), xmlSettings), xsc))
            {
                writer.WriteNamespace(new NamespaceDeclaration("http://hello", String.Empty));
                XamlType a = xsc.GetXamlType(typeof(X.M.L.A));
                writer.WriteStartObject(a);
                writer.WriteStartMember(a.GetMember("B"));
                writer.WriteStartObject(XamlLanguage.String);
                writer.WriteStartMember(XamlLanguage.Lang);
                writer.WriteValue("en-US");
                writer.WriteEndMember();
                writer.WriteStartMember(XamlLanguage.Initialization);
                writer.WriteValue("Text");
                writer.WriteEndMember();
                writer.WriteEndObject();
                writer.WriteEndMember();
                writer.WriteEndObject();
                writer.Close();
            }

            var expected = 
@"<p:A xmlns=""http://hello"" xmlns:p=""clr-namespace:X.M.L;assembly=XamlTestClasses"">
  <p:A.B>
    <x:String xml:lang=""en-US"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">Text</x:String>
  </p:A.B>
</p:A>";
            Assert.AreEqual(expected, generated.ToString());
        }

        [TestMethod, TestExpectedException(typeof(ArgumentException))]
        public void XmlAsPrefixInWriteNamespace()
        {
            StringWriter stringWriter = new StringWriter();
            XamlWriter xamlWriter = new XamlXmlWriter(stringWriter, new XamlSchemaContext());
            xamlWriter.WriteNamespace(new NamespaceDeclaration("http://whatever", "xml"));
        }

        [TestMethod, TestExpectedException(typeof(ObjectDisposedException))]
        public void WriteAfterDispose()
        {
            XamlSchemaContext xsc = new XamlSchemaContext();
            XamlXmlWriter writer = new XamlXmlWriter(XmlWriter.Create(new StringWriter(new StringBuilder())), xsc);

            XamlType bc = xsc.GetXamlType(typeof(BigContainer));
            writer.WriteStartObject(bc);
            ((IDisposable)writer).Dispose();
            writer.WriteStartMember(bc.GetMember("Integer"));
        }
        
        [TestMethod]
        public void SimpleWrite()
        {
            XamlSchemaContext xsc = new XamlSchemaContext();
            var generated = new StringBuilder();
            var xmlSettings = new XmlWriterSettings { Indent = true, OmitXmlDeclaration = true };

            using (XamlXmlWriter writer = new XamlXmlWriter(XmlWriter.Create(new StringWriter(generated), xmlSettings), xsc))
            {
                XamlType bc = xsc.GetXamlType(typeof(BigContainer));
                writer.WriteStartObject(bc);
                writer.WriteStartMember(bc.GetMember("Integer"));
                writer.WriteValue("123");
                writer.WriteEndMember();
                writer.WriteEndObject();
            }

            var expected = @"<BigContainer Integer=""123"" xmlns=""clr-namespace:Test.Elements;assembly=XamlTestClasses"" />";
            Assert.AreEqual(expected, generated.ToString());
        }

        [TestMethod]
        public void AssignNamespacePrefixBeforeObject()
        {
            XamlSchemaContext xsc = new XamlSchemaContext();

            var generated = new StringBuilder();
            var xmlSettings = new XmlWriterSettings { Indent = true, OmitXmlDeclaration = true };
            var xamlSettings = new XamlXmlWriterSettings { CloseOutput = true };
            using (XamlXmlWriter writer = new XamlXmlWriter(XmlWriter.Create(new StringWriter(generated), xmlSettings), xsc, xamlSettings))
            {
                string ns = "clr-namespace:Test.Elements;assembly=XamlTestClasses";
                writer.WriteNamespace(new NamespaceDeclaration(ns, "PreferredNamespace"));

                XamlType bc = xsc.GetXamlType(typeof(BigContainer));
                writer.WriteStartObject(bc);
                writer.WriteEndObject();
            }

            var target = @"<PreferredNamespace:BigContainer xmlns:PreferredNamespace=""clr-namespace:Test.Elements;assembly=XamlTestClasses"" />";
            Assert.AreEqual(target, generated.ToString());
        }

        [TestMethod]
        public void AssignNamespacePrefixBeforeMember()
        {
            XamlSchemaContext xsc = new XamlSchemaContext();

            var generated = new StringBuilder();
            var xmlSettings = new XmlWriterSettings { Indent = true, OmitXmlDeclaration = true };
            var xamlSettings = new XamlXmlWriterSettings { CloseOutput = true };
            using (XamlXmlWriter writer = new XamlXmlWriter(XmlWriter.Create(new StringWriter(generated), xmlSettings), xsc, xamlSettings))
            {
                XamlType bc = xsc.GetXamlType(typeof(BigContainer));
                writer.WriteStartObject(bc);

                string ns = "clr-namespace:Test.Elements.AnotherNamespace;assembly=XamlTestClasses";
                writer.WriteNamespace(new NamespaceDeclaration(ns, "PreferredPrefix"));
                writer.WriteStartMember(bc.GetMember("MediumContainer"));

                XamlType mc = xsc.GetXamlType(typeof(MediumContainer));
                writer.WriteStartObject(mc);
                writer.WriteEndObject();
                writer.WriteEndMember();
                writer.WriteEndObject();
            }

            var target = @"<BigContainer xmlns=""clr-namespace:Test.Elements;assembly=XamlTestClasses"">
  <BigContainer.MediumContainer xmlns:PreferredPrefix=""clr-namespace:Test.Elements.AnotherNamespace;assembly=XamlTestClasses"">
    <PreferredPrefix:MediumContainer />
  </BigContainer.MediumContainer>
</BigContainer>";
            Assert.AreEqual(target, generated.ToString());
        }

        [TestMethod]
        public void CloseOutstandingStartTags()
        {
            XamlSchemaContext xsc = new XamlSchemaContext();

            var generated = new StringBuilder();
            var xmlSettings = new XmlWriterSettings { Indent = true, OmitXmlDeclaration = true };
            var xamlSettings = new XamlXmlWriterSettings { CloseOutput = true };
            using (XamlXmlWriter writer = new XamlXmlWriter(XmlWriter.Create(new StringWriter(generated), xmlSettings), xsc, xamlSettings))
            {
                string ns = "clr-namespace:Test.Elements;assembly=XamlTestClasses";
                writer.WriteNamespace(new NamespaceDeclaration(ns, "te"));
                XamlType bc = xsc.GetXamlType(typeof(BigContainer));
                writer.WriteStartObject(bc);
                writer.WriteStartMember(bc.GetMember("Integer"));
                writer.WriteValue("123");
                writer.WriteEndMember();
                writer.WriteStartMember(bc.GetMember("MediumContainer"));
                XamlType mc = xsc.GetXamlType(typeof(MediumContainer));
                writer.WriteStartObject(mc);
                writer.WriteStartMember(mc.GetMember("Integer"));
                writer.WriteValue("321");
                writer.WriteEndMember();
            }

            var target = @"<te:BigContainer Integer=""123"" xmlns:te=""clr-namespace:Test.Elements;assembly=XamlTestClasses"">
  <te:BigContainer.MediumContainer>
    <MediumContainer Integer=""321"" xmlns=""clr-namespace:Test.Elements.AnotherNamespace;assembly=XamlTestClasses"" />
  </te:BigContainer.MediumContainer>
</te:BigContainer>";
            Assert.AreEqual(target, generated.ToString());
        }

        [TestMethod, TestExpectedException(typeof(ArgumentNullException))]
        public void CreateNullStreamOutput()
        {
            XamlSchemaContext xsc = new XamlSchemaContext();
            var generated = new StringBuilder();
            using (XamlXmlWriter writer = new XamlXmlWriter((Stream)null, xsc))
            {
            }
        }


        [TestMethod, TestExpectedException(typeof(ArgumentNullException))]
        public void CreateNullXmlWriterOutput()
        {
            XamlSchemaContext xsc = new XamlSchemaContext();
            var generated = new StringBuilder();
            using (XamlXmlWriter writer = new XamlXmlWriter((XmlWriter)null, xsc))
            {
            }
        }

        [TestMethod]
        public void EncodingStream()
        {
            XamlSchemaContext xsc = new XamlSchemaContext();

            MemoryStream ms = new MemoryStream();
            using (XamlXmlWriter xw = new XamlXmlWriter(ms, xsc, new XamlXmlWriterSettings { CloseOutput = false }))
            {
                XamlType bc = xsc.GetXamlType(typeof(BigContainer));
                xw.WriteStartObject(bc);
                xw.WriteEndObject();
            }

            byte[] octets = ms.ToArray();
            StringBuilder sb = new StringBuilder();
            foreach (byte b in octets)
            {
                sb.Append((char)b);
            }

            string xaml = sb.ToString();
            Assert.IsTrue(xaml.ToUpper().Contains("UTF-8"), "Xaml written to Stream should be UTF-8 by default");
        }

        [TestMethod]
        public void EncodingTextWriter()
        {
            XamlSchemaContext xsc = new XamlSchemaContext();

            StringBuilder sb = new StringBuilder();
            using (XamlXmlWriter xw = new XamlXmlWriter(new StringWriter(sb), xsc, new XamlXmlWriterSettings { CloseOutput = true }))
            {
                XamlType bc = xsc.GetXamlType(typeof(BigContainer));
                xw.WriteStartObject(bc);
                xw.WriteEndObject();
            }

            string xaml = sb.ToString();
            Assert.IsTrue(xaml.ToUpper().Contains("UTF-16"), "Xaml written to StringWriter should be UTF-16 by default");
        }

        [TestMethod]
        public void Settings()
        {
            XamlSchemaContext xsc = new XamlSchemaContext();

            var generated = new StringBuilder();
            var xamlSettings = new XamlXmlWriterSettings { CloseOutput = true };
            using (XamlXmlWriter writer = new XamlXmlWriter(XmlWriter.Create(new StringWriter(generated)), xsc, xamlSettings))
            {
                writer.Settings.AssumeValidInput = !xamlSettings.AssumeValidInput;
                writer.Settings.CloseOutput = !xamlSettings.CloseOutput;

                Assert.AreEqual(writer.Settings.AssumeValidInput, xamlSettings.AssumeValidInput);
                Assert.AreEqual(writer.Settings.CloseOutput, xamlSettings.CloseOutput);
            }
        }

        [TestMethod]
        [TestExpectedException(typeof(ObjectDisposedException))]
        public void CloseOutputSetting()
        {
            XamlSchemaContext xsc = new XamlSchemaContext();

            StringWriter stringWriter = new StringWriter(new StringBuilder());
            var xamlSettings = new XamlXmlWriterSettings { CloseOutput = true };
            using (XamlXmlWriter xw = new XamlXmlWriter(stringWriter, xsc, new XamlXmlWriterSettings { CloseOutput = true }))
            {
                XamlType bc = xsc.GetXamlType(typeof(BigContainer));
                xw.WriteStartObject(bc);
                xw.WriteEndObject();
            }
            stringWriter.WriteLine("Hello");
        }

        [TestMethod]
        public void WriteValueContentProperty()
        {
            XamlSchemaContext xsc = new XamlSchemaContext();

            var generated = new StringBuilder();
            var xmlSettings = new XmlWriterSettings { OmitXmlDeclaration = true };
            var xamlSettings = new XamlXmlWriterSettings { CloseOutput = true };
            using (XamlXmlWriter writer = new XamlXmlWriter(XmlWriter.Create(new StringWriter(generated), xmlSettings), xsc, xamlSettings))
            {
                XamlType bc = xsc.GetXamlType(typeof(BigContainer));
                writer.WriteStartObject(bc);
                writer.WriteStartMember(bc.GetMember("Content"));
                writer.WriteValue("Hello World");
                writer.WriteEndMember();
                writer.WriteEndObject();
            }

            var target = @"<BigContainer xmlns=""clr-namespace:Test.Elements;assembly=XamlTestClasses"">Hello World</BigContainer>";
            Assert.AreEqual(target, generated.ToString());
        }

        [TestMethod]
        public void WriteEmptyStringContentProperty()
        {
            XamlSchemaContext xsc = new XamlSchemaContext();

            var generated = new StringBuilder();
            var xmlSettings = new XmlWriterSettings { OmitXmlDeclaration = true };
            var xamlSettings = new XamlXmlWriterSettings { CloseOutput = true };
            using (XamlXmlWriter writer = new XamlXmlWriter(XmlWriter.Create(new StringWriter(generated), xmlSettings), xsc, xamlSettings))
            {
                XamlType bc = xsc.GetXamlType(typeof(BigContainer));
                writer.WriteStartObject(bc);
                writer.WriteStartMember(bc.GetMember("Content"));
                writer.WriteValue("");
                writer.WriteEndMember();
                writer.WriteEndObject();
            }

            var target = @"<BigContainer Content="""" xmlns=""clr-namespace:Test.Elements;assembly=XamlTestClasses"" />";
            Assert.AreEqual(target, generated.ToString());
        }

        [TestMethod, TestExpectedException(typeof(XamlXmlWriterException))]
        public void WriteValueOrderError()
        {
            XamlSchemaContext xsc = new XamlSchemaContext();

            var generated = new StringBuilder();
            using (XamlXmlWriter writer = new XamlXmlWriter(XmlWriter.Create(new StringWriter(generated)), xsc))
            {
                XamlType bc = xsc.GetXamlType(typeof(BigContainer));
                writer.WriteValue("hello world");
            }
        }

        [TestMethod, TestExpectedException(typeof(XamlXmlWriterException))]
        public void WriteValueRootError()
        {
            XamlSchemaContext xsc = new XamlSchemaContext();

            var generated = new StringBuilder();
            using (XamlXmlWriter writer = new XamlXmlWriter(XmlWriter.Create(new StringWriter(generated)), xsc))
            {
                writer.WriteValue("hello world");
            }
        }

        [TestMethod, TestExpectedException(typeof(XamlXmlWriterException))]
        public void WriteValueTwice()
        {
            XamlSchemaContext xsc = new XamlSchemaContext();

            var generated = new StringBuilder();
            var xmlSettings = new XmlWriterSettings { Indent = true, OmitXmlDeclaration = true };
            var xamlSettings = new XamlXmlWriterSettings { CloseOutput = true };
            using (XamlXmlWriter writer = new XamlXmlWriter(XmlWriter.Create(new StringWriter(generated), xmlSettings), xsc, xamlSettings))
            {
                XamlType bc = xsc.GetXamlType(typeof(BigContainer));
                writer.WriteStartObject(bc);
                writer.WriteStartMember(bc.GetMember("Integer"));
                writer.WriteValue("d");
                writer.WriteValue("d");
            }
        }

        [TestMethod, TestExpectedException(typeof(ArgumentException))]
        public void WriteValueTypeConverterIgnored()
        {
            XamlSchemaContext xsc = new XamlSchemaContext();

            using (XamlXmlWriter writer = new XamlXmlWriter(XmlWriter.Create(new StringWriter(new StringBuilder())), xsc))
            {
                XamlType bc = xsc.GetXamlType(typeof(BigContainer));
                writer.WriteStartObject(bc);
                writer.WriteStartMember(bc.GetMember("Obj"));
                writer.WriteValue(new Manufacturer { Name = "General Motors" });
                writer.WriteEndMember();
            }
        }

        [TestMethod, TestExpectedException(typeof(ArgumentException))]
        public void WriteValueTypeConverterNone()
        {
            XamlSchemaContext xsc = new XamlSchemaContext();

            using (XamlXmlWriter writer = new XamlXmlWriter(XmlWriter.Create(new StringWriter(new StringBuilder())), xsc))
            {
                XamlType bc = xsc.GetXamlType(typeof(BigContainer));
                writer.WriteStartObject(bc);
                writer.WriteStartMember(bc.GetMember("Obj"));
                writer.WriteValue(new Driver { Name = "Joseph McDonough" });
                writer.WriteEndMember();
            }
        }


        [TestMethod]
        public void WriteDirective()
        {
            XamlSchemaContext xsc = new XamlSchemaContext();

            var generated = new StringBuilder();
            var xmlSettings = new XmlWriterSettings { Indent = true, OmitXmlDeclaration = true };
            var xamlSettings = new XamlXmlWriterSettings { CloseOutput = true };
            using (XamlXmlWriter writer = new XamlXmlWriter(XmlWriter.Create(new StringWriter(generated), xmlSettings), xsc, xamlSettings))
            {
                writer.WriteNamespace(new NamespaceDeclaration("clr-namespace:Test.Elements;assembly=XamlTestClasses", "strange_prefix"));
                string xaml2006Namespace = "http://schemas.microsoft.com/winfx/2006/xaml";
                writer.WriteNamespace(new NamespaceDeclaration(xaml2006Namespace, "x2"));
                XamlType bc = xsc.GetXamlType(typeof(BigContainer));
                writer.WriteStartObject(bc);
                XamlMember RuntimeNameProperty = xsc.GetXamlDirective(xaml2006Namespace, "Name");
                writer.WriteStartMember(RuntimeNameProperty);
                writer.WriteValue("test");
                writer.WriteEndMember();
                writer.WriteEndObject();
            }

            var target = @"<strange_prefix:BigContainer x2:Name=""test"" xmlns:strange_prefix=""clr-namespace:Test.Elements;assembly=XamlTestClasses"" xmlns:x2=""http://schemas.microsoft.com/winfx/2006/xaml"" />";
            Assert.AreEqual(target, generated.ToString());
        }

        [TestMethod]
        public void WriteDirectives()
        {
            XamlSchemaContext xsc = new XamlSchemaContext();
            var generated = new StringBuilder();
            var xmlSettings = new XmlWriterSettings { Indent = true, OmitXmlDeclaration = true };
            var xamlSettings = new XamlXmlWriterSettings { CloseOutput = true };
            using (XamlXmlWriter writer = new XamlXmlWriter(XmlWriter.Create(new StringWriter(generated), xmlSettings), xsc, xamlSettings))
            {
                string xaml2006Namespace = "http://schemas.microsoft.com/winfx/2006/xaml";
                writer.WriteNamespace(new NamespaceDeclaration(xaml2006Namespace, "x2"));
                XamlType bc = xsc.GetXamlType(typeof(BigContainer));
                writer.WriteStartObject(bc);
                XamlMember KeyProperty = xsc.GetXamlDirective(xaml2006Namespace, "Key");
                writer.WriteStartMember(KeyProperty);
                writer.WriteValue("my_key");
                writer.WriteEndMember();
                XamlMember RuntimeNameProperty = xsc.GetXamlDirective(xaml2006Namespace, "Name");
                writer.WriteStartMember(RuntimeNameProperty);
                writer.WriteValue("my_Name");
                writer.WriteEndMember();
                writer.WriteEndObject();
            }

            var target = @"<BigContainer x2:Key=""my_key"" x2:Name=""my_Name"" xmlns=""clr-namespace:Test.Elements;assembly=XamlTestClasses"" xmlns:x2=""http://schemas.microsoft.com/winfx/2006/xaml"" />";
            Assert.AreEqual(target, generated.ToString());
        }

        [TestMethod, TestExpectedException(typeof(XamlXmlWriterException))]
        public void WriteEmptyMemberAttribute()
        {
            XamlSchemaContext xsc = new XamlSchemaContext();
            var generated = new StringBuilder();
            var xamlSettings = new XamlXmlWriterSettings { CloseOutput = true };
            using (XamlXmlWriter writer = new XamlXmlWriter(XmlWriter.Create(new StringWriter(generated)), xsc, xamlSettings))
            {
                XamlType bc = xsc.GetXamlType(typeof(BigContainer));
                writer.WriteStartObject(bc);
                writer.WriteStartMember(bc.GetMember("Integer"));
                writer.WriteEndMember();
                writer.WriteEndObject();
            }
        }

        [TestMethod, TestExpectedException(typeof(XamlXmlWriterException))]
        public void WriteEndOrderError()
        {
            XamlSchemaContext xsc = new XamlSchemaContext();
            var generated = new StringBuilder();
            using (XamlXmlWriter writer = new XamlXmlWriter(XmlWriter.Create(new StringWriter(generated)), xsc))
            {
                XamlType bc = xsc.GetXamlType(typeof(BigContainer));
                writer.WriteStartObject(bc);
                writer.WriteEndMember();
            }
        }

        [TestMethod]
        public void WriteMixedContentElementForm()
        {
            XamlSchemaContext xsc = new XamlSchemaContext();
            var generated = new StringBuilder();
            var xmlSettings = new XmlWriterSettings { Indent = true, OmitXmlDeclaration = true };
            var xamlSettings = new XamlXmlWriterSettings { CloseOutput = true,  AssumeValidInput = true };
            using (XamlXmlWriter writer = new XamlXmlWriter(XmlWriter.Create(new StringWriter(generated), xmlSettings), xsc, xamlSettings))
            {
                XamlType bc = xsc.GetXamlType(typeof(BigContainer));
                XamlType sc = xsc.GetXamlType(typeof(SmallContainer));
                XamlType mc = xsc.GetXamlType(typeof(MediumContainer));

                writer.WriteNamespace(new NamespaceDeclaration("clr-namespace:Test.Elements;assembly=XamlTestClasses", "te"));
                writer.WriteNamespace(new NamespaceDeclaration("clr-namespace:Test.Elements.AnotherNamespace;assembly=XamlTestClasses", "tea"));

                writer.WriteStartObject(bc);
                // Member with atom then record
                writer.WriteStartMember(bc.GetMember("Double"));
                writer.WriteValue("1.23");
                writer.WriteStartObject(sc);
                writer.WriteStartMember(sc.GetMember("Integer"));
                writer.WriteValue("92");
                writer.WriteEndMember();
                writer.WriteStartMember(sc.GetMember("Chars"));
                writer.WriteValue("44");
                writer.WriteEndMember();
                writer.WriteEndObject();
                writer.WriteEndMember();

                // Member with record then atom
                writer.WriteStartMember(bc.GetMember("Obj"));
                writer.WriteStartObject(mc);
                writer.WriteStartMember(mc.GetMember("Integer"));
                writer.WriteValue("32");
                writer.WriteEndMember();
                writer.WriteEndObject();
                writer.WriteValue("Siren");
                writer.WriteEndMember();

                writer.WriteEndObject();
            }

            var target = @"<te:BigContainer xmlns:te=""clr-namespace:Test.Elements;assembly=XamlTestClasses"" xmlns:tea=""clr-namespace:Test.Elements.AnotherNamespace;assembly=XamlTestClasses"">
  <te:BigContainer.Double>1.23<te:SmallContainer Integer=""92"" Chars=""44"" /></te:BigContainer.Double>
  <te:BigContainer.Obj>
    <tea:MediumContainer Integer=""32"" />Siren</te:BigContainer.Obj>
</te:BigContainer>"; 
            Assert.AreEqual(target, generated.ToString());
        }

        [TestMethod]
        public void WriteMixedContentTryAttributes()
        {
            XamlSchemaContext xsc = new XamlSchemaContext();
            var generated = new StringBuilder();
            var xmlSettings = new XmlWriterSettings { Indent = true, OmitXmlDeclaration = true };
            var xamlSettings = new XamlXmlWriterSettings { CloseOutput = true, AssumeValidInput = true };
            using (XamlXmlWriter writer = new XamlXmlWriter(XmlWriter.Create(new StringWriter(generated), xmlSettings), xsc, xamlSettings))
            {
                writer.WriteNamespace(new NamespaceDeclaration("clr-namespace:Test.Elements;assembly=XamlTestClasses", "te"));
                writer.WriteNamespace(new NamespaceDeclaration("clr-namespace:Test.Elements.AnotherNamespace;assembly=XamlTestClasses", "tea"));
                XamlType bc = xsc.GetXamlType(typeof(BigContainer));
                XamlType sc = xsc.GetXamlType(typeof(SmallContainer));
                XamlType mc = xsc.GetXamlType(typeof(MediumContainer));
                writer.WriteStartObject(bc);
                // Member with atom then record
                writer.WriteStartMember(bc.GetMember("Double"));
                writer.WriteValue("1.23");
                writer.WriteStartObject(sc);
                writer.WriteStartMember(sc.GetMember("Integer"));
                writer.WriteValue("92");
                writer.WriteEndMember();
                writer.WriteStartMember(sc.GetMember("Chars"));
                writer.WriteValue("44");
                writer.WriteEndMember();
                writer.WriteStartMember(bc.GetMember("SmallContainer"));
                writer.WriteValue("140");
                writer.WriteEndMember();
                writer.WriteEndObject();
                writer.WriteEndMember();

                // Member with record then atom
                writer.WriteStartMember(bc.GetMember("Obj"));
                writer.WriteStartObject(mc);
                writer.WriteStartMember(mc.GetMember("Integer"));
                writer.WriteValue("32");
                writer.WriteEndMember();
                writer.WriteEndObject();
                writer.WriteValue("Siren");
                writer.WriteEndMember();

                writer.WriteEndObject();
            }

            var target = @"<te:BigContainer xmlns:te=""clr-namespace:Test.Elements;assembly=XamlTestClasses"" xmlns:tea=""clr-namespace:Test.Elements.AnotherNamespace;assembly=XamlTestClasses"">
  <te:BigContainer.Double>1.23<te:SmallContainer Integer=""92"" Chars=""44"" SmallContainer=""140"" /></te:BigContainer.Double>
  <te:BigContainer.Obj>
    <tea:MediumContainer Integer=""32"" />Siren</te:BigContainer.Obj>
</te:BigContainer>";
            Assert.AreEqual(target, generated.ToString());
        }

        [TestMethod]
        public void WriteNode()
        {
            var target = @"<te:BigContainer Integer=""123"" xmlns:te=""clr-namespace:Test.Elements;assembly=XamlTestClasses"" />";

            XamlSchemaContext xsc = new XamlSchemaContext();
            XamlReader reader = new XamlXmlReader(XmlReader.Create(new StringReader(target)), xsc);
            var generated = new StringBuilder();
            var xmlSettings = new XmlWriterSettings { Indent = true, OmitXmlDeclaration = true };
            var xamlSettings = new XamlXmlWriterSettings { CloseOutput = true };

            // Read and write the entire FireEngine
            using (XamlXmlWriter writer = new XamlXmlWriter(XmlWriter.Create(new StringWriter(generated), xmlSettings), xsc, xamlSettings))
            {
                while (reader.Read())
                {
                    writer.WriteNode(reader);
                }
            }

            Assert.AreEqual(target, generated.ToString());
        }

        [TestMethod]
        [TestExpectedException(typeof(ArgumentNullException))]
        public void WriteNodeNullArgument()
        {
            XamlSchemaContext xsc = new XamlSchemaContext();
            TextWriter writer = new StringWriter();
            new XamlXmlWriter(writer, xsc).WriteNode(null);
        }

        [TestMethod]
        public void ImplicitFlushOutput()
        {
            XamlSchemaContext xsc = new XamlSchemaContext();
            StringBuilder sb = new StringBuilder();
            using (XamlXmlWriter writer = new XamlXmlWriter(new StringWriter(sb), xsc))
            {
                XamlType bc = xsc.GetXamlType(typeof(BigContainer));
                writer.WriteStartObject(bc);
                writer.WriteStartMember(bc.GetMember("Integer"));
                writer.WriteValue("123");
                writer.WriteEndMember();
                writer.WriteEndObject();
            }

            var target = @"<?xml version=""1.0"" encoding=""utf-16""?><BigContainer Integer=""123"" xmlns=""clr-namespace:Test.Elements;assembly=XamlTestClasses"" />";
            Assert.AreEqual(target, sb.ToString());
        }

        [TestMethod]
        public void ExplicitFlushOutput()
        {
            XamlSchemaContext xsc = new XamlSchemaContext();
            StringBuilder sb = new StringBuilder();
            using (XamlXmlWriter writer = new XamlXmlWriter(new StringWriter(sb), xsc))
            {
                XamlType bc = xsc.GetXamlType(typeof(BigContainer));
                writer.WriteStartObject(bc);
                writer.WriteStartMember(bc.GetMember("Integer"));
                writer.WriteValue("123");
                writer.WriteEndMember();
                writer.WriteEndObject();
                writer.Flush();

                var target = @"<?xml version=""1.0"" encoding=""utf-16""?><BigContainer Integer=""123"" xmlns=""clr-namespace:Test.Elements;assembly=XamlTestClasses"" />";
                Assert.AreEqual(target, sb.ToString());
            }
        }

        [TestMethod]
        public void CloseUnderlyingWriter()
        {
            XamlSchemaContext xsc = new XamlSchemaContext();
            var sb = new StringBuilder();
            using (XamlWriter xw = new XamlXmlWriter(new StringWriter(sb), xsc))
            {
                XamlType bc = xsc.GetXamlType(typeof(BigContainer));
                xw.WriteStartObject(bc);
                xw.WriteEndObject();
            }

            var expected = @"<?xml version=""1.0"" encoding=""utf-16""?><BigContainer xmlns=""clr-namespace:Test.Elements;assembly=XamlTestClasses"" />";
            Assert.AreEqual(expected, sb.ToString());
        }

        [TestMethod]
        public void WritePropertiesWithDifferentTypesInDifferentNamespace()
        {
            var xsc = new XamlSchemaContext();
            var generated = new StringBuilder();
            var xmlSettings = new XmlWriterSettings { OmitXmlDeclaration = true, Indent = true };
            var xamlSettings = new XamlXmlWriterSettings { CloseOutput = true };
            using (XamlXmlWriter writer = new XamlXmlWriter(XmlWriter.Create(new StringWriter(generated), xmlSettings), xsc, xamlSettings))
            {
                XamlType mc = xsc.GetXamlType(typeof(MediumContainer));
                XamlMember thing = mc.GetAttachableMember("Thing");
                XamlType bc = xsc.GetXamlType(typeof(BigContainer));
                writer.WriteStartObject(bc);
                writer.WriteStartMember(bc.GetMember("Integer"));
                writer.WriteValue("value1");
                writer.WriteEndMember();
                writer.WriteStartMember(thing);
                writer.WriteValue("value2");
                writer.WriteEndMember();
                writer.WriteEndObject();
            }

            var target = @"<BigContainer Integer=""value1"" xmlns=""clr-namespace:Test.Elements;assembly=XamlTestClasses"">
  <tea:MediumContainer.Thing xmlns:tea=""clr-namespace:Test.Elements.AnotherNamespace;assembly=XamlTestClasses"">value2</tea:MediumContainer.Thing>
</BigContainer>";
            Assert.AreEqual(target, generated.ToString());
        }

        [TestMethod]
        public void WritePropertiesWithDifferentTypesInSameNamespace()
        {
            var xsc = new XamlSchemaContext();
            var generated = new StringBuilder();
            var xmlSettings = new XmlWriterSettings { OmitXmlDeclaration = true };
            var xamlSettings = new XamlXmlWriterSettings { CloseOutput = true };
            using (XamlXmlWriter writer = new XamlXmlWriter(XmlWriter.Create(new StringWriter(generated), xmlSettings), xsc, xamlSettings))
            {
                XamlType mc = xsc.GetXamlType(typeof(MediumContainer));
                XamlMember thing = mc.GetAttachableMember("Thing");
                XamlType ac = xsc.GetXamlType(typeof(AnotherContainer));
                writer.WriteStartObject(ac);
                writer.WriteStartMember(ac.GetMember("Integer"));
                writer.WriteValue("value1");
                writer.WriteEndMember();
                writer.WriteStartMember(thing);
                writer.WriteValue("value2");
                writer.WriteEndMember();
                writer.WriteEndObject();
            }

            var target = @"<AnotherContainer Integer=""value1"" MediumContainer.Thing=""value2"" xmlns=""clr-namespace:Test.Elements.AnotherNamespace;assembly=XamlTestClasses"" />";
            Assert.AreEqual(target, generated.ToString());
        }

        [TestMethod]
        public void WriteRecordNested()
        {
            var xsc = new XamlSchemaContext();
            var generated = new StringBuilder();
            var xmlSettings = new XmlWriterSettings { Indent = true, OmitXmlDeclaration = true };
            var xamlSettings = new XamlXmlWriterSettings { CloseOutput = true };
            using (XamlXmlWriter writer = new XamlXmlWriter(XmlWriter.Create(new StringWriter(generated), xmlSettings), xsc, xamlSettings))
            {
                string ns = "clr-namespace:Test.Elements;assembly=XamlTestClasses";
                writer.WriteNamespace(new NamespaceDeclaration(ns, "te"));

                XamlType bc = xsc.GetXamlType(typeof(BigContainer));
                writer.WriteStartObject(bc);
                writer.WriteStartMember(bc.GetMember("Chars"));
                writer.WriteValue("Red");
                writer.WriteEndMember();
                writer.WriteStartMember(bc.GetMember("Obj"));
                writer.WriteStartObject(bc);
                writer.WriteStartMember(bc.GetMember("Obj"));
                writer.WriteValue("32");
                writer.WriteEndMember();
                writer.WriteEndObject();
                writer.WriteEndMember();
                writer.WriteEndObject();
            }

            var target = @"<te:BigContainer Chars=""Red"" xmlns:te=""clr-namespace:Test.Elements;assembly=XamlTestClasses"">
  <te:BigContainer.Obj>
    <te:BigContainer Obj=""32"" />
  </te:BigContainer.Obj>
</te:BigContainer>";
            Assert.AreEqual(target, generated.ToString());
        }

        [TestMethod, TestExpectedException(typeof(XamlXmlWriterException))]
        public void WriteObjectOrderError()
        {
            XamlSchemaContext xsc = new XamlSchemaContext();
            var generated = new StringBuilder();
            using (XamlXmlWriter writer = new XamlXmlWriter(XmlWriter.Create(new StringWriter(generated)), xsc))
            {
                writer.WriteStartObject(xsc.GetXamlType(typeof(BigContainer)));
                writer.WriteStartObject(xsc.GetXamlType(typeof(SmallContainer)));
            }
        }

        [TestMethod, TestExpectedException(typeof(XamlXmlWriterException))]
        public void WriteMemberOrderError()
        {
            XamlSchemaContext xsc = new XamlSchemaContext();
            var generated = new StringBuilder();
            using (XamlXmlWriter writer = new XamlXmlWriter(XmlWriter.Create(new StringWriter(generated)), xsc))
            {
                XamlType bc = xsc.GetXamlType(typeof(BigContainer));
                writer.WriteStartMember(bc.GetMember("Integer"));
            }
        }

        [TestMethod, TestExpectedException(typeof(XamlXmlWriterException))]
        public void WriteMemberOrderError2()
        {
            XamlSchemaContext xsc = new XamlSchemaContext();
            var generated = new StringBuilder();
            using (XamlXmlWriter writer = new XamlXmlWriter(XmlWriter.Create(new StringWriter(generated)), xsc))
            {
                XamlType bc = xsc.GetXamlType(typeof(BigContainer));
                writer.WriteStartMember(bc.GetMember("Integer"));
                writer.WriteStartMember(bc.GetMember("Double"));
            }
        }

        [TestMethod]
        public void WriteMemberAlternateSimpleAndComplex()
        {
            var xsc = new XamlSchemaContext();

            var generated = new StringBuilder();
            var xmlSettings = new XmlWriterSettings { Indent = true, OmitXmlDeclaration = true };
            var xamlSettings = new XamlXmlWriterSettings { CloseOutput = true };
            using (XamlXmlWriter writer = new XamlXmlWriter(XmlWriter.Create(new StringWriter(generated), xmlSettings), xsc, xamlSettings))
            {
                string ns = "clr-namespace:Test.Elements;assembly=XamlTestClasses";
                writer.WriteNamespace(new NamespaceDeclaration(ns, "te"));

                XamlType bc = xsc.GetXamlType(typeof(BigContainer));
                writer.WriteStartObject(bc);
                writer.WriteStartMember(bc.GetMember("Integer"));
                writer.WriteValue("3");
                writer.WriteEndMember();

                writer.WriteStartMember(bc.GetMember("SmallContainer"));

                XamlType sc = xsc.GetXamlType(typeof(SmallContainer));
                writer.WriteStartObject(sc);
                writer.WriteStartMember(sc.GetMember("Integer"));
                writer.WriteValue("1");
                writer.WriteEndMember();
                writer.WriteEndObject();

                writer.WriteStartObject(sc);
                writer.WriteStartMember(sc.GetMember("Integer"));
                writer.WriteValue("2");
                writer.WriteEndMember();
                writer.WriteEndObject();

                writer.WriteEndMember();

                writer.WriteStartMember(bc.GetMember("Double"));
                writer.WriteValue("12.34");
                writer.WriteEndMember();
                writer.WriteEndObject();
            }

            var target = @"<te:BigContainer Integer=""3"" xmlns:te=""clr-namespace:Test.Elements;assembly=XamlTestClasses"">
  <te:BigContainer.SmallContainer>
    <te:SmallContainer Integer=""1"" />
    <te:SmallContainer Integer=""2"" />
  </te:BigContainer.SmallContainer>
  <te:BigContainer.Double>12.34</te:BigContainer.Double>
</te:BigContainer>";
            Assert.AreEqual(target, generated.ToString());
        }

        [TestMethod, TestExpectedException(typeof(ArgumentNullException))]
        public void WriteObjectNullTypeName()
        {
            var generated = new StringBuilder();
            using (XamlXmlWriter writer = new XamlXmlWriter(XmlWriter.Create(new StringWriter(generated)), new XamlSchemaContext()))
            {
                writer.WriteStartObject(null);
            }
        }

        [TestMethod, TestExpectedException(typeof(ArgumentNullException))]
        public void WriteMemberNullTypeName()
        {
            var generated = new StringBuilder();
            using (XamlXmlWriter writer = new XamlXmlWriter(XmlWriter.Create(new StringWriter(generated)), new XamlSchemaContext()))
            {
                writer.WriteStartMember(null);
            }
        }

        [TestMethod]
        public void WriteObjectContentProperty()
        {
            var xsc = new XamlSchemaContext();
            var generated = new StringBuilder();
            var xmlSettings = new XmlWriterSettings { OmitXmlDeclaration = true, Indent = true };
            var xamlSettings = new XamlXmlWriterSettings { CloseOutput = true };
            using (XamlXmlWriter writer = new XamlXmlWriter(XmlWriter.Create(new StringWriter(generated), xmlSettings), xsc, xamlSettings))
            {
                string ns = "clr-namespace:Test.Elements;assembly=XamlTestClasses";
                writer.WriteNamespace(new NamespaceDeclaration(ns, "te"));

                XamlType bc = xsc.GetXamlType(typeof(BigContainer));
                XamlType sc = xsc.GetXamlType(typeof(SmallContainer));
                XamlType mc = xsc.GetXamlType(typeof(MediumContainer));

                writer.WriteStartObject(bc);
                writer.WriteStartMember(bc.GetMember("Content"));
                writer.WriteStartObject(sc);
                writer.WriteStartMember(sc.GetMember("Content"));
                writer.WriteStartObject(mc);
                writer.WriteEndObject();
                writer.WriteEndMember();
                writer.WriteEndObject();
                writer.WriteEndMember();
                writer.WriteEndObject();
            }

            var target = @"<te:BigContainer xmlns:te=""clr-namespace:Test.Elements;assembly=XamlTestClasses"">
  <te:SmallContainer>
    <MediumContainer xmlns=""clr-namespace:Test.Elements.AnotherNamespace;assembly=XamlTestClasses"" />
  </te:SmallContainer>
</te:BigContainer>";
            Assert.AreEqual(target, generated.ToString());
        }

        [TestMethod]
        public void WriteNullAtomValue()
        {
            var xsc = new XamlSchemaContext();
            var generated = new StringBuilder();
            var xmlSettings = new XmlWriterSettings { OmitXmlDeclaration = true, Indent = true };
            var xamlSettings = new XamlXmlWriterSettings { CloseOutput = true };
            using (XamlXmlWriter writer = new XamlXmlWriter(XmlWriter.Create(new StringWriter(generated), xmlSettings), xsc, xamlSettings))
            {
                string ns = "clr-namespace:Test.Elements;assembly=XamlTestClasses";
                writer.WriteNamespace(new NamespaceDeclaration(ns, "te"));
                XamlType bc = xsc.GetXamlType(typeof(BigContainer));
                writer.WriteStartObject(bc);
                string xaml2006Namespace = "http://schemas.microsoft.com/winfx/2006/xaml";
                writer.WriteNamespace(new NamespaceDeclaration(xaml2006Namespace, "x"));
                writer.WriteStartMember(bc.GetMember("SmallContainer"));
                writer.WriteStartObject(xsc.GetXamlType(xaml2006Namespace, "Null"));
                writer.WriteEndObject();
                writer.WriteEndMember();
                writer.WriteEndObject();
            }

            var target = @"<te:BigContainer xmlns:te=""clr-namespace:Test.Elements;assembly=XamlTestClasses"">
  <te:BigContainer.SmallContainer xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
    <x:Null />
  </te:BigContainer.SmallContainer>
</te:BigContainer>";
            Assert.AreEqual(target, generated.ToString());
        }

        [TestMethod]
        public void WriteNullAtomValueInMemberSyntax()
        {
            var xsc = new XamlSchemaContext();
            var generated = new StringBuilder();
            var xmlSettings = new XmlWriterSettings { OmitXmlDeclaration = true, Indent = true };
            var xamlSettings = new XamlXmlWriterSettings { CloseOutput = true };
            using (XamlXmlWriter writer = new XamlXmlWriter(XmlWriter.Create(new StringWriter(generated), xmlSettings), xsc, xamlSettings))
            {
                string ns = "clr-namespace:Test.Elements;assembly=XamlTestClasses";
                writer.WriteNamespace(new NamespaceDeclaration(ns, "te"));

                XamlType bc = xsc.GetXamlType(typeof(BigContainer));
                writer.WriteStartObject(bc);
                writer.WriteStartMember(bc.GetMember("SmallContainer"));
                writer.WriteValue(null);
                writer.WriteEndMember();
                writer.WriteEndObject();
            }

            var target = @"<te:BigContainer xmlns:te=""clr-namespace:Test.Elements;assembly=XamlTestClasses"">
  <te:BigContainer.SmallContainer>
    <Null xmlns=""http://schemas.microsoft.com/winfx/2006/xaml"" />
  </te:BigContainer.SmallContainer>
</te:BigContainer>";
            Assert.AreEqual(target, generated.ToString());
        }

        [TestMethod]
        public void WriteNullAtomInMixedContent()
        {
            XamlSchemaContext xsc = new XamlSchemaContext();
            var generated = new StringBuilder();
            var xmlSettings = new XmlWriterSettings { Indent = true, OmitXmlDeclaration = true };
            var xamlSettings = new XamlXmlWriterSettings { CloseOutput = true,  AssumeValidInput = true };
            using (XamlXmlWriter writer = new XamlXmlWriter(XmlWriter.Create(new StringWriter(generated), xmlSettings), xsc, xamlSettings))
            {
                string ns = "clr-namespace:Test.Elements;assembly=XamlTestClasses";
                writer.WriteNamespace(new NamespaceDeclaration(ns, "te"));

                XamlType bc = xsc.GetXamlType(typeof(BigContainer));
                XamlType sc = xsc.GetXamlType(typeof(SmallContainer));
                XamlType mc = xsc.GetXamlType(typeof(MediumContainer));
                writer.WriteStartObject(bc);
                // Member with atom then record
                writer.WriteStartMember(bc.GetMember("Double"));
                writer.WriteValue(null);
                writer.WriteStartObject(sc);
                writer.WriteStartMember(sc.GetMember("Integer"));
                writer.WriteValue("92");
                writer.WriteEndMember();
                writer.WriteStartMember(sc.GetMember("Chars"));
                writer.WriteValue("44");
                writer.WriteEndMember();
                writer.WriteEndObject();
                writer.WriteEndMember();

                // Member with record then atom
                writer.WriteStartMember(bc.GetMember("Obj"));
                writer.WriteStartObject(mc);
                writer.WriteStartMember(mc.GetMember("Integer"));
                writer.WriteValue("32");
                writer.WriteEndMember();
                writer.WriteEndObject();
                writer.WriteValue(null);
                writer.WriteEndMember();

                writer.WriteEndObject();
            }

            var target = @"<te:BigContainer xmlns:te=""clr-namespace:Test.Elements;assembly=XamlTestClasses"">
  <te:BigContainer.Double>
    <Null xmlns=""http://schemas.microsoft.com/winfx/2006/xaml"" />
    <te:SmallContainer Integer=""92"" Chars=""44"" />
  </te:BigContainer.Double>
  <te:BigContainer.Obj>
    <tea:MediumContainer Integer=""32"" xmlns:tea=""clr-namespace:Test.Elements.AnotherNamespace;assembly=XamlTestClasses"" />
    <Null xmlns=""http://schemas.microsoft.com/winfx/2006/xaml"" />
  </te:BigContainer.Obj>
</te:BigContainer>";
            Assert.AreEqual(target, generated.ToString());
        }

        [TestMethod, TestExpectedException(typeof(XamlXmlWriterException))]
        public void WriteMemberDuplicateChecked()
        {
            var xsc = new XamlSchemaContext();

            var generated = new StringBuilder();
            var xmlSettings = new XmlWriterSettings { Indent = true, OmitXmlDeclaration = true };
            var xamlSettings = new XamlXmlWriterSettings { CloseOutput = true };
            using (XamlXmlWriter writer = new XamlXmlWriter(XmlWriter.Create(new StringWriter(generated), xmlSettings), xsc, xamlSettings))
            {
                XamlType bc = xsc.GetXamlType(typeof(BigContainer));
                writer.WriteStartObject(bc);
                writer.WriteStartMember(bc.GetMember("Integer"));
                writer.WriteValue("123");
                writer.WriteEndMember();
                writer.WriteStartMember(bc.GetMember("Integer"));
            }
        }

        [TestMethod, TestExpectedException(typeof(XamlXmlWriterException))]
        public void WriteMemberDuplicateCheckedTryAttribute()
        {
            var xsc = new XamlSchemaContext();

            var generated = new StringBuilder();
            var xmlSettings = new XmlWriterSettings { Indent = true, OmitXmlDeclaration = true };
            var xamlSettings = new XamlXmlWriterSettings { CloseOutput = true };
            using (XamlXmlWriter writer = new XamlXmlWriter(XmlWriter.Create(new StringWriter(generated), xmlSettings), xsc, xamlSettings))
            {
                XamlType bc = xsc.GetXamlType(typeof(BigContainer));
                writer.WriteStartObject(bc);
                writer.WriteStartMember(bc.GetMember("Integer"));
                writer.WriteValue("123");
                writer.WriteEndMember();
                writer.WriteStartMember(bc.GetMember("Integer"));
            }
        }

        [TestMethod, TestExpectedException(typeof(XamlXmlWriterException))]
        public void WriteMemberDuplicateCheckedElementForm()
        {
            var xsc = new XamlSchemaContext();

            var generated = new StringBuilder();
            var xmlSettings = new XmlWriterSettings { Indent = true, OmitXmlDeclaration = true };
            var xamlSettings = new XamlXmlWriterSettings { CloseOutput = true };
            using (XamlXmlWriter writer = new XamlXmlWriter(XmlWriter.Create(new StringWriter(generated), xmlSettings), xsc, xamlSettings))
            {
                XamlType bc = xsc.GetXamlType(typeof(BigContainer));
                writer.WriteStartObject(bc);
                writer.WriteStartMember(bc.GetMember("Integer"));
                writer.WriteValue("123");
                writer.WriteEndMember();
                writer.WriteStartMember(bc.GetMember("Integer"));
            }
        }

        [TestMethod, TestExpectedException(typeof(XamlXmlWriterException))]
        public void WriteRootObjectFromMember()
        {
            XamlSchemaContext xsc = new XamlSchemaContext();
            var generated = new StringBuilder();
            var xmlSettings = new XmlWriterSettings { Indent = true, OmitXmlDeclaration = true };

            using (XamlXmlWriter writer = new XamlXmlWriter(XmlWriter.Create(new StringWriter(generated), xmlSettings), xsc))
            {
                XamlType bc = xsc.GetXamlType(typeof(Dictionary<int, int>));
                writer.WriteGetObject();
            }
        }

        [TestMethod, TestExpectedException(typeof(InvalidOperationException))]
        public void WriteNamespaceBeforeObjectFromMember()
        {
            XamlSchemaContext xsc = new XamlSchemaContext();
            var generated = new StringBuilder();
            var xmlSettings = new XmlWriterSettings { Indent = true, OmitXmlDeclaration = true };

            using (XamlXmlWriter writer = new XamlXmlWriter(XmlWriter.Create(new StringWriter(generated), xmlSettings), xsc))
            {
                XamlType bc = xsc.GetXamlType(typeof(BigContainer));
                XamlType list = xsc.GetXamlType(typeof(List<string>));
                writer.WriteStartObject(bc);
                writer.WriteStartMember(bc.GetMember("SmallContainer"));
                writer.WriteNamespace(new NamespaceDeclaration("http://schemas.microsoft.com/winfx/2006/xaml", "x"));
                writer.WriteGetObject();
            }
        }

        [TestMethod]
        public void WriteObjectFromMember()
        {
            XamlSchemaContext xsc = new XamlSchemaContext();
            var generated = new StringBuilder();
            var xmlSettings = new XmlWriterSettings { Indent = true, OmitXmlDeclaration = true };

            using (XamlXmlWriter writer = new XamlXmlWriter(XmlWriter.Create(new StringWriter(generated), xmlSettings), xsc))
            {
                XamlType bc = xsc.GetXamlType(typeof(BigContainer));
                XamlType sc = xsc.GetXamlType(typeof(SmallContainer));
                XamlType list = xsc.GetXamlType(typeof(List<int>));
                writer.WriteStartObject(bc);
                writer.WriteStartMember(bc.GetMember("ListOfInts"));
                writer.WriteGetObject();
                writer.WriteStartMember(XamlLanguage.Items);
                writer.WriteValue("123");
                writer.WriteEndMember();
                writer.WriteEndObject();
                writer.WriteEndMember();
                writer.WriteEndObject();
            }

            var expected = @"<BigContainer xmlns=""clr-namespace:Test.Elements;assembly=XamlTestClasses"">
  <BigContainer.ListOfInts>123</BigContainer.ListOfInts>
</BigContainer>";

            Assert.AreEqual(expected, generated.ToString());
        }

        [TestMethod]
        public void WriteObjectFromMemberFollowedByImplicitProperty()
        {
            XamlSchemaContext xsc = new XamlSchemaContext();
            var generated = new StringBuilder();
            var xmlSettings = new XmlWriterSettings { Indent = true, OmitXmlDeclaration = true };

            using (XamlXmlWriter writer = new XamlXmlWriter(XmlWriter.Create(new StringWriter(generated), xmlSettings), xsc))
            {
                string ns = "clr-namespace:Test.Elements;assembly=XamlTestClasses";
                writer.WriteNamespace(new NamespaceDeclaration(ns, "te"));
                XamlType bc = xsc.GetXamlType(typeof(BigContainer));
                XamlType sc = xsc.GetXamlType(typeof(SmallContainer));
                XamlType list = xsc.GetXamlType(typeof(List<int>));
                writer.WriteStartObject(bc);
                writer.WriteStartMember(bc.GetMember("ListOfInts"));
                writer.WriteGetObject();
                writer.WriteStartMember(XamlLanguage.Items);
                writer.WriteValue("123");
                writer.WriteEndMember();
                writer.WriteEndObject();
                writer.WriteEndMember();
                writer.WriteEndObject();
            }

            var expected = @"<te:BigContainer xmlns:te=""clr-namespace:Test.Elements;assembly=XamlTestClasses"">
  <te:BigContainer.ListOfInts>123</te:BigContainer.ListOfInts>
</te:BigContainer>";

            Assert.AreEqual(expected, generated.ToString());
        }

        [TestMethod]
        public void WriteImplicitProperty()
        {
            XamlSchemaContext xsc = new XamlSchemaContext();
            var generated = new StringBuilder();
            var xmlSettings = new XmlWriterSettings { Indent = true, OmitXmlDeclaration = true };

            using (XamlXmlWriter writer = new XamlXmlWriter(XmlWriter.Create(new StringWriter(generated), xmlSettings), xsc))
            {
                string ns = "clr-namespace:Test.Elements;assembly=XamlTestClasses";
                writer.WriteNamespace(new NamespaceDeclaration(ns, "te"));
                XamlType bc = xsc.GetXamlType(typeof(BigContainer));
                XamlType sc = xsc.GetXamlType(typeof(SmallContainer));
                writer.WriteStartObject(bc);
                writer.WriteStartMember(bc.GetMember("SmallContainer"));
                writer.WriteStartObject(sc);
                writer.WriteStartMember(XamlLanguage.Initialization);
                writer.WriteValue("123");
                writer.WriteEndMember();
                writer.WriteEndObject();
                writer.WriteEndMember();
                writer.WriteEndObject();
            }

            var expected = @"<te:BigContainer xmlns:te=""clr-namespace:Test.Elements;assembly=XamlTestClasses"">
  <te:BigContainer.SmallContainer>
    <te:SmallContainer>123</te:SmallContainer>
  </te:BigContainer.SmallContainer>
</te:BigContainer>";

            Assert.AreEqual(expected, generated.ToString());
        }

        [TestMethod]
        public void WriteSimpleGeneric()
        {
            XamlSchemaContext xsc = new XamlSchemaContext();
            var generated = new StringBuilder();
            var xmlSettings = new XmlWriterSettings { Indent = true, OmitXmlDeclaration = true };

            // typically mscorlib in .NET Framework, and System.Private.CoreLib in .NET Core
            string listAssemblyName = typeof(List<string>).GetAssemblyName();

            using (XamlXmlWriter writer = new XamlXmlWriter(XmlWriter.Create(new StringWriter(generated), xmlSettings), xsc))
            {
                string ns = $"clr-namespace:System.Collections.Generic;assembly={listAssemblyName}";

                writer.WriteNamespace(new NamespaceDeclaration(ns, "scg"));

                XamlType list = xsc.GetXamlType(typeof(List<string>));
                string ns2 = "http://schemas.microsoft.com/winfx/2006/xaml";
                writer.WriteNamespace(new NamespaceDeclaration(ns2, "x"));
                writer.WriteStartObject(list);
                writer.WriteEndObject();
            }

            var expected = @"<scg:List x:TypeArguments=""x:String"" xmlns:scg=""clr-namespace:System.Collections.Generic;assembly={0}"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"" />";
            expected = string.Format(expected, listAssemblyName);

            Assert.AreEqual(expected, generated.ToString());
        }

        [TestMethod]
        public void WriteNestedGeneric()
        {
            XamlSchemaContext xsc = new XamlSchemaContext();
            var generated = new StringBuilder();
            var xmlSettings = new XmlWriterSettings { Indent = true, OmitXmlDeclaration = true };

            var hashSetAssemblyName = typeof(HashSet<List<BigContainer>>).GetAssemblyName();
            var listAssemblyName = typeof(List<Dictionary<int, HashSet<List<BigContainer>>>>).GetAssemblyName();

            using (XamlXmlWriter writer = new XamlXmlWriter(XmlWriter.Create(new StringWriter(generated), xmlSettings), xsc))
            {
                XamlType list = xsc.GetXamlType(typeof(List<Dictionary<int, HashSet<List<BigContainer>>>>));
                var ns1 = $"clr-namespace:System.Collections.Generic;assembly={hashSetAssemblyName}";
                var ns2 = "clr-namespace:Test.Elements;assembly=XamlTestClasses";
                var ns3 = "http://schemas.microsoft.com/winfx/2006/xaml";
                writer.WriteNamespace(new NamespaceDeclaration(ns1, "a"));
                writer.WriteNamespace(new NamespaceDeclaration(ns2, "b"));
                writer.WriteNamespace(new NamespaceDeclaration(ns3, "c"));
                writer.WriteStartObject(list);
                writer.WriteEndObject();
            }

            var expected = @"<List c:TypeArguments=""Dictionary(c:Int32, a:HashSet(List(b:BigContainer)))"" xmlns=""clr-namespace:System.Collections.Generic;assembly={0}"" xmlns:a=""clr-namespace:System.Collections.Generic;assembly={1}"" xmlns:b=""clr-namespace:Test.Elements;assembly=XamlTestClasses"" xmlns:c=""http://schemas.microsoft.com/winfx/2006/xaml"" />";
            expected = string.Format(expected, listAssemblyName, hashSetAssemblyName);

            Assert.AreEqual(expected, generated.ToString());
        }

        [TestMethod]
        public void WriteNestedGenericWithTypeArgumentsContainingUndefinedNamespaces()
        {
            XamlSchemaContext xsc = new XamlSchemaContext();
            var generated = new StringBuilder();
            var xmlSettings = new XmlWriterSettings { Indent = true, OmitXmlDeclaration = true };

            var strListAssemblyName = typeof(List<Dictionary<int, HashSet<List<BigContainer>>>>).GetAssemblyName();
            var strHashSetAssemblyName = typeof(HashSet<List<BigContainer>>).GetAssemblyName();

            using (XamlXmlWriter writer = new XamlXmlWriter(XmlWriter.Create(new StringWriter(generated), xmlSettings), xsc))
            {
                string ns = $"clr-namespace:System.Collections.Generic;assembly={strListAssemblyName}";
                writer.WriteNamespace(new NamespaceDeclaration(ns, "scg"));

                XamlType list = xsc.GetXamlType(typeof(List<Dictionary<int, HashSet<List<BigContainer>>>>));
                writer.WriteStartObject(list);
                writer.WriteEndObject();
            }

            var expected = @"<scg:List xmlns:scg=""clr-namespace:System.Collections.Generic;assembly={0}"">
  <TypeArguments xmlns=""http://schemas.microsoft.com/winfx/2006/xaml"" xmlns:scg1=""clr-namespace:System.Collections.Generic;assembly={1}"" xmlns:te=""clr-namespace:Test.Elements;assembly=XamlTestClasses"">scg:Dictionary(Int32, scg1:HashSet(scg:List(te:BigContainer)))</TypeArguments>
</scg:List>";
            expected = string.Format(expected, strListAssemblyName, strHashSetAssemblyName);

            Assert.AreEqual(expected, generated.ToString());
        }

        [TestMethod]
        public void WriteNestedGenericWithTypeArgumentsContainingShadowedNamespaces()
        {
            XamlSchemaContext xsc = new XamlSchemaContext();
            var generated = new StringBuilder();
            var xmlSettings = new XmlWriterSettings { Indent = true, OmitXmlDeclaration = true };

            var hashSetAssemblyName = typeof(HashSet<List<BigContainer>>).GetAssemblyName();
            var dictionaryAssemblyName = typeof(Dictionary<int, HashSet<List<BigContainer>>>).GetAssemblyName();
            var listAssemblyName = typeof(List<Dictionary<int, HashSet<List<BigContainer>>>>).GetAssemblyName();

            using (XamlXmlWriter writer = new XamlXmlWriter(XmlWriter.Create(new StringWriter(generated), xmlSettings), xsc))
            {
                XamlType bc = xsc.GetXamlType(typeof(BigContainer));
                string xns = "clr-namespace:Test.Elements;assembly=XamlTestClasses";
                writer.WriteNamespace(new NamespaceDeclaration(xns, "a"));
                writer.WriteStartObject(bc);

                var ns1 = $"clr-namespace:System.Collections.Generic;assembly={hashSetAssemblyName}";
                var ns3 = "http://schemas.microsoft.com/winfx/2006/xaml";
                writer.WriteNamespace(new NamespaceDeclaration(ns1, "a"));
                writer.WriteNamespace(new NamespaceDeclaration(ns3, "c"));
                XamlType mc = xsc.GetXamlType(typeof(MediumContainer));
                XamlMember thing = mc.GetAttachableMember("Thing");
                writer.WriteStartMember(thing);
                XamlType list = xsc.GetXamlType(typeof(List<Dictionary<int, HashSet<List<BigContainer>>>>));
                writer.WriteStartObject(list);
                writer.WriteEndObject();
                writer.WriteEndMember();
                writer.WriteEndObject();
            }

            var expected = @"<a:BigContainer xmlns:a=""clr-namespace:Test.Elements;assembly=XamlTestClasses"">
  <MediumContainer.Thing xmlns=""clr-namespace:Test.Elements.AnotherNamespace;assembly=XamlTestClasses"" xmlns:a=""clr-namespace:System.Collections.Generic;assembly={0}"" xmlns:c=""http://schemas.microsoft.com/winfx/2006/xaml"">
    <scg:List xmlns:scg=""clr-namespace:System.Collections.Generic;assembly={1}"">
      <c:TypeArguments xmlns:te=""clr-namespace:Test.Elements;assembly=XamlTestClasses"">scg:Dictionary(c:Int32, a:HashSet(scg:List(te:BigContainer)))</c:TypeArguments>
    </scg:List>
  </MediumContainer.Thing>
</a:BigContainer>";
            expected = string.Format(expected, hashSetAssemblyName, dictionaryAssemblyName);

            Assert.AreEqual(expected, generated.ToString());
        }

        [TestMethod]
        public void WriteNestedGenericComplex()
        {
            XamlSchemaContext xsc = new XamlSchemaContext();
            var generated = new StringBuilder();
            var xmlSettings = new XmlWriterSettings { Indent = true, OmitXmlDeclaration = true };

            var hashSetAssemblyName = typeof(HashSet<List<BigContainer>>).GetAssemblyName();
            var listAssemblyName = typeof(List<Dictionary<int, HashSet<List<BigContainer>>>>).GetAssemblyName();

            using (XamlXmlWriter writer = new XamlXmlWriter(XmlWriter.Create(new StringWriter(generated), xmlSettings), xsc))
            {
                XamlType list = xsc.GetXamlType(typeof(List<Dictionary<int, HashSet<List<BigContainer>>>>));
                var ns1 = $"clr-namespace:System.Collections.Generic;assembly={hashSetAssemblyName}";
                var ns2 = "clr-namespace:Test.Elements;assembly=XamlTestClasses";
                var ns3 = "http://schemas.microsoft.com/winfx/2006/xaml";
                writer.WriteNamespace(new NamespaceDeclaration(ns1, "a"));
                writer.WriteNamespace(new NamespaceDeclaration(ns2, "b"));
                writer.WriteNamespace(new NamespaceDeclaration(ns3, "c"));
                writer.WriteStartObject(list);
                XamlType bc = xsc.GetXamlType(typeof(BigContainer));
                writer.WriteStartMember(bc.GetMember("Integer"));
                writer.WriteValue("123");
                writer.WriteEndMember();
                writer.WriteEndObject();
            }

            var expected = @"<List c:TypeArguments=""Dictionary(c:Int32, a:HashSet(List(b:BigContainer)))"" Integer=""123"" xmlns=""clr-namespace:System.Collections.Generic;assembly={0}"" xmlns:a=""clr-namespace:System.Collections.Generic;assembly={1}"" xmlns:b=""clr-namespace:Test.Elements;assembly=XamlTestClasses"" xmlns:c=""http://schemas.microsoft.com/winfx/2006/xaml"" />";
            expected = string.Format(expected, listAssemblyName, hashSetAssemblyName);

            Assert.AreEqual(expected, generated.ToString());
        }

        [TestMethod]
        void ChoosePreviousDefinedNamespace()
        {
            XamlSchemaContext xsc = new XamlSchemaContext();
            var generated = new StringBuilder();
            var xmlSettings = new XmlWriterSettings { Indent = true, OmitXmlDeclaration = true };

            using (XamlXmlWriter writer = new XamlXmlWriter(XmlWriter.Create(new StringWriter(generated), xmlSettings), xsc))
            {
                string ns = "clr-namespace:Test.Elements.OneMoreNamespace;assembly=XamlTestClasses";
                writer.WriteNamespace(new NamespaceDeclaration(ns, "te"));

                XamlType omc = xsc.GetXamlType(typeof(OneMoreContainer));
                Assert.AreEqual(ns, omc.GetXamlNamespaces()[2]);
                writer.WriteStartObject(omc);
                writer.WriteEndObject();
            }

            var expected = @"<te:OneMoreContainer xmlns:te=""clr-namespace:Test.Elements.OneMoreNamespace;assembly=XamlTestClasses"" />";

            Assert.AreEqual(expected, generated.ToString());            
        }

        [TestMethod]
        void ChoosePreviousDefinedNamespaceComplex()
        {
            XamlSchemaContext xsc = new XamlSchemaContext();
            var generated = new StringBuilder();
            var xmlSettings = new XmlWriterSettings { Indent = true, OmitXmlDeclaration = true, CloseOutput = true };

            using (XamlXmlWriter writer = new XamlXmlWriter(XmlWriter.Create(new StringWriter(generated), xmlSettings), xsc))
            {
                XamlType omc = xsc.GetXamlType(typeof(OneMoreContainer));
                writer.WriteStartObject(omc);
                string ns = "clr-namespace:Test.Elements.OneMoreNamespace;assembly=XamlTestClasses";
                writer.WriteNamespace(new NamespaceDeclaration(ns, "teom"));
                writer.WriteStartMember(omc.GetMember("Integer"));
                writer.WriteStartObject(xsc.GetXamlType(typeof(OneMoreContainer)));
                writer.WriteEndObject();
                writer.WriteEndMember();
                writer.WriteEndObject();
            }

            var expected = @"<OneMoreContainer xmlns=""http://bar"">
  <teom:OneMoreContainer.Integer xmlns:teom=""clr-namespace:Test.Elements.OneMoreNamespace;assembly=XamlTestClasses"">
    <teom:OneMoreContainer />
  </teom:OneMoreContainer.Integer>
</OneMoreContainer>";

            Assert.AreEqual(expected, generated.ToString());
        }

        [TestMethod]
        void WriteOutOfScopePrefixForDifferentNamespace()
        {
            XamlSchemaContext xsc = new XamlSchemaContext();
            var generated = new StringBuilder();
            var xmlSettings = new XmlWriterSettings { Indent = true, OmitXmlDeclaration = true, CloseOutput = true };

            using (XamlXmlWriter writer = new XamlXmlWriter(XmlWriter.Create(new StringWriter(generated), xmlSettings), xsc))
            {
                XamlType bc = xsc.GetXamlType(typeof(BigContainer));
                writer.WriteStartObject(bc);                
                writer.WriteStartMember(bc.GetMember("Integer"));
                writer.WriteStartObject(xsc.GetXamlType(typeof(Test.Elements.AnotherNamespace.MediumContainer)));
                writer.WriteEndObject();
                writer.WriteStartObject(xsc.GetXamlType(typeof(Test.Elements.ANamespace.DummyContainer)));
                writer.WriteEndObject();
                writer.WriteEndMember();
                writer.WriteEndObject();
            }

            var expected = @"<BigContainer xmlns=""clr-namespace:Test.Elements;assembly=XamlTestClasses"">
  <BigContainer.Integer>
    <tea:MediumContainer xmlns:tea=""clr-namespace:Test.Elements.AnotherNamespace;assembly=XamlTestClasses"" />
    <tea1:DummyContainer xmlns:tea1=""clr-namespace:Test.Elements.ANamespace;assembly=XamlTestClasses"" />
  </BigContainer.Integer>
</BigContainer>";

            Assert.AreEqual(expected, generated.ToString());
        }

        [TestMethod]
        void WriteOutOfScopePrefixForSameNamespace()
        {
            XamlSchemaContext xsc = new XamlSchemaContext();
            var generated = new StringBuilder();
            var xmlSettings = new XmlWriterSettings { Indent = true, OmitXmlDeclaration = true, CloseOutput = true };

            using (XamlXmlWriter writer = new XamlXmlWriter(XmlWriter.Create(new StringWriter(generated), xmlSettings), xsc))
            {
                XamlType bc = xsc.GetXamlType(typeof(BigContainer));
                writer.WriteStartObject(bc);
                writer.WriteStartMember(bc.GetMember("Integer"));
                writer.WriteStartObject(xsc.GetXamlType(typeof(MediumContainer)));
                writer.WriteEndObject();
                writer.WriteStartObject(xsc.GetXamlType(typeof(MediumContainer)));
                writer.WriteEndObject();
                writer.WriteEndMember();
                writer.WriteEndObject();
            }

            var expected = @"<BigContainer xmlns=""clr-namespace:Test.Elements;assembly=XamlTestClasses"">
  <BigContainer.Integer>
    <tea:MediumContainer xmlns:tea=""clr-namespace:Test.Elements.AnotherNamespace;assembly=XamlTestClasses"" />
    <tea:MediumContainer xmlns:tea=""clr-namespace:Test.Elements.AnotherNamespace;assembly=XamlTestClasses"" />
  </BigContainer.Integer>
</BigContainer>";

            Assert.AreEqual(expected, generated.ToString());
        }

        [TestMethod]
        void WriteShadowedPrefix()
        {
            XamlSchemaContext xsc = new XamlSchemaContext();
            var generated = new StringBuilder();
            var xmlSettings = new XmlWriterSettings { Indent = true, OmitXmlDeclaration = true, CloseOutput = true };

            using (XamlXmlWriter writer = new XamlXmlWriter(XmlWriter.Create(new StringWriter(generated), xmlSettings), xsc))
            {
                XamlType bc = xsc.GetXamlType(typeof(BigContainer));
                writer.WriteStartObject(bc);
                string ns = "http://foo";
                writer.WriteNamespace(new NamespaceDeclaration(ns, ""));
                writer.WriteStartMember(bc.GetMember("Integer"));
                writer.WriteStartObject(bc);
                writer.WriteEndObject();
                writer.WriteEndMember();
                writer.WriteStartMember(bc.GetMember("Double"));
                writer.WriteValue("2");
                writer.WriteEndMember();
                writer.WriteEndObject();
            }

            var expected = @"<BigContainer xmlns=""clr-namespace:Test.Elements;assembly=XamlTestClasses"">
  <te:BigContainer.Integer xmlns=""http://foo"" xmlns:te=""clr-namespace:Test.Elements;assembly=XamlTestClasses"">
    <te:BigContainer />
  </te:BigContainer.Integer>
  <BigContainer.Double>2</BigContainer.Double>
</BigContainer>";

            Assert.AreEqual(expected, generated.ToString());
        }

        [TestMethod]
        void FlushAtTheEnd()
        {
            XamlObjectReader reader = new XamlObjectReader(new SmallContainer());
            StringWriter stringWriter = new StringWriter();
            XamlXmlWriter writer = new XamlXmlWriter(stringWriter, reader.SchemaContext);

            while (reader.Read())
            {
                writer.WriteNode(reader);
            }

            // Users shouldn't have to call Flush() here
            // writer.Flush();

            var generated = stringWriter.ToString();
            var expected = @"<?xml version=""1.0"" encoding=""utf-16""?><SmallContainer Chars=""{x:Null}"" Integer=""0"" xmlns=""clr-namespace:Test.Elements;assembly=XamlTestClasses"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""><x:Null /></SmallContainer>";
            Assert.AreEqual(expected, generated);
        }     

        [TestMethod]
        void WhiteSpaceSignificantCollectionReadWriteCollection()
        {
            string xaml = @"
<WhiteSpaceSignificantCollectionWrapper xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses'>
  <WhiteSpaceSignificantCollectionWrapper.Collection>
    <WhitespaceSignificantCollectionType>
      <Element /> <Element />
<Element />
    </WhitespaceSignificantCollectionType>
  </WhiteSpaceSignificantCollectionWrapper.Collection>
</WhiteSpaceSignificantCollectionWrapper>
";
            
            XamlXmlReader xxr = new XamlXmlReader(XmlReader.Create(new StringReader(xaml)), new XamlXmlReaderSettings() { ValuesMustBeString = true });
            TextWriter t = new StringWriter(CultureInfo.InvariantCulture);
            var xmlSettings = new XmlWriterSettings { Indent = true, OmitXmlDeclaration = true, CloseOutput = true };
            XamlXmlWriter xxw = new XamlXmlWriter(XmlWriter.Create(t, xmlSettings), xxr.SchemaContext);
            XamlServices.Transform(xxr, xxw);

            string expectedXaml = @"<WhiteSpaceSignificantCollectionWrapper xmlns=""clr-namespace:Test.Elements;assembly=XamlTestClasses"">
  <WhiteSpaceSignificantCollectionWrapper.Collection>
    <WhitespaceSignificantCollectionType>
      <Element /> <Element /> <Element /></WhitespaceSignificantCollectionType>
  </WhiteSpaceSignificantCollectionWrapper.Collection>
</WhiteSpaceSignificantCollectionWrapper>";
            Assert.AreEqual(t.ToString(), expectedXaml);
        }

        [TestMethod]
        public void RoundTripWithXmlWriterIndents()
        {
            #region Test Data

            const string source_xaml =
@"<Section xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"">
    <Paragraph>
        <Span>
            <Run Text=""Test""/>
        </Span>
        <Run Text="" ""/>
    </Paragraph>
    <Paragraph><Span><Run Text=""Test""/></Span><Run Text=""NoSpaceHere""/></Paragraph>
</Section>";

            // Different XAML's are generated after a roundtrip. The resultant XAML depends on the value of 
            // XmlWriterSettings.Indent supplied to XmlWriter.Create.
            //
            // These expected outcomes are maintained in the following dictionary. 
            Dictionary<bool, string> expected_results = new Dictionary<bool, string>()
            {
                {
                    /*XmlWriterSettings.Indent*/true,
@"<?xml version=""1.0"" encoding=""utf-16""?>
<Section xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"">
  <Paragraph xml:space=""preserve""><Span>Test</Span> <Run> </Run></Paragraph>
  <Paragraph>
    <Span>Test</Span>NoSpaceHere</Paragraph>
</Section>"     },

                {
                    /*XmlWriterSettings.Indent*/false,
                    @"<?xml version=""1.0"" encoding=""utf-16""?><Section xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""><Paragraph xml:space=""preserve""><Span>Test</Span> <Run> </Run></Paragraph><Paragraph><Span>Test</Span>NoSpaceHere</Paragraph></Section>"
                }
            };


            #endregion

            Section section = (Section)XamlServices.Parse(source_xaml);

            foreach (var expected_result in expected_results)
            {
                var builder = new StringBuilder();
                var xmlWriter = XmlWriter.Create(builder, new XmlWriterSettings { Indent = expected_result.Key });

                System.Windows.Markup.XamlWriter.Save(section, xmlWriter);

                string roundtripped_xaml = builder.ToString();

                Assert.IsTrue(expected_result.Value == roundtripped_xaml, "Indented XAML is not logically identical to the source XAML");
            }
        }

        public void UntypedScenariosTest1()
        {
            string originalXaml = @"<UnknownType xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml' xmlns:u='clr-namespace:LocalNamespace'>
  <UnknownType.Property1>
    <u:UnknownType2 />
  </UnknownType.Property1>
</UnknownType>";
            string generatedXaml = WriteToString(originalXaml);
            string expectedXaml = @"<UnknownType xmlns=""clr-namespace:LocalNamespace"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
  <UnknownType.Property1>
    <UnknownType2 />
  </UnknownType.Property1>
</UnknownType>";
            Assert.AreEqual(generatedXaml, expectedXaml);
        }

        public void UntypedScenariosTest2()
        {
            string originalXaml = @"<List x:TypeArguments=""u:UnknownType"" xmlns=""clr-namespace:System.Collections.Generic;assembly={0}"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"" xmlns:u=""clr-namespace:LocalNamespace"">
  <x:String>Whatever</x:String>
  <u:NotAString>Hello</u:NotAString>
  <x:String>Another thing</x:String>
  <x:Null />
</List>";
            originalXaml = string.Format(originalXaml, typeof(List<>).GetAssemblyName());

            string generatedXaml = WriteToString(originalXaml);
            string expectedXaml = @"<List x:TypeArguments=""u:UnknownType"" xmlns=""clr-namespace:System.Collections.Generic;assembly={0}"" xmlns:u=""clr-namespace:LocalNamespace"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
  <x:String>Whatever</x:String>
  <u:NotAString>Hello</u:NotAString>
  <x:String>Another thing</x:String>
  <x:Null />
</List>";
            expectedXaml = string.Format(expectedXaml, typeof(List<>).GetAssemblyName());

            Assert.AreEqual(generatedXaml, expectedXaml);
        }

        public void UntypedScenariosTest3()
        {
            string originalXaml = @"
<BigContainer Chars=""{x:Null}"" ListOfInts=""{x:Null}"" Double=""0"" Integer=""0"" xmlns=""clr-namespace:Test.Elements;assembly=XamlTestClasses"" xmlns:scg=""clr-namespace:System.Collections.Generic;assembly={0}"" xmlns:tea=""clr-namespace:Test.Elements.AnotherNamespace;assembly=XamlTestClasses"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"" xmlns:u=""clr-namespace:UnknownNS;assembly=LocalAssembly"">
  <BigContainer.SmallContainer>
    <u:UnknownElement />
  </BigContainer.SmallContainer>
  <BigContainer.MediumContainer>
    <tea:UnknownMediumContainer Integer=""20"" />
  </BigContainer.MediumContainer>
  <BigContainer.Obj>
    <scg:Dictionary x:TypeArguments=""x:String, Element"">
      <Element x:Key=""SomeKey"" />
      <AnotherUnknownElement><x:Key><UnknownKey /></x:Key></AnotherUnknownElement>
      <Element x:Key=""SomeKey2"" />
    </scg:Dictionary>
  </BigContainer.Obj>
  <x:Null />
</BigContainer>
";
            originalXaml = string.Format(originalXaml, typeof(Dictionary<string, Element>).GetAssemblyName());

            string generatedXaml = WriteToString(originalXaml);
            string expectedXaml = @"
<BigContainer Integer=""0"" Chars=""{x:Null}"" ListOfInts=""{x:Null}"" Double=""0"" xmlns=""clr-namespace:Test.Elements;assembly=XamlTestClasses"" xmlns:scg=""clr-namespace:System.Collections.Generic;assembly={0}"" xmlns:tea=""clr-namespace:Test.Elements.AnotherNamespace;assembly=XamlTestClasses"" xmlns:u=""clr-namespace:UnknownNS;assembly=LocalAssembly"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
  <BigContainer.SmallContainer>
    <u:UnknownElement />
  </BigContainer.SmallContainer>
  <BigContainer.MediumContainer>
    <tea:UnknownMediumContainer Integer=""20"" />
  </BigContainer.MediumContainer>
  <BigContainer.Obj>
    <scg:Dictionary x:TypeArguments=""x:String, Element"">
      <Element x:Key=""SomeKey"" />
      <AnotherUnknownElement>
        <x:Key>
          <UnknownKey />
        </x:Key>
      </AnotherUnknownElement>
      <Element x:Key=""SomeKey2"" />
    </scg:Dictionary>
  </BigContainer.Obj>
  <x:Null />
</BigContainer>
";
            expectedXaml = string.Format(expectedXaml, typeof(Dictionary<string, Element>).GetAssemblyName());

            Assert.AreEqual(generatedXaml, expectedXaml);
        }

        public void UntypedScenariosTest4()
        {
            string originalXaml = @"<List x:TypeArguments=""Dictionary(x:Int32, u:UnknownOuterType(u:UnknownNestedType))"" xmlns=""clr-namespace:System.Collections.Generic;assembly={0}"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"" xmlns:u=""clr-namespace:LocalNamespace"">
</List>";
            originalXaml = string.Format(originalXaml, typeof(Dictionary<int,int>).GetAssemblyName());

            string generatedXaml = WriteToString(originalXaml);
            string expectedXaml = @"<List x:TypeArguments=""Dictionary(x:Int32, u:UnknownOuterType(u:UnknownNestedType))"" xmlns=""clr-namespace:System.Collections.Generic;assembly={0}"" xmlns:u=""clr-namespace:LocalNamespace"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"" />";
            expectedXaml = string.Format(expectedXaml, typeof(Dictionary<int, int>).GetAssemblyName());

            Assert.AreEqual(generatedXaml, expectedXaml);
        }

        public void UntypedScenariosWithWhitespace1()
        {
            string originalXaml = @"
<BigContainer xmlns=""clr-namespace:Test.Elements;assembly=XamlTestClasses"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
  <BigContainer.UnknownContainer>
    <x:Object />
    <x:Object />
  </BigContainer.UnknownContainer>
</BigContainer>";
            string generatedXaml = WriteToString(originalXaml);
            string expectedXaml = @"<BigContainer xmlns=""clr-namespace:Test.Elements;assembly=XamlTestClasses"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
  <BigContainer.UnknownContainer>
    <x:Object /> <x:Object /></BigContainer.UnknownContainer>
</BigContainer>";
            Assert.AreEqual(generatedXaml, expectedXaml);
        }

        public void UntypedScenariosWithWhitespace2()
        {
            string originalXaml = @"<Window
        xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'
        xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
    <RobPanel>
        <Button>Button1</Button>
        <Button>Button2</Button>
    </RobPanel>
</Window>";
            string generatedXaml = WriteToString(originalXaml);
            string expectedXaml = @"<Window xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
  <RobPanel>
    <Button>Button1</Button> <Button>Button2</Button></RobPanel>
</Window>";
            Assert.AreEqual(generatedXaml, expectedXaml);
        }

        public string WriteToString(string xaml)
        {
            XamlXmlReader xxr = new XamlXmlReader(XmlReader.Create(new StringReader(xaml)), new XamlXmlReaderSettings() { ValuesMustBeString = true });
            TextWriter t = new StringWriter(CultureInfo.InvariantCulture);
            var xmlSettings = new XmlWriterSettings { Indent = true, OmitXmlDeclaration = true, CloseOutput = true };
            XamlXmlWriter xxw = new XamlXmlWriter(XmlWriter.Create(t, xmlSettings), xxr.SchemaContext);
            XamlServices.Transform(xxr, xxw);
            return t.ToString();
        }

        [TypeConverter(typeof(ManufacturerTypeConverter))]
        class Manufacturer
        {
            public string Name
            { get; set; }
            class ManufacturerTypeConverter : TypeConverter
            {
                public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
                {
                    return destinationType == typeof(string);
                }
                public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
                {
                    return ((Manufacturer)value).Name;
                }
            }
        }

        class Driver
        {
            public string Name
            { get; set; }

            public override string ToString()
            {
                return Name;
            }
        }
    }
}
