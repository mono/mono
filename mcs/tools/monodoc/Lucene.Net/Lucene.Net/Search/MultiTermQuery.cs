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
using Term = Monodoc.Lucene.Net.Index.Term;
namespace Monodoc.Lucene.Net.Search
{
	
	/// <summary> A {@link Query} that matches documents containing a subset of terms provided
	/// by a {@link FilteredTermEnum} enumeration.
	/// <P>
	/// <code>MultiTermQuery</code> is not designed to be used by itself.
	/// <BR>
	/// The reason being that it is not intialized with a {@link FilteredTermEnum}
	/// enumeration. A {@link FilteredTermEnum} enumeration needs to be provided.
	/// <P>
	/// For example, {@link WildcardQuery} and {@link FuzzyQuery} extend
	/// <code>MultiTermQuery</code> to provide {@link WildcardTermEnum} and
	/// {@link FuzzyTermEnum}, respectively.
	/// </summary>
	[Serializable]
	public abstract class MultiTermQuery:Query
	{
		private Term term;
		
		/// <summary>Constructs a query for terms matching <code>term</code>. </summary>
		public MultiTermQuery(Term term)
		{
			this.term = term;
		}
		
		/// <summary>Returns the pattern term. </summary>
		public virtual Term GetTerm()
		{
			return term;
		}
		
		/// <summary>Construct the enumeration to be used, expanding the pattern term. </summary>
		protected internal abstract FilteredTermEnum GetEnum(Monodoc.Lucene.Net.Index.IndexReader reader);
		
		public override Query Rewrite(Monodoc.Lucene.Net.Index.IndexReader reader)
		{
			FilteredTermEnum enumerator = GetEnum(reader);
			BooleanQuery query = new BooleanQuery();
			try
			{
				do 
				{
					Term t = enumerator.Term();
					if (t != null)
					{
						TermQuery tq = new TermQuery(t); // found a match
						tq.SetBoost(GetBoost() * enumerator.Difference()); // set the boost
						query.Add(tq, false, false); // add to query
					}
				}
				while (enumerator.Next());
			}
			finally
			{
				enumerator.Close();
			}
			return query;
		}
		
		public override Query Combine(Query[] queries)
		{
			return Query.MergeBooleanQueries(queries);
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
			if (GetBoost() != 1.0f)
			{
                System.Globalization.NumberFormatInfo nfi = new System.Globalization.CultureInfo("en-US", false).NumberFormat;
                nfi.NumberDecimalDigits = 1;

				buffer.Append("^");
				buffer.Append(GetBoost().ToString("N", nfi));
			}
			return buffer.ToString();
		}
	}
}