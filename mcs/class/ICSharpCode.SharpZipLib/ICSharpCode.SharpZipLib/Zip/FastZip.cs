// FastZip.cs
//
// Copyright 2005 John Reilly
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
using ICSharpCode.SharpZipLib.Core;

namespace ICSharpCode.SharpZipLib.Zip
{
	/// <summary>
	/// FastZipEvents supports all events applicable to <see cref="FastZip">FastZip</see> operations.
	/// </summary>
	public class FastZipEvents
	{
		/// <summary>
		/// Delegate to invoke when processing directories.
		/// </summary>
		public ProcessDirectoryHandler ProcessDirectory;

		/// <summary>
		/// Delegate to invoke when processing files.
		/// </summary>
		public ProcessFileHandler ProcessFile;

		/// <summary>
		/// Delegate to invoke during processing of files.
		/// </summary>
		public ProgressHandler Progress;

		/// <summary>
		/// Delegate to invoke when processing for a file has been completed.
		/// </summary>
		public CompletedFileHandler CompletedFile;

		/// <summary>
		/// Delegate to invoke when processing directory failures.
		/// </summary>
		public DirectoryFailureHandler DirectoryFailure;

		/// <summary>
		/// Delegate to invoke when processing file failures.
		/// </summary>
		public FileFailureHandler FileFailure;

		/// <summary>
		/// Raise the <see cref="DirectoryFailure">directory failure</see> event.
		/// </summary>
		/// <param name="directory">The directory causing the failure.</param>
		/// <param name="e">The exception for this event.</param>
		/// <returns>A boolean indicating if execution should continue or not.</returns>
		public bool OnDirectoryFailure(string directory, Exception e)
		{
			bool result = false;
			DirectoryFailureHandler handler = DirectoryFailure;

			if ( handler != null ) {
				ScanFailureEventArgs args = new ScanFailureEventArgs(directory, e);
				handler(this, args);
				result = args.ContinueRunning;
			}
			return result;
		}

		/// <summary>
		/// Fires the <see cref="FileFailure"> file failure handler delegate</see>.
		/// </summary>
		/// <param name="file">The file causing the failure.</param>
		/// <param name="e">The exception for this failure.</param>
		/// <returns>A boolean indicating if execution should continue or not.</returns>
		public bool OnFileFailure(string file, Exception e)
		{
			FileFailureHandler handler = FileFailure;
            bool result = (handler != null);

			if ( result ) {
				ScanFailureEventArgs args = new ScanFailureEventArgs(file, e);
				handler(this, args);
				result = args.ContinueRunning;
			}
			return result;
		}

		/// <summary>
		/// Fires the <see cref="ProcessFile">ProcessFile delegate</see>.
		/// </summary>
		/// <param name="file">The file being processed.</param>
		/// <returns>A boolean indicating if execution should continue or not.</returns>
		public bool OnProcessFile(string file)
		{
			bool result = true;
			ProcessFileHandler handler = ProcessFile;

			if ( handler != null ) {
				ScanEventArgs args = new ScanEventArgs(file);
				handler(this, args);
				result = args.ContinueRunning;
			}
			return result;
		}

		/// <summary>
        /// Fires the <see cref="CompletedFile"/> delegate
		/// </summary>
		/// <param name="file">The file whose processing has been completed.</param>
		/// <returns>A boolean indicating if execution should continue or not.</returns>
		public bool OnCompletedFile(string file)
		{
			bool result = true;
			CompletedFileHandler handler = CompletedFile;
			if ( handler != null ) {
				ScanEventArgs args = new ScanEventArgs(file);
				handler(this, args);
				result = args.ContinueRunning;
			}
			return result;
		}

