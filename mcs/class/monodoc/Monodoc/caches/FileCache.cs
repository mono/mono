using System;
using System.IO;

namespace Monodoc.Caches
{
	public class FileCache : IDocCache
	{
		string baseCacheDir;

		public FileCache (string baseCacheDir)
		{
			this.baseCacheDir = baseCacheDir;
			if (!Directory.Exists (baseCacheDir))
				Directory.CreateDirectory (baseCacheDir);
		}

		public bool IsCached (string id)
		{
			return File.Exists (MakePath (id));
		}

		public bool CanCache (DocEntity entity)
		{
			return true;
		}

		public Stream GetCachedStream (string id)
		{
			return File.OpenRead (MakePath (id));
		}

		public string GetCachedString (string id)
		{
			return File.ReadAllText (MakePath (id));
		}

		public void CacheText (string id, string content)
		{
			File.WriteAllText (MakePath (id), content);
		}

		public void CacheText (string id, Stream stream)
		{
			using (var file = File.OpenWrite (MakePath (id)))
				stream.CopyTo (file);
		}

		public void CacheBlob (string id, byte[] data)
		{
			File.WriteAllBytes (MakePath (id), data);
		}

		public void CacheBlob (string id, Stream stream)
		{
			using (var file = File.OpenWrite (MakePath (id)))
				stream.CopyTo (file);
		}

		string MakePath (string id)
		{
			id = id.Replace (Path.DirectorySeparatorChar, '_');
			return Path.Combine (baseCacheDir, id);
		}

		public void Dispose ()
		{
			if (!Directory.Exists (baseCacheDir))
				return;

			try {
				Directory.Delete (baseCacheDir, true);
			} catch {}
		}
	}
}
