// 
// Copyright (c) 2006 Mainsoft Co.
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

using System;
using System.Data.OleDb ;


using MonoTests.System.Data.Utils;


using NUnit.Framework;

namespace MonoTests.System.Data.OleDb
{
[TestFixture]
public class OleDbCommand_CommandTimeout : GHTBase
{
	public static void Main()
	{
		OleDbCommand_CommandTimeout tc = new OleDbCommand_CommandTimeout();
		Exception exp = null;
		try
		{
			tc.BeginTest("OleDbCommand_CommandTimeout");
			tc.run();
		}
		catch(Exception ex){exp = ex;}
		finally	{tc.EndTest(exp);}
	}

	[Test]
	public void run()
	{
		OleDbCommand cmd = new OleDbCommand();
		Assert.AreEqual(cmd.CommandTimeout , 30);
		cmd.CommandTimeout = 12;
		Assert.AreEqual(cmd.CommandTimeout , 12);
	}


	//public TestClass():base(true){}

	//Activate this constructor to log Failures to a log file
	//public TestClass(System.IO.TextWriter tw):base(tw, false){}

	//Activate this constructor to log All to a log file
	//public TestClass(System.IO.TextWriter tw):base(tw, true){}

	//BY DEFAULT LOGGING IS DONE TO THE STANDARD OUTPUT ONLY FOR FAILURES

}
}