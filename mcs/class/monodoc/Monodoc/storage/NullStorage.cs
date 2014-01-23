using System;
using System.IO;
using System.Xml;
using System.Linq;
using System.Collections.Generic;

namespace Monodoc.Storage
{
	// A storage that doesn't store
	public class NullStorage : IDocStorage
	{
		public NullStorage ()
		{
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
				return true;
			}
		}

		public string Store (string id, string text)
		{
			return id;
		}

		public string Store (string id, byte[] data)
		{
			return id;
		}

		public string Store (string id, Stream stream)
		{
			return id;
		}

		public Stream Retrieve (string id)
		{
			return null;
		}

		public IEnumerable<string> GetAvailableIds ()
		{
			return Enumerable.Empty<string> (); 
		}

		public void Dispose ()
		{
		}
	}
}
