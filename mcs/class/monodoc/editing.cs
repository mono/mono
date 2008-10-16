//
// editing.cs
//
// Author:
//   Ben Maurer (bmaurer@users.sourceforge.net)
//
// (C) 2003 Ben Maurer
//

using System;
using System.Collections;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.XPath;
using System.Web;

namespace Monodoc {
	public class EditingUtils {
		
		public static string FormatEditUri (string document_identifier, string xpath)
		{
			return String.Format ("edit:{0}@{1}", HttpUtility.UrlEncode (document_identifier),
				HttpUtility.UrlEncode (xpath));
		}
		
		public static string GetXPath (XPathNavigator n)
		{
			switch (n.NodeType) {
				case XPathNodeType.Root: return "/";
				case XPathNodeType.Attribute: {
					string ret = "@" + n.Name;
					n.MoveToParent ();
					string s = GetXPath (n);
					return s + (s == "/" ? "" : "/") + ret;
				}

				case XPathNodeType.Element: {
					string ret = n.Name;
					int i = 1;
					while (n.MoveToPrevious ()) {
						if (n.NodeType == XPathNodeType.Element && n.Name == ret)
							i++;
					}
					ret += "[" + i + "]";
					if (n.MoveToParent ()) {
						string s = GetXPath (n);
						return s + (s == "/" ? "" : "/") + ret;
					}
				}
				break;
			}
			throw new Exception ("node type not supported for editing");
			
		}
		
		public static XmlNode GetNodeFromUrl (string url, RootTree tree)
		{
			Console.WriteLine ("Url is: {0}", url);
			string [] uSplit = ParseEditUrl (url);
			Console.WriteLine ("Results are: {0}\n{1}\n{2}", uSplit [0], uSplit [1], uSplit [2]);
			
			string xp = uSplit [2];
			string id =  uSplit [1];
			
			XmlDocument d;
			
			if (uSplit[0].StartsWith("monodoc:///")) {
				int prov = int.Parse (uSplit [0].Substring("monodoc:///".Length));
				d = tree.GetHelpSourceFromId (prov).GetHelpXmlWithChanges (id);
			} else if (uSplit[0].StartsWith("file:")) {
				d = new XmlDocument();
				d.PreserveWhitespace = true;
				d.Load(uSplit[0].Substring(5));
			} else {
				throw new NotImplementedException("Don't know how to load " + url); 
			}			
			
			return d.SelectSingleNode (xp);
				
		}
		
		public static void SaveChange (string url, RootTree tree, XmlNode node, string node_url)
		{
			string [] uSplit = ParseEditUrl (url);
		
			string xp = uSplit [2];
			string id =  uSplit [1];
						
			if (uSplit[0].StartsWith("monodoc:///")) {
				int prov = int.Parse (uSplit [0].Substring("monodoc:///".Length));
				HelpSource hs = tree.GetHelpSourceFromId (prov);
				
				changes.AddChange (hs.Name, hs.GetRealPath (id), xp, node, node_url);
				changes.Save ();
			} else if (uSplit[0].StartsWith("file:")) {
				uSplit[0] = uSplit[0].Substring(5);
				
				XmlDocument d = new XmlDocument();
				d.PreserveWhitespace = true;
				d.Load(uSplit[0]);
				
				XmlNode original = d.SelectSingleNode(xp);
				original.ParentNode.ReplaceChild(d.ImportNode(node, true), original);
				
				d.Save(uSplit[0]);
			} else {				
				throw new NotImplementedException("Don't know how to save to " + url); 
			}
		}

		public static void RemoveChange (string url, RootTree tree)
		{
			string [] uSplit = ParseEditUrl (url);
		
			string xp = uSplit [2];
			string id = uSplit [1];
						
			if (uSplit[0].StartsWith("monodoc:///")) {
				int prov = int.Parse (uSplit [0].Substring("monodoc:///".Length));
				HelpSource hs = tree.GetHelpSourceFromId (prov);
				
				changes.RemoveChange (hs.Name, hs.GetRealPath (id), xp);
				changes.Save ();
			} else if (uSplit[0].StartsWith("file:")) {
				//TODO: Not implemented
			}
		}
		
