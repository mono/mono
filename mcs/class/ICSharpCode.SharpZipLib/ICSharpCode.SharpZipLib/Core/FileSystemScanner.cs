// FileSystemScanner.cs
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

namespace ICSharpCode.SharpZipLib.Core
{
	#region EventArgs
	/// <summary>
	/// Event arguments for scanning.
	/// </summary>
	public class ScanEventArgs : EventArgs
	{
		#region Constructors
		/// <summary>
		/// Initialise a new instance of <see cref="ScanEventArgs"/>
		/// </summary>
		/// <param name="name">The file or directory name.</param>
		public ScanEventArgs(string name)
		{
			name_ = name;
		}
		#endregion
		
		/// <summary>
		/// The file or directory name for this event.
		/// </summary>
		public string Name
		{
			get { return name_; }
		}
		
		/// <summary>
		/// Get set a value indicating if scanning should continue or not.
		/// </summary>
		public bool ContinueRunning
		{
			get { return continueRunning_; }
			set { continueRunning_ = value; }
		}

		#region Instance Fields
		string name_;
		bool continueRunning_ = true;
		#endregion
	}

	/// <summary>
	/// Event arguments during processing of a single file or directory.
	/// </summary>
	public class ProgressEventArgs : EventArgs
	{
		#region Constructors
		/// <summary>
		/// Initialise a new instance of <see cref="ScanEventArgs"/>
		/// </summary>
		/// <param name="name">The file or directory name if known.</param>
		/// <param name="processed">The number of bytes processed so far</param>
		/// <param name="target">The total number of bytes to process, 0 if not known</param>
		public ProgressEventArgs(string name, long processed, long target)
		{
			name_ = name;
			processed_ = processed;
			target_ = target;
		}
		#endregion

		/// <summary>
		/// The name for this event if known.
		/// </summary>
		public string Name
		{
			get { return name_; }
		}

		/// <summary>
		/// Get set a value indicating wether scanning should continue or not.
		/// </summary>
		public bool ContinueRunning
		{
			get { return continueRunning_; }
			set { continueRunning_ = value; }
		}

		/// <summary>
		/// Get a percentage representing how much of the <see cref="Target"></see> has been processed
		/// </summary>
		/// <value>0.0 to 100.0 percent; 0 if target is not known.</value>
		public float PercentComplete
		{
			get
			{
			    float result;
				if (target_ <= 0)
				{
					result = 0;
				}
				else
				{
					result = ((float)processed_ / (float)target_) * 100.0f;
				}
			    return result;
			}
		}

		/// <summary>
		/// The number of bytes processed so far
		/// </summary>
		public long Processed
		{
			get { return processed_; }
		}

		/// <summary>
		/// The number of bytes to process.
		/// </summary>
		/// <remarks>Target may be 0 or negative if the value isnt known.</remarks>
		public long Target
		{
			get { return target_; }
		}

		#region Instance Fields
		string name_;
		long processed_;
		long target_;
		bool continueRunning_ = true;
		#endregion
	}

	/// <summary>
	/// Event arguments for directories.
	/// </summary>
	public class DirectoryEventArgs : ScanEventArgs
	{
		#region Constructors
		/// <summary>
		/// Initialize an instance of <see cref="DirectoryEventArgs"></see>.
		/// </summary>
		/// <param name="name">The name for this directory.</param>
		/// <param name="hasMatchingFiles">Flag value indicating if any matching files are contained in this directory.</param>
		public DirectoryEventArgs(string name, bool hasMatchingFiles)
			: base (name)
		{
			hasMatchingFiles_ = hasMatchingFiles;
		}
		#endregion
		
		/// <summary>
		/// Get a value indicating if the directory contains any matching files or not.
		/// </summary>
		public bool HasMatchingFiles
		{
			get { return hasMatchingFiles_; }
		}
		
		#region Instance Fields
		bool hasMatchingFiles_;
		#endregion
	}
	
	/// <summary>
	/// Arguments passed when scan failures are detected.
	/// </summary>
	public class ScanFailureEventArgs : EventArgs
	{
		#region Constructors
		/// <summary>
		/// Initialise a new instance of <see cref="ScanFailureEventArgs"></see>
		/// </summary>
		/// <param name="name">The name to apply.</param>
		/// <param name="e">The exception to use.</param>
		public ScanFailureEventArgs(string name, Exception e)
		{
			name_ = name;
			exception_ = e;
			continueRunning_ = true;
		}
		#endregion
		
		/// <summary>
		/// The applicable name.
		/// </summary>
		public string Name
		{
			get { return name_; }
		}
		