		/// <summary>
		/// Fires the <see cref="ProcessDirectory">process directory</see> delegate.
		/// </summary>
		/// <param name="directory">The directory being processed.</param>
		/// <param name="hasMatchingFiles">Flag indicating if the directory has matching files as determined by the current filter.</param>
		/// <returns>A <see cref="bool"/> of true if the operation should continue; false otherwise.</returns>
		public bool OnProcessDirectory(string directory, bool hasMatchingFiles)
		{
			bool result = true;
			ProcessDirectoryHandler handler = ProcessDirectory;
			if ( handler != null ) {
				DirectoryEventArgs args = new DirectoryEventArgs(directory, hasMatchingFiles);
				handler(this, args);
				result = args.ContinueRunning;
			}
			return result;
		}

		/// <summary>
		/// The minimum timespan between <see cref="Progress"/> events.
		/// </summary>
		/// <value>The minimum period of time between <see cref="Progress"/> events.</value>
		/// <seealso cref="Progress"/>
        /// <remarks>The default interval is three seconds.</remarks>
		public TimeSpan ProgressInterval
		{
			get { return progressInterval_; }
			set { progressInterval_ = value; }
		}

		#region Instance Fields
		TimeSpan progressInterval_ = TimeSpan.FromSeconds(3);
		#endregion
	}

	/// <summary>
	/// FastZip provides facilities for creating and extracting zip files.
	/// </summary>
	public class FastZip
	{
		#region Enumerations
		/// <summary>
		/// Defines the desired handling when overwriting files during extraction.
		/// </summary>
		public enum Overwrite
		{
			/// <summary>
			/// Prompt the user to confirm overwriting
			/// </summary>
			Prompt,
			/// <summary>
			/// Never overwrite files.
			/// </summary>
			Never,
			/// <summary>
			/// Always overwrite files.
			/// </summary>
			Always
		}
		#endregion

		#region Constructors
		/// <summary>
		/// Initialise a default instance of <see cref="FastZip"/>.
		/// </summary>
		public FastZip()
		{
		}

		/// <summary>
		/// Initialise a new instance of <see cref="FastZip"/>
		/// </summary>
		/// <param name="events">The <see cref="FastZipEvents">events</see> to use during operations.</param>
		public FastZip(FastZipEvents events)
		{
			events_ = events;
		}
		#endregion

		#region Properties
		/// <summary>
		/// Get/set a value indicating wether empty directories should be created.
		/// </summary>
		public bool CreateEmptyDirectories
		{
			get { return createEmptyDirectories_; }
			set { createEmptyDirectories_ = value; }
		}

#if !NETCF_1_0
		/// <summary>
		/// Get / set the password value.
		/// </summary>
		public string Password
		{
			get { return password_; }
			set { password_ = value; }
		}
#endif

		/// <summary>
		/// Get or set the <see cref="INameTransform"></see> active when creating Zip files.
		/// </summary>
		/// <seealso cref="EntryFactory"></seealso>
		public INameTransform NameTransform
		{
			get { return entryFactory_.NameTransform; }
			set {
				entryFactory_.NameTransform = value;
			}
		}

		/// <summary>
		/// Get or set the <see cref="IEntryFactory"></see> active when creating Zip files.
		/// </summary>
		public IEntryFactory EntryFactory
		{
			get { return entryFactory_; }
			set {
				if ( value == null ) {
					entryFactory_ = new ZipEntryFactory();
				}
				else {
					entryFactory_ = value;
				}
			}
		}

		/// <summary>
		/// Gets or sets the setting for <see cref="UseZip64">Zip64 handling when writing.</see>
		/// </summary>
        /// <remarks>
        /// The default value is dynamic which is not backwards compatible with old
        /// programs and can cause problems with XP's built in compression which cant
        /// read Zip64 archives. However it does avoid the situation were a large file
        /// is added and cannot be completed correctly.
        /// NOTE: Setting the size for entries before they are added is the best solution!
        /// By default the EntryFactory used by FastZip will set fhe file size.
        /// </remarks>
		public UseZip64 UseZip64
		{
			get { return useZip64_; }
			set { useZip64_ = value; }
		}

