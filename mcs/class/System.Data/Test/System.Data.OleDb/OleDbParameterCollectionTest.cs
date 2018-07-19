//
// OleDbParameterCollectionTest.cs -
//	NUnit Test Cases for OleDbParameterCollection
//
// Author:
//	Frederik Carlier  <frederik.carlier@ugent.be>
//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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

#if !NO_OLEDB
using NUnit.Framework;
using System;
using System.Data;
using System.Data.OleDb;

namespace MonoTests.System.Data.OleDb {

	[TestFixture]
	public class OleDbParameterCollectionTest {

		[Test]
		[Category ("NotWorking")] // it tries to PInvoke LocalAlloc() and fails on non-Windows.
		public void AddWithValueTest ()
		{
			OleDbCommand command = new OleDbCommand();
			OleDbParameterCollection parameters = command.Parameters;

			// Test with string
			OleDbParameter parameter = parameters.AddWithValue("parameterName", "parameterValue");
			
			Assert.AreEqual("parameterValue", parameter.Value);
			Assert.AreEqual("parameterName", parameter.ParameterName);
			Assert.AreEqual(DbType.AnsiString, parameter.DbType);
			Assert.AreEqual(OleDbType.VarChar, parameter.OleDbType);
			Assert.AreEqual(1, parameters.Count);
			Assert.AreEqual(parameter, parameters[0]);	
		}
	}
}

#endif