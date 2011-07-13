// 
// CodeLayerFactory.cs
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
using Mono.CodeContracts.Static.ControlFlow;
using Mono.CodeContracts.Static.Providers;

namespace Mono.CodeContracts.Static.Analysis {
	static class CodeLayerFactory {
		public static ICodeLayer<Expression, Variable, ContextData, EdgeConversionData>
			Create<Expression, Variable, ContextData, EdgeConversionData> (
			IILDecoder<APC, Expression, Variable, ContextData, EdgeConversionData> ilDecoder,
			IMetaDataProvider metadataDecoder,
			IContractProvider contractDecoder, Func<Expression, string> expressionToString, Func<Variable, string> variableToString)
			where ContextData : IMethodContextProvider
		{
			return new CodeLayer<Expression, Variable, ContextData, EdgeConversionData>
				(ilDecoder, metadataDecoder, contractDecoder, expressionToString, variableToString);
		}

		public static ICodeLayer<Expression, Variable, ContextData, EdgeConversionData>
			Create<Expression, Variable, ContextData, EdgeConversionData> (
			IILDecoder<APC, Expression, Variable, ContextData, EdgeConversionData> ilDecoder,
			IMetaDataProvider metadataDecoder,
			IContractProvider contractDecoder, Func<Expression, string> expressionToString, Func<Variable, string> variableToString, ILPrinter<APC> printer)
			where ContextData : IMethodContextProvider
		{
			return new CodeLayer<Expression, Variable, ContextData, EdgeConversionData>
				(ilDecoder, metadataDecoder, contractDecoder, expressionToString, variableToString, printer);
		}
	}
}
