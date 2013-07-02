using System;
using System.IO;

namespace Monodoc.Caches
{
	// This is basically a no-cache implementation
	public class NullCache : IDocCache
	{
		public bool IsCached (string id)
		{
			return false;
		}

		public bool CanCache (DocEntity entity)
		{
			return false;
		}

		public Stream GetCachedStream (string id)
		{
			return null;
		}

		public string GetCachedString (string id)
		{
			return null;
		}

		public void CacheText (string id, string content)
		{

		}

		public void CacheText (string id, Stream stream)
		{

		}

		public void CacheBlob (string id, byte[] data)
		{

		}

		public void CacheBlob (string id, Stream stream)
		{

		}

		public void Dispose ()
		{
			
		}
	}
}
