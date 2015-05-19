using System;
using System.Collections.Generic;
using Microsoft.Build.Execution;

namespace Microsoft.Build.Evaluation
{
	public
	class SubToolset
	{
		internal SubToolset (IDictionary<string, ProjectPropertyInstance> properties, string subToolsetVersion)
		{
			Properties = properties;
			SubToolsetVersion = subToolsetVersion;
		}

		public IDictionary<string, ProjectPropertyInstance> Properties { get; private set; }
		public string SubToolsetVersion { get; private set; }
	}
}

