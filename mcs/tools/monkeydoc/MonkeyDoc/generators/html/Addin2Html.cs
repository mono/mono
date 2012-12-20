using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Xsl;
using System.Xml.XPath;
using System.Collections.Generic;

namespace Monodoc.Generators.Html
{
	public class Addin2Html : IHtmlExporter
	{
		public string CssCode {
			get {
				return string.Empty;
			}
		}

		public string Export (Stream stream, Dictionary<string, string> extraArgs)
		{
			using (var reader = new StreamReader (stream))
				return Htmlize (GetAddin (reader, extraArgs["AddinID"]),
				                extraArgs["show"],
				                extraArgs["AddinID"],
				                extraArgs["FileID"],
				                extraArgs["NodeID"]);
		}

		public string Export (string input, Dictionary<string, string> extraArgs)
		{
			return Htmlize (GetAddin (new StringReader (input), extraArgs["AddinID"]),
			                extraArgs["show"],
			                extraArgs["AddinID"],
			                extraArgs["FileID"],
			                extraArgs["NodeID"]);
		}

		XmlElement GetAddin (TextReader reader, string addinId)
		{
			XmlDocument doc = new XmlDocument ();
			doc.Load (reader);
			XmlElement addin = (XmlElement) doc.SelectSingleNode ("Addins/Addin[@fullId='" + addinId + "']");
			return addin != null ? addin : null;
		}

		public string Htmlize (XmlElement addin, string urlType, string addinId, string fileId, string path)
		{
			if (urlType == Monodoc.Providers.AddinsHelpSource.AddinPrefix)
				return GetAddinTextFromUrl (addin, addinId, fileId);
			else if (urlType == Monodoc.Providers.AddinsHelpSource.ExtensionPrefix)
				return GetExtensionTextFromUrl (addin, addinId, fileId, path);
			else if (urlType == Monodoc.Providers.AddinsHelpSource.ExtensionNodePrefix)
				return GetExtensionNodeTextFromUrl (addin, addinId, fileId, path);

			return null;
		}

		protected string GetAddinTextFromUrl (XmlElement addin, string addinId, string fileId)
		{
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
		
		protected string GetExtensionTextFromUrl (XmlElement addin, string addinId, string fileId, string path)
		{
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
		
		protected string GetExtensionNodeTextFromUrl (XmlElement addin, string addinId, string fileId, string nodeId)
		{
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
	}
}
