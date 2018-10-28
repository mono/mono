//
// ServicePointScheduler.cs
//
// Author:
//       Martin Baulig <mabaul@microsoft.com>
//
// Copyright (c) 2017 Xamarin Inc. (http://www.xamarin.com)
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
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.ExceptionServices;
using System.Diagnostics;

namespace System.Net
{
	class ServicePointScheduler
	{
		ServicePoint ServicePoint {
			get; set;
		}

		public int MaxIdleTime {
			get { return maxIdleTime; }
			set {
				if (value < Timeout.Infinite || value > Int32.MaxValue)
					throw new ArgumentOutOfRangeException ();
				if (value == maxIdleTime)
					return;
				maxIdleTime = value;
				Debug ($"MAX IDLE TIME = {value}");
				Run ();
			}
		}

		public int ConnectionLimit {
			get { return connectionLimit; }
			set {
				if (value <= 0)
					throw new ArgumentOutOfRangeException ();

				if (value == connectionLimit)
					return;
				connectionLimit = value;
				Debug ($"CONNECTION LIMIT = {value}");
				Run ();
			}
		}

		public ServicePointScheduler (ServicePoint servicePoint, int connectionLimit, int maxIdleTime)
		{
			ServicePoint = servicePoint;
			this.connectionLimit = connectionLimit;
			this.maxIdleTime = maxIdleTime;

			schedulerEvent = new AsyncManualResetEvent (false);
			defaultGroup = new ConnectionGroup (this, string.Empty);
			operations = new LinkedList<(ConnectionGroup, WebOperation)> ();
			idleConnections = new LinkedList<(ConnectionGroup, WebConnection, Task)> ();
			idleSince = DateTime.UtcNow;
		}

		[Conditional ("MONO_WEB_DEBUG")]
		void Debug (string message)
		{
			WebConnection.Debug ($"SPS({ID}): {message}");
		}

		int running;
		int maxIdleTime = 100000;
		AsyncManualResetEvent schedulerEvent;
		ConnectionGroup defaultGroup;
		Dictionary<string, ConnectionGroup> groups;
		LinkedList<(ConnectionGroup, WebOperation)> operations;
		LinkedList<(ConnectionGroup, WebConnection, Task)> idleConnections;
		int currentConnections;
		int connectionLimit;
		DateTime idleSince;

		public int CurrentConnections {
			get {
				return currentConnections;
			}
		}

		public DateTime IdleSince {
			get {
				return idleSince;
			}
		}

		static int nextId;
		public readonly int ID = ++nextId;

		internal string ME {
			get;
		}

		public void Run ()
		{
			Debug ($"RUN");
			if (Interlocked.CompareExchange (ref running, 1, 0) == 0)
				Task.Run (() => RunScheduler ());

			schedulerEvent.Set ();
		}

		async Task RunScheduler ()
		{
			idleSince = DateTime.UtcNow + TimeSpan.FromDays (3650);

			while (true) {
				Debug ($"MAIN LOOP");

				// Gather list of currently running operations.
				ValueTuple<ConnectionGroup, WebOperation>[] operationArray;
				ValueTuple<ConnectionGroup, WebConnection, Task>[] idleArray;
				var taskList = new List<Task> ();
				Task<bool> schedulerTask;
				bool finalCleanup = false;
				lock (ServicePoint) {
					Cleanup ();

					operationArray = new ValueTuple<ConnectionGroup, WebOperation>[operations.Count];
					operations.CopyTo (operationArray, 0);
					idleArray = new ValueTuple<ConnectionGroup, WebConnection, Task>[idleConnections.Count];
					idleConnections.CopyTo (idleArray, 0);

					schedulerTask = schedulerEvent.WaitAsync (maxIdleTime);
					taskList.Add (schedulerTask);

					if (groups == null && defaultGroup.IsEmpty () && operations.Count == 0 && idleConnections.Count == 0) {
						Debug ($"MAIN LOOP DONE");
						idleSince = DateTime.UtcNow;
						finalCleanup = true;
					} else {
						foreach (var item in operationArray)
							taskList.Add (item.Item2.Finished.Task);
						foreach (var item in idleArray)
							taskList.Add (item.Item3);
					}
				}

				Debug ($"MAIN LOOP #1: operations={operationArray.Length} idle={idleArray.Length} maxIdleTime={maxIdleTime} finalCleanup={finalCleanup}");

				var ret = await Task.WhenAny (taskList).ConfigureAwait (false);

				lock (ServicePoint) {
					bool runMaster = false;
					if (finalCleanup) {
						if (schedulerTask.Result)
							runMaster = true;
						else {
							FinalCleanup ();
							return;
						}
					} else if (ret == taskList[0]) {
						runMaster = true;
					}

					/*
					 * We discard the `taskList` at this point as it is only used to wake us up.
					 *
					 * The `WebCompletionSource<T>` assigns its `CurrentResult` property prior
					 * to completing the `Task` instance, so whenever a task is finished we will
					 * also get a non-null `CurrentResult`.
					 * 
					 */
					for (int i = 0; i < operationArray.Length; i++) {
						var item = operationArray[i];
						var result = item.Item2.Finished.CurrentResult;
						if (result == null)
							continue;

						Debug ($"MAIN LOOP #2: {i} group={item.Item1.ID} Op={item.Item2.ID} Status={result.Status}");
						operations.Remove (item);

						var runLoop = OperationCompleted (item.Item1, item.Item2);
						Debug ($"MAIN LOOP #2 DONE: {i} {runLoop}");
						runMaster |= runLoop;
					}

					/*
					 * This needs to be called after we deal with pending completions to
					 * ensure that connections are properly recognized as being idle.
					 * 
					 */
					Debug ($"MAIN LOOP #3: runMaster={runMaster}");
					if (runMaster)
						RunSchedulerIteration ();

					int idx = -1;
					for (int i = 0; i < idleArray.Length; i++) {
						if (ret == taskList[i + 1 + operationArray.Length]) {
							idx = i;
							break;
						}
					}

					if (idx >= 0) {
						var item = idleArray[idx];
						Debug ($"MAIN LOOP #4: {idx} group={item.Item1.ID} Cnc={item.Item2.ID}");
						idleConnections.Remove (item);
						CloseIdleConnection (item.Item1, item.Item2);
					}
				}
			}
		}

