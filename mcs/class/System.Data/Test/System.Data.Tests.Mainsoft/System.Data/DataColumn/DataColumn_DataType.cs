// Authors:
//   Rafael Mizrahi   <rafim@mainsoft.com>
//   Erez Lotan       <erezl@mainsoft.com>
//   Oren Gurfinkel   <oreng@mainsoft.com>
//   Ofer Borstein
// 
// Copyright (c) 2004 Mainsoft Co.
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

using GHTUtils;
using GHTUtils.Base;

using NUnit.Framework;

namespace tests.system_data_dll.System_Data
{
	[TestFixture] public class DataColumn_DataType : GHTBase
	{
		public void SetUp()
		{
			Exception exp = null;
			BeginCase("Setup");
			try
			{
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}
		}

		public void TearDown()
		{
		}

		[Test] public void Main()
		{
			DataColumn_DataType tc = new DataColumn_DataType();
			Exception exp = null;
			try
			{
				tc.BeginTest("DataColumn_DataType");
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

		public void run()
		{
			Exception exp = null;
			DataColumn dc;
			dc = new DataColumn();
			string[] sTypeArr = {   "System.Boolean", "System.Byte", "System.Char", "System.DateTime",
									"System.Decimal", "System.Double", "System.Int16", "System.Int32",
									"System.Int64", "System.SByte", "System.Single", "System.String", 
									"System.TimeSpan", "System.UInt16", "System.UInt32", "System.UInt64" };
			
			//Checking default value (string)
        
			try
			{
				BeginCase("GetType - Default");
				Compare( dc.DataType,Type.GetType("System.String") );
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}
	
			foreach (string sType in sTypeArr) 
			{
				//Cheking Set
				dc.DataType = Type.GetType(sType);
				//Checking Get
				try
				{
					BeginCase("Checking GetType " + sType);
					Compare(dc.DataType ,Type.GetType(sType) );
				}
				catch(Exception ex)	{exp = ex;}
				finally	{EndCase(exp); exp = null;}
			}
			
		}
	}
}
