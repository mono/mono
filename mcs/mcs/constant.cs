//
// constant.cs: Constants.
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//   Marek Safar (marek.safar@seznam.cz)
//
// (C) 2001 Ximian, Inc.
//
//

namespace Mono.CSharp {

	using System;
	using System.Reflection.Emit;
	using System.Collections;

	/// <summary>
	///   Base class for constants and literals.
	/// </summary>
	public abstract class Constant : Expression {

		protected Constant (Location loc)
		{
			this.loc = loc;
		}

		/// <remarks>
		///   This is different from ToString in that ToString
		///   is supposed to be there for debugging purposes,
		///   and is not guaranteed to be useful for anything else,
		///   AsString() will provide something that can be used
		///   for round-tripping C# code.  Maybe it can be used
		///   for IL assembly as well.
		/// </remarks>
		public abstract string AsString ();

		override public string ToString ()
		{
			return this.GetType ().Name + " (" + AsString () + ")";
		}

		public override bool GetAttributableValue (Type value_type, out object value)
		{
			if (value_type == TypeManager.object_type) {
				value = GetTypedValue ();
				return true;
			}

			Constant c = ImplicitConversionRequired (value_type, loc);
			if (c == null) {
				value = null;
				return false;
			}

			value = c.GetTypedValue ();
			return true;
		}

		/// <summary>
		///  This is used to obtain the actual value of the literal
		///  cast into an object.
		/// </summary>
		public abstract object GetValue ();

		public virtual object GetTypedValue ()
		{
			return GetValue ();
		}

		/// <summary>
		///   Constants are always born in a fully resolved state
		/// </summary>
		public override Expression DoResolve (EmitContext ec)
		{
			return this;
		}

		public Constant ImplicitConversionRequired (Type type, Location loc)
		{
			Constant c = ConvertImplicitly (type);
			if (c == null)
				Error_ValueCannotBeConverted (null, loc, type, false);
			return c;
		}

		public virtual Constant ConvertImplicitly (Type type)
		{
			if (this.type == type)
				return this;

			if (Convert.ImplicitNumericConversion (this, type) == null) 
				return null;

			bool fail;			
			object constant_value = TypeManager.ChangeType (GetValue (), type, out fail);
			if (fail){
				//
				// We should always catch the error before this is ever
				// reached, by calling Convert.ImplicitStandardConversionExists
				//
				throw new InternalErrorException ("Missing constant conversion between `{0}' and `{1}'",
				  TypeManager.CSharpName (Type), TypeManager.CSharpName (type));
			}

			return CreateConstant (type, constant_value, loc);
		}

		///  Returns a constant instance based on Type
		///  The returned value is already resolved.
		public static Constant CreateConstant (Type t, object v, Location loc)
		{
			if (t == TypeManager.int32_type)
				return new IntConstant ((int) v, loc);
			if (t == TypeManager.string_type)
				return new StringConstant ((string) v, loc);
			if (t == TypeManager.uint32_type)
				return new UIntConstant ((uint) v, loc);
			if (t == TypeManager.int64_type)
				return new LongConstant ((long) v, loc);
			if (t == TypeManager.uint64_type)
				return new ULongConstant ((ulong) v, loc);
			if (t == TypeManager.float_type)
				return new FloatConstant ((float) v, loc);
			if (t == TypeManager.double_type)
				return new DoubleConstant ((double) v, loc);
			if (t == TypeManager.short_type)
				return new ShortConstant ((short)v, loc);
			if (t == TypeManager.ushort_type)
				return new UShortConstant ((ushort)v, loc);
			if (t == TypeManager.sbyte_type)
				return new SByteConstant ((sbyte)v, loc);
			if (t == TypeManager.byte_type)
				return new ByteConstant ((byte)v, loc);
			if (t == TypeManager.char_type)
				return new CharConstant ((char)v, loc);
			if (t == TypeManager.bool_type)
				return new BoolConstant ((bool) v, loc);
			if (t == TypeManager.decimal_type)
				return new DecimalConstant ((decimal) v, loc);
			if (TypeManager.IsEnumType (t)) {
				Type real_type = TypeManager.GetEnumUnderlyingType (t);
				return new EnumConstant (CreateConstant (real_type, v, loc), t);
			} 
			if (v == null && !TypeManager.IsValueType (t))
				return new EmptyConstantCast (new NullLiteral (loc), t);

			throw new Exception ("Unknown type for constant (" + t +
					"), details: " + v);
		}

		public override Expression CreateExpressionTree (EmitContext ec)
		{
			ArrayList args = new ArrayList (2);
			args.Add (new Argument (this));
			args.Add (new Argument (
				new TypeOf (new TypeExpression (type, loc), loc)));

			return CreateExpressionFactoryCall ("Constant", args);
		}


		/// <summary>
		/// Maybe ConvertTo name is better. It tries to convert `this' constant to target_type.
		/// It throws OverflowException 
		/// </summary>
		// DON'T CALL THIS METHOD DIRECTLY AS IT DOES NOT HANDLE ENUMS
		public abstract Constant ConvertExplicitly (bool in_checked_context, Type target_type);

		/// <summary>
		///   Attempts to do a compile-time folding of a constant cast.
		/// </summary>
		public Constant TryReduce (EmitContext ec, Type target_type, Location loc)
		{
			try {
				return TryReduce (ec, target_type);
			}
			catch (OverflowException) {
				Report.Error (221, loc, "Constant value `{0}' cannot be converted to a `{1}' (use `unchecked' syntax to override)",
					GetValue ().ToString (), TypeManager.CSharpName (target_type));
				return null;
			}
		}

		Constant TryReduce (EmitContext ec, Type target_type)
		{
			if (Type == target_type)
				return this;

			if (TypeManager.IsEnumType (target_type)) {
				Constant c = TryReduce (ec, TypeManager.GetEnumUnderlyingType (target_type));
				if (c == null)
					return null;

				return new EnumConstant (c, target_type);
			}

			return ConvertExplicitly (ec.ConstantCheckState, target_type);
		}

		public abstract Constant Increment ();
		
