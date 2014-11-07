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
using System.Text;
using System.IO;
using System.Data;
using MonoTests.System.Data.Utils;

namespace MonoTests.System.Data
{
	[TestFixture] public class InRowChangingEventExceptionTest
	{
		private bool _EventTriggered = false;
		[Test] public void Generate()
		{
			DataTable dt = DataProvider.CreateParentDataTable();

			dt.RowChanging  += new DataRowChangeEventHandler ( Row_Changing );
			dt.Rows[0][1] = "NewValue";

			//this event must be raised in order to test the exception
			// RowChanging - Event raised
			Assert.AreEqual(true , _EventTriggered , "IRCEE1");
		}

		private void Row_Changing( object sender, DataRowChangeEventArgs e )
		{
			// InRowChangingEventException - EndEdit
			try 
			{
				e.Row.EndEdit(); //can't invoke EndEdit while in ChangingEvent
				Assert.Fail("IRCEE2: Row.EndEdit failed to raise InRowChangingEventException.");
			}
			catch (InRowChangingEventException) {}
			catch (AssertionException) { throw; }
			catch (Exception exc)
			{
				Assert.Fail("IRCEE3: Columns.Add wrong exception type. Got: " + exc);
			}
			_EventTriggered = true;
		}
	}
}
