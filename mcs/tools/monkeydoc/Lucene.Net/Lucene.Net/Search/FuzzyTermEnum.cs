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

using IndexReader = Mono.Lucene.Net.Index.IndexReader;
using Term = Mono.Lucene.Net.Index.Term;

namespace Mono.Lucene.Net.Search
{
	
	/// <summary>Subclass of FilteredTermEnum for enumerating all terms that are similiar
	/// to the specified filter term.
	/// 
	/// <p/>Term enumerations are always ordered by Term.compareTo().  Each term in
	/// the enumeration is greater than all that precede it.
	/// </summary>
	public sealed class FuzzyTermEnum:FilteredTermEnum
	{
		
		/* This should be somewhere around the average long word.
		* If it is longer, we waste time and space. If it is shorter, we waste a
		* little bit of time growing the array as we encounter longer words.
		*/
		private const int TYPICAL_LONGEST_WORD_IN_INDEX = 19;
		
		/* Allows us save time required to create a new array
		* everytime similarity is called.
		*/
		private int[][] d;
		
		private float similarity;
		private bool endEnum = false;
		
		private Term searchTerm = null;
		private System.String field;
		private System.String text;
		private System.String prefix;
		
		private float minimumSimilarity;
		private float scale_factor;
		private int[] maxDistances = new int[TYPICAL_LONGEST_WORD_IN_INDEX];
		
		/// <summary> Creates a FuzzyTermEnum with an empty prefix and a minSimilarity of 0.5f.
		/// <p/>
		/// After calling the constructor the enumeration is already pointing to the first 
		/// valid term if such a term exists. 
		/// 
		/// </summary>
		/// <param name="reader">
		/// </param>
		/// <param name="term">
		/// </param>
		/// <throws>  IOException </throws>
		/// <seealso cref="FuzzyTermEnum(IndexReader, Term, float, int)">
		/// </seealso>
		public FuzzyTermEnum(IndexReader reader, Term term):this(reader, term, FuzzyQuery.defaultMinSimilarity, FuzzyQuery.defaultPrefixLength)
		{
		}
		
		/// <summary> Creates a FuzzyTermEnum with an empty prefix.
		/// <p/>
		/// After calling the constructor the enumeration is already pointing to the first 
		/// valid term if such a term exists. 
		/// 
		/// </summary>
		/// <param name="reader">
		/// </param>
		/// <param name="term">
		/// </param>
		/// <param name="minSimilarity">
		/// </param>
		/// <throws>  IOException </throws>
		/// <seealso cref="FuzzyTermEnum(IndexReader, Term, float, int)">
		/// </seealso>
		public FuzzyTermEnum(IndexReader reader, Term term, float minSimilarity):this(reader, term, minSimilarity, FuzzyQuery.defaultPrefixLength)
		{
		}
		
		/// <summary> Constructor for enumeration of all terms from specified <code>reader</code> which share a prefix of
		/// length <code>prefixLength</code> with <code>term</code> and which have a fuzzy similarity &gt;
		/// <code>minSimilarity</code>.
		/// <p/>
		/// After calling the constructor the enumeration is already pointing to the first 
		/// valid term if such a term exists. 
		/// 
		/// </summary>
		/// <param name="reader">Delivers terms.
		/// </param>
		/// <param name="term">Pattern term.
		/// </param>
		/// <param name="minSimilarity">Minimum required similarity for terms from the reader. Default value is 0.5f.
		/// </param>
		/// <param name="prefixLength">Length of required common prefix. Default value is 0.
		/// </param>
		/// <throws>  IOException </throws>
		public FuzzyTermEnum(IndexReader reader, Term term, float minSimilarity, int prefixLength):base()
		{
			
			if (minSimilarity >= 1.0f)
				throw new System.ArgumentException("minimumSimilarity cannot be greater than or equal to 1");
			else if (minSimilarity < 0.0f)
				throw new System.ArgumentException("minimumSimilarity cannot be less than 0");
			if (prefixLength < 0)
				throw new System.ArgumentException("prefixLength cannot be less than 0");
			
			this.minimumSimilarity = minSimilarity;
			this.scale_factor = 1.0f / (1.0f - minimumSimilarity);
			this.searchTerm = term;
			this.field = searchTerm.Field();
			
			//The prefix could be longer than the word.
			//It's kind of silly though.  It means we must match the entire word.
			int fullSearchTermLength = searchTerm.Text().Length;
			int realPrefixLength = prefixLength > fullSearchTermLength?fullSearchTermLength:prefixLength;
			
			this.text = searchTerm.Text().Substring(realPrefixLength);
			this.prefix = searchTerm.Text().Substring(0, (realPrefixLength) - (0));
			
			InitializeMaxDistances();
			this.d = InitDistanceArray();
			
			SetEnum(reader.Terms(new Term(searchTerm.Field(), prefix)));
		}
		
