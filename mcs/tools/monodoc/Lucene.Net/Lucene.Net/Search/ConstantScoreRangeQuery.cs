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
	
	/// <summary> A range query that returns a constant score equal to its boost for
	/// all documents in the exclusive range of terms.
	/// 
	/// <p/>It does not have an upper bound on the number of clauses covered in the range.
	/// 
	/// <p/>This query matches the documents looking for terms that fall into the
	/// supplied range according to {@link String#compareTo(String)}. It is not intended
	/// for numerical ranges, use {@link NumericRangeQuery} instead.
	/// 
	/// <p/>This query is hardwired to {@link MultiTermQuery#CONSTANT_SCORE_AUTO_REWRITE_DEFAULT}.
	/// If you want to change this, use {@link TermRangeQuery} instead.
	/// 
	/// </summary>
	/// <deprecated> Use {@link TermRangeQuery} for term ranges or
	/// {@link NumericRangeQuery} for numeric ranges instead.
	/// This class will be removed in Lucene 3.0.
	/// </deprecated>
	/// <version>  $Id: ConstantScoreRangeQuery.java 797694 2009-07-25 00:03:33Z mikemccand $
	/// </version>
    [Obsolete("Use TermRangeQuery for term ranges or NumericRangeQuery for numeric ranges instead. This class will be removed in Lucene 3.0.")]
	[Serializable]
	public class ConstantScoreRangeQuery:TermRangeQuery
	{
		
		public ConstantScoreRangeQuery(System.String fieldName, System.String lowerVal, System.String upperVal, bool includeLower, bool includeUpper):base(fieldName, lowerVal, upperVal, includeLower, includeUpper)
		{
			rewriteMethod = CONSTANT_SCORE_AUTO_REWRITE_DEFAULT;
		}
		
		public ConstantScoreRangeQuery(System.String fieldName, System.String lowerVal, System.String upperVal, bool includeLower, bool includeUpper, System.Globalization.CompareInfo collator):base(fieldName, lowerVal, upperVal, includeLower, includeUpper, collator)
		{
			rewriteMethod = CONSTANT_SCORE_AUTO_REWRITE_DEFAULT;
		}
		
		public virtual System.String GetLowerVal()
		{
			return GetLowerTerm();
		}
		
		public virtual System.String GetUpperVal()
		{
			return GetUpperTerm();
		}
		
		/// <summary>Changes of mode are not supported by this class (fixed to constant score rewrite mode) </summary>
		public override void  SetRewriteMethod(RewriteMethod method)
		{
			throw new System.NotSupportedException("Use TermRangeQuery instead to change the rewrite method.");
		}
	}
}
