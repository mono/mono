using System;
using Mono.CodeContracts.Static.DataStructures;
using Mono.CodeContracts.Static.ControlFlow;
using Mono.CodeContracts.Static.Proving;
using Mono.CodeContracts.Static.Providers;
using Mono.CodeContracts.Static.Analysis;

namespace Mono.CodeContracts.Static.Inference.Interface
{
	interface ICodeFixesManager
	{
		int SuggestCodeFixes();

		bool TrySuggestConstantInititalizationFix(ProofObligation obl, APC pc, BoxedExpression dest, BoxedExpression oldInitialization, BoxedExpression newInitialization, BoxedExpression constraint);

		bool TrySuggestConstantInitializationFix<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Variable>(ProofObligation obl, Func<APC> pc, BoxedExpression failingCondition, BoxedExpression falseCondition, IDecodeMetaData<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly> metaDataDecoder, Func<Variable, BoxedExpression> VariableValueBeforeRenaming, Func<Variable, BoxedExpression> VariableName);

    	bool TrySuggestFixForMethodCallReturnValue<Variable, ArgList>(ProofObligation obl, APC pc, Variable dest, ArgList args, BoxedExpression condition, Func<bool> CalleeIsStaticMethod, Func<string> MethodName, Func<Variable, FList<PathElement>> AccessPath, Func<Variable, FList<PathElement>, BoxedExpression> MakeMethodCall, Func<Variable, BoxedExpression> MakeEqualZero) where ArgList : IIndexable<Variable>;

    	bool TrySuggestLargerAllocation<Variable>(ProofObligation obl, Func<APC> definitionPC, APC failingConditionPC, BoxedExpression failingCondition, Variable array, Variable length, Func<Variable, BoxedExpression> Converter, IFactQuery<BoxedExpression, Variable> factQuery);

    	bool TrySuggestTestFix<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly>(ProofObligation obl, Func<APC> pc, BoxedExpression guard, BoxedExpression failingCondition, IDecodeMetaData<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly> metaDataDecoder, Func<BoxedExpression, bool> IsArrayLength);

    	bool TrySuggestTestStrengthening(ProofObligation obl, APC pc, BoxedExpression additionalGuard);

    	bool TrySuggestFloatingPointComparisonFix(ProofObligation obl, APC pc, BoxedExpression left, BoxedExpression right, ConcreteFloat leftType, ConcreteFloat rightType);

    	bool TrySuggestRemovingConstructor(APC pc, string name, bool isConstructor, Func<bool> IsFalseInferred);

    	bool TrySuggestNonOverflowingExpression<Variable>(APC pc, BoxedExpression exp, IFactQuery<BoxedExpression, Variable> factQuery, Func<Variable, IntervalStruct> TypeRange);

    	bool TrySuggestOffByOneFix<Variable>(ProofObligation obl, APC pc, bool isArrayAccess, BoxedExpression exp, Func<Variable, FList<PathElement>> AccessPath, Func<BoxedExpression, bool> IsArrayLength, IFactQuery<BoxedExpression, Variable> factQuery);
  
	}
}

