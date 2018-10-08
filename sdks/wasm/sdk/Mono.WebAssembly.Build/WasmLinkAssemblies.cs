// Copyright (c) Microsoft Corporation. All rights reserved.
//
// Licensed under the MIT license.

using System;
using System.IO;
using System.Text;
using Microsoft.Build.Utilities;

namespace Mono.WebAssembly.Build
{
    class WasmLinkAssemblies : ToolTask
    {
        public TaskItem[] Assemblies { get; set; }
        public string FrameworkDir { get; set; }
        public string OutputDir { get; set; }

        protected override string ToolName => "monolinker";

        protected override string GenerateFullPathToTool()
        {
            var dir = Path.GetDirectoryName(GetType().Assembly.Location);
            return Path.Combine(dir, "monolinker.exe");
        }

        protected override bool ValidateParameters()
        {
            if (string.IsNullOrEmpty(OutputDir))
            {
                Log.LogError("OutputDir is required");
                return false;
            }

            if (string.IsNullOrEmpty(FrameworkDir))
            {
                Log.LogError("FrameworkDir is required");
                return false;
            }

            return base.ValidateParameters();
        }

        protected override string GenerateCommandLineCommands()
        {
            var sb = new StringBuilder();

            sb.Append(" -verbose");
            sb.Append(" -c copyused");
            sb.Append(" -u copy");
            sb.AppendFormat(" -out \"{0}\"", OutputDir);
            sb.AppendFormat(" -d \"{0}\"", FrameworkDir);

            return sb.ToString();
        }
    }
}
