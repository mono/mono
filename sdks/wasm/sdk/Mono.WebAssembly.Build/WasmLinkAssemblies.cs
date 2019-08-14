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
using System.Runtime.InteropServices;


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
		/// Full paths to managed assemblies from packages to run against.
		/// </summary>
		public ITaskItem[] RuntimeCopyLocalAssemblies { get; set; }

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

		/// <summary>
		/// Custom Linker Configurations
		/// </summary>
		public ITaskItem[] LinkDescriptions { get; set; }


		protected override string ToolName => (InvokeLinkerUsingMono) ? "mono" : "monolinker.exe";
		bool IsWindows => System.Runtime.InteropServices.RuntimeInformation
                                               .IsOSPlatform(OSPlatform.Windows);
		bool InvokeLinkerUsingMono => (!IsWindows && !String.IsNullOrEmpty(GetNetCoreVersion()));
		protected override string GenerateFullPathToTool ()
		{
			Log.LogMessage(MessageImportance.High, $"InvokeLinkerUsingMono {InvokeLinkerUsingMono}");
			if (!InvokeLinkerUsingMono) {
				var toolsPath = GetPathToMonoLinker ();
				Log.LogMessage(MessageImportance.High, $"Running monolinker from {toolsPath}");
				return toolsPath;
			}
			else
				return ToolName;
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

			ProcessArguments arguments = null;
			
			if (!InvokeLinkerUsingMono) {
				arguments = ProcessArguments.Create ("--verbose");
			}
			else {
				var toolsPath = GetPathToMonoLinker();
				Log.LogMessage(MessageImportance.High, $"Running monolinker from {toolsPath}.");
				arguments = ProcessArguments.Create (toolsPath);
				arguments = arguments.Add("--verbose");
			}

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

			// add custom link descriptions
			// to ensure the type, methods and/or fields are not eliminated from your application.
			if (LinkDescriptions != null) {
				foreach (var desc in LinkDescriptions) {
					var l = desc.GetMetadata ("FullPath");
					arguments = arguments.AddRange ("-x", l);
				}
			}

			arguments = arguments.AddRange ("-a", RootAssembly[0].GetMetadata ("FullPath"));

			//we'll normally have to check most of the files because the SDK references most framework asm by default
			//so let's enumerate upfront
			var frameworkAssemblies = new HashSet<string> (StringComparer.OrdinalIgnoreCase);
			foreach (var f in Directory.EnumerateFiles (FrameworkDir)) {
				frameworkAssemblies.Add (Path.GetFileNameWithoutExtension (f));
			}
			foreach (var f in Directory.EnumerateFiles (Path.Combine (FrameworkDir, "Facades"))) {
				frameworkAssemblies.Add (Path.GetFileNameWithoutExtension (f));
			}

			// Load the runtime assemblies to be replaced in the references below
			var runtimeCopyLocal = new Dictionary<string, string> (StringComparer.OrdinalIgnoreCase);
			if (RuntimeCopyLocalAssemblies != null) {
				foreach (var copyAsm in RuntimeCopyLocalAssemblies) {
					var p = copyAsm.GetMetadata ("FullPath");
					
					if (frameworkAssemblies.Contains (Path.GetFileNameWithoutExtension (p))) {
						continue;
					}
					runtimeCopyLocal.Add(Path.GetFileNameWithoutExtension (p), p);
				}
			}

			//add references for non-framework assemblies
			if (Assemblies != null) {
				foreach (var asm in Assemblies) {
					var p = asm.GetMetadata ("FullPath");
					if (frameworkAssemblies.Contains (Path.GetFileNameWithoutExtension (p))) {
						continue;
					}

					if (runtimeCopyLocal.TryGetValue(Path.GetFileNameWithoutExtension (p), out var runtimePath))
					{
						// Just in case
						if (File.Exists(runtimePath))
							p = runtimePath;
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

		static string GetNetCoreVersion()
		{
			var assembly = typeof(System.Runtime.GCSettings).Assembly;
			var assemblyPath = assembly.CodeBase.Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);
			int netCoreAppIndex = Array.IndexOf(assemblyPath, "Microsoft.NETCore.App");
			if (netCoreAppIndex > 0 && netCoreAppIndex < assemblyPath.Length - 2)
				return assemblyPath[netCoreAppIndex + 1];
			return null;
		}

		string GetPathToMonoLinker () {
			var dir = Path.GetDirectoryName (GetType ().Assembly.Location);
			// Check if coming from nuget or local
			var toolsPath = Path.Combine (Path.GetDirectoryName( dir ), "tools", "monolinker.exe");
			if (!File.Exists(toolsPath))
				toolsPath = Path.GetFullPath(Path.Combine (GetParentDirectoryOf(dir,6), "out", "wasm-bcl", "wasm_tools", "monolinker.exe"));
			return toolsPath;
		} 

		static string GetParentDirectoryOf (string path, int up)
		{
			if (up == 0)
				return path;
			for (int i = path.Length -1; i >= 0; i--) {
				if (path[i] == Path.DirectorySeparatorChar) {
					up--;
					if (up == 0)
						return path.Substring (0, i);
				}
			}
			return null;
		}
	}

	public enum WasmLinkMode
	{
		None,
		SdkOnly,
		Full
	}
}
