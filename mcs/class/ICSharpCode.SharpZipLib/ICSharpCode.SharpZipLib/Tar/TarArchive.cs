// TarArchive.cs
//
// Copyright (C) 2001 Mike Krueger
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either version 2
// of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.
//
// Linking this library statically or dynamically with other modules is
// making a combined work based on this library.  Thus, the terms and
// conditions of the GNU General Public License cover the whole
// combination.
//
// As a special exception, the copyright holders of this library give you
// permission to link this library with independent modules to produce an
// executable, regardless of the license terms of these independent
// modules, and to copy and distribute the resulting executable under
// terms of your choice, provided that you also meet, for each linked
// independent module, the terms and conditions of the license of that
// module.  An independent module is a module which is not derived from
// or based on this library.  If you modify this library, you may extend
// this exception to your version of the library, but you are not
// obligated to do so.  If you do not wish to do so, delete this
// exception statement from your version.

using System;
using System.IO;
using System.Text;

namespace ICSharpCode.SharpZipLib.Tar {
	/// <summary>
	/// Used to advise clients of 'events' while processing archives
	/// </summary>
	[System.ObsoleteAttribute("This assembly has been deprecated. Please use https://www.nuget.org/packages/SharpZipLib/ instead.")]
	public delegate void ProgressMessageHandler(TarArchive archive, TarEntry entry, string message);

	/// <summary>
	/// The TarArchive class implements the concept of a
	/// 'Tape Archive'. A tar archive is a series of entries, each of
	/// which represents a file system object. Each entry in
	/// the archive consists of a header block followed by 0 or more data blocks.
	/// Directory entries consist only of the header block, and are followed by entries
	/// for the directory's contents. File entries consist of a
	/// header followed by the number of blocks needed to
	/// contain the file's contents. All entries are written on
	/// block boundaries. Blocks are 512 bytes long.
	/// 
	/// TarArchives are instantiated in either read or write mode,
	/// based upon whether they are instantiated with an InputStream
	/// or an OutputStream. Once instantiated TarArchives read/write
	/// mode can not be changed.
	/// 
	/// There is currently no support for random access to tar archives.
	/// However, it seems that subclassing TarArchive, and using the
	/// TarBuffer.getCurrentRecordNum() and TarBuffer.getCurrentBlockNum()
	/// methods, this would be rather trvial.
	/// </summary>
	[System.ObsoleteAttribute("This assembly has been deprecated. Please use https://www.nuget.org/packages/SharpZipLib/ instead.")]
	public class TarArchive
	{
		bool keepOldFiles;
		bool asciiTranslate;
		
		int    userId;
		string userName;
		int    groupId;
		string groupName;
		
		string rootPath;
		string pathPrefix;
		
		int    recordSize;
		byte[] recordBuf;
		
		TarInputStream  tarIn;
		TarOutputStream tarOut;
		
		/// <summary>
		/// Client hook allowing detailed information to be reported during processing
		/// </summary>
		public event ProgressMessageHandler ProgressMessageEvent;
		
		/// <summary>
		/// Raises the ProgressMessage event
		/// </summary>
		/// <param name="entry">TarEntry for this event</param>
		/// <param name="message">message for this event.  Null is no message</param>
		protected virtual void OnProgressMessageEvent(TarEntry entry, string message)
		{
			if (ProgressMessageEvent != null) {
				ProgressMessageEvent(this, entry, message);
			}
		}
		
		/// <summary>
		/// Constructor for a TarArchive.
		/// </summary>
		protected TarArchive()
		{
		}
		
		/// <summary>
		/// The InputStream based constructors create a TarArchive for the
		/// purposes of extracting or listing a tar archive. Thus, use
		/// these constructors when you wish to extract files from or list
		/// the contents of an existing tar archive.
		/// </summary>
		public static TarArchive CreateInputTarArchive(Stream inputStream)
		{
			return CreateInputTarArchive(inputStream, TarBuffer.DefaultBlockFactor);
		}
		
