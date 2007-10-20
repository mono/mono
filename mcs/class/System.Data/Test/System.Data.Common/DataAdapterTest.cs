//
// DataAdapterTest.cs - NUnit Test Cases for testing the DataAdapter class
//
// Author:
//      Miguel de Icaza (miguel@novell.com)
//      Gert Driesen (drieseng@users.sourceforge.net)
//
// Copyright (c) 2006 Novell Inc., and the individuals listed
// on the ChangeLog entries.
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
	public class DataAdapterTest
	{
		[Test]
		public void AcceptChangesDuringFill ()
		{
			DataAdapter da = new MyAdapter ();
			da.AcceptChangesDuringFill = true;
			Assert.IsTrue (da.AcceptChangesDuringFill, "#1");
			da.AcceptChangesDuringFill = false;
			Assert.IsFalse (da.AcceptChangesDuringFill, "#2");
			da.AcceptChangesDuringFill = true;
			Assert.IsTrue (da.AcceptChangesDuringFill, "#3");
		}

#if NET_2_0
		[Test]
		public void AcceptChangesDuringUpdate ()
		{
			DataAdapter da = new MyAdapter ();
			da.AcceptChangesDuringUpdate = true;
			Assert.IsTrue (da.AcceptChangesDuringUpdate, "#1");
			da.AcceptChangesDuringUpdate = false;
			Assert.IsFalse (da.AcceptChangesDuringUpdate, "#2");
			da.AcceptChangesDuringUpdate = true;
			Assert.IsTrue (da.AcceptChangesDuringUpdate, "#3");
		}
#endif

		[Test]
		public void ContinueUpdateOnError ()
		{
			DataAdapter da = new MyAdapter ();
			da.ContinueUpdateOnError = true;
			Assert.IsTrue (da.ContinueUpdateOnError, "#1");
			da.ContinueUpdateOnError = false;
			Assert.IsFalse (da.ContinueUpdateOnError, "#2");
			da.ContinueUpdateOnError = true;
			Assert.IsTrue (da.ContinueUpdateOnError, "#3");
		}

#if NET_2_0
		[Test]
		public void Fill_Direct ()
		{
			DataAdapter da = new MyAdapter ();
			DataSet ds = new DataSet ();
			try {
				da.Fill (ds);
				Assert.Fail ("#1");
			} catch (NotSupportedException ex) {
				// Specified method is not supported
				Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
			}
		}

		[Test]
		public void FillLoadOption ()
		{
			DataAdapter da = new MyAdapter ();
			da.FillLoadOption = LoadOption.PreserveChanges;
			Assert.AreEqual (LoadOption.PreserveChanges, da.FillLoadOption, "#1");
			da.FillLoadOption = LoadOption.OverwriteChanges;
			Assert.AreEqual (LoadOption.OverwriteChanges, da.FillLoadOption, "#2");
			da.FillLoadOption = LoadOption.Upsert;
			Assert.AreEqual (LoadOption.Upsert, da.FillLoadOption, "#3");
		}

		[Test]
		public void FillLoadOption_Invalid ()
		{
			DataAdapter da = new MyAdapter ();
			try {
				da.FillLoadOption = (LoadOption) 666;
				Assert.Fail ("#1");
			} catch (ArgumentOutOfRangeException ex) {
				// The LoadOption enumeration value, 666, is invalid
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsTrue (ex.Message.IndexOf ("LoadOption") != -1, "#5");
				Assert.IsTrue (ex.Message.IndexOf ("666") != -1, "#6");
				Assert.IsNotNull (ex.ParamName, "#7");
				Assert.AreEqual ("LoadOption", ex.ParamName, "#8");
			}
		}
#endif

		[Test]
		public void MissingMappingAction_Valid ()
		{
			DataAdapter da = new MyAdapter ();
			da.MissingMappingAction = MissingMappingAction.Passthrough;
			Assert.AreEqual (MissingMappingAction.Passthrough, da.MissingMappingAction, "#1");
			da.MissingMappingAction = MissingMappingAction.Ignore;
			Assert.AreEqual (MissingMappingAction.Ignore, da.MissingMappingAction, "#2");
			da.MissingMappingAction = MissingMappingAction.Error;
			Assert.AreEqual (MissingMappingAction.Error, da.MissingMappingAction, "#3");
		}

		[Test]
		public void MissingMappingAction_Invalid ()
		{
			DataAdapter da = new MyAdapter ();
			try {
				da.MissingMappingAction = (MissingMappingAction) 666;
				Assert.Fail ("#1");
#if NET_2_0
			} catch (ArgumentOutOfRangeException ex) {
				// The MissingMappingAction enumeration value, 666, is invalid
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsTrue (ex.Message.IndexOf ("MissingMappingAction") != -1, "#5");
				Assert.IsTrue (ex.Message.IndexOf ("666") != -1, "#6");
				Assert.IsNotNull (ex.ParamName, "#7");
				Assert.AreEqual ("MissingMappingAction", ex.ParamName, "#8");
			}
#else
			} catch (ArgumentException ex) {
				// The MissingMappingAction enumeration value, 666, is invalid
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsTrue (ex.Message.IndexOf ("MissingMappingAction") != -1, "#5");
				Assert.IsTrue (ex.Message.IndexOf ("666") != -1, "#6");
				Assert.IsNull (ex.ParamName, "#7");
			}
#endif
		}

		[Test]
		public void MissingSchemaAction_Valid ()
		{
			DataAdapter da = new MyAdapter ();
			da.MissingSchemaAction = MissingSchemaAction.AddWithKey;
			Assert.AreEqual (MissingSchemaAction.AddWithKey, da.MissingSchemaAction, "#1");
			da.MissingSchemaAction = MissingSchemaAction.Ignore;
			Assert.AreEqual (MissingSchemaAction.Ignore, da.MissingSchemaAction, "#2");
			da.MissingSchemaAction = MissingSchemaAction.Error;
			Assert.AreEqual (MissingSchemaAction.Error, da.MissingSchemaAction, "#3");
		}

		[Test]
		public void MissingSchemaAction_Invalid ()
		{
			DataAdapter da = new MyAdapter ();
			try {
				da.MissingSchemaAction = (MissingSchemaAction) 666;
				Assert.Fail ("#1");
#if NET_2_0
			} catch (ArgumentOutOfRangeException ex) {
				// The MissingSchemaAction enumeration value, 666, is invalid
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsTrue (ex.Message.IndexOf ("MissingSchemaAction") != -1, "#5");
				Assert.IsTrue (ex.Message.IndexOf ("666") != -1, "#6");
				Assert.IsNotNull (ex.ParamName, "#7");
				Assert.AreEqual ("MissingSchemaAction", ex.ParamName, "#8");
			}
#else
			} catch (ArgumentException ex) {
				// The MissingSchemaAction enumeration value, 666, is invalid
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsTrue (ex.Message.IndexOf ("MissingSchemaAction") != -1, "#5");
				Assert.IsTrue (ex.Message.IndexOf ("666") != -1, "#6");
				Assert.IsNull (ex.ParamName, "#7");
			}
#endif
		}

#if NET_2_0
		[Test]
		public void ReturnProviderSpecificTypes ()
		{
			DataAdapter da = new MyAdapter ();
			da.ReturnProviderSpecificTypes = true;
			Assert.IsTrue (da.ReturnProviderSpecificTypes, "#1");
			da.ReturnProviderSpecificTypes = false;
			Assert.IsFalse (da.ReturnProviderSpecificTypes, "#2");
			da.ReturnProviderSpecificTypes = true;
			Assert.IsTrue (da.ReturnProviderSpecificTypes, "#3");
		}
#endif
	}

	class MyAdapter : DataAdapter
	{
#if ONLY_1_1
		public override int Fill (DataSet dataSet)
		{
			throw new NotImplementedException ();
		}

		public override DataTable[] FillSchema (DataSet dataSet, SchemaType schemaType)
		{
			throw new NotImplementedException ();
		}

		public override IDataParameter[] GetFillParameters ()
		{
			throw new NotImplementedException ();
		}

		public override int Update (DataSet dataSet)
		{
			throw new NotImplementedException ();
		}
#endif
	}
}
