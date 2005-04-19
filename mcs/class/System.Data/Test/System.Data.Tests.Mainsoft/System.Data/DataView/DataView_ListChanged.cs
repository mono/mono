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
[TestFixture] public class DataView_ListChanged : GHTBase
{
	[Test] public void Main()
	{
		DataView_ListChanged tc = new DataView_ListChanged();
		Exception exp = null;
		try
		{
			tc.BeginTest("DataView_ListChanged");
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

	class EventProperties  //hold the event properties to be checked
	{
		public System.ComponentModel.ListChangedType lstType ;
		public int NewIndex;
		public int OldIndex;
	}
	//a variable that will be use to check the event
	EventProperties evProp;


	public void run()
	{
		Exception exp = null;
		DataTable dt = GHTUtils.DataProvider.CreateParentDataTable();
		DataView dv = new DataView(dt);

		//add event handler
		dv.ListChanged +=new System.ComponentModel.ListChangedEventHandler(dv_ListChanged);

		// ----- Change Value ---------
		evProp = null;
		try
		{
			BeginCase("change value - Event raised");
			dv[1]["String1"] = "something";
			Compare(evProp!=null ,true );
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}
		try
		{
			BeginCase("change value - ListChangedType");
			Compare(evProp.lstType ,System.ComponentModel.ListChangedType.ItemChanged);
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}
		try
		{
			BeginCase("change value - NewIndex");
			Compare(evProp.NewIndex,1);
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}
		try
		{
			BeginCase("change value - OldIndex");
			Compare(evProp.OldIndex ,-1);
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}

		// ----- Add New ---------
		evProp = null;
		try
		{
			BeginCase("Add New  - Event raised");
			dv.AddNew();
			Compare(evProp!=null ,true );
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}
		try
		{
			BeginCase("Add New  - ListChangedType");
			Compare(evProp.lstType ,System.ComponentModel.ListChangedType.ItemAdded );
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}
		try
		{
			BeginCase("Add New  - NewIndex");
			Compare(evProp.NewIndex,6);
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}
		try
		{
			BeginCase("Add New  - OldIndex");
			Compare(evProp.OldIndex ,-1);
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}

		// ----- Sort ---------
		evProp = null;
		try
		{
			BeginCase("sort  - Event raised");
			dv.Sort = "ParentId Desc";
			Compare(evProp!=null ,true );
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}
		try
		{
			BeginCase("sort - ListChangedType");
			Compare(evProp.lstType ,System.ComponentModel.ListChangedType.Reset );
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}
		try
		{
			BeginCase("sort - NewIndex");
			Compare(evProp.NewIndex,-1);
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}
		try
		{
			BeginCase("sort - OldIndex");
			Compare(evProp.OldIndex ,-1);
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}



		//ListChangedType - this was not checked
		//Move
		//PropertyDescriptorAdded - A PropertyDescriptor was added, which changed the schema. 
		//PropertyDescriptorChanged - A PropertyDescriptor was changed, which changed the schema. 
		//PropertyDescriptorDeleted 
       
	}

	private void dv_ListChanged(object sender, System.ComponentModel.ListChangedEventArgs e)
	{
		evProp = new EventProperties();	
		evProp.lstType = e.ListChangedType;
		evProp.NewIndex = e.NewIndex;
		evProp.OldIndex = e.OldIndex; 
	}
}
}