using System;

namespace Microsoft.Build.Framework
{
	[MonoTODO ("This needs to be taken into consideration in the build engine")]
	public interface ICancelableTask : ITask
	{
		void Cancel ();
	}
}

