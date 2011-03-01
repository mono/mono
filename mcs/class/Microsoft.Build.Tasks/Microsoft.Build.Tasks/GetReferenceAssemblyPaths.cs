//
// GetReferenceAssemblyPaths.cs: Gets the target framework directories corresponding
// to target framework moniker
//
// Author:
//   Ankit Jain (jankit@novell.com)
//
// Copyright 2011 Novell, Inc (http://www.novell.com)
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

using System;
using Microsoft.Build.Framework;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Collections.Generic;

using Mono.XBuild.Utilities;

#if NET_4_0

namespace Microsoft.Build.Tasks
{
	public class GetReferenceAssemblyPaths : TaskExtension
	{
		static string framework_base_path;
		static string PathSeparatorAsString = Path.PathSeparator.ToString ();
		const string MacOSXExternalXBuildDir = "/Library/Frameworks/Mono.framework/External/xbuild-frameworks";

		public GetReferenceAssemblyPaths ()
		{
		}

		public override bool Execute ()
		{
			FrameworkMoniker moniker = null;
			if (!TryParseTargetFrameworkMoniker (TargetFrameworkMoniker, out moniker))
				return false;

			var framework = GetFrameworkDirectoriesForMoniker (moniker);
			if (framework == null) {
				Log.LogWarning ("Unable to find framework corresponding to the target framework moniker '{0}'. " +
						"Framework assembly references will be resolved from the GAC, which might not be " +
						"the intended behavior.", TargetFrameworkMoniker);
				return true;
			}

			ReferenceAssemblyPaths = FullFrameworkReferenceAssemblyPaths = framework.Directories;
			TargetFrameworkMonikerDisplayName = framework.DisplayName;

			return true;
		}

		Framework GetFrameworkDirectoriesForMoniker (FrameworkMoniker moniker)
		{
			string dirs = String.Join (PathSeparatorAsString, new string [] {
							Environment.GetEnvironmentVariable ("XBUILD_FRAMEWORK_FOLDERS_PATH") ?? String.Empty,
							MSBuildUtils.RunningOnMac ? MacOSXExternalXBuildDir : String.Empty,
							RootPath,
							DefaultFrameworksBasePath });

			string [] paths = dirs.Split (new char [] {Path.PathSeparator}, StringSplitOptions.RemoveEmptyEntries);
			foreach (string path in paths) {
				var framework = GetFrameworkDirectoriesForMoniker (moniker, path);
				if (framework != null)
					return framework;
			}

			return null;
		}

		//@base_path must be absolute
		Framework GetFrameworkDirectoriesForMoniker (FrameworkMoniker moniker, string base_path)
		{
			if (String.IsNullOrEmpty (base_path)) {
				Log.LogMessage (MessageImportance.Low, "Invalid *empty* base path, ignoring. " + Environment.StackTrace);
				return null;
			}

			Log.LogMessage (MessageImportance.Low, "Looking for framework '{0}' in root path '{1}'",
					moniker, base_path);
			string framework_path = Path.Combine (base_path, Path.Combine (moniker.Identifier, moniker.Version));
			if (!String.IsNullOrEmpty (moniker.Profile))
				framework_path = Path.Combine (framework_path, moniker.Profile);

			string redistlist_dir = Path.Combine (framework_path, "RedistList");
			string framework_list = Path.Combine (redistlist_dir, "FrameworkList.xml");
			if (!File.Exists (framework_list)) {
				Log.LogMessage (MessageImportance.Low,
							"Unable to find framework definition file '{0}' for Target Framework Moniker '{1}'",
							framework_list, moniker);
				return null;
			}

			Log.LogMessage (MessageImportance.Low, "Found framework definition list '{0}' for framework '{1}'",
					framework_list, moniker);
			XmlReader xr = XmlReader.Create (framework_list);
			try {
				xr.MoveToContent ();
				if (xr.LocalName != "FileList") {
					Log.LogMessage (MessageImportance.Low, "Invalid frameworklist '{0}', expected a 'FileList' root element.",
							framework_list);
					return null;
				}

				var framework = new Framework ();
				framework.DisplayName = xr.GetAttribute ("Name");
				string framework_dir = xr.GetAttribute ("TargetFrameworkDirectory");
				if (String.IsNullOrEmpty (framework_dir))
					framework_dir = Path.Combine (redistlist_dir, "..");
				else
					framework_dir = Path.Combine (redistlist_dir, framework_dir);

				var directories = new List<string> ();
				directories.Add (MSBuildUtils.FromMSBuildPath (framework_dir));

				string include = xr.GetAttribute ("IncludeFramework");
				if (!String.IsNullOrEmpty (include)) {
					var included_framework = GetFrameworkDirectoriesForMoniker (new FrameworkMoniker (moniker.Identifier, include, null));

					if (included_framework != null && included_framework.Directories != null)
						directories.AddRange (included_framework.Directories);
				}

				framework.Directories = directories.ToArray ();

				return framework;
			} catch (XmlException xe) {
				Log.LogWarning ("Error reading framework definition file '{0}': {1}", framework_list, xe.Message);
				Log.LogMessage (MessageImportance.Low, "Error reading framework definition file '{0}': {1}", framework_list,
						xe.ToString ());
				return null;
			} finally {
				if (xr != null)
					((IDisposable)xr).Dispose ();
			}
		}

