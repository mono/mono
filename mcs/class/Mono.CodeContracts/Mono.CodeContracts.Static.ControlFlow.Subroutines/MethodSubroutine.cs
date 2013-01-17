// 
// MethodSubroutine.cs
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
using Mono.CodeContracts.Static.ControlFlow.Blocks;
using Mono.CodeContracts.Static.ControlFlow.Subroutines.Builders;
using Mono.CodeContracts.Static.Providers;

namespace Mono.CodeContracts.Static.ControlFlow.Subroutines {
	class MethodSubroutine<Label, Handler> : SubroutineWithHandlers<Label, Handler>, IMethodInfo {
		private readonly Method method;
		private HashSet<BlockWithLabels<Label>> blocks_ending_in_return_point;

		public MethodSubroutine (SubroutineFacade subroutineFacade, Method method)
			: base (subroutineFacade)
		{
			this.method = method;
		}

		public MethodSubroutine (SubroutineFacade SubroutineFacade,
		                         Method method, Label startLabel,
		                         SubroutineWithHandlersBuilder<Label, Handler> builder) : base (SubroutineFacade, startLabel, builder)
		{
			this.method = method;
			IMetaDataProvider metaDataProvider = this.SubroutineFacade.MetaDataProvider;
			builder.BuildBlocks (startLabel, this);
			BlockWithLabels<Label> targetBlock = GetTargetBlock (startLabel);
			Commit ();

			TypeNode type = metaDataProvider.DeclaringType (method);
			Subroutine invariant = this.SubroutineFacade.GetInvariant (type);
			if (invariant != null && !metaDataProvider.IsConstructor (method) && !metaDataProvider.IsStatic (method)) {
				AddEdgeSubroutine (Entry, targetBlock, invariant, EdgeTag.Entry);
				Subroutine requires = this.SubroutineFacade.GetRequires (method);
				if (requires != null)
					AddEdgeSubroutine (Entry, targetBlock, requires, EdgeTag.Entry);
			} else
				AddEdgeSubroutine (Entry, targetBlock, this.SubroutineFacade.GetRequires (method), EdgeTag.Entry);

			if (this.blocks_ending_in_return_point == null)
				return;

			Subroutine ensures = this.SubroutineFacade.GetEnsures (method);
			bool putInvariantAfterExit = !metaDataProvider.IsStatic (method) 
				&& !metaDataProvider.IsFinalizer (method) && !metaDataProvider.IsDispose (method);
			foreach (var block in this.blocks_ending_in_return_point) {
				if (putInvariantAfterExit)
					AddEdgeSubroutine (block, Exit, invariant, EdgeTag.Exit);
				AddEdgeSubroutine (block, Exit, ensures, EdgeTag.Exit);
			}

			if (ensures != null) {
				throw new NotImplementedException();
			}

			this.blocks_ending_in_return_point = null;
		}

		#region Overrides of Subroutine
		public override void Initialize ()
		{
		}
		#endregion

		#region Overrides of SubroutineBase<Label>
		public override void AddReturnBlock (BlockWithLabels<Label> block)
		{
			if (this.blocks_ending_in_return_point == null)
				this.blocks_ending_in_return_point = new HashSet<BlockWithLabels<Label>> ();

			this.blocks_ending_in_return_point.Add (block);

			base.AddReturnBlock (block);
		}
		#endregion

		#region Implementation of IMethodInfo<Method>
		public Method Method
		{
			get { return this.method; }
		}
		#endregion

		public override SubroutineKind Kind
		{
			get { return SubroutineKind.Method; }
		}

		public override bool HasReturnValue
		{
			get { return !this.SubroutineFacade.MetaDataProvider.IsVoidMethod (this.method); }
		}

		public override bool IsMethod
		{
			get { return true; }
		}

		public override bool IsConstructor
		{
			get { return this.SubroutineFacade.MetaDataProvider.IsConstructor (this.method); }
		}

		public override string Name
		{
			get { return this.SubroutineFacade.MetaDataProvider.FullName (this.method); }
		}
	}
}
