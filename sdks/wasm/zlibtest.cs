using System;
using System.IO.Compression;
using System.IO;
using System.Text;

public class ZlibTest {
	public static void Main (String [] args)
	{

		var compressedString = CompressionUtilities.Compress ("Hello");
		var uncompressedString = CompressionUtilities.Decompress (compressedString);
		Console.WriteLine (uncompressedString);
		compressedString = CompressionUtilities.CompressGZip ("Hello GZip");
		uncompressedString = CompressionUtilities.DecompressGZip (compressedString);
		Console.WriteLine (uncompressedString);
	}

	internal static class CompressionUtilities {
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
	}

}
