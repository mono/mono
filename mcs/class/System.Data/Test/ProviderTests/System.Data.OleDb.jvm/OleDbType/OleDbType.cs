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
	public class OleDbType_Enum : ADONetTesterClass
	{
		public static void Main()
		{
			OleDbType_Enum tc = new OleDbType_Enum();
			Exception exp = null;
			try
			{
				tc.BeginTest("OleDbType_Enum");
				tc.run();
			}
			catch(Exception ex){exp = ex;}
			finally	{tc.EndTest(exp);}
		}

		[Test]
		public void run()
		{
			Exception exp = null;

			try
			{
				BeginCase("Checking Empty value");			Compare((int)OleDbType.Empty,0  );
			}
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp); exp = null;}
			try
			{
				BeginCase("Checking SmallInt value");			Compare((int)OleDbType.SmallInt,2  );
			}
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp); exp = null;}
			try
			{
				BeginCase("Checking Integer value");			Compare((int)OleDbType.Integer,3  );
			}
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp); exp = null;}
			try
			{
				BeginCase("Checking Single value");			Compare((int)OleDbType.Single,4  );
			}
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp); exp = null;}
			try
			{
				BeginCase("Checking Double value");			Compare((int)OleDbType.Double,5  );
			}
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp); exp = null;}
			try
			{
				BeginCase("Checking Currency value");			Compare((int)OleDbType.Currency,6  );
			}
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp); exp = null;}
			try
			{
				BeginCase("Checking Date value");			Compare((int)OleDbType.Date,7  );
			}
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp); exp = null;}
			try
			{
				BeginCase("Checking BSTR value");			Compare((int)OleDbType.BSTR,8  );
			}
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp); exp = null;}
			try
			{
				BeginCase("Checking IDispatch value");			Compare((int)OleDbType.IDispatch,9  );
			}
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp); exp = null;}
			try
			{
				BeginCase("Checking Error value");			Compare((int)OleDbType.Error,10 );
			}
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp); exp = null;}
			try
			{
				BeginCase("Checking Boolean value");			Compare((int)OleDbType.Boolean,11 );
			}
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp); exp = null;}
			try
			{
				BeginCase("Checking Variant value");			Compare((int)OleDbType.Variant,12 );
			}
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp); exp = null;}
			try
			{
				BeginCase("Checking IUnknown value");			Compare((int)OleDbType.IUnknown,13 );
			}
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp); exp = null;}
			try
			{
				BeginCase("Checking Decimal value");			Compare((int)OleDbType.Decimal,14 );
			}
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp); exp = null;}
			try
			{
				BeginCase("Checking TinyInt value");			Compare((int)OleDbType.TinyInt,16 );
			}
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp); exp = null;}
			try
			{
				BeginCase("Checking UnsignedTinyInt value");		Compare((int)OleDbType.UnsignedTinyInt,17 );
			}
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp); exp = null;}
			try
			{
				BeginCase("Checking UnsignedSmallInt value");		Compare((int)OleDbType.UnsignedSmallInt,18 );
			}
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp); exp = null;}
			try
			{
				BeginCase("Checking UnsignedInt value");		Compare((int)OleDbType.UnsignedInt,19 );
			}
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp); exp = null;}
			try
			{
				BeginCase("Checking BigInt value");			Compare((int)OleDbType.BigInt,20 );
			}
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp); exp = null;}
			try
			{
				BeginCase("Checking UnsignedBigInt value");		Compare((int)OleDbType.UnsignedBigInt,21 );
			}
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp); exp = null;}
			try
			{
				BeginCase("Checking Filetime value");			Compare((int)OleDbType.Filetime,64 );
			}
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp); exp = null;}
			try
			{
				BeginCase("Checking Guid value");			Compare((int)OleDbType.Guid,72 );
			}
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp); exp = null;}
			try
			{
				BeginCase("Checking Binary value");			Compare((int)OleDbType.Binary,128);
			}
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp); exp = null;}
			try
			{
				BeginCase("Checking Char value");			Compare((int)OleDbType.Char,129);
			}
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp); exp = null;}
			try
			{
				BeginCase("Checking WChar value");			Compare((int)OleDbType.WChar,130);
			}
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp); exp = null;}
			try
			{
				BeginCase("Checking Numeric value");			Compare((int)OleDbType.Numeric,131);
			}
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp); exp = null;}
			try
			{
				BeginCase("Checking DBDate value");			Compare((int)OleDbType.DBDate,133);
			}
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp); exp = null;}
			try
			{
				BeginCase("Checking DBTime value");			Compare((int)OleDbType.DBTime,134);
			}
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp); exp = null;}
			try
			{
				BeginCase("Checking DBTimeStamp value");		Compare((int)OleDbType.DBTimeStamp,135);
			}
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp); exp = null;}
			try
			{
				BeginCase("Checking PropVariant value");		Compare((int)OleDbType.PropVariant,138);
			}
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp); exp = null;}
			try
			{
				BeginCase("Checking VarNumeric value");			Compare((int)OleDbType.VarNumeric,139);
			}
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp); exp = null;}
			try
			{
				BeginCase("Checking VarChar value");			Compare((int)OleDbType.VarChar,200);
			}
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp); exp = null;}
			try
			{
				BeginCase("Checking LongVarChar value");		Compare((int)OleDbType.LongVarChar,201);
			}
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp); exp = null;}
			try
			{
				BeginCase("Checking VarWChar value");			Compare((int)OleDbType.VarWChar,202);
			}
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp); exp = null;}
			try
			{
				BeginCase("Checking LongVarWChar value");		Compare((int)OleDbType.LongVarWChar,203);
			}
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp); exp = null;}
			try
			{
				BeginCase("Checking VarBinary value");			Compare((int)OleDbType.VarBinary,204);
			}
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp); exp = null;}
			try
			{
				BeginCase("Checking LongVarBinary value");		Compare((int)OleDbType.LongVarBinary,205);
			}
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp); exp = null;}


		}
	}
}