		bool TryParseTargetFrameworkMoniker (string moniker_literal, out FrameworkMoniker moniker)
		{
			moniker = null;
			if (String.IsNullOrEmpty (moniker_literal))
				throw new ArgumentException ("Empty moniker string");

			string [] parts = moniker_literal.Split (new char [] {','}, StringSplitOptions.RemoveEmptyEntries);

			if (parts.Length < 2 || parts.Length > 3) {
				LogInvalidMonikerError (null, moniker_literal);
				return false;
			}

			string identifier = parts [0];
			string version = null;
			string profile = null;

			if (!parts [1].StartsWith ("Version=")) {
				LogInvalidMonikerError ("Invalid framework name", moniker_literal);
				return false;
			}

			version = parts [1].Substring (8);
			if (String.IsNullOrEmpty (version)) {
				LogInvalidMonikerError ("Invalid framework version", moniker_literal);
				return false;
			}

			if (parts.Length > 2) {
				if (!parts [2].StartsWith ("Profile=")) {
					LogInvalidMonikerError ("Invalid framework version", moniker_literal);
					return false;
				}

				profile = parts [2].Substring (8);
				if (String.IsNullOrEmpty (profile)) {
					LogInvalidMonikerError ("Invalid framework profile", moniker_literal);
					return false;
				}
			}

			moniker = new FrameworkMoniker (identifier, version, profile);
			return true;
		}

		void LogInvalidMonikerError (string msg, string moniker_literal)
		{
			if (msg != null)
				Log.LogError ("{0} in the Target Framework Moniker '{1}'. Expected format: 'Identifier,Version=<version>[,Profile=<profile>]'. " +
							"It should have either 2 or 3 comma separated components.", msg, moniker_literal);
			else
				Log.LogError ("Invalid Target Framework Moniker '{0}'. Expected format: 'Identifier,Version=<version>[,Profile=<profile>]'. " +
							"It should have either 2 or 3 comma separated components.", moniker_literal);
		}

		[Required]
		public string TargetFrameworkMoniker { get; set; }

		public string RootPath { get; set; }

		public bool BypassFrameworkInstallChecks { get; set; }

		[Output]
		public string TargetFrameworkMonikerDisplayName { get; set; }

		[Output]
		public string[] ReferenceAssemblyPaths { get; set; }

		[Output]
		public string[] FullFrameworkReferenceAssemblyPaths { get; set; }

		static string DefaultFrameworksBasePath {
			get {
				if (framework_base_path == null) {
					// NOTE: code from mcs/tools/gacutil/driver.cs
					PropertyInfo gac = typeof (System.Environment).GetProperty (
							"GacPath", BindingFlags.Static | BindingFlags.NonPublic);

					if (gac != null) {
						MethodInfo get_gac = gac.GetGetMethod (true);
						string gac_path = (string) get_gac.Invoke (null, null);
						framework_base_path = Path.GetFullPath (Path.Combine (
									gac_path, Path.Combine ("..", "xbuild-frameworks")));
					}
				}
				return framework_base_path;
			}
		}
	}

	class FrameworkMoniker {
		public readonly string Identifier;
		public readonly string Version;
		public readonly string Profile;

		public FrameworkMoniker (string identifier, string version, string profile)
		{
			this.Identifier = identifier;
			this.Version = version;
			this.Profile = profile;
		}

		public override string ToString ()
		{
			if (String.IsNullOrEmpty (Profile))
				return String.Format ("{0},Version={1}", Identifier, Version);
			return  String.Format ("{0},Version={1},Profile={2}", Identifier, Version, Profile);
		}
	}

	class Framework {
		public string[] Directories;
		public string DisplayName;
	}
}

#endif
