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

using Directory = Mono.Lucene.Net.Store.Directory;

namespace Mono.Lucene.Net.Index
{
	
	/// <summary>A {@link MergeScheduler} that runs each merge using a
	/// separate thread, up until a maximum number of threads
	/// ({@link #setMaxThreadCount}) at which when a merge is
	/// needed, the thread(s) that are updating the index will
	/// pause until one or more merges completes.  This is a
	/// simple way to use concurrency in the indexing process
	/// without having to create and manage application level
	/// threads. 
	/// </summary>
	
	public class ConcurrentMergeScheduler:MergeScheduler
	{
		
		private int mergeThreadPriority = - 1;
		
		protected internal System.Collections.IList mergeThreads = new System.Collections.ArrayList();
		
		// Max number of threads allowed to be merging at once
		private int maxThreadCount = 1;
		
		protected internal Directory dir;
		
		private bool closed;
		protected internal IndexWriter writer;
		protected internal int mergeThreadCount;
		
		public ConcurrentMergeScheduler()
		{
			if (allInstances != null)
			{
				// Only for testing
				AddMyself();
			}
		}
		
		/// <summary>Sets the max # simultaneous threads that may be
		/// running.  If a merge is necessary yet we already have
		/// this many threads running, the incoming thread (that
		/// is calling add/updateDocument) will block until
		/// a merge thread has completed. 
		/// </summary>
		public virtual void  SetMaxThreadCount(int count)
		{
			if (count < 1)
				throw new System.ArgumentException("count should be at least 1");
			maxThreadCount = count;
		}
		
		/// <summary>Get the max # simultaneous threads that may be</summary>
		/// <seealso cref="setMaxThreadCount">
		/// </seealso>
		public virtual int GetMaxThreadCount()
		{
			return maxThreadCount;
		}
		
		/// <summary>Return the priority that merge threads run at.  By
		/// default the priority is 1 plus the priority of (ie,
		/// slightly higher priority than) the first thread that
		/// calls merge. 
		/// </summary>
		public virtual int GetMergeThreadPriority()
		{
			lock (this)
			{
				InitMergeThreadPriority();
				return mergeThreadPriority;
			}
		}
		
		/// <summary>Return the priority that merge threads run at. </summary>
		public virtual void  SetMergeThreadPriority(int pri)
		{
			lock (this)
			{
				if (pri > (int) System.Threading.ThreadPriority.Highest || pri < (int) System.Threading.ThreadPriority.Lowest)
					throw new System.ArgumentException("priority must be in range " + (int) System.Threading.ThreadPriority.Lowest + " .. " + (int) System.Threading.ThreadPriority.Highest + " inclusive");
				mergeThreadPriority = pri;
				
				int numThreads = MergeThreadCount();
				for (int i = 0; i < numThreads; i++)
				{
					MergeThread merge = (MergeThread) mergeThreads[i];
					merge.SetThreadPriority(pri);
				}
			}
		}
		
		private bool Verbose()
		{
			return writer != null && writer.Verbose();
		}
		
		private void  Message(System.String message)
		{
			if (Verbose())
				writer.Message("CMS: " + message);
		}
		
		private void  InitMergeThreadPriority()
		{
			lock (this)
			{
				if (mergeThreadPriority == - 1)
				{
					// Default to slightly higher priority than our
					// calling thread
					mergeThreadPriority = 1 + (System.Int32) SupportClass.ThreadClass.Current().Priority;
					if (mergeThreadPriority > (int) System.Threading.ThreadPriority.Highest)
						mergeThreadPriority = (int) System.Threading.ThreadPriority.Highest;
				}
			}
		}
		
		public override void  Close()
		{
			closed = true;
		}
		
		public virtual void  Sync()
		{
			lock (this)
			{
				while (MergeThreadCount() > 0)
				{
					if (Verbose())
						Message("now wait for threads; currently " + mergeThreads.Count + " still running");
					int count = mergeThreads.Count;
					if (Verbose())
					{
						for (int i = 0; i < count; i++)
							Message("    " + i + ": " + ((MergeThread) mergeThreads[i]));
					}
					
					try
					{
						System.Threading.Monitor.Wait(this);
					}
					catch (System.Threading.ThreadInterruptedException ie)
					{
						// In 3.0 we will change this to throw
						// InterruptedException instead
						SupportClass.ThreadClass.Current().Interrupt();
						throw new System.SystemException(ie.Message, ie);
					}
				}
			}
		}
		
		private int MergeThreadCount()
		{
            lock (this)
            {
                return MergeThreadCount(false);
            }
		}

        private int MergeThreadCount(bool excludeDone)
        {
            lock (this)
            {
                int count = 0;
                int numThreads = mergeThreads.Count;
                for (int i = 0; i < numThreads; i++)
                {
                    MergeThread t = (MergeThread)mergeThreads[i];
                    if (t.IsAlive)
                    {
                        MergePolicy.OneMerge runningMerge = t.GetRunningMerge();
                        if (!excludeDone || (runningMerge != null && !runningMerge.mergeDone))
                        {
                            count++;
                        }
                    }
                }
                return count;
            }
        }
		
