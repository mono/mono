//
// Task.cs: Represents a task.
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
using System.Resources;
using Microsoft.Build.Framework;

namespace Microsoft.Build.Utilities
{
	public abstract class Task : ITask
	{
		IBuildEngine		buildEngine;
		string			helpKeywordPrefix;
		ITaskHost		hostObject;
		TaskLoggingHelper	log;
		ResourceManager		taskResources;
		
		protected Task()
			: this (null, null)
		{
		}

		protected Task(ResourceManager taskResources)
			: this (taskResources, null)
		{
		}

		protected Task(ResourceManager taskResources,
			       string helpKeywordPrefix)
		{
			this.taskResources = taskResources;
			this.helpKeywordPrefix = helpKeywordPrefix;
		}

		public abstract bool Execute();

		public IBuildEngine BuildEngine	{
			get {
				return buildEngine;
			}
			set {
				buildEngine = value;
				log = new TaskLoggingHelper (this); 
			}
		}

		protected string HelpKeywordPrefix {
			get {
				return helpKeywordPrefix;
			}
			set {
				helpKeywordPrefix = value;
			}
		}

		public ITaskHost HostObject {
			get {
				return hostObject;
			}
			set {
				hostObject = value;
			}
		}

		public TaskLoggingHelper Log {
			get {
				return log;
			}
		}

		protected ResourceManager TaskResources	{
			get {
				return taskResources;
			}
			set {
				taskResources = value;
			}
		}
	}
}

#endif
