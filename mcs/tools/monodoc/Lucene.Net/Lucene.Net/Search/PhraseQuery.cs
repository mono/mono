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
using TermPositions = Monodoc.Lucene.Net.Index.TermPositions;
namespace Monodoc.Lucene.Net.Search
{
	
	/// <summary>A Query that matches documents containing a particular sequence of terms.
	/// This may be combined with other terms with a {@link BooleanQuery}.
	/// </summary>
	[Serializable]
	public class PhraseQuery:Query
	{
		private System.String field;
		private System.Collections.ArrayList terms = System.Collections.ArrayList.Synchronized(new System.Collections.ArrayList(10));
        private System.Collections.ArrayList positions = System.Collections.ArrayList.Synchronized(new System.Collections.ArrayList(10));
        private int slop = 0;
		
		/// <summary>Constructs an empty phrase query. </summary>
		public PhraseQuery()
		{
		}
		
		/// <summary>Sets the number of other words permitted between words in query phrase.
		/// If zero, then this is an exact phrase search.  For larger values this works
		/// like a <code>WITHIN</code> or <code>NEAR</code> operator.
		/// <p>The slop is in fact an edit-distance, where the units correspond to
		/// moves of terms in the query phrase out of position.  For example, to switch
		/// the order of two words requires two moves (the first move places the words
		/// atop one another), so to permit re-orderings of phrases, the slop must be
		/// at least two.
		/// <p>More exact matches are scored higher than sloppier matches, thus search
		/// results are sorted by exactness.
		/// <p>The slop is zero by default, requiring exact matches.
		/// </summary>
		public virtual void  SetSlop(int s)
		{
			slop = s;
		}
		/// <summary>Returns the slop.  See setSlop(). </summary>
		public virtual int GetSlop()
		{
			return slop;
		}
		
        /// <summary> Adds a term to the end of the query phrase.
        /// The relative position of the term is the one immediately after the last term added.
        /// </summary>
        public virtual void  Add(Term term)
        {
            int position = 0;
            if (positions.Count > 0)
                position = ((System.Int32) positions[positions.Count - 1]) + 1;
			
            Add(term, position);
        }
		
        /// <summary> Adds a term to the end of the query phrase.
        /// The relative position of the term within the phrase is specified explicitly.
        /// This allows e.g. phrases with more than one term at the same position
        /// or phrases with gaps (e.g. in connection with stopwords).
        /// 
        /// </summary>
        /// <param name="">term
        /// </param>
        /// <param name="">position
        /// </param>
        public virtual void  Add(Term term, int position)
        {
            if (terms.Count == 0)
                field = term.Field();
            else if ((System.Object) term.Field() != (System.Object) field)
            {
                throw new System.ArgumentException("All phrase terms must be in the same field: " + term);
            }
			
            terms.Add(term);
            positions.Add((System.Int32) position);
        }
		
        /// <summary>Returns the set of terms in this phrase. </summary>
		public virtual Term[] GetTerms()
		{
			return (Term[]) terms.ToArray(typeof(Term));
		}
		
        /// <summary> Returns the relative positions of terms in this phrase.</summary>
        public virtual int[] GetPositions()
        {
            int[] result = new int[positions.Count];
            for (int i = 0; i < positions.Count; i++)
                result[i] = ((System.Int32) positions[i]);
            return result;
        }
		
        [Serializable]
		private class PhraseWeight : Weight
		{
			private void  InitBlock(PhraseQuery enclosingInstance)
			{
				this.enclosingInstance = enclosingInstance;
			}
			private PhraseQuery enclosingInstance;
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
            public PhraseQuery Enclosing_Instance
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
			
			public PhraseWeight(PhraseQuery enclosingInstance, Searcher searcher)
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
				idf = Enclosing_Instance.GetSimilarity(searcher).Idf(Enclosing_Instance.terms, searcher);
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
				if (Enclosing_Instance.terms.Count == 0)
				// optimize zero-term case
					return null;
				
				TermPositions[] tps = new TermPositions[Enclosing_Instance.terms.Count];
				for (int i = 0; i < Enclosing_Instance.terms.Count; i++)
				{
					TermPositions p = reader.TermPositions((Term) Enclosing_Instance.terms[i]);
					if (p == null)
						return null;
					tps[i] = p;
				}
				
