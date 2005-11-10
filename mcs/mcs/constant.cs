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

		//
		// The various ToXXXX conversion functions are used by the constant
		// folding evaluator.   A null value is returned if the conversion is
		// not possible.   
		//
		// Note: not all the patterns for catching `implicit_conv' are the same.
		// some implicit conversions can never be performed between two types
		// even if the conversion would be lossless (for example short to uint),
		// but some conversions are explicitly permitted by the standard provided
		// that there will be no loss of information (for example, int to uint).
		//
		public DoubleConstant ToDouble (Location loc)
		{
			DoubleConstant c = ConvertToDouble ();

			if (c == null)
				Error_ValueCannotBeConverted (loc, TypeManager.double_type, false);

			return c;
		}

		public FloatConstant ToFloat (Location loc)
		{
			FloatConstant c = ConvertToFloat ();

			if (c == null)
				Error_ValueCannotBeConverted (loc, TypeManager.float_type, false);

			return c;
		}

		public ULongConstant ToULong (Location loc)
		{
			ULongConstant c = ConvertToULong ();

			if (c == null)
				Error_ValueCannotBeConverted (loc, TypeManager.uint64_type, false);

			return c;
		}

		public LongConstant ToLong (Location loc)
		{
			LongConstant c = ConvertToLong ();

			if (c == null)
				Error_ValueCannotBeConverted (loc, TypeManager.int64_type, false);

			return c;
		}
		
		public UIntConstant ToUInt (Location loc)
		{
			UIntConstant c = ConvertToUInt ();

			if (c == null)
				Error_ValueCannotBeConverted (loc, TypeManager.uint32_type, false);

			return c;
		}

		public IntConstant ToInt (Location loc)
		{
			IntConstant c = ConvertToInt ();

			if (c == null)
				Error_ValueCannotBeConverted (loc, TypeManager.int32_type, false);

			return c;
		}

		public DecimalConstant ToDecimal (Location loc)
		{
			DecimalConstant c = ConvertToDecimal ();

			if (c == null)
				Error_ValueCannotBeConverted (loc, TypeManager.decimal_type, false);

			return c;
		}

		public virtual Constant ToType (Type type, Location loc)
		{
			if (Type == type)
				return this;

			if (type == TypeManager.object_type)
				return this;

			if (!Convert.ImplicitStandardConversionExists (Convert.ConstantEC, this, type)){
				Error_ValueCannotBeConverted (loc, type, false);
				return null;
			}

			// Special-case: The 0 literal can be converted to an enum value,
			// and ImplicitStandardConversionExists will return true in that case.
			if (IsZeroInteger && Type == TypeManager.int32_type && TypeManager.IsEnumType (type)) {
				return new EnumConstant (this, type);
			}

			bool fail;			
			object constant_value = TypeManager.ChangeType (GetValue (), type, out fail);
			if (fail){
				Error_ValueCannotBeConverted (loc, type, false);
				
				//
				// We should always catch the error before this is ever
				// reached, by calling Convert.ImplicitStandardConversionExists
				//
				throw new Exception (
					String.Format ("LookupConstantValue: This should never be reached {0} {1}", Type, type));
			}

			Constant retval;
			if (type == TypeManager.int32_type)
				retval = new IntConstant ((int) constant_value, loc);
			else if (type == TypeManager.uint32_type)
				retval = new UIntConstant ((uint) constant_value, loc);
			else if (type == TypeManager.int64_type)
				retval = new LongConstant ((long) constant_value, loc);
			else if (type == TypeManager.uint64_type)
				retval = new ULongConstant ((ulong) constant_value, loc);
			else if (type == TypeManager.float_type)
				retval = new FloatConstant ((float) constant_value, loc);
			else if (type == TypeManager.double_type)
				retval = new DoubleConstant ((double) constant_value, loc);
			else if (type == TypeManager.string_type)
				retval = new StringConstant ((string) constant_value, loc);
			else if (type == TypeManager.short_type)
				retval = new ShortConstant ((short) constant_value, loc);
			else if (type == TypeManager.ushort_type)
				retval = new UShortConstant ((ushort) constant_value, loc);
			else if (type == TypeManager.sbyte_type)
				retval = new SByteConstant ((sbyte) constant_value, loc);
			else if (type == TypeManager.byte_type)
				retval = new ByteConstant ((byte) constant_value, loc);
			else if (type == TypeManager.char_type)
				retval = new CharConstant ((char) constant_value, loc);
			else if (type == TypeManager.bool_type)
				retval = new BoolConstant ((bool) constant_value, loc);
			else if (type == TypeManager.decimal_type)
				retval = new DecimalConstant ((decimal) constant_value, loc);
			else
				throw new Exception ("LookupConstantValue: Unhandled constant type: " + type);
			
			return retval;
		}

		protected void CheckRange (EmitContext ec, ulong value, Type type, ulong max)
		{
			if (!ec.ConstantCheckState)
				return;

			if (value > max)
				throw new OverflowException ();
		}

		protected void CheckRange (EmitContext ec, double value, Type type, long min, long max)
		{
			if (!ec.ConstantCheckState)
				return;

			if (((value < min) || (value > max)))
				throw new OverflowException ();
		}

		protected void CheckUnsigned (EmitContext ec, long value, Type type)
		{
			if (!ec.ConstantCheckState)
				return;

			if (value < 0)
				throw new OverflowException ();
		}

		public abstract Constant Reduce (EmitContext ec, Type target_type);

		/// <summary>
		///   Attempts to do a compile-time folding of a constant cast.
		/// </summary>
		public Constant TryReduce (EmitContext ec, Type target_type, Location loc)
		{
			try {
				return  TryReduce (ec, target_type);
			}
			catch (OverflowException) {
				if (ec.ConstantCheckState) {
					Report.Error (221, loc, "Constant value `{0}' cannot be converted to a `{1}' (use `unchecked' syntax to override)",
						GetValue ().ToString (), TypeManager.CSharpName (target_type));
				}
				return null;
			}
		}

		Constant TryReduce (EmitContext ec, Type target_type)
		{
			if (Type == target_type)
				return this;

			if (TypeManager.IsEnumType (target_type)) {
				Constant c = TryReduce (ec, TypeManager.EnumToUnderlying (target_type));
				if (c == null)
					return null;

				return new EnumConstant (c, target_type);
			}

			return Reduce (ec, target_type);
		}


		public virtual DecimalConstant ConvertToDecimal ()
		{
			return null;
		}
		
		public virtual DoubleConstant ConvertToDouble ()
		{
			return null;
		}

		public virtual FloatConstant ConvertToFloat ()
		{
			return null;
		}

		public virtual ULongConstant ConvertToULong ()
		{
			return null;
		}

		public virtual LongConstant ConvertToLong ()
		{
			return null;
		}

		public virtual UIntConstant ConvertToUInt ()
		{
			return null;
		}

		public virtual IntConstant ConvertToInt ()
		{
			return null;
		}

		public abstract Constant Increment ();
		
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

		public override Constant Reduce (EmitContext ec, Type target_type)
		{
			return null;
		}

	}

	public class ByteConstant : Constant {
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

		public override DoubleConstant ConvertToDouble ()
		{
			return new DoubleConstant (Value, loc);
		}

		public override FloatConstant ConvertToFloat ()
		{
			return new FloatConstant (Value, loc);
		}

		public override ULongConstant ConvertToULong ()
		{
			return new ULongConstant (Value, loc);
		}

		public override LongConstant ConvertToLong ()
		{
			return new LongConstant (Value, loc);
		}

		public override UIntConstant ConvertToUInt ()
		{
			return new UIntConstant (Value, loc);
		}

		public override IntConstant ConvertToInt ()
		{
			return new IntConstant (Value, loc);
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

		public override Constant Reduce (EmitContext ec, Type target_type)
		{
			if (target_type == TypeManager.sbyte_type) {
				CheckRange (ec, Value, target_type, SByte.MinValue, SByte.MaxValue);
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

		public override DoubleConstant ConvertToDouble ()
		{
			return new DoubleConstant (Value, loc);
		}

		public override FloatConstant ConvertToFloat ()
		{
			return new FloatConstant (Value, loc);
		}

		public override ULongConstant ConvertToULong ()
		{
			return new ULongConstant (Value, loc);
		}

		public override LongConstant ConvertToLong ()
		{
			return new LongConstant (Value, loc);
		}

		public override UIntConstant ConvertToUInt ()
		{
			return new UIntConstant (Value, loc);
		}

		public override IntConstant ConvertToInt ()
		{
			return new IntConstant (Value, loc);
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

		public override Constant Reduce (EmitContext ec, Type target_type)
		{
			if (target_type == TypeManager.byte_type) {
				CheckRange (ec, Value, target_type, Byte.MinValue, Byte.MaxValue);
				return new ByteConstant ((byte) Value, Location);
			}
			if (target_type == TypeManager.sbyte_type) {
				CheckRange (ec, Value, target_type, SByte.MinValue, SByte.MaxValue);
				return new SByteConstant ((sbyte) Value, Location);
			}
			if (target_type == TypeManager.short_type) {
				CheckRange (ec, Value, target_type, Int16.MinValue, Int16.MaxValue);
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

	public class SByteConstant : Constant {
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

		public override DoubleConstant ConvertToDouble ()
		{
			return new DoubleConstant (Value, loc);
		}

		public override FloatConstant ConvertToFloat ()
		{
			return new FloatConstant (Value, loc);
		}

		public override ULongConstant ConvertToULong ()
		{
			if (Value >= 0)
				return new ULongConstant ((ulong) Value, loc);
			
			return null;
		}

		public override LongConstant ConvertToLong ()
		{
			return new LongConstant (Value, loc);
		}

		public override UIntConstant ConvertToUInt ()
		{
			return null;
		}

		public override IntConstant ConvertToInt ()
		{
			return new IntConstant (Value, loc);
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

		public override Constant Reduce (EmitContext ec, Type target_type)
		{
			if (target_type == TypeManager.byte_type) {
				CheckUnsigned (ec, Value, target_type);
				return new ByteConstant ((byte) Value, Location);
			}
			if (target_type == TypeManager.short_type)
				return new ShortConstant ((short) Value, Location);
			if (target_type == TypeManager.ushort_type) {
				CheckUnsigned (ec, Value, target_type);
				return new UShortConstant ((ushort) Value, Location);
			} if (target_type == TypeManager.int32_type)
				  return new IntConstant ((int) Value, Location);
			if (target_type == TypeManager.uint32_type) {
				CheckUnsigned (ec, Value, target_type);
				return new UIntConstant ((uint) Value, Location);
			} if (target_type == TypeManager.int64_type)
				  return new LongConstant ((long) Value, Location);
			if (target_type == TypeManager.uint64_type) {
				CheckUnsigned (ec, Value, target_type);
				return new ULongConstant ((ulong) Value, Location);
			}
			if (target_type == TypeManager.float_type)
				return new FloatConstant ((float) Value, Location);
			if (target_type == TypeManager.double_type)
				return new DoubleConstant ((double) Value, Location);
			if (target_type == TypeManager.char_type) {
				CheckUnsigned (ec, Value, target_type);
				return new CharConstant ((char) Value, Location);
			}
			if (target_type == TypeManager.decimal_type)
				return new DecimalConstant ((decimal) Value, Location);

			return null;
		}

	}

	public class ShortConstant : Constant {
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

		public override DoubleConstant ConvertToDouble ()
		{
			return new DoubleConstant (Value, loc);
		}

		public override FloatConstant ConvertToFloat ()
		{
			return new FloatConstant (Value, loc);
		}

		public override ULongConstant ConvertToULong ()
		{
			return null;
		}

		public override LongConstant ConvertToLong ()
		{
			return new LongConstant (Value, loc);
		}

		public override UIntConstant ConvertToUInt ()
		{
			return null;
		}

		public override IntConstant ConvertToInt ()
		{
			return new IntConstant (Value, loc);
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

		public override Constant Reduce (EmitContext ec, Type target_type)
		{
			if (target_type == TypeManager.byte_type) {
				CheckRange (ec, Value, target_type, Byte.MinValue, Byte.MaxValue);
				return new ByteConstant ((byte) Value, Location);
			}
			if (target_type == TypeManager.sbyte_type) {
				CheckRange (ec, Value, target_type, SByte.MinValue, SByte.MaxValue);
				return new SByteConstant ((sbyte) Value, Location);
			}
			if (target_type == TypeManager.ushort_type) {
				CheckUnsigned (ec, Value, target_type);
				return new UShortConstant ((ushort) Value, Location);
			}
			if (target_type == TypeManager.int32_type)
				return new IntConstant ((int) Value, Location);
			if (target_type == TypeManager.uint32_type) {
				CheckUnsigned (ec, Value, target_type);
				return new UIntConstant ((uint) Value, Location);
			}
			if (target_type == TypeManager.int64_type)
				return new LongConstant ((long) Value, Location);
			if (target_type == TypeManager.uint64_type) {
				CheckUnsigned (ec, Value, target_type);
				return new ULongConstant ((ulong) Value, Location);
			}
			if (target_type == TypeManager.float_type)
				return new FloatConstant ((float) Value, Location);
			if (target_type == TypeManager.double_type)
				return new DoubleConstant ((double) Value, Location);
			if (target_type == TypeManager.char_type) {
				CheckRange (ec, Value, target_type, Char.MinValue, Char.MaxValue);
				return new CharConstant ((char) Value, Location);
			}
			if (target_type == TypeManager.decimal_type)
				return new DecimalConstant ((decimal) Value, Location);

			return null;
		}

	}

	public class UShortConstant : Constant {
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

		public override DoubleConstant ConvertToDouble ()
		{
			return new DoubleConstant (Value, loc);
		}

		public override FloatConstant ConvertToFloat ()
		{
			return new FloatConstant (Value, loc);
		}

		public override ULongConstant ConvertToULong ()
		{
			return new ULongConstant (Value, loc);
		}

		public override LongConstant ConvertToLong ()
		{
			return new LongConstant (Value, loc);
		}

		public override UIntConstant ConvertToUInt ()
		{
			return new UIntConstant (Value, loc);
		}

		public override IntConstant ConvertToInt ()
		{
			return new IntConstant (Value, loc);
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

		public override Constant Reduce (EmitContext ec, Type target_type)
		{
			if (target_type == TypeManager.byte_type) {
				CheckRange (ec, Value, target_type, Byte.MinValue, Byte.MaxValue);
				return new ByteConstant ((byte) Value, Location);
			}
			if (target_type == TypeManager.sbyte_type) {
				CheckRange (ec, Value, target_type, SByte.MinValue, SByte.MaxValue);
				return new SByteConstant ((sbyte) Value, Location);
			}
			if (target_type == TypeManager.short_type) {
				CheckRange (ec, Value, target_type, Int16.MinValue, Int16.MaxValue);
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
				CheckRange (ec, Value, target_type, Char.MinValue, Char.MaxValue);
				return new CharConstant ((char) Value, Location);
			}
			if (target_type == TypeManager.decimal_type)
				return new DecimalConstant ((decimal) Value, Location);

			return null;
		}
	}

	public class IntConstant : Constant {
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

		public override DecimalConstant ConvertToDecimal()
		{
			return new DecimalConstant (Value, loc);
		}

		public override DoubleConstant ConvertToDouble ()
		{
			return new DoubleConstant (Value, loc);
		}

		public override FloatConstant ConvertToFloat ()
		{
			return new FloatConstant (Value, loc);
		}

		public override ULongConstant ConvertToULong ()
		{
			if (Value < 0)
				return null;

			return new ULongConstant ((ulong) Value, loc);
		}

		public override LongConstant ConvertToLong ()
		{
			return new LongConstant (Value, loc);
		}

		public override UIntConstant ConvertToUInt ()
		{
			if (Value < 0)
				return null;

			return new UIntConstant ((uint) Value, loc);
		}

		public override IntConstant ConvertToInt ()
		{
			return this;
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

		public override Constant Reduce (EmitContext ec, Type target_type)
		{
			if (target_type == TypeManager.byte_type) {
				CheckRange (ec, Value, target_type, Byte.MinValue, Byte.MaxValue);
				return new ByteConstant ((byte) Value, Location);
			}
			if (target_type == TypeManager.sbyte_type) {
				CheckRange (ec, Value, target_type, SByte.MinValue, SByte.MaxValue);
				return new SByteConstant ((sbyte) Value, Location);
			}
			if (target_type == TypeManager.short_type) {
				CheckRange (ec, Value, target_type, Int16.MinValue, Int16.MaxValue);
				return new ShortConstant ((short) Value, Location);
			}
			if (target_type == TypeManager.ushort_type) {
				CheckRange (ec, Value, target_type, UInt16.MinValue, UInt16.MaxValue);
				return new UShortConstant ((ushort) Value, Location);
			}
			if (target_type == TypeManager.uint32_type) {
				CheckRange (ec, Value, target_type, Int32.MinValue, Int32.MaxValue);
				return new UIntConstant ((uint) Value, Location);
			}
			if (target_type == TypeManager.int64_type)
				return new LongConstant ((long) Value, Location);
			if (target_type == TypeManager.uint64_type) {
				CheckUnsigned (ec, Value, target_type);
				return new ULongConstant ((ulong) Value, Location);
			}
			if (target_type == TypeManager.float_type)
				return new FloatConstant ((float) Value, Location);
			if (target_type == TypeManager.double_type)
				return new DoubleConstant ((double) Value, Location);
			if (target_type == TypeManager.char_type) {
				CheckRange (ec, Value, target_type, Char.MinValue, Char.MaxValue);
				return new CharConstant ((char) Value, Location);
			}
			if (target_type == TypeManager.decimal_type)
				return new DecimalConstant ((decimal) Value, Location);

			return null;
		}
	}

	public class UIntConstant : Constant {
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

		public override DoubleConstant ConvertToDouble ()
		{
			return new DoubleConstant (Value, loc);
		}

		public override FloatConstant ConvertToFloat ()
		{
			return new FloatConstant (Value, loc);
		}

		public override ULongConstant ConvertToULong ()
		{
			return new ULongConstant (Value, loc);
		}

		public override LongConstant ConvertToLong ()
		{
			return new LongConstant (Value, loc);
		}

		public override UIntConstant ConvertToUInt ()
		{
			return this;
		}

		public override IntConstant ConvertToInt ()
		{
			return null;
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

		public override Constant Reduce (EmitContext ec, Type target_type)
		{
			if (target_type == TypeManager.byte_type) {
				CheckRange (ec, Value, target_type, Char.MinValue, Char.MaxValue);
				return new ByteConstant ((byte) Value, Location);
			}
			if (target_type == TypeManager.sbyte_type) {
				CheckRange (ec, Value, target_type, SByte.MinValue, SByte.MaxValue);
				return new SByteConstant ((sbyte) Value, Location);
			}
			if (target_type == TypeManager.short_type) {
				CheckRange (ec, Value, target_type, Int16.MinValue, Int16.MaxValue);
				return new ShortConstant ((short) Value, Location);
			}
			if (target_type == TypeManager.ushort_type) {
				CheckRange (ec, Value, target_type, UInt16.MinValue, UInt16.MaxValue);
				return new UShortConstant ((ushort) Value, Location);
			}
			if (target_type == TypeManager.int32_type) {
				CheckRange (ec, Value, target_type, Int32.MinValue, Int32.MaxValue);
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
				CheckRange (ec, Value, target_type, Char.MinValue, Char.MaxValue);
				return new CharConstant ((char) Value, Location);
			}
			if (target_type == TypeManager.decimal_type)
				return new DecimalConstant ((decimal) Value, Location);

			return null;
		}

	}

	public class LongConstant : Constant {
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
			ILGenerator ig = ec.ig;

			EmitLong (ig, Value);
		}

		static public void EmitLong (ILGenerator ig, long l)
		{
			if ((l >> 32) == 0){
				IntLiteral.EmitInt (ig, unchecked ((int) l));
				ig.Emit (OpCodes.Conv_U8);
			} else {
				ig.Emit (OpCodes.Ldc_I8, l);
			}
		}

		public override string AsString ()
		{
			return Value.ToString ();
		}

		public override object GetValue ()
		{
			return Value;
		}

		public override DoubleConstant ConvertToDouble ()
		{
			return new DoubleConstant (Value, loc);
		}

		public override FloatConstant ConvertToFloat ()
		{
			return new FloatConstant (Value, loc);
		}

		public override ULongConstant ConvertToULong ()
		{
			if (Value < 0)
				return null;
			
			return new ULongConstant ((ulong) Value, loc);
		}

		public override LongConstant ConvertToLong ()
		{
			return this;
		}

		public override UIntConstant ConvertToUInt ()
		{
			return null;
		}

		public override IntConstant ConvertToInt ()
		{
			return null;
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

		public override Constant Reduce (EmitContext ec, Type target_type)
		{
			if (target_type == TypeManager.byte_type) {
				CheckRange (ec, Value, target_type, Byte.MinValue, Byte.MaxValue);
				return new ByteConstant ((byte) Value, Location);
			}
			if (target_type == TypeManager.sbyte_type) {
				CheckRange (ec, Value, target_type, SByte.MinValue, SByte.MaxValue);
				return new SByteConstant ((sbyte) Value, Location);
			}
			if (target_type == TypeManager.short_type) {
				CheckRange (ec, Value, target_type, Int16.MinValue, Int16.MaxValue);
				return new ShortConstant ((short) Value, Location);
			}
			if (target_type == TypeManager.ushort_type) {
				CheckRange (ec, Value, target_type, UInt16.MinValue, UInt16.MaxValue);
				return new UShortConstant ((ushort) Value, Location);
			}
			if (target_type == TypeManager.int32_type) {
				CheckRange (ec, Value, target_type, Int32.MinValue, Int32.MaxValue);
				return new IntConstant ((int) Value, Location);
			}
			if (target_type == TypeManager.uint32_type) {
				CheckRange (ec, Value, target_type, UInt32.MinValue, UInt32.MaxValue);
				return new UIntConstant ((uint) Value, Location);
			}
			if (target_type == TypeManager.uint64_type) {
				CheckUnsigned (ec, Value, target_type);
				return new ULongConstant ((ulong) Value, Location);
			}
			if (target_type == TypeManager.float_type)
				return new FloatConstant ((float) Value, Location);
			if (target_type == TypeManager.double_type)
				return new DoubleConstant ((double) Value, Location);
			if (target_type == TypeManager.char_type) {
				CheckRange (ec, Value, target_type, Char.MinValue, Char.MaxValue);
				return new CharConstant ((char) Value, Location);
			}
			if (target_type == TypeManager.decimal_type)
				return new DecimalConstant ((decimal) Value, Location);

			return null;
		}

	}

	public class ULongConstant : Constant {
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

		public override DoubleConstant ConvertToDouble ()
		{
			return new DoubleConstant (Value, loc);
		}

		public override FloatConstant ConvertToFloat ()
		{
			return new FloatConstant (Value, loc);
		}

		public override ULongConstant ConvertToULong ()
		{
			return this;
		}

		public override LongConstant ConvertToLong ()
		{
			return null;
		}

		public override UIntConstant ConvertToUInt ()
		{
			return null;
		}

		public override IntConstant ConvertToInt ()
		{
			return null;
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

		public override Constant Reduce (EmitContext ec, Type target_type)
		{
			if (target_type == TypeManager.byte_type) {
				CheckRange (ec, Value, target_type, Byte.MaxValue);
				return new ByteConstant ((byte) Value, Location);
			}
			if (target_type == TypeManager.sbyte_type) {
				CheckRange (ec, Value, target_type, (ulong) SByte.MaxValue);
				return new SByteConstant ((sbyte) Value, Location);
			}
			if (target_type == TypeManager.short_type) {
				CheckRange (ec, Value, target_type, (ulong) Int16.MaxValue);
				return new ShortConstant ((short) Value, Location);
			}
			if (target_type == TypeManager.ushort_type) {
				CheckRange (ec, Value, target_type, UInt16.MaxValue);
				return new UShortConstant ((ushort) Value, Location);
			}
			if (target_type == TypeManager.int32_type) {
				CheckRange (ec, Value, target_type, Int32.MaxValue);
				return new IntConstant ((int) Value, Location);
			}
			if (target_type == TypeManager.uint32_type) {
				CheckRange (ec, Value, target_type, UInt32.MaxValue);
				return new UIntConstant ((uint) Value, Location);
			}
			if (target_type == TypeManager.int64_type) {
				CheckRange (ec, Value, target_type, (ulong) Int64.MaxValue);
				return new LongConstant ((long) Value, Location);
			}
			if (target_type == TypeManager.float_type)
				return new FloatConstant ((float) Value, Location);
			if (target_type == TypeManager.double_type)
				return new DoubleConstant ((double) Value, Location);
			if (target_type == TypeManager.char_type) {
				CheckRange (ec, Value, target_type, Char.MaxValue);
				return new CharConstant ((char) Value, Location);
			}
			if (target_type == TypeManager.decimal_type)
				return new DecimalConstant ((decimal) Value, Location);

			return null;
		}

	}

	public class FloatConstant : Constant {
		public readonly float Value;

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

		public override DoubleConstant ConvertToDouble ()
		{
			return new DoubleConstant (Value, loc);
		}

		public override FloatConstant ConvertToFloat ()
		{
			return this;
		}

		public override LongConstant ConvertToLong ()
		{
			return null;
		}

		public override UIntConstant ConvertToUInt ()
		{
			return null;
		}

		public override IntConstant ConvertToInt ()
		{
			return null;
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

		public override Constant Reduce (EmitContext ec, Type target_type)
		{
			if (target_type == TypeManager.byte_type)
				return new ByteConstant ((byte) Value, Location);
			if (target_type == TypeManager.sbyte_type)
				return new SByteConstant ((sbyte) Value, Location);
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
			if (target_type == TypeManager.double_type)
				return new DoubleConstant ((double) Value, Location);
			if (target_type == TypeManager.char_type)
				return new CharConstant ((char) Value, Location);
			if (target_type == TypeManager.decimal_type)
				return new DecimalConstant ((decimal) Value, Location);

			return null;
		}

	}

	public class DoubleConstant : Constant {
		public readonly double Value;

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

		public override DoubleConstant ConvertToDouble ()
		{
			return this;
		}

		public override FloatConstant ConvertToFloat ()
		{
			return null;
		}

		public override ULongConstant ConvertToULong ()
		{
			return null;
		}

		public override LongConstant ConvertToLong ()
		{
			return null;
		}

		public override UIntConstant ConvertToUInt ()
		{
			return null;
		}

		public override IntConstant ConvertToInt ()
		{
			return null;
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

		public override Constant Reduce (EmitContext ec, Type target_type)
		{
			if (target_type == TypeManager.byte_type) {
				CheckRange (ec, Value, target_type, Byte.MinValue, Byte.MaxValue);
				return new ByteConstant ((byte) Value, Location);
			}
			if (target_type == TypeManager.sbyte_type) {
				CheckRange (ec, Value, target_type, SByte.MinValue, SByte.MaxValue);
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
			if (target_type == TypeManager.char_type) {
				CheckRange (ec, Value, target_type, char.MinValue, char.MaxValue);
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

		public override Constant Reduce (EmitContext ec, Type target_type)
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

		public override Constant Reduce (EmitContext ec, Type target_type)
		{
			return null;
		}
	}

}


