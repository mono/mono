//
// OracleCommandTest.cs -
//      NUnit Test Cases for OraclePermissionAttribute
//
// Author:
//      Leszek Ciesielski  <skolima@gmail.com>
//
// Copyright (C) 2006 Forcom (http://www.forcom.com.pl/)
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

using NUnit.Framework;
using System.Data.OracleClient;
using System.Data;
using System;

namespace MonoTests.System.Data.OracleClient {

        [TestFixture]
        public class OracleCommandTest {

                OracleCommand command;
                IDbCommand interface_command;

                [SetUp]
                public void SetUp ()
                {
                        command = new OracleCommand ();
                        interface_command = command;
                }

                [TearDown]
                public void TearDown ()
                {
                        command.Dispose ();
                }

                [Test] // regression for bug #78765
                public void AllowNullConnectionTest ()
                {
                        command.Connection = null;
                        try {
                                interface_command.Connection = null;
                        }
                        catch (Exception) {
                                Assert.Fail ("Connection property should be nullable");
                        }
                }

                [Test] // regression for bug #78765
                public void AllowNullTransactionTest ()
                {
                        command.Transaction = null;
                        try {
                                interface_command.Transaction = null;
                        }
                        catch (Exception) {
                                Assert.Fail ("Transaction property should be nullable");
                        }
                }
        }
}