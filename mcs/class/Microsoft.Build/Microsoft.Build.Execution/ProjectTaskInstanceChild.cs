using System;
using Microsoft.Build.Construction;

namespace Microsoft.Build.Execution
{
	public abstract class ProjectTaskInstanceChild
	{
		public abstract string Condition { get; }
		#if NET_4_5
		public abstract ElementLocation ConditionLocation { get; }
		public abstract ElementLocation Location { get; }
		public abstract ElementLocation TaskParameterLocation { get; }
		#endif
	}
}

