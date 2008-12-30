// SqlInt32Test.cs - NUnit Test Cases for System.Data.SqlTypes.SqlInt32
//
// Authors:
//   Tim Coleman (tim@timcoleman.com)
//   Martin Willemoes Hansen (mwh@sysrq.dk)
//
// (C) Tim Coleman
// (C) 2003 Martin Willemoes Hansen
// 

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

using System;
using System.Data;
using System.Data.SqlTypes;
#if NET_2_0
using System.IO;
#endif
using System.Xml;
#if NET_2_0
using System.Xml.Serialization;
#endif 

using NUnit.Framework;

namespace MonoTests.System.Data.SqlTypes
{
	[TestFixture]
	public class SqlInt32Test
	{
		[Test]
		public void Create ()
		{
			SqlInt32 foo = new SqlInt32 (5);
			Assert.AreEqual ((int)foo, 5, "Test explicit cast to int");
		}

		[Test]
		public void Add () 
		{
			int a = 5;
			int b = 7;

			SqlInt32 x;
			SqlInt32 y;
			SqlInt32 z;

			x = new SqlInt32 (a);
			y = new SqlInt32 (b);
			z = x + y;
			Assert.AreEqual (z.Value, a + b, "Addition operator does not work correctly");
			z = SqlInt32.Add (x, y);
			Assert.AreEqual (z.Value, a + b, "Addition function does not work correctly");
		}

		[Test]
		public void BitwiseAnd () 
		{
			int a = 5;
			int b = 7;

			SqlInt32 x = new SqlInt32 (a);
			SqlInt32 y = new SqlInt32 (b);
			SqlInt32 z = x & y;
			Assert.AreEqual (z.Value, a & b, "Bitwise And operator does not work correctly");
			z = SqlInt32.BitwiseAnd (x, y);
			Assert.AreEqual (z.Value, a & b, "Bitwise And function does not work correctly");
		}

		[Test]
		public void BitwiseOr () 
		{
			int a = 5;
			int b = 7;

			SqlInt32 x = new SqlInt32 (a);
			SqlInt32 y = new SqlInt32 (b);
			SqlInt32 z = x | y;
			Assert.AreEqual (z.Value, a | b, "Bitwise Or operator does not work correctly");
			z = SqlInt32.BitwiseOr (x, y);
			Assert.AreEqual (z.Value, a | b, "Bitwise Or function does not work correctly");
		}

		[Test]
		public void Divide () 
		{
			int a = 5;
			int b = 7;

			SqlInt32 x = new SqlInt32 (a);
			SqlInt32 y = new SqlInt32 (b);
			SqlInt32 z = x / y;
			Assert.AreEqual (z.Value, a / b, "Division operator does not work correctly");
			z = SqlInt32.Divide (x, y);
			Assert.AreEqual (z.Value, a / b, "Division function does not work correctly");
		}
		
		[Test]
		public void Equals ()
		{
			SqlInt32 x;
			SqlInt32 y;

			// Case 1: either is SqlInt32.Null
			x = SqlInt32.Null;
			y = new SqlInt32 (5);
			Assert.AreEqual (x == y, SqlBoolean.Null, "Equality operator didn't return Null when one was Null.");
			Assert.AreEqual (SqlInt32.Equals (x, y), SqlBoolean.Null, "Equality function didn't return Null when one was Null.");

			// Case 2: both are SqlInt32.Null
			y = SqlInt32.Null;
			Assert.AreEqual (x == y, SqlBoolean.Null, "Equality operator didn't return Null when both were Null.");
			Assert.AreEqual (SqlInt32.Equals (x, y), SqlBoolean.Null, "Equality function didn't return Null when both were Null.");

			// Case 3: both are equal
			x = new SqlInt32 (5);
			y = new SqlInt32 (5);
			Assert.AreEqual (x == y, SqlBoolean.True, "Equality operator didn't return true when they were equal.");
			Assert.AreEqual (SqlInt32.Equals (x, y), SqlBoolean.True, "Equality function didn't return true when they were equal.");

			// Case 4: inequality
			x = new SqlInt32 (5);
			y = new SqlInt32 (6);
			Assert.AreEqual (x == y, SqlBoolean.False, "Equality operator didn't return false when they were not equal.");
			Assert.AreEqual (SqlInt32.Equals (x, y), SqlBoolean.False, "Equality function didn't return false when they were not equal.");
		}