		/// <summary>
		/// The applicable exception.
		/// </summary>
		public Exception Exception
		{
			get { return exception_; }
		}
		
		/// <summary>
		/// Get / set a value indicating wether scanning should continue.
		/// </summary>
		public bool ContinueRunning
		{
			get { return continueRunning_; }
			set { continueRunning_ = value; }
		}

		#region Instance Fields
		string name_;
		Exception exception_;
		bool continueRunning_;
		#endregion
	}
	
	#endregion

	#region Delegates
	/// <summary>
	/// Delegate invoked before starting to process a directory.
	/// </summary>
	public delegate void ProcessDirectoryHandler(object sender, DirectoryEventArgs e);
	
	/// <summary>
	/// Delegate invoked before starting to process a file.
	/// </summary>
	/// <param name="sender">The source of the event</param>
	/// <param name="e">The event arguments.</param>
	public delegate void ProcessFileHandler(object sender, ScanEventArgs e);

	/// <summary>
	/// Delegate invoked during processing of a file or directory
	/// </summary>
	/// <param name="sender">The source of the event</param>
	/// <param name="e">The event arguments.</param>
	public delegate void ProgressHandler(object sender, ProgressEventArgs e);

	/// <summary>
	/// Delegate invoked when a file has been completely processed.
	/// </summary>
	/// <param name="sender">The source of the event</param>
	/// <param name="e">The event arguments.</param>
	public delegate void CompletedFileHandler(object sender, ScanEventArgs e);
	
	/// <summary>
	/// Delegate invoked when a directory failure is detected.
	/// </summary>
	/// <param name="sender">The source of the event</param>
	/// <param name="e">The event arguments.</param>
	public delegate void DirectoryFailureHandler(object sender, ScanFailureEventArgs e);
	
	/// <summary>
	/// Delegate invoked when a file failure is detected.
	/// </summary>
	/// <param name="sender">The source of the event</param>
	/// <param name="e">The event arguments.</param>
	public delegate void FileFailureHandler(object sender, ScanFailureEventArgs e);
	#endregion

	/// <summary>
	/// FileSystemScanner provides facilities scanning of files and directories.
	/// </summary>
	public class FileSystemScanner
	{
		#region Constructors
		/// <summary>
		/// Initialise a new instance of <see cref="FileSystemScanner"></see>
		/// </summary>
		/// <param name="filter">The <see cref="PathFilter">file filter</see> to apply when scanning.</param>
		public FileSystemScanner(string filter)
		{
			fileFilter_ = new PathFilter(filter);
		}
		
		/// <summary>
		/// Initialise a new instance of <see cref="FileSystemScanner"></see>
		/// </summary>
		/// <param name="fileFilter">The <see cref="PathFilter">file filter</see> to apply.</param>
		/// <param name="directoryFilter">The <see cref="PathFilter"> directory filter</see> to apply.</param>
		public FileSystemScanner(string fileFilter, string directoryFilter)
		{
			fileFilter_ = new PathFilter(fileFilter);
			directoryFilter_ = new PathFilter(directoryFilter);
		}
		
		/// <summary>
		/// Initialise a new instance of <see cref="FileSystemScanner"></see>
		/// </summary>
		/// <param name="fileFilter">The file <see cref="IScanFilter">filter</see> to apply.</param>
		public FileSystemScanner(IScanFilter fileFilter)
		{
			fileFilter_ = fileFilter;
		}
		
		/// <summary>
		/// Initialise a new instance of <see cref="FileSystemScanner"></see>
		/// </summary>
		/// <param name="fileFilter">The file <see cref="IScanFilter">filter</see>  to apply.</param>
		/// <param name="directoryFilter">The directory <see cref="IScanFilter">filter</see>  to apply.</param>
		public FileSystemScanner(IScanFilter fileFilter, IScanFilter directoryFilter)
		{
			fileFilter_ = fileFilter;
			directoryFilter_ = directoryFilter;
		}
		#endregion

		#region Delegates
		/// <summary>
		/// Delegate to invoke when a directory is processed.
		/// </summary>
		public ProcessDirectoryHandler ProcessDirectory;
		
		/// <summary>
		/// Delegate to invoke when a file is processed.
		/// </summary>
		public ProcessFileHandler ProcessFile;

		/// <summary>
		/// Delegate to invoke when processing for a file has finished.
		/// </summary>
		public CompletedFileHandler CompletedFile;

		/// <summary>
		/// Delegate to invoke when a directory failure is detected.
		/// </summary>
		public DirectoryFailureHandler DirectoryFailure;
		
		/// <summary>
		/// Delegate to invoke when a file failure is detected.
		/// </summary>
		public FileFailureHandler FileFailure;
		#endregion

