
using System;
using System.Collections.Generic;
using System.Linq;

using DRT;
using System.Xaml;
using System.Text;
using System.IO;
using DrtXaml.XamlTestFramework;
using System.Xml;
using Xunit;
using Xunit.Abstractions;
using System.Threading.Tasks;
using System.Threading;

namespace DrtXaml
{
    public class TestFrontEnd
    {
        public TestFrontEnd(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [Theory]
        [InlineData("ValueSerializerSerializationTests")]
        [InlineData("ValueSerializerDeserializationAndRoundtripTest")]
        [InlineData("NameReferenceTests")]
        [InlineData("AdvXamlFeatureTests")]
        [InlineData("AttachablePropertyServicesTests")]
        [InlineData("AvoidSystemXmlTest")]
        [InlineData("BamlTests")]
        [InlineData("BasicXamlTests")]
        [InlineData("CollectionTests")]
        [InlineData("EventTests")]
        [InlineData("FactoryArgumentTests")]
        [InlineData("LoadPermissionTests")]
        [InlineData("MarkupExtensionTests")]
        [InlineData("NodeStreamTests")]
        [InlineData("ObjectReaderTests")]
        [InlineData("ObjectWriterBasicTests")]
        [InlineData("SavedContextTests")]
        [InlineData("SchemaTests")]
        [InlineData("SubreaderTests")]
        [InlineData("TypeArgumentTests")]
        [InlineData("WpfUsageTests")]
        [InlineData("XamlMarkupExtensionWriterTests")]
        [InlineData("XamlXmlWriterTests")]
        [InlineData("XamlXmlReaderTests")]
        [InlineData("XamlTemplateTests")]
        [InlineData("XamlServicesTests")]
        public async Task RunSuite(params string[] data)
        {
            string[] switches = new[] { "-verbose", "-catchexceptions", "-quietasserts" };

            var args = new List<string>(switches);
            foreach (var arg in data)
            {
                if (!arg.StartsWith("-"))
                {
                    args.Add("-suite");
                }
                args.Add(arg);
            }

            await StartSTATask(() => new XamlDrt(_testOutputHelper).RunTests(args.ToArray()));
        }

        private static Task StartSTATask(Action action)
        {
            var tcs = new TaskCompletionSource<object>();
            var thread = new Thread(() =>
            {
                try
                {
                    action();
                    tcs.SetResult(new object());
                }
                catch (Exception e)
                {
                    tcs.SetException(e);
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            return tcs.Task;
        }

        private ITestOutputHelper _testOutputHelper;
    }
    public sealed class XamlDrt : DrtBase
    {
        public void RunTests(params string[] args)
        {
            Run(args);
        }

        public XamlDrt(ITestOutputHelper output) : base(output)
        {
            DrtName = "DrtXaml";
            WindowTitle = "XAML DRT";

            Suites = DrtTestFinder.TestSuites;
        }

        protected override bool HandleCommandLineArgument(string arg, bool option, string[] args, ref int k)
        {
            // start by giving the base class the first chance
            if (base.HandleCommandLineArgument(arg, option, args, ref k))
                return true;

            // See the comment in DrtBase.cs for how to add
            // command line options to this code.

            return false;
        }

        protected override void PrintOptions()
        {
            Console.WriteLine("Currenly no XAML Drt command line options.");
            base.PrintOptions();
        }
    }


    [TestStandardXamlLoader("StandardXamlLoader")]
    public class XamlTestSuite : DrtTestSuite
    {
        public XamlTestSuite(string name) : base(name)
        {
        }

        public override DrtTest[] PrepareTests()
        {
            return new DrtTest[]{
                new DrtTest( BadTest ),
            };
        }

        void BadTest()
        {
            throw new NotImplementedException("XamlTestSuite PrepareTests must be overriden");
        }

        public virtual object StandardXamlLoader(string xamlString)
        {
            var xamlXmlReader = new XamlXmlReader(XmlReader.Create(new StringReader(xamlString)));
            XamlNodeList xamlNodeList = new XamlNodeList(xamlXmlReader.SchemaContext);
            XamlServices.Transform(xamlXmlReader, xamlNodeList.Writer);

            NodeListValidator.Validate(xamlNodeList);

            XamlReader reader = xamlNodeList.GetReader();
            XamlObjectWriter objWriter = new XamlObjectWriter(reader.SchemaContext);
            XamlServices.Transform(reader, objWriter);
            object root = objWriter.Result;
            if (root == null)
                throw new NullReferenceException("Load returned null Root");
            return root;
        }

        public object DRT_XamlLoader(string name, string xamlString,
                                    XamlStringParser loader,
                                    Type expectedExceptionType,
                                    PostTreeValidator validator)
        {
            object root = null;
            bool hasExpectedException = (expectedExceptionType != null);

            try
            {
                root = loader(xamlString);
            }
            catch (Exception ex)
            {
                if (expectedExceptionType != null && expectedExceptionType == ex.GetType())
                {
                    hasExpectedException = false;
                }
                else
                {
                    // otherwise we got the unexpected exception;
                    DRT.Assert(false, "XAML Test '{0}' failed.{1}", name, ex.ToString());
                    return null;
                }
            }

            if (hasExpectedException)
            {
                DRT.Assert(false, "XAAML Test '{0}' did not throw expected exception of type '{1}'.", name, expectedExceptionType);
            }

            if (validator != null)
            {
                try
                {
                    validator(root);
                }
                catch (Exception ex)
                {
                    DRT.Assert(false, "XAML Test String '{0}' failed post validation.{1}", name, ex.ToString());
                }
            }
            return root;
        }

        public void DRT_TestValidator(SimpleTest test, Type expectedExceptionType)
        {
            try
            {
                test();
            }
            catch (Exception ex)
            {
                if (expectedExceptionType != null && expectedExceptionType == ex.GetType())
                {
                    return;
                }
                else
                {
                    // otherwise we got an unexpected exception
                    DRT.Assert(false, "XAML Test '{0}' failed.{1}", test.Method.Name, ex.ToString());
                    return;
                }
            }

            if (expectedExceptionType != null)
            {
                DRT.Assert(false, "XAAML Test '{0}' did not throw expected exception of type '{1}'.", test.Method.Name, expectedExceptionType);
            }
        }

    }
}