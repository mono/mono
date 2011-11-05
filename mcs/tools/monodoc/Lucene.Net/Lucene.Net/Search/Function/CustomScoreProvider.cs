/**
 * Licensed to the Apache Software Foundation (ASF) under one or more
 * contributor license agreements.  See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The ASF licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance with
 * the License.  You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.Collections.Generic;
using System.Text;

using Mono.Lucene.Net.Index;

namespace Mono.Lucene.Net.Search.Function
{
    /**
 * An instance of this subclass should be returned by
 * {@link CustomScoreQuery#getCustomScoreProvider}, if you want
 * to modify the custom score calculation of a {@link CustomScoreQuery}.
 * <p>Since Lucene 2.9, queries operate on each segment of an Index separately,
 * so overriding the similar (now deprecated) methods in {@link CustomScoreQuery}
 * is no longer suitable, as the supplied <code>doc</code> ID is per-segment
 * and without knowledge of the IndexReader you cannot access the
 * document or {@link FieldCache}.
 * 
 * @lucene.experimental
 * @since 2.9.2
 */
    public class CustomScoreProvider
    {

        protected IndexReader reader;

        /// <summary>
        /// Creates a new instance of the provider class for the given IndexReader.
        /// </summary>
        public CustomScoreProvider(IndexReader reader)
        {
            this.reader = reader;
        }

        /// <summary>
        /// * Compute a custom score by the subQuery score and a number of 
        /// ValueSourceQuery scores.
        /// <p/> 
        /// Subclasses can override this method to modify the custom score.  
        /// <p/>
        /// If your custom scoring is different than the default herein you 
        /// should override at least one of the two customScore() methods.
        /// If the number of ValueSourceQueries is always &lt; 2 it is 
        /// sufficient to override the other 
        /// {@link #customScore(int, float, float) customScore()} 
        /// method, which is simpler. 
        /// <p/>
        /// The default computation herein is a multiplication of given scores:
        /// <pre>
        ///     ModifiedScore = valSrcScore * valSrcScores[0] * valSrcScores[1] * ...
        /// </pre>
        /// </summary>
        /// <param name="doc">id of scored doc</param>
        /// <param name="subQueryScore">score of that doc by the subQuery</param>
        /// <param name="valSrcScores">scores of that doc by the ValueSourceQuery</param>
        /// <returns>custom score</returns>
        public virtual float CustomScore(int doc, float subQueryScore, float[] valSrcScores)
        {
            if (valSrcScores.Length == 1)
            {
                return CustomScore(doc, subQueryScore, valSrcScores[0]);
            }
            if (valSrcScores.Length == 0)
            {
                return CustomScore(doc, subQueryScore, 1);
            }
            float score = subQueryScore;
            for (int i = 0; i < valSrcScores.Length; i++)
            {
                score *= valSrcScores[i];
            }
            return score;
        }
                
        /// <summary>
        /// Compute a custom score by the subQuery score and the ValueSourceQuery score.
        /// <p/> 
        /// Subclasses can override this method to modify the custom score.
        /// <p/>
        /// If your custom scoring is different than the default herein you 
        /// should override at least one of the two customScore() methods.
        /// If the number of ValueSourceQueries is always < 2 it is 
        /// sufficient to override this customScore() method, which is simpler. 
        /// <p/>
        /// The default computation herein is a multiplication of the two scores:
        /// <pre>
        ///     ModifiedScore = subQueryScore /// valSrcScore
        /// </pre>
        /// </summary>
        /// <param name="doc">id of scored doc</param>
        /// <param name="subQueryScore">score of that doc by the subQuery</param>
        /// <param name="valSrcScore">score of that doc by the ValueSourceQuery</param>
        /// <returns>custom score</returns>
        public virtual float CustomScore(int doc, float subQueryScore, float valSrcScore)
        {
            return subQueryScore * valSrcScore;
        }

        /// <summary>
        /// Explain the custom score.
        /// Whenever overriding {@link #customScore(int, float, float[])}, 
        /// this method should also be overridden to provide the correct explanation
        /// for the part of the custom scoring.
        /// </summary>
        /// <param name="doc">doc being explained</param>
        /// <param name="subQueryExpl">explanation for the sub-query part</param>
        /// <param name="valSrcExpls">explanation for the value source part</param>
        /// <returns>an explanation for the custom score</returns>
        public virtual Explanation CustomExplain(int doc, Explanation subQueryExpl, Explanation[] valSrcExpls)
        {
            if (valSrcExpls.Length == 1)
            {
                return CustomExplain(doc, subQueryExpl, valSrcExpls[0]);
            }
            if (valSrcExpls.Length == 0)
            {
                return subQueryExpl;
            }
            float valSrcScore = 1;
            for (int i = 0; i < valSrcExpls.Length; i++)
            {
                valSrcScore *= valSrcExpls[i].GetValue();
            }
            Explanation exp = new Explanation(valSrcScore * subQueryExpl.GetValue(), "custom score: product of:");
            exp.AddDetail(subQueryExpl);
            for (int i = 0; i < valSrcExpls.Length; i++)
            {
                exp.AddDetail(valSrcExpls[i]);
            }
            return exp;
        }
                
        /// <summary>
        /// Explain the custom score.
        /// Whenever overriding {@link #customScore(int, float, float)}, 
        /// this method should also be overridden to provide the correct explanation
        /// for the part of the custom scoring.
        /// 
        /// </summary>
        /// <param name="doc">doc being explained</param>
        /// <param name="subQueryExpl">explanation for the sub-query part</param>
        /// <param name="valSrcExpl">explanation for the value source part</param>
        /// <returns>an explanation for the custom score</returns>
        public virtual Explanation CustomExplain(int doc, Explanation subQueryExpl, Explanation valSrcExpl)
        {
            float valSrcScore = 1;
            if (valSrcExpl != null)
            {
                valSrcScore *= valSrcExpl.GetValue();
            }
            Explanation exp = new Explanation(valSrcScore * subQueryExpl.GetValue(), "custom score: product of:");
            exp.AddDetail(subQueryExpl);
            exp.AddDetail(valSrcExpl);
            return exp;
        }

    }
}
