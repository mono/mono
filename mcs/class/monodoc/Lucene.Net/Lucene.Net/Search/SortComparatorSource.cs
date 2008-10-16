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
using Monodoc.Lucene.Net.Index;
namespace Monodoc.Lucene.Net.Search
{
	
	/// <summary> Expert: returns a comparator for sorting ScoreDocs.
	/// 
	/// <p>Created: Apr 21, 2004 3:49:28 PM
	/// 
	/// </summary>
	/// <author>   Tim Jones
	/// </author>
	/// <version>  $Id: SortComparatorSource.java,v 1.2 2004/05/19 23:05:27 tjones Exp $
	/// </version>
	/// <since>   1.4
	/// </since>
	public interface SortComparatorSource
	{
		
		/// <summary> Creates a comparator for the Field in the given index.</summary>
		/// <param name="reader">Index to create comparator for.
		/// </param>
		/// <param name="fieldname"> Field to create comparator for.
		/// </param>
		/// <returns> Comparator of ScoreDoc objects.
		/// </returns>
		/// <throws>  IOException If an error occurs reading the index. </throws>
		ScoreDocComparator NewComparator(Monodoc.Lucene.Net.Index.IndexReader reader, System.String fieldname);
	}
}