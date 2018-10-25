// Copyright (c) Microsoft Corporation. All rights reserved.
//
// Licensed under the MIT license.

using System;
using System.IO;
using System.Text;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Mono.WebAssembly.Build
{
	class WasmLinkAssemblies : ToolTask
	{
		/// <summary>
		/// The application assemblies to be linked.
		/// </summary>
		[Required]
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
		public LinkMode LinkMode { get; set; }

		/// <summary>
		/// Semicolon separated list of assembly names that should not be linked.
		/// </summary>
		public string LinkSkip { get; set; }

		/// <summary>
		/// Whether to include debug information
		/// </summary>
		public bool Debug { get; set; }

		protected override string ToolName => "monolinker";

		protected override string GenerateFullPathToTool ()
		{
			var dir = Path.GetDirectoryName (GetType ().Assembly.Location);
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

			return base.ValidateParameters ();
		}

		protected override string GenerateCommandLineCommands ()
		{
			var sb = new StringBuilder ();

			sb.Append (" -verbose");

			switch (LinkMode) {
			case LinkMode.None:
				sb.Append (" -c copyused -u copy");
				break;
			case LinkMode.SdkOnly:
				sb.Append (" -c link -u copy");
				break;
			case LinkMode.Full:
				sb.Append (" -c link -u link");
				break;
			}

			var skips = LinkSkip.Split (new[] { ';', ',', ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
			foreach (var s in skips) {
				sb.AppendFormat (" -p {0} copy", s);
			}

			sb.AppendFormat (" -out \"{0}\"", OutputDir);
			sb.AppendFormat (" -d \"{0}\"", FrameworkDir);

			if (Debug) {
				sb.Append (" -b -v");
			}

			return sb.ToString ();
		}
	}

	public enum LinkMode
	{
		None,
		SdkOnly,
		Full
	}
}
