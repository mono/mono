// cs-treedump.cs: Dumps the parsed tree to standard output
//
// Author: Miguel de Icaza (miguel@gnu.org)
//
// Licensed under the terms of the GNU GPL
//
// (C) 2001 Ximian, Inc. (http://www.ximian.com)
//
// TODO:
//	Variable declarations
//      Fix precedence rules to lower the number of parenthesis.
//

using System;
using System.IO;
using System.Collections;
using CIR;

namespace Generator {
	
	public class TreeDump : CIR.ITreeDump {
		StreamWriter o;
		int indent;
		bool indented;
		bool tag_values;
		
		void space ()
		{
			if (!indented)
				output (new String (' ', indent * 8));
			indented = true;
		}

		void output (string s)
		{
			Console.Write (s);
			//o.Write (s);
		}

		void newline ()
		{
			output ("\n");
			indented = false;
		}

		void output_newline (string s)
		{
			output (s);
			newline ();
		}
		
		void ioutput (string s)
		{
			space ();
			output (s);
		}

		string GetParameter (Parameter par)
		{
			Parameter.Modifier f = par.ModFlags;
			string mod = "";
			
			switch (f){
			case Parameter.Modifier.REF:
				mod = "ref "; break;
			case Parameter.Modifier.OUT:
				mod = "out "; break;
			case Parameter.Modifier.PARAMS:
				mod = "params "; break;
			case Parameter.Modifier.NONE:
				mod = ""; break;
			}
			return mod + par.Type + " " + par.Name;
		}

		string GetUnary (Unary u, int paren_level)
		{
			string e;
			bool left = true;
			string s = "0_ERROR>";
			int prec = 0;
			
			switch (u.Oper){
			case Unary.Operator.UnaryPlus:
				prec = 10;
				s = "+";
				break;
				
			case Unary.Operator.UnaryNegation:
				prec = 10;
				s = "-";
				break;
				
			case Unary.Operator.LogicalNot:
				s = "!";
				prec = 10;
				break;
				
			case Unary.Operator.OnesComplement:
				prec = 10;
				s = "~";
				break;
				
			case Unary.Operator.Indirection:
				prec = 10;
				s = "*";
				break;
				
			case Unary.Operator.AddressOf:
				prec = 10;
				s = "&";
				break;
				
			case Unary.Operator.PreIncrement:
				prec = 11;
				s = "++";
				break;
				
			case Unary.Operator.PreDecrement:
				prec = 11;
				s = "--";
				break;
				
			case Unary.Operator.PostDecrement:
				left = false;
				prec = 12;
				s = "--";
				break;
				
			case Unary.Operator.PostIncrement:
				s = "++";
				prec = 12;
				left = false;
				break;
			}

			e = GetExpression (u.Expr, prec);
			if (left)
				e = s + e;
			else
				e = e + s;
			
			if (prec < paren_level)
				return "(" + e + ")";
			else
				return e;
		}