		/// <summary>
		/// Need to pass type as the constant can require a boxing
		/// and in such case no optimization is possible
		/// </summary>
		public bool IsDefaultInitializer (Type type)
		{
			if (type == Type)
				return IsDefaultValue;

			return Type == TypeManager.null_type;
		}

		public abstract bool IsDefaultValue {
			get;
		}

		public abstract bool IsNegative {
			get;
		}

		//
		// Returns true iff 1) the stack type of this is one of Object, 
		// int32, int64 and 2) this == 0 or this == null.
		//
		public virtual bool IsZeroInteger {
			get { return false; }
		}

		protected override void CloneTo (CloneContext clonectx, Expression target)
		{
			// CloneTo: Nothing, we do not keep any state on this expression
		}
	}

	public abstract class IntegralConstant : Constant {
		protected IntegralConstant (Location loc) :
			base (loc)
		{
		}

		public override void Error_ValueCannotBeConverted (EmitContext ec, Location loc, Type target, bool expl)
		{
			try {
				ConvertExplicitly (true, target);
				base.Error_ValueCannotBeConverted (ec, loc, target, expl);
			}
			catch
			{
				Report.Error (31, loc, "Constant value `{0}' cannot be converted to a `{1}'",
					GetValue ().ToString (), TypeManager.CSharpName (target));
			}
		}
	}
	
	public class BoolConstant : Constant {
		public readonly bool Value;
		
		public BoolConstant (bool val, Location loc):
			base (loc)
		{
			type = TypeManager.bool_type;
			eclass = ExprClass.Value;

			Value = val;
		}

		override public string AsString ()
		{
			return Value ? "true" : "false";
		}

		public override object GetValue ()
		{
			return (object) Value;
		}
				
		
		public override void Emit (EmitContext ec)
		{
			if (Value)
				ec.ig.Emit (OpCodes.Ldc_I4_1);
			else
				ec.ig.Emit (OpCodes.Ldc_I4_0);
		}

		public override Constant Increment ()
		{
			throw new NotSupportedException ();
		}
	
		public override bool IsDefaultValue {
			get {
				return !Value;
			}
		}

		public override bool IsNegative {
			get {
				return false;
			}
		}
	
		public override bool IsZeroInteger {
			get { return Value == false; }
		}

		public override Constant ConvertExplicitly (bool in_checked_context, Type target_type)
		{
			return null;
		}

	}

	public class ByteConstant : IntegralConstant {
		public readonly byte Value;

		public ByteConstant (byte v, Location loc):
			base (loc)
		{
			type = TypeManager.byte_type;
			eclass = ExprClass.Value;
			Value = v;
		}

		public override void Emit (EmitContext ec)
		{
			IntLiteral.EmitInt (ec.ig, Value);
		}

		public override string AsString ()
		{
			return Value.ToString ();
		}

		public override object GetValue ()
		{
			return Value;
		}

		public override Constant Increment ()
		{
			return new ByteConstant (checked ((byte)(Value + 1)), loc);
		}

		public override bool IsDefaultValue {
			get {
				return Value == 0;
			}
		}

		public override bool IsNegative {
			get {
				return false;
			}
		}

		public override bool IsZeroInteger {
			get { return Value == 0; }
		}

		public override Constant ConvertExplicitly (bool in_checked_context, Type target_type)
		{
			if (target_type == TypeManager.sbyte_type) {
				if (in_checked_context){
					if (Value > SByte.MaxValue)
						throw new OverflowException ();
				}
				return new SByteConstant ((sbyte) Value, Location);
			}
			if (target_type == TypeManager.short_type)
				return new ShortConstant ((short) Value, Location);
			if (target_type == TypeManager.ushort_type)
				return new UShortConstant ((ushort) Value, Location);
			if (target_type == TypeManager.int32_type)
				return new IntConstant ((int) Value, Location);
			if (target_type == TypeManager.uint32_type)
				return new UIntConstant ((uint) Value, Location);
			if (target_type == TypeManager.int64_type)
				return new LongConstant ((long) Value, Location);
			if (target_type == TypeManager.uint64_type)
				return new ULongConstant ((ulong) Value, Location);
			if (target_type == TypeManager.float_type)
				return new FloatConstant ((float) Value, Location);
			if (target_type == TypeManager.double_type)
				return new DoubleConstant ((double) Value, Location);
			if (target_type == TypeManager.char_type)
				return new CharConstant ((char) Value, Location);
			if (target_type == TypeManager.decimal_type)
				return new DecimalConstant ((decimal) Value, Location);

			return null;
		}

	}

	public class CharConstant : Constant {
		public readonly char Value;

		public CharConstant (char v, Location loc):
			base (loc)
		{
			type = TypeManager.char_type;
			eclass = ExprClass.Value;
			Value = v;
		}

		public override void Emit (EmitContext ec)
		{
			IntLiteral.EmitInt (ec.ig, Value);
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

		public override string AsString ()
		{
			return "\"" + descape (Value) + "\"";
		}

		public override object GetValue ()
		{
			return Value;
		}

		public override Constant Increment ()
		{
			return new CharConstant (checked ((char)(Value + 1)), loc);
		}
		
		public override bool IsDefaultValue {
			get {
				return Value == 0;
			}
		}

		public override bool IsNegative {
			get {
				return false;
			}
		}

		public override bool IsZeroInteger {
			get { return Value == '\0'; }
		}

		public override Constant ConvertExplicitly (bool in_checked_context, Type target_type)
		{
			if (target_type == TypeManager.byte_type) {
				if (in_checked_context){
					if (Value < Byte.MinValue || Value > Byte.MaxValue)
						throw new OverflowException ();
				}
				return new ByteConstant ((byte) Value, Location);
			}
			if (target_type == TypeManager.sbyte_type) {
				if (in_checked_context){
					if (Value > SByte.MaxValue)
						throw new OverflowException ();
				}
				return new SByteConstant ((sbyte) Value, Location);
			}
			if (target_type == TypeManager.short_type) {
				if (in_checked_context){
					if (Value > Int16.MaxValue)
						throw new OverflowException ();
				}					
				return new ShortConstant ((short) Value, Location);
			}
			if (target_type == TypeManager.int32_type)
				return new IntConstant ((int) Value, Location);
			if (target_type == TypeManager.uint32_type)
				return new UIntConstant ((uint) Value, Location);
			if (target_type == TypeManager.int64_type)
				return new LongConstant ((long) Value, Location);
			if (target_type == TypeManager.uint64_type)
				return new ULongConstant ((ulong) Value, Location);
			if (target_type == TypeManager.float_type)
				return new FloatConstant ((float) Value, Location);
			if (target_type == TypeManager.double_type)
				return new DoubleConstant ((double) Value, Location);
			if (target_type == TypeManager.decimal_type)
				return new DecimalConstant ((decimal) Value, Location);

			return null;
		}

	}

