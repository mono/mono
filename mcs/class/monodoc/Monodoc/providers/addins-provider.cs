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
using System.Linq;
using System.Diagnostics;
using System.Text;
using System.IO;
using System.Xml;
using System.Collections.Generic;

namespace Monodoc.Providers
{
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
			string fileId = Path.GetFileNameWithoutExtension (file);
			using (var f = File.OpenRead (file))
				tree.HelpSource.Storage.Store (fileId, f);

			XmlDocument doc = new XmlDocument ();
			doc.Load (file);
			
			foreach (XmlElement addin in doc.SelectNodes ("Addins/Addin")) {

				string addinId = addin.GetAttribute ("fullId");
				Node newNode = tree.RootNode.CreateNode (addin.GetAttribute ("name"), "addin:" + fileId + "#" + addinId);

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

	public class AddinsHelpSource : HelpSource
	{
		public AddinsHelpSource (string base_file, bool create) : base (base_file, create) 
		{
		}
		
		internal protected const string AddinPrefix = "addin:";
		internal protected const string ExtensionPrefix = "extension-point:";
		internal protected const string ExtensionNodePrefix = "extension-node:";

		public override bool CanHandleUrl (string url)
		{
			return url.StartsWith (AddinPrefix, StringComparison.OrdinalIgnoreCase)
				|| url.StartsWith (ExtensionPrefix, StringComparison.OrdinalIgnoreCase)
				|| url.StartsWith (ExtensionNodePrefix, StringComparison.OrdinalIgnoreCase);
		}

		protected override string UriPrefix {
			get {
				return AddinPrefix;
			}
		}
		
		public override DocumentType GetDocumentTypeForId (string id)
		{
			return DocumentType.AddinXml;
		}

		public override string GetInternalIdForUrl (string url, out Node node, out Dictionary<string, string> context)
		{
			var id = base.GetInternalIdForUrl (url, out node, out context);
			var idParts = id.Split ('#');
			context = new Dictionary<string, string> ();
			context["FileID"] = idParts[0];
			context["AddinID"] = idParts[1];
			context["NodeID"] = idParts[2];

			return idParts[0];
		}

		public override Node MatchNode (string url)
		{
			var prefix = new[] { AddinPrefix, ExtensionPrefix, ExtensionNodePrefix }.First (p => url.StartsWith (p, StringComparison.OrdinalIgnoreCase));
			return base.MatchNode (prefix != null ? url.Substring (prefix.Length) : url);
		}
	}
}