		string GetBinary (Binary b, int paren_level)
		{
			string l, r;
			string op = null;
			bool assoc_left = true;
			int prec = 0;
			
			switch (b.Oper){
				case Binary.Operator.Multiply:
					prec = 9;
					op = "*"; break;

			        case Binary.Operator.Division:
					prec = 9;
					op = "/"; break;
				
				case Binary.Operator.Modulus:
					prec = 9;
					op = "%"; break;
				
				case Binary.Operator.Addition:
					prec = 8;
					op = "+"; break;
				
				case Binary.Operator.Subtraction:
					prec = 8;
					op = "-"; break;
				
				case Binary.Operator.LeftShift:
					prec = 7;
					op = "<<"; break;
				
				case Binary.Operator.RightShift:
					prec = 7;
					op = ">>"; break;
				
				case Binary.Operator.LessThan:
					prec = 6;
					op = "<"; break;
				
				case Binary.Operator.GreaterThan:
					prec = 6;
					op = ">"; break;
				
				case Binary.Operator.LessThanOrEqual:
					prec = 6;
					op = "<="; break;
				
				case Binary.Operator.GreaterThanOrEqual:
					prec = 6;
					op = ">="; break;
				
				case Binary.Operator.Equality:
					prec = 5;
					op = "=="; break;
				
				case Binary.Operator.Inequality:
					prec = 5;
					op = "!="; break;
				
				case Binary.Operator.BitwiseAnd:
					prec = 4;
					op = "&"; break;
				
				case Binary.Operator.BitwiseOr:
					prec = 2;
					op = "|"; break;
				
				case Binary.Operator.LogicalAnd:
					prec = 1;
					op = "&&"; break;
				
				case Binary.Operator.LogicalOr:
					prec = 0;
					op = "||"; break;
				
				case Binary.Operator.ExclusiveOr:
					prec = 3;
					op = "^"; break;
			}

			l = GetExpression (b.Left, prec - (assoc_left ? 0 : 1));
			r = GetExpression (b.Right, prec - (assoc_left ? 0 : 1));
			
			if (prec <= paren_level)
				return "(" + l + " " + op + " " + r + ")";
			else
				return l + " " + op + " " + r;
		}

		string GetCast (Cast c)
		{
			return "(" + c.TargetType + ") (" + GetExpression (c.Expr, 0) + ")";
		}

		string GetConditional (Conditional c)
		{
			return "(" + GetExpression (c.Expr, 0) + ") ? (" +
				GetExpression (c.TrueExpr, 0) + ") : (" +
				GetExpression (c.FalseExpr, 0) + ")";
		}

		string GetAssign (Assign a)
		{
			return GetExpression (a.Target, 0) + " = " + GetExpression (a.Source, 0);
		}

		string GetArguments (ArrayList args)
		{
			string r = "";
			
			if (args != null){
				int top = args.Count;
				
				for (int i = 0; i < top; i++){
					Argument arg = (Argument) args [i];
						
					switch (arg.ArgType){
						case Argument.AType.Ref:
							r += "ref "; break;
						case Argument.AType.Out:
							r += "out "; break;
					}
					r += GetExpression (arg.Expr, 0);

					if (i+1 != top)
						r += ", ";
				}
			}

			return "(" + r + ")";
		}
		
		string GetInvocation (Invocation i)
		{
			return GetExpression (i.Expr, 0) + " " + GetArguments (i.Arguments);
		}

		string GetNew (New n)
		{
			return "new " + n.RequestedType + GetArguments (n.Arguments);
		}

		string GetTypeOf (TypeOf t)
		{
			return "typeof (" + t.QueriedType + ")";
		}

		string GetSizeOf (SizeOf t)
		{
			return "sizeof (" + t.QueriedType + ")";
		}

		string GetMemberAccess (MemberAccess m)
		{
			return GetExpression (m.Expr, 0) +
				(tag_values ? "/* member access */ . " : ".") +
				m.Identifier;
		}

		string GetSimpleName (SimpleName n)
		{
			string s = n.Name;
			
			if (s.StartsWith ("0_"))
				return "id_" + s;
			else
				return s;
		}

		string GetProbe (Probe p)
		{
			string s = GetExpression (p.Expr, 6);

			if (p.Oper == CIR.Probe.Operator.Is)
				s += " is ";
			else if (p.Oper == CIR.Probe.Operator.As)
				s += " as ";
			else
				s += "UNHANDLED";

			s += p.ProbeType;

			return s;
		}

		string GetLocalVariableReference (LocalVariableReference l)
		{
			if (tag_values)
				return "/* local var: */" + l.Name;
			else
				return l.Name;
		}

		string GetParameterReference (ParameterReference r)
		{
			if (tag_values)
				return "/* par: */ " + r.Name;
			else
				return r.Name;
		}
		
