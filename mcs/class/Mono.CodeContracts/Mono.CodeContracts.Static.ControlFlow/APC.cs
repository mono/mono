// 
// APC.cs
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
using System.Text;
using Mono.CodeContracts.Static.AST;
using Mono.CodeContracts.Static.DataStructures;

namespace Mono.CodeContracts.Static.ControlFlow {
	struct APC : IEquatable<APC> {
		public static readonly APC Dummy = new APC (null, 0, null);

		public readonly CFGBlock Block;
		public readonly int Index;
		public readonly LispList<Edge<CFGBlock, EdgeTag>> SubroutineContext;

		public APC (CFGBlock block, int index, LispList<Edge<CFGBlock, EdgeTag>> subroutineContext)
		{
			this.Block = block;
			this.Index = index;
			this.SubroutineContext = subroutineContext;
		}

		public IEnumerable<APC> Successors
		{
			get { return this.Block.Subroutine.Successors (this); }
		}

		public bool InsideContract
		{
			get
			{
				Subroutine sub = this.Block.Subroutine;
				return sub.IsContract || sub.IsOldValue;
			}
		}

		public bool InsideConstructor
		{
			get
			{
				LispList<Edge<CFGBlock, EdgeTag>> ctx = this.SubroutineContext;
				CFGBlock block = this.Block;
				while (block != null) {
					Subroutine subroutine = block.Subroutine;
					if (subroutine.IsConstructor)
						return true;
					if (subroutine.IsMethod)
						return false;
					if (ctx != null) {
						block = ctx.Head.From;
						ctx = ctx.Tail;
					} else
						block = null;
				}
				return false;
			}
		}

		public bool InsideEnsuresInMethod
		{
			get
			{
				if (!this.Block.Subroutine.IsEnsuresOrOldValue || this.SubroutineContext == null)
					return false;
				foreach (var edge in this.SubroutineContext.AsEnumerable ()) {
					if (edge.Tag == EdgeTag.Exit || edge.Tag == EdgeTag.Entry || edge.Tag.Is (EdgeTag.AfterMask))
						return true;
				}

				return false;
			}
		}

		public bool InsideRequiresAtCall
		{
			get
			{
				if (!this.Block.Subroutine.IsRequires || this.SubroutineContext == null)
					return false;

				foreach (var edge in this.SubroutineContext.AsEnumerable ()) {
					if (edge.Tag == EdgeTag.Entry)
						return false;
					if (edge.Tag.Is (EdgeTag.BeforeMask))
						return true;
				}

				return false;
			}
		}

		public bool InsideEnsuresAtCall
		{
			get
			{
				if (!this.Block.Subroutine.IsRequires || this.SubroutineContext == null)
					return false;

				foreach (var edge in this.SubroutineContext.AsEnumerable ()) {
					if (edge.Tag == EdgeTag.Exit)
						return false;
					if (edge.Tag.Is (EdgeTag.BeforeMask))
						return true;
				}

				return false;
			}
		}

		public bool InsideInvariantOnExit
		{
			get
			{
				if (!this.Block.Subroutine.IsInvariant || this.SubroutineContext == null)
					return false;
				foreach (var edge in this.SubroutineContext.AsEnumerable ()) {
					if (edge.Tag == EdgeTag.Exit)
						return true;
					if (edge.Tag == EdgeTag.Entry || edge.Tag.Is (EdgeTag.AfterMask))
						return false;
				}

				return false;
			}
		}

		public bool InsideInvariantInMethod
		{
			get
			{
				if (!this.Block.Subroutine.IsInvariant || this.SubroutineContext == null)
					return false;

				foreach (var edge in this.SubroutineContext.AsEnumerable ()) {
					if (edge.Tag == EdgeTag.Exit || edge.Tag == EdgeTag.Entry || edge.Tag.Is (EdgeTag.AfterMask))
						return true;
				}

				return false;
			}
		}

		public bool InsideInvariantAtCall
		{
			get
			{
				if (!this.Block.Subroutine.IsInvariant || this.SubroutineContext == null)
					return false;
				foreach (var edge in this.SubroutineContext.AsEnumerable ()) {
					if (edge.Tag == EdgeTag.Exit || edge.Tag == EdgeTag.Entry)
						return false;
					if (edge.Tag.Is (EdgeTag.AfterMask))
						return true;
				}

				return false;
			}
		}

		public bool InsideOldManifestation
		{
			get { throw new NotImplementedException (); }
		}

		public bool InsideRequiresAtCallInsideContract
		{
			get
			{
				if (!this.Block.Subroutine.IsRequires || this.SubroutineContext == null)
					return false;
				for (LispList<Edge<CFGBlock, EdgeTag>> list = this.SubroutineContext; list != null; list = list.Tail) {
					if (list.Head.Tag == EdgeTag.Entry)
						return false;
					if (list.Head.Tag.Is (EdgeTag.BeforeMask)) {
						Subroutine sub = list.Head.From.Subroutine;
						return sub.IsEnsuresOrOldValue || sub.IsRequires || sub.IsInvariant;
					}
				}
				throw new InvalidOperationException ("Should not happen");
			}
		}

		#region IEquatable<APC> Members
		public bool Equals (APC other)
		{
			return (this.Block == other.Block && this.Index == other.Index && this.SubroutineContext == other.SubroutineContext);
		}
		#endregion

		public APC Next ()
		{
			if (this.Index < this.Block.Count)
				return new APC (this.Block, this.Index + 1, this.SubroutineContext);

			return this;
		}

		public static APC ForEnd (CFGBlock block, LispList<Edge<CFGBlock, EdgeTag>> subroutineContext)
		{
			return new APC (block, block.Count, subroutineContext);
		}

		public static APC ForStart (CFGBlock block, LispList<Edge<CFGBlock, EdgeTag>> subroutineContext)
		{
			return new APC (block, 0, subroutineContext);
		}

		public APC LastInBlock ()
		{
			return ForEnd (this.Block, this.SubroutineContext);
		}

		public bool TryGetContainingMethod (out Method method)
		{
			LispList<Edge<CFGBlock, EdgeTag>> list = this.SubroutineContext;
			CFGBlock block = this.Block;
			while (block != null) {
				var mi = block.Subroutine as IMethodInfo;
				if (mi != null) {
					method = mi.Method;
					return true;
				}

				if (list != null) {
					block = list.Head.From;
					list = list.Tail;
				} else
					block = null;
			}
			method = default(Method);
			return false;
		}

		public static void ToString (StringBuilder sb, LispList<Edge<CFGBlock, EdgeTag>> context)
		{
			bool wasFirst = false;
			for (; context != null; context = context.Tail) {
				if (!wasFirst) {
					sb.Append ("{");
					wasFirst = true;
				} else
					sb.Append (",");
				Edge<CFGBlock, EdgeTag> head = context.Head;
				sb.AppendFormat ("(SR{2} {0},{1}) [{3}]", head.From.Index, head.To.Index, head.From.Subroutine.Id, head.Tag);
			}
			if (!wasFirst)
				return;
			sb.Append ("}");
		}

		public override string ToString ()
		{
			var sb = new StringBuilder ();
			sb.Append ("[");
			sb.AppendFormat ("SR{2} {0},{1}", this.Block.Index, this.Index, this.Block.Subroutine.Id);
			ToString (sb, this.SubroutineContext);
			sb.Append ("]");

			return sb.ToString ();
		}
	}
}
