using System;
using System.Collections.Generic;

namespace Microsoft.Build.Framework
{
	public interface ITaskFactory2 : ITaskFactory
	{
		ITask CreateTask (IBuildEngine taskFactoryLoggingHost, IDictionary<string, string> taskIdentityParameters);
		bool Initialize (string taskName, IDictionary<string, string> factoryIdentityParameters, IDictionary<string, TaskPropertyInfo> parameterGroup, string taskBody, IBuildEngine taskFactoryLoggingHost);
	}
}