		/// <summary> The termCompare method in FuzzyTermEnum uses Levenshtein distance to 
		/// calculate the distance between the given term and the comparing term. 
		/// </summary>
		public /*protected internal*/ override bool TermCompare(Term term)
		{
			if ((System.Object) field == (System.Object) term.Field() && term.Text().StartsWith(prefix))
			{
				System.String target = term.Text().Substring(prefix.Length);
				this.similarity = Similarity(target);
				return (similarity > minimumSimilarity);
			}
			endEnum = true;
			return false;
		}
		
		public override float Difference()
		{
			return (float) ((similarity - minimumSimilarity) * scale_factor);
		}
		
		public override bool EndEnum()
		{
			return endEnum;
		}
		
		/// <summary>***************************
		/// Compute Levenshtein distance
		/// ****************************
		/// </summary>
		
		/// <summary> Finds and returns the smallest of three integers </summary>
		private static int Min(int a, int b, int c)
		{
			int t = (a < b)?a:b;
			return (t < c)?t:c;
		}
		
		private int[][] InitDistanceArray()
		{
			int[][] tmpArray = new int[this.text.Length + 1][];
			for (int i = 0; i < this.text.Length + 1; i++)
			{
				tmpArray[i] = new int[TYPICAL_LONGEST_WORD_IN_INDEX];
			}
			return tmpArray;
		}
		
