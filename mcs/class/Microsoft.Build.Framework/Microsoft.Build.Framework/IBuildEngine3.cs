using System;
using System.Collections;
using System.Collections.Generic;

namespace Microsoft.Build.Framework
{
	[MonoTODO]
	public interface IBuildEngine3 : IBuildEngine2
	{
		BuildEngineResult BuildProjectFilesInParallel (
			string[] projectFileNames,
			string[] targetNames,
			IDictionary[] globalProperties,
			IList<string>[] removeGlobalProperties,
			string[] toolsVersion,
			bool returnTargetOutputs
		);
		void Reacquire ();
		void Yield ();
	}
}

