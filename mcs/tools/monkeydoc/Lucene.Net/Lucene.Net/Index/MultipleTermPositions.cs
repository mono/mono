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

using PriorityQueue = Mono.Lucene.Net.Util.PriorityQueue;

namespace Mono.Lucene.Net.Index
{
	
	/// <summary> Allows you to iterate over the {@link TermPositions} for multiple {@link Term}s as
	/// a single {@link TermPositions}.
	/// 
	/// </summary>
	public class MultipleTermPositions : TermPositions
	{
		
		private sealed class TermPositionsQueue:PriorityQueue
		{
			internal TermPositionsQueue(System.Collections.IList termPositions)
			{
				Initialize(termPositions.Count);
				
				System.Collections.IEnumerator i = termPositions.GetEnumerator();
				while (i.MoveNext())
				{
					TermPositions tp = (TermPositions) i.Current;
					if (tp.Next())
						Put(tp);
				}
			}
			
			internal TermPositions Peek()
			{
				return (TermPositions) Top();
			}
			
			public override bool LessThan(System.Object a, System.Object b)
			{
				return ((TermPositions) a).Doc() < ((TermPositions) b).Doc();
			}
		}
		
		private sealed class IntQueue
		{
			public IntQueue()
			{
				InitBlock();
			}
			private void  InitBlock()
			{
				_array = new int[_arraySize];
			}
			private int _arraySize = 16;
			private int _index = 0;
			private int _lastIndex = 0;
			private int[] _array;
			
			internal void  add(int i)
			{
				if (_lastIndex == _arraySize)
					growArray();
				
				_array[_lastIndex++] = i;
			}
			
			internal int next()
			{
				return _array[_index++];
			}
			
			internal void  sort()
			{
				System.Array.Sort(_array, _index, _lastIndex - _index);
			}
			
			internal void  clear()
			{
				_index = 0;
				_lastIndex = 0;
			}
			
			internal int size()
			{
				return (_lastIndex - _index);
			}
			
			private void  growArray()
			{
				int[] newArray = new int[_arraySize * 2];
				Array.Copy(_array, 0, newArray, 0, _arraySize);
				_array = newArray;
				_arraySize *= 2;
			}
		}
		
		private int _doc;
		private int _freq;
		private TermPositionsQueue _termPositionsQueue;
		private IntQueue _posList;
		
		/// <summary> Creates a new <code>MultipleTermPositions</code> instance.
		/// 
		/// </summary>
		/// <exception cref="IOException">
		/// </exception>
		public MultipleTermPositions(IndexReader indexReader, Term[] terms)
		{
			System.Collections.IList termPositions = new System.Collections.ArrayList();
			
			for (int i = 0; i < terms.Length; i++)
				termPositions.Add(indexReader.TermPositions(terms[i]));
			
			_termPositionsQueue = new TermPositionsQueue(termPositions);
			_posList = new IntQueue();
		}
		
		public bool Next()
		{
			if (_termPositionsQueue.Size() == 0)
				return false;
			
			_posList.clear();
			_doc = _termPositionsQueue.Peek().Doc();
			
			TermPositions tp;
			do 
			{
				tp = _termPositionsQueue.Peek();
				
				for (int i = 0; i < tp.Freq(); i++)
					_posList.add(tp.NextPosition());
				
				if (tp.Next())
					_termPositionsQueue.AdjustTop();
				else
				{
					_termPositionsQueue.Pop();
					tp.Close();
				}
			}
			while (_termPositionsQueue.Size() > 0 && _termPositionsQueue.Peek().Doc() == _doc);
			
			_posList.sort();
			_freq = _posList.size();
			
			return true;
		}
		
		public int NextPosition()
		{
			return _posList.next();
		}
		
		public bool SkipTo(int target)
		{
			while (_termPositionsQueue.Peek() != null && target > _termPositionsQueue.Peek().Doc())
			{
				TermPositions tp = (TermPositions) _termPositionsQueue.Pop();
				if (tp.SkipTo(target))
					_termPositionsQueue.Put(tp);
				else
					tp.Close();
			}
			return Next();
		}
		
		public int Doc()
		{
			return _doc;
		}
		
		public int Freq()
		{
			return _freq;
		}
		
		public void  Close()
		{
			while (_termPositionsQueue.Size() > 0)
				((TermPositions) _termPositionsQueue.Pop()).Close();
		}
		
		/// <summary> Not implemented.</summary>
		/// <throws>  UnsupportedOperationException </throws>
		public virtual void  Seek(Term arg0)
		{
			throw new System.NotSupportedException();
		}
		
		/// <summary> Not implemented.</summary>
		/// <throws>  UnsupportedOperationException </throws>
		public virtual void  Seek(TermEnum termEnum)
		{
			throw new System.NotSupportedException();
		}
		
		/// <summary> Not implemented.</summary>
		/// <throws>  UnsupportedOperationException </throws>
		public virtual int Read(int[] arg0, int[] arg1)
		{
			throw new System.NotSupportedException();
		}
		
		
		/// <summary> Not implemented.</summary>
		/// <throws>  UnsupportedOperationException </throws>
		public virtual int GetPayloadLength()
		{
			throw new System.NotSupportedException();
		}
		
		/// <summary> Not implemented.</summary>
		/// <throws>  UnsupportedOperationException </throws>
		public virtual byte[] GetPayload(byte[] data, int offset)
		{
			throw new System.NotSupportedException();
		}
		
		/// <summary> </summary>
		/// <returns> false
		/// </returns>
		// TODO: Remove warning after API has been finalized
		public virtual bool IsPayloadAvailable()
		{
			return false;
		}
	}
}
