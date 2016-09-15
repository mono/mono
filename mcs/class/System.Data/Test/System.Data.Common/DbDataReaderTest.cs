// DbDataReaderTest.cs - NUnit Test Cases for DbDataReader class
//
// Author:
//	Mika Aalto (mika@aalto.pro)
//
// Copyright (C) 2014 Mika Aalto
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
using System.Data;
using System.Data.Common;
using System.IO;

namespace MonoTests.System.Data.Common
{
	[TestFixture]
	public class DbDataReaderTest
	{
		DbDataReaderMock dataReader;

		[SetUp]
		public void SetUp ()
		{
			//Setup test data table
			DataTable testData = new DataTable ();
			testData.Columns.Add ("text_col", typeof(string));
			testData.Columns.Add ("binary_col", typeof(byte[]));

			testData.Rows.Add ("row_1", new byte[] { 0xde, 0xad, 0xbe, 0xef });
			testData.Rows.Add ("row_2", DBNull.Value);
			testData.Rows.Add ("row_3", new byte[] { 0x00 });

			dataReader = new DbDataReaderMock (testData);

			Assert.AreEqual (3, testData.Rows.Count);
		}

		[TearDown]
		public void TearDown ()
		{
		}

		[Test]
		public void GetFieldValueTest ()
		{
			//First row
			dataReader.Read ();
			Assert.AreEqual ("row_1", dataReader.GetFieldValue<string> (0), "#1");
			byte[] expected_data = new byte[] { 0xde, 0xad, 0xbe, 0xef };
			byte[] actual_data = dataReader.GetFieldValue<byte[]> (1);
			Assert.AreEqual (expected_data.Length, actual_data.Length, "#2");
			for (int i = 0; i < expected_data.Length; i++) {
				Assert.AreEqual (expected_data [i], actual_data [i], "#3 at index " + i);
			}

			//Second row where data row column value is DBNull
			dataReader.Read ();
			Assert.AreEqual ("row_2", dataReader.GetFieldValue<string> (0), "#4");
			try {
				actual_data = dataReader.GetFieldValue<byte[]> (1);
				Assert.Fail ("GetFieldValue method should throw InvalidCastException for DBNull values #5");
			} catch (InvalidCastException) {
				//This is expected
			}

			//Third row
			dataReader.Read ();
			Assert.AreEqual ("row_3", dataReader.GetFieldValue<string> (0), "#6");
			expected_data = new byte[] { 0x00 };
			actual_data = dataReader.GetFieldValue<byte[]> (1);
			Assert.AreEqual (expected_data.Length, actual_data.Length, "#7");
			Assert.AreEqual (expected_data [0], actual_data [0], "#8");
		}

		[Test]
		public void GetStreamTest ()
		{
			int testColOrdinal = 1;
			byte[] buffer = new byte[1024];

			dataReader.Read ();
			Stream stream = dataReader.GetStream (testColOrdinal);
			Assert.IsNotNull (stream, "Stream from datareader is null #1");

			//Read stream content to byte buffer
			int data_length = stream.Read (buffer, 0, buffer.Length);

			//Verify that content is expected
			byte[] expected = new byte[] { 0xde, 0xad, 0xbe, 0xef };
			Assert.AreEqual (expected.Length, data_length, "#2");
			for (int i = 0; i < expected.Length; i++) {
				Assert.AreEqual (expected [i], buffer [i], "#3 at index " + i);
			}

			//Get DBNull value stream
			Assert.IsTrue (dataReader.Read ());
			stream = dataReader.GetStream (testColOrdinal);
			Assert.AreEqual (0, stream.Length, "#4");

			//Get single byte value stream
			Assert.IsTrue (dataReader.Read ());
			stream = dataReader.GetStream (testColOrdinal);
			expected = new byte[] { 0x00 };
			Assert.AreEqual (expected.Length, stream.Length, "#5");
			Assert.AreEqual (expected [0], stream.ReadByte (), "#6");
		}

		[Test]
		public void GetTextReader ()
		{
			int testColOrdinal = 0;

			//Read first row
			dataReader.Read ();
			TextReader textReader = dataReader.GetTextReader (testColOrdinal);
			Assert.IsNotNull (textReader, "return value from datareader GetTextReader method is null #1");

			string txt = textReader.ReadToEnd ();
			Assert.AreEqual ("row_1", txt, "#2");

			//Move to second row
			Assert.IsTrue (dataReader.Read ());
			textReader = dataReader.GetTextReader (testColOrdinal);
			txt = textReader.ReadToEnd ();
			Assert.AreEqual ("row_2", txt, "#3");

			//Move to third row
			Assert.IsTrue (dataReader.Read ());
			textReader = dataReader.GetTextReader (testColOrdinal);
			txt = textReader.ReadToEnd ();
			Assert.AreEqual ("row_3", txt, "#4");

			Assert.IsFalse (dataReader.Read (), "#5");
		}

	}
}

