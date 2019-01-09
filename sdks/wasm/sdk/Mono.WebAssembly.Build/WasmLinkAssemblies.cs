﻿// Copyright (c) Microsoft Corporation. All rights reserved.
//
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Mono.WebAssembly.Build
{
	public class WasmLinkAssemblies : ToolTask
	{
		/// <summary>
		/// The root application assembly.
		/// </summary>
		[Required]
		public ITaskItem[] RootAssembly { get; set; }

		/// <summary>
		/// Any additional assemblies to be considered by the linker.
		/// </summary>
		public ITaskItem[] Assemblies { get; set; }

		/// <summary>
		/// The directory containing the framework assemblies.
		/// </summary>
		[Required]
		public string FrameworkDir { get; set; }

		/// <summary>
		/// The directory into which to output the linked assemblies.
		/// </summary>
		[Required]
		public string OutputDir { get; set; }

		/// <summary>
		/// Controls which kinds of assemblies are linked.
		/// </summary>
		// HACK: for some reason MSBuild doesn't like us typing this as an enum
		public string LinkMode { get; set; }

		/// <summary>
		/// Semicolon separated list of assembly names that should not be linked.
		/// </summary>
		public string LinkSkip { get; set; }

		/// <summary>
		/// Whether to include debug information
		/// </summary>
		public bool Debug { get; set; }

		/// <summary>
		/// Internationalization code pages to be supported
		/// </summary>
		public string I18n { get; set; }

		protected override string ToolName => "monolinker";

		protected override string GenerateFullPathToTool ()
		{
			var dir = Path.GetDirectoryName (GetType ().Assembly.Location);
			// Check if coming from nuget or local
			var toolsPath = Path.Combine (Path.GetDirectoryName( dir ), "tools", "monolinker.exe");
			if (File.Exists(toolsPath))
				return toolsPath;
			else 
				return Path.Combine (dir, "monolinker.exe");
		}

		protected override bool ValidateParameters ()
		{
			if (string.IsNullOrEmpty (OutputDir)) {
				Log.LogError ("OutputDir is required");
				return false;
			}

			if (string.IsNullOrEmpty (FrameworkDir)) {
				Log.LogError ("FrameworkDir is required");
				return false;
			}

			WasmLinkMode linkme;
			if (!Enum.TryParse<WasmLinkMode>(LinkMode, out linkme)) {
				Log.LogError ("LinkMode is invalid.");
				return false;
			}


			return base.ValidateParameters ();
		}

		protected override string GenerateCommandLineCommands ()
		{
			var sb = new StringBuilder ();

			sb.Append (" --verbose");

			string coremode, usermode;

			switch ((WasmLinkMode)Enum.Parse (typeof (WasmLinkMode), LinkMode)) {
			case WasmLinkMode.SdkOnly:
				coremode = "link";
				usermode = "copy";
				break;
			case WasmLinkMode.Full:
				coremode = "link";
				usermode = "link";
				break;
			default:
				coremode = "copyused";
				usermode = "copy";
				break;
			}

			sb.AppendFormat (" -c {0} -u {1}", coremode, usermode);

			//the linker doesn't consider these core by default
			sb.AppendFormat (" -p {0} netstandard -p {1} WebAssembly.Bindings -p {1} WebAssembly.Net.Http", coremode, usermode);

			if (!string.IsNullOrEmpty (LinkSkip)) {
				var skips = LinkSkip.Split (new[] { ';', ',', ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
				foreach (var s in skips) {
					sb.AppendFormat (" -p \"{0}\" copy", s);
				}
			}

			sb.AppendFormat (" -out \"{0}\"", OutputDir);
			sb.AppendFormat (" -d \"{0}\"", FrameworkDir);
			sb.AppendFormat (" -b {0} -v {0}", Debug);

			sb.AppendFormat (" -a \"{0}\"", RootAssembly[0].GetMetadata("FullPath"));

			//we'll normally have to check most of the because the SDK references most framework asm by default
			//so let's enumerate upfront
			var frameworkAssemblies = new HashSet<string> (StringComparer.OrdinalIgnoreCase);
			foreach (var f in Directory.EnumerateFiles (FrameworkDir)) {
				frameworkAssemblies.Add (Path.GetFileNameWithoutExtension (f));
			}
			foreach (var f in Directory.EnumerateFiles (Path.Combine (FrameworkDir, "Facades"))) {
				frameworkAssemblies.Add (Path.GetFileNameWithoutExtension (f));
			}

			//add references for non-framework assemblies
			if (Assemblies != null) {
				foreach (var asm in Assemblies) {
					var p = asm.GetMetadata ("FullPath");
					if (frameworkAssemblies.Contains(Path.GetFileNameWithoutExtension(p))) {
						continue;
					}
					sb.AppendFormat (" -r \"{0}\"", p);
				}
			}

			if (string.IsNullOrEmpty (I18n)) {
				sb.Append (" -l none");
			} else {
				var vals = I18n.Split (new[] { ',', ';', ' ', '\r', '\n', '\t' });
				sb.AppendFormat (" -l {0}", string.Join(",", vals));
			}

			return sb.ToString ();
		}
	}

	public enum WasmLinkMode
	{
		None,
		SdkOnly,
		Full
	}
}
