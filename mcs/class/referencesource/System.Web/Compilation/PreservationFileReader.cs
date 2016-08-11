//------------------------------------------------------------------------------
// <copyright file="PreservationFileReader.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------



namespace System.Web.Compilation {

using System;
using System.IO;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Xml;
using System.Security;
using System.Web.Configuration;
using System.Web.Util;
using System.Web.UI;

internal class PreservationFileReader {

    private XmlNode _root;
    private bool _precompilationMode;
    private DiskBuildResultCache _diskCache;

    private ArrayList _sourceDependencies;

    internal PreservationFileReader(DiskBuildResultCache diskCache, bool precompilationMode) {
        _diskCache = diskCache;
        _precompilationMode = precompilationMode;
    }

    internal BuildResult ReadBuildResultFromFile(VirtualPath virtualPath, string preservationFile, long hashCode, bool ensureIsUpToDate) {

        // Ignore if the preservation file doesn't exist
        if (!FileUtil.FileExists(preservationFile)) {
            Debug.Trace("PreservationFileReader", "Can't find preservation file " + Path.GetFileName(preservationFile));
            return null;
        }

        BuildResult result = null;
        try {
            result = ReadFileInternal(virtualPath, preservationFile, hashCode, ensureIsUpToDate);
        }
        catch (SecurityException) {
            // We eat all exceptions, except for SecurityException's, because they
            // are ususally a sign that something is not set up correctly, and we
            // don't want to lose the stack (VSWhidbey 269566)
            throw;
        }
        catch {
            if (!_precompilationMode) {
                // The preservation file can't be used, so get rid of it
                Util.RemoveOrRenameFile(preservationFile);
            }
        }

        return result;
    }

    [SuppressMessage("Microsoft.Security", "MSEC1207:UseXmlReaderForLoad", Justification = "Xml file is created by us and only accessible to admins.")]
    [SuppressMessage("Microsoft.Security.Xml", "CA3056:UseXmlReaderForLoad", Justification = "Xml file is created by us and only accessible to admins.")]
    private BuildResult ReadFileInternal(VirtualPath virtualPath, string preservationFile, long hashCode, bool ensureIsUpToDate) {

        XmlDocument doc = new XmlDocument();

        doc.Load(preservationFile);

        // Get the root element, and make sure it's what we expect
        _root = doc.DocumentElement;
        Debug.Assert(_root != null && _root.Name == "preserve", "_root != null && _root.Name == \"preserve\"");
        if (_root == null || _root.Name != "preserve")
            return null;

        // Get the type of the BuildResult preserved in this file
        string resultTypeCodeString = GetAttribute("resultType");
        BuildResultTypeCode resultTypeCode = (BuildResultTypeCode)Int32.Parse(
            resultTypeCodeString, CultureInfo.InvariantCulture);

        // Get the config path that affects this BuildResult if one wasn't passed in.
        // Note that the passed in path may be different with Sharepoint-like ghosting (VSWhidbey 343230)
        if (virtualPath == null)
            virtualPath = VirtualPath.Create(GetAttribute("virtualPath"));

        long savedHash = 0;
        string savedFileHash = null;

        // Ignore dependencies in precompilation mode
        if (!_precompilationMode) {
            // Read the saved hash from the preservation file
            string hashString = GetAttribute("hash");
            Debug.Assert(hashString != null, "hashString != null");
            if (hashString == null)
                return null;

            // Parse the saved hash string as an hex int
            savedHash = Int64.Parse(hashString, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);

            // Read the saved file hash from the preservation file.  This is the hash the represents
            // the state of all the virtual files that the build result depends on.
            savedFileHash = GetAttribute("filehash");
        }

        // Create the BuildResult accordingly
        BuildResult result = BuildResult.CreateBuildResultFromCode(resultTypeCode, virtualPath);

        // Ignore dependencies in precompilation mode
        if (!_precompilationMode) {

            ReadDependencies();
            if (_sourceDependencies != null)
                result.SetVirtualPathDependencies(_sourceDependencies);

            result.VirtualPathDependenciesHash = savedFileHash;

            // Check if the build result is up to date
            bool outOfDate = false;
            if (!result.IsUpToDate(virtualPath, ensureIsUpToDate)) {
                Debug.Trace("PreservationFileReader", Path.GetFileName(preservationFile) +
                    " is out of date (IsUpToDate==false)");

                outOfDate = true;
            }
            else {

                // The virtual paths hash code was up to date, so check the
                // other hash code.

                // Get the current hash code
                long currentHash = result.ComputeHashCode(hashCode);

                // If the hash doesn't match, the preserved data is out of date
                if (currentHash == 0 || currentHash != savedHash) {
                    outOfDate = true;
                    Debug.Trace("PreservationFileReader", Path.GetFileName(preservationFile) +
                        " is out of date (ComputeHashCode)");
                }
            }

            if (outOfDate) {
                bool gotLock = false;
                try {
                    // We need to delete the preservation file together with the assemblies/pdbs
                    // under the same lock so to avoid bad interleaving where one process 
                    // deletes the .compiled file that another process just created, orphaning
                    // the files generated by the other process. 
                    // (Dev10 bug 791299)
                    CompilationLock.GetLock(ref gotLock);

                    // Give the BuildResult a chance to do some cleanup
                    result.RemoveOutOfDateResources(this);

                    // The preservation file is not useable, so delete it
                    File.Delete(preservationFile);
                }
                finally {
                    // Always release the mutex if we had taken it
                    if (gotLock) {
                        CompilationLock.ReleaseLock();
                    }
                }
                return null;
            }
        }

        // Ask the BuildResult to read the data it needs
        result.GetPreservedAttributes(this);

        return result;
    }

    private void ReadDependencies() {

        IEnumerator childEnumerator = _root.ChildNodes.GetEnumerator();
        while (childEnumerator.MoveNext()) {
            XmlNode dependenciesNode = (XmlNode)childEnumerator.Current;
            if (dependenciesNode.NodeType != XmlNodeType.Element)
                continue;

            // verify no unrecognized attributes
            Debug.Assert(dependenciesNode.Attributes.Count == 0); 

            switch (dependenciesNode.Name) {
            case PreservationFileWriter.fileDependenciesTagName:
                Debug.Assert(_sourceDependencies == null);
                _sourceDependencies = ReadDependencies(dependenciesNode,
                    PreservationFileWriter.fileDependencyTagName);
                break;

            default:
                Debug.Assert(false, dependenciesNode.Name);
                break;
            }
        }
    }

    private ArrayList ReadDependencies(XmlNode parent, string tagName) {

        ArrayList dependencies = new ArrayList();

        IEnumerator childEnumerator = parent.ChildNodes.GetEnumerator();
        while (childEnumerator.MoveNext()) {
            XmlNode dependencyNode = (XmlNode)childEnumerator.Current;
            if (dependencyNode.NodeType != XmlNodeType.Element)
                continue;

            Debug.Assert(dependencyNode.Name.Equals(tagName));
            if (!dependencyNode.Name.Equals(tagName))
                break;

            string fileName = HandlerBase.RemoveAttribute(dependencyNode, "name");

            Debug.Assert(fileName != null, "fileName != null");

            // verify no unrecognized attributes
            Debug.Assert(dependencyNode.Attributes.Count == 0); 

            if (fileName == null)
                return null;

            dependencies.Add(fileName);
        }

        return dependencies;
    }

    internal string GetAttribute(string name) {
        return HandlerBase.RemoveAttribute(_root, name);
    }

    internal DiskBuildResultCache DiskCache {
        get { return _diskCache; }
    }
}

}
