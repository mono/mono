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

using Mono.Lucene.Net.Index;

namespace Mono.Lucene.Net.Search
{
	
	/// <summary> Position of a term in a document that takes into account the term offset within the phrase. </summary>
	sealed class PhrasePositions
	{
		internal int doc; // current doc
		internal int position; // position in doc
		internal int count; // remaining pos in this doc
		internal int offset; // position in phrase
		internal TermPositions tp; // stream of positions
		internal PhrasePositions next; // used to make lists
		internal bool repeats; // there's other pp for same term (e.g. query="1st word 2nd word"~1) 
		
		internal PhrasePositions(TermPositions t, int o)
		{
			tp = t;
			offset = o;
		}
		
		internal bool Next()
		{
			// increments to next doc
			if (!tp.Next())
			{
				tp.Close(); // close stream
				doc = System.Int32.MaxValue; // sentinel value
				return false;
			}
			doc = tp.Doc();
			position = 0;
			return true;
		}
		
		internal bool SkipTo(int target)
		{
			if (!tp.SkipTo(target))
			{
				tp.Close(); // close stream
				doc = System.Int32.MaxValue; // sentinel value
				return false;
			}
			doc = tp.Doc();
			position = 0;
			return true;
		}
		
		
		internal void  FirstPosition()
		{
			count = tp.Freq(); // read first pos
			NextPosition();
		}
		
		/// <summary> Go to next location of this term current document, and set 
		/// <code>position</code> as <code>location - offset</code>, so that a 
		/// matching exact phrase is easily identified when all PhrasePositions 
		/// have exactly the same <code>position</code>.
		/// </summary>
		internal bool NextPosition()
		{
			if (count-- > 0)
			{
				// read subsequent pos's
				position = tp.NextPosition() - offset;
				return true;
			}
			else
				return false;
		}
	}
}
