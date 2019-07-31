//
// BitConverterTest.cs - NUnit Test Cases for System.BitConverter
//
// author:
//   Duco Fijma (duco@lorentz.xs4all.nl)
//
// (C) 2002 Duco Fijma
// Copyright (C) 2004 Novell (http://www.novell.com)
// 

using NUnit.Framework;
using System;

namespace MonoTests.System
{
	[TestFixture]
	public class BitConverterTest
	{
		public void TestIsLittleEndian ()
		{
			byte [] b = BitConverter.GetBytes (1);
			Assert.AreEqual (b [0] == 1, BitConverter.IsLittleEndian, "#1");
		}

		private void PrivateTestSingle (float v1)
		{
			float v2;
			byte [] b;
			byte [] larger = new byte [] { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09 };

			b = BitConverter.GetBytes (v1);
			Assert.AreEqual (4, b.Length, "#A1");

			v2 = BitConverter.ToSingle (b, 0);
			Assert.AreEqual (v1, v2, "#A2");

			b.CopyTo (larger, 1);
			v2 = BitConverter.ToSingle (larger, 1);
			Assert.AreEqual (v1, v2, "#A3");

			try {
				BitConverter.ToSingle (larger, 8);
				Assert.Fail ("#B1");
			} catch (ArgumentException ex) {
				// Destination array is not long enough to copy all the items
				// in the collection. Check array index and length
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
				Assert.IsNotNull (ex.ParamName, "#B5");
			}

			try {
				BitConverter.ToSingle (larger, 10);
				Assert.Fail ("#C1");
			} catch (ArgumentOutOfRangeException ex) {
				// Index was out of range. Must be non-negative and less than
				// the size of the collection
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#C2");
				Assert.IsNull (ex.InnerException, "#C3");
				Assert.IsNotNull (ex.Message, "#C4");
				Assert.IsNotNull (ex.ParamName, "#C5");
				Assert.AreEqual ("startIndex", ex.ParamName, "#C6");
			}
		}

		[Test]
		public void TestSingle ()
		{
			PrivateTestSingle (0.1f);
			PrivateTestSingle (24.1e30f);
		}

		[Test]
		public void ToSingle_Value_Null ()
		{
			try {
				BitConverter.ToSingle ((byte []) null, 77);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("value", ex.ParamName, "#6");
			}
		}

		private void PrivateTestDouble (double v1)
		{
			double v2;
			byte [] b;
			byte [] larger = new byte [] { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09 };

			b = BitConverter.GetBytes (v1);
			Assert.AreEqual (8, b.Length, "#A1");

			v2 = BitConverter.ToDouble (b, 0);
			Assert.AreEqual (v1, v2, "#A2");

			b.CopyTo (larger, 1);
			v2 = BitConverter.ToDouble (larger, 1);
			Assert.AreEqual (v1, v2, "#A3");

			try {
				BitConverter.ToDouble (larger, 3);
				Assert.Fail ("#B1");
			} catch (ArgumentException ex) {
				// Destination array is not long enough to copy all the items
				// in the collection. Check array index and length
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
				Assert.IsNotNull (ex.ParamName, "#B5");
			}

			try {
				BitConverter.ToDouble (larger, 10);
				Assert.Fail ("#C1");
			} catch (ArgumentOutOfRangeException ex) {
				// Index was out of range. Must be non-negative and less than
				// the size of the collection
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#C2");
				Assert.IsNull (ex.InnerException, "#C3");
				Assert.IsNotNull (ex.Message, "#C4");
				Assert.IsNotNull (ex.ParamName, "#C5");
				Assert.AreEqual ("startIndex", ex.ParamName, "#C6");
			}
		}

		[Test]
		public void TestDouble ()
		{
			double d = 123.321;

			Assert.AreEqual (d, BitConverter.Int64BitsToDouble (BitConverter.DoubleToInt64Bits (d)), "#1");

			PrivateTestDouble (0.1);
			PrivateTestDouble (24.1e77);
		}

		[Test]
		public void ToDouble_Value_Null ()
		{
			try {
				BitConverter.ToDouble ((byte []) null, 77);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("value", ex.ParamName, "#6");
			}
		}