		[Test]
		public void GreaterThan ()
		{
			SqlInt32 x;
			SqlInt32 y;

			// Case 1: either is SqlInt32.Null
			x = SqlInt32.Null;
			y = new SqlInt32 (5);
			Assert.AreEqual (x > y, SqlBoolean.Null, "Greater Than operator didn't return Null when one was Null.");
			Assert.AreEqual (SqlInt32.GreaterThan (x, y), SqlBoolean.Null, "Greater Than function didn't return Null when one was Null.");

			// Case 2: both are SqlInt32.Null
			y = SqlInt32.Null;
			Assert.AreEqual (x > y, SqlBoolean.Null, "Greater Than operator didn't return Null when both were Null.");
			Assert.AreEqual (SqlInt32.GreaterThan (x, y), SqlBoolean.Null, "Greater Than function didn't return Null when both were Null.");

			// Case 3: x > y
			x = new SqlInt32 (5);
			y = new SqlInt32 (4);
			Assert.AreEqual (x > y, SqlBoolean.True, "Greater than operator didn't return true when x > y.");
			Assert.AreEqual (SqlInt32.GreaterThan (x,y), SqlBoolean.True, "Greater than function didn't return true when x > y.");

			// Case 4: x < y
			x = new SqlInt32 (5);
			y = new SqlInt32 (6);
			Assert.AreEqual (x > y, SqlBoolean.False, "Greater than operator didn't return false when x < y.");
			Assert.AreEqual (SqlInt32.GreaterThan (x,y), SqlBoolean.False, "Greater than function didn't return false when x < y.");
		}

		[Test]
		public void GreaterThanOrEqual ()
		{
			SqlInt32 x;
			SqlInt32 y;

			// Case 1: either is SqlInt32.Null
			x = SqlInt32.Null;
			y = new SqlInt32 (5);
			Assert.AreEqual (x >= y, SqlBoolean.Null, "Greater Than Or Equal operator didn't return Null when one was Null.");
			Assert.AreEqual (SqlInt32.GreaterThanOrEqual (x, y), SqlBoolean.Null, "Greater Than Or Equal function didn't return Null when one was Null.");

			// Case 2: both are SqlInt32.Null
			y = SqlInt32.Null;
			Assert.AreEqual (x >= y, SqlBoolean.Null, "Greater Than Or Equal operator didn't return Null when both were Null.");
			Assert.AreEqual (SqlInt32.GreaterThanOrEqual (x, y), SqlBoolean.Null, "Greater Than Or Equal function didn't return Null when both were Null.");

			// Case 3: x > y
			x = new SqlInt32 (5);
			y = new SqlInt32 (4);
			Assert.AreEqual (x >= y, SqlBoolean.True, "Greater than or equal operator didn't return true when x > y.");
			Assert.AreEqual (SqlInt32.GreaterThanOrEqual (x,y), SqlBoolean.True, "Greater than or equal function didn't return true when x > y.");

			// Case 4: x < y
			x = new SqlInt32 (5);
			y = new SqlInt32 (6);
			Assert.AreEqual (x >= y, SqlBoolean.False, "Greater than or equal operator didn't return false when x < y.");
			Assert.AreEqual (SqlInt32.GreaterThanOrEqual (x,y), SqlBoolean.False, "Greater than or equal function didn't return false when x < y.");

			// Case 5: x == y
			x = new SqlInt32 (5);
			y = new SqlInt32 (5);
			Assert.AreEqual (x >= y, SqlBoolean.True, "Greater than or equal operator didn't return true when x == y.");
			Assert.AreEqual (SqlInt32.GreaterThanOrEqual (x,y), SqlBoolean.True, "Greater than or equal function didn't return true when x == y.");
		}

		[Test]
		public void LessThan ()
		{
			SqlInt32 x;
			SqlInt32 y;

			// Case 1: either is SqlInt32.Null
			x = SqlInt32.Null;
			y = new SqlInt32 (5);
			Assert.AreEqual (x < y, SqlBoolean.Null, "Less Than operator didn't return Null when one was Null.");
			Assert.AreEqual (SqlInt32.LessThan (x, y), SqlBoolean.Null, "Less Than function didn't return Null when one was Null.");

			// Case 2: both are SqlInt32.Null
			y = SqlInt32.Null;
			Assert.AreEqual (x < y, SqlBoolean.Null, "Less Than operator didn't return Null when both were Null.");
			Assert.AreEqual (SqlInt32.LessThan (x, y), SqlBoolean.Null, "Less Than function didn't return Null when both were Null.");

			// Case 3: x > y
			x = new SqlInt32 (5);
			y = new SqlInt32 (4);
			Assert.AreEqual (x < y, SqlBoolean.False, "Less than operator didn't return false when x > y.");
			Assert.AreEqual (SqlInt32.LessThan (x,y), SqlBoolean.False, "Less than function didn't return false when x > y.");

			// Case 4: x < y
			x = new SqlInt32 (5);
			y = new SqlInt32 (6);
			Assert.AreEqual (x < y, SqlBoolean.True, "Less than operator didn't return true when x < y.");
			Assert.AreEqual (SqlInt32.LessThan (x,y), SqlBoolean.True, "Less than function didn't return true when x < y.");
		}

