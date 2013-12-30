using System;

namespace Microsoft.Build.Framework
{
	public interface IEventRedirector
	{
		void ForwardEvent (BuildEventArgs buildEvent);
	}
}

