//------------------------------------------------------------------------------
// <copyright file="DynamicPhysicalDiscoSearcher.cs" company="Microsoft">
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

    
    /// <include file='doc\DynamicPhysicalDiscoSearcher.uex' path='docs/doc[@for="DynamicPhysicalDiscoSearcher"]/*' />
    /// <devdoc>
    /// Does a recursive search of virtual subdirectories to find stuff to
    /// make a disco file from. *.disco files (or whatever the PrimarySearchPattern is) are
    /// treated as end-points - recursion stops where they are found.
    /// </devdoc>
    internal class DynamicPhysicalDiscoSearcher : DynamicDiscoSearcher {
    
        private string startDir;
    
        internal DynamicPhysicalDiscoSearcher(string searchDir, string[] excludedUrls, string startUrl) : 
               base(excludedUrls)
        {
        startDir = searchDir;
        origUrl = startUrl;
        }
        
        // -------------------------------------------------------------------------------
        internal override void Search(string fileToSkipAtBegin) {
            SearchInit(fileToSkipAtBegin);
            ScanDirectory( startDir );
        }

        // -------------------------------------------------------------------------------
        //  Look in iven directory for subdirectories, feasable for further searching.
        protected override void SearchSubDirectories(string localDir) {
            DirectoryInfo dir = new DirectoryInfo(localDir);
            if (!dir.Exists)
                return;
            DirectoryInfo[] subDirs = dir.GetDirectories();
            
            foreach (DirectoryInfo subDir in subDirs) {
                if (subDir.Name == "." || subDir.Name == ".." ) {
                    continue;
                }
                ScanDirectory( localDir + '\\' + subDir.Name );
            }
        }

        // -------------------------------------------------------------------------------
        protected override DirectoryInfo GetPhysicalDir(string dir ) {
            
            if ( !Directory.Exists(dir) )
                return null;

            DirectoryInfo directory = new DirectoryInfo(dir);
            if ( !directory.Exists )
                return null;

            if ( 0 != (directory.Attributes & (FileAttributes.Hidden | FileAttributes.System | FileAttributes.Temporary))) {
                return null;
            }
                                       
            return directory;    
            }    

        // -------------------------------------------------------------------------------
        // Makes result URL found file path from diectory name and short file name.
        protected override string MakeResultPath(string dirName, string fileName) {
            string res = origUrl 
                   + dirName.Substring(startDir.Length, dirName.Length - startDir.Length).Replace('\\', '/') 
                   + '/' + fileName;  
            return res;
        }
    
        // -------------------------------------------------------------------------------
        // Makes exclusion path absolute for quick comparision on search.
        protected override string MakeAbsExcludedPath(string pathRelativ) {
            return startDir + '\\' + pathRelativ.Replace('/', '\\' );
        }
    
        // -------------------------------------------------------------------------------
        protected override bool IsVirtualSearch {
            get { return false; }
        }
        
    }
    

}
