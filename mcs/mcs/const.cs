//
// const.cs: Constant declarations.
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) 2001 Ximian, Inc.
//
//

//
// This is needed because the following situation arises:
//
//     The FieldBuilder is declared with the real type for an enumeration
//
//     When we attempt to set the value for the constant, the FieldBuilder.SetConstant
//     function aborts because it requires its argument to be of the same type
//

namespace Mono.CSharp {

	using System;
	using System.Reflection;
	using System.Reflection.Emit;
	using System.Collections;

	public class Const : FieldBase {
		public Expression Expr;
		EmitContext const_ec;

		bool resolved = false;
		object ConstantValue = null;
		Type type;

		bool in_transit = false;

		public const int AllowedModifiers =
			Modifiers.NEW |
			Modifiers.PUBLIC |
			Modifiers.PROTECTED |
			Modifiers.INTERNAL |
			Modifiers.PRIVATE;

		public Const (TypeContainer parent, Expression constant_type, string name,
			      Expression expr, int mod_flags, Attributes attrs, Location loc)
			: base (parent, constant_type, mod_flags, AllowedModifiers, name,
				null, attrs, loc)
		{
			Expr = expr;
		}

		public FieldAttributes FieldAttr {
			get {
				return FieldAttributes.Literal | FieldAttributes.Static |
					Modifiers.FieldAttr (ModFlags) ;
			}
		}

#if DEBUG
		void dump_tree (Type t)
		{
			Console.WriteLine ("Dumping hierarchy");
			while (t != null){
				Console.WriteLine ("   " + t.FullName + " " +
					(t.GetType ().IsEnum ? "yes" : "no"));
				t = t.BaseType;
			}
		}
#endif

		/// <summary>
		///   Defines the constant in the @parent
		/// </summary>
		public override bool Define ()
		{
			type = Parent.ResolveType (Type, false, Location);

			if (type == null)
				return false;

			const_ec = new EmitContext (Parent, Location, null, type, ModFlags);

			Type ttype = type;
			while (ttype.IsArray)
			    ttype = TypeManager.GetElementType (ttype);
			
			if (!TypeManager.IsBuiltinType (ttype) &&
			    (!ttype.IsSubclassOf (TypeManager.enum_type))) {
				Report.Error (
					-3, Location,
					"Constant type is not valid (only system types are allowed)");
				return false;
			}

			if (!CheckBase ())
				return false;

			FieldBuilder = Parent.TypeBuilder.DefineField (Name, type, FieldAttr);

			TypeManager.RegisterConstant (FieldBuilder, this);

			return true;
		}

		//
		// Changes the type of the constant expression `expr' to the Type `type'
		// Returns null on failure.
		//
		public static Constant ChangeType (Location loc, Constant expr, Type type)
		{
			if (type == TypeManager.object_type)
				return expr;

			bool fail;

			// from the null type to any reference-type.
			if (expr is NullLiteral && !type.IsValueType && !TypeManager.IsEnumType (type))
				return NullLiteral.Null;

			if (!Convert.ImplicitStandardConversionExists (expr, type)){
				Convert.Error_CannotImplicitConversion (loc, expr.Type, type);
				return null;
			}
			
			object constant_value = TypeManager.ChangeType (expr.GetValue (), type, out fail);
			if (fail){
				Convert.Error_CannotImplicitConversion (loc, expr.Type, type);
				
				//
				// We should always catch the error before this is ever
				// reached, by calling Convert.ImplicitStandardConversionExists
				//
				throw new Exception (
						     String.Format ("LookupConstantValue: This should never be reached {0} {1}", expr.Type, type));
			}

			Constant retval;
			if (type == TypeManager.int32_type)
				retval = new IntConstant ((int) constant_value);
			else if (type == TypeManager.uint32_type)
				retval = new UIntConstant ((uint) constant_value);
			else if (type == TypeManager.int64_type)
				retval = new LongConstant ((long) constant_value);
			else if (type == TypeManager.uint64_type)
				retval = new ULongConstant ((ulong) constant_value);
			else if (type == TypeManager.float_type)
				retval = new FloatConstant ((float) constant_value);
			else if (type == TypeManager.double_type)
				retval = new DoubleConstant ((double) constant_value);
			else if (type == TypeManager.string_type)
				retval = new StringConstant ((string) constant_value);
			else if (type == TypeManager.short_type)
				retval = new ShortConstant ((short) constant_value);
			else if (type == TypeManager.ushort_type)
				retval = new UShortConstant ((ushort) constant_value);
			else if (type == TypeManager.sbyte_type)
				retval = new SByteConstant ((sbyte) constant_value);
			else if (type == TypeManager.byte_type)
				retval = new ByteConstant ((byte) constant_value);
			else if (type == TypeManager.char_type)
				retval = new CharConstant ((char) constant_value);
			else if (type == TypeManager.bool_type)
				retval = new BoolConstant ((bool) constant_value);
			else
				throw new Exception ("LookupConstantValue: Unhandled constant type: " + type);
			
			return retval;
		}
		
