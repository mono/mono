/* 
 * Licensed to the Apache Software Foundation (ASF) under one or more
 * contributor license agreements.  See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The ASF licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance with
 * the License.  You may obtain a copy of the License at
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

namespace Mono.Lucene.Net.Util
{
	
	/// <summary> An average, best guess, MemoryModel that should work okay on most systems.
	/// 
	/// </summary>
	public class AverageGuessMemoryModel:MemoryModel
	{
		public AverageGuessMemoryModel()
		{
			InitBlock();
		}
		internal class AnonymousClassIdentityHashMap : System.Collections.Hashtable /*IdentityHashMap*/  // {{Aroush-2.9.0}} Port issue? Will this do the trick to mimic java's IdentityHashMap?
		{
			public AnonymousClassIdentityHashMap(AverageGuessMemoryModel enclosingInstance)
			{
				InitBlock(enclosingInstance);
			}
			private void  InitBlock(AverageGuessMemoryModel enclosingInstance)
			{
				this.enclosingInstance = enclosingInstance;
				Add(typeof(bool), 1);
				Add(typeof(byte), 1);
                Add(typeof(sbyte), 1);
				Add(typeof(char), 2);
				Add(typeof(short), 2);
				Add(typeof(int), 4);
				Add(typeof(float), 4);
				Add(typeof(double), 8);
				Add(typeof(long), 8);
			}
			private AverageGuessMemoryModel enclosingInstance;
			public AverageGuessMemoryModel Enclosing_Instance
			{
				get
				{
					return enclosingInstance;
				}
				
			}
            // {{Aroush-2.9.0}} Port issue? Will this do the trick to mimic java's IdentityHashMap?
            /*
             * From Java docs:
             * This class implements the Map interface with a hash table, using 
             * reference-equality in place of object-equality when comparing keys 
             * (and values). In other words, in an IdentityHashMap, two keys k1 and k2 
             * are considered equal if and only if (k1==k2). (In normal Map 
             * implementations (like HashMap) two keys k1 and k2 are considered 
             * equal if and only if (k1==null ? k2==null : k1.equals(k2)).) 
             */
            public new bool Equals(Object obj)
            {
                return this.GetHashCode() == obj.GetHashCode();
            }
            public new static bool Equals(Object objA, Object objB)
            {
                return objA.GetHashCode() == objB.GetHashCode();
            }
            // {{Aroush-2.9.0}} Port issue, need to mimic java's IdentityHashMap
		}
		private void  InitBlock()
		{
			sizes = new AnonymousClassIdentityHashMap(this);
		}
		// best guess primitive sizes
		private System.Collections.IDictionary sizes;
		
		/*
		* (non-Javadoc)
		* 
		* @see Mono.Lucene.Net.Util.MemoryModel#getArraySize()
		*/
		public override int GetArraySize()
		{
			return 16;
		}
		
		/*
		* (non-Javadoc)
		* 
		* @see Mono.Lucene.Net.Util.MemoryModel#getClassSize()
		*/
		public override int GetClassSize()
		{
			return 8;
		}
		
		/* (non-Javadoc)
		* @see Mono.Lucene.Net.Util.MemoryModel#getPrimitiveSize(java.lang.Class)
		*/
		public override int GetPrimitiveSize(System.Type clazz)
		{
			return ((System.Int32) sizes[clazz]);
		}
		
		/* (non-Javadoc)
		* @see Mono.Lucene.Net.Util.MemoryModel#getReferenceSize()
		*/
		public override int GetReferenceSize()
		{
			return 4;
		}
	}
}
