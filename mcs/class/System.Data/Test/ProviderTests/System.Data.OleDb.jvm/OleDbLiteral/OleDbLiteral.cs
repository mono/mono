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
	public class OleDbLiteral_Enum : ADONetTesterClass
	{
		public static void Main()
		{
			OleDbLiteral_Enum tc = new OleDbLiteral_Enum();
			Exception exp = null;
			try
			{
				tc.BeginTest("OleDbLiteral_Enum");
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
				BeginCase("Checking Invalid value"); Compare((int)OleDbLiteral.Invalid,0  );
			}
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp); exp = null;}
			try
			{
				BeginCase("Checking Binary_Literalvalue"); Compare((int)OleDbLiteral.Binary_Literal,1  );
			}
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp); exp = null;}
			try
			{
				BeginCase("Checking Catalog_Namevalue"); Compare((int)OleDbLiteral.Catalog_Name,2  );
			}
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp); exp = null;}
			try
			{
				BeginCase("Checking Catalog_Separatorvalue"); Compare((int)OleDbLiteral.Catalog_Separator,3  );
			}
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp); exp = null;}
			try
			{
				BeginCase("Checking Char_Literalvalue"); Compare((int)OleDbLiteral.Char_Literal,4  );
			}
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp); exp = null;}
			try
			{
				BeginCase("Checking Column_Aliasvalue"); Compare((int)OleDbLiteral.Column_Alias,5  );
			}
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp); exp = null;}
			try
			{
				BeginCase("Checking Column_Namevalue"); Compare((int)OleDbLiteral.Column_Name,6  );
			}
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp); exp = null;}
			try
			{
				BeginCase("Checking Correlation_Namevalue"); Compare((int)OleDbLiteral.Correlation_Name,7  );
			}
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp); exp = null;}
			try
			{
				BeginCase("Checking Cursor_Namevalue"); Compare((int)OleDbLiteral.Cursor_Name,8  );
			}
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp); exp = null;}
			try
			{
				BeginCase("Checking Escape_Percent_Prefixvalue"); Compare((int)OleDbLiteral.Escape_Percent_Prefix,9  );
			}
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp); exp = null;}
			try
			{
				BeginCase("Checking Escape_Underscore_Prefixvalue"); Compare((int)OleDbLiteral.Escape_Underscore_Prefix,10  );
			}
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp); exp = null;}
			try
			{
				BeginCase("Checking Index_Namevalue"); Compare((int)OleDbLiteral.Index_Name,11  );
			}
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp); exp = null;}
			try
			{
				BeginCase("Checking Like_Percentvalue"); Compare((int)OleDbLiteral.Like_Percent,12  );
			}
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp); exp = null;}
			try
			{
				BeginCase("Checking Like_Underscorevalue"); Compare((int)OleDbLiteral.Like_Underscore,13  );
			}
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp); exp = null;}
			try
			{
				BeginCase("Checking Procedure_Namevalue"); Compare((int)OleDbLiteral.Procedure_Name,14  );
			}
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp); exp = null;}
			try
			{
				BeginCase("Checking Quote_Prefixvalue"); Compare((int)OleDbLiteral.Quote_Prefix,15  );
			}
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp); exp = null;}
			try
			{
				BeginCase("Checking Schema_Namevalue"); Compare((int)OleDbLiteral.Schema_Name,16  );
			}
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp); exp = null;}
			try
			{
				BeginCase("Checking Table_Namevalue"); Compare((int)OleDbLiteral.Table_Name,17  );
			}
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp); exp = null;}
			try
			{
				BeginCase("Checking Text_Commandvalue"); Compare((int)OleDbLiteral.Text_Command,18  );
			}
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp); exp = null;}
			try
			{
				BeginCase("Checking User_Namevalue"); Compare((int)OleDbLiteral.User_Name,19  );
			}
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp); exp = null;}
			try
			{
				BeginCase("Checking View_Namevalue"); Compare((int)OleDbLiteral.View_Name,20  );
			}
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp); exp = null;}
			try
			{
				BeginCase("Checking Cube_Namevalue"); Compare((int)OleDbLiteral.Cube_Name,21  );
			}
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp); exp = null;}
			try
			{
				BeginCase("Checking Dimension_Namevalue"); Compare((int)OleDbLiteral.Dimension_Name,22  );
			}
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp); exp = null;}
			try
			{
				BeginCase("Checking Hierarchy_Namevalue"); Compare((int)OleDbLiteral.Hierarchy_Name,23  );
			}
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp); exp = null;}
			try
			{
				BeginCase("Checking Level_Namevalue"); Compare((int)OleDbLiteral.Level_Name,24  );
			}
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp); exp = null;}
			try
			{
				BeginCase("Checking Member_Namevalue"); Compare((int)OleDbLiteral.Member_Name,25  );
			}
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp); exp = null;}
			try
			{
				BeginCase("Checking Property_Namevalue"); Compare((int)OleDbLiteral.Property_Name,26  );
			}
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp); exp = null;}
			try
			{
				BeginCase("Checking Schema_Separatorvalue"); Compare((int)OleDbLiteral.Schema_Separator,27  );
			}
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp); exp = null;}
			try
			{
				BeginCase("Checking Quote_Suffixvalue"); Compare((int)OleDbLiteral.Quote_Suffix,28  );
			}
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp); exp = null;}
			try
			{
				BeginCase("Checking Escape_Percent_Suffixvalue"); Compare((int)OleDbLiteral.Escape_Percent_Suffix,29  );
			}
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp); exp = null;}
			try
			{
				BeginCase("Checking Escape_Underscore_Suffixvalue"); Compare((int)OleDbLiteral.Escape_Underscore_Suffix,30  );
			}
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp); exp = null;}


		}
	}
}