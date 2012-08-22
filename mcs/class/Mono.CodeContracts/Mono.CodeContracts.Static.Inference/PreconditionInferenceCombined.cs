using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Mono.CodeContracts.Static.Inference.Interface;
using Mono.CodeContracts.Static.Proving;

namespace Mono.CodeContracts.Static.Inference
{
	public class PreconditionInferenceCombined : IPreconditionInference
	{
		private readonly ReadOnlyCollection<IPreconditionInference> inferencers;

		public PreconditionInferenceCombined (List<IPreconditionInference> inferencers	)
		{
			this.inferencers = inferencers.AsReadOnly();
		}

		public bool TryInferPrecondition(ProofObligation obl, ICodeFixesManager codefixesManager, out InferredPreconditions preConditions)
		{
			preConditions = (InferredPreconditions) null;
			foreach (IPreconditionInference preconditionInference in this.inferencers)
			{
				InferredPreconditions preConditions1;
        		if (preconditionInference != null && preconditionInference.TryInferPrecondition(obl, codefixesManager, out preConditions1))
				{
					if (preConditions == null)
            			preConditions = preConditions1;
         			else
            		preConditions.AddRange((IEnumerable<IInferredPrecond>) preConditions1);
				}
			}
			return preConditions != null;
		}

		private void ObjectInvariant()
	    {
	    }
	}
}

