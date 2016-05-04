//------------------------------------------------------------------------------
// <copyright file="VirtualPathProvider.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Hosting {

using System;
using System.IO;
using System.Collections;
using System.Globalization;
using System.Configuration;
using System.Web.Caching;
using System.Web.Util;
using Util=System.Web.UI.Util;
using System.Security.Permissions;

/*
 * Base class for virtual path providers
 */

public abstract class VirtualPathProvider: MarshalByRefObject {

    private VirtualPathProvider _previous;


    public override Object InitializeLifetimeService(){
        return null; // never expire lease
    }

    internal virtual void Initialize(VirtualPathProvider previous) {
        _previous = previous;
        Initialize();
    }

    /*
     * Initialize is called on the provider after it is registered.
     */

    protected virtual void Initialize() {
    }

    /*
     * Gives the provider access to the Previous provider.  It can be used to delegate some of the calls 
     * (e.g. as a way of having some files comes from the file system, and others from the database)
     */

    protected internal VirtualPathProvider Previous { get { return _previous; } }

    /*
     * Asks the provider for a hash string based on the state of a set of virtual paths.
     * The primary virtualPath is also passed in by itself.
     * If they match, the cached data held by the user of the provider is still
     * valid.  Otherwise, it should be discarded, and a new version needs to be
     * obtained via GetFile/GetDirectory.
     */

    public virtual string GetFileHash(string virtualPath, IEnumerable virtualPathDependencies) {

        // Delegate to the previous VirtualPathProvider, if any

        if (_previous == null)
            return null;

        return _previous.GetFileHash(virtualPath, virtualPathDependencies);
    }

    internal string GetFileHash(VirtualPath virtualPath, IEnumerable virtualPathDependencies) {
        return GetFileHash(virtualPath.VirtualPathString, virtualPathDependencies);
    }

    /*
     * Asks the provider for a CacheDependency that will be invalidated if any of the
     * input files become invalid.
     * utcStart contains the time (UTC) at which the files were read.  Any change to the file
     * made after that time (even if the change is in the past) should invalidate the
     * CacheDependency.
     * If the provider doesn't support using a CacheDependency, it should return null,
     * or simply not override GetCacheDependency (the base implementation returns null).
     */

    public virtual CacheDependency GetCacheDependency(string virtualPath, IEnumerable virtualPathDependencies, DateTime utcStart) {
        // Delegate to the previous VirtualPathProvider, if any
        if (_previous == null) {
            return null;
        }

        return _previous.GetCacheDependency(virtualPath, virtualPathDependencies, utcStart);
    }

    internal CacheDependency GetCacheDependency(VirtualPath virtualPath, IEnumerable virtualPathDependencies, DateTime utcStart) {
        return GetCacheDependency(virtualPath.VirtualPathString, virtualPathDependencies, utcStart);
    }

    /*
     * Returns whether the file described by the virtual path exists from
     * the point of view of this provider.
     */

    public virtual bool FileExists(string virtualPath) {

        // Delegate to the previous VirtualPathProvider, if any

        if (_previous == null)
            return false;

        return _previous.FileExists(virtualPath);
    }

    internal bool FileExists(VirtualPath virtualPath) {
        return FileExists(virtualPath.VirtualPathString);
    }

    /*
     * Returns whether the directory described by the virtual path exists from
     * the point of view of this provider.
     */

    public virtual bool DirectoryExists(string virtualDir) {

        // Delegate to the previous VirtualPathProvider, if any

        if (_previous == null)
            return false;

        return _previous.DirectoryExists(virtualDir);
    }

    internal bool DirectoryExists(VirtualPath virtualDir) {
        return DirectoryExists(virtualDir.VirtualPathString);
    }

    /*
     * Returns a VirtualFile object for the passed in virtual path
     */

    public virtual VirtualFile GetFile(string virtualPath) {

        // Delegate to the previous VirtualPathProvider, if any

        if (_previous == null)
            return null;

        return _previous.GetFile(virtualPath);
    }

