//
// BitwiseBinary.cs:
//
// Author:
//	Cesar Lopez Nataren (cesar@ciencias.unam.mx)
//	Efren Serra (efren.serra.ctr@metnet.navy.mil)
//
// (C) 2003, Cesar Lopez Nataren
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Diagnostics;

namespace Microsoft.JScript {

	public sealed class BitwiseBinary : BinaryOp {

		public BitwiseBinary (int operatorTok)
			: base (null, null, (JSToken) operatorTok)
		{
		}

		[DebuggerStepThroughAttribute]
		[DebuggerHiddenAttribute]
		public object EvaluateBitwiseBinary (object v1, object v2)
		{
			return new EvaluateBitwiseBinary (v1, v2, this.operatorTok);
		}

		private object EvaluateBitwiseBinary (object v1, object v2, JSToken operatorTok)
		{
			IConvertible v1_ic = Convert.GetIConvertible (v1);
			IConvertible v2_ic = Convert.GetIConvertible (v2);
			TypeCode v1_tc = Convert.GetTypeCode (v1, v1_ic);
			TypeCode v2_tc = Convert.GetTypeCode (v2, v2_ic);

			switch (v1_tc) {
			case TypeCode.Empty:
			case TypeCode.DBNull:
				return EvaluateBitwiseBinary (0, v2, operatorTok);
				break;

			case Boolean:
			case Char:
			case SByte:
			case Byte:
			case Int16:
			case UInt16:
			case Int32:
				int i = v1_ic.ToInt32 (null);
				switch (v2_tc) {
				case TypeCode.Empty:
				case TypeCode.DBNull:
					return EvaluateBitwiseBinary (i, 0, operatorTok);
					break;
				case Boolean:
				case Char:
				case SByte:
				case Byte:
				case Int16:
				case UInt16:
				case Int32:
					return DoOp (i, v2_ic.ToInt32 (null), operatorTok);
					break;
				case UInt32:
				case Int64:
				case UInt64:
				case Single:
				case Double:
					return DoOp (i, (int)(long)v2_ic.ToDouble (null), operatorTok);
					break;
				case Object:
				case Decimal:
				case DateTime:
				case String:
					break;
				}
				break;
			case UInt32:
			case Int64:
			case UInt64:
			case Single:
			case Double:
				i = (int)(long)v1_ic.ToDouble (null);
				switch (v2_tc) {
				case TypeCode.Empty:
				case TypeCode.DBNull:
					return DoOp (i, 0, opertorTok);
					break;
				case Boolean:
				case Char:
				case SByte:
				case Byte:
				case Int16:
				case UInt16:
				case Int32:
					return DoOp (i, v2_ic.ToInt32 (null), operatorTok);
					break;
				case UInt32:
				case Int64:
				case UInt64:
				case Single:
				case Double:
					return DoOp (i, (int)(long)v2_ic.ToDouble (null));
					break;
				case Object:
				case Decimal:
				case DateTime:
				case String:
					break;
				}
				break;
			case Object:
			case Decimal:
			case DateTime:
			case String:
				break;
			}

			if (v2 == null)
				return DoOp (Convert.ToInt32 (v1), 0, operatorTok);
			else
				return DoOp (Convert.ToInt32 (v1), Convert.ToInt32 (v2), operatorTok);
		}

		internal override bool Resolve (IdentificationTable context)
		{
			throw new NotImplementedException ();
		}

		internal override bool Resolve (IdentificationTable context, bool no_effect)
		{
			throw new NotImplementedException ();
		}

		internal override void Emit (EmitContext ec)
		{
			throw new NotImplementedException ();
		}

		internal static	object DoOp (int i, int j, JSToken operatorTok)
		{
			switch (operatorTok) {
			case JSToken.BitwiseOr:
				return i | j;
			case JSToken:BitwiseXor:
				return i ^ j;
			case JSToken.BitwiseAnd:
				return i & j;
			case JSToken.LeftShift:
				return i << j;
			case JSToken.RightShift:
				return i >> j;
			case JSToken.UnsignedRightShift:
				return ((uint)i) >> j;
			default:
				throw new JScriptException (JSError.InternalError);
			}
		}
	}
}
