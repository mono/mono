// addins-provider.cs
//
// A provider to display Mono.Addins extension models
//
// Author:
//   Lluis Sanchez Gual <lluis@novell.com>
//
// Copyright (c) 2007 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//
//

using System;
using System.Diagnostics;
using System.Text;
using System.IO;
using System.Xml;

namespace Monodoc
{ 

	//
	// The simple provider generates the information source
	//
	public class AddinsProvider : Provider
	{
		string file;
		
		public AddinsProvider (string xmlModelFile)
		{
			file = xmlModelFile;
			
			if (!File.Exists (file))
				throw new FileNotFoundException (String.Format ("The file `{0}' does not exist", file));
		}

		public override void PopulateTree (Tree tree)
		{
			string fileId = tree.tree.HelpSource.PackFile (file);
			XmlDocument doc = new XmlDocument ();
			doc.Load (file);
			
			foreach (XmlElement addin in doc.SelectNodes ("Addins/Addin")) {

				string addinId = addin.GetAttribute ("fullId");
				Node newNode = tree.CreateNode (addin.GetAttribute ("name"), "addin:" + fileId + "#" + addinId);

				foreach (XmlElement node in addin.SelectNodes ("ExtensionPoint")) {
					string target = "extension-point:" + fileId + "#" + addinId + "#" + node.GetAttribute ("path");
					Node newExt = newNode.CreateNode (node.GetAttribute ("name"), target);
			
					foreach (XmlElement en in node.SelectNodes ("ExtensionNode")) {
						string nid = en.GetAttribute ("id");
						string nname = en.GetAttribute ("name");
						newExt.CreateNode (nname, "extension-node:" + fileId + "#" + addinId + "#" + nid);
					}
				}
			}
		}


		public override void CloseTree (HelpSource hs, Tree tree)
		{
		}
	}

	//
	// The HelpSource is used during the rendering phase.
	//

	public class AddinsHelpSource : HelpSource
	{
		public AddinsHelpSource (string base_file, bool create) : base (base_file, create) 
		{
		}
		
		protected const string AddinPrefix = "addin:";
		protected const string ExtensionPrefix = "extension-point:";
		protected const string ExtensionNodePrefix = "extension-node:";
		
		public override string GetText (string url, out Node match_node)
		{
			match_node = null;

			string c = GetCachedText (url);
			if (c != null)
				return c;

			if (url.StartsWith (AddinPrefix))
				return GetAddinTextFromUrl (url);
			else if (url.StartsWith (ExtensionPrefix))
				return GetExtensionTextFromUrl (url);
			else if (url.StartsWith (ExtensionNodePrefix))
				return GetExtensionNodeTextFromUrl (url);

			return null;
		}
		
		protected string GetAddinTextFromUrl (string url)
		{
			// Remove "addin:" prefix including any help-source id on the front.
			url = url.Substring (AddinPrefix.Length);
			int i = url.IndexOf ('#');

			if (i == -1) {
				Message (TraceLevel.Warning, "Warning, NULL url!");
				return "<html>Invalid url</html>";
			}
			
			string fileId = url.Substring (0, i);
			string addinId = url.Substring (i+1);

			XmlElement addin = GetAddin (fileId, addinId);
			if (addin == null)
				return "<html>Add-in not found: " + addinId + "</html>";
			
			StringBuilder sb = new StringBuilder ("<html>");
			sb.Append ("<h1>").Append (addin.GetAttribute ("name")).Append ("</h1>");
			XmlElement docs = (XmlElement) addin.SelectSingleNode ("Description");
			if (docs != null)
				sb.Append (docs.InnerText);

			sb.Append ("<p><table border=\"1\" cellpadding=\"4\" cellspacing=\"0\">");
			sb.AppendFormat ("<tr><td><b>Id</b></td><td>{0}</td></tr>", addin.GetAttribute ("addinId"));
			sb.AppendFormat ("<tr><td><b>Namespace</b></td><td>{0}</td></tr>", addin.GetAttribute ("namespace"));
			sb.AppendFormat ("<tr><td><b>Version</b></td><td>{0}</td></tr>", addin.GetAttribute ("version"));
			sb.Append ("</table></p>");
			sb.Append ("<p><b>Extension Points</b>:</p>");
			sb.Append ("<ul>");
			
			foreach (XmlElement ep in addin.SelectNodes ("ExtensionPoint")) {
				sb.AppendFormat ("<li><a href=\"extension-point:{0}#{1}#{2}\">{3}</li>", fileId, addinId, ep.GetAttribute ("path"), ep.GetAttribute ("name"));
			}
			sb.Append ("</ul>");
			
			sb.Append ("</html>");
			return sb.ToString ();
		}
		
