//
// Copyright (c) 2018 Microsoft Corp
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System.IO;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System.Linq;
namespace Mono.WebAssembly.Build
{
	public class WasmCreateRuntimeJs : Task
	{
		public string VfsPrefix { get; set; }
		public string DeployPrefix { get; set; }
		public bool EnableDebugging { get; set; }
		public bool InitBindings { get; set; }

		[Required]
		public ITaskItem[] FileList { get; set; }

		[Required]
		public string OutputFile { get; set; }

		public override bool Execute ()
		{
			var template = new RuntimeJs {
				EnableDebugging = EnableDebugging,
				VfsPrefix = VfsPrefix,
				DeployPrefix = DeployPrefix,
				FileList = FileList.Select (f => f.GetMetadata ("Filename") + f.GetMetadata ("Extension"))
			};

			var text = template.TransformText ();

			Directory.CreateDirectory (Path.GetDirectoryName (OutputFile));
			File.WriteAllText (OutputFile, text);

			return true;
		}
	}
}
