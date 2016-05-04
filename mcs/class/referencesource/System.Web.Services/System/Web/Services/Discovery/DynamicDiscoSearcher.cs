//------------------------------------------------------------------------------
// <copyright file="DynamicDiscoSearcher.cs" company="Microsoft">
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
    using System.Web.Services.Configuration;
    using System.ComponentModel;
    using System.Globalization;
    

    /// <include file='doc\DynamicDiscoSearcher.uex' path='docs/doc[@for="DynamicDiscoSearcher"]/*' />
    /// <devdoc>
    /// Does a recursive search of subdirectories (physical and virtual) to find stuff to
    /// make a disco file from. *.disco files (or whatever the PrimarySearchPattern is) are
    /// treated as end-points - recursion stops where they are found.
    /// It's a base class for DynamicVirtualDiscoSearcher and DynamicPhysicalDiscoSearcher.
    /// </devdoc>
    internal abstract class DynamicDiscoSearcher {

        protected string origUrl;            // original URL to start search
        protected string[] excludedUrls;     // names relative to starting path
        protected string fileToSkipFirst;    // name of file to skip on 1st level
        protected ArrayList filesFound;
        protected DiscoverySearchPattern[] primarySearchPatterns = null;
        protected DiscoverySearchPattern[] secondarySearchPatterns = null;
        protected DiscoveryDocument discoDoc = new DiscoveryDocument();
        protected Hashtable excludedUrlsTable = null;
        protected int subDirLevel = 0;       // current nested level of subdirectory relative to search root

        // -------------------------------------------------------------------------------
        internal DynamicDiscoSearcher(string[] excludeUrlsList) {
            excludedUrls = excludeUrlsList;
            filesFound = new ArrayList();
        }

        // -------------------------------------------------------------------------------
        internal virtual void SearchInit(string fileToSkipAtBegin) {
            subDirLevel = 0;
            fileToSkipFirst = fileToSkipAtBegin;
        }

        // -------------------------------------------------------------------------------
        protected bool IsExcluded(string url) {
            if (excludedUrlsTable == null) {
                excludedUrlsTable = new Hashtable();
                foreach (string s in excludedUrls) {
                     Debug.Assert( s != null, "null element in excluded list" );
                     excludedUrlsTable.Add( MakeAbsExcludedPath(s).ToLower(CultureInfo.InvariantCulture), null);
                }
            }

            return excludedUrlsTable.Contains( url.ToLower(CultureInfo.InvariantCulture) );
        }

        // -------------------------------------------------------------------------------
        internal DiscoveryDocument DiscoveryDocument {
            get {
                return discoDoc;
            }
        }

        // -------------------------------------------------------------------------------
        internal DiscoverySearchPattern[] PrimarySearchPattern {
            get {
                if (primarySearchPatterns == null) {
                    // For the primary search the pattern is ".vsdisco"
                    primarySearchPatterns = new DiscoverySearchPattern[] { new DiscoveryDocumentSearchPattern() };
                }
                return primarySearchPatterns;
            }
        }

        // -------------------------------------------------------------------------------
        internal DiscoverySearchPattern[] SecondarySearchPattern {
            get {
                if (secondarySearchPatterns == null) {

                //  ******  Get pattern type from Config (no more needed)  *******
                //  Type[] searchPattern = WebServicesConfiguration.Current.DiscoverySearchPatternTypes;
                //  secondarySearchPatterns = new DiscoverySearchPattern[searchPattern.Length];
                //
                //  for (int i = 0; i < searchPattern.Length; i++) {
                //      secondarySearchPatterns[i] = (DiscoverySearchPattern) Activator.CreateInstance(searchPattern[i]);
                //  }

                secondarySearchPatterns = new DiscoverySearchPattern[] { new ContractSearchPattern(),
                                                                         new DiscoveryDocumentLinksPattern() };
                }

                return secondarySearchPatterns;
            }
        }

        // -------------------------------------------------------------------------------
        // Invokes searching by patterns in current dir. If needed, initiates further search in subdirectories.
        protected void ScanDirectory(string directory) {
            if ( CompModSwitches.DynamicDiscoverySearcher.TraceVerbose ) Debug.WriteLine( "DynamicDiscoSearcher.ScanDirectory(): directory=" + directory);
            if ( IsExcluded(directory) )                // what name is meant here?
                return;

            bool primaryFound = ScanDirByPattern(directory, true /*primary*/, PrimarySearchPattern);

            if (!primaryFound) {
                if (!IsVirtualSearch ) {
                    ScanDirByPattern(directory, false /*secondary*/, SecondarySearchPattern);
                } else {
                    // We restrict second stage of a virtual discovery only to static .disco documents
                    // We assume that starting directory does not need a second stage
                    if (subDirLevel != 0) {
                        DiscoverySearchPattern[] staticDiscoPattern = new DiscoverySearchPattern[] { new DiscoveryDocumentLinksPattern() };
                        ScanDirByPattern(directory, false /*secondary*/, staticDiscoPattern);
                    }
                }

                if ( IsVirtualSearch && subDirLevel > 0 )
                    return;                         // stop search in subdir levels deeper than 1 for virtual search

                subDirLevel++;
                fileToSkipFirst = "";               // do not skip this file on lower levels
                SearchSubDirectories(directory);    // search deeper (indirect recursion)
                subDirLevel--;
            }
        }

        // -------------------------------------------------------------------------------
        // Looks in a physical directory for a file matching whatever the configured pattern is.
        // Returns: 'true' if primary file has been found (and added to Discovery References).
        protected bool ScanDirByPattern(string dir, bool IsPrimary, DiscoverySearchPattern[] patterns) {

            DirectoryInfo directory = GetPhysicalDir(dir);              // comment here
            if ( directory == null )
                return false;
            if ( CompModSwitches.DynamicDiscoverySearcher.TraceVerbose )
                Debug.WriteLine( "= DynamicDiscoSearcher.ScanDirByPattern(): dir=" + dir + "  Phys.dir=" + directory.Name);

            bool isFileFound = false;
            for (int i = 0; i < patterns.Length; i++) {
                FileInfo[] files = directory.GetFiles(patterns[i].Pattern);             // search in dir

                foreach (FileInfo file in files) {

                    if ((file.Attributes & FileAttributes.Directory) == 0) {

                        if ( CompModSwitches.DynamicDiscoverySearcher.TraceVerbose ) Debug.WriteLine( "=== DynamicDiscoSearcher.ScanDirByPattern(): file.Name=" + file.Name + "  fileToSkipFirst=" + fileToSkipFirst);

                        // first skip given (i.e. starting) file
                        if ( String.Compare(file.Name, fileToSkipFirst, StringComparison.OrdinalIgnoreCase) == 0 ) {       // ignore case compare
                            continue;
                            }

                        string resultName = MakeResultPath(dir, file.Name);
                        filesFound.Add( resultName );
                        discoDoc.References.Add(patterns[i].GetDiscoveryReference(resultName));
                        isFileFound = true;
                    }
                }

            }

        return (IsPrimary && isFileFound);
        }

        // ------------  abstract methods  -----------------

        /// <include file='doc\DynamicDiscoSearcher.uex' path='docs/doc[@for="DynamicDiscoSearcher.Search"]/*' />
        /// <devdoc>
        /// Main function. Searches dir recursively for primary (.vsdisco) and seconary (.asmx) files.
        /// </devdoc>
        internal abstract void Search(string fileToSkipAtBegin);

        // Gets phisycal directory info from its virtual or actual name.
        protected abstract DirectoryInfo GetPhysicalDir(string dir );

        //  Search given directory for subdirectories, feasable for further searching.
        protected abstract void SearchSubDirectories(string directory);

        // Makes result URL found file path from diectory name and short file name.
        protected abstract string MakeResultPath(string dirName, string fileName);

        // Makes exclusion path absolute for quick comparision on search.
        protected abstract string MakeAbsExcludedPath(string pathRelativ);

        // 'true' if search isVirtual
        protected abstract bool IsVirtualSearch { get; }
    }

}
