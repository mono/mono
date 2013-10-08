using System;
using Microsoft.Build.Construction;

namespace Microsoft.Build.Execution
{
	public class ProjectOnErrorInstance : ProjectTargetInstanceChild
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
	}
}