		[Test]
		public void LessThanOrEqual ()
		{
			SqlInt32 x;
			SqlInt32 y;

			// Case 1: either is SqlInt32.Null
			x = SqlInt32.Null;
			y = new SqlInt32 (5);
			Assert.AreEqual (x <= y, SqlBoolean.Null, "Less Than Or Equal operator didn't return Null when one was Null.");
			Assert.AreEqual (SqlInt32.LessThanOrEqual (x, y), SqlBoolean.Null, "Less Than Or Equal function didn't return Null when one was Null.");

			// Case 2: both are SqlInt32.Null
			y = SqlInt32.Null;
			Assert.AreEqual (x <= y, SqlBoolean.Null, "Less Than Or Equal operator didn't return Null when both were Null.");
			Assert.AreEqual (SqlInt32.LessThanOrEqual (x, y), SqlBoolean.Null, "Less Than Or Equal function didn't return Null when both were Null.");

			// Case 3: x > y
			x = new SqlInt32 (5);
			y = new SqlInt32 (4);
			Assert.AreEqual (x <= y, SqlBoolean.False, "Less than or equal operator didn't return false when x > y.");
			Assert.AreEqual (SqlInt32.LessThanOrEqual (x,y), SqlBoolean.False, "Less than or equal function didn't return false when x > y.");

			// Case 4: x < y
			x = new SqlInt32 (5);
			y = new SqlInt32 (6);
			Assert.AreEqual (x <= y, SqlBoolean.True, "Less than or equal operator didn't return true when x < y.");
			Assert.AreEqual (SqlInt32.LessThanOrEqual (x,y), SqlBoolean.True, "Less than or equal function didn't return true when x < y.");

			// Case 5: x == y
			x = new SqlInt32 (5);
			y = new SqlInt32 (5);
			Assert.AreEqual (x <= y, SqlBoolean.True, "Less than or equal operator didn't return true when x == y.");
			Assert.AreEqual (SqlInt32.LessThanOrEqual (x,y), SqlBoolean.True, "Less than or equal function didn't return true when x == y.");
		}

		[Test]
		public void Mod ()
		{
			int a = 5;
			int b = 7;

			SqlInt32 x = new SqlInt32 (a);
			SqlInt32 y = new SqlInt32 (b);
			SqlInt32 z = x % y;
			Assert.AreEqual (z.Value, a % b, "Modulus operator does not work correctly");
			z = SqlInt32.Mod (x, y);
			Assert.AreEqual (z.Value, a % b, "Modulus function does not work correctly");
		}

#if NET_2_0
		[Test]
		public void Modulus ()
		{
			int a = 50;
			int b = 7;
			SqlInt32 x = new SqlInt32 (a);
			SqlInt32 y = new SqlInt32 (b);
			SqlInt32 z = x % y;
			Assert.AreEqual (z.Value, a % b, "Modulus operator does not work correctly");
			z = SqlInt32.Modulus (x, y);
			Assert.AreEqual (z.Value, a % b, "Modulus function does not work correctly");
		}
#endif

		[Test]
		public void Multiply ()
		{
			int a = 5;
			int b = 7;
			
			SqlInt32 x = new SqlInt32 (a);
			SqlInt32 y = new SqlInt32 (b);
			SqlInt32 z = x * y;
			Assert.AreEqual (z.Value, a * b, "Multiplication operator does not work correctly");
			z = SqlInt32.Multiply (x, y);
			Assert.AreEqual (z.Value, a * b, "Multiplication function does not work correctly");
		}

