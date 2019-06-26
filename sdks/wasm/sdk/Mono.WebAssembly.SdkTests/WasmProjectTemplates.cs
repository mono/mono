//
// Copyright (c) Microsoft Corp
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

using System;
using System.IO;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Utilities.ProjectCreation;

namespace Mono.WebAssembly.SdkTests
{
	public static class WasmProjectTemplates
	{
		public static ProjectCreator WasmProject (
			this ProjectCreatorTemplates templates,
			string[] projectReferences,
			string path = null,
			string defaultTargets = null,
			string initialTargets = null,
			string sdk = null,
			string toolsVersion = null,
			string treatAsLocalProperty = null,
			ProjectCollection projectCollection = null,
			NewProjectFileOptions? projectFileOptions = NewProjectFileOptions.None)
		{
			//this is in wasm\sdk\Mono.WebAssembly.SdkTest\bin\Debug\net461
			string currentDirectory = Environment.CurrentDirectory;
			var monoSdkDir = Path.GetFullPath (Path.Combine (currentDirectory, "..", "..", "..", "..", "..", ".."));
			var sdkPropsDir = Path.Combine (monoSdkDir, "wasm", "sdk", "Mono.WebAssembly.Sdk", "sdk");

			return ProjectCreator.Create (
					path,
					defaultTargets,
					initialTargets,
					sdk,
					toolsVersion,
					treatAsLocalProperty,
					projectCollection,
					projectFileOptions)
				.PropertyGroup ()
				.Property ("MonoWasmSdkPath", monoSdkDir)
				.Import (Path.Combine (sdkPropsDir, "Sdk.props"))
				.ForEach (projectReferences, (projectReference, i) => {
					i.ItemProjectReference (projectReference);
				})
				.Import (Path.Combine (sdkPropsDir, "Sdk.targets"));
		}
	}
}