		/// <summary>
		/// Raise the DirectoryFailure event.
		/// </summary>
		/// <param name="directory">The directory name.</param>
		/// <param name="e">The exception detected.</param>
		bool OnDirectoryFailure(string directory, Exception e)
		{
            DirectoryFailureHandler handler = DirectoryFailure;
            bool result = (handler != null);
            if ( result ) {
				ScanFailureEventArgs args = new ScanFailureEventArgs(directory, e);
				handler(this, args);
				alive_ = args.ContinueRunning;
			}
            return result;
		}
		
		/// <summary>
		/// Raise the FileFailure event.
		/// </summary>
		/// <param name="file">The file name.</param>
		/// <param name="e">The exception detected.</param>
		bool OnFileFailure(string file, Exception e)
		{
            FileFailureHandler handler = FileFailure;

            bool result = (handler != null);

			if ( result ){
				ScanFailureEventArgs args = new ScanFailureEventArgs(file, e);
				FileFailure(this, args);
				alive_ = args.ContinueRunning;
			}
            return result;
		}

		/// <summary>
		/// Raise the ProcessFile event.
		/// </summary>
		/// <param name="file">The file name.</param>
		void OnProcessFile(string file)
		{
			ProcessFileHandler handler = ProcessFile;

			if ( handler!= null ) {
				ScanEventArgs args = new ScanEventArgs(file);
				handler(this, args);
				alive_ = args.ContinueRunning;
			}
		}

		/// <summary>
		/// Raise the complete file event
		/// </summary>
		/// <param name="file">The file name</param>
		void OnCompleteFile(string file)
		{
			CompletedFileHandler handler = CompletedFile;

			if (handler != null)
			{
				ScanEventArgs args = new ScanEventArgs(file);
				handler(this, args);
				alive_ = args.ContinueRunning;
			}
		}

		/// <summary>
		/// Raise the ProcessDirectory event.
		/// </summary>
		/// <param name="directory">The directory name.</param>
		/// <param name="hasMatchingFiles">Flag indicating if the directory has matching files.</param>
		void OnProcessDirectory(string directory, bool hasMatchingFiles)
		{
			ProcessDirectoryHandler handler = ProcessDirectory;

			if ( handler != null ) {
				DirectoryEventArgs args = new DirectoryEventArgs(directory, hasMatchingFiles);
				handler(this, args);
				alive_ = args.ContinueRunning;
			}
		}

		/// <summary>
		/// Scan a directory.
		/// </summary>
		/// <param name="directory">The base directory to scan.</param>
		/// <param name="recurse">True to recurse subdirectories, false to scan a single directory.</param>
		public void Scan(string directory, bool recurse)
		{
			alive_ = true;
			ScanDir(directory, recurse);
		}
		
		void ScanDir(string directory, bool recurse)
		{

			try {
				string[] names = System.IO.Directory.GetFiles(directory);
				bool hasMatch = false;
				for (int fileIndex = 0; fileIndex < names.Length; ++fileIndex) {
					if ( !fileFilter_.IsMatch(names[fileIndex]) ) {
						names[fileIndex] = null;
					} else {
						hasMatch = true;
					}
				}
				
				OnProcessDirectory(directory, hasMatch);
				
				if ( alive_ && hasMatch ) {
					foreach (string fileName in names) {
						try {
							if ( fileName != null ) {
								OnProcessFile(fileName);
								if ( !alive_ ) {
									break;
								}
							}
						}
						catch (Exception e) {
                            if (!OnFileFailure(fileName, e)) {
                                throw;
                            }
						}
					}
				}
			}
			catch (Exception e) {
                if (!OnDirectoryFailure(directory, e)) {
                    throw;
                }
			}

			if ( alive_ && recurse ) {
				try {
					string[] names = System.IO.Directory.GetDirectories(directory);
					foreach (string fulldir in names) {
						if ((directoryFilter_ == null) || (directoryFilter_.IsMatch(fulldir))) {
							ScanDir(fulldir, true);
							if ( !alive_ ) {
								break;
							}
						}
					}
				}
				catch (Exception e) {
                    if (!OnDirectoryFailure(directory, e)) {
                        throw;
                    }
				}
			}
		}
		
		#region Instance Fields
		/// <summary>
		/// The file filter currently in use.
		/// </summary>
		IScanFilter fileFilter_;
		/// <summary>
		/// The directory filter currently in use.
		/// </summary>
		IScanFilter directoryFilter_;
		/// <summary>
		/// Flag indicating if scanning should continue running.
		/// </summary>
		bool alive_;
		#endregion
	}
}
