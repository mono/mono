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

	public class Const : MemberCore {
		public Expression ConstantType;
		public Expression Expr;
		public Attributes  OptAttributes;
		public FieldBuilder FieldBuilder;
		EmitContext const_ec;

		object ConstantValue = null;
		Type type;

		bool in_transit = false;

		public const int AllowedModifiers =
			Modifiers.NEW |
			Modifiers.PUBLIC |
			Modifiers.PROTECTED |
			Modifiers.INTERNAL |
			Modifiers.PRIVATE;

		public Const (Expression constant_type, string name, Expression expr, int mod_flags,
			      Attributes attrs, Location loc)
			: base (name, loc)
		{
			ConstantType = constant_type;
			Name = name;
			Expr = expr;
			ModFlags = Modifiers.Check (AllowedModifiers, mod_flags, Modifiers.PRIVATE, loc);
			OptAttributes = attrs;
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
		public override bool Define (TypeContainer parent)
		{
			type = parent.ResolveType (ConstantType, false, Location);

			if (type == null)
				return false;

			const_ec = new EmitContext (parent, Location, null, type, ModFlags);
			
			if (!TypeManager.IsBuiltinType (type) &&
			    (!type.IsSubclassOf (TypeManager.enum_type))) {
				Report.Error (
					-3, Location,
					"Constant type is not valid (only system types are allowed)");
				return false;
			}

			Type ptype = parent.TypeBuilder.BaseType;

			if (ptype != null) {
				MemberList list = TypeContainer.FindMembers (
					ptype, MemberTypes.Field, BindingFlags.Public,
					Type.FilterName, Name);
				
				if (list.Count == 0)
					if ((ModFlags & Modifiers.NEW) != 0)
						WarningNotHiding (parent);

			} else if ((ModFlags & Modifiers.NEW) != 0)
				WarningNotHiding (parent);

			FieldBuilder = parent.TypeBuilder.DefineField (Name, type, FieldAttr);

			TypeManager.RegisterConstant (FieldBuilder, this);

			return true;
		}

		/// <summary>
		///  Looks up the value of a constant field. Defines it if it hasn't
		///  already been. Similar to LookupEnumValue in spirit.
		/// </summary>
		public object LookupConstantValue ()
		{
			if (ConstantValue != null)
				return ConstantValue;

			if (in_transit) {
				Report.Error (110, Location,
					      "The evaluation of the constant value for `" +
					      Name + "' involves a circular definition.");
				return null;
			}

			in_transit = true;
			int errors = Report.Errors;

			Expr = Expr.Resolve (const_ec);

			in_transit = false;

			if (Expr == null) {
				if (errors == Report.Errors)
					Report.Error (150, Location, "A constant value is expected");
				return null;
			}

			if (!(Expr is Constant)) {
				UnCheckedExpr un_expr = Expr as UnCheckedExpr;
				CheckedExpr ch_expr = Expr as CheckedExpr;

				if ((un_expr != null) && (un_expr.Expr is Constant))
					Expr = un_expr.Expr;
				else if ((ch_expr != null) && (ch_expr.Expr is Constant))
					Expr = ch_expr.Expr;
				else {
					if (errors == Report.Errors)
						Report.Error (150, Location, "A constant value is expected");
					return null;
				}
			}

			ConstantValue = ((Constant) Expr).GetValue ();

			if (type != Expr.Type) {
				bool fail;

				// from the null type to any reference-type.
				if (Expr is NullLiteral && !type.IsValueType &&
				    !TypeManager.IsEnumType (type)){
					return NullLiteral.Null;
				}
				
				ConstantValue = TypeManager.ChangeType (ConstantValue, type, out fail);
				if (fail){
					Expression.Error_CannotConvertImplicit (Location, Expr.Type, type);
					return null;
				}

				if (type == TypeManager.int32_type)
					Expr = new IntConstant ((int) ConstantValue);
				else if (type == TypeManager.uint32_type)
					Expr = new UIntConstant ((uint) ConstantValue);
				else if (type == TypeManager.int64_type)
					Expr = new LongConstant ((long) ConstantValue);
				else if (type == TypeManager.uint64_type)
					Expr = new ULongConstant ((ulong) ConstantValue);
				else if (type == TypeManager.float_type)
					Expr = new FloatConstant ((float) ConstantValue);
				else if (type == TypeManager.double_type)
					Expr = new DoubleConstant ((double) ConstantValue);
				else if (type == TypeManager.string_type)
					Expr = new StringConstant ((string) ConstantValue);
				else if (type == TypeManager.short_type)
					Expr = new ShortConstant ((short) ConstantValue);
				else if (type == TypeManager.ushort_type)
					Expr = new UShortConstant ((ushort) ConstantValue);
				else if (type == TypeManager.sbyte_type)
					Expr = new SByteConstant ((sbyte) ConstantValue);
				else if (type == TypeManager.byte_type)
					Expr = new ByteConstant ((byte) ConstantValue);
				else if (type == TypeManager.char_type)
					Expr = new CharConstant ((char) ConstantValue);
				else if (type == TypeManager.bool_type)
					Expr = new BoolConstant ((bool) ConstantValue);
			}

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
				return null;

			return ConstantValue;
		}
		
		
		/// <summary>
		///  Emits the field value by evaluating the expression
		/// </summary>
		public void EmitConstant (TypeContainer parent)
		{
			LookupConstantValue ();
			
			return;
		}
	}
}


