using System;
using Microsoft.Build.Construction;

namespace Microsoft.Build.Evaluation
{
	public enum ProjectCollectionChangedState
	{
		DefaultToolsVersion,
		DisableMarkDirty,
		GlobalProperties,
		HostServices,
		IsBuildEnabled,
		Loggers,
		OnlyLogCriticalEvents,
		SkipEvaluation,
		Toolsets
	}
}

