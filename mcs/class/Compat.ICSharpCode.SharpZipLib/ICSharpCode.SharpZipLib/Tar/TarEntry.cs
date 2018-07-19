// TarEntry.cs
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

namespace ICSharpCode.SharpZipLib.Tar 
{
	
	/// <summary>
	/// This class represents an entry in a Tar archive. It consists
	/// of the entry's header, as well as the entry's File. Entries
	/// can be instantiated in one of three ways, depending on how
	/// they are to be used.
	/// <p>
	/// TarEntries that are created from the header bytes read from
	/// an archive are instantiated with the TarEntry( byte[] )
	/// constructor. These entries will be used when extracting from
	/// or listing the contents of an archive. These entries have their
	/// header filled in using the header bytes. They also set the File
	/// to null, since they reference an archive entry not a file.</p>
	/// <p>
	/// TarEntries that are created from Files that are to be written
	/// into an archive are instantiated with the TarEntry( File )
	/// constructor. These entries have their header filled in using
	/// the File's information. They also keep a reference to the File
	/// for convenience when writing entries.</p>
	/// <p>
	/// Finally, TarEntries can be constructed from nothing but a name.
	/// This allows the programmer to construct the entry by hand, for
	/// instance when only an InputStream is available for writing to
	/// the archive, and the header information is constructed from
	/// other information. In this case the header fields are set to
	/// defaults and the File is set to null.</p>
	/// 
	/// <see cref="TarHeader"/>
	/// </summary>
	[System.ObsoleteAttribute("This assembly has been deprecated. Please use https://www.nuget.org/packages/SharpZipLib/ instead.")]
	public class TarEntry
	{
		/// <summary>
		/// If this entry represents a File, this references it.
		/// </summary>
		string    file;
		
		/// <summary>
		/// This is the entry's header information.
		/// </summary>
		TarHeader	header;
		
		/// <summary>
		/// Only Create Entries with the static CreateXYZ methods or a headerBuffer.
		/// </summary>
		private TarEntry()
		{
		}
		
		/// <summary>
		/// Construct an entry from an archive's header bytes. File is set
		/// to null.
		/// </summary>
		/// <param name = "headerBuf">
		/// The header bytes from a tar archive entry.
		/// </param>
		public TarEntry(byte[] headerBuf)
		{
			this.Initialize();
			this.header.ParseBuffer(headerBuf);
		}
		

      public TarEntry(TarHeader header)
      {
         file = null;
         this.header = header;
      }
		
		/// <summary>
		/// Construct an entry with only a name. This allows the programmer
		/// to construct the entry's header "by hand". File is set to null.
		/// </summary>
		public static TarEntry CreateTarEntry(string name)
		{
			TarEntry entry = new TarEntry();
			entry.Initialize();
			entry.NameTarHeader(entry.header, name);
			return entry;
		}
		
		/// <summary>
		/// Construct an entry for a file. File is set to file, and the
		/// header is constructed from information from the file.
		/// </summary>
		/// <param name = "fileName">
		/// The file that the entry represents.
		/// </param>
		public static TarEntry CreateEntryFromFile(string fileName)
		{
			TarEntry entry = new TarEntry();
			entry.Initialize();
			entry.GetFileTarHeader(entry.header, fileName);
			return entry;
		}
		
		/// <summary>
		/// Initialization code common to all pseudo constructors.
		/// </summary>
		void Initialize()
		{
			this.file   = null;
			this.header = new TarHeader();
		}
		
		/// <summary>
		/// Determine if the two entries are equal. Equality is determined
		/// by the header names being equal.
		/// </summary>
		/// <returns>
		/// True if the entries are equal.
		/// </returns>
		public override bool Equals(object it)
		{
			if (!(it is TarEntry)) 
			{
				return false;
			}
			return this.header.name.ToString().Equals(((TarEntry)it).header.name.ToString());
		}
		
		/// <summary>
		/// Must be overridden when you override Equals.
		/// </summary>
		public override int GetHashCode()
		{
			return this.header.name.ToString().GetHashCode();
		}
		
		
		/// <summary>
		/// Determine if the given entry is a descendant of this entry.
		/// Descendancy is determined by the name of the descendant
		/// starting with this entry's name.
		/// </summary>
		/// <param name = "desc">
		/// Entry to be checked as a descendent of this.
		/// </param>
		/// <returns>
		/// True if entry is a descendant of this.
		/// </returns>
		public bool IsDescendent(TarEntry desc)
		{
			return desc.header.name.ToString().StartsWith(this.header.name.ToString());
		}
		
