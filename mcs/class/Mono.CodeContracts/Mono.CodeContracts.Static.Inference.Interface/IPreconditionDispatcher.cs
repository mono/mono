using System;
using System.Collections.Generic;
using Mono.CodeContracts.Static.Proving;

namespace Mono.CodeContracts.Static.Inference.Interface
{
	interface IPreconditionDispatcher
	{
		IEnumerable<KeyValuePair<BoxedExpression, IEnumerable<MinimalProofObligation>>> GeneratePreconditions();

		ProofOutcome AddPreconditions(ProofObligation obl, IEnumerable<BoxedExpression> preconditions, ProofOutcome originalOutcome);

    	int SuggestPreconditions();

    	int PropagatePreconditions();
	}
}

