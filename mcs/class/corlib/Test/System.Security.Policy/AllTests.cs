//
// MonoTests.System.Security.Policy.AllTests.cs
//
// Author:
//   Nick Drochak (ndrochak@gol.com)
//
// (C) 2001 Nick Drochak II
//

using System;
using NUnit.Framework;

namespace MonoTests.System.Security.Policy {
        public class AllTests : TestCase {
                public AllTests(string name) : base(name) {}
                
                public static ITest Suite 
                { 
                        get 
                        {
                                TestSuite suite = new TestSuite();
                                suite.AddTest (CodeGroupTest.Suite);
				suite.AddTest (EvidenceTest.Suite);
                                suite.AddTest (PublisherTest.Suite);
				suite.AddTest (PublisherMembershipConditionTest.Suite);
				suite.AddTest (StrongNameTest.Suite);
                                return suite;
                        }
                }
        }
}
