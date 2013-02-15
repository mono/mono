//
// error-provider.cs
//
// Author:
//   Ben Maurer (bmaurer@users.sourceforge.net)
//
// (C) 2003 Ben Maurer
// Copyright 2003-2011 Novell
// Copyright 2011 Xamarin Inc
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.Linq;
using Lucene.Net.Index;
using Lucene.Net.Documents;

namespace Monodoc.Providers
{
	public class ErrorProviderConfig
	{
		public string FilesPath;
		public string Match;
		public int ErrorNumSubstringStart;
		public int ErrorNumSubstringLength;
		public string FriendlyFormatString;

		public override string ToString ()
		{
			var sb = new StringBuilder ();
			var w = new StringWriter (sb);
			
			w.WriteLine ("FilesPath: {0}", FilesPath);
			w.WriteLine ("Match: {0}", Match);
			w.WriteLine ("Error Number Substring: {0} Length:{1}", ErrorNumSubstringStart, ErrorNumSubstringLength);
			w.WriteLine ("FriendlyFormatString: {0}", FriendlyFormatString);
			
			return w.ToString ();
		}
		
		public Dictionary<string, ErrorDocumentation> Compile (HelpSource hs)
		{
			string[] files = Directory.GetFiles (FilesPath, Match);
			var ret = new Dictionary<string, ErrorDocumentation> ();
			
			foreach (string s in files) {
				ErrorDocumentation d;
				int errorNum = 0;

				try {
					errorNum = int.Parse (Path.GetFileName (s).Substring (ErrorNumSubstringStart, ErrorNumSubstringLength));
				} catch {
					Console.WriteLine ("Ignoring file {0}", s);
				}
				
				string errorName = String.Format (FriendlyFormatString, errorNum);
				
				if (!ret.TryGetValue (errorName, out d))
					ret[errorName] = d = new ErrorDocumentation (errorName);

				if (d.Details == null) {
					string xmlFile = Path.ChangeExtension (s, "xml");
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

	public class ErrorDocumentation
	{
		public string ErrorName;
		public ErrorDetails Details;
		public List<string> Examples = new List<string> ();
		
		public ErrorDocumentation () {}
		public ErrorDocumentation (string ErrorName)
		{
			this.ErrorName = ErrorName;
		}
	}
	
	public class ErrorDetails
	{
		public XmlNode Summary;
		public XmlNode Details;
	}

	public class ErrorProvider : Provider
	{
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
			var entries = config.Compile (hs);
			MemoryStream ms = new MemoryStream ();
			XmlSerializer writer = new XmlSerializer (typeof (ErrorDocumentation));
			
			foreach (var de in entries) {
				ErrorDocumentation d = de.Value;
				string s = de.Key;

				tree.RootNode.GetOrCreateNode (s, "error:" + s);
				
				writer.Serialize (ms, d);
				ms.Position = 0;
				hs.Storage.Store (s, ms);
				ms.SetLength (0);
			}
			
			tree.RootNode.Sort ();
		}
	}
	
	public class ErrorHelpSource : HelpSource
	{		
		public ErrorHelpSource (string base_file, bool create) : base (base_file, create)
		{
		}

		public override string GetText (string id)
		{
			return TreeDumper.ExportToTocXml (Tree.RootNode, "Compiler Error Reference", "In this section:");
		}
		
		protected override string UriPrefix {
			get {
				return "error:";
			}
		}

		public override bool IsGeneratedContent (string id)
		{
			return id == "root:";
		}

		public override DocumentType GetDocumentTypeForId (string id)
		{
			return id == "root:" ? DocumentType.TocXml : DocumentType.ErrorXml;
		}

		public override string GetInternalIdForUrl (string url, out Node node, out Dictionary<string, string> context)
		{
			var result = base.GetInternalIdForUrl (url, out node, out context);
			return result.ToLower ();
		}
		
		public override void PopulateIndex (IndexMaker index_maker)
		{
			foreach (Node n in Tree.RootNode.ChildNodes)
				index_maker.Add (n.Caption, n.Caption, n.Element);
		}

		public override void PopulateSearchableIndex (IndexWriter writer) 
		{
			foreach (Node n in Tree.RootNode.ChildNodes) {
				XmlSerializer reader = new XmlSerializer (typeof (ErrorDocumentation));
				ErrorDocumentation d = (ErrorDocumentation)reader.Deserialize (GetHelpStream (n.Element.Substring (6)));
				SearchableDocument doc = new SearchableDocument ();
				doc.Title = d.ErrorName;
				doc.Url = n.Element;
				doc.Text = d.Details != null ? d.Details.ToString () : string.Empty;
				doc.Examples = d.Examples.Cast<string> ().Aggregate ((e1, e2) => e1 + Environment.NewLine + e2);
				doc.HotText = d.ErrorName;
				writer.AddDocument (doc.LuceneDoc);
			}
		}
	}
}
