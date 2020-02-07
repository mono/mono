using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Mono.Cecil;

namespace WebAssembly.Net.Debugging
{
	class DebugStoreLoader : BaseAssemblyResolver
	{
		readonly IDictionary<string, AssemblyDefinition> cache;
		readonly IDictionary<string, AssemblyEntry> assemblies;
		readonly HttpClient client;
		readonly MonoProxy proxy;
		readonly SessionId session;
		CancellationTokenSource cancellationTokenSource;

		public bool ParallelLoad { get; set; } = false;

		class AssemblyEntry {
			readonly DebugStoreLoader loader;
			TaskCompletionSource<AssemblyInfo> resolvePromise;

			public string Url { get; }
			public string PdbUrl { get; }
			public string Name { get; }
			public AssemblyInfo Assembly { get; private set; }

			public AssemblyEntry (DebugStoreLoader loader, string file, string pdb)
			{
				this.loader = loader;
				Url = file;
				PdbUrl = pdb;
				Name = Path.GetFileNameWithoutExtension (file);
			}

			public Task<AssemblyInfo> Resolve ()
			{
				/*
				 * We use the `resolvePromise` here to allow this `Resolve ()` method to be safely
				 * called both repeatedly as well as concurrently.
				*/
				var old = Interlocked.CompareExchange (ref resolvePromise, new TaskCompletionSource<AssemblyInfo> (), null);
				if (old != null)
					return old.Task;

				// We only get here the first time we're called.

				Task.Factory.StartNew (async () => {
					try {
						await Resolve_internal ().ConfigureAwait (false);
						resolvePromise.TrySetResult (Assembly);
					} catch (OperationCanceledException) {
						resolvePromise.TrySetCanceled ();
					} catch (Exception error) {
						resolvePromise.TrySetException (error);
					}
				});

				return resolvePromise.Task;
			}

			async Task Resolve_internal ()
			{
				var asm_task = loader.DownloadFile (Url);
				Task<byte[]> pdb_task = null;
				if (PdbUrl != null) {
					pdb_task = loader.DownloadFile (PdbUrl);
					await Task.WhenAll (asm_task, pdb_task).ConfigureAwait (false);
				} else {
					await asm_task.ConfigureAwait (false);
				}

				Assembly = new AssemblyInfo (Url, asm_task.Result, pdb_task?.Result, loader);
			}
		}

		public DebugStoreLoader (MonoProxy proxy, SessionId session, string[] loaded_files, CancellationToken token)
		{
			this.proxy = proxy;
			this.session = session;
			cache = new Dictionary<string, AssemblyDefinition> (StringComparer.Ordinal);
			assemblies = new Dictionary<string, AssemblyEntry> (StringComparer.Ordinal);
			client = new HttpClient ();

			cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource (token);
			cancellationTokenSource.Token.Register (() => client.CancelPendingRequests ());

			static bool MatchPdb (string asm, string pdb)
				=> Path.ChangeExtension (asm, "pdb") == pdb;

			var asm_files = new List<string> ();
			var pdb_files = new List<string> ();
			foreach (var f in loaded_files) {
				var file_name = f;
				if (file_name.EndsWith (".pdb", StringComparison.OrdinalIgnoreCase))
					pdb_files.Add (file_name);
				else
					asm_files.Add (file_name);
			}

			foreach (var file in asm_files) {
					var pdb = pdb_files.FirstOrDefault (n => MatchPdb (file, n));
					var entry = new AssemblyEntry (this, file, pdb);
					assemblies.Add (entry.Name, entry);
			}
		}

		public async Task<IList<AssemblyInfo>> Load ()
		{
			try {
				var tasks = new List<Task> ();
				foreach (var entry in assemblies.Values) {
					if (ParallelLoad)
						tasks.Add (entry.Resolve ());
					else
						await entry.Resolve ().ConfigureAwait (false);
				}
				await Task.WhenAll (tasks).ConfigureAwait (false);
			} catch (Exception e) {
				SendError ("Unexpected exception while loading assemblies", e);
			}

			return assemblies.Values.Where (e => e.Assembly != null).Select (e => e.Assembly).ToList ();
		}

		void SendError (string file, Exception error)
		{
			Console.WriteLine ($"Failed to read {file} ({error.Message})");
			var o = JObject.FromObject (new {
				entry = new {
					source = "other",
					level = "warning",
					text = $"Failed to read {file} ({error.Message})"
				}
			});
			proxy.SendEvent (session, "Log.entryAdded", o, cancellationTokenSource.Token);
		}

		async Task<byte[]> DownloadFile (string file)
		{
			try {
				return await client.GetByteArrayAsync (file).ConfigureAwait (false);
			} catch (Exception e) {
				SendError (file, e);
				return null;
			}
		}

		public override AssemblyDefinition Resolve (AssemblyNameReference name, ReaderParameters parameters)
		{
			if (cache.TryGetValue (name.FullName, out var assembly))
				return assembly;

			if (!assemblies.TryGetValue (name.Name, out var info))
				throw new AssemblyResolutionException (name);

			info.Resolve ().Wait ();
			assembly = info.Assembly.Assembly;
			cache [name.FullName] = assembly;
			return assembly;
		}

		protected void RegisterAssembly (AssemblyDefinition assembly)
		{
			if (assembly == null)
				throw new ArgumentNullException ("assembly");

			var name = assembly.Name.FullName;
			if (cache.ContainsKey (name))
				return;

			cache [name] = assembly;
		}

		protected override void Dispose (bool disposing)
		{
			foreach (var assembly in cache.Values)
				assembly.Dispose ();

			cache.Clear ();

			cancellationTokenSource.Dispose ();

			client.CancelPendingRequests ();

			client.Dispose ();

			base.Dispose (disposing);
		}
	}
}
