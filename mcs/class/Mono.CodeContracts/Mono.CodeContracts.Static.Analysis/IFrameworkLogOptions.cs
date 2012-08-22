using System;

namespace Mono.CodeContracts.Static.Analysis
{
	interface IFrameworkLogOptions
	{
		bool TraceDFA { get; }

		bool TraceHeapAnalysis { get; }

		bool TraceExpressionAnalysis { get; }

		bool TraceEGraph { get; }

		bool TraceWP { get; }

		bool TraceNumericalAnalysis { get; }

		bool TracePartitionAnalysis { get; }

		bool TraceInference { get; }

		bool TraceChecks { get; }

		bool TraceTimings { get; }

		bool PrintIL { get; }

		bool PrioritizeWarnings { get; }

		bool SuggestRequires { get; }

		bool SuggestRequiresForArrays { get; }

		bool SuggestRequiresPurityForArrays { get; }

		bool SuggestNonNullReturn { get; }

		bool InferPreconditionsFromPostconditions { get; }

		bool PropagateObjectInvariants { get; }

		bool PropagateInferredNonNullReturn { get; }

		bool PropagateRequiresPurityForArrays { get; }

		bool PropagatedRequiresAreSufficient { get; }

		bool OutputOnlyExternallyVisibleMembers { get; }

		int Timeout { get; }

		int IterationsBeforeWidening { get; }

		int MaxVarsForOctagonInference { get; }

		bool EnforceFairJoin { get; }

		bool SuggestEnsures (bool isProperty);

		bool PropagateInferredRequires (bool isCurrentMethodGetterOrSetter);

		bool PropagateInferredEnsures (bool isCurrentMethodGetterOrSetter);
	}
}