		/// <summary>
		/// Create TarArchive for reading setting block factor
		/// </summary>
		/// <param name="inputStream">Stream for tar archive contents</param>
		/// <param name="blockFactor">The blocking factor to apply</param>
		/// <returns>
		/// TarArchive
		/// </returns>
		public static TarArchive CreateInputTarArchive(Stream inputStream, int blockFactor)
		{
			TarArchive archive = new TarArchive();
			archive.tarIn = new TarInputStream(inputStream, blockFactor);
			archive.Initialize(blockFactor * TarBuffer.BlockSize);
			return archive;
		}
		
		/// <summary>
		/// Create a TarArchive for writing to, using the default blocking factor
		/// </summary>
		/// <param name="outputStream">Stream to write to</param>
		public static TarArchive CreateOutputTarArchive(Stream outputStream)
		{
			return CreateOutputTarArchive(outputStream, TarBuffer.DefaultBlockFactor);
		}

		/// <summary>
		/// Create a TarArchive for writing to
		/// </summary>
		/// <param name="outputStream">The stream to write to</param>
		/// <param name="blockFactor">The blocking factor to use for buffering.</param>
		public static TarArchive CreateOutputTarArchive(Stream outputStream, int blockFactor)
		{
			TarArchive archive = new TarArchive();
			archive.tarOut = new TarOutputStream(outputStream, blockFactor);
			archive.Initialize(blockFactor * TarBuffer.BlockSize);
			return archive;
		}
		
		/// <summary>
		/// Common constructor initialization code.
		/// </summary>
		void Initialize(int recordSize)
		{
			this.recordSize = recordSize;
			this.rootPath   = null;
			this.pathPrefix = null;
			
			this.userId    = 0;
			this.userName  = String.Empty;
			this.groupId   = 0;
			this.groupName = String.Empty;
			
			this.keepOldFiles    = false;
			
			this.recordBuf = new byte[RecordSize];
		}
		
		/// <summary>
		/// Set the flag that determines whether existing files are
		/// kept, or overwritten during extraction.
		/// </summary>
		/// <param name="keepOldFiles">
		/// If true, do not overwrite existing files.
		/// </param>
		public void SetKeepOldFiles(bool keepOldFiles)
		{
			this.keepOldFiles = keepOldFiles;
		}
		
		/// <summary>
		/// Set the ascii file translation flag. If ascii file translation
		/// is true, then the file is checked to see if it a binary file or not. 
		/// If the flag is true and the test indicates it is ascii text 
		/// file, it will be translated. The translation converts the local
		/// operating system's concept of line ends into the UNIX line end,
		/// '\n', which is the defacto standard for a TAR archive. This makes
		/// text files compatible with UNIX.
		/// </summary>
		/// <param name= "asciiTranslate">
		/// If true, translate ascii text files.
		/// </param>
		public void SetAsciiTranslation(bool asciiTranslate)
		{
			this.asciiTranslate = asciiTranslate;
		}

		/// <summary>
		/// PathPrefix is added to entry names as they are written if the value is not null.
		/// A slash character is appended after PathPrefix 
		/// </summary>
		public string PathPrefix
		{
			get { return pathPrefix; }
			set { pathPrefix = value; }
		
		}
		
		/// <summary>
		/// RootPath is removed from entry names if it is found at the
		/// beginning of the name.
		/// </summary>
		public string RootPath
		{
			get { return rootPath; }
			set { rootPath = value; }
		}
		
		/// <summary>
		/// Set user and group information that will be used to fill in the
		/// tar archive's entry headers. This information based on that available 
		/// for the linux operating system, which is not always available on other
		/// operating systems.  TarArchive allows the programmer to specify values
		/// to be used in their place.
		/// </summary>
		/// <param name="userId">
		/// The user id to use in the headers.
		/// </param>
		/// <param name="userName">
		/// The user name to use in the headers.
		/// </param>
		/// <param name="groupId">
		/// The group id to use in the headers.
		/// </param>
		/// <param name="groupName">
		/// The group name to use in the headers.
		/// </param>
		public void SetUserInfo(int userId, string userName, int groupId, string groupName)
		{
			this.userId    = userId;
			this.userName  = userName;
			this.groupId   = groupId;
			this.groupName = groupName;
			applyUserInfoOverrides = true;
		}
		
		bool applyUserInfoOverrides = false;

