using System;
using System.IO;
using System.Collections.Generic;

namespace Monodoc
{
	// Define a storage mechanism for a help source
	public interface IDocStorage : IDisposable
	{
		// Tell if the storage can store successive change to the doc as revision
		bool SupportRevision { get; }
		IDocRevisionManager RevisionManager { get; }

		// Tell if the storage support modifying an existing data
		bool SupportChange { get; }

		/* Store data inside the storage backend
		 * if SupportChange is false and user try to store something with an existing id
		 * an exception will be thrown
		 * if id is null or empty, the storage will try to create an automatic id. In all
		 * case the id that has been used to store the content is returned by the method
		 */
		string Store (string id, string text);
		string Store (string id, byte[] data);
		string Store (string id, Stream stream);

		Stream Retrieve (string id);

		IEnumerable<string> GetAvailableIds ();
	}

	public interface IDocRevisionManager
	{
		Stream RetrieveWithRevision (string id, string revision);

		// This should be ordered by most recent first
		IEnumerable<string> AvailableRevisionsForId (string id);
		// This can simply be implemented with above property but it can also be
		// a revision storage symbolic value like "HEAD"
		string LatestRevisionForId (string id);

		// A commit message for instance
		string GetRevisionDescription (string revision);
	}

	public static class DocRevisionManagerExtensions
	{
		public static Stream RetrieveLatestRevision (this IDocRevisionManager revManager, string id)
		{
			return revManager.RetrieveWithRevision (id, revManager.LatestRevisionForId (id));
		}
	}

	public static class DocStorageExtensions
	{
		public static bool TryRetrieve (this IDocStorage storage, string id, out Stream stream)
		{
			stream = null;
			try {
				stream = storage.Retrieve (id);
				return true;
			} catch {
				return false;
			}
		}
	}
}