    internal VirtualFile GetFile(VirtualPath virtualPath) {
        return GetFileWithCheck(virtualPath.VirtualPathString);
    }

    internal VirtualFile GetFileWithCheck(string virtualPath) {

        VirtualFile virtualFile = GetFile(virtualPath);

        if (virtualFile == null)
            return null;

        // Make sure the VirtualFile's path is the same as what was passed to GetFile
        if (!StringUtil.EqualsIgnoreCase(virtualPath, virtualFile.VirtualPath)) {
            throw new HttpException(SR.GetString(SR.Bad_VirtualPath_in_VirtualFileBase,
                "VirtualFile", virtualFile.VirtualPath, virtualPath));
        }

        return virtualFile;
    }


    /*
     * Returns a VirtualDirectory object for the passed in virtual path
     */

    public virtual VirtualDirectory GetDirectory(string virtualDir) {

        // Delegate to the previous VirtualPathProvider, if any

        if (_previous == null)
            return null;

        return _previous.GetDirectory(virtualDir);
    }

    internal VirtualDirectory GetDirectory(VirtualPath virtualDir) {
        Debug.Assert(virtualDir.HasTrailingSlash);
        return GetDirectoryWithCheck(virtualDir.VirtualPathString);
    }

    internal VirtualDirectory GetDirectoryWithCheck(string virtualPath) {

        VirtualDirectory virtualDir = GetDirectory(virtualPath);

        if (virtualDir == null)
            return null;

        // Make sure the VirtualDirectory's path is the same as what was passed to GetDirectory
        if (!StringUtil.EqualsIgnoreCase(virtualPath, virtualDir.VirtualPath)) {
            throw new HttpException(SR.GetString(SR.Bad_VirtualPath_in_VirtualFileBase,
                "VirtualDirectory", virtualDir.VirtualPath, virtualPath));
        }

        return virtualDir;
    }

#if OLD
    /*
     * Allow the file provider to replace a virtual path by a different (ghosted) one.  This can
     * be used to have several virtual paths be mapped to the same compilation, hence saving
     * resources.  If the path is not ghosted, this method must return null.
     */

    public virtual string GetGhostedVirtualPath(string virtualPath) {

        // By default, it's not supported
        return null;
    }
#endif

    /*
     * Returns a cache key to be used for this virtual path.  If not overridden, this returns
     * null meaning that the virtual path itself should be used as the cache key.
     * This should only be overridden to achieve Sharepoint like ghosting behavior.
     */

    public virtual string GetCacheKey(string virtualPath) {

        // By default, return null, meaning use a key based on the virtual path
        return null;
    }

    internal string GetCacheKey(VirtualPath virtualPath) {
        return GetCacheKey(virtualPath.VirtualPathString);
    }

    /*
     * Allows the VirtualPathProvider to use custom logic to combine virtual path.
     * This can be used to give a special meaning to app relative paths (DevDiv 31438).
     * basePath is the path to the file in which the relative reference was found.
     */
    public virtual string CombineVirtualPaths(string basePath, string relativePath) {

        string baseDir = null;
        if (!String.IsNullOrEmpty(basePath))
            baseDir = UrlPath.GetDirectory(basePath);

        // By default, just combine them normally
        return UrlPath.Combine(baseDir, relativePath);
    }

    internal VirtualPath CombineVirtualPaths(VirtualPath basePath, VirtualPath relativePath) {
        string virtualPath = CombineVirtualPaths(basePath.VirtualPathString,
            relativePath.VirtualPathString);
        return VirtualPath.Create(virtualPath);
    }

    /*
     * Helper method to open a file from its virtual path
     */

    public static Stream OpenFile(string virtualPath) {
        VirtualPathProvider vpathProvider = HostingEnvironment.VirtualPathProvider;
        VirtualFile vfile = vpathProvider.GetFileWithCheck(virtualPath);
        return vfile.Open();
    }

    internal static Stream OpenFile(VirtualPath virtualPath) {
        return OpenFile(virtualPath.VirtualPathString);
    }