		[Test]
		public void NotEquals ()
		{
			SqlInt32 x;
			SqlInt32 y;

			x = new SqlInt32 (5);
			y = SqlInt32.Null;

			Assert.AreEqual (x != y, SqlBoolean.Null, "Not Equals operator does not return null when one or both of the parameters is Null.");
			Assert.AreEqual (SqlInt32.NotEquals (x, y), SqlBoolean.Null, "Not Equals function does not return null when one or both of the parameters is Null.");

			y = new SqlInt32 (5);
			Assert.AreEqual (x != y, SqlBoolean.False, "Not Equals operator does not return false when x == y.");
			Assert.AreEqual (SqlInt32.NotEquals (x, y), SqlBoolean.False, "Not Equals function does not return false when x == y.");

			y = new SqlInt32 (6);
			Assert.AreEqual (x != y, SqlBoolean.True, "Not Equals operator does not return true when x != y.");
			Assert.AreEqual (SqlInt32.NotEquals (x, y), SqlBoolean.True, "Not Equals function does not return true when x != y.");
		}

		[Test]
		public void OnesComplement ()
		{
			int a = 5;

			SqlInt32 x = new SqlInt32 (a);
			SqlInt32 z = ~x;
			Assert.AreEqual (z.Value, ~a, "Ones Complement operator does not work correctly");
			z = SqlInt32.OnesComplement (x);
			Assert.AreEqual (z.Value, ~a, "Ones Complement function does not work correctly");
		}

		[Test]
		public void IsNullProperty ()
		{
			SqlInt32 n = SqlInt32.Null;
			Assert.IsTrue (n.IsNull, "Null is not defined correctly");
		}

		[Test]
		public void Subtract () 
		{
			int a = 7;
			int b = 5;

			SqlInt32 x = new SqlInt32 (a);
			SqlInt32 y = new SqlInt32 (b);
			SqlInt32 z = x - y;
			Assert.AreEqual (z.Value, a - b, "Subtraction operator does not work correctly");
			z = SqlInt32.Subtract (x, y);
			Assert.AreEqual (z.Value, a - b, "Subtraction function does not work correctly");
		}

		[Test]
		public void ConversionMethods ()
		{
			SqlInt32 x;

			// Case 1: SqlInt32.Null -> SqlBoolean == SqlBoolean.Null
			x = SqlInt32.Null;
			Assert.AreEqual (x.ToSqlBoolean (), SqlBoolean.Null , "SqlInt32.Null -> SqlBoolean does not yield SqlBoolean.Null.");

			// Case 2: SqlInt32.Zero -> SqlBoolean == False
			x = SqlInt32.Zero;
			Assert.AreEqual (x.ToSqlBoolean (), SqlBoolean.False , "SqlInt32.Zero -> SqlBoolean does not yield SqlBoolean.False.");
		
			// Case 3: SqlInt32(nonzero) -> SqlBoolean == True
			x = new SqlInt32 (27);
			Assert.AreEqual (x.ToSqlBoolean (), SqlBoolean.True , "SqlInt32 (27) -> SqlBoolean does not yield SqlBoolean.True.");
		
			// Case 4: SqlInt32.Null -> SqlByte == SqlByte.Null
			x = SqlInt32.Null;
			Assert.AreEqual (x.ToSqlByte (), SqlByte.Null , "SqlInt32.Null -> SqlByte does not yield SqlByte.Null.");

			// Case 5: Test non-null conversion to SqlByte
			x = new SqlInt32 (27);
			Assert.AreEqual (x.ToSqlByte ().Value, (byte)27, "SqlInt32 (27) -> SqlByte does not yield a value of 27");

			// Case 6: SqlInt32.Null -> SqlDecimal == SqlDecimal.Null
			x = SqlInt32.Null;
			Assert.AreEqual (x.ToSqlDecimal (), SqlDecimal.Null , "SqlInt32.Null -> SqlDecimal does not yield SqlDecimal.Null.");

			// Case 7: Test non-null conversion to SqlDecimal
			x = new SqlInt32 (27);
			Assert.AreEqual (x.ToSqlDecimal ().Value, (decimal)27, "SqlInt32 (27) -> SqlDecimal does not yield a value of 27");

			// Case 8: SqlInt32.Null -> SqlDouble == SqlDouble.Null
			x = SqlInt32.Null;
			Assert.AreEqual (x.ToSqlDouble (), SqlDouble.Null , "SqlInt32.Null -> SqlDouble does not yield SqlDouble.Null.");

			// Case 9: Test non-null conversion to SqlDouble
			x = new SqlInt32 (27);
			Assert.AreEqual (x.ToSqlDouble ().Value, (double)27, "SqlInt32 (27) -> SqlDouble does not yield a value of 27");

			// Case 10: SqlInt32.Null -> SqlInt16 == SqlInt16.Null
			x = SqlInt32.Null;
			Assert.AreEqual (x.ToSqlInt16 (), SqlInt16.Null , "SqlInt32.Null -> SqlInt16 does not yield SqlInt16.Null.");

			// Case 11: Test non-null conversion to SqlInt16
			x = new SqlInt32 (27);
			Assert.AreEqual (x.ToSqlInt16 ().Value, (short)27, "SqlInt32 (27) -> SqlInt16 does not yield a value of 27");

			// Case 12: SqlInt32.Null -> SqlInt64 == SqlInt64.Null
			x = SqlInt32.Null;
			Assert.AreEqual (x.ToSqlInt64 (), SqlInt64.Null , "SqlInt32.Null -> SqlInt64 does not yield SqlInt64.Null.");

			// Case 13: Test non-null conversion to SqlInt64
			x = new SqlInt32 (27);
			Assert.AreEqual (x.ToSqlInt64 ().Value, (long)27, "SqlInt32 (27) -> SqlInt64 does not yield a value of 27");

			// Case 14: SqlInt32.Null -> SqlMoney == SqlMoney.Null
			x = SqlInt32.Null;
			Assert.AreEqual (x.ToSqlMoney (), SqlMoney.Null , "SqlInt32.Null -> SqlMoney does not yield SqlMoney.Null.");

			// Case 15: Test non-null conversion to SqlMoney
			x = new SqlInt32 (27);
			Assert.AreEqual (x.ToSqlMoney ().Value, 27.0000M, "SqlInt32 (27) -> SqlMoney does not yield a value of 27");

			// Case 16: SqlInt32.Null -> SqlSingle == SqlSingle.Null
			x = SqlInt32.Null;
			Assert.AreEqual (x.ToSqlSingle (), SqlSingle.Null , "SqlInt32.Null -> SqlSingle does not yield SqlSingle.Null.");

			// Case 17: Test non-null conversion to SqlSingle
			x = new SqlInt32 (27);
			Assert.AreEqual (x.ToSqlSingle ().Value, (float)27, "SqlInt32 (27) -> SqlSingle does not yield a value of 27");
		}

