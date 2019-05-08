using System;
using System.Collections.Generic;
using System.Net.Http;
using WebAssembly.Net.Http.HttpClient;
using System.Threading.Tasks;
using WebAssembly;
using System.Threading;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace TestSuite {
	public class Program {

		public static async Task<object> ZipGetEntryReadMode (string zipEntry)
		{
			var requestTcs = new TaskCompletionSource<object> ();

			using (var memoryStream = await GetArchiveStreamAsync ()) {
				using (var archive = new ZipArchive (memoryStream, ZipArchiveMode.Read)) {
					var entry = archive.GetEntry (zipEntry);
					requestTcs.SetResult (entry);
				}
			}
			return requestTcs.Task;

		}
		public static async Task<object> ZipGetEntryCreateMode ()
		{
			var requestTcs = new TaskCompletionSource<object> ();

			using (var memoryStream = await GetArchiveStreamAsync ()) {
				using (var archive = new ZipArchive (memoryStream, ZipArchiveMode.Create)) {
					try {
						archive.GetEntry ("foo");
						requestTcs.SetResult (true);
					} catch (NotSupportedException ex) {
						requestTcs.SetException (ex); ;
					}
				}
			}
			return requestTcs.Task;

		}

		public static async Task<object> ZipGetEntryUpdateMode (string zipEntry)
		{
			var requestTcs = new TaskCompletionSource<object> ();

			using (var memoryStream = await GetArchiveStreamAsync ()) {
				using (var archive = new ZipArchive (memoryStream, ZipArchiveMode.Update)) {
					var entry = archive.GetEntry (zipEntry);
					requestTcs.SetResult (entry);
				}
			}
			return requestTcs.Task;

		}

		public static async Task<object> ZipGetEntryOpen ()
		{
			var requestTcs = new TaskCompletionSource<object> ();

			using (var memoryStream = await GetArchiveStreamAsync ()) {
				using (var archive = new ZipArchive (memoryStream, ZipArchiveMode.Read)) {
					var entry = archive.GetEntry ("foo.txt");
					var foo = entry.Open ();
					requestTcs.SetResult (foo);
				}
			}
			return requestTcs.Task;

		}
		public static async Task<object> ZipOpenAndReopenEntry ()
		{
			var requestTcs = new TaskCompletionSource<object> ();

			using (var memoryStream = await GetArchiveStreamAsync ()) {
				using (var archive = new ZipArchive (memoryStream, ZipArchiveMode.Update)) {
					var entry = archive.GetEntry ("foo.txt");
					var stream = entry.Open ();

					try {
						stream = entry.Open ();
						requestTcs.SetResult (true);
					} catch (global::System.IO.IOException ex) {
						requestTcs.SetException (ex);
					}
				}
			}
			return requestTcs.Task;

		}

		public static async Task<object> ZipOpenCloseAndReopenEntry ()
		{
			var requestTcs = new TaskCompletionSource<object> ();

			using (var memoryStream = await GetArchiveStreamAsync ()) {
				using (var archive = new ZipArchive (memoryStream, ZipArchiveMode.Update)) {
					var entry = archive.GetEntry ("foo.txt");
					var stream = entry.Open ();
					stream.Dispose ();
					stream = entry.Open ();
					requestTcs.SetResult (true);
				}
			}
			return requestTcs.Task;

		}
		public static async Task<object> ZipDeleteEntryCheckEntries ()
		{
			var requestTcs = new TaskCompletionSource<object> ();

			using (var memoryStream = await GetArchiveStreamAsync ()) {
				using (var archive = new ZipArchive (memoryStream, ZipArchiveMode.Update)) {
					var entry = archive.GetEntry ("foo.txt");
					entry.Delete ();
					requestTcs.SetResult (archive.Entries.FirstOrDefault (e => e == entry));
				}
			}
			return requestTcs.Task;

		}
		public static async Task<object> ZipGetEntryDeleteUpdateMode ()
		{
			var requestTcs = new TaskCompletionSource<object> ();

			using (var memoryStream = await GetArchiveStreamAsync ()) {
				using (var archive = new ZipArchive (memoryStream, ZipArchiveMode.Update, true)) {
					var entry = archive.GetEntry ("foo.txt");
					entry.Delete ();
				}
				using (var archive = new ZipArchive (memoryStream, ZipArchiveMode.Read)) {
					var entry = archive.GetEntry ("foo.txt");
					requestTcs.SetResult (entry);
				}

			}
			return requestTcs.Task;

		}

		public static Task<object> ZipCreateArchive ()
		{
			var requestTcs = new TaskCompletionSource<object> ();

			using (var memoryStream = new MemoryStream ()) {
				using (var archive = new ZipArchive (memoryStream, ZipArchiveMode.Create, true)) {
					var dir = archive.CreateEntry ("foobar/");

					var entry = archive.CreateEntry ("foo.txt");
					using (var stream = entry.Open ()) {
						using (var streamWriter = new StreamWriter (stream))
							streamWriter.Write ("foo");
					}
				}
				using (var archive = new ZipArchive (memoryStream, ZipArchiveMode.Read)) {
					var returnArray = new WebAssembly.Core.Array ();
					returnArray.Push (archive.GetEntry ("foobar/"));

					var entry = archive.GetEntry ("foo.txt");
					returnArray.Push (entry);

					var streamReader = new StreamReader (entry.Open ());
					var text = streamReader.ReadToEnd ();

					returnArray.Push (text);

					requestTcs.SetResult (returnArray);
				}

			}
			return requestTcs.Task;

		}
		public static async Task<object> ZipEnumerateEntriesModifiedTime ()
		{
			var requestTcs = new TaskCompletionSource<object> ();
			var date = DateTimeOffset.Now;
			using (var memoryStream = await GetArchiveStreamAsync ()) {
				using (var archive = new ZipArchive (memoryStream, ZipArchiveMode.Update, true)) {
					var entry = archive.GetEntry ("foo.txt");
					entry.LastWriteTime = date;
				}
				using (var archive = new ZipArchive (memoryStream, ZipArchiveMode.Read)) {
					var returnArray = new WebAssembly.Core.Array ();
					var entry = archive.GetEntry ("foo.txt");
					returnArray.Push (entry.LastWriteTime.Year);
					returnArray.Push (entry.LastWriteTime.Month);
					returnArray.Push (entry.LastWriteTime.Day);

					requestTcs.SetResult (returnArray);
				}

			}
			return requestTcs.Task;

		}
		public static async Task<object> ZipEnumerateArchiveDefaultLastWriteTime ()
		{
			var requestTcs = new TaskCompletionSource<object> ();
			using (var memoryStream = await GetPackageArchiveStreamAsync ()) {
				using (var archive = new ZipArchive (memoryStream, ZipArchiveMode.Read)) {
					var entry = archive.GetEntry ("_rels/.rels");
					requestTcs.SetResult (entry.LastWriteTime.Ticks);
				}

			}
			return requestTcs.Task;

		}

		public static async Task<WebAssembly.Core.Array> ZipGetArchiveEntryStreamLengthPosition (ZipArchiveMode mode)
		{
			var requestTcs = new TaskCompletionSource<WebAssembly.Core.Array> ();

			using (var memoryStream = await GetPackageArchiveStreamAsync ()) {
				using (var archive = new ZipArchive (memoryStream, mode)) {
					var resultsArray = new WebAssembly.Core.Array ();
					var entry = archive.GetEntry ("_rels/.rels");

					using (var stream = entry.Open ()) {
						resultsArray.Push ((double)stream.Position);
						resultsArray.Push ((double)stream.Length);
					}

					var entry2 = archive.GetEntry ("modernhttpclient.nuspec");
					using (var stream = entry2.Open ()) {
						// .NET does not support these in Read mode
						if (mode == ZipArchiveMode.Update) {
							resultsArray.Push ((double)stream.Length);
							resultsArray.Push ((double)stream.Position);
						}
					}
					requestTcs.SetResult (resultsArray);
				}
			}

			return await requestTcs.Task;
		}
		public static async Task<object> ZipGetArchiveEntryStreamLengthPositionReadMode ()
		{
			var requestTcs = new TaskCompletionSource<object> ();
			var resultsArray = await ZipGetArchiveEntryStreamLengthPosition (ZipArchiveMode.Read);
			requestTcs.SetResult (resultsArray);
			return requestTcs.Task;

		}
		public static async Task<object> ZipGetArchiveEntryStreamLengthPositionUpdateMode ()
		{
			var requestTcs = new TaskCompletionSource<object> ();
			var resultsArray = await ZipGetArchiveEntryStreamLengthPosition (ZipArchiveMode.Update);
			requestTcs.SetResult (resultsArray);
			return requestTcs.Task;

		}

		static async Task<MemoryStream> GetArchiveStreamAsync ()
		{
			MemoryStream archiveStream = new MemoryStream ();
			using (HttpClient client = CreateHttpClient ())
			using (Stream stream = await client.GetStreamAsync ("base/publish/archive.zip")) {
				await stream.CopyToAsync (archiveStream);
			}
			return archiveStream;
		}
		static async Task<MemoryStream> GetPackageArchiveStreamAsync ()
		{
			MemoryStream archiveStream = new MemoryStream ();
			using (HttpClient client = CreateHttpClient ())
			using (Stream stream = await client.GetStreamAsync ("base/publish/test.nupkg")) {
				await stream.CopyToAsync (archiveStream);
			}
			return archiveStream;
		}

		static HttpClient CreateHttpClient ()
		{
			string BaseApiUrl = string.Empty;
			using (var window = (JSObject)WebAssembly.Runtime.GetGlobalObject ("window"))
			using (var location = (JSObject)window.GetObjectProperty ("location")) {
				BaseApiUrl = (string)location.GetObjectProperty ("origin");
			}
			return new HttpClient () { BaseAddress = new Uri (BaseApiUrl) };
		}
	}

}