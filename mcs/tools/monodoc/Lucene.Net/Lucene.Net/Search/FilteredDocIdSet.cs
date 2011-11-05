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
	
	/// <summary> Abstract decorator class for a DocIdSet implementation
	/// that provides on-demand filtering/validation
	/// mechanism on a given DocIdSet.
	/// 
	/// <p/>
	/// 
	/// Technically, this same functionality could be achieved
	/// with ChainedFilter (under contrib/misc), however the
	/// benefit of this class is it never materializes the full
	/// bitset for the filter.  Instead, the {@link #match}
	/// method is invoked on-demand, per docID visited during
	/// searching.  If you know few docIDs will be visited, and
	/// the logic behind {@link #match} is relatively costly,
	/// this may be a better way to filter than ChainedFilter.
	/// 
	/// </summary>
	/// <seealso cref="DocIdSet">
	/// </seealso>
	
	public abstract class FilteredDocIdSet:DocIdSet
	{
		private class AnonymousClassFilteredDocIdSetIterator:FilteredDocIdSetIterator
		{
			public AnonymousClassFilteredDocIdSetIterator(FilteredDocIdSet enclosingInstance) : base(null)
			{
                System.Diagnostics.Debug.Fail("Port issue:", "Lets see if we need this"); // {{Aroush-2.9}}
				InitBlock(enclosingInstance);
			}
			private void InitBlock(FilteredDocIdSet enclosingInstance)
			{
				this.enclosingInstance = enclosingInstance;
			}
			private FilteredDocIdSet enclosingInstance;
			public FilteredDocIdSet Enclosing_Instance
			{
				get
				{
					return enclosingInstance;
				}
				
			}
			internal AnonymousClassFilteredDocIdSetIterator(FilteredDocIdSet enclosingInstance, Mono.Lucene.Net.Search.DocIdSetIterator Param1):base(Param1)
			{
				InitBlock(enclosingInstance);
			}
			public /*protected internal*/ override bool Match(int docid)
			{
				return Enclosing_Instance.Match(docid);
			}
		}
		private DocIdSet _innerSet;
		
		/// <summary> Constructor.</summary>
		/// <param name="innerSet">Underlying DocIdSet
		/// </param>
		public FilteredDocIdSet(DocIdSet innerSet)
		{
			_innerSet = innerSet;
		}
		
		/// <summary>This DocIdSet implementation is cacheable if the inner set is cacheable. </summary>
		public override bool IsCacheable()
		{
			return _innerSet.IsCacheable();
		}
		
		/// <summary> Validation method to determine whether a docid should be in the result set.</summary>
		/// <param name="docid">docid to be tested
		/// </param>
		/// <returns> true if input docid should be in the result set, false otherwise.
		/// </returns>
		public /*protected internal*/ abstract bool Match(int docid);
		
		/// <summary> Implementation of the contract to build a DocIdSetIterator.</summary>
		/// <seealso cref="DocIdSetIterator">
		/// </seealso>
		/// <seealso cref="FilteredDocIdSetIterator">
		/// </seealso>
		// @Override
		public override DocIdSetIterator Iterator()
		{
			return new AnonymousClassFilteredDocIdSetIterator(this, _innerSet.Iterator());
		}
	}
}
