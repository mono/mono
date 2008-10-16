/*
 * Copyright 2004 The Apache Software Foundation
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 * http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
using System;
using NUnit.Framework;
using Directory = Lucene.Net.Store.Directory;
using RAMDirectory = Lucene.Net.Store.RAMDirectory;
namespace Lucene.Net.Util
{
	
	/// <summary> <code>TestBitVector</code> tests the <code>BitVector</code>, obviously.
	/// 
	/// </summary>
	/// <author>  "Peter Mularien" <pmularien@deploy.com>
	/// </author>
	/// <version>  $Id: TestBitVector.java,v 1.3 2004/03/29 22:48:07 cutting Exp $
	/// </version>
	[TestFixture]
    public class TestBitVector
	{
		/// <summary> Test the default constructor on BitVectors of various sizes.</summary>
		/// <throws>  Exception </throws>
		[Test]
        public virtual void  TestConstructSize()
		{
			DoTestConstructOfSize(8);
			DoTestConstructOfSize(20);
			DoTestConstructOfSize(100);
			DoTestConstructOfSize(1000);
		}
		
		private void  DoTestConstructOfSize(int n)
		{
			BitVector bv = new BitVector(n);
			Assert.AreEqual(n, bv.Size());
		}
		
		/// <summary> Test the get() and set() methods on BitVectors of various sizes.</summary>
		/// <throws>  Exception </throws>
		[Test]
        public virtual void  TestGetSet()
		{
			DoTestGetSetVectorOfSize(8);
			DoTestGetSetVectorOfSize(20);
			DoTestGetSetVectorOfSize(100);
			DoTestGetSetVectorOfSize(1000);
		}
		
		private void  DoTestGetSetVectorOfSize(int n)
		{
			BitVector bv = new BitVector(n);
			for (int i = 0; i < bv.Size(); i++)
			{
				// ensure a set bit can be git'
				Assert.IsFalse(bv.Get(i));
				bv.Set(i);
				Assert.IsTrue(bv.Get(i));
			}
		}
		
		/// <summary> Test the clear() method on BitVectors of various sizes.</summary>
		/// <throws>  Exception </throws>
		[Test]
        public virtual void  TestClear()
		{
			DoTestClearVectorOfSize(8);
			DoTestClearVectorOfSize(20);
			DoTestClearVectorOfSize(100);
			DoTestClearVectorOfSize(1000);
		}
		
		private void  DoTestClearVectorOfSize(int n)
		{
			BitVector bv = new BitVector(n);
			for (int i = 0; i < bv.Size(); i++)
			{
				// ensure a set bit is cleared
				Assert.IsFalse(bv.Get(i));
				bv.Set(i);
				Assert.IsTrue(bv.Get(i));
				bv.Clear(i);
				Assert.IsFalse(bv.Get(i));
			}
		}
		
		/// <summary> Test the count() method on BitVectors of various sizes.</summary>
		/// <throws>  Exception </throws>
		[Test]
        public virtual void  TestCount()
		{
			DoTestCountVectorOfSize(8);
			DoTestCountVectorOfSize(20);
			DoTestCountVectorOfSize(100);
			DoTestCountVectorOfSize(1000);
		}
		
		private void  DoTestCountVectorOfSize(int n)
		{
			BitVector bv = new BitVector(n);
			// test count when incrementally setting bits
			for (int i = 0; i < bv.Size(); i++)
			{
				Assert.IsFalse(bv.Get(i));
				Assert.AreEqual(i, bv.Count());
				bv.Set(i);
				Assert.IsTrue(bv.Get(i));
				Assert.AreEqual(i + 1, bv.Count());
			}
			
			bv = new BitVector(n);
			// test count when setting then clearing bits
			for (int i = 0; i < bv.Size(); i++)
			{
				Assert.IsFalse(bv.Get(i));
				Assert.AreEqual(0, bv.Count());
				bv.Set(i);
				Assert.IsTrue(bv.Get(i));
				Assert.AreEqual(1, bv.Count());
				bv.Clear(i);
				Assert.IsFalse(bv.Get(i));
				Assert.AreEqual(0, bv.Count());
			}
		}
		
		/// <summary> Test writing and construction to/from Directory.</summary>
		/// <throws>  Exception </throws>
		[Test]
        public virtual void  TestWriteRead()
		{
			DoTestWriteRead(8);
			DoTestWriteRead(20);
			DoTestWriteRead(100);
			DoTestWriteRead(1000);
		}
		
		private void  DoTestWriteRead(int n)
		{
			Directory d = new RAMDirectory();
			
			BitVector bv = new BitVector(n);
			// test count when incrementally setting bits
			for (int i = 0; i < bv.Size(); i++)
			{
				Assert.IsFalse(bv.Get(i));
				Assert.AreEqual(i, bv.Count());
				bv.Set(i);
				Assert.IsTrue(bv.Get(i));
				Assert.AreEqual(i + 1, bv.Count());
				bv.Write(d, "TESTBV");
				BitVector compare = new BitVector(d, "TESTBV");
				// compare bit vectors with bits set incrementally
				Assert.IsTrue(DoCompare(bv, compare));
			}
		}
		
		/// <summary> Compare two BitVectors.
		/// This should really be an equals method on the BitVector itself.
		/// </summary>
		/// <param name="bv">One bit vector
		/// </param>
		/// <param name="compare">The second to compare
		/// </param>
		private bool DoCompare(BitVector bv, BitVector compare)
		{
			bool equal = true;
			for (int i = 0; i < bv.Size(); i++)
			{
				// bits must be equal
				if (bv.Get(i) != compare.Get(i))
				{
					equal = false;
					break;
				}
			}
			return equal;
		}
	}
}