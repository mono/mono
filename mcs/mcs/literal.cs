//
// literal.cs: Literal representation for the IL tree.
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//   Marek Safar (marek.safar@seznam.cz)
//
// Copyright 2001 Ximian, Inc.
//
//
// Notice that during parsing we create objects of type Literal, but the
// types are not loaded (thats why the Resolve method has to assign the
// type at that point).
//
// Literals differ from the constants in that we know we encountered them
// as a literal in the source code (and some extra rules apply there) and
// they have to be resolved (since during parsing we have not loaded the
// types yet) while constants are created only after types have been loaded
// and are fully resolved when born.
//

using System;
using System.Reflection;
using System.Reflection.Emit;

namespace Mono.CSharp {

	//
	// The null literal
	//
	// Note: C# specification null-literal is NullLiteral of NullType type
	//
	public class NullLiteral : Constant
	{
		//
		// Default type of null is an object
		//
		public NullLiteral (Location loc):
			this (typeof (NullLiteral), loc)
		{
		}

		//
		// Null can have its own type, think of default (Foo)
		//
		public NullLiteral (Type type, Location loc)
			: base (loc)
		{
			eclass = ExprClass.Value;
			this.type = type;
		}

		override public string AsString ()
		{
			return GetSignatureForError ();
		}
		
		public override Expression CreateExpressionTree (ResolveContext ec)
		{
			// HACK: avoid referencing mcs internal type
			if (type == typeof (NullLiteral))
				type = TypeManager.object_type;

			return base.CreateExpressionTree (ec);
		}		

		public override Expression DoResolve (ResolveContext ec)
		{
			return this;
		}

		public override void Emit (EmitContext ec)
		{
			ec.ig.Emit (OpCodes.Ldnull);

#if GMCS_SOURCE
			// Only to make verifier happy
			if (TypeManager.IsGenericParameter (type))
				ec.ig.Emit (OpCodes.Unbox_Any, type);
#endif
		}

		public override string ExprClassName {
			get {
				return GetSignatureForError ();
			}
		}

		public override string GetSignatureForError ()
		{
			return "null";
		}

		public override void Error_ValueCannotBeConverted (ResolveContext ec, Location loc, Type t, bool expl)
		{
			if (TypeManager.IsGenericParameter (t)) {
				ec.Report.Error(403, loc,
					"Cannot convert null to the type parameter `{0}' because it could be a value " +
					"type. Consider using `default ({0})' instead", t.Name);
				return;
			}

			if (TypeManager.IsValueType (t)) {
				ec.Report.Error(37, loc, "Cannot convert null to `{0}' because it is a value type",
					TypeManager.CSharpName(t));
				return;
			}

			base.Error_ValueCannotBeConverted (ec, loc, t, expl);
		}

		public override Constant ConvertExplicitly (bool inCheckedContext, Type targetType)
		{
			if (targetType.IsPointer) {
				if (type == TypeManager.null_type || this is NullPointer)
					return new EmptyConstantCast (new NullPointer (loc), targetType);

				return null;
			}

			// Exlude internal compiler types
			if (targetType == InternalType.AnonymousMethod)
				return null;

			if (type != TypeManager.null_type && !Convert.ImplicitStandardConversionExists (this, targetType))
				return null;

			if (TypeManager.IsReferenceType (targetType))
				return new NullLiteral (targetType, loc);

			if (TypeManager.IsNullableType (targetType))
				return Nullable.LiftedNull.Create (targetType, loc);

			return null;
		}

		public override Constant ConvertImplicitly (Type targetType)
		{
			//
			// Null literal is of object type
			//
			if (targetType == TypeManager.object_type)
				return this;

			return ConvertExplicitly (false, targetType);
		}

		public override object GetValue ()
		{
			return null;
		}

		public override Constant Increment ()
		{
			throw new NotSupportedException ();
		}

		public override bool IsDefaultValue {
			get { return true; }
		}

		public override bool IsLiteral {
			get { return true; }
		}

		public override bool IsNegative {
			get { return false; }
		}

		public override bool IsNull {
			get { return true; }
		}

		public override bool IsZeroInteger {
			get { return true; }
		}
		
		public override void MutateHoistedGenericType (AnonymousMethodStorey storey)
		{
			type = storey.MutateType (type);
		}
	}

	//
	// A null literal in a pointer context
	//
	class NullPointer : NullLiteral {
		public NullPointer (Location loc):
			base (loc)
		{
			type = TypeManager.object_type;
		}

