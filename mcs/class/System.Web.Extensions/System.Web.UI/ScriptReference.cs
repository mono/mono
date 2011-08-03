//
// ScriptReference.cs
//
// Authors:
//   Igor Zelmanovich <igorz@mainsoft.com>
//   Marek Habersack <grendel@twistedcode.net>
//
// (C) 2007 Mainsoft, Inc.  http://www.mainsoft.com
// (C) 2011 Novell, Inc. http://novell.com/
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
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading;
using System.Web.Handlers;
using System.Web.UI.WebControls;

namespace System.Web.UI
{
	[DefaultProperty ("Path")]
	public class ScriptReference : ScriptReferenceBase
	{
		string _name;
		string _assembly;
		bool _ignoreScriptPath;
		Assembly _resolvedAssembly;
		
		public ScriptReference ()
		{
		}

		public ScriptReference (string path)
		{
			this.Path = path;
		}

		public ScriptReference (string name, string assembly)
		{
			_name = name;
			_assembly = assembly;
		}

		public string Assembly {
			get {
				return _assembly;
			}
			set {
				_assembly = value;
				_resolvedAssembly = null;
			}
		}

		internal Assembly ResolvedAssembly {
			get {
				if (_resolvedAssembly == null) {
					string assemblyName = this.Assembly;
				
					if (String.IsNullOrEmpty (assemblyName))
						_resolvedAssembly = typeof (ScriptManager).Assembly;
					else
						_resolvedAssembly = global::System.Reflection.Assembly.Load (assemblyName);
				}
				return _resolvedAssembly;
			}
		}

		ScriptMode ScriptModeInternal {
			get {
				if (ScriptMode == ScriptMode.Auto) {
					if (!String.IsNullOrEmpty (Name))
						return ScriptMode.Inherit;
					else
						return ScriptMode.Release;
				}
				else
					return ScriptMode;
			}
		}
		
		public bool IgnoreScriptPath {
			get {
				return _ignoreScriptPath;
			}
			set {
				_ignoreScriptPath = value;
			}
		}

		public string Name {
			get {
				return _name != null ? _name : String.Empty;
			}
			set {
				_name = value;
			}
		}

		internal bool IsDebugMode (ScriptManager scriptManager)
		{
			if (scriptManager == null)
				return ScriptModeInternal == ScriptMode.Debug;
			
			if (scriptManager.IsDeploymentRetail)
				return false;

			switch (ScriptModeInternal) {
				case ScriptMode.Inherit:
					return scriptManager.IsDebuggingEnabled;

				case ScriptMode.Debug:
					return true;

				default:
					return false;
			}
		}
		
		[MonoTODO ("Compression not supported yet.")]
		protected internal override string GetUrl (ScriptManager scriptManager, bool zip)
		{
			bool isDebugMode = IsDebugMode (scriptManager);
			string path;
			string url = String.Empty;
			string name = Name;
			WebResourceAttribute wra;
			
			// LAMESPEC: Name property takes precedence
			if (!String.IsNullOrEmpty (name)) {
				Assembly assembly = ResolvedAssembly;
				name = GetScriptName (name, isDebugMode, null, assembly, out wra);
				path = scriptManager.ScriptPath;
				if (IgnoreScriptPath || String.IsNullOrEmpty (path))
					url = ScriptResourceHandler.GetResourceUrl (assembly, name, NotifyScriptLoaded);
				else {
					AssemblyName an = assembly.GetName ();
					url = scriptManager.ResolveClientUrl (String.Concat (VirtualPathUtility.AppendTrailingSlash (path), an.Name, '/', an.Version, '/', name));
				}
			} else if (!String.IsNullOrEmpty ((path = Path))) {
				url = GetScriptName (path, isDebugMode, scriptManager.EnableScriptLocalization ? ResourceUICultures : null, null, out wra);
			} else {
				throw new InvalidOperationException ("Name and Path cannot both be empty.");
			}

			return url;
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
			return ResolvedAssembly == ThisAssembly;
		}
		
		public override string ToString ()
		{
			return Name.Length > 0 ? Name : Path;
		}
	}
}