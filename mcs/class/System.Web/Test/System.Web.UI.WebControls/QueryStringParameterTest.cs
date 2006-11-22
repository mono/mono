//
// Tests for System.Web.UI.WebControls.FormView.cs 
//
// Author:
//	Merav Sudri (meravs@mainsoft.com)
//
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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

#if NET_2_0

using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace MonoTests.System.Web.UI.WebControls
{
	public class QueryStringParameterPoker : QueryStringParameter
	{
		public QueryStringParameterPoker()       
		{
			TrackViewState();
		}

		public QueryStringParameterPoker(QueryStringParameter param)
			: base(param)
		{
		}

		public QueryStringParameterPoker(string name, string queryStringField)
			: base(name, queryStringField)
		{
		}
		public QueryStringParameterPoker(string name,TypeCode type, string queryStringField)
			: base(name, type, queryStringField)
		{
		}

		public object DoEvaluate(HttpContext context, Control control)
		{
			return base.Evaluate(context, control);
		}

		public Parameter DoClone()
		{
			return base.Clone();
		}

		public object SaveState()
		{
			return SaveViewState();
		}
		public void LoadState(object o)
		{
			LoadViewState(o);
		}

		public StateBag StateBag
		{
			get { return base.ViewState; }
		}

	}

	[TestFixture]
	public class QueryStringParameterTest
	{
		[Test]
		public void QueryStringParameter_DefaultProperties()
		{
			QueryStringParameterPoker queryParam1 = new QueryStringParameterPoker();
			Assert.AreEqual("", queryParam1.QueryStringField, "DefaultQueryStringField");
			QueryStringParameterPoker queryParam2 = new QueryStringParameterPoker("Name", "id");
			Assert.AreEqual("Name", queryParam2.Name, "OverloadConstructorName1");
			Assert.AreEqual("id", queryParam2.QueryStringField, "OverloadConstructorQueryStringField1");
			QueryStringParameterPoker queryParam3 = new QueryStringParameterPoker("Name", TypeCode.Int64, "id");
			Assert.AreEqual("Name", queryParam3.Name, "OverloadConstructorName2");
			Assert.AreEqual("id", queryParam3.QueryStringField, "OverloadConstructorQueryStringField2");
			Assert.AreEqual(TypeCode.Int64, queryParam3.Type, "OverloadConstructorType2");
			QueryStringParameterPoker queryParam4 = new QueryStringParameterPoker(queryParam3);
			Assert.AreEqual("Name", queryParam4.Name, "OverloadConstructorName3");
			Assert.AreEqual("id", queryParam4.QueryStringField, "OverloadConstructorQueryStringField3");
			Assert.AreEqual(TypeCode.Int64, queryParam4.Type, "OverloadConstructorType3");
 
			
		}

		[Test]
		public void QueryStringParameter_AssignToDefaultProperties()
		{
			QueryStringParameterPoker queryParam = new QueryStringParameterPoker();
			queryParam.QueryStringField = "Test";
			Assert.AreEqual("Test", queryParam.QueryStringField, "AssignToQueryStringField"); 
			
		}

		//Protected Methods

		[Test]
		public void QueryStringParameter_Clone()
		{
			QueryStringParameterPoker queryParam = new QueryStringParameterPoker("EmployeeName", TypeCode.String, "Name");
			QueryStringParameter clonedParam = (QueryStringParameter)queryParam.DoClone();
			Assert.AreEqual("EmployeeName", clonedParam.Name, "QueryStringParameterCloneName");
			Assert.AreEqual(TypeCode.String, clonedParam.Type, "QueryStringParameterCloneType");
			Assert.AreEqual("Name", clonedParam.QueryStringField , "QueryStringParameterCloneFormField");
			
		}

		[Test]
		public void QueryStringParameter_Evaluate()
		{
			QueryStringParameterPoker queryParam = new QueryStringParameterPoker("Employee", TypeCode.Int32, "id");
			HttpRequest request = new HttpRequest(String.Empty, "http://www.mono-project.com","id=332");
			HttpResponse response = new HttpResponse(new StringWriter());
			TextBox tb = new TextBox();
			tb.ID = "id";				
			string value = (string)queryParam.DoEvaluate(null, tb);
			Assert.AreEqual(null, value, "EvaluateWhenNullContext");			
			HttpContext context = new HttpContext(request, response);
			value = (string)queryParam.DoEvaluate(context, tb);
			Assert.AreEqual("332", value, "EvaluateQueryString1");
			value = (string) queryParam.DoEvaluate (context, null);
			Assert.AreEqual ("332", value, "EvaluateQueryString1");
			request = new HttpRequest (String.Empty, "http://www.mono-project.com", "id=500");
			context = new HttpContext(request, response);
			value = (string)queryParam.DoEvaluate(context, tb);
			Assert.AreEqual("500", value, "EvaluateQueryString2");

		}

	}
}

#endif