		private void PrivateTestBool (bool v1)
		{
			bool v2;
			byte [] b;
			byte [] larger = new byte [] { 0x00, 0x01, 0x02, 0x03 };

			b = BitConverter.GetBytes (v1);
			Assert.AreEqual (1, b.Length, "#A1");

			v2 = BitConverter.ToBoolean (b, 0);
			Assert.AreEqual (v1, v2, "#A2");

			b.CopyTo (larger, 1);
			v2 = BitConverter.ToBoolean (larger, 1);
			Assert.AreEqual (v1, v2, "#A3");

			try {
				BitConverter.ToBoolean (larger, 4);
				Assert.Fail ("#B1");
			} catch (ArgumentOutOfRangeException ex) {
				// Index was out of range. Must be non-negative and less than
				// the size of the collection
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
				Assert.IsNotNull (ex.ParamName, "#B5");
				Assert.AreEqual ("startIndex", ex.ParamName, "#B6");
			}
		}

		[Test]
		public void TestBool ()
		{
			PrivateTestBool (true);
			PrivateTestBool (false);
		}

		[Test]
		public void ToBoolean_Value_Null ()
		{
			try {
				BitConverter.ToBoolean ((byte []) null, 77);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("value", ex.ParamName, "#6");
			}
		}

		private void PrivateTestChar (char v1)
		{
			char v2;
			byte [] b;
			byte [] larger = new byte [] { 0x00, 0x01, 0x02, 0x03 };

			b = BitConverter.GetBytes (v1);
			Assert.AreEqual (2, b.Length, "#A1");

			v2 = BitConverter.ToChar (b, 0);
			Assert.AreEqual (v1, v2, "#A2");

			b.CopyTo (larger, 1);
			v2 = BitConverter.ToChar (larger, 1);
			Assert.AreEqual (v1, v2, "#A3");

			try {
				BitConverter.ToChar (larger, 3);
				Assert.Fail ("#B1");
			} catch (ArgumentException ex) {
				// Destination array is not long enough to copy all the items
				// in the collection. Check array index and length
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
				Assert.IsNotNull (ex.ParamName, "#B5");
			}

			try {
				BitConverter.ToChar (larger, 4);
				Assert.Fail ("#C1");
			} catch (ArgumentOutOfRangeException ex) {
				// Index was out of range. Must be non-negative and less than
				// the size of the collection
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#C2");
				Assert.IsNull (ex.InnerException, "#C3");
				Assert.IsNotNull (ex.Message, "#C4");
				Assert.IsNotNull (ex.ParamName, "#C5");
				Assert.AreEqual ("startIndex", ex.ParamName, "#C6");
			}
		}

		[Test]
		public void TestChar ()
		{
			PrivateTestChar ('A');
			PrivateTestChar ('\x01ff');
		}

		[Test]
		public void ToChar_Value_Null ()
		{
			try {
				BitConverter.ToChar ((byte []) null, 77);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("value", ex.ParamName, "#6");
			}
		}

		private void PrivateTestInt16 (short v1)
		{
			short v2;
			byte [] b;
			byte [] larger = new byte [] { 0x00, 0x01, 0x02, 0x03 };

			b = BitConverter.GetBytes (v1);
			Assert.AreEqual (2, b.Length, "#A1");

			v2 = BitConverter.ToInt16 (b, 0);
			Assert.AreEqual (v1, v2, "#A2");

			b.CopyTo (larger, 1);
			v2 = BitConverter.ToInt16 (larger, 1);
			Assert.AreEqual (v1, v2, "#A3");

			try {
				BitConverter.ToInt16 (larger, 3);
				Assert.Fail ("#B1");
			} catch (ArgumentException ex) {
				// Destination array is not long enough to copy all the items
				// in the collection. Check array index and length
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
				Assert.IsNotNull (ex.ParamName, "#B5");
			}

			try {
				BitConverter.ToInt16 (larger, 4);
				Assert.Fail ("#C1");
			} catch (ArgumentOutOfRangeException ex) {
				// Index was out of range. Must be non-negative and less than
				// the size of the collection
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#C2");
				Assert.IsNull (ex.InnerException, "#C3");
				Assert.IsNotNull (ex.Message, "#C4");
				Assert.IsNotNull (ex.ParamName, "#C5");
				Assert.AreEqual ("startIndex", ex.ParamName, "#C6");
			}
		}