	public class SByteConstant : IntegralConstant {
		public readonly sbyte Value;

		public SByteConstant (sbyte v, Location loc):
			base (loc)
		{
			type = TypeManager.sbyte_type;
			eclass = ExprClass.Value;
			Value = v;
		}

		public override void Emit (EmitContext ec)
		{
			IntLiteral.EmitInt (ec.ig, Value);
		}

		public override string AsString ()
		{
			return Value.ToString ();
		}

		public override object GetValue ()
		{
			return Value;
		}

		public override Constant Increment ()
		{
		    return new SByteConstant (checked((sbyte)(Value + 1)), loc);
		}

		public override bool IsDefaultValue {
			get {
				return Value == 0;
			}
		}

		public override bool IsNegative {
			get {
				return Value < 0;
			}
		}
		
		public override bool IsZeroInteger {
			get { return Value == 0; }
		}

		public override Constant ConvertExplicitly (bool in_checked_context, Type target_type)
		{
			if (target_type == TypeManager.byte_type) {
				if (in_checked_context && Value < 0)
					throw new OverflowException ();
				return new ByteConstant ((byte) Value, Location);
			}
			if (target_type == TypeManager.short_type)
				return new ShortConstant ((short) Value, Location);
			if (target_type == TypeManager.ushort_type) {
				if (in_checked_context && Value < 0)
					throw new OverflowException ();
				return new UShortConstant ((ushort) Value, Location);
			} if (target_type == TypeManager.int32_type)
				  return new IntConstant ((int) Value, Location);
			if (target_type == TypeManager.uint32_type) {
				if (in_checked_context && Value < 0)
					throw new OverflowException ();
				return new UIntConstant ((uint) Value, Location);
			} if (target_type == TypeManager.int64_type)
				  return new LongConstant ((long) Value, Location);
			if (target_type == TypeManager.uint64_type) {
				if (in_checked_context && Value < 0)
					throw new OverflowException ();
				return new ULongConstant ((ulong) Value, Location);
			}
			if (target_type == TypeManager.float_type)
				return new FloatConstant ((float) Value, Location);
			if (target_type == TypeManager.double_type)
				return new DoubleConstant ((double) Value, Location);
			if (target_type == TypeManager.char_type) {
				if (in_checked_context && Value < 0)
					throw new OverflowException ();
				return new CharConstant ((char) Value, Location);
			}
			if (target_type == TypeManager.decimal_type)
				return new DecimalConstant ((decimal) Value, Location);

			return null;
		}

	}

	public class ShortConstant : IntegralConstant {
		public readonly short Value;

		public ShortConstant (short v, Location loc):
			base (loc)
		{
			type = TypeManager.short_type;
			eclass = ExprClass.Value;
			Value = v;
		}

		public override void Emit (EmitContext ec)
		{
			IntLiteral.EmitInt (ec.ig, Value);
		}

		public override string AsString ()
		{
			return Value.ToString ();
		}

		public override object GetValue ()
		{
			return Value;
		}

		public override Constant Increment ()
		{
			return new ShortConstant (checked((short)(Value + 1)), loc);
		}

		public override bool IsDefaultValue {
			get {
				return Value == 0;
			}
		}
		
		public override bool IsZeroInteger {
			get { return Value == 0; }
		}

		public override bool IsNegative {
			get {
				return Value < 0;
			}
		}

		public override Constant ConvertExplicitly (bool in_checked_context, Type target_type)
		{
			if (target_type == TypeManager.byte_type) {
				if (in_checked_context){
					if (Value < Byte.MinValue || Value > Byte.MaxValue)
						throw new OverflowException ();
				}
				return new ByteConstant ((byte) Value, Location);
			}
			if (target_type == TypeManager.sbyte_type) {
				if (in_checked_context){
					if (Value < SByte.MinValue || Value > SByte.MaxValue)
						throw new OverflowException ();
				}
				return new SByteConstant ((sbyte) Value, Location);
			}
			if (target_type == TypeManager.ushort_type) {
				if (in_checked_context && Value < 0)
					throw new OverflowException ();
				
				return new UShortConstant ((ushort) Value, Location);
			}
			if (target_type == TypeManager.int32_type)
				return new IntConstant ((int) Value, Location);
			if (target_type == TypeManager.uint32_type) {
				if (in_checked_context && Value < 0)
					throw new OverflowException ();
				return new UIntConstant ((uint) Value, Location);
			}
			if (target_type == TypeManager.int64_type)
				return new LongConstant ((long) Value, Location);
			if (target_type == TypeManager.uint64_type) {
				if (in_checked_context && Value < 0)
					throw new OverflowException ();
				return new ULongConstant ((ulong) Value, Location);
			}
			if (target_type == TypeManager.float_type)
				return new FloatConstant ((float) Value, Location);
			if (target_type == TypeManager.double_type)
				return new DoubleConstant ((double) Value, Location);
			if (target_type == TypeManager.char_type) {
				if (in_checked_context){
					if (Value < Char.MinValue)
						throw new OverflowException ();
				}
				return new CharConstant ((char) Value, Location);
			}
			if (target_type == TypeManager.decimal_type)
				return new DecimalConstant ((decimal) Value, Location);

			return null;
		}

	}

	public class UShortConstant : IntegralConstant {
		public readonly ushort Value;

