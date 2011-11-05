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

namespace Mono.Lucene.Net.Search
{
	
	/// <summary> <p/>
	/// The TimeLimitedCollector is used to timeout search requests that take longer
	/// than the maximum allowed search time limit. After this time is exceeded, the
	/// search thread is stopped by throwing a TimeExceeded Exception.
	/// <p/>
	/// 
	/// </summary>
	/// <deprecated> Use {@link TimeLimitingCollector} instead, which extends the new
	/// {@link Collector}. This class will be removed in 3.0.
	/// </deprecated>
    [Obsolete("Use TimeLimitingCollector instead, which extends the new Collector. This class will be removed in 3.0.")]
	public class TimeLimitedCollector:HitCollector
	{
		private void  InitBlock()
		{
			greedy = DEFAULT_GREEDY;
		}
		
		/// <summary> Default timer resolution.</summary>
		/// <seealso cref="SetResolution(long)">
		/// </seealso>
		public const int DEFAULT_RESOLUTION = 20;
		
		/// <summary> Default for {@link #IsGreedy()}.</summary>
		/// <seealso cref="IsGreedy()">
		/// </seealso>
		public bool DEFAULT_GREEDY = false;
		
		private static uint resolution = DEFAULT_RESOLUTION;
		
		private bool greedy;
		
		private class TimerThread:SupportClass.ThreadClass
		{
			
			// NOTE: we can avoid explicit synchronization here for several reasons:
			// * updates to volatile long variables are atomic
			// * only single thread modifies this value
			// * use of volatile keyword ensures that it does not reside in
			//   a register, but in main memory (so that changes are visible to
			//   other threads).
			// * visibility of changes does not need to be instantanous, we can
			//   afford losing a tick or two.
			//
			// See section 17 of the Java Language Specification for details.
			private volatile uint time = 0;
			
			/// <summary> TimerThread provides a pseudo-clock service to all searching
			/// threads, so that they can count elapsed time with less overhead
			/// than repeatedly calling System.currentTimeMillis.  A single
			/// thread should be created to be used for all searches.
			/// </summary>
			internal TimerThread():base("TimeLimitedCollector timer thread")
			{
				this.IsBackground = true;
			}
			
			override public void  Run()
			{
				while (true)
				{
					// TODO: Use System.nanoTime() when Lucene moves to Java SE 5.
					time += Mono.Lucene.Net.Search.TimeLimitedCollector.resolution;
					try
					{
						System.Threading.Thread.Sleep(new System.TimeSpan((System.Int64) 10000 * Mono.Lucene.Net.Search.TimeLimitedCollector.resolution));
					}
					catch (System.Threading.ThreadInterruptedException ie)
					{
						SupportClass.ThreadClass.Current().Interrupt();
						throw new System.SystemException(ie.Message, ie);
					}
				}
			}
			
			/// <summary> Get the timer value in milliseconds.</summary>
			public virtual long GetMilliseconds()
			{
				return time;
			}
		}
		
		/// <summary> Thrown when elapsed search time exceeds allowed search time. </summary>
		[Serializable]
		public class TimeExceededException:System.SystemException
		{
			private long timeAllowed;
			private long timeElapsed;
			private int lastDocCollected;
			internal TimeExceededException(long timeAllowed, long timeElapsed, int lastDocCollected):base("Elapsed time: " + timeElapsed + "Exceeded allowed search time: " + timeAllowed + " ms.")
			{
				this.timeAllowed = timeAllowed;
				this.timeElapsed = timeElapsed;
				this.lastDocCollected = lastDocCollected;
			}
			/// <summary> Returns allowed time (milliseconds).</summary>
			public virtual long GetTimeAllowed()
			{
				return timeAllowed;
			}
			/// <summary> Returns elapsed time (milliseconds).</summary>
			public virtual long GetTimeElapsed()
			{
				return timeElapsed;
			}
			/// <summary> Returns last doc that was collected when the search time exceeded.  </summary>
			public virtual int GetLastDocCollected()
			{
				return lastDocCollected;
			}
		}
		
