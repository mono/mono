using System;
using System.Collections.Generic;
using System.Net.Http;
using WebAssembly.Net.Http.HttpClient;
using System.Threading.Tasks;
using WebAssembly;
using System.Threading;
using System.IO;
using System.IO.Compression;

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

		static async Task<MemoryStream> GetArchiveStreamAsync ()
		{
			MemoryStream archiveStream;
			using (HttpClient client = CreateHttpClient ())
			using (Stream stream = await client.GetStreamAsync ("base/publish/archive.zip")) {
				archiveStream = new MemoryStream ();
				await stream.CopyToAsync (archiveStream);
			}
			return archiveStream;
		}

		static HttpClient CreateHttpClient ()
		{
			string BaseApiUrl = string.Empty;
			var window = (JSObject)WebAssembly.Runtime.GetGlobalObject ("window");
			using (var location = (JSObject)window.GetObjectProperty ("location")) {
				BaseApiUrl = (string)location.GetObjectProperty ("origin");
			}
			WasmHttpMessageHandler.StreamingEnabled = true;
			return new HttpClient () { BaseAddress = new Uri (BaseApiUrl) };
		}
	}

}