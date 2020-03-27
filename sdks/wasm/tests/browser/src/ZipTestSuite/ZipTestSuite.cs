using System;
using System.Net.Http;
using System.Threading.Tasks;
using WebAssembly;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

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
			var date = DateTimeOffset.UtcNow;
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

		public static async Task<object> ZipEnumerateEntriesReadMode ()
		{
			var requestTcs = new TaskCompletionSource<object> ();
			using (var memoryStream = await GetArchiveStreamAsync ()) {
				using (var archive = new ZipArchive (memoryStream, ZipArchiveMode.Read)) {
					var resultsArray = new WebAssembly.Core.Array ();
					var entries = archive.Entries;
					resultsArray.Push (entries [0].FullName);
					resultsArray.Push (entries [1].FullName);
					resultsArray.Push (entries [2].FullName);
					resultsArray.Push (entries [3].FullName);
					resultsArray.Push (entries [4].FullName);
					requestTcs.SetResult (resultsArray);
				}

			}
			return requestTcs.Task;

		}
		public static async Task<object> ZipEnumerateEntriesUpdateMode ()
		{
			var requestTcs = new TaskCompletionSource<object> ();
			using (var memoryStream = await GetArchiveStreamAsync ()) {
				using (var archive = new ZipArchive (memoryStream, ZipArchiveMode.Update)) {
					var resultsArray = new WebAssembly.Core.Array ();
					var entries = archive.Entries;
					resultsArray.Push (entries [0].FullName);
					resultsArray.Push (entries [1].FullName);
					resultsArray.Push (entries [2].FullName);
					resultsArray.Push (entries [3].FullName);
					resultsArray.Push (entries [4].FullName);
					requestTcs.SetResult (resultsArray);
				}

			}
			return requestTcs.Task;

		}
		public static async Task<object> ZipEnumerateEntriesCreateMode ()
		{
			var requestTcs = new TaskCompletionSource<object> ();
			using (var memoryStream = await GetArchiveStreamAsync ()) {
				using (var archive = new ZipArchive (memoryStream, ZipArchiveMode.Create)) {
					try {
						archive.Entries.ToList ();
					} catch (NotSupportedException ex) {
						requestTcs.SetException (ex); ;
					}
					requestTcs.SetResult (true);
				}

			}
			return requestTcs.Task;

		}
		public static Task<object> ZipUpdateEmptyArchive ()
		{
			var requestTcs = new TaskCompletionSource<object> ();
			using (var memoryStream = new MemoryStream ()) {
				using (var archive = new ZipArchive (memoryStream, ZipArchiveMode.Update)) {
					requestTcs.SetResult (true);
				}

			}
			return requestTcs.Task;

		}

		public static string Compress (string uncompressedString)
		{

			byte [] compressedBytes;

			using (var uncompressedStream = new MemoryStream (Encoding.UTF8.GetBytes (uncompressedString))) {
				using (var compressedStream = new MemoryStream ()) {
					// setting the leaveOpen parameter to true to ensure that compressedStream will not be closed when compressorStream is disposed
					// this allows compressorStream to close and flush its buffers to compressedStream and guarantees that compressedStream.ToArray() can be called afterward
					// although MSDN documentation states that ToArray() can be called on a closed MemoryStream, I don't want to rely on that very odd behavior should it ever change
					using (var compressorStream = new DeflateStream (compressedStream, CompressionLevel.Fastest, true)) {
						uncompressedStream.CopyTo (compressorStream);
					}

					// call compressedStream.ToArray() after the enclosing DeflateStream has closed and flushed its buffer to compressedStream
					compressedBytes = compressedStream.ToArray ();
				}
			}

			return Convert.ToBase64String (compressedBytes);

		}

		public static string Decompress (string compressedString)
		{
			byte [] decompressedBytes;

			var compressedStream = new MemoryStream (Convert.FromBase64String (compressedString));

			using (var decompressorStream = new DeflateStream (compressedStream, CompressionMode.Decompress)) {
				using (var decompressedStream = new MemoryStream ()) {
					decompressorStream.CopyTo (decompressedStream);

					decompressedBytes = decompressedStream.ToArray ();
				}
			}

			return Encoding.UTF8.GetString (decompressedBytes);
		}

		public static string CompressGZip (string uncompressedString)
		{
			byte [] compressedBytes;

			using (var uncompressedStream = new MemoryStream (Encoding.UTF8.GetBytes (uncompressedString))) {
				using (var compressedStream = new MemoryStream ()) {
					using (var gzip = new GZipStream (compressedStream,
						CompressionMode.Compress, true)) {
						uncompressedStream.CopyTo (gzip);
					}

					// call compressedStream.ToArray() after the enclosing DeflateStream has closed and flushed its buffer to compressedStream
					compressedBytes = compressedStream.ToArray ();
				}
			}

			return Convert.ToBase64String (compressedBytes);

		}

		public static string DecompressGZip (string compressedString)
		{
			byte [] decompressedBytes;

			var compressedStream = new MemoryStream (Convert.FromBase64String (compressedString));

			using (var decompressorStream = new GZipStream (compressedStream, CompressionMode.Decompress)) {
				using (var decompressedStream = new MemoryStream ()) {
					decompressorStream.CopyTo (decompressedStream);

					decompressedBytes = decompressedStream.ToArray ();
				}
			}

			return Encoding.UTF8.GetString (decompressedBytes);
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