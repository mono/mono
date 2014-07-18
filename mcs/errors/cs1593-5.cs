// CS1593: Delegate `System.Action<System.Threading.Tasks.Task>' does not take `0' arguments
// Line: 17

using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace CrashRepro.Core
{
	public class X
	{		
		async void Foo ()
		{
			var pushes = await Run ().ContinueWith (l =>
			{
				for (int i = 0; i < 1; ++i)
					Run ().ContinueWith(() => { });
			});
		}

		Task Run ()
		{
			return null;
		}
	}
}

