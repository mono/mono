using System;
using Microsoft.Build.Framework;

namespace Microsoft.Build.Logging
{
	public class DistributedFileLogger : IForwardingLogger
	{
		#region INodeLogger implementation

		public void Initialize (IEventSource eventSource, int nodeCount)
		{
			throw new NotImplementedException ();
		}

		#endregion

		#region ILogger implementation

		public void Initialize (IEventSource eventSource)
		{
			throw new NotImplementedException ();
		}

		public void Shutdown ()
		{
			throw new NotImplementedException ();
		}

		public string Parameters { get; set; }

		public LoggerVerbosity Verbosity { get; set; }

		#endregion

		#region IForwardingLogger implementation

		public IEventRedirector BuildEventRedirector { get; set; }

		public int NodeId { get; set; }

		#endregion
	}
}

