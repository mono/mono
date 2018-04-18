//------------------------------------------------------------------------------
// <copyright file="MapPathBasedVirtualPathProvider.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 * Implementation of VirtualPathProvider based on the metabase and the standard
 * file system.  This is what ASP.NET uses by default.
 */

namespace System.Web.Hosting {

using System;
using System.IO;
using System.Collections;
using System.Collections.Specialized;
using System.Globalization;
using System.Configuration;
using System.Web.Util;
using System.Web.Configuration;
using System.Web.Caching;
using System.Web.Compilation;
using Util=System.Web.UI.Util;
using System.Security.Permissions;
using System.Web.Security;

internal class MapPathBasedVirtualPathProvider: VirtualPathProvider {

    public override string GetFileHash(string virtualPath, IEnumerable virtualPathDependencies) {

        HashCodeCombiner hashCodeCombiner = new HashCodeCombiner();

        // Calculate the hash based on the time stamps of all the virtual paths
        foreach (string virtualDependency in virtualPathDependencies) {
            string physicalDependency = HostingEnvironment.MapPathInternal(virtualDependency);
            hashCodeCombiner.AddFile(physicalDependency);
        }

        return hashCodeCombiner.CombinedHashString;
    }

    public override CacheDependency GetCacheDependency(string virtualPath, IEnumerable virtualPathDependencies, DateTime utcStart) {

        if (virtualPathDependencies == null)
            return null;

        StringCollection physicalDependencies = null;

        // Get the list of physical dependencies
        foreach (string virtualDependency in virtualPathDependencies) {
            string physicalDependency = HostingEnvironment.MapPathInternal(virtualDependency);

            if (physicalDependencies == null)
                physicalDependencies = new StringCollection();

            physicalDependencies.Add(physicalDependency);
        }

        if (physicalDependencies == null)
            return null;

        // Copy the list of physical dependencies into an array
        string[] physicalDependenciesArray = new string[physicalDependencies.Count];
        physicalDependencies.CopyTo(physicalDependenciesArray, 0);

        return new CacheDependency(0, physicalDependenciesArray, utcStart);
    }

    private string CreateCacheKey(bool isFile, string physicalPath) {
        // Need different prefixes for file/directory lookups
        if (isFile)
            return CacheInternal.PrefixMapPathVPPFile + physicalPath;
        else
            return CacheInternal.PrefixMapPathVPPDir + physicalPath;
    }

    private bool CacheLookupOrInsert(string virtualPath, bool isFile) {
        string physicalPath = HostingEnvironment.MapPathInternal(virtualPath);
        bool doNotCache = CachedPathData.DoNotCacheUrlMetadata;
        string cacheKey = null;

        if (!doNotCache) {
            cacheKey = CreateCacheKey(isFile, physicalPath);
            // tri-state: 
            //       * null means it's not cached
            //       * true means it's cached and it exists
            //       * false means it's cached and it doesn't exist
            bool? cacheValue = HttpRuntime.Cache.InternalCache.Get(cacheKey) as bool?;
            if (cacheValue != null) {
                return cacheValue.Value;
            }
        }

        bool exists = (isFile) ? File.Exists(physicalPath) : Directory.Exists(physicalPath);
        
        if (doNotCache) {
            return exists;
        }

        // Setup a cache entry for this so we don't hit the file system every time        
        CacheDependency dep = null;
        // Code based on similar logic from FileAuthorizationModule.
        // If file does not exist, but it's path is beneath the app root, we will cache it and
        // use the first existing directory as the cache depenedency path.  If it does not exist
        // and it's not beneath the app root, we cannot cache it.
        string existingDir = (exists) ? physicalPath : FileUtil.GetFirstExistingDirectory(AppRoot, physicalPath);
        if (existingDir != null) {
            dep = new CacheDependency(existingDir);
            TimeSpan slidingExp = CachedPathData.UrlMetadataSlidingExpiration;
            HttpRuntime.Cache.InternalCache.Insert(cacheKey, exists, new CacheInsertOptions() { Dependencies = dep, SlidingExpiration = slidingExp });
        }

        return exists;
    }

    private static string _AppRoot;
    private static string AppRoot {
        get {
            string appRoot = _AppRoot;
            if (appRoot == null) {
                InternalSecurityPermissions.AppPathDiscovery.Assert();
                appRoot = Path.GetFullPath(HttpRuntime.AppDomainAppPathInternal);
                appRoot = FileUtil.RemoveTrailingDirectoryBackSlash(appRoot);
                _AppRoot = appRoot;
            }
            return appRoot;
        }
    }

    public override bool FileExists(string virtualPath) {
        return CacheLookupOrInsert(virtualPath, true);
    }

    public override bool DirectoryExists(string virtualDir) {
        return CacheLookupOrInsert(virtualDir, false);
    }

    public override VirtualFile GetFile(string virtualPath) {
        return new MapPathBasedVirtualFile(virtualPath);
    }

