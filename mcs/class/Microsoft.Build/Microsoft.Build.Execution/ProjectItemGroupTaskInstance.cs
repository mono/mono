using System;
using System.Collections.Generic;
using Microsoft.Build.Construction;

namespace Microsoft.Build.Execution
{
	public sealed class ProjectItemGroupTaskInstance : ProjectTargetInstanceChild
	{
		public override string Condition {
			get { throw new NotImplementedException (); }
		}
		public ICollection<ProjectItemGroupTaskItemInstance> Items { get; private set; }

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
	}
}

