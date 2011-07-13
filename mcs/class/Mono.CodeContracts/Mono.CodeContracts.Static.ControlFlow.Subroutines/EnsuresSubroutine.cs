// 
// EnsuresSubroutine.cs
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
using Mono.CodeContracts.Static.DataStructures;

namespace Mono.CodeContracts.Static.ControlFlow.Subroutines {
	sealed class EnsuresSubroutine<Label> : MethodContractSubroutine<Label>, IEquatable<EnsuresSubroutine<Label>> {
		private readonly Dictionary<int, Pair<CFGBlock, TypeNode>> inferred_old_label_reverse_map;

		public EnsuresSubroutine (SubroutineFacade subroutineFacade,
		                          Method method, IImmutableSet<Subroutine> inherited) : base (subroutineFacade, method)
		{
			this.inferred_old_label_reverse_map = new Dictionary<int, Pair<CFGBlock, TypeNode>> ();
			AddSuccessor (Entry, EdgeTag.Entry, Exit);
			AddBaseEnsures (Entry, Exit, inherited);
			Commit ();
		}

		public EnsuresSubroutine (SubroutineFacade subroutineFacade,
		                          Method method,
		                          SimpleSubroutineBuilder<Label> builder, Label startLabel, IImmutableSet<Subroutine> inherited)
			: base (subroutineFacade, method, builder, startLabel)
		{
			this.inferred_old_label_reverse_map = new Dictionary<int, Pair<CFGBlock, TypeNode>> ();
			AddBaseEnsures (Entry, GetTargetBlock (startLabel), inherited);
		}

		public override SubroutineKind Kind
		{
			get { return SubroutineKind.Ensures; }
		}

		public override bool IsEnsures
		{
			get { return true; }
		}

		public override bool IsContract
		{
			get { return true; }
		}

		#region IEquatable<EnsuresSubroutine<Label>> Members
		public bool Equals (EnsuresSubroutine<Label> other)
		{
			return Id == other.Id;
		}
		#endregion

		private void AddBaseEnsures (CFGBlock from, CFGBlock to, IImmutableSet<Subroutine> inherited)
		{
			if (inherited == null)
				return;
			foreach (Subroutine subroutine in inherited.Elements)
				AddEdgeSubroutine (from, to, subroutine, EdgeTag.Inherited);
		}

		public override void Initialize ()
		{
			if (Builder == null)
				return;
			Builder.BuildBlocks (this.StartLabel, this);
			Commit ();
			Builder = null;
		}

		public override BlockWithLabels<Label> NewBlock ()
		{
			return new EnsuresBlock<Label> (this, ref this.BlockIdGenerator);
		}

		public override void Commit ()
		{
			base.Commit ();

			var visitor = new OldScanStateMachine<Label> (this);
			EnsuresBlock<Label> priorBlock = null;

			foreach (CFGBlock block in Blocks) {
				var ensuresBlock = block as EnsuresBlock<Label>;
				if (ensuresBlock != null) {
					priorBlock = ensuresBlock;
					int count = ensuresBlock.Count;
					visitor.StartBlock (ensuresBlock);

					for (int i = 0; i < count; i++) {
						if (ensuresBlock.OriginalForwardDecode<int, Boolean, OldScanStateMachine<Label>> (i, visitor, i))
							ensuresBlock.AddInstruction (i);
					}
				} else
					visitor.HandlePotentialCallBlock (block as MethodCallBlock<Label>, priorBlock);
				foreach (CFGBlock succ in SuccessorBlocks (block))
					visitor.SetStartState (succ);
			}
		}

		public void AddInferredOldMap (int blockIndex, int instructionIndex, CFGBlock otherBlock, TypeNode endOldType)
		{
			this.inferred_old_label_reverse_map.Add (OverlayInstructionKey (blockIndex, instructionIndex), new Pair<CFGBlock, TypeNode> (otherBlock, endOldType));
		}

		private static int OverlayInstructionKey (int blockIndex, int instructionIndex)
		{
			return (instructionIndex << 16) + blockIndex;
		}

		public CFGBlock InferredBeginEndBijection (APC pc)
		{
			TypeNode endOldType;
			return InferredBeginEndBijection (pc, out endOldType);
		}

		public CFGBlock InferredBeginEndBijection (APC pc, out TypeNode endOldType)
		{
			Pair<CFGBlock, TypeNode> pair;
			if (!this.inferred_old_label_reverse_map.TryGetValue (OverlayInstructionKey (pc.Block.Index, pc.Index), out pair))
				throw new InvalidOperationException ("Fatal bug in ensures CFG begin/end old map");
			endOldType = pair.Value;
			return pair.Key;
		}
	}
}
