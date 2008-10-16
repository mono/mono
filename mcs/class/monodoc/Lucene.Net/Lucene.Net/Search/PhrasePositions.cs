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
namespace Monodoc.Lucene.Net.Search
{
	
	sealed class PhrasePositions
	{
		internal int doc; // current doc
		internal int position; // position in doc
		internal int count; // remaining pos in this doc
		internal int offset; // position in phrase
		internal TermPositions tp; // stream of positions
		internal PhrasePositions next; // used to make lists
		
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