//
// OdbcParameterCollectionTest.cs - NUnit Test Cases for testing the
//			  OdbcParameterCollection class
// Author:
//      Sureshkumar T (TSureshkumar@novell.com)
//	Amit Biswas (amit@amitbiswas.com)
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

#if !NO_ODBC

using System;
using System.Text;
using System.Data;
using System.Data.Odbc;

using NUnit.Framework;

namespace MonoTests.System.Data.Connected.Odbc
{
	[TestFixture]
	[Category ("odbc")]
	public class OdbcParameterCollectionTest
	{
		/// <remarks>
		/// This tests whether the value is trimmed to the
		/// given length while passing parameters
		/// </remarks>
		[Test]
		public void ParameterLengthTrimTest ()
		{
			OdbcConnection conn = ConnectionManager.Instance.Odbc.Connection;

			try {
				OdbcCommand cmd = conn.CreateCommand ();
				cmd.CommandType = CommandType.Text;
				cmd.CommandText = "SELECT count(*) FROM employee WHERE fname=?";

				OdbcParameter param = cmd.Parameters.Add("@fname", OdbcType.VarChar, 15);
				param.Value = DateTime.Now.ToString ();
				Assert.AreEqual (15, param.Size, "#1");
				Assert.AreEqual (0, cmd.ExecuteScalar(), "#2");
			} finally {
				ConnectionManager.Instance.Odbc.CloseConnection ();
			}
		}

		[Test]
		public void InsertTest ()
		{
			OdbcConnection conn = ConnectionManager.Instance.Odbc.Connection;

			try {
				OdbcCommand OdbcCmd = conn.CreateCommand ();
				OdbcCmd.CommandType = CommandType.Text;
				OdbcCmd.CommandText = "SELECT fname FROM employee WHERE lname=? AND age=?";

				/*OdbcParameter p1Lname = */OdbcCmd.Parameters.Add ("@lname", OdbcType.Text, 15);
				OdbcParameter p2Age = new OdbcParameter (); //not initialized and not yet added
				OdbcParameter p3Tmp = new OdbcParameter ("p3", "abc"); //not added

				Assert.AreEqual (1, OdbcCmd.Parameters.Count, "#1 Initialization error, the collection does not contain desired no. of parameters");
				OdbcCmd.Parameters.Insert (1, p2Age); //Inserting the second parameter
				Assert.AreEqual (2, OdbcCmd.Parameters.Count, "#2 The collection should contain 2 parameters");
				
				//inserting at upper boundary
				OdbcCmd.Parameters.Insert (OdbcCmd.Parameters.Count, p3Tmp); //Inserting the third parameter, with name and value at index = count
				Assert.AreEqual (3, OdbcCmd.Parameters.Count, "#3 The collection should contain 2 parameters");

				//check the inserted positions
				Assert.AreEqual (0, OdbcCmd.Parameters.IndexOf ("@lname"), "#4 The first parameter must be at index 0");
				Assert.AreEqual (1, OdbcCmd.Parameters.IndexOf (p2Age), "#5 The inserted parameter p2Age must be at index 1");
				Assert.AreEqual (2, OdbcCmd.Parameters.IndexOf (p3Tmp), "#6 The inserted parameter p3Tmp must be at index 2");
				Assert.AreEqual (2, OdbcCmd.Parameters.IndexOf ("p3"), "#7 The inserted parameter p3 must be at index 2");
				Assert.AreEqual (-1, OdbcCmd.Parameters.IndexOf ("non-existing-parameter"), "#8 Non-existing parameters should return -1");
				
				//check for default names and default values
				Assert.AreEqual ("Parameter1", OdbcCmd.Parameters[1].ParameterName, "#9 Parameters inserted without any name must get a default name");
				Assert.AreEqual (null, OdbcCmd.Parameters[1].Value, "#10 Parameters inserted without any value must have null value");
				
				Assert.AreEqual ("p3", OdbcCmd.Parameters[2].ParameterName, "#11 Parameters inserted without any name must get a default name");
				Assert.AreEqual ("abc", OdbcCmd.Parameters[2].Value, "#12 Parameters inserted without any value must have null value");
				
				Assert.AreEqual (OdbcType.NVarChar,OdbcCmd.Parameters[1].OdbcType, "#13 Parameters with null value must be of type NVarChar");
				Assert.AreEqual (OdbcType.Text,OdbcCmd.Parameters[0].OdbcType, "#14 Parameter at index 0 is of type Text");
			} finally {
				ConnectionManager.Instance.Odbc.CloseConnection ();
			}
		}

		/// <remarks>
		/// Inserting parameters in between the collection should not
		/// overwrite the existing parameters
		/// </remarks>
		[Test]
		public void InsertNoOverwriteTest ()
		{
			OdbcConnection conn = ConnectionManager.Instance.Odbc.Connection;

			try {
				OdbcCommand OdbcCmd = conn.CreateCommand ();
				OdbcCmd.CommandType = CommandType.Text;
				OdbcCmd.CommandText = "SELECT fname FROM employee WHERE lname=? AND age=?";

				OdbcParameter p1Lname = OdbcCmd.Parameters.Add ("@lname", OdbcType.Text, 15);
				OdbcParameter p2Age = OdbcCmd.Parameters.Add ("@age", OdbcType.Int, 2);
				OdbcParameter p3Tmp = OdbcCmd.Parameters.Add ("@Tmp", OdbcType.Text, 15);
				OdbcParameter p4Tmp = new OdbcParameter ();
				
				Assert.AreEqual (3, OdbcCmd.Parameters.Count, "#1 Initialization error, the collection does not contain desired no. of parameters");
				OdbcCmd.Parameters.Insert (1, p4Tmp); //Inserting at index 1
				Assert.AreEqual (4, OdbcCmd.Parameters.Count, "#2 Collection should contain 4 parameters");

				//Existing parameters should not be overwritten
				Assert.AreEqual (0, OdbcCmd.Parameters.IndexOf (p1Lname), "#3 The parameter at index 0 should not change");
				Assert.AreEqual (1, OdbcCmd.Parameters.IndexOf (p4Tmp), "#4 The inserted parameter should be at index 1");
				Assert.AreEqual (2, OdbcCmd.Parameters.IndexOf (p2Age), "#5 The parameter at index 1 should be at index 2 after inserting");
				Assert.AreEqual (3, OdbcCmd.Parameters.IndexOf (p3Tmp), "#6 The parameter at index 2 should be at index 3 after inserting");
				Assert.AreEqual (-1, OdbcCmd.Parameters.IndexOf ("non-existing-parameter"), "#7 Non-existing parameters should return -1");
				
				//check for default names and default values
				Assert.AreEqual ("Parameter1", OdbcCmd.Parameters[1].ParameterName, "#8 Parameters inserted without any name must get a default name");
				Assert.AreEqual (null, OdbcCmd.Parameters[1].Value, "#9 Parameters inserted without any value must have null value");
			} finally {
				ConnectionManager.Instance.Odbc.CloseConnection ();
			}
		}

		[Test]
		public void InsertNullTest ()
		{
			OdbcConnection conn = ConnectionManager.Instance.Odbc.Connection;

			try {
				OdbcCommand OdbcCmd = conn.CreateCommand ();
				OdbcCmd.CommandType = CommandType.Text;
				OdbcCmd.CommandText = "SELECT fname FROM employee WHERE lname=? AND age=?";

				/*OdbcParameter p1Lname = */OdbcCmd.Parameters.Add ("@lname", OdbcType.Text, 15);

				Assert.AreEqual (1, OdbcCmd.Parameters.Count, "#1 Initialization error, the collection does not contain desired no. of parameters");
				try {
					OdbcCmd.Parameters.Insert (1, null);
					Assert.Fail ("Expected exception ArgumentNullException was not thrown");
				} catch (ArgumentNullException) {
					Assert.AreEqual (1, OdbcCmd.Parameters.Count, "#2 The collection must contain only one parameter after Insert failed");
				}
			} finally {
				ConnectionManager.Instance.Odbc.CloseConnection ();
			}
		}

		[Test]
		public void InsertEmptyTest ()
		{
			OdbcConnection conn = ConnectionManager.Instance.Odbc.Connection;

			try {
				OdbcCommand OdbcCmd = conn.CreateCommand ();
				OdbcCmd.CommandType = CommandType.Text;
				OdbcCmd.CommandText = "SELECT fname FROM employee WHERE lname=? AND age=?";

				/*OdbcParameter p1Lname = */OdbcCmd.Parameters.Add ("@lname", OdbcType.Text, 15);

				Assert.AreEqual (1, OdbcCmd.Parameters.Count, "#1 Initialization error, the collection does not contain desired no. of parameters");
				try {
					OdbcCmd.Parameters.Insert (1, string.Empty);
					Assert.Fail ("Expected exception InvalidCastException was not thrown");
				} catch (InvalidCastException) {
					Assert.AreEqual (1, OdbcCmd.Parameters.Count, "#2 The collection must contain only one parameter after Insert failed");
				}
					
			} finally {
				ConnectionManager.Instance.Odbc.CloseConnection ();
			}
		}

