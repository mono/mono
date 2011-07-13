// 
// Subroutine.cs
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
using System.IO;
using Mono.CodeContracts.Static.Analysis;
using Mono.CodeContracts.Static.DataStructures;

namespace Mono.CodeContracts.Static.ControlFlow {
	abstract class Subroutine : ITypedProperties, IEquatable<Subroutine> {
		private static int _subroutineIdGenerator;
		private readonly TypedProperties properties = new TypedProperties ();
		private readonly int subroutine_id = _subroutineIdGenerator++;

		public virtual SubroutineKind Kind
		{
			get { return SubroutineKind.Unknown; }
		}

		public int Id
		{
			get { return this.subroutine_id; }
		}

		public abstract CFGBlock Entry { get; }
		public abstract CFGBlock EntryAfterRequires { get; }
		public abstract CFGBlock Exit { get; }
		public abstract CFGBlock ExceptionExit { get; }
		public abstract string Name { get; }
		public abstract int BlockCount { get; }
		public abstract IEnumerable<CFGBlock> Blocks { get; }

		public virtual bool IsRequires
		{
			get { return false; }
		}

		public virtual bool IsEnsures
		{
			get { return false; }
		}

		public virtual bool IsOldValue
		{
			get { return false; }
		}

		public virtual bool IsMethod
		{
			get { return false; }
		}

		public virtual bool IsConstructor
		{
			get { return false; }
		}

		public virtual bool IsInvariant
		{
			get { return false; }
		}

		public virtual bool IsContract
		{
			get { return false; }
		}

		public bool IsEnsuresOrOldValue
		{
			get { return IsEnsures || IsOldValue; }
		}

		public abstract EdgeMap<EdgeTag> SuccessorEdges { get; }
		public abstract EdgeMap<EdgeTag> PredecessorEdges { get; }
		public abstract DepthFirst.Visitor<CFGBlock, Dummy> EdgeInfo { get; }

		public virtual bool IsFaultFinally
		{
			get { return false; }
		}

		public abstract bool HasReturnValue { get; }
		public abstract bool HasContextDependentStackDepth { get; }

		public abstract int StackDelta { get; }

		#region ITypedProperties Members
		public bool TryGetValue<T> (TypedKey key, out T value)
		{
			return this.properties.TryGetValue (key, out value);
		}
		#endregion

		#region Implementation of ITypedProperties
		public bool Contains (TypedKey key)
		{
			return this.properties.Contains (key);
		}

		public void Add<T> (TypedKey key, T value)
		{
			this.properties.Add (key, value);
		}
		#endregion

		public override string ToString ()
		{
			return string.Format ("SR {0}: BlockCount:{1}, Kind:{2}", Id, BlockCount, Kind);
		}

		public abstract IEnumerable<CFGBlock> SuccessorBlocks (CFGBlock block);

		public IEnumerable<Pair<EdgeTag, CFGBlock>> SuccessorEdgesFor (CFGBlock block)
		{
			return SuccessorEdges [block];
		}

		public abstract IEnumerable<CFGBlock> PredecessorBlocks (CFGBlock block);

		public abstract bool IsJoinPoint (CFGBlock block);
		public abstract bool IsSplitPoint (CFGBlock block);
		public abstract bool HasSingleSuccessor (APC point, out APC ifFound);
		public abstract bool HasSinglePredecessor (APC point, out APC ifFound);

		public abstract void AddEdgeSubroutine (CFGBlock from, CFGBlock to, Subroutine subroutine, EdgeTag tag);

		public abstract IEnumerable<APC> Successors (APC pc);
		public abstract IEnumerable<APC> Predecessors (APC pc);

		public abstract bool IsSubroutineEnd (CFGBlock block);
		public abstract bool IsSubroutineStart (CFGBlock block);
		public abstract bool IsCatchFilterHeader (CFGBlock block);

		public abstract APC ComputeTargetFinallyContext (APC pc, CFGBlock succ);
		public abstract LispList<Pair<EdgeTag, Subroutine>> EdgeSubroutinesOuterToInner (CFGBlock current, CFGBlock succ, out bool isExceptionHandlerEdge, LispList<Edge<CFGBlock, EdgeTag>> context);
		public abstract LispList<Pair<EdgeTag, Subroutine>> GetOrdinaryEdgeSubroutines (CFGBlock current, CFGBlock succ, LispList<Edge<CFGBlock, EdgeTag>> context);
		public abstract void Initialize ();
		public abstract IEnumerable<Subroutine> UsedSubroutines (HashSet<int> alreadySeen);

		public IEnumerable<Subroutine> UsedSubroutines ()
		{
			return UsedSubroutines (new HashSet<int> ());
		}

		public abstract IEnumerable<CFGBlock> ExceptionHandlers<Data, Type> (CFGBlock block, Subroutine innerSubroutine,
		                                                                     Data data, IHandlerFilter<Data> handlerPredicate);

		public abstract void Print (TextWriter tw, ILPrinter<APC> printer,
		                            Func<CFGBlock, IEnumerable<LispList<Edge<CFGBlock, EdgeTag>>>> contextLookup,
		                            LispList<Edge<CFGBlock, EdgeTag>> context,
		                            HashSet<Pair<Subroutine, LispList<Edge<CFGBlock, EdgeTag>>>> set);

		#region Implementation of IEquatable<Subroutine>
		public bool Equals (Subroutine other)
		{
			return Id == other.Id;
		}
		#endregion
	}
}
