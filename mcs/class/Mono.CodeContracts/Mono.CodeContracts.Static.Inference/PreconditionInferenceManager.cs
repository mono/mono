using System;
using System.Collections.Generic;
using Mono.CodeContracts.Static.Inference.Interface;
using Mono.CodeContracts.Static.Proving;

namespace Mono.CodeContracts.Static.Inference
{
	public class PreconditionInferenceManager
	{
		public readonly IPreconditionInference Inference;
		public readonly IPreconditionDispatcher Dispatch;
		private static PreconditionInferenceManager dummy;

		public PreconditionInferenceManager (IPreconditionInference Inference, IPreconditionDispatcher Dispatcher)
		{
			this.Inference = Inference;
			this.Dispatch = Dispatcher;
		}

		public static PreconditionInferenceManager Dummy {
			get {
				if (PreconditionInferenceManager.dummy == null)
					PreconditionInferenceManager.dummy = new PreconditionInferenceManager ((IPreconditionInference)new PreconditionInferenceManager.DummyIPreconditionInference (), (IPreconditionDispatcher)new PreconditionInferenceManager.DummyIPreconditionDispatcher ());
				return PreconditionInferenceManager.dummy;
			}
		}

		private class DummyIPreconditionInference : IPreconditionInference
		{
			public bool TryInferPrecondition (ProofObligation obl, ICodeFixesManager codefixesManager, out InferredPreconditions preConditions)
			{
				preConditions = (InferredPreconditions)null;
				return false;
			}
		}

		private class DummyIPreconditionDispatcher : IPreconditionDispatcher
		{
			public ProofOutcome AddPreconditions (ProofObligation obl, IEnumerable<BoxedExpression> preconditions, ProofOutcome originalOutcome)
			{
				return originalOutcome;
			}

			public IEnumerable<KeyValuePair<BoxedExpression, IEnumerable<MinimalProofObligation>>> GeneratePreconditions ()
			{
				return (IEnumerable<KeyValuePair<BoxedExpression, IEnumerable<MinimalProofObligation>>>)new List<KeyValuePair<BoxedExpression, IEnumerable<MinimalProofObligation>>> ();
			}

			public int SuggestPreconditions ()
			{
				return 0;
			}

			public int PropagatePreconditions ()
			{
				return 0;
			}
		}
	}
}

