using System;

namespace Mono.CodeContracts.Static.Analysis
{
	public class TimeoutExceptionFixpointComputation
	{
		private static uint count;

	    public static uint ThrownExceptions
	    {
	      get
	      {
	        return TimeoutExceptionFixpointComputation.count;
	      }
	    }

	    static TimeoutExceptionFixpointComputation()
	    {
	    }

	    public TimeoutExceptionFixpointComputation()
	    {
	      ++TimeoutExceptionFixpointComputation.count;
	    }
	}
}

