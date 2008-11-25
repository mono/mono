using System;
using System.Reflection;

namespace NUnit.Core.Builders
{
    public class MultiCultureDecorator : Extensibility.ITestDecorator
    {
        public Test Decorate(Test test, MemberInfo member)
        {
            Attribute attr = Reflect.GetAttribute( member, "NUnit.Framework.MultiCultureAttribute", true );
            if (attr == null) return test;
            
            string cultures = Reflect.GetPropertyValue( attr, "Cultures" ) as string;
            if ( cultures == null ) return test;

            return new MultiCultureTestSuite(test, cultures);
        }
    }
}
