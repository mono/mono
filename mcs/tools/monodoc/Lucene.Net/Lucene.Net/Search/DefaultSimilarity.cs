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
	/// <summary>Expert: Default scoring implementation. </summary>
	public class DefaultSimilarity:Similarity
	{
		/// <summary>Implemented as <code>1/sqrt(numTerms)</code>. </summary>
		public override float LengthNorm(System.String fieldName, int numTerms)
		{
			return (float) (1.0 / System.Math.Sqrt(numTerms));
		}
		
		/// <summary>Implemented as <code>1/sqrt(sumOfSquaredWeights)</code>. </summary>
		public override float QueryNorm(float sumOfSquaredWeights)
		{
			return (float) (1.0 / System.Math.Sqrt(sumOfSquaredWeights));
		}
		
		/// <summary>Implemented as <code>sqrt(freq)</code>. </summary>
		public override float Tf(float freq)
		{
			return (float) System.Math.Sqrt(freq);
		}
		
		/// <summary>Implemented as <code>1 / (distance + 1)</code>. </summary>
		public override float SloppyFreq(int distance)
		{
			return 1.0f / (distance + 1);
		}
		
		/// <summary>Implemented as <code>log(numDocs/(docFreq+1)) + 1</code>. </summary>
		public override float Idf(int docFreq, int numDocs)
		{
			return (float) (System.Math.Log(numDocs / (double) (docFreq + 1)) + 1.0);
		}
		
		/// <summary>Implemented as <code>overlap / maxOverlap</code>. </summary>
		public override float Coord(int overlap, int maxOverlap)
		{
			return overlap / (float) maxOverlap;
		}
	}
}