		public static void RenderEditPreview (string url, RootTree tree, XmlNode new_node, XmlWriter w)
		{
			string [] uSplit = ParseEditUrl (url);
		
			if (uSplit[0].StartsWith("monodoc:///")) {
				int prov = int.Parse (uSplit [0].Substring("monodoc:///".Length));
				HelpSource hs = tree.GetHelpSourceFromId (prov);
				hs.RenderPreviewDocs (new_node, w);
			} else {
				foreach (HelpSource hs in tree.HelpSources) {
					if (hs is EcmaUncompiledHelpSource) {
						// It doesn't matter which EcmaHelpSource is chosen.
						hs.RenderPreviewDocs (new_node, w);
						break;
					}
				}				
			}
		}
		
		public static string [] ParseEditUrl (string url)
		{
			if (!url.StartsWith ("edit:"))
				throw new Exception ("wtf");
			
			string [] parts = url.Split ('@');
			if (parts.Length != 2)
				throw new Exception (String.Format ("invalid editing url {0}", parts.Length));
			
			string xp = HttpUtility.UrlDecode (parts [1]);
			parts = HttpUtility.UrlDecode (parts [0]).Substring ("edit:".Length).Split ('@');
			if (parts.Length == 1) {
				string p = parts[0];
				parts = new string[2];
				parts[0] = p;
				parts[1] = "";
			}
			
			return new string [] {parts [0], parts [1], xp};
		}
		
		public static void AccountForChanges (XmlDocument d, string doc_set, string real_file)
		{
			try {
				FileChangeset fcs = changes.GetChangeset (doc_set, real_file);
				if (fcs == null)
					return;
				
				foreach (Change c in fcs.Changes) {
					// Filter out old changes
					if (c.FromVersion != RootTree.MonodocVersion)
						continue;
					
					XmlNode old = d.SelectSingleNode (c.XPath);
					if (old != null)
						old.ParentNode.ReplaceChild (d.ImportNode (c.NewNode, true), old);
				}
			} catch {
				return;
			}
		}
	
		public static GlobalChangeset changes = GlobalChangeset.Load ();

		static public GlobalChangeset GetChangesFrom (int starting_serial_id)
		{
			return changes.GetFrom (starting_serial_id);
		}
	}

#region Data Model
	public class GlobalChangeset {

		public static XmlSerializer serializer = new XmlSerializer (typeof (GlobalChangeset));
		static string changeset_file = Path.Combine (SettingsHandler.Path, "changeset.xml");
		static string changeset_backup_file = Path.Combine (SettingsHandler.Path, "changeset.xml~");
	
		public static GlobalChangeset Load ()
		{
			try {
				if (File.Exists (changeset_file))
					return LoadFromFile (changeset_file);
			} catch {}
			
			return new GlobalChangeset ();
		}
		
		public static GlobalChangeset LoadFromFile (string fileName)
		{
			using (Stream s = File.OpenRead (fileName)) {
				return (GlobalChangeset) serializer.Deserialize (s);
			}
		}			
		
		public void Save ()
		{
			SettingsHandler.EnsureSettingsDirectory ();

			try {    
				if (File.Exists(changeset_file))  // create backup copy
					File.Copy (changeset_file, changeset_backup_file, true);
           
				using (FileStream fs = File.Create (changeset_file)){
					serializer.Serialize (fs, this);
				}
			} catch (Exception e) {
				Console.WriteLine ("Error while saving changes. " + e);
				if (File.Exists(changeset_backup_file))  // if saving fails then use backup if we have one				
					File.Copy (changeset_backup_file, changeset_file, true);
				else
					File.Delete (changeset_file);   // if no backup, delete invalid changeset 
			}
		}
		
