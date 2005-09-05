//
// constant.cs: Constants.
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
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

		public override void Error_ValueCannotBeConverted (Location loc, Type t)
		{
			// string is not real constant
			if (type == TypeManager.string_type)
				base.Error_ValueCannotBeConverted (loc, t);
			else
				Report.Error (31, loc, "Constant value `{0}' cannot be converted to a `{1}'",
					AsString (), TypeManager.CSharpName (t));
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
				Convert.Error_CannotImplicitConversion (loc, Type, TypeManager.double_type);

			return c;
		}

		public FloatConstant ToFloat (Location loc)
		{
			FloatConstant c = ConvertToFloat ();

			if (c == null)
				Convert.Error_CannotImplicitConversion (loc, Type, TypeManager.float_type);

			return c;
		}

		public ULongConstant ToULong (Location loc)
		{
			ULongConstant c = ConvertToULong ();

			if (c == null)
				Convert.Error_CannotImplicitConversion (loc, Type, TypeManager.uint64_type);

			return c;
		}

		public LongConstant ToLong (Location loc)
		{
			LongConstant c = ConvertToLong ();

			if (c == null)
				Convert.Error_CannotImplicitConversion (loc, Type, TypeManager.int64_type);

			return c;
		}
		
		public UIntConstant ToUInt (Location loc)
		{
			UIntConstant c = ConvertToUInt ();

			if (c == null)
				Convert.Error_CannotImplicitConversion (loc, Type, TypeManager.uint32_type);

			return c;
		}

		public IntConstant ToInt (Location loc)
		{
			IntConstant c = ConvertToInt ();

			if (c == null)
				Convert.Error_CannotImplicitConversion (loc, Type, TypeManager.int32_type);

			return c;
		}

		public DecimalConstant ToDecimal (Location loc)
		{
			DecimalConstant c = ConvertToDecimal ();

			if (c == null)
				Convert.Error_CannotConvertType (loc, Type, TypeManager.decimal_type);

			return c;
		}

		public virtual Constant ToType (Type type, Location loc)
		{
			if (Type == type)
				return this;

			if (type == TypeManager.object_type)
				return this;

			if (!Convert.ImplicitStandardConversionExists (Convert.ConstantEC, this, type)){
				Error_ValueCannotBeConverted (loc, type);
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
				Convert.Error_CannotImplicitConversion (loc, Type, type);
				
				//
				// We should always catch the error before this is ever
				// reached, by calling Convert.ImplicitStandardConversionExists
				//
				throw new Exception (
					String.Format ("LookupConstantValue: This should never be reached {0} {1}", Type, type));
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
		
		public BoolConstant (bool val)
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
	}

	public class ByteConstant : Constant {
		public readonly byte Value;

		public ByteConstant (byte v)
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
			return new DoubleConstant (Value);
		}

		public override FloatConstant ConvertToFloat ()
		{
			return new FloatConstant (Value);
		}

		public override ULongConstant ConvertToULong ()
		{
			return new ULongConstant (Value);
		}

		public override LongConstant ConvertToLong ()
		{
			return new LongConstant (Value);
		}

		public override UIntConstant ConvertToUInt ()
		{
			return new UIntConstant (Value);
		}

		public override IntConstant ConvertToInt ()
		{
			return new IntConstant (Value);
		}
		
		public override Constant Increment ()
		{
			return new ByteConstant (checked ((byte)(Value + 1)));
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
	}

	public class CharConstant : Constant {
		public readonly char Value;

		public CharConstant (char v)
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
			return new DoubleConstant (Value);
		}

		public override FloatConstant ConvertToFloat ()
		{
			return new FloatConstant (Value);
		}

		public override ULongConstant ConvertToULong ()
		{
			return new ULongConstant (Value);
		}

		public override LongConstant ConvertToLong ()
		{
			return new LongConstant (Value);
		}

		public override UIntConstant ConvertToUInt ()
		{
			return new UIntConstant (Value);
		}

		public override IntConstant ConvertToInt ()
		{
			return new IntConstant (Value);
		}

		public override Constant Increment ()
		{
			return new CharConstant (checked ((char)(Value + 1)));
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
	}

	public class SByteConstant : Constant {
		public readonly sbyte Value;

		public SByteConstant (sbyte v)
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
			return new DoubleConstant (Value);
		}

		public override FloatConstant ConvertToFloat ()
		{
			return new FloatConstant (Value);
		}

		public override ULongConstant ConvertToULong ()
		{
			if (Value >= 0)
				return new ULongConstant ((ulong) Value);
			
			return null;
		}

		public override LongConstant ConvertToLong ()
		{
			return new LongConstant (Value);
		}

		public override UIntConstant ConvertToUInt ()
		{
			return null;
		}

		public override IntConstant ConvertToInt ()
		{
			return new IntConstant (Value);
		}
		
		public override Constant Increment ()
		{
		    return new SByteConstant (checked((sbyte)(Value + 1)));
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
	}

	public class ShortConstant : Constant {
		public readonly short Value;

		public ShortConstant (short v)
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
			return new DoubleConstant (Value);
		}

		public override FloatConstant ConvertToFloat ()
		{
			return new FloatConstant (Value);
		}

		public override ULongConstant ConvertToULong ()
		{
			return null;
		}

		public override LongConstant ConvertToLong ()
		{
			return new LongConstant (Value);
		}

		public override UIntConstant ConvertToUInt ()
		{
			return null;
		}

		public override IntConstant ConvertToInt ()
		{
			return new IntConstant (Value);
		}

		public override Constant Increment ()
		{
			return new ShortConstant (checked((short)(Value + 1)));
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
	}

	public class UShortConstant : Constant {
		public readonly ushort Value;

		public UShortConstant (ushort v)
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
			return new DoubleConstant (Value);
		}

		public override FloatConstant ConvertToFloat ()
		{
			return new FloatConstant (Value);
		}

		public override ULongConstant ConvertToULong ()
		{
			return new ULongConstant (Value);
		}

		public override LongConstant ConvertToLong ()
		{
			return new LongConstant (Value);
		}

		public override UIntConstant ConvertToUInt ()
		{
			return new UIntConstant (Value);
		}

		public override IntConstant ConvertToInt ()
		{
			return new IntConstant (Value);
		}
		
		public override Constant Increment ()
		{
			return new UShortConstant (checked((ushort)(Value + 1)));
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
	}

	public class IntConstant : Constant {
		public readonly int Value;

		public IntConstant (int v)
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
			return new DecimalConstant (Value);
		}

		public override DoubleConstant ConvertToDouble ()
		{
			return new DoubleConstant (Value);
		}

		public override FloatConstant ConvertToFloat ()
		{
			return new FloatConstant (Value);
		}

		public override ULongConstant ConvertToULong ()
		{
			if (Value < 0)
				return null;

			return new ULongConstant ((ulong) Value);
		}

		public override LongConstant ConvertToLong ()
		{
			return new LongConstant (Value);
		}

		public override UIntConstant ConvertToUInt ()
		{
			if (Value < 0)
				return null;

			return new UIntConstant ((uint) Value);
		}

		public override IntConstant ConvertToInt ()
		{
			return this;
		}

		public override Constant Increment ()
		{
			return new IntConstant (checked(Value + 1));
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
	}

	public class UIntConstant : Constant {
		public readonly uint Value;

		public UIntConstant (uint v)
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
			return new DoubleConstant (Value);
		}

		public override FloatConstant ConvertToFloat ()
		{
			return new FloatConstant (Value);
		}

		public override ULongConstant ConvertToULong ()
		{
			return new ULongConstant (Value);
		}

		public override LongConstant ConvertToLong ()
		{
			return new LongConstant (Value);
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
			return new UIntConstant (checked(Value + 1));
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
	}

	public class LongConstant : Constant {
		public readonly long Value;

		public LongConstant (long v)
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
			return new DoubleConstant (Value);
		}

		public override FloatConstant ConvertToFloat ()
		{
			return new FloatConstant (Value);
		}

		public override ULongConstant ConvertToULong ()
		{
			if (Value < 0)
				return null;
			
			return new ULongConstant ((ulong) Value);
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
			return new LongConstant (checked(Value + 1));
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
	}

	public class ULongConstant : Constant {
		public readonly ulong Value;

		public ULongConstant (ulong v)
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
			return new DoubleConstant (Value);
		}

		public override FloatConstant ConvertToFloat ()
		{
			return new FloatConstant (Value);
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
			return new ULongConstant (checked(Value + 1));
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
	}

	public class FloatConstant : Constant {
		public readonly float Value;

		public FloatConstant (float v)
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
			return new DoubleConstant (Value);
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
			return new FloatConstant (checked(Value + 1));
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
	}

	public class DoubleConstant : Constant {
		public readonly double Value;

		public DoubleConstant (double v)
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
			return new DoubleConstant (checked(Value + 1));
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
	}

	public class DecimalConstant : Constant {
		public readonly decimal Value;

		public DecimalConstant (decimal d)
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
			return new DecimalConstant (checked (Value + 1));
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
	}

	public class StringConstant : Constant {
		public readonly string Value;

		public StringConstant (string s)
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
	}

}


