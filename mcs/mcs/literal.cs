//
// literal.cs: Literal representation for the IL tree.
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) 2001 Ximian, Inc.
//

using System;
using System.Reflection;
using System.Reflection.Emit;

namespace CIR {
	public abstract class Literal : Expression {
		// <summary>
		//   This is different from ToString in that ToString
		//   is supposed to be there for debugging purposes,
		//   and is not guarantee to be useful for anything else,
		//   AsString() will provide something that can be used
		//   for round-tripping C# code.  Maybe it can be used
		//   for IL assembly as well.
		// </summary>
		public abstract string AsString ();

		override public string ToString ()
		{
			return AsString ();
		}

		static public string descape (char c)
		{
			switch (c){
			case '\a':
				return "\\a"; 
			case '\b':
				return "\\b"; 
			case '\n':
				return "\\n"; 
			case '\t':
				return "\\t"; 
			case '\v':
				return "\\v"; 
			case '\r':
				return "\\r"; 
			case '\\':
				return "\\\\";
			case '\f':
				return "\\f"; 
			case '\0':
				return "\\0"; 
			case '"':
				return "\\\""; 
			case '\'':
				return "\\\'"; 
			}
			return c.ToString ();
		}
	}

	public class NullLiteral : Literal {
		public NullLiteral ()
		{
		}
		
		override public string AsString ()
		{
			return "null";
		}

		public override Expression Resolve (TypeContainer tc)
		{
			eclass = ExprClass.Value;
			type = TypeManager.object_type;

			return this;
		}

		public override void Emit (EmitContext ec)
		{
		}
	}

	public class BoolLiteral : Literal {
		static Type bool_type = Type.GetType ("System.Bool");
		bool val;
		
		public BoolLiteral (bool val)
		{
			this.val = val;
		}

		override public string AsString ()
		{
			return val ? "true" : "false";
		}

		public override Expression Resolve (TypeContainer tc)
		{
			eclass = ExprClass.Value;
			type = bool_type;

			return this;
		}

		public override void Emit (EmitContext ec)
		{
		}
	}

	public class CharLiteral : Literal {
		char c;
		
		public CharLiteral (char c)
		{
			this.c = c;
		}

		override public string AsString ()
		{
			return "\"" + descape (c) + "\"";
		}

		public override Expression Resolve (TypeContainer tc)
		{
			eclass = ExprClass.Value;
			type = TypeManager.char_type;

			return this;
		}

		public override void Emit (EmitContext ec)
		{
		}
	}

	public class IntLiteral : Literal {
		int i;

		public IntLiteral (int l)
		{
			i = l;
		}

		override public string AsString ()
		{
			return i.ToString ();
		}

		public override Expression Resolve (TypeContainer tc)
		{
			eclass = ExprClass.Value;
			type = TypeManager.int32_type;

			return this;
		}

		public override void Emit (EmitContext ec)
		{
		}
	}

	public class FloatLiteral : Literal {
		float f;

		public FloatLiteral (float f)
		{
			this.f = f;
		}

		override public string AsString ()
		{
			return f.ToString ();
		}

		public override Expression Resolve (TypeContainer tc)
		{
			eclass = ExprClass.Value;
			type = TypeManager.float_type;

			return this;
		}

		public override void Emit (EmitContext ec)
		{
		}
	}

	public class DoubleLiteral : Literal {
		double d;

		public DoubleLiteral (double d)
		{
			this.d = d;
		}

		override public string AsString ()
		{
			return d.ToString ();
		}

		public override Expression Resolve (TypeContainer tc)
		{
			eclass = ExprClass.Value;
			type = TypeManager.double_type;

			return this;
		}

		public override void Emit (EmitContext ec)
		{
		}
	}

	public class DecimalLiteral : Literal {
		decimal d;

		public DecimalLiteral (decimal d)
		{
			this.d = d;
		}

		override public string AsString ()
		{
			return d.ToString ();
		}

		public override Expression Resolve (TypeContainer tc)
		{
			eclass = ExprClass.Value;
			type = TypeManager.decimal_type;

			return this;
		}

		public override void Emit (EmitContext ec)
		{
		}
	}

	public class StringLiteral : Literal {
		string s;

		public StringLiteral (string s)
		{
			this.s = s;
		}

		// FIXME: Escape the string.
		override public string AsString ()
		{
			return "\"" + s + "\"";
		}

		public override Expression Resolve (TypeContainer tc)
		{
			eclass = ExprClass.Value;
			type = TypeManager.string_type;

			return this;
		}

		public override void Emit (EmitContext ec)
		{
			ec.ig.Emit (OpCodes.Ldstr, s);
		}
	}
}