		[Test]
		public void TestInt16 ()
		{
			PrivateTestInt16 (0);
			PrivateTestInt16 (1000);
			PrivateTestInt16 (-32768);
			PrivateTestInt16 (32767);
		}

		[Test]
		public void ToInt16_Value_Null ()
		{
			try {
				BitConverter.ToInt16 ((byte []) null, 77);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("value", ex.ParamName, "#6");
			}
		}

		private void PrivateTestUInt16 (ushort v1)
		{
			ushort v2;
			byte [] b;
			byte [] larger = new byte [] { 0x00, 0x01, 0x02, 0x03 };

			b = BitConverter.GetBytes (v1);
			Assert.AreEqual (2, b.Length, "#A1");

			v2 = BitConverter.ToUInt16 (b, 0);
			Assert.AreEqual (v1, v2, "#A2");

			b.CopyTo (larger, 1);
			v2 = BitConverter.ToUInt16 (larger, 1);
			Assert.AreEqual (v1, v2, "#A3");

			try {
				BitConverter.ToUInt16 (larger, 3);
				Assert.Fail ("#B1");
			} catch (ArgumentException ex) {
				// Destination array is not long enough to copy all the items
				// in the collection. Check array index and length
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
				Assert.IsNotNull (ex.ParamName, "#B5");
			}

			try {
				BitConverter.ToUInt16 (larger, 4);
				Assert.Fail ("#C1");
			} catch (ArgumentOutOfRangeException ex) {
				// Index was out of range. Must be non-negative and less than
				// the size of the collection
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#C2");
				Assert.IsNull (ex.InnerException, "#C3");
				Assert.IsNotNull (ex.Message, "#C4");
				Assert.IsNotNull (ex.ParamName, "#C5");
				Assert.AreEqual ("startIndex", ex.ParamName, "#C6");
			}
		}

		[Test]
		public void TestUInt16 ()
		{
			PrivateTestUInt16 (0);
			PrivateTestUInt16 (1000);
			PrivateTestUInt16 (65535);
		}

		[Test]
		public void ToUInt16_Value_Null ()
		{
			try {
				BitConverter.ToUInt16 ((byte []) null, 77);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("value", ex.ParamName, "#6");
			}
		}

		private void PrivateTestInt32 (int v1)
		{
			int v2;
			byte [] b;
			byte [] larger = new byte [] { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09 };

			b = BitConverter.GetBytes (v1);
			Assert.AreEqual (4, b.Length, "#A1");

			v2 = BitConverter.ToInt32 (b, 0);
			Assert.AreEqual (v1, v2, "#A2");

			b.CopyTo (larger, 1);
			v2 = BitConverter.ToInt32 (larger, 1);
			Assert.AreEqual (v1, v2, "#A3");

			try {
				BitConverter.ToInt32 (larger, 8);
				Assert.Fail ("#B1");
			} catch (ArgumentException ex) {
				// Destination array is not long enough to copy all the items
				// in the collection. Check array index and length
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
				Assert.IsNotNull (ex.ParamName, "#B5");
			}

			try {
				BitConverter.ToInt32 (larger, 10);
				Assert.Fail ("#C1");
			} catch (ArgumentOutOfRangeException ex) {
				// Index was out of range. Must be non-negative and less than
				// the size of the collection
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#C2");
				Assert.IsNull (ex.InnerException, "#C3");
				Assert.IsNotNull (ex.Message, "#C4");
				Assert.IsNotNull (ex.ParamName, "#C5");
				Assert.AreEqual ("startIndex", ex.ParamName, "#C6");
			}
		}

		[Test]
		public void TestInt32 ()
		{
			PrivateTestInt32 (0);
			PrivateTestInt32 (1000);
			PrivateTestInt32 (-2147483648);
			PrivateTestInt32 (2147483647);
		}

		[Test]
		public void ToInt32_Value_Null ()
		{
			try {
				BitConverter.ToInt32 ((byte []) null, 77);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("value", ex.ParamName, "#6");
			}
		}