		[Test]
		public void InsertAlreadyContainedParameterTest ()
		{
			OdbcConnection conn = ConnectionManager.Instance.Odbc.Connection;

			try {
				OdbcCommand OdbcCmd = conn.CreateCommand ();
				OdbcCmd.CommandType = CommandType.Text;
				OdbcCmd.CommandText = "SELECT fname FROM employee WHERE lname=? AND age=?";

				/*OdbcParameter p1Lname = */OdbcCmd.Parameters.Add ("@lname", OdbcType.Text, 15);
				OdbcParameter p2Age = new OdbcParameter (); //not initialized and not yet added

				Assert.AreEqual (1, OdbcCmd.Parameters.Count, "#1 Initialization error, the collection does not contain desired no. of parameters");
				OdbcCmd.Parameters.Insert (1, p2Age);
				Assert.AreEqual (2, OdbcCmd.Parameters.Count, "#2 The collection must contain 2 parameters");
				try {
					OdbcCmd.Parameters.Insert (2, p2Age); //p2Age is already contained
					Assert.Fail ("Expected exception ArgumentException not thrown");
				} catch (ArgumentException) {
					Assert.AreEqual (2, OdbcCmd.Parameters.Count, "#3 The collection must contain 2 parameters after Insert failed");
				}
			} finally {
				ConnectionManager.Instance.Odbc.CloseConnection ();
			}
		}

		[Test]
		public void InsertArgumentGreaterThanCountTest ()
		{
			OdbcConnection conn = ConnectionManager.Instance.Odbc.Connection;

			try {
				OdbcCommand OdbcCmd = conn.CreateCommand ();
				OdbcCmd.CommandType = CommandType.Text;
				OdbcCmd.CommandText = "SELECT fname FROM employee WHERE lname=? AND age=?";
				/*OdbcParameter p1Lname = */OdbcCmd.Parameters.Add ("@lname", OdbcType.Text, 15);
				OdbcParameter p2Age = new OdbcParameter ();

				Assert.AreEqual (1, OdbcCmd.Parameters.Count, "#1 Initialization error, the collection does not contain desired no. of parameters");
				try {
					OdbcCmd.Parameters.Insert (OdbcCmd.Parameters.Count + 1, p2Age); //Inserting with wrong index
					Assert.Fail ("Expected Exception ArgumentOutOfRangeException not thrown");
				} catch (ArgumentOutOfRangeException) {
					Assert.AreEqual (1, OdbcCmd.Parameters.Count, "#2 The collection must contain only 1 parameter after Insert failed");
				}
			} finally {
				ConnectionManager.Instance.Odbc.CloseConnection ();
			}
		}

		[Test]
		public void InsertNegativeArgumentTest ()
		{
			OdbcConnection conn = ConnectionManager.Instance.Odbc.Connection;

			try {
				OdbcCommand OdbcCmd = conn.CreateCommand ();
				OdbcCmd.CommandType = CommandType.Text;
				OdbcCmd.CommandText = "SELECT fname FROM employee WHERE lname=? AND age=?";
				/*OdbcParameter p1Lname = */OdbcCmd.Parameters.Add ("@lname", OdbcType.Text, 15);
				OdbcParameter p2Age = new OdbcParameter ();

				Assert.AreEqual (1, OdbcCmd.Parameters.Count, "#1 Initialization error, the collection does not contain desired no. of parameters");
				try {
					OdbcCmd.Parameters.Insert (-3, p2Age); //Insert with negative index
					Assert.Fail ("Expected Exception ArgumentOutOfRangeException not thrown");
				} catch (ArgumentOutOfRangeException) {
					Assert.AreEqual (1, OdbcCmd.Parameters.Count, "#2 The collection must contain only 1 parameter after Insert failed");
				}
			} finally {
				ConnectionManager.Instance.Odbc.CloseConnection ();
			}
		}

		[Test]
		public void InsertNonOdbcParameterTest ()
		{
			OdbcConnection conn = ConnectionManager.Instance.Odbc.Connection;

			try {
				OdbcCommand OdbcCmd = conn.CreateCommand ();
				OdbcCmd.CommandType = CommandType.Text;
				OdbcCmd.CommandText = "SELECT fname FROM employee WHERE lname=? AND age=?";

				/*OdbcParameter p1Lname = */OdbcCmd.Parameters.Add ("@lname", OdbcType.Text, 15);

				Assert.AreEqual (1, OdbcCmd.Parameters.Count, "#1 Initialization error, the collection does not contain desired no. of parameters");
				try {
					OdbcCmd.Parameters.Insert (1, 4);
					Assert.Fail ("Expected exception InvalidCastException was not thrown");
				} catch (InvalidCastException) {
					Assert.AreEqual (1, OdbcCmd.Parameters.Count, "#2 The collection must contain only 1 parameter after Insert failed");
				}
			} finally {
				ConnectionManager.Instance.Odbc.CloseConnection ();
			}
		}


		[Test]
		public void AddRangeTest ()
		{
			OdbcConnection conn = ConnectionManager.Instance.Odbc.Connection;

			try {
				OdbcCommand OdbcCmd = conn.CreateCommand ();
				OdbcCmd.CommandType = CommandType.Text;
				OdbcCmd.CommandText = "SELECT fname FROM employee WHERE lname=? AND age=?";
				/*OdbcParameter p1Lname = */OdbcCmd.Parameters.Add ("@lname", OdbcType.Text, 15);
				OdbcParameter p2Age = new OdbcParameter (); //not initialized and not yet added
				OdbcParameter p3Tmp = new OdbcParameter (); //not initialized and not yet added
				OdbcParameter p4Tmp = new OdbcParameter (); //not initialized and not yet added
				OdbcParameter p5Tmp = new OdbcParameter (); //not initialized and not yet added

				Assert.AreEqual (1, OdbcCmd.Parameters.Count, "#1 Initialization error, the collection does not contain desired no. of parameters");
				OdbcParameter [] paramArray = new OdbcParameter [4];
				paramArray [0] = p2Age;
				paramArray [1] = p3Tmp;
				paramArray [2] = p4Tmp;
				paramArray [3] = p5Tmp;
				OdbcCmd.Parameters.AddRange (paramArray);
				Assert.AreEqual (5, OdbcCmd.Parameters.Count, "#2 The array elements are not added");
				Assert.AreEqual (1, OdbcCmd.Parameters.IndexOf (p2Age), "#3 The first element must be added after the last parameter");
				Assert.AreEqual (4, OdbcCmd.Parameters.IndexOf (p5Tmp), "#4 Not all elements are added");
				Assert.AreEqual ("Parameter1", OdbcCmd.Parameters[1].ParameterName, "#5 Parameters added without any name must get a default name");
				Assert.AreEqual (OdbcType.NVarChar,OdbcCmd.Parameters[1].OdbcType, "#6 Parameters with null value must be of type NVarChar");
			} finally {
				ConnectionManager.Instance.Odbc.CloseConnection ();
			}
		}

		//<remarks>
		//If a parameter in the range is already contained, all the parameters before it are added and
		//all the parameters after it are rejected
		//</remarks>
		[Test]
		public void AddRangeParameterAlreadyContainedTest ()
		{
			OdbcConnection conn = ConnectionManager.Instance.Odbc.Connection;

			try {
				OdbcCommand OdbcCmd = conn.CreateCommand ();
				OdbcCmd.CommandType = CommandType.Text;
				OdbcCmd.CommandText = "SELECT fname FROM employee WHERE lname=? AND age=?";
				OdbcParameter p1Lname = OdbcCmd.Parameters.Add ("@lname", OdbcType.Text, 15);
				OdbcParameter p2Age = new OdbcParameter ();
				OdbcParameter p3Tmp = new OdbcParameter ();

				Assert.AreEqual (1, OdbcCmd.Parameters.Count, "#1 Initialization error, the collection does not contain desired no. of parameters");
				OdbcParameter [] paramArray = new OdbcParameter [3];
				paramArray [0] = p2Age;
				paramArray [1] = p1Lname; //p1Lname is already contained
				paramArray [2] = p3Tmp;
				try {
					OdbcCmd.Parameters.AddRange (paramArray);
					Assert.Fail ("Expected Exception ArgumentException not thrown");
				} catch (ArgumentException){
					Assert.AreEqual (2, OdbcCmd.Parameters.Count, "#2 The collection must contain excatly 2 elements after AddRange failed for the third element");
				}
			} finally {
				ConnectionManager.Instance.Odbc.CloseConnection ();
			}
		}

