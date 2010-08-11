using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.CodeContracts.Rewrite.AstVisitors;
using System.IO;

namespace Mono.CodeContracts.Rewrite {
	class ConditionTextExtractor {

		public ConditionTextExtractor (string filename, SourcePositionVisitor.CodePosition start, SourcePositionVisitor.CodePosition end)
		{
			this.filename = filename;
			this.start = start;
			this.end = end;
		}

		private string filename;
		private SourcePositionVisitor.CodePosition start, end;

		enum State {
			FunctionName,
			Parameters,
		};

		private State state;

		public string GetConditionText ()
		{
			if (this.filename == null || this.start.IsEmpty || this.end.IsEmpty){
				return "<unknown source code position>";
			}
			string[] lines;
			try {
				lines = File.ReadAllLines (this.filename);
			} catch {
				return "<cannot access source code>";
			}
			try {
				StringBuilder sb = new StringBuilder ();
				for (int i = this.start.Line; i <= this.end.Line; i++) {
					string line = lines [i - 1];
					if (i == this.end.Line && this.end.Column != 0) {
						line = line.Substring (0, this.end.Column - 1);
					}
					if (i == this.start.Line && this.start.Column != 0) {
						line = line.Substring (this.start.Column - 1);
					}
					sb.Append (line.Trim());
				}
				string cndStr = sb.ToString ();
	
				this.state = State.FunctionName;
	
				var cnd = this.RunStateMachine (cndStr);
	
				return cnd.ToString ().Trim ();
			} catch {
				return "<source-code parse error>";
			}
		}

		private StringBuilder RunStateMachine (string line)
		{
			StringBuilder cnd = new StringBuilder ();
			int inBrackets = 0;
			bool inDoubleQuotes = false;
			bool inSingleQuotes = false;
			bool inEscape = false;
			foreach (char c in line) {
				switch (this.state) {
				case State.FunctionName:
					if (c == '(') {
						this.state = State.Parameters;
					}
					break;
				case State.Parameters:
					switch (c) {
					case ',':
						if (inBrackets == 0 && !inDoubleQuotes && !inSingleQuotes) {
							return cnd;
						}
						break;
					case '(':
						if (!inDoubleQuotes && !inSingleQuotes) {
							inBrackets++;
						}
						break;
					case ')':
						if (!inDoubleQuotes && !inSingleQuotes) {
							if (inBrackets == 0) {
								return cnd;
							}
							inBrackets--;
						}
						break;
					case '"':
						if (!inEscape) {
							inDoubleQuotes = !inDoubleQuotes;
						}
						break;
					case '\'':
						if (!inEscape) {
							inSingleQuotes = !inSingleQuotes;
						}
						break;
					case '\\':
						inEscape = true;
						goto forceEscape;
					}
					inEscape = false;
				forceEscape:
					cnd.Append (c);
					break;
				default:
					throw new NotSupportedException ("Cannot handle state: " + this.state);
				}
			}

			return new StringBuilder ("<bad source code>");
		}

	}
}
