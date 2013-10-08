using System;
using Microsoft.Build.Construction;
using System.Collections.Generic;

namespace Microsoft.Build.Execution
{
	public class ProjectItemGroupTaskItemInstance
	{
		public string Condition { get; private set; }
		public string Exclude { get; private set; }
		public string Include { get; private set; }
		public string ItemType { get; private set; }
		public string KeepDuplicates { get; private set; }
		public string KeepMetadata { get; private set; }
		public ICollection<ProjectItemGroupTaskMetadataInstance> Metadata { get; private set; }
		public string Remove { get; private set; }
		public string RemoveMetadata { get; private set; }

		#if NET_4_5
		public ElementLocation ConditionLocation { get; private set; }
		public ElementLocation ExcludeLocation { get; private set; }
		public ElementLocation IncludeLocation { get; private set; }
		public ElementLocation KeepDuplicatesLocation { get; private set; }
		public ElementLocation KeepMetadataLocation { get; private set; }
		public ElementLocation Location { get; private set; }
		public ElementLocation RemoveLocation { get; private set; }
		public ElementLocation RemoveMetadataLocation { get; private set; }
		#endif
	}
}

