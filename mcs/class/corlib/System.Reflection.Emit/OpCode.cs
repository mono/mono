//
// System.Reflection.Emit.OpCode
//
// Author:
//   Sergey Chaban (serge@wildwestsoftware.com)
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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


namespace System.Reflection.Emit {

	public struct OpCode {

		string name;
		internal byte op1;
		internal byte op2;
		byte size;
		byte type;
		byte flowCtrl;
		byte pop;
		byte push;
		byte operandType;

		internal OpCode (string name, int size,
		                 OpCodeType opcodeType,
		                 OperandType operandType,
                                 StackBehaviour pop,
                                 StackBehaviour push,
		                 FlowControl flowCtrl,
		                 byte op1, byte op2)
		{
			this.name = name;
			this.size = (byte)size;
			this.type = (byte)opcodeType;
			this.operandType = (byte)operandType;
			this.pop = (byte)pop;
			this.push = (byte)push;
			this.flowCtrl = (byte)flowCtrl;
			this.op1 = op1;
			this.op2 = op2;
		}


		public override int GetHashCode ()
		{
			return Value;
		}

		public override bool Equals (Object obj)
		{
			if (obj == null || !(obj is OpCode))
				return false;
			OpCode v = (OpCode)obj;
			return v.op1 == op1 && v.op2 == op2;
		}

		/// <summary>
		/// </summary>
		public string Name {
			get {
				return name;
			}
		}

		/// <summary>
		/// </summary>
		public int Size {
			get {
				return size;
			}
		}


		/// <summary>
		/// </summary>
		public OpCodeType OpCodeType {
			get {
				return (OpCodeType)type;
			}
		}

		/// <summary>
		/// </summary>
		public OperandType OperandType {
			get {
				return (OperandType)operandType;
			}
		}

		/// <summary>
		/// </summary>
		public FlowControl FlowControl {
			get {
				return (FlowControl)flowCtrl;
			}
		}


		/// <summary>
		/// </summary>
		public StackBehaviour StackBehaviourPop {
			get {
				return (StackBehaviour)pop;
			}
		}


		/// <summary>
		/// </summary>
		public StackBehaviour StackBehaviourPush {
			get {
				return (StackBehaviour)push;
			}
		}


		/// <summary>
		/// </summary>
		public short Value {
			get {
				if (size == 1) {
					return op2;
				} else {
					// two byte instruction - combine
					// Some old MS betas returned (op1 << 2) | op2 here...
					return (short) ((op1 << 8) | op2);
				}
			}
		}

		public override string ToString()
		{
			return Name;
		}
	} // OpCode

} // System.Reflection.Emit
