using System;
using System.IO;
using System.Text;
using System.Security.Cryptography.X509Certificates;
using Mono.Btls;

namespace Mono.Btls
{
	static class BtlsCertSync
	{
		static void Main (string[] args)
		{
			if (!MonoBtlsProvider.IsSupported ()) {
				Console.Error.WriteLine ("BTLS is not supported in this runtime!");
				Environment.Exit (255);
			}

			var configPath = Environment.GetFolderPath (Environment.SpecialFolder.ApplicationData);
			configPath = Path.Combine (configPath, ".mono");

			var oldStorePath = Path.Combine (configPath, "certs", "Trust");
			var newStorePath = MonoBtlsX509StoreManager.GetStorePath (MonoBtlsX509StoreType.UserTrustedRoots);

			if (!Directory.Exists (oldStorePath)) {
				Console.WriteLine ("Old trust store {0} does not exist.");
				Environment.Exit (255);
			}

			if (Directory.Exists (newStorePath))
				Directory.Delete (newStorePath, true);
			Directory.CreateDirectory (newStorePath);

			var oldfiles = Directory.GetFiles (oldStorePath, "*.cer");
			Console.WriteLine ("Found {0} files in the old store.", oldfiles.Length);

			foreach (var file in oldfiles) {
				Console.WriteLine ("Converting {0}.", file);
				var data = File.ReadAllBytes (file);
				using (var x509 = MonoBtlsX509.LoadFromData (data, MonoBtlsX509Format.DER)) {
					ConvertToNewFormat (newStorePath, x509);
				}
			}
		}

		static void ConvertToNewFormat (string root, MonoBtlsX509 x509)
		{
			long hash = x509.GetSubjectNameHash ();

			string newName;
			int index = 0;
			do {
				newName = Path.Combine (root, string.Format ("{0:x8}.{1}", hash, index++));
			} while (File.Exists (newName));
			Console.WriteLine ("  new name: {0}", newName);

			using (var stream = new FileStream (newName, FileMode.Create))
			using (var bio = MonoBtlsBio.CreateMonoStream (stream))
                                x509.ExportAsPEM (bio, true);
		}
	}
}
