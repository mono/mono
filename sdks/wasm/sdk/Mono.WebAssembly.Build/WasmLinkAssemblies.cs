// Copyright (c) Microsoft Corporation. All rights reserved.
//
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Xamarin.ProcessControl;

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

			if (!Enum.TryParse<WasmLinkMode> (LinkMode, out var linkme)) {
				Log.LogError ("LinkMode is invalid.");
				return false;
			}


			return base.ValidateParameters ();
		}

		protected override string GenerateCommandLineCommands ()
		{
			ProcessArguments arguments = ProcessArguments.Create ("--verbose");

			// add exclude features
			arguments = arguments.AddRange ("--exclude-feature", "remoting", "--exclude-feature", "com", "--exclude-feature", "etw");

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

			arguments = arguments.AddRange ("-c", coremode, "-u", usermode);

			//the linker doesn't consider these core by default
			arguments = arguments.AddRange ("-p", coremode, "WebAssembly.Bindings");
			arguments = arguments.AddRange ("-p", coremode, "WebAssembly.Net.Http");
			arguments = arguments.AddRange ("-p", coremode, "WebAssembly.Net.WebSockets");

			if (!string.IsNullOrEmpty (LinkSkip)) {
				var skips = LinkSkip.Split (new[] { ';', ',', ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
				foreach (var s in skips) {
					arguments = arguments.AddRange ("-p", "copy", s);
				}
			}

			arguments = arguments.AddRange ("-out", OutputDir);

			arguments = arguments.AddRange ("-d", FrameworkDir);

			arguments = arguments.AddRange ("-d", Path.Combine(FrameworkDir, "Facades"));

			arguments = arguments.AddRange ("-b", Debug.ToString());
			arguments = arguments.AddRange ("-v", Debug.ToString());

			arguments = arguments.AddRange ("-a", RootAssembly[0].GetMetadata ("FullPath"));

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
					if (frameworkAssemblies.Contains (Path.GetFileNameWithoutExtension (p))) {
						continue;
					}
					arguments = arguments.AddRange ("-r", p);
				}
			}

			if (string.IsNullOrEmpty (I18n)) {
				arguments = arguments.AddRange ("-l", "none");
			} else {
				var vals = I18n.Split (new[] { ',', ';', ' ', '\r', '\n', '\t' });
				arguments = arguments.AddRange ("-l", string.Join (",", vals));
			}

			return arguments.ToString ();
		}
	}

	public enum WasmLinkMode
	{
		None,
		SdkOnly,
		Full
	}
}