		//<remarks>
		//If a parameter in the range is null, all the elements in the range are rejected
		//</remarks>
		[Test]
		public void AddRangeArgumentNullExceptionTest ()
		{
			OdbcConnection conn = ConnectionManager.Instance.Odbc.Connection;

			try {
				OdbcCommand OdbcCmd = conn.CreateCommand ();
				OdbcCmd.CommandType = CommandType.Text;
				OdbcCmd.CommandText = "SELECT fname FROM employee WHERE lname=? AND age=?";
				/*OdbcParameter p1Lname = */OdbcCmd.Parameters.Add ("@lname", OdbcType.Text, 15);
				OdbcParameter p2Age = new OdbcParameter ();
				OdbcParameter p3Tmp = new OdbcParameter ();

				OdbcParameter [] paramArray = new OdbcParameter [3];
				paramArray [0] = p2Age;
				paramArray [1] = p3Tmp;
				paramArray [2] = null;
				Assert.AreEqual (1, OdbcCmd.Parameters.Count, "#1 Initialization error, the collection does not contain desired no. of parameters");
				try {
					OdbcCmd.Parameters.AddRange (paramArray);
					Assert.Fail ("Expected Exception ArgumentNullException not thrown");
				} catch (ArgumentNullException){
					Assert.AreEqual (1, OdbcCmd.Parameters.Count, "#2 If any of the parameters in the range is null, none of them should be added");
				}
			} finally {
				ConnectionManager.Instance.Odbc.CloseConnection ();
			}
		}

		[Test]
		public void AddRangeParameterContainedInAnotherCollTest ()
		{
			OdbcConnection conn = ConnectionManager.Instance.Odbc.Connection;

			try {
				OdbcCommand OdbcCmd = conn.CreateCommand ();
				OdbcCmd.CommandType = CommandType.Text;
				OdbcCmd.CommandText = "SELECT fname FROM employee WHERE lname=? AND age=?";

				OdbcCommand OdbcCmd2 = conn.CreateCommand ();
				OdbcCmd2.CommandType = CommandType.Text;
				OdbcCmd2.CommandText = "SELECT lname FROM employee WHERE fname=? AND age=?";

				OdbcParameter p1Lname = OdbcCmd.Parameters.Add ("@lname", OdbcType.Text, 15);
				OdbcParameter p2Age = new OdbcParameter ();
				OdbcParameter p3Tmp = new OdbcParameter ();

				OdbcParameter [] paramArray = new OdbcParameter [3];
				paramArray [0] = p2Age;
				paramArray [1] = p1Lname; //p1Lname is already contained in Odbccmd
				paramArray [2] = p3Tmp;
				Assert.AreEqual (1, OdbcCmd.Parameters.Count, "#1 Initialization error, the parameter collection of OdbcCmd should contain only 1 parameter");
				Assert.AreEqual (0, OdbcCmd2.Parameters.Count, "#2 Initialization error, the parameter collection of OdbcCmd2 should not contain any parameters");
				try {
					OdbcCmd2.Parameters.AddRange (paramArray);
					Assert.Fail ("Expected Exception ArgumentException not thrown");
				} catch (ArgumentException) {
					Assert.AreEqual (1, OdbcCmd.Parameters.Count, "#3 The parameter collection of OdbcCmd should not change");
					Assert.AreEqual (1, OdbcCmd2.Parameters.Count, "#4 All the elements before the invalid element must be added to the collection of OdbcCmd2");
				}
			} finally {
				ConnectionManager.Instance.Odbc.CloseConnection ();
			}
		}

		[Test]
		public void AddRangeMultiDimensionalArrayTest ()
		{
			OdbcConnection conn = ConnectionManager.Instance.Odbc.Connection;

			try {
				OdbcCommand OdbcCmd = conn.CreateCommand ();
				OdbcCmd.CommandType = CommandType.Text;
				OdbcCmd.CommandText = "SELECT fname FROM employee WHERE lname=? AND age=?";
				/*OdbcParameter p1Lname = */OdbcCmd.Parameters.Add ("@lname", OdbcType.Text, 15);
				OdbcParameter p2Age = new OdbcParameter ();
				OdbcParameter p3Tmp = new OdbcParameter ();
				OdbcParameter p4Tmp = new OdbcParameter ();
				OdbcParameter p5Tmp = new OdbcParameter ();

				Assert.AreEqual (1, OdbcCmd.Parameters.Count, "#1 Initialization error, the collection does not contain desired no. of parameters");
				OdbcParameter [,] paramArray = new OdbcParameter [2, 2];
				paramArray [0, 0] = p2Age;
				paramArray [0, 1] = p3Tmp;
				paramArray [1, 0] = p4Tmp;
				paramArray [1, 1] = p5Tmp;
				OdbcCmd.Parameters.AddRange (paramArray);
				Assert.AreEqual (5, OdbcCmd.Parameters.Count, "#2 Not all four parameters of the 2x2 Array are added");
				Assert.AreEqual (1, OdbcCmd.Parameters.IndexOf (p2Age), "#3 The first element must be added after the last parameter");
				Assert.AreEqual (4, OdbcCmd.Parameters.IndexOf (p5Tmp), "#4 Not all elements are added");
				Assert.AreEqual (OdbcType.NVarChar,OdbcCmd.Parameters[4].OdbcType, "#5 Parameters with null value must be of type NVarChar");
			} finally {
				ConnectionManager.Instance.Odbc.CloseConnection ();
			}
		}

		[Test]
		public void AddRangeArrayValuesArgumentNullExceptionTest ()
		{
			OdbcConnection conn = ConnectionManager.Instance.Odbc.Connection;

			try {
				OdbcCommand OdbcCmd = conn.CreateCommand ();
				OdbcCmd.CommandType = CommandType.Text;
				OdbcCmd.CommandText = "SELECT fname FROM employee WHERE lname=? AND age=?";
				/*OdbcParameter p1Lname = */OdbcCmd.Parameters.Add ("@lname", OdbcType.Text, 15);
				OdbcParameter p2Age = new OdbcParameter ();
				OdbcParameter p3Tmp = new OdbcParameter ();
				OdbcParameter p5Tmp = new OdbcParameter ();

				Assert.AreEqual (1, OdbcCmd.Parameters.Count, "#1 Initialization error, the collection does not contain desired no. of parameters");
				OdbcParameter [,] paramArray = new OdbcParameter [2, 2];
				paramArray [0, 0] = p2Age;
				paramArray [0, 1] = p3Tmp;
				paramArray [1, 0] = null;
				paramArray [1, 1] = p5Tmp;
				try {
					OdbcCmd.Parameters.AddRange (paramArray);
					Assert.Fail ("Expected Exception ArgumentOutOfRangeException not thrown");
				} catch (ArgumentNullException) {
					Assert.AreEqual (1, OdbcCmd.Parameters.Count, "#2 None of the elememts must be added if any one of them is null");
				}
			} finally {
				ConnectionManager.Instance.Odbc.CloseConnection ();
			}
		}

		//<remarks>
		//Tests all the three overloads of Contains
		//</remarks>
		[Test]
		public void ContainsTest ()
		{
			OdbcConnection conn = ConnectionManager.Instance.Odbc.Connection;

			try {
				OdbcCommand OdbcCmd = conn.CreateCommand ();
				OdbcCmd.CommandType = CommandType.Text;
				OdbcCmd.CommandText = "SELECT fname FROM employee WHERE lname=? AND age=?";
				OdbcParameter p1Lname = OdbcCmd.Parameters.Add ("@lname", OdbcType.Text, 15);
				/*OdbcParameter p2Age = */OdbcCmd.Parameters.Add ("@age", OdbcType.Int, 2);
				OdbcParameter p3Tmp = new OdbcParameter ();
				OdbcCmd.Parameters.Insert (2, p3Tmp);

				Assert.AreEqual (3, OdbcCmd.Parameters.Count, "#1 Not all parameters are added");
				Assert.IsTrue (OdbcCmd.Parameters.Contains (p1Lname), "#2 Contains failed for OdbcParameter value");
				Assert.IsTrue (OdbcCmd.Parameters.Contains ("@age"), "#3 Contains failed for string value");
				Assert.IsTrue (OdbcCmd.Parameters.Contains (p3Tmp), "#4 Contains failed for object value");
				Assert.IsFalse (OdbcCmd.Parameters.Contains (null), "#5 Contains must return false for null value");
				Assert.IsFalse (OdbcCmd.Parameters.Contains (""), "#6 Contains must return false for empty string");
				Assert.IsFalse (OdbcCmd.Parameters.Contains ((Object)null), "#6 Contains must return false for empty string");
				Assert.IsFalse (OdbcCmd.Parameters.Contains ((String)null), "#6 Contains must return false for empty string");
			} finally {
				ConnectionManager.Instance.Odbc.CloseConnection ();
			}
		}

