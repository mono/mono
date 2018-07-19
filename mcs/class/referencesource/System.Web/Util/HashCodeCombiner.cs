//------------------------------------------------------------------------------
// <copyright file="HashCodeCombiner.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 * HashCodeCombiner class
 * 
 * Copyright (c) 1999 Microsoft Corporation
 */

namespace System.Web.Util {
using System.Text;
using System.IO;
using System.Globalization;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Web.Security.Cryptography;

/*
 * Class used to combine several hashcodes into a single hashcode
 */
internal class HashCodeCombiner {

    private long _combinedHash;

    internal HashCodeCombiner() {
       // Start with a seed (obtained from String.GetHashCode implementation)
       _combinedHash = 5381;
    }

    internal HashCodeCombiner(long initialCombinedHash)   {
        _combinedHash = initialCombinedHash;
    }

    internal static int CombineHashCodes(int h1, int h2) {
        return ((h1 << 5) + h1) ^ h2;
    }

    internal static int CombineHashCodes(int h1, int h2, int h3) {
        return CombineHashCodes(CombineHashCodes(h1, h2), h3);
    }

    internal static int CombineHashCodes(int h1, int h2, int h3, int h4) {
        return CombineHashCodes(CombineHashCodes(h1, h2), CombineHashCodes(h3, h4));
    }

    internal static int CombineHashCodes(int h1, int h2, int h3, int h4, int h5) {
        return CombineHashCodes(CombineHashCodes(h1, h2, h3, h4), h5);
    }

    internal static string GetDirectoryHash(VirtualPath virtualDir) {
        HashCodeCombiner hashCodeCombiner = new HashCodeCombiner();
        hashCodeCombiner.AddDirectory(virtualDir.MapPathInternal());
        return hashCodeCombiner.CombinedHashString;
    }

    internal void AddArray(string[] a) {
        if (a != null) {
            int n = a.Length;
            for (int i = 0; i < n; i++) {
                AddObject(a[i]);
            }
        }
    }

    internal void AddInt(int n) {
        _combinedHash = ((_combinedHash << 5) + _combinedHash) ^ n;
        Debug.Trace("HashCodeCombiner", "Adding " + n.ToString("x") + " --> " + _combinedHash.ToString("x"));
    }

    internal void AddObject(int n) {
        AddInt(n);
    }

    internal void AddObject(byte b) {
        AddInt(b.GetHashCode());
    }

    internal void AddObject(long l) {
        AddInt(l.GetHashCode());
    }

    internal void AddObject(bool b) {
        AddInt(b.GetHashCode());
    }

    internal void AddObject(string s) {
        if (s != null)
            AddInt(StringUtil.GetStringHashCode(s));
    }

    internal void AddObject(Type t) {
        if (t != null)
            AddObject(System.Web.UI.Util.GetAssemblyQualifiedTypeName(t));
    }

    internal void AddObject(object o) {
        if (o != null)
            AddInt(o.GetHashCode());
    }

    internal void AddCaseInsensitiveString(string s) {
        if (s != null)
            AddInt(StringUtil.GetNonRandomizedHashCode(s, ignoreCase:true));
    }

    internal void AddDateTime(DateTime dt) {
        Debug.Trace("HashCodeCombiner", "Ticks: " + dt.Ticks.ToString("x", CultureInfo.InvariantCulture));
        Debug.Trace("HashCodeCombiner", "Hashcode: " + dt.GetHashCode().ToString("x", CultureInfo.InvariantCulture));
        AddInt(dt.GetHashCode());
    }

    private void AddFileSize(long fileSize) {
        Debug.Trace("HashCodeCombiner", "file size: " + fileSize.ToString("x", CultureInfo.InvariantCulture));
        Debug.Trace("HashCodeCombiner", "Hashcode: " + fileSize.GetHashCode().ToString("x", CultureInfo.InvariantCulture));
        AddInt(fileSize.GetHashCode());
    }