		/// <summary>
		///  Looks up the value of a constant field. Defines it if it hasn't
		///  already been. Similar to LookupEnumValue in spirit.
		/// </summary>
		public bool LookupConstantValue (out object value)
		{
			if (resolved) {
				value = ConstantValue;
				return true;
			}

			if (in_transit) {
				Report.Error (110, Location,
					      "The evaluation of the constant value for `" +
					      Name + "' involves a circular definition.");
				value = null;
				return false;
			}

			in_transit = true;
			int errors = Report.Errors;

			//
			// We might have cleared Expr ourselves in a recursive definition
			//
			if (Expr == null){
				value = null;
				return false;
			}

			Expr = Expr.Resolve (const_ec);

			in_transit = false;

			if (Expr == null) {
				if (errors == Report.Errors)
					Report.Error (150, Location, "A constant value is expected");
				value = null;
				return false;
			}

			Expression real_expr = Expr;

			Constant ce = Expr as Constant;
			if (ce == null){
				UnCheckedExpr un_expr = Expr as UnCheckedExpr;
				CheckedExpr ch_expr = Expr as CheckedExpr;
				EmptyCast ec_expr = Expr as EmptyCast;

				if ((un_expr != null) && (un_expr.Expr is Constant))
					Expr = un_expr.Expr;
				else if ((ch_expr != null) && (ch_expr.Expr is Constant))
					Expr = ch_expr.Expr;
				else if ((ec_expr != null) && (ec_expr.Child is Constant))
					Expr = ec_expr.Child;
				else if (Expr is ArrayCreation){
					Report.Error (133, Location, "Arrays can not be constant");
				} else {
					if (errors == Report.Errors)
						Report.Error (150, Location, "A constant value is expected");
					value = null;
					return false;
				}

				ce = Expr as Constant;
			}

			if (type != real_expr.Type) {
				ce = ChangeType (Location, ce, type);
				if (ce == null){
					value = null;
					return false;
				}
				Expr = ce;
			}
			ConstantValue = ce.GetValue ();

			if (type.IsEnum){
				//
				// This sadly does not work for our user-defined enumerations types ;-(
				//
				try {
					ConstantValue = System.Enum.ToObject (
						type, ConstantValue);
				} catch (ArgumentException){
					Report.Error (
						-16, Location,
						".NET SDK 1.0 does not permit to create the constant "+
						" field from a user-defined enumeration");
				}
			}

			FieldBuilder.SetConstant (ConstantValue);

			if (!TypeManager.RegisterFieldValue (FieldBuilder, ConstantValue))
				throw new Exception ("Cannot register const value");

			value = ConstantValue;
			resolved = true;
			return true;
		}
		
		
		/// <summary>
		///  Emits the field value by evaluating the expression
		/// </summary>
		public override void Emit ()
		{
			object value;
			LookupConstantValue (out value);

			if (OptAttributes != null) {
				OptAttributes.Emit (const_ec, this);
			}

			base.Emit ();
		}
	}
}