		/// <summary>
		/// Get or set a value indicating if overrides defined by <see cref="SetUserInfo">SetUserInfo</see> should be applied.
		/// </summary>
		/// <remarks>If overrides are not applied then the values as set in each header will be used.</remarks>
		public bool ApplyUserInfoOverrides
		{
			get { return applyUserInfoOverrides; }
			set { applyUserInfoOverrides = value; }
		}

		/// <summary>
		/// Get the archive user id.
		/// See <see cref="ApplyUserInfoOverrides">ApplyUserInfoOverrides</see> for detail
		/// on how to allow setting values on a per entry basis.
		/// </summary>
		/// <returns>
		/// The current user id.
		/// </returns>
		public int UserId {
			get {
				return this.userId;
			}
		}
		
		/// <summary>
		/// Get the archive user name.
		/// See <see cref="ApplyUserInfoOverrides">ApplyUserInfoOverrides</see> for detail
		/// on how to allow setting values on a per entry basis.
		/// </summary>
		/// <returns>
		/// The current user name.
		/// </returns>
		public string UserName {
			get {
				return this.userName;
			}
		}
		
		/// <summary>
		/// Get the archive group id.
		/// See <see cref="ApplyUserInfoOverrides">ApplyUserInfoOverrides</see> for detail
		/// on how to allow setting values on a per entry basis.
		/// </summary>
		/// <returns>
		/// The current group id.
		/// </returns>
		public int GroupId {
			get {
				return this.groupId;
			}
		}
		
		/// <summary>
		/// Get the archive group name.
		/// See <see cref="ApplyUserInfoOverrides">ApplyUserInfoOverrides</see> for detail
		/// on how to allow setting values on a per entry basis.
		/// </summary>
		/// <returns>
		/// The current group name.
		/// </returns>
		public string GroupName {
			get {
				return this.groupName;
			}
		}
		
		/// <summary>
		/// Get the archive's record size. Because of its history, tar
		/// supports the concept of buffered IO consisting of RECORDS of
		/// BLOCKS. This allowed tar to match the IO characteristics of
		/// the physical device being used. Of course, in the C# world,
		/// this makes no sense, WITH ONE EXCEPTION - archives are expected
		/// to be properly "blocked". Thus, all of the horrible TarBuffer
		/// support boils down to simply getting the "boundaries" correct.
		/// </summary>
		/// <returns>
		/// The record size this archive is using.
		/// </returns>
		public int RecordSize {
			get {
				if (this.tarIn != null) {
					return this.tarIn.GetRecordSize();
				} else if (this.tarOut != null) {
					return this.tarOut.GetRecordSize();
				}
				return TarBuffer.DefaultRecordSize;
			}
		}
		
		/// <summary>
		/// Close the archive. This simply calls the underlying
		/// tar stream's close() method.
		/// </summary>
		public void CloseArchive()
		{
			if (this.tarIn != null) {
				this.tarIn.Close();
			} else if (this.tarOut != null) {
				this.tarOut.Flush();
				this.tarOut.Close();
			}
		}
		
		/// <summary>
		/// Perform the "list" command for the archive contents.
		/// 
		/// NOTE That this method uses the <see cref="ProgressMessageEvent"> progress event</see> to actually list
		/// the contents. If the progress display event is not set, nothing will be listed!
		/// </summary>
		public void ListContents()
		{
			while (true) {
				TarEntry entry = this.tarIn.GetNextEntry();
				
				if (entry == null) {
					break;
				}
				OnProgressMessageEvent(entry, null);
			}
		}
		
		/// <summary>
		/// Perform the "extract" command and extract the contents of the archive.
		/// </summary>
		/// <param name="destDir">
		/// The destination directory into which to extract.
		/// </param>
		public void ExtractContents(string destDir)
		{
			while (true) {
				TarEntry entry = this.tarIn.GetNextEntry();
				
				if (entry == null) {
					break;
				}
				
				this.ExtractEntry(destDir, entry);
			}
		}
		
		void EnsureDirectoryExists(string directoryName)
		{
			if (!Directory.Exists(directoryName)) {
				try {
					Directory.CreateDirectory(directoryName);
				}
				catch (Exception e) {
					throw new TarException("Exception creating directory '" + directoryName + "', " + e.Message);
				}
			}
		}
		
