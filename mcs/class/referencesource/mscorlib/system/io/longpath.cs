// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==g
/*============================================================
**
** Class:  File
** 
** <OWNER>[....]</OWNER>
**
**
** Purpose: Long paths
**
===========================================================*/

using System;
using System.Security.Permissions;
using PermissionSet = System.Security.PermissionSet;
using Win32Native = Microsoft.Win32.Win32Native;
using System.Runtime.InteropServices;
using System.Security;
#if FEATURE_MACL
using System.Security.AccessControl;
#endif
using System.Text;
using Microsoft.Win32.SafeHandles;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.Versioning;
using System.Diagnostics.Contracts;
    
namespace System.IO {
 
    [ComVisible(false)] 
    static class LongPath
    {
        [System.Security.SecurityCritical]
        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        internal unsafe static String NormalizePath(String path)
        {
            Contract.Requires(path != null);
            return NormalizePath(path, true);
        }

        [System.Security.SecurityCritical]
        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        internal unsafe static String NormalizePath(String path, bool fullCheck)
        {
            Contract.Requires(path != null);
            return Path.NormalizePath(path, fullCheck, Path.MaxLongPath);
        }

        internal static String InternalCombine(String path1, String path2)
        {
            Contract.Requires(path1 != null);
            Contract.Requires(path2 != null);
            Contract.Requires(path2.Length != 0);
            Contract.Requires(!IsPathRooted(path2));

            bool removedPrefix;
            String tempPath1 = TryRemoveLongPathPrefix(path1, out removedPrefix);

            String tempResult = Path.InternalCombine(tempPath1, path2);

            if (removedPrefix)
            {
                tempResult = Path.AddLongPathPrefix(tempResult);
            }
            return tempResult;
        }

        internal static int GetRootLength(String path)
        {
            bool removedPrefix;
            String tempPath = TryRemoveLongPathPrefix(path, out removedPrefix);

            int root = Path.GetRootLength(tempPath);
            if (removedPrefix)
            {
                root += 4;
            }
            return root;
        }

        // Tests if the given path contains a root. A path is considered rooted
        // if it starts with a backslash ("\") or a drive letter and a colon (":").
        //
        [Pure]
        internal static bool IsPathRooted(String path)
        {
            Contract.Requires(path != null);
            String tempPath = Path.RemoveLongPathPrefix(path);
            return Path.IsPathRooted(tempPath);
        }

        // Returns the root portion of the given path. The resulting string
        // consists of those rightmost characters of the path that constitute the
        // root of the path. Possible patterns for the resulting string are: An
        // empty string (a relative path on the current drive), "\" (an absolute
        // path on the current drive), "X:" (a relative path on a given drive,
        // where X is the drive letter), "X:\" (an absolute path on a given drive),
        // and "\\server\share" (a UNC path for a given server and share name).
        // The resulting string is null if path is null.
        //
        [System.Security.SecurityCritical]
        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        internal static String GetPathRoot(String path)
        {
            if (path == null) return null;

            bool removedPrefix;
            String tempPath = TryRemoveLongPathPrefix(path, out removedPrefix);

            tempPath = NormalizePath(tempPath, false);
            String result = path.Substring(0, GetRootLength(tempPath));

            if (removedPrefix)
            {
                result = Path.AddLongPathPrefix(result);
            }
            return result;
        }

        // Returns the directory path of a file path. This method effectively
        // removes the last element of the given file path, i.e. it returns a
        // string consisting of all characters up to but not including the last
        // backslash ("\") in the file path. The returned value is null if the file
        // path is null or if the file path denotes a root (such as "\", "C:", or
        // "\\server\share").
        [System.Security.SecurityCritical]  // auto-generated
        [ResourceExposure(ResourceScope.None)]
        [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)]
        internal static String GetDirectoryName(String path)
        {
            if (path != null)
            {
                bool removedPrefix;
                String tempPath = TryRemoveLongPathPrefix(path, out removedPrefix);

                Path.CheckInvalidPathChars(tempPath);
                path = NormalizePath(tempPath, false);
                int root = GetRootLength(tempPath);
                int i = tempPath.Length;
                if (i > root)
                {
                    i = tempPath.Length;
                    if (i == root) return null;
                    while (i > root && tempPath[--i] != Path.DirectorySeparatorChar && tempPath[i] != Path.AltDirectorySeparatorChar);
                    String result = tempPath.Substring(0, i);
                    if (removedPrefix)
                    {
                        result = Path.AddLongPathPrefix(result);
                    }

                    return result;
                }
            }
            return null;
        }

