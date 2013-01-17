// 
// RequiresSubroutine.cs
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
using Mono.CodeContracts.Static.ControlFlow.Subroutines.Builders;
using Mono.CodeContracts.Static.DataStructures;

namespace Mono.CodeContracts.Static.ControlFlow.Subroutines {
	sealed class RequiresSubroutine<TLabel> : MethodContractSubroutine<TLabel>, IEquatable<RequiresSubroutine<TLabel>>
	{
		public override SubroutineKind Kind { get { return SubroutineKind.Requires; } }

		public override bool IsContract { get { return true; } }
		public override bool IsRequires { get { return true; } }

		public RequiresSubroutine(SubroutineFacade subroutineFacade,
		                          Method method,
		                          IImmutableSet<Subroutine> inherited)
			: base(subroutineFacade, method)
		{
			AddSuccessor(Entry, EdgeTag.Entry, Exit);
			AddBaseRequires(Exit, inherited);
			Commit();
		}

		public RequiresSubroutine(SubroutineFacade subroutineFacade,
		                          Method method,
		                          SimpleSubroutineBuilder<TLabel> builder,
		                          TLabel entryLabel,
		                          IImmutableSet<Subroutine> inheritedRequires)
			: base(subroutineFacade, method, builder, entryLabel)
		{
			AddBaseRequires(this.GetTargetBlock(entryLabel), inheritedRequires);
		}

		public override void Initialize()
		{
			if (this.Builder == null)
				return;

			this.Builder.BuildBlocks(this.StartLabel, this);
			this.Commit();

			this.Builder = null;
		}

		private void AddBaseRequires(CFGBlock targetOfEntry, IImmutableSet<Subroutine> inherited)
		{
			foreach (var subroutine in inherited.Elements)
				this.AddEdgeSubroutine(this.Entry, targetOfEntry, subroutine, EdgeTag.Inherited);
		}

		public bool Equals(RequiresSubroutine<TLabel> that)
		{
			return this.Id == that.Id;
		}
	}
}