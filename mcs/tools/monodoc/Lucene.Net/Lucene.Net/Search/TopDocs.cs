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
namespace Monodoc.Lucene.Net.Search
{
	/// <summary>Expert: Returned by low-level search implementations.</summary>
	/// <seealso cref="Searcher#Search(Query,Filter,int)">
	/// </seealso>
	[Serializable]
	public class TopDocs
	{
		/// <summary>Expert: The total number of hits for the query.</summary>
		/// <seealso cref="Hits#Length()">
		/// </seealso>
		public int totalHits;
		/// <summary>Expert: The top hits for the query. </summary>
		public ScoreDoc[] scoreDocs;
		
		/// <summary>Expert: Constructs a TopDocs.</summary>
		internal TopDocs(int totalHits, ScoreDoc[] scoreDocs)
		{
			this.totalHits = totalHits;
			this.scoreDocs = scoreDocs;
		}
	}
}