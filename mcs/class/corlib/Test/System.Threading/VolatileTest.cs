//
// VolatileTest.cs
//
// Authors:
//       Marek Safar (marek.safar@gmail.com)
//
// Copyright 2011 Xamarin, Inc (http://www.xamarin.com)
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

#if NET_4_5

using System;
using System.Threading;
using NUnit.Framework;

namespace MonoTests.System.Threading
{
	[TestFixture]
	public class VolatileTest
	{
		[Test]
		public void ReadPrimitives ()
		{
			bool v1 = true;
			Assert.AreEqual (true, Volatile.Read (ref v1), "#v1");

			byte v2 = 4;
			Assert.AreEqual (4, Volatile.Read (ref v2), "#v2");

			double v3 = double.MaxValue;
			Assert.AreEqual (double.MaxValue, Volatile.Read (ref v3), "#v3");

			float v4 = float.Epsilon;
			Assert.AreEqual (float.Epsilon, Volatile.Read (ref v4), "#v4");

			int v5 = int.MinValue;
			Assert.AreEqual (int.MinValue, Volatile.Read (ref v5), "#v5");

			IntPtr v6 = IntPtr.Zero;
			Assert.AreEqual (IntPtr.Zero, Volatile.Read (ref v6), "#v6");

			long v7 = long.MaxValue;
			Assert.AreEqual (long.MaxValue, Volatile.Read (ref v7), "#v7");

			sbyte v8 = 44;
			Assert.AreEqual (44, Volatile.Read (ref v8), "#v8");

			short v9 = -999;
			Assert.AreEqual (-999, Volatile.Read (ref v9), "#v9");

			uint v10 = uint.MaxValue;
			Assert.AreEqual (uint.MaxValue, Volatile.Read (ref v10), "#v10");

			UIntPtr v11 = (UIntPtr) uint.MaxValue;
			Assert.AreEqual (new UIntPtr (uint.MaxValue), Volatile.Read (ref v11), "#v11");

			ulong v12 = ulong.MaxValue;
			Assert.AreEqual (ulong.MaxValue, Volatile.Read (ref v12), "#v12");

			ushort v13 = ushort.MaxValue;
			Assert.AreEqual (ushort.MaxValue, Volatile.Read (ref v13), "#v13");
		}

		[Test]
		public void WritePrimitives ()
		{
			bool v1 = false;
			Volatile.Write (ref v1, true);
			Assert.AreEqual (true, v1, "#v1");

			byte v2 = 2;
			Volatile.Write (ref v2, 4);
			Assert.AreEqual (4, v2, "#v2");

			double v3 = 55667;
			Volatile.Write (ref v3, double.MaxValue);
			Assert.AreEqual (double.MaxValue, v3, "#v3");

			float v4 = 1;
			Volatile.Write (ref v4, float.MaxValue);
			Assert.AreEqual (float.MaxValue, v4, "#v4");

			int v5 = 0;
			Volatile.Write (ref v5, int.MinValue);
			Assert.AreEqual (int.MinValue, v5, "#v5");

			IntPtr v6 = IntPtr.Zero;
			Volatile.Write (ref v6, new IntPtr (5));
			Assert.AreEqual (new IntPtr (5), v6, "#v6");

			long v7 = 0;
			Volatile.Write (ref v7, long.MaxValue);
			Assert.AreEqual (long.MaxValue, v7, "#v7");

			sbyte v8 = 2;
			Volatile.Write (ref v8, 44);
			Assert.AreEqual (44, v8, "#v8");

			short v9 = 3;
			Volatile.Write (ref v9, -999);
			Assert.AreEqual (-999, v9, "#v9");

			uint v10 = 1;
			Volatile.Write (ref v10, uint.MaxValue);
			Assert.AreEqual (uint.MaxValue, v10, "#v10");

			UIntPtr v11 = UIntPtr.Zero;
			Volatile.Write (ref v11, (UIntPtr) uint.MaxValue);
			Assert.AreEqual (new UIntPtr (uint.MaxValue), v11, "#v11");

			ulong v12 = 0;
			Volatile.Write (ref v12, ulong.MaxValue);
			Assert.AreEqual (ulong.MaxValue, v12, "#v12");

			ushort v13 = 1;
			Volatile.Write (ref v13, ushort.MaxValue);
			Assert.AreEqual (ushort.MaxValue, v13, "#v13");
		}

	}
}

#endif

