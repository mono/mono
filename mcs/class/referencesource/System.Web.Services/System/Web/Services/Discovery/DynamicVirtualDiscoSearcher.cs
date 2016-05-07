//------------------------------------------------------------------------------
// <copyright file="DynamicVirtualDiscoSearcher.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Services.Discovery {
    using System;
    using System.IO;
    using System.Collections;
    using System.Diagnostics;
    using System.Text;
    using System.DirectoryServices;
    using System.ComponentModel;
    using System.Globalization;
    using System.Threading;    
    using System.Web.Services.Diagnostics;

    /// <include file='doc\DynamicVirtualDiscoSearcher.uex' path='docs/doc[@for="DynamicVirtualDiscoSearcher"]/*' />
    /// <devdoc>
    /// Does a recursive search of virtual subdirectories to find stuff to
    /// make a disco file from. *.disco files (or whatever the PrimarySearchPattern is) are
    /// treated as end-points - recursion stops where they are found.
    /// </devdoc>
    internal class DynamicVirtualDiscoSearcher : DynamicDiscoSearcher {

        private string rootPathAsdi; // ADSI search root path with prefix
        private string entryPathPrefix;

        private string startDir;

// If we could get an event back from IIS Admin Object that some directory is addred/removed/renamed
// then the following memeber should become static, so we get 5-10 _times_ performace gain on
// processing .vsdisco in the Web Root
// !!SEE ALSO!! CleanupCache method
        private /*static*/ Hashtable webApps = new Hashtable();
        private Hashtable Adsi = new Hashtable();


        // -------------------------------------------------------------------------------
        internal DynamicVirtualDiscoSearcher(string startDir, string[] excludedUrls, string rootUrl) :
                base(excludedUrls)
        {
            origUrl = rootUrl;
            entryPathPrefix = GetWebServerForUrl( rootUrl ) + "/ROOT";

            this.startDir = startDir;

            string localPath = (new System.Uri(rootUrl)).LocalPath;
            if ( localPath.Equals("/") ) localPath = "";     // empty local path should be ""
            rootPathAsdi = entryPathPrefix + localPath;
        }

        // -------------------------------------------------------------------------------
        /// <include file='doc\DynamicVirtualDiscoSearcher.uex' path='docs/doc[@for="DynamicVirtualDiscoSearcher.Search"]/*' />
        /// <devdoc>
        /// Main function. Searches dir recursively for primary (.vsdisco) and seconary (.asmx) files.
        /// </devdoc>
        internal override void Search(string fileToSkipAtBegin) {
            SearchInit(fileToSkipAtBegin);
            ScanDirectory( rootPathAsdi );
            CleanupCache();
        }

        // -------------------------------------------------------------------------------
        // Look in virtual subdirectories.
        protected override void SearchSubDirectories(string nameAdsiDir) {

            if ( CompModSwitches.DynamicDiscoverySearcher.TraceVerbose ) Debug.WriteLine( "DynamicVirtualDiscoSearcher.SearchSubDirectories(): nameAdsiDir=" + nameAdsiDir);

            DirectoryEntry vdir = (DirectoryEntry)Adsi[nameAdsiDir];    //may be already bound
            if (vdir == null) {
                if ( !DirectoryEntry.Exists(nameAdsiDir) )
                    return;
                vdir = new DirectoryEntry(nameAdsiDir);
                Adsi[nameAdsiDir] = vdir;
            }

            foreach (DirectoryEntry obj in vdir.Children) {
                DirectoryEntry child = (DirectoryEntry)Adsi[obj.Path];
                if (child == null) {
                    child = obj;
                    Adsi[obj.Path] = obj;
                } else {
                    obj.Dispose();
                }
                AppSettings settings = GetAppSettings(child);
                if (settings != null) {
                    ScanDirectory(child.Path);                      //go down ADSI path
                }
            }

        }

        // -------------------------------------------------------------------------------
        protected override DirectoryInfo GetPhysicalDir(string dir ) {
            DirectoryEntry vdir = (DirectoryEntry)Adsi[dir];
            if (vdir == null) {
                if (!DirectoryEntry.Exists(dir) ) {
                    return null;
                }
                vdir = new DirectoryEntry(dir);
                Adsi[dir] = vdir;
            }
            try {
                DirectoryInfo directory = null;
                AppSettings settings = GetAppSettings(vdir);
                if (settings == null) {
                    return null;
                }
                if (settings.VPath == null) {                   //SchemaClassName == "IIsWebDirectory"
                    //NOTE This assumes there was a known physical directory
                    //corresponding to a parent WebDirectory.
                    //And incoming 'dir' is a child of that parent.
                    if ( !dir.StartsWith(rootPathAsdi, StringComparison.Ordinal) ) {
                        throw new ArgumentException(Res.GetString(Res.WebVirtualDisoRoot, dir, rootPathAsdi), "dir");
                    }
                    string physicalDir = dir.Substring(rootPathAsdi.Length);
                    physicalDir = physicalDir.Replace('/', '\\'); //it always begins with '/' or is empty
                    directory = new DirectoryInfo(startDir + physicalDir);

                }
                else {
                    directory = new DirectoryInfo(settings.VPath); //SchemaClassName == "IIsWebVirtualDir
                }

                if ( directory.Exists )
                    return directory;
            }
            catch (Exception e) {
                if (e is ThreadAbortException || e is StackOverflowException || e is OutOfMemoryException) {
                    throw;
                }
                if ( CompModSwitches.DynamicDiscoverySearcher.TraceVerbose ) Debug.WriteLine( "+++ DynamicVirtualDiscoSearcher.GetPhysicalDir(): dir=" + dir + " Exception=" + e.ToString() );
                if (Tracing.On) Tracing.ExceptionCatch(TraceEventType.Warning, this, "GetPhysicalDir", e);
                return null;
            }
            return null;
        }


        // -------------------------------------------------------------------------------
        // Calculate root ADSI virtual directory name (func by '[....]').
        private string GetWebServerForUrl(string url) {
            Uri uri = new Uri(url);
            DirectoryEntry w3Service = new DirectoryEntry("IIS://" + uri.Host + "/W3SVC");

            foreach (DirectoryEntry obj in w3Service.Children) {
                DirectoryEntry site = (DirectoryEntry)Adsi[obj.Path];           //may be already bound
                if (site == null) {
                    site = obj;
                    Adsi[obj.Path] = obj;
                }
                else {
                    obj.Dispose();
                }
                AppSettings settings = GetAppSettings(site);

                if (settings == null || settings.Bindings == null) {            //SchemaClassName != "IIsWebServer"
                    continue;
                }

                foreach (string bindingsEntry in settings.Bindings) {
                    if ( CompModSwitches.DynamicDiscoverySearcher.TraceVerbose ) Debug.WriteLine("GetWebServerForUrl() bindingsEntry=" + bindingsEntry);
                    string[] bindings = bindingsEntry.Split(':');
                    string ip = bindings[0];
                    string port = bindings[1];
                    string hostname = bindings[2];

                    if (Convert.ToInt32(port, CultureInfo.InvariantCulture) != uri.Port)
                        continue;

                    if (uri.HostNameType == UriHostNameType.Dns) {
                        if (hostname.Length == 0 || string.Compare(hostname, uri.Host, StringComparison.OrdinalIgnoreCase) == 0)
                            return site.Path;
                        }
                    else {
                        if (ip.Length == 0 || string.Compare(ip, uri.Host, StringComparison.OrdinalIgnoreCase) == 0)
                            return site.Path;
                    }
                }
            }
            return null;
        }

        // -------------------------------------------------------------------------------
        // Makes result URL found file path from diectory name and short file name.
        protected override string MakeResultPath(string dirName, string fileName) {
            string res = origUrl
                   + dirName.Substring(rootPathAsdi.Length, dirName.Length - rootPathAsdi.Length)
                   + '/' + fileName;
            return res;
        }

        // -------------------------------------------------------------------------------
        // Makes exclusion path absolute for quick comparision on search.
        protected override string MakeAbsExcludedPath(string pathRelativ) {
            return rootPathAsdi + '/' + pathRelativ.Replace('\\', '/');
        }

        // -------------------------------------------------------------------------------
        protected override bool IsVirtualSearch {
            get { return true; }
        }

        private AppSettings GetAppSettings(DirectoryEntry entry) {
            string key = entry.Path;                   //this is fast since does not cause bind()
            AppSettings result = null;

            object obj = webApps[key];

            if (obj == null) {
                // We provie a write lock while Hashtable supports multiple readers under single writer
                lock (webApps) {
                    obj = webApps[key];
                    if (obj == null) {                          //make sure other thread not taken care of
                        result = new AppSettings(entry);        //that consumes a 50-2000 ms
                        webApps[key] = result;
                    }
                }
            }
            else {
                result = (AppSettings)obj;
            }
            return result.AccessRead ? result : null;         //ignore denied object on upper level
        }

        private void CleanupCache() {
            //Destroy system resources excplicitly since the destructor is called sometime late
            foreach (DictionaryEntry obj in Adsi) {
                ((DirectoryEntry)(obj.Value)).Dispose();
            }
            rootPathAsdi = null;
            entryPathPrefix = null;
            startDir = null;

            Adsi     = null;
//REMOVE NEXT LINE IF the member webApps has turned into static (see webApps declaration line)
            webApps = null;
        }

        private class AppSettings {
            internal readonly bool AccessRead = false; // if false the caller will ignore the object
            internal readonly string[] Bindings = null;  // the field is only for WebServers
            internal readonly string VPath = null;  // the filed is only for VirtualDirs

            internal AppSettings(DirectoryEntry entry) {

                string schema = entry.SchemaClassName;
                AccessRead = true;

                if (schema == "IIsWebVirtualDir" || schema == "IIsWebDirectory") {
                    if (!(bool)(entry.Properties["AccessRead"][0])) {
                        AccessRead = false;
                        return;
                    }
                    if (schema == "IIsWebVirtualDir") {
                        VPath = (string) (entry.Properties["Path"][0]);
                    }
                }
                else if (schema == "IIsWebServer") {
                    Bindings = new string[entry.Properties["ServerBindings"].Count];
                    for (int i = 0; i < Bindings.Length; ++i) {
                        Bindings[i] = (string) (entry.Properties["ServerBindings"][i]);
                    }
                }
                else {
                    //schema is not recognized add to the cache but never look at the object
                    AccessRead = false;
                }
            }
        }

    }
}