		/// <summary>
		/// Get this entry's header.
		/// </summary>
		/// <returns>
		/// This entry's TarHeader.
		/// </returns>
		public TarHeader TarHeader 
		{
			get 
			{
				return this.header;
			}
		}
		
		/// <summary>
		/// Get/Set this entry's name.
		/// </summary>
		public string Name 
		{
			get 
			{
				return this.header.name.ToString();
			}
			set 
			{
				this.header.name = new StringBuilder(value);
			}
		}
		
		/// <summary>
		/// Get/set this entry's user id.
		/// </summary>
		public int UserId 
		{
			get 
			{
				return this.header.userId;
			}
			set 
			{
				this.header.userId = value;
			}
		}
		
		/// <summary>
		/// Get/set this entry's group id.
		/// </summary>
		public int GroupId 
		{
			get 
			{
				return this.header.groupId;
			}
			set 
			{
				this.header.groupId = value;
			}
		}
		
		/// <summary>
		/// Get/set this entry's user name.
		/// </summary>
		public string UserName 
		{
			get 
			{
				return this.header.userName.ToString();
			}
			set 
			{
				this.header.userName = new StringBuilder(value);
			}
		}
		
		/// <summary>
		/// Get/set this entry's group name.
		/// </summary>
		public string GroupName 
		{
			get 
			{
				return this.header.groupName.ToString();
			}
			set 
			{
				this.header.groupName = new StringBuilder(value);
			}
		}
		
		/// <summary>
		/// Convenience method to set this entry's group and user ids.
		/// </summary>
		/// <param name="userId">
		/// This entry's new user id.
		/// </param>
		/// <param name="groupId">
		/// This entry's new group id.
		/// </param>
		public void SetIds(int userId, int groupId)
		{
			UserId  = userId; 
			GroupId = groupId;
		}
		
		/// <summary>
		/// Convenience method to set this entry's group and user names.
		/// </summary>
		/// <param name="userName">
		/// This entry's new user name.
		/// </param>
		/// <param name="groupName">
		/// This entry's new group name.
		/// </param>
		public void SetNames(string userName, string groupName)
		{
			UserName  = userName;
			GroupName = groupName;
		}

		//	TODO :
		//		/**
		//		* Set this entry's modification time. The parameter passed
		//		* to this method is in "Java time".
		//		*
		//		* @param time This entry's new modification time.
		//		*/
		//		public void setModTime( long time )
		//		{
		//			this.header.modTime = time / 1000;
		//		}
		
		/// Convert time to DateTimes
		/**
		* Get/Set this entry's modification time.
		*
		* @param time This entry's new modification time.
		*/
		public DateTime ModTime 
		{
			get 
			{
				return this.header.modTime;
			}
			set 
			{
				this.header.modTime = value;
			}
		}
		
		/// <summary>
		/// Get this entry's file.
		/// </summary>
		/// <returns>
		/// This entry's file.
		/// </returns>
		public string File 
		{
			get 
			{
				return this.file;
			}
		}
		
		/// <summary>
		/// Get/set this entry's file size.
		/// </summary>
		public long Size 
		{
			get 
			{
				return this.header.size;
			}
			set 
			{
				this.header.size = value;
			}
		}
		
		/// <summary>
		/// Convenience method that will modify an entry's name directly
		/// in place in an entry header buffer byte array.
		/// </summary>
		/// <param name="outbuf">
		/// The buffer containing the entry header to modify.
		/// </param>
		/// <param name="newName">
		/// The new name to place into the header buffer.
		/// </param>
		public void AdjustEntryName(byte[] outbuf, string newName)
		{
			int offset = 0;
			offset = TarHeader.GetNameBytes(new StringBuilder(newName), outbuf, offset, TarHeader.NAMELEN);
		}
		
		/// <summary>
		/// Return whether or not this entry represents a directory.
		/// </summary>
		/// <returns>
		/// True if this entry is a directory.
		/// </returns>
		public bool IsDirectory
		{
			get 
			{
				if (this.file != null) 
				{
					return Directory.Exists(file);
				}
				
				if (this.header != null) 
				{
					if (this.header.typeFlag == TarHeader.LF_DIR || this.header.name.ToString().EndsWith( "/" )) 
					{
						return true;
					}
				}
				return false;
			}
		}
		
