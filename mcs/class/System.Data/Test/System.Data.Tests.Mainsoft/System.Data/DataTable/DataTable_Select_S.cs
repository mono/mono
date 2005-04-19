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

using NUnit.Framework;
using System;
using System.Data;

using GHTUtils;
using GHTUtils.Base;

namespace tests.system_data_dll.System_Data
{
	[TestFixture] public class DataTable_Select_S : GHTBase
	{
		[Test] public void Main()
		{
			DataTable_Select_S tc = new DataTable_Select_S();
			Exception exp = null;
			try
			{
				tc.BeginTest("DataTable_Select_S");
				tc.run();
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

		//Activate This Construntor to log All To Standard output
		//public TestClass():base(true){}

		//Activate this constructor to log Failures to a log file
		//public TestClass(System.IO.TextWriter tw):base(tw, false){}


		//Activate this constructor to log All to a log file
		//public TestClass(System.IO.TextWriter tw):base(tw, true){}

		//BY DEFAULT LOGGING IS DONE TO THE STANDARD OUTPUT ONLY FOR FAILURES

		public void run()
		{
		
			Exception exp = null;
			DataSet ds = new DataSet();
			ds.Tables.Add(GHTUtils.DataProvider.CreateParentDataTable());
		
			DataTable dt = GHTUtils.DataProvider.CreateChildDataTable();
			ds.Tables.Add(dt);
			DataRow[] drSelect = null;
			System.Collections.ArrayList al = new System.Collections.ArrayList();



			//add column with special name
			DataColumn dc = new DataColumn("Column#",typeof(int));
			dc.DefaultValue=-1;
			dt.Columns.Add(dc);
			//put some values
			dt.Rows[0][dc] = 100;
			dt.Rows[1][dc] = 200;
			dt.Rows[2][dc] = 300;
			dt.Rows[4][dc] = -400;

			//for trim function
			dt.Rows[0]["String1"] = dt.Rows[0]["String1"] + "   \t\n ";
			dt.Rows[0]["String1"] = "   \t\n " + dt.Rows[0]["String1"] ;
			dt.Rows[0]["String1"] = dt.Rows[0]["String1"] + "    ";

			ds.Tables[0].Rows[0]["ParentBool"] = DBNull.Value;
			ds.Tables[0].Rows[2]["ParentBool"] = DBNull.Value;
			ds.Tables[0].Rows[3]["ParentBool"] = DBNull.Value;

			//-------------------------------------------------------------
			al.Clear();
			foreach (DataRow dr in dt.Rows ) 
			{
				if ((int)dr["ChildId"] == 1)
				{
					al.Add(dr);
				}
			}
			try
			{
				BeginCase("Select_S - ChildId=1");
				drSelect = dt.Select("ChildId=1");
				Compare(drSelect ,al.ToArray());
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}
		
			//-------------------------------------------------------------
			al.Clear();
			foreach (DataRow dr in dt.Rows ) 
			{
				if ((int)dr["ChildId"] == 1)
				{
					al.Add(dr);
				}
			}
			try
			{
				BeginCase("Select_S - ChildId='1'");
				drSelect = dt.Select("ChildId='1'");
				Compare(drSelect ,al.ToArray());
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}
			//-------------------------------------------------------------
			try
			{
				BeginCase("Select_S - ChildId= '1'  (whitespace in filter string.");
				drSelect = dt.Select("ChildId= '1'");
				Compare(drSelect ,al.ToArray());
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}
			//-------------------------------------------------------------
			al.Clear();
			foreach (DataRow dr in dt.Rows ) if (dr["String1"].ToString() == "1-String1") al.Add(dr);
			try
			{
				BeginCase("Select_S - String1='1-String1'");
				drSelect = dt.Select("String1='1-String1'");
				Compare(drSelect ,al.ToArray());
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}

			//-------------------------------------------------------------
			al.Clear();
			foreach (DataRow dr in dt.Rows ) if ((int)dr["ChildId"] == 1 && dr["String1"].ToString() == "1-String1" ) al.Add(dr);
			try
			{
				BeginCase("Select_S - ChildId=1 and String1='1-String1'");
				drSelect = dt.Select("ChildId=1 and String1='1-String1'");
				Compare(drSelect ,al.ToArray());
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}
		
			//-------------------------------------------------------------
			al.Clear();
			foreach (DataRow dr in dt.Rows ) if ((int)dr["ChildId"] + (int)dr["ParentId"] >= 4 ) al.Add(dr);
			try
			{
				BeginCase("Select_S - ChildId+ParentId >= 4");
				drSelect = dt.Select("ChildId+ParentId >= 4");
				CompareUnSorted(drSelect ,al.ToArray());
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}

			//-------------------------------------------------------------
			al.Clear();
			foreach (DataRow dr in dt.Rows ) 
			{
				if ((((int)dr["ChildId"] - (int)dr["ParentId"]) * -1) != 0 )
				{
					al.Add(dr);
				}
			}
			try
			{
				BeginCase("Select_S - ChildId-ParentId) * -1  <> 0");
				drSelect = dt.Select("(ChildId-ParentId) * -1  <> 0");
				CompareUnSorted(drSelect ,al.ToArray());
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}

			//-------------------------------------------------------------
			al.Clear();
			foreach (DataRow dr in dt.Rows ) if ( (double)dr["ChildDouble"] < ((int)dr["ParentId"]) % 4 ) al.Add(dr);
			try
			{
				BeginCase("Select_S - ChildDouble < ParentId % 4");
				drSelect = dt.Select("ChildDouble < ParentId % 4");
				CompareUnSorted(drSelect ,al.ToArray());
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}

			//-------------------------------------------------------------
			al.Clear();
			foreach (DataRow dr in dt.Rows ) if ( (double)dr["ChildDouble"] == 10 || (double)dr["ChildDouble"] == 20 || (double)dr["ChildDouble"] == 25 ) al.Add(dr);
			try
			{
				BeginCase("Select_S - ChildDouble in (10,20,25)");
				drSelect = dt.Select("ChildDouble in (10,20,25)");
				CompareUnSorted(drSelect ,al.ToArray());
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}

			//-------------------------------------------------------------
			al.Clear();
			foreach (DataRow dr in dt.Rows ) if ( dr["String2"].ToString().IndexOf("1-S") >= 0 ) al.Add(dr);
			try
			{
				BeginCase("Select_S - String2 like '%1-S%'");
				drSelect = dt.Select("String2 like '%1-S%'");
				Compare(drSelect ,al.ToArray());
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}

			//-------------------------------------------------------------
			//If a column name contains one of the above characters,(ex. #\/=><+-*%&|^'" and so on) the name must be wrapped in brackets. For example to use a column named "Column#" in an expression, you would write "[Column#]":
			al.Clear();
			foreach (DataRow dr in dt.Rows ) if ( (int)dr["Column#"] <= 0) al.Add(dr);
			try
			{
				BeginCase("Select_S - [Column#] <= 0 ");
				drSelect = dt.Select("[Column#] <= 0 ");
				CompareUnSorted(drSelect ,al.ToArray());
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}
			//-------------------------------------------------------------
			al.Clear();
			foreach (DataRow dr in dt.Rows ) if ( (int)dr["Column#"] <= 0) al.Add(dr);
			try
			{
				BeginCase("Select_S - [Column#] <= 0");
				drSelect = dt.Select("[Column#] <= 0");
				CompareUnSorted(drSelect ,al.ToArray());
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}

			//-------------------------------------------------------------
			al.Clear();
			foreach (DataRow dr in dt.Rows ) if (((DateTime)dr["ChildDateTime"]).CompareTo(new DateTime(2000,12,12)) > 0  ) al.Add(dr);
			try
			{
				BeginCase("Select_S - ChildDateTime > #12/12/2000# ");
				drSelect = dt.Select("ChildDateTime > #12/12/2000# ");
				CompareUnSorted(drSelect ,al.ToArray());
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}

			//-------------------------------------------------------------
			
			al.Clear();
			foreach (DataRow dr in dt.Rows ) if ( ((DateTime)dr["ChildDateTime"]).CompareTo(new DateTime(1999,1,12,12,06,30)) > 0  ) al.Add(dr);
			try
			{
				
				BeginCase("Select_S - ChildDateTime > #1/12/1999 12:06:30 PM#  ");
				drSelect = dt.Select("ChildDateTime > #1/12/1999 12:06:30 PM#  ");
				CompareUnSorted(drSelect ,al.ToArray());
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}

			//-------------------------------------------------------------
			
			al.Clear();
			foreach (DataRow dr in dt.Rows ) if ( ((DateTime)dr["ChildDateTime"]).CompareTo(new DateTime(2005,12,03,17,06,30)) >= 0  || ((DateTime)dr["ChildDateTime"]).CompareTo(new DateTime(1980,11,03)) <= 0 ) al.Add(dr);
			try
			{
				
				BeginCase("Select_S - ChildDateTime >= #12/3/2005 5:06:30 PM# or ChildDateTime <= #11/3/1980#  ");
				drSelect = dt.Select("ChildDateTime >= #12/3/2005 5:06:30 PM# or ChildDateTime <= #11/3/1980#  ");
				CompareUnSorted(drSelect ,al.ToArray());
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}
			
#if LATER
			//-------------------------------------------------------------
			al.Clear();
			foreach (DataRow dr in dt.Rows ) if ( dr["ChildDouble"].ToString().Length > 10) al.Add(dr);
			try
			{
				BeginCase("Select_S - Len(Convert(ChildDouble,'System.String')) > 10");
				drSelect = dt.Select("Len(Convert(ChildDouble,'System.String')) > 10");
				Compare(drSelect ,al.ToArray());
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}
#endif
			//-------------------------------------------------------------
			al.Clear();
			foreach (DataRow dr in dt.Rows ) if ( dr["String1"].ToString().Trim().Substring(0,2) == "1-") al.Add(dr);
			try
			{
				BeginCase("Select_S - SubString(Trim(String1),1,2) = '1-'");
				drSelect = dt.Select("SubString(Trim(String1),1,2) = '1-'");
				Compare(drSelect ,al.ToArray());
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}
			//-------------------------------------------------------------
			/*
			al.Clear();
			foreach (DataRow dr in ds.Tables[0].Rows ) if ( dr.IsNull("ParentBool") || (bool)dr["ParentBool"]) al.Add(dr);
			try
			{
				BeginCase("Select_S - IsNull(ParentBool,true)");
				drSelect = ds.Tables[0].Select("IsNull(ParentBool,true) ");
				Compare(drSelect ,al.ToArray());
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}
			*/
			//-------------------------------------------------------------
			al.Clear();
			try
			{
				BeginCase("Select_S - Relation not exists, Exception");
				try
				{
					drSelect = dt.Select("Parent.ParentId = ChildId");
				}
				catch (IndexOutOfRangeException ex) {exp = ex;}
				Compare(exp != null, true );
				exp = null;
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}
			//-------------------------------------------------------------
			al.Clear();
			ds.Relations.Add(new DataRelation("ParentChild",ds.Tables[0].Columns[0],ds.Tables[1].Columns[0]));
			foreach (DataRow dr in dt.Rows ) if ( (int)dr["ChildId"] == (int)dr.GetParentRow("ParentChild")["ParentId"]) al.Add(dr);
			try
			{
				BeginCase("Select_S - Parent.ParentId = ChildId");
				drSelect = dt.Select("Parent.ParentId = ChildId");
				Compare(drSelect ,al.ToArray());
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}


		}

		private void CompareUnSorted(Array a, Array b)
		{
			string msg = string.Format("Failed while comparing(Array a ={0} ({1}), Array b = {2} ({3}))]", a.ToString(), a.GetType().FullName, b.ToString(), b.GetType().FullName);
			foreach (object item in a)
			{
				if (Array.IndexOf(b, item) < 0)	//b does not contain the current item.
				{
					this.Fail(msg);
					return;
				}
			}

			foreach (object item in b)
			{
				if (Array.IndexOf(a, item) < 0)	//a does not contain the current item.
				{
					this.Fail(msg);
					return;
				}
			}
			
			//b contains all items of a, and a contains all items of b.
			this.Pass(msg);
		}
	}
}
