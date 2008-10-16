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
	
	/// <summary>Expert: Implements scoring for a class of queries. </summary>
	public abstract class Scorer
	{
		private Similarity similarity;
		
		/// <summary>Constructs a Scorer. </summary>
		protected internal Scorer(Similarity similarity)
		{
			this.similarity = similarity;
		}
		
		/// <summary>Returns the Similarity implementation used by this scorer. </summary>
		public virtual Similarity GetSimilarity()
		{
			return this.similarity;
		}
		
		/// <summary>Scores all documents and passes them to a collector. </summary>
		public virtual void  Score(HitCollector hc)
		{
			while (Next())
			{
				hc.Collect(Doc(), Score());
			}
		}
		
		/// <summary>Advance to the next document matching the query.  Returns true iff there
		/// is another match. 
		/// </summary>
		public abstract bool Next();
		
		/// <summary>Returns the current document number.  Initially invalid, until {@link
		/// #Next()} is called the first time. 
		/// </summary>
		public abstract int Doc();
		
		/// <summary>Returns the score of the current document.  Initially invalid, until
		/// {@link #Next()} is called the first time. 
		/// </summary>
		public abstract float Score();
		
		/// <summary>Skips to the first match beyond the current whose document number is
		/// greater than or equal to <i>target</i>. <p>Returns true iff there is such
		/// a match.  <p>Behaves as if written: <pre>
		/// boolean SkipTo(int target) {
		/// do {
		/// if (!next())
		/// return false;
		/// } while (target > doc());
		/// return true;
		/// }
		/// </pre>
		/// Most implementations are considerably more efficient than that.
		/// </summary>
		public abstract bool SkipTo(int target);
		
		/// <summary>Returns an explanation of the score for <code>doc</code>. </summary>
		public abstract Explanation Explain(int doc);
	}
}