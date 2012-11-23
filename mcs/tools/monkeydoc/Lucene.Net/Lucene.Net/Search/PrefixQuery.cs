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
	
	/// <summary>A Query that matches documents containing terms with a specified prefix. A PrefixQuery
	/// is built by QueryParser for input like <code>app*</code>.
	/// 
	/// <p/>This query uses the {@link
	/// MultiTermQuery#CONSTANT_SCORE_AUTO_REWRITE_DEFAULT}
	/// rewrite method. 
	/// </summary>
	[Serializable]
	public class PrefixQuery:MultiTermQuery
	{
		private Term prefix;
		
		/// <summary>Constructs a query for terms starting with <code>prefix</code>. </summary>
		public PrefixQuery(Term prefix):base(prefix)
		{ //will be removed in 3.0
			this.prefix = prefix;
		}
		
		/// <summary>Returns the prefix of this query. </summary>
		public virtual Term GetPrefix()
		{
			return prefix;
		}
		
		public /*protected internal*/ override FilteredTermEnum GetEnum(IndexReader reader)
		{
			return new PrefixTermEnum(reader, prefix);
		}
		
		/// <summary>Prints a user-readable version of this query. </summary>
		public override System.String ToString(System.String field)
		{
			System.Text.StringBuilder buffer = new System.Text.StringBuilder();
			if (!prefix.Field().Equals(field))
			{
				buffer.Append(prefix.Field());
				buffer.Append(":");
			}
			buffer.Append(prefix.Text());
			buffer.Append('*');
			buffer.Append(ToStringUtils.Boost(GetBoost()));
			return buffer.ToString();
		}
		
		//@Override
		public override int GetHashCode()
		{
			int prime = 31;
			int result = base.GetHashCode();
			result = prime * result + ((prefix == null)?0:prefix.GetHashCode());
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
			PrefixQuery other = (PrefixQuery) obj;
			if (prefix == null)
			{
				if (other.prefix != null)
					return false;
			}
			else if (!prefix.Equals(other.prefix))
				return false;
			return true;
		}
	}
}
