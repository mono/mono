//
// DbDataAdapterTest.cs - NUnit Test Cases for testing the DbDataAdapter class
//
// Author:
//      Gert Driesen (drieseng@users.sourceforge.net)
//
// Copyright (c) 2007 Gert Driesen
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
using System.Data.Common;

using NUnit.Framework;

namespace MonoTests.System.Data.Common
{
	[TestFixture]
	public class DbDataAdapterTest
	{
#if NET_2_0
		[Test]
		public void UpdateBatchSize ()
		{
			MyAdapter da = new MyAdapter ();
			try {
				da.UpdateBatchSize = 0;
				Assert.Fail ("#A1");
			} catch (NotSupportedException ex) {
				// Specified method is not supported
				Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
			}
			Assert.AreEqual (1, da.UpdateBatchSize, "#A5");

			try {
				da.UpdateBatchSize = int.MaxValue;
				Assert.Fail ("#B1");
			} catch (NotSupportedException ex) {
				// Specified method is not supported
				Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
			}
			Assert.AreEqual (1, da.UpdateBatchSize, "#B5");

			da.UpdateBatchSize = 1;
			Assert.AreEqual (1, da.UpdateBatchSize, "#C");
		}

		[Test]
		public void UpdateBatchSize_Negative ()
		{
			MyAdapter da = new MyAdapter ();
			try {
				da.UpdateBatchSize = -1;
				Assert.Fail ("#1");
			} catch (NotSupportedException ex) {
				// Specified method is not supported
				Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
			}
		}
#endif

		class MyAdapter : DbDataAdapter
		{
#if ONLY_1_1
			protected override RowUpdatedEventArgs CreateRowUpdatedEvent (DataRow dataRow, IDbCommand command,
										     StatementType statementType,
										     DataTableMapping tableMapping)
			{
				throw new NotImplementedException ();
			}

			protected override RowUpdatingEventArgs CreateRowUpdatingEvent (DataRow dataRow, IDbCommand command,
										       StatementType statementType,
										       DataTableMapping tableMapping)
			{
				throw new NotImplementedException ();
			}

			protected override void OnRowUpdated (RowUpdatedEventArgs value)
			{
				throw new NotImplementedException ();
			}

			protected override void OnRowUpdating (RowUpdatingEventArgs value)
			{
				throw new NotImplementedException ();
			}
#endif
		}
	}
}
