//------------------------------------------------------------------------------
// <copyright file="UrlPath.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

/*
 * UrlPath class
 *
 * Copyright (c) 1999 Microsoft Corporation
 */

namespace System.Web.Util {
using System.Security.Permissions;
using System.Text;
using System.Runtime.Serialization.Formatters;
using System.Runtime.InteropServices;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Web.Hosting;


internal struct FileTimeInfo {
    internal long LastWriteTime;
    internal long Size;

    internal static readonly FileTimeInfo MinValue = new FileTimeInfo(0, 0);

    internal FileTimeInfo(long lastWriteTime, long size) {
        LastWriteTime = lastWriteTime;
        Size = size;
    }

    public override bool Equals(object obj) {
        FileTimeInfo fti;

        if (obj is FileTimeInfo) {
            fti = (FileTimeInfo) obj;
            return (LastWriteTime == fti.LastWriteTime) && (Size == fti.Size);
        }
        else {
            return false;
        }
    }

    public static bool operator == (FileTimeInfo value1, FileTimeInfo value2) 
    {
        return (value1.LastWriteTime == value2.LastWriteTime) &&
               (value1.Size == value2.Size);
    }

    public unsafe static bool operator != (FileTimeInfo value1, FileTimeInfo value2) 
    {
        return !(value1 == value2);
    }

    public override int GetHashCode(){
        return HashCodeCombiner.CombineHashCodes(LastWriteTime.GetHashCode(), Size.GetHashCode());
    }

    
}

/*
 * Helper methods relating to file operations
 */
internal class FileUtil {

    private FileUtil() {
    }

    [FileIOPermission(SecurityAction.Assert, AllFiles = FileIOPermissionAccess.Read)]
    internal static bool FileExists(String filename) {
        bool exists = false;

        try {
            exists = File.Exists(filename);
        }
        catch {
        }

        return exists;
    }

    // For a given path, if its beneath the app root, return the first existing directory
    internal static string GetFirstExistingDirectory(string appRoot, string fileName) {
        if (IsBeneathAppRoot(appRoot, fileName)) {
            string existingDir = appRoot;
            do {
                int nextSeparator = fileName.IndexOf(Path.DirectorySeparatorChar, existingDir.Length + 1);
                if (nextSeparator > -1) {
                    string nextDir = fileName.Substring(0, nextSeparator);
                    if (FileUtil.DirectoryExists(nextDir, false)) {
                        existingDir = nextDir;
                        continue;
                    }
                }
                break;
            } while (true);

            return existingDir;
        }
        return null;
    }

    internal static bool IsBeneathAppRoot(string appRoot, string filePath) {
        if (filePath.Length > appRoot.Length + 1
            && filePath.IndexOf(appRoot, StringComparison.OrdinalIgnoreCase) > -1
            && filePath[appRoot.Length] == Path.DirectorySeparatorChar) {
            return true;
        }
        return false;
    }

    // Remove the final backslash from a directory path, unless it's something like c:\
    internal static String RemoveTrailingDirectoryBackSlash(String path) {

        if (path == null)
            return null;

        int length = path.Length;
        if (length > 3 && path[length - 1] == '\\')
            path = path.Substring(0, length - 1);

        return path;
    }

    private static int _maxPathLength = 259;
    // If the path is longer than the maximum length
    // Trim the end and append the hashcode to it.
    internal static String TruncatePathIfNeeded(string path, int reservedLength) {
        int maxPathLength = _maxPathLength - reservedLength;
        if (path.Length > maxPathLength) {
            // 

            path = path.Substring(0, maxPathLength - 13) +
                path.GetHashCode().ToString(CultureInfo.InvariantCulture);
        }

        return path;
    }