		[Test]
		public void ContainsNonOdbcParameterTest ()
		{
			OdbcConnection conn = ConnectionManager.Instance.Odbc.Connection;

			try {
				OdbcCommand OdbcCmd = conn.CreateCommand ();
				OdbcCmd.CommandType = CommandType.Text;
				OdbcCmd.CommandText = "SELECT fname FROM employee WHERE lname=? AND age=?";
				/*OdbcParameter p1Lname = */OdbcCmd.Parameters.Add ("@lname", OdbcType.Text, 15);

				Assert.AreEqual (1, OdbcCmd.Parameters.Count, "#1 The collection must contain only one parameter");
				try{
					Assert.IsFalse (OdbcCmd.Parameters.Contains (4), "#2 Contains must return false for non-odbcParameter arguments");
					Assert.Fail ("Expected Exception InvalidCastException not thrown");
				} catch (InvalidCastException) {
					Assert.AreEqual (1, OdbcCmd.Parameters.Count, "#3 The collection must contain only one parameter");
				}
			} finally {
				ConnectionManager.Instance.Odbc.CloseConnection ();
			}
		}

		[Test]
		public void ContainsCaseSensitivityTest ()
		{
			OdbcConnection conn = ConnectionManager.Instance.Odbc.Connection;

			try {
				OdbcCommand OdbcCmd = conn.CreateCommand ();
				OdbcCmd.CommandType = CommandType.Text;
				OdbcCmd.CommandText = "SELECT fname FROM employee WHERE lname=? AND age=?";

				/*OdbcParameter p1Lname = */OdbcCmd.Parameters.Add ("@lname", OdbcType.Text, 15);

				Assert.AreEqual (1, OdbcCmd.Parameters.Count, "#1 Initialization error, the collection does not contain desired no. of parameters");

				Assert.AreEqual (true, OdbcCmd.Parameters.Contains ("@lname"), "#2 Case sensitivity failed for Contains, should be case insensitive");
				Assert.AreEqual (true, OdbcCmd.Parameters.Contains ("@LNAME"), "#3 Case sensitivity failed for Contains, should be case insensitive");
				Assert.AreEqual (true, OdbcCmd.Parameters.Contains ("@LnAmE"), "#4 Case sensitivity failed for Contains, should be case insensitive");
			} finally {
				ConnectionManager.Instance.Odbc.CloseConnection ();
			}
		}

		[Test]
		public void ContainsNotMineTest ()
		{
			OdbcConnection conn = ConnectionManager.Instance.Odbc.Connection;

			try {
				OdbcCommand OdbcCmd1 = conn.CreateCommand ();
				OdbcCommand OdbcCmd2 = conn.CreateCommand ();
				OdbcCmd1.CommandType = CommandType.Text;
				OdbcCmd1.CommandText = "SELECT fname FROM employee WHERE lname=? AND age=?";
				OdbcCmd2.CommandType = CommandType.Text;
				OdbcCmd2.CommandText = "SELECT fname FROM employee WHERE lname=? AND age=?";

				OdbcParameter p1 = OdbcCmd1.Parameters.Add ("@lname", OdbcType.Text, 15);
				OdbcParameter p2 = OdbcCmd2.Parameters.Add ("@lname", OdbcType.Text, 15);

				Assert.IsTrue (OdbcCmd1.Parameters.Contains (p1));
				Assert.IsFalse (OdbcCmd1.Parameters.Contains (p2));
			} finally {
				ConnectionManager.Instance.Odbc.CloseConnection ();
			}
		}

		[Test]
		public void IndexOfTest ()
		{
			OdbcConnection conn = ConnectionManager.Instance.Odbc.Connection;

			try {
				OdbcCommand OdbcCmd = conn.CreateCommand ();
				OdbcCmd.CommandType = CommandType.Text;
				OdbcCmd.CommandText = "SELECT fname FROM employee WHERE lname=? AND age=?";
				OdbcParameter p1Lname = OdbcCmd.Parameters.Add ("@lname", OdbcType.Text, 15);
				OdbcParameter p2Age = OdbcCmd.Parameters.Add ("@age", OdbcType.Int, 2);
				OdbcParameter p3Tmp = new OdbcParameter ();
				Assert.AreEqual (2, OdbcCmd.Parameters.Count, "#1 Initialization error, the collection does not contain desired no. of parameters");

				Assert.AreEqual (0, OdbcCmd.Parameters.IndexOf (p1Lname), "#1 first parameter not with index 0");
				Assert.AreEqual (1, OdbcCmd.Parameters.IndexOf (p2Age), "#2 second parameter not with index 1");
				Assert.AreEqual (-1, OdbcCmd.Parameters.IndexOf (p3Tmp), "#3 non-existing parameter should return -1");
				Assert.AreEqual (-1, OdbcCmd.Parameters.IndexOf (null), "#4 null value should return -1");
				Assert.AreEqual (-1, OdbcCmd.Parameters.IndexOf (""), "#5 Empty string parameter should return -1");
				Assert.AreEqual (-1, OdbcCmd.Parameters.IndexOf ((string)null), "#6 Null string parameter should return -1");
				Assert.AreEqual (-1, OdbcCmd.Parameters.IndexOf ("foo"), "#7 non-existing string parameter should return -1");
				Assert.AreEqual (1, OdbcCmd.Parameters.IndexOf ((Object)p2Age), "#8 second parameter passed as Object did not return index 1");
				Assert.AreEqual (-1, OdbcCmd.Parameters.IndexOf ((Object)p3Tmp), "#9 non-existing parameter passed as Object did not return index -1");
				Assert.AreEqual (-1, OdbcCmd.Parameters.IndexOf ((Object)null), "#10 null parameter passed as Object should return index -1");
			} finally {
				ConnectionManager.Instance.Odbc.CloseConnection ();
			}
		}

		[Test]
		public void IndexOfCaseSensitivityTest ()
		{
			OdbcConnection conn = ConnectionManager.Instance.Odbc.Connection;

			try {
				OdbcCommand OdbcCmd = conn.CreateCommand ();
				OdbcCmd.CommandType = CommandType.Text;
				OdbcCmd.CommandText = "SELECT fname FROM employee WHERE lname=? AND age=?";
				/*OdbcParameter p1Lname = */OdbcCmd.Parameters.Add ("@lname", OdbcType.Text, 15);
				/*OdbcParameter p2Age = */OdbcCmd.Parameters.Add ("@AGE", OdbcType.Int, 2);
				Assert.AreEqual (2, OdbcCmd.Parameters.Count, "#1 Initialization error, the collection does not contain desired no. of parameters");

				Assert.AreEqual (0, OdbcCmd.Parameters.IndexOf ("@lname"), "#2 Case sensitivity failed for IndexOf, should be case insensitive");
				Assert.AreEqual (0, OdbcCmd.Parameters.IndexOf ("@LNAME"), "#3 Case sensitivity failed for IndexOf, should be case insensitive");
				Assert.AreEqual (1, OdbcCmd.Parameters.IndexOf ("@AGE"), "#4 Case sensitivity failed for IndexOf, should be case insensitive");
				Assert.AreEqual (1, OdbcCmd.Parameters.IndexOf ("@age"), "#5 Case sensitivity failed for IndexOf, should be case insensitive");
			} finally {
				ConnectionManager.Instance.Odbc.CloseConnection ();
			}
		}

		[Test]
		public void IndexOfNonOdbcParameterTest ()
		{
			OdbcConnection conn = ConnectionManager.Instance.Odbc.Connection;

			try {
				OdbcCommand OdbcCmd = conn.CreateCommand ();
				OdbcCmd.CommandType = CommandType.Text;
				OdbcCmd.CommandText = "SELECT fname FROM employee WHERE lname=? AND age=?";
				/*OdbcParameter p1Lname = */OdbcCmd.Parameters.Add ("@lname", OdbcType.Text, 15);

				Assert.AreEqual (1, OdbcCmd.Parameters.Count, "#1 The collection must contain only one parameter");
				try{
					Assert.AreEqual (-1, OdbcCmd.Parameters.IndexOf (4), "#2 IndexOf must return -1 for non-odbcParameter arguments");
					Assert.Fail ("Expected Exception InvalidCastException not thrown");
				} catch (InvalidCastException) {
					Assert.AreEqual (1, OdbcCmd.Parameters.Count, "#3 The collection must contain only one parameter");
				}
			} finally {
				ConnectionManager.Instance.Odbc.CloseConnection ();
			}
		}

