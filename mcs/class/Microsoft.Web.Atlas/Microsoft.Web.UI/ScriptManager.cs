//
// Microsoft.Web.UI.ScriptManager
//
// Author:
//   Chris Toshok (toshok@ximian.com)
//
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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

#if NET_2_0

using System;
using System.Collections;
using System.Collections.Generic;
using System.Web.UI;

namespace Microsoft.Web.UI
{
	class ScriptReference
	{
		public string scriptPath;
		public bool commonScript;
	}

	class ScriptNamespace
	{
		public string prefix;
		public string namespaceUri;
	}

	internal class XmlScriptNode : Control
	{
		ScriptManager mgr;

		public XmlScriptNode (ScriptManager mgr)
		{
			this.mgr = mgr;
		}

		protected override void Render (HtmlTextWriter writer)
		{
			ScriptTextWriter scriptwriter = new ScriptTextWriter (writer);

			scriptwriter.WriteStartElement ("script");
			scriptwriter.WriteAttributeString ("type", "text/xml-script");
			scriptwriter.WriteStartElement ("page");
			scriptwriter.WriteAttributeString ("xmlns:script", "http://schemas.microsoft.com/xml-script/2005");

			scriptwriter.WriteStartElement ("components");
			foreach (IScriptComponent component in mgr.Components) {
				if (((IScriptObject)component).Owner == null) // only render the toplevel script objects
					component.RenderScript (scriptwriter);
			}
			scriptwriter.WriteEndElement (); // components 

			scriptwriter.WriteStartElement ("references");
			foreach (string scriptPath in mgr.ScriptRefs.Keys) {
				if ((bool)mgr.ScriptRefs[scriptPath]) {
					scriptwriter.WriteStartElement ("add");
					scriptwriter.WriteAttributeString ("src", scriptPath);
					scriptwriter.WriteEndElement ();
				}
			}
			scriptwriter.WriteEndElement (); //references

			scriptwriter.WriteEndElement (); // page
			scriptwriter.WriteEndElement (); // script

			ScriptManager.Pages.Remove (Page);
		}
	}

	public class ScriptManager : Control, INamingContainer, IScriptComponentContainer
	{
		internal ScriptComponentCollection Components;
		internal Hashtable ScriptRefs;
		internal List<ScriptNamespace> Namespaces;

		internal static Hashtable Pages = new Hashtable();

		public ScriptManager ()
		{
			Components = new ScriptComponentCollection();
			Namespaces = new List<ScriptNamespace>();
			ScriptRefs = new Hashtable();
		}

		protected override void OnInit (EventArgs e)
		{
			base.OnInit(e);

			if (Page != null) {
				Pages.Add (Page, this);
				Page.Controls.Add (new XmlScriptNode(this));
			}
		}

		public static ScriptManager GetCurrentScriptManager (Page page)
		{
			return (ScriptManager)Pages[page];
		}

		public void RegisterComponent (IScriptComponent component)
		{
			Components.Add (component);
		}

		public void RegisterScriptNamespace (string prefix, string namespaceUri)
		{
			ScriptNamespace ns = new ScriptNamespace ();
			ns.prefix = prefix;
			ns.namespaceUri = namespaceUri;
			Namespaces.Add (ns);
		}

		public void RegisterScriptReference (string scriptPath, bool commonScript)
		{
			if (ScriptRefs.Contains (scriptPath))
				return;

			ScriptRefs.Add (scriptPath, commonScript);
		}

		public void RegisterScriptReference (string scriptPath)
		{
			RegisterScriptReference (scriptPath, false);
		}
	}
}

#endif