		private void PrivateTestUInt32 (uint v1)
		{
			uint v2;
			byte [] b;
			byte [] larger = new byte [] { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09 };

			b = BitConverter.GetBytes (v1);
			Assert.AreEqual (4, b.Length, "#A1");

			v2 = BitConverter.ToUInt32 (b, 0);
			Assert.AreEqual (v1, v2, "#A2");

			b.CopyTo (larger, 1);
			v2 = BitConverter.ToUInt32 (larger, 1);
			Assert.AreEqual (v1, v2, "#A3");

			try {
				BitConverter.ToUInt32 (larger, 8);
				Assert.Fail ("#B1");
			} catch (ArgumentException ex) {
				// Destination array is not long enough to copy all the items
				// in the collection. Check array index and length
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
				Assert.IsNotNull (ex.ParamName, "#B5");
			}

			try {
				BitConverter.ToUInt32 (larger, 10);
				Assert.Fail ("#C1");
			} catch (ArgumentOutOfRangeException ex) {
				// Index was out of range. Must be non-negative and less than
				// the size of the collection
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#C2");
				Assert.IsNull (ex.InnerException, "#C3");
				Assert.IsNotNull (ex.Message, "#C4");
				Assert.IsNotNull (ex.ParamName, "#C5");
				Assert.AreEqual ("startIndex", ex.ParamName, "#C6");
			}
		}

		[Test]
		public void TestUInt32 ()
		{
			PrivateTestUInt32 (0u);
			PrivateTestUInt32 (1000u);
			PrivateTestUInt32 (4294967295u);
		}

		[Test]
		public void ToUInt32_Value_Null ()
		{
			try {
				BitConverter.ToUInt32 ((byte []) null, 77);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("value", ex.ParamName, "#6");
			}
		}

		private void PrivateTestInt64 (long v1)
		{
			long v2;
			byte [] b;
			byte [] larger = new byte [] { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09 };

			b = BitConverter.GetBytes (v1);
			Assert.AreEqual (8, b.Length, "#A1");

			v2 = BitConverter.ToInt64 (b, 0);
			Assert.AreEqual (v1, v2, "#A2");

			b.CopyTo (larger, 1);
			v2 = BitConverter.ToInt64 (larger, 1);
			Assert.AreEqual (v1, v2, "#A3");

			try {
				BitConverter.ToInt64 (larger, 8);
				Assert.Fail ("#B1");
			} catch (ArgumentException ex) {
				// Destination array is not long enough to copy all the items
				// in the collection. Check array index and length
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
				Assert.IsNotNull (ex.ParamName, "#B5");
			}

			try {
				BitConverter.ToInt64 (larger, 10);
				Assert.Fail ("#C1");
			} catch (ArgumentOutOfRangeException ex) {
				// Index was out of range. Must be non-negative and less than
				// the size of the collection
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#C2");
				Assert.IsNull (ex.InnerException, "#C3");
				Assert.IsNotNull (ex.Message, "#C4");
				Assert.IsNotNull (ex.ParamName, "#C5");
				Assert.AreEqual ("startIndex", ex.ParamName, "#C6");
			}
		}

		[Test]
		public void TestInt64 ()
		{
			PrivateTestInt64 (0);
			PrivateTestInt64 (1000);
			PrivateTestInt64 (-9223372036854775808);
			PrivateTestInt64 (9223372036854775807);
		}

		[Test]
		public void ToInt64_Value_Null ()
		{
			try {
				BitConverter.ToInt64 ((byte []) null, 77);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("value", ex.ParamName, "#6");
			}
		}

		private void PrivateTestUInt64 (ulong v1)
		{
			ulong v2;
			byte [] b;
			byte [] larger = new byte [] { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09 };

			b = BitConverter.GetBytes (v1);
			Assert.AreEqual (8, b.Length, "#A1");

			v2 = BitConverter.ToUInt64 (b, 0);
			Assert.AreEqual (v1, v2, "#A2");

			b.CopyTo (larger, 1);
			v2 = BitConverter.ToUInt64 (larger, 1);
			Assert.AreEqual (v1, v2, "#A3");

			try {
				BitConverter.ToUInt64 (larger, 8);
				Assert.Fail ("#B1");
			} catch (ArgumentException ex) {
				// Destination array is not long enough to copy all the items
				// in the collection. Check array index and length
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
				Assert.IsNotNull (ex.ParamName, "#B5");
			}

			try {
				BitConverter.ToUInt64 (larger, 10);
				Assert.Fail ("#C1");
			} catch (ArgumentOutOfRangeException ex) {
				// Index was out of range. Must be non-negative and less than
				// the size of the collection
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#C2");
				Assert.IsNull (ex.InnerException, "#C3");
				Assert.IsNotNull (ex.Message, "#C4");
				Assert.IsNotNull (ex.ParamName, "#C5");
				Assert.AreEqual ("startIndex", ex.ParamName, "#C6");
			}
		}

