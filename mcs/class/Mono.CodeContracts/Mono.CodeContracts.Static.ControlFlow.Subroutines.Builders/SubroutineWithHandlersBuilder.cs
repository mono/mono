// 
// SubroutineWithHandlersBuilder.cs
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
using Mono.CodeContracts.Static.DataStructures;
using Mono.CodeContracts.Static.Providers;

namespace Mono.CodeContracts.Static.ControlFlow.Subroutines.Builders {
	class SubroutineWithHandlersBuilder<Label, Handler> : SubroutineBuilder<Label> {
		private readonly Dictionary<Label, Handler> handler_starting_at = new Dictionary<Label, Handler> ();
		private readonly Method method;
		private readonly Dictionary<Label, Queue<Handler>> subroutine_handler_end_list = new Dictionary<Label, Queue<Handler>> ();
		private readonly Dictionary<Label, Queue<Handler>> try_end_list = new Dictionary<Label, Queue<Handler>> ();
		private readonly Dictionary<Label, Stack<Handler>> try_start_list = new Dictionary<Label, Stack<Handler>> ();
		private LispList<SubroutineWithHandlers<Label, Handler>> subroutine_stack;

		public SubroutineWithHandlersBuilder (IMethodCodeProvider<Label, Handler> codeProvider,
		                                      SubroutineFacade subroutineFacade,
		                                      Method method,
		                                      Label entry)
			: base (codeProvider, subroutineFacade, entry)
		{
			this.method = method;
			ComputeTryBlockStartAndEndInfo (this.method);
			Initialize (entry);
		}

		private new IMethodCodeProvider<Label, Handler> CodeProvider
		{
			get { return (IMethodCodeProvider<Label, Handler>) base.CodeProvider; }
		}

		protected SubroutineWithHandlers<Label, Handler> CurrentSubroutineWithHandlers
		{
			get { return this.subroutine_stack.Head; }
		}

		private LispList<Handler> CurrentProtectingHanlders
		{
			get { return CurrentSubroutineWithHandlers.CurrentProtectingHandlers; }
			set { CurrentSubroutineWithHandlers.CurrentProtectingHandlers = value; }
		}

		public override SubroutineBase<Label> CurrentSubroutine
		{
			get { return this.subroutine_stack.Head; }
		}

		private void ComputeTryBlockStartAndEndInfo (Method method)
		{
			foreach (Handler handler in CodeProvider.GetTryBlocks (method)) {
				if (CodeProvider.IsFilterHandler (handler))
					AddTargetLabel (CodeProvider.FilterExpressionStart (handler));
				AddTargetLabel (CodeProvider.HandlerStart (handler));
				AddTargetLabel (CodeProvider.HandlerEnd (handler));
				AddTryStart (handler);
				AddTryEnd (handler);
				AddHandlerEnd (handler);
				this.handler_starting_at.Add (CodeProvider.HandlerStart (handler), handler);
			}
		}

		private void AddHandlerEnd (Handler handler)
		{
			if (!IsFaultOrFinally (handler))
				return;

			Label handlerEnd = CodeProvider.HandlerEnd (handler);
			Queue<Handler> queue;
			this.subroutine_handler_end_list.TryGetValue (handlerEnd, out queue);
			if (queue == null) {
				queue = new Queue<Handler> ();
				this.subroutine_handler_end_list [handlerEnd] = queue;
			}
			queue.Enqueue (handler);
			AddTargetLabel (handlerEnd);
		}

		private void AddTryEnd (Handler handler)
		{
			Label tryEnd = CodeProvider.TryEnd (handler);
			Queue<Handler> queue;
			this.try_end_list.TryGetValue (tryEnd, out queue);
			if (queue == null) {
				queue = new Queue<Handler> ();
				this.try_end_list [tryEnd] = queue;
			}
			queue.Enqueue (handler);
			AddTargetLabel (tryEnd);
		}

		private void AddTryStart (Handler handler)
		{
			Label tryStart = CodeProvider.TryStart (handler);
			Stack<Handler> stack;
			this.try_start_list.TryGetValue (tryStart, out stack);
			if (stack == null) {
				stack = new Stack<Handler> ();
				this.try_start_list [tryStart] = stack;
			}
			stack.Push (handler);
			AddTargetLabel (tryStart);
		}

