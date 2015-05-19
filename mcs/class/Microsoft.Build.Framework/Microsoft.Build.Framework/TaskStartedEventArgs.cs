//
// TaskStartedEventArgs: Provides data for the Microsoft.Build.Framework.
// IEventSource.TaskStarted event.
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

namespace Microsoft.Build.Framework {
	[Serializable]
	public class TaskStartedEventArgs : BuildStatusEventArgs {
	
		string projectFile;
		string taskFile;
		string taskName;

		protected TaskStartedEventArgs ()
		{
		}

		public TaskStartedEventArgs (string message, string helpKeyword,
					     string projectFile,
					     string taskFile, string taskName)
			: base (message, helpKeyword, null)
		{
			this.projectFile = projectFile;
			this.taskFile = taskFile;
			this.taskName = taskName;
		}

		public string ProjectFile {
			get {
				return projectFile;
			}
		}

		public string TaskFile {
			get {
				return taskFile;
				
			}
		}

		public string TaskName {
			get {
				return taskName;
			}
		}
	}
}

