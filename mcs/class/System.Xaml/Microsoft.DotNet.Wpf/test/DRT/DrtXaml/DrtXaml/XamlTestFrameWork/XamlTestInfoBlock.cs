using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DrtXaml.XamlTestFramework
{
    public class XamlTestInfoBlock
    {
        private string _name;
        private SimpleTest _test;
        private string _xaml;
        private XamlStringParser _loader;
        private PostTreeValidator _validator;
        private Type _expectedExceptionType, _expectedInnerExceptionType;

        public XamlTestInfoBlock(string name, SimpleTest test, Type expectedExceptionType) : this(name, test, expectedExceptionType, null)
        {
        }

        public XamlTestInfoBlock(string name, SimpleTest test, Type expectedExceptionType, Type expectedInnerExceptionType)
        {
            _name = name;
            _test = test;
            _expectedExceptionType = expectedExceptionType;
            _expectedInnerExceptionType = expectedInnerExceptionType;
        }

        public XamlTestInfoBlock(string name, string xaml, XamlStringParser loader, PostTreeValidator validator, Type expectedExceptionType)
            : this(name, xaml, loader, validator, expectedExceptionType, null)
        {
        }

        public XamlTestInfoBlock(string name, string xaml, XamlStringParser loader, PostTreeValidator validator, Type expectedExceptionType, Type expectedInnerExceptionType)
        {
            _name = name;
            _xaml = xaml;
            _loader = loader;
            _validator = validator;
            _expectedExceptionType = expectedExceptionType;
            _expectedInnerExceptionType = expectedInnerExceptionType;
        }

        public XamlTestInfoBlock(string name, string xaml, XamlStringParser loader, PostTreeValidator validator) :
            this(name, xaml, loader, validator, null) { }

        public string Name { get { return _name; } }
        public SimpleTest TestDelegate { get{ return _test; } }
        public string XamlString { get { return _xaml; } }
        public XamlStringParser StringParserDelegate { get { return _loader; } }
        public PostTreeValidator TreeValidatorDelegate { get { return _validator; } }
        public Type ExpectedExceptionType { get { return _expectedExceptionType; } }
        public Type ExpectedInnerExceptionType { get { return _expectedInnerExceptionType; } }
        public bool IsTestKnownFailure { get; set; }
        public string OwnerName { get; set; }

        public override string ToString()
        {
            return Name;
        }

        public void RunTest()
        {
            if (TestDelegate != null)
            {
                RunMethodTest();
            }
            else
            {
                RunStringTest();
            }
        }

        private void RunMethodTest()
        {
            SimpleTest test = TestDelegate;

            if (ExpectedExceptionType == null)
            {
                test();
            }
            // otherwise some sort of exception is expected
            else
            {
                try
                {
                    test();
                }
                catch (Exception e)
                {
                    if (ExpectedExceptionType == e.GetType())
                    {
                        // TODO: Bug 736396
                        //if((ExpectedInnerExceptionType == null && e.InnerException == null) || (ExpectedInnerExceptionType == e.InnerException.GetType()))
                            return;
                    }
                    throw new InvalidOperationException("Wrong Exception was thrown", e);
                }
                throw new InvalidOperationException(String.Format("Expected exception {0} was not thrown", ExpectedExceptionType.ToString()));
            }
        }

        private void RunStringTest()
        {
            // If it is a String Test then load the string.
            string xamlString = XamlString;
            XamlStringParser loader = StringParserDelegate;
            PostTreeValidator validator = TreeValidatorDelegate;

            if (ExpectedExceptionType == null)
            {
                LoadAndValidate(loader, xamlString, validator);
            }
            // otherwise some sort of exception is expected
            else
            {
                try
                {
                    LoadAndValidate(loader, xamlString, validator);
                }
                catch (Exception e)
                {
                    if (ExpectedExceptionType == e.GetType())
                    {
                        if((ExpectedInnerExceptionType == null && e.InnerException == null) || (ExpectedInnerExceptionType == e.InnerException.GetType()))
                            return;
                    }
                    throw new InvalidOperationException("Wrong Exception was thrown", e);
                }
                throw new InvalidOperationException(String.Format("Expected exception {0} was not thrown", ExpectedExceptionType.ToString()));
            }
        }

        private void LoadAndValidate(XamlStringParser loader, string xamlString, PostTreeValidator validator)
        {
            object root = loader(xamlString);
            if (validator != null)
            {
                validator(root);
            }
        }
    }
}
