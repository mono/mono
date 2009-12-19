#if NET_4_0
// Parallel.cs
//
// Copyright (c) 2008 Jérémie "Garuma" Laval
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//
//

using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace System.Threading
{
	public static class Parallel
	{
		internal static int GetBestWorkerNumber ()
		{
			return GetBestWorkerNumber (TaskScheduler.Current);
		}
		
		internal static int GetBestWorkerNumber (TaskScheduler scheduler)
		{	
			return scheduler.MaximumConcurrencyLevel;
		}
		
		static void HandleExceptions (IEnumerable<Task> tasks)
		{
			HandleExceptions (tasks, null);
		}
		
		static void HandleExceptions (IEnumerable<Task> tasks, ParallelLoopState.ExternalInfos infos)
		{
			List<Exception> exs = new List<Exception> ();
			foreach (Task t in tasks) {
				if (t.Exception != null && !(t.Exception is TaskCanceledException))
					exs.Add (t.Exception);
			}
			
			if (exs.Count > 0) {
				if (infos != null)
					infos.IsExceptional = true;
				
				throw new AggregateException (exs);
			}
		}
		
		static void InitTasks (Task[] tasks, Action action, int count)
		{
			InitTasks (tasks, count, () => Task.Factory.StartNew (action, TaskCreationOptions.DetachedFromParent));
		}
		
		static void InitTasks (Task[] tasks, Action action, int count, TaskScheduler scheduler)
		{
			InitTasks (tasks, count, () => Task.Factory.StartNew (action, TaskCreationOptions.DetachedFromParent, scheduler));
		}
		
		static void InitTasks (Task[] tasks, int count, Func<Task> taskCreator)
		{
			for (int i = 0; i < count; i++) {
				tasks [i] = taskCreator ();
			}
		}
		#region For
		
		public static ParallelLoopResult For (int from, int to, Action<int> action)
		{
			return For (from, to, null, action);
		}
		
		public static ParallelLoopResult For (int from, int to, Action<int, ParallelLoopState> action)
		{
			return For (from, to, null, action);
		}
		
		public static ParallelLoopResult For (int from, int to, ParallelOptions options, Action<int> action)
		{
			return For (from, to, options, (index, state) => action (index));
		}
		
		public static ParallelLoopResult For (int from, int to, ParallelOptions options, Action<int, ParallelLoopState> action)
		{
			return For<object> (from, to, options, null, (i, s, l) => { action (i, s); return null; }, null);
		}
		
		public static ParallelLoopResult For<TLocal> (int from, int to, Func<TLocal> init,
		                                              Func<int, ParallelLoopState, TLocal, TLocal> action, Action<TLocal> destruct)
		{
			return For<TLocal> (from, to, null, init, action, destruct);
		}
		
		public static ParallelLoopResult For<TLocal> (int from, int to, ParallelOptions options, 
		                                              Func<TLocal> init, 
		                                              Func<int, ParallelLoopState, TLocal, TLocal> action,
		                                              Action<TLocal> destruct)
		{			
			if (action == null)
				throw new ArgumentNullException ("action");
			
			// Number of task to be launched (normally == Env.ProcessorCount)
			int num = Math.Min (GetBestWorkerNumber (), 
			                    options != null && options.MaxDegreeOfParallelism != -1 ? options.MaxDegreeOfParallelism : int.MaxValue);
			// Integer range that each task process
			int step = Math.Min (5, (to - from) / num);
			if (step <= 0)
				step = 1;

			// Each worker put the indexes it's responsible for here
			// so that other worker may steal if they starve.
			ConcurrentBag<int> bag = new ConcurrentBag<int> ();
			Task[] tasks = new Task [num];
			ParallelLoopState.ExternalInfos infos = new ParallelLoopState.ExternalInfos ();
			
			int currentIndex = from;
		
			Action workerMethod = delegate {
				int index, actual;
				TLocal local = (init == null) ? default (TLocal) : init ();
				
				ParallelLoopState state = new ParallelLoopState (tasks, infos);
				
				try {
					while ((index = Interlocked.Add (ref currentIndex, step) - step) < to) {
						if (infos.IsStopped.Value)
							return;
						
						if (options != null && options.CancellationToken.IsCancellationRequested) {
							state.Stop ();
							return;
						}
						
						for (int i = index; i < to && i < index + step; i++)
							bag.Add (i);
						
						for (int i = index; i < to && i < index + step && bag.TryTake (out actual); i++) {
							if (infos.IsStopped.Value)
								return;
							
							if (options != null && options.CancellationToken.IsCancellationRequested) {
								state.Stop ();
								return;
							}
							
							if (infos.LowestBreakIteration != null && infos.LowestBreakIteration > actual)
								return;
							
							state.CurrentIteration = actual;
							local = action (actual, state, local);
						}
					}
					
					while (bag.TryTake (out actual)) {
						if (infos.IsStopped.Value)
							return;
						
						if (options != null && options.CancellationToken.IsCancellationRequested) {
							state.Stop ();
							return;
						}
						
						if (infos.LowestBreakIteration != null && infos.LowestBreakIteration > actual)
							continue;
						
						state.CurrentIteration = actual;
						local = action (actual, state, local);
					}
				} finally {
					if (destruct != null)
						destruct (local);
				}
			};
		
			if (options != null && options.TaskScheduler != null)
				InitTasks (tasks, workerMethod, num, options.TaskScheduler);
			else
				InitTasks (tasks, workerMethod, num);
			
			try {
				Task.WaitAll (tasks);
			} catch {
				HandleExceptions (tasks, infos);
			}
			
			return new ParallelLoopResult (infos.LowestBreakIteration, !(infos.IsStopped.Value || infos.IsExceptional));
		}

		#endregion
		
		#region Foreach
		
		static ParallelLoopResult ForEach<TSource, TLocal> (Func<int, IList<IEnumerator<TSource>>> enumerable, ParallelOptions options,
		                                                    Func<TLocal> init, Func<TSource, ParallelLoopState, TLocal, TLocal> action,
		                                                    Action<TLocal> destruct)
		{		
			int num = Math.Min (GetBestWorkerNumber (),
			                    options != null && options.MaxDegreeOfParallelism != -1 ? options.MaxDegreeOfParallelism : int.MaxValue);
			
			Task[] tasks = new Task[num];
			ParallelLoopState.ExternalInfos infos = new ParallelLoopState.ExternalInfos ();
			
			ConcurrentBag<TSource> bag = new ConcurrentBag<TSource> ();
			const int bagCount = 5;
			
			IList<IEnumerator<TSource>> slices = enumerable (num);
			int sliceIndex = 0;

			Action workerMethod = delegate {
				IEnumerator<TSource> slice = slices[Interlocked.Increment (ref sliceIndex) - 1];
				
				TLocal local = (init != null) ? init () : default (TLocal);
				ParallelLoopState state = new ParallelLoopState (tasks, infos);
				
				try {
					bool cont = true;
					TSource element;
					
					while (cont) {
						if (infos.IsStopped.Value)
							return;
						
						if (options != null && options.CancellationToken.IsCancellationRequested) {
							state.Stop ();
							return;
						}
						
						for (int i = 0; i < bagCount && (cont = slice.MoveNext ()); i++) {
							bag.Add (slice.Current);
						}
						
						for (int i = 0; i < bagCount && bag.TryTake (out element); i++) {
							if (infos.IsStopped.Value)
								return;
							
							if (options != null && options.CancellationToken.IsCancellationRequested) {
								state.Stop ();
								return;
							}
							
							local = action (element, state, local);
						}
					}
					
					while (bag.TryTake (out element)) {
						if (infos.IsStopped.Value)
							return;
						
						if (options != null && options.CancellationToken.IsCancellationRequested) {
							state.Stop ();
							return;
						}
						
						local = action (element, state, local);
					}
				} finally {
					if (destruct != null)
						destruct (local);
				}
			};
			
			if (options != null && options.TaskScheduler != null)
				InitTasks (tasks, workerMethod, num, options.TaskScheduler);
			else
				InitTasks (tasks, workerMethod, num);
			
			try {
				Task.WaitAll (tasks);
			} catch {
				HandleExceptions (tasks, infos);
			}
			
			return new ParallelLoopResult (infos.LowestBreakIteration, !(infos.IsStopped.Value || infos.IsExceptional));
		}
		
		public static ParallelLoopResult ForEach<TSource> (IEnumerable<TSource> enumerable, Action<TSource> action)
		{
			return ForEach<TSource, object> (Partitioner.Create (enumerable), ParallelOptions.Default, null, 
			                                 (e, s, l) => { action (e); return null; }, null);
		}
		
		public static ParallelLoopResult ForEach<TSource> (IEnumerable<TSource> enumerable, Action<TSource, ParallelLoopState> action)
		{
			return ForEach<TSource, object> (Partitioner.Create (enumerable), ParallelOptions.Default, null,
			                                 (e, s, l) => { action (e, s); return null; }, null);
		}
		
		public static ParallelLoopResult ForEach<TSource> (IEnumerable<TSource> enumerable,
		                                                   Action<TSource, ParallelLoopState, long> action)
		{
			return ForEach<TSource, object> (Partitioner.Create (enumerable), ParallelOptions.Default, null,
			                                 (e, s, l) => { action (e, s, -1); return null; }, null);
		}
		
		public static ParallelLoopResult ForEach<TSource> (Partitioner<TSource> source,
		                                                   Action<TSource, ParallelLoopState> body)
		{
			return ForEach<TSource, object> (source, ParallelOptions.Default, null, (e, s, l) => { body (e, s); return null; }, null);
		}
		
		public static ParallelLoopResult ForEach<TSource> (OrderablePartitioner<TSource> source, 
		                                                   Action<TSource, ParallelLoopState, long> body)

		{
			return ForEach<TSource, object> (source, ParallelOptions.Default, null, (e, s, i, l) => { body (e, s, i); return null; }, null);
		}
		
		public static ParallelLoopResult ForEach<TSource> (Partitioner<TSource> source,
		                                                   Action<TSource> body)

		{
			return ForEach<TSource, object> (source, ParallelOptions.Default, null, (e, s, l) => { body (e); return null; }, null);
		}
		
		public static ParallelLoopResult ForEach<TSource> (IEnumerable<TSource> source, ParallelOptions parallelOptions,
		                                                   Action<TSource> body)
		{
			return ForEach<TSource, object> (Partitioner.Create (source), parallelOptions, null,
			                                 (e, s, l) => { body (e); return null; }, null);
		}
		
		public static ParallelLoopResult ForEach<TSource> (IEnumerable<TSource> source, ParallelOptions parallelOptions,
		                                                   Action<TSource, ParallelLoopState> body)
		{
			return ForEach<TSource, object> (Partitioner.Create (source), parallelOptions, null, 
			                                 (e, s, l) => { body (e, s); return null; }, null);
		}
		
		public static ParallelLoopResult ForEach<TSource> (IEnumerable<TSource> source, ParallelOptions parallelOptions,
		                                                   Action<TSource, ParallelLoopState, long> body)
		{
			return ForEach<TSource, object> (Partitioner.Create (source), parallelOptions,
			                                 null, (e, s, i, l) => { body (e, s, i); return null; }, null);
		}
		
		public static ParallelLoopResult ForEach<TSource> (OrderablePartitioner<TSource> source, ParallelOptions parallelOptions,
		                                                   Action<TSource, ParallelLoopState, long> body)

		{
			return ForEach<TSource, object> (source, parallelOptions, null, (e, s, i, l) => { body (e, s, i); return null; }, null);
		}
		
		public static ParallelLoopResult ForEach<TSource> (Partitioner<TSource> source, ParallelOptions parallelOptions,
		                                                   Action<TSource> body)
		{
			return ForEach<TSource, object> (source, parallelOptions, null, (e, s, l) => {body (e); return null; }, null);
		}
		
		public static ParallelLoopResult ForEach<TSource> (Partitioner<TSource> source, ParallelOptions parallelOptions, 
		                                                   Action<TSource, ParallelLoopState> body)
		{
			return ForEach<TSource, object> (source, parallelOptions, null, (e, s, l) => { body (e, s); return null; }, null);
		}
		
		public static ParallelLoopResult ForEach<TSource, TLocal> (IEnumerable<TSource> source, Func<TLocal> localInit,
		                                                           Func<TSource, ParallelLoopState, TLocal, TLocal> body,
		                                                           Action<TLocal> localFinally)
		{
			return ForEach<TSource, TLocal> ((Partitioner<TSource>)Partitioner.Create (source), null, localInit, body, localFinally);
		}
		
		public static ParallelLoopResult ForEach<TSource, TLocal> (IEnumerable<TSource> source, Func<TLocal> localInit,
		                                                           Func<TSource, ParallelLoopState, long, TLocal, TLocal> body,
		                                                           Action<TLocal> localFinally)
		{
			return ForEach<TSource, TLocal> (Partitioner.Create (source), null, localInit, body, localFinally);
		}
		
		public static ParallelLoopResult ForEach<TSource, TLocal> (OrderablePartitioner<TSource> source, Func<TLocal> localInit,
		                                                           Func<TSource, ParallelLoopState, long, TLocal, TLocal> body,
		                                                           Action<TLocal> localFinally)
		{
			return ForEach<TSource, TLocal> (source, ParallelOptions.Default, localInit, body, localFinally);
		}
		
		public static ParallelLoopResult ForEach<TSource, TLocal> (Partitioner<TSource> source, Func<TLocal> localInit,
		                                                           Func<TSource, ParallelLoopState, TLocal, TLocal> body,
		                                                           Action<TLocal> localFinally)
		{
			return ForEach<TSource, TLocal> (source, ParallelOptions.Default, localInit, body, localFinally);
		}
		
		public static ParallelLoopResult ForEach<TSource, TLocal> (IEnumerable<TSource> source, ParallelOptions parallelOptions,
		                                                           Func<TLocal> localInit,
		                                                           Func<TSource, ParallelLoopState, TLocal, TLocal> body,
		                                                           Action<TLocal> localFinally)
		{
			return ForEach<TSource, TLocal> (Partitioner.Create (source), parallelOptions, localInit, body, localFinally);
		}
		
		public static ParallelLoopResult ForEach<TSource, TLocal> (IEnumerable<TSource> source, ParallelOptions parallelOptions,
		                                                           Func<TLocal> localInit, 
		                                                           Func<TSource, ParallelLoopState, long, TLocal, TLocal> body,
		                                                           Action<TLocal> localFinally)
		{
			return ForEach<TSource, TLocal> (Partitioner.Create (source), parallelOptions, localInit, body, localFinally);
		}
		
		public static ParallelLoopResult ForEach<TSource, TLocal> (Partitioner<TSource> enumerable, ParallelOptions options,
		                                                           Func<TLocal> init,
		                                                           Func<TSource, ParallelLoopState, TLocal, TLocal> action,
		                                                           Action<TLocal> destruct)
		{
			return ForEach<TSource, TLocal> (enumerable.GetPartitions, options, init, action, destruct);
		}
			
		public static ParallelLoopResult ForEach<TSource, TLocal> (OrderablePartitioner<TSource> enumerable, ParallelOptions options,
		                                                           Func<TLocal> init,
		                                                           Func<TSource, ParallelLoopState, long, TLocal, TLocal> action,
		                                                           Action<TLocal> destruct)
		{
			return ForEach<KeyValuePair<long, TSource>, TLocal> (enumerable.GetOrderablePartitions, options,
			                                                    init, (e, s, l) => action (e.Value, s, e.Key, l), destruct);
		}
		#endregion
		
		/* Disabled as this is an API addition
		#region While		
		public static void While (Func<bool> predicate, Action body)
		{
			if (body == null)
				throw new ArgumentNullException ("body");
			if (predicate == null)
				throw new ArgumentNullException ("predicate");
			
			int num = GetBestWorkerNumber ();
			
			Task[] tasks = new Task [num];
			
			Action action = delegate {
				while (predicate ())
				body ();
			};
			
			InitTasks (tasks, action, num);
			Task.WaitAll (tasks);
			HandleExceptions (tasks);
		}
		
		#endregion
		*/

		#region Invoke
		public static void Invoke (params Action[] actions)
		{
			if (actions == null)
				throw new ArgumentNullException ("actions");
			
			Invoke (actions, (Action a) => Task.Factory.StartNew (a));
		}
		
		public static void Invoke (ParallelOptions parallelOptions, params Action[] actions)
		{
			if (parallelOptions == null)
				throw new ArgumentNullException ("parallelOptions");
			if (actions == null)
				throw new ArgumentNullException ("actions");
			
			Invoke (actions, (Action a) => Task.Factory.StartNew (a, TaskCreationOptions.None, parallelOptions.TaskScheduler));
		}
		
		static void Invoke (Action[] actions, Func<Action, Task> taskCreator)
		{
			if (actions.Length == 0)
				throw new ArgumentException ("actions is empty");
			
			// Execute it directly
			if (actions.Length == 1 && actions[0] != null)
				actions[0] ();
			
			bool shouldThrow = false;
			Task[] ts = Array.ConvertAll (actions, delegate (Action a) {
				if (a == null) {
					shouldThrow = true;
					return null;
				}
				
				return taskCreator (a);
			});
			
			if (shouldThrow)
				throw new ArgumentException ("One action in actions is null", "actions");
			
			try {
				Task.WaitAll (ts);
			} catch {
				HandleExceptions (ts);
			}
		}
		#endregion

		#region SpawnBestNumber, used by PLinq
		internal static Task[] SpawnBestNumber (Action action, Action callback)
		{
			return SpawnBestNumber (action, -1, callback);
		}
		
		internal static Task[] SpawnBestNumber (Action action, int dop, Action callback)
		{
			return SpawnBestNumber (action, dop, false, callback);
		}
		
		internal static Task[] SpawnBestNumber (Action action, int dop, bool wait, Action callback)
		{
			// Get the optimum amount of worker to create
			int num = dop == -1 ? (wait ? GetBestWorkerNumber () + 1 : GetBestWorkerNumber ()) : dop;
			
			// Initialize worker
			CountdownEvent evt = new CountdownEvent (num);
			Task[] tasks = new Task [num];
			for (int i = 0; i < num; i++) {
				tasks [i] = Task.Factory.StartNew (() => { 
					action ();
					evt.Signal ();
					if (callback != null && evt.IsSet)
						callback ();
				});
			}

			// If explicitely told, wait for all workers to complete 
			// and thus let main thread participate in the processing
			if (wait)
				Task.WaitAll (tasks);
			
			return tasks;
		}
		#endregion
	}
}
#endif
