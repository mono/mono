//
// ProjectTargetInstance.cs
//
// Author:
//   Rolf Bjarne Kvinge (rolf@xamarin.com)
//   Atsushi Enomoto (atsshi@xamarin.com)
//
// Copyright (C) 2011,2013 Xamarin Inc.
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
using System.Linq;

namespace Microsoft.Build.Execution
{
#if NET_4_5
	public
#endif
	sealed class ProjectTargetInstance
	{
		internal ProjectTargetInstance (ProjectTargetElement xml)
		{
			FullPath = xml.ContainingProject.FullPath;
			Children = xml.Children.Select<ProjectElement,ProjectTargetInstanceChild> (c => {
				if (c is ProjectOnErrorElement)
					return new ProjectOnErrorInstance ((ProjectOnErrorElement) c);
				if (c is ProjectItemGroupElement)
					return new ProjectItemGroupTaskInstance ((ProjectItemGroupElement) c);
				if (c is ProjectPropertyGroupElement)
					return new ProjectPropertyGroupTaskInstance ((ProjectPropertyGroupElement) c);
				if (c is ProjectTaskElement)
					return new ProjectTaskInstance ((ProjectTaskElement) c);
				throw new NotSupportedException ();
			}).ToArray ();
			Condition = xml.Condition;
			DependsOnTargets = xml.DependsOnTargets;
			//FullPath = fullPath;
			Inputs = xml.Inputs;
			KeepDuplicateOutputs = xml.KeepDuplicateOutputs;
			Name = xml.Name;
			OnErrorChildren = xml.OnErrors.Select (c => new ProjectOnErrorInstance (c)).ToArray ();
			Outputs = xml.Outputs;
			Returns = xml.Returns;
			Tasks = xml.Tasks.Select (t => new ProjectTaskInstance (t)).ToArray ();
			#if NET_4_5
			AfterTargetsLocation = xml.AfterTargetsLocation;
			BeforeTargetsLocation = xml.BeforeTargetsLocation;
			ConditionLocation = xml.ConditionLocation;
			DependsOnTargetsLocation = xml.DependsOnTargetsLocation;
			InputsLocation = xml.InputsLocation;
			KeepDuplicateOutputsLocation = xml.KeepDuplicateOutputsLocation;
			Location = xml.Location;
			OutputsLocation = xml.OutputsLocation;
			ReturnsLocation = xml.ReturnsLocation;
			#endif
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