		string GetExpression (Expression e, int paren_level)
		{
			if (e == null){
				return "<NULL EXPRESSION>";
			}
			
			if (e is Unary)
				return GetUnary ((Unary) e, paren_level);
			else if (e is Binary)
				return GetBinary ((Binary) e, paren_level);
			else if (e is Cast)
				return GetCast ((Cast) e);
			else if (e is Conditional)
				return GetConditional ((Conditional) e);
			else if (e is SimpleName)
				return GetSimpleName ((SimpleName)e);
			else if (e is LocalVariableReference)
				return GetLocalVariableReference ((LocalVariableReference) e);
			else if (e is ParameterReference)
				return GetParameterReference ((ParameterReference) e);
			else if (e is Assign)
				return GetAssign ((Assign) e);
			else if (e is Literal)
				return e.ToString ();
			else if (e is Invocation)
				return GetInvocation ((Invocation) e);
			else if (e is New)
				return GetNew ((New) e);
			else if (e is This)
				return "this";
			else if (e is TypeOf)
				return GetTypeOf ((TypeOf) e);
			else if (e is SizeOf)
				return GetSizeOf ((SizeOf) e);
			else if (e is MemberAccess)
				return GetMemberAccess ((MemberAccess) e);
			else if (e is Probe)
				return GetProbe ((Probe) e);
			else
				return "WARNING {" + e.ToString () + "} WARNING";
		}
		
		void GenerateParameters (Parameters pars)
		{
			Parameter [] pfixed;
			Parameter parray;

			pfixed = pars.FixedParameters;

			if (pfixed != null){
				for (int i = 0; i < pfixed.Length; i++){
					output (GetParameter (pfixed [i]));
					if (i+1 != pfixed.Length)
						output (", ");
				}
			}

			parray = pars.ArrayParameter;
			if (parray != null){
				output (GetParameter (parray));
			}
		}

		void GenerateIf (If s)
		{
			bool do_indent;
			
			output ("if (" + GetExpression (s.Expr, 0) + ") ");
			do_indent = !(s.TrueStatement is Block);
			if (do_indent)
				indent++;
			GenerateStatement (s.TrueStatement, true, false, false);
			if (do_indent)
				indent--;
			if (s.FalseStatement != null){
				ioutput ("else");
				newline ();
				GenerateStatement (s.FalseStatement, false, true, false);
			}
		}

		void GenerateDo (Do s)
		{
			output ("do"); newline ();
			indent++;
			GenerateStatement (s.EmbeddedStatement, false, false, false);
			indent--;
			output (" while (" + GetExpression (s.Expr, 0) + ");");
			newline ();
		}

		void GenerateWhile (While s)
		{
			output ("while (" + GetExpression (s.Expr, 0) + ")");
			GenerateStatement (s.Statement, true, true, false);
		}

		void GenerateFor (For s)
		{
			output ("for (");
			if (! (s.InitStatement == EmptyStatement.Value))
				GenerateStatement (s.InitStatement, true, true, true);
			output ("; ");
			output (GetExpression (s.Test, 0));
			output ("; ");
			if (! (s.Increment == EmptyStatement.Value))
				GenerateStatement (s.Increment, true, true, true);
			output (") ");
			GenerateStatement (s.Statement, true, true, false);
		}

		void GenerateReturn (Return s)
		{
			output ("return " +
				(s.Expr != null ?
				 GetExpression (s.Expr, 0) : "" + ";") +
				";");
			newline ();
		}

		void GenerateGoto (Goto s)
		{
			output ("goto " + s.Target + ";");
			newline ();
		}

		void GenerateThrow (Throw s)
		{
		}

		void GenerateStatementExpression (StatementExpression s)
		{
			output (GetExpression (s.Expr, 0) + ";");
			newline ();
		}

		void GenerateSwitchLabels (ArrayList labels)
		{
			foreach (SwitchLabel sl in labels){
				Expression lab = sl.Label;
				
				if (lab == null){
					ioutput ("default:");
					newline ();
				} else {
					ioutput ("case " + GetExpression (lab, 0) + ":");
					newline ();
				}
			}
		}
		