		void Cleanup ()
		{
			if (groups != null) {
				var keys = new string[groups.Count];
				groups.Keys.CopyTo (keys, 0);
				foreach (var groupName in keys) {
					if (!groups.ContainsKey (groupName))
						continue;
					var group = groups[groupName];
					if (group.IsEmpty ()) {
						Debug ($"CLEANUP - REMOVING group={group.ID}");
						groups.Remove (groupName);
					}
				}
				if (groups.Count == 0)
					groups = null;
			}
		}

		void RunSchedulerIteration ()
		{
			schedulerEvent.Reset ();

			bool repeat;
			do {
				Debug ($"ITERATION");

				repeat = SchedulerIteration (defaultGroup);

				Debug ($"ITERATION #1: repeat={repeat} groups={groups?.Count}");

				if (groups != null) {
					foreach (var group in groups) {
						Debug ($"ITERATION #2: group={group.Value.ID}");
						repeat |= SchedulerIteration (group.Value);
					}
				}

				Debug ($"ITERATION #3: repeat={repeat}");
			} while (repeat);
		}

		bool OperationCompleted (ConnectionGroup group, WebOperation operation)
		{
#if MONO_WEB_DEBUG
			var me = $"{nameof (OperationCompleted)}(group={group.ID}, Op={operation.ID}, Cnc={operation.Connection.ID})";
#else
			string me = null;
#endif

			var result = operation.Finished.CurrentResult;
			var (ok, next) = result.Success ? result.Argument : (false, null);

			Debug ($"{me}: {operation.Finished.CurrentStatus} {ok} {next?.ID}");

			if (!ok || !operation.Connection.Continue (next)) {
				group.RemoveConnection (operation.Connection);
				if (next == null) {
					Debug ($"{me}: closed connection and done.");
					return true;
				}
				ok = false;
			}

			if (next == null) {
				if (ok) {
					var idleTask = Task.Delay (MaxIdleTime);
					idleConnections.AddLast ((group, operation.Connection, idleTask));
					Debug ($"{me} keeping connection open for {MaxIdleTime} milliseconds.");
				} else {
					Debug ($"{me}: closed connection and done.");
				}
				return true;
			}

			Debug ($"{me} got new operation next={next.ID}.");
			operations.AddLast ((group, next));

			if (ok) {
				Debug ($"{me} continuing next={next.ID} on same connection.");
				RemoveIdleConnection (operation.Connection);
				return false;
			}

			group.Cleanup ();

			var (connection, created) = group.CreateOrReuseConnection (next, true);
			Debug ($"{me} created new connection Cnc={connection.ID} next={next.ID}.");
			return false;
		}

		void CloseIdleConnection (ConnectionGroup group, WebConnection connection)
		{
			Debug ($"{nameof (CloseIdleConnection)}(group={group.ID}, Cnc={connection.ID}) closing idle connection.");

			group.RemoveConnection (connection);
			RemoveIdleConnection (connection);
		}