		// TODO: Is there a better way to test for a text file?
		// It no longer reads entire files into memory but is still a weak test!
		// assumes that ascii 0-7, 14-31 or 255 are binary
		// and that all non text files contain one of these values
		bool IsBinary(string filename)
		{
			using (FileStream fs = File.OpenRead(filename))
			{
				int sampleSize = System.Math.Min(4096, (int)fs.Length);
				byte[] content = new byte[sampleSize];
			
				int bytesRead = fs.Read(content, 0, sampleSize);
			
				for (int i = 0; i < bytesRead; ++i) {
					byte b = content[i];
					if (b < 8 || (b > 13 && b < 32) || b == 255) {
						return true;
					}
				}
			}
			return false;
		}		
		
		/// <summary>
		/// Extract an entry from the archive. This method assumes that the
		/// tarIn stream has been properly set with a call to getNextEntry().
		/// </summary>
		/// <param name="destDir">
		/// The destination directory into which to extract.
		/// </param>
		/// <param name="entry">
		/// The TarEntry returned by tarIn.getNextEntry().
		/// </param>
		void ExtractEntry(string destDir, TarEntry entry)
		{
			OnProgressMessageEvent(entry, null);
			
			string name = entry.Name;
			
			if (Path.IsPathRooted(name) == true) {
				// NOTE:
				// for UNC names...  \\machine\share\zoom\beet.txt gives \zoom\beet.txt
				name = name.Substring(Path.GetPathRoot(name).Length);
			}
			
			name = name.Replace('/', Path.DirectorySeparatorChar);
			
			string destFile = Path.Combine(destDir, name);
			
			if (entry.IsDirectory) {
				EnsureDirectoryExists(destFile);
			} else {
				string parentDirectory = Path.GetDirectoryName(destFile);
				EnsureDirectoryExists(parentDirectory);
				
				bool process = true;
				FileInfo fileInfo = new FileInfo(destFile);
				if (fileInfo.Exists) {
					if (this.keepOldFiles) {
						OnProgressMessageEvent(entry, "Destination file already exists");
						process = false;
					} else if ((fileInfo.Attributes & FileAttributes.ReadOnly) != 0) {
						OnProgressMessageEvent(entry, "Destination file already exists, and is read-only");
						process = false;
					}
				}
				
				if (process) {
					bool asciiTrans = false;
					
					Stream outputStream = File.Create(destFile);
					if (this.asciiTranslate) {
						asciiTrans = !IsBinary(destFile);
					}
					
					StreamWriter outw = null;
					if (asciiTrans) {
						outw = new StreamWriter(outputStream);
					}
					
					byte[] rdbuf = new byte[32 * 1024];
					
					while (true) {
						int numRead = this.tarIn.Read(rdbuf, 0, rdbuf.Length);
						
						if (numRead <= 0) {
							break;
						}
						
						if (asciiTrans) {
							for (int off = 0, b = 0; b < numRead; ++b) {
								if (rdbuf[b] == 10) {
									string s = Encoding.ASCII.GetString(rdbuf, off, (b - off));
									outw.WriteLine(s);
									off = b + 1;
								}
							}
						} else {
							outputStream.Write(rdbuf, 0, numRead);
						}
					}
					
					if (asciiTrans) {
						outw.Close();
					} else {
						outputStream.Close();
					}
				}
			}
		}

		/// <summary>
		/// Write an entry to the archive. This method will call the putNextEntry
		/// and then write the contents of the entry, and finally call closeEntry()
		/// for entries that are files. For directories, it will call putNextEntry(),
		/// and then, if the recurse flag is true, process each entry that is a
		/// child of the directory.
		/// </summary>
		/// <param name="sourceEntry">
		/// The TarEntry representing the entry to write to the archive.
		/// </param>
		/// <param name="recurse">
		/// If true, process the children of directory entries.
		/// </param>
		public void WriteEntry(TarEntry sourceEntry, bool recurse)
		{
			try
			{
				if ( recurse ) {
					TarHeader.SetValueDefaults(sourceEntry.UserId, sourceEntry.UserName,
					                           sourceEntry.GroupId, sourceEntry.GroupName);
				}
				InternalWriteEntry(sourceEntry, recurse);
			}
			finally
			{
				if ( recurse ) {
					TarHeader.RestoreSetValues();
				}
			}
		}
		
