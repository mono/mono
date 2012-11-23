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

namespace Mono.Lucene.Net.Analysis
{
	
	/// <summary> Simplistic {@link CharFilter} that applies the mappings
	/// contained in a {@link NormalizeCharMap} to the character
	/// stream, and correcting the resulting changes to the
	/// offsets.
	/// </summary>
	public class MappingCharFilter:BaseCharFilter
	{
		
		private NormalizeCharMap normMap;
		//private LinkedList<Character> buffer;
		private System.Collections.ArrayList buffer;
		private System.String replacement;
		private int charPointer;
		private int nextCharCounter;
		
		/// Default constructor that takes a {@link CharStream}.
		public MappingCharFilter(NormalizeCharMap normMap, CharStream in_Renamed):base(in_Renamed)
		{
			this.normMap = normMap;
		}
		
		/// Easy-use constructor that takes a {@link Reader}.
		public MappingCharFilter(NormalizeCharMap normMap, System.IO.TextReader in_Renamed):base(CharReader.Get(in_Renamed))
		{
			this.normMap = normMap;
		}
		
		public  override int Read()
		{
			while (true)
			{
				if (replacement != null && charPointer < replacement.Length)
				{
					return replacement[charPointer++];
				}
				
				int firstChar = NextChar();
				if (firstChar == - 1)
					return - 1;
				NormalizeCharMap nm = normMap.submap != null?(NormalizeCharMap) normMap.submap[CharacterCache.ValueOf((char) firstChar)]:null;
				if (nm == null)
					return firstChar;
				NormalizeCharMap result = Match(nm);
				if (result == null)
					return firstChar;
				replacement = result.normStr;
				charPointer = 0;
				if (result.diff != 0)
				{
					int prevCumulativeDiff = GetLastCumulativeDiff();
					if (result.diff < 0)
					{
						for (int i = 0; i < - result.diff; i++)
							AddOffCorrectMap(nextCharCounter + i - prevCumulativeDiff, prevCumulativeDiff - 1 - i);
					}
					else
					{
						AddOffCorrectMap(nextCharCounter - result.diff - prevCumulativeDiff, prevCumulativeDiff + result.diff);
					}
				}
			}
		}
		
		private int NextChar()
		{
			nextCharCounter++;
			if (buffer != null && !(buffer.Count == 0))
			{
				System.Object tempObject;
				tempObject = buffer[0];
				buffer.RemoveAt(0);
				return ((System.Char) tempObject);
			}
			return input.Read();
		}
		
		private void  PushChar(int c)
		{
			nextCharCounter--;
			if (buffer == null)
			{
				buffer = new System.Collections.ArrayList();
			}
			buffer.Insert(0, (char) c);
		}
		
		private void  PushLastChar(int c)
		{
			if (buffer == null)
			{
				buffer = new System.Collections.ArrayList();
			}
			buffer.Insert(buffer.Count, (char) c);
		}
		
		private NormalizeCharMap Match(NormalizeCharMap map)
		{
			NormalizeCharMap result = null;
			if (map.submap != null)
			{
				int chr = NextChar();
				if (chr != - 1)
				{
					NormalizeCharMap subMap = (NormalizeCharMap) map.submap[CharacterCache.ValueOf((char) chr)];
					if (subMap != null)
					{
						result = Match(subMap);
					}
					if (result == null)
					{
						PushChar(chr);
					}
				}
			}
			if (result == null && map.normStr != null)
			{
				result = map;
			}
			return result;
		}
		
		public  override int Read(System.Char[] cbuf, int off, int len)
		{
			char[] tmp = new char[len];
			int l = input.Read(tmp, 0, len);
			if (l != 0)
			{
				for (int i = 0; i < l; i++)
					PushLastChar(tmp[i]);
			}
			l = 0;
			for (int i = off; i < off + len; i++)
			{
				int c = Read();
				if (c == - 1)
					break;
				cbuf[i] = (char) c;
				l++;
			}
			return l == 0?- 1:l;
		}
	}
}