        internal static String TryRemoveLongPathPrefix(String path, out bool removed)
        {
            Contract.Requires(path != null);
            removed = Path.HasLongPathPrefix(path);
            if (!removed)
                return path;
            return Path.RemoveLongPathPrefix(path);
        }
    }

    [ComVisible(false)] 
    static class LongPathFile
    {

        // Copies an existing file to a new file. If overwrite is 
        // false, then an IOException is thrown if the destination file 
        // already exists.  If overwrite is true, the file is 
        // overwritten.
        //
        // The caller must have certain FileIOPermissions.  The caller must have
        // Read permission to sourceFileName 
        // and Write permissions to destFileName.
        // 
        [System.Security.SecurityCritical]
        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        internal static void Copy(String sourceFileName, String destFileName, bool overwrite) {
            Contract.Requires(sourceFileName != null);
            Contract.Requires(destFileName != null);
            Contract.Requires(sourceFileName.Length > 0);
            Contract.Requires(destFileName.Length > 0);

            String fullSourceFileName = LongPath.NormalizePath(sourceFileName);
            new FileIOPermission(FileIOPermissionAccess.Read, new String[] { fullSourceFileName }, false, false).Demand();
            String fullDestFileName = LongPath.NormalizePath(destFileName);
            new FileIOPermission(FileIOPermissionAccess.Write, new String[] { fullDestFileName }, false, false).Demand();

            InternalCopy(fullSourceFileName, fullDestFileName, sourceFileName, destFileName, overwrite);
        }

        [System.Security.SecurityCritical]
        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        private static String InternalCopy(String fullSourceFileName, String fullDestFileName, String sourceFileName, String destFileName, bool overwrite) {
            Contract.Requires(fullSourceFileName != null);
            Contract.Requires(fullDestFileName != null);
            Contract.Requires(fullSourceFileName.Length > 0);
            Contract.Requires(fullDestFileName.Length > 0);

            fullSourceFileName = Path.AddLongPathPrefix(fullSourceFileName);
            fullDestFileName = Path.AddLongPathPrefix(fullDestFileName);
            bool r = Win32Native.CopyFile(fullSourceFileName, fullDestFileName, !overwrite);
            if (!r) {
                // Save Win32 error because subsequent checks will overwrite this HRESULT.
                int errorCode = Marshal.GetLastWin32Error();
                String fileName = destFileName;

                if (errorCode != Win32Native.ERROR_FILE_EXISTS) {
                    // For a number of error codes (sharing violation, path 
                    // not found, etc) we don't know if the problem was with
                    // the source or dest file.  Try reading the source file.
                    using(SafeFileHandle handle = Win32Native.UnsafeCreateFile(fullSourceFileName, FileStream.GENERIC_READ, FileShare.Read, null, FileMode.Open, 0, IntPtr.Zero)) {
                        if (handle.IsInvalid)
                            fileName = sourceFileName;
                    }

                    if (errorCode == Win32Native.ERROR_ACCESS_DENIED) {
                        if (LongPathDirectory.InternalExists(fullDestFileName))
                            throw new IOException(Environment.GetResourceString("Arg_FileIsDirectory_Name", destFileName), Win32Native.ERROR_ACCESS_DENIED, fullDestFileName);
                    }
                }

                __Error.WinIOError(errorCode, fileName);
            }
                
            return fullDestFileName;
        }

        // Deletes a file. The file specified by the designated path is deleted.
        // If the file does not exist, Delete succeeds without throwing
        // an exception.
        // 
        // On NT, Delete will fail for a file that is open for normal I/O
        // or a file that is memory mapped.  
        // 
        // Your application must have Delete permission to the target file.
        // 
        [System.Security.SecurityCritical] 
        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        internal static void Delete(String path) {
            Contract.Requires(path != null);

            String fullPath = LongPath.NormalizePath(path);

            // For security check, path should be resolved to an absolute path.
            new FileIOPermission(FileIOPermissionAccess.Write, new String[] { fullPath }, false, false ).Demand();

            String tempPath = Path.AddLongPathPrefix(fullPath);
            bool r = Win32Native.DeleteFile(tempPath);
            if (!r) {
                int hr = Marshal.GetLastWin32Error();
                if (hr==Win32Native.ERROR_FILE_NOT_FOUND)
                    return;
                else
                    __Error.WinIOError(hr, fullPath);
            }
        }
        
        // Tests if a file exists. The result is true if the file
        // given by the specified path exists; otherwise, the result is
        // false.  Note that if path describes a directory,
        // Exists will return true.
        //
        // Your application must have Read permission for the target directory.
        // 
        [System.Security.SecurityCritical] 
        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        internal static bool Exists(String path) {
            try
            {
                if (path==null)
                    return false;
                if (path.Length==0)
                    return false;
            
                path = LongPath.NormalizePath(path);
                // After normalizing, check whether path ends in directory separator.
                // Otherwise, FillAttributeInfo removes it and we may return a false positive.
                // GetFullPathInternal should never return null
                Contract.Assert(path != null, "File.Exists: GetFullPathInternal returned null");
                if (path.Length > 0 && Path.IsDirectorySeparator(path[path.Length - 1])) {
                    return false;
                }
                    
                new FileIOPermission(FileIOPermissionAccess.Read, new String[] { path }, false, false ).Demand();

                return InternalExists(path);
            }
            catch(ArgumentException) {} 
            catch(NotSupportedException) {} // Security can throw this on ":"
            catch(SecurityException) {}
            catch(IOException) {}
            catch(UnauthorizedAccessException) {}

            return false;
        }

        [System.Security.SecurityCritical]
        internal static bool InternalExists(String path) {
            Contract.Requires(path != null);
            String tempPath = Path.AddLongPathPrefix(path);
            return File.InternalExists(tempPath);
        }

        [System.Security.SecurityCritical]
        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        internal static DateTimeOffset GetCreationTime(String path)
        {
            Contract.Requires(path != null);

            String fullPath = LongPath.NormalizePath(path);
            new FileIOPermission(FileIOPermissionAccess.Read, new String[] { fullPath }, false, false ).Demand();

            String tempPath = Path.AddLongPathPrefix(fullPath);

            Win32Native.WIN32_FILE_ATTRIBUTE_DATA data = new Win32Native.WIN32_FILE_ATTRIBUTE_DATA();
            int dataInitialised = File.FillAttributeInfo(tempPath, ref data, false, false);
            if (dataInitialised != 0)
                __Error.WinIOError(dataInitialised, fullPath);

            long dt = ((long)(data.ftCreationTimeHigh) << 32) | ((long)data.ftCreationTimeLow);
            DateTime dtLocal = DateTime.FromFileTimeUtc(dt).ToLocalTime();
            return new DateTimeOffset(dtLocal).ToLocalTime();
        }

        [System.Security.SecurityCritical]
        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        internal static DateTimeOffset GetLastAccessTime(String path)
        {
            Contract.Requires(path != null);

            String fullPath = LongPath.NormalizePath(path);
            new FileIOPermission(FileIOPermissionAccess.Read, new String[] { fullPath }, false, false ).Demand();

            String tempPath = Path.AddLongPathPrefix(fullPath);
            Win32Native.WIN32_FILE_ATTRIBUTE_DATA data = new Win32Native.WIN32_FILE_ATTRIBUTE_DATA();
            int dataInitialised = File.FillAttributeInfo(tempPath, ref data, false, false);
            if (dataInitialised != 0)
                __Error.WinIOError(dataInitialised, fullPath);

            long dt = ((long)(data.ftLastAccessTimeHigh) << 32) | ((long)data.ftLastAccessTimeLow);
            DateTime dtLocal = DateTime.FromFileTimeUtc(dt).ToLocalTime();
            return new DateTimeOffset(dtLocal).ToLocalTime();
        }

        [System.Security.SecurityCritical] 
        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        internal static DateTimeOffset GetLastWriteTime(String path)
        {
            Contract.Requires(path != null);

            String fullPath = LongPath.NormalizePath(path);
            new FileIOPermission(FileIOPermissionAccess.Read, new String[] { fullPath }, false, false ).Demand();

            String tempPath = Path.AddLongPathPrefix(fullPath);
            Win32Native.WIN32_FILE_ATTRIBUTE_DATA data = new Win32Native.WIN32_FILE_ATTRIBUTE_DATA();
            int dataInitialised = File.FillAttributeInfo(tempPath, ref data, false, false);
            if (dataInitialised != 0)
                __Error.WinIOError(dataInitialised, fullPath);

            long dt = ((long)data.ftLastWriteTimeHigh << 32) | ((long)data.ftLastWriteTimeLow);
            DateTime dtLocal = DateTime.FromFileTimeUtc(dt).ToLocalTime();
            return new DateTimeOffset(dtLocal).ToLocalTime();
        }

        // Moves a specified file to a new location and potentially a new file name.
        // This method does work across volumes.
        //
        // The caller must have certain FileIOPermissions.  The caller must
        // have Read and Write permission to 
        // sourceFileName and Write 
        // permissions to destFileName.
        // 
        [System.Security.SecurityCritical]
        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        internal static void Move(String sourceFileName, String destFileName) {
            Contract.Requires(sourceFileName != null);
            Contract.Requires(destFileName != null);
            Contract.Requires(sourceFileName.Length > 0);
            Contract.Requires(destFileName.Length > 0);

            String fullSourceFileName = LongPath.NormalizePath(sourceFileName);
            new FileIOPermission(FileIOPermissionAccess.Write | FileIOPermissionAccess.Read, new String[] { fullSourceFileName }, false, false).Demand();
            String fullDestFileName = LongPath.NormalizePath(destFileName);
            new FileIOPermission(FileIOPermissionAccess.Write, new String[] { fullDestFileName }, false, false).Demand();

            if (!LongPathFile.InternalExists(fullSourceFileName))
                __Error.WinIOError(Win32Native.ERROR_FILE_NOT_FOUND, fullSourceFileName);

            String tempSourceFileName = Path.AddLongPathPrefix(fullSourceFileName);
            String tempDestFileName = Path.AddLongPathPrefix(fullDestFileName);

            if (!Win32Native.MoveFile(tempSourceFileName, tempDestFileName))
            {
                __Error.WinIOError();
            }
        }

        // throws FileNotFoundException if not found
        [System.Security.SecurityCritical]
        internal static long GetLength(String path)
        {
            Contract.Requires(path != null);

            String fullPath = LongPath.NormalizePath(path);
            new FileIOPermission(FileIOPermissionAccess.Read, new String[] { fullPath }, false, false ).Demand();

            String tempPath = Path.AddLongPathPrefix(fullPath);
            Win32Native.WIN32_FILE_ATTRIBUTE_DATA data = new Win32Native.WIN32_FILE_ATTRIBUTE_DATA();
            int dataInitialised = File.FillAttributeInfo(tempPath, ref data, false, true); // return error
            if (dataInitialised != 0)
                __Error.WinIOError(dataInitialised, path); // from FileInfo.

            if ((data.fileAttributes & Win32Native.FILE_ATTRIBUTE_DIRECTORY) != 0)
                __Error.WinIOError(Win32Native.ERROR_FILE_NOT_FOUND, path);

            return ((long)data.fileSizeHigh) << 32 | ((long)data.fileSizeLow & 0xFFFFFFFFL);
            
        }

         // Defined in WinError.h
        private const int ERROR_ACCESS_DENIED = 0x5;     
    }

    [ComVisible(false)] 
    static class LongPathDirectory
    {
        [System.Security.SecurityCritical]  // auto-generated
        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        internal static void CreateDirectory(String path)
        {
            Contract.Requires(path != null);
            Contract.Requires(path.Length > 0);

            String fullPath = LongPath.NormalizePath(path);

            // You need read access to the directory to be returned back and write access to all the directories 
            // that you need to create. If we fail any security checks we will not create any directories at all.
            // We attempt to create directories only after all the security checks have passed. This is avoid doing
            // a demand at every level.
            String demandDir = GetDemandDir(fullPath, true);
            new FileIOPermission(FileIOPermissionAccess.Read, new String[] { demandDir }, false, false).Demand();

            InternalCreateDirectory(fullPath, path, null);
        }

        [System.Security.SecurityCritical]  // auto-generated
        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        private unsafe static void InternalCreateDirectory(String fullPath, String path, Object dirSecurityObj)
        {
#if FEATURE_MACL
            DirectorySecurity dirSecurity = (DirectorySecurity)dirSecurityObj;
#endif // FEATURE_MACL

            int length = fullPath.Length;

            // We need to trim the trailing slash or the code will try to create 2 directories of the same name.
            if (length >= 2 && Path.IsDirectorySeparator(fullPath[length - 1]))
                length--;

            int lengthRoot = LongPath.GetRootLength(fullPath); 

#if !PLATFORM_UNIX
            // For UNC paths that are only // or /// 
            if (length == 2 && Path.IsDirectorySeparator(fullPath[1]))
                throw new IOException(Environment.GetResourceString("IO.IO_CannotCreateDirectory", path));
#endif // !PLATFORM_UNIX

            List<string> stackDir = new List<string>();

            // Attempt to figure out which directories don't exist, and only
            // create the ones we need.  Note that InternalExists may fail due
            // to Win32 ACL's preventing us from seeing a directory, and this
            // isn't threadsafe.

            bool somepathexists = false;

            if (length > lengthRoot)
            { // Special case root (fullpath = X:\\)
                int i = length - 1;
                while (i >= lengthRoot && !somepathexists)
                {
                    String dir = fullPath.Substring(0, i + 1);

                    if (!InternalExists(dir)) // Create only the ones missing
                        stackDir.Add(dir);
                    else
                        somepathexists = true;

                    while (i > lengthRoot && fullPath[i] != Path.DirectorySeparatorChar && fullPath[i] != Path.AltDirectorySeparatorChar) i--;
                    i--;
                }
            }

            int count = stackDir.Count;

            if (stackDir.Count != 0)
            {
                String[] securityList = new String[stackDir.Count];
                stackDir.CopyTo(securityList, 0);
                for (int j = 0; j < securityList.Length; j++)
                    securityList[j] += "\\."; // leaf will never have a slash at the end

                // Security check for all directories not present only.
#if !FEATURE_PAL  && FEATURE_MACL
                AccessControlActions control = (dirSecurity == null) ? AccessControlActions.None : AccessControlActions.Change;
                new FileIOPermission(FileIOPermissionAccess.Write, control, securityList, false, false ).Demand();
#else
                new FileIOPermission(FileIOPermissionAccess.Write, securityList, false, false).Demand();
#endif
            }

            // If we were passed a DirectorySecurity, convert it to a security
            // descriptor and set it in he call to CreateDirectory.
            Win32Native.SECURITY_ATTRIBUTES secAttrs = null;
#if FEATURE_MACL
            if (dirSecurity != null) {
                secAttrs = new Win32Native.SECURITY_ATTRIBUTES();
                secAttrs.nLength = (int)Marshal.SizeOf(secAttrs);

                // For ACL's, get the security descriptor from the FileSecurity.
                byte[] sd = dirSecurity.GetSecurityDescriptorBinaryForm();
                byte * bytesOnStack = stackalloc byte[sd.Length];
                Buffer.Memcpy(bytesOnStack, 0, sd, 0, sd.Length);
                secAttrs.pSecurityDescriptor = bytesOnStack;
            }
#endif

            bool r = true;
            int firstError = 0;
            String errorString = path;
            // If all the security checks succeeded create all the directories
            while (stackDir.Count > 0)
            {
                String name = stackDir[stackDir.Count - 1];
                stackDir.RemoveAt(stackDir.Count - 1);
                if (name.Length >= Path.MaxLongPath)
                    throw new PathTooLongException(Environment.GetResourceString("IO.PathTooLong"));
                r = Win32Native.CreateDirectory(Path.AddLongPathPrefix(name), secAttrs);
                if (!r && (firstError == 0))
                {
                    int currentError = Marshal.GetLastWin32Error();
                    // While we tried to avoid creating directories that don't
                    // exist above, there are at least two cases that will 
                    // cause us to see ERROR_ALREADY_EXISTS here.  InternalExists 
                    // can fail because we didn't have permission to the 
                    // directory.  Secondly, another thread or process could
                    // create the directory between the time we check and the
                    // time we try using the directory.  Thirdly, it could
                    // fail because the target does exist, but is a file.
                    if (currentError != Win32Native.ERROR_ALREADY_EXISTS)
                        firstError = currentError;
                    else
                    {
                        // If there's a file in this directory's place, or if we have ERROR_ACCESS_DENIED when checking if the directory already exists throw.
                        if (LongPathFile.InternalExists(name) || (!InternalExists(name, out currentError) && currentError == Win32Native.ERROR_ACCESS_DENIED))
                        {
                            firstError = currentError;
                            // Give the user a nice error message, but don't leak path information.
                            try
                            {
                                new FileIOPermission(FileIOPermissionAccess.PathDiscovery, new String[] { GetDemandDir(name, true) }, false, false).Demand();
                                errorString = name;
                            }
                            catch (SecurityException) { }
                        }
                    }
                }
            }

            // We need this check to mask OS differences
            // Handle CreateDirectory("X:\\foo") when X: doesn't exist. Similarly for n/w paths.
            if ((count == 0) && !somepathexists)
            {
                String root = InternalGetDirectoryRoot(fullPath);
                if (!InternalExists(root))
                {
                    // Extract the root from the passed in path again for security.
                    __Error.WinIOError(Win32Native.ERROR_PATH_NOT_FOUND, InternalGetDirectoryRoot(path));
                }
                return;
            }

            // Only throw an exception if creating the exact directory we 
            // wanted failed to work correctly.
            if (!r && (firstError != 0))
            {
                __Error.WinIOError(firstError, errorString);
            }
        }
      
        [System.Security.SecurityCritical] 
        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        internal static void Move(String sourceDirName, String destDirName)
        {
            Contract.Requires(sourceDirName != null);
            Contract.Requires(destDirName != null);
            Contract.Requires(sourceDirName.Length != 0);
            Contract.Requires(destDirName.Length != 0);

            String fullsourceDirName = LongPath.NormalizePath(sourceDirName);
            String sourcePath = GetDemandDir(fullsourceDirName, false);

            if (sourcePath.Length >= Path.MaxLongPath)
                throw new PathTooLongException(Environment.GetResourceString("IO.PathTooLong"));

            String fulldestDirName = LongPath.NormalizePath(destDirName);
            String destPath = GetDemandDir(fulldestDirName, false);

            if (destPath.Length >= Path.MaxLongPath)
                throw new PathTooLongException(Environment.GetResourceString("IO.PathTooLong"));

            new FileIOPermission(FileIOPermissionAccess.Write | FileIOPermissionAccess.Read, new String[] { sourcePath }, false, false).Demand();
            new FileIOPermission(FileIOPermissionAccess.Write, new String[] { destPath }, false, false).Demand();

            if (String.Compare(sourcePath, destPath, StringComparison.OrdinalIgnoreCase) == 0)
                throw new IOException(Environment.GetResourceString("IO.IO_SourceDestMustBeDifferent"));

            String sourceRoot = LongPath.GetPathRoot(sourcePath);
            String destinationRoot = LongPath.GetPathRoot(destPath);
            if (String.Compare(sourceRoot, destinationRoot, StringComparison.OrdinalIgnoreCase) != 0)
                throw new IOException(Environment.GetResourceString("IO.IO_SourceDestMustHaveSameRoot"));


            String tempSourceDirName = Path.AddLongPathPrefix(sourceDirName);
            String tempDestDirName = Path.AddLongPathPrefix(destDirName);

            if (!Win32Native.MoveFile(tempSourceDirName, tempDestDirName))
            {
                int hr = Marshal.GetLastWin32Error();
                if (hr == Win32Native.ERROR_FILE_NOT_FOUND) // Source dir not found
                {
                    hr = Win32Native.ERROR_PATH_NOT_FOUND;
                    __Error.WinIOError(hr, fullsourceDirName);
                }
                // This check was originally put in for Win9x (unfortunately without special casing it to be for Win9x only). We can't change the NT codepath now for backcomp reasons.
                if (hr == Win32Native.ERROR_ACCESS_DENIED) // WinNT throws IOException. This check is for Win9x. We can't change it for backcomp.
                    throw new IOException(Environment.GetResourceString("UnauthorizedAccess_IODenied_Path", sourceDirName), Win32Native.MakeHRFromErrorCode(hr));
                __Error.WinIOError(hr, String.Empty);
            }
        }

        [System.Security.SecurityCritical]
        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        internal static void Delete(String path, bool recursive)
        {
            String fullPath = LongPath.NormalizePath(path);
           InternalDelete(fullPath, path, recursive);
        }

        // FullPath is fully qualified, while the user path is used for feedback in exceptions
        [System.Security.SecurityCritical]
        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        private static void InternalDelete(String fullPath, String userPath, bool recursive)
        {
            String demandPath;

            // If not recursive, do permission check only on this directory
            // else check for the whole directory structure rooted below 
            demandPath = GetDemandDir(fullPath, !recursive);

            // Make sure we have write permission to this directory
            new FileIOPermission(FileIOPermissionAccess.Write, new String[] { demandPath }, false, false).Demand();

            String longPath = Path.AddLongPathPrefix(fullPath);
            // Do not recursively delete through reparse points.  Perhaps in a 
            // future version we will add a new flag to control this behavior, 
            // but for now we're much safer if we err on the conservative side.
            // This applies to symbolic links and mount points.
            Win32Native.WIN32_FILE_ATTRIBUTE_DATA data = new Win32Native.WIN32_FILE_ATTRIBUTE_DATA();
            int dataInitialised = File.FillAttributeInfo(longPath, ref data, false, true);
            if (dataInitialised != 0)
            {
                // Ensure we throw a DirectoryNotFoundException.
                if (dataInitialised == Win32Native.ERROR_FILE_NOT_FOUND)
                    dataInitialised = Win32Native.ERROR_PATH_NOT_FOUND;
                __Error.WinIOError(dataInitialised, fullPath);
            }

            if (((FileAttributes)data.fileAttributes & FileAttributes.ReparsePoint) != 0)
                recursive = false;

            DeleteHelper(longPath, userPath, recursive, true);
        }

        // Note that fullPath is fully qualified, while userPath may be 
        // relative.  Use userPath for all exception messages to avoid leaking
        // fully qualified path information.
        [System.Security.SecurityCritical]
        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        private static void DeleteHelper(String fullPath, String userPath, bool recursive, bool throwOnTopLevelDirectoryNotFound)
        {
            bool r;
            int hr;
            Exception ex = null;

            // Do not recursively delete through reparse points.  Perhaps in a 
            // future version we will add a new flag to control this behavior, 
            // but for now we're much safer if we err on the conservative side.
            // This applies to symbolic links and mount points.
            // Note the logic to check whether fullPath is a reparse point is
            // in Delete(String, String, bool), and will set "recursive" to false.
            // Note that Win32's DeleteFile and RemoveDirectory will just delete
            // the reparse point itself.

            if (recursive)
            {
                Win32Native.WIN32_FIND_DATA data = new Win32Native.WIN32_FIND_DATA();

                String searchPath = null;
                if (fullPath.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal))
                {
                    searchPath = fullPath + "*";
                }
                else
                {
                    searchPath = fullPath + Path.DirectorySeparatorChar + "*";
                }

                // Open a Find handle
                using (SafeFindHandle hnd = Win32Native.FindFirstFile(searchPath, data))
                {
                    if (hnd.IsInvalid)
                    {
                        hr = Marshal.GetLastWin32Error();
                        __Error.WinIOError(hr, userPath);
                    }

                    do
                    {
                        bool isDir = (0 != (data.dwFileAttributes & Win32Native.FILE_ATTRIBUTE_DIRECTORY));
                        if (isDir)
                        {
                            // Skip ".", "..".
                            if (data.cFileName.Equals(".") || data.cFileName.Equals(".."))
                                continue;

                            // Recurse for all directories, unless they are 
                            // reparse points.  Do not follow mount points nor
                            // symbolic links, but do delete the reparse point 
                            // itself.
                            bool shouldRecurse = (0 == (data.dwFileAttributes & (int)FileAttributes.ReparsePoint));
                            if (shouldRecurse)
                            {
                                String newFullPath = LongPath.InternalCombine(fullPath, data.cFileName);
                                String newUserPath = LongPath.InternalCombine(userPath, data.cFileName);
                                try
                                {
                                    DeleteHelper(newFullPath, newUserPath, recursive, false);
                                }
                                catch (Exception e)
                                {
                                    if (ex == null)
                                    {
                                        ex = e;
                                    }
                                }
                            }
                            else
                            {
                                // Check to see if this is a mount point, and
                                // unmount it.
                                if (data.dwReserved0 == Win32Native.IO_REPARSE_TAG_MOUNT_POINT)
                                {
                                    // Use full path plus a trailing '\'
                                    String mountPoint = LongPath.InternalCombine(fullPath, data.cFileName + Path.DirectorySeparatorChar);
                                    r = Win32Native.DeleteVolumeMountPoint(mountPoint);
                                    if (!r)
                                    {
                                        hr = Marshal.GetLastWin32Error();
                                        if (hr != Win32Native.ERROR_PATH_NOT_FOUND)
                                        {
                                            try
                                            {
                                                __Error.WinIOError(hr, data.cFileName);
                                            }
                                            catch (Exception e)
                                            {
                                                if (ex == null)
                                                {
                                                    ex = e;
                                                }
                                            }
                                        }
                                    }
                                }

                                // RemoveDirectory on a symbolic link will
                                // remove the link itself.
                                String reparsePoint = LongPath.InternalCombine(fullPath, data.cFileName);
                                r = Win32Native.RemoveDirectory(reparsePoint);
                                if (!r)
                                {
                                    hr = Marshal.GetLastWin32Error();
                                    if (hr != Win32Native.ERROR_PATH_NOT_FOUND)
                                    {
                                        try
                                        {
                                            __Error.WinIOError(hr, data.cFileName);
                                        }
                                        catch (Exception e)
                                        {
                                            if (ex == null)
                                            {
                                                ex = e;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            String fileName = LongPath.InternalCombine(fullPath, data.cFileName);
                            r = Win32Native.DeleteFile(fileName);
                            if (!r)
                            {
                                hr = Marshal.GetLastWin32Error();
                                if (hr != Win32Native.ERROR_FILE_NOT_FOUND)
                                {
                                    try
                                    {
                                        __Error.WinIOError(hr, data.cFileName);
                                    }
                                    catch (Exception e)
                                    {
                                        if (ex == null)
                                        {
                                            ex = e;
                                        }
                                    }
                                }
                            }
                        }
                    } while (Win32Native.FindNextFile(hnd, data));
                    // Make sure we quit with a sensible error.
                    hr = Marshal.GetLastWin32Error();
                }

                if (ex != null)
                    throw ex;
                if (hr != 0 && hr != Win32Native.ERROR_NO_MORE_FILES)
                    __Error.WinIOError(hr, userPath);
            }

            r = Win32Native.RemoveDirectory(fullPath);

            if (!r)
            {
                hr = Marshal.GetLastWin32Error();
                if (hr == Win32Native.ERROR_FILE_NOT_FOUND) // A dubious error code.
                    hr = Win32Native.ERROR_PATH_NOT_FOUND;
                // This check was originally put in for Win9x (unfortunately without special casing it to be for Win9x only). We can't change the NT codepath now for backcomp reasons.
                if (hr == Win32Native.ERROR_ACCESS_DENIED)
                    throw new IOException(Environment.GetResourceString("UnauthorizedAccess_IODenied_Path", userPath));

                // don't throw the DirectoryNotFoundException since this is a subdir and there could be a ----
                // between two Directory.Delete callers
                if (hr == Win32Native.ERROR_PATH_NOT_FOUND && !throwOnTopLevelDirectoryNotFound)
                    return;  

                __Error.WinIOError(hr, userPath);
            }
        }

        [System.Security.SecurityCritical] 
        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        internal static bool Exists(String path)
        {
            try
            {
                if (path == null)
                    return false;
                if (path.Length == 0)
                    return false;

                // Get fully qualified file name ending in \* for security check

                String fullPath = LongPath.NormalizePath(path);
                String demandPath = GetDemandDir(fullPath, true);

                new FileIOPermission(FileIOPermissionAccess.Read, new String[] { demandPath }, false, false).Demand();

                return InternalExists(fullPath);
            }
            catch (ArgumentException) { }
            catch (NotSupportedException) { }  // Security can throw this on ":"
            catch (SecurityException) { }
            catch (IOException) { }
            catch (UnauthorizedAccessException)
            {
#if !FEATURE_PAL
                Contract.Assert(false, "Ignore this assert and file a bug to the BCL team. This assert was tracking purposes only.");
#endif //!FEATURE_PAL
            }
            return false;
        }

        [System.Security.SecurityCritical]  // auto-generated
        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        internal static bool InternalExists(String path)
        {
            Contract.Requires(path != null);
            int lastError = Win32Native.ERROR_SUCCESS;
            return InternalExists(path, out lastError);
        }

        [System.Security.SecurityCritical]  // auto-generated
        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        internal static bool InternalExists(String path, out int lastError) {
            Contract.Requires(path != null);            
            String tempPath = Path.AddLongPathPrefix(path);
            return Directory.InternalExists(tempPath, out lastError);
        }

        // Input to this method should already be fullpath. This method will ensure that we append 
        // the trailing slash only when appropriate and when thisDirOnly is specified append a "." 
        // at the end of the path to indicate that the demand is only for the fullpath and not 
        // everything underneath it.
        [ResourceExposure(ResourceScope.None)]
        [ResourceConsumption(ResourceScope.None, ResourceScope.None)]
        private static String GetDemandDir(string fullPath, bool thisDirOnly)
        {
            String demandPath;
            fullPath = Path.RemoveLongPathPrefix(fullPath);
            if (thisDirOnly)
            {
                if (fullPath.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal)
                    || fullPath.EndsWith(Path.AltDirectorySeparatorChar.ToString(), StringComparison.Ordinal))
                    demandPath = fullPath + '.';
                else
                    demandPath = fullPath + Path.DirectorySeparatorChar + '.';
            }
            else
            {
                if (!(fullPath.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal)
                    || fullPath.EndsWith(Path.AltDirectorySeparatorChar.ToString(), StringComparison.Ordinal)))
                    demandPath = fullPath + Path.DirectorySeparatorChar;
                else
                    demandPath = fullPath;
            }
            return demandPath;
        }

        private static String InternalGetDirectoryRoot(String path)
        {
            if (path == null) return null;
            return path.Substring(0, LongPath.GetRootLength(path));
        }
    }
}
