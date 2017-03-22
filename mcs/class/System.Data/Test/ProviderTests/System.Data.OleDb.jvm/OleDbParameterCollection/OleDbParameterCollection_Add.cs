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
using System.Data.OleDb;

using MonoTests.System.Data.Utils;


using NUnit.Framework;

namespace MonoTests.System.Data.OleDb
{
	[TestFixture]
	public class OleDbParameterCollection_Add : ADONetTesterClass
	{
		public static void Main()
		{
			OleDbParameterCollection_Add tc = new OleDbParameterCollection_Add();
			Exception exp = null;
			try
			{
				tc.BeginTest("OleDbParameterCollection_Add");
				tc.run();
			}
			catch(Exception ex){exp = ex;}
			finally	{tc.EndTest(exp);}
		}

		[Test]
		public void run()
		{
			Exception exp = null;


			OleDbCommand cmd = new OleDbCommand();
			OleDbParameter param = cmd.Parameters.Add(new OleDbParameter("MyParam", "abcd"));

			try
			{
				BeginCase("check value");
				Compare(param.Value ,"abcd" );
			} 
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp); exp = null;}

			param = cmd.Parameters.Add("MyParam", OleDbType.VarChar, 50);
			try
			{
				BeginCase("check parameter type");
				Compare(param.GetType().FullName ,typeof(OleDbParameter).FullName  );
			} 
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp); exp = null;}

			try
			{
				BeginCase("check parameter name");
				Compare(param.ParameterName ,"MyParam" );
			} 
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp); exp = null;}
		}
		
		[Test]
		public void TestAddCloned()
		{
			OleDbCommand c = new OleDbCommand ();
			OleDbParameter p = c.Parameters.Add ("SDF", OleDbType.BigInt);
			OleDbCommand c1 = new OleDbCommand ();
			c1.Parameters.Add ((OleDbParameter) ((ICloneable) p).Clone ());
			Assert.AreEqual(1, c1.Parameters.Count, "#01");
		}
	}
}