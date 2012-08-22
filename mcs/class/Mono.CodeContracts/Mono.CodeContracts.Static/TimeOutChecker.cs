using System;
using Mono.CodeContracts.Static.Analysis;

namespace Mono.CodeContracts.Static
{
	class TimeOutChecker
	{
		private DateTime start;
    	private TimeSpan totalElapsed;
    	private readonly int timeout;
    	private TimeoutExceptionFixpointComputation exception;
    	private TimeOutChecker.State state;

		public bool HasAlreadyTimeOut
	    {
	      get
	      {
	        return this.exception != null;
	      }
	    }

		public TimeOutChecker(int seconds, bool start = true)
	    {
	      this.totalElapsed = new TimeSpan();
	      if (start)
	      {
	        this.start = DateTime.Now;
	        this.state = TimeOutChecker.State.Running;
	      }
	      else
	        this.state = TimeOutChecker.State.Stopped;
	      this.timeout = seconds;
	      this.exception = (TimeoutExceptionFixpointComputation) null;
	    }

		public void Start()
	    {
	      switch (this.state)
	      {
	        case TimeOutChecker.State.Stopped:
	          this.state = TimeOutChecker.State.Running;
	          this.start = DateTime.Now;
	          break;
	      }
	    }

		public void Stop()
	    {
	      switch (this.state)
	      {
	        case TimeOutChecker.State.Running:
	          this.state = TimeOutChecker.State.Stopped;
	          this.totalElapsed += DateTime.Now - this.start;
	          this.start = new DateTime();
	          break;
	      }
	    }

		public void CheckTimeOut(string reason = "")
	    {
	      this.Start();
	      DateTime now = DateTime.Now;
	      this.totalElapsed += now - this.start;
	      this.start = now;
	      if (this.totalElapsed.TotalSeconds < (double) this.timeout)
	        return;
	      if (this.exception == null)
	        this.exception = new TimeoutExceptionFixpointComputation();
	      throw this.exception;
	    }

		private enum State
	    {
	      Stopped,
	      Running,
	    }
	}
}

