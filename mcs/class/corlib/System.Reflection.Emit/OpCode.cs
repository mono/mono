//
// System.Reflection.Emit.OpCode
//
// Author:
//   Sergey Chaban (serge@wildwestsoftware.com)
//

using System;
using System.Reflection;
using System.Reflection.Emit;


namespace System.Reflection.Emit {

	public struct OpCode {

		internal string name;
		internal int size;
		internal OpCodeType type;
		internal OperandType operandType;
		internal StackBehaviour pop;
		internal StackBehaviour push;
		internal FlowControl flowCtrl;
		internal byte op1;
		internal byte op2;

		internal OpCode (string name, int size,
		                 OpCodeType opcodeType,
		                 OperandType operandType,
                                 StackBehaviour pop,
                                 StackBehaviour push,
		                 FlowControl flowCtrl,
		                 byte op1, byte op2)
		{
			this.name = name;
			this.size = size;
			this.type = opcodeType;
			this.operandType = operandType;
			this.pop = pop;
			this.push = push;
			this.flowCtrl = flowCtrl;
			this.op1 = op1;
			this.op2 = op2;
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
				return type;
			}
		}

		/// <summary>
		/// </summary>
		public OperandType OperandType {
			get {
				return operandType;
			}
		}

		/// <summary>
		/// </summary>
		public FlowControl FlowControl {
			get {
				return flowCtrl;
			}
		}


		/// <summary>
		/// </summary>
		public StackBehaviour StackBehaviourPop {
			get {
				return pop;
			}
		}


		/// <summary>
		/// </summary>
		public StackBehaviour StackBehaviourPush {
			get {
				return push;
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
					// give the same values as the mscorlib impl
					// this makes the Value property useless
					return (short) ((op1 << 2) | op2);
				}
			}
		}

		public override string ToString()
		{
			return Name;
		}
	} // OpCode

} // System.Reflection.Emit
