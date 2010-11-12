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

#if NET_4_0
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading;

namespace System.Threading.Tasks
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

		static int GetBestWorkerNumber (int from, int to, ParallelOptions options, out int step)
		{
			int num = Math.Min (GetBestWorkerNumber (),
			                    options != null && options.MaxDegreeOfParallelism != -1 ? options.MaxDegreeOfParallelism : int.MaxValue);
			// Integer range that each task process
			if ((step = (to - from) / num) < 5) {
				step = 5;
				num = (to - from) / 5;
				if (num < 1)
					num = 1;
			}

			return num;
		}

		static void HandleExceptions (IEnumerable<Task> tasks)
		{
			HandleExceptions (tasks, null);
		}

		static void HandleExceptions (IEnumerable<Task> tasks, ParallelLoopState.ExternalInfos infos)
		{
			List<Exception> exs = new List<Exception> ();
			foreach (Task t in tasks) {
				if (t.Exception != null)
					exs.Add (t.Exception);
			}

			if (exs.Count > 0) {
				if (infos != null)
					infos.IsExceptional = true;

				throw new AggregateException (exs);
			}
		}

		static void InitTasks (Task[] tasks, int count, Action action, ParallelOptions options)
		{
			TaskCreationOptions creation = TaskCreationOptions.LongRunning;

			for (int i = 0; i < count; i++) {
				if (options == null)
					tasks [i] = Task.Factory.StartNew (action, creation);
				else
					tasks [i] = Task.Factory.StartNew (action, options.CancellationToken, creation, options.TaskScheduler);
			}
		}

		#region For

		public static ParallelLoopResult For (int from, int to, Action<int> action)
		{
			return For (from, to, ParallelOptions.Default, action);
		}

		public static ParallelLoopResult For (int from, int to, Action<int, ParallelLoopState> action)
		{
			return For (from, to, ParallelOptions.Default, action);
		}

		public static ParallelLoopResult For (int from, int to, ParallelOptions options, Action<int> action)
		{
			return For (from, to, options, (index, state) => action (index));
		}

		public static ParallelLoopResult For (int from, int to, ParallelOptions options, Action<int, ParallelLoopState> action)
		{
			return For<object> (from, to, options, () => null, (i, s, l) => { action (i, s); return null; }, _ => {});
		}

		public static ParallelLoopResult For<TLocal> (int from,
		                                              int to,
		                                              Func<TLocal> init,
		                                              Func<int, ParallelLoopState, TLocal, TLocal> action,
		                                              Action<TLocal> destruct)
		{
			return For<TLocal> (from, to, ParallelOptions.Default, init, action, destruct);
		}

		public static ParallelLoopResult For<TLocal> (int from,
		                                              int to,
		                                              ParallelOptions options,
		                                              Func<TLocal> init,
		                                              Func<int, ParallelLoopState, TLocal, TLocal> action,
		                                              Action<TLocal> destruct)
		{
			if (action == null)
				throw new ArgumentNullException ("action");
			if (init == null)
				throw new ArgumentNullException ("localInit");
			if (destruct == null)
				throw new ArgumentNullException ("localFinally");
			if (options == null)
				throw new ArgumentNullException ("options");
			if (from >= to)
				return new ParallelLoopResult (null, true);

			// Number of task to be launched (normally == Env.ProcessorCount)
			int step;
			int num = GetBestWorkerNumber (from, to, options, out step);

			Task[] tasks = new Task [num];

			StealRange[] ranges = new StealRange[num];
			for (int i = 0; i < num; i++)
				ranges[i] = new StealRange (from, i, step);

			ParallelLoopState.ExternalInfos infos = new ParallelLoopState.ExternalInfos ();

			int currentIndex = -1;

			Action workerMethod = delegate {
				int localWorker = Interlocked.Increment (ref currentIndex);
				StealRange range = ranges[localWorker];
				int index = range.Actual;
				int stopIndex = localWorker + 1 == num ? to : Math.Min (to, index + step);
				TLocal local = init ();

				ParallelLoopState state = new ParallelLoopState (infos);
				CancellationToken token = options.CancellationToken;

				try {
					for (int i = index; i < stopIndex; range.Actual = ++i) {
						if (infos.IsStopped)
							return;

						token.ThrowIfCancellationRequested ();

						if (infos.LowestBreakIteration != null && infos.LowestBreakIteration > i)
							return;

						state.CurrentIteration = i;
						local = action (i, state, local);
						if (i >= stopIndex - range.Stolen)
							break;
					}

					// Try to steal from our right neighbor (cyclic)
					int len = num + localWorker;
					for (int sIndex = localWorker + 1; sIndex < len; ++sIndex) {
						int extWorker = sIndex % num;
						range = ranges[extWorker];

						stopIndex = extWorker + 1 == num ? to : Math.Min (to, from + (extWorker + 1) * step);

						int stolen;
						do {
							stolen = range.Stolen;
							if (stopIndex - stolen > range.Actual)
								goto next;
						} while (Interlocked.CompareExchange (ref range.Stolen, stolen + 1, stolen) != stolen);

						stolen = stopIndex - stolen - 1;

						if (stolen > range.Actual)
							local = action (stolen, state, local);

					next:
						continue;
					}
				} finally {
					destruct (local);
				}
			};

			InitTasks (tasks, num, workerMethod, options);

			try {
				Task.WaitAll (tasks);
			} catch {
				HandleExceptions (tasks, infos);
			}

			return new ParallelLoopResult (infos.LowestBreakIteration, !(infos.IsStopped || infos.IsExceptional));
		}

		class StealRange
		{
			public int Stolen;
			public int Actual;

			public StealRange (int from, int i, int step)
			{
				Actual = from + i * step;
			}
		}

		#endregion

		#region Foreach
		static ParallelLoopResult ForEach<TSource, TLocal> (Func<int, IList<IEnumerator<TSource>>> enumerable, ParallelOptions options,
		                                                    Func<TLocal> init, Func<TSource, ParallelLoopState, TLocal, TLocal> action,
		                                                    Action<TLocal> destruct)
		{
			if (enumerable == null)
				throw new ArgumentNullException ("source");
			if (options == null)
				throw new ArgumentNullException ("options");
			if (action == null)
				throw new ArgumentNullException ("action");
			if (init == null)
				throw new ArgumentNullException ("init");
			if (destruct == null)
				throw new ArgumentNullException ("destruct");

			int num = Math.Min (GetBestWorkerNumber (),
			                    options != null && options.MaxDegreeOfParallelism != -1 ? options.MaxDegreeOfParallelism : int.MaxValue);

			Task[] tasks = new Task[num];
			ParallelLoopState.ExternalInfos infos = new ParallelLoopState.ExternalInfos ();

			SimpleConcurrentBag<TSource> bag = new SimpleConcurrentBag<TSource> (num);
			const int bagCount = 5;

			IList<IEnumerator<TSource>> slices = enumerable (num);

			int sliceIndex = -1;

			Action workerMethod = delegate {
				IEnumerator<TSource> slice = slices[Interlocked.Increment (ref sliceIndex)];

				TLocal local = init ();
				ParallelLoopState state = new ParallelLoopState (infos);
				int workIndex = bag.GetNextIndex ();
				CancellationToken token = options.CancellationToken;

				try {
					bool cont = true;
					TSource element;

					while (cont) {
						if (infos.IsStopped || infos.IsBroken.Value)
							return;

						token.ThrowIfCancellationRequested ();

						for (int i = 0; i < bagCount && (cont = slice.MoveNext ()); i++) {
							bag.Add (workIndex, slice.Current);
						}

						for (int i = 0; i < bagCount && bag.TryTake (workIndex, out element); i++) {
							if (infos.IsStopped)
								return;

							token.ThrowIfCancellationRequested ();

							local = action (element, state, local);
						}
					}

					while (bag.TrySteal (workIndex, out element)) {
						token.ThrowIfCancellationRequested ();

						local = action (element, state, local);

						if (infos.IsStopped || infos.IsBroken.Value)
							return;
					}
				} finally {
					destruct (local);
				}
			};

			InitTasks (tasks, num, workerMethod, options);

			try {
				Task.WaitAll (tasks);
			} catch {
				HandleExceptions (tasks, infos);
			}

			return new ParallelLoopResult (infos.LowestBreakIteration, !(infos.IsStopped || infos.IsExceptional));
		}

		public static ParallelLoopResult ForEach<TSource> (IEnumerable<TSource> enumerable, Action<TSource> action)
		{
			if (enumerable == null)
				throw new ArgumentNullException ("source");
			if (action == null)
				throw new ArgumentNullException ("action");

			return ForEach<TSource, object> (Partitioner.Create (enumerable),
			                                 ParallelOptions.Default,
			                                 () => null,
			                                 (e, s, l) => { action (e); return null; },
			                                 _ => {});
		}

		public static ParallelLoopResult ForEach<TSource> (IEnumerable<TSource> enumerable, Action<TSource, ParallelLoopState> action)
		{
			if (enumerable == null)
				throw new ArgumentNullException ("source");
			if (action == null)
				throw new ArgumentNullException ("action");

			return ForEach<TSource, object> (Partitioner.Create (enumerable),
			                                 ParallelOptions.Default,
			                                 () => null,
			                                 (e, s, l) => { action (e, s); return null; },
			                                 _ => {});
		}

		public static ParallelLoopResult ForEach<TSource> (IEnumerable<TSource> enumerable,
		                                                   Action<TSource, ParallelLoopState, long> action)
		{
			if (enumerable == null)
				throw new ArgumentNullException ("source");
			if (action == null)
				throw new ArgumentNullException ("action");


			return ForEach<TSource, object> (Partitioner.Create (enumerable),
			                                 ParallelOptions.Default,
			                                 () => null,
			                                 (e, s, l) => { action (e, s, -1); return null; },
			                                 _ => {});
		}

		public static ParallelLoopResult ForEach<TSource> (Partitioner<TSource> source,
		                                                   Action<TSource, ParallelLoopState> body)
		{
			if (body == null)
				throw new ArgumentNullException ("body");

			return ForEach<TSource, object> (source,
			                                 ParallelOptions.Default,
			                                 () => null,
			                                 (e, s, l) => { body (e, s); return null; },
			                                 _ => {});
		}

		public static ParallelLoopResult ForEach<TSource> (OrderablePartitioner<TSource> source,
		                                                   Action<TSource, ParallelLoopState, long> body)

		{
			if (body == null)
				throw new ArgumentNullException ("body");

			return ForEach<TSource, object> (source,
			                                 ParallelOptions.Default,
			                                 () => null,
			                                 (e, s, i, l) => { body (e, s, i); return null; },
			                                 _ => {});
		}

		public static ParallelLoopResult ForEach<TSource> (Partitioner<TSource> source,
		                                                   Action<TSource> body)

		{
			if (body == null)
				throw new ArgumentNullException ("body");

			return ForEach<TSource, object> (source,
			                                 ParallelOptions.Default,
			                                 () => null,
			                                 (e, s, l) => { body (e); return null; },
			                                 _ => {});
		}

		public static ParallelLoopResult ForEach<TSource> (IEnumerable<TSource> source,
		                                                   ParallelOptions parallelOptions,
		                                                   Action<TSource> body)
		{
			if (source == null)
				throw new ArgumentNullException ("source");
			if (body == null)
				throw new ArgumentNullException ("body");

			return ForEach<TSource, object> (Partitioner.Create (source),
			                                 parallelOptions,
			                                 () => null,
			                                 (e, s, l) => { body (e); return null; },
			                                 _ => {});
		}

		public static ParallelLoopResult ForEach<TSource> (IEnumerable<TSource> source, ParallelOptions parallelOptions,
		                                                   Action<TSource, ParallelLoopState> body)
		{
			if (source == null)
				throw new ArgumentNullException ("source");
			if (body == null)
				throw new ArgumentNullException ("body");

			return ForEach<TSource, object> (Partitioner.Create (source),
			                                 parallelOptions,
			                                 () => null,
			                                 (e, s, l) => { body (e, s); return null; },
			                                 _ => {});
		}

		public static ParallelLoopResult ForEach<TSource> (IEnumerable<TSource> source, ParallelOptions parallelOptions,
		                                                   Action<TSource, ParallelLoopState, long> body)
		{
			if (source == null)
				throw new ArgumentNullException ("source");
			if (body == null)
				throw new ArgumentNullException ("body");

			return ForEach<TSource, object> (Partitioner.Create (source),
			                                 parallelOptions,
			                                 () => null,
			                                 (e, s, i, l) => { body (e, s, i); return null; },
			                                 _ => {});
		}

		public static ParallelLoopResult ForEach<TSource> (OrderablePartitioner<TSource> source, ParallelOptions parallelOptions,
		                                                   Action<TSource, ParallelLoopState, long> body)

		{
			if (body == null)
				throw new ArgumentNullException ("body");

			return ForEach<TSource, object> (source,
			                                 parallelOptions,
			                                 () => null,
			                                 (e, s, i, l) => { body (e, s, i); return null; },
			                                 _ => {});
		}

		public static ParallelLoopResult ForEach<TSource> (Partitioner<TSource> source, ParallelOptions parallelOptions,
		                                                   Action<TSource> body)
		{
			if (body == null)
				throw new ArgumentNullException ("body");

			return ForEach<TSource, object> (source,
			                                 parallelOptions,
			                                 () => null,
			                                 (e, s, l) => { body (e); return null; },
			                                 _ => {});
		}

		public static ParallelLoopResult ForEach<TSource> (Partitioner<TSource> source, ParallelOptions parallelOptions,
		                                                   Action<TSource, ParallelLoopState> body)
		{
			return ForEach<TSource, object> (source,
			                                 parallelOptions,
			                                 () => null,
			                                 (e, s, l) => { body (e, s); return null; },
			                                 _ => {});
		}

		public static ParallelLoopResult ForEach<TSource, TLocal> (IEnumerable<TSource> source, Func<TLocal> localInit,
		                                                           Func<TSource, ParallelLoopState, TLocal, TLocal> body,
		                                                           Action<TLocal> localFinally)
		{
			if (source == null)
				throw new ArgumentNullException ("source");

			return ForEach<TSource, TLocal> ((Partitioner<TSource>)Partitioner.Create (source),
			                                 ParallelOptions.Default,
			                                 localInit,
			                                 body,
			                                 localFinally);
		}

		public static ParallelLoopResult ForEach<TSource, TLocal> (IEnumerable<TSource> source, Func<TLocal> localInit,
		                                                           Func<TSource, ParallelLoopState, long, TLocal, TLocal> body,
		                                                           Action<TLocal> localFinally)
		{
			return ForEach<TSource, TLocal> (Partitioner.Create (source),
			                                 ParallelOptions.Default,
			                                 localInit,
			                                 body,
			                                 localFinally);
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
			if (source == null)
				throw new ArgumentNullException ("source");

			return ForEach<TSource, TLocal> (Partitioner.Create (source), parallelOptions, localInit, body, localFinally);
		}

		public static ParallelLoopResult ForEach<TSource, TLocal> (IEnumerable<TSource> source, ParallelOptions parallelOptions,
		                                                           Func<TLocal> localInit,
		                                                           Func<TSource, ParallelLoopState, long, TLocal, TLocal> body,
		                                                           Action<TLocal> localFinally)
		{
			if (source == null)
				throw new ArgumentNullException ("source");

			return ForEach<TSource, TLocal> (Partitioner.Create (source), parallelOptions, localInit, body, localFinally);
		}

		public static ParallelLoopResult ForEach<TSource, TLocal> (Partitioner<TSource> enumerable, ParallelOptions options,
		                                                           Func<TLocal> init,
		                                                           Func<TSource, ParallelLoopState, TLocal, TLocal> action,
		                                                           Action<TLocal> destruct)
		{
			if (enumerable == null)
				throw new ArgumentNullException ("source");
			if (action == null)
				throw new ArgumentNullException ("action");

			return ForEach<TSource, TLocal> (enumerable.GetPartitions, options, init, action, destruct);
		}

		public static ParallelLoopResult ForEach<TSource, TLocal> (OrderablePartitioner<TSource> enumerable, ParallelOptions options,
		                                                           Func<TLocal> init,
		                                                           Func<TSource, ParallelLoopState, long, TLocal, TLocal> action,
		                                                           Action<TLocal> destruct)
		{
			if (enumerable == null)
				throw new ArgumentNullException ("source");
			if (action == null)
				throw new ArgumentNullException ("action");

			return ForEach<KeyValuePair<long, TSource>, TLocal> (enumerable.GetOrderablePartitions,
			                                                     options,
			                                                     init,
			                                                     (e, s, l) => action (e.Value, s, e.Key, l),
			                                                     destruct);
		}
		#endregion

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

			Invoke (actions, (Action a) => Task.Factory.StartNew (a, parallelOptions.CancellationToken, TaskCreationOptions.None, parallelOptions.TaskScheduler));
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
