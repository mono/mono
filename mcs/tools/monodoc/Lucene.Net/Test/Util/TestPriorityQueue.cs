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
namespace Lucene.Net.Util
{
	[TestFixture]
	public class TestPriorityQueue
	{
		private class IntegerQueue : PriorityQueue
		{
			public IntegerQueue(int count) : base()
			{
				Initialize(count);
			}
			
			public override bool LessThan(System.Object a, System.Object b)
			{
				return ((System.Int32) a) < ((System.Int32) b);
			}
		}
		
		[Test]
        public virtual void  TestPQ()
		{
			_TestPQ(10000);
		}
		
        public static void  _TestPQ(int count)
		{
			PriorityQueue pq = new IntegerQueue(count);
			System.Random gen = new System.Random();
			int sum = 0, sum2 = 0;
			
			System.DateTime start = System.DateTime.Now;
			
			for (int i = 0; i < count; i++)
			{
				int next = gen.Next();
				sum += next;
				pq.Put((System.Object) next);
			}
			
			//      Date end = new Date();
			
			//      System.out.print(((float)(end.getTime()-start.getTime()) / count) * 1000);
			//      System.out.println(" microseconds/put");
			
			//      start = new Date();
			
			int last = System.Int32.MinValue;
			for (int i = 0; i < count; i++)
			{
				System.Int32 next = (System.Int32) pq.Pop();
				Assert.IsTrue(next >= last);
				last = next;
				sum2 += last;
			}
			
			Assert.AreEqual(sum, sum2);
			//      end = new Date();
			
			//      System.out.print(((float)(end.getTime()-start.getTime()) / count) * 1000);
			//      System.out.println(" microseconds/pop");
		}
		
        [Test]
		public virtual void  TestClear()
		{
			PriorityQueue pq = new IntegerQueue(3);
			pq.Put((System.Object) 2);
			pq.Put((System.Object) 3);
			pq.Put((System.Object) 1);
			Assert.AreEqual(3, pq.Size());
			pq.Clear();
			Assert.AreEqual(0, pq.Size());
		}
		
        [Test]
		public virtual void  TestFixedSize()
		{
			PriorityQueue pq = new IntegerQueue(3);
			pq.Insert((System.Object) 2);
			pq.Insert((System.Object) 3);
			pq.Insert((System.Object) 1);
			pq.Insert((System.Object) 5);
			pq.Insert((System.Object) 7);
			pq.Insert((System.Object) 1);
			Assert.AreEqual(3, pq.Size());
			Assert.AreEqual(3, ((System.Int32) pq.Top()));
		}
	}
}