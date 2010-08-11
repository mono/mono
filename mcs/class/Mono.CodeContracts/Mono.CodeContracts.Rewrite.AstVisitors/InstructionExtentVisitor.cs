using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.CodeContracts.Rewrite.Ast;
using Mono.Cecil.Cil;

namespace Mono.CodeContracts.Rewrite.AstVisitors {
	class InstructionExtentVisitor : ExprVisitor {

		public InstructionExtentVisitor (Dictionary<Expr, Instruction> instructionLookup)
		{
			this.instructionLookup = instructionLookup;
		}

		private Dictionary<Expr, Instruction> instructionLookup;
		private List<Instruction> instructions = new List<Instruction> ();

		public IEnumerable<Instruction> Instructions
		{
			get
			{
				return this.instructions.OrderBy (x => x.Offset);
			}
		}

		public override Expr Visit (Expr e)
		{
			Instruction inst;
			if (this.instructionLookup.TryGetValue (e, out inst)) {
				this.instructions.Add (inst);
			}
			return base.Visit (e);
		}

	}
}
