using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.CodeContracts.Rewrite;
using Mono.CodeContracts.Rewrite.Ast;
using System.Diagnostics.Contracts;
using Mono.Cecil;
using Mono.CodeContracts.Rewrite.AstVisitors;
using Mono.Cecil.Cil;

namespace Mono.CodeContracts.Rewrite {
	class TransformContractsVisitor : ExprVisitor {

		public TransformContractsVisitor (MethodDefinition method, Dictionary<Expr, Instruction> instructionLookup, ContractsRuntime contractsRuntime)
		{
			this.module = method.Module;
			this.instructionLookup = instructionLookup;
			this.contractsRuntime = contractsRuntime;
			this.methodInfo = new MethodInfo (method);
		}

		private ModuleDefinition module;
		private Dictionary<Expr, Instruction> instructionLookup;
		private ContractsRuntime contractsRuntime;
		private MethodInfo methodInfo;

		private List<ContractRequiresInfo> contractRequiresInfo = new List<ContractRequiresInfo> ();

		public IEnumerable<ContractRequiresInfo> ContractRequiresInfo
		{
			get { return this.contractRequiresInfo; }
		}

		protected override Expr VisitCall (ExprCall e)
		{
			var call = (ExprCall)base.VisitCall (e);

			var method = e.Method;
			if (method.DeclaringType.FullName == "System.Diagnostics.Contracts.Contract") {
				switch (method.Name) {
				case "Requires":
					if (!method.HasGenericParameters) {
						switch (method.Parameters.Count) {
						case 1:
							return this.ProcessRequires1 (call);
						case 2:
							return this.ProcessRequires2 (call);
						default:
							throw new NotSupportedException ("Invalid number of parameters to Contract.Requires()");
						}
					} else {
						goto default;
					}
				default:
					throw new NotSupportedException ("Cannot handle Contract." + e.Method.Name + "()");
				}
			}

			return call;
		}

		private string GetConditionString (Expr e)
		{
			var vSource = new SourcePositionVisitor (this.instructionLookup);
			vSource.Visit (e);
			var extractor = new ConditionTextExtractor (vSource.SourceCodeFileName, vSource.StartPosition, vSource.EndPosition);
			return extractor.GetConditionText ();
		}

		private Expr ProcessRequires1 (ExprCall e)
		{
			MethodDefinition mRequires = this.contractsRuntime.GetRequires ();
			Expr conditionExpr = e.Parameters.First ();
			Expr nullArgExpr = new ExprLoadConstant (this.methodInfo, null);
			string conditionText = this.GetConditionString (e);
			Expr conditionStringExpr = new ExprLoadConstant (this.methodInfo, conditionText);
			var call = new ExprCall (this.methodInfo, mRequires, new Expr [] { conditionExpr, nullArgExpr, conditionStringExpr });

			this.contractRequiresInfo.Add (new ContractRequiresInfo (e, call));

			return call;
		}

		private Expr ProcessRequires2 (ExprCall e)
		{
			MethodDefinition mRequires = this.contractsRuntime.GetRequires ();
			Expr conditionExpr = e.Parameters.First ();
			Expr msgExpr = e.Parameters.ElementAt (1);
			string conditionText = this.GetConditionString (e);
			Expr conditionStringExpr = new ExprLoadConstant (this.methodInfo, conditionText);
			var call = new ExprCall (this.methodInfo, mRequires, new Expr [] { conditionExpr, msgExpr, conditionStringExpr });

			this.contractRequiresInfo.Add (new ContractRequiresInfo (e, call));

			return call;
		}

	}
}
