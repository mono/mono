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
	
	/// <summary> A Filter that restricts search results to a range of values in a given
	/// field.
	/// 
	/// <p/>This filter matches the documents looking for terms that fall into the
	/// supplied range according to {@link String#compareTo(String)}. It is not intended
	/// for numerical ranges, use {@link NumericRangeFilter} instead.
	/// 
	/// <p/>If you construct a large number of range filters with different ranges but on the 
	/// same field, {@link FieldCacheRangeFilter} may have significantly better performance. 
	/// </summary>
	/// <since> 2.9
	/// </since>
	[Serializable]
	public class TermRangeFilter:MultiTermQueryWrapperFilter
	{
		
		/// <param name="fieldName">The field this range applies to
		/// </param>
		/// <param name="lowerTerm">The lower bound on this range
		/// </param>
		/// <param name="upperTerm">The upper bound on this range
		/// </param>
		/// <param name="includeLower">Does this range include the lower bound?
		/// </param>
		/// <param name="includeUpper">Does this range include the upper bound?
		/// </param>
		/// <throws>  IllegalArgumentException if both terms are null or if </throws>
		/// <summary>  lowerTerm is null and includeLower is true (similar for upperTerm
		/// and includeUpper)
		/// </summary>
		public TermRangeFilter(System.String fieldName, System.String lowerTerm, System.String upperTerm, bool includeLower, bool includeUpper):base(new TermRangeQuery(fieldName, lowerTerm, upperTerm, includeLower, includeUpper))
		{
		}
		
		/// <summary> <strong>WARNING:</strong> Using this constructor and supplying a non-null
		/// value in the <code>collator</code> parameter will cause every single 
		/// index Term in the Field referenced by lowerTerm and/or upperTerm to be
		/// examined.  Depending on the number of index Terms in this Field, the 
		/// operation could be very slow.
		/// 
		/// </summary>
		/// <param name="lowerTerm">The lower bound on this range
		/// </param>
		/// <param name="upperTerm">The upper bound on this range
		/// </param>
		/// <param name="includeLower">Does this range include the lower bound?
		/// </param>
		/// <param name="includeUpper">Does this range include the upper bound?
		/// </param>
		/// <param name="collator">The collator to use when determining range inclusion; set
		/// to null to use Unicode code point ordering instead of collation.
		/// </param>
		/// <throws>  IllegalArgumentException if both terms are null or if </throws>
		/// <summary>  lowerTerm is null and includeLower is true (similar for upperTerm
		/// and includeUpper)
		/// </summary>
		public TermRangeFilter(System.String fieldName, System.String lowerTerm, System.String upperTerm, bool includeLower, bool includeUpper, System.Globalization.CompareInfo collator):base(new TermRangeQuery(fieldName, lowerTerm, upperTerm, includeLower, includeUpper, collator))
		{
		}
		
		/// <summary> Constructs a filter for field <code>fieldName</code> matching
		/// less than or equal to <code>upperTerm</code>.
		/// </summary>
		public static TermRangeFilter Less(System.String fieldName, System.String upperTerm)
		{
			return new TermRangeFilter(fieldName, null, upperTerm, false, true);
		}
		
		/// <summary> Constructs a filter for field <code>fieldName</code> matching
		/// greater than or equal to <code>lowerTerm</code>.
		/// </summary>
		public static TermRangeFilter More(System.String fieldName, System.String lowerTerm)
		{
			return new TermRangeFilter(fieldName, lowerTerm, null, true, false);
		}
		
		/// <summary>Returns the field name for this filter </summary>
		public virtual System.String GetField()
		{
			return ((TermRangeQuery) query).GetField();
		}
		
		/// <summary>Returns the lower value of this range filter </summary>
		public virtual System.String GetLowerTerm()
		{
			return ((TermRangeQuery) query).GetLowerTerm();
		}
		
		/// <summary>Returns the upper value of this range filter </summary>
		public virtual System.String GetUpperTerm()
		{
			return ((TermRangeQuery) query).GetUpperTerm();
		}
		
		/// <summary>Returns <code>true</code> if the lower endpoint is inclusive </summary>
		public virtual bool IncludesLower()
		{
			return ((TermRangeQuery) query).IncludesLower();
		}
		
		/// <summary>Returns <code>true</code> if the upper endpoint is inclusive </summary>
		public virtual bool IncludesUpper()
		{
			return ((TermRangeQuery) query).IncludesUpper();
		}
		
		/// <summary>Returns the collator used to determine range inclusion, if any. </summary>
		public virtual System.Globalization.CompareInfo GetCollator()
		{
			return ((TermRangeQuery) query).GetCollator();
		}
	}
}
