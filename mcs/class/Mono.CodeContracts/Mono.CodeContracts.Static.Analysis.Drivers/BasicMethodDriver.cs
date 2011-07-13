// 
// BasicMethodDriver.cs
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
using Mono.CodeContracts.Static.AST;
using Mono.CodeContracts.Static.Analysis.StackAnalysis;
using Mono.CodeContracts.Static.ControlFlow;
using Mono.CodeContracts.Static.DataStructures;

namespace Mono.CodeContracts.Static.Analysis.Drivers {
	class BasicMethodDriver {
		private readonly Method method;
		private readonly IBasicAnalysisDriver parent;
		private ICFG contract_free_cfg;

		private ICodeLayer<Dummy, Dummy, IMethodContextProvider, Dummy> contract_free_raw_layer;
		private ICodeLayer<int, int, IStackContextProvider, Dummy> contract_free_stack_layer;

		public BasicMethodDriver (Method method, IBasicAnalysisDriver parent)
		{
			this.method = method;
			this.parent = parent;

			RawLayer = CodeLayerFactory.Create (
			                                    this.parent.SubroutineFacade.GetControlFlowGraph (method).GetDecoder (parent.MetaDataProvider),
			                                    parent.MetaDataProvider,
			                                    parent.ContractProvider, dummy => "", dummy => "");

			if (DebugOptions.Debug) {
				Console.WriteLine ("-----APC based CFG-----");
				RawLayer.ILDecoder.ContextProvider.MethodContext.CFG.Print (Console.Out, RawLayer.Printer, null, null);
			}

			StackLayer = CodeLayerFactory.Create (
			                                      StackDepthFactory.Create (RawLayer.ILDecoder, RawLayer.MetaDataProvider),
			                                      RawLayer.MetaDataProvider,
			                                      RawLayer.ContractProvider, (i => "s" + i.ToString ()), i => "s" + i.ToString ()
				);

			if (DebugOptions.Debug)
			{
				Console.WriteLine ("-----Stack based CFG-----");
				StackLayer.ILDecoder.ContextProvider.MethodContext.CFG.Print (Console.Out, StackLayer.Printer, null, null);
			}
		}

		public Method CurrentMethod
		{
			get { return this.method; }
		}

		public IBasicAnalysisDriver AnalysisDriver
		{
			get { return this.parent; }
		}

		public ICodeLayer<Dummy, Dummy, IMethodContextProvider, Dummy> RawLayer { get; private set; }
		public ICodeLayer<int, int, IStackContextProvider, Dummy> StackLayer { get; private set; }

		public ICodeLayer<Dummy, Dummy, IMethodContextProvider, Dummy> ContractFreeRawLayer
		{
			get
			{
				if (this.contract_free_raw_layer == null) {
					this.contract_free_raw_layer =
						CodeLayerFactory.Create (ContractFreeCFG.GetDecoder (this.parent.MetaDataProvider),
						                         RawLayer.MetaDataProvider,
						                         RawLayer.ContractProvider,
						                         RawLayer.ExpressionToString, RawLayer.VariableToString, RawLayer.Printer);
				}
				return this.contract_free_raw_layer;
			}
		}

		public ICodeLayer<int, int, IStackContextProvider, Dummy> ContractFreeStackLayer
		{
			get
			{
				if (this.contract_free_stack_layer == null) {
					this.contract_free_stack_layer =
						CodeLayerFactory.Create (StackDepthFactory.Create (ContractFreeRawLayer.ILDecoder, this.contract_free_raw_layer.MetaDataProvider),
						                         ContractFreeRawLayer.MetaDataProvider,
						                         ContractFreeRawLayer.ContractProvider,
						                         StackLayer.ExpressionToString, StackLayer.VariableToString, StackLayer.Printer);
				}
				return this.contract_free_stack_layer;
			}
		}

		public ICFG ContractFreeCFG
		{
			get
			{
				if (this.contract_free_cfg == null) {
					this.contract_free_cfg = new ContractFilteredCFG (RawLayer.ILDecoder.ContextProvider.MethodContext.CFG);

					if (DebugOptions.Debug)
					{
						Console.WriteLine ("------raw contract-free cfg -----------------");
						this.contract_free_cfg.Print (Console.Out, RawLayer.Printer, null, null);
					}
				}
				return this.contract_free_cfg;
			}
		}

		public ICFG CFG
		{
			get { return StackLayer.ILDecoder.ContextProvider.MethodContext.CFG; }
		}
	}
}