		static void VerifyDirectoryExists (DirectoryInfo d) {
			if (d.Exists)
				return;

			VerifyDirectoryExists (d.Parent);
			d.Create ();
		}

		[XmlElement ("DocSetChangeset", typeof (DocSetChangeset))]
		public ArrayList DocSetChangesets = new ArrayList ();

		public FileChangeset GetChangeset (string doc_set, string real_file)
		{
			foreach (DocSetChangeset dscs in DocSetChangesets) {
				if (dscs.DocSet != doc_set) 
					continue;
			
				foreach (FileChangeset fcs in dscs.FileChangesets) {
					if (fcs.RealFile == real_file)
						return fcs;
				}
			}
			
			return null;
		}

		public int Count {
			get {
				int count = 0;
				
				foreach (DocSetChangeset dscs in DocSetChangesets){
					foreach (FileChangeset fcs in dscs.FileChangesets){
						count += fcs.Changes.Count;
					}
				}

				return count;
			}
		}

		Change NewChange (string xpath, XmlNode new_node, string node_url)
		{
			Change new_change = new Change ();
			new_change.XPath = xpath;
			new_change.NewNode = new_node;
			new_change.NodeUrl = node_url;

			Console.WriteLine ("New serial:" + SettingsHandler.Settings.SerialNumber);
			new_change.Serial = SettingsHandler.Settings.SerialNumber;

			return new_change;
		}
		
		public void AddChange (string doc_set, string real_file, string xpath, XmlNode new_node, string node_url)
		{
			FileChangeset new_file_change_set;
			Change new_change = NewChange (xpath, new_node, node_url);
			
			if (real_file == null)
				throw new Exception ("Could not find real_file. Please talk to Miguel or Ben about this");
			
			foreach (DocSetChangeset dscs in DocSetChangesets) {
				if (dscs.DocSet != doc_set) 
					continue;

				foreach (FileChangeset fcs in dscs.FileChangesets) {
					if (fcs.RealFile != real_file)
						continue;
					
					foreach (Change c in fcs.Changes) {
						if (c.XPath == xpath) {
							c.NewNode = new_node;
							c.Serial = SettingsHandler.Settings.SerialNumber;
							return;
						}
					}

					fcs.Changes.Add (new_change);
					return;
					
				}
				
				new_file_change_set = new FileChangeset ();
				new_file_change_set.RealFile = real_file;
				new_file_change_set.Changes.Add (new_change);
				dscs.FileChangesets.Add (new_file_change_set);
				return;
					
			}
			
			DocSetChangeset new_dcs = new DocSetChangeset ();
			new_dcs.DocSet = doc_set;
			
			new_file_change_set = new FileChangeset ();
			new_file_change_set.RealFile = real_file;
			
			new_file_change_set.Changes.Add (new_change);
			new_dcs.FileChangesets.Add (new_file_change_set);
			DocSetChangesets.Add (new_dcs);
		}

		public void RemoveChange (string doc_set, string real_file, string xpath)
		{
			if (real_file == null)
				throw new Exception ("Could not find real_file. Please talk to Miguel or Ben about this");
			
			for (int i = 0; i < DocSetChangesets.Count; i++) {
				DocSetChangeset dscs = DocSetChangesets [i] as DocSetChangeset;
				if (dscs.DocSet != doc_set) 
					continue;

				for (int j = 0; j < dscs.FileChangesets.Count; j++) {
					FileChangeset fcs = dscs.FileChangesets [j] as FileChangeset;
					if (fcs.RealFile != real_file)
						continue;

					for (int k = 0; k < fcs.Changes.Count; k++) {
						Change c = fcs.Changes [k] as Change;
						if (c.XPath == xpath) {
							fcs.Changes.Remove (c);
							break;
						}
					}
					if (fcs.Changes.Count == 0)
						dscs.FileChangesets.Remove (fcs);
				}

				if (dscs.FileChangesets.Count == 0)
					DocSetChangesets.Remove (dscs);
			}
		}