		/// <summary>
		/// Get/set a value indicating wether file dates and times should
		/// be restored when extracting files from an archive.
		/// </summary>
		/// <remarks>The default value is false.</remarks>
		public bool RestoreDateTimeOnExtract
		{
			get {
				return restoreDateTimeOnExtract_;
			}
			set {
				restoreDateTimeOnExtract_ = value;
			}
		}

		/// <summary>
		/// Get/set a value indicating wether file attributes should
		/// be restored during extract operations
		/// </summary>
		public bool RestoreAttributesOnExtract
		{
			get { return restoreAttributesOnExtract_; }
			set { restoreAttributesOnExtract_ = value; }
		}
		#endregion

		#region Delegates
		/// <summary>
		/// Delegate called when confirming overwriting of files.
		/// </summary>
		public delegate bool ConfirmOverwriteDelegate(string fileName);
		#endregion

		#region CreateZip
		/// <summary>
		/// Create a zip file.
		/// </summary>
		/// <param name="zipFileName">The name of the zip file to create.</param>
		/// <param name="sourceDirectory">The directory to source files from.</param>
		/// <param name="recurse">True to recurse directories, false for no recursion.</param>
		/// <param name="fileFilter">The <see cref="PathFilter">file filter</see> to apply.</param>
		/// <param name="directoryFilter">The <see cref="PathFilter">directory filter</see> to apply.</param>
		public void CreateZip(string zipFileName, string sourceDirectory,
			bool recurse, string fileFilter, string directoryFilter)
		{
			CreateZip(File.Create(zipFileName), sourceDirectory, recurse, fileFilter, directoryFilter);
		}

		/// <summary>
		/// Create a zip file/archive.
		/// </summary>
		/// <param name="zipFileName">The name of the zip file to create.</param>
		/// <param name="sourceDirectory">The directory to obtain files and directories from.</param>
		/// <param name="recurse">True to recurse directories, false for no recursion.</param>
		/// <param name="fileFilter">The file filter to apply.</param>
		public void CreateZip(string zipFileName, string sourceDirectory, bool recurse, string fileFilter)
		{
			CreateZip(File.Create(zipFileName), sourceDirectory, recurse, fileFilter, null);
		}

		/// <summary>
		/// Create a zip archive sending output to the <paramref name="outputStream"/> passed.
		/// </summary>
		/// <param name="outputStream">The stream to write archive data to.</param>
		/// <param name="sourceDirectory">The directory to source files from.</param>
		/// <param name="recurse">True to recurse directories, false for no recursion.</param>
		/// <param name="fileFilter">The <see cref="PathFilter">file filter</see> to apply.</param>
		/// <param name="directoryFilter">The <see cref="PathFilter">directory filter</see> to apply.</param>
        /// <remarks>The <paramref name="outputStream"/> is closed after creation.</remarks>
		public void CreateZip(Stream outputStream, string sourceDirectory, bool recurse, string fileFilter, string directoryFilter)
		{
			NameTransform = new ZipNameTransform(sourceDirectory);
			sourceDirectory_ = sourceDirectory;

			using ( outputStream_ = new ZipOutputStream(outputStream) ) {

#if !NETCF_1_0
				if ( password_ != null ) {
					outputStream_.Password = password_;
				}
#endif

				outputStream_.UseZip64 = UseZip64;
				FileSystemScanner scanner = new FileSystemScanner(fileFilter, directoryFilter);
				scanner.ProcessFile += new ProcessFileHandler(ProcessFile);
				if ( this.CreateEmptyDirectories ) {
					scanner.ProcessDirectory += new ProcessDirectoryHandler(ProcessDirectory);
				}

				if (events_ != null) {
					if ( events_.FileFailure != null ) {
						scanner.FileFailure += events_.FileFailure;
					}

					if ( events_.DirectoryFailure != null ) {
						scanner.DirectoryFailure += events_.DirectoryFailure;
					}
				}

				scanner.Scan(sourceDirectory, recurse);
			}
		}

