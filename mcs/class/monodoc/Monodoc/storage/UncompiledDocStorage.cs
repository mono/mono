using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace Monodoc.Storage
{
	// A read-only storage to access ecma XML document based on a standard directory layout
	// id are relative path inside the base doc directory
	public class UncompiledDocStorage : IDocStorage
	{
		readonly string basePath;

		public UncompiledDocStorage (string basePath)
		{
			this.basePath = basePath;
		}

		public bool SupportRevision {
			get {
				return false;
			}
		}

		public IDocRevisionManager RevisionManager {
			get {
				return null;
			}
		}

		public bool SupportChange {
			get {
				return false;
			}
		}

		public string Store (string id, string text)
		{
			throw new NotSupportedException ();
		}

		public string Store (string id, byte[] data)
		{
			throw new NotSupportedException ();
		}

		public string Store (string id, Stream stream)
		{
			throw new NotSupportedException ();
		}

		public Stream Retrieve (string id)
		{
			var path = id;
			if ('/' != Path.DirectorySeparatorChar)
				path = path.Replace ('/', Path.DirectorySeparatorChar);
			return File.OpenRead (Path.Combine (basePath, path));
		}

		public IEnumerable<string> GetAvailableIds ()
		{
			return Directory.EnumerateFiles (basePath, "*.xml", SearchOption.AllDirectories);
		}

		public void Dispose ()
		{
		}
	}
}