		public CFGBlock BuildBlocks (Label entry, SubroutineWithHandlers<Label, Handler> subroutine)
		{
			this.subroutine_stack = LispList<SubroutineWithHandlers<Label, Handler>>.Cons (subroutine, null);
			return base.BuildBlocks (entry);
		}

		public override void RecordInformationSameAsOtherBlock (BlockWithLabels<Label> newBlock, BlockWithLabels<Label> currentBlock)
		{
			LispList<Handler> list;
			if (!CurrentSubroutineWithHandlers.ProtectingHandlers.TryGetValue (currentBlock, out list))
				return;
			CurrentSubroutineWithHandlers.ProtectingHandlers.Add (newBlock, list);
		}

		public override BlockWithLabels<Label> RecordInformationForNewBlock (Label currentLabel, BlockWithLabels<Label> previousBlock)
		{
			BlockWithLabels<Label> result = null;
			Queue<Handler> handlerEnd = GetHandlerEnd (currentLabel);
			if (handlerEnd != null) {
				foreach (Handler handler in handlerEnd) {
					this.subroutine_stack.Head.Commit ();
					this.subroutine_stack = this.subroutine_stack.Tail;
					previousBlock = null;
				}
			}
			Queue<Handler> tryEnd = GetTryEnd (currentLabel);
			if (tryEnd != null) {
				foreach (Handler handler in tryEnd) {
					if (!Equals (handler, CurrentProtectingHanlders.Head))
						throw new InvalidOperationException ("wrong handler");
					CurrentProtectingHanlders = CurrentProtectingHanlders.Tail;
				}
			}
			Handler handler1;
			if (IsHandlerStart (currentLabel, out handler1)) {
				if (IsFaultOrFinally (handler1)) {
					SubroutineWithHandlers<Label, Handler> sub = !CodeProvider.IsFaultHandler (handler1)
					                                             	? new FinallySubroutine<Label, Handler> (this.SubroutineFacade, currentLabel, this)
					                                             	: (FaultFinallySubroutineBase<Label, Handler>) new FaultSubroutine<Label, Handler> (this.SubroutineFacade, currentLabel, this);
					CurrentSubroutineWithHandlers.FaultFinallySubroutines.Add (handler1, sub);
					this.subroutine_stack = this.subroutine_stack.Cons (sub);
					previousBlock = null;
				} else
					result = CurrentSubroutineWithHandlers.CreateCatchFilterHeader (handler1, currentLabel);
			}
			if (result == null)
				result = base.RecordInformationForNewBlock (currentLabel, previousBlock);
			Stack<Handler> tryStart = GetTryStart (currentLabel);
			if (tryStart != null) {
				foreach (Handler handler in tryStart)
					CurrentProtectingHanlders = CurrentProtectingHanlders.Cons (handler);
			}

			CurrentSubroutineWithHandlers.ProtectingHandlers.Add (result, CurrentProtectingHanlders);
			return result;
		}

		private bool IsFaultOrFinally (Handler handler)
		{
			return CodeProvider.IsFaultHandler (handler) || CodeProvider.IsFinallyHandler (handler);
		}

		private bool IsHandlerStart (Label currentLabel, out Handler handler)
		{
			return this.handler_starting_at.TryGetValue (currentLabel, out handler);
		}

		private Stack<Handler> GetTryStart (Label currentLabel)
		{
			Stack<Handler> queue;
			this.try_start_list.TryGetValue (currentLabel, out queue);
			return queue;
		}

		private Queue<Handler> GetTryEnd (Label currentLabel)
		{
			Queue<Handler> queue;
			this.try_end_list.TryGetValue (currentLabel, out queue);
			return queue;
		}

		private Queue<Handler> GetHandlerEnd (Label currentLabel)
		{
			Queue<Handler> queue;
			this.subroutine_handler_end_list.TryGetValue (currentLabel, out queue);
			return queue;
		}
	}
}
