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
	/// <summary>A clause in a BooleanQuery. </summary>
	[Serializable]
	public class BooleanClause
	{
		/// <summary>The query whose matching documents are combined by the boolean query. </summary>
		public Query query;
		/// <summary>If true, documents documents which <i>do not</i>
		/// match this sub-query will <i>not</i> match the boolean query. 
		/// </summary>
		public bool required = false;
		/// <summary>If true, documents documents which <i>do</i>
		/// match this sub-query will <i>not</i> match the boolean query. 
		/// </summary>
		public bool prohibited = false;
		
		/// <summary>Constructs a BooleanClause with query <code>q</code>, required
		/// <code>r</code> and prohibited <code>p</code>. 
		/// </summary>
		public BooleanClause(Query q, bool r, bool p)
		{
			query = q;
			required = r;
			prohibited = p;
		}
		
		/// <summary>Returns true iff <code>o</code> is equal to this. </summary>
		public  override bool Equals(System.Object o)
		{
			if (!(o is BooleanClause))
				return false;
			BooleanClause other = (BooleanClause) o;
			return this.query.Equals(other.query) && (this.required == other.required) && (this.prohibited == other.prohibited);
		}
		
		/// <summary>Returns a hash code value for this object.</summary>
		public override int GetHashCode()
		{
			return query.GetHashCode() ^ (this.required?1:0) ^ (this.prohibited?2:0);
		}
	}
}