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

using IndexReader = Mono.Lucene.Net.Index.IndexReader;
using Term = Mono.Lucene.Net.Index.Term;
using ToStringUtils = Mono.Lucene.Net.Util.ToStringUtils;

namespace Mono.Lucene.Net.Search
{
	
	/// <summary>Implements the wildcard search query. Supported wildcards are <code>*</code>, which
	/// matches any character sequence (including the empty one), and <code>?</code>,
	/// which matches any single character. Note this query can be slow, as it
	/// needs to iterate over many terms. In order to prevent extremely slow WildcardQueries,
	/// a Wildcard term should not start with one of the wildcards <code>*</code> or
	/// <code>?</code>.
	/// 
	/// <p/>This query uses the {@link
	/// MultiTermQuery#CONSTANT_SCORE_AUTO_REWRITE_DEFAULT}
	/// rewrite method.
	/// 
	/// </summary>
	/// <seealso cref="WildcardTermEnum">
	/// </seealso>
	[Serializable]
	public class WildcardQuery:MultiTermQuery
	{
		private bool termContainsWildcard;
		new protected internal Term term;
		
		public WildcardQuery(Term term):base(term)
		{ //will be removed in 3.0
			this.term = term;
			this.termContainsWildcard = (term.Text().IndexOf('*') != - 1) || (term.Text().IndexOf('?') != - 1);
		}
		
		public /*protected internal*/ override FilteredTermEnum GetEnum(IndexReader reader)
		{
			return new WildcardTermEnum(reader, GetTerm());
		}
		
		/// <summary> Returns the pattern term.</summary>
        [Obsolete("Mono.Lucene.Net-2.9.1. This method overrides obsolete member Mono.Lucene.Net.Search.MultiTermQuery.GetTerm()")]
		public override Term GetTerm()
		{
			return term;
		}
		
		public override Query Rewrite(IndexReader reader)
		{
			if (!termContainsWildcard)
				return new TermQuery(GetTerm());
			else
				return base.Rewrite(reader);
		}
		
		/// <summary>Prints a user-readable version of this query. </summary>
		public override System.String ToString(System.String field)
		{
			System.Text.StringBuilder buffer = new System.Text.StringBuilder();
			if (!term.Field().Equals(field))
			{
				buffer.Append(term.Field());
				buffer.Append(":");
			}
			buffer.Append(term.Text());
			buffer.Append(ToStringUtils.Boost(GetBoost()));
			return buffer.ToString();
		}
		
		//@Override
		public override int GetHashCode()
		{
			int prime = 31;
			int result = base.GetHashCode();
			result = prime * result + ((term == null)?0:term.GetHashCode());
			return result;
		}
		
		//@Override
		public  override bool Equals(System.Object obj)
		{
			if (this == obj)
				return true;
			if (!base.Equals(obj))
				return false;
			if (GetType() != obj.GetType())
				return false;
			WildcardQuery other = (WildcardQuery) obj;
			if (term == null)
			{
				if (other.term != null)
					return false;
			}
			else if (!term.Equals(other.term))
				return false;
			return true;
		}
	}
}
