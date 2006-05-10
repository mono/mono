using System;
using System.Configuration;
using FrontDesk.Testing.Configuration;

class X {
        static void Main () {
                TestCenterSection tcs = (TestCenterSection)
                        ConfigurationManager.GetSection
("frontdesk/testing/testCenter");
                Console.WriteLine (tcs.Sources);
        }
}

