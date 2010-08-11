using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.CodeContracts.Rewrite.Ast;
using Mono.Cecil.Cil;

namespace Mono.CodeContracts.Rewrite.AstVisitors {
	class SourcePositionVisitor : ExprVisitor {

		public struct CodePosition {

			public static readonly CodePosition Empty = new CodePosition ();

			public CodePosition (int line, int column)
				: this ()
			{
				this.Line = line;
				this.Column = column;
			}

			public int Line { get; private set; }
			public int Column { get; private set; }

			public bool IsEmpty
			{
				get
				{
					return this.Line == 0 && this.Column == 0;
				}
			}

			public static bool operator < (CodePosition a, CodePosition b)
			{
				if (a.Line < b.Line) {
					return true;
				}
				if (a.Line > b.Line) {
					return false;
				}
				return a.Column < b.Column;
			}

			public static bool operator > (CodePosition a, CodePosition b)
			{
				if (a.Line > b.Line) {
					return true;
				}
				if (a.Line < b.Line) {
					return false;
				}
				return a.Column > b.Column;
			}

		}

		public SourcePositionVisitor (Dictionary<Expr, Instruction> instructionLookup)
		{
			this.instructionLookup = instructionLookup;
			this.SourceCodeFileName = null;
			this.StartPosition = CodePosition.Empty;
			this.EndPosition = CodePosition.Empty;
		}

		private Dictionary<Expr, Instruction> instructionLookup;

		public string SourceCodeFileName { get; private set; }
		public CodePosition StartPosition { get; private set; }
		public CodePosition EndPosition { get; private set; }

		public override Expr Visit (Expr e)
		{
			Instruction inst;
			if (this.instructionLookup.TryGetValue (e, out inst)) {
				var seq = inst.SequencePoint;
				if (seq != null) {
					this.SourceCodeFileName = seq.Document.Url;
					var instStart = new CodePosition(seq.StartLine, seq.StartColumn);
					if (this.StartPosition.IsEmpty || instStart < this.StartPosition) {
						this.StartPosition = instStart;
					}
					var instEnd = new CodePosition (seq.EndLine, seq.EndColumn);
					if (this.EndPosition.IsEmpty || instEnd > this.EndPosition) {
						this.EndPosition = instEnd;
					}
				}
			}
			return base.Visit (e);
		}

	}
}
