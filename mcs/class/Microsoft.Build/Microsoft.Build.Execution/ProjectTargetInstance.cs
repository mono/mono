// ProjectTargetInstance.cs
//
// Author:
//   Rolf Bjarne Kvinge (rolf@xamarin.com)
//
// Copyright (C) 2011 Xamarin Inc.
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
//


using System;
using Microsoft.Build.Construction;
using System.Collections.Generic;

namespace Microsoft.Build.Execution
{
#if NET_4_5
	public
#endif
	sealed class ProjectTargetInstance
	{
		private ProjectTargetInstance ()
		{
			throw new NotImplementedException ();
		}

		public ElementLocation AfterTargetsLocation { get; private set; }
		public ElementLocation BeforeTargetsLocation { get; private set; }
		public IList<ProjectTargetInstanceChild> Children { get; private set; }
		public string Condition { get; private set; }
		public ElementLocation ConditionLocation { get; private set; }
		public string DependsOnTargets { get; private set; }
		public ElementLocation DependsOnTargetsLocation { get; private set; }
		public string FullPath { get; private set; }
		public string Inputs { get; private set; }
		public ElementLocation InputsLocation { get; private set; }
		public string KeepDuplicateOutputs { get; private set; }
		public ElementLocation KeepDuplicateOutputsLocation { get; private set; }
		public ElementLocation Location { get; private set; }
		public string Name { get; private set; }
		public IList<ProjectOnErrorInstance> OnErrorChildren { get; private set; }
		public string Outputs { get; private set; }
		public ElementLocation OutputsLocation { get; private set; }
		public string Returns { get; private set; }
		public ElementLocation ReturnsLocation { get; private set; }
		public ICollection<ProjectTaskInstance> Tasks { get; private set; }
	}
}

