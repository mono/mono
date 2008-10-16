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
	
	/// <summary> A Query that matches documents within an exclusive range.
	/// 
	/// </summary>
	/// <version>  $Id: RangeQuery.java,v 1.12 2004/03/29 22:48:03 cutting Exp $
	/// </version>
	[Serializable]
	public class RangeQuery:Query
	{
		private Term lowerTerm;
		private Term upperTerm;
		private bool inclusive;
		
		/// <summary>Constructs a query selecting all terms greater than
		/// <code>lowerTerm</code> but less than <code>upperTerm</code>.
		/// There must be at least one term and either term may be null,
		/// in which case there is no bound on that side, but if there are
		/// two terms, both terms <b>must</b> be for the same Field.
		/// </summary>
		public RangeQuery(Term lowerTerm, Term upperTerm, bool inclusive)
		{
			if (lowerTerm == null && upperTerm == null)
			{
				throw new System.ArgumentException("At least one term must be non-null");
			}
			if (lowerTerm != null && upperTerm != null && (System.Object) lowerTerm.Field() != (System.Object) upperTerm.Field())
			{
				throw new System.ArgumentException("Both terms must be for the same Field");
			}
			
			// if we have a lowerTerm, start there. otherwise, start at beginning
			if (lowerTerm != null)
			{
				this.lowerTerm = lowerTerm;
			}
			else
			{
				this.lowerTerm = new Term(upperTerm.Field(), "");
			}
			
			this.upperTerm = upperTerm;
			this.inclusive = inclusive;
		}
		
		/// <summary> FIXME: Describe <code>rewrite</code> method here.
		/// 
		/// </summary>
		/// <param name="reader">an <code>Monodoc.Lucene.Net.Index.IndexReader</code> value
		/// </param>
		/// <returns> a <code>Query</code> value
		/// </returns>
		/// <exception cref=""> IOException if an error occurs
		/// </exception>
		public override Query Rewrite(Monodoc.Lucene.Net.Index.IndexReader reader)
		{
			
			BooleanQuery query = new BooleanQuery();
			TermEnum enumerator = reader.Terms(lowerTerm);
			
			try
			{
				
				bool checkLower = false;
				if (!inclusive)
				// make adjustments to set to exclusive
					checkLower = true;
				
				System.String testField = GetField();
				
				do 
				{
					Term term = enumerator.Term();
					if (term != null && (System.Object) term.Field() == (System.Object) testField)
					{
						if (!checkLower || String.CompareOrdinal(term.Text(), lowerTerm.Text()) > 0)
						{
							checkLower = false;
							if (upperTerm != null)
							{
								int compare = String.CompareOrdinal(upperTerm.Text(), term.Text());
								/* if beyond the upper term, or is exclusive and
								* this is equal to the upper term, break out */
								if ((compare < 0) || (!inclusive && compare == 0))
									break;
							}
							TermQuery tq = new TermQuery(term); // found a match
							tq.SetBoost(GetBoost()); // set the boost
							query.Add(tq, false, false); // add to query
						}
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
		
		/// <summary>Returns the Field name for this query </summary>
		public virtual System.String GetField()
		{
			return (lowerTerm != null?lowerTerm.Field():upperTerm.Field());
		}
		
		/// <summary>Returns the lower term of this range query </summary>
		public virtual Term GetLowerTerm()
		{
			return lowerTerm;
		}
		
		/// <summary>Returns the upper term of this range query </summary>
		public virtual Term GetUpperTerm()
		{
			return upperTerm;
		}
		
		/// <summary>Returns <code>true</code> if the range query is inclusive </summary>
		public virtual bool IsInclusive()
		{
			return inclusive;
		}
		
		
		/// <summary>Prints a user-readable version of this query. </summary>
		public override System.String ToString(System.String field)
		{
			System.Text.StringBuilder buffer = new System.Text.StringBuilder();
			if (!GetField().Equals(field))
			{
				buffer.Append(GetField());
				buffer.Append(":");
			}
			buffer.Append(inclusive?"[":"{");
			buffer.Append(lowerTerm != null?lowerTerm.Text():"null");
			buffer.Append(" TO ");
			buffer.Append(upperTerm != null?upperTerm.Text():"null");
			buffer.Append(inclusive?"]":"}");
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