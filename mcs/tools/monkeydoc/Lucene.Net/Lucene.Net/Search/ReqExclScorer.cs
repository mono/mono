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

namespace Mono.Lucene.Net.Search
{
	
	
	/// <summary>A Scorer for queries with a required subscorer
	/// and an excluding (prohibited) sub DocIdSetIterator.
	/// <br/>
	/// This <code>Scorer</code> implements {@link Scorer#SkipTo(int)},
	/// and it uses the skipTo() on the given scorers.
	/// </summary>
	class ReqExclScorer:Scorer
	{
		private Scorer reqScorer;
		private DocIdSetIterator exclDisi;
		private int doc = - 1;
		
		/// <summary>Construct a <code>ReqExclScorer</code>.</summary>
		/// <param name="reqScorer">The scorer that must match, except where
		/// </param>
		/// <param name="exclDisi">indicates exclusion.
		/// </param>
		public ReqExclScorer(Scorer reqScorer, DocIdSetIterator exclDisi):base(null)
		{ // No similarity used.
			this.reqScorer = reqScorer;
			this.exclDisi = exclDisi;
		}
		
		/// <deprecated> use {@link #NextDoc()} instead. 
		/// </deprecated>
        [Obsolete("use NextDoc() instead. ")]
		public override bool Next()
		{
			return NextDoc() != NO_MORE_DOCS;
		}
		
		public override int NextDoc()
		{
			if (reqScorer == null)
			{
				return doc;
			}
			doc = reqScorer.NextDoc();
			if (doc == NO_MORE_DOCS)
			{
				reqScorer = null; // exhausted, nothing left
				return doc;
			}
			if (exclDisi == null)
			{
				return doc;
			}
			return doc = ToNonExcluded();
		}
		
		/// <summary>Advance to non excluded doc.
		/// <br/>On entry:
		/// <ul>
		/// <li>reqScorer != null, </li>
		/// <li>exclScorer != null, </li>
		/// <li>reqScorer was advanced once via next() or skipTo() 
        /// and reqScorer.doc() may still be excluded.</li>
		/// </ul>
		/// Advances reqScorer a non excluded required doc, if any.
		/// </summary>
		/// <returns> true iff there is a non excluded required doc.
		/// </returns>
		private int ToNonExcluded()
		{
			int exclDoc = exclDisi.DocID();
			int reqDoc = reqScorer.DocID(); // may be excluded
			do 
			{
				if (reqDoc < exclDoc)
				{
					return reqDoc; // reqScorer advanced to before exclScorer, ie. not excluded
				}
				else if (reqDoc > exclDoc)
				{
					exclDoc = exclDisi.Advance(reqDoc);
					if (exclDoc == NO_MORE_DOCS)
					{
						exclDisi = null; // exhausted, no more exclusions
						return reqDoc;
					}
					if (exclDoc > reqDoc)
					{
						return reqDoc; // not excluded
					}
				}
			}
			while ((reqDoc = reqScorer.NextDoc()) != NO_MORE_DOCS);
			reqScorer = null; // exhausted, nothing left
			return NO_MORE_DOCS;
		}
		
		/// <deprecated> use {@link #DocID()} instead. 
		/// </deprecated>
        [Obsolete("use DocID() instead.")]
		public override int Doc()
		{
			return reqScorer.Doc(); // reqScorer may be null when next() or skipTo() already return false
		}
		
		public override int DocID()
		{
			return doc;
		}
		
		/// <summary>Returns the score of the current document matching the query.
		/// Initially invalid, until {@link #Next()} is called the first time.
		/// </summary>
		/// <returns> The score of the required scorer.
		/// </returns>
		public override float Score()
		{
			return reqScorer.Score(); // reqScorer may be null when next() or skipTo() already return false
		}
		
		/// <deprecated> use {@link #Advance(int)} instead. 
		/// </deprecated>
        [Obsolete("use Advance(int) instead.")]
		public override bool SkipTo(int target)
		{
			return Advance(target) != NO_MORE_DOCS;
		}
		
		public override int Advance(int target)
		{
			if (reqScorer == null)
			{
				return doc = NO_MORE_DOCS;
			}
			if (exclDisi == null)
			{
				return doc = reqScorer.Advance(target);
			}
			if (reqScorer.Advance(target) == NO_MORE_DOCS)
			{
				reqScorer = null;
				return doc = NO_MORE_DOCS;
			}
			return doc = ToNonExcluded();
		}
		
		public override Explanation Explain(int doc)
		{
			Explanation res = new Explanation();
			if (exclDisi.Advance(doc) == doc)
			{
				res.SetDescription("excluded");
			}
			else
			{
				res.SetDescription("not excluded");
				res.AddDetail(reqScorer.Explain(doc));
			}
			return res;
		}
	}
}
