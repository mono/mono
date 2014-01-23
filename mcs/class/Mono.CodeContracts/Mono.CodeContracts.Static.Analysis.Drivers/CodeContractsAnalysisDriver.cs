// 
// CodeContractsAnalysisDriver.cs
// 
// Authors:
//	Alexander Chebaturkin (chebaturkin@gmail.com)
// 
// Copyright (C) 2012 Alexander Chebaturkin
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
using Mono.CodeContracts.Static.AST;
using Mono.CodeContracts.Static.Analysis.ExpressionAnalysis;
using Mono.CodeContracts.Static.Analysis.ExpressionAnalysis.Decoding;
using Mono.CodeContracts.Static.Analysis.HeapAnalysis;
using Mono.CodeContracts.Static.ControlFlow;
using Mono.CodeContracts.Static.DataStructures;
using Mono.CodeContracts.Static.Lattices;
using Mono.CodeContracts.Static.Providers;
using Mono.CodeContracts.Static.Proving;

namespace Mono.CodeContracts.Static.Analysis.Drivers {
	class CodeContractsAnalysisDriver<MethodResult>
		: AnalysisDriver<LabeledSymbol<APC, SymbolicValue>, SymbolicValue> {
		public CodeContractsAnalysisDriver (IBasicAnalysisDriver basicDriver)
			: base (basicDriver)
		{
		}

		public override IMethodDriver<LabeledSymbol<APC, SymbolicValue>, SymbolicValue> CreateMethodDriver (Method method)
		{
			return new MethodDriver (method, this);
		}

		#region Nested type: MethodDriver
		private class MethodDriver : BasicMethodDriver,
		                             IMethodDriver<LabeledSymbol<APC, SymbolicValue>, SymbolicValue>,
		                             IFactBase<SymbolicValue> {
			private Func<LabeledSymbol<APC, SymbolicValue>, string> expr2String;
			private IFullExpressionDecoder<SymbolicValue, LabeledSymbol<APC, SymbolicValue>> expression_decoder;
			private HeapAnalysis.HeapAnalysis heap_analysis;

			public MethodDriver (Method method,
			                     CodeContractsAnalysisDriver<MethodResult> parent)
				: base (method, parent)
			{
			}

			private new AnalysisDriver<LabeledSymbol<APC, SymbolicValue>, SymbolicValue> AnalysisDriver
			{
				get { return (AnalysisDriver<LabeledSymbol<APC, SymbolicValue>, SymbolicValue>) base.AnalysisDriver; }
			}

			#region IFactBase<SymbolicValue> Members
			public FlatDomain<bool> IsNull (APC pc, SymbolicValue variable)
			{
				return ProofOutcome.Top;
			}

                        public FlatDomain<bool> IsNonNull(APC pc, SymbolicValue variable)
			{
				return ProofOutcome.Top;
			}

			public bool IsUnreachable (APC pc)
			{
				return this.heap_analysis.IsUnreachable (pc);
			}
			#endregion

			#region IMethodDriver<LabeledSymbol<APC,SymbolicValue>,SymbolicValue> Members
			public ICodeLayer<SymbolicValue, SymbolicValue,
				IValueContextProvider<SymbolicValue>,
				IImmutableMap<SymbolicValue, Sequence<SymbolicValue>>> ValueLayer { get; private set; }

			public ICodeLayer<LabeledSymbol<APC, SymbolicValue>, SymbolicValue,
				IExpressionContextProvider<LabeledSymbol<APC, SymbolicValue>, SymbolicValue>,
				IImmutableMap<SymbolicValue, Sequence<SymbolicValue>>> ExpressionLayer { get; private set; }

			public ICodeLayer<SymbolicValue, SymbolicValue,
				IValueContextProvider<SymbolicValue>,
				IImmutableMap<SymbolicValue, Sequence<SymbolicValue>>> HybridLayer { get; private set; }

			public IExpressionContextProvider<LabeledSymbol<APC, SymbolicValue>, SymbolicValue> ContextProvider
			{
				get { return ExpressionLayer.ILDecoder.ContextProvider; }
			}

			public IMetaDataProvider MetaDataProvider
			{
				get { return RawLayer.MetaDataProvider; }
			}

			public IFactBase<SymbolicValue> BasicFacts
			{
				get { return this; }
			}

			public IFullExpressionDecoder<SymbolicValue, LabeledSymbol<APC, SymbolicValue>> ExpressionDecoder
			{
				get
				{
					if (this.expression_decoder == null)
						this.expression_decoder = 
							new FullExpressionDecoder<SymbolicValue, LabeledSymbol<APC, SymbolicValue>>(MetaDataProvider, ContextProvider);
					return this.expression_decoder;
				}
			}

			public void RunHeapAndExpressionAnalyses ()
			{
				if (this.heap_analysis != null)
					return;

				this.heap_analysis = new HeapAnalysis.HeapAnalysis (StackLayer);
				StackLayer.CreateForward (this.heap_analysis) (this.heap_analysis.InitialValue ());

				ValueLayer = CodeLayerFactory.Create (
				                                      this.heap_analysis.GetDecoder (StackLayer.ILDecoder), StackLayer.MetaDataProvider, StackLayer.ContractProvider,
				                                      source => source.ToString (), dest => dest.ToString ());
				var expressionAnalysis = new ExpressionAnalysisFacade<SymbolicValue, IValueContextProvider<SymbolicValue>, IImmutableMap<SymbolicValue, Sequence<SymbolicValue>>>
					(ValueLayer, this.heap_analysis.IsUnreachable);
				ValueLayer.CreateForward (expressionAnalysis.CreateExpressionAnalysis ()) (expressionAnalysis.InitialValue (SymbolicValue.GetUniqueKey));

				if (DebugOptions.Debug)
				{
					Console.WriteLine ("------------Value based CFG-----------------");
					ValueLayer.ILDecoder.ContextProvider.MethodContext.CFG.Print (Console.Out, ValueLayer.Printer, null, null);
				}

				IILDecoder
					<APC, LabeledSymbol<APC, SymbolicValue>, SymbolicValue, IExpressionContextProvider<LabeledSymbol<APC, SymbolicValue>, SymbolicValue>, IImmutableMap<SymbolicValue, Sequence<SymbolicValue>>>
					decoder = expressionAnalysis.GetDecoder (ValueLayer.ILDecoder);
				this.expr2String = ExpressionPrinterFactory.Printer (decoder.ContextProvider, this);
				ExpressionLayer = CodeLayerFactory.Create (decoder, ValueLayer.MetaDataProvider, ValueLayer.ContractProvider,
				                                           this.expr2String, ValueLayer.VariableToString);

				if (DebugOptions.Debug)
				{
					Console.WriteLine ("------------Expression based CFG-------------");
					ExpressionLayer.ILDecoder.ContextProvider.MethodContext.CFG.Print (Console.Out, ExpressionLayer.Printer, null, null);
				}

				HybridLayer = CodeLayerFactory.Create (ValueLayer.ILDecoder, ValueLayer.MetaDataProvider, ValueLayer.ContractProvider,
				                                       ValueLayer.ExpressionToString,
				                                       ValueLayer.VariableToString, ExpressionLayer.Printer);
			}

			public int KeyConverter (SymbolicValue var)
			{
				return SymbolicValue.GetUniqueKey (var);
			}
			#endregion
                }
		#endregion
	}
}
