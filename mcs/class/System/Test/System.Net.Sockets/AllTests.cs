//
// MonoTests.System.Net.Sockets.AllTests, System.dll
//
// Author:
//   Lawrence Pit <loz@cable.a2000.nl>
//

using System;
using NUnit.Framework;

namespace MonoTests.System.Net.Sockets {
        /// <summary>
        ///   Combines all available unit tests into one test suite.
        /// </summary>
        public class AllTests : TestCase {

                public AllTests (string name) : base (name) {}
                
                public static ITest Suite { 
                        get 
                        {
                                TestSuite suite = new TestSuite ();
				#if NETWORKTEST                                                            
					suite.AddTest (TcpListenerTest.Suite);
					suite.AddTest (TcpClientTest.Suite);                                
					//suite.AddTest (UdpClientTest.Suite);
				#endif                                
				return suite;
                        }
                }
        }
}

