// ArraySerializationTest.cs
//
// Author:
//   Lluis Sanchez Gual (lluis@ideary.com)
//
// (C) 2005 Lluis Sanchez Gual

//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Reflection;
using System.Collections;
using NUnit.Framework;

namespace MonoTests.System.Runtime.Serialization
{
	[TestFixture]
	public class ArraySerializationTest
	{
		[Test]
		public void TestBoolArray ()
		{
			bool[] array = new bool[2000];
			for (int n=0; n<2000; n++)
				array [n] = (n % 3) == 0;
			CheckArray (array);
		}
		
		[Test]
		public void TestByteArray ()
		{
			byte[] array = new byte[10000];
			for (int n=0; n<10000; n++)
				array [n] = (byte) (n % 253);
			CheckArray (array);
		}
		
		[Test]
		public void TestCharArray ()
		{
			char[] array = new char[2000];
			for (int n=0; n<2000; n++)
				array [n] = (char) n;
			CheckArray (array);
		}
		
		[Test]
		public void TestDateTimeArray ()
		{
			DateTime[] array = new DateTime[10000];
			for (int n=0; n<10000; n++)
				array [n] = new DateTime (n);
			CheckArray (array);
		}
		
		[Test]
		public void TestDecimalArray ()
		{
			decimal[] array = new decimal[2000];
			for (int n=0; n<2000; n++)
				array [n] = decimal.MaxValue / (decimal) (n+1);
			CheckArray (array);
		}
		
		[Test]
		public void TestDoubleArray ()
		{
			double[] array = new double[10000];
			for (int n=0; n<10000; n++)
				array [n] = (double) n;
			CheckArray (array);
		}
		
		[Test]
		public void TestShortArray ()
		{
			short[] array = new short[10000];
			for (int n=0; n<10000; n++)
				array [n] = (short) n;
			CheckArray (array);
		}
		
		[Test]
		public void TestIntArray ()
		{
			int[] array = new int[10000];
			for (int n=0; n<10000; n++)
				array [n] = n;
			CheckArray (array);
		}
		
		[Test]
		public void TestLongArray ()
		{
			long[] array = new long[10000];
			for (int n=0; n<10000; n++)
				array [n] = n;
			CheckArray (array);
		}
		
		[Test]
		public void TestSByteArray ()
		{
			sbyte[] array = new sbyte[10000];
			for (int n=0; n<10000; n++)
				array [n] = (sbyte) (n % 121);
			CheckArray (array);
		}
		
		[Test]
		public void TestFloatArray ()
		{
			float[] array = new float[10000];
			for (int n=0; n<10000; n++)
				array [n] = (float) n;
			CheckArray (array);
		}
		
		[Test]
		public void TestUShortArray ()
		{
			ushort[] array = new ushort[10000];
			for (int n=0; n<10000; n++)
				array [n] = (ushort) n;
			CheckArray (array);
		}
		
		[Test]
		public void TestUIntArray ()
		{
			uint[] array = new uint[10000];
			for (int n=0; n<10000; n++)
				array [n] = (uint) n;
			CheckArray (array);
		}
		
		[Test]
		public void TestULongArray ()
		{
			ulong[] array = new ulong[10000];
			for (int n=0; n<10000; n++)
				array [n] = (ulong) n;
			CheckArray (array);
		}
		
		[Test]
		public void TestStringArray ()
		{
			string[] array = new string[10000];
			for (int n=0; n<10000; n++)
				array [n] = n.ToString ();
			CheckArray (array);
		}
		
		void CheckArray (Array src)
		{
			MemoryStream ms = new MemoryStream ();
			BinaryFormatter bf = new BinaryFormatter ();
			bf.Serialize (ms, src);
			ms.Position = 0;
			Array des = (Array) bf.Deserialize (ms);
			CompareArrays (src.GetType().ToString(), src, des);
		}
		
		void CompareArrays (string txt, Array a1, Array a2)
		{
			Assert.AreEqual (a1.Length, a2.Length, txt + " length");
			for (int n=0; n<a1.Length; n++)
				Assert.AreEqual (a1.GetValue (n), a2.GetValue (n), txt + " [" + n + "]");
		}
	}
}
