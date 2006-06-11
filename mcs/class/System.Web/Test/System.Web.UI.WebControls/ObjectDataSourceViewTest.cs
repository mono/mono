//
// Tests for System.Web.UI.WebControls.ObjectDataSourceView
//
// Author:
//	Chris Toshok (toshok@novell.com)
//  Konstantin Triger (kostat@mainsoft.com)
//

//
// Copyright (C) 2006 Novell, Inc (http://www.novell.com)
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

using NUnit.Framework;
using System;
using System.Configuration;
using System.Data.Common;
using System.IO;
using System.Globalization;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace MonoTests.System.Web.UI.WebControls
{
	class ObjectViewPoker : ObjectDataSourceView {
		public ObjectViewPoker (ObjectDataSource ds, string name, HttpContext context)
			: base (ds, name, context)
		{
			TrackViewState ();
		}

		public object SaveToViewState ()
		{
			return SaveViewState ();
		}

		public void LoadFromViewState (object savedState)
		{
			LoadViewState (savedState);
		}
	}

	[TestFixture]
	public class ObjectDataSourceViewTest {
		[Test]
		public void Defaults ()
		{
			ObjectDataSource ds = new ObjectDataSource ();
			ObjectViewPoker sql = new ObjectViewPoker (ds, "DefaultView", null);

			Assert.IsFalse (sql.CanDelete,"A2");
			Assert.IsFalse (sql.CanInsert,"A3");
			Assert.IsFalse (sql.CanPage,"A4");
			Assert.IsFalse (sql.CanRetrieveTotalRowCount,"A5");
			Assert.IsTrue (sql.CanSort,"A6");
			Assert.IsFalse (sql.CanUpdate,"A7");
			Assert.AreEqual (ConflictOptions.OverwriteChanges, sql.ConflictDetection, "A8");
			Assert.IsNotNull (sql.DeleteParameters, "A11");
			Assert.AreEqual (0, sql.DeleteParameters.Count, "A12");
			Assert.AreEqual ("", sql.FilterExpression, "A13");
			Assert.IsNotNull (sql.FilterParameters, "A14");
			Assert.AreEqual (0, sql.FilterParameters.Count, "A15");
			Assert.IsNotNull (sql.InsertParameters, "A18");
			Assert.AreEqual (0, sql.InsertParameters.Count, "A19");
			Assert.AreEqual ("{0}", sql.OldValuesParameterFormatString, "A20");
			Assert.IsNotNull (sql.SelectParameters, "A23");
			Assert.AreEqual (0, sql.SelectParameters.Count, "A24");
			Assert.AreEqual ("", sql.SortParameterName, "A25");
			Assert.IsNotNull (sql.UpdateParameters, "A28");
			Assert.AreEqual (0, sql.UpdateParameters.Count, "A29");
		}

		[Test]
		public void ViewState ()
		{
			ObjectDataSource ds = new ObjectDataSource ();
			ObjectViewPoker sql = new ObjectViewPoker (ds, "DefaultView", null);

			/* XXX test parameters */

			sql.ConflictDetection = ConflictOptions.CompareAllValues;
			sql.FilterExpression = "filter expression";
			sql.OldValuesParameterFormatString = "{1}";
			sql.SortParameterName = "sort parameter";

			Assert.AreEqual (ConflictOptions.CompareAllValues, sql.ConflictDetection, "A2");
			Assert.AreEqual ("filter expression", sql.FilterExpression, "A5");
			Assert.AreEqual ("{1}", sql.OldValuesParameterFormatString, "A8");
			Assert.AreEqual ("sort parameter", sql.SortParameterName, "A11");

			object state = sql.SaveToViewState();

			sql = new ObjectViewPoker (ds, "DefaultView", null);
			sql.LoadFromViewState (state);

			Assert.AreEqual (ConflictOptions.CompareAllValues, sql.ConflictDetection, "B2");
			Assert.AreEqual ("filter expression", sql.FilterExpression, "B5");
			Assert.AreEqual ("{1}", sql.OldValuesParameterFormatString, "B8");
			Assert.AreEqual ("sort parameter", sql.SortParameterName, "B11");
		}

		[Test]
		public void CanDelete ()
		{
			ObjectDataSource ds = new ObjectDataSource ();
			ObjectViewPoker sql = new ObjectViewPoker (ds, "DefaultView", null);

			sql.DeleteMethod = "DeleteMethod";
			Assert.IsTrue (sql.CanDelete, "A1");

			sql.DeleteMethod = "";
			Assert.IsFalse (sql.CanDelete, "A2");

			sql.DeleteMethod = null;
			Assert.IsFalse (sql.CanDelete, "A3");
		}

		[Test]
		public void CanInsert ()
		{
			ObjectDataSource ds = new ObjectDataSource ();
			ObjectViewPoker sql = new ObjectViewPoker (ds, "DefaultView", null);

			sql.InsertMethod = "InsertMethod";
			Assert.IsTrue (sql.CanInsert, "A1");

			sql.InsertMethod = "";
			Assert.IsFalse (sql.CanInsert, "A2");

			sql.InsertMethod = null;
			Assert.IsFalse (sql.CanInsert, "A3");
		}

		[Test]
		public void CanUpdate ()
		{
			ObjectDataSource ds = new ObjectDataSource ();
			ObjectViewPoker sql = new ObjectViewPoker (ds, "DefaultView", null);

			sql.UpdateMethod = "UpdateMethod";
			Assert.IsTrue (sql.CanUpdate, "A1");

			sql.UpdateMethod = "";
			Assert.IsFalse (sql.CanUpdate, "A2");

			sql.UpdateMethod = null;
			Assert.IsFalse (sql.CanUpdate, "A3");
		}

		[Test]
		public void OldValuesParameterFormatString ()
		{
			ObjectDataSource ds = new ObjectDataSource ();
			
			Assert.AreEqual ("{0}", ds.OldValuesParameterFormatString, "A1");

			ds.OldValuesParameterFormatString = "hi {0}";

			ObjectViewPoker sql = new ObjectViewPoker (ds, "DefaultView", null);

			Assert.AreEqual ("{0}", sql.OldValuesParameterFormatString, "A2");

			ds.OldValuesParameterFormatString = "hi {0}";

			Assert.AreEqual ("{0}", sql.OldValuesParameterFormatString, "A3");

			ds.OldValuesParameterFormatString = "{0}";
			sql.OldValuesParameterFormatString = "hi {0}";

			Assert.AreEqual ("{0}", ds.OldValuesParameterFormatString, "A4");
		}
	}

}

#endif