		bool SchedulerIteration (ConnectionGroup group)
		{
#if MONO_WEB_DEBUG
			var me = $"{nameof (SchedulerIteration)}(group={group.ID})";
#else
			string me = null;
#endif
			Debug ($"{me}");

			// First, let's clean up.
			group.Cleanup ();

			// Is there anything in the queue?
			var next = group.GetNextOperation ();
			if (next == null)
				return false;

			Debug ($"{me} found pending operation Op={next.ID}");

			var (connection, created) = group.CreateOrReuseConnection (next, false);
			if (connection == null) {
				// All connections are currently busy, need to keep it in the queue for now.
				Debug ($"{me} all connections busy, keeping operation in queue.");
				return false;
			}

			Debug ($"{me} started operation: Op={next.ID} Cnc={connection.ID}");
			operations.AddLast ((group, next));
			RemoveIdleConnection (connection);
			return true;
		}

		void RemoveOperation (WebOperation operation)
		{
			var iter = operations.First;
			while (iter != null) {
				var node = iter;
				iter = iter.Next;

				if (node.Value.Item2 == operation)
					operations.Remove (node);
			}
		}

		void RemoveIdleConnection (WebConnection connection)
		{
			var iter = idleConnections.First;
			while (iter != null) {
				var node = iter;
				iter = iter.Next;

				if (node.Value.Item2 == connection)
					idleConnections.Remove (node);
			}
		}

		void FinalCleanup ()
		{
			Debug ($"FINAL CLEANUP");

			groups = null;
			operations = null;
			idleConnections = null;
			defaultGroup = null;

			ServicePoint.FreeServicePoint ();
			ServicePointManager.RemoveServicePoint (ServicePoint);
			ServicePoint = null;
		}

		public void SendRequest (WebOperation operation, string groupName)
		{
			lock (ServicePoint) {
				var group = GetConnectionGroup (groupName);
				Debug ($"SEND REQUEST: Op={operation.ID} group={group.ID}");
				group.EnqueueOperation (operation);
				Run ();
				Debug ($"SEND REQUEST DONE: Op={operation.ID} group={group.ID}");
			}
		}

		public bool CloseConnectionGroup (string groupName)
		{
			ConnectionGroup group;
			if (string.IsNullOrEmpty (groupName))
				group = defaultGroup;
			else if (groups == null || !groups.TryGetValue (groupName, out group))
				return false;

			Debug ($"CLOSE CONNECTION GROUP: group={group.ID}");

			if (group != defaultGroup) {
				groups.Remove (groupName);
				if (groups.Count == 0)
					groups = null;
			}

			group.Close ();
			Run ();
			return true;
		}

		ConnectionGroup GetConnectionGroup (string name)
		{
			lock (ServicePoint) {
				if (string.IsNullOrEmpty (name))
					return defaultGroup;

				if (groups == null)
					groups = new Dictionary<string, ConnectionGroup> (); 

				if (groups.TryGetValue (name, out ConnectionGroup group))
					return group;

				group = new ConnectionGroup (this, name);
				groups.Add (name, group);
				return group;
			}
		}

		void OnConnectionCreated (WebConnection connection)
		{
			Interlocked.Increment (ref currentConnections);
		}

		void OnConnectionClosed (WebConnection connection)
		{
			RemoveIdleConnection (connection);
			Interlocked.Decrement (ref currentConnections);
		}

		public static async Task<bool> WaitAsync (Task workerTask, int millisecondTimeout)
		{
			var cts = new CancellationTokenSource ();
			try {
				var timeoutTask = Task.Delay (millisecondTimeout, cts.Token);
				var ret = await Task.WhenAny (workerTask, timeoutTask).ConfigureAwait (false);
				return ret != timeoutTask;
			} finally {
				cts.Cancel ();
				cts.Dispose ();
			}
		}

		class ConnectionGroup
		{
			public ServicePointScheduler Scheduler {
				get;
			}

			public string Name {
				get;
			}

			public bool IsDefault => string.IsNullOrEmpty (Name);

			static int nextId;
			public readonly int ID = ++nextId;
			LinkedList<WebConnection> connections;
			LinkedList<WebOperation> queue;

			public ConnectionGroup (ServicePointScheduler scheduler, string name)
			{
				Scheduler = scheduler;
				Name = name;

				connections = new LinkedList<WebConnection> ();
				queue = new LinkedList<WebOperation> (); 
			}

			public bool IsEmpty ()
			{
				return connections.Count == 0 && queue.Count == 0;
			}

