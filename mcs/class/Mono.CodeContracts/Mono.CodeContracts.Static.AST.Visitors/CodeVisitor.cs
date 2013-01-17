// 
// CodeVisitor.cs
// 
// Authors:
//	Alexander Chebaturkin (chebaturkin@gmail.com)
// 
// Copyright (C) 2012 Alexander Chebaturkin
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
using System.IO;
using Mono.CodeContracts.Static.Analysis;
using Mono.CodeContracts.Static.ControlFlow;
using Mono.CodeContracts.Static.DataFlowAnalysis;
using Mono.CodeContracts.Static.DataStructures;
using Mono.CodeContracts.Static.Providers;

namespace Mono.CodeContracts.Static.AST.Visitors {
	class CodeVisitor<Variable, Expression, ContextData, EdgeData>
		: ILVisitorBase<APC, Expression, Variable, bool, bool>,
		  IAnalysis<APC, bool, IILVisitor<APC, Expression, Variable, bool, bool>, EdgeData>
		where ContextData : IMethodContextProvider {
		private ICodeLayer<Expression, Variable, ContextData, EdgeData> codeLayer;

		public ContextData Context
		{
			get { return this.codeLayer.ILDecoder.ContextProvider; }
		}

		protected IMetaDataProvider MetaDataProvider
		{
			get { return this.codeLayer.MetaDataProvider; }
		}

		public void Run (ICodeLayer<Expression, Variable, ContextData, EdgeData> codeLayer)
		{
			this.codeLayer = codeLayer;
			codeLayer.CreateForward (this) (true);
		}

		#region Overrides of ILVisitorBase<APC,Expression,Variable,bool,bool>
		public override bool DefaultVisit (APC pc, bool data)
		{
			return data;
		}
		#endregion

		#region Implementation of IAnalysis<APC,bool,IILVisitor<APC,Expression,Variable,bool,bool>,EdgeData>
		public IILVisitor<APC, Expression, Variable, bool, bool> GetVisitor ()
		{
			return this;
		}

		public virtual bool Join (Pair<APC, APC> edge, bool newstate, bool prevstate, out bool weaker, bool widen)
		{
			weaker = false;
			return true;
		}

		public bool ImmutableVersion (bool arg)
		{
			return arg;
		}

		public bool MutableVersion (bool arg)
		{
			return arg;
		}

		public bool EdgeConversion (APC @from, APC to, bool isJoinPoint, EdgeData data, bool state)
		{
			return state;
		}

		public bool IsBottom (APC pc, bool state)
		{
			return !state;
		}

		public Predicate<APC> SaveFixPointInfo (IFixPointInfo<APC, bool> fixPointInfo)
		{
			return null;
		}

		public void Dump (Pair<bool, TextWriter> pair)
		{
		}
		#endregion
	}
}
