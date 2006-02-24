//
// LoggerVerbosity.cs: Specifies the available levels of a Microsoft.Build.
// Utilities.Logger.
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

using System.Runtime.InteropServices;

namespace Microsoft.Build.Framework
{
	[ComVisible (true)]
	public enum LoggerVerbosity
	{
		// Quiet verbosity, which displays a build summary.
		Quiet,
		// Minimal verbosity, which displays errors, warnings,
		// messages with MessageImportance values of High, and a build
		// summary.
		Minimal,
		// Normal verbosity, which displays error, warning, messages
		// with MessageImportance values of High, some status events,
		// and a build summary.
		Normal,
		// Minimal verbosity, which displays error, warnings, messages
		// with MessageImportance values of High or Normal, all status
		// events, and a build summary.
		Detailed,
		// Diagnostic verbosity, which displays all errors, warnings,
		// messages, status events, and a build summary.
		Diagnostic
	}
}

#endif