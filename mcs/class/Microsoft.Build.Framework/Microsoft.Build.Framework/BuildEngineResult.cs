using System;
using System.Collections.Generic;

namespace Microsoft.Build.Framework
{
	[SerializableAttribute]
	public struct BuildEngineResult
	{
		public BuildEngineResult (bool result, List<IDictionary<string, ITaskItem[]>> targetOutputsPerProject)
		{
			this.result = result;
			this.outputs = targetOutputsPerProject;
		}

		readonly bool result;
		public bool Result {
			get { return result; }
		}

		readonly IList<IDictionary<string, ITaskItem[]>> outputs;
		public IList<IDictionary<string, ITaskItem[]>> TargetOutputsPerProject {
			get { return outputs; }
		}
	}
}
