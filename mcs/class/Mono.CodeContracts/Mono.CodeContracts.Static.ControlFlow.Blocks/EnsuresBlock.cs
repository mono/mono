// 
// EnsuresBlock.cs
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
using Mono.CodeContracts.Static.ControlFlow.Subroutines;
using Mono.CodeContracts.Static.DataStructures;

namespace Mono.CodeContracts.Static.ControlFlow.Blocks {
	class EnsuresBlock<Label> : BlockWithLabels<Label> {
		private const uint Mask = 0xC0000000u;
		private const uint BeginOldMask = 0x80000000u;
		private const uint EndOldMask = 0x40000000u;

		private List<uint> overridingLabels;

		public EnsuresBlock (SubroutineBase<Label> subroutine, ref int idGen)
			: base (subroutine, ref idGen)
		{
		}

		private new EnsuresSubroutine<Label> Subroutine
		{
			get { return (EnsuresSubroutine<Label>) base.Subroutine; }
		}

		public override int Count
		{
			get
			{
				if (this.overridingLabels != null)
					return this.overridingLabels.Count;
				return base.Count;
			}
		}

		public bool UsesOverriding
		{
			get { return this.overridingLabels != null; }
		}

		public override bool TryGetLabel (int index, out Label label)
		{
			int originalOffset;
			if (IsOriginal (index, out originalOffset))
				return base.TryGetLabel (originalOffset, out label);
			label = default(Label);
			return false;
		}

		public Result OriginalForwardDecode<Data, Result, Visitor> (int index, Visitor visitor, Data data)
			where Visitor : IAggregateVisitor<Label, Data, Result>
		{
			Label label;
			if (base.TryGetLabel (index, out label))
				return Subroutine.CodeProvider.Decode<Visitor, Data, Result> (label, visitor, data);

			throw new InvalidOperationException ("should not happen");
		}

		public override Result ForwardDecode<Data, Result, Visitor> (APC pc, Visitor visitor, Data data)
		{
			Label label;
			if (TryGetLabel (pc.Index, out label))
				return base.ForwardDecode<Data, Result, Visitor> (pc, visitor, data);

			int endOldIndex;
			if (IsBeginOld (pc.Index, out endOldIndex)) {
				CFGBlock block = Subroutine.InferredBeginEndBijection (pc);
				return visitor.BeginOld (pc, new APC (block, endOldIndex, pc.SubroutineContext), data);
			}

			int beginOldIndex;
			if (IsEndOld (pc.Index, out beginOldIndex)) {
				TypeNode endOldType;
				CFGBlock block = Subroutine.InferredBeginEndBijection (pc, out endOldType);
				return visitor.EndOld (pc, new APC (block, beginOldIndex, pc.SubroutineContext), endOldType, Dummy.Value, Dummy.Value, data);
			}

			return visitor.Nop (pc, data);
		}

		private bool IsEndOld (int index, out int beginOldIndex)
		{
			if (this.overridingLabels != null && index < this.overridingLabels.Count && (this.overridingLabels [index] & EndOldMask) != 0) {
				beginOldIndex = (int) (this.overridingLabels [index] & (EndOldMask - 1));
				return true;
			}

			beginOldIndex = 0;
			return false;
		}

		private bool IsBeginOld (int index, out int endOldIndex)
		{
			if (this.overridingLabels != null && index < this.overridingLabels.Count && (this.overridingLabels [index] & BeginOldMask) != 0) {
				endOldIndex = (int) (this.overridingLabels [index] & (EndOldMask - 1));
				return true;
			}

			endOldIndex = 0;
			return false;
		}

		private bool IsOriginal (int index, out int originalOffset)
		{
			if (this.overridingLabels == null) {
				originalOffset = index;
				return true;
			}
			if (index < this.overridingLabels.Count && (this.overridingLabels [index] & Mask) == 0) {
				originalOffset = (int) (this.overridingLabels [index] & (EndOldMask - 1));
				return true;
			}

			originalOffset = 0;
			return false;
		}

		public void StartOverridingLabels ()
		{
			this.overridingLabels = new List<uint> ();
		}

		public void BeginOld (int index)
		{
			if (this.overridingLabels == null) {
				StartOverridingLabels ();
				for (int i = 0; i < index; ++i)
					this.overridingLabels.Add ((uint) i);
			}
			this.overridingLabels.Add (BeginOldMask);
		}

		public void AddInstruction (int index)
		{
			this.overridingLabels.Add ((uint) index);
		}

		public void EndOld (int index, TypeNode nextEndOldType)
		{
			AddInstruction (index);
			EndOldWithoutInstruction (nextEndOldType);
		}

		public void EndOldWithoutInstruction (TypeNode nextEndOldType)
		{
			int endOldIndex = this.overridingLabels.Count;
			CFGBlock beginBlock;
			this.overridingLabels.Add ((uint) (EndOldMask | PatchPriorBeginOld (this, endOldIndex, out beginBlock)));
			Subroutine.AddInferredOldMap (this.Index, endOldIndex, beginBlock, nextEndOldType);
		}

		private int PatchPriorBeginOld (CFGBlock endBlock, int endOldIndex, out CFGBlock beginBlock)
		{
			for (int i = this == endBlock ? endOldIndex - 2 : Count - 1; i >= 0; i--) {
				int endOldI;
				if (IsBeginOld (i, out endOldI)) {
					this.overridingLabels [i] = BeginOldMask | (uint)endOldIndex;
					beginBlock = this;
					Subroutine.AddInferredOldMap (this.Index, i, endBlock, default(TypeNode));
					return i;
				}
			}

			IEnumerator<CFGBlock> enumerator = Subroutine.PredecessorBlocks (this).GetEnumerator ();
			if (!enumerator.MoveNext ())
				throw new InvalidOperationException ("missing begin_old");
			int result = PatchPriorBeginOld (endBlock, endOldIndex, enumerator.Current, out beginBlock);
			enumerator.MoveNext ();
			return result;
		}

		private int PatchPriorBeginOld (CFGBlock endBlock, int endOldIndex, CFGBlock current, out CFGBlock beginBlock)
		{
			var ensuresBlock = current as EnsuresBlock<Label>;
			if (ensuresBlock != null)
				return ensuresBlock.PatchPriorBeginOld (endBlock, endOldIndex, out beginBlock);
			IEnumerator<CFGBlock> enumerator = current.Subroutine.PredecessorBlocks (current).GetEnumerator ();
			if (!enumerator.MoveNext ())
				throw new InvalidOperationException ("missing begin_old");

			int result = PatchPriorBeginOld (endBlock, endOldIndex, enumerator.Current, out beginBlock);
			enumerator.MoveNext ();
			return result;
		}
	}
}
