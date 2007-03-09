//
// Authors:
//   Marek Habersack (mhabersack@novell.com)
//
// (C) 2007 Novell, Inc
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
using System;
using System.Collections;
using System.IO;
using System.Xml;

public class App
{
  string templateFile;
  string targetDir;
  string targetPath;
  string sourceDir;
  
  XmlNamespaceManager nsmgr;

  Stack folders;
  
  public App (string templateFile, string targetDir)
  {
    this.templateFile = templateFile;
    this.targetDir = targetDir;

    this.sourceDir = Path.GetDirectoryName (templateFile);
    if (this.sourceDir == null || this.sourceDir.Length == 0)
      this.sourceDir = Directory.GetCurrentDirectory ();
  }

  public void Run ()
  {
    if (templateFile == null || templateFile.Length == 0 ||
	targetDir == null || targetDir.Length == 0)
      throw new ApplicationException ("Missing or invalid installation data");

    XmlDocument doc = new XmlDocument ();
    doc.Load (templateFile);
    nsmgr = new XmlNamespaceManager (doc.NameTable);
    nsmgr.AddNamespace ("def", "http://schemas.microsoft.com/developer/vstemplate/2005");
    
    ReadTemplateData (doc);
    InstallProject (doc);
  }

  void ReadTemplateData (XmlDocument doc)
  {
    XmlNode defaultName = doc.SelectSingleNode ("//def:VSTemplate[@Type='Project']/def:TemplateData/def:DefaultName", nsmgr);
    if (defaultName == null)
      throw new ApplicationException ("Input file is not a VisualStudio Template");
    string folderName = defaultName.InnerText;
    targetPath = Path.Combine (targetDir, folderName);
    
    if (!Directory.Exists (targetPath))
      Directory.CreateDirectory (targetPath);

    folders = new Stack ();
  }

  string SafeGetAttribute (XmlNode node, string name)
  {
    XmlAttribute attr = node.Attributes [name];
    if (attr != null)
      return attr.Value;
    return String.Empty;
  }

  string GetCurPath ()
  {
    string curPath = folders.Count > 0 ? (string) folders.Peek () : null;
    if (curPath == null || curPath.Length == 0)
      return targetPath;
    return curPath;
  }
  
  void ProcessFolder (XmlNode node)
  {
    string curPath = GetCurPath ();
    string folderPath = Path.Combine (curPath, SafeGetAttribute (node, "Name"));
    if (!Directory.Exists (folderPath))
      Directory.CreateDirectory (folderPath);
    folders.Push (folderPath);
    foreach (XmlNode child in node.ChildNodes)
      ProcessNode (child);
    folders.Pop ();
  }

  void ProcessItem (XmlNode node)
  {
    string curPath = GetCurPath ();
    string srcName = node.InnerText;
    string targetName = SafeGetAttribute (node, "TargetFileName");
    string src = Path.Combine (sourceDir, srcName);
    string dst = Path.Combine (curPath, targetName.Length > 0 ? targetName : srcName);

    if (!File.Exists (src)) {
      Console.WriteLine ("Warning: source file {0} does not exist.", src);
      return;
    }
    File.Copy (src, dst, true);
  }
  
  void ProcessNode (XmlNode node)
  {
    if (node.NodeType != XmlNodeType.Element)
      return;
    
    switch (node.Name) {
      case "Folder":
	ProcessFolder (node);
	break;

      case "ProjectItem":
	ProcessItem (node);
	break;
      }
  }
  
  void InstallProject (XmlDocument doc)
  {
    XmlNode project = doc.SelectSingleNode ("//def:VSTemplate[@Type='Project']/def:TemplateContent/def:Project", nsmgr);
    if (project == null)
      throw new ApplicationException ("Missing project contents in the template file");

    foreach (XmlNode child in project.ChildNodes)
      ProcessNode (child);
  }
}

public class AppMain
{
  public static void Main (string[] args)
  {
    if (args.Length < 2)
      Usage ();

    App app = new App (args [0], args [1]);

    try {
      app.Run ();
    } catch (Exception ex) {
      Console.WriteLine ("Failed to install template {0} in {1}\n{2}\n", args [0], args [1], ex.Message);
      Console.WriteLine ("Exception: {0}", ex);
      Environment.Exit (1);
    }
  }

  static void Usage ()
  {
    Console.WriteLine ("Usage: installvst <VSTemplateFile> <DestinationPath>\n");
    Environment.Exit (1);
  }
}
