//
// BinaryExpression.cs
//
// Author:
//   Jb Evain (jbevain@novell.com)
//   Miguel de Icaza (miguel@novell.com)
//
// Contains code from the Mono C# compiler:
//   Marek Safar (marek.safar@seznam.cz)
//   Martin Baulig (martin@ximian.com)
//   Raja Harinath (harinath@gmail.com)
//
// (C) 2001-2003 Ximian, Inc.
// (C) 2004-2008 Novell, Inc. (http://www.novell.com)
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
using System.Reflection;
using System.Reflection.Emit;

namespace System.Linq.Expressions {

	public sealed class BinaryExpression : Expression {

		Expression left;
		Expression right;
		LambdaExpression conversion;
		MethodInfo method;
		bool lift_to_null;

		internal BinaryExpression (ExpressionType node_type, Type type, Expression left, Expression right)
			: base (node_type, type)
		{
			this.left = left;
			this.right = right;
		}

		internal BinaryExpression (ExpressionType node_type, Type type, Expression left, Expression right, MethodInfo method)
			: base (node_type, type)
		{
			this.left = left;
			this.right = right;
			this.method = method;
		}

		internal BinaryExpression (ExpressionType node_type, Type type, Expression left, Expression right, bool lift_to_null,
					   MethodInfo method, LambdaExpression conversion) : base (node_type, type)
		{
			this.left = left;
			this.right = right;
			this.method = method;
			this.lift_to_null = lift_to_null;
			this.conversion = conversion;
		}

		public Expression Left {
			get { return left; }
		}

		public Expression Right {
			get { return right; }
		}

		public MethodInfo Method {
			get { return method; }
		}

		public bool IsLifted {
			get { throw new NotImplementedException (); }
		}

		public bool IsLiftedToNull {
			get { return lift_to_null; }
		}

		static bool IsUnsigned (Type t)
		{
			if (t.IsPointer)
				return IsUnsigned (t.GetElementType ());

			return t == typeof (ushort) || t == typeof (uint) || t == typeof (ulong) || t == typeof (byte);
		}
				
		internal override void Emit (EmitContext ec)
		{
			ILGenerator ig = ec.ig;
                        OpCode opcode;
			
			left.Emit (ec);
			right.Emit (ec);

                        bool is_unsigned = IsUnsigned (left.Type);

			switch (NodeType){
			case ExpressionType.Add:
				opcode = OpCodes.Add;
				break;
				
			case ExpressionType.AddChecked:
				if (left.Type == typeof (int) || left.Type == typeof (long))
					opcode = OpCodes.Add_Ovf;
				else if (is_unsigned)
					opcode = OpCodes.Add_Ovf_Un;
				else
					opcode = OpCodes.Add;
				break;
				
			case ExpressionType.Subtract:
				opcode = OpCodes.Sub;
				break;

			case ExpressionType.SubtractChecked:
				if (left.Type == typeof (int) || left.Type == typeof (long))
					opcode = OpCodes.Sub_Ovf;
				else if (is_unsigned)
					opcode = OpCodes.Sub_Ovf_Un;
				else
					opcode = OpCodes.Sub;
				break;
				
			case ExpressionType.Multiply:
				opcode = OpCodes.Mul;
				break;

			case ExpressionType.MultiplyChecked:
				if (left.Type == typeof (int) || left.Type == typeof (long))
					opcode = OpCodes.Mul_Ovf;
				else if (is_unsigned)
					opcode = OpCodes.Mul_Ovf_Un;
				else
					opcode = OpCodes.Mul;
				break;
				
			case ExpressionType.Divide:
				if (is_unsigned)
					opcode = OpCodes.Div_Un;
				else
					opcode = OpCodes.Div;
				break;

			case ExpressionType.Modulo:
                                if (is_unsigned)
                                        opcode = OpCodes.Rem_Un;
                                else
                                        opcode = OpCodes.Rem;
                                break;

			case ExpressionType.RightShift:
                                if (is_unsigned)
                                        opcode = OpCodes.Shr_Un;
                                else
                                        opcode = OpCodes.Shr;
                                break;

                        case ExpressionType.LeftShift:
                                opcode = OpCodes.Shl;
                                break;

			case ExpressionType.And:
                                opcode = OpCodes.And;
				break;
				
			case ExpressionType.Or:
                                opcode = OpCodes.Or;
				break;

			case ExpressionType.ExclusiveOr:
				opcode = OpCodes.Xor;
				break;
					
			case ExpressionType.AndAlso:
			case ExpressionType.Coalesce:
			case ExpressionType.Equal:
			case ExpressionType.GreaterThan:
			case ExpressionType.GreaterThanOrEqual:
			case ExpressionType.LessThan:
			case ExpressionType.LessThanOrEqual:
			case ExpressionType.NotEqual:
			case ExpressionType.OrElse:
			case ExpressionType.Power:
				throw new NotImplementedException (String.Format ("No support for {0} node yet", NodeType));
					
			default:
				throw new Exception (String.Format ("Internal error: BinaryExpression contains non-Binary nodetype {0}", NodeType));
			}
			ig.Emit (opcode);
		}
		
		[MonoTODO]
		public LambdaExpression Conversion {
			get {
				if (this.NodeType != ExpressionType.Coalesce)
					return null;

				throw new System.NotImplementedException ();
			}
		}
	}
}
