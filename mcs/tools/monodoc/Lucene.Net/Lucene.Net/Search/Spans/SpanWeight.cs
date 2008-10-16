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
using Explanation = Monodoc.Lucene.Net.Search.Explanation;
using Query = Monodoc.Lucene.Net.Search.Query;
using Scorer = Monodoc.Lucene.Net.Search.Scorer;
using Searcher = Monodoc.Lucene.Net.Search.Searcher;
using Similarity = Monodoc.Lucene.Net.Search.Similarity;
using Weight = Monodoc.Lucene.Net.Search.Weight;
namespace Monodoc.Lucene.Net.Search.Spans
{
	
	[Serializable]
	class SpanWeight : Weight
	{
        virtual public Query Query
        {
            get
            {
                return query;
            }
			
        }
        virtual public float Value
        {
            get
            {
                return value_Renamed;
            }
			
        }
        private Searcher searcher;
		private float value_Renamed;
		private float idf;
		private float queryNorm;
		private float queryWeight;
		
		private System.Collections.ICollection terms;
		private SpanQuery query;
		
		public SpanWeight(SpanQuery query, Searcher searcher)
		{
			this.searcher = searcher;
			this.query = query;
			this.terms = query.GetTerms();
		}
		
		public virtual float SumOfSquaredWeights()
		{
			idf = this.query.GetSimilarity(searcher).Idf(terms, searcher);
			queryWeight = idf * query.GetBoost(); // compute query weight
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
			return new SpanScorer(query.GetSpans(reader), this, query.GetSimilarity(searcher), reader.Norms(query.GetField()));
		}
		
		public virtual Explanation Explain(Monodoc.Lucene.Net.Index.IndexReader reader, int doc)
		{
			
			Explanation result = new Explanation();
			result.SetDescription("weight(" + Query + " in " + doc + "), product of:");
			System.String field = ((SpanQuery) Query).GetField();
			
			System.Text.StringBuilder docFreqs = new System.Text.StringBuilder();
			System.Collections.IEnumerator i = terms.GetEnumerator();
			while (i.MoveNext())
			{
				Term term = (Term) i.Current;
				docFreqs.Append(term.Text());
				docFreqs.Append("=");
				docFreqs.Append(searcher.DocFreq(term));
				
				if (i.MoveNext())
				{
					docFreqs.Append(" ");
				}
			}
			
			Explanation idfExpl = new Explanation(idf, "idf(" + field + ": " + docFreqs + ")");
			
			// explain query weight
			Explanation queryExpl = new Explanation();
			queryExpl.SetDescription("queryWeight(" + Query + "), product of:");
			
			Explanation boostExpl = new Explanation(Query.GetBoost(), "boost");
			if (Query.GetBoost() != 1.0f)
				queryExpl.AddDetail(boostExpl);
			queryExpl.AddDetail(idfExpl);
			
			Explanation queryNormExpl = new Explanation(queryNorm, "queryNorm");
			queryExpl.AddDetail(queryNormExpl);
			
			queryExpl.SetValue(boostExpl.GetValue() * idfExpl.GetValue() * queryNormExpl.GetValue());
			
			result.AddDetail(queryExpl);
			
			// explain Field weight
			Explanation fieldExpl = new Explanation();
			fieldExpl.SetDescription("fieldWeight(" + field + ":" + query.ToString(field) + " in " + doc + "), product of:");
			
			Explanation tfExpl = Scorer(reader).Explain(doc);
			fieldExpl.AddDetail(tfExpl);
			fieldExpl.AddDetail(idfExpl);
			
			Explanation fieldNormExpl = new Explanation();
			byte[] fieldNorms = reader.Norms(field);
			float fieldNorm = fieldNorms != null ? Similarity.DecodeNorm(fieldNorms[doc]) : 0.0f;
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
}