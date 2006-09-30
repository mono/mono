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
using System.Data;
using System.Data.OracleClient;

using MonoTests.System.Data.Utils;


using NUnit.Framework;

namespace MonoTests.System.Data.OracleClient
{
	[TestFixture]
	public class OracleConnection_Open : GHTBase
	{
		OracleConnection con = null;
		string ConString = "";

		[SetUp]
		public void SetUp()
		{
			Exception exp = null;
			BeginCase("Setup");
			try
			{
				ConString = MonoTests.System.Data.Utils.ConnectedDataProvider.ConnectionString;
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}
		}

		[TearDown]
		public void TearDown()
		{
			if (con != null)
			{
				con.Close();
			}
		}

		public static void Main()
		{
			OracleConnection_Open tc = new OracleConnection_Open();
			Exception exp = null;
			try
			{
				tc.BeginTest("OracleOpenConnection");
				tc.SetUp();
				tc.run();
				tc.TearDown();
			}
			catch(Exception ex)
			{
				exp = ex;
			}
			finally
			{
				tc.EndTest(exp);
			}
		}

		[Test]
		public void run()
		{
			Exception exp = null;
      
			BeginCase("Open Connection ");
			try
			{
				con = new OracleConnection(ConString);
				con.Open();
				Compare(con.State , ConnectionState.Open);
			}
			catch (Exception ex)
			{
				exp = ex;
			}
			finally
			{
				if (con != null) con.Close();
				EndCase(exp);
				exp = null;
			}

			BeginCase("Open Connection - garbage value");
			try
			{
				con = new OracleConnection("xxx");
				con.Open();
			}
			catch (ArgumentException ex)
			{
				ExpectedExceptionCaught(ex); 
			}
			catch 
			{
				ExpectedExceptionNotCaught("System.ArgumentException");
			}
			finally
			{
				if (con != null) con.Close();
				EndCase(exp);
				exp = null;
			}
		}
	}
}