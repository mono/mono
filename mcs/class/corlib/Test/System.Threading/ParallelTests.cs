#if NET_4_0
// ParallelTests.cs
//
// Copyright (c) 2008 Jérémie "Garuma" Laval
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//
//

using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.IO;
using System.Xml.Serialization;
using System.Collections.Generic;

using ParallelFxTests.RayTracer;

using NUnit;
using NUnit.Core;
using NUnit.Framework;

namespace ParallelFxTests
{
	
	[TestFixture()]
	public class ParallelTests
	{
		int[] pixels;
		RayTracerApp rayTracer;
		
		public void Setup()
		{
			Stream stream = Assembly.GetAssembly(typeof(ParallelTests)).GetManifestResourceStream("raytracer-output.xml");
			Console.WriteLine(stream == null);
			XmlSerializer serializer = new XmlSerializer(typeof(int[]));
			pixels = (int[])serializer.Deserialize(stream);
			rayTracer = new RayTracerApp();
		}
		
		[Test]
		public void ParallelForTestCase()
		{
			Setup();
			// Test the the output of the Parallel RayTracer is the same than the synchronous ones 
			CollectionAssert.AreEquivalent(pixels, rayTracer.Pixels, "#1, same pixels");
			CollectionAssert.AreEqual(pixels, rayTracer.Pixels, "#2, pixels in order");
		}

		[Test, ExpectedException(typeof(AggregateException))]
		public void ParallelForExceptionTestCase()
		{
			Parallel.For(1, 10, delegate (int i) { throw new Exception("foo"); });
		}
		
		[Test]
		public void ParallelForEachTestCase()
		{
			IEnumerable<int> e = Enumerable.Repeat(1, 10);
			int count = 0;
			
			Parallel.ForEach(e, (element) => Interlocked.Increment(ref count));
			
			Assert.AreEqual(10, count);
		}
		
		[Test, ExpectedException(typeof(AggregateException))]
		public void ParallelForEachExceptionTestCse()
		{
			IEnumerable<int> e = Enumerable.Repeat(1, 10);
			Parallel.ForEach(e, delegate (int element) { throw new Exception("foo"); });
		}
		
		[Test]
		public void ParallelWhileTestCase()
		{
			int i = 0;
			int count = 0;
			
			Parallel.While(() => Interlocked.Increment(ref i) <= 10, () => Interlocked.Increment(ref count));
			
			Assert.Greater(i, 10, "#1");
			Assert.AreEqual(10, count, "#2");
		}
	}
}
#endif
