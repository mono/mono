// NAnt - A .NET build tool
// Copyright (C) 2001 Gerry Shaw
//
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
//
// Serge (serge@wildwestsoftware.com)
// Gerry Shaw (gerry_shaw@yahoo.com)

namespace SourceForge.NAnt {

    using System;
    using System.IO;
    using System.Xml;
    using System.Xml.Xsl;
    using System.Xml.XPath;
    using System.Text.RegularExpressions;

    [TaskName("style")]
    public class StyleTask : Task {

        // TODO: consider prefixing private fields with _ to stay consistent (gs)

        [TaskAttribute("basedir", Required=false)]
        string baseDir = null;

        [TaskAttribute("destdir", Required=false)]
        string destDir = null;

        [TaskAttribute("extension", Required=false)]
        string extension = "html";

        [TaskAttribute("style", Required=true)]
        string xsltFile = null;

        [TaskAttribute("in", Required=true)]
        string srcFile = null;

        [TaskAttribute("out", Required=false)]
        string destFile = null;

        private static string GetPath(string dir, string file) {
            // TODO: remove platform dependencies by using System.IO.Path (gs)
            string d = (dir == null)
                ? ""
                : Regex.Replace(dir, "/", "\\");

            return (d==null || d=="")
                ? (file==null || file=="") ? "" : file
                : d.EndsWith("\\")
                ? d +file : d + "\\" + file;
        }

        private XmlReader CreateXmlReader(string dir, string file) {
            string xmlPath = GetPath(dir, file);
            XmlTextReader xmlReader = null;

            try {
                xmlReader = new XmlTextReader(new FileStream(xmlPath, FileMode.Open));
            } catch (Exception) {
                xmlReader = null;
            }

            return xmlReader;
        }

        private XmlWriter CreateXmlWriter(string dir, string file) {
            string xmlPath = GetPath(dir, file);

            XmlWriter xmlWriter = null;

            string targetDir = Path.GetDirectoryName(Path.GetFullPath(xmlPath));
            if (targetDir != null && targetDir != "" && !Directory.Exists(targetDir)) {
                Directory.CreateDirectory(targetDir);
            }

            try {
                // UTF-8 encoding will be used
                xmlWriter = new XmlTextWriter(xmlPath, null);
            } catch (Exception) {
                xmlWriter = null;
            }

            return xmlWriter;
        }

        protected override void ExecuteTask() {
            string destFile = this.destFile;

            if (destFile == null || destFile == "") {
                // TODO: use System.IO.Path (gs)
                string ext = extension[0]=='.'
                    ? extension
                    : "." + extension;

                int extPos = srcFile.LastIndexOf('.');

                if (extPos == -1) {
                    destFile = srcFile + ext;
                } else {
                    destFile = srcFile.Substring(0, extPos) + ext;
                }
            }

            string srcPath = GetPath(baseDir, srcFile);
            string destPath = GetPath(destDir, destFile);
            string xsltPath = GetPath(baseDir, xsltFile);

            FileInfo srcInfo = new FileInfo(srcPath);
            FileInfo destInfo = new FileInfo(destPath);
            FileInfo xsltInfo = new FileInfo(xsltPath);

            if (!srcInfo.Exists) {
                throw new BuildException("Unable to find source xml file.");
            }
            if (!xsltInfo.Exists) {
                throw new BuildException("Unable to find stylesheet file.");
            }

            bool destOutdated = !destInfo.Exists
                || srcInfo.LastWriteTime > destInfo.LastWriteTime
                || xsltInfo.LastWriteTime > destInfo.LastWriteTime;

            if (destOutdated) {
                XmlReader xmlReader = CreateXmlReader(baseDir, srcFile);
                XmlReader xslReader = CreateXmlReader(baseDir, xsltFile);
                XmlWriter xmlWriter = CreateXmlWriter(destDir, destFile);

                Log.WriteLine(LogPrefix + "Transforming into " + Path.GetFullPath(destDir));

                // TODO: remove assignments from conditional statement (gs)
                if (xmlReader != null && xslReader != null && xmlWriter != null) {
                    XslTransform xslt = new XslTransform();
                    XPathDocument xml = new XPathDocument(xmlReader);

                    Log.WriteLine(LogPrefix + "Loading stylesheet " + Path.GetFullPath(xsltPath));
                    try {
                        xslt.Load(xslReader);
                    } catch (XsltCompileException xce) {
                        throw new BuildException(xce.Message, xce);
                    } catch (Exception e) {
                        throw new BuildException(e.Message, e);
                    }

                    Log.WriteLine(LogPrefix + "Processing " + Path.GetFullPath(srcPath) + " to " + Path.GetFullPath(destPath));
                    try {
                        xslt.Transform(xml, null, xmlWriter);
                    } catch (Exception e) {
                        throw new BuildException(e.Message, e);
                    }
                } else {
                    // not sure how to deal with this...
                    // TODO: remove this statement or do something useful (gs)
                    // Can this condition occur? I would have thought
                    // that an exception would be thrown. (gs)
                }
            }
        }
    }
}