		void GenerateSwitch (Switch s)
		{
			output_newline ("switch (" + GetExpression (s.Expr, 0) + ")");
			foreach (SwitchSection ss in s.Sections){
				GenerateSwitchLabels (ss.Labels);
				GenerateBlock (ss.Block, false, false);
			}
		}
		
		void GenerateChecked (Checked c)
		{
			output ("checked ");
			GenerateBlock (c.Block, false, false);
		}

		void GenerateUnchecked (Unchecked c)
		{
			output ("unchecked ");
			GenerateBlock (c.Block, false, false);
		}

		void GenerateCatchClauses (ArrayList list)
		{
			foreach (Catch c in list){
				space ();
				output ("catch ");
				
				if (c.Type != null){
					output ("(" + c.Type +
						(c.Name != null ? " " + c.Name : "") + ")");
				} 
				GenerateBlock (c.Block, false, false);
			}
		}

		void GenerateTry (Try t)
		{
			output ("try");
			GenerateBlock (t.Block, false, false);

			if (t.Specific != null){
				GenerateCatchClauses (t.Specific);
			}

			if (t.General != null){
				space ();
				output ("catch");
				GenerateBlock (t.Block, false, false);
			}

			if (t.Fini != null){
				GenerateBlock (t.Fini, false, false);
			}
		}
		
		void GenerateStatement (Statement s, bool doPlacement, bool blockFlushesLine, bool embedded)
		{
			if (s == null){
				output ("WARNING: got a null Statement");
				newline ();
				return;
			}

			if (doPlacement){
				if (s is Block){
					GenerateBlock ((Block) s, doPlacement, embedded);
					return;
				} else 
					newline ();
			} 
				
			space ();
			if (s is If)
				GenerateIf ((If) s);
			else if (s is Do)
				GenerateDo ((Do) s);
			else if (s is While)
				GenerateWhile ((While) s);
			else if (s is For)
				GenerateFor ((For) s);
			else if (s is Return)
				GenerateReturn ((Return) s);
			else if (s is Goto)
				GenerateGoto ((Goto) s);
			else if (s is Throw)
				GenerateThrow ((Throw) s);
			else if (s is Break)
				output_newline ("break;");
			else if (s is Continue)
				output_newline ("continue;");
			else if (s == EmptyStatement.Value)
				output_newline ("/* empty statement */;");
			else if (s is Block)
				GenerateBlock ((Block) s, doPlacement, embedded);
			else if (s is StatementExpression)
				GenerateStatementExpression ((StatementExpression) s);
			else if (s is Switch)
				GenerateSwitch ((Switch) s);
			else if (s is Checked)
				GenerateChecked ((Checked) s);
			else if (s is Unchecked)
				GenerateUnchecked ((Unchecked) s);
			else if (s is Try)
				GenerateTry ((Try) s);
			else {
				System.Type t = s.GetType ();

				output ("\n*****UNKNOWN Statement:" + t.ToString ());
			}
		}

		//
		// embedded is used only for things like the For thing
		// that has blocks but for display purposes we want to keep
		// without newlines.
		void GenerateBlock (Block b, bool doPlacement, bool embedded)
		{
			if (b.Label != null)
				output (b.Label + ":");

			if (!b.Implicit){
				if (!doPlacement)
					space ();
			
				output ("{");
				if (!embedded)
					newline ();
				indent++;
			}

			if (b.Variables != null){
				foreach (DictionaryEntry entry in b.Variables){
					VariableInfo vi = (VariableInfo) entry.Value;
					
					space ();
					output_newline (
						vi.Type + " " +
						(string) entry.Key + ";");
				}
				newline ();
			}
			
			foreach (Statement s in b.Statements){
				GenerateStatement (s, false, true, false);
			}
			
			if (!b.Implicit){
				indent--;
				ioutput ("}");
				if (!embedded)
					newline ();
			}
		}
		
