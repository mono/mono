using System;
using Microsoft.Build.Construction;

namespace Microsoft.Build.Evaluation
{
	public enum ProjectCollectionChangedState
	{
		DefaultToolsVersion,
		Toolsets,
		Loggers,
		GlobalProperties,
		IsBuildEnabled,
		OnlyLogCriticalEvents,
		HostServices,
		DisableMarkDirty,
		SkipEvaluation
	}
}