		public UShortConstant (ushort v, Location loc):
			base (loc)
		{
			type = TypeManager.ushort_type;
			eclass = ExprClass.Value;
			Value = v;
		}

		public override void Emit (EmitContext ec)
		{
			IntLiteral.EmitInt (ec.ig, Value);
		}

		public override string AsString ()
		{
			return Value.ToString ();
		}

		public override object GetValue ()
		{
			return Value;
		}
	
		public override Constant Increment ()
		{
			return new UShortConstant (checked((ushort)(Value + 1)), loc);
		}

		public override bool IsDefaultValue {
			get {
				return Value == 0;
			}
		}

		public override bool IsNegative {
			get {
				return false;
			}
		}
	
		public override bool IsZeroInteger {
			get { return Value == 0; }
		}

		public override Constant ConvertExplicitly (bool in_checked_context, Type target_type)
		{
			if (target_type == TypeManager.byte_type) {
				if (in_checked_context){
					if (Value > Byte.MaxValue)
						throw new OverflowException ();
				}
				return new ByteConstant ((byte) Value, Location);
			}
			if (target_type == TypeManager.sbyte_type) {
				if (in_checked_context){
					if (Value > SByte.MaxValue)
						throw new OverflowException ();
				}
				return new SByteConstant ((sbyte) Value, Location);
			}
			if (target_type == TypeManager.short_type) {
				if (in_checked_context){
					if (Value > Int16.MaxValue)
						throw new OverflowException ();
				}
				return new ShortConstant ((short) Value, Location);
			}
			if (target_type == TypeManager.int32_type)
				return new IntConstant ((int) Value, Location);
			if (target_type == TypeManager.uint32_type)
				return new UIntConstant ((uint) Value, Location);
			if (target_type == TypeManager.int64_type)
				return new LongConstant ((long) Value, Location);
			if (target_type == TypeManager.uint64_type)
				return new ULongConstant ((ulong) Value, Location);
			if (target_type == TypeManager.float_type)
				return new FloatConstant ((float) Value, Location);
			if (target_type == TypeManager.double_type)
				return new DoubleConstant ((double) Value, Location);
			if (target_type == TypeManager.char_type) {
				if (in_checked_context){
					if (Value > Char.MaxValue)
						throw new OverflowException ();
				}
				return new CharConstant ((char) Value, Location);
			}
			if (target_type == TypeManager.decimal_type)
				return new DecimalConstant ((decimal) Value, Location);

			return null;
		}
	}

	public class IntConstant : IntegralConstant {
		public readonly int Value;

		public IntConstant (int v, Location loc):
			base (loc)
		{
			type = TypeManager.int32_type;
			eclass = ExprClass.Value;
			Value = v;
		}

		static public void EmitInt (ILGenerator ig, int i)
		{
			switch (i){
			case -1:
				ig.Emit (OpCodes.Ldc_I4_M1);
				break;
				
			case 0:
				ig.Emit (OpCodes.Ldc_I4_0);
				break;
				
			case 1:
				ig.Emit (OpCodes.Ldc_I4_1);
				break;
				
			case 2:
				ig.Emit (OpCodes.Ldc_I4_2);
				break;
				
			case 3:
				ig.Emit (OpCodes.Ldc_I4_3);
				break;
				
			case 4:
				ig.Emit (OpCodes.Ldc_I4_4);
				break;
				
			case 5:
				ig.Emit (OpCodes.Ldc_I4_5);
				break;
				
			case 6:
				ig.Emit (OpCodes.Ldc_I4_6);
				break;
				
			case 7:
				ig.Emit (OpCodes.Ldc_I4_7);
				break;
				
			case 8:
				ig.Emit (OpCodes.Ldc_I4_8);
				break;

			default:
				if (i >= -128 && i <= 127){
					ig.Emit (OpCodes.Ldc_I4_S, (sbyte) i);
				} else
					ig.Emit (OpCodes.Ldc_I4, i);
				break;
			}
		}

		public override void Emit (EmitContext ec)
		{
			EmitInt (ec.ig, Value);
		}

		public override string AsString ()
		{
			return Value.ToString ();
		}

		public override object GetValue ()
		{
			return Value;
		}

		public override Constant Increment ()
		{
			return new IntConstant (checked(Value + 1), loc);
		}

		public override bool IsDefaultValue {
			get {
				return Value == 0;
			}
		}
		
		public override bool IsNegative {
			get {
				return Value < 0;
			}
		}

		public override bool IsZeroInteger {
			get { return Value == 0; }
		}

		public override Constant ConvertExplicitly (bool in_checked_context, Type target_type)
		{
			if (target_type == TypeManager.byte_type) {
				if (in_checked_context){
					if (Value < Byte.MinValue || Value > Byte.MaxValue)
						throw new OverflowException ();
				}
				return new ByteConstant ((byte) Value, Location);
			}
			if (target_type == TypeManager.sbyte_type) {
				if (in_checked_context){
					if (Value < SByte.MinValue || Value > SByte.MaxValue)
						throw new OverflowException ();
				}
				return new SByteConstant ((sbyte) Value, Location);
			}
			if (target_type == TypeManager.short_type) {
				if (in_checked_context){
					if (Value < Int16.MinValue || Value > Int16.MaxValue)
						throw new OverflowException ();
				}
				return new ShortConstant ((short) Value, Location);
			}
			if (target_type == TypeManager.ushort_type) {
				if (in_checked_context){
					if (Value < UInt16.MinValue || Value > UInt16.MaxValue)
						throw new OverflowException ();
				}
				return new UShortConstant ((ushort) Value, Location);
			}
			if (target_type == TypeManager.uint32_type) {
				if (in_checked_context){
					if (Value < UInt32.MinValue)
						throw new OverflowException ();
				}
				return new UIntConstant ((uint) Value, Location);
			}
			if (target_type == TypeManager.int64_type)
				return new LongConstant ((long) Value, Location);
			if (target_type == TypeManager.uint64_type) {
				if (in_checked_context && Value < 0)
					throw new OverflowException ();
				return new ULongConstant ((ulong) Value, Location);
			}
			if (target_type == TypeManager.float_type)
				return new FloatConstant ((float) Value, Location);
			if (target_type == TypeManager.double_type)
				return new DoubleConstant ((double) Value, Location);
			if (target_type == TypeManager.char_type) {
				if (in_checked_context){
					if (Value < Char.MinValue || Value > Char.MaxValue)
						throw new OverflowException ();
				}
				return new CharConstant ((char) Value, Location);
			}
			if (target_type == TypeManager.decimal_type)
				return new DecimalConstant ((decimal) Value, Location);

			return null;
		}

