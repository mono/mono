//
// Authors:
//   Marek Habersack <grendel@twistedcode.net>
//
// (C) 2011 Novell, Inc (http://novell.com/)
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
#if NET_3_5
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Handlers;
using System.Web.Hosting;

namespace System.Web.UI
{
	[DefaultProperty ("Path")]
	public class CompositeScriptReference : ScriptReferenceBase
	{
		public const string COMPOSITE_SCRIPT_REFERENCE_PREFIX = "CSR:";

		static SplitOrderedList <string, List <CompositeEntry>> entriesCache;
		
		ScriptReferenceCollection scripts;
		
		[PersistenceMode (PersistenceMode.InnerProperty)]
		[Editor ("System.Web.UI.Design.CollectionEditorBase, " + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, "+ Consts.AssemblySystem_Design)]
		[MergableProperty (false)]
		[Category ("Behavior")]
		[DefaultValue (null)]
		[NotifyParentProperty (true)]	
		public ScriptReferenceCollection Scripts {
			get {
				if (scripts == null)
					scripts = new ScriptReferenceCollection ();
				return scripts;
			}
		}

		static CompositeScriptReference ()
		{
			entriesCache = new SplitOrderedList <string, List <CompositeEntry>> (StringComparer.Ordinal);
		}

		internal static List <CompositeEntry> GetCompositeScriptEntries (string url)
		{
			if (String.IsNullOrEmpty (url) || entriesCache.Count == 0)
				return null;
			
			List <CompositeEntry> ret;
			if (!entriesCache.Find ((uint)url.GetHashCode (), url, out ret))
				return null;

			return ret;
		}
		
		protected internal override string GetUrl (ScriptManager scriptManager, bool zip)
		{
			if (scriptManager == null)
				// .NET emulation...
				throw new NullReferenceException (".NET emulation");
			
			var url = new StringBuilder (COMPOSITE_SCRIPT_REFERENCE_PREFIX);
			string path;
			string name;
			CompositeEntry entry;
			List <CompositeEntry> entries = null;
			WebResourceAttribute wra;
			
			foreach (ScriptReference sr in Scripts) {
				if (sr == null)
					continue;

				name = sr.Name;
				if (!String.IsNullOrEmpty (name)) {
					Assembly assembly = sr.ResolvedAssembly;
					name = GetScriptName (name, sr.IsDebugMode (scriptManager), null, assembly, out wra);
					path = scriptManager.ScriptPath;
					if (sr.IgnoreScriptPath || String.IsNullOrEmpty (path)) {
						entry = new CompositeEntry {
							Assembly = assembly,
							NameOrPath = name,
							Attribute = wra
						};
					} else {
						AssemblyName an = assembly.GetName ();
						entry = new CompositeEntry {
							NameOrPath = String.Concat (VirtualPathUtility.AppendTrailingSlash (path), an.Name, '/', an.Version, '/', name),
							Attribute = wra
						};
					}
				} else if (!String.IsNullOrEmpty ((path = sr.Path))) {
					bool notFound = false;
					name = GetScriptName (path, sr.IsDebugMode (scriptManager), scriptManager.EnableScriptLocalization ? ResourceUICultures : null, null, out wra);
					if (!HostingEnvironment.HaveCustomVPP)
						notFound = !File.Exists (HostingEnvironment.MapPath (name));
					else 
						notFound = !HostingEnvironment.VirtualPathProvider.FileExists (name);

					if (notFound)
						throw new HttpException ("Web resource '" + name + "' was not found.");
					
					entry = new CompositeEntry {
						NameOrPath = name
					};
				} else
					entry = null;

				if (entry != null) {
					if (entries == null)
						entries = new List <CompositeEntry> ();
					entries.Add (entry);
					url.Append (entry.GetHashCode ().ToString ("x"));
					entry = null;
				}
			}
			
			if (entries == null || entries.Count == 0)
				return String.Empty;

			string ret = ScriptResourceHandler.GetResourceUrl (ThisAssembly, url.ToString (), NotifyScriptLoaded);
			entriesCache.InsertOrUpdate ((uint)ret.GetHashCode (), ret, entries, entries);
			return ret;
		}
#if NET_4_0
		protected internal override bool IsAjaxFrameworkScript (ScriptManager scriptManager)
		{
			return false;
		}
		
		[Obsolete ("Use IsAjaxFrameworkScript(ScriptManager)")]
#endif
		protected internal override bool IsFromSystemWebExtensions ()
		{
			if (scripts == null || scripts.Count == 0)
				return false;

			Assembly myAssembly = ThisAssembly;
			foreach (ScriptReference sr in scripts)
				if (sr.ResolvedAssembly == myAssembly)
					return true;

			return false;
		}

		internal bool HaveScripts ()
		{
			return (scripts != null && scripts.Count > 0);
		}
	}
}
#endif