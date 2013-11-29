//
// ProjectOnErrorInstance.cs
//
// Author:
//    Atsushi Enomoto (atsushi@xamarin.com)
//
// (C) 2013 Xamarin Inc.
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
using System.Collections.Generic;
using Microsoft.Build.Construction;
using System.Linq;

namespace Microsoft.Build.Execution
{
	public sealed class ProjectTaskInstance : ProjectTargetInstanceChild
	{
		internal ProjectTaskInstance (ProjectTaskElement xml)
		{
			condition = xml.Condition;
			ContinueOnError = xml.ContinueOnError;
			Name = xml.Name;
			Outputs = xml.Outputs.Select (o => {
				if (o.IsOutputItem)
					return (ProjectTaskInstanceChild) new ProjectTaskOutputItemInstance ((ProjectOutputElement) o);
				if (o.IsOutputProperty)
					return new ProjectTaskOutputPropertyInstance ((ProjectOutputElement) o);
				throw new NotSupportedException ();
				}).ToArray ();
			Parameters = new Dictionary<string,string> (xml.Parameters);
			#if NET_4_5
			MSBuildArchitecture = xml.MSBuildArchitecture;
			MSBuildRuntime = xml.MSBuildRuntime;
			
			condition_location = xml.ConditionLocation;
			ContinueOnErrorLocation = xml.ContinueOnErrorLocation;
			location = xml.Location;
			MSBuildArchitectureLocation = xml.MSBuildArchitectureLocation;
			MSBuildRuntimeLocation = xml.MSBuildRuntimeLocation;
			#endif
		}
		
		string condition;
		public override string Condition {
			get { return condition; }
		}
		
		ElementLocation condition_location, location;
		
		#if NET_4_5
		public
		#else
		internal
		#endif
		override ElementLocation ConditionLocation {
			get { return condition_location; }
		}

		#if NET_4_5
		public
		#else
		internal
		#endif
		override ElementLocation Location {
			get { return location; }
		}
		
		public string ContinueOnError { get; private set; }
		
		#if NET_4_5
		public ElementLocation ContinueOnErrorLocation { get; private set; }

		public string MSBuildArchitecture { get; private set; }

		public ElementLocation MSBuildArchitectureLocation { get; private set; }

		public string MSBuildRuntime { get; private set; }

		public ElementLocation MSBuildRuntimeLocation { get; private set; }
		#endif
		
		public string Name { get; private set; }

		public IList<ProjectTaskInstanceChild> Outputs { get; private set; }

		public IDictionary<string, string> Parameters { get; private set; }
	}
}

