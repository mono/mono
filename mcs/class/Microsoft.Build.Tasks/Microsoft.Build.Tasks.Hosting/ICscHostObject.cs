//
// ICscHostObject.cs: Host object interface for C# compiler.
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

using System;
using System.Runtime.InteropServices;
using Microsoft.Build.Framework;

namespace Microsoft.Build.Tasks.Hosting {
	
	[InterfaceType (ComInterfaceType.InterfaceIsIUnknown)]
	[Guid ("8520CC4D-64DC-4855-BE3F-4C28CCE048EE")]
	[ComVisible (true)]
	public interface ICscHostObject : ITaskHost {
	
		void BeginInitialization ();
		
		bool Compile ();
		
		bool EndInitialization (out string errorMessage, out int errorCode);
		
		bool IsDesignTime ();
		
		bool IsUpToDate ();
		
		bool SetAdditionalLibPaths (string[] additionalLibPaths);
		
		bool SetAddModules (string[] addModules);
		
		bool SetAllowUnsafeBlocks (bool allowUnsafeBlocks);
		
		bool SetBaseAddress (string baseAddress);
		
		bool SetCheckForOverflowUnderflow (bool checkForOverflowUnderflow);
		
		bool SetCodePage (int codePage);
		
		bool SetDebugType (string debugType);
		
		bool SetDefineConstants (string defineConstants);
		
		bool SetDelaySign (bool delaySignExplicitlySet, bool delaySign);
		
		bool SetDisabledWarnings (string disabledWarnings);
		
		bool SetDocumentationFile (string documentationFile);
		
		bool SetEmitDebugInformation (bool emitDebugInformation);
		
		bool SetErrorReport (string errorReport);
		
		bool SetFileAlignment (int fileAlignment);
		
		bool SetGenerateFullPaths (bool generateFullPaths);
		
		bool SetKeyContainer (string keyContainer);
		
		bool SetKeyFile (string keyFile);
		
		bool SetLangVersion (string langVersion);
		
		bool SetLinkResources (ITaskItem[] linkResources);
		
		bool SetMainEntryPoint (string targetType, string mainEntryPoint);
		
		bool SetModuleAssemblyName (string moduleAssemblyName);
		
		bool SetNoConfig (bool noConfig);
		
		bool SetNoStandardLib (bool noStandardLib);
		
		bool SetOptimize (bool optimize);
		
		bool SetOutputAssembly (string outputAssembly);
		
		bool SetPdbFile (string pdbFile);
		
		bool SetPlatform (string platform);
		
		bool SetReferences (ITaskItem[] references);
		
		bool SetResources (ITaskItem[] resources);
		
		bool SetResponseFiles (ITaskItem[] responseFiles);
		
		bool SetSources (ITaskItem[] sources);
		
		bool SetTargetType (string targetType);
		
		bool SetTreatWarningsAsErrors (bool treatWarningsAsErrors);
		
		bool SetWarningLevel (int warningLevel);
		
		bool SetWarningsAsErrors (string warningsAsErrors);
		
		bool SetWarningsNotAsErrors (string warningsNotAsErrors);
		
		bool SetWin32Icon (string win32Icon);
		
		bool SetWin32Resource (string win32Resource);
	}
}

#endif