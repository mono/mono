using System;
using System.IO;
using System.ServiceModel;
using Mono.ServiceModel.IdentitySelectors;

namespace Mono.ServiceModel.IdentitySelectors
{
	public abstract class IdentityStore
	{
		public static IdentityStore GetDefaultStore ()
		{
			return new LocalFileIdentityStore ();
		}

		public abstract void StoreCard (IdentityCard card, string password);
	}

	public class LocalFileIdentityStore : IdentityStore
	{
		static string GetStoreFile ()
		{
			return Path.Combine (GetStorePath (), "identity.lst");
		}

		static string GetStorePath ()
		{
			// FIXME: support other alternatives
			return Path.Combine (
				Environment.GetFolderPath (Environment.SpecialFolder.ApplicationData),
				"identities");
		}

		public LocalFileIdentityStore ()
			: this (GetStoreFile ())
		{
		}

		string store_file;

		public LocalFileIdentityStore (string storeFile)
		{
			store_file = storeFile;
		}

		public override void StoreCard (IdentityCard card, string password)
		{
			// FIXME: store card both as public-only and encrypted state
		}
	}
}
