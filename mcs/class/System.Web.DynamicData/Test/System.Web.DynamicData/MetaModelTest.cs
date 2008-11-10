//
// MetaModelTest.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2008 Novell Inc. http://novell.com
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
using System.Collections.Specialized;
using System.Data.SqlClient;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Globalization;
using System.Reflection;
using System.Security.Permissions;
using System.Security.Principal;
using System.Web.Caching;
using System.Web.DynamicData;
using System.Web.DynamicData.ModelProviders;
using NUnit.Framework;

using MetaModel = System.Web.DynamicData.MetaModel;
using MetaTable = System.Web.DynamicData.MetaTable;

namespace MonoTests.System.Web.DynamicData
{
	[TestFixture]
	public class MetaModelTest
	{
		[Test]
		public void Default ()
		{
			// other tests create MetaModel and set Default and this test does not always run first, so it does not make sense anymore.
			//Assert.IsNull (MetaModel.Default, "#1");
			bool isFirst = (MetaModel.Default == null);
			var m = new MetaModel (); // it automatically fills Default
			if (isFirst)
				Assert.AreEqual (m, MetaModel.Default, "#2");

			Assert.IsNotNull (m.Tables, "#3");
			Assert.IsNotNull (m.VisibleTables, "#4");
			Assert.IsNotNull (m.FieldTemplateFactory, "#5");
			Assert.AreEqual ("~/DynamicData/", m.DynamicDataFolderVirtualPath, "#6");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void GetTableNull ()
		{
			new MetaModel ().GetTable ((Type) null);
		}

		[Test]
		[Category ("NotWorking")]
		public void RegisterContext ()
		{
			var m = new MetaModel ();
			try {
				m.RegisterContext (typeof (Foo));
				Assert.Fail ("#1");
			} catch (TargetInvocationException ex) {
				Assert.AreEqual ("ERROR", ex.InnerException.Message, "#2");
			} finally {
				MetaModel.ResetRegistrationException ();
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void RegisterContext2 ()
		{
			try {
				var m = new MetaModel ();
				m.RegisterContext (typeof (Bar)); // not supported
			} finally {
				MetaModel.ResetRegistrationException ();
			}
		}

		[Test]
		[ExpectedException (typeof (MissingMethodException))]
		public void RegisterContext3 ()
		{
			var m = new MetaModel ();
			try {
				// no public constructor
				m.RegisterContext (typeof (DataContext));
			} finally {
				MetaModel.ResetRegistrationException ();
			}
		}

		[Test]
		[Category ("NotWorking")]
		public void RegisterContext4 ()
		{
			var m = new MetaModel ();
			try {
				m.RegisterContext (typeof (MyDataContext1));
				Assert.AreEqual (0, m.Tables.Count, "#1-1");
				Assert.AreEqual (0, m.VisibleTables.Count, "#1-2");
				Assert.IsNotNull (MetaModel.GetModel (typeof (MyDataContext1)), "#2");
			} finally {
				MetaModel.ResetRegistrationException ();
			}
		}

		[Test]
		[Category ("NotWorking")]
		public void RegisterContext5 ()
		{
			var m = new MetaModel ();
			try {
				m.RegisterContext (typeof (MyDataContext2));
				Assert.AreEqual (1, m.Tables.Count, "#1-1");
				// VisibleTables property somehow requires live HttpContext.
				//Assert.AreEqual (1, m.VisibleTables.Count, "#1-2");
				MetaTable t = m.Tables [0];
				// Those names are only the last part before '.' (i.e. without schema name).
				Assert.AreEqual ("FooTable", t.Name, "#2-1");
				Assert.AreEqual ("FooTable", t.DisplayName, "#2-2");
				// FIXME: test it too.
				//Assert.AreEqual ("FooTable", t.DataContextPropertyName, "#2-3");
				// ListActionPath property somehow requires live HttpContext.
				//Assert.AreEqual ("FooTable/List", t.ListActionPath, "#2-4");

				Assert.AreEqual ("FooTable", t.Provider.Name, "#3-1");
			} finally {
				MetaModel.ResetRegistrationException ();
			}
		}

		[Test]
		public void ResetRegistrationException ()
		{
			MetaModel.ResetRegistrationException ();

			var m = new MetaModel ();
			try {
				m.RegisterContext (typeof (Foo));
				Assert.Fail ("#1");
			} catch (TargetInvocationException) {
			}

			try {
				m.RegisterContext (typeof (MyDataContext1));
				Assert.Fail ("#2");
			} catch (InvalidOperationException) {
			} finally {
				MetaModel.ResetRegistrationException ();
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void GetTableObject ()
		{
			// entity type 'System.Object' not found.
			new MetaModel ().GetTable (typeof (object));
		}
	}

	class MyDataContext1 : DataContext
	{
		public MyDataContext1 ()
			: base (new SqlConnection ("Data Source=localhost"))
		{
		}
	}

	[Database (Name = "MyDB1")]
	class MyDataContext2 : DataContext
	{
		public MyDataContext2 ()
			: base (new SqlConnection ("Data Source=localhost"))
		{
		}

		public Table<Foo> FooTable { get { return GetTable<Foo> (); } }
	}

	class MyDataContext3 : MyDataContext2
	{
	}

	[Table (Name = "dbo...FooTable")]
	class Foo
	{
		public Foo ()
		{
			throw new Exception ("ERROR");
		}

		[Column (Name = "Col1")]
		public string Column1 { get; set; }
	}

	[Table (Name = "BarTable")]
	class Bar
	{
		[Column (Name = "Col1")]
		public string Column1 { get; set; }

		[Column (Name = "Col2")]
		public string Column2 { get; set; }
	}
}
