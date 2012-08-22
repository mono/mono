using System;
using Mono.CodeContracts.Static.Proving;

namespace Mono.CodeContracts.Static.Inference.Interface
{
	interface IPreconditionInference
	{
		 bool TryInferPrecondition(ProofObligation obl, ICodeFixesManager codefixesManager, out InferredPreconditions preConditions);
	}
}

