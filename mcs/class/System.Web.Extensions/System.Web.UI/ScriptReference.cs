//
// ScriptReference.cs
//
// Author:
//   Igor Zelmanovich <igorz@mainsoft.com>
//
// (C) 2007 Mainsoft, Inc.  http://www.mainsoft.com
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
using System.ComponentModel;
using System.Reflection;
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

		internal ScriptMode ScriptModeInternal {
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

		[MonoTODO ("Compression not supported yet.")]
		protected internal override string GetUrl (ScriptManager scriptManager, bool zip)
		{
			bool isDebugMode = scriptManager.IsDeploymentRetail ? false :
				(ScriptModeInternal == ScriptMode.Inherit ? scriptManager.IsDebuggingEnabled : (ScriptModeInternal == ScriptMode.Debug));
			string path = Path;
			string url = String.Empty;
			
			if (!String.IsNullOrEmpty (path)) {
				url = GetScriptName (path, isDebugMode, scriptManager.EnableScriptLocalization ? ResourceUICultures : null);
			} else if (!String.IsNullOrEmpty (Name)) {
				Assembly assembly;
				string assemblyName = this.Assembly;
				
				if (String.IsNullOrEmpty (assemblyName))
					assembly = typeof (ScriptManager).Assembly;
				else
					assembly = global::System.Reflection.Assembly.Load (assemblyName);
				string name = GetScriptName (Name, isDebugMode, null);
				string scriptPath = scriptManager.ScriptPath;
				if (IgnoreScriptPath || String.IsNullOrEmpty (scriptPath))
					url = ScriptResourceHandler.GetResourceUrl (assembly, name, NotifyScriptLoaded);
				else {
					AssemblyName an = assembly.GetName ();
					url = scriptManager.ResolveClientUrl (String.Concat (VirtualPathUtility.AppendTrailingSlash (scriptPath), an.Name, '/', an.Version, '/', name));
				}
			} else {
				throw new InvalidOperationException ("Name and Path cannot both be empty.");
			}

			return url;
		}

		static string GetScriptName (string releaseName, bool isDebugMode, string [] supportedUICultures) {
			if (!isDebugMode && (supportedUICultures == null || supportedUICultures.Length == 0))
				return releaseName;

			if (releaseName.Length < 3 || !releaseName.EndsWith (".js", StringComparison.OrdinalIgnoreCase))
				throw new InvalidOperationException (String.Format ("'{0}' is not a valid script path.  The path must end in '.js'.", releaseName));

			StringBuilder sb = new StringBuilder (releaseName);
			sb.Length -= 3;
			if (isDebugMode)
				sb.Append (".debug");
			string culture = Thread.CurrentThread.CurrentUICulture.Name;
			if (supportedUICultures != null && Array.IndexOf<string> (supportedUICultures, culture) >= 0)
				sb.AppendFormat (".{0}", culture);
			sb.Append (".js");

			return sb.ToString ();
		}
		
		protected internal override bool IsFromSystemWebExtensions ()
		{
			return false;
		}
		
		public override string ToString ()
		{
			return Name.Length > 0 ? Name : Path;
		}
	}
}