		protected string GetExtensionTextFromUrl (string url)
		{
			// Remove "addin:" prefix including any help-source id on the front.
			url = url.Substring (ExtensionPrefix.Length);
			int i = url.IndexOf ('#');

			if (i == -1) {
				Message (TraceLevel.Warning, "Warning, NULL url!");
				return "<html>Invalid url</html>";
			}
			
			string fileId = url.Substring (0, i);
			
			int j = url.IndexOf ('#', i+1);
			string addinId = url.Substring (i+1, j-i-1);
			string path = url.Substring (j+1);

			XmlElement addin = GetAddin (fileId, addinId);
			if (addin == null)
				return "<html>Add-in not found: " + addinId + "</html>";
			
			XmlElement ext = (XmlElement) addin.SelectSingleNode ("ExtensionPoint[@path='" + path + "']");
			if (ext == null)
				return "<html>Extension point not found: " + path + "</html>";
			
			StringBuilder sb = new StringBuilder ("<html>");
			sb.Append ("<h1>").Append (ext.GetAttribute ("name")).Append ("</h1>");

			path = path.Replace ("/", " <b>/</b> ");
			sb.Append ("<p><b>Path</b>: ").Append (path).Append ("</p>");
			XmlElement desc = (XmlElement) ext.SelectSingleNode ("Description");
			if (desc != null)
				sb.Append (desc.InnerText);

			sb.Append ("<p><b>Extension Nodes</b>:</p>");
			sb.Append ("<table border=\"1\" cellpadding=\"4\" cellspacing=\"0\">");
			
			foreach (XmlElement en in ext.SelectNodes ("ExtensionNode")) {
				string nid = en.GetAttribute ("id");
				string nname = en.GetAttribute ("name"); 
				string sdesc = "";
				desc = (XmlElement) en.SelectSingleNode ("Description");
				if (desc != null)
					sdesc = desc.InnerText;
				
				sb.AppendFormat ("<tr><td><a href=\"extension-node:{0}#{1}#{2}\">{3}</td><td>{4}</td></tr>", fileId, addinId, nid, nname, sdesc);
			}
			sb.Append ("</table>");
			
			sb.Append ("</html>");
			return sb.ToString ();
		}
		
		protected string GetExtensionNodeTextFromUrl (string url)
		{
			// Remove "addin:" prefix including any help-source id on the front.
			url = url.Substring (ExtensionNodePrefix.Length);
			int i = url.IndexOf ('#');

			if (i == -1) {
				Message (TraceLevel.Warning, "Warning, NULL url!");
				return "<html>Invalid url</html>";
			}
			
			string fileId = url.Substring (0, i);
			
			int j = url.IndexOf ('#', i+1);
			string addinId = url.Substring (i+1, j-i-1);
			string nodeId = url.Substring (j+1);

			XmlElement addin = GetAddin (fileId, addinId);
			if (addin == null)
				return "<html>Add-in not found: " + addinId + "</html>";
			
			XmlElement node = (XmlElement) addin.SelectSingleNode ("ExtensionNodeType[@id='" + nodeId + "']");
			if (node == null)
				return "<html>Extension point not found: " + nodeId + "</html>";
			
			StringBuilder sb = new StringBuilder ("<html>");
			sb.Append ("<h1>").Append (node.GetAttribute ("name")).Append ("</h1>");
			XmlElement desc = (XmlElement) node.SelectSingleNode ("Description");
			if (desc != null)
				sb.Append (desc.InnerText);

			sb.Append ("<p><b>Attributes</b>:</p>");
			sb.Append ("<table border=\"1\" cellpadding=\"4\" cellspacing=\"0\"><tr>");
			sb.Append ("<td><b>Name</b></td>");
			sb.Append ("<td><b>Type</b></td>");
			sb.Append ("<td><b>Required</b></td>");
			sb.Append ("<td><b>Localizable</b></td>");
			sb.Append ("<td><b>Description</b></td>");
			sb.Append ("<tr>");
			sb.Append ("<td>id</td>");
			sb.Append ("<td>System.String</td>");
			sb.Append ("<td></td>");
			sb.Append ("<td></td>");
			sb.Append ("<td>Identifier of the node.</td>");
			sb.Append ("</tr>");
			
			foreach (XmlElement at in node.SelectNodes ("Attributes/Attribute")) {
				sb.Append ("<tr>");
				sb.AppendFormat ("<td>{0}</td>", at.GetAttribute ("name"));
				sb.AppendFormat ("<td>{0}</td>", at.GetAttribute ("type"));
				if (at.GetAttribute ("required") == "True")
					sb.Append ("<td>Yes</td>");
				else
					sb.Append ("<td></td>");
				if (at.GetAttribute ("localizable") == "True")
					sb.Append ("<td>Yes</td>");
				else
					sb.Append ("<td></td>");
				string sdesc = "";
				desc = (XmlElement) at.SelectSingleNode ("Description");
				if (desc != null)
					sdesc = desc.InnerText;
				
				sb.AppendFormat ("<td>{0}</td>", sdesc);
				sb.Append ("</tr>");
			}
			sb.Append ("</table>");

			XmlNodeList children = node.SelectNodes ("ChildNodes/ExtensionNode");
			if (children.Count > 0) {
				sb.Append ("<p><b>Child Nodes</b>:</p>");
				sb.Append ("<table border=\"1\" cellpadding=\"4\" cellspacing=\"0\">");
				
				foreach (XmlElement en in children) {
					string nid = en.GetAttribute ("id");
					string nname = en.GetAttribute ("name"); 
					string sdesc = "";
					desc = (XmlElement) en.SelectSingleNode ("Description");
					if (desc != null)
						sdesc = desc.InnerText;
					
					sb.AppendFormat ("<tr><td><a href=\"extension-node:{0}#{1}#{2}\">{3}</td><td>{4}</td></tr>", fileId, addinId, nid, nname, sdesc);
				}
				sb.Append ("</table>");
			}
			
			sb.Append ("</html>");
			return sb.ToString ();
		}
		
		XmlElement GetAddin (string fileId, string addinId)
		{
			Stream s = GetHelpStream (fileId);
			StreamReader file;
			using (file = new StreamReader (s)) {
				XmlDocument doc = new XmlDocument ();
				doc.Load (file);
				XmlElement addin = (XmlElement) doc.SelectSingleNode ("Addins/Addin[@fullId='" + addinId + "']");
				if (addin != null)
					return addin;
				else
					return null;
			}
		}
	}
}