		/// <summary> <p/>Similarity returns a number that is 1.0f or less (including negative numbers)
		/// based on how similar the Term is compared to a target term.  It returns
		/// exactly 0.0f when
		/// <pre>
		/// editDistance &lt; maximumEditDistance</pre>
		/// Otherwise it returns:
		/// <pre>
		/// 1 - (editDistance / length)</pre>
		/// where length is the length of the shortest term (text or target) including a
		/// prefix that are identical and editDistance is the Levenshtein distance for
		/// the two words.<p/>
		/// 
		/// <p/>Embedded within this algorithm is a fail-fast Levenshtein distance
		/// algorithm.  The fail-fast algorithm differs from the standard Levenshtein
		/// distance algorithm in that it is aborted if it is discovered that the
		/// mimimum distance between the words is greater than some threshold.
		/// 
		/// <p/>To calculate the maximum distance threshold we use the following formula:
		/// <pre>
		/// (1 - minimumSimilarity) * length</pre>
		/// where length is the shortest term including any prefix that is not part of the
		/// similarity comparision.  This formula was derived by solving for what maximum value
		/// of distance returns false for the following statements:
		/// <pre>
		/// similarity = 1 - ((float)distance / (float) (prefixLength + Math.min(textlen, targetlen)));
		/// return (similarity > minimumSimilarity);</pre>
		/// where distance is the Levenshtein distance for the two words.
		/// <p/>
		/// <p/>Levenshtein distance (also known as edit distance) is a measure of similiarity
		/// between two strings where the distance is measured as the number of character
		/// deletions, insertions or substitutions required to transform one string to
		/// the other string.
		/// </summary>
		/// <param name="target">the target word or phrase
		/// </param>
		/// <returns> the similarity,  0.0 or less indicates that it matches less than the required
		/// threshold and 1.0 indicates that the text and target are identical
		/// </returns>
        private float Similarity(System.String target)
        {

            int m = target.Length;
            int n = text.Length;
            if (n == 0)
            {
                //we don't have anything to compare.  That means if we just add
                //the letters for m we get the new word
                return prefix.Length == 0 ? 0.0f : 1.0f - ((float)m / prefix.Length);
            }
            if (m == 0)
            {
                return prefix.Length == 0 ? 0.0f : 1.0f - ((float)n / prefix.Length);
            }

            int maxDistance = GetMaxDistance(m);

            if (maxDistance < System.Math.Abs(m - n))
            {
                //just adding the characters of m to n or vice-versa results in
                //too many edits
                //for example "pre" length is 3 and "prefixes" length is 8.  We can see that
                //given this optimal circumstance, the edit distance cannot be less than 5.
                //which is 8-3 or more precisesly Math.abs(3-8).
                //if our maximum edit distance is 4, then we can discard this word
                //without looking at it.
                return 0.0f;
            }

            //let's make sure we have enough room in our array to do the distance calculations.
            if (d[0].Length <= m)
            {
                GrowDistanceArray(m);
            }

            // init matrix d
            for (int i = 0; i <= n; i++)
                d[i][0] = i;
            for (int j = 0; j <= m; j++)
                d[0][j] = j;

            // start computing edit distance
            for (int i = 1; i <= n; i++)
            {
                int bestPossibleEditDistance = m;
                char s_i = text[i - 1];
                for (int j = 1; j <= m; j++)
                {
                    if (s_i != target[j - 1])
                    {
                        d[i][j] = Min(d[i - 1][j], d[i][j - 1], d[i - 1][j - 1]) + 1;
                    }
                    else
                    {
                        d[i][j] = Min(d[i - 1][j] + 1, d[i][j - 1] + 1, d[i - 1][j - 1]);
                    }
                    bestPossibleEditDistance = System.Math.Min(bestPossibleEditDistance, d[i][j]);
                }

                //After calculating row i, the best possible edit distance
                //can be found by found by finding the smallest value in a given column.
                //If the bestPossibleEditDistance is greater than the max distance, abort.

                if (i > maxDistance && bestPossibleEditDistance > maxDistance)
                {
                    //equal is okay, but not greater
                    //the closest the target can be to the text is just too far away.
                    //this target is leaving the party early.
                    return 0.0f;
                }
            }

            // this will return less than 0.0 when the edit distance is
            // greater than the number of characters in the shorter word.
            // but this was the formula that was previously used in FuzzyTermEnum,
            // so it has not been changed (even though minimumSimilarity must be
            // greater than 0.0)
            return 1.0f - ((float)d[n][m] / (float)(prefix.Length + System.Math.Min(n, m)));

        }
		
		/// <summary> Grow the second dimension of the array, so that we can calculate the
		/// Levenshtein difference.
		/// </summary>
		private void  GrowDistanceArray(int m)
		{
			for (int i = 0; i < d.Length; i++)
			{
				d[i] = new int[m + 1];
			}
		}
		
		/// <summary> The max Distance is the maximum Levenshtein distance for the text
		/// compared to some other value that results in score that is
		/// better than the minimum similarity.
		/// </summary>
		/// <param name="m">the length of the "other value"
		/// </param>
		/// <returns> the maximum levenshtein distance that we care about
		/// </returns>
		private int GetMaxDistance(int m)
		{
			return (m < maxDistances.Length)?maxDistances[m]:CalculateMaxDistance(m);
		}
		
		private void  InitializeMaxDistances()
		{
			for (int i = 0; i < maxDistances.Length; i++)
			{
				maxDistances[i] = CalculateMaxDistance(i);
			}
		}
		
		private int CalculateMaxDistance(int m)
		{
			return (int) ((1 - minimumSimilarity) * (System.Math.Min(text.Length, m) + prefix.Length));
		}
		
		public override void  Close()
		{
			base.Close(); //call super.close() and let the garbage collector do its work.
		}
	}
}
