// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
//

namespace DefaultNamespace
{
    //@BEGINRENAME; Verify this renames
    using System.Security;
    //@ENDRENAME; Verify this renames
    using System;

    class TestClass
    {
        public static int iExitCode;

        protected void TestMain()
        {
            int caught = 0;
            //int fincount = 0;

            try
            {
                throw new ArgumentException();
            }
            catch (ArgumentException /*e1*/)
            {
                caught++;
                try
                {
                    throw new SecurityException();
                }
                catch (SecurityException /*e2*/)
                {
                }
                finally
                {
                    try
                    {
                        throw new NullReferenceException();
                    }
                    catch (Exception /*e3*/)
                    {
                    }

                }

            }


        }

        public static int Main(String[] args)
        {
            (new TestClass()).TestMain();
            Console.WriteLine("Passed.");
            return 100;
        }

    };
};