		public override Constant ConvertImplicitly (Type type)
		{
			if (this.type == type)
				return this;

			Constant c = TryImplicitIntConversion (type);
			if (c != null)
				return c;

			return base.ConvertImplicitly (type);
		}

		/// <summary>
		///   Attempts to perform an implicit constant conversion of the IntConstant
		///   into a different data type using casts (See Implicit Constant
		///   Expression Conversions)
		/// </summary>
		Constant TryImplicitIntConversion (Type target_type)
		{
			if (target_type == TypeManager.sbyte_type) {
				if (Value >= SByte.MinValue && Value <= SByte.MaxValue)
					return new SByteConstant ((sbyte) Value, loc);
			} 
			else if (target_type == TypeManager.byte_type) {
				if (Value >= Byte.MinValue && Value <= Byte.MaxValue)
					return new ByteConstant ((byte) Value, loc);
			} 
			else if (target_type == TypeManager.short_type) {
				if (Value >= Int16.MinValue && Value <= Int16.MaxValue)
					return new ShortConstant ((short) Value, loc);
			} 
			else if (target_type == TypeManager.ushort_type) {
				if (Value >= UInt16.MinValue && Value <= UInt16.MaxValue)
					return new UShortConstant ((ushort) Value, loc);
			} 
			else if (target_type == TypeManager.uint32_type) {
				if (Value >= 0)
					return new UIntConstant ((uint) Value, loc);
			} 
			else if (target_type == TypeManager.uint64_type) {
				//
				// we can optimize this case: a positive int32
				// always fits on a uint64.  But we need an opcode
				// to do it.
				//
				if (Value >= 0)
					return new ULongConstant ((ulong) Value, loc);
			} 
			else if (target_type == TypeManager.double_type)
				return new DoubleConstant ((double) Value, loc);
			else if (target_type == TypeManager.float_type)
				return new FloatConstant ((float) Value, loc);

			return null;
		}
	}

	public class UIntConstant : IntegralConstant {
		public readonly uint Value;

		public UIntConstant (uint v, Location loc):
			base (loc)
		{
			type = TypeManager.uint32_type;
			eclass = ExprClass.Value;
			Value = v;
		}

		public override void Emit (EmitContext ec)
		{
			IntLiteral.EmitInt (ec.ig, unchecked ((int) Value));
		}

		public override string AsString ()
		{
			return Value.ToString ();
		}

		public override object GetValue ()
		{
			return Value;
		}

		public override Constant Increment ()
		{
			return new UIntConstant (checked(Value + 1), loc);
		}
	
		public override bool IsDefaultValue {
			get {
				return Value == 0;
			}
		}

		public override bool IsNegative {
			get {
				return false;
			}
		}

		public override bool IsZeroInteger {
			get { return Value == 0; }
		}

		public override Constant ConvertExplicitly (bool in_checked_context, Type target_type)
		{
			if (target_type == TypeManager.byte_type) {
				if (in_checked_context){
					if (Value < Char.MinValue || Value > Char.MaxValue)
						throw new OverflowException ();
				}
				return new ByteConstant ((byte) Value, Location);
			}
			if (target_type == TypeManager.sbyte_type) {
				if (in_checked_context){
					if (Value > SByte.MaxValue)
						throw new OverflowException ();
				}
				return new SByteConstant ((sbyte) Value, Location);
			}
			if (target_type == TypeManager.short_type) {
				if (in_checked_context){
					if (Value > Int16.MaxValue)
						throw new OverflowException ();
				}
				return new ShortConstant ((short) Value, Location);
			}
			if (target_type == TypeManager.ushort_type) {
				if (in_checked_context){
					if (Value < UInt16.MinValue || Value > UInt16.MaxValue)
						throw new OverflowException ();
				}
				return new UShortConstant ((ushort) Value, Location);
			}
			if (target_type == TypeManager.int32_type) {
				if (in_checked_context){
					if (Value > Int32.MaxValue)
						throw new OverflowException ();
				}
				return new IntConstant ((int) Value, Location);
			}
			if (target_type == TypeManager.int64_type)
				return new LongConstant ((long) Value, Location);
			if (target_type == TypeManager.uint64_type)
				return new ULongConstant ((ulong) Value, Location);
			if (target_type == TypeManager.float_type)
				return new FloatConstant ((float) Value, Location);
			if (target_type == TypeManager.double_type)
				return new DoubleConstant ((double) Value, Location);
			if (target_type == TypeManager.char_type) {
				if (in_checked_context){
					if (Value < Char.MinValue || Value > Char.MaxValue)
						throw new OverflowException ();
				}
				return new CharConstant ((char) Value, Location);
			}
			if (target_type == TypeManager.decimal_type)
				return new DecimalConstant ((decimal) Value, Location);

			return null;
		}

	}

	public class LongConstant : IntegralConstant {
		public readonly long Value;

		public LongConstant (long v, Location loc):
			base (loc)
		{
			type = TypeManager.int64_type;
			eclass = ExprClass.Value;
			Value = v;
		}

		public override void Emit (EmitContext ec)
		{
			EmitLong (ec.ig, Value);
		}

		static public void EmitLong (ILGenerator ig, long l)
		{
			if (l >= int.MinValue && l <= int.MaxValue) {
				IntLiteral.EmitInt (ig, unchecked ((int) l));
				ig.Emit (OpCodes.Conv_I8);
				return;
			}
			ig.Emit (OpCodes.Ldc_I8, l);
		}

		public override string AsString ()
		{
			return Value.ToString ();
		}

		public override object GetValue ()
		{
			return Value;
		}

		public override Constant Increment ()
		{
			return new LongConstant (checked(Value + 1), loc);
		}
		
