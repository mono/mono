//
// System.Reflection.Emit.OpCode
//
// Author:
//   Duncan Mak (duncan@ximian.com)
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
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
//

#if !FULL_AOT_RUNTIME
using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;

namespace System.Reflection.Emit {

	[ComVisible (true)]
	public struct OpCode {

		internal byte op1, op2;
		byte push, pop, size, type, args, flow;

		//
		// The order is:
		//	 Op1, Op2, StackBehaviourPush, StackBehaviourPop
		//	 Size, OpCodeType, OperandType, FlowControl
		//
		internal OpCode (int p, int q)
		{

			op1  = (byte) ((p >> 0)  & 0xFF);
			op2  = (byte) ((p >> 8)  & 0xFF);
			push = (byte) ((p >> 16) & 0xFF);
			pop  = (byte) ((p >> 24) & 0xFF);

			size = (byte) ((q >> 0)  & 0xFF);
			type = (byte) ((q >> 8)  & 0xFF);
			args = (byte) ((q >> 16) & 0xFF);
			flow = (byte) ((q >> 24) & 0xFF);
		}


		public override int GetHashCode ()
		{
			return Name.GetHashCode ();
		}

		public override bool Equals (Object obj)
		{
			if (obj == null || !(obj is OpCode))
				return false;

			OpCode v = (OpCode) obj;

			return v.op1 == op1 && v.op2 == op2;
		}

		public bool Equals (OpCode obj)
		{
			return obj.op1 == op1 && obj.op2 == op2;
		}

		public override string ToString ()
		{
			return Name;
		}

		public string Name {
			get {
				if (op1 == 0xFF)
					return OpCodeNames.names [op2];

				return OpCodeNames.names [256 + op2];
			}
		}

		public int Size {
			get {
				return (int) size;
			}
		}

		public OpCodeType OpCodeType {
			get {
				return (OpCodeType) type;
			}
		}

		public OperandType OperandType {
			get {
				return (OperandType) args;
			}
		}

		public FlowControl FlowControl {
			get {
				return (FlowControl) flow;
			}
		}

		public StackBehaviour StackBehaviourPop {
			get {
				return (StackBehaviour) pop;
			}
		}

		public StackBehaviour StackBehaviourPush {
			get {
				return (StackBehaviour) push;
			}
		}

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

		public static bool operator == (OpCode a, OpCode b)
		{
			return a.op1 == b.op1 && a.op2 == b.op2;
		}

		public static bool operator != (OpCode a, OpCode b)
		{
			return a.op1 != b.op1 || a.op2 != b.op2;
		}
	}
} 
#endif
