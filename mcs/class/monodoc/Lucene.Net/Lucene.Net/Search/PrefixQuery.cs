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
using TermEnum = Monodoc.Lucene.Net.Index.TermEnum;
namespace Monodoc.Lucene.Net.Search
{
	
	/// <summary>A Query that matches documents containing terms with a specified prefix. </summary>
	[Serializable]
	public class PrefixQuery:Query
	{
		private Term prefix;
		
		/// <summary>Constructs a query for terms starting with <code>prefix</code>. </summary>
		public PrefixQuery(Term prefix)
		{
			this.prefix = prefix;
		}
		
		/// <summary>Returns the prefix of this query. </summary>
		public virtual Term GetPrefix()
		{
			return prefix;
		}
		
		public override Query Rewrite(Monodoc.Lucene.Net.Index.IndexReader reader)
		{
			BooleanQuery query = new BooleanQuery();
			TermEnum enumerator = reader.Terms(prefix);
			try
			{
				System.String prefixText = prefix.Text();
				System.String prefixField = prefix.Field();
				do 
				{
					Term term = enumerator.Term();
					if (term != null && term.Text().StartsWith(prefixText) && (System.Object) term.Field() == (System.Object) prefixField)
					{
						TermQuery tq = new TermQuery(term); // found a match
						tq.SetBoost(GetBoost()); // set the boost
						query.Add(tq, false, false); // add to query
						//System.out.println("added " + term);
					}
					else
					{
						break;
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
			if (!prefix.Field().Equals(field))
			{
				buffer.Append(prefix.Field());
				buffer.Append(":");
			}
			buffer.Append(prefix.Text());
			buffer.Append('*');
			if (GetBoost() != 1.0f)
			{
                System.Globalization.NumberFormatInfo nfi = new System.Globalization.CultureInfo("en-US", false).NumberFormat;
                nfi.NumberDecimalDigits = 1;

				buffer.Append("^");
				buffer.Append(GetBoost().ToString("N", nfi));
			}
			return buffer.ToString();
		}
		override public System.Object Clone()
		{
			return null;
		}
	}
}