		#endregion

		#region ExtractZip
		/// <summary>
		/// Extract the contents of a zip file.
		/// </summary>
		/// <param name="zipFileName">The zip file to extract from.</param>
		/// <param name="targetDirectory">The directory to save extracted information in.</param>
		/// <param name="fileFilter">A filter to apply to files.</param>
		public void ExtractZip(string zipFileName, string targetDirectory, string fileFilter)
		{
			ExtractZip(zipFileName, targetDirectory, Overwrite.Always, null, fileFilter, null, restoreDateTimeOnExtract_);
		}

		/// <summary>
		/// Extract the contents of a zip file.
		/// </summary>
		/// <param name="zipFileName">The zip file to extract from.</param>
		/// <param name="targetDirectory">The directory to save extracted information in.</param>
		/// <param name="overwrite">The style of <see cref="Overwrite">overwriting</see> to apply.</param>
		/// <param name="confirmDelegate">A delegate to invoke when confirming overwriting.</param>
		/// <param name="fileFilter">A filter to apply to files.</param>
		/// <param name="directoryFilter">A filter to apply to directories.</param>
		/// <param name="restoreDateTime">Flag indicating whether to restore the date and time for extracted files.</param>
		public void ExtractZip(string zipFileName, string targetDirectory,
							   Overwrite overwrite, ConfirmOverwriteDelegate confirmDelegate,
							   string fileFilter, string directoryFilter, bool restoreDateTime)
		{
			Stream inputStream = File.Open(zipFileName, FileMode.Open, FileAccess.Read, FileShare.Read);
			ExtractZip(inputStream, targetDirectory, overwrite, confirmDelegate, fileFilter, directoryFilter, restoreDateTime, true);
		}

		/// <summary>
		/// Extract the contents of a zip file held in a stream.
		/// </summary>
		/// <param name="inputStream">The seekable input stream containing the zip to extract from.</param>
		/// <param name="targetDirectory">The directory to save extracted information in.</param>
		/// <param name="overwrite">The style of <see cref="Overwrite">overwriting</see> to apply.</param>
		/// <param name="confirmDelegate">A delegate to invoke when confirming overwriting.</param>
		/// <param name="fileFilter">A filter to apply to files.</param>
		/// <param name="directoryFilter">A filter to apply to directories.</param>
		/// <param name="restoreDateTime">Flag indicating whether to restore the date and time for extracted files.</param>
		/// <param name="isStreamOwner">Flag indicating whether the inputStream will be closed by this method.</param>
		public void ExtractZip(Stream inputStream, string targetDirectory,
					   Overwrite overwrite, ConfirmOverwriteDelegate confirmDelegate,
					   string fileFilter, string directoryFilter, bool restoreDateTime,
					   bool isStreamOwner)
		{
			if ((overwrite == Overwrite.Prompt) && (confirmDelegate == null)) {
				throw new ArgumentNullException("confirmDelegate");
			}

			continueRunning_ = true;
			overwrite_ = overwrite;
			confirmDelegate_ = confirmDelegate;
			extractNameTransform_ = new WindowsNameTransform(targetDirectory);

			fileFilter_ = new NameFilter(fileFilter);
			directoryFilter_ = new NameFilter(directoryFilter);
			restoreDateTimeOnExtract_ = restoreDateTime;

			using (zipFile_ = new ZipFile(inputStream)) {

#if !NETCF_1_0
				if (password_ != null) {
					zipFile_.Password = password_;
				}
#endif
				zipFile_.IsStreamOwner = isStreamOwner;
				System.Collections.IEnumerator enumerator = zipFile_.GetEnumerator();
				while (continueRunning_ && enumerator.MoveNext()) {
					ZipEntry entry = (ZipEntry)enumerator.Current;
					if (entry.IsFile)
					{
						// TODO Path.GetDirectory can fail here on invalid characters.
						if (directoryFilter_.IsMatch(Path.GetDirectoryName(entry.Name)) && fileFilter_.IsMatch(entry.Name)) {
							ExtractEntry(entry);
						}
					}
					else if (entry.IsDirectory) {
						if (directoryFilter_.IsMatch(entry.Name) && CreateEmptyDirectories) {
							ExtractEntry(entry);
						}
					}
					else {
						// Do nothing for volume labels etc...
					}
				}
			}
		}
		#endregion

