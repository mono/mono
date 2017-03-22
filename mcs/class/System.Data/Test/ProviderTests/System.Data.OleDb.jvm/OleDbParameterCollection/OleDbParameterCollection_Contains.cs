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
	public class OleDbParameterCollection_Contains : ADONetTesterClass
	{
		public static void Main()
		{
			OleDbParameterCollection_Contains tc = new OleDbParameterCollection_Contains();
			Exception exp = null;
			try
			{
				tc.BeginTest("OleDbParameterCollection_Contains");
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
			OleDbParameter param = new OleDbParameter();
			cmd.Parameters.Add(param);
			cmd.Parameters.Add(new OleDbParameter("MyParam", 12));

			try
			{
				BeginCase("Check contains - 1");
				Compare(cmd.Parameters.Contains(param),true );
			} 
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp); exp = null;}

			try
			{
				BeginCase("Check contains - 2");
				Compare(cmd.Parameters.Contains("MyParam"),true );
			} 
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp); exp = null;}

			try
			{
				BeginCase("Check contains - 3");
				Compare(cmd.Parameters.Contains("abcd"),false );
			} 
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp); exp = null;}

			try
			{
				BeginCase("Check contains - 4");
				Compare(cmd.Parameters.Contains(" MyParam"),false );
			} 
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp); exp = null;}
        
			try
			{
				BeginCase("Check contains - 5");
				cmd.Parameters.Add(new OleDbParameter("MyParam", 12));
				Compare(cmd.Parameters.Contains("MyParam"),true );
				Compare(cmd.Parameters.Contains("myparam"),true );
			} 
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp); exp = null;}
        
		}
	}
}