		public override bool IsDefaultValue {
			get {
				return Value == 0;
			}
		}

		public override bool IsNegative {
			get {
				return Value < 0;
			}
		}

		public override bool IsZeroInteger {
			get { return Value == 0; }
		}

		public override Constant ConvertExplicitly (bool in_checked_context, Type target_type)
		{
			if (target_type == TypeManager.byte_type) {
				if (in_checked_context){
					if (Value < Byte.MinValue || Value > Byte.MaxValue)
						throw new OverflowException ();
				}
				return new ByteConstant ((byte) Value, Location);
			}
			if (target_type == TypeManager.sbyte_type) {
				if (in_checked_context){
					if (Value < SByte.MinValue || Value > SByte.MaxValue)
						throw new OverflowException ();
				}
				return new SByteConstant ((sbyte) Value, Location);
			}
			if (target_type == TypeManager.short_type) {
				if (in_checked_context){
					if (Value < Int16.MinValue || Value > Int16.MaxValue)
						throw new OverflowException ();
				}
				return new ShortConstant ((short) Value, Location);
			}
			if (target_type == TypeManager.ushort_type) {
				if (in_checked_context){
					if (Value < UInt16.MinValue || Value > UInt16.MaxValue)
						throw new OverflowException ();
				}
				return new UShortConstant ((ushort) Value, Location);
			}
			if (target_type == TypeManager.int32_type) {
				if (in_checked_context){
					if (Value < Int32.MinValue || Value > Int32.MaxValue)
						throw new OverflowException ();
				}
				return new IntConstant ((int) Value, Location);
			}
			if (target_type == TypeManager.uint32_type) {
				if (in_checked_context){
					if (Value < UInt32.MinValue || Value > UInt32.MaxValue)
						throw new OverflowException ();
				}
				return new UIntConstant ((uint) Value, Location);
			}
			if (target_type == TypeManager.uint64_type) {
				if (in_checked_context && Value < 0)
					throw new OverflowException ();
				return new ULongConstant ((ulong) Value, Location);
			}
			if (target_type == TypeManager.float_type)
				return new FloatConstant ((float) Value, Location);
			if (target_type == TypeManager.double_type)
				return new DoubleConstant ((double) Value, Location);
			if (target_type == TypeManager.char_type) {
				if (in_checked_context){
					if (Value < Char.MinValue || Value > Char.MaxValue)
						throw new OverflowException ();
				}
				return new CharConstant ((char) Value, Location);
			}
			if (target_type == TypeManager.decimal_type)
				return new DecimalConstant ((decimal) Value, Location);

			return null;
		}

		public override Constant ConvertImplicitly (Type type)
		{
			if (Value >= 0 && type == TypeManager.uint64_type) {
				return new ULongConstant ((ulong) Value, loc);
			}

			return base.ConvertImplicitly (type);
		}
	}

	public class ULongConstant : IntegralConstant {
		public readonly ulong Value;

		public ULongConstant (ulong v, Location loc):
			base (loc)
		{
			type = TypeManager.uint64_type;
			eclass = ExprClass.Value;
			Value = v;
		}

		public override void Emit (EmitContext ec)
		{
			ILGenerator ig = ec.ig;

			LongLiteral.EmitLong (ig, unchecked ((long) Value));
		}

		public override string AsString ()
		{
			return Value.ToString ();
		}

		public override object GetValue ()
		{
			return Value;
		}

		public override Constant Increment ()
		{
			return new ULongConstant (checked(Value + 1), loc);
		}

		public override bool IsDefaultValue {
			get {
				return Value == 0;
			}
		}

		public override bool IsNegative {
			get {
				return false;
			}
		}

		public override bool IsZeroInteger {
			get { return Value == 0; }
		}

		public override Constant ConvertExplicitly (bool in_checked_context, Type target_type)
		{
			if (target_type == TypeManager.byte_type) {
				if (in_checked_context && Value > Byte.MaxValue)
					throw new OverflowException ();
				return new ByteConstant ((byte) Value, Location);
			}
			if (target_type == TypeManager.sbyte_type) {
				if (in_checked_context && Value > ((ulong) SByte.MaxValue))
					throw new OverflowException ();
				return new SByteConstant ((sbyte) Value, Location);
			}
			if (target_type == TypeManager.short_type) {
				if (in_checked_context && Value > ((ulong) Int16.MaxValue))
					throw new OverflowException ();
				return new ShortConstant ((short) Value, Location);
			}
			if (target_type == TypeManager.ushort_type) {
				if (in_checked_context && Value > UInt16.MaxValue)
					throw new OverflowException ();
				return new UShortConstant ((ushort) Value, Location);
			}
			if (target_type == TypeManager.int32_type) {
				if (in_checked_context && Value > UInt32.MaxValue)
					throw new OverflowException ();
				return new IntConstant ((int) Value, Location);
			}
			if (target_type == TypeManager.uint32_type) {
				if  (in_checked_context && Value > UInt32.MaxValue)
					throw new OverflowException ();
				return new UIntConstant ((uint) Value, Location);
			}
			if (target_type == TypeManager.int64_type) {
				if (in_checked_context && Value > Int64.MaxValue)
					throw new OverflowException ();
				return new LongConstant ((long) Value, Location);
			}
			if (target_type == TypeManager.float_type)
				return new FloatConstant ((float) Value, Location);
			if (target_type == TypeManager.double_type)
				return new DoubleConstant ((double) Value, Location);
			if (target_type == TypeManager.char_type) {
				if (in_checked_context && Value > Char.MaxValue)
					throw new OverflowException ();
				return new CharConstant ((char) Value, Location);
			}
			if (target_type == TypeManager.decimal_type)
				return new DecimalConstant ((decimal) Value, Location);

			return null;
		}

	}

	public class FloatConstant : Constant {
		public float Value;

		public FloatConstant (float v, Location loc):
			base (loc)
		{
			type = TypeManager.float_type;
			eclass = ExprClass.Value;
			Value = v;
		}

		public override void Emit (EmitContext ec)
		{
			ec.ig.Emit (OpCodes.Ldc_R4, Value);
		}

