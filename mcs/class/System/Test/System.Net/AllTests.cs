// Testsuite.System.AllSystemTests.cs
//
// Mario Martinez (mariom925@home.om)
//
// (C) Ximian, Inc.  http://www.ximian.com
// 

using System;
using NUnit.Framework;

namespace MonoTests.System.Net {
        /// <summary>
        ///   Combines all available unit tests into one test suite.
        /// </summary>
        public class AllTests : TestCase {

                public AllTests (string name) : base (name) {}
                
                public static ITest Suite { 
                        get 
                        {
                                TestSuite suite = new TestSuite ();
                                suite.AddTest (CookieTest.Suite);
                                suite.AddTest (CookieCollectionTest.Suite);
                                //suite.AddTest (CookieContainerTest.Suite);
                                suite.AddTest (CredentialCacheTest.Suite);
                                suite.AddTest (FileWebRequestTest.Suite);
                                suite.AddTest (IPAddressTest.Suite);
                                suite.AddTest (IPEndPointTest.Suite);
                                suite.AddTest (SocketPermissionTest.Suite);
                                suite.AddTest (WebHeaderCollectionTest.Suite);
                                suite.AddTest (WebProxyTest.Suite);
                                suite.AddTest (WebRequestTest.Suite);
                                
				#if NETWORKTEST                                
                                	suite.AddTest (DnsTest.Suite);
                                	suite.AddTest (HttpWebRequestTest.Suite);
                                	suite.AddTest (ServicePointTest.Suite);
                                	suite.AddTest (ServicePointManagerTest.Suite);
				#endif                                
                                
				return suite;
                        }
                }
        }
}

