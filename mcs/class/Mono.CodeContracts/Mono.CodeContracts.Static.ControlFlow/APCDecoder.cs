// 
// APCDecoder.cs
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

using System.Collections.Generic;
using Mono.CodeContracts.Static.AST;
using Mono.CodeContracts.Static.AST.Visitors;
using Mono.CodeContracts.Static.Analysis;
using Mono.CodeContracts.Static.ControlFlow.Subroutines;
using Mono.CodeContracts.Static.DataStructures;
using Mono.CodeContracts.Static.Providers;

namespace Mono.CodeContracts.Static.ControlFlow {
	class APCDecoder : IILDecoder<APC, Dummy, Dummy, IMethodContextProvider, Dummy>, IMethodContextProvider, IMethodContext {
		private readonly SubroutineFacade subroutine_facade;
		private readonly ControlFlowGraph cfg;
		private readonly IMetaDataProvider meta_data_provider;

		public APCDecoder (ControlFlowGraph underlyingCFG,
		                   IMetaDataProvider metaDataProvider,
		                   SubroutineFacade subroutineFacade)
		{
			this.cfg = underlyingCFG;
			this.meta_data_provider = metaDataProvider;
			this.subroutine_facade = subroutineFacade;
		}

		#region IILDecoder<APC,Dummy,Dummy,IMethodContextProvider,Dummy> Members
		public IMethodContextProvider ContextProvider
		{
			get { return this; }
		}

		public TResult ForwardDecode<TData, TResult, TVisitor> (APC pc, TVisitor visitor, TData data)
			where TVisitor : IILVisitor<APC, Dummy, Dummy, TData, TResult>
		{
		        return this.subroutine_facade.ForwardDecode<TData, TResult, RemoveBranchDelegator<TData, TResult, TVisitor>> 
                                         (pc, new RemoveBranchDelegator<TData, TResult, TVisitor> (visitor, this.meta_data_provider), data);
		}

	        public bool IsUnreachable (APC pc)
		{
			return false;
		}

		public Dummy EdgeData (APC from, APC to)
		{
			return Dummy.Value;
		}
		#endregion

		public IMethodContext MethodContext
		{
			get { return this; }
		}

		public Method CurrentMethod
		{
			get { return this.cfg.CFGMethod; }
		}

		public ICFG CFG
		{
			get { return this.cfg; }
		}

		public IEnumerable<Field> Modifies (Method method)
		{
			return this.subroutine_facade.GetModifies (method);
		}

		public IEnumerable<Method> AffectedGetters (Field field)
		{
			return this.subroutine_facade.GetAffectedGetters (field);
		}
	}
}
