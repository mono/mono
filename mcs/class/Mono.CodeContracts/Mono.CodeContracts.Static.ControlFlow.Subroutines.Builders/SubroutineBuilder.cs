// 
// SubroutineBuilder.cs
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
using Mono.CodeContracts.Static.ControlFlow.Blocks;
using Mono.CodeContracts.Static.DataStructures;
using Mono.CodeContracts.Static.Providers;

namespace Mono.CodeContracts.Static.ControlFlow.Subroutines.Builders {
	abstract class SubroutineBuilder<Label> {
		public readonly SubroutineFacade SubroutineFacade;

		private readonly Dictionary<Label, Pair<Method, bool>> labels_for_call_sites = new Dictionary<Label, Pair<Method, bool>> ();
		private readonly Dictionary<Label, Method> labels_for_new_obj_sites = new Dictionary<Label, Method> ();
		private readonly HashSet<Label> labels_starting_blocks = new HashSet<Label> ();
		private readonly HashSet<Label> target_labels = new HashSet<Label> ();

		protected SubroutineBuilder (ICodeProvider<Label> codeProvider,
		                             SubroutineFacade subroutineFacade,
		                             Label entry)
		{
			this.SubroutineFacade = subroutineFacade;
			CodeProvider = codeProvider;
			AddTargetLabel (entry);
		}

		public ICodeProvider<Label> CodeProvider { get; private set; }
		public abstract SubroutineBase<Label> CurrentSubroutine { get; }

		public IMetaDataProvider MetaDataProvider
		{
			get { return this.SubroutineFacade.MetaDataProvider; }
		}

		protected IContractProvider ContractProvider
		{
			get { return this.SubroutineFacade.ContractProvider; }
		}

		public void AddTargetLabel (Label target)
		{
			AddBlockStart (target);
			this.target_labels.Add (target);
		}

		public void AddBlockStart (Label target)
		{
			this.labels_starting_blocks.Add (target);
		}

		protected void Initialize (Label entry)
		{
			new BlockStartGatherer<Label> (this).TraceAggregateSequentally (entry);
		}

		public bool IsBlockStart (Label label)
		{
			return this.labels_starting_blocks.Contains (label);
		}

		public bool IsTargetLabel (Label label)
		{
			return this.target_labels.Contains (label);
		}

		public bool IsMethodCallSite (Label label, out Pair<Method, bool> methodVirtPair)
		{
			return this.labels_for_call_sites.TryGetValue (label, out methodVirtPair);
		}

		public bool IsNewObjSite (Label label, out Method constructor)
		{
			return this.labels_for_new_obj_sites.TryGetValue (label, out constructor);
		}

		protected BlockWithLabels<Label> BuildBlocks (Label entry)
		{
			return BlockBuilder<Label>.BuildBlocks (entry, this);
		}

		public virtual void RecordInformationSameAsOtherBlock (BlockWithLabels<Label> newBlock, BlockWithLabels<Label> currentBlock)
		{
		}

		public virtual BlockWithLabels<Label> RecordInformationForNewBlock (Label currentLabel, BlockWithLabels<Label> previousBlock)
		{
			BlockWithLabels<Label> block = CurrentSubroutine.GetBlock (currentLabel);
			if (previousBlock != null) {
				BlockWithLabels<Label> newBlock = block;
				BlockWithLabels<Label> prevBlock = previousBlock;
				if (block is MethodCallBlock<Label> && previousBlock is MethodCallBlock<Label>) {
					BlockWithLabels<Label> ab = CurrentSubroutine.NewBlock ();
					RecordInformationSameAsOtherBlock (ab, previousBlock);
					newBlock = ab;
					prevBlock = ab;
					CurrentSubroutine.AddSuccessor (previousBlock, EdgeTag.FallThrough, ab);
					CurrentSubroutine.AddSuccessor (ab, EdgeTag.FallThrough, block);
				} else
					CurrentSubroutine.AddSuccessor (previousBlock, EdgeTag.FallThrough, block);

				InsertPostConditionEdges (previousBlock, newBlock);
				InsertPreConditionEdges (prevBlock, block);
			}
			return block;
		}

		protected void InsertPreConditionEdges (BlockWithLabels<Label> previousBlock, BlockWithLabels<Label> newBlock)
		{
			var methodCallBlock = newBlock as MethodCallBlock<Label>;
			if (methodCallBlock == null || CurrentSubroutine.IsContract || CurrentSubroutine.IsOldValue)
				return;

			if (CurrentSubroutine.IsMethod) {
				var methodInfo = CurrentSubroutine as IMethodInfo;
				Property property;
				if (methodInfo != null && MetaDataProvider.IsConstructor (methodInfo.Method)
				    && MetaDataProvider.IsPropertySetter (methodCallBlock.CalledMethod, out property)
				    && MetaDataProvider.IsAutoPropertyMember (methodCallBlock.CalledMethod))
					return;
			}

			EdgeTag callTag = methodCallBlock.IsNewObj ? EdgeTag.BeforeNewObj : EdgeTag.BeforeCall;
			Subroutine requires = this.SubroutineFacade.GetRequires (methodCallBlock.CalledMethod);

			CurrentSubroutine.AddEdgeSubroutine (previousBlock, newBlock, requires, callTag);
		}

		protected void InsertPostConditionEdges (BlockWithLabels<Label> previousBlock, BlockWithLabels<Label> newBlock)
		{
			var methodCallBlock = previousBlock as MethodCallBlock<Label>;
			if (methodCallBlock == null)
				return;

			if (CurrentSubroutine.IsMethod) {
				var methodInfo = CurrentSubroutine as IMethodInfo;
				Property property;
				if (methodInfo != null && MetaDataProvider.IsConstructor (methodInfo.Method)
				    && MetaDataProvider.IsPropertyGetter (methodCallBlock.CalledMethod, out property)
				    && MetaDataProvider.IsAutoPropertyMember (methodCallBlock.CalledMethod))
					return;
			}

			EdgeTag callTag = methodCallBlock.IsNewObj ? EdgeTag.AfterNewObj : EdgeTag.AfterCall;
			Subroutine ensures = this.SubroutineFacade.GetEnsures (methodCallBlock.CalledMethod);

			CurrentSubroutine.AddEdgeSubroutine (previousBlock, newBlock, ensures, callTag);
		}

		public virtual void BeginOldHook (Label label)
		{
		}

		public virtual void EndOldHook (Label label)
		{
		}

		public void AddMethodCallSite (Label pc, Pair<Method, bool> methodVirtPair)
		{
			this.labels_for_call_sites [pc] = methodVirtPair;
		}

		public void AddNewObjSite (Label pc, Method method)
		{
			this.labels_for_new_obj_sites [pc] = method;
		}
	}
}
