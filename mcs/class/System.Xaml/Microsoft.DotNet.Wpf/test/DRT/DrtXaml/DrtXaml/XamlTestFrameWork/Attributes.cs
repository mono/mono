using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DrtXaml.XamlTestFramework
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class TestClassAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Method)]
    public sealed class TestMethodAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Field)]
    public sealed class TestXamlAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Field)]
    public sealed class TestDisabledAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class TestSetupAttribute : Attribute { }    

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Field)]
    public sealed class TestKnownFailureAttribute : Attribute
    {
        public String Reason { get; set; }

        public TestKnownFailureAttribute(string Reason)
        {
            this.Reason = Reason;
        }

        public TestKnownFailureAttribute() : this ("Known Failure") { }
    }

    public class TestXamlMethodAttribute : Attribute
    {
        private readonly string _methodName;

        public TestXamlMethodAttribute(string methodName)
        {
            _methodName = methodName;
        }
        public string MethodName { get { return _methodName; } }
    }

    // A TestSuite can provide a "standard" Xaml Loader method.
    // otherwise a default one lives in XamlTools.
    [AttributeUsage(AttributeTargets.Class, AllowMultiple=false, Inherited=true)]
    public sealed class TestStandardXamlLoaderAttribute : TestXamlMethodAttribute
    {
        public TestStandardXamlLoaderAttribute(string methodName) : base(methodName) { }
    }

    // Standard load of the XAML + given method to validate the tree.
    [AttributeUsage(AttributeTargets.Field, AllowMultiple=false)]
    public sealed class TestTreeValidatorAttribute : TestXamlMethodAttribute
    {
        public TestTreeValidatorAttribute(string methodName) : base(methodName) { }
    }

    // Alternate Method to load the XAML text.  Used for "MustFail" or other special loading.
    [AttributeUsage(AttributeTargets.Field, AllowMultiple=false)]
    public sealed class TestAlternateXamlLoaderAttribute : TestXamlMethodAttribute
    {
        public TestAlternateXamlLoaderAttribute(string methodName) : base(methodName) { }
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method, AllowMultiple=false)]
    public sealed class TestExpectedExceptionAttribute : Attribute
    {
        public Type ExpectedExceptionType { get; private set; }
        public Type ExpectedInnerExceptionType { get; private set; }
        public TestExpectedExceptionAttribute(Type outerException) : this(outerException, null)
        {

        }

        public TestExpectedExceptionAttribute(Type outerException, Type innerException)
        {
            ExpectedExceptionType = outerException;
            ExpectedInnerExceptionType = innerException;
        }
    }
}