		public GlobalChangeset GetFrom (int starting_serial_id)
		{
			GlobalChangeset s = null;
			
			foreach (DocSetChangeset dscs in DocSetChangesets){
				object o = dscs.GetFrom (starting_serial_id);
				if (o == null)
					continue;
				if (s == null)
					s = new GlobalChangeset ();
				s.DocSetChangesets.Add (o);
			}
			return s;
		}
	}
	
	public class DocSetChangeset {
		[XmlAttribute] public string DocSet;
		
		[XmlElement ("FileChangeset", typeof (FileChangeset))]
		public ArrayList FileChangesets = new ArrayList ();

		public DocSetChangeset GetFrom (int starting_serial_id)
		{
			DocSetChangeset dsc = null;
			
			foreach (FileChangeset fcs in FileChangesets){
				object o = fcs.GetFrom (starting_serial_id);
				if (o == null)
					continue;
				if (dsc == null){
					dsc = new DocSetChangeset ();
					dsc.DocSet = DocSet;
				}
				dsc.FileChangesets.Add (o);
			}
			return dsc;
		}
	}
	
	public class FileChangeset {
		[XmlAttribute] public string RealFile;
		
		[XmlElement ("Change", typeof (Change))]
		public ArrayList Changes = new ArrayList ();

		public FileChangeset GetFrom (int starting_serial_id)
		{
			FileChangeset fcs = null;

			foreach (Change c in Changes){
				if (c.Serial < starting_serial_id)
					continue;
				if (fcs == null){
					fcs = new FileChangeset ();
					fcs.RealFile = RealFile;
				}
				fcs.Changes.Add (c);
			}
			return fcs;
		}
	}
	
	public class Change {
		[XmlAttribute] public string XPath;
		[XmlAttribute] public int FromVersion = RootTree.MonodocVersion;
		[XmlAttribute] public string NodeUrl;
		
		public XmlNode NewNode;

		public int Serial;

		bool applied = false;
		
		//
		// These are not a property, because we dont want them serialized;
		// Only used by the Admin Client.
		//
		public bool Applied ()
		{
			return applied;
		}

		public void SetApplied (bool value)
		{
			applied = value;
		}
	}
#endregion
	
	public class EditMerger {
		GlobalChangeset changeset;
		ArrayList targetDirs;
		
		public EditMerger (GlobalChangeset changeset, ArrayList targetDirs)
		{
			this.changeset = changeset;
			this.targetDirs = targetDirs;
		}
		
		public void Merge ()
		{
			foreach (DocSetChangeset dsc in changeset.DocSetChangesets) {
				bool merged = false;
				foreach (string path in targetDirs) {
					if (File.Exists (Path.Combine (path, dsc.DocSet + ".source"))) {
						Merge (dsc, path);
						merged = true;
						break;
					}
				}
				if (!merged) Console.WriteLine ("Could not merge docset {0}", dsc.DocSet);
			}
		}
		
		void Merge (DocSetChangeset dsc, string path)
		{
			Console.WriteLine ("Merging changes in {0} ({1})", dsc.DocSet, path);
			
			foreach (FileChangeset fcs in dsc.FileChangesets) {
				if (File.Exists (Path.Combine (path, fcs.RealFile)))
					Merge (fcs, path);
				else
					Console.WriteLine ("\tCould not find file {0}", Path.Combine (path, fcs.RealFile));
			}
		}
		
		void Merge (FileChangeset fcs, string path)
		{
			XmlDocument d = new XmlDocument ();
			d.Load (Path.Combine (path, fcs.RealFile));
			
			foreach (Change c in fcs.Changes) {
				XmlNode old = d.SelectSingleNode (c.XPath);
				if (old != null)
					old.ParentNode.ReplaceChild (d.ImportNode (c.NewNode, true), old);
			}
			
			d.Save (Path.Combine (path, fcs.RealFile));
		}
	}
}
