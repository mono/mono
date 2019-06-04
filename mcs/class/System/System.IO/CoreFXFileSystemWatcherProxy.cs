// Bridges the Mono and CoreFX FileSystemWatcher types.

using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

using C = System.IO.CoreFX.FileSystemWatcher;
using M = System.IO.FileSystemWatcher;
using Handle = System.Object;

namespace System.IO {

	internal class CoreFXFileSystemWatcherProxy : IFileWatcher
	{
		static IFileWatcher instance; // Mono FSW objects -> this
		static IDictionary<Handle, C> internal_map;  // this -> CoreFX FSW objects
		static ConditionalWeakTable<Handle, M> external_map; // this -> Mono FSW objects
		static IDictionary<object, Handle> event_map; // CoreFX FSW events -> this

		const int INTERRUPT_MS = 300;

		protected void Operation (Action<IDictionary<object, C>, ConditionalWeakTable<object, M>, IDictionary<object, object>, Handle> map_op = null, Action<C, M> object_op = null, object handle = null, Action<C, M> cancel_op = null)
		{
			C internal_fsw = null;
			M fsw = null;
			bool live, havelock;

			if (cancel_op != null) { // highest priority and must not lock
				havelock = Monitor.TryEnter (instance, INTERRUPT_MS);
				live = (handle != null && (internal_map.TryGetValue (handle, out internal_fsw) || external_map.TryGetValue (handle, out fsw))) ;
				if (live && havelock)
					try { cancel_op (internal_fsw, fsw); }
					catch (Exception) { };

				if (havelock)
					Monitor.Exit (instance);
				if (live && !havelock)
					try {
						var t = Task<bool>.Run( () => { cancel_op (internal_fsw, fsw); return true;});
						t.Wait (INTERRUPT_MS);
					}
					catch (Exception) { };
				return;
			}
			if (map_op != null && handle == null) {
				lock (instance) {
					try { map_op (internal_map, external_map, event_map, null); }
					catch (Exception e) { throw new InvalidOperationException (nameof(map_op), e); }
				}
				return;
			}

			if (handle == null)
				return;

			lock (instance) {
				live = (internal_map.TryGetValue (handle, out internal_fsw) && external_map.TryGetValue (handle, out fsw)) ;
				if (live && map_op != null) {
					try { map_op (internal_map, external_map, event_map, handle); }
					catch (Exception e) { throw new InvalidOperationException (nameof(map_op), e); };
				}
			}
			if (!live || object_op == null)
				return;

			try { object_op (internal_fsw, fsw); }
			catch (Exception e) { throw new InvalidOperationException (nameof(object_op), e); };
		}

		protected void ProxyDispatch (object sender, FileAction action, FileSystemEventArgs args)
		{
			RenamedEventArgs renamed =
				action == FileAction.RenamedNewName ? (RenamedEventArgs) args : null;

			object handle = null;

			Operation (map_op: (in_map, out_map, event_map, h) => event_map.TryGetValue (sender, out handle));

			Operation (object_op: (_, fsw) => {
						if (!fsw.EnableRaisingEvents)
							return;
						fsw.DispatchEvents (action, args.Name, ref renamed);
						if (fsw.Waiting) {
							fsw.Waiting = false;
							System.Threading.Monitor.PulseAll (fsw);
						}
					}, handle: handle);
		}

		protected void ProxyDispatchError (object sender, ErrorEventArgs args)
		{
			object handle = null;

			Operation (map_op: (in_map, out_map, event_map, _) => event_map.TryGetValue (sender, out handle));

			Operation (object_op: (_, fsw) => fsw.DispatchErrorEvents (args),
				handle: handle);
		}

		public object NewWatcher (M fsw)
		{
			var handle = new object ();
			var result = new C ();

			result.Changed += (object o, FileSystemEventArgs args) =>
								Task.Run (() => ProxyDispatch (o, FileAction.Modified, args));
			result.Created += (object o, FileSystemEventArgs args) =>
								Task.Run (() => ProxyDispatch (o, FileAction.Added, args));
			result.Deleted += (object o, FileSystemEventArgs args) =>
								Task.Run (() => ProxyDispatch (o, FileAction.Removed, args));
			result.Renamed += (object o, RenamedEventArgs args) =>
								Task.Run (() => ProxyDispatch (o, FileAction.RenamedNewName, args));

			result.Error += (object o, ErrorEventArgs args) =>
								Task.Run (() => ProxyDispatchError (handle, args));

			Operation (map_op: (in_map, out_map, event_map, _) => {
				in_map.Add (handle, result);
				out_map.Add (handle, fsw);
				event_map.Add (result, handle);
			});

			return handle;
		}

		public void StartDispatching (object handle)
		{
			if (handle == null)
				return;

			Operation (object_op: (internal_fsw, fsw) => {
					internal_fsw.Path = fsw.Path;
					internal_fsw.Filter = fsw.Filter;
					internal_fsw.IncludeSubdirectories = fsw.IncludeSubdirectories;
					internal_fsw.InternalBufferSize = fsw.InternalBufferSize;
					internal_fsw.NotifyFilter = fsw.NotifyFilter;
					internal_fsw.Site = fsw.Site;
					internal_fsw.EnableRaisingEvents = true;
				}, handle: handle);
		}

		public void StopDispatching (object handle)
		{
			if (handle == null)
				return;

			Operation (handle: handle,
				cancel_op: (internal_fsw, fsw) =>
				{
					if (internal_fsw != null)
						internal_fsw.EnableRaisingEvents = false;

				});
		}

		public void Dispose (object handle)
		{
			if (handle == null)
				return;

			Operation (handle: handle,
				cancel_op: (internal_fsw, fsw) => {
						if (internal_fsw != null)
							internal_fsw.Dispose ();
						var inner_key = internal_map [handle];
						internal_map.Remove (handle);
						external_map.Remove (handle);
						event_map.Remove (inner_key);
						handle = null;
					});
		}

		public static bool GetInstance (out IFileWatcher watcher)
		{
			if (instance != null) {
				watcher = instance;
				return true;
			}

			internal_map = new ConcurrentDictionary <object, C> ();
			external_map = new ConditionalWeakTable <object, M> ();
			event_map = new ConcurrentDictionary <object, object> ();
			instance = watcher = new CoreFXFileSystemWatcherProxy ();
			return true;
		}
	}
}
