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

			using (HttpClient client = CreateHttpClient ())
			using (Stream stream = await client.GetStreamAsync ("base/publish/archive.zip"))
			using (var memoryStream = new MemoryStream ()) {
				await stream.CopyToAsync (memoryStream);
				using (var archive = new ZipArchive (memoryStream, ZipArchiveMode.Read)) {
					var entry = archive.GetEntry (zipEntry);
					requestTcs.SetResult (entry);
				}
			}
			return requestTcs.Task;

		}
		public static async Task<object> GetStreamAsync_ReadZeroBytes_Success ()
		{
			var requestTcs = new TaskCompletionSource<object> ();

			using (HttpClient client = CreateHttpClient ())
			using (Stream stream = await client.GetStreamAsync ("base/publish/NowIsTheTime.txt")) {
				requestTcs.SetResult (await stream.ReadAsync (new byte [1], 0, 0));
			}

			return requestTcs.Task;
		}

		static HttpClient CreateHttpClient ()
		{
			//Console.WriteLine("Create  HttpClient");
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