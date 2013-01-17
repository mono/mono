// 
// BlockWithLabels.cs
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
using Mono.CodeContracts.Static.ControlFlow.Subroutines;

namespace Mono.CodeContracts.Static.ControlFlow.Blocks {
	class BlockWithLabels<Label> : BlockBase, IEquatable<BlockWithLabels<Label>> {
		private readonly List<Label> labels;

		public BlockWithLabels (SubroutineBase<Label> subroutine, ref int idGen)
			: base (subroutine, ref idGen)
		{
			this.labels = new List<Label> ();
		}

		public override int Count
		{
			get { return this.labels.Count; }
		}

		protected new SubroutineBase<Label> Subroutine
		{
			get { return (SubroutineBase<Label>) base.Subroutine; }
		}

		#region IEquatable<BlockWithLabels<Label>> Members
		public bool Equals (BlockWithLabels<Label> other)
		{
			return this == other;
		}
		#endregion

		public override int GetILOffset (APC pc)
		{
			Label label;
			if (TryGetLabel (pc.Index, out label))
				return Subroutine.GetILOffset (label);

			return 0;
		}

		public void AddLabel (Label label)
		{
			this.labels.Add (label);
		}

		public bool TryGetLastLabel (out Label label)
		{
			if (this.labels.Count > 0) {
				label = this.labels [this.labels.Count - 1];
				return true;
			}

			label = default(Label);
			return false;
		}

		public virtual bool TryGetLabel (int index, out Label label)
		{
			if (index < this.labels.Count) {
				label = this.labels [index];
				return true;
			}

			label = default(Label);
			return false;
		}

		public override string ToString ()
		{
			return string.Format ("{0}: {1}", this.Index, GetType ().Name);
		}

		public override Result ForwardDecode<Data, Result, Visitor> (APC pc, Visitor visitor, Data data)
		{
			Label label;
			if (TryGetLabel (pc.Index, out label))
				return Subroutine.CodeProvider.Decode<LabelAdapter<Label, Data, Result, Visitor>, Data, Result> (label, new LabelAdapter<Label, Data, Result, Visitor> (visitor, pc), data);

			return visitor.Nop (pc, data);
		}
	}
}
