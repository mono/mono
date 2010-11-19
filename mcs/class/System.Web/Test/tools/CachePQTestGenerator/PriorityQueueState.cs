//
//  PriorityQueueState.cs
//
//  Author:
//    Marek Habersack <grendel@twistedcode.net>
//
//  Copyright (c) 2010, Marek Habersack
//
//  All rights reserved.
//
//  Redistribution and use in source and binary forms, with or without modification, are permitted
//  provided that the following conditions are met:
//
//     * Redistributions of source code must retain the above copyright notice, this list of
//       conditions and the following disclaimer.
//     * Redistributions in binary form must reproduce the above copyright notice, this list of
//       conditions and the following disclaimer in the documentation and/or other materials
//       provided with the distribution.
//     * Neither the name of Marek Habersack nor the names of its contributors may be used to
//       endorse or promote products derived from this software without specific prior written
//       permission.
//
//  THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
//  "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
//  LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
//  A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR
//  CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL,
//  EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
//  PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR
//  PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
//  LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
//  NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
//  SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
//
using System;
using System.Collections.Generic;
using System.Web.Caching;

using BenTools.Data;

namespace Tester
{
	class PriorityQueueState
	{
		public readonly BinaryPriorityQueue Queue;
		public readonly string ListName;
		public readonly string TestsName;
		
		public int EnqueueCount;
		public int DequeueCount;
		public int DisableCount;
		public int PeekCount;
		
		public PriorityQueueState (string listName, string testsName)
		{
			Queue = new BinaryPriorityQueue (new CacheItemComparer ());
			EnqueueCount = 0;
			DequeueCount = 0;
			DisableCount = 0;
			PeekCount = 0;
			ListName = listName;
			TestsName = testsName;
		}

		public void Enqueue (CacheItem item)
		{
			Queue.Push (item);
		}

		public CacheItem Dequeue ()
		{
			return Queue.Pop () as CacheItem;
		}

		public CacheItem Peek ()
		{
			return Queue.Peek () as CacheItem;
		}
	}
}