		#region Internal Processing
		void ProcessDirectory(object sender, DirectoryEventArgs e)
		{
			if ( !e.HasMatchingFiles && CreateEmptyDirectories ) {
				if ( events_ != null ) {
					events_.OnProcessDirectory(e.Name, e.HasMatchingFiles);
				}

				if ( e.ContinueRunning ) {
					if (e.Name != sourceDirectory_) {
						ZipEntry entry = entryFactory_.MakeDirectoryEntry(e.Name);
						outputStream_.PutNextEntry(entry);
					}
				}
			}
		}

		void ProcessFile(object sender, ScanEventArgs e)
		{
			if ( (events_ != null) && (events_.ProcessFile != null) ) {
				events_.ProcessFile(sender, e);
			}

			if ( e.ContinueRunning ) {
                try {
                    // The open below is equivalent to OpenRead which gaurantees that if opened the
                    // file will not be changed by subsequent openers, but precludes opening in some cases
                    // were it could succeed. ie the open may fail as its already open for writing and the share mode should reflect that.
                    using (FileStream stream = File.Open(e.Name, FileMode.Open, FileAccess.Read, FileShare.Read)) {
                        ZipEntry entry = entryFactory_.MakeFileEntry(e.Name);
                        outputStream_.PutNextEntry(entry);
                        AddFileContents(e.Name, stream);
                    }
                }
                catch(Exception ex) {
                    if (events_ != null) {
                        continueRunning_ = events_.OnFileFailure(e.Name, ex);
                    }
                    else {
                        continueRunning_ = false;
                        throw;
                    }
                }
			}
		}

		void AddFileContents(string name, Stream stream)
		{
			if( stream==null ) {
				throw new ArgumentNullException("stream");
			}

			if( buffer_==null ) {
				buffer_=new byte[4096];
			}

			if( (events_!=null)&&(events_.Progress!=null) ) {
				StreamUtils.Copy(stream, outputStream_, buffer_,
					events_.Progress, events_.ProgressInterval, this, name);
			}
			else {
				StreamUtils.Copy(stream, outputStream_, buffer_);
			}

			if( events_!=null ) {
				continueRunning_=events_.OnCompletedFile(name);
			}
		}

		void ExtractFileEntry(ZipEntry entry, string targetName)
		{
			bool proceed = true;
			if ( overwrite_ != Overwrite.Always ) {
				if ( File.Exists(targetName) ) {
					if ( (overwrite_ == Overwrite.Prompt) && (confirmDelegate_ != null) ) {
						proceed = confirmDelegate_(targetName);
					}
					else {
						proceed = false;
					}
				}
			}

			if ( proceed ) {
				if ( events_ != null ) {
					continueRunning_ = events_.OnProcessFile(entry.Name);
				}

				if ( continueRunning_ ) {
					try {
						using ( FileStream outputStream = File.Create(targetName) ) {
							if ( buffer_ == null ) {
								buffer_ = new byte[4096];
							}
							if ((events_ != null) && (events_.Progress != null))
							{
								StreamUtils.Copy(zipFile_.GetInputStream(entry), outputStream, buffer_,
									events_.Progress, events_.ProgressInterval, this, entry.Name, entry.Size);
							}
							else
							{
								StreamUtils.Copy(zipFile_.GetInputStream(entry), outputStream, buffer_);
							}

							if (events_ != null) {
								continueRunning_ = events_.OnCompletedFile(entry.Name);
							}
						}

#if !NETCF_1_0 && !NETCF_2_0
						if ( restoreDateTimeOnExtract_ ) {
							File.SetLastWriteTime(targetName, entry.DateTime);
						}

						if ( RestoreAttributesOnExtract && entry.IsDOSEntry && (entry.ExternalFileAttributes != -1)) {
							FileAttributes fileAttributes = (FileAttributes) entry.ExternalFileAttributes;
							// TODO: FastZip - Setting of other file attributes on extraction is a little trickier.
							fileAttributes &= (FileAttributes.Archive | FileAttributes.Normal | FileAttributes.ReadOnly | FileAttributes.Hidden);
							File.SetAttributes(targetName, fileAttributes);
						}
#endif
					}
					catch(Exception ex) {
						if ( events_ != null ) {
							continueRunning_ = events_.OnFileFailure(targetName, ex);
						}
						else {
                            continueRunning_ = false;
                            throw;
						}
					}
				}
			}
		}

