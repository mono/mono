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
	
	/// <summary>Scorer for conjunctions, sets of queries, all of which are required. </summary>
	sealed class ConjunctionScorer:Scorer
	{
		private class AnonymousClassComparator : System.Collections.IComparer
		{
			public AnonymousClassComparator(ConjunctionScorer enclosingInstance)
			{
				InitBlock(enclosingInstance);
			}
			private void  InitBlock(ConjunctionScorer enclosingInstance)
			{
				this.enclosingInstance = enclosingInstance;
			}
			private ConjunctionScorer enclosingInstance;
			public ConjunctionScorer Enclosing_Instance
			{
				get
				{
					return enclosingInstance;
				}
				
			}
			// sort the array
			public int Compare(System.Object o1, System.Object o2)
			{
				return ((Scorer) o1).Doc() - ((Scorer) o2).Doc();
			}
			public bool equals(System.Object o1, System.Object o2)
			{
				return ((Scorer) o1).Doc() == ((Scorer) o2).Doc();
			}
		}
		private System.Collections.ArrayList scorers = new System.Collections.ArrayList();
		private bool firstTime = true;
		private bool more = true;
		private float coord;
		
		public ConjunctionScorer(Similarity similarity):base(similarity)
		{
		}
		
		internal void  Add(Scorer scorer)
		{
			scorers.Insert(scorers.Count, scorer);
		}
		
		private Scorer First()
		{
			return (Scorer) scorers[0];
		}
		private Scorer Last()
		{
			return (Scorer) scorers[scorers.Count - 1];
		}
		
		public override int Doc()
		{
			return First().Doc();
		}
		
		public override bool Next()
		{
			if (firstTime)
			{
				Init();
			}
			else if (more)
			{
				more = Last().Next(); // trigger further scanning
			}
			return DoNext();
		}
		
		private bool DoNext()
		{
			while (more && First().Doc() < Last().Doc())
			{
				// find doc w/ all clauses
				more = First().SkipTo(Last().Doc()); // skip first upto last
				System.Object tempObject;
				tempObject = scorers[0];
				scorers.RemoveAt(0);
				scorers.Insert(scorers.Count, tempObject); // move first to last
			}
			return more; // found a doc with all clauses
		}
		
		public override bool SkipTo(int target)
		{
			System.Collections.IEnumerator i = scorers.GetEnumerator();
			while (more && i.MoveNext())
			{
				more = ((Scorer) i.Current).SkipTo(target);
			}
			if (more)
				SortScorers(); // re-sort scorers
			return DoNext();
		}
		
		public override float Score()
		{
			float score = 0.0f; // sum scores
			System.Collections.IEnumerator i = scorers.GetEnumerator();
			while (i.MoveNext())
			{
				score += ((Scorer) i.Current).Score();
			}
			score *= coord;
			return score;
		}
		
		private void  Init()
		{
			more = scorers.Count > 0;
			
			// compute coord factor
			coord = GetSimilarity().Coord(scorers.Count, scorers.Count);
			
			// move each scorer to its first entry
			System.Collections.IEnumerator i = scorers.GetEnumerator();
			while (more && i.MoveNext())
			{
				more = ((Scorer) i.Current).Next();
			}
			if (more)
				SortScorers(); // initial sort of list
			
			firstTime = false;
		}
		
		private void  SortScorers()
		{
			// move scorers to an array
            Scorer[] array = (Scorer[]) scorers.ToArray(typeof(Scorer));
			scorers.Clear(); // empty the list
			
			// note that this comparator is not consistent with equals!
			System.Array.Sort(array, new AnonymousClassComparator(this));
			
			for (int i = 0; i < array.Length; i++)
			{
				scorers.Insert(scorers.Count, array[i]); // re-build list, now sorted
			}
		}
		
		public override Explanation Explain(int doc)
		{
			throw new System.NotSupportedException();
		}
	}
}