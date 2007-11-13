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
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.XPath;

namespace Mono.MonoConfig
{
	class DefaultConfigFile
	{
		string name;
		string fileName;
		FeatureTarget target;
		Section sections;

		public string Name {
			get { return name; }
		}

		public string FileName {
			get {
				if (!String.IsNullOrEmpty (fileName))
					return fileName;
				return Name;
			}
		}
		
		public FeatureTarget Target {
			get { return target; }
		}

		public Section Sections {
			get { return sections; }
		}

		public DefaultConfigFile (string name, string fileName, FeatureTarget target, Section sections)
		{
			this.name = name;
			this.fileName = fileName;
			this.target = target;
			this.sections = sections;
		}
	}

	public delegate void OverwriteFileEventHandler (object sender, OverwriteFileEventArgs e);

	public sealed class OverwriteFileEventArgs : System.EventArgs
	{
		string name;
		string path;
		FeatureTarget target;
		bool overwrite;
		
		public string Name {
			get { return name; }
		}

		public string Path {
			get { return path; }
		}

		public FeatureTarget Target {
			get { return target; }
		}

		public bool Overwrite {
			get { return overwrite; }
			set { overwrite = value; }
		}
		
		public OverwriteFileEventArgs (string name, string path, FeatureTarget target, bool overwrite)
		{
			this.name = name;
			this.path = path;
			this.target = target;
			this.overwrite = overwrite;
		}
	}
	
	public class DefaultConfigFileNodeHandler : IDocumentNodeHandler, IDefaultConfigFileContainer, IStorageConsumer
	{
		string name;
		string fileName;
		FeatureTarget target;
		Section sections;
		Dictionary <string, DefaultConfigFile> storage;

		public event OverwriteFileEventHandler OverwriteFile;
		
		public void ReadConfiguration (XPathNavigator nav)
		{
			name = Helpers.GetRequiredNonEmptyAttribute (nav, "name");
			target = Helpers.ConvertEnum <FeatureTarget> (Helpers.GetRequiredNonEmptyAttribute (nav, "target"), "target");
			fileName = Helpers.GetOptionalAttribute (nav, "fileName");
			
			if (String.IsNullOrEmpty (fileName))
				fileName = name;
			
			sections = new Section ();
			Helpers.BuildSectionTree (nav.Select ("./section[string-length (@name) > 0]"), sections);
		}
		
		public void StoreConfiguration ()
		{
			AssertStorage ();

			DefaultConfigFile dcf = new DefaultConfigFile (name, fileName, target, sections);
			if (storage.ContainsKey (name))
				storage [name] = dcf;
			else
				storage.Add (name, dcf);

			name = null;
			fileName = null;
			sections = null;
		}

		public void SetStorage (object storage)
		{
			this.storage = storage as Dictionary <string, DefaultConfigFile>;
			if (this.storage == null)
				throw new ApplicationException ("Invalid storage type");
		}

		public ICollection <string> DefaultConfigFiles {
			get {
				AssertStorage ();
				
				if (storage.Count == 0)
					return null;

				List <string> ret = new List <string>(storage.Count);
				DefaultConfigFile dcf;
				
				foreach (KeyValuePair <string, DefaultConfigFile> kvp in storage) {
					dcf = kvp.Value;
					ret.Add (String.Format ("{0} (Target: {1}; Output file: {2})",
								kvp.Key, dcf.Target, dcf.FileName));
				}

				return ret;
			}
		}
		
		public bool HasDefaultConfigFile (string name, FeatureTarget target)
		{
			AssertStorage ();

			if (storage.ContainsKey (name)) {
				DefaultConfigFile dcf = storage [name];
				if (dcf == null)
					return false;

				if (target != FeatureTarget.Any && dcf.Target != target)
					return false;
				
				return true;
			}
		
			return false;
		}
		
		public void WriteDefaultConfigFile (string name, FeatureTarget target, string path, IDefaultContainer[] defaults)
		{
			AssertStorage ();

			DefaultConfigFile dcf;
			if (!storage.ContainsKey (name) || (dcf = storage [name]) == null)
				throw new ApplicationException (
					String.Format ("Definition of the '{0}' default config file not found.", name));

			if (target != FeatureTarget.Any && dcf.Target != target)
				throw new ApplicationException (
					String.Format ("Config file '{0}' can be generated only for the '{1}' target",
						       name, target));

			string targetFile = Path.Combine (path, dcf.FileName);
			if (File.Exists (targetFile)) {
				OverwriteFileEventArgs args = new OverwriteFileEventArgs (
					dcf.FileName,
					path,
					target,
					true
				);

				OnOverwriteFile (args);
				if (!args.Overwrite)
					return;
			}

			try {
				if (!Directory.Exists (path))
					Directory.CreateDirectory (path);
			} catch (Exception ex) {
				throw new ApplicationException (
					String.Format ("Could not create directory '{0}'", path),
					ex);
			}

			XmlDocument doc = new XmlDocument ();
			PopulateDocument (name, target, doc, dcf, defaults);
			Helpers.SaveXml (doc, targetFile);
		}

		void OnOverwriteFile (OverwriteFileEventArgs args)
		{
			if (OverwriteFile == null)
				return;

			OverwriteFile (this, args);
		}
		
		void PopulateDocument (string name, FeatureTarget target, XmlDocument doc, DefaultConfigFile dcf,
				       IDefaultContainer[] defaults)
		{
			List <Section> children = dcf.Sections != null ? dcf.Sections.Children : null;
			if (children == null || children.Count == 0)
				return;

			PopulateDocument (name, target, doc, doc, defaults, children);
		}

		void PopulateDocument (string name, FeatureTarget target, XmlDocument doc, XmlNode parent,
				       IDefaultContainer[] defaults, List <Section> children)
		{
			if (defaults == null || defaults.Length == 0)
				return;
			
			XmlNode node;
			XmlDocument tmp;
			
			foreach (Section s in children) {
				tmp = Helpers.FindDefault (defaults, s.DefaultBlockName, target);
				if (tmp == null)
					continue;
				
				node = doc.ImportNode (tmp.DocumentElement.FirstChild, true);
				try {
					PopulateDocument (name, target, doc, node, defaults, s.Children);
				} catch (Exception ex) {
					throw new ApplicationException (
						String.Format ("Error building default config file '{0}'", name),
						ex);
				}

				parent.AppendChild (node);
			}
		}
		
		void AssertStorage ()
		{
			if (storage == null)
				throw new ApplicationException ("No storage attached");
		}
	}
}
