//------------------------------------------------------------------------------
// <copyright file="WriteFileContext.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Configuration.Internal {
    using System.Configuration;
    using System.IO;
    using System.Security.Permissions;
    using System.Reflection;
    using System.Threading;
    using System.Security;
    using System.CodeDom.Compiler;
    using Microsoft.Win32;	
#if !FEATURE_PAL
    using System.Security.AccessControl;
#endif

    internal class WriteFileContext {
        private const  int          SAVING_TIMEOUT        = 10000;  // 10 seconds
        private const  int          SAVING_RETRY_INTERVAL =   100;  // 100 milliseconds
        private static volatile bool _osPlatformDetermined;
        private static volatile PlatformID _osPlatform;
        
        private TempFileCollection  _tempFiles;
        private string              _tempNewFilename;
        private string              _templateFilename;

        internal WriteFileContext(string filename, string templateFilename) {
            string directoryname = UrlPath.GetDirectoryOrRootName(filename);

            _templateFilename = templateFilename;
            _tempFiles = new TempFileCollection(directoryname);
            try {
                _tempNewFilename = _tempFiles.AddExtension("newcfg");
            }
            catch {
                ((IDisposable)_tempFiles).Dispose();
                _tempFiles = null;
                throw;
            }
        }

        static WriteFileContext() {
            _osPlatformDetermined = false;
        }

        internal string TempNewFilename {
            get {return _tempNewFilename;}
        }

        // Complete
        //
        // Cleanup the WriteFileContext object based on either success
        // or failure
        //
        // Note: The current algorithm guarantess
        //         1) The file we are saving to will always be present 
        //            on the file system (ie. there will be no window
        //            during saving in which there won't be a file there)
        //         2) It will always be available for reading from a 
        //            client and it will be complete and accurate.
        //
        // ... This means that writing is a bit more complicated, and may
        // have to be retried (because of reading lock), but I don't see 
        // anyway to get around this given 1 and 2.
        //
        internal void Complete(string filename, bool success) {
            try {
                if (success) {
                    if ( File.Exists( filename ) ) {
                        // Test that we can write to the file
                        ValidateWriteAccess( filename );

                        // Copy Attributes from original
                        DuplicateFileAttributes( filename, _tempNewFilename );
                    } 
                    else {
                        if ( _templateFilename != null ) {
                            // Copy Acl from template file
                            DuplicateTemplateAttributes( _templateFilename, _tempNewFilename );
                        }
                    }

                    ReplaceFile(_tempNewFilename, filename);

                    // Don't delete, since we just moved it.
                    _tempFiles.KeepFiles = true;
                }
            }
            finally {
                ((IDisposable)_tempFiles).Dispose();
                _tempFiles = null;
            }
        }

        // DuplicateFileAttributes
        //
        // Copy all the files attributes that we care about from the source
        // file to the destination file
        //
        private void DuplicateFileAttributes( string source, string destination )
        {
#if !FEATURE_PAL
            FileAttributes      attributes;
            DateTime            creationTime;

            // Copy File Attributes, ie. Hidden, Readonly, etc.
            attributes = File.GetAttributes( source );
            File.SetAttributes( destination, attributes );

            // Copy Creation Time
            creationTime = File.GetCreationTimeUtc( source );
            File.SetCreationTimeUtc( destination, creationTime );

            // Copy ACL's
            DuplicateTemplateAttributes( source, destination );
#endif	// FEATURE_PAL
        }

        // DuplicateTemplateAttributes
        //
        // Copy over all the attributes you would want copied from a template file.
        // As of right now this is just acl's
        //
        private void DuplicateTemplateAttributes( string source, string destination ) {
#if !FEATURE_PAL
            if (IsWinNT) {
                FileSecurity        fileSecurity;

                // Copy Security information
                fileSecurity = File.GetAccessControl( source, AccessControlSections.Access );

                // Mark dirty, so effective for write
                fileSecurity.SetAccessRuleProtection( fileSecurity.AreAccessRulesProtected, true );
                File.SetAccessControl( destination, fileSecurity );
            }
            else {
                FileAttributes  fileAttributes;

                fileAttributes = File.GetAttributes( source );
                File.SetAttributes( destination, fileAttributes );
            }
#endif	// FEATURE_PAL
        }

        // ValidateWriteAccess
        //
        // Validate that we can write to the file.  This will enforce the ACL's
        // on the file.  Since we do our moving of files to replace, this is 
        // nice to ensure we are not by-passing some security permission
        // that someone set (although that could bypass this via move themselves)
        //
        // Note: 1) This is really just a nice to have, since with directory permissions
        //          they could do the same thing we are doing
        //
        //       2) We are depending on the current behavior that if the file is locked 
        //          and we can not open it, that we will get an UnauthorizedAccessException
        //          and not the IOException.
        //
        private void ValidateWriteAccess( string filename ) {
            FileStream fs = null;

            try {
                // Try to open file for write access
                fs = new FileStream( filename,
                                     FileMode.Open,
                                     FileAccess.Write,
                                     FileShare.ReadWrite );
            }
            catch ( UnauthorizedAccessException ) {
                // Access was denied, make sure we throw this
                throw;
            }
            catch ( IOException ) {
                // Someone else was using the file.  Since we did not get
                // the unauthorizedAccessException we have access to the file
            }
            catch ( Exception ) {
                // Unexpected, so just throw for safety sake
                throw;
            }
            finally {
                if ( fs != null ) {
                    fs.Close();
                }
            }
        }
        
        // ReplaceFile
        //
        // Replace one file with another using MoveFileEx.  This will
        // retry the operation if the file is locked because someone
        // is reading it
        //
        private void ReplaceFile( string Source, string Target )
        {
            bool WriteSucceeded = false;
            int  Duration       = 0;

            WriteSucceeded = AttemptMove( Source, Target );

            // The file may be open for read, if it is then 
            // lets try again because maybe they will finish
            // soon, and we will be able to replace
            while ( !WriteSucceeded                &&
                    ( Duration < SAVING_TIMEOUT )  &&
                    File.Exists( Target )          &&
                    !FileIsWriteLocked( Target ) ) {
                    
                Thread.Sleep( SAVING_RETRY_INTERVAL );

                Duration += SAVING_RETRY_INTERVAL;                

                WriteSucceeded = AttemptMove( Source, Target );
            }

            if ( !WriteSucceeded ) {
                
                throw new ConfigurationErrorsException(
                              SR.GetString(SR.Config_write_failed, Target) );
            }
        }

        // AttemptMove
        //
        // Attempt to move a file from one location to another
        //
        // Return Values:
        //   TRUE  - Move Successful
        //   FALSE - Move Failed
        private bool AttemptMove( string Source, string Target ) {
            bool MoveSuccessful = false;

            if ( IsWinNT ) {

                // We can only call this when we have kernel32.dll
                MoveSuccessful = UnsafeNativeMethods.MoveFileEx( 
                                     Source,
                                     Target,
                                     UnsafeNativeMethods.MOVEFILE_REPLACE_EXISTING );
            }
            else {

                try {
                    // VSWhidbey 548017:
                    // File.Move isn't supported on Win9x.  We'll use File.Copy
                    // instead.  Please note that Source is a temporary file which 
                    // will be deleted when _tempFiles is disposed.
                    File.Copy(Source, Target, true);
                    MoveSuccessful = true;
                }
                catch {
                    
                    MoveSuccessful = false;
                }
                
            }

            return MoveSuccessful;
        }
        
        // FileIsWriteLocked
        //
        // Is the file write locked or not?
        //
        private bool FileIsWriteLocked( string FileName ) {
            Stream FileStream  = null;
            bool   WriteLocked = true;

            if (!FileUtil.FileExists(FileName, true)) {
                // It can't be locked if it doesn't exist
                return false;
            }

            try {
                FileShare fileShare = FileShare.Read;

                if (IsWinNT) {
                    fileShare |= FileShare.Delete;
                }
                
                // Try to open for shared reading
                FileStream  = new FileStream( FileName, 
                                              FileMode.Open, 
                                              FileAccess.Read, 
                                              fileShare);

                // If we can open it for shared reading, it is not 
                // write locked
                WriteLocked = false;
            }
            finally {
                if ( FileStream != null ) {
                    
                    FileStream.Close();
                    FileStream = null;
                }
            }
            
            return WriteLocked;
        }

        // IsWinNT
        //
        // Are we running in WinNT or not?
        //
        private bool IsWinNT {
            get {
                if ( !_osPlatformDetermined ) {
                    
                    _osPlatform = Environment.OSVersion.Platform;
                    _osPlatformDetermined = true;
                }

                return ( _osPlatform == System.PlatformID.Win32NT );
            }
        }
    }
}