    internal static CacheDependency GetCacheDependency(VirtualPath virtualPath) {
        VirtualPathProvider vpathProvider = HostingEnvironment.VirtualPathProvider;
        return vpathProvider.GetCacheDependency(virtualPath,
            new SingleObjectCollection(virtualPath.VirtualPathString), DateTime.MaxValue);
    }

    /*
     * Helper method to call CombineVirtualPaths if there is a VirtualPathProvider
     */
    internal static VirtualPath CombineVirtualPathsInternal(VirtualPath basePath, VirtualPath relativePath) {

        VirtualPathProvider vpathProvider = HostingEnvironment.VirtualPathProvider;
        if (vpathProvider != null) {
            return vpathProvider.CombineVirtualPaths(basePath, relativePath);
        }

        // If there is no provider, just combine them normally
        return basePath.Parent.Combine(relativePath);
    }

    internal static bool DirectoryExistsNoThrow(string virtualDir) {
        try {
            return HostingEnvironment.VirtualPathProvider.DirectoryExists(virtualDir);
        }
        catch {
            // If it throws, act is if it doesn't exist
            return false;
        }
    }

    internal static bool DirectoryExistsNoThrow(VirtualPath virtualDir) {
        return DirectoryExistsNoThrow(virtualDir.VirtualPathString);
    }
}

/*
 * Base class for VirtualFile and VirtualDirectory.  This is analogous to
 * System.IO.FileSystemInfo, but for virtual paths instead of physical.
 */

public abstract class VirtualFileBase: MarshalByRefObject {

    internal VirtualPath _virtualPath;

    public override Object InitializeLifetimeService(){
        return null; // never expire lease
    }

    /*
     * Returns the name of the file or directory, without any path info.
     * e.g. if the virtual path is /app/sub/foo.aspx, this returns foo.aspx.
     * Note that this is expected to return the name in the correct casing,
     * which may be different from the casing in the original virtual path.
     */

    public virtual string Name {
        get {
            // By default, return the last chunk of the virtual path
            return _virtualPath.FileName;
        }
    }

    /*
     * Returns the virtual path to the file or directory that this object
     * represents.  This is typically the path passed in to the constructor.
     */

    public string VirtualPath {
        get { return _virtualPath.VirtualPathString; }
    }

    internal VirtualPath VirtualPathObject {
        get { return _virtualPath; }
    }

    /*
     * Returns true if this is a directory, and false if its a file
     */

    public abstract bool IsDirectory {get;}
}

/*
 * Object that represents a virtual file.  This is analogous to
 * System.IO.FileInfo, but for virtual paths instead of physical.
 */

public abstract class VirtualFile: VirtualFileBase {

    /*
     * Contructs a VirtualFile, passing it the virtual path to the
     * file it represents
     */

    protected VirtualFile(string virtualPath) {
        _virtualPath = System.Web.VirtualPath.Create(virtualPath);
    }


    public override bool IsDirectory {
        get { return false; }
    }

    /*
     * Returns a readonly stream to the file
     */

    public abstract Stream Open();
}

/*
 * Object that represents a virtual directory.  This is analogous to
 * System.IO.DirectoryInfo, but for virtual paths instead of physical.
 */

public abstract class VirtualDirectory: VirtualFileBase {

    /*
     * Contructs a VirtualDirectory, passing it the virtual path to the
     * directory it represents
     */

    protected VirtualDirectory(string virtualPath) {
        // Make sure it always has a trailing slash
        _virtualPath = System.Web.VirtualPath.CreateTrailingSlash(virtualPath);
    }


    public override bool IsDirectory {
        get { return true; }
    }

    /*
     * Returns an object that enumerates all the children VirtualDirectory's
     * of this directory.
     */

    public abstract IEnumerable Directories {get;}

    /*
     * Returns an object that enumerates all the children VirtualFile's
     * of this directory.
     */

    public abstract IEnumerable Files {get;}

    /*
     * Returns an object that enumerates all the children VirtualDirectory's
     * and VirtualFiles of this directory.
     */

    public abstract IEnumerable Children {get;}
}
}