		[Test]
		public void TestUInt64 ()
		{
			PrivateTestUInt64 (0);
			PrivateTestUInt64 (1000);
			PrivateTestUInt64 (18446744073709551615);
		}

		[Test]
		public void ToUInt64_Value_Null ()
		{
			try {
				BitConverter.ToUInt64 ((byte []) null, 77);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("value", ex.ParamName, "#6");
			}
		}

		[Test]
		public void TestToString ()
		{
			byte [] b = new byte [] { 0x00, 0x11, 0x22, 0x33, 0x44, 0x55, 0x66, 0x77, 0x88, 0x99, 0xaa, 0xbb, 0xcc, 0xdd, 0xee, 0xff };

			Assert.AreEqual ("00-11-22-33-44-55-66-77-88-99-AA-BB-CC-DD-EE-FF", BitConverter.ToString (b), "#A1");
			Assert.AreEqual ("66-77-88-99-AA-BB-CC-DD-EE-FF", BitConverter.ToString (b, 6), "#A2");
			Assert.AreEqual ("66-77-88", BitConverter.ToString (b, 6, 3), "#A3");
			Assert.AreEqual (string.Empty, BitConverter.ToString (b, 6, 0), "#A4");

			try {
				BitConverter.ToString (b, 20);
				Assert.Fail ("#B1");
			} catch (ArgumentOutOfRangeException ex) {
				// StartIndex cannot be less than zero
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
				Assert.IsNotNull (ex.ParamName, "#B5");
				Assert.AreEqual ("startIndex", ex.ParamName, "#B6");
			}

			try {
				BitConverter.ToString (b, 20, 3);
				Assert.Fail ("#C1");
			} catch (ArgumentOutOfRangeException ex) {
				// StartIndex cannot be less than zero
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#C2");
				Assert.IsNull (ex.InnerException, "#C3");
				Assert.IsNotNull (ex.Message, "#C4");
				Assert.IsNotNull (ex.ParamName, "#C5");
				Assert.AreEqual ("startIndex", ex.ParamName, "#C6");
			}

			try {
				BitConverter.ToString (b, 16, 0);
				Assert.Fail ("#D1");
			} catch (ArgumentOutOfRangeException ex) {
				// StartIndex cannot be less than zero
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#D2");
				Assert.IsNull (ex.InnerException, "#D3");
				Assert.IsNotNull (ex.Message, "#D4");
				Assert.IsNotNull (ex.ParamName, "#D5");
				Assert.AreEqual ("startIndex", ex.ParamName, "#D6");
			}
		}

		[Test]
		public void ToString_Value_Empty ()
		{
			byte [] empty = new byte [0];
			Assert.AreEqual (String.Empty, BitConverter.ToString (empty), "#A1");

			try {
				BitConverter.ToString (empty, 3);
				Assert.Fail ("#B1");
			} catch (ArgumentOutOfRangeException ex) {
				// StartIndex cannot be less than zero
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
				Assert.IsNotNull (ex.ParamName, "#B5");
				Assert.AreEqual ("startIndex", ex.ParamName, "#B6");
			}

			Assert.AreEqual (String.Empty, BitConverter.ToString (empty, 0), "#C1");

			try {
				BitConverter.ToString (empty, 3, 0);
				Assert.Fail ("#D1");
			} catch (ArgumentOutOfRangeException ex) {
				// StartIndex cannot be less than zero
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#D2");
				Assert.IsNull (ex.InnerException, "#D3");
				Assert.IsNotNull (ex.Message, "#D4");
				Assert.IsNotNull (ex.ParamName, "#D5");
				Assert.AreEqual ("startIndex", ex.ParamName, "#D6");
			}

			Assert.AreEqual (string.Empty, BitConverter.ToString (empty, 0, 0), "#E1");

			try {
				BitConverter.ToString (empty, 3, -1);
				Assert.Fail ("#F1");
			} catch (ArgumentOutOfRangeException ex) {
				// StartIndex cannot be less than zero
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#F2");
				Assert.IsNull (ex.InnerException, "#F3");
				Assert.IsNotNull (ex.Message, "#F4");
				Assert.IsNotNull (ex.ParamName, "#F5");
				Assert.AreEqual ("startIndex", ex.ParamName, "#F6");
			}
		}

