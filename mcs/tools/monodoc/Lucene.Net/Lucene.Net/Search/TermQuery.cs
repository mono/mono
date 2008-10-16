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
using TermDocs = Monodoc.Lucene.Net.Index.TermDocs;
namespace Monodoc.Lucene.Net.Search
{
	
	/// <summary>A Query that matches documents containing a term.
	/// This may be combined with other terms with a {@link BooleanQuery}.
	/// </summary>
	[Serializable]
	public class TermQuery : Query
	{
		private Term term;
		
		[Serializable]
		private class TermWeight : Weight
		{
			private void  InitBlock(TermQuery enclosingInstance)
			{
				this.enclosingInstance = enclosingInstance;
			}
			private TermQuery enclosingInstance;
            virtual public Query Query
            {
                get
                {
                    return Enclosing_Instance;
                }
				
            }
            virtual public float Value
            {
                get
                {
                    return value_Renamed;
                }
				
            }
            public TermQuery Enclosing_Instance
			{
				get
				{
					return enclosingInstance;
				}
				
			}
			private Searcher searcher;
			private float value_Renamed;
			private float idf;
			private float queryNorm;
			private float queryWeight;
			
			public TermWeight(TermQuery enclosingInstance, Searcher searcher)
			{
				InitBlock(enclosingInstance);
				this.searcher = searcher;
			}
			
			public override System.String ToString()
			{
				return "weight(" + Enclosing_Instance + ")";
			}
			
			public virtual float SumOfSquaredWeights()
			{
				idf = Enclosing_Instance.GetSimilarity(searcher).Idf(Enclosing_Instance.term, searcher); // compute idf
				queryWeight = idf * Enclosing_Instance.GetBoost(); // compute query weight
				return queryWeight * queryWeight; // square it
			}
			
			public virtual void  Normalize(float queryNorm)
			{
				this.queryNorm = queryNorm;
				queryWeight *= queryNorm; // normalize query weight
				value_Renamed = queryWeight * idf; // idf for document 
			}
			
			public virtual Scorer Scorer(Monodoc.Lucene.Net.Index.IndexReader reader)
			{
				TermDocs termDocs = reader.TermDocs(Enclosing_Instance.term);
				
				if (termDocs == null)
					return null;
				
				return new TermScorer(this, termDocs, Enclosing_Instance.GetSimilarity(searcher), reader.Norms(Enclosing_Instance.term.Field()));
			}
			
			public virtual Explanation Explain(Monodoc.Lucene.Net.Index.IndexReader reader, int doc)
			{
				
				Explanation result = new Explanation();
				result.SetDescription("weight(" + Query + " in " + doc + "), product of:");
				
				Explanation idfExpl = new Explanation(idf, "idf(docFreq=" + searcher.DocFreq(Enclosing_Instance.term) + ")");
				
				// explain query weight
				Explanation queryExpl = new Explanation();
				queryExpl.SetDescription("queryWeight(" + Query + "), product of:");
				
				Explanation boostExpl = new Explanation(Enclosing_Instance.GetBoost(), "boost");
				if (Enclosing_Instance.GetBoost() != 1.0f)
					queryExpl.AddDetail(boostExpl);
				queryExpl.AddDetail(idfExpl);
				
				Explanation queryNormExpl = new Explanation(queryNorm, "queryNorm");
				queryExpl.AddDetail(queryNormExpl);
				
				queryExpl.SetValue(boostExpl.GetValue() * idfExpl.GetValue() * queryNormExpl.GetValue());
				
				result.AddDetail(queryExpl);
				
				// explain Field weight
				System.String field = Enclosing_Instance.term.Field();
				Explanation fieldExpl = new Explanation();
				fieldExpl.SetDescription("fieldWeight(" + Enclosing_Instance.term + " in " + doc + "), product of:");
				
				Explanation tfExpl = Scorer(reader).Explain(doc);
				fieldExpl.AddDetail(tfExpl);
				fieldExpl.AddDetail(idfExpl);
				
				Explanation fieldNormExpl = new Explanation();
				byte[] fieldNorms = reader.Norms(field);
				float fieldNorm = fieldNorms != null?Similarity.DecodeNorm(fieldNorms[doc]):0.0f;
				fieldNormExpl.SetValue(fieldNorm);
				fieldNormExpl.SetDescription("fieldNorm(Field=" + field + ", doc=" + doc + ")");
				fieldExpl.AddDetail(fieldNormExpl);
				
				fieldExpl.SetValue(tfExpl.GetValue() * idfExpl.GetValue() * fieldNormExpl.GetValue());
				
				result.AddDetail(fieldExpl);
				
				// combine them
				result.SetValue(queryExpl.GetValue() * fieldExpl.GetValue());
				
				if (queryExpl.GetValue() == 1.0f)
					return fieldExpl;
				
				return result;
			}
		}
		
		/// <summary>Constructs a query for the term <code>t</code>. </summary>
		public TermQuery(Term t)
		{
			term = t;
		}
		
		/// <summary>Returns the term of this query. </summary>
		public virtual Term GetTerm()
		{
			return term;
		}
		
		protected internal override Weight CreateWeight(Searcher searcher)
		{
			return new TermWeight(this, searcher);
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

				//buffer.Append("^");
				//buffer.Append(GetBoost().ToString());
			}
			return buffer.ToString();
		}
		
		/// <summary>Returns true iff <code>o</code> is equal to this. </summary>
		public  override bool Equals(System.Object o)
		{
			if (!(o is TermQuery))
				return false;
			TermQuery other = (TermQuery) o;
			return (this.GetBoost() == other.GetBoost()) && this.term.Equals(other.term);
		}
		
		/// <summary>Returns a hash code value for this object.</summary>
		public override int GetHashCode()
		{
            return BitConverter.ToInt32(BitConverter.GetBytes(GetBoost()), 0) ^ term.GetHashCode();
		}
	}
}