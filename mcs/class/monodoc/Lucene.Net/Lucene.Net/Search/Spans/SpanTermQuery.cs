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
namespace Monodoc.Lucene.Net.Search.Spans
{
	
	/// <summary>Matches spans containing a term. </summary>
	[Serializable]
	public class SpanTermQuery:SpanQuery
	{
		private class AnonymousClassSpans : Spans
		{
			public AnonymousClassSpans(Monodoc.Lucene.Net.Index.IndexReader reader, SpanTermQuery enclosingInstance)
			{
				InitBlock(reader, enclosingInstance);
			}
			private void  InitBlock(Monodoc.Lucene.Net.Index.IndexReader reader, SpanTermQuery enclosingInstance)
			{
				this.reader = reader;
				this.enclosingInstance = enclosingInstance;
				positions = reader.TermPositions(Enclosing_Instance.term);
			}
			private Monodoc.Lucene.Net.Index.IndexReader reader;
			private SpanTermQuery enclosingInstance;
			public SpanTermQuery Enclosing_Instance
			{
				get
				{
					return enclosingInstance;
				}
				
			}
			private TermPositions positions;
			
			private int doc = - 1;
			private int freq;
			private int count;
			private int position;
			
			public virtual bool Next()
			{
				if (count == freq)
				{
					if (!positions.Next())
					{
						doc = System.Int32.MaxValue;
						return false;
					}
					doc = positions.Doc();
					freq = positions.Freq();
					count = 0;
				}
				position = positions.NextPosition();
				count++;
				return true;
			}
			
			public virtual bool SkipTo(int target)
			{
				if (!positions.SkipTo(target))
				{
					doc = System.Int32.MaxValue;
					return false;
				}
				
				doc = positions.Doc();
				freq = positions.Freq();
				count = 0;
				
				position = positions.NextPosition();
				count++;
				
				return true;
			}
			
			public virtual int Doc()
			{
				return doc;
			}
			public virtual int Start()
			{
				return position;
			}
			public virtual int End()
			{
				return position + 1;
			}
			
			public override System.String ToString()
			{
				return "spans(" + Enclosing_Instance.ToString() + ")@" + (doc == - 1?"START":((doc == System.Int32.MaxValue)?"END":doc + "-" + position));
			}
		}
		private Term term;
		
		/// <summary>Construct a SpanTermQuery matching the named term's spans. </summary>
		public SpanTermQuery(Term term)
		{
			this.term = term;
		}
		
		/// <summary>Return the term whose spans are matched. </summary>
		public virtual Term GetTerm()
		{
			return term;
		}
		
		public override System.String GetField()
		{
			return term.Field();
		}
		
		public override System.Collections.ICollection GetTerms()
		{
			System.Collections.ArrayList terms = new System.Collections.ArrayList();
            terms.Add(term);
			return terms;
		}
		
		public override System.String ToString(System.String field)
		{
			if (term.Field().Equals(field))
				return term.Text();
			else
			{
				return term.ToString();
			}
		}
		
		public override Spans GetSpans(Monodoc.Lucene.Net.Index.IndexReader reader)
		{
			return new AnonymousClassSpans(reader, this);
		}
	}
}