		public override void Emit (EmitContext ec)
		{
			ILGenerator ig = ec.ig;
				
			//
			// Emits null pointer
			//
			ig.Emit (OpCodes.Ldc_I4_0);
			ig.Emit (OpCodes.Conv_U);
		}
	}

	public class BoolLiteral : BoolConstant {
		public BoolLiteral (bool val, Location loc) : base (val, loc)
		{
		}

		public override Expression DoResolve (ResolveContext ec)
		{
			type = TypeManager.bool_type;
			return this;
		}

		public override bool IsLiteral {
			get { return true; }
		}
	}

	public class CharLiteral : CharConstant {
		public CharLiteral (char c, Location loc) : base (c, loc)
		{
		}

		public override Expression DoResolve (ResolveContext ec)
		{
			type = TypeManager.char_type;
			return this;
		}

		public override bool IsLiteral {
			get { return true; }
		}
	}

	public class IntLiteral : IntConstant {
		public IntLiteral (int l, Location loc) : base (l, loc)
		{
		}

		public override Expression DoResolve (ResolveContext ec)
		{
			type = TypeManager.int32_type;
			return this;
		}

		public override Constant ConvertImplicitly (Type type)
		{
			///
			/// The 0 literal can be converted to an enum value,
			///
			if (Value == 0 && TypeManager.IsEnumType (type)) {
				Constant c = ConvertImplicitly (TypeManager.GetEnumUnderlyingType (type));
				if (c == null)
					return null;

				return new EnumConstant (c, type);
			}
			return base.ConvertImplicitly (type);
		}

		public override bool IsLiteral {
			get { return true; }
		}
	}

	public class UIntLiteral : UIntConstant {
		public UIntLiteral (uint l, Location loc) : base (l, loc)
		{
		}

		public override Expression DoResolve (ResolveContext ec)
		{
			type = TypeManager.uint32_type;
			return this;
		}

		public override bool IsLiteral {
			get { return true; }
		}
	}
	
	public class LongLiteral : LongConstant {
		public LongLiteral (long l, Location loc) : base (l, loc)
		{
		}

		public override Expression DoResolve (ResolveContext ec)
		{
			type = TypeManager.int64_type;
			return this;
		}

		public override bool IsLiteral {
			get { return true; }
		}
	}

	public class ULongLiteral : ULongConstant {
		public ULongLiteral (ulong l, Location loc) : base (l, loc)
		{
		}

		public override Expression DoResolve (ResolveContext ec)
		{
			type = TypeManager.uint64_type;
			return this;
		}

		public override bool IsLiteral {
			get { return true; }
		}
	}
	
	public class FloatLiteral : FloatConstant {
		
		public FloatLiteral (float f, Location loc) : base (f, loc)
		{
		}

		public override Expression DoResolve (ResolveContext ec)
		{
			type = TypeManager.float_type;
			return this;
		}

		public override bool IsLiteral {
			get { return true; }
		}

	}

	public class DoubleLiteral : DoubleConstant {
		public DoubleLiteral (double d, Location loc) : base (d, loc)
		{
		}

		public override Expression DoResolve (ResolveContext ec)
		{
			type = TypeManager.double_type;

			return this;
		}

		public override void Error_ValueCannotBeConverted (ResolveContext ec, Location loc, Type target, bool expl)
		{
			if (target == TypeManager.float_type) {
				Error_664 (ec, loc, "float", "f");
				return;
			}

			if (target == TypeManager.decimal_type) {
				Error_664 (ec, loc, "decimal", "m");
				return;
			}

			base.Error_ValueCannotBeConverted (ec, loc, target, expl);
		}

		static void Error_664 (ResolveContext ec, Location loc, string type, string suffix)
		{
			ec.Report.Error (664, loc,
				"Literal of type double cannot be implicitly converted to type `{0}'. Add suffix `{1}' to create a literal of this type",
				type, suffix);
		}

		public override bool IsLiteral {
			get { return true; }
		}

	}

	public class DecimalLiteral : DecimalConstant {
		public DecimalLiteral (decimal d, Location loc) : base (d, loc)
		{
		}

		public override Expression DoResolve (ResolveContext ec)
		{
			type = TypeManager.decimal_type;
			return this;
		}

		public override bool IsLiteral {
			get { return true; }
		}
	}

	public class StringLiteral : StringConstant {
		public StringLiteral (string s, Location loc) : base (s, loc)
		{
		}

		public override Expression DoResolve (ResolveContext ec)
		{
			type = TypeManager.string_type;

			return this;
		}

		public override bool IsLiteral {
			get { return true; }
		}

	}
}
