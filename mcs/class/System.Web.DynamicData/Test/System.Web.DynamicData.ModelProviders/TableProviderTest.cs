//
// DynamicDataRouteTest.cs
//
// Author:
//      Marek Habersack <mhabersack@novell.com>
//
// Copyright (C) 2009 Novell Inc. http://novell.com
//

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
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Web.DynamicData;
using System.Web.DynamicData.ModelProviders;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using NUnit.Framework;

namespace MonoTests.System.Web.DynamicData.ModelProviders
{
	class MyDataModelProvider : DataModelProvider
	{
		public override object CreateContext()
		{
			throw new NotImplementedException();
		}

		public override ReadOnlyCollection<TableProvider> Tables
		{
			get { throw new NotImplementedException(); }
		}
	}

	class MyTableProvider : TableProvider
	{
		public MyTableProvider(DataModelProvider model)
			: base(model)
		{ }

		public override ReadOnlyCollection<ColumnProvider> Columns
		{
			get { throw new NotImplementedException(); }
		}

		public override IQueryable GetQuery(object context)

		{
			throw new NotImplementedException();
		}

		public void SetName(string name)
		{
			Name = name;
		}
	}

	class MyRow
	{
		public string ForeignKey
		{
			get { return "Something"; }
		}

		public int AnotherForeignKey
		{
			get { return 1; }
		}
	}
	
	[TestFixture]
	public class TableProviderTest
	{
		[Test]
		public void TableProvider_Constructor()
		{
			var tp = new MyTableProvider(null);
			Assert.AreEqual(null, tp.DataModel, "#A1");
		}
		
		[Test]
		public void TableProvider_Defaults()
		{
			var dmp = new MyDataModelProvider();
			var tp = new MyTableProvider(dmp);

			Assert.IsTrue(tp.DataModel != null, "#A1");
			Assert.AreEqual(typeof (MyDataModelProvider), tp.DataModel.GetType (), "#A2");
			Assert.AreEqual(null, tp.EntityType, "#A3");
			Assert.AreEqual(null, tp.Name, "#A4");
			Assert.AreEqual(tp.GetType ().ToString (), tp.ToString(), "#A5");
		}

		[Test]
		public void TableProvider_ToString()
		{
			var dmp = new MyDataModelProvider();
			var tp = new MyTableProvider(dmp);

			tp.SetName ("MyName");
			Assert.AreEqual(tp.Name, tp.ToString(), "#A1");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void TableProvider_EvaluateForeignKey()
		{
			var dmp = new MyDataModelProvider();
			var tp = new MyTableProvider(dmp);

			tp.EvaluateForeignKey(null, String.Empty);
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void TableProvider_EvaluateForeignKey_2()
		{
			var dmp = new MyDataModelProvider();
			var tp = new MyTableProvider(dmp);
			var row = new MyRow();

			tp.EvaluateForeignKey(row, String.Empty);
		}

		[Test]
		[ExpectedException(typeof(HttpException))]
		public void TableProvider_EvaluateForeignKey_3()
		{
			var dmp = new MyDataModelProvider();
			var tp = new MyTableProvider(dmp);
			var row = new MyRow();

			tp.EvaluateForeignKey(row, "BogusName");
		}

		[Test]
		public void TableProvider_EvaluateForeignKey_4()
		{
			var dmp = new MyDataModelProvider();
			var tp = new MyTableProvider(dmp);
			var row = new MyRow();

			object o = tp.EvaluateForeignKey(row, "ForeignKey");
			Assert.IsNotNull(o, "#A1");
			Assert.IsTrue(o is string, "#A2");
			Assert.AreEqual("Something", (string)o, "#A3");

			o = tp.EvaluateForeignKey(row, "AnotherForeignKey");
			Assert.IsNotNull(o, "#B1");
			Assert.IsTrue(o is int, "#B2");
			Assert.AreEqual(1, (int)o, "#B3");
		}
	}
}