		public override string AsString ()
		{
			return Value.ToString ();
		}

		public override object GetValue ()
		{
			return Value;
		}

		public override Constant Increment ()
		{
			return new FloatConstant (checked(Value + 1), loc);
		}

		public override bool IsDefaultValue {
			get {
				return Value == 0;
			}
		}

		public override bool IsNegative {
			get {
				return Value < 0;
			}
		}

		public override Constant ConvertExplicitly (bool in_checked_context, Type target_type)
		{
			if (target_type == TypeManager.byte_type) {
				if (in_checked_context){
					if (Value < byte.MinValue || Value > byte.MaxValue)
						throw new OverflowException ();
				}
				return new ByteConstant ((byte) Value, Location);
			}
			if (target_type == TypeManager.sbyte_type) {
				if (in_checked_context){
					if (Value <  sbyte.MinValue || Value > sbyte.MaxValue)
						throw new OverflowException ();
				}
				return new SByteConstant ((sbyte) Value, Location);
			}
			if (target_type == TypeManager.short_type) {
				if (in_checked_context){
					if (Value < short.MinValue || Value > short.MaxValue)
						throw new OverflowException ();
				}
				return new ShortConstant ((short) Value, Location);
			}
			if (target_type == TypeManager.ushort_type) {
				if (in_checked_context){
					if (Value < ushort.MinValue || Value > ushort.MaxValue)
						throw new OverflowException ();
				}
				return new UShortConstant ((ushort) Value, Location);
			}
			if (target_type == TypeManager.int32_type) {
				if (in_checked_context){
					if (Value < int.MinValue || Value > int.MaxValue)
						throw new OverflowException ();
				}
				return new IntConstant ((int) Value, Location);
			}
			if (target_type == TypeManager.uint32_type) {
				if (in_checked_context){
					if (Value < uint.MinValue || Value > uint.MaxValue)
						throw new OverflowException ();
				}
				return new UIntConstant ((uint) Value, Location);
			}
			if (target_type == TypeManager.int64_type) {
				if (in_checked_context){
					if (Value < long.MinValue || Value > long.MaxValue)
						throw new OverflowException ();
				}
				return new LongConstant ((long) Value, Location);
			}
			if (target_type == TypeManager.uint64_type) {
				if (in_checked_context){
					if (Value < ulong.MinValue || Value > ulong.MaxValue)
						throw new OverflowException ();
				}
				return new ULongConstant ((ulong) Value, Location);
			}
			if (target_type == TypeManager.double_type)
				return new DoubleConstant ((double) Value, Location);
			if (target_type == TypeManager.char_type) {
				if (in_checked_context){
					if (Value < (float) char.MinValue || Value > (float) char.MaxValue)
						throw new OverflowException ();
				}
				return new CharConstant ((char) Value, Location);
			}
			if (target_type == TypeManager.decimal_type)
				return new DecimalConstant ((decimal) Value, Location);

			return null;
		}

	}

	public class DoubleConstant : Constant {
		public double Value;

		public DoubleConstant (double v, Location loc):
			base (loc)
		{
			type = TypeManager.double_type;
			eclass = ExprClass.Value;
			Value = v;
		}

		public override void Emit (EmitContext ec)
		{
			ec.ig.Emit (OpCodes.Ldc_R8, Value);
		}

		public override string AsString ()
		{
			return Value.ToString ();
		}

		public override object GetValue ()
		{
			return Value;
		}

		public override Constant Increment ()
		{
			return new DoubleConstant (checked(Value + 1), loc);
		}

		public override bool IsDefaultValue {
			get {
				return Value == 0;
			}
		}

		public override bool IsNegative {
			get {
				return Value < 0;
			}
		}

		public override Constant ConvertExplicitly (bool in_checked_context, Type target_type)
		{
			if (target_type == TypeManager.byte_type) {
				if (in_checked_context){
					if (Value < Byte.MinValue || Value > Byte.MaxValue)
						throw new OverflowException ();
				}
				return new ByteConstant ((byte) Value, Location);
			}
			if (target_type == TypeManager.sbyte_type) {
				if (in_checked_context){
					if (Value < SByte.MinValue || Value > SByte.MaxValue)
						throw new OverflowException ();
				}
				return new SByteConstant ((sbyte) Value, Location);
			}
			if (target_type == TypeManager.short_type) {
				if (in_checked_context){
					if (Value < short.MinValue || Value > short.MaxValue)
						throw new OverflowException ();
				}
				return new ShortConstant ((short) Value, Location);
			}
			if (target_type == TypeManager.ushort_type) {
				if (in_checked_context){
					if (Value < ushort.MinValue || Value > ushort.MaxValue)
						throw new OverflowException ();
				}
				return new UShortConstant ((ushort) Value, Location);
			}
			if (target_type == TypeManager.int32_type) {
				if (in_checked_context){
					if (Value < int.MinValue || Value > int.MaxValue)
						throw new OverflowException ();
				}
				return new IntConstant ((int) Value, Location);
			}
			if (target_type == TypeManager.uint32_type) {
				if (in_checked_context){
					if (Value < uint.MinValue || Value > uint.MaxValue)
						throw new OverflowException ();
				}
				return new UIntConstant ((uint) Value, Location);
			}
			if (target_type == TypeManager.int64_type) {
				if (in_checked_context){
					if (Value < long.MinValue || Value > long.MaxValue)
						throw new OverflowException ();
				}
				return new LongConstant ((long) Value, Location);
			}
			if (target_type == TypeManager.uint64_type) {
				if (in_checked_context){
					if (Value < ulong.MinValue || Value > ulong.MaxValue)
						throw new OverflowException ();
				}
				return new ULongConstant ((ulong) Value, Location);
			}
			if (target_type == TypeManager.float_type)
				return new FloatConstant ((float) Value, Location);
			if (target_type == TypeManager.char_type) {
				if (in_checked_context){
					if (Value < (double) char.MinValue || Value > (double) char.MaxValue)
						throw new OverflowException ();
				}
				return new CharConstant ((char) Value, Location);
			}
			if (target_type == TypeManager.decimal_type)
				return new DecimalConstant ((decimal) Value, Location);

			return null;
		}

	}

