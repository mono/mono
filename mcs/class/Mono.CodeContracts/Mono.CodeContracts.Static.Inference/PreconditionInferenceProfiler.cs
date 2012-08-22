using System;
using System.Collections.Generic;
using Mono.CodeContracts.Static.Proving;
using Mono.CodeContracts.Static.Inference.Interface;
using Mono.CodeContracts.Static.Analysis;

namespace Mono.CodeContracts.Static.Inference
{
	public class PreconditionInferenceProfiler : IPreconditionInference
	{
		private static int inferred = 0;
	    private static TimeSpan inferenceTime = new TimeSpan(0L);
	    private static int totalMethodsWithPreconditons = 0;
	    private static int totalMethodsWithNecessaryPreconditions = -1;
	    private readonly IPreconditionInference inner;

	    static PreconditionInferenceProfiler()
	    {
	    }

		public PreconditionInferenceProfiler (IPreconditionInference inner)
		{
			 this.inner = inner;
		}

		public bool TryInferPrecondition(ProofObligation obl, ICodeFixesManager codefixesManager, out InferredPreconditions preConditions)
    	{
			DateTime now = DateTime.Now;
      		bool flag = this.inner.TryInferPrecondition(obl, codefixesManager, out preConditions);
      		PreconditionInferenceProfiler.inferenceTime += DateTime.Now - now;
	      	if (preConditions == null)
	        	return false;
	      	if (flag)
	        	PreconditionInferenceProfiler.inferred += Enumerable.Count<IInferredPrecond>((IEnumerable<IInferredPrecond>) preConditions);
	      	return flag;
		}

		public static void NotifyMethodWithAPrecondition()
		{
			++PreconditionInferenceProfiler.totalMethodsWithPreconditons;
		}

		public static void NotifyCheckInferredRequiresResult(uint tops)
		{
			PreconditionInferenceProfiler.totalMethodsWithNecessaryPreconditions += (int) tops == 0 ? 1 : 0;
		}

		public static void DumpStatistics(IOutput output)
		{
			if (PreconditionInferenceProfiler.totalMethodsWithPreconditons <= 0)
        		return;

      		output.WriteLine("Methods with necessary preconditions: {0}", new object[1]
	      	{
	        	(object) PreconditionInferenceProfiler.totalMethodsWithPreconditons
	      	});

      		if (PreconditionInferenceProfiler.totalMethodsWithNecessaryPreconditions > -1)
	        output.WriteLine("Methods where preconditions were also sufficient: {0}", new object[1]
	        {
	          (object) (PreconditionInferenceProfiler.totalMethodsWithNecessaryPreconditions + 1)
	        });

	      	if (PreconditionInferenceProfiler.inferred <= 0)
	        	return;
      		output.WriteLine("Discovered {0} new candidate preconditions in {1}", (object) PreconditionInferenceProfiler.inferred, (object) PreconditionInferenceProfiler.inferenceTime);
		}

		private void ObjectInvariant()
	    {
	    }
	}
}

