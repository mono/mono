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
using Mono.CodeContracts.Static.Analysis.ExpressionAnalysis.Decoding;
using Mono.CodeContracts.Static.ControlFlow;
using Mono.CodeContracts.Static.DataStructures;
using Mono.CodeContracts.Static.Providers;
using Mono.CodeContracts.Static.Proving;

namespace Mono.CodeContracts.Static.Analysis.Drivers {
	interface IMethodDriver<Expression, Variable> : IBasicMethodDriver {
		ICodeLayer<Variable, Variable, IValueContextProvider<Variable>, IImmutableMap<Variable, Sequence<Variable>>> ValueLayer { get; }
		ICodeLayer<Expression, Variable, IExpressionContextProvider<Expression, Variable>, IImmutableMap<Variable, Sequence<Variable>>> ExpressionLayer { get; }
		ICodeLayer<Variable, Variable, IValueContextProvider<Variable>, IImmutableMap<Variable, Sequence<Variable>>> HybridLayer { get; }

		IExpressionContextProvider<Expression, Variable> ContextProvider { get; }
		IMetaDataProvider MetaDataProvider { get; }

		ICFG CFG { get; }
		Method CurrentMethod { get; }
		IFactBase<Variable> BasicFacts { get; }
		IFullExpressionDecoder<Variable, Expression> ExpressionDecoder { get; }

		void RunHeapAndExpressionAnalyses ();
		int KeyConverter (Variable var);
	}
}