		[Test]
		public void CopyToTest ()
		{
			OdbcConnection conn = ConnectionManager.Instance.Odbc.Connection;

			try {
				OdbcCommand OdbcCmd = conn.CreateCommand ();
				OdbcCmd.CommandType = CommandType.Text;
				OdbcCmd.CommandText = "SELECT fname FROM employee WHERE lname=? AND age=?";
				/*OdbcParameter p1Lname = */OdbcCmd.Parameters.Add ("@lname", OdbcType.Text, 15);
				/*OdbcParameter p2Age = */OdbcCmd.Parameters.Add ("@age", OdbcType.Int, 2);

				Assert.AreEqual (2, OdbcCmd.Parameters.Count, "#1 Initialization error, the collection does not contain desired no. of parameters");
				OdbcParameter [] DestinationParamArray = new OdbcParameter [4];

				OdbcCmd.Parameters.CopyTo (DestinationParamArray, 1); //starting at 1 instead of 0

				Assert.AreEqual (4, DestinationParamArray.Length, "#2 The array length should not change");
				Assert.AreEqual ("@lname", DestinationParamArray [1].ParameterName, "#3 The first parameter must be copied to array at index 1");
				Assert.AreEqual ("@age", DestinationParamArray [2].ParameterName, "#4 The second parameter must be copied to array at index 2");
				Assert.AreEqual (null, DestinationParamArray [0], "#5 The remaining elements must remain un-initialized");
				Assert.AreEqual (null, DestinationParamArray [3], "#6 The remaining elements must remain un-initialized");
			} finally {
				ConnectionManager.Instance.Odbc.CloseConnection ();
			}
		}

		[Test]
		public void CopyToArgumentExceptionTest ()
		{
			OdbcConnection conn = ConnectionManager.Instance.Odbc.Connection;

			try {
				OdbcCommand OdbcCmd = conn.CreateCommand ();
				OdbcCmd.CommandType = CommandType.Text;
				OdbcCmd.CommandText = "SELECT fname FROM employee WHERE lname=? AND age=?";
				/*OdbcParameter p1Lname = */OdbcCmd.Parameters.Add ("@lname", OdbcType.Text, 15);
				/*OdbcParameter p2Age = */OdbcCmd.Parameters.Add ("@age", OdbcType.Int, 2);

				Assert.AreEqual (2, OdbcCmd.Parameters.Count, "#1 Initialization error, the collection does not contain desired no. of parameters");
				OdbcParameter [] DestinationParamArray = new OdbcParameter [4];
				try{
					OdbcCmd.Parameters.CopyTo (DestinationParamArray, 3); //starting at 3, thus the second element will be at index 4
					Assert.Fail ("Expected Exception ArgumentException not thrown");
				} catch (ArgumentException) {
					Assert.AreEqual (null, DestinationParamArray [0], "#2 The DestinationParamArray must remain un-initialized");
					Assert.AreEqual (null, DestinationParamArray [1], "#3 The DestinationParamArray must remain un-initialized");
					Assert.AreEqual (null, DestinationParamArray [2], "#4 The DestinationParamArray must remain un-initialized");
					Assert.AreEqual (null, DestinationParamArray [3], "#5 The DestinationParamArray must remain un-initialized");
				}
			} finally {
				ConnectionManager.Instance.Odbc.CloseConnection ();
			}
		}

		[Test]
		public void CopyToMultiDimensionalArrayTest ()
		{
			OdbcConnection conn = ConnectionManager.Instance.Odbc.Connection;

			try {
				OdbcCommand OdbcCmd = conn.CreateCommand ();
				OdbcCmd.CommandType = CommandType.Text;
				OdbcCmd.CommandText = "SELECT fname FROM employee WHERE lname=? AND age=?";
				/*OdbcParameter p1Lname = */OdbcCmd.Parameters.Add ("@lname", OdbcType.Text, 15);
				/*OdbcParameter p2Age = */OdbcCmd.Parameters.Add ("@age", OdbcType.Int, 2);

				Assert.AreEqual (2, OdbcCmd.Parameters.Count, "#1 Initialization error, the collection does not contain desired no. of parameters");
				OdbcParameter [,] DestinationParamArray = new OdbcParameter [2, 4];

				try{
					OdbcCmd.Parameters.CopyTo (DestinationParamArray, 1); //DestinationParamArray is multi Dimensional
					Assert.Fail ("Expected Exception ArgumentException not thrown");
				} catch (ArgumentException) {
					Assert.AreEqual (null, DestinationParamArray [0, 0], "#2 The DestinationParamArray must remain un-initialized");
					Assert.AreEqual (null, DestinationParamArray [0, 1], "#3 The DestinationParamArray must remain un-initialized");
					Assert.AreEqual (null, DestinationParamArray [1, 2], "#4 The DestinationParamArray must remain un-initialized");
					Assert.AreEqual (null, DestinationParamArray [1, 3], "#5 The DestinationParamArray must remain un-initialized");
				}
			} finally {
				ConnectionManager.Instance.Odbc.CloseConnection ();
			}
		}

		[Test]
		public void CopyToLowerBoundCheckTest ()
		{
			OdbcConnection conn = ConnectionManager.Instance.Odbc.Connection;

			try {
				OdbcCommand OdbcCmd = conn.CreateCommand ();
				OdbcCmd.CommandType = CommandType.Text;
				OdbcCmd.CommandText = "SELECT fname FROM employee WHERE lname=? AND age=?";
				/*OdbcParameter p1Lname = */OdbcCmd.Parameters.Add ("@lname", OdbcType.Text, 15);
				/*OdbcParameter p2Age = */OdbcCmd.Parameters.Add ("@age", OdbcType.Int, 2);

				Assert.AreEqual (2, OdbcCmd.Parameters.Count, "#1 Initialization error, the collection does not contain desired no. of parameters");
				OdbcParameter [] DestinationParamArray = new OdbcParameter [4];
				try {
					OdbcCmd.Parameters.CopyTo (DestinationParamArray, -1); //index must be >= 0
					Assert.Fail ("Expected Exception ArgumentOutOfRangeException not thrown");
				} catch (ArgumentOutOfRangeException) {
					Assert.AreEqual (null, DestinationParamArray [0], "#2 The DestinationParamArray must remain un-initialized");
					Assert.AreEqual (null, DestinationParamArray [1], "#3 The DestinationParamArray must remain un-initialized");
					Assert.AreEqual (null, DestinationParamArray [2], "#4 The DestinationParamArray must remain un-initialized");
					Assert.AreEqual (null, DestinationParamArray [3], "#5 The DestinationParamArray must remain un-initialized");
				}
			} finally {
				ConnectionManager.Instance.Odbc.CloseConnection ();
			}
		}

		[Test]
		public void DuplicateParameterNameTest ()
		{
			OdbcConnection conn = ConnectionManager.Instance.Odbc.Connection;

			try {
				OdbcCommand OdbcCmd = conn.CreateCommand ();
				OdbcCmd.CommandType = CommandType.Text;
				OdbcCmd.CommandText = "SELECT fname FROM employee WHERE lname=? AND age=?";

				OdbcParameter p1Lname = OdbcCmd.Parameters.Add ("@lname", OdbcType.Text, 15);
				
				Assert.AreEqual (1, OdbcCmd.Parameters.Count, "#1 Initialization error, the collection does not contain desired no. of parameters");
				OdbcParameter p2Lname = OdbcCmd.Parameters.Add ("@lname", OdbcType.Text, 15);
				Assert.AreEqual (2, OdbcCmd.Parameters.Count, "#2 Collection should contain 2 parameters");
				
				//Checking IndexOf (string) overload
				Assert.AreEqual (0, OdbcCmd.Parameters.IndexOf ("@lname"));
				
				//Checking IndexOf (OdbcParameter) overload
				Assert.AreEqual (0, OdbcCmd.Parameters.IndexOf (p1Lname));
				Assert.AreEqual (1, OdbcCmd.Parameters.IndexOf (p2Lname));
				
				//Checking IndexOf (object) overload
				Assert.AreEqual (0, OdbcCmd.Parameters.IndexOf ((object) p1Lname));
				Assert.AreEqual (1, OdbcCmd.Parameters.IndexOf ((object) p2Lname));
			} finally {
				ConnectionManager.Instance.Odbc.CloseConnection ();
			}
		}