    public override VirtualDirectory GetDirectory(string virtualDir) {
        return new MapPathBasedVirtualDirectory(virtualDir);
    }
}

internal class MapPathBasedVirtualFile: VirtualFile {

    private string _physicalPath;
    private FindFileData _ffd;

    internal MapPathBasedVirtualFile(string virtualPath) : base(virtualPath) { }

    internal MapPathBasedVirtualFile(string virtualPath, string physicalPath,
        FindFileData ffd) : base(virtualPath) {

        _physicalPath = physicalPath;
        _ffd = ffd;
    }

    private void EnsureFileInfoObtained() {

        // Get the physical path and FindFileData on demand
        if (_physicalPath == null) {
            Debug.Assert(_ffd == null);
            _physicalPath = HostingEnvironment.MapPathInternal(VirtualPath);
            FindFileData.FindFile(_physicalPath, out _ffd);
        }
    }

    public override string Name {
        get {
            EnsureFileInfoObtained();

            // If for whatever reason we couldn't get the FindFileData, just call the base (VSWhidbey 501294)
            if (_ffd == null)
                return base.Name;

            return _ffd.FileNameLong;
        }
    }

    public override Stream Open() {
        EnsureFileInfoObtained();

        TimeStampChecker.AddFile(VirtualPath, _physicalPath);

        return new FileStream(_physicalPath, FileMode.Open, FileAccess.Read, FileShare.Read);
    }

    internal string PhysicalPath {
        get {
            EnsureFileInfoObtained();
            return _physicalPath;
        }
    }
}

internal class MapPathBasedVirtualDirectory: VirtualDirectory {

    public MapPathBasedVirtualDirectory(string virtualPath) : base(virtualPath) { }

    public override IEnumerable Directories {
        get {
            return new MapPathBasedVirtualPathCollection(
                System.Web.VirtualPath.CreateNonRelative(VirtualPath), RequestedEntryType.Directories);
        }
    }

    public override IEnumerable Files {
        get {
            return new MapPathBasedVirtualPathCollection(
                System.Web.VirtualPath.CreateNonRelative(VirtualPath), RequestedEntryType.Files);
        }
    }

    public override IEnumerable Children {
        get {
            return new MapPathBasedVirtualPathCollection(
                System.Web.VirtualPath.CreateNonRelative(VirtualPath), RequestedEntryType.All);
        }
    }
}

internal enum RequestedEntryType {
    Files,
    Directories,
    All
}

internal class MapPathBasedVirtualPathCollection: MarshalByRefObject, IEnumerable {

    private VirtualPath _virtualPath;
    private RequestedEntryType _requestedEntryType;

    internal MapPathBasedVirtualPathCollection(VirtualPath virtualPath, RequestedEntryType requestedEntryType) {
        _virtualPath = virtualPath;
        _requestedEntryType = requestedEntryType;
    }

    public override Object InitializeLifetimeService(){
        return null; // never expire lease
    }

    IEnumerator IEnumerable.GetEnumerator() {
        return new MapPathBasedVirtualPathEnumerator(_virtualPath, _requestedEntryType);
    }
}

internal class MapPathBasedVirtualPathEnumerator : MarshalByRefObject, IEnumerator, IDisposable {
    VirtualPath              _virtualPath;           // virtual path we are enumerating
    Hashtable                _exclude;               // names of files and dirs to exclude
    Hashtable                _virtualPaths;          // names of virtual directories to include
    IEnumerator              _fileEnumerator;        // the physical file enumerator
    IEnumerator              _virtualEnumerator;     // the virtual file enumerator
    bool                     _useFileEnumerator;     // use the file enumerator
    RequestedEntryType       _requestedEntryType;
    IServerConfig2           _serverConfig2;

