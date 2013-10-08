using System;
using Microsoft.Build.Construction;

namespace Microsoft.Build.Execution
{
	public abstract class ProjectTargetInstanceChild
	{
		public abstract string Condition { get; }
		public string FullPath { get; internal set; }
#if NET_4_5
		public abstract ElementLocation ConditionLocation { get; }
		public abstract ElementLocation Location { get; }
#endif
	}
}

