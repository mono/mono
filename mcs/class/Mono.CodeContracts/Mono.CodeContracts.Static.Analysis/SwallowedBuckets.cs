using System;

namespace Mono.CodeContracts.Static.Analysis
{
	public	 class SwallowedBuckets
	{
		private readonly int[] counter;

		public SwallowedBuckets()
	    {
	      this.counter = new int[4];
	    }

		public SwallowedBuckets(SwallowedBuckets.CounterGetter counterGetter):this()
	    {
	      for (int index = 0; index < this.counter.Length; ++index)
	        this.counter[index] = counterGetter((ProofOutcome) index);
	    }

		public static SwallowedBuckets operator -(SwallowedBuckets sw1, SwallowedBuckets sw2)
	    {
	      return new SwallowedBuckets((SwallowedBuckets.CounterGetter) (outcome => sw1.GetCounter(outcome) - sw2.GetCounter(outcome)));
	    }

		public void UpdateCounter(ProofOutcome outcome)
	    {
	      ++this.counter[(int) outcome];
	    }

		public int GetCounter(ProofOutcome outcome)
	    {
	      return this.counter[(int) outcome];
	    }

		public delegate int CounterGetter(ProofOutcome outcome);
	}
}

