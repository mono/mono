//
// SqlBulkCopyTest.cs - NUnit Test Cases for testing the
//                      SqlBulkCopy class
// Author:
//      Oleg Petrov (ch5oh@qip.ru)
//
// Copyright (c) 2004 Novell Inc., and the individuals listed
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
using System.Data.SqlClient;

using NUnit.Framework;

namespace MonoTests.System.Data.SqlClient {
	[TestFixture]
	public class SqlBulkCopyTest {
		private const string testFailParamNameMessage = "We have to provide the same parameter name as in original .NET";
		
		[Test] // .ctor(SqlConnection connection)
		[ExpectedException (typeof(ArgumentNullException))]
		public void ConstructorNotNull1 ()
		{
			new SqlBulkCopy ((SqlConnection)null);
		}
		
		[Test] // .ctor(string connectionString)
		[ExpectedException (typeof(ArgumentNullException))]
		public void ConstructorNotNull2 ()
		{
			new SqlBulkCopy ((string)null);
		}
		
		[Test] // .ctor(SqlConnection connection)
		[ExpectedException (typeof(ArgumentNullException))]
		public void ConstructorNotNull3 ()
		{
			try {
				new SqlBulkCopy ((SqlConnection)null);
			} catch (ArgumentNullException ane) {
				Assert.AreEqual ("connection", ane.ParamName, "#001 - " + testFailParamNameMessage);
				throw;
			}
		}
		
		[Test] // .ctor(string connectionString)
		[ExpectedException (typeof(ArgumentNullException))]
		public void ConstructorNotNull4 ()
		{
			try {
				new SqlBulkCopy ((string)null);
			} catch (ArgumentNullException ane) {
				Assert.AreEqual ("connectionString", ane.ParamName, "#002 - " + testFailParamNameMessage);
				throw;
			}
		}
		
		[Test] // .ctor(string connectionString)
		[ExpectedException (typeof(ArgumentNullException))]
		public void ConstructorNotNull5 ()
		{
			try {
				new SqlBulkCopy ((string)null, SqlBulkCopyOptions.Default);
			} catch (ArgumentNullException ane) {
				Assert.AreEqual ("connectionString", ane.ParamName, "#003 - " + testFailParamNameMessage);
				throw;
			}
		}
		
		[Test] // .ctor(string connectionString)
		[ExpectedException (typeof(ArgumentNullException))]
		public void ConstructorNotNull6 ()
		{
			try {
				new SqlBulkCopy ((SqlConnection)null, SqlBulkCopyOptions.Default, null);
			} catch (ArgumentNullException ane) {
				Assert.AreEqual ("connection", ane.ParamName, "#004 - " + testFailParamNameMessage);
				throw;
			}
		}
	}
}