		[Test]
		public void Xor ()
		{
			int a = 5;
			int b = 7;

			SqlInt32 x = new SqlInt32 (a);
			SqlInt32 y = new SqlInt32 (b);
			SqlInt32 z = x ^ y;
			Assert.AreEqual (z.Value, a ^ b, "Xor operator does not work correctly");
			z = SqlInt32.Xor (x, y);
			Assert.AreEqual (z.Value, a ^ b, "Xor function does not work correctly");
		}

#if NET_2_0
		[Test]
		public void GetXsdTypeTest ()
		{
			XmlQualifiedName qualifiedName = SqlInt32.GetXsdType (null);
			NUnit.Framework.Assert.AreEqual ("int", qualifiedName.Name, "#A01");
		}

		internal void ReadWriteXmlTestInternal (string xml, 
						       int testval, 
						       string unit_test_id)
		{
			SqlInt32 test;
			SqlInt32 test1;
			XmlSerializer ser;
			StringWriter sw;
			XmlTextWriter xw;
			StringReader sr;
			XmlTextReader xr;

			test = new SqlInt32 (testval);
			ser = new XmlSerializer(typeof(SqlInt32));
			sw = new StringWriter ();
			xw = new XmlTextWriter (sw);
			
			ser.Serialize (xw, test);

			// Assert.AreEqual (xml, sw.ToString (), unit_test_id);

			sr = new StringReader (xml);
			xr = new XmlTextReader (sr);
			test1 = (SqlInt32)ser.Deserialize (xr);

			Assert.AreEqual (testval, test1.Value, unit_test_id);
		}

		[Test]
		public void ReadWriteXmlTest ()
		{
			string xml1 = "<?xml version=\"1.0\" encoding=\"utf-16\"?><int>4556</int>";
			string xml2 = "<?xml version=\"1.0\" encoding=\"utf-16\"?><int>-6445</int>";
			string xml3 = "<?xml version=\"1.0\" encoding=\"utf-16\"?><int>0x455687AB3E4D56F</int>";
			int test1 = 4556;
			int test2 = -6445;
			int test3 = 0x4F56;

			ReadWriteXmlTestInternal (xml1, test1, "BA01");
			ReadWriteXmlTestInternal (xml2, test2, "BA02");

			try {
				ReadWriteXmlTestInternal (xml3, test3, "#BA03");
				Assert.Fail ("BA03");
			} catch (FormatException e) {
				Assert.AreEqual (typeof (FormatException), e.GetType (), "#BA03");
			}
		}
#endif
	}
}
