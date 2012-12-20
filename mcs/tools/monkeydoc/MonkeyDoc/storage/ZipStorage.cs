using System;
using System.IO;
using System.Xml;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using ICSharpCode.SharpZipLib.Zip;

namespace Monodoc.Storage
{
	public class ZipStorage : IDocStorage
	{
		string zipFileName;
		int code;
		ZipOutputStream zipOutput;
		ZipFile zipFile;
		// SharpZipLib use linear search to map name to index, correct that a bit
		Dictionary<string, int> entries = new Dictionary<string, int> ();

		public ZipStorage (string zipFileName)
		{
			this.zipFileName = zipFileName;
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
			EnsureOutput ();
			SetupEntry (zipOutput, ref id);
			var writer = new StreamWriter (zipOutput);
			writer.Write (text);
			writer.Flush ();
			
			return id;
		}

		public string Store (string id, byte[] data)
		{
			EnsureOutput ();
			SetupEntry (zipOutput, ref id);
			zipOutput.Write (data, 0, data.Length);
			return id;
		}

		public string Store (string id, Stream stream)
		{
			EnsureOutput ();
			SetupEntry (zipOutput, ref id);
			stream.CopyTo (zipOutput);
			return id;
		}

		void SetupEntry (ZipOutputStream zipOutput, ref string id)
		{
			if (string.IsNullOrEmpty (id))
				id = GetNewCode ();

			ZipEntry entry = new ZipEntry (id);
			zipOutput.PutNextEntry (entry);
		}

		public Stream Retrieve (string id)
		{
			EnsureInput ();
			int index;
			ZipEntry entry;
			if (!entries.TryGetValue (id, out index) || (entry = zipFile[index]) == null)
				entry = zipFile.GetEntry (id);
			if (entry != null)
				return zipFile.GetInputStream (entry);
			else
				throw new ArgumentException ("id", string.Format ("'{0}' isn't a valid id for this storage", id));
		}

		public IEnumerable<string> GetAvailableIds ()
		{
			EnsureInput ();
			return zipFile.Cast<ZipEntry> ().Select (ze => ze.Name);
		}

		void EnsureOutput ()
		{
			if (zipFile != null)
				throw new InvalidOperationException ("This ZipStorage instance is already used in read-mode");
			if (zipOutput != null)
				return;
			zipOutput = new ZipOutputStream (File.Create (zipFileName));
		}

		void EnsureInput ()
		{
			if (zipOutput != null)
				throw new InvalidOperationException ("This ZipStorage instance is already used in write-mode");
			if (zipFile != null)
				return;
			zipFile = new ZipFile (zipFileName);
			entries = Enumerable.Range (0, zipFile.Size).ToDictionary (i => zipFile[i].Name, i => i);
		}

		public void Dispose ()
		{
			if (zipOutput != null)
				zipOutput.Dispose ();
			if (zipFile != null)
				zipFile.Close ();
		}

		string GetNewCode ()
		{
			return String.Format ("{0}", code++);
		}
	}
}
