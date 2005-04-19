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
[TestFixture] public class DataTable_primaryKey : GHTBase
{
	[Test] public void Main()
	{
		DataTable_primaryKey tc = new DataTable_primaryKey();
		Exception exp = null;
		try
		{
			tc.BeginTest("DataTable_PrimaryKey");
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

	public void run()
	{
		Exception exp = null;
		DataTable dtParent;
		dtParent = GHTUtils.DataProvider.CreateParentDataTable();
        
		try
		{
			base.BeginCase("Checking PrimaryKey default");
			base.Compare(dtParent.PrimaryKey.Length , 0);
		
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}
		
		try
		{
			base.BeginCase("Checking PrimaryKey set/get");
			DataColumn[] dcArr = new DataColumn[] {dtParent.Columns[0]}; 
			dtParent.PrimaryKey = new DataColumn[] {dtParent.Columns[0]};
			base.Compare(dtParent.PrimaryKey  ,dcArr);
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}

		dtParent.PrimaryKey=null;
		DataSet ds = new DataSet();
		DataRow dr = null;
		ds.Tables.Add(dtParent);

		//check primary key - ColumnType String, ds.CaseSensitive = false;
		ds.CaseSensitive = false;
		dtParent.PrimaryKey = new DataColumn[] {dtParent.Columns["String1"]};
		try
		{
			base.BeginCase("check primary key - ColumnType String, ds.CaseSensitive = false;");
			dr = dtParent.NewRow();
			dr.ItemArray = dtParent.Rows[0].ItemArray;
			dr["String1"] = dr["String1"].ToString().ToUpper();  
			try
			{
				dtParent.Rows.Add(dr);
			}
			catch (ConstraintException ex) {exp = ex;}
			base.Compare(exp.GetType().FullName , typeof(ConstraintException).FullName);
			exp = null;
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}
		if (dr.RowState != DataRowState.Detached) dtParent.Rows.Remove(dr);

		//check primary key - ColumnType String, ds.CaseSensitive = true;		
		ds.CaseSensitive = true;
		try
		{
			base.BeginCase("check primary key ConstraintException - ColumnType String, ds.CaseSensitive = true;");
			dr = dtParent.NewRow();
			dr.ItemArray = dtParent.Rows[0].ItemArray;
			dr["String1"] = dr["String1"].ToString();  
			try
			{
				dtParent.Rows.Add(dr);
			}
			catch (ConstraintException ex) {exp = ex;}
			base.Compare(exp.GetType().FullName , typeof(ConstraintException).FullName);
			exp = null;
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}
		if (dr.RowState != DataRowState.Detached) dtParent.Rows.Remove(dr);

		//check primary key - ColumnType String, ds.CaseSensitive = true;		
		ds.CaseSensitive = true;


		try
		{
			base.BeginCase("check primary key - ColumnType String, ds.CaseSensitive = true;");
			dr = dtParent.NewRow();
			dr.ItemArray = dtParent.Rows[0].ItemArray;
			dr["String1"] = dr["String1"].ToString().ToUpper() ;  
			dtParent.Rows.Add(dr);
			base.Compare(dtParent.Rows.Contains(dr["String1"]),true);
			exp = null;
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}

		if (dr.RowState != DataRowState.Detached) dtParent.Rows.Remove(dr);
		
		dtParent.PrimaryKey=null;
		dtParent.PrimaryKey = new DataColumn[] {dtParent.Columns["ParentDateTime"]};
		try
		{
			base.BeginCase("check primary key - ColumnType DateTime");
			dr = dtParent.NewRow();
			dr.ItemArray = dtParent.Rows[0].ItemArray;
			dr["ParentDateTime"] = DateTime.Now;
			dtParent.Rows.Add(dr);
			base.Compare(dtParent.Rows.Contains(dr["ParentDateTime"]),true);
			exp = null;
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}
		if (dr.RowState != DataRowState.Detached) dtParent.Rows.Remove(dr);

		try
		{
			base.BeginCase("check primary key ConstraintException- ColumnType DateTime");
			dr = dtParent.NewRow();
			dr.ItemArray = dtParent.Rows[0].ItemArray;
			dr["ParentDateTime"] = dtParent.Rows[0]["ParentDateTime"];
			try
			{
				dtParent.Rows.Add(dr);
			}
			catch (ConstraintException ex) {exp = ex;}
			base.Compare(exp.GetType(), typeof(ConstraintException));
			exp = null;
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}
		if (dr.RowState != DataRowState.Detached) dtParent.Rows.Remove(dr);


		dtParent.PrimaryKey=null;
		dtParent.PrimaryKey = new DataColumn[] {dtParent.Columns["ParentDouble"]};
		try
		{
			base.BeginCase("check primary key - ColumnType ParentDouble, value=Epsilon");
			dr = dtParent.NewRow();
			dr.ItemArray = dtParent.Rows[0].ItemArray;
			dr["ParentDouble"] = Double.Epsilon ;
			dtParent.Rows.Add(dr);
			base.Compare(dtParent.Rows.Contains(dr["ParentDouble"]),true);
			exp = null;
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}
		if (dr.RowState != DataRowState.Detached) dtParent.Rows.Remove(dr);

		try
		{
			base.BeginCase("check primary key ConstraintException - ColumnType ParentDouble");
			dr = dtParent.NewRow();
			dr.ItemArray = dtParent.Rows[0].ItemArray;
			dr["ParentDouble"] = dtParent.Rows[0]["ParentDouble"];
			try
			{
				dtParent.Rows.Add(dr);
			}
			catch (ConstraintException ex) {exp = ex;}
			base.Compare(exp.GetType(), typeof(ConstraintException));
			exp = null;
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}
		if (dr.RowState != DataRowState.Detached) dtParent.Rows.Remove(dr);


		//
		// SubTest
		//
		dtParent.PrimaryKey=null;
		try
		{
			base.BeginCase("check primary key ConstraintException - ColumnType ParentBool ");
			try
			{
				//ParentBool is not unique, will raise exception
				dtParent.PrimaryKey = new DataColumn[] {dtParent.Columns["ParentBool"]};
			}
			catch (ArgumentException ex) {exp = ex;}
			base.Compare(exp.GetType().FullName , typeof(ArgumentException).FullName );
			exp = null;
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}
		if (dr.RowState != DataRowState.Detached) dtParent.Rows.Remove(dr);

		//
		// SubTest
		//
		dtParent.PrimaryKey=null;
		dtParent.PrimaryKey = new DataColumn[] {dtParent.Columns["ParentDouble"],dtParent.Columns["ParentDateTime"]};
		try
		{
			base.BeginCase("check primary key - ColumnType Double,DateTime test1");
			dr = dtParent.NewRow();
			dr.ItemArray = dtParent.Rows[0].ItemArray;
			dr["ParentDouble"] = dtParent.Rows[0]["ParentDouble"];
			dr["ParentDateTime"] = DateTime.Now;
			dtParent.Rows.Add(dr);
			base.Compare(dtParent.Rows.Contains(new object[] {dr["ParentDouble"],dr["ParentDateTime"]}),true);
			exp = null;
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}
		if (dr.RowState != DataRowState.Detached) dtParent.Rows.Remove(dr);

		try
		{
			base.BeginCase("check primary key - ColumnType Double,DateTime test2");
			dr = dtParent.NewRow();
			dr.ItemArray = dtParent.Rows[0].ItemArray;
			dr["ParentDateTime"] = dtParent.Rows[0]["ParentDateTime"];
			dr["ParentDouble"] = 99.399;
			dtParent.Rows.Add(dr);
			base.Compare(dtParent.Rows.Contains(new object[] {dr["ParentDouble"],dr["ParentDateTime"]}),true);
			exp = null;
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}
		if (dr.RowState != DataRowState.Detached) dtParent.Rows.Remove(dr);

		
		try
		{
			base.BeginCase("check primary key ConstraintException - ColumnType Double,DateTime ");
			dr = dtParent.NewRow();
			dr.ItemArray = dtParent.Rows[0].ItemArray;
			dr["ParentDouble"] = dtParent.Rows[0]["ParentDouble"];
			dr["ParentDateTime"] = dtParent.Rows[0]["ParentDateTime"];
			try
			{
				dtParent.Rows.Add(dr);
			}
			catch (ConstraintException ex) {exp = ex;}
			base.Compare(exp.GetType(), typeof(ConstraintException) );
			exp = null;
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}
		if (dr.RowState != DataRowState.Detached) dtParent.Rows.Remove(dr);


		DataTable dtChild = GHTUtils.DataProvider.CreateChildDataTable();
		ds.Tables.Add(dtChild);
		dtParent.PrimaryKey = null;
		//this test was addedd to check java exception: 
		//System.ArgumentException: Cannot remove UniqueConstraint because the ForeignKeyConstraint myRelation exists.
		try
		{
			base.BeginCase("check add primary key with relation ");
			ds.Relations.Add(new DataRelation("myRelation",ds.Tables[0].Columns[0],ds.Tables[1].Columns[0]));
			//the following line will cause java to fail
			ds.Tables[0].PrimaryKey = new DataColumn[] {ds.Tables[0].Columns[0],ds.Tables[0].Columns[1]}; 
			base.Compare(ds.Tables[0].PrimaryKey.Length , 2);
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}
		


				
	}
}
}