//
// error-provider.cs
//
// Author:
//   Ben Maurer (bmaurer@users.sourceforge.net)
//
// (C) 2003 Ben Maurer
//

using System;
using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Monodoc {

#region Data Model
	public class ErrorDocumentation {
		public string ErrorName;
		public ErrorDetails Details;
		public StringCollection Examples = new StringCollection ();
		
		public ErrorDocumentation () {}
		public ErrorDocumentation (string ErrorName)
		{
			this.ErrorName = ErrorName;
		}
		
		public override string ToString ()
		{
			StringWriter w = new StringWriter ();
			
			w.WriteLine ("Error Name: {0}", ErrorName);
			w.WriteLine ("Details: \n", Details);
			
			w.WriteLine ("Examples: ");
			foreach (string s in Examples) {
				w.WriteLine (s);
				w.WriteLine ();
			}
			
			return w.ToString ();
		}
		
		public string RenderAsHtml ()
		{
			StringWriter sw = new StringWriter ();
			XmlWriter w = new XmlTextWriter (sw);
			if (HelpSource.use_css)
				w.WriteRaw ("<div class=\"header\" id=\"error_ref\">" +
				"	<div class=\"subtitle\">Compiler Error Reference</div> " +
				"	<div class=\"title\">Error " + ErrorName + " </div></div>");
			else
			w.WriteRaw (@"
				
				<table width='100%'>
					<tr bgcolor='#b0c4de'><td>
					<i>Compiler Error Reference</i>
					<h3>Error " + ErrorName + @"</h2>
					</td></tr>
				</table><br />");
			
			
			if (Details != null) {
				if (HelpSource.use_css)
					w.WriteRaw ("<div class=\"summary\">Summary</div>");
				else
				w.WriteRaw (@"<h3>Summary</h3>");
				Details.Summary.WriteTo (w);
				
				if (HelpSource.use_css)
					w.WriteRaw ("<div class=\"details\">Details</div>");
				else
				w.WriteRaw (@"<h3>Details</h3>");

				Details.Details.WriteTo (w);
			}
			
			foreach (string xmp in Examples) {
				if (HelpSource.use_css)
					w.WriteRaw ("<div class=\"code_example\">" +
						"<div class=\"code_ex_title\">Example</div>");
				else
				w.WriteRaw (@"<table bgcolor='#f5f5dd' border='1'>
						<tr><td><b><font size='-1'>Example</font></b></td></tr>
						<tr><td><font size='-1'><pre>");
				w.WriteRaw (Mono.Utilities.Colorizer.Colorize (xmp, "c#"));
				if (HelpSource.use_css)
					w.WriteRaw ("</div>");
				else
				w.WriteRaw (@"</pre></font></td></tr></table>");
			}
			
			w.Close ();
			
			return sw.ToString ();
		}
	}
	
	public class ErrorDetails {
		public XmlNode Summary;
		public XmlNode Details;
			
		public override string ToString ()
		{
			StringWriter w = new StringWriter ();

			w.WriteLine ("Summary: \n {0}", Summary.OuterXml);
			w.WriteLine ("Details: \n {0}", Summary.OuterXml);
			
			return w.ToString ();
		}
	}
	
	public class ErrorProviderConfig {
		public string FilesPath;
		public string Match;
		public int ErrorNumSubstringStart;
		public int ErrorNumSubstringLength;
		public string FriendlyFormatString;

		public override string ToString ()
		{
			StringWriter w = new StringWriter ();
			
			w.WriteLine ("FilesPath: {0}", FilesPath);
			w.WriteLine ("Match: {0}", Match);
			w.WriteLine ("Error Number Substring: {0} Length:{1}", ErrorNumSubstringStart, ErrorNumSubstringLength);
			w.WriteLine ("FriendlyFormatString: {0}", FriendlyFormatString);
			
			return w.ToString ();
		}
		
		public Hashtable Compile (HelpSource hs)
		{
			string [] files = Directory.GetFiles (FilesPath, Match);
			Hashtable ret = new Hashtable ();
			
			foreach (string s in files) {
				ErrorDocumentation d;
				
				hs.Message (TraceLevel.Info, s);

				int errorNum = 0;

				try {
					errorNum = int.Parse (Path.GetFileName (s).Substring (ErrorNumSubstringStart, ErrorNumSubstringLength));
				} catch {
					hs.Message (TraceLevel.Info, "Ignoring file {0}", s);
				}
				
				string errorName = String.Format (FriendlyFormatString, errorNum);
				
				d = (ErrorDocumentation)ret [errorName];
				if (d == null)
					ret [errorName] = d = new ErrorDocumentation (errorName);
				
				if (d.Details == null) {
					string xmlFile = Path.ChangeExtension (s, "xml");
					hs.Message (TraceLevel.Verbose, xmlFile);
					if (File.Exists (xmlFile)) {
						XmlSerializer cfgRdr = new XmlSerializer (typeof (ErrorDetails));
						d.Details = (ErrorDetails)cfgRdr.Deserialize (new XmlTextReader (xmlFile));
					}
				}
				// Encoding is same as used in MCS, so we will be able to do all those files
				using (StreamReader reader = new StreamReader (s, Encoding.GetEncoding (28591))) {
					d.Examples.Add (reader.ReadToEnd ());
				}
			}
			
			return ret;
		}
	}
	
#endregion

#region Monodoc Rendering
	public class ErrorProvider : Provider {
		ErrorProviderConfig config;
		
		public ErrorProvider (string configFile)
		{
			config = ReadConfig (configFile);
		}
		
		public static ErrorProviderConfig ReadConfig (string file)
		{
			XmlSerializer cfgRdr = new XmlSerializer (typeof (ErrorProviderConfig));
			ErrorProviderConfig ret = (ErrorProviderConfig)cfgRdr.Deserialize (new XmlTextReader (file));
			// handle path rel to the config file
			ret.FilesPath = Path.Combine (Path.GetDirectoryName (file), ret.FilesPath);
			return ret;
		}
	
		public override void PopulateTree (Tree tree)
		{
			// everything is done in CloseTree so we can pack
		}
	
		public override void CloseTree (HelpSource hs, Tree tree)
		{
			Hashtable entries = config.Compile (hs);
			MemoryStream ms = new MemoryStream ();
			XmlSerializer writer = new XmlSerializer (typeof (ErrorDocumentation));
			
			foreach (DictionaryEntry de in entries) {
				ErrorDocumentation d = (ErrorDocumentation)de.Value;
				string s = (string)de.Key;

				tree.LookupNode (s, "error:" + s);
				
				writer.Serialize (ms, d);
				ms.Position = 0;
				hs.PackStream (ms, s);
				ms.SetLength (0);
			}
			
			tree.Sort ();
		}
	}
	
	
	public class ErrorHelpSource : HelpSource {
		
		public ErrorHelpSource (string base_file, bool create) : base (base_file, create) {}
	
		public override string InlineCss {
			get {return base.InlineCss + css_error_code;}
		}

		public override string GetText (string url, out Node match_node)
		{
			match_node = null;

			string c = GetCachedText (url);
			if (c != null)
				return c;
			
			if (url == "root:")
				if (HelpSource.use_css)
					return BuildHtml (css_error_code, "<div id=\"error_ref\" class=\"header\"><div class=\"title\">Compiler Error Reference</div></div>");
				else
					return BuildHtml (String.Empty, "<table width=\"100%\" bgcolor=\"#b0c4de\" cellpadding=\"5\"><tr><td><h3>Compiler Error Reference</h3></tr></td></table>");
			
			if (!url.StartsWith ("error:"))
				return null;
				
			foreach (Node n in Tree.Nodes) {
				if (n.Element != url) continue;
					
				match_node = n;
				XmlSerializer reader = new XmlSerializer (typeof (ErrorDocumentation));
				ErrorDocumentation d = (ErrorDocumentation)reader.Deserialize (
					GetHelpStream (n.Element.Substring (6))
				);
				return BuildHtml (css_error_code, d.RenderAsHtml ());
			}
			
			return null;
		}
		
		public override void PopulateIndex (IndexMaker index_maker)
		{
			foreach (Node n in Tree.Nodes)
				index_maker.Add (n.Caption, n.Caption, n.Element);
		}

		public static string css_error_code = @"
								 #error_ref { 
								    background: #debcb0; 
								    border: 2px solid #782609; 
								 }
								 div.summary {
									 font-size: 110%;
									 font-weight: bolder;
								 }
								 div.details {
									 font-size: 110%;
									 font-weight: bolder;
								 }
								 div.code_example {
									background: #f5f5dd;
									border: 1px solid #cdcd82;
									border: 1px solid black;
									padding-left: 1em;
									padding-bottom: 1em;
									margin-top: 1em;
									white-space: pre;
									margin-bottom: 1em;
								 }
								 div.code_ex_title {
									position: relative;
									top: -1em;
									left: 30%;
									background: #cdcd82;
									border: 1px solid black;
									color: black;
									font-size: 65%;
									text-transform: uppercase;
									width: 40%;
									padding: 0.3em;
									text-align: center;
								 }";
	}
#endregion
	
}
