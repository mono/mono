#if NET_2_0
/*
Used to determine Browser Capabilities by the Browsers UserAgent String and related
Browser supplied Headers.
Copyright (C) 2002-Present  Owen Brady (Ocean at owenbrady dot net) 
and Dean Brettle (dean at brettle dot com)

Permission is hereby granted, free of charge, to any person obtaining a copy 
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights 
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell 
copies of the Software, and to permit persons to whom the Software is furnished
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all 
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A 
PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT 
HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION 
OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE 
SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

namespace System.Web.Configuration.nBrowser
{
	using System;
	using System.Collections.Generic;
	using System.Text;
	internal class Build : System.Web.Configuration.CapabilitiesBuild
	{
		//This keeps a list of filenames and FileNodes linked
		private System.Collections.Generic.Dictionary<string, System.Web.Configuration.nBrowser.File> Browserfiles;

		//Just links FileNodes
		private System.Collections.Generic.List<System.Web.Configuration.nBrowser.File> nbrowserfiles;

		//
		private System.Collections.Generic.Dictionary<string, string> DefaultKeys;
		private System.Collections.Generic.Dictionary<string, string> BrowserKeys;

		//
		private object browserSyncRoot = new object();
		private System.Web.Configuration.nBrowser.Node browser;

		/// <summary>
		/// 
		/// </summary>
		public Build()
			: base()
		{
			Browserfiles = new System.Collections.Generic.Dictionary<string, System.Web.Configuration.nBrowser.File>();
			nbrowserfiles = new System.Collections.Generic.List<System.Web.Configuration.nBrowser.File>();

			DefaultKeys = new System.Collections.Generic.Dictionary<string, string>();
			BrowserKeys = new System.Collections.Generic.Dictionary<string, string>();
		}
		/// <summary>
		/// Reads an entire directory and process's all of the browser files in that
		/// directory.
		/// </summary>
		/// <param name="path"></param>
		public void AddBrowserDirectory(string path)
		{
			//I allow this function to be a little messy
			//just in case they pass in a path that really a file
			//name
			if (System.IO.Directory.Exists(path) == true)
			{
				System.IO.DirectoryInfo dir = new System.IO.DirectoryInfo(path);
				System.IO.FileInfo[] file = dir.GetFiles("*.browser");
				//we are done with it so let the GC have it early as possible
				dir = null;
				for (int a = 0;a <= file.Length - 1;a++)
				{
					AddBrowserFile(file[a].FullName);
				}
			}
			else if (System.IO.File.Exists(path) == true)
			{
				AddBrowserFile(path);
			}
		}
		/// <summary>
		/// Reads a browser file and builds the Nodes which that file contains.
		/// </summary>
		/// <param name="filename"></param>
		public void AddBrowserFile(string fileName)
		{
			if (Browserfiles.ContainsKey(fileName) == false)
			{
				nBrowser.File b = new nBrowser.File(fileName);
				this.AddBrowserFile(b);
			}

		}
		private void AddBrowserFile(nBrowser.File file)
		{
			if (Browserfiles.ContainsKey(file.FileName) == false)
			{
				Browserfiles.Add(file.FileName, file);
				nbrowserfiles.Add(file);

				string[] keys = file.Keys;
				for (int i = 0;i <= keys.Length - 1;i++)
				{
					if (BrowserKeys.ContainsKey(keys[i]) == false)
					{
						BrowserKeys.Add(keys[i], file.FileName);
					}
					else
					{
						throw new nBrowser.Exception("Duplicate Key \"" + keys[i] + "\" found in " + file.FileName + " and in file " + BrowserKeys[keys[i]]);
					}
				}
				keys = file.DefaultKeys;
				for (int i = 0;i <= keys.Length - 1;i++)
				{
					if (DefaultKeys.ContainsKey(keys[i]) == false)
					{
						DefaultKeys.Add(keys[i], file.FileName);
					}
					else
					{
						throw new nBrowser.Exception("Duplicate Key \"" + keys[i] + "\" found in " + file.FileName + " and in file " + DefaultKeys[keys[i]]);
					}
				}
			}
		}
		/// <summary>
		/// Reads a browser file and builds the Nodes which that file contains.
		/// </summary>
		/// <param name="file"></param>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1059")]
		public void AddBrowserFile(System.Xml.XmlDocument browser, string fileName)
		{
			if (Browserfiles.ContainsKey(fileName) == false)
			{
				nBrowser.File file = new nBrowser.File(browser, fileName);
				this.AddBrowserFile(file);
			}
		}
		/// <summary>
		/// Returns the root Node of the Browser Tree.
		/// </summary>
		/// <returns></returns>
		public Node Browser()
		{
			if (browser == null)
			{
				lock (browserSyncRoot)
				{
					if (browser == null)
					{
						browser = InitializeTree();
					}
				}
			}
			return browser;
		}

		private Node InitializeTree()
		{
			Node root = new Node();
			//Custom Sorted List, to allow where Multple files in Diff directorys might have the same
			//filename. So still to some degree first come first serve but might be close enough
			//to how microsoft System to match much more closely.
			System.Collections.Generic.SortedList<string, System.Collections.Generic.List<nBrowser.File>> list;
			list = new System.Collections.Generic.SortedList<string, System.Collections.Generic.List<nBrowser.File>>();

			for (int i = 0;i <= Browserfiles.Count - 1;i++)
			{
				if (list.ContainsKey(nbrowserfiles[i].FileName) == false)
				{
					System.Collections.Generic.List<nBrowser.File> l;
					l = new System.Collections.Generic.List<nBrowser.File>();
					list.Add(nbrowserfiles[i].FileName, l);
				}
				list[nbrowserfiles[i].FileName].Add(nbrowserfiles[i]);
			}
			nBrowser.File[] files = new nBrowser.File[Browserfiles.Count];

			int count = 0;
			for (int i = 0;i <= list.Count - 1;i++)
			{
				System.Collections.Generic.List<nBrowser.File> l = list[list.Keys[i]];
				for (int b = 0;b <= l.Count - 1;b++)
				{
					files[count] = l[b];
					count++;
				}
			}

			#region Connect Nodes
			for (int i = 0;i <= Browserfiles.Count - 1;i++)
			{
				for (int a = 0;a <= files[i].Keys.Length - 1;a++)
				{
					Node child = files[i].GetNode(files[i].Keys[a]);
					Node parent = null;
					if (child.ParentId.Length > 0)
					{
						parent = this.GetNode(child.ParentId);
						if (parent == null)
							throw new nBrowser.Exception(String.Format("Parent not found with id = {0}", child.ParentId));
					}
					if (parent == null)
						parent = root;
					parent.AddChild(child);
				}
			}
			#endregion
			
			#region Inject DefaultBrowser Nodes
			for (int i = 0;i <= Browserfiles.Count - 1;i++)
			{
				for (int a = 0;a <= files[i].DefaultKeys.Length - 1;a++)
				{
					Node defaultNode = files[i].GetDefaultNode(files[i].DefaultKeys[a]);
					Node node = this.GetNode(defaultNode.Id);
					if (node == defaultNode) 
					{
						// there is no regular node so the defaultNode is already at
						// the correct spot in the tree.
						continue;
					}
					Node parentNode = this.GetNode(node.ParentId);
					if (parentNode == null)
						parentNode = root;
					// insert the default node between the regular node and it's parent.
					parentNode.RemoveChild(node);
					defaultNode.AddChild(node);
					parentNode.AddChild(defaultNode);
				}
			}
			#endregion

			#region Merge Ref Nodes
			for (int i = 0;i <= Browserfiles.Count - 1;i++)
			{
				foreach (Node refNode in files[i].RefNodes) {
					GetNode(refNode.RefId).MergeFrom(refNode);
				}
			}
			#endregion

			return root;
		}
						
		/// <summary>
		/// returns a Node Matching the Key supplied, for either a 
		/// DefaultBrowser, or Gatway/Browser node.
		/// </summary>
		/// <param name="Key"></param>
		/// <returns></returns>
		private Node GetNode(string Key)
		{
			if (Key == null || Key.Length == 0)
				return null;
				
			string filename;
			//Must find what file node that this key is located in
			//so we look it up in the string dictionary's
			if (!BrowserKeys.TryGetValue(Key, out filename)
				&& !DefaultKeys.TryGetValue(Key, out filename))
				return null;
				
			//fxcop sugguested this was faster then
			//filename!= string.Empty
			if (filename != null && filename.Length > 0)
			{
				//now that we have a name we look it up in the hasttable containing
				//the actual node.
				nBrowser.File b = Browserfiles[filename];
				Node n = b.GetNode(Key);
				return n;
			}

			return null;			
		}
		
		/// <summary>
		/// Returns an Array of Nodes that have been built. To be used for Design/Editors of Browser
		/// files.
		/// </summary>
		/// <returns></returns>
		public Node[] Nodes()
		{
			Node[] browsers;
			nBrowser.File[] files = new nBrowser.File[Browserfiles.Count];
			Browserfiles.Values.CopyTo(files, 0);
			int count = 0;
			for (int i = 0;i <= files.Length - 1;i++)
			{
				count += files[i].Nodes.Length;
			}
			browsers = new Node[count];
			count = 0;
			for (int i = 0;i <= files.Length - 1;i++)
			{
				for (int a = 0;a <= files[i].Nodes.Length - 1;a++)
				{
					browsers[count] = files[i].Nodes[a];
					count++;
				}
			}
			return browsers;
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="header"></param>
		/// <param name="initialCapabilities"></param>
		/// <returns></returns>
		public override System.Web.Configuration.CapabilitiesResult Process(System.Collections.Specialized.NameValueCollection header, System.Collections.IDictionary initialCapabilities)
		{
			if (initialCapabilities == null)
				initialCapabilities = new System.Collections.Generic.Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
			System.Web.Configuration.nBrowser.Result r = new System.Web.Configuration.nBrowser.Result(initialCapabilities);

#if trace
			System.Diagnostics.Trace.WriteLine(string.Join("+", new string[50]));
			for (int i=0;i <= header.Count -1;i++)
			{
				System.Diagnostics.Trace.WriteLine(string.Format("{0}{1}",header.GetKey(i).PadRight(25),header[i]));
			}
			System.Diagnostics.Trace.WriteLine(string.Join("+", new string[50]));
#endif			
			Browser().Process(header, r, new System.Collections.Generic.List<System.Text.RegularExpressions.Match>());
			return r;
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="list"></param>
		/// <returns></returns>
		protected override System.Collections.ObjectModel.Collection<string> HeaderNames(System.Collections.ObjectModel.Collection<string> list)
		{
			return this.Browser().HeaderNames(list);
		}
	}
}
#endif