    /*
     * Canonicalize the directory, and makes sure it ends with a '\'
     */
    internal static string FixUpPhysicalDirectory(string dir) {
        if (dir == null)
            return null;

        dir = Path.GetFullPath(dir);

        // Append '\' to the directory if necessary.
        if (!StringUtil.StringEndsWith(dir, @"\"))
            dir = dir + @"\";

        return dir;
    }

    // Fail if the physical path is not canonical
    static internal void CheckSuspiciousPhysicalPath(string physicalPath) {
        if (IsSuspiciousPhysicalPath(physicalPath)) {
            throw new HttpException(404, String.Empty);
        }
    }

    // Check whether the physical path is not canonical
    // NOTE: this API throws if we don't have permission to the file.
    // NOTE: The compare needs to be case insensitive (VSWhidbey 444513)
    static internal bool IsSuspiciousPhysicalPath(string physicalPath) {
        bool pathTooLong;

        if (!IsSuspiciousPhysicalPath(physicalPath, out pathTooLong)) {
            return false;
        }

        if (!pathTooLong) {
            return true;
        }

        // physical path too long -> not good because we still need to make
        // it work for virtual path provider scenarios

        // first a few simple checks:
        if (physicalPath.IndexOf('/') >= 0) {
            return true;
        }
        
        string slashDots = "\\..";
        int idxSlashDots = physicalPath.IndexOf(slashDots, StringComparison.Ordinal);
        if (idxSlashDots >= 0
            && (physicalPath.Length == idxSlashDots + slashDots.Length
                || physicalPath[idxSlashDots + slashDots.Length] == '\\')) {
            return true;
        }

        // the real check is to go right to left until there is no longer path-too-long
        // and see if the canonicalization check fails then

        int pos = physicalPath.LastIndexOf('\\');

        while (pos >= 0) {
            string path = physicalPath.Substring(0, pos);

            if (!IsSuspiciousPhysicalPath(path, out pathTooLong)) {
                // reached a non-suspicious path that is not too long
                return false;
            }

            if (!pathTooLong) {
                // reached a suspicious path that is not too long
                return true;
            }

            // trim the path some more
            pos = physicalPath.LastIndexOf('\\', pos-1);
        }

        // backtracted to the end without reaching a non-suspicious path
        // this is suspicious (should happen because app root at least should be ok)
        return true;
    }

    private static readonly char[] s_invalidPathChars = Path.GetInvalidPathChars();

    // VSWhidbey 609102 - Medium trust apps may hit this method, and if the physical path exists,
    // Path.GetFullPath will seek PathDiscovery permissions and throw an exception.
    [FileIOPermissionAttribute(SecurityAction.Assert, AllFiles=FileIOPermissionAccess.PathDiscovery)]
    static internal bool IsSuspiciousPhysicalPath(string physicalPath, out bool pathTooLong) {
        bool isSuspicious;

        // DevDiv 340712: GetConfigPathData generates n^2 exceptions where n is number of incorrectly placed '/'
        // Explicitly prevent frequent exception cases since this method is called a few times per url segment
        if ((physicalPath != null) &&
             (physicalPath.Length > _maxPathLength ||
             physicalPath.IndexOfAny(s_invalidPathChars) != -1 ||
             // Contains ':' at any position other than 2nd char
             (physicalPath.Length > 0 && physicalPath[0] == ':') ||
             (physicalPath.Length > 2 && physicalPath.IndexOf(':', 2) > 0))) {

            // see comment below
            pathTooLong = true;
            return true;
        }

        try {
            isSuspicious = !String.IsNullOrEmpty(physicalPath) &&
                String.Compare(physicalPath, Path.GetFullPath(physicalPath),
                    StringComparison.OrdinalIgnoreCase) != 0;
            pathTooLong = false;
        }
        catch (PathTooLongException) {
            isSuspicious = true;
            pathTooLong = true;
        }
        catch (NotSupportedException) {
            // see comment below -- we do the same for ':'
            isSuspicious = true;
            pathTooLong = true;
        }
        catch (ArgumentException) {
            // DevDiv Bugs 152256:  Illegal characters {",|} in path prevent configuration system from working.
            // We need to catch this exception and conservatively assume that the path is suspicious in 
            // such a case.
            // We also set pathTooLong to true because at this point we do not know if the path is too long
            // or not. If we assume that pathTooLong is false, it means that our path length enforcement
            // is bypassed by using URLs with illegal characters. We do not want that. Moreover, returning 
            // pathTooLong = true causes the current logic to peel of URL fragments, which can also find a 
            // path without illegal characters to retrieve the config.
            isSuspicious = true;
            pathTooLong = true;
        }

        return isSuspicious;
    }

    static bool HasInvalidLastChar(string physicalPath) {
        // see VSWhidbey #108945
        // We need to filter out directory names which end
        // in " " or ".".  We want to treat path names that 
        // end in these characters as files - however, Windows
        // will strip these characters off the end of the name,
        // which may result in the name being treated as a 
        // directory instead.

        if (String.IsNullOrEmpty(physicalPath)) {
            return false;
        }
        
        char lastChar = physicalPath[physicalPath.Length - 1];
        return lastChar == ' ' || lastChar == '.';
    }

    internal static bool DirectoryExists(String dirname) {
        bool exists = false;
        dirname = RemoveTrailingDirectoryBackSlash(dirname);
        if (HasInvalidLastChar(dirname))
            return false;

        try {
            exists = Directory.Exists(dirname);
        }
        catch {
        }

        return exists;
    }

    internal static bool DirectoryAccessible(String dirname) {
        bool accessible = false;
        dirname = RemoveTrailingDirectoryBackSlash(dirname);
        if (HasInvalidLastChar(dirname))
            return false;

        try {
            accessible = (new DirectoryInfo(dirname)).Exists;
        }
        catch {
        }

        return accessible;
    }

    private static Char[] _invalidFileNameChars = Path.GetInvalidFileNameChars();
    internal static bool IsValidDirectoryName(String name) {
        if (String.IsNullOrEmpty(name)) {
            return false;
        }

        if (name.IndexOfAny(_invalidFileNameChars, 0) != -1) {
            return false;
        }

        if (name.Equals(".") || name.Equals("..")) {
            return false;
        }

        return true;
    }

    //
    // Given a physical path, determine if it exists, and whether it is a directory or file.
    //
    // If directoryExistsOnError is set, set exists=true and isDirectory=true if we cannot confirm that the path does not exist.
    // If fileExistsOnError is set, set exists=true and isDirectory=false if we cannot confirm that the path does not exist.
    //

    // this code is called by config that doesn't have AspNetHostingPermission
    internal static void PhysicalPathStatus(string physicalPath, bool directoryExistsOnError, bool fileExistsOnError, out bool exists, out bool isDirectory) {
        exists = false;
        isDirectory = true;

        Debug.Assert(!(directoryExistsOnError && fileExistsOnError), "!(directoryExistsOnError && fileExistsOnError)");

        if (String.IsNullOrEmpty(physicalPath))
            return;

        using (new ApplicationImpersonationContext()) {
            UnsafeNativeMethods.WIN32_FILE_ATTRIBUTE_DATA data;
            bool ok = UnsafeNativeMethods.GetFileAttributesEx(physicalPath, UnsafeNativeMethods.GetFileExInfoStandard, out data);
            if (ok) {
                exists = true;
                isDirectory = ((data.fileAttributes & (int) FileAttributes.Directory) == (int) FileAttributes.Directory);
                if (isDirectory && HasInvalidLastChar(physicalPath)) {
                    exists = false;
                }
            }
            else {
                if (directoryExistsOnError || fileExistsOnError) {
                    // Set exists to true if we cannot confirm that the path does NOT exist.
                    int hr = Marshal.GetHRForLastWin32Error();
                    if (!(hr == HResults.E_FILENOTFOUND || hr == HResults.E_PATHNOTFOUND)) {
                        exists = true;
                        isDirectory = directoryExistsOnError;
                    }
                }
            }
        }
    }

    //
    // Use to avoid the perf hit of a Demand when the Demand is not necessary for security.
    //
    // If trueOnError is set, then return true if we cannot confirm that the file does NOT exist.
    //
    internal static bool DirectoryExists(string filename, bool trueOnError) {
        filename = RemoveTrailingDirectoryBackSlash(filename);
        if (HasInvalidLastChar(filename)) {
            return false;
        }

        UnsafeNativeMethods.WIN32_FILE_ATTRIBUTE_DATA data;
        bool ok = UnsafeNativeMethods.GetFileAttributesEx(filename, UnsafeNativeMethods.GetFileExInfoStandard, out data);
        if (ok) {
            // The path exists. Return true if it is a directory, false if a file.
            return (data.fileAttributes & (int) FileAttributes.Directory) == (int) FileAttributes.Directory;
        }
        else {
            if (!trueOnError) {
                return false;
            }
            else {
                // Return true if we cannot confirm that the file does NOT exist.
                int hr = Marshal.GetHRForLastWin32Error();
                if (hr == HResults.E_FILENOTFOUND || hr == HResults.E_PATHNOTFOUND) {
                    return false;
                }
                else {
                    return true;
                }
            }
        }
    }
}


//
// Wraps the Win32 API FindFirstFile
//
sealed class FindFileData {

    private FileAttributesData _fileAttributesData;
    private string _fileNameLong;
    private string _fileNameShort;

    internal string FileNameLong { get { return _fileNameLong; } }
    internal string FileNameShort { get { return _fileNameShort; } }
    internal FileAttributesData FileAttributesData { get { return _fileAttributesData; } }

    // FindFile - given a file name, gets the file attributes and short form (8.3 format) of a file name.
    static internal int FindFile(string path, out FindFileData data) {
        IntPtr hFindFile;
        UnsafeNativeMethods.WIN32_FIND_DATA wfd;

        data = null;

        // Remove trailing slash if any, otherwise FindFirstFile won't work correctly
        path = FileUtil.RemoveTrailingDirectoryBackSlash(path);
#if DBG
        Debug.Assert(Path.GetDirectoryName(path) != null, "Path.GetDirectoryName(path) != null");
        Debug.Assert(Path.GetFileName(path) != null, "Path.GetFileName(path) != null");
#endif

        hFindFile = UnsafeNativeMethods.FindFirstFile(path, out wfd);
        int lastError = Marshal.GetLastWin32Error(); // FXCOP demands that this preceed the == 
        if (hFindFile == UnsafeNativeMethods.INVALID_HANDLE_VALUE) {
            return HttpException.HResultFromLastError(lastError);
        }

        UnsafeNativeMethods.FindClose(hFindFile);

#if DBG
        string file = Path.GetFileName(path);
        file = file.TrimEnd(' ', '.');
        Debug.Assert(StringUtil.EqualsIgnoreCase(file, wfd.cFileName) ||
                     StringUtil.EqualsIgnoreCase(file, wfd.cAlternateFileName),
                     "Path to FindFile is not for a single file: " + path);
#endif

        data = new FindFileData(ref wfd);
        return HResults.S_OK;
    }

    // FindFile - takes a full-path and a root-directory-path, and is used to get the
    // short form (8.3 format) of the relative-path.  A FindFileData structure is returned
    // with FileNameLong and FileNameShort relative to the specified root-directory-path.
    //
    // For example, if full-path is "c:\vdir\subdirectory\t.aspx" and root-directory-path 
    // is "c:\vdir", then the relative-path will be "subdirectory\t.aspx" and it's short 
    // form will be something like "subdir~1\t~1.ASP".
    //
    // This is used by FileChangesMonitor to support the ability to monitor all files and 
    // directories at any depth beneath the application root directory. 
    internal static int FindFile(string fullPath, string rootDirectoryPath, out FindFileData data) {

        int hr = FindFileData.FindFile(fullPath, out data);
        if (hr != HResults.S_OK || String.IsNullOrEmpty(rootDirectoryPath)) {
            return hr;
        }
        
#if DBG
        // The trailing slash should have been removed already, unless the root is "c:\"
        Debug.Assert(rootDirectoryPath.Length < 4 || rootDirectoryPath[rootDirectoryPath.Length-1] != '\\', "Trailing slash unexpected: " + rootDirectoryPath);
#endif
        
        // remove it just in case
        rootDirectoryPath = FileUtil.RemoveTrailingDirectoryBackSlash(rootDirectoryPath);
       
#if DBG 
        Debug.Assert(fullPath.IndexOf(rootDirectoryPath, StringComparison.OrdinalIgnoreCase) == 0, 
                     "fullPath (" + fullPath + ") is not within rootDirectoryPath (" + rootDirectoryPath + ")");
#endif
        
        // crawl backwards along the subdirectories of fullPath until we get to the specified rootDirectoryPath
        string relativePathLong = String.Empty;
        string relativePathShort = String.Empty;
        string currentParentDir = Path.GetDirectoryName(fullPath);
        while (currentParentDir != null 
               && currentParentDir.Length > rootDirectoryPath.Length+1 
               && currentParentDir.IndexOf(rootDirectoryPath, StringComparison.OrdinalIgnoreCase) == 0) {
            
            UnsafeNativeMethods.WIN32_FIND_DATA fd;
            IntPtr hFindFile = UnsafeNativeMethods.FindFirstFile(currentParentDir, out fd);
            int lastError = Marshal.GetLastWin32Error(); // FXCOP demands that this preceed the == 
            if (hFindFile == UnsafeNativeMethods.INVALID_HANDLE_VALUE) {
                return HttpException.HResultFromLastError(lastError);
            }
            UnsafeNativeMethods.FindClose(hFindFile);
            
#if DBG
            Debug.Assert(!String.IsNullOrEmpty(fd.cFileName), "!String.IsNullOrEmpty(fd.cFileName)");
#endif

            // build the long and short versions of the relative path
            relativePathLong = fd.cFileName + Path.DirectorySeparatorChar + relativePathLong;
            if (!String.IsNullOrEmpty(fd.cAlternateFileName)) {
                relativePathShort = fd.cAlternateFileName + Path.DirectorySeparatorChar + relativePathShort;
            }
            else {
                relativePathShort = fd.cFileName + Path.DirectorySeparatorChar + relativePathShort;
            }

            currentParentDir = Path.GetDirectoryName(currentParentDir);
        }

        if (!String.IsNullOrEmpty(relativePathLong)) {
            data.PrependRelativePath(relativePathLong, relativePathShort);
        }

#if DBG
        Debug.Trace("FindFile", "fullPath=" + fullPath + ", rootDirectoryPath=" + rootDirectoryPath);
        Debug.Trace("FindFile", "relativePathLong=" + relativePathLong + ", relativePathShort=" + relativePathShort);
        string fileNameShort = data.FileNameShort == null ? "<null>" : data.FileNameShort;
        Debug.Trace("FindFile", "FileNameLong=" + data.FileNameLong + ", FileNameShrot=" + fileNameShort);
#endif
        
        return hr;
    }

    internal FindFileData(ref UnsafeNativeMethods.WIN32_FIND_DATA wfd) {
        _fileAttributesData = new FileAttributesData(ref wfd);
        _fileNameLong = wfd.cFileName;
        if (wfd.cAlternateFileName != null
            && wfd.cAlternateFileName.Length > 0
            && !StringUtil.EqualsIgnoreCase(wfd.cFileName, wfd.cAlternateFileName)) {
            _fileNameShort = wfd.cAlternateFileName;
        }
    }
    
    private void PrependRelativePath(string relativePathLong, string relativePathShort) {
        _fileNameLong = relativePathLong + _fileNameLong;

        // if the short form is null or empty, prepend the short relative path to the long form
        string fileName = String.IsNullOrEmpty(_fileNameShort) ? _fileNameLong : _fileNameShort;
        _fileNameShort = relativePathShort + fileName;

        // if the short form is the same as the long form, set the short form to null
        if (StringUtil.EqualsIgnoreCase(_fileNameShort, _fileNameLong)) {
            _fileNameShort = null;
        }
    }
}

//
// Wraps the Win32 API GetFileAttributesEx
// We use this api in addition to FindFirstFile because FindFirstFile
// does not work for volumes (e.g. "c:\")
//
sealed class FileAttributesData {
    internal readonly FileAttributes    FileAttributes;
    internal readonly DateTime          UtcCreationTime;
    internal readonly DateTime          UtcLastAccessTime;
    internal readonly DateTime          UtcLastWriteTime;
    internal readonly long              FileSize;

    static internal FileAttributesData NonExistantAttributesData {
        get {
            return new FileAttributesData();
        }
    }

    static internal int GetFileAttributes(string path, out FileAttributesData fad) {
        fad = null;

        UnsafeNativeMethods.WIN32_FILE_ATTRIBUTE_DATA  data;
        if (!UnsafeNativeMethods.GetFileAttributesEx(path, UnsafeNativeMethods.GetFileExInfoStandard, out data)) {
            return HttpException.HResultFromLastError(Marshal.GetLastWin32Error());
        }

        fad = new FileAttributesData(ref data);
        return HResults.S_OK;
    }

    FileAttributesData() {
        FileSize = -1;
    }

    FileAttributesData(ref UnsafeNativeMethods.WIN32_FILE_ATTRIBUTE_DATA data) {
        FileAttributes    = (FileAttributes) data.fileAttributes;
        UtcCreationTime   = DateTimeUtil.FromFileTimeToUtc(((long)data.ftCreationTimeHigh)   << 32 | (long)data.ftCreationTimeLow);
        UtcLastAccessTime = DateTimeUtil.FromFileTimeToUtc(((long)data.ftLastAccessTimeHigh) << 32 | (long)data.ftLastAccessTimeLow);
        UtcLastWriteTime  = DateTimeUtil.FromFileTimeToUtc(((long)data.ftLastWriteTimeHigh)  << 32 | (long)data.ftLastWriteTimeLow);
        FileSize          = (long)(uint)data.fileSizeHigh << 32 | (long)(uint)data.fileSizeLow;
    }

    internal FileAttributesData(ref UnsafeNativeMethods.WIN32_FIND_DATA wfd) {
        FileAttributes    = (FileAttributes) wfd.dwFileAttributes;
        UtcCreationTime   = DateTimeUtil.FromFileTimeToUtc(((long)wfd.ftCreationTime_dwHighDateTime)   << 32 | (long)wfd.ftCreationTime_dwLowDateTime);
        UtcLastAccessTime = DateTimeUtil.FromFileTimeToUtc(((long)wfd.ftLastAccessTime_dwHighDateTime) << 32 | (long)wfd.ftLastAccessTime_dwLowDateTime);
        UtcLastWriteTime  = DateTimeUtil.FromFileTimeToUtc(((long)wfd.ftLastWriteTime_dwHighDateTime)  << 32 | (long)wfd.ftLastWriteTime_dwLowDateTime);
        FileSize          = (long)wfd.nFileSizeHigh << 32 | (long)wfd.nFileSizeLow;
    }

#if DBG
    internal string DebugDescription(string indent) {
        StringBuilder   sb = new StringBuilder(200);
        string          i2 = indent + "    ";

        sb.Append(indent + "FileAttributesData\n");
        sb.Append(i2 + "FileAttributes: " + FileAttributes + "\n");
        sb.Append(i2 + "  CreationTime: " + Debug.FormatUtcDate(UtcCreationTime) + "\n");
        sb.Append(i2 + "LastAccessTime: " + Debug.FormatUtcDate(UtcLastAccessTime) + "\n");
        sb.Append(i2 + " LastWriteTime: " + Debug.FormatUtcDate(UtcLastWriteTime) + "\n");
        sb.Append(i2 + "      FileSize: " + FileSize.ToString("n0", NumberFormatInfo.InvariantInfo) + "\n");

        return sb.ToString();
    }
#endif

}

}
