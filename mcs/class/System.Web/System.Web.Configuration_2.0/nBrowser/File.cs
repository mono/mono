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
	using System.Web.Configuration.nBrowser;

	class File
	{
		private System.Xml.XmlDocument BrowserFile;
		internal System.Web.Configuration.nBrowser.Node[] Nodes;
		private System.Collections.Specialized.ListDictionary Lookup;
		private System.Collections.Specialized.ListDictionary DefaultLookup;
		internal List<Node> RefNodes;
		
		public string FileName
		{
			get
			{
				return pFileName;
			}
		}
		private string pFileName = string.Empty;
		public File(string file)
		{
			pFileName = file;

			BrowserFile = new System.Xml.XmlDocument();
			//I can put this in a try /catch but I want
			//this to bubble up.
			BrowserFile.Load(file);

			this.Load(BrowserFile);
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="file"></param>
		public File(System.Xml.XmlDocument BrowserFile, string filename)
		{
			pFileName = filename;
			this.Load(BrowserFile);
		}
		private void Load(System.Xml.XmlDocument BrowserFile)
		{
			Lookup = new System.Collections.Specialized.ListDictionary();
			DefaultLookup = new System.Collections.Specialized.ListDictionary();
			RefNodes = new List<Node>();
			System.Xml.XmlNode node;
			//I know this might allocate more nodes then needed but never less.
			Nodes = new Node[BrowserFile.DocumentElement.ChildNodes.Count];
			for (int a = 0;a <= BrowserFile.DocumentElement.ChildNodes.Count - 1;a++)
			{
				node = BrowserFile.DocumentElement.ChildNodes[a];

				if (node.NodeType == System.Xml.XmlNodeType.Comment)
				{
					continue;
				}
				Nodes[a] = new Node(node);
				Nodes[a].FileName = FileName;
				if (Nodes[a].NameType != NodeType.DefaultBrowser)
				{
					//fxcop sugguested this was faster then
					//Nodes[a].refID != string.Empty
					if (Nodes[a].RefId.Length > 0)
					{
						RefNodes.Add(Nodes[a]);
					}
					else if (Lookup.Contains(Nodes[a].Id) == false)
					{
						Lookup.Add(Nodes[a].Id, a);
					}
					else
					{
						throw new nBrowser.Exception("Duplicate ID found \"" + Nodes[a].Id + "\"");
					}
				}
				else
				{
					//fxcop sugguested this was faster then
					//Nodes[a].refID != string.Empty
					if (Nodes[a].RefId.Length > 0)
					{
						RefNodes.Add(Nodes[a]);
					}
					else if (DefaultLookup.Contains(Nodes[a].Id) == false)
					{
						DefaultLookup.Add(Nodes[a].Id, a);
					}
					else
					{
						throw new nBrowser.Exception("Duplicate ID found \"" + Nodes[a].Id + "\"");
					}
				}
			}
		}
		/// <summary>
		/// Returns a Array of strings, which represent the Id Attributes of all the
		/// Browser/Gatway Nodes
		/// </summary>
		public string[] Keys
		{
			get
			{

				string[] k = new string[Lookup.Keys.Count];
				//12-29-05
				//This will copy the Keys In Alphabetical Order
				//Lookup.Keys.CopyTo(k,0);
				//This Method is ment to copy the Keys in the order
				//that they were in the xml file.
				int b = 0;
				for (int i = 0;i <= Nodes.Length - 1;i++)
				{
					if (Nodes[i] != null && Nodes[i].NameType != NodeType.DefaultBrowser 
						&& Nodes[i].RefId.Length == 0)
					{
						k[b] = Nodes[i].Id;
						b++;
					}
				}
				return k;
			}
		}
		/// <summary>
		/// Returns a Array of strings, which represent the Id Attributes of all the
		/// DefaultBrowser Nodes
		/// </summary>
		public string[] DefaultKeys
		{
			get
			{
				string[] k = new string[DefaultLookup.Keys.Count];
				//12-29-05
				//This will copy the Keys In Alphabetical Order
				//DefaultLookup.Keys.CopyTo(k,0);
				//This Method is ment to copy the Keys in the order
				//that they were in the xml file.
				int b = 0;
				for (int i = 0;i <= Nodes.Length - 1;i++)
				{
					if (Nodes[i] != null && Nodes[i].NameType == NodeType.DefaultBrowser)
					{
						k[b] = Nodes[i].Id;
						b++;
					}
				}
				return k;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Key"></param>
		/// <returns></returns>
		internal Node GetNode(string Key)
		{
			object o = Lookup[Key];
			if (o == null)
				return GetDefaultNode (Key);
			return Nodes[(int)o];
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Key"></param>
		/// <returns></returns>
		internal Node GetDefaultNode(string Key)
		{
			object o = DefaultLookup[Key];
			if (o == null)
				return null;
			return Nodes[(int)o];
		}
	}
}
#endif
