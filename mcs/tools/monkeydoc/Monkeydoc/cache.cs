using System;
using System.IO;

namespace MonkeyDoc
{
	public enum DocEntity
	{
		Text,
		Blob
	}

	public interface IDocCache : IDisposable
	{
		bool IsCached (string id);
		bool CanCache (DocEntity entity);

		Stream GetCachedStream (string id);
		string GetCachedString (string id);

		void CacheText (string id, string content);
		void CacheText (string id, Stream stream);

		void CacheBlob (string id, byte[] data);
		void CacheBlob (string id, Stream stream);
	}
}