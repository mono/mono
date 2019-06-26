using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Test_NUnit.Internals
{
    [TestFixture]
    public class ReflectionTest
    {
        //there was a bug where intField1 would be listed multiple times in a select statement:
        public class Class1 
        { 
            protected int intField1;
            public int publicField;
        }
        public class Class2 : Class1 
        { 
            protected int intField2; 
        }
#if OBSOLETE
        [Test]
        public void AttribHelper_NoDuplicateFields()
        {
            //Andrus pointed out that one of the internal classes that help with reflection
            //returns fields in duplicate, which kills SQL SELECT and UPDATEs.
            System.Reflection.MemberInfo[] members = DbLinq.Util.AttribHelper.GetMemberFields(typeof(Class2));
            Assert.IsTrue(members.Length == 3);
        }

        [Test]
        public void AttribHelper_IncludePublicFields()
        {
            //Andrus pointed out that one of the internal classes that help with reflection
            //returns fields in duplicate, which kills SQL SELECT and UPDATEs.
            System.Reflection.MemberInfo[] members = DbLinq.Util.AttribHelper.GetMemberFields(typeof(Class1));
            Assert.IsTrue(members.Length == 2);
        }
#endif

    }
}
