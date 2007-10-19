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
	public class FeatureNodeHandler : IDocumentNodeHandler, IStorageConsumer, IFeatureGenerator
	{
		string name;
		FeatureTarget target;
		List <FeatureBlock> blocks;
		Dictionary <string, List <FeatureBlock>> storage;
		
		public FeatureNodeHandler ()
		{
			blocks = new List <FeatureBlock> ();
		}
		
		public void ReadConfiguration (XPathNavigator nav)
		{
			name = Helpers.GetRequiredNonEmptyAttribute (nav, "name");
			target = Helpers.ConvertTarget (Helpers.GetRequiredNonEmptyAttribute (nav, "target"));

			XPathNodeIterator iter = nav.Select ("blocks/block[string-length (@name) > 0]");
			while (iter.MoveNext ())
				blocks.Add (new FeatureBlock (iter.Current, target));
		}
		
		public void StoreConfiguration ()
		{
			AssertStorage ();

			List <FeatureBlock> blocksClone = new List <FeatureBlock> (blocks.Count);
			blocksClone.AddRange (blocks);
			
			if (storage.ContainsKey (name))
				storage [name] = blocksClone; // allow for silent override
			else
				storage.Add (name, blocksClone);
				
			blocks.Clear ();
		}

		public void SetStorage (object storage)
		{
			this.storage = storage as Dictionary <string, List <FeatureBlock>>;
			if (this.storage == null)
				throw new ApplicationException ("Invalid storage type");
		}

		public ICollection <string> Features {
			get {
				AssertStorage ();

				if (storage.Count == 0)
					return null;

				List <string> ret = new List <string> (storage.Count);
				List <FeatureBlock> lfb;
				
				foreach (KeyValuePair <string, List <FeatureBlock>> kvp in storage) {
					lfb = kvp.Value;
					if (lfb.Count == 0)
						continue;
					
					ret.Add (String.Format ("{0} (Target: {1})", kvp.Key, lfb [0].Target));
				}

				return ret;
			}
		}
		
		public bool HasFeature (string featureName)
		{
			AssertStorage ();

			if (!storage.ContainsKey (featureName))
				return false;
			
			List <FeatureBlock> blocks = storage [featureName];
			if (blocks == null || blocks.Count == 0)
				return false;

			return true;
		}

		public void AddFeature (string configFilePath, string featureName, FeatureTarget target,
					IDefaultContainer[] defaults, IConfigBlockContainer[] configBlocks)
		{
			AssertStorage ();

			if (!storage.ContainsKey (featureName))
				throw new ApplicationException (String.Format ("Missing definition of feature '{0}'", featureName));
			
			List <FeatureBlock> blocks = storage [featureName];
			if (blocks == null || blocks.Count == 0)
				throw new ApplicationException (String.Format ("Definition of feature '{0}' is empty", featureName));
			
			XmlDocument doc = new XmlDocument ();

			if (File.Exists (configFilePath))
				doc.Load (configFilePath);

			foreach (FeatureBlock block in blocks)
				AddFeatureBlock (doc, block, target, defaults, configBlocks);
			
			Helpers.SaveXml (doc, configFilePath);
		}

		void AddFeatureBlock (XmlDocument doc, FeatureBlock block, FeatureTarget target, IDefaultContainer[] defaults,
				      IConfigBlockContainer[] configBlocks)
		{
			if (target != FeatureTarget.Any && block.Target != target)
				return;

			ConfigBlockBlock configBlock = Helpers.FindConfigBlock (configBlocks, block.Name);
			if (configBlock == null)
				throw new ApplicationException (String.Format ("Config block '{0}' cannot be found", block.Name));

			XmlNode attachPoint = null;

			ProcessSections (doc, doc, "/", configBlock.Requires, defaults, configBlock.Name, ref attachPoint);
			if (attachPoint == null)
				attachPoint = FindDefaultAttachPoint (doc, configBlock.Requires);
			if (attachPoint == null)
				throw new ApplicationException (
					String.Format ("Missing attachment point for block '{0}'", configBlock.Name));
			
			XmlDocument contents = new XmlDocument ();
			contents.LoadXml (String.Format ("<{0}>{1}</{0}>", Helpers.FakeRootName, configBlock.Contents));
			AddFeatureRecursively (doc, attachPoint, contents.DocumentElement);
		}

		// TODO: handle comment and text nodes to avoid their duplication
		void AddFeatureRecursively (XmlDocument doc, XmlNode attachPoint, XmlNode top)
		{
			bool topIsFake = top.Name == Helpers.FakeRootName;
			XmlNode parent = null;
			string xpath;

			if (top.NodeType == XmlNodeType.Element) {
				xpath = BuildFeaturePath (attachPoint, topIsFake ? null : top);
				parent = DocumentHasFeatureFragment (doc, top, xpath);
			}
			
			if (!topIsFake && parent == null) {
				parent = doc.ImportNode (top, false);
				attachPoint.AppendChild (parent);
				if (parent.NodeType == XmlNodeType.Comment)
					return;
			}
			
			if (top.HasChildNodes)
				foreach (XmlNode node in top.ChildNodes)
					AddFeatureRecursively (doc, topIsFake ? attachPoint : parent, node);
		}

		XmlNode FindDefaultAttachPoint (XmlDocument doc, Section req)
		{
			List <Section> children = req.Children;
			if (children == null || children.Count == 0)
				return null;

			StringBuilder sb = new StringBuilder ("/");
			BuildPathToLastRequirement (sb, children);
			
			return doc.SelectSingleNode (sb.ToString ());
		}
		
		void BuildPathToLastRequirement (StringBuilder sb, List <Section> sections)
		{
			Section last = sections [sections.Count - 1];
			sb.AppendFormat ("/{0}", last.Name);

			List <Section> children = last.Children;
			if (children == null || children.Count == 0)
				return;

			BuildPathToLastRequirement (sb, children);
		}
		
		XmlNode DocumentHasFeatureFragment (XmlDocument doc, XmlNode top, string xpath)
		{
			if (top.NodeType == XmlNodeType.Comment)
				return null;
			
			return doc.SelectSingleNode (xpath);
		}

		string BuildFeaturePath (XmlNode parent, XmlNode child)
		{
			if (parent == null)
				return "/";
			
			List <string> path = new List <string> ();
			
			XmlNode cur = parent, last = null;
			while (cur != null && cur.NodeType != XmlNodeType.Document) {
				if (cur.NodeType == XmlNodeType.Element && cur.Name != Helpers.FakeRootName)
					path.Insert (0, cur.Name);
				last = cur;
				cur = cur.ParentNode;
			}
			
			string attributes = null;
			if (child != null && last.Name != child.Name) {
				if (child.NodeType == XmlNodeType.Element)
					path.Add (child.Name);
				
				attributes = BuildXPathFromAttributes (child);
			} else if (last != null)
				attributes = BuildXPathFromAttributes (last);
			
			path [path.Count - 1] += attributes;
			path.Insert (0, "/");
			
			return String.Join ("/", path.ToArray ());
		}		

		string BuildXPathFromAttributes (XmlNode node)
		{
			XmlAttributeCollection attrs = node.Attributes;
			StringBuilder sb = new StringBuilder ();
			string and = String.Empty;
			bool first = true;
			
			foreach (XmlAttribute attr in attrs) {
				sb.AppendFormat ("{0}@{1}=\"{2}\"",
						 and,
						 attr.Name,
						 attr.Value);
				if (first) {
					first = false;
					and = " and ";
				}
			}
			
			if (sb.Length == 0)
				return String.Empty;
			
			sb.Insert (0, "[");
			sb.Append ("]");
			
			return sb.ToString ();
		}
		
		void ProcessSections (XmlDocument doc, XmlNode parent, string topPath, Section top, IDefaultContainer[] defaults,
				      string blockName, ref XmlNode attachPoint)
		{
			List <Section> topChildren, children;
			if (top == null || (topChildren = top.Children) == null)
				return;
			
			XmlNode node;
			string curPath;

			foreach (Section s in topChildren) {
				curPath = String.Format ("{0}/{1}", topPath, s.Name);
				
				node = FindNodeOrAddDefault (doc, s.Name, curPath, defaults);
				if (node != null && s.AttachPoint) {
					if (attachPoint != null)
						throw new ApplicationException (
							String.Format ("Config block '{0}' has more than one attachment point",
								       blockName));
					attachPoint = node;
				}
				parent.AppendChild (node);
				
				if ((children = s.Children) != null && children.Count > 0)
					ProcessSections (doc, node, curPath, s, defaults, blockName, ref attachPoint);
			}

			return;
		}
		
		XmlNode FindNodeOrAddDefault (XmlDocument doc, string nodeName, string nodePath, IDefaultContainer[] defaults)
		{
			XmlNode ret = doc.SelectSingleNode (nodePath);

			if (ret != null)
				return ret;
			
			XmlDocument defDoc = Helpers.FindDefault (defaults, nodeName, FeatureTarget.Any);
			if (defDoc == null)
				throw new ApplicationException (
					String.Format ("Document doesn't contain node '{0}' and no default can be found",
						       nodePath));

			return doc.ImportNode (defDoc.DocumentElement.FirstChild, true);
		}
		
		void AssertStorage ()
		{
			if (storage == null)
				throw new ApplicationException ("No storage attached");
		}
	}
}
