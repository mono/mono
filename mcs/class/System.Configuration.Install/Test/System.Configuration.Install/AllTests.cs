//
// MonoTests.System.Configuration.Install.AllTests
//
// Authors:
// 	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc.  (http://www.ximian.com)
// 

using System;
using NUnit.Framework;

namespace MonoTests.System.Configuration.Install {
        public class AllTests : TestCase {

                public AllTests (string name) : base (name) {}
                
                public static ITest Suite { 
                        get 
                        {
                                TestSuite suite = new TestSuite();
				return suite;
                        }
                }
        }
}

