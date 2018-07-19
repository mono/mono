//
// OdbcParameterCollectionTest.cs - NUnit Test Cases for testing the
//                          OdbcParameterCollection class
// Author:
//      Sureshkumar T (TSureshkumar@novell.com)
//
// Copyright (c) 2004 Novell Inc., and the individuals listed
// on the ChangeLog entries.
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

#if !NO_ODBC

using System;
using System.Text;
using System.Data;
using System.Data.Odbc;

using NUnit.Framework;

namespace MonoTests.System.Data.Odbc
{

        [TestFixture]
        public class OdbcParameterCollectionTest
        {
                [Test]
                public void OdbcParameterAddTest ()
                {
                        AddTest (new OdbcCommand ());
                }

                /// <remarks>
                /// Test for parameter length of various data types.
                /// </remarks>
                public void AddTest (OdbcCommand cmd)
                {
                        OdbcParameter param = cmd.Parameters.Add ("param1", (int) 1);
                        param = cmd.Parameters.Add ("param1", (long) 1);
                        Assert.AreEqual (0, param.Size, "#1");
                        param = cmd.Parameters.Add ("param1", (float) 1.0);
                        Assert.AreEqual (0, param.Size, "#2");
                        param = cmd.Parameters.Add ("param1", (double) 1.0);
                        Assert.AreEqual (0, param.Size, "#3");
                        param = cmd.Parameters.Add ("param1", 
                                                    ASCIIEncoding.ASCII.GetBytes("this is considerably long test"));
                        Assert.AreEqual (30, param.Size, "#4");
                        param = cmd.Parameters.Add ("param1", true);
                        Assert.AreEqual (0, param.Size, "#5");
                        param = cmd.Parameters.Add ("param1", "suresh");
                        Assert.AreEqual (6, param.Size, "#6");
                        param = cmd.Parameters.Add ("param1", DateTime.Now);
                        Assert.AreEqual (0, param.Size, "#7");
                        param = cmd.Parameters.Add ("param1", (object) DateTime.Now);
                        Assert.AreEqual (0, param.Size, "#8");

                        int [] arr = new int [] {1, 2, 3} ;
                        param = cmd.Parameters.Add ("param1", arr);

                        Assert.AreEqual (0, param.Size, "#8");
       
                }
        }
}

#endif