		[Test]
		public void RemoveTest ()
		{
			OdbcConnection conn = ConnectionManager.Instance.Odbc.Connection;

			try {
				OdbcCommand OdbcCmd = conn.CreateCommand ();
				OdbcCmd.CommandType = CommandType.Text;
				OdbcCmd.CommandText = "SELECT fname FROM employee WHERE lname=? AND age=?";

				OdbcParameter p1Lname = OdbcCmd.Parameters.Add ("@lname", OdbcType.Text, 15);
				
				Assert.AreEqual (1, OdbcCmd.Parameters.Count, "#1 Initialization error, the collection does not contain desired no. of parameters");
				OdbcCmd.Parameters.Remove (p1Lname);
				Assert.AreEqual (0, OdbcCmd.Parameters.Count, "#2 Collection should not contain any parameters");
			} finally {
				ConnectionManager.Instance.Odbc.CloseConnection ();
			}
		}

		[Test]
		public void RemoveNullTest ()
		{
			OdbcConnection conn = ConnectionManager.Instance.Odbc.Connection;

			try {
				OdbcCommand OdbcCmd = conn.CreateCommand ();
				OdbcCmd.CommandType = CommandType.Text;
				OdbcCmd.CommandText = "SELECT fname FROM employee WHERE lname=? AND age=?";

				/*OdbcParameter p1Lname = */OdbcCmd.Parameters.Add ("@lname", OdbcType.Text, 15);

				Assert.AreEqual (1, OdbcCmd.Parameters.Count, "#1 Initialization error, the collection does not contain desired no. of parameters");
				try {
					OdbcCmd.Parameters.Remove (null);
					Assert.Fail ("Expected exception ArgumentNullException was not thrown");
				} catch (ArgumentNullException) {
					Assert.AreEqual (1, OdbcCmd.Parameters.Count, "#2 The collection must contain only one parameter");
				}
			} finally {
				ConnectionManager.Instance.Odbc.CloseConnection ();
			}
		}
		
		[Test]
		public void RemoveEmptyTest ()
		{
			OdbcConnection conn = ConnectionManager.Instance.Odbc.Connection;

			try {
				OdbcCommand OdbcCmd = conn.CreateCommand ();
				OdbcCmd.CommandType = CommandType.Text;
				OdbcCmd.CommandText = "SELECT fname FROM employee WHERE lname=? AND age=?";

				/*OdbcParameter p1Lname = */OdbcCmd.Parameters.Add ("@lname", OdbcType.Text, 15);

				Assert.AreEqual (1, OdbcCmd.Parameters.Count, "#1 Initialization error, the collection does not contain desired no. of parameters");
				try {
					OdbcCmd.Parameters.Remove (string.Empty);
					Assert.Fail ("Expected exception InvalidCastException was not thrown");
				} catch (InvalidCastException) {
					Assert.AreEqual (1, OdbcCmd.Parameters.Count, "#2 The collection must contain only one parameter");
				}
			} finally {
				ConnectionManager.Instance.Odbc.CloseConnection ();
			}
		}

		[Test]
		public void RemoveNonOdbcParameterTest ()
		{
			OdbcConnection conn = ConnectionManager.Instance.Odbc.Connection;

			try {
				OdbcCommand OdbcCmd = conn.CreateCommand ();
				OdbcCmd.CommandType = CommandType.Text;
				OdbcCmd.CommandText = "SELECT fname FROM employee WHERE lname=? AND age=?";

				/*OdbcParameter p1Lname = */OdbcCmd.Parameters.Add ("@lname", OdbcType.Text, 15);

				Assert.AreEqual (1, OdbcCmd.Parameters.Count, "#1 Initialization error, the collection does not contain desired no. of parameters");
				try {
					OdbcCmd.Parameters.Remove (4);
					Assert.Fail ("Expected exception InvalidCastException was not thrown");
				} catch (InvalidCastException) {
					Assert.AreEqual (1, OdbcCmd.Parameters.Count, "#2 The collection must contain only one parameter");
				}
			} finally {
				ConnectionManager.Instance.Odbc.CloseConnection ();
			}
		}

		[Test]
		public void RemoveNonExistingParameterTest ()
		{
			OdbcConnection conn = ConnectionManager.Instance.Odbc.Connection;

			try {
				OdbcCommand OdbcCmd = conn.CreateCommand ();
				OdbcCmd.CommandType = CommandType.Text;
				OdbcCmd.CommandText = "SELECT fname FROM employee WHERE lname=? AND age=?";

				OdbcParameter p1Lname = OdbcCmd.Parameters.Add ("@lname", OdbcType.Text, 15);
				/*OdbcParameter p2Age = new OdbcParameter ();*/
				
				Assert.AreEqual (1, OdbcCmd.Parameters.Count, "#1 Initialization error, the collection does not contain desired no. of parameters");
				OdbcCmd.Parameters.Remove (p1Lname);
				Assert.AreEqual (0, OdbcCmd.Parameters.Count, "#2 Collection should not contain any parameters");
				try {
					OdbcCmd.Parameters.Remove (p1Lname);
					Assert.Fail ("Expected exception ArgumentException not thrown");
				} catch (ArgumentException) {
					Assert.AreEqual (0, OdbcCmd.Parameters.Count, "#3 The collection should not contain any parameters");
				}
			} finally {
				ConnectionManager.Instance.Odbc.CloseConnection ();
			}
		}

		[Test]
		public void RemoveParameterContainedInAnotherCollTest ()
		{
			OdbcConnection conn = ConnectionManager.Instance.Odbc.Connection;

			try {
				OdbcCommand OdbcCmd = conn.CreateCommand ();
				OdbcCmd.CommandType = CommandType.Text;
				OdbcCmd.CommandText = "SELECT fname FROM employee WHERE lname=? AND age=?";

				OdbcCommand OdbcCmd2 = conn.CreateCommand ();
				OdbcCmd2.CommandType = CommandType.Text;
				OdbcCmd2.CommandText = "SELECT lname FROM employee WHERE fname=? AND age=?";

				OdbcParameter p1 = OdbcCmd.Parameters.Add ("@p1", OdbcType.Text, 15);
				/*OdbcParameter p2 = */OdbcCmd.Parameters.Add ("@p2", OdbcType.Text, 15);
				
				/*OdbcParameter p3 = */OdbcCmd2.Parameters.Add ("@p3", OdbcType.Text, 15);
				/*OdbcParameter p4 = */OdbcCmd2.Parameters.Add ("@p4", OdbcType.Text, 15);

				Assert.AreEqual (2, OdbcCmd.Parameters.Count, "#5 The parameter collection of OdbcCmd should contain 2 parameters");
				Assert.AreEqual (2, OdbcCmd2.Parameters.Count, "#6 The parameter collection of OdbcCmd2 should contain 2 parameters");
				try {
					OdbcCmd2.Parameters.Remove (p1);
					Assert.Fail ("Expected Exception ArgumentException not thrown");
				} catch (ArgumentException) {
					Assert.AreEqual (2, OdbcCmd.Parameters.Count, "#5 The parameter collection of OdbcCmd should contain 2 parameters");
					Assert.AreEqual (2, OdbcCmd2.Parameters.Count, "#6 The parameter collection of OdbcCmd2 should contain 2 parameters");
				}
			} finally {
				ConnectionManager.Instance.Odbc.CloseConnection ();
			}
		}
		
