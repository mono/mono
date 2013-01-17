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
	using System.Text.RegularExpressions;
		
	internal class Node
	{
		#region Public Properties
		/// <summary>
		/// 
		/// </summary>
		public NodeType NameType
		{
			get
			{
				return pName;
			}
			set
			{
				pName = value;
			}
		}
		/// <summary>
		/// 
		/// </summary>
		public string Id
		{
			get
			{
				return pId;
			}
			set
			{
				pId = value;
			}
		}
		/// <summary>
		/// 
		/// </summary>
		public string ParentId
		{
			get
			{
				return pParentID;
			}
			set
			{
				pParentID = value;
			}
		}
		/// <summary>
		/// 
		/// </summary>
		public string RefId
		{
			get
			{
				return pRefID;
			}
			set
			{
				pRefID = value;
			}
		}
		/// <summary>
		/// 
		/// </summary>
		public string MarkupTextWriterType
		{
			get
			{
				return pMarkupTextWriterType;
			}
			set
			{
				pMarkupTextWriterType = value;
			}
		}
		/// <summary>
		/// 
		/// </summary>
		public string FileName
		{
			get
			{
				return pFileName;
			}
			set
			{
				pFileName = value;
			}
		}
		#endregion

		private NodeType pName = NodeType.None;
		private string pId = string.Empty;
		private string pParentID = string.Empty;
		private string pRefID = string.Empty;
		private string pMarkupTextWriterType = string.Empty;
		private string pFileName = string.Empty;

		private System.Xml.XmlNode xmlNode;
		private Identification[] Identification;
		private Identification[] Capture;
		private System.Collections.Specialized.NameValueCollection Capabilities;
		private System.Collections.Specialized.NameValueCollection Adapter;
		private Type[] AdapterControlTypes, AdapterTypes;
		private System.Collections.Generic.List<string> ChildrenKeys;
		private System.Collections.Generic.List<string> DefaultChildrenKeys;
		private System.Collections.Generic.SortedList<string, nBrowser.Node> Children;
		private System.Collections.Generic.SortedList<string, nBrowser.Node> DefaultChildren;
		private System.Collections.Specialized.NameValueCollection sampleHeaders;


		/// <summary>
		/// 
		/// </summary>
		/// <param name="xmlNode"></param>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1059")]
		public Node(System.Xml.XmlNode xmlNode)
		{
			this.xmlNode = xmlNode;
			this.ResetChildern();
			this.Reset();
		}
		/// <summary>
		/// 
		/// </summary>
		internal Node()
		{
			this.ResetChildern();
			Identification = new System.Web.Configuration.nBrowser.Identification[1];
			Identification[0] = new System.Web.Configuration.nBrowser.Identification(true, "header", "User-Agent", ".");
			this.Id = "[Base Node]";
			this.NameType = NodeType.Browser;

		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="node"></param>
		private void ProcessIdentification(System.Xml.XmlNode node)
		{
			//I know not all of these will be used but enough will
			Identification = new System.Web.Configuration.nBrowser.Identification[node.ChildNodes.Count];

			int i = -1;
			for (int a = 0;a <= node.ChildNodes.Count - 1;a++)
			{
				switch (node.ChildNodes[a].NodeType)
				{
					case System.Xml.XmlNodeType.Text:
						continue;
					case System.Xml.XmlNodeType.Comment:
						continue;
				}

				string patterngroup = string.Empty;
				string patternname = string.Empty;

				if (string.Compare(node.ChildNodes[a].Name, "userAgent", true, System.Globalization.CultureInfo.CurrentCulture) == 0)
				{
					patterngroup = "header";
					patternname = "User-Agent";
				}
				else if (string.Compare(node.ChildNodes[a].Name, "header", true, System.Globalization.CultureInfo.CurrentCulture) == 0)
				{
					patterngroup = node.ChildNodes[a].Name;
					patternname = node.ChildNodes[a].Attributes["name"].Value;
				}
				else if (string.Compare(node.ChildNodes[a].Name, "capability", true, System.Globalization.CultureInfo.CurrentCulture) == 0)
				{
					patterngroup = node.ChildNodes[a].Name;
					patternname = node.ChildNodes[a].Attributes["name"].Value;
				}
				else
				{
					throw new nBrowser.Exception("Invalid Node found in Identification");
				}

				if (node.ChildNodes[a].Attributes["match"] != null)
				{
					i++;
					Identification[i] = new System.Web.Configuration.nBrowser.Identification(true, patterngroup, patternname, node.ChildNodes[a].Attributes["match"].Value);
				}
				else if (node.ChildNodes[a].Attributes["nonMatch"] != null)
				{
					i++;
					Identification[i] = new System.Web.Configuration.nBrowser.Identification(false, patterngroup, patternname, node.ChildNodes[a].Attributes["nonMatch"].Value);
				}
			}
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="node"></param>
		private void ProcessCapture(System.Xml.XmlNode node)
		{
			//I know not all of these will be used but enough will
			Capture = new System.Web.Configuration.nBrowser.Identification[node.ChildNodes.Count];

			int i = -1;
			for (int a = 0;a <= node.ChildNodes.Count - 1;a++)
			{
				switch (node.ChildNodes[a].NodeType)
				{
					case System.Xml.XmlNodeType.Text:
						continue;
					case System.Xml.XmlNodeType.Comment:
						continue;
				}

				string pattern = string.Empty;
				string patterngroup = string.Empty;
				string patternname = string.Empty;

				if (node.ChildNodes[a].Name == "userAgent")
				{
					patterngroup = "header";
					patternname = "User-Agent";
				}
				else
				{
					patterngroup = node.ChildNodes[a].Name;
					patternname = node.ChildNodes[a].Attributes["name"].Value;
				}
				pattern = node.ChildNodes[a].Attributes["match"].Value;
				i++;
				Capture[i] = new System.Web.Configuration.nBrowser.Identification(true, patterngroup, patternname, pattern);
			}
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="node"></param>
		private void ProcessCapabilities(System.Xml.XmlNode node)
		{
			Capabilities = new System.Collections.Specialized.NameValueCollection(node.ChildNodes.Count, StringComparer.OrdinalIgnoreCase);

			for (int a = 0;a <= node.ChildNodes.Count - 1;a++)
			{
				if (node.ChildNodes[a].NodeType == System.Xml.XmlNodeType.Comment)
				{
					continue;
				}
				string name = string.Empty;
				string value = string.Empty;
				for (int b = 0;b <= node.ChildNodes[a].Attributes.Count - 1;b++)
				{
					switch (node.ChildNodes[a].Attributes[b].Name)
					{
						case "name":
							name = node.ChildNodes[a].Attributes[b].Value;
							break;
						case "value":
							value = node.ChildNodes[a].Attributes[b].Value;
							break;
					}
				}
				if (name.Length > 0)
				{
					Capabilities[name] = value;
				}
			}
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="node"></param>
		private void ProcessControlAdapters(System.Xml.XmlNode node)
		{
			Adapter = new System.Collections.Specialized.NameValueCollection();
			for (int b = 0;b <= node.Attributes.Count - 1;b++)
			{
				switch (node.Attributes[b].Name)
				{
					case "markupTextWriterType":
						MarkupTextWriterType = node.Attributes[b].Value;
						break;
				}
			}
			for (int a = 0;a <= node.ChildNodes.Count - 1;a++)
			{
				if (node.ChildNodes[a].NodeType == System.Xml.XmlNodeType.Comment)
				{
					continue;
				}
				else if (node.ChildNodes[a].NodeType == System.Xml.XmlNodeType.Text)
				{
					continue;
				}
				System.Xml.XmlNode x = node.ChildNodes[a];
				string controlType = string.Empty;
				string adapterType = string.Empty;
				for (int i = 0;i <= x.Attributes.Count - 1;i++)
				{
					if (string.Compare(x.Attributes[i].Name, "controlType", true, System.Globalization.CultureInfo.CurrentCulture) == 0)
					{
						controlType = x.Attributes[i].Value;
					}
					else if (string.Compare(x.Attributes[i].Name, "adapterType", true, System.Globalization.CultureInfo.CurrentCulture) == 0)
					{
						adapterType = x.Attributes[i].Value;
					}
				}
				if (controlType.Length > 0 && adapterType.Length > 0)
				{
					Adapter[controlType] = adapterType;
				}
			}

			AdapterControlTypes = null;
			AdapterTypes = null;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="node"></param>
		private void ProcessSampleHeaders(System.Xml.XmlNode node)
		{
			sampleHeaders = new System.Collections.Specialized.NameValueCollection(node.ChildNodes.Count);

			for (int a = 0;a <= node.ChildNodes.Count - 1;a++)
			{
				if (node.ChildNodes[a].NodeType == System.Xml.XmlNodeType.Comment)
				{
					continue;
				}
				string name = string.Empty;
				string value = string.Empty;
				for (int b = 0;b <= node.ChildNodes[a].Attributes.Count - 1;b++)
				{
					switch (node.ChildNodes[a].Attributes[b].Name)
					{
						case "name":
							name = node.ChildNodes[a].Attributes[b].Value;
							break;
						case "value":
							value = node.ChildNodes[a].Attributes[b].Value;
							break;
					}
				}
				if (name.Length > 0)
				{
					sampleHeaders[name] = value;
				}
			}
		}
		internal void ResetChildern()
		{
			Children = new System.Collections.Generic.SortedList<string, nBrowser.Node>();
			DefaultChildren = new System.Collections.Generic.SortedList<string, nBrowser.Node>();
			ChildrenKeys = new System.Collections.Generic.List<string>();
			DefaultChildrenKeys = new System.Collections.Generic.List<string>();
		}
		/// <summary>
		/// 
		/// </summary>
		public bool HasChildren
		{
			get
			{
				if (Children.Count > -1)
				{
					return true;
				}
				return false;
			}
		}
		/// <summary>
		/// 
		/// </summary>
		public void Reset()
		{
			//Making sure we start off on a good footing.
			Capture = null;
			Capabilities = null;
			Adapter = null;
			AdapterControlTypes = null;
			AdapterTypes = null;
			if (string.Compare(this.xmlNode.Name, "browser", true, System.Globalization.CultureInfo.CurrentCulture) == 0)
			{
				this.NameType = NodeType.Browser;
			}
			else if (string.Compare(this.xmlNode.Name, "defaultBrowser", true, System.Globalization.CultureInfo.CurrentCulture) == 0)
			{
				this.NameType = NodeType.DefaultBrowser;
			}
			else if (string.Compare(this.xmlNode.Name, "gateway", true, System.Globalization.CultureInfo.CurrentCulture) == 0)
			{
				this.NameType = NodeType.Gateway;
			}


			//-------------------------------------------------------------------------
			//Looping though the Attributes is easier since and more efficient
			//then doing finds for specific attribute names. This also handles the
			//cases where there are no attributes really well. Also it doesn't care
			//about the order in witch the attributes are found either
			//-------------------------------------------------------------------------
			for (int a = 0;a <= xmlNode.Attributes.Count - 1;a++)
			{
				//Reason I am not using a switch here because I do not have the ability
				//to make sure the items are in the same upper/lower case as I am expecting
				//so I default to ignore case and compare.
				if (string.Compare(xmlNode.Attributes[a].Name, "id", true, System.Globalization.CultureInfo.CurrentCulture) == 0)
				{
					Id = xmlNode.Attributes[a].Value.ToLower(System.Globalization.CultureInfo.CurrentCulture);
				}
				else if (string.Compare(xmlNode.Attributes[a].Name, "parentID", true, System.Globalization.CultureInfo.CurrentCulture) == 0)
				{
					ParentId = xmlNode.Attributes[a].Value.ToLower(System.Globalization.CultureInfo.CurrentCulture);
				}
				else if (string.Compare(xmlNode.Attributes[a].Name, "refID", true, System.Globalization.CultureInfo.CurrentCulture) == 0)
				{
					RefId = xmlNode.Attributes[a].Value.ToLower(System.Globalization.CultureInfo.CurrentCulture);
				}
			}

			for (int a = 0;a <= xmlNode.ChildNodes.Count - 1;a++)
			{
				//Reason I am not using a switch here because I do not have the ability
				//to make sure the items are in the same upper/lower case as I am expecting
				//so I default to ignore case and compare.
				if (string.Compare(xmlNode.ChildNodes[a].Name, "identification", true, System.Globalization.CultureInfo.CurrentCulture) == 0)
				{
					ProcessIdentification(xmlNode.ChildNodes[a]);
				}
				else if (string.Compare(xmlNode.ChildNodes[a].Name, "capture", true, System.Globalization.CultureInfo.CurrentCulture) == 0)
				{
					ProcessCapture(xmlNode.ChildNodes[a]);
				}
				else if (string.Compare(xmlNode.ChildNodes[a].Name, "capabilities", true, System.Globalization.CultureInfo.CurrentCulture) == 0)
				{
					ProcessCapabilities(xmlNode.ChildNodes[a]);
				}
				else if (string.Compare(xmlNode.ChildNodes[a].Name, "controlAdapters", true, System.Globalization.CultureInfo.CurrentCulture) == 0)
				{
					ProcessControlAdapters(xmlNode.ChildNodes[a]);
				}
				else if (string.Compare(xmlNode.ChildNodes[a].Name, "sampleHeaders", true, System.Globalization.CultureInfo.CurrentCulture) == 0)
				{
					ProcessSampleHeaders(xmlNode.ChildNodes[a]);
				}
				
				if (Id == "default" && (Identification == null || Identification.Length == 0))
				{
					Identification = new Identification[1];
					Identification[0] = new System.Web.Configuration.nBrowser.Identification(true, "header", "User-Agent", ".");
				}
			}
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="child"></param>
		public void AddChild(Node child)
		{
			if (child == null)
			{
				return;
			}
			if (child.NameType == nBrowser.NodeType.Browser || child.NameType == nBrowser.NodeType.Gateway)
			{
				Children.Add(child.Id, child);
				ChildrenKeys.Add(child.Id);
			}
			else if (child.NameType == NodeType.DefaultBrowser)
			{
				DefaultChildren.Add(child.Id, child);
				DefaultChildrenKeys.Add(child.Id);
			}
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="child"></param>
		public void RemoveChild(Node child)
		{
			if (child == null)
			{
				return;
			}
			if (child.NameType == nBrowser.NodeType.Browser || child.NameType == nBrowser.NodeType.Gateway)
			{
				Children.Remove(child.Id);
				ChildrenKeys.Remove(child.Id);
			}
			else if (child.NameType == NodeType.DefaultBrowser)
			{
				DefaultChildren.Remove(child.Id);
				DefaultChildrenKeys.Remove(child.Id);
			}
		}
				
		private Type FindType(string typeName)
		{
			foreach (System.Reflection.Assembly a in System.AppDomain.CurrentDomain.GetAssemblies())
			{
				string fullTypeName = typeName + "," + a.FullName;
				Type t = System.Type.GetType(fullTypeName); // case-sensitive
				if (t != null)
					return t;
				t = System.Type.GetType(fullTypeName, false, true); // case-insensitive
				if (t != null)
					return t;
			}
			throw new TypeLoadException(typeName);
		}
		
		/// <summary>
		/// Matches the header collection against this subtree and uses the matchList
		/// and any new matches to augment the result.  This method calls ProcessSubtree()
		/// but then removes the matches that it adds to the matchList.
		/// </summary>
		/// <param name="header">the header collection to evaluate (invariant)</param>
		/// <param name="result">the result of the match (might be changed if a match is found)</param>
		/// <param name="matchList">the matches to use to do substitutions (invariant)</param>
		/// <returns>true iff this node or one of it's descendants matches</returns>
		internal bool Process(System.Collections.Specialized.NameValueCollection header, nBrowser.Result result,
		                      System.Collections.Generic.List<Match> matchList)
		{
			// The real work is done in ProcessSubtree.  This method just ensures that matchList is restored
			// to its original state before returning.
			int origMatchListCount = matchList.Count;
			bool matched = ProcessSubtree(header, result, matchList);
			if (matchList.Count > origMatchListCount)
				matchList.RemoveRange(origMatchListCount, matchList.Count-origMatchListCount);
			return matched;
		}
		
		/// <summary>
		/// Matches the header collection against this subtree, adds any new matches for this node to
		/// matchList, and uses the matchList to augment the result.  
		/// </summary>
		/// <param name="header">the header collection to evaluate (invariant)</param>
		/// <param name="result">the result of the match (might be changed if a match is found)</param>
		/// <param name="matchList">the matches to use to do substitutions, 
		/// possibly including new matches for this node.</param>
		/// <returns>true iff this node or one of it's descendants matches</returns>
		private bool ProcessSubtree(System.Collections.Specialized.NameValueCollection header, nBrowser.Result result, System.Collections.Generic.List<Match> matchList)
		{
			//----------------------------------------------------------------------
			//This is just coded over from MS version since if you pass in an empty
			//string for the key it returns the UserAgent header as a response.
			//----------------------------------------------------------------------
			result.AddCapabilities("", header["User-Agent"]);

			if (RefId.Length == 0 && this.NameType != NodeType.DefaultBrowser)
			{
				//----------------------------------------------------------------------
				//BrowserIdentification adds all the Identifiction matches to the match
				//list if this node matches.
				//----------------------------------------------------------------------
				if (!BrowserIdentification(header, result, matchList))
					return false;
			}

			result.AddMatchingBrowserId (this.Id);
			#region Browser Identification Successfull
			//----------------------------------------------------------------------
			//By reaching this point, it either means there were no Identification 
			//items to be processed or that all the Identification items have been 
			//passed. So just for debuging I want to output this Groups unique ID.
			//----------------------------------------------------------------------
			result.AddTrack("[" + this.NameType + "]\t" + this.Id);

			//----------------------------------------------------------------------
			//Just adding all the Adapters to the current list.
			//----------------------------------------------------------------------
			if (Adapter != null)
			{
				LookupAdapterTypes();
				for (int i = 0;i <= Adapter.Count - 1;i++)
				{
					result.AddAdapter(AdapterControlTypes [i], AdapterTypes [i]);
				}
			}

			//----------------------------------------------------------------------
			//Set the MarkupTextWriter in the result if set in this node.
			//----------------------------------------------------------------------
			if (MarkupTextWriterType != null && MarkupTextWriterType.Length > 0)
			{
				// Look for the type using a case-sensitive search
				result.MarkupTextWriter = Type.GetType(MarkupTextWriterType);
				// If we don't find it, try again using a case-insensitive search and throw
				// and exception if we can't find it.
				if (result.MarkupTextWriter == null)
					result.MarkupTextWriter = Type.GetType(MarkupTextWriterType, true, true);
			}

			#endregion
			#region Capture
			if (Capture != null)
			{
				//----------------------------------------------------------------------
				//Adds all the sucessfull Capture matches to the matchList
				//----------------------------------------------------------------------
				for (int i = 0;i <= Capture.Length - 1;i++)
				{
					//shouldn't happen often, the null should
					//signal the end of the list, I keep procssing
					//the rest just in case
					if (Capture[i] == null)
					{
						continue;
					}
					Match m = null;
					if (Capture[i].Group == "header")
					{
						m = Capture[i].GetMatch(header[Capture[i].Name]);
					}
					else if (Capture[i].Group == "capability")
					{
						m = Capture[i].GetMatch(result[Capture[i].Name]);
					}
					if (Capture[i].IsMatchSuccessful(m) && m.Groups.Count > 0)
					{
						matchList.Add(m);
					}
				}
			}
			#endregion
			#region Capabilities
			if (Capabilities != null)
			{
				//----------------------------------------------------------------------
				//This section is what the whole exercise is about. Determining
				//the Browser Capabilities. We know already that the current
				//browser matches the criteria, now its a mater of updating
				//the results with the new Capabilties listed.
				//----------------------------------------------------------------------
				for (int i = 0;i <= Capabilities.Count - 1;i++)
				{
					//----------------------------------------------------------------------
					//We need to further process these Capabilities to 
					//insert the proper information.
					//----------------------------------------------------------------------
					string v = Capabilities[i];

					//----------------------------------------------------------------------
					//Loop though the list of Identifiction/Capture Matches
					//in reverse order. Meaning the newest Items in the list
					//get checked first, then working to the oldest. Often times
					//Minor /Major revisition numbers will be listed multple times
					//and only the newest one (most recent matches) are the ones
					//we want to insert.
					//----------------------------------------------------------------------
					for (int a = matchList.Count - 1; a >= 0 && v != null && v.Length > 0 &&  v.IndexOf('$') > -1; a--)
					{
						// Don't do substitution if the match has no groups or was a nonMatch
						if (matchList[a].Groups.Count == 0 || !matchList[a].Success)
							continue;
						v = matchList[a].Result(v);
					}

					//----------------------------------------------------------------------
					//Checks to make sure we extract the result we where looking for.
					//----------------------------------------------------------------------
					if (v.IndexOf('$') > -1 || v.IndexOf('%') > -1)
					{
						//----------------------------------------------------------------------
						//Microsoft has a nasty habbit of using capability items in regular expressions
						//so I have to figure a nice way to working around it
						// <capability name="msdomversion"		value="${majorversion}${minorversion}" />
						//----------------------------------------------------------------------

						//double checks the values against the current Capabilities. to 
						//find any last minute matches. that are not defined by regluar
						//expressions
						v = result.Replace(v);
					}

					result.AddCapabilities(Capabilities.Keys[i], v);
				}
			}
			#endregion

			//----------------------------------------------------------------------
			//Run the Default Children after the Parent Node is finished with 
			//what it is doing
			//----------------------------------------------------------------------
			for (int i = 0;i <= DefaultChildren.Count - 1;i++)
			{
				string key = DefaultChildrenKeys[i];
				Node node = DefaultChildren[key];
				if (node.NameType == NodeType.DefaultBrowser)
				{
					node.Process(header, result, matchList);
				}
			}
			//----------------------------------------------------------------------
			//processing all the children Browsers of this Parent if there are any.
			//----------------------------------------------------------------------
			//In nBrowser files, the Gateways should of been sorted so they are all 
			//at the top so that they can be ran first.
			//----------------------------------------------------------------------
			//According to the msdn2 documentation Gateways are suppost to be
			//all processed first. before the browser objects.
			for (int i = 0;i <= Children.Count - 1;i++)
			{
				string key = ChildrenKeys[i];
				Node node = Children[key];
				if (node.NameType == NodeType.Gateway)
				{
					node.Process(header, result, matchList);
				}
			}
			for (int i = 0;i <= Children.Count - 1;i++)
			{
				string key = ChildrenKeys[i];
				Node node = Children[key];
				if (node.NameType == NodeType.Browser 
				    && node.Process(header, result, matchList))
					break;
			}

			return true;
		}
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="header"></param>
		/// <param name="result"></param>
		/// <param name="matchList"></param>
		/// <returns>true iff this node is a match</returns>
		private bool BrowserIdentification(System.Collections.Specialized.NameValueCollection header, System.Web.Configuration.CapabilitiesResult result, System.Collections.Generic.List<Match> matchList)
		{
			if (Id.Length > 0 && RefId.Length > 0)
			{
				throw new nBrowser.Exception("Id and refID Attributes givin when there should only be one set not both");
			}
			if (Identification == null || Identification.Length == 0)
			{
				throw new nBrowser.Exception(String.Format("Missing Identification Section where one is required (Id={0}, RefID={1})", Id, RefId));
			}
			if (header == null)
			{
				throw new nBrowser.Exception("Null Value where NameValueCollection expected ");
			}
			if (result == null)
			{
				throw new nBrowser.Exception("Null Value where Result expected ");
			}

#if trace			   
			System.Diagnostics.Trace.WriteLine(string.Format("{0}[{1}]", ("[" + this.Id + "]").PadRight(45), this.ParentId));
#endif
			
			for (int i = 0;i <= Identification.Length - 1;i++)
			{

				//shouldn't happen often, the null should
				//signal the end of the list, I keep procssing
				//the rest just in case
				if (Identification[i] == null)
				{
					continue;
				}
				string v = string.Empty;
				if (string.Compare(Identification[i].Group, "header", true, System.Globalization.CultureInfo.CurrentCulture) == 0)
				{
					v = header[Identification[i].Name];
				}
				else if (string.Compare(Identification[i].Group, "capability", true, System.Globalization.CultureInfo.CurrentCulture) == 0)
				{
					v = result[Identification[i].Name];
				}
				//Not all headers will be sent by all browsers.
				//so often a header search will return Null.
				if (v == null)
				{
					v = string.Empty;
				}
				Match m = Identification[i].GetMatch(v);
				//----------------------------------------------------------------------
				//we exit this method return the orginal Result back to  the calling method.
				//----------------------------------------------------------------------
				if (Identification[i].IsMatchSuccessful(m) == false)
				{
#if trace 
					System.Diagnostics.Trace.WriteLine(string.Format("{0}{1}", "Failed:".PadRight(45), Identification[i].Pattern));
#endif
					return false;
				}
				else
				{
#if trace 
					System.Diagnostics.Trace.WriteLine(string.Format("{0}{1}", "Passed:".PadRight(45), Identification[i].Pattern));
#endif
					if (m.Groups.Count > 0)
					{
						matchList.Add(m);
					}
				}
			}
#if trace
			System.Diagnostics.Trace.WriteLine("");
#endif
			return true;
		}
		
		private bool HaveAdapterTypes = false;
		private object LookupAdapterTypesLock = new object();
		private void LookupAdapterTypes()
		{
			if (Adapter == null || HaveAdapterTypes) return;
			lock (LookupAdapterTypesLock)
			{
				if (HaveAdapterTypes) return;
				/* Lookup the types and store them for future use */
				if (AdapterControlTypes == null)
					AdapterControlTypes = new Type [Adapter.Count];
				if (AdapterTypes == null)
					AdapterTypes = new Type [Adapter.Count];
				for (int i = 0;i <= Adapter.Count - 1;i++) {
					if (AdapterControlTypes [i] == null)
						AdapterControlTypes [i] = FindType (Adapter.GetKey (i));
					if (AdapterTypes [i] == null)
						AdapterTypes [i] = FindType (Adapter [i]);
				}
				HaveAdapterTypes = true;
			}
		}
		
		/// <summary>
		/// 
		/// </summary>
		public System.Collections.Specialized.NameValueCollection SampleHeader
		{
			get
			{
				return sampleHeaders;
			}
		}
		/// <summary>
		/// Used to Display a Tree Like View of how the Nodes are organized.
		/// </summary>
		/// <param name="writer"></param>
		/// <param name="Position"></param>
		public void Tree(System.Xml.XmlTextWriter xmlwriter, int position)
		{
			if (position == 0)
			{
				xmlwriter.WriteStartDocument();
				xmlwriter.WriteStartElement(this.NameType.ToString());
				xmlwriter.WriteRaw(System.Environment.NewLine);
			}

			string f = this.FileName;
			xmlwriter.WriteStartElement(this.NameType.ToString());
			xmlwriter.WriteAttributeString("FileName", f);
			xmlwriter.WriteAttributeString("ID", this.Id);
			xmlwriter.WriteRaw(System.Environment.NewLine);

			if (position != int.MaxValue)
			{
				position++;
			}
			for (int i = 0;i <= DefaultChildren.Count - 1;i++)
			{
				string key = (string)DefaultChildrenKeys[i];
				Node node = DefaultChildren[key];
				if (node.NameType == nBrowser.NodeType.DefaultBrowser)
				{
					node.Tree(xmlwriter, position);
				}
			}

			for (int i = 0;i <= Children.Count - 1;i++)
			{
				string key = (string)ChildrenKeys[i];
				Node node = Children[key];
				if (node.NameType == nBrowser.NodeType.Gateway)
				{
					node.Tree(xmlwriter, position);
				}
			}

			for (int i = 0;i <= Children.Count - 1;i++)
			{
				string key = (string)ChildrenKeys[i];
				Node node = Children[key];
				if (node.NameType == nBrowser.NodeType.Browser)
				{
					node.Tree(xmlwriter, position);
				}
			}
			if (position != int.MinValue)
			{
				position--;
			}
			xmlwriter.WriteEndElement();
			xmlwriter.WriteRaw(System.Environment.NewLine);
			if (position == 0)
			{
				xmlwriter.WriteEndDocument();
				xmlwriter.Flush();
			}
		}
		public System.Collections.ObjectModel.Collection<string> HeaderNames(System.Collections.ObjectModel.Collection<string> list)
		{
			if (Identification != null)
			{
				for (int i = 0;i <= Identification.Length - 1;i++)
				{
					if (Identification[i] == null)
					{
						continue;
					}
					if (Identification[i].Group == "header")
					{
						if (list.Contains(Identification[i].Name) == false)
						{
							list.Add(Identification[i].Name);
						}
					}
				}
			}
			if (Capture != null)
			{
				for (int i = 0;i <= Capture.Length - 1;i++)
				{
					if (Capture[i] == null)
					{
						continue;
					}
					if (Capture[i].Group == "header")
					{
						if (list.Contains(Capture[i].Name) == false)
						{
							list.Add(Capture[i].Name);
						}
					}
				}
			}
			for (int i = 0;i <= DefaultChildren.Count - 1;i++)
			{
				string key = (string)DefaultChildrenKeys[i];
				Node node = DefaultChildren[key];
				if (node.NameType == nBrowser.NodeType.DefaultBrowser)
				{
					list = node.HeaderNames(list);
				}
			}

			for (int i = 0;i <= Children.Count - 1;i++)
			{
				string key = (string)ChildrenKeys[i];
				Node node = Children[key];
				if (node.NameType == nBrowser.NodeType.Gateway)
				{
					list = node.HeaderNames(list);
				}
			}

			for (int i = 0;i <= Children.Count - 1;i++)
			{
				string key = (string)ChildrenKeys[i];
				Node node = Children[key];
				if (node.NameType == nBrowser.NodeType.Browser)
				{
					list = node.HeaderNames(list);
				}
			}
			return list;
		}
		
		/// <summary>
		/// Merge capabilities, captures, markupTextWriters, and adapters from another Node into this Node.
		/// </summary>
		/// <param name="n">node to merge with this node</param>
		public void MergeFrom(Node n)
		{
			if (n.Capabilities != null)
			{
				if (Capabilities == null)
					Capabilities =  new System.Collections.Specialized.NameValueCollection(n.Capabilities.Count, StringComparer.OrdinalIgnoreCase);
				foreach (string capName in n.Capabilities)
					Capabilities[capName] = n.Capabilities[capName];
			}
			
			int newLength = 0;
			if (Capture != null)
				newLength += Capture.Length;
			if (n.Capture != null)
				newLength += n.Capture.Length;
			Identification[] newCapture = new Identification[newLength];
			if (Capture != null)
				Array.Copy(Capture, 0, newCapture, 0, Capture.Length);
			if (n.Capture != null)
				Array.Copy(n.Capture, 0, newCapture, (Capture != null ? Capture.Length : 0), n.Capture.Length);
			Capture = newCapture;
			
			if (n.MarkupTextWriterType != null && n.MarkupTextWriterType.Length > 0)
				MarkupTextWriterType = n.MarkupTextWriterType;
			
			if (n.Adapter != null)
			{
				if (Adapter == null)
					Adapter = new System.Collections.Specialized.NameValueCollection();
				foreach (string controlType in n.Adapter)
					Adapter[controlType] = n.Adapter[controlType];
			}
		}
	}
}
#endif
