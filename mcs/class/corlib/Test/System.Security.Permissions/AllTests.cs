//
// MonoTests.System.Security.Permissions.AllTests.cs
//
// Author:
//   Nick Drochak (ndrochak@gol.com)
//
// (C) 2001 Nick Drochak II
//

using System;
using NUnit.Framework;

namespace MonoTests.System.Security.Permissions {
        public class AllTests : TestCase {
                public AllTests(string name) : base(name) {}
                
                public static ITest Suite 
                { 
                        get 
                        {
                                TestSuite suite =  new TestSuite();
                                suite.AddTest (FileIOPermissionTest.Suite);
                                suite.AddTest (StrongNamePublicKeyBlobTest.Suite);
                                return suite;
                        }
                }
        }
}