		// Declare and initialize a single static timer thread to be used by
		// all TimeLimitedCollector instances.  The JVM assures that
		// this only happens once.
		private static readonly TimerThread TIMER_THREAD = new TimerThread();
		
		private long t0;
		private long timeout;
		private HitCollector hc;
		
		/// <summary> Create a TimeLimitedCollector wrapper over another HitCollector with a specified timeout.</summary>
		/// <param name="hc">the wrapped HitCollector
		/// </param>
		/// <param name="timeAllowed">max time allowed for collecting hits after which {@link TimeExceededException} is thrown
		/// </param>
		public TimeLimitedCollector(HitCollector hc, long timeAllowed)
		{
			InitBlock();
			this.hc = hc;
			t0 = TIMER_THREAD.GetMilliseconds();
			this.timeout = t0 + timeAllowed;
		}
		
		/// <summary> Calls collect() on the decorated HitCollector.
		/// 
		/// </summary>
		/// <throws>  TimeExceededException if the time allowed has been exceeded. </throws>
		public override void  Collect(int doc, float score)
		{
			long time = TIMER_THREAD.GetMilliseconds();
			if (timeout < time)
			{
				if (greedy)
				{
					//System.out.println(this+"  greedy: before failing, collecting doc: "+doc+"  "+(time-t0));
					hc.Collect(doc, score);
				}
				//System.out.println(this+"  failing on:  "+doc+"  "+(time-t0));
				throw new TimeExceededException(timeout - t0, time - t0, doc);
			}
			//System.out.println(this+"  collecting: "+doc+"  "+(time-t0));
			hc.Collect(doc, score);
		}
		
		/// <summary> Return the timer resolution.</summary>
		/// <seealso cref="SetResolution(long)">
		/// </seealso>
		public static long GetResolution()
		{
			return resolution;
		}
		
		/// <summary> Set the timer resolution.
		/// The default timer resolution is 20 milliseconds. 
		/// This means that a search required to take no longer than 
		/// 800 milliseconds may be stopped after 780 to 820 milliseconds.
		/// <br/>Note that: 
		/// <ul>
		/// <li>Finer (smaller) resolution is more accurate but less efficient.</li>
		/// <li>Setting resolution to less than 5 milliseconds will be silently modified to 5 milliseconds.</li>
		/// <li>Setting resolution smaller than current resolution might take effect only after current 
		/// resolution. (Assume current resolution of 20 milliseconds is modified to 5 milliseconds, 
		/// then it can take up to 20 milliseconds for the change to have effect.</li>
		/// </ul>      
		/// </summary>
		public static void  SetResolution(uint newResolution)
		{
			resolution = System.Math.Max(newResolution, 5); // 5 milliseconds is about the minimum reasonable time for a Object.wait(long) call.
		}
		
		/// <summary> Checks if this time limited collector is greedy in collecting the last hit.
		/// A non greedy collector, upon a timeout, would throw a {@link TimeExceededException} 
		/// without allowing the wrapped collector to collect current doc. A greedy one would 
		/// first allow the wrapped hit collector to collect current doc and only then 
		/// throw a {@link TimeExceededException}.
		/// </summary>
		/// <seealso cref="SetGreedy(boolean)">
		/// </seealso>
		public virtual bool IsGreedy()
		{
			return greedy;
		}
		
		/// <summary> Sets whether this time limited collector is greedy.</summary>
		/// <param name="greedy">true to make this time limited greedy
		/// </param>
		/// <seealso cref="IsGreedy()">
		/// </seealso>
		public virtual void  SetGreedy(bool greedy)
		{
			this.greedy = greedy;
		}
		static TimeLimitedCollector()
		{
			{
				TIMER_THREAD.Start();
			}
		}
	}
}
