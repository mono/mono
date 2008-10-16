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
namespace Monodoc.Lucene.Net.Search
{
	
	/// <summary>Subclass of FilteredTermEnum for enumerating all terms that are similiar to the specified filter term.
	/// <p>Term enumerations are always ordered by Term.compareTo().  Each term in
	/// the enumeration is greater than all that precede it.  
	/// </summary>
	public sealed class FuzzyTermEnum:FilteredTermEnum
	{
		private void  InitBlock()
		{
			for (int i = 0; i < 1; i++)
			{
				e[i] = new int[1];
			}
		}
		internal double distance;
		internal bool endEnum = false;
		
		internal Term searchTerm = null;
		internal System.String field = "";
		internal System.String text = "";
		internal int textlen;
        internal System.String prefix = "";
        internal int prefixLength = 0;
        internal float minimumSimilarity;
        internal double scale_factor;
		
		
        /// <summary> Empty prefix and minSimilarity of 0.5f are used.
        /// 
        /// </summary>
        /// <param name="">reader
        /// </param>
        /// <param name="">term
        /// </param>
        /// <throws>  IOException </throws>
        /// <seealso cref="Term, float, int)">
        /// </seealso>
        public FuzzyTermEnum(Monodoc.Lucene.Net.Index.IndexReader reader, Term term):this(reader, term, FuzzyQuery.defaultMinSimilarity, 0)
        {
        }
		
        /// <summary> This is the standard FuzzyTermEnum with an empty prefix.
        /// 
        /// </summary>
        /// <param name="">reader
        /// </param>
        /// <param name="">term
        /// </param>
        /// <param name="">minSimilarity
        /// </param>
        /// <throws>  IOException </throws>
        /// <seealso cref="Term, float, int)">
        /// </seealso>
        public FuzzyTermEnum(Monodoc.Lucene.Net.Index.IndexReader reader, Term term, float minSimilarity):this(reader, term, minSimilarity, 0)
        {
        }
		
        /// <summary> Constructor for enumeration of all terms from specified <code>reader</code> which share a prefix of
        /// length <code>prefixLength</code> with <code>term</code> and which have a fuzzy similarity &gt;
        /// <code>minSimilarity</code>. 
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
        public FuzzyTermEnum(Monodoc.Lucene.Net.Index.IndexReader reader, Term term, float minSimilarity, int prefixLength):base()
        {
            InitBlock();
            minimumSimilarity = minSimilarity;
            scale_factor = 1.0f / (1.0f - minimumSimilarity);
            searchTerm = term;
            field = searchTerm.Field();
            text = searchTerm.Text();
            textlen = text.Length;
            if (prefixLength > 0 && prefixLength < textlen)
            {
                this.prefixLength = prefixLength;
                prefix = text.Substring(0, (prefixLength) - (0));
                text = text.Substring(prefixLength);
                textlen = text.Length;
            }
            SetEnum(reader.Terms(new Term(searchTerm.Field(), prefix)));
        }
		
        /// <summary>The termCompare method in FuzzyTermEnum uses Levenshtein distance to 
        /// calculate the distance between the given term and the comparing term. 
        /// </summary>
        protected internal override bool TermCompare(Term term)
        {
            System.String termText = term.Text();
            if ((System.Object) field == (System.Object) term.Field() && termText.StartsWith(prefix))
            {
                System.String target = termText.Substring(prefixLength);
                int targetlen = target.Length;
                int dist = EditDistance(text, target, textlen, targetlen);
                distance = 1 - ((double) dist / (double) System.Math.Min(textlen, targetlen));
                return (distance > minimumSimilarity);
            }
            endEnum = true;
            return false;
        }
		
        public override float Difference()
        {
            return (float) ((distance - minimumSimilarity) * scale_factor);
        }
		
		public override bool EndEnum()
		{
			return endEnum;
		}
		
		/// <summary>***************************
		/// Compute Levenshtein distance
		/// ****************************
		/// </summary>
		
		/// <summary>Finds and returns the smallest of three integers </summary>
		private static int Min(int a, int b, int c)
		{
			int t = (a < b) ? a : b;
			return (t < c) ? t : c;
		}
		
		/// <summary> This static array saves us from the time required to create a new array
		/// everytime editDistance is called.
		/// </summary>
		private int[][] e = new int[1][];
		
		/// <summary>Levenshtein distance also known as edit distance is a measure of similiarity
		/// between two strings where the distance is measured as the number of character 
		/// deletions, insertions or substitutions required to transform one string to 
		/// the other string. 
		/// <p>This method takes in four parameters; two strings and their respective 
		/// lengths to compute the Levenshtein distance between the two strings.
		/// The result is returned as an integer.
		/// </summary>
		private int EditDistance(System.String s, System.String t, int n, int m)
		{
			if (e.Length <= n || e[0].Length <= m)
			{
				int[][] tmpArray = new int[System.Math.Max(e.Length, n + 1)][];
				for (int i = 0; i < System.Math.Max(e.Length, n + 1); i++)
				{
					tmpArray[i] = new int[System.Math.Max(e[0].Length, m + 1)];
				}
				e = tmpArray;
			}
			int[][] d = e; // matrix
			int i2; // iterates through s
			int j; // iterates through t
			char s_i; // ith character of s
			
			if (n == 0)
				return m;
			if (m == 0)
				return n;
			
			// init matrix d
			for (i2 = 0; i2 <= n; i2++)
				d[i2][0] = i2;
			for (j = 0; j <= m; j++)
				d[0][j] = j;
			
			// start computing edit distance
			for (i2 = 1; i2 <= n; i2++)
			{
				s_i = s[i2 - 1];
				for (j = 1; j <= m; j++)
				{
					if (s_i != t[j - 1])
						d[i2][j] = Min(d[i2 - 1][j], d[i2][j - 1], d[i2 - 1][j - 1]) + 1;
					else
						d[i2][j] = Min(d[i2 - 1][j] + 1, d[i2][j - 1] + 1, d[i2 - 1][j - 1]);
				}
			}
			
			// we got the result!
			return d[n][m];
		}
		
		public override void  Close()
		{
			base.Close();
			searchTerm = null;
			field = null;
			text = null;
		}
	}
}