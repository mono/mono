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

namespace Mono.Lucene.Net.Search
{
	
	/// <summary> A DocIdSet contains a set of doc ids. Implementing classes must
	/// only implement {@link #iterator} to provide access to the set. 
	/// </summary>
	[Serializable]
	public abstract class DocIdSet
	{
		public class AnonymousClassDocIdSet:DocIdSet
		{
			public AnonymousClassDocIdSet()
			{
				InitBlock();
			}
			public class AnonymousClassDocIdSetIterator:DocIdSetIterator
			{
				public AnonymousClassDocIdSetIterator(AnonymousClassDocIdSet enclosingInstance)
				{
					InitBlock(enclosingInstance);
				}
				private void  InitBlock(AnonymousClassDocIdSet enclosingInstance)
				{
					this.enclosingInstance = enclosingInstance;
				}
				private AnonymousClassDocIdSet enclosingInstance;
				public AnonymousClassDocIdSet Enclosing_Instance
				{
					get
					{
						return enclosingInstance;
					}
					
				}
				public override int Advance(int target)
				{
					return NO_MORE_DOCS;
				}
				public override int DocID()
				{
					return NO_MORE_DOCS;
				}
				public override int NextDoc()
				{
					return NO_MORE_DOCS;
				}
			}
			private void  InitBlock()
			{
				iterator = new AnonymousClassDocIdSetIterator(this);
			}
			
			private DocIdSetIterator iterator;
			
			public override DocIdSetIterator Iterator()
			{
				return iterator;
			}

			public override bool IsCacheable()
			{
				return true;
			}
		}
		
		/// <summary>An empty {@code DocIdSet} instance for easy use, e.g. in Filters that hit no documents. </summary>
		[NonSerialized]
		public static readonly DocIdSet EMPTY_DOCIDSET;
		
		/// <summary>Provides a {@link DocIdSetIterator} to access the set.
		/// This implementation can return <code>null</code> or
		/// <code>{@linkplain #EMPTY_DOCIDSET}.iterator()</code> if there
		/// are no docs that match. 
		/// </summary>
		public abstract DocIdSetIterator Iterator();

		/// <summary>This method is a hint for {@link CachingWrapperFilter}, if this <code>DocIdSet</code>
		/// should be cached without copying it into a BitSet. The default is to return
		/// <code>false</code>. If you have an own <code>DocIdSet</code> implementation
		/// that does its iteration very effective and fast without doing disk I/O,
		/// override this method and return true.
		/// </summary>
		public virtual bool IsCacheable()
		{
			return false;
		}
		static DocIdSet()
		{
			EMPTY_DOCIDSET = new AnonymousClassDocIdSet();
		}
	}
}