		[Test]
		public void RemoveAtTest ()
		{
			OdbcConnection conn = ConnectionManager.Instance.Odbc.Connection;

			try {
				OdbcCommand OdbcCmd = conn.CreateCommand ();
				OdbcCmd.CommandType = CommandType.Text;
				OdbcCmd.CommandText = "SELECT fname FROM employee WHERE lname=? AND age=?";

				OdbcParameter p1Lname = OdbcCmd.Parameters.Add ("@lname", OdbcType.Text, 15);
				OdbcParameter p2Age = OdbcCmd.Parameters.Add ("@age", OdbcType.Int, 2);
				OdbcParameter p3Tmp = OdbcCmd.Parameters.Add ("@p3Tmp", OdbcType.Text, 15);
				OdbcParameter p4Tmp = OdbcCmd.Parameters.Add ("@p4Tmp", OdbcType.Text, 15);

				Assert.AreEqual (4, OdbcCmd.Parameters.Count, "#1 Initialization error, the collection does not contain desired no. of parameters");
				
				Assert.AreEqual (true, OdbcCmd.Parameters.Contains(p1Lname), "#2 the collection does not contain p1Lname");
				Assert.AreEqual (0, OdbcCmd.Parameters.IndexOf(p1Lname), "#3 p1Lname is not at index 0");

				//remove the first parameter
				OdbcCmd.Parameters.RemoveAt (0);
				Assert.AreEqual (3, OdbcCmd.Parameters.Count, "#4 Collection should contain only 3 parameters");
				Assert.AreEqual (false, OdbcCmd.Parameters.Contains(p1Lname), "#5 the collection should not contain p1Lname");
				Assert.AreEqual (-1, OdbcCmd.Parameters.IndexOf(p1Lname), "#6 the collection should not contain p1Lname");
				Assert.AreEqual (0, OdbcCmd.Parameters.IndexOf(p2Age), "#7 p2Age should now be at index 0");
				Assert.AreEqual (1, OdbcCmd.Parameters.IndexOf(p3Tmp), "#8 p3Tmp should now be at index 1");
				Assert.AreEqual (2, OdbcCmd.Parameters.IndexOf(p4Tmp), "#9 p4Tmp should now be at index 2");

				//remove the last parameter
				OdbcCmd.Parameters.RemoveAt (OdbcCmd.Parameters.Count-1);
				Assert.AreEqual (2, OdbcCmd.Parameters.Count, "#10 Collection should contain only 2 parameters");
				Assert.AreEqual (false, OdbcCmd.Parameters.Contains(p4Tmp), "#11 the collection should not contain p4Tmp");
				Assert.AreEqual (-1, OdbcCmd.Parameters.IndexOf(p4Tmp), "#12 the collection should not contain p4Tmp");
				Assert.AreEqual (0, OdbcCmd.Parameters.IndexOf(p2Age), "#13 p2Age should be at index 0");
				Assert.AreEqual (1, OdbcCmd.Parameters.IndexOf(p3Tmp), "#14 p3Tmp should be at index 1");				
			} finally {
				ConnectionManager.Instance.Odbc.CloseConnection ();
			}
		}

		[Test]
		public void RemoveAtOutOfRangeIndexTest ()
		{
			OdbcConnection conn = ConnectionManager.Instance.Odbc.Connection;

			try {
				OdbcCommand OdbcCmd = conn.CreateCommand ();
				OdbcCmd.CommandType = CommandType.Text;
				OdbcCmd.CommandText = "SELECT fname FROM employee WHERE lname=? AND age=?";

				OdbcParameter p1Lname = OdbcCmd.Parameters.Add ("@lname", OdbcType.Text, 15);

				Assert.AreEqual (1, OdbcCmd.Parameters.Count, "#1 Initialization error, the collection does not contain desired no. of parameters");
				try {
					OdbcCmd.Parameters.RemoveAt (9);
					Assert.Fail ("Expected exception IndexOutOfRangeException not thrown");
				} catch (IndexOutOfRangeException) {
					Assert.AreEqual (1, OdbcCmd.Parameters.Count, "#2 The collection must contain 1 parameter");
					Assert.AreEqual (true, OdbcCmd.Parameters.Contains(p1Lname), "#3 the collection does not contain p1Lname");
					Assert.AreEqual (0, OdbcCmd.Parameters.IndexOf(p1Lname), "#4 p1Lname is not at index 0");
				}
			} finally {
				ConnectionManager.Instance.Odbc.CloseConnection ();
			}
		}
		
		[Test]
		public void RemoveAtNegativeIndexTest ()
		{
			OdbcConnection conn = ConnectionManager.Instance.Odbc.Connection;

			try {
				OdbcCommand OdbcCmd = conn.CreateCommand ();
				OdbcCmd.CommandType = CommandType.Text;
				OdbcCmd.CommandText = "SELECT fname FROM employee WHERE lname=? AND age=?";

				OdbcParameter p1Lname = OdbcCmd.Parameters.Add ("@lname", OdbcType.Text, 15);

				Assert.AreEqual (1, OdbcCmd.Parameters.Count, "#1 Initialization error, the collection does not contain desired no. of parameters");
				try {
					OdbcCmd.Parameters.RemoveAt (-1);
					Assert.Fail ("Expected exception IndexOutOfRangeException not thrown");
				} catch (IndexOutOfRangeException) {
					Assert.AreEqual (1, OdbcCmd.Parameters.Count, "#2 The collection must contain 1 parameter");
					Assert.AreEqual (true, OdbcCmd.Parameters.Contains(p1Lname), "#3 the collection does not contain p1Lname");
					Assert.AreEqual (0, OdbcCmd.Parameters.IndexOf(p1Lname), "#4 p1Lname is not at index 0");
				}
			} finally {
				ConnectionManager.Instance.Odbc.CloseConnection ();
			}
		}
		
		[Test]
		public void RemoveAtBoundaryTest ()
		{
			OdbcConnection conn = ConnectionManager.Instance.Odbc.Connection;

			try {
				OdbcCommand OdbcCmd = conn.CreateCommand ();
				OdbcCmd.CommandType = CommandType.Text;
				OdbcCmd.CommandText = "SELECT fname FROM employee WHERE lname=? AND age=?";

				OdbcParameter p1Lname = OdbcCmd.Parameters.Add ("@lname", OdbcType.Text, 15);

				Assert.AreEqual (1, OdbcCmd.Parameters.Count, "#1 Initialization error, the collection does not contain desired no. of parameters");
				try {
					OdbcCmd.Parameters.RemoveAt (OdbcCmd.Parameters.Count);
					Assert.Fail ("Expected exception IndexOutOfRangeException not thrown");
				} catch (IndexOutOfRangeException) {
					Assert.AreEqual (1, OdbcCmd.Parameters.Count, "#2 The collection must contain 1 parameter");
					Assert.AreEqual (true, OdbcCmd.Parameters.Contains(p1Lname), "#3 the collection does not contain p1Lname");
					Assert.AreEqual (0, OdbcCmd.Parameters.IndexOf(p1Lname), "#4 p1Lname is not at index 0");
				}
			} finally {
				ConnectionManager.Instance.Odbc.CloseConnection ();
			}
		}

