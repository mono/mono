using System;
using System.Collections.Generic;
using Microsoft.Build.Construction;

namespace Microsoft.Build.Execution
{
	public sealed class ProjectItemGroupTaskMetadataInstance
	{
		public string Condition { get; private set; }
		public string Name { get; private set; }
		public string Value { get; private set; }

		#if NET_4_5
		public ElementLocation ConditionLocation { get; private set; }
		public ElementLocation Location { get; private set; }
		#endif
	}
}

