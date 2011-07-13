// 
// SubroutineFacade.cs
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
using System.Collections.Generic;
using Mono.CodeContracts.Static.AST;
using Mono.CodeContracts.Static.AST.Visitors;
using Mono.CodeContracts.Static.ControlFlow.Blocks;
using Mono.CodeContracts.Static.ControlFlow.Subroutines.Builders;
using Mono.CodeContracts.Static.DataStructures;
using Mono.CodeContracts.Static.Providers;

namespace Mono.CodeContracts.Static.ControlFlow.Subroutines {
	class SubroutineFacade : IMethodCodeConsumer<Dummy, Subroutine> {
		public readonly IContractProvider ContractProvider;
		public readonly IMetaDataProvider MetaDataProvider;

//		private readonly EnsuresFactory ensures_factory;
		private readonly Dictionary<Method, ICFG> method_cache = new Dictionary<Method, ICFG> ();
		private readonly RequiresFactory requires_factory;

		public SubroutineFacade (IMetaDataProvider metaDataProvider,
		                         IContractProvider contractProvider)
		{
			this.MetaDataProvider = metaDataProvider;
			this.ContractProvider = contractProvider;
			this.requires_factory = new RequiresFactory (this);
//			this.ensures_factory = new EnsuresFactory (this);
		}

		#region IMethodCodeConsumer<Dummy,Subroutine> Members
		Subroutine IMethodCodeConsumer<Dummy, Subroutine>.Accept<Label, Handler> (
			IMethodCodeProvider<Label, Handler> codeProvider,
			Label entry,
			Method method,
			Dummy data)
		{
			var builder = new SubroutineWithHandlersBuilder<Label, Handler> (codeProvider, this, method, entry);
			return new MethodSubroutine<Label, Handler> (this, method, entry, builder);
		}
		#endregion

		public Result ForwardDecode<Data, Result, Visitor> (APC pc, Visitor visitor, Data data)
			where Visitor : IILVisitor<APC, Dummy, Dummy, Data, Result>
		{
			var block = pc.Block as BlockBase;
			if (block != null)
				return block.ForwardDecode<Data, Result, Visitor> (pc, visitor, data);

			return visitor.Nop (pc, data);
		}

		public Subroutine GetRequires (Method method)
		{
			method = this.MetaDataProvider.Unspecialized (method);
			return this.requires_factory.Get (method);
		}

		public Subroutine GetEnsures (Method method)
		{
			return null;
			//todo: implement handling this in MethodSubroutine and uncomment lines below

//			method = this.MetaDataProvider.Unspecialized (method);
//			return this.ensures_factory.Get (method);
		}

		public Subroutine GetInvariant (TypeNode type)
		{
			//todo: implement this
			return null;
		}

		public ICFG GetControlFlowGraph (Method method)
		{
			if (this.method_cache.ContainsKey (method))
				return this.method_cache [method];

			if (!this.MetaDataProvider.HasBody (method))
				throw new InvalidOperationException ("Method has no body");

			return new ControlFlowGraph (this.MetaDataProvider.AccessMethodBody (method, this, Dummy.Value), this);
		}


		public void AddReads (Method method, Field field)
		{
			throw new NotImplementedException ();
		}


		public IEnumerable<Method> GetAffectedGetters (Field field)
		{
			//todo: implement this

			return new Method[0];
		}

		public IEnumerable<Field> GetModifies (Method method)
		{
			//todo: implement this
			return new Field[0];
		}
	}
}