		[Test]
		public void ToString_Value_Null ()
		{
			try {
				BitConverter.ToString ((byte []) null);
				Assert.Fail ("#A1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.IsNotNull (ex.ParamName, "#A5");
				Assert.AreEqual ("value", ex.ParamName, "#A6");
			}

			try {
				BitConverter.ToString ((byte []) null, 20);
				Assert.Fail ("#B1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
				Assert.IsNotNull (ex.ParamName, "#B5");
				Assert.AreEqual ("value", ex.ParamName, "#B6");
			}

			try {
				BitConverter.ToString ((byte []) null, 20, 3);
				Assert.Fail ("#C1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#C2");
				Assert.IsNull (ex.InnerException, "#C3");
				Assert.IsNotNull (ex.Message, "#C4");
				Assert.IsNotNull (ex.ParamName, "#C5");
				Assert.AreEqual ("value", ex.ParamName, "#C6");
			}
		}

		[Test]
		public void ToString_StartIndex_Negative ()
		{
			try {
				BitConverter.ToString (new byte [0], -1);
				Assert.Fail ("#A1");
			} catch (ArgumentOutOfRangeException ex) {
				// StartIndex cannot be less than zero
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.IsNotNull (ex.ParamName, "#A5");
				Assert.AreEqual ("startIndex", ex.ParamName, "#A6");
			}

			try {
				BitConverter.ToString (new byte [4], -1, 1);
				Assert.Fail ("#B1");
			} catch (ArgumentOutOfRangeException ex) {
				// StartIndex cannot be less than zero
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
				Assert.IsNotNull (ex.ParamName, "#B5");
				Assert.AreEqual ("startIndex", ex.ParamName, "#B6");
			}
		}

		[Test]
		public void ToString_StartIndex_Overflow ()
		{
			byte [] array = new byte [4];
			try {
				BitConverter.ToString (array, Int32.MaxValue, 1);
				Assert.Fail ("#1");
			} catch (ArgumentOutOfRangeException ex) {
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("startIndex", ex.ParamName, "#6");
			}
		}

		[Test]
		public void ToString_Length_Negative ()
		{
			byte [] array = new byte [4];
			try {
				BitConverter.ToString (array, 1, -1);
				Assert.Fail ("#1");
			} catch (ArgumentOutOfRangeException ex) {
				// Value must be positive
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("length", ex.ParamName, "#6");
			}
		}

		[Test]
		public void ToString_Length_Overflow ()
		{
			byte [] array = new byte [4];
			try {
				BitConverter.ToString (array, 1, Int32.MaxValue);
				Assert.Fail ("#1");
			} catch (ArgumentException ex) {
				// Destination array is not long enough to copy all the items
				// in the collection. Check array index and length
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
			}
		}

		[Test]
		public void ToInt32_UpperLimit ()
		{
			byte [] array = new byte [4];
			try {
				BitConverter.ToInt32 (array, Int32.MaxValue);
				Assert.Fail ("#1");
			} catch (ArgumentOutOfRangeException ex) {
				// Index was out of range. Must be non-negative and less than
				// the size of the collection
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("startIndex", ex.ParamName, "#6");
			}
		}

		[Test]
		public void ToInt32_LowerLimit ()
		{
			byte [] array = new byte [4];
			try {
				BitConverter.ToInt32 (array, Int32.MinValue);
				Assert.Fail ("#1");
			} catch (ArgumentOutOfRangeException ex) {
				// Index was out of range. Must be non-negative and less than
				// the size of the collection
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("startIndex", ex.ParamName, "#6");
			}
		}

		[Test]
		public void ToBoolean ()
		{
			byte [] array = new byte [2] { 0x02, 0x00 };
			Assert.IsTrue (BitConverter.ToBoolean (array, 0), "#1");
			Assert.IsFalse (BitConverter.ToBoolean (array, 1), "#2");
		}
	}
}
