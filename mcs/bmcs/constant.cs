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
				Convert.Error_CannotWideningConversion (loc, Type, TypeManager.double_type);

			return c;
		}

		public FloatConstant ToFloat (Location loc)
		{
			FloatConstant c = ConvertToFloat ();

			if (c == null)
				Convert.Error_CannotWideningConversion (loc, Type, TypeManager.float_type);

			return c;
		}

		public ULongConstant ToULong (Location loc)
		{
			ULongConstant c = ConvertToULong ();

			if (c == null)
				Convert.Error_CannotWideningConversion (loc, Type, TypeManager.uint64_type);

			return c;
		}

		public LongConstant ToLong (Location loc)
		{
			LongConstant c = ConvertToLong ();

			if (c == null)
				Convert.Error_CannotWideningConversion (loc, Type, TypeManager.int64_type);

			return c;
		}
		
		public UIntConstant ToUInt (Location loc)
		{
			UIntConstant c = ConvertToUInt ();

			if (c == null)
				Convert.Error_CannotWideningConversion (loc, Type, TypeManager.uint32_type);

			return c;
		}

		public IntConstant ToInt (Location loc)
		{
			IntConstant c = ConvertToInt ();

			if (c == null)
				Convert.Error_CannotWideningConversion (loc, Type, TypeManager.int32_type);

			return c;
		}

		public DecimalConstant ToDecimal (Location loc)
		{
			DecimalConstant c = ConvertToDecimal ();

			if (c == null)
				Convert.Error_CannotConvertType (loc, Type, TypeManager.decimal_type);

			return c;
		}

		public ShortConstant ToShort (Location loc)
		{
			ShortConstant c = ConvertToShort ();

			if (c == null)
				Convert.Error_CannotConvertType (loc, Type, TypeManager.short_type);

			return c;
		}

		public ByteConstant ToByte (Location loc)
		{
			ByteConstant c = ConvertToByte ();

			if (c == null)
				Convert.Error_CannotConvertType (loc, Type, TypeManager.byte_type);

			return c;
		}

		public BoolConstant ToBoolean (Location loc)
		{
			BoolConstant c = ConvertToBoolean ();

			if (c == null)
				Convert.Error_CannotConvertType (loc, Type, TypeManager.bool_type);

			return c;
		}

		public CharConstant ToChar (Location loc)
		{
			CharConstant c = ConvertToChar ();

			if (c == null)
				Convert.Error_CannotConvertType (loc, Type, TypeManager.char_type);

			return c;
		}

		public virtual CharConstant ConvertToChar ()
		{
			return null;
		}
		
		public virtual BoolConstant ConvertToBoolean ()
		{
			return null;
		}

		public virtual ByteConstant ConvertToByte ()
		{
			return null;
		}

		public virtual ShortConstant ConvertToShort ()
		{
			return null;
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

		public override DecimalConstant ConvertToDecimal ()
		{
			return Value ? new DecimalConstant (0) : new DecimalConstant (-1);
		}
		
		public override DoubleConstant ConvertToDouble ()
		{
			return Value ? new DoubleConstant (0) : new DoubleConstant (-1);
		}

		public override FloatConstant ConvertToFloat ()
		{
			return Value ? new FloatConstant (0) : new FloatConstant (-1);
		}

		public override LongConstant ConvertToLong ()
		{
			return Value ? new LongConstant (0) : new LongConstant (-1);
		}

		public override IntConstant ConvertToInt ()
		{
			return Value ? new IntConstant (0) : new IntConstant (-1);
		}

		public override ShortConstant ConvertToShort ()
		{
			return Value ? new ShortConstant (0) : new ShortConstant (-1);
		}

		public override ByteConstant ConvertToByte ()
		{
			return Value ? new ByteConstant (0) : new ByteConstant (255);
		}

		public override BoolConstant ConvertToBoolean ()
		{
			return this;
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

		public override LongConstant ConvertToLong ()
		{
			return new LongConstant (Value);
		}

		public override ShortConstant ConvertToShort ()
		{
			return new ShortConstant (Value);
		}

		public override ByteConstant ConvertToByte ()
		{
			return this;
		}

		public override DecimalConstant ConvertToDecimal ()
		{
			return new DecimalConstant (Value);
		}

		public override IntConstant ConvertToInt ()
		{
			return new IntConstant (Value);
		}

		public override BoolConstant ConvertToBoolean ()
		{
			return new BoolConstant (Value != 0);
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

		public override LongConstant ConvertToLong ()
		{
			return new LongConstant (Value);
		}

		public override ShortConstant ConvertToShort ()
		{
			return this;
		}

		public override IntConstant ConvertToInt ()
		{
			return new IntConstant (Value);
		}

		public override DecimalConstant ConvertToDecimal ()
		{
			return new DecimalConstant (Value);
		}

		public override ByteConstant ConvertToByte ()
		{
			byte val;
			
			try {
				val = System.Convert.ToByte (Value);
			} catch (Exception ex) {
				return null;
			}

			return new ByteConstant (val);
		}

		public override BoolConstant ConvertToBoolean ()
		{
			return new BoolConstant (Value != 0);
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

		public override LongConstant ConvertToLong ()
		{
			return new LongConstant (Value);
		}

		public override IntConstant ConvertToInt ()
		{
			return this;
		}

		public override ShortConstant ConvertToShort ()
		{
			short val;
			
			try {
				val = System.Convert.ToInt16 (Value);
			} catch (Exception ex) {
				return null;
			}

			return new ShortConstant (val);
		}

		public override ByteConstant ConvertToByte ()
		{
			byte val;
			
			try {
				val = System.Convert.ToByte (Value);
			} catch (Exception ex) {
				return null;
			}

			return new ByteConstant (val);
		}

		public override BoolConstant ConvertToBoolean ()
		{
			return new BoolConstant (Value != 0);
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

		public override DecimalConstant ConvertToDecimal()
		{
			return new DecimalConstant (Value);
		}

		public override LongConstant ConvertToLong ()
		{
			return this;
		}

		public override IntConstant ConvertToInt ()
		{
			int val;
			
			try {
				val = System.Convert.ToInt32 (Value);
			} catch (Exception ex) {
				return null;
			}

			return new IntConstant (val);
		}

		public override ShortConstant ConvertToShort ()
		{
			short val;
			
			try {
				val = System.Convert.ToInt16 (Value);
			} catch (Exception ex) {
				return null;
			}

			return new ShortConstant (val);
		}

		public override ByteConstant ConvertToByte ()
		{
			byte val;
			
			try {
				val = System.Convert.ToByte (Value);
			} catch (Exception ex) {
				return null;
			}

			return new ByteConstant (val);
		}

		public override BoolConstant ConvertToBoolean ()
		{
			return new BoolConstant (Value != 0);
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

		public override DecimalConstant ConvertToDecimal ()
		{
			return new DecimalConstant (new System.Decimal (Value));
		}

		public override LongConstant ConvertToLong ()
		{
			long val;
			
			try {
				val = System.Convert.ToInt64 (System.Math.Round (Value));
			} catch (Exception ex) {
				return null;
			}

			return new LongConstant (val);
			
		}

		public override IntConstant ConvertToInt ()
		{
			int val;
			
			try {
				val = System.Convert.ToInt32 (System.Math.Round (Value));
			} catch (Exception ex) {
				return null;
			}

			return new IntConstant (val);
		}

		public override ShortConstant ConvertToShort ()
		{
			short val;
			
			try {
				val = System.Convert.ToInt16 (System.Math.Round (Value));
			} catch (Exception ex) {
				return null;
			}

			return new ShortConstant (val);
		}

		public override ByteConstant ConvertToByte ()
		{
			byte val;
			
			try {
				val = System.Convert.ToByte (System.Math.Round (Value));
			} catch (Exception ex) {
				return null;
			}

			return new ByteConstant (val);
		}

		public override BoolConstant ConvertToBoolean ()
		{
			return new BoolConstant (Value != 0);
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
			float val;
			
			try {
				val = System.Convert.ToSingle (Value);
			} catch (Exception ex) {
				return null;
			}

			return new FloatConstant (val);
		}

		public override DecimalConstant ConvertToDecimal ()
		{
			return new DecimalConstant (new System.Decimal (Value));
		}

		public override LongConstant ConvertToLong ()
		{
			long val;
			
			try {
				val = System.Convert.ToInt64 (System.Math.Round (Value));
			} catch (Exception ex) {
				return null;
			}

			return new LongConstant (val);
			
		}

		public override IntConstant ConvertToInt ()
		{
			int val;
			
			try {
				val = System.Convert.ToInt32 (System.Math.Round (Value));
			} catch (Exception ex) {
				return null;
			}

			return new IntConstant (val);
		}

		public override ShortConstant ConvertToShort ()
		{
			short val;
			
			try {
				val = System.Convert.ToInt16 (System.Math.Round (Value));
			} catch (Exception ex) {
				return null;
			}

			return new ShortConstant (val);
		}

		public override ByteConstant ConvertToByte ()
		{
			byte val;
			
			try {
				val = System.Convert.ToByte (System.Math.Round (Value));
			} catch (Exception ex) {
				return null;
			}

			return new ByteConstant (val);
		}

		public override BoolConstant ConvertToBoolean ()
		{
			return new BoolConstant (Value != 0);
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

		public override DoubleConstant ConvertToDouble ()
		{
			double val;
			
			try {
				val = System.Convert.ToDouble (Value);
			} catch (Exception ex) {
				return null;
			}

			return new DoubleConstant (val);
		}

		public override FloatConstant ConvertToFloat ()
		{
			float val;
			
			try {
				val = System.Convert.ToSingle (Value);
			} catch (Exception ex) {
				return null;
			}

			return new FloatConstant (val);
		}

		public override DecimalConstant ConvertToDecimal ()
		{
			return this;
		}

		public override LongConstant ConvertToLong ()
		{
			long val;
			
			try {
				val = System.Convert.ToInt64 (Value);
			} catch (Exception ex) {
				return null;
			}

			return new LongConstant (val);
		}

		public override IntConstant ConvertToInt ()
		{
			int val;
			
			try {
				val = System.Convert.ToInt32 (Value);
			} catch (Exception ex) {
				return null;
			}

			return new IntConstant (val);
		}

		public override ShortConstant ConvertToShort ()
		{
			short val;
			
			try {
				val = System.Convert.ToInt16 (Value);
			} catch (Exception ex) {
				return null;
			}

			return new ShortConstant (val);
		}

		public override ByteConstant ConvertToByte ()
		{
			byte val;
			
			try {
				val = System.Convert.ToByte (Value);
			} catch (Exception ex) {
				return null;
			}

			return new ByteConstant (val);
		}

		public override BoolConstant ConvertToBoolean ()
		{
			return new BoolConstant (Value != 0);
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

		public override bool IsNegative {
			get {
				return false;
			}
		}
	}

	//
	// VB.NET specific
	//
	public class DateConstant : Constant {
		public readonly DateTime Value;

		public DateConstant (DateTime s)
		{
			type = TypeManager.date_type;
			eclass = ExprClass.Value;
			Value = s;
		}

		override public string AsString ()
		{
			return "#" + Value.ToString() + "#";
		}

		public override object GetValue ()
		{
			return Value;
		}
		
		public override void Emit (EmitContext ec)
		{
			ec.ig.Emit (OpCodes.Ldc_I8, Value.Ticks);
			ec.ig.Emit (OpCodes.Newobj, TypeManager.void_datetime_ctor_ticks_arg);
		}

		// FIXME:
		public override bool IsNegative {
			get {
				return false;
			}
		}
	}

}


