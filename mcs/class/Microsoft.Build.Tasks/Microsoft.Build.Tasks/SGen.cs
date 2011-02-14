//
// SGen.cs
//
// Author:
//   Marek Sieradzki (marek.sieradzki@gmail.com)
//
// (C) 2006 Marek Sieradzki
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

#if NET_2_0

using System;
using Microsoft.Build.Framework; 
using Microsoft.Build.Tasks;
using Mono.XBuild.Utilities;

namespace Microsoft.Build.Tasks {
	public class SGen : ToolTaskExtension {
	
		string buildAssemblyName;
		string buildAssemblyPath;
		bool delaySign;
		string keyContainer;
		string keyFile;
		string [] references;
		ITaskItem [] serializationAssembly;
		string serializationAssemblyName;
		bool shouldGenerateSerializer;
		bool useProxyTypes;

		public SGen ()
		{
		}

		[MonoTODO]
		[Required]
		public string BuildAssemblyName {
			get { return buildAssemblyName; }
			set { buildAssemblyName = value; }
		}

		[MonoTODO]
		[Required]
		public string BuildAssemblyPath {
			get { return buildAssemblyPath; }
			set { buildAssemblyPath = value; }
		}

		[MonoTODO]
		public bool DelaySign {
			get { return delaySign; }
			set { delaySign = value; }
		}

		[MonoTODO]
		public string KeyContainer {
			get { return keyContainer; }
			set { keyContainer = value; }
		}

		[MonoTODO]
		public string KeyFile {
			get { return keyFile; }
			set { keyFile = value; }
		}

		[MonoTODO]
		public string [] References {
			get { return references; }
			set { references = value; }
		}

		[MonoTODO]
		[Output]
		public ITaskItem [] SerializationAssembly {
			get { return serializationAssembly; }
			set { serializationAssembly = value; }
		}

		[MonoTODO]
		public string SerializationAssemblyName {
			get { return serializationAssemblyName; }
		}

		[MonoTODO]
		[Required]
		public bool ShouldGenerateSerializer {
			get { return shouldGenerateSerializer; }
			set { shouldGenerateSerializer = value; }
		}

		[MonoTODO]
		[Required]
		public bool UseProxyTypes {
			get { return useProxyTypes; }
			set { useProxyTypes = value; }
		}

		[MonoTODO]
		protected override string ToolName {
			get { return MSBuildUtils.RunningOnWindows ? "sgen.bat" : "sgen"; }
		}

		[MonoTODO]
		protected override string GenerateCommandLineCommands ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override string GenerateFullPathToTool ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override bool SkipTaskExecution ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override bool ValidateParameters ()
		{
			throw new NotImplementedException ();
		}
	}
}

#endif