		void GenerateMethod (Method m)
		{
			ioutput (GetModifiers (m.ModFlags) +
				 m.ReturnType + " " +
				 m.Name + " (");
			GenerateParameters (m.Parameters);
			output_newline (")");
			

			GenerateBlock (m.Block, false, false);
			newline ();
		}

		void GenerateInterfaceMethod (InterfaceMethod imethod)
		{
			space ();
			output (imethod.IsNew ? "new " : "");
			output (imethod.ReturnType + " " + imethod.Name + " (");
			GenerateParameters (imethod.Parameters);
			output (");");
			newline ();
		}

		void GenerateInterfaceProperty (InterfaceProperty iprop)
		{
			space ();
			output (iprop.IsNew ? "new " : "");
			output (iprop.Type + " " + iprop.Name + " { ");
			if (iprop.HasGet) output ("get; ");
			if (iprop.HasSet) output ("set; ");
			output ("}");
			newline ();
		}
		
		void GenerateInterfaceEvent (InterfaceEvent ievent)
		{
			space ();
			output ((ievent.IsNew ? "new " : "") + "event ");
			output (ievent.Type + " " + ievent.Name + ";");
			newline ();
		}
		
		void GenerateInterfaceIndexer (InterfaceIndexer iindexer)
		{
			space ();
			output (iindexer.IsNew ? "new " : "");
			output (iindexer.Type + " this [");
			output (iindexer.Parameters + "] {");
			if (iindexer.HasGet) output ("get; ");
			if (iindexer.HasSet) output ("set; ");
			output ("}");
			newline ();
		}

		string GenIfaceBases (Interface iface)
		{
			return GenBases (iface.Bases);
		}
		
		void GenerateInterface (Interface iface)
		{
			ioutput (GetModifiers (iface.ModFlags) + "interface " +
				 ClassName (iface.Name) + GenIfaceBases (iface) + " {");
			newline ();
			indent++;
			
			if (iface.InterfaceMethods != null){
				foreach (DictionaryEntry de in iface.InterfaceMethods){
					InterfaceMethod method = (InterfaceMethod) de.Value;
					GenerateInterfaceMethod (method);
				}
			}

			if (iface.InterfaceProperties != null){
				foreach (DictionaryEntry de in iface.InterfaceProperties){
					InterfaceProperty iprop = (InterfaceProperty) de.Value;
					GenerateInterfaceProperty (iprop);
				}
			}

			if (iface.InterfaceEvents != null){
				foreach (DictionaryEntry de in iface.InterfaceEvents){
					InterfaceEvent ievent = (InterfaceEvent) de.Value;
					GenerateInterfaceEvent (ievent);
				}
			}

			if (iface.InterfaceIndexers != null){
				foreach (DictionaryEntry de in iface.InterfaceIndexers){
					InterfaceIndexer iindexer = (InterfaceIndexer) de.Value;
					GenerateInterfaceIndexer (iindexer);
				}
			}
			indent--;
			ioutput ("}");
			newline ();
			newline ();
		}

		void GenerateField (Field f)
		{
			space ();
			output (GetModifiers (f.ModFlags) + 
				f.Type + " " + f.Name);
			if (f.Initializer != null){
				if (f.Initializer is Expression)
					output (" = " + GetExpression ((Expression) f.Initializer, 0));
				else
					output ("ADD SUPPORT FOR ARRAYS");
			}
			output (";");
			newline ();
		}

		void GenerateConstructor (Constructor c)
		{
			ConstructorInitializer init = c.Initializer;

			space ();
			output (GetModifiers (c.ModFlags) + c.Name + " (");
			GenerateParameters (c.Parameters);
			output (")");

			if (init != null){
				if (init is ConstructorThisInitializer)
					output (": this (");
				else
					output (": base (");
				output (GetArguments (init.Arguments));
				output (")");
			}
			newline ();
			GenerateBlock (c.Block, false, false);
		}

