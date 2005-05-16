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

using System;

namespace Mono.MonoBASIC {


	using System;
	using System.Reflection;
	using System.Reflection.Emit;
	using System.Collections;

	public class Const : MemberCore {
		public Expression ConstantType;
		public Expression Expr;
		public FieldBuilder FieldBuilder;


		bool resolved = false;
		object ConstantValue = null;
		Type type;

		bool in_transit = false;

		public const int AllowedModifiers =
			Modifiers.PUBLIC |
			Modifiers.PROTECTED |
			Modifiers.INTERNAL |
			Modifiers.PRIVATE |
			Modifiers.SHADOWS;

		public Const (Expression constant_type, string name, Expression expr, int mod_flags,
			      Attributes attrs, Location loc)
			: base (name, attrs, loc)
		{
			ConstantType = constant_type;
			Name = name;
			Expr = expr;
			ModFlags = Modifiers.Check (AllowedModifiers, mod_flags, Modifiers.PRIVATE, loc);
		}

		public override AttributeTargets AttributeTargets {
			get {
				return AttributeTargets.Field;
			}
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

			if (!TypeManager.IsBuiltinType (type) &&
			    (!type.IsSubclassOf (TypeManager.enum_type))) {
				Report.Error (
					30424, Location,
					"Constant type is not valid (only system types are allowed)");
				return false;
			}

			// if no type is declared expicitely 
			// set the expression type as the type of constant
			if (type == TypeManager.object_type){
				if (Expr is IntLiteral)
					type = TypeManager.int32_type;
				else if (Expr is UIntLiteral)
					type = TypeManager.uint32_type;
				else if (Expr is LongLiteral)
					type = TypeManager.int64_type;
				else if (Expr is ULongLiteral)
					type = TypeManager.uint64_type;
				else if (Expr is FloatLiteral)
					type = TypeManager.float_type;
				else if (Expr is DoubleLiteral)
					type = TypeManager.double_type;
				else if (Expr is StringLiteral)
					type = TypeManager.string_type;
				else if (Expr is ShortLiteral)
					type = TypeManager.short_type;
				else if (Expr is UShortConstant)
					type = TypeManager.ushort_type;
				else if (Expr is SByteConstant)
					type = TypeManager.sbyte_type;
				else if (Expr is ByteConstant)
					type = TypeManager.byte_type;
				else if (Expr is CharConstant)
					type = TypeManager.char_type;
				else if (Expr is BoolConstant)
					type = TypeManager.bool_type;
				else if (Expr is DateConstant)
					type = TypeManager.date_type;
			}
			Type ptype = parent.TypeBuilder.BaseType;

			if (ptype != null) {
				MemberList list = TypeContainer.FindMembers (
					ptype, MemberTypes.Field, BindingFlags.Public,
					Type.FilterName, Name);
				
				if ((list.Count > 0) && ((ModFlags & Modifiers.SHADOWS) == 0))
					Report.Warning (
						40004, 2, Location, 
						"Const '" + Name + "' should be declared " +
						"Shadows since the base type '" + ptype.Name + 
						"' has a Const with same name");
				if (list.Count == 0) {
					// if a member of module is not inherited from Object class
					// can not be declared protected
					if ((parent is Module) && ((ModFlags & Modifiers.PROTECTED) != 0))
						Report.Error (30593, Location,
							"'Const' inside a 'Module' can not be " +
							"declared as 'Protected'");

					/*if ((ModFlags & Modifiers.NEW) != 0)
						WarningNotHiding (parent);*/
				}
			} 
			/*else if ((ModFlags & Modifiers.NEW) != 0)
				WarningNotHiding (parent);*/

			if ((parent is Struct) && ((ModFlags & Modifiers.PROTECTED) != 0))
				Report.Error (30435, Location,
					"'Const' inside a 'Structure' can not be " +
					"declared as 'Protected'");

			FieldBuilder = parent.TypeBuilder.DefineField (Name, type, FieldAttr);

			TypeManager.RegisterConstant (FieldBuilder, this);

			return true;
		}

		//
                // Changes the type of the constant expression `expr' to the Type `type'
                // Returns null on failure.
                //
                public static Constant ChangeType (Location loc, Constant expr, Type type, EmitContext ec)
                {
                        if (type == TypeManager.object_type)
                                return expr;

                        bool fail;

                        // from the null type to any reference-type.
                        if (expr.Type == TypeManager.null_type && !type.IsValueType && !TypeManager.IsEnumType (type))
                                return NullLiteral.Null;
                        
			if (!Expression.ImplicitConversionExists (ec, expr, type)){
                                Expression.Error_CannotConvertImplicit (loc, expr.Type, type);
                                return null;
			  }

                        // Special-case: The 0 literal can be converted to an enum value,
                        // and ImplicitStandardConversionExists will return true in that case.
                        if (expr.Type == TypeManager.int32_type && TypeManager.IsEnumType (type)){
                                if (expr is IntLiteral && ((IntLiteral) expr).Value == 0)
                                        return new EnumConstant (expr, type);
                        }

                        object constant_value = TypeManager.ChangeType1 (expr.GetValue (), type, out fail);
                        if (fail){
                                // Error_CannotImplicitConversion (loc, expr.Type, type);

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
                        else if (type == TypeManager.decimal_type)
                                retval = new DecimalConstant ((decimal) constant_value);
                        else
				 throw new Exception ("LookupConstantValue: Unhandled constant type: " + type);

                        return retval;
                }
		
		 /// <summary>
                ///  Looks up the value of a constant field. Defines it if it hasn't
                ///  already been. Similar to LookupEnumValue in spirit.
                /// </summary>
                public bool LookupConstantValue (out object value, EmitContext ec)
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

                        Expr = Expr.Resolve (ec);

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
                                //else if ((ec_expr != null) && (ec_expr.Child is Constant))
                                 //       Expr = ec_expr.Child;
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

                     //   if (MemberType != real_expr.Type) {
			   if (type != Expr.Type) {

                                ce = ChangeType (Location, ce, type, ec);
                                if (ce == null){
                                        value = null;
                                        return false;
                                }
                                Expr = ce;
                        }
			
			if (ce != null)
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

                        if (ce is DecimalConstant) {
                                Decimal d = ((DecimalConstant)ce).Value;
                                int[] bits = Decimal.GetBits (d);
                                object[] args = new object[] { (byte)(bits [3] >> 16), (byte)(bits [3] >> 31), (uint)bits [2], (uint)bits [1], (uint)bits [0] };
                                CustomAttributeBuilder cab = new CustomAttributeBuilder (TypeManager.decimal_constant_attribute_ctor, args);
                                FieldBuilder.SetCustomAttribute (cab);
                        }
			else{
                                FieldBuilder.SetConstant (ConstantValue);
                        }

                        if (!TypeManager.RegisterFieldValue (FieldBuilder, ConstantValue))
                                throw new Exception ("Cannot register const value");

                        value = ConstantValue;
                        resolved = true;
                        return true;
                }



		
		
		
		/// <summary>
		///  Emits the field value by evaluating the expression
		/// </summary>
		public void EmitConstant (TypeContainer parent)
		{
			EmitContext ec = new EmitContext (parent, Location, null, type, ModFlags);
			object value;
			 LookupConstantValue (out value,ec);
			
			return;
		}
		
		public override void ApplyAttributeBuilder (Attribute a, CustomAttributeBuilder cb)
		{
			FieldBuilder.SetCustomAttribute (cb);
		}
		
	}
}
