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

using DocIdSetIterator = Mono.Lucene.Net.Search.DocIdSetIterator;

namespace Mono.Lucene.Net.Util
{
	
	[Serializable]
	public class OpenBitSetDISI:OpenBitSet
	{
		
		/// <summary>Construct an OpenBitSetDISI with its bits set
		/// from the doc ids of the given DocIdSetIterator.
		/// Also give a maximum size one larger than the largest doc id for which a
		/// bit may ever be set on this OpenBitSetDISI.
		/// </summary>
		public OpenBitSetDISI(DocIdSetIterator disi, int maxSize):base(maxSize)
		{
			InPlaceOr(disi);
		}
		
		/// <summary>Construct an OpenBitSetDISI with no bits set, and a given maximum size
		/// one larger than the largest doc id for which a bit may ever be set
		/// on this OpenBitSetDISI.
		/// </summary>
		public OpenBitSetDISI(int maxSize):base(maxSize)
		{
		}
		
		/// <summary> Perform an inplace OR with the doc ids from a given DocIdSetIterator,
		/// setting the bit for each such doc id.
		/// These doc ids should be smaller than the maximum size passed to the
		/// constructor.
		/// </summary>
		public virtual void  InPlaceOr(DocIdSetIterator disi)
		{
			int doc;
			long size = Size();
			while ((doc = disi.NextDoc()) < size)
			{
				FastSet(doc);
			}
		}
		
		/// <summary> Perform an inplace AND with the doc ids from a given DocIdSetIterator,
		/// leaving only the bits set for which the doc ids are in common.
		/// These doc ids should be smaller than the maximum size passed to the
		/// constructor.
		/// </summary>
		public virtual void  InPlaceAnd(DocIdSetIterator disi)
		{
			int bitSetDoc = NextSetBit(0);
			int disiDoc;
			while (bitSetDoc != - 1 && (disiDoc = disi.Advance(bitSetDoc)) != DocIdSetIterator.NO_MORE_DOCS)
			{
				Clear(bitSetDoc, disiDoc);
				bitSetDoc = NextSetBit(disiDoc + 1);
			}
			if (bitSetDoc != - 1)
			{
				Clear(bitSetDoc, Size());
			}
		}
		
		/// <summary> Perform an inplace NOT with the doc ids from a given DocIdSetIterator,
		/// clearing all the bits for each such doc id.
		/// These doc ids should be smaller than the maximum size passed to the
		/// constructor.
		/// </summary>
		public virtual void  InPlaceNot(DocIdSetIterator disi)
		{
			int doc;
			long size = Size();
			while ((doc = disi.NextDoc()) < size)
			{
				FastClear(doc);
			}
		}
		
		/// <summary> Perform an inplace XOR with the doc ids from a given DocIdSetIterator,
		/// flipping all the bits for each such doc id.
		/// These doc ids should be smaller than the maximum size passed to the
		/// constructor.
		/// </summary>
		public virtual void  InPlaceXor(DocIdSetIterator disi)
		{
			int doc;
			long size = Size();
			while ((doc = disi.NextDoc()) < size)
			{
				FastFlip(doc);
			}
		}
	}
}