			public void RemoveConnection (WebConnection connection)
			{
				Scheduler.Debug ($"REMOVING CONNECTION: group={ID} cnc={connection.ID}");
				connections.Remove (connection);
				connection.Dispose ();
				Scheduler.OnConnectionClosed (connection);
			}

			public void Cleanup ()
			{
				var iter = connections.First;
				while (iter != null) {
					var connection = iter.Value;
					var node = iter;
					iter = iter.Next;

					if (connection.Closed) {
						Scheduler.Debug ($"REMOVING CONNECTION: group={ID} cnc={connection.ID}");
						connections.Remove (node);
						Scheduler.OnConnectionClosed (connection);
					}
				}
			}

			public void Close ()
			{
				foreach (var operation in queue) {
					operation.Abort ();
					Scheduler.RemoveOperation (operation);
				}
				queue.Clear ();

				foreach (var connection in connections) {
					connection.Dispose ();
					Scheduler.OnConnectionClosed (connection);
				}
				connections.Clear ();
			}

			public void EnqueueOperation (WebOperation operation)
			{
				queue.AddLast (operation);
			}

			public WebOperation GetNextOperation ()
			{
				// Is there anything in the queue?
				var iter = queue.First;
				while (iter != null) {
					var operation = iter.Value;
					var node = iter;
					iter = iter.Next;

					if (operation.Aborted) {
						queue.Remove (node);
						Scheduler.RemoveOperation (operation);
						continue;
					}

					return operation;
				}

				return null;
			}

			public WebConnection FindIdleConnection (WebOperation operation)
			{
				// First let's find the ideal candidate.
				WebConnection candidate = null;
				foreach (var connection in connections) {
					if (connection.CanReuseConnection (operation)) {
						if (candidate == null || connection.IdleSince > candidate.IdleSince)
							candidate = connection;
					}
				}

				// Found one?  Make sure it's actually willing to run it.
				if (candidate != null && candidate.StartOperation (operation, true)) {
					queue.Remove (operation);
					return candidate;
				}

				// Ok, let's loop again and pick the first one that accepts the new operation.
				foreach (var connection in connections) {
					if (connection.StartOperation (operation, true)) {
						queue.Remove (operation);
						return connection;
					}
				}

				return null;
			}

			public (WebConnection connection, bool created) CreateOrReuseConnection (WebOperation operation, bool force)
			{
				Scheduler.Debug ($"CREATE OR REUSE: group={ID} Op={operation.ID} force={force}");
				var connection = FindIdleConnection (operation);
				Scheduler.Debug ($"CREATE OR REUSE #1: group={ID} Op={operation.ID} force={force} - connection={connection?.ID}");
				if (connection != null)
					return (connection, false);

				if (force || Scheduler.ServicePoint.ConnectionLimit > connections.Count || connections.Count == 0) {
					connection = new WebConnection (Scheduler.ServicePoint);
					connection.StartOperation (operation, false);
					connections.AddFirst (connection);
					Scheduler.OnConnectionCreated (connection);
					queue.Remove (operation);
					return (connection, true);
				}

				return (null, false);
			}
		}

		// https://blogs.msdn.microsoft.com/pfxteam/2012/02/11/building-async-coordination-primitives-part-1-asyncmanualresetevent/
		class AsyncManualResetEvent
		{
			volatile TaskCompletionSource<bool> m_tcs = new TaskCompletionSource<bool> ();

			public Task WaitAsync () { return m_tcs.Task; }

			public bool WaitOne (int millisecondTimeout)
			{
				WebConnection.Debug ($"AMRE WAIT ONE: {millisecondTimeout}");
				return m_tcs.Task.Wait (millisecondTimeout);
			}

			public Task<bool> WaitAsync (int millisecondTimeout)
			{
				return ServicePointScheduler.WaitAsync (m_tcs.Task, millisecondTimeout);
			}

			public void Set ()
			{
				var tcs = m_tcs;
				Task.Factory.StartNew (s => ((TaskCompletionSource<bool>)s).TrySetResult (true),
				    tcs, CancellationToken.None, TaskCreationOptions.PreferFairness, TaskScheduler.Default);
				tcs.Task.Wait ();
			}

			public void Reset ()
			{
				while (true) {
					var tcs = m_tcs;
					if (!tcs.Task.IsCompleted ||
					    Interlocked.CompareExchange (ref m_tcs, new TaskCompletionSource<bool> (), tcs) == tcs)
						return;
				}
			}

			public AsyncManualResetEvent (bool state)
			{
				if (state)
					Set ();
			}
		}
	}
}
