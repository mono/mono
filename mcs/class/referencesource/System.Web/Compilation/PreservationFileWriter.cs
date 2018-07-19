//------------------------------------------------------------------------------
// <copyright file="PreservationFileReader.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------



namespace System.Web.Compilation {

using System;
using System.IO;
using System.Collections;
using System.Globalization;
using System.Text;
using System.Xml;
using System.Web.Util;
using System.Web.UI;

internal class PreservationFileWriter {

    private XmlTextWriter _writer;
    private bool _precompilationMode;

    internal const string fileDependenciesTagName = "filedeps";
    internal const string fileDependencyTagName = "filedep";
    internal const string buildResultDependenciesTagName = "builddeps";
    internal const string buildResultDependencyTagName = "builddep";

    internal PreservationFileWriter(bool precompilationMode) {
        _precompilationMode = precompilationMode;
    }

    internal void SaveBuildResultToFile(string preservationFile,
        BuildResult result, long hashCode) {

        _writer = new XmlTextWriter(preservationFile, Encoding.UTF8);

        try {
            _writer.Formatting = Formatting.Indented;
            _writer.Indentation = 4;
            _writer.WriteStartDocument();

            // <preserve assem="assemblyFile">
            _writer.WriteStartElement("preserve");

            // Save the type of BuildResult we're dealing with
            Debug.Assert(result.GetCode() != BuildResultTypeCode.Invalid); 
            SetAttribute("resultType", ((int)result.GetCode()).ToString(CultureInfo.InvariantCulture));

            // Save the virtual path for this BuildResult
            if (result.VirtualPath != null)
                SetAttribute("virtualPath", result.VirtualPath.VirtualPathString);

            // Get the hash code of the BuildResult
            long hash = result.ComputeHashCode(hashCode);

            // The hash should always be valid if we got here.
            Debug.Assert(hash != 0, "hash != 0"); 

            // Save it to the preservation file
            SetAttribute("hash", hash.ToString("x", CultureInfo.InvariantCulture));

            // Can be null if that's what the VirtualPathProvider returns
            string fileHash = result.VirtualPathDependenciesHash;
            if (fileHash != null)
                SetAttribute("filehash", fileHash);

            result.SetPreservedAttributes(this);

            SaveDependencies(result.VirtualPathDependencies);

            // </preserve>
            _writer.WriteEndElement();
            _writer.WriteEndDocument();

            _writer.Close();
        }
        catch {

            // If an exception occurs during the writing of the xml file, clean it up
            _writer.Close();
            File.Delete(preservationFile);

            throw;
        }
    }

    private void SaveDependencies(ICollection dependencies) {

        // Write all the dependencies
        if (dependencies != null) {
            // <filedeps>
            _writer.WriteStartElement(fileDependenciesTagName);

            foreach (string vpath in dependencies) {
                // e.g. <filedep name="/testapp/foo.aspx" />
                _writer.WriteStartElement(fileDependencyTagName);
                _writer.WriteAttributeString("name", vpath);
                _writer.WriteEndElement();
            }

            // </filedeps>
            _writer.WriteEndElement();
        }
    }

    internal void SetAttribute(string name, string value) {
        _writer.WriteAttributeString(name, value);
    }
}

}
