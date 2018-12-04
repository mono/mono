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
    sealed class EventTests : XamlTestSuite
    {
        public EventTests()
            : base("EventTests")
        {
        }

        public override DrtTest[] PrepareTests()
        {
            DrtTest[] tests = DrtTestFinder.FindTests(this);
            return tests;
        }

        private bool eventHandled = false;

        public override object StandardXamlLoader(string xamlString)
        {
            eventHandled = false;
            var xamlXmlReader = new XamlXmlReader(XmlReader.Create(new StringReader(xamlString)));
            XamlNodeList xamlNodeList = new XamlNodeList(xamlXmlReader.SchemaContext);
            XamlServices.Transform(xamlXmlReader, xamlNodeList.Writer);

            XamlReader reader = xamlNodeList.GetReader();
            XamlObjectWriterSettings settings = new XamlObjectWriterSettings() { RootObjectInstance = null, AfterBeginInitHandler = ObjectCreated };
            XamlObjectWriter objWriter = new XamlObjectWriter(reader.SchemaContext, settings);
            XamlServices.Transform(reader, objWriter);
            object root = objWriter.Result;
            if (root == null)
                throw new NullReferenceException("Load returned null Root");
            return root;
        }

        // =============================================

        [TestXaml, TestTreeValidator("VerifyObjectCreatedEvent")]
        const string ChainedSavedContext_XAML =
@"<EventElement Foo='Hello'
    xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses'/>";

        private void ObjectCreated(object sender, XamlObjectEventArgs e)
        {
            // Make sure event is fired before any properties are set
            eventHandled = ((EventElement)e.Instance).Foo == null;
        }

        public void VerifyObjectCreatedEvent(object o)
        {
            if (!eventHandled)
                throw new Exception("Object Created Event call expected");
        }
    }
}
