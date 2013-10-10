using System;
using System.Collections.Generic;
using Microsoft.Build.Construction;

namespace Microsoft.Build.Execution
{
	public sealed class ProjectTaskInstance : ProjectTargetInstanceChild
	{
		public override string Condition {
			get { throw new NotImplementedException (); }
		}
		public string ExecuteTargets { get; private set; }
		#if NET_4_5
		public override ElementLocation ConditionLocation {
			get { throw new NotImplementedException (); }
		}
		public ElementLocation ExecuteTargetsLocation {
			get { throw new NotImplementedException (); }
		}
		public override ElementLocation Location {
			get { throw new NotImplementedException (); }
		}
		#endif
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

