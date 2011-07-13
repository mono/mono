// 
// StackDepthProvider.cs
// 
// Authors:
//	Alexander Chebaturkin (chebaturkin@gmail.com)
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
using Mono.CodeContracts.Static.AST;
using Mono.CodeContracts.Static.AST.Visitors;
using Mono.CodeContracts.Static.ControlFlow;
using Mono.CodeContracts.Static.DataStructures;
using Mono.CodeContracts.Static.Providers;

namespace Mono.CodeContracts.Static.Analysis.StackAnalysis {
	class StackDepthProvider<TContext> : IILVisitor<APC, Dummy, Dummy, StackInfo, StackInfo>,
	                                            IILDecoder<APC, int, int, IStackContextProvider, Dummy>,
	                                            IStackContextProvider, IStackContext,
	                                            IMethodContext,
	                                            ICFG,
	                                            IStackInfo where TContext : IMethodContextProvider {
		private readonly IILDecoder<APC, Dummy, Dummy, TContext, Dummy> il_decoder;
		private int cached_subroutine;
		private APCMap<int> local_stack_depth_cache;
		private APCMap<int> stack_depth_mirror_for_end_old;
		private bool recursion_guard;

		public StackDepthProvider (IILDecoder<APC, Dummy, Dummy, TContext, Dummy> ilDecoder,
		                           IMetaDataProvider metaDataProvider)
		{
			this.il_decoder = ilDecoder;
			MetaDataProvider = metaDataProvider;
		}

		#region Implementation of IILDecoder<APC,Local,Parameter,Method,Field,Type,int,int,IStackContextProvider<Field,Method>,Dummy>
		public TResult ForwardDecode<TData, TResult, TVisitor> (APC pc, TVisitor visitor, TData data)
			where TVisitor : IILVisitor<APC, int, int, TData, TResult>
		{
			if (pc.Index != 0 || pc.SubroutineContext != null || pc.Block != pc.Block.Subroutine.Exit || !pc.Block.Subroutine.IsMethod)
				return this.il_decoder.ForwardDecode<TData, TResult, StackDecoder<TContext, TData, TResult, TVisitor>> (pc, new StackDecoder<TContext, TData, TResult, TVisitor> (this, visitor), data);
			if (!pc.Block.Subroutine.HasReturnValue)
				return visitor.Return (pc, -1, data);

			int source = GlobalStackDepth (pc) - 1;
			return visitor.Return (pc, source, data);
		}

		public bool IsUnreachable (APC pc)
		{
			return false;
		}

		public Dummy EdgeData (APC @from, APC to)
		{
			return Dummy.Value;
		}
		#endregion

		public IMetaDataProvider MetaDataProvider { get; private set; }

		private ICFG UnderlyingCFG
		{
			get { return this.il_decoder.ContextProvider.MethodContext.CFG; }
		}

		#region IILDecoder<APC,int,int,IStackContextProvider,Dummy> Members
		public IStackContextProvider ContextProvider
		{
			get { return this; }
		}
		#endregion

		#region IStackContextProvider Members
		public IStackContext StackContext
		{
			get { return this; }
		}

		public IMethodContext MethodContext
		{
			get { return this; }
		}
		#endregion

		public int GlobalStackDepth (APC pc)
		{
			int num = LocalStackDepth (pc);

			if (pc.SubroutineContext == null || !pc.Block.Subroutine.HasContextDependentStackDepth)
				return num;

			CFGBlock block = pc.SubroutineContext.Head.From;
			return num + GlobalStackDepth (APC.ForEnd (block, pc.SubroutineContext.Tail));
		}

		public int LocalStackDepth (APC pc)
		{
			return LocalStackMap (pc.Block.Subroutine) [pc];
		}

		private int OldStartDepth (Subroutine subroutine)
		{
			Method method = ((IMethodInfo) subroutine).Method;
			int count = MetaDataProvider.Parameters (method).Count;
			if (!MetaDataProvider.IsConstructor (method) && !MetaDataProvider.IsStatic (method))
				++count;
			return count;
		}

		private APCMap<int> LocalStackMap (Subroutine subroutine)
		{
			if (this.local_stack_depth_cache == null || this.cached_subroutine != subroutine.Id) {
				this.local_stack_depth_cache = GetStackDepthMap (subroutine);
				this.cached_subroutine = subroutine.Id;
			}
			return this.local_stack_depth_cache;
		}

		private APCMap<int> GetStackDepthMap (Subroutine subroutine)
		{
			APCMap<int> result;
			var key = new TypedKey ("stackDepthKey");
			if (!subroutine.TryGetValue (key, out result)) {
				result = ComputeStackDepthMap (subroutine);
				subroutine.Add (key, result);
			}
			return result;
		}

		private APCMap<int> ComputeStackDepthMap (Subroutine subroutine)
		{
			var startDepths = new Dictionary<int, StackInfo> (subroutine.BlockCount);
			APCMap<int> apcMap = this.stack_depth_mirror_for_end_old = new APCMap<int> (subroutine);

			foreach (CFGBlock block in subroutine.Blocks) {
				StackInfo stackInfo;
				if (!startDepths.TryGetValue (block.Index, out stackInfo))
					stackInfo = ComputeBlockStartDepth (block);
				foreach (APC apc in block.APCs ()) {
					apcMap.Add (apc, stackInfo.Depth);
					stackInfo = this.il_decoder.ForwardDecode<StackInfo, StackInfo, IILVisitor<APC, Dummy, Dummy, StackInfo, StackInfo>> (apc, this, stackInfo);
				}
				if (!apcMap.ContainsKey (block.Last))
					apcMap.Add (block.Last, stackInfo.Depth);
				foreach (CFGBlock successor in subroutine.SuccessorBlocks (block)) {
					bool oldRecursionGuard = this.recursion_guard;
					this.recursion_guard = true;
					try {
						bool isExceptionHandlerEdge;
						foreach (var info in subroutine.EdgeSubroutinesOuterToInner (block, successor, out isExceptionHandlerEdge, null).AsEnumerable ())
							stackInfo.Adjust (info.Value.StackDelta);
					} finally {
						this.recursion_guard = oldRecursionGuard;
					}
					AddStartDepth (startDepths, successor, stackInfo);
				}
			}
			return apcMap;
		}

		private StackInfo ComputeBlockStartDepth (CFGBlock block)
		{
			if (block.Subroutine.IsCatchFilterHeader (block))
				return new StackInfo (1, 2);
			return new StackInfo (0, 4);
		}

		private void AddStartDepth (Dictionary<int, StackInfo> dict, CFGBlock block, StackInfo stackDepth)
		{
			StackInfo stackInfo;
			if (dict.TryGetValue (block.Index, out stackInfo))
				return;
			dict.Add (block.Index, stackDepth.Clone ());
		}

		#region Implementation of ICFG
		APC ICFG.Entry
		{
			get { return UnderlyingCFG.Entry; }
		}

		APC ICFG.EntryAfterRequires
		{
			get { return UnderlyingCFG.EntryAfterRequires; }
		}

		APC ICFG.NormalExit
		{
			get { return UnderlyingCFG.NormalExit; }
		}

		APC ICFG.ExceptionExit
		{
			get { return UnderlyingCFG.ExceptionExit; }
		}

		Subroutine ICFG.Subroutine
		{
			get { return UnderlyingCFG.Subroutine; }
		}

		APC ICFG.Next (APC pc)
		{
			APC singleSuccessor;
			if (((ICFG) this).HasSingleSuccessor (pc, out singleSuccessor))
				return singleSuccessor;
			return pc;
		}

		bool ICFG.HasSingleSuccessor (APC pc, out APC successor)
		{
			DecoratorHelper.Push (this);
			try {
				return UnderlyingCFG.HasSingleSuccessor (pc, out successor);
			} finally {
				DecoratorHelper.Pop ();
			}
		}

		bool ICFG.HasSinglePredecessor (APC pc, out APC predecessor)
		{
			DecoratorHelper.Push (this);
			try {
				return UnderlyingCFG.HasSinglePredecessor (pc, out predecessor);
			} finally {
				DecoratorHelper.Pop ();
			}
		}

		IEnumerable<APC> ICFG.Successors (APC pc)
		{
			DecoratorHelper.Push (this);
			try {
				return UnderlyingCFG.Successors (pc);
			} finally {
				DecoratorHelper.Pop ();
			}
		}

		IEnumerable<APC> ICFG.Predecessors (APC pc)
		{
			DecoratorHelper.Push (this);
			try {
				return UnderlyingCFG.Predecessors (pc);
			} finally {
				DecoratorHelper.Pop ();
			}
		}

		bool ICFG.IsJoinPoint (APC pc)
		{
			return UnderlyingCFG.IsJoinPoint (pc);
		}

		bool ICFG.IsSplitPoint (APC pc)
		{
			return UnderlyingCFG.IsSplitPoint (pc);
		}

		bool ICFG.IsBlockStart (APC pc)
		{
			return UnderlyingCFG.IsBlockStart (pc);
		}

		bool ICFG.IsBlockEnd (APC pc)
		{
			return UnderlyingCFG.IsBlockEnd (pc);
		}

		IILDecoder<APC, Dummy, Dummy, IMethodContextProvider, Dummy> ICFG.GetDecoder (IMetaDataProvider metaDataProvider)
		{
			return UnderlyingCFG.GetDecoder (metaDataProvider);
		}

		void ICFG.Print (TextWriter tw, ILPrinter<APC> printer, Func<CFGBlock, IEnumerable<LispList<Edge<CFGBlock, EdgeTag>>>> contextLookup,
		                 LispList<Edge<CFGBlock, EdgeTag>> context)
		{
			DecoratorHelper.Push (this);
			try {
				UnderlyingCFG.Print (tw, printer, contextLookup, context);
			} finally {
				DecoratorHelper.Pop ();
			}
		}
		#endregion

		#region Implementation of IStackInfo
		bool IStackInfo.IsCallOnThis (APC pc)
		{
			if (this.recursion_guard)
				return false;
			return LocalStackMap (pc.Block.Subroutine).IsCallOnThis (pc);
		}
		#endregion

		#region Implementation of IStackContext<Field,Method>
		public int StackDepth (APC pc)
		{
			return GlobalStackDepth (pc);
		}
		#endregion

		#region Implementation of IMethodContext<Field,Method>
		Method IMethodContext.CurrentMethod
		{
			get { return this.il_decoder.ContextProvider.MethodContext.CurrentMethod; }
		}

		ICFG IMethodContext.CFG
		{
			get { return this; }
		}

		public IEnumerable<Field> Modifies (Method method)
		{
			return this.il_decoder.ContextProvider.MethodContext.Modifies (method);
		}

		public IEnumerable<Method> AffectedGetters (Field field)
		{
			return this.il_decoder.ContextProvider.MethodContext.AffectedGetters (field);
		}
		#endregion

		#region Implementation of IExpressionILVisitor<APC,Type,Dummy,Dummy,StackInfo,StackInfo>
		public StackInfo Binary (APC pc, BinaryOperator op, Dummy dest, Dummy operand1, Dummy operand2, StackInfo data)
		{
			return data.Pop (2).Push ();
		}

		public StackInfo Isinst (APC pc, TypeNode type, Dummy dest, Dummy obj, StackInfo data)
		{
			return data;
		}

		public StackInfo LoadNull (APC pc, Dummy dest, StackInfo polarity)
		{
			return polarity.Push ();
		}

		public StackInfo LoadConst (APC pc, TypeNode type, object constant, Dummy dest, StackInfo data)
		{
			return data.Push ();
		}

		public StackInfo Sizeof (APC pc, TypeNode type, Dummy dest, StackInfo data)
		{
			return data.Push ();
		}

		public StackInfo Unary (APC pc, UnaryOperator op, bool unsigned, Dummy dest, Dummy source, StackInfo data)
		{
			return data.Pop (1).Push ();
		}
		#endregion

		#region Implementation of ISyntheticILVisitor<APC,Method,Field,Type,Dummy,Dummy,StackInfo,StackInfo>
		public StackInfo Entry (APC pc, Method method, StackInfo data)
		{
			return data;
		}

		public StackInfo Assume (APC pc, EdgeTag tag, Dummy condition, StackInfo data)
		{
			return data.Pop (1);
		}

		public StackInfo Assert (APC pc, EdgeTag tag, Dummy condition, StackInfo data)
		{
			return data.Pop (1);
		}

		public StackInfo BeginOld (APC pc, APC matchingEnd, StackInfo data)
		{
			return new StackInfo (OldStartDepth (pc.Block.Subroutine), 4);
		}


		public StackInfo EndOld (APC pc, APC matchingBegin, TypeNode type, Dummy dest, Dummy source, StackInfo data)
		{
			return new StackInfo (this.stack_depth_mirror_for_end_old [matchingBegin] + 1, 4);
		}

		public StackInfo LoadStack (APC pc, int offset, Dummy dest, Dummy source, bool isOld, StackInfo data)
		{
			return data.Push (data [offset]);
		}

		public StackInfo LoadStackAddress (APC pc, int offset, Dummy dest, Dummy source, TypeNode type, bool isOld, StackInfo data)
		{
			return data.Push ();
		}

		public StackInfo LoadResult (APC pc, TypeNode type, Dummy dest, Dummy source, StackInfo data)
		{
			return data.Push ();
		}
		#endregion

		#region Implementation of IILVisitor<APC,Local,Parameter,Method,Field,Type,Dummy,Dummy,StackInfo,StackInfo>
		public StackInfo Arglist (APC pc, Dummy dest, StackInfo data)
		{
			return data.Push ();
		}

		public StackInfo Branch (APC pc, APC target, bool leavesExceptionBlock, StackInfo data)
		{
			return data;
		}

		public StackInfo BranchCond (APC pc, APC target, BranchOperator bop, Dummy value1, Dummy value2, StackInfo data)
		{
			return data.Pop (2);
		}

		public StackInfo BranchTrue (APC pc, APC target, Dummy cond, StackInfo data)
		{
			return data.Pop (1);
		}

		public StackInfo BranchFalse (APC pc, APC target, Dummy cond, StackInfo data)
		{
			return data.Pop (1);
		}

		public StackInfo Break (APC pc, StackInfo data)
		{
			return data;
		}

		public StackInfo Call<TypeList, ArgList> (APC pc, Method method, bool virt, TypeList extraVarargs, Dummy dest, ArgList args, StackInfo data)
			where TypeList : IIndexable<TypeNode>
			where ArgList : IIndexable<Dummy>
		{
			int count = MetaDataProvider.Parameters (method).Count + (extraVarargs == null ? 0 : extraVarargs.Count);
			if (!MetaDataProvider.IsStatic (method)) {
				if (data.IsThis (count))
					this.stack_depth_mirror_for_end_old.AddCallOnThis (pc);
				++count;
			}
			data = data.Pop (count);
			if (MetaDataProvider.IsVoidMethod (method))
				return data;
			return data.Push ();
		}

		public StackInfo Calli<TypeList, ArgList> (APC pc, TypeNode returnType, TypeList argTypes, bool instance, Dummy dest, Dummy functionPointer, ArgList args, StackInfo data)
			where TypeList : IIndexable<TypeNode>
			where ArgList : IIndexable<Dummy>
		{
			int count = 1;
			if (instance)
				++count;
			int slots = count + (argTypes == null ? 0 : argTypes.Count);
			data.Pop (slots);
			if (MetaDataProvider.IsVoid (returnType))
				return data;
			return data.Push ();
		}

		public StackInfo CheckFinite (APC pc, Dummy dest, Dummy source, StackInfo data)
		{
			return data;
		}

		public StackInfo CopyBlock (APC pc, Dummy destAddress, Dummy srcAddress, Dummy len, StackInfo data)
		{
			return data.Pop (3);
		}

		public StackInfo EndFilter (APC pc, Dummy decision, StackInfo data)
		{
			return data.Pop (1);
		}

		public StackInfo EndFinally (APC pc, StackInfo data)
		{
			return new StackInfo (0, 0);
		}

		public StackInfo Jmp (APC pc, Method method, StackInfo data)
		{
			return new StackInfo (0, 0);
		}

		public StackInfo LoadArg (APC pc, Parameter argument, bool isOld, Dummy dest, StackInfo data)
		{
			if (!MetaDataProvider.IsStatic (MetaDataProvider.DeclaringMethod (argument)) && MetaDataProvider.ParameterIndex (argument) == 0)
				return data.PushThis ();

			return data.Push ();
		}

		public StackInfo LoadArgAddress (APC pc, Parameter argument, bool isOld, Dummy dest, StackInfo data)
		{
			return data.Push ();
		}

		public StackInfo LoadLocal (APC pc, Local local, Dummy dest, StackInfo data)
		{
			return data.Push ();
		}

		public StackInfo LoadLocalAddress (APC pc, Local local, Dummy dest, StackInfo data)
		{
			return data.Push ();
		}

		public StackInfo Nop (APC pc, StackInfo data)
		{
			return data;
		}

		public StackInfo Pop (APC pc, Dummy source, StackInfo data)
		{
			return data.Pop (1);
		}

		public StackInfo Return (APC pc, Dummy source, StackInfo data)
		{
			return data;
		}

		public StackInfo StoreArg (APC pc, Parameter argument, Dummy source, StackInfo data)
		{
			return data.Pop (1);
		}

		public StackInfo StoreLocal (APC pc, Local local, Dummy source, StackInfo data)
		{
			return data.Pop (1);
		}

		public StackInfo Switch (APC pc, TypeNode type, IEnumerable<Pair<object, APC>> cases, Dummy value, StackInfo data)
		{
			return data.Pop (1);
		}

		public StackInfo Box (APC pc, TypeNode type, Dummy dest, Dummy source, StackInfo data)
		{
			return data.Pop (1).Push ();
		}

		public StackInfo ConstrainedCallvirt<TypeList, ArgList> (APC pc, Method method, TypeNode constraint, TypeList extraVarargs, Dummy dest, ArgList args, StackInfo data)
			where TypeList : IIndexable<TypeNode>
			where ArgList : IIndexable<Dummy>
		{
			int paramsCount = MetaDataProvider.Parameters (method).Count + (extraVarargs == null ? 0 : extraVarargs.Count);
			if (!MetaDataProvider.IsStatic (method)) {
				if (data.IsThis (paramsCount))
					this.stack_depth_mirror_for_end_old.AddCallOnThis (pc);
				++paramsCount;
			}

			data = data.Pop (paramsCount);
			if (MetaDataProvider.IsVoid (MetaDataProvider.ReturnType (method)))
				return data;

			return data.Push ();
		}

		public StackInfo CastClass (APC pc, TypeNode type, Dummy dest, Dummy obj, StackInfo data)
		{
			return data;
		}

		public StackInfo CopyObj (APC pc, TypeNode type, Dummy destPtr, Dummy sourcePtr, StackInfo data)
		{
			return data.Pop (2);
		}

		public StackInfo Initobj (APC pc, TypeNode type, Dummy ptr, StackInfo data)
		{
			return data.Pop (1);
		}

		public StackInfo LoadElement (APC pc, TypeNode type, Dummy dest, Dummy array, Dummy index, StackInfo data)
		{
			return data.Pop (2).Push ();
		}

		public StackInfo LoadField (APC pc, Field field, Dummy dest, Dummy obj, StackInfo data)
		{
			return data.Pop (1).Push ();
		}

		public StackInfo LoadFieldAddress (APC pc, Field field, Dummy dest, Dummy obj, StackInfo data)
		{
			return data.Pop (1).Push ();
		}

		public StackInfo LoadLength (APC pc, Dummy dest, Dummy array, StackInfo data)
		{
			return data.Pop (1).Push ();
		}

		public StackInfo LoadStaticField (APC pc, Field field, Dummy dest, StackInfo data)
		{
			return data.Push ();
		}

		public StackInfo LoadStaticFieldAddress (APC pc, Field field, Dummy dest, StackInfo data)
		{
			return data.Push ();
		}

		public StackInfo LoadTypeToken (APC pc, TypeNode type, Dummy dest, StackInfo data)
		{
			return data.Push ();
		}

		public StackInfo LoadFieldToken (APC pc, Field type, Dummy dest, StackInfo data)
		{
			return data.Push ();
		}

		public StackInfo LoadMethodToken (APC pc, Method type, Dummy dest, StackInfo data)
		{
			return data.Push ();
		}

		public StackInfo NewArray<ArgList> (APC pc, TypeNode type, Dummy dest, ArgList lengths, StackInfo data) where ArgList : IIndexable<Dummy>
		{
			return data.Pop (lengths.Count).Push ();
		}

		public StackInfo NewObj<ArgList> (APC pc, Method ctor, Dummy dest, ArgList args, StackInfo data) where ArgList : IIndexable<Dummy>
		{
			int paramsCount = MetaDataProvider.Parameters (ctor).Count;
			return data.Pop (paramsCount).Push ();
		}

		public StackInfo MkRefAny (APC pc, TypeNode type, Dummy dest, Dummy obj, StackInfo data)
		{
			return data;
		}

		public StackInfo RefAnyType (APC pc, Dummy dest, Dummy source, StackInfo data)
		{
			return data.Pop (1).Push ();
		}

		public StackInfo RefAnyVal (APC pc, TypeNode type, Dummy dest, Dummy source, StackInfo data)
		{
			return data.Pop (1).Push ();
		}

		public StackInfo Rethrow (APC pc, StackInfo data)
		{
			return new StackInfo (0, 0);
		}

		public StackInfo StoreElement (APC pc, TypeNode type, Dummy array, Dummy index, Dummy value, StackInfo data)
		{
			return data.Pop (3);
		}

		public StackInfo StoreField (APC pc, Field field, Dummy obj, Dummy value, StackInfo data)
		{
			return data.Pop (2);
		}

		public StackInfo StoreStaticField (APC pc, Field field, Dummy value, StackInfo data)
		{
			return data.Pop (1);
		}

		public StackInfo Throw (APC pc, Dummy exception, StackInfo data)
		{
			return new StackInfo (0, 0);
		}

		public StackInfo Unbox (APC pc, TypeNode type, Dummy dest, Dummy obj, StackInfo data)
		{
			return data.Pop (1).Push ();
		}

		public StackInfo UnboxAny (APC pc, TypeNode type, Dummy dest, Dummy obj, StackInfo data)
		{
			return data.Pop (1).Push ();
		}
		#endregion
	}
}
