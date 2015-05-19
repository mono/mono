//
// BuildStartedEventArgs.cs: Provides data for the Microsoft.Build.Framework.
// IEventSource.BuildStarted event.
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

using System;
using System.Collections.Generic;

namespace Microsoft.Build.Framework {
	[Serializable]
	public class BuildStartedEventArgs : BuildStatusEventArgs {

		IDictionary<string, string> buildEnvironment;
	
		protected BuildStartedEventArgs ()
		{
		}

		public BuildStartedEventArgs (string message,
					      string helpKeyword)
			: base (message, helpKeyword, null)
		{
		}

		public BuildStartedEventArgs (string message,
					      string helpKeyword,
					      IDictionary<string, string> environmentOfBuild)
			: base (message, helpKeyword, null)
		{
			buildEnvironment = environmentOfBuild;
		}

		public BuildStartedEventArgs (string message,
					      string helpKeyword,
					      DateTime eventTimestamp)
			: base (message, helpKeyword, null, eventTimestamp)
		{
		}

		public BuildStartedEventArgs (string message,
					      string helpKeyword,
					      DateTime eventTimestamp,
					      params object [] messageArgs)
			: base (message, helpKeyword, null, eventTimestamp, messageArgs)
		{
		}

		public IDictionary<string, string> BuildEnvironment {
			get { return buildEnvironment; }
		}
	}
}

