//
// IVbcHostObject.cs: Host object interface for VB.NET compiler.
//
// Author:
//   Marek Sieradzki (marek.sieradzki@gmail.com)
//
// (C) 2005 Marek Sieradzki
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

using Microsoft.Build.Framework;
using System;
using System.Runtime.InteropServices;

namespace Microsoft.Build.Tasks.Hosting {

	[InterfaceType (ComInterfaceType.InterfaceIsIUnknown)]
	[Guid ("7D7AC3BE-253A-40e8-A3FF-357D0DA7C47A")]
	[ComVisible (true)]
	public interface IVbcHostObject : ITaskHost {
		void BeginInitialization ();
		
		bool Compile ();
		
		void EndInitialization ();
		
		bool IsDesignTime ();
		
		bool IsUpToDate ();
		
		bool SetAdditionalLibPaths (string[] additionalLibPaths);
		
		bool SetAddModules (string[] addModules);
		
		bool SetBaseAddress (string targetType, string baseAddress);
		
		bool SetCodePage (int codePage);
		
		bool SetDebugType (bool emitDebugInformation, string debugType);
		
		bool SetDefineConstants (string defineConstants);
		
		bool SetDelaySign (bool delaySign);
		
		bool SetDisabledWarnings (string disabledWarnings);
		
		bool SetDocumentationFile (string documentationFile);
		
		bool SetErrorReport (string errorReport);
		
		bool SetFileAlignment (int fileAlignment);
		
		bool SetGenerateDocumentation (bool generateDocumentation);
		
		bool SetImports (ITaskItem[] importsList);
		
		bool SetKeyContainer (string keyContainer);
		
		bool SetKeyFile (string keyFile);
		
		bool SetLinkResources (ITaskItem[] linkResources);
		
		bool SetMainEntryPoint (string mainEntryPoint);
		
		bool SetNoConfig (bool noConfig);
		
		bool SetNoStandardLib (bool noStandardLib);
		
		bool SetNoWarnings (bool noWarnings);
		
		bool SetOptimize (bool optimize);
		
		bool SetOptionCompare (string optionCompare);
		
		bool SetOptionExplicit (bool optionExplicit);
		
		bool SetOptionStrict (bool optionStrict);
		
		bool SetOptionStrictType (string optionStrictType);
		
		bool SetOutputAssembly (string outputAssembly);
		
		bool SetPlatform (string platform);
		
		bool SetReferences (ITaskItem[] references);
		
		bool SetRemoveIntegerChecks (bool removeIntegerChecks);
		
		bool SetResources (ITaskItem[] resources);
		
		bool SetResponseFiles (ITaskItem[] responseFiles);
		
		bool SetRootNamespace (string rootNamespace);
		
		bool SetSdkPath (string sdkPath);
		
		bool SetSources (ITaskItem[] sources);
		
		bool SetTargetCompactFramework (bool targetCompactFramework);
		
		bool SetTargetType (string targetType);
		
		bool SetTreatWarningsAsErrors (bool treatWarningsAsErrors);
		
		bool SetWarningsAsErrors (string warningsAsErrors);
		
		bool SetWarningsNotAsErrors (string warningsNotAsErrors);
		
		bool SetWin32Icon (string win32Icon);
		
		bool SetWin32Resource (string win32Resource);
	}
}

#endif