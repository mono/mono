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
	
	/// <summary> Provides a {@link FieldComparator} for custom field sorting.
	/// 
	/// <b>NOTE:</b> This API is experimental and might change in
	/// incompatible ways in the next release.
	/// 
	/// </summary>
	[Serializable]
	public abstract class FieldComparatorSource
	{
		
		/// <summary> Creates a comparator for the field in the given index.
		/// 
		/// </summary>
		/// <param name="fieldname">Name of the field to create comparator for.
		/// </param>
		/// <returns> FieldComparator.
		/// </returns>
		/// <throws>  IOException </throws>
		/// <summary>           If an error occurs reading the index.
		/// </summary>
		public abstract FieldComparator NewComparator(System.String fieldname, int numHits, int sortPos, bool reversed);
	}
}