	public class DecimalConstant : Constant {
		public readonly decimal Value;

		public DecimalConstant (decimal d, Location loc):
			base (loc)
		{
			type = TypeManager.decimal_type;
			eclass = ExprClass.Value;
			Value = d;
		}

		override public string AsString ()
		{
			return Value.ToString ();
		}

		public override object GetValue ()
		{
			return (object) Value;
		}

		public override void Emit (EmitContext ec)
		{
			ILGenerator ig = ec.ig;

			int [] words = Decimal.GetBits (Value);
			int power = (words [3] >> 16) & 0xff;

			if (power == 0 && Value <= int.MaxValue && Value >= int.MinValue)
			{
				if (TypeManager.void_decimal_ctor_int_arg == null) {
					TypeManager.void_decimal_ctor_int_arg = TypeManager.GetPredefinedConstructor (
						TypeManager.decimal_type, loc, TypeManager.int32_type);

					if (TypeManager.void_decimal_ctor_int_arg == null)
						return;
				}

				IntConstant.EmitInt (ig, (int)Value);
				ig.Emit (OpCodes.Newobj, TypeManager.void_decimal_ctor_int_arg);
				return;
			}

			
			//
			// FIXME: we could optimize this, and call a better 
			// constructor
			//

			IntConstant.EmitInt (ig, words [0]);
			IntConstant.EmitInt (ig, words [1]);
			IntConstant.EmitInt (ig, words [2]);

			// sign
			IntConstant.EmitInt (ig, words [3] >> 31);

			// power
			IntConstant.EmitInt (ig, power);

			if (TypeManager.void_decimal_ctor_five_args == null) {
				TypeManager.void_decimal_ctor_five_args = TypeManager.GetPredefinedConstructor (
					TypeManager.decimal_type, loc, TypeManager.int32_type, TypeManager.int32_type,
					TypeManager.int32_type, TypeManager.bool_type, TypeManager.byte_type);

				if (TypeManager.void_decimal_ctor_five_args == null)
					return;
			}

			ig.Emit (OpCodes.Newobj, TypeManager.void_decimal_ctor_five_args);
		}

		public override Constant Increment ()
		{
			return new DecimalConstant (checked (Value + 1), loc);
		}

		public override bool IsDefaultValue {
			get {
				return Value == 0;
			}
		}

		public override bool IsNegative {
			get {
				return Value < 0;
			}
		}

		public override Constant ConvertExplicitly (bool in_checked_context, Type target_type)
		{
			if (target_type == TypeManager.sbyte_type)
				return new SByteConstant ((sbyte)Value, loc);
			if (target_type == TypeManager.byte_type)
				return new ByteConstant ((byte)Value, loc);
			if (target_type == TypeManager.short_type)
				return new ShortConstant ((short)Value, loc);
			if (target_type == TypeManager.ushort_type)
				return new UShortConstant ((ushort)Value, loc);
			if (target_type == TypeManager.int32_type)
				return new IntConstant ((int)Value, loc);
			if (target_type == TypeManager.uint32_type)
				return new UIntConstant ((uint)Value, loc);
			if (target_type == TypeManager.int64_type)
				return new LongConstant ((long)Value, loc);
			if (target_type == TypeManager.uint64_type)
				return new ULongConstant ((ulong)Value, loc);
			if (target_type == TypeManager.char_type)
				return new CharConstant ((char)Value, loc);
			if (target_type == TypeManager.float_type)
				return new FloatConstant ((float)Value, loc);
			if (target_type == TypeManager.double_type)
				return new DoubleConstant ((double)Value, loc);

			return null;
		}

	}

	public class StringConstant : Constant {
		public readonly string Value;

		public StringConstant (string s, Location loc):
			base (loc)
		{
			type = TypeManager.string_type;
			eclass = ExprClass.Value;
			Value = s;
		}

		// FIXME: Escape the string.
		override public string AsString ()
		{
			return "\"" + Value + "\"";
		}

		public override object GetValue ()
		{
			return Value;
		}
		
		public override void Emit (EmitContext ec)
		{
			if (Value == null)
				ec.ig.Emit (OpCodes.Ldnull);
			else
				ec.ig.Emit (OpCodes.Ldstr, Value);
		}

		public override Constant Increment ()
		{
			throw new NotSupportedException ();
		}

		public override bool IsDefaultValue {
			get {
				return Value == null;
			}
		}

		public override bool IsNegative {
			get {
				return false;
			}
		}

		public override Constant ConvertExplicitly (bool in_checked_context, Type target_type)
		{
			return null;
		}
	}

	/// <summary>
	///   The value is constant, but when emitted has a side effect.  This is
	///   used by BitwiseAnd to ensure that the second expression is invoked
	///   regardless of the value of the left side.  
	/// </summary>
	
	public class SideEffectConstant : Constant {
		public Constant left;
		Expression right;
		
		public SideEffectConstant (Constant left, Expression right, Location loc) : base (loc)
		{
			this.left = left;
			this.right = right;
			eclass = ExprClass.Value;
			type = left.Type;
		}

		public override string AsString ()
		{
			return left.AsString ();
		}

		public override object GetValue ()
		{
			return left.GetValue ();
		}

		public override void Emit (EmitContext ec)
		{
			//
			// This happens when both sides have side-effects and
			// the result is a constant
			//
			if (left is SideEffectConstant) {
				left.Emit (ec);
				ec.ig.Emit (OpCodes.Pop);
			}

			right.Emit (ec);
		}

		public override bool IsDefaultValue {
			get {
				return left.IsDefaultValue;
			}
		}

		public override Constant Increment ()
		{
			throw new NotSupportedException ();
		}
		
		public override bool IsNegative {
			get {
				return left.IsNegative;
			}
		}

		public override bool IsZeroInteger {
			get {
				return left.IsZeroInteger;
			}
		}

		public override Constant ConvertExplicitly (bool in_checked_context, Type target_type)
		{
			return left.ConvertExplicitly (in_checked_context, target_type);
		}
	}
}


