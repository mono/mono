//
// OdbcParameterCollectionTest.cs - NUnit Test Cases for testing the
//			  OdbcParameterCollection class
// Author:
//      Sureshkumar T (TSureshkumar@novell.com)
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
using System.Text;
using System.Data;
using System.Data.Odbc;

using NUnit.Framework;

namespace MonoTests.System.Data.Odbc
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
			IDbConnection conn = ConnectionManager.Singleton.Connection;
			try {
				ConnectionManager.Singleton.OpenConnection ();
				OdbcCommand cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandType = CommandType.Text;
				cmd.CommandText = "SELECT count(*) FROM employee WHERE fname=?";
															     
				OdbcParameter param = cmd.Parameters.Add("@fname", OdbcType.Text, 15);
				param.Value = DateTime.Now.ToString ();
				Assert.AreEqual (15, param.Size, "#1");
				Convert.ToInt32(cmd.ExecuteScalar());
			} finally {
				ConnectionManager.Singleton.CloseConnection ();
			}
        }

		[Test]
		public void InsertTest()
		{
			IDbConnection conn = ConnectionManager.Singleton.Connection;
			try {
				ConnectionManager.Singleton.OpenConnection ();
				OdbcCommand OdbcCmd = (OdbcCommand) conn.CreateCommand ();
				OdbcCmd.CommandType = CommandType.Text;
				OdbcCmd.CommandText = "SELECT fname FROM employee WHERE lname=? AND age=?";

				OdbcParameter p1Lname = OdbcCmd.Parameters.Add ("@lname", OdbcType.Text, 15);
				OdbcParameter p2Age = new OdbcParameter (); //not initialized and not yet added

				Assert.AreEqual (1, OdbcCmd.Parameters.Count, "#1 Make sure only the initializing parameter is added");
				OdbcCmd.Parameters.Insert (1, p2Age); //Inserting the second parameter
				Assert.AreEqual (2, OdbcCmd.Parameters.Count, "#2 Collection now contains both the parameters");

				//check the inserted positions
				Assert.AreEqual (0, OdbcCmd.Parameters.IndexOf ("@lname"));  //checking the positions as string
				Assert.AreEqual (1, OdbcCmd.Parameters.IndexOf (p2Age));     //checking the positions as OdbcParameter
				Assert.AreEqual (-1, OdbcCmd.Parameters.IndexOf ("non-exixting-parameter"));     //non exixting parameters should return index -1
			} finally {
				ConnectionManager.Singleton.CloseConnection ();
			}

		}


		[Test]
		public void AddRangeTest ()
		{
			IDbConnection conn = ConnectionManager.Singleton.Connection;
			try {
				ConnectionManager.Singleton.OpenConnection ();
				OdbcCommand OdbcCmd = (OdbcCommand) conn.CreateCommand ();
				OdbcCmd.CommandType = CommandType.Text;
				OdbcCmd.CommandText = "SELECT fname FROM employee WHERE lname=? AND age=?";
				OdbcParameter p1Lname = OdbcCmd.Parameters.Add ("@lname", OdbcType.Text, 15);
				OdbcParameter p2Age = new OdbcParameter (); //not initialized and not yet added
				OdbcParameter p3Tmp = new OdbcParameter (); //not initialized and not yet added
				OdbcParameter p4Tmp = new OdbcParameter (); //not initialized and not yet added
				OdbcParameter p5Tmp = new OdbcParameter (); //not initialized and not yet added

				Assert.AreEqual (1, OdbcCmd.Parameters.Count, "#1 Make sure only the initializing parameter is added");
				OdbcParameter [] paramArray = new OdbcParameter [4];
				paramArray [0] = p2Age;
				paramArray [1] = p3Tmp;
				paramArray [2] = p4Tmp;
				paramArray [3] = p5Tmp;
				OdbcCmd.Parameters.AddRange (paramArray);
				Assert.AreEqual (5, OdbcCmd.Parameters.Count, "#2 The array elements are added");
				Assert.AreEqual (1, OdbcCmd.Parameters.IndexOf (p2Age), "#3 The first element must be added after the last parameter");
				Assert.AreEqual (4, OdbcCmd.Parameters.IndexOf (p5Tmp), "#4 Ensure all the elements are added");
			} finally {
				ConnectionManager.Singleton.CloseConnection ();
			}
		}


		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void InsertArgumentGreaterThanCountTest ()
		{
			IDbConnection conn = ConnectionManager.Singleton.Connection;
			try {
				ConnectionManager.Singleton.OpenConnection ();
				OdbcCommand OdbcCmd = (OdbcCommand) conn.CreateCommand ();
				OdbcCmd.CommandType = CommandType.Text;
				OdbcCmd.CommandText = "SELECT fname FROM employee WHERE lname=? AND age=?";
				OdbcParameter p1Lname = OdbcCmd.Parameters.Add ("@lname", OdbcType.Text, 15);
				OdbcParameter p2Age = new OdbcParameter ();

				Assert.AreEqual (1, OdbcCmd.Parameters.Count, "#1 Make sure only the initializing parameter is added");
				OdbcCmd.Parameters.Insert (2, p2Age); //Inserting with wrong index
			} finally {
				ConnectionManager.Singleton.CloseConnection ();
			}
		}


		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void InsertNegetiveArgumentTest ()
		{
			IDbConnection conn = ConnectionManager.Singleton.Connection;
			try {
				ConnectionManager.Singleton.OpenConnection ();
				OdbcCommand OdbcCmd = (OdbcCommand) conn.CreateCommand ();
				OdbcCmd.CommandType = CommandType.Text;
				OdbcCmd.CommandText = "SELECT fname FROM employee WHERE lname=? AND age=?";
				OdbcParameter p1Lname = OdbcCmd.Parameters.Add ("@lname", OdbcType.Text, 15);
				OdbcParameter p2Age = new OdbcParameter (); //not initialized and not added

				Assert.AreEqual (1, OdbcCmd.Parameters.Count, "#1 Make sure no other parameters are present");
				OdbcCmd.Parameters.Insert (-3, p2Age); //Insert with negetive index
			} finally {
				ConnectionManager.Singleton.CloseConnection ();
			}
		}


		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void AddRangeParameterAlreadyContainedTest ()
		{
			IDbConnection conn = ConnectionManager.Singleton.Connection;
			try {
				ConnectionManager.Singleton.OpenConnection ();
				OdbcCommand OdbcCmd = (OdbcCommand) conn.CreateCommand ();
				OdbcCmd.CommandType = CommandType.Text;
				OdbcCmd.CommandText = "SELECT fname FROM employee WHERE lname=? AND age=?";
				OdbcParameter p1Lname = OdbcCmd.Parameters.Add ("@lname", OdbcType.Text, 15);
				OdbcParameter p2Age = new OdbcParameter (); //not initialized and not yet added
				OdbcParameter p3Tmp = new OdbcParameter (); //not initialized and not yet added

				Assert.AreEqual (1, OdbcCmd.Parameters.Count, "#1 Make sure only the initializing parameter is added");
				OdbcParameter [] paramArray = new OdbcParameter [3];
				paramArray [0] = p2Age;
				paramArray [1] = p1Lname; //p1Lname is already contained
				paramArray [2] = p3Tmp;
				OdbcCmd.Parameters.AddRange (paramArray);
			} finally {
				ConnectionManager.Singleton.CloseConnection ();
			}
		}


		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void AddRangeArgumentNullExceptionTest ()
		{
			IDbConnection conn = ConnectionManager.Singleton.Connection;
			try {
				ConnectionManager.Singleton.OpenConnection ();
				OdbcCommand OdbcCmd = (OdbcCommand) conn.CreateCommand ();
				OdbcCmd.CommandType = CommandType.Text;
				OdbcCmd.CommandText = "SELECT fname FROM employee WHERE lname=? AND age=?";
				OdbcParameter p1Lname = OdbcCmd.Parameters.Add ("@lname", OdbcType.Text, 15);
				OdbcParameter p2Age = new OdbcParameter ();
				OdbcParameter p3Tmp = new OdbcParameter ();

				Assert.AreEqual (1, OdbcCmd.Parameters.Count, "#1 Make sure only the initializing parameter is added");
				OdbcParameter [] paramArray = new OdbcParameter [3];
				paramArray [0] = p2Age;
				paramArray [1] = p3Tmp;
				paramArray [2] = null;
				OdbcCmd.Parameters.AddRange (paramArray);
			} finally {
				ConnectionManager.Singleton.CloseConnection ();
			}
		}


		[Test]
		public void AddRangeMultiDimensionalArrayTest ()
		{
			IDbConnection conn = ConnectionManager.Singleton.Connection;
			try {
				ConnectionManager.Singleton.OpenConnection ();
				OdbcCommand OdbcCmd = (OdbcCommand) conn.CreateCommand ();
				OdbcCmd.CommandType = CommandType.Text;
				OdbcCmd.CommandText = "SELECT fname FROM employee WHERE lname=? AND age=?";
				OdbcParameter p1Lname = OdbcCmd.Parameters.Add ("@lname", OdbcType.Text, 15);
				OdbcParameter p2Age = new OdbcParameter ();
				OdbcParameter p3Tmp = new OdbcParameter ();
				OdbcParameter p4Tmp = new OdbcParameter ();
				OdbcParameter p5Tmp = new OdbcParameter ();

				Assert.AreEqual (1, OdbcCmd.Parameters.Count, "#1 Make sure only the initializing parameter is added");
				OdbcParameter [,] paramArray = new OdbcParameter [2, 2];
				paramArray [0, 0] = p2Age;
				paramArray [0, 1] = p3Tmp;
				paramArray [1, 0] = p4Tmp;
				paramArray [1, 1] = p5Tmp;
				OdbcCmd.Parameters.AddRange (paramArray);
				Assert.AreEqual (5, OdbcCmd.Parameters.Count, "#2 Four parameters of the 2x2 Array are added");
				Assert.AreEqual (1, OdbcCmd.Parameters.IndexOf (p2Age), "#3 The first element must be added after the last parameter");
				Assert.AreEqual (4, OdbcCmd.Parameters.IndexOf (p5Tmp), "#4 Ensure all the elements are added");
			} finally {
				ConnectionManager.Singleton.CloseConnection ();
			}
		}


		//<remarks>
		//Tests all the three overloads of Contains
		//</remarks>
		[Test]
		public void ContainsTest ()
		{
			IDbConnection conn = ConnectionManager.Singleton.Connection;
			try {
				ConnectionManager.Singleton.OpenConnection ();
				OdbcCommand OdbcCmd = (OdbcCommand) conn.CreateCommand ();
				OdbcCmd.CommandType = CommandType.Text;
				OdbcCmd.CommandText = "SELECT fname FROM employee WHERE lname=? AND age=?";
				OdbcParameter p1Lname = OdbcCmd.Parameters.Add ("@lname", OdbcType.Text, 15);
				OdbcParameter p2Age = OdbcCmd.Parameters.Add ("@age", OdbcType.Int, 2);
				OdbcParameter p3Tmp = new OdbcParameter ();
				OdbcCmd.Parameters.Insert (2, p3Tmp);

				Assert.AreEqual (3, OdbcCmd.Parameters.Count, "#1 All the parameters are added");
				Assert.IsTrue (OdbcCmd.Parameters.Contains (p1Lname), "#2 Checking Contains with OdbcParameter value");
				Assert.IsTrue (OdbcCmd.Parameters.Contains ("@age"), "#3 Checking Contains with string value");
				Assert.IsTrue (OdbcCmd.Parameters.Contains (p3Tmp), "#4 Checking Contains with object value");
			} finally {
				ConnectionManager.Singleton.CloseConnection ();
			}
		}

		[Test]
		public void IndexOfTest ()
		{
			IDbConnection conn = ConnectionManager.Singleton.Connection;
			try {
				ConnectionManager.Singleton.OpenConnection ();
				OdbcCommand OdbcCmd = (OdbcCommand) conn.CreateCommand ();
				OdbcCmd.CommandType = CommandType.Text;
				OdbcCmd.CommandText = "SELECT fname FROM employee WHERE lname=? AND age=?";
				OdbcParameter p1Lname = OdbcCmd.Parameters.Add ("@lname", OdbcType.Text, 15);
				OdbcParameter p2Age = OdbcCmd.Parameters.Add ("@age", OdbcType.Int, 2);
				OdbcParameter p3Tmp = new OdbcParameter ();
				Assert.AreEqual (2, OdbcCmd.Parameters.Count, "#1 both the parameters are added");

				Assert.AreEqual (0, OdbcCmd.Parameters.IndexOf (p1Lname), "#1 first parameter");
				Assert.AreEqual (1, OdbcCmd.Parameters.IndexOf (p2Age), "#2 second parametr");
				Assert.AreEqual (-1, OdbcCmd.Parameters.IndexOf (p3Tmp), "#3 not present");
			} finally {
				ConnectionManager.Singleton.CloseConnection ();
			}
		}

		[Test]
		public void CopyToTest ()
		{
			IDbConnection conn = ConnectionManager.Singleton.Connection;
			try {
				ConnectionManager.Singleton.OpenConnection ();
				OdbcCommand OdbcCmd = (OdbcCommand) conn.CreateCommand ();
				OdbcCmd.CommandType = CommandType.Text;
				OdbcCmd.CommandText = "SELECT fname FROM employee WHERE lname=? AND age=?";
				OdbcParameter p1Lname = OdbcCmd.Parameters.Add ("@lname", OdbcType.Text, 15);
				OdbcParameter p2Age = OdbcCmd.Parameters.Add ("@age", OdbcType.Int, 2);

				Assert.AreEqual (2, OdbcCmd.Parameters.Count, "#1 Make sure both the parameters are present");
				OdbcParameter [] DestinationParamArray = new OdbcParameter [4];

				OdbcCmd.Parameters.CopyTo (DestinationParamArray, 1); //starting at 1 instead of 0

				Assert.AreEqual (4, DestinationParamArray.Length, "#2 The array length should not change");
				Assert.AreEqual ("@lname", DestinationParamArray [1].ParameterName, "#3 The first parameter copied to array at index 1");
				Assert.AreEqual ("@age", DestinationParamArray [2].ParameterName, "#4 The second parameter copied to array at index 2");
				Assert.AreEqual (null, DestinationParamArray [0], "#5 The remaining elements remain un-initialized");
				Assert.AreEqual (null, DestinationParamArray [3], "#6 The remaining elements remain un-initialized");
			} finally {
				ConnectionManager.Singleton.CloseConnection ();
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void CopyToArgumentExceptionTest ()
		{
			IDbConnection conn = ConnectionManager.Singleton.Connection;
			try {
				ConnectionManager.Singleton.OpenConnection ();
				OdbcCommand OdbcCmd = (OdbcCommand) conn.CreateCommand ();
				OdbcCmd.CommandType = CommandType.Text;
				OdbcCmd.CommandText = "SELECT fname FROM employee WHERE lname=? AND age=?";
				OdbcParameter p1Lname = OdbcCmd.Parameters.Add ("@lname", OdbcType.Text, 15);
				OdbcParameter p2Age = OdbcCmd.Parameters.Add ("@age", OdbcType.Int, 2);

				Assert.AreEqual (2, OdbcCmd.Parameters.Count, "#1 Make sure both the parameters are present");
				OdbcParameter [] DestinationParamArray = new OdbcParameter [4];

				OdbcCmd.Parameters.CopyTo (DestinationParamArray, 3); //starting at 3, thus the second element will be at index 4
			} finally {
				ConnectionManager.Singleton.CloseConnection ();
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void CopyToMultiDimensionalArrayTest ()
		{
			IDbConnection conn = ConnectionManager.Singleton.Connection;
			try {
				ConnectionManager.Singleton.OpenConnection ();
				OdbcCommand OdbcCmd = (OdbcCommand) conn.CreateCommand ();
				OdbcCmd.CommandType = CommandType.Text;
				OdbcCmd.CommandText = "SELECT fname FROM employee WHERE lname=? AND age=?";
				OdbcParameter p1Lname = OdbcCmd.Parameters.Add ("@lname", OdbcType.Text, 15);
				OdbcParameter p2Age = OdbcCmd.Parameters.Add ("@age", OdbcType.Int, 2);

				Assert.AreEqual (2, OdbcCmd.Parameters.Count, "#1 Make sure both the parameters are present");
				OdbcParameter [,] DestinationParamArray = new OdbcParameter [2, 4];

				OdbcCmd.Parameters.CopyTo (DestinationParamArray, 1); //DestinationParamArray is multi Dimensional
			} finally {
				ConnectionManager.Singleton.CloseConnection ();
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void CopyToLowerBoundCheckTest ()
		{
			IDbConnection conn = ConnectionManager.Singleton.Connection;
			try {
				ConnectionManager.Singleton.OpenConnection ();
				OdbcCommand OdbcCmd = (OdbcCommand) conn.CreateCommand ();
				OdbcCmd.CommandType = CommandType.Text;
				OdbcCmd.CommandText = "SELECT fname FROM employee WHERE lname=? AND age=?";
				OdbcParameter p1Lname = OdbcCmd.Parameters.Add ("@lname", OdbcType.Text, 15);
				OdbcParameter p2Age = OdbcCmd.Parameters.Add ("@age", OdbcType.Int, 2);

				Assert.AreEqual (2, OdbcCmd.Parameters.Count, "#1 Make sure both the parameters are present");
				OdbcParameter [] DestinationParamArray = new OdbcParameter [4];

				OdbcCmd.Parameters.CopyTo (DestinationParamArray, -1); //index must be >= 0
			} finally {
				ConnectionManager.Singleton.CloseConnection ();
			}
		}
	}
}