		[Test]
		public void AddWithValueTest ()
		{
			OdbcConnection conn = ConnectionManager.Instance.Odbc.Connection;

			try {
				OdbcCommand OdbcCmd = conn.CreateCommand ();
				OdbcCmd.CommandType = CommandType.Text;
				OdbcCmd.CommandText = "SELECT fname FROM employee WHERE lname=? AND age=?";
				
				/*OdbcParameter p1Lname = */OdbcCmd.Parameters.Add ("@lname", OdbcType.Text, 15);
				OdbcParameter rt = null; //to check return type
				Assert.AreEqual (1, OdbcCmd.Parameters.Count, "#1 Initialization error, the collection does not contain desired no. of parameters");				
				
				rt = OdbcCmd.Parameters.AddWithValue ("@P2", "Param2");
				Assert.AreEqual (typeof(OdbcParameter), rt.GetType(), "#1a AddWithValue didnt retuen type OdbcParameter");
				Assert.AreEqual (2, OdbcCmd.Parameters.Count, "#2 AddWithValue failed for valid parameter name and value");
				Assert.AreEqual (true, OdbcCmd.Parameters.Contains ("@P2"), "#3 collection does not contain @P2");
				Assert.AreEqual (1, OdbcCmd.Parameters.IndexOf ("@P2"), "#4 Index of added parameter must be 1");
				Assert.AreEqual ("Param2", OdbcCmd.Parameters["@P2"].Value, "#5 Value of added parameter must be Param2");
				Assert.AreEqual (OdbcType.NVarChar,OdbcCmd.Parameters[1].OdbcType, "#6 Parameters with null value must be of type NVarChar");

				OdbcCmd.Parameters.AddWithValue ("@P2", "Param2ReAdded"); //adding again
				Assert.AreEqual (3, OdbcCmd.Parameters.Count, "#7 AddWithValue must append at the end of the collection even for same parameter names");
				Assert.AreEqual (true, OdbcCmd.Parameters.Contains ("@P2"), "#8 collection does not contain @P2");
				Assert.AreEqual (1, OdbcCmd.Parameters.IndexOf ("@P2"), "#9 Index of @P2 must be 1");
				Assert.AreEqual ("Param2", OdbcCmd.Parameters["@P2"].Value, "#10 Value of added parameter must be Param2");
				Assert.AreEqual (OdbcType.NVarChar,OdbcCmd.Parameters["@P2"].OdbcType, "#11 Parameters with null value must be of type NVarChar");

				//Two different parameters with same name but at different positions ie 1 and 2
				Assert.AreEqual ("@P2",OdbcCmd.Parameters[1].ParameterName, "#12 The parameter at index 1 must be @P2");
				Assert.AreEqual ("@P2",OdbcCmd.Parameters[2].ParameterName, "#13 The parameter at index 2 must be @P2");

				//Confirming the parameters by checking their values
				Assert.AreEqual ("Param2",OdbcCmd.Parameters[1].Value, "#14The parameter at index 1 must have value Param2");
				Assert.AreEqual ("Param2ReAdded",OdbcCmd.Parameters[2].Value, "#15The parameter at index 2 must have value Param2ReAdded");

				//Testing for null values
				OdbcCmd.Parameters.AddWithValue (null, null);
				Assert.AreEqual (4, OdbcCmd.Parameters.Count, "#16 AddWithValue must accept null parameter names and null values");
				Assert.AreEqual (false, OdbcCmd.Parameters.Contains (null), "#17 AddWithValue must return false for Contains (null)");
				Assert.AreEqual (-1, OdbcCmd.Parameters.IndexOf (null), "#18 AddWithValue must return -1 for IndexOf (null)");
				Assert.AreEqual (null, OdbcCmd.Parameters["Parameter1"].Value, "#19 Value of added parameter must be null");
				Assert.AreEqual ("Parameter1",OdbcCmd.Parameters[3].ParameterName, "#20 The parameter at index 3 must be Parameter1");
				Assert.AreEqual (OdbcType.NVarChar,OdbcCmd.Parameters[3].OdbcType, "#21 Parameters with null value must be of type NVarChar");

				OdbcCmd.Parameters.AddWithValue (null, null); //adding another null parameter
				Assert.AreEqual (5, OdbcCmd.Parameters.Count, "#22 AddWithValue must accept null parameter names and null values");
				Assert.AreEqual (false, OdbcCmd.Parameters.Contains (null), "#23 AddWithValue must return false for Contains (null)");
				Assert.AreEqual (-1, OdbcCmd.Parameters.IndexOf (null), "#24 AddWithValue must return -1 for IndexOf (null)");
				Assert.AreEqual (null, OdbcCmd.Parameters["Parameter2"].Value, "#25 Value of added parameter must be null");
				Assert.AreEqual ("Parameter2",OdbcCmd.Parameters[4].ParameterName, "#26 The parameter at index 1 must be Parameter2");
				Assert.AreEqual (OdbcType.NVarChar,OdbcCmd.Parameters[4].OdbcType, "#27 Parameters with null value must be of type NVarChar");

				//Testing for empty strings
				OdbcCmd.Parameters.AddWithValue ("", ""); //adding empty parameter
				Assert.AreEqual (6, OdbcCmd.Parameters.Count, "#28 AddWithValue must accept empty names and empty values");
				Assert.AreEqual (false, OdbcCmd.Parameters.Contains (""), "#29 AddWithValue must return false for Contains ('')");
				Assert.AreEqual (-1, OdbcCmd.Parameters.IndexOf (""), "#30 AddWithValue must return -1 for IndexOf ('')");
				Assert.AreEqual ("Parameter3",OdbcCmd.Parameters[5].ParameterName, "#31 The parameter at index 5 must be Parameter3");      
				Assert.AreEqual ("",OdbcCmd.Parameters[5].Value, "#32 The parameter at index 5 must have value as empty string");      
				Assert.AreEqual (OdbcType.NVarChar,OdbcCmd.Parameters[5].OdbcType, "#33 Parameters with null value must be of type NVarChar");

				OdbcCmd.Parameters.AddWithValue ("", ""); //adding another empty parameter
				Assert.AreEqual (7, OdbcCmd.Parameters.Count, "#34 AddWithValue must accept empty names and empty values");
				Assert.AreEqual (false, OdbcCmd.Parameters.Contains (""), "#35 AddWithValue must return false for Contains ('')");
				Assert.AreEqual (-1, OdbcCmd.Parameters.IndexOf (""), "#36 AddWithValue must return -1 for IndexOf ('')");
				Assert.AreEqual ("Parameter4",OdbcCmd.Parameters[6].ParameterName, "#37 The parameter at index 6 must have name as Parameter4");
				Assert.AreEqual ("",OdbcCmd.Parameters[6].Value, "#38 The parameter at index 6 must have value as empty string");                                                    
				Assert.AreEqual (OdbcType.NVarChar,OdbcCmd.Parameters[6].OdbcType, "#39 Parameters with null value must be of type NVarChar");

				OdbcCmd.Parameters.AddWithValue ("foo", null);
				Assert.AreEqual (8, OdbcCmd.Parameters.Count, "#40 AddWithValue must accept string names and null values");
				Assert.AreEqual (true, OdbcCmd.Parameters.Contains ("foo"), "#41 AddWithValue must return true for Contains ('foo')");
				Assert.AreEqual (7, OdbcCmd.Parameters.IndexOf ("foo"), "#42 AddWithValue must return 7 for IndexOf ('foo')");
				Assert.AreEqual ("foo",OdbcCmd.Parameters[7].ParameterName, "#43 The parameter at index 7 must have name foo");
				Assert.AreEqual (null,OdbcCmd.Parameters[7].Value, "#44 The parameter at index 7 must have value as null");                                                    
				Assert.AreEqual (OdbcType.NVarChar,OdbcCmd.Parameters[7].OdbcType, "#45 Parameters with null value must be of type NVarChar");

				OdbcCmd.Parameters.AddWithValue (null, 2);
				Assert.AreEqual (9, OdbcCmd.Parameters.Count, "#46 AddWithValue must accept empty names and empty values");
				Assert.AreEqual (false, OdbcCmd.Parameters.Contains (null), "#47 AddWithValue must return false for Contains (null)");
				Assert.AreEqual (-1, OdbcCmd.Parameters.IndexOf (null), "#48 AddWithValue must return -1 for IndexOf ('')");
				Assert.AreEqual ("Parameter5",OdbcCmd.Parameters[8].ParameterName, "#49 The parameter at index 8 must have name as Parameter5");
				Assert.AreEqual (2,OdbcCmd.Parameters[8].Value, "#50 The parameter at index 8 must have value as 2");                                                    
				Assert.AreEqual (OdbcType.NVarChar,OdbcCmd.Parameters[8].OdbcType, "#51 Parameter must be of type NVarChar");
			} finally {
				ConnectionManager.Instance.Odbc.CloseConnection ();
			}
		}

		[Test]
		public void DefaultNamesAndValuesTest ()
		{
			OdbcConnection conn = ConnectionManager.Instance.Odbc.Connection;

			try {
				OdbcCommand OdbcCmd = conn.CreateCommand ();
				OdbcCmd.CommandType = CommandType.Text;
				OdbcCmd.CommandText = "SELECT fname FROM employee WHERE lname=? AND age=?";

				/*OdbcParameter p1Lname = */OdbcCmd.Parameters.Add ("@lname", OdbcType.Text, 15);
				Assert.AreEqual (1, OdbcCmd.Parameters.Count, "#1 Initialization error, the collection does not contain desired no. of parameters");

				OdbcParameter p2Age = new OdbcParameter ();
				OdbcParameter p3Tmp = new OdbcParameter ();

				OdbcCmd.Parameters.Add (p2Age);
				Assert.AreEqual (2, OdbcCmd.Parameters.Count, "#2 The collection must contain 2 parameters");
				Assert.AreEqual (true, OdbcCmd.Parameters.Contains (p2Age), "#3 Collection does not contain p2Age");
				Assert.AreEqual (1, OdbcCmd.Parameters.IndexOf (p2Age), "#4 Index of p2Age must be 1");
				Assert.AreEqual (null, OdbcCmd.Parameters[1].Value, "#5 Value of added parameter must be null");
				Assert.AreEqual ("Parameter1",OdbcCmd.Parameters[1].ParameterName, "#6 The parameter must have a default name");
				Assert.AreEqual (OdbcType.NVarChar,OdbcCmd.Parameters[1].OdbcType, "#7 Parameters with null value must be of type NVarChar");

				OdbcCmd.Parameters.Insert (2,p3Tmp);
				Assert.AreEqual (3, OdbcCmd.Parameters.Count, "#8 The collection must contain 3 parameters");
				Assert.AreEqual (true, OdbcCmd.Parameters.Contains (p3Tmp), "#9 Collection does not contain p3Tmp");
				Assert.AreEqual (2, OdbcCmd.Parameters.IndexOf (p3Tmp), "#10 Index of p3Tmp must be 2");
				Assert.AreEqual (null, OdbcCmd.Parameters[2].Value, "#11 Value of added parameter must be null");
				Assert.AreEqual ("Parameter2",OdbcCmd.Parameters[2].ParameterName, "#12 The parameter must have a default name");
				Assert.AreEqual (OdbcType.NVarChar,OdbcCmd.Parameters[2].OdbcType, "#13 Parameters with null value must be of type NVarChar");
			} finally {
				ConnectionManager.Instance.Odbc.CloseConnection ();
			}
		}
	}
}

#endif