		/// <summary>
		/// Fill in a TarHeader with information from a File.
		/// </summary>
		/// <param name="hdr">
		/// The TarHeader to fill in.
		/// </param>
		/// <param name="file">
		/// The file from which to get the header information.
		/// </param>
		public void GetFileTarHeader(TarHeader hdr, string file)
		{
			this.file = file;

			// bugfix from torhovl from #D forum:
			string name = file;

         // -jr- 23-Jan-2004 HAK HAK HAK, GnuTar allows device names in path where the name is not local to the current directory
         if (Environment.CurrentDirectory == Path.GetDirectoryName(name))
         {
            name = Path.GetFileName(name);
         }
/*			
         if (Path.DirectorySeparatorChar == '\\') 
			{  // check if the OS is Windows
				// Strip off drive letters!
				if (name.Length > 2) 
				{
					char ch1 = name[0];
					char ch2 = name[1];
					
					if (ch2 == ':' && Char.IsLetter(ch1)) 
					{
						name = name.Substring(2);
					}
				}
			}
*/
			
			name = name.Replace(Path.DirectorySeparatorChar, '/').ToLower();

			// No absolute pathnames
			// Windows (and Posix?) paths can start with UNC style "\\NetworkDrive\",
			// so we loop on starting /'s.
			while (name.StartsWith("/")) {
				name = name.Substring(1);
			}

			hdr.linkName = new StringBuilder(String.Empty);
			hdr.name     = new StringBuilder(name);
			
			if (Directory.Exists(file)) {
				hdr.mode     = 1003; // 01753 -jr- no octal constants!! 040755; // Magic number for security access for a UNIX filesystem
				hdr.typeFlag = TarHeader.LF_DIR;
				if (hdr.name.Length == 0 || hdr.name[hdr.name.Length - 1] != '/') {
					hdr.name.Append("/");
				}
				
				hdr.size     = 0;
			} else {
				hdr.mode     = 33216; // 0100700 -jr-  // 0100644; // Magic number for security access for a UNIX filesystem
				hdr.typeFlag = TarHeader.LF_NORMAL;
				hdr.size     = new FileInfo(file.Replace('/', Path.DirectorySeparatorChar)).Length;
			}
			
			// UNDONE When File lets us get the userName, use it!
         hdr.modTime = System.IO.File.GetLastWriteTimeUtc(file.Replace('/', Path.DirectorySeparatorChar)); // -jr- Unix times are in UTC
         hdr.checkSum = 0;
			hdr.devMajor = 0;
			hdr.devMinor = 0;
		}
		
		/// <summary>
		/// If this entry represents a file, and the file is a directory, return
		/// an array of TarEntries for this entry's children.
		/// </summary>
		/// <returns>
		/// An array of TarEntry's for this entry's children.
		/// </returns>
		public TarEntry[] GetDirectoryEntries()
		{
			if (this.file == null || !Directory.Exists(this.file)) 
			{
				return new TarEntry[0];
			}
			
			string[]   list   = Directory.GetFileSystemEntries(this.file);
			TarEntry[] result = new TarEntry[list.Length];

			for (int i = 0; i < list.Length; ++i) 
			{
				result[i] = TarEntry.CreateEntryFromFile(list[i]);
			}
			
			return result;
		}
		
      /// <summary>
		/// Write an entry's header information to a header buffer.
		/// </summary>
		/// <param name = "outbuf">
		/// The tar entry header buffer to fill in.
		/// </param>
		public void WriteEntryHeader(byte[] outbuf)
		{
         this.header.WriteHeader(outbuf);
		}
		
		/// <summary>
		/// Fill in a TarHeader given only the entry's name.
		/// </summary>
		/// <param name="hdr">
		/// The TarHeader to fill in.
		/// </param>
		/// <param name="name">
		/// The tar entry name.
		/// </param>
		public void NameTarHeader(TarHeader hdr, string name)
		{
			bool isDir = name.EndsWith("/");   // -jr- this is true for BSD tar but not all others I think?
			
			hdr.checkSum = 0;
			
			hdr.name = new StringBuilder(name);
//			hdr.mode = isDir ? 040755 : 0100644; // TODO : I think I've seen these magics before ...
         hdr.mode = isDir ? 1003 : 33216;
         hdr.userId   = 0;
			hdr.groupId  = 0;
			hdr.size     = 0;
			hdr.checkSum = 0;
			
         hdr.modTime  = DateTime.UtcNow;        // -jr- 24-Jan-2004 Unix times are in utc!
//			hdr.modTime  = DateTime.Now;   // (new java.util.Date()).getTime() / 1000;
			
			hdr.typeFlag = isDir ? TarHeader.LF_DIR : TarHeader.LF_NORMAL;
			
			hdr.linkName  = new StringBuilder(String.Empty);
			hdr.userName  = new StringBuilder(String.Empty);
			hdr.groupName = new StringBuilder(String.Empty);
			
			hdr.devMajor = 0;
			hdr.devMinor = 0;
		}
	}
}



/* The original Java file had this header:
	*
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