		public override void  Merge(IndexWriter writer)
		{
			
			// TODO: enable this once we are on JRE 1.5
			// assert !Thread.holdsLock(writer);
			
			this.writer = writer;
			
			InitMergeThreadPriority();
			
			dir = writer.GetDirectory();
			
			// First, quickly run through the newly proposed merges
			// and add any orthogonal merges (ie a merge not
			// involving segments already pending to be merged) to
			// the queue.  If we are way behind on merging, many of
			// these newly proposed merges will likely already be
			// registered.
			
			if (Verbose())
			{
				Message("now merge");
				Message("  index: " + writer.SegString());
			}
			
			// Iterate, pulling from the IndexWriter's queue of
			// pending merges, until it's empty:
			while (true)
			{
				
				// TODO: we could be careful about which merges to do in
				// the BG (eg maybe the "biggest" ones) vs FG, which
				// merges to do first (the easiest ones?), etc.
				
				MergePolicy.OneMerge merge = writer.GetNextMerge();
				if (merge == null)
				{
					if (Verbose())
						Message("  no more merges pending; now return");
					return ;
				}
				
				// We do this w/ the primary thread to keep
				// deterministic assignment of segment names
				writer.MergeInit(merge);
				
				bool success = false;
				try
				{
					lock (this)
					{
						MergeThread merger;
						while (MergeThreadCount(true) >= maxThreadCount)
						{
							if (Verbose())
								Message("    too many merge threads running; stalling...");
							try
							{
								System.Threading.Monitor.Wait(this);
							}
							catch (System.Threading.ThreadInterruptedException ie)
							{
								// In 3.0 we will change this to throw
								// InterruptedException instead
								SupportClass.ThreadClass.Current().Interrupt();
								throw new System.SystemException(ie.Message, ie);
							}
						}
						
						if (Verbose())
							Message("  consider merge " + merge.SegString(dir));
						
												
						// OK to spawn a new merge thread to handle this
						// merge:
						merger = GetMergeThread(writer, merge);
						mergeThreads.Add(merger);
						if (Verbose())
							Message("    launch new thread [" + merger.Name + "]");
						
						merger.Start();
						success = true;
					}
				}
				finally
				{
					if (!success)
					{
						writer.MergeFinish(merge);
					}
				}
			}
		}
		
		/// <summary>Does the actual merge, by calling {@link IndexWriter#merge} </summary>
		protected internal virtual void  DoMerge(MergePolicy.OneMerge merge)
		{
			writer.Merge(merge);
		}
		
		/// <summary>Create and return a new MergeThread </summary>
		protected internal virtual MergeThread GetMergeThread(IndexWriter writer, MergePolicy.OneMerge merge)
		{
			lock (this)
			{
				MergeThread thread = new MergeThread(this, writer, merge);
				thread.SetThreadPriority(mergeThreadPriority);
				thread.IsBackground = true;
				thread.Name = "Lucene Merge Thread #" + mergeThreadCount++;
				return thread;
			}
		}
		
