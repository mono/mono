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
using MultipleTermPositions = Monodoc.Lucene.Net.Index.MultipleTermPositions;
using Term = Monodoc.Lucene.Net.Index.Term;
using TermPositions = Monodoc.Lucene.Net.Index.TermPositions;
namespace Monodoc.Lucene.Net.Search
{
	
	/// <summary> PhrasePrefixQuery is a generalized version of PhraseQuery, with an added
	/// method {@link #Add(Term[])}.
	/// To use this class, to search for the phrase "Microsoft app*" first use
	/// add(Term) on the term "Microsoft", then find all terms that has "app" as
	/// prefix using Monodoc.Lucene.Net.Index.IndexReader.terms(Term), and use PhrasePrefixQuery.add(Term[]
	/// terms) to add them to the query.
	/// 
	/// </summary>
	/// <author>  Anders Nielsen
	/// </author>
	/// <version>  1.0
	/// </version>
	[Serializable]
	public class PhrasePrefixQuery:Query
	{
		private System.String field;
		private System.Collections.ArrayList termArrays = new System.Collections.ArrayList();
		private System.Collections.ArrayList positions = System.Collections.ArrayList.Synchronized(new System.Collections.ArrayList(10));
		
		private int slop = 0;
		
        /// <summary>Sets the phrase slop for this query.</summary>
        /// <seealso cref="PhraseQuery#SetSlop(int)">
        /// </seealso>
        public virtual void  SetSlop(int s)
		{
			slop = s;
		}
		
        /// <summary>Sets the phrase slop for this query.</summary>
        /// <seealso cref="PhraseQuery#GetSlop()">
        /// </seealso>
        public virtual int GetSlop()
		{
			return slop;
		}
		
        /// <summary>Add a single term at the next position in the phrase.</summary>
        /// <seealso cref="PhraseQuery#Add(Term)">
        /// </seealso>
        public virtual void  Add(Term term)
		{
			Add(new Term[]{term});
		}
		
        /// <summary>Add multiple terms at the next position in the phrase.  Any of the terms
        /// may match.
        /// 
        /// </summary>
        /// <seealso cref="PhraseQuery#Add(Term)">
        /// </seealso>
		public virtual void  Add(Term[] terms)
		{
			int position = 0;
			if (positions.Count > 0)
				position = ((System.Int32) positions[positions.Count - 1]) + 1;
			
			Add(terms, position);
		}
		
        /// <summary> Allows to specify the relative position of terms within the phrase.
        /// 
        /// </summary>
        /// <seealso cref="int)">
        /// </seealso>
        /// <param name="">terms
        /// </param>
        /// <param name="">position
        /// </param>
        public virtual void  Add(Term[] terms, int position)
        {
            if (termArrays.Count == 0)
                field = terms[0].Field();
			
            for (int i = 0; i < terms.Length; i++)
            {
                if ((System.Object) terms[i].Field() != (System.Object) field)
                {
                    throw new System.ArgumentException("All phrase terms must be in the same field (" + field + "): " + terms[i]);
                }
            }
			
            termArrays.Add(terms);
            positions.Add((System.Int32) position);
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
		private class PhrasePrefixWeight : Weight
		{
			private void  InitBlock(PhrasePrefixQuery enclosingInstance)
			{
				this.enclosingInstance = enclosingInstance;
			}
			private PhrasePrefixQuery enclosingInstance;
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
            public PhrasePrefixQuery Enclosing_Instance
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
			
			public PhrasePrefixWeight(PhrasePrefixQuery enclosingInstance, Searcher searcher)
			{
				InitBlock(enclosingInstance);
				this.searcher = searcher;
			}
			
			public virtual float SumOfSquaredWeights()
			{
				System.Collections.IEnumerator i = Enclosing_Instance.termArrays.GetEnumerator();
				while (i.MoveNext())
				{
					Term[] terms = (Term[]) i.Current;
					for (int j = 0; j < terms.Length; j++)
						idf += Enclosing_Instance.GetSimilarity(searcher).Idf(terms[j], searcher);
				}
				
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
				if (Enclosing_Instance.termArrays.Count == 0)
				// optimize zero-term case
					return null;
				
				TermPositions[] tps = new TermPositions[Enclosing_Instance.termArrays.Count];
				for (int i = 0; i < tps.Length; i++)
				{
					Term[] terms = (Term[]) Enclosing_Instance.termArrays[i];
					
					TermPositions p;
					if (terms.Length > 1)
						p = new MultipleTermPositions(reader, terms);
					else
						p = reader.TermPositions(terms[0]);
					
					if (p == null)
						return null;
					
					tps[i] = p;
				}
				
				if (Enclosing_Instance.slop == 0)
					return new ExactPhraseScorer(this, tps, Enclosing_Instance.GetPositions(), Enclosing_Instance.GetSimilarity(searcher), reader.Norms(Enclosing_Instance.field));
				else
					return new SloppyPhraseScorer(this, tps, Enclosing_Instance.GetPositions(), Enclosing_Instance.GetSimilarity(searcher), Enclosing_Instance.slop, reader.Norms(Enclosing_Instance.field));
			}
			
			public virtual Explanation Explain(Monodoc.Lucene.Net.Index.IndexReader reader, int doc)
			{
				Explanation result = new Explanation();
				result.SetDescription("weight(" + Query + " in " + doc + "), product of:");
				
				Explanation idfExpl = new Explanation(idf, "idf(" + Query + ")");
				
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
				fieldExpl.SetDescription("fieldWeight(" + Query + " in " + doc + "), product of:");
				
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
			if (termArrays.Count == 1)
			{
				// optimize one-term case
				Term[] terms = (Term[]) termArrays[0];
				BooleanQuery boq = new BooleanQuery();
				for (int i = 0; i < terms.Length; i++)
				{
					boq.Add(new TermQuery(terms[i]), false, false);
				}
				boq.SetBoost(GetBoost());
				return boq.CreateWeight(searcher);
			}
			return new PhrasePrefixWeight(this, searcher);
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
			System.Collections.IEnumerator i = termArrays.GetEnumerator();
			while (i.MoveNext())
			{
				Term[] terms = (Term[]) i.Current;
				buffer.Append(terms[0].Text() + (terms.Length > 0 ? "*" : ""));
                if (i.MoveNext())
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
				buffer.Append("^");
				buffer.Append(GetBoost().ToString());
			}
			
			return buffer.ToString();
		}
		override public System.Object Clone()
		{
			return null;
		}
	}
}