    internal MapPathBasedVirtualPathEnumerator(VirtualPath virtualPath, RequestedEntryType requestedEntryType) {

        if (virtualPath.IsRelative) {
            throw new ArgumentException(SR.GetString(SR.Invalid_app_VirtualPath), "virtualPath");
        }

        _virtualPath = virtualPath;
        _requestedEntryType = requestedEntryType;

        string physicalPath;
        if (!ServerConfig.UseServerConfig) {
            // Use the hosting environment to map the virtual path
            physicalPath = _virtualPath.MapPathInternal();
        }
        else {
            IServerConfig serverConfig = ServerConfig.GetInstance();
            _serverConfig2 = serverConfig as IServerConfig2;
            
            // Use serverConfig to map the virtual path
            physicalPath = serverConfig.MapPath(null, _virtualPath);

            if (_requestedEntryType != RequestedEntryType.Files) {
                // For MetabaseServerConfig, get the subdirs that are not in the application, and add them to the exclude list.
                if (_serverConfig2 == null) {
                    string [] virtualSubdirsNotInApp = serverConfig.GetVirtualSubdirs(_virtualPath, false);
                    if (virtualSubdirsNotInApp != null) {
                        _exclude = new Hashtable(StringComparer.OrdinalIgnoreCase);
                        foreach (string subdir in virtualSubdirsNotInApp) {
                            _exclude[subdir] = subdir;
                        }
                    }
                }

                // Get subdirs that are virtual directories, and record their physical mappings.
                // Ignore the virtualPaths if we only need files, since it only contains directories
                string [] virtualSubdirsInApp = serverConfig.GetVirtualSubdirs(_virtualPath, true);
                if (virtualSubdirsInApp != null) {
                    _virtualPaths = new Hashtable(StringComparer.OrdinalIgnoreCase);
                    foreach (string subdir in virtualSubdirsInApp) {
                        VirtualPath subpath = _virtualPath.SimpleCombineWithDir(subdir);
                        string subPhysicalPath = serverConfig.MapPath(null, subpath);
                        if (FileUtil.DirectoryExists(subPhysicalPath)) {
                            _virtualPaths[subdir] = new MapPathBasedVirtualDirectory(subpath.VirtualPathString);
                        }
                    }

                    // Create enumerator for the virtual paths
                    _virtualEnumerator = _virtualPaths.Values.GetEnumerator();
                }
            }
        }

        // Create an enumerator for the physical files and directories at this path
        _fileEnumerator = FileEnumerator.Create(physicalPath);

        // Reset the enumerator. Note that we don't support the Reset method.
        _useFileEnumerator = false;
    }

    public override Object InitializeLifetimeService(){
        return null; // never expire lease
    }
            
    // Dispose the file enumerator
    void IDisposable.Dispose() {
        if (_fileEnumerator != null) {
            ((IDisposable)_fileEnumerator).Dispose();
            _fileEnumerator = null;
        }
    }

    // First MoveNext() with the file enumerator, then with the virtual directories
    // that have not been enumerated.
    bool IEnumerator.MoveNext() {
        bool more = false;

        if (_virtualEnumerator != null)
            more = _virtualEnumerator.MoveNext();

        if (!more) {
            _useFileEnumerator = true;
            for (;;) {
                more = _fileEnumerator.MoveNext();
                if (!more)
                    break;

                FileData fileData = (FileData) _fileEnumerator.Current;

                // Ignore all hidden files and directories
                if (fileData.IsHidden)
                    continue;

                // Ignore it if it's not of the right type (i.e. directory vs file)
                if (fileData.IsDirectory) {
                    if (_requestedEntryType == RequestedEntryType.Files)
                        continue;

                    // Check whether the file is the same as a virtual path
                    // that we have already enumerated
                    string name = fileData.Name;
                    if (_virtualPaths != null && _virtualPaths.Contains(name))
                        continue;
                    
                    // Check whether the file should be excluded because it is
                    // not part of this app.
                    
                    // MetabaseServerConfig
                    if (_exclude != null && _exclude.Contains(name))
                        continue;
                    
                    // IServerConfig2
                    if (_serverConfig2 != null && !_serverConfig2.IsWithinApp(UrlPath.SimpleCombine(_virtualPath.VirtualPathString, name))) {
                        continue;
                    }
                }
                else {
                    if (_requestedEntryType == RequestedEntryType.Directories)
                        continue;
                }

                // We've found the file
                break;
            }
        }

        return more;
    }

    internal VirtualFileBase Current {
        get {
            if (_useFileEnumerator) {
                FileData fileData = (FileData) _fileEnumerator.Current;
                VirtualPath childVirtualPath;
                if (fileData.IsDirectory) {
                    childVirtualPath = _virtualPath.SimpleCombineWithDir(fileData.Name);
                    return new MapPathBasedVirtualDirectory(childVirtualPath.VirtualPathString);
                }
                else {
                    childVirtualPath = _virtualPath.SimpleCombine(fileData.Name);
                    FindFileData ffd = fileData.GetFindFileData();
                    return new MapPathBasedVirtualFile(childVirtualPath.VirtualPathString, fileData.FullName, ffd);
                }
            }
            else {
                return (VirtualFileBase) _virtualEnumerator.Current;
            }
        }
    }

    object IEnumerator.Current {
        get { return Current; }
    }

    void IEnumerator.Reset() {
        // We don't support reset, though it would be easy to add if needed
        throw new InvalidOperationException();
    }
}

#if NO
// TEST CODE

public class TestEnum {

    public static void Enum(HttpResponse response) {
        VirtualDirectory vdir = HostingEnvironment.VirtualPathProvider.GetDirectory(
            "~");
        EnumRecursive(response, vdir);
    }

    static void EnumRecursive(HttpResponse response, VirtualDirectory vdir) {
        foreach (VirtualFile vfile in vdir.Files) {
            response.Write("File: " + vfile.VirtualPath + "<br>\r\n");
        }

        foreach (VirtualDirectory childVdir in vdir.Directories) {
            response.Write("Directory: " + childVdir.VirtualPath + "<br>\r\n");
            EnumRecursive(response, childVdir);
        }
    }
}
#endif

}
