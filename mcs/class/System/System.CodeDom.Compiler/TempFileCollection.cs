//
// System.CodeDom.Compiler TempFileCollection Class implementation
//
// Author:
//	Dick Porter (dick@ximian.com)
//
// (C) 2003 Ximian, Inc.
//

using System.IO;
using System.Collections;

namespace System.CodeDom.Compiler
{
	public class TempFileCollection:ICollection, IEnumerable, IDisposable
	{
		Hashtable filehash;
		string tempdir;
		bool keepfiles;
		
		public TempFileCollection(): this(null, false)
		{
		}

		public TempFileCollection(string tempDir): this(tempDir, false)
		{
		}

		public TempFileCollection(string tempDir, bool keepFiles)
		{
			filehash=new Hashtable();
			tempdir=tempDir;
			keepfiles=keepFiles;
		}

		private string basepath=null;
		
		public string BasePath
		{
			get {
				if(basepath==null) {
					if(tempdir==null) {
						/* Get the system temp dir */
						MonoIO.GetTempPath(out tempdir);
					}

					string random=new Random().Next(10000,99999).ToString();
					
					if(tempdir.EndsWith("\\") ||
					   tempdir.EndsWith("/")) {
						basepath=tempdir+random;
					} else {
						basepath=tempdir+"/"+random;
					}
				}

				return(basepath);
			}
		}

		public int Count
		{
			get {
				return(filehash.Count);
			}
		}

		public bool KeepFiles
		{
			get {
				return(keepfiles);
			}
			set {
				keepfiles=value;
			}
		}

		public string TempDir
		{
			get {
				if(tempdir==null) {
					return(String.Empty);
				} else {
					return(tempdir);
				}
			}
		}

		public string AddExtension(string fileExtension)
		{
			return(AddExtension(fileExtension, keepfiles));
		}

		public string AddExtension(string fileExtension, bool keepFile)
		{
			string filename=BasePath+"."+fileExtension;
			AddFile(filename, keepFile);
			return(filename);
		}

		public void AddFile(string fileName, bool keepFile)
		{
			filehash.Add(fileName, keepFile);
		}

		public void CopyTo(string[] fileNames, int start)
		{
			filehash.Keys.CopyTo(fileNames, start);
		}

		void ICollection.CopyTo(Array array, int start)
		{
			filehash.Keys.CopyTo(array, start);
		}

		object ICollection.SyncRoot {
			get {
				return filehash.SyncRoot;
			}
		}

		bool ICollection.IsSynchronized {
			get {
				return(false);
			}
		}
		
		void IDisposable.Dispose() 
		{
			Dispose(true);
		}
		
		public void Delete()
		{
			string[] filenames=new string[filehash.Count];
			filehash.Keys.CopyTo(filenames, 0);

			foreach(string file in filenames) {
				if((bool)filehash[file]==true) {
					File.Delete(file);
					filehash.Remove(file);
				}
			}
		}

		public IEnumerator GetEnumerator()
		{
			return(filehash.Keys.GetEnumerator());
		}

		protected virtual void Dispose(bool disposing)
		{
			Delete();
		}

		~TempFileCollection()
		{
			Dispose(false);
		}
		
	}
}