		void ExtractEntry(ZipEntry entry)
		{
			bool doExtraction = entry.IsCompressionMethodSupported();
			string targetName = entry.Name;

			if ( doExtraction ) {
				if ( entry.IsFile ) {
					targetName = extractNameTransform_.TransformFile(targetName);
				}
				else if ( entry.IsDirectory ) {
					targetName = extractNameTransform_.TransformDirectory(targetName);
				}

				doExtraction = !((targetName == null) || (targetName.Length == 0));
			}

			// TODO: Fire delegate/throw exception were compression method not supported, or name is invalid?

			string dirName = null;

			if ( doExtraction ) {
					if ( entry.IsDirectory ) {
						dirName = targetName;
					}
					else {
						dirName = Path.GetDirectoryName(Path.GetFullPath(targetName));
					}
			}

			if ( doExtraction && !Directory.Exists(dirName) ) {
				if ( !entry.IsDirectory || CreateEmptyDirectories ) {
					try {
						Directory.CreateDirectory(dirName);
					}
					catch (Exception ex) {
						doExtraction = false;
						if ( events_ != null ) {
							if ( entry.IsDirectory ) {
								continueRunning_ = events_.OnDirectoryFailure(targetName, ex);
							}
							else {
								continueRunning_ = events_.OnFileFailure(targetName, ex);
							}
						}
						else {
							continueRunning_ = false;
                            throw;
						}
					}
				}
			}

			if ( doExtraction && entry.IsFile ) {
				ExtractFileEntry(entry, targetName);
			}
		}

		static int MakeExternalAttributes(FileInfo info)
		{
			return (int)info.Attributes;
		}

#if NET_1_0 || NET_1_1 || NETCF_1_0
		static bool NameIsValid(string name)
		{
			return (name != null) &&
				(name.Length > 0) &&
				(name.IndexOfAny(Path.InvalidPathChars) < 0);
		}
#else
		static bool NameIsValid(string name)
		{
			return (name != null) &&
				(name.Length > 0) &&
				(name.IndexOfAny(Path.GetInvalidPathChars()) < 0);
		}
#endif
		#endregion

		#region Instance Fields
		bool continueRunning_;
		byte[] buffer_;
		ZipOutputStream outputStream_;
		ZipFile zipFile_;
		string sourceDirectory_;
		NameFilter fileFilter_;
		NameFilter directoryFilter_;
		Overwrite overwrite_;
		ConfirmOverwriteDelegate confirmDelegate_;

		bool restoreDateTimeOnExtract_;
		bool restoreAttributesOnExtract_;
		bool createEmptyDirectories_;
		FastZipEvents events_;
		IEntryFactory entryFactory_ = new ZipEntryFactory();
		INameTransform extractNameTransform_;
		UseZip64 useZip64_=UseZip64.Dynamic;

#if !NETCF_1_0
		string password_;
#endif

		#endregion
	}
}
