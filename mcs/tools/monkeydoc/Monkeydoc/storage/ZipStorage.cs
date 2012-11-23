using System;
using System.IO;
using System.Xml;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using ICSharpCode.SharpZipLib.Zip;

namespace MonkeyDoc.Storage
{
	public class ZipStorage : IDocStorage
	{
		ZipOutputStream zipOutput;
		string zipFileName;
		ZipFile zipFile;
		int code;

		public ZipStorage (string zipFileName)
		{
			this.zipFileName = zipFileName;
			this.zipFile = new ZipFile (zipFileName);
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
			SetupEntry (ref id);
			using (var writer = new StreamWriter (zipOutput)) {
				writer.Write (text);
				writer.Flush ();
			}
			return id;
		}

		public string Store (string id, byte[] data)
		{
			SetupEntry (ref id);
			zipOutput.Write (data, 0, data.Length);
			return id;
		}

		public string Store (string id, Stream stream)
		{
			SetupEntry (ref id);
			stream.CopyTo (zipOutput);
			return id;
		}

		void SetupEntry (ref string id)
		{
			if (string.IsNullOrEmpty (id))
				id = GetNewCode ();

			ZipEntry entry = new ZipEntry (id);
			zipOutput.PutNextEntry (entry);
		}

		public Stream Retrieve (string id)
		{
			ZipEntry entry = zipFile.GetEntry (id);
			if (entry != null)
				return zipFile.GetInputStream (entry);
			else
				throw new ArgumentException ("id", string.Format ("'{0}' isn't a valid id for this storage", id));
		}

		public IEnumerable<string> GetAvailableIds ()
		{
			return zipFile.Cast<ZipEntry> ().Select (ze => ze.Name);
		}

		public void Dispose ()
		{
			zipOutput.Dispose ();
		}

		string GetNewCode ()
		{
			return String.Format ("{0}", code++);
		}
	}
}