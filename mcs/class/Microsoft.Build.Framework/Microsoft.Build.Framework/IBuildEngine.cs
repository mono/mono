//
// IBuildEngine.cs: Provides a way for task authors to use the functionality
// of the MSBuild engine.
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
using System.Collections;

namespace Microsoft.Build.Framework
{
	public interface IBuildEngine
	{
		// Initiates a build of a project file. If the build is
		// successful, the outputs (if any) of the specified targets
		// are returned.
		bool BuildProjectFile (string projectFileName,
				       string[] targetNames,
				       IDictionary globalProperties,
				       IDictionary targetOutputs);

		// Raises a custom event to all registered loggers.
		void LogCustomEvent (CustomBuildEventArgs e);

		// Raises an error to all registered loggers.
		void LogErrorEvent (BuildErrorEventArgs e);

		// Raises a message event to all registered loggers.
		void LogMessageEvent (BuildMessageEventArgs e);

		// Raises a warning to all registered loggers.
		void LogWarningEvent (BuildWarningEventArgs e);

		int ColumnNumberOfTaskNode {
			get;
		}

		bool ContinueOnError {
			get;
		}

		int LineNumberOfTaskNode {
			get;
		}

		string ProjectFileOfTaskNode {
			get;
		}
	}
}

#endif