		/// <summary>
		/// Write an entry to the archive. This method will call the putNextEntry
		/// and then write the contents of the entry, and finally call closeEntry()
		/// for entries that are files. For directories, it will call putNextEntry(),
		/// and then, if the recurse flag is true, process each entry that is a
		/// child of the directory.
		/// </summary>
		/// <param name="sourceEntry">
		/// The TarEntry representing the entry to write to the archive.
		/// </param>
		/// <param name="recurse">
		/// If true, process the children of directory entries.
		/// </param>
		void InternalWriteEntry(TarEntry sourceEntry, bool recurse)
		{
			bool asciiTrans = false;
			
			string tempFileName = null;
			string entryFilename   = sourceEntry.File;
			
			TarEntry entry = (TarEntry)sourceEntry.Clone();

			if ( applyUserInfoOverrides ) {
				entry.GroupId = groupId;
				entry.GroupName = groupName;
				entry.UserId = userId;
				entry.UserName = userName;
			}
			
			OnProgressMessageEvent(entry, null);
			
			if (this.asciiTranslate && !entry.IsDirectory) {
				asciiTrans = !IsBinary(entryFilename);

				if (asciiTrans) {
					tempFileName = Path.GetTempFileName();
					
					StreamReader inStream  = File.OpenText(entryFilename);
					Stream       outStream = File.Create(tempFileName);
					
					while (true) {
						string line = inStream.ReadLine();
						if (line == null) {
							break;
						}
						byte[] data = Encoding.ASCII.GetBytes(line);
						outStream.Write(data, 0, data.Length);
						outStream.WriteByte((byte)'\n');
					}
					
					inStream.Close();

					outStream.Flush();
					outStream.Close();
					
					entry.Size = new FileInfo(tempFileName).Length;
					
					entryFilename = tempFileName;
				}
			}
			
			string newName = null;
		
			if (this.rootPath != null) {
				if (entry.Name.StartsWith(this.rootPath)) {
					newName = entry.Name.Substring(this.rootPath.Length + 1 );
				}
			}
			
			if (this.pathPrefix != null) {
				newName = (newName == null) ? this.pathPrefix + "/" + entry.Name : this.pathPrefix + "/" + newName;
			}
			
			if (newName != null) {
				entry.Name = newName;
			}
			
			this.tarOut.PutNextEntry(entry);
			
			if (entry.IsDirectory) {
				if (recurse) {
					TarEntry[] list = entry.GetDirectoryEntries();
					for (int i = 0; i < list.Length; ++i) {
						InternalWriteEntry(list[i], recurse);
					}
				}
			} else {
				Stream inputStream = File.OpenRead(entryFilename);
				int numWritten = 0;
				byte[] eBuf = new byte[32 * 1024];
				while (true) {
					int numRead = inputStream.Read(eBuf, 0, eBuf.Length);
					
					if (numRead <=0) {
						break;
					}
					
					this.tarOut.Write(eBuf, 0, numRead);
					numWritten +=  numRead;
				}

				inputStream.Close();
				
				if (tempFileName != null && tempFileName.Length > 0) {
					File.Delete(tempFileName);
				}
				
				this.tarOut.CloseEntry();
			}
		}
	}
}


/* The original Java file had this header:
	** Authored by Timothy Gerard Endres
	** <mailto:time@gjt.org>  <http://www.trustice.com>
	**
	** This work has been placed into the public domain.
	** You may use this work in any way and for any purpose you wish.
	**
	** THIS SOFTWARE IS PROVIDED AS-IS WITHOUT WARRANTY OF ANY KIND,
	** NOT EVEN THE IMPLIED WARRANTY OF MERCHANTABILITY. THE AUTHOR
	** OF THIS SOFTWARE, ASSUMES _NO_ RESPONSIBILITY FOR ANY
	** CONSEQUENCE RESULTING FROM THE USE, MODIFICATION, OR
	** REDISTRIBUTION OF THIS SOFTWARE.
	**
	*/