    [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands", Justification = "This call site is trusted.")]
    private void AddFileVersionInfo(FileVersionInfo fileVersionInfo) {
        Debug.Trace("HashCodeCombiner", "FileMajorPart: " + fileVersionInfo.FileMajorPart.GetHashCode().ToString("x", CultureInfo.InvariantCulture));
        Debug.Trace("HashCodeCombiner", "FileMinorPart: " + fileVersionInfo.FileMinorPart.GetHashCode().ToString("x", CultureInfo.InvariantCulture));
        Debug.Trace("HashCodeCombiner", "FileBuildPart: " + fileVersionInfo.FileBuildPart.GetHashCode().ToString("x", CultureInfo.InvariantCulture));
        Debug.Trace("HashCodeCombiner", "FilePrivatePart: " + fileVersionInfo.FilePrivatePart.GetHashCode().ToString("x", CultureInfo.InvariantCulture));
        AddInt(fileVersionInfo.FileMajorPart.GetHashCode());
        AddInt(fileVersionInfo.FileMinorPart.GetHashCode());
        AddInt(fileVersionInfo.FileBuildPart.GetHashCode());
        AddInt(fileVersionInfo.FilePrivatePart.GetHashCode());
    }

    private void AddFileContentHashKey(string fileContentHashKey) {
        AddInt(StringUtil.GetNonRandomizedHashCode(fileContentHashKey));
    }

    internal void AddFileContentHash(string fileName) {
        // Convert file content to hash bytes
        byte[] fileContentBytes = File.ReadAllBytes(fileName);
        byte[] fileContentHashBytes = CryptoUtil.ComputeSHA256Hash(fileContentBytes);

        // Convert byte[] to hex string representation
        StringBuilder sbFileContentHashBytes = new StringBuilder();
        for (int index = 0; index < fileContentHashBytes.Length; index++) {
            sbFileContentHashBytes.Append(fileContentHashBytes[index].ToString("X2", CultureInfo.InvariantCulture));
        }

        AddFileContentHashKey(sbFileContentHashBytes.ToString());
    }

    internal void AddFile(string fileName) {
        Debug.Trace("HashCodeCombiner", "AddFile: " + fileName);
        if (!FileUtil.FileExists(fileName)) {

            // Review: Should we change the dependency model to take directory into account?
            if (FileUtil.DirectoryExists(fileName)) {
                // Add as a directory dependency if it's a directory.
                AddDirectory(fileName);
                return;
            }

            Debug.Trace("HashCodeCombiner", "Could not find target " + fileName);
            return;
        }

        AddExistingFile(fileName);
    }

    // Same as AddFile, but only called for a file which is known to exist
    private void AddExistingFile(string fileName) {
        Debug.Trace("HashCodeCombiner", "AddExistingFile: " + fileName);

        AddInt(StringUtil.GetStringHashCode(fileName));
        FileInfo file = new FileInfo(fileName);
        if (!AppSettings.PortableCompilationOutput) {
            AddDateTime(file.CreationTimeUtc);
        }
        AddDateTime(file.LastWriteTimeUtc);
        AddFileSize(file.Length);
    }

    [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands", Justification = "This call site is trusted.")]
    internal void AddExistingFileVersion(string fileName) {
        Debug.Trace("HashCodeCombiner", "AddExistingFileVersion: " + fileName);

        AddInt(StringUtil.GetStringHashCode(fileName));
        FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(fileName);

        AddFileVersionInfo(fileVersionInfo);
    }

    internal void AddDirectory(string directoryName) {

        DirectoryInfo directory = new DirectoryInfo(directoryName);
        if (!directory.Exists) {
            return;
        }

        AddObject(directoryName);

        // Go through all the files in the directory
        foreach (FileData fileData in FileEnumerator.Create(directoryName)) {

            if (fileData.IsDirectory)
                AddDirectory(fileData.FullName);
            else
                AddExistingFile(fileData.FullName);
        }

        if (!AppSettings.PortableCompilationOutput) {
            AddDateTime(directory.CreationTimeUtc);
            AddDateTime(directory.LastWriteTimeUtc);
        }
    }

    // Same as AddDirectory, but only look at files that don't have a culture
    internal void AddResourcesDirectory(string directoryName) {

        DirectoryInfo directory = new DirectoryInfo(directoryName);
        if (!directory.Exists) {
            return;
        }

        AddObject(directoryName);

        // Go through all the files in the directory
        foreach (FileData fileData in FileEnumerator.Create(directoryName)) {

            if (fileData.IsDirectory)
                AddResourcesDirectory(fileData.FullName);
            else {
                // Ignore the file if it has a culture, since only neutral files
                // need to re-trigger compilation (VSWhidbey 359029)
                string fullPath = fileData.FullName;
                if (System.Web.UI.Util.GetCultureName(fullPath) == null) {
                    AddExistingFile(fullPath);
                }
            }
        }

        if (!AppSettings.PortableCompilationOutput) {
            AddDateTime(directory.CreationTimeUtc);
        }
    }

    internal long CombinedHash { get { return _combinedHash; } }
    internal int CombinedHash32 { get { return _combinedHash.GetHashCode(); } }

    internal string CombinedHashString {
        get {
            return _combinedHash.ToString("x", CultureInfo.InvariantCulture);
        }
    }
}


}
