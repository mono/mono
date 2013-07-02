using System;
using System.Linq;
using System.IO;
using System.Configuration;
using System.Collections.Specialized;
using Monodoc.Caches;

namespace Monodoc
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

	public static class DocCacheHelper
	{
		static string cacheBaseDirectory;

		static DocCacheHelper ()
		{
			try {
				var cacheValues = Config.Get ("cache").Split (',');
				if (cacheValues.Length == 2 && cacheValues[0].Equals ("file", StringComparison.Ordinal))
					cacheBaseDirectory = cacheValues[1].Replace ("~", Environment.GetFolderPath (Environment.SpecialFolder.Personal));
			} catch {}
		}

		// Use configuration option to query for cache directory, if it doesn't exist we instantiate a nullcache
		public static IDocCache GetDefaultCache (string name)
		{
			if (cacheBaseDirectory == null)
				return new NullCache ();

			return new FileCache (Path.Combine (cacheBaseDirectory, name));
		}
	}
}