		public /*protected internal*/ class MergeThread:SupportClass.ThreadClass
		{
			private void  InitBlock(ConcurrentMergeScheduler enclosingInstance)
			{
				this.enclosingInstance = enclosingInstance;
			}
			private ConcurrentMergeScheduler enclosingInstance;
			public ConcurrentMergeScheduler Enclosing_Instance
			{
				get
				{
					return enclosingInstance;
				}
				
			}
			
			internal IndexWriter writer;
			internal MergePolicy.OneMerge startMerge;
			internal MergePolicy.OneMerge runningMerge;
			
			public MergeThread(ConcurrentMergeScheduler enclosingInstance, IndexWriter writer, MergePolicy.OneMerge startMerge)
			{
				InitBlock(enclosingInstance);
				this.writer = writer;
				this.startMerge = startMerge;
			}
			
			public virtual void  SetRunningMerge(MergePolicy.OneMerge merge)
			{
				lock (this)
				{
					runningMerge = merge;
				}
			}
			
			public virtual MergePolicy.OneMerge GetRunningMerge()
			{
				lock (this)
				{
					return runningMerge;
				}
			}
			
			public virtual void  SetThreadPriority(int pri)
			{
				try
				{
					Priority = (System.Threading.ThreadPriority) pri;
				}
				catch (System.NullReferenceException npe)
				{
					// Strangely, Sun's JDK 1.5 on Linux sometimes
					// throws NPE out of here...
				}
				catch (System.Security.SecurityException se)
				{
					// Ignore this because we will still run fine with
					// normal thread priority
				}
			}
			
			override public void  Run()
			{
				
				// First time through the while loop we do the merge
				// that we were started with:
				MergePolicy.OneMerge merge = this.startMerge;
				
				try
				{
					
					if (Enclosing_Instance.Verbose())
						Enclosing_Instance.Message("  merge thread: start");
					
					while (true)
					{
						SetRunningMerge(merge);
						Enclosing_Instance.DoMerge(merge);
						
						// Subsequent times through the loop we do any new
						// merge that writer says is necessary:
						merge = writer.GetNextMerge();
						if (merge != null)
						{
							writer.MergeInit(merge);
							if (Enclosing_Instance.Verbose())
								Enclosing_Instance.Message("  merge thread: do another merge " + merge.SegString(Enclosing_Instance.dir));
						}
						else
							break;
					}
					
					if (Enclosing_Instance.Verbose())
						Enclosing_Instance.Message("  merge thread: done");
				}
				catch (System.Exception exc)
				{
					
					// Ignore the exception if it was due to abort:
					if (!(exc is MergePolicy.MergeAbortedException))
					{
						if (!Enclosing_Instance.suppressExceptions)
						{
							// suppressExceptions is normally only set during
							// testing.
							Mono.Lucene.Net.Index.ConcurrentMergeScheduler.anyExceptions = true;
							Enclosing_Instance.HandleMergeException(exc);
						}
					}
				}
				finally
				{
					lock (Enclosing_Instance)
					{
						System.Threading.Monitor.PulseAll(Enclosing_Instance);
						Enclosing_Instance.mergeThreads.Remove(this);
                        bool removed = !Enclosing_Instance.mergeThreads.Contains(this);
						System.Diagnostics.Debug.Assert(removed);
					}
				}
			}
			
			public override System.String ToString()
			{
				MergePolicy.OneMerge merge = GetRunningMerge();
				if (merge == null)
					merge = startMerge;
				return "merge thread: " + merge.SegString(Enclosing_Instance.dir);
			}
		}
		
		/// <summary>Called when an exception is hit in a background merge
		/// thread 
		/// </summary>
		protected internal virtual void  HandleMergeException(System.Exception exc)
		{
			try
			{
				// When an exception is hit during merge, IndexWriter
				// removes any partial files and then allows another
				// merge to run.  If whatever caused the error is not
				// transient then the exception will keep happening,
				// so, we sleep here to avoid saturating CPU in such
				// cases:
				System.Threading.Thread.Sleep(new System.TimeSpan((System.Int64) 10000 * 250));
			}
			catch (System.Threading.ThreadInterruptedException ie)
			{
				SupportClass.ThreadClass.Current().Interrupt();
				// In 3.0 this will throw InterruptedException
				throw new System.SystemException(ie.Message, ie);
			}
			throw new MergePolicy.MergeException(exc, dir);
		}
		
		internal static bool anyExceptions = false;
		
		/// <summary>Used for testing </summary>
		public static bool AnyUnhandledExceptions()
		{
			if (allInstances == null)
			{
				throw new System.SystemException("setTestMode() was not called; often this is because your test case's setUp method fails to call super.setUp in LuceneTestCase");
			}
			lock (allInstances.SyncRoot)
			{
				int count = allInstances.Count;
				// Make sure all outstanding threads are done so we see
				// any exceptions they may produce:
				for (int i = 0; i < count; i++)
					((ConcurrentMergeScheduler) allInstances[i]).Sync();
				bool v = anyExceptions;
				anyExceptions = false;
				return v;
			}
		}
		
		public static void  ClearUnhandledExceptions()
		{
			lock (allInstances.SyncRoot)
			{
				anyExceptions = false;
			}
		}
		
		/// <summary>Used for testing </summary>
		private void  AddMyself()
		{
			lock (allInstances.SyncRoot)
			{
				int size = allInstances.Count;
				int upto = 0;
				for (int i = 0; i < size; i++)
				{
					ConcurrentMergeScheduler other = (ConcurrentMergeScheduler) allInstances[i];
					if (!(other.closed && 0 == other.MergeThreadCount()))
					// Keep this one for now: it still has threads or
					// may spawn new threads
						allInstances[upto++] = other;
				}
				((System.Collections.IList) ((System.Collections.ArrayList) allInstances).GetRange(upto, allInstances.Count - upto)).Clear();
				allInstances.Add(this);
			}
		}
		
		private bool suppressExceptions;
		
		/// <summary>Used for testing </summary>
		public /*internal*/ virtual void  SetSuppressExceptions()
		{
			suppressExceptions = true;
		}
		
		/// <summary>Used for testing </summary>
		public /*internal*/ virtual void  ClearSuppressExceptions()
		{
			suppressExceptions = false;
		}
		
		/// <summary>Used for testing </summary>
		private static System.Collections.IList allInstances;
		public static void  SetTestMode()
		{
			allInstances = new System.Collections.ArrayList();
		}
	}
}
