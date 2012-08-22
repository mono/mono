// 
// IMethodDriver.cs
// 
// Authors:
// 	Alexander Chebaturkin (chebaturkin@gmail.com)
// 
// Copyright (C) 2011 Alexander Chebaturkin
// 
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//  
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF 
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// 
using Mono.CodeContracts.Static.AST;
using Mono.CodeContracts.Static.Analysis;
using Mono.CodeContracts.Static.Analysis.ExpressionAnalysis.Decoding;
using Mono.CodeContracts.Static.ControlFlow;
using Mono.CodeContracts.Static.DataStructures;
using Mono.CodeContracts.Static.Providers;
using Mono.CodeContracts.Static.Proving;

namespace Mono.CodeContracts.Static.Analysis.Drivers
{
	interface IMethodDriver<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions> : IBasicMethodDriver<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, LogOptions> where Type : IEquatable<Type> where LogOptions : IFrameworkLogOptions
	{
		ICodeLayer<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Variable, Variable, IValueContext<Local, Parameter, Method, Field, Type, Variable>, IFunctionalMap<Variable, FList<Variable>>> ValueLayer { get; }

		ICodeLayer<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, IExpressionContext<Local, Parameter, Method, Field, Type, Expression, Variable>, IFunctionalMap<Variable, FList<Variable>>> ExpressionLayer { get; }

		ICodeLayer<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Variable, Variable, IValueContext<Local, Parameter, Method, Field, Type, Variable>, IFunctionalMap<Variable, FList<Variable>>> HybridLayer { get; }

		IExpressionContext<Local, Parameter, Method, Field, Type, Expression, Variable> Context { get; }

		IDecodeMetaData<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly> MetaDataDecoder { get; }

		ICFG CFG { get; }

		Method CurrentMethod { get; }

		Converter<Variable, int> KeyNumber { get; }

		Comparison<Variable> VariableComparer { get; }

		IEnumerable<BoxedExpression> InferredPreConditions { get; }

		IEnumerable<BoxedExpression> InferredPostConditions { get; }

		IEnumerable<BoxedExpression> InferredObjectInvariants { get; }

		IFullExpressionDecoder<Type, Variable, Expression> ExpressionDecoder { get; }

		IFactBase<Variable> BasicFacts { get; }

		IDisjunctiveExpressionRefiner<Variable, BoxedExpression> DisjunctiveExpressionRefiner { get; set; }

		SyntacticInformation<Variable> AdditionalSyntacticInformation { get; set; }

		bool AddPreCondition (BoxedExpression boxedExpression, APC pc, object provenance);

		bool AddPostCondition (BoxedExpression boxedExpression, APC pc, object provenance);

		bool AddObjectInvariant (BoxedExpression boxedExpression, object provenance);

		State BackwardTransfer<State, Visitor> (APC from, APC to, State state, Visitor visitor) where Visitor : IEdgeVisit<APC, Local, Parameter, Method, Field, Type, Variable, State>;

		bool CanAddRequires ();

		bool CanAddEnsures ();

		void EndAnalysis ();

		void RunHeapAndExpressionAnalyses ();
	}
}
