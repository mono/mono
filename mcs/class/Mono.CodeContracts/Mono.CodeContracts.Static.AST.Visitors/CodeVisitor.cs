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
