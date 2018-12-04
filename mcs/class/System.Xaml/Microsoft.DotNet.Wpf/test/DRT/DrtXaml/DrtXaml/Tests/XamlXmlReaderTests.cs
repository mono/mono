using System;
using System.Xaml;
using DRT;
using DrtXaml.XamlTestFramework;
using System.IO;
using System.Text;
using System.Xml;

namespace DrtXaml.Tests
{
    [TestClass]
    sealed class XamlXmlReaderTests : XamlTestSuite
    {
        public XamlXmlReaderTests() : base("XamlXmlReaderTests")
        {
        }

        public override DrtTest[] PrepareTests()
        {
            DrtTest[] tests = DrtTestFinder.FindTests(this);
            return tests;
        }

        [TestXaml, TestExpectedException(typeof(XamlParseException))]
        const string FuzzedPropertyElementRoot = "<Style.Setters>";

        const string RobPanel = @"<Window
        xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'
        xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
    <RobPanel>
        <Button>Button1</Button>
        <Button>Button2</Button>
    </RobPanel>
</Window>";
        [TestMethod]
        public void UnknownContentWS()
        {
            XamlXmlReader xxr = new XamlXmlReader(new MemoryStream(ASCIIEncoding.Default.GetBytes(RobPanel)));
            bool containsWS = false;
            while (xxr.Read())
            {
                if ((xxr.NodeType == XamlNodeType.Value) && ((string)xxr.Value == " "))
                {
                    containsWS = true;
                }
            }
            if (!containsWS)
                throw new Exception("Whitespace expected in unknown content");
        }

        [TestMethod]
        public void XmlNodeReaderTest()
        {
            string url = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";
            XmlElement parentElem = new XmlDocument().CreateElement(null, "FlowDocument", url);
            parentElem.SetAttribute("xml:space", "preserve");
            XmlNodeReader xr = new XmlNodeReader(parentElem);
            object o = System.Windows.Markup.XamlReader.Load(xr);
        }

        [TestMethod]
        public void XmlNodeReaderPropertyElementTest()
        {
            string pf = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";
            XmlDocument doc = new XmlDocument();
            XmlElement canvas = doc.CreateElement(null, @"Canvas", pf);
            doc.AppendChild(canvas);
            XmlElement button = doc.CreateElement(null, @"Button", pf);
            canvas.AppendChild(button);
            XmlElement left = doc.CreateElement(null, @"Canvas.Left", pf);
            button.AppendChild(left);
            left.AppendChild(doc.CreateTextNode("5"));
            XmlReader reader = new XmlNodeReader(doc);
            object o = System.Windows.Markup.XamlReader.Load(reader);
        }
    }
}