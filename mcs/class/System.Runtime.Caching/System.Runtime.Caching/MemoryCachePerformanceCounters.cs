//
// MemoryCache.cs
//
// Authors:
//      Marek Habersack <mhabersack@novell.com>
//
// Copyright (C) 2010 Novell, Inc. (http://novell.com/)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

//
// Counters in the ".NET Memory Cache 4.0" are not documented on MSDN. They were discovered using
// perfmon there their definition may change without any notice
//
namespace System.Runtime.Caching
{
	sealed class MemoryCachePerformanceCounters : IDisposable
	{
		const string dotNetCategoryName = ".NET Memory Cache 4.0";

		public const int CACHE_ENTRIES = 0;
		public const int CACHE_HIT_RATIO = 1;
		public const int CACHE_HITS = 2;
		public const int CACHE_MISSES = 3;
		public const int CACHE_TRIMS = 4;
		public const int CACHE_TURNOVER_RATE = 5;
		const int COUNTERS_LAST = CACHE_TURNOVER_RATE;
		
		PerformanceCounter[] perfCounters;

		public MemoryCachePerformanceCounters (string instanceName, bool noCounters)
		{
			var collection = new CounterCreationDataCollection ();

			if (!noCounters) {
				if (!PerformanceCounterCategory.Exists (dotNetCategoryName)) {
					// TODO: check:
					//
					//  - types of all the counters
					//
					CreateCounter ("Cache Entries", PerformanceCounterType.NumberOfItems64, collection);
					CreateCounter ("Cache Hit Ratio", PerformanceCounterType.RawFraction, collection);
					CreateCounter ("Cache Hits", PerformanceCounterType.NumberOfItems64, collection);
					CreateCounter ("Cache Misses", PerformanceCounterType.NumberOfItems64, collection);
					CreateCounter ("Cache Trims", PerformanceCounterType.NumberOfItems64, collection);
					CreateCounter ("Cache Turnover Rate", PerformanceCounterType.RateOfCountsPerSecond64, collection);
			
					PerformanceCounterCategory.Create (dotNetCategoryName, "System.Runtime.Caching.MemoryCache Performance Counters",
									   PerformanceCounterCategoryType.MultiInstance, collection);
				}
				
				perfCounters = new PerformanceCounter [COUNTERS_LAST + 1];
				perfCounters [CACHE_ENTRIES] = new PerformanceCounter (dotNetCategoryName, "Cache Entries", instanceName, false);
				perfCounters [CACHE_ENTRIES].RawValue = 0;
				perfCounters [CACHE_HIT_RATIO] = new PerformanceCounter (dotNetCategoryName, "Cache Hit Ratio", instanceName, false);
				perfCounters [CACHE_HIT_RATIO].RawValue = 0;
				perfCounters [CACHE_HITS] = new PerformanceCounter (dotNetCategoryName, "Cache Hits", instanceName, false);
				perfCounters [CACHE_HITS].RawValue = 0;
				perfCounters [CACHE_MISSES] = new PerformanceCounter (dotNetCategoryName, "Cache Misses", instanceName, false);
				perfCounters [CACHE_MISSES].RawValue = 0;
				perfCounters [CACHE_TRIMS] = new PerformanceCounter (dotNetCategoryName, "Cache Trims", instanceName, false);
				perfCounters [CACHE_TRIMS].RawValue = 0;
				perfCounters [CACHE_TURNOVER_RATE] = new PerformanceCounter (dotNetCategoryName, "Cache Turnover Rate", instanceName, false);
				perfCounters [CACHE_TURNOVER_RATE].RawValue = 0;
			}
		}

		public void Dispose ()
		{
			foreach (PerformanceCounter counter in perfCounters) {
				if (counter == null)
					continue;

				counter.Dispose ();
			}
		}
		
		public void Decrement (int counteridx)
		{
			if (perfCounters == null || counteridx < 0 || counteridx > COUNTERS_LAST)
				return;

			perfCounters [counteridx].Decrement ();
		}
		
		public void Increment (int counteridx)
		{
			if (perfCounters == null || counteridx < 0 || counteridx > COUNTERS_LAST)
				return;

			perfCounters [counteridx].Increment ();
		}
		
		void CreateCounter (string name, PerformanceCounterType type, CounterCreationDataCollection collection)
		{
			var ccd = new CounterCreationData ();

			ccd.CounterName = name;
			ccd.CounterType = type;
			collection.Add (ccd);
		}
	}
}
