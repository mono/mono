using System;
using System.Collections.Generic;

namespace Microsoft.Build.Framework
{
	public interface ITaskFactory
	{
		string FactoryName { get; }
		Type TaskType { get; }
		void CleanupTask (ITask task);
		ITask CreateTask (IBuildEngine taskFactoryLoggingHost);
		TaskPropertyInfo [] GetTaskParameters ();
		bool Initialize (string taskName, IDictionary<string, TaskPropertyInfo> parameterGroup, string taskBody, IBuildEngine taskFactoryLoggingHost);
	}
}

