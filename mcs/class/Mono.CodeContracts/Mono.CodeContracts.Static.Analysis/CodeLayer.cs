// 
// CodeLayer.cs
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

using System;
using Mono.CodeContracts.Static.AST.Visitors;
using Mono.CodeContracts.Static.ControlFlow;
using Mono.CodeContracts.Static.DataFlowAnalysis;
using Mono.CodeContracts.Static.Providers;

namespace Mono.CodeContracts.Static.Analysis {
	class CodeLayer<Expression, Variable, ContextData, EdgeData>
		: ICodeLayer<Expression, Variable, ContextData, EdgeData>
		where ContextData : IMethodContextProvider {
		private readonly Lazy<ILPrinter<APC>> printer;

		public CodeLayer (IILDecoder<APC, Expression, Variable, ContextData, EdgeData> ilDecoder,
		                  IMetaDataProvider metadataDecoder,
		                  IContractProvider contractDecoder,
		                  Func<Expression, string> expressionToString,
		                  Func<Variable, string> variableToString)
		{
			ExpressionToString = expressionToString;
			VariableToString = variableToString;
			ILDecoder = ilDecoder;
			MetaDataProvider = metadataDecoder;
			ContractProvider = contractDecoder;

			this.printer = new Lazy<ILPrinter<APC>> (() => PrinterFactory.Create (ILDecoder, MetaDataProvider, ExpressionToString, VariableToString));
		}

		public CodeLayer (IILDecoder<APC, Expression, Variable, ContextData, EdgeData> ilDecoder,
		                  IMetaDataProvider metadataDecoder,
		                  IContractProvider contractDecoder,
		                  Func<Expression, string> expressionToString,
		                  Func<Variable, string> variableToString, ILPrinter<APC> printer)
			: this (ilDecoder, metadataDecoder, contractDecoder, expressionToString, variableToString)
		{
			this.printer = new Lazy<ILPrinter<APC>> (() => printer);
		}

		public IILDecoder<APC, Expression, Variable, ContextData, EdgeData> ILDecoder { get; private set; }

		public ILPrinter<APC> Printer
		{
			get { return this.printer.Value; }
		}

		public Func<Expression, string> ExpressionToString { get; private set; }

		public Func<Variable, string> VariableToString { get; private set; }

		public IMetaDataProvider MetaDataProvider { get; private set; }
		public IContractProvider ContractProvider { get; private set; }

		public Func<AState, IFixPointInfo<APC, AState>> CreateForward<AState> (
			IAnalysis<APC, AState, IILVisitor<APC, Expression, Variable, AState, AState>, EdgeData> analysis)
		{
			ForwardAnalysis<AState, EdgeData> solver = ForwardAnalysis<AState, EdgeData>.Make (ILDecoder, analysis);
			return (initialState) => {
			       	solver.Run (initialState);
			       	return solver;
			       };
		}
	}
}