		void GenerateProperty (Property prop)
		{
			space ();
			output (GetModifiers (prop.ModFlags) + prop.Type +
				" " + prop.Name + " {");
			newline ();
			indent++;
			if (prop.Get != null){
				space ();
				output ("get ");
				GenerateBlock (prop.Get, false, false);
				newline (); 
			}

			if (prop.Set != null){
				space ();
				output ("set ");
				GenerateBlock (prop.Set, false, false);
			}
			indent--;
			space ();
			output ("}");
			newline ();
		}

		void GenerateEnum (CIR.Enum e)
		{
			space ();
			output ("enum " + e.Name + " {");
			newline ();

			indent++;
			foreach (string name in e.ValueNames){
				Expression expr = e [name];

				space ();

				output (name);
				if (expr != null)
					output (" = " + GetExpression (expr, 0));

				output (",");
				newline ();
			}
			indent--;
			space ();
			output_newline ("}");
		}
		
		void GenerateTypeContainerData (TypeContainer tc)
		{
			if (tc.Constants != null){
				foreach (Constant c in tc.Constants){
					space ();

					output ("const " + c.ConstantType + " " + c.Name + " = " +
						GetExpression (c.Expr, 0) + ";");
					newline ();
				}
				newline ();
			}

			if (tc.Enums != null){
				foreach (CIR.Enum e in tc.Enums)
					GenerateEnum (e);
			}

			if (tc.Fields != null){
				foreach (Field f in tc.Fields)
					GenerateField (f);
				newline ();
			}

			if (tc.Constructors != null){
				foreach (Constructor c in tc.Constructors)
					GenerateConstructor (c);

				newline ();
					
			}

			if (tc.Properties != null){
				foreach (Property prop in tc.Properties)
					GenerateProperty (prop);
			}
			
			GenerateFromTypes (tc);
			
			if (tc.Methods != null){
				foreach (Method m in tc.Methods){
					GenerateMethod (m);
				}
			}
		}

		string GetModifiers (int mod_flags)
		{
			string s = "";

			for (int i = 1; i <= (int) CIR.Modifiers.TOP; i <<= 1){
				if ((mod_flags & i) != 0)
					s += Modifiers.Name (i) + " ";
			}

			return s;
		}

		string ClassName (string name)
		{
			return name;
			//return name.Substring (1 + name.LastIndexOf ('.'));
		}

		string GenBases (ArrayList bases)
		{
			if (bases == null)
				return "";

			string res = ": ";
			int top = bases.Count;
			for (int i = 0; i < bases.Count; i++){
				Type t = (Type) bases [i];

				res += t.Name;
				if (i + 1 != top)
					res += ", ";
			}
			return res;
		}
		
		string GenClassBases (Class c)
		{
			return GenBases (c.Bases);
		}
		
		void GenerateFromClass (Class c)
		{
			ioutput (GetModifiers (c.ModFlags) + "class " + ClassName (c.Name) +
				 " " + GenClassBases (c) + " {");
			newline ();
			indent++;
			
			GenerateTypeContainerData (c);

			indent--;
			ioutput ("}");
			newline ();
			newline ();
		}

		void GenerateFromStruct (Struct s)
		{
			GenerateTypeContainerData (s);
		}
		
		void GenerateFromTypes (TypeContainer types)
		{
			if (types.Types == null)
				return;
			
			foreach (DictionaryEntry de in types.Types){
				TypeContainer type = (TypeContainer) de.Value;
				
				if (type is Class)
					GenerateFromClass ((Class) type);
				if (type is Struct)
					GenerateFromStruct ((Struct) type);
					
			}

			if (types.Interfaces != null){
				foreach (DictionaryEntry de in types.Interfaces){
					Interface iface = (Interface) de.Value;

					GenerateInterface (iface);
				}
			}
		}
		
		public int Dump (Tree tree, StreamWriter output)
		{
			this.o = output;

			indent = 0;
			GenerateFromTypes (tree.Types);

			return 0;
		}

		public void ParseOptions (string options)
		{
			if (options == "tag")
				tag_values = true;
		}
	}
}