				if (Enclosing_Instance.slop == 0)
				// optimize exact case
					return new ExactPhraseScorer(this, tps, Enclosing_Instance.GetPositions(), Enclosing_Instance.GetSimilarity(searcher), reader.Norms(Enclosing_Instance.field));
				else
					return new SloppyPhraseScorer(this, tps, Enclosing_Instance.GetPositions(), Enclosing_Instance.GetSimilarity(searcher), Enclosing_Instance.slop, reader.Norms(Enclosing_Instance.field));
			}
			
			public virtual Explanation Explain(Monodoc.Lucene.Net.Index.IndexReader reader, int doc)
			{
				
				Explanation result = new Explanation();
				result.SetDescription("weight(" + Query + " in " + doc + "), product of:");
				
				System.Text.StringBuilder docFreqs = new System.Text.StringBuilder();
				System.Text.StringBuilder query = new System.Text.StringBuilder();
				query.Append('\"');
				for (int i = 0; i < Enclosing_Instance.terms.Count; i++)
				{
					if (i != 0)
					{
						docFreqs.Append(" ");
						query.Append(" ");
					}
					
					Term term = (Term) Enclosing_Instance.terms[i];
					
					docFreqs.Append(term.Text());
					docFreqs.Append("=");
					docFreqs.Append(searcher.DocFreq(term));
					
					query.Append(term.Text());
				}
				query.Append('\"');
				
				Explanation idfExpl = new Explanation(idf, "idf(" + Enclosing_Instance.field + ": " + docFreqs + ")");
				
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
				Explanation fieldExpl = new Explanation();
				fieldExpl.SetDescription("fieldWeight(" + Enclosing_Instance.field + ":" + query + " in " + doc + "), product of:");
				
				Explanation tfExpl = Scorer(reader).Explain(doc);
				fieldExpl.AddDetail(tfExpl);
				fieldExpl.AddDetail(idfExpl);
				
				Explanation fieldNormExpl = new Explanation();
				byte[] fieldNorms = reader.Norms(Enclosing_Instance.field);
				float fieldNorm = fieldNorms != null?Similarity.DecodeNorm(fieldNorms[doc]):0.0f;
				fieldNormExpl.SetValue(fieldNorm);
				fieldNormExpl.SetDescription("fieldNorm(Field=" + Enclosing_Instance.field + ", doc=" + doc + ")");
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
		
		protected internal override Weight CreateWeight(Searcher searcher)
		{
			if (terms.Count == 1)
			{
				// optimize one-term case
				Term term = (Term) terms[0];
				Query termQuery = new TermQuery(term);
				termQuery.SetBoost(GetBoost());
				return termQuery.CreateWeight(searcher);
			}
			return new PhraseWeight(this, searcher);
		}
		
		
		/// <summary>Prints a user-readable version of this query. </summary>
		public override System.String ToString(System.String f)
		{
			System.Text.StringBuilder buffer = new System.Text.StringBuilder();
			if (!field.Equals(f))
			{
				buffer.Append(field);
				buffer.Append(":");
			}
			
			buffer.Append("\"");
			for (int i = 0; i < terms.Count; i++)
			{
				buffer.Append(((Term) terms[i]).Text());
				if (i != terms.Count - 1)
					buffer.Append(" ");
			}
			buffer.Append("\"");
			
			if (slop != 0)
			{
				buffer.Append("~");
				buffer.Append(slop);
			}
			
			if (GetBoost() != 1.0f)
			{
                System.Globalization.NumberFormatInfo nfi = new System.Globalization.CultureInfo("en-US", false).NumberFormat;
                nfi.NumberDecimalDigits = 1;

                buffer.Append("^");
                buffer.Append(GetBoost().ToString("N", nfi));
			}
			
			return buffer.ToString();
		}
		
		/// <summary>Returns true iff <code>o</code> is equal to this. </summary>
		public  override bool Equals(System.Object o)
		{
			if (!(o is PhraseQuery))
				return false;
			PhraseQuery other = (PhraseQuery) o;
			return (this.GetBoost() == other.GetBoost()) && 
                (this.slop == other.slop) && 
                this.terms.Equals(other.terms) && 
                this.positions.Equals(other.positions);
		}
		
		/// <summary>Returns a hash code value for this object.</summary>
		public override int GetHashCode()
		{
            return System.BitConverter.ToInt32(System.BitConverter.GetBytes(GetBoost()), 0) ^ 
                System.BitConverter.ToInt32(System.BitConverter.GetBytes(slop), 0) ^ 
                terms.GetHashCode() ^ 
                positions.GetHashCode();
		}
		override public System.Object Clone()
		{
			return null;
		}
	}
}