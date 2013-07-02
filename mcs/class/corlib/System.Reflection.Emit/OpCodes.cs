#if !FULL_AOT_RUNTIME
using System.Runtime.InteropServices;

namespace System.Reflection.Emit {
	[ComVisible (true)]
	public class OpCodes {

		internal OpCodes () {
		}

		//
		// The order is:
		//	 Op1, Op2, StackBehaviourPush, StackBehaviourPop
		//	 Size, OpCodeType, OperandType, FlowControl
		//
		public static readonly OpCode Nop = new OpCode (
			0xFF << 0 | 0x00 << 8 | (byte) StackBehaviour.Push0 << 16 | (byte) StackBehaviour.Pop0 << 24,
			1 << 0 | (byte) OpCodeType.Primitive << 8 | (byte) OperandType.InlineNone << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Break = new OpCode (
			0xFF << 0 | 0x01 << 8 | (byte) StackBehaviour.Push0 << 16 | (byte) StackBehaviour.Pop0 << 24,
			1 << 0 | (byte) OpCodeType.Primitive << 8 | (byte) OperandType.InlineNone << 16 | (byte) FlowControl.Break << 24);

		public static readonly OpCode Ldarg_0 = new OpCode (
			0xFF << 0 | 0x02 << 8 | (byte) StackBehaviour.Push1 << 16 | (byte) StackBehaviour.Pop0 << 24,
			1 << 0 | (byte) OpCodeType.Macro << 8 | (byte) OperandType.InlineNone << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Ldarg_1 = new OpCode (
			0xFF << 0 | 0x03 << 8 | (byte) StackBehaviour.Push1 << 16 | (byte) StackBehaviour.Pop0 << 24,
			1 << 0 | (byte) OpCodeType.Macro << 8 | (byte) OperandType.InlineNone << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Ldarg_2 = new OpCode (
			0xFF << 0 | 0x04 << 8 | (byte) StackBehaviour.Push1 << 16 | (byte) StackBehaviour.Pop0 << 24,
			1 << 0 | (byte) OpCodeType.Macro << 8 | (byte) OperandType.InlineNone << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Ldarg_3 = new OpCode (
			0xFF << 0 | 0x05 << 8 | (byte) StackBehaviour.Push1 << 16 | (byte) StackBehaviour.Pop0 << 24,
			1 << 0 | (byte) OpCodeType.Macro << 8 | (byte) OperandType.InlineNone << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Ldloc_0 = new OpCode (
			0xFF << 0 | 0x06 << 8 | (byte) StackBehaviour.Push1 << 16 | (byte) StackBehaviour.Pop0 << 24,
			1 << 0 | (byte) OpCodeType.Macro << 8 | (byte) OperandType.InlineNone << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Ldloc_1 = new OpCode (
			0xFF << 0 | 0x07 << 8 | (byte) StackBehaviour.Push1 << 16 | (byte) StackBehaviour.Pop0 << 24,
			1 << 0 | (byte) OpCodeType.Macro << 8 | (byte) OperandType.InlineNone << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Ldloc_2 = new OpCode (
			0xFF << 0 | 0x08 << 8 | (byte) StackBehaviour.Push1 << 16 | (byte) StackBehaviour.Pop0 << 24,
			1 << 0 | (byte) OpCodeType.Macro << 8 | (byte) OperandType.InlineNone << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Ldloc_3 = new OpCode (
			0xFF << 0 | 0x09 << 8 | (byte) StackBehaviour.Push1 << 16 | (byte) StackBehaviour.Pop0 << 24,
			1 << 0 | (byte) OpCodeType.Macro << 8 | (byte) OperandType.InlineNone << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Stloc_0 = new OpCode (
			0xFF << 0 | 0x0A << 8 | (byte) StackBehaviour.Push0 << 16 | (byte) StackBehaviour.Pop1 << 24,
			1 << 0 | (byte) OpCodeType.Macro << 8 | (byte) OperandType.InlineNone << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Stloc_1 = new OpCode (
			0xFF << 0 | 0x0B << 8 | (byte) StackBehaviour.Push0 << 16 | (byte) StackBehaviour.Pop1 << 24,
			1 << 0 | (byte) OpCodeType.Macro << 8 | (byte) OperandType.InlineNone << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Stloc_2 = new OpCode (
			0xFF << 0 | 0x0C << 8 | (byte) StackBehaviour.Push0 << 16 | (byte) StackBehaviour.Pop1 << 24,
			1 << 0 | (byte) OpCodeType.Macro << 8 | (byte) OperandType.InlineNone << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Stloc_3 = new OpCode (
			0xFF << 0 | 0x0D << 8 | (byte) StackBehaviour.Push0 << 16 | (byte) StackBehaviour.Pop1 << 24,
			1 << 0 | (byte) OpCodeType.Macro << 8 | (byte) OperandType.InlineNone << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Ldarg_S = new OpCode (
			0xFF << 0 | 0x0E << 8 | (byte) StackBehaviour.Push1 << 16 | (byte) StackBehaviour.Pop0 << 24,
			1 << 0 | (byte) OpCodeType.Macro << 8 | (byte) OperandType.ShortInlineVar << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Ldarga_S = new OpCode (
			0xFF << 0 | 0x0F << 8 | (byte) StackBehaviour.Pushi << 16 | (byte) StackBehaviour.Pop0 << 24,
			1 << 0 | (byte) OpCodeType.Macro << 8 | (byte) OperandType.ShortInlineVar << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Starg_S = new OpCode (
			0xFF << 0 | 0x10 << 8 | (byte) StackBehaviour.Push0 << 16 | (byte) StackBehaviour.Pop1 << 24,
			1 << 0 | (byte) OpCodeType.Macro << 8 | (byte) OperandType.ShortInlineVar << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Ldloc_S = new OpCode (
			0xFF << 0 | 0x11 << 8 | (byte) StackBehaviour.Push1 << 16 | (byte) StackBehaviour.Pop0 << 24,
			1 << 0 | (byte) OpCodeType.Macro << 8 | (byte) OperandType.ShortInlineVar << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Ldloca_S = new OpCode (
			0xFF << 0 | 0x12 << 8 | (byte) StackBehaviour.Pushi << 16 | (byte) StackBehaviour.Pop0 << 24,
			1 << 0 | (byte) OpCodeType.Macro << 8 | (byte) OperandType.ShortInlineVar << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Stloc_S = new OpCode (
			0xFF << 0 | 0x13 << 8 | (byte) StackBehaviour.Push0 << 16 | (byte) StackBehaviour.Pop1 << 24,
			1 << 0 | (byte) OpCodeType.Macro << 8 | (byte) OperandType.ShortInlineVar << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Ldnull = new OpCode (
			0xFF << 0 | 0x14 << 8 | (byte) StackBehaviour.Pushref << 16 | (byte) StackBehaviour.Pop0 << 24,
			1 << 0 | (byte) OpCodeType.Primitive << 8 | (byte) OperandType.InlineNone << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Ldc_I4_M1 = new OpCode (
			0xFF << 0 | 0x15 << 8 | (byte) StackBehaviour.Pushi << 16 | (byte) StackBehaviour.Pop0 << 24,
			1 << 0 | (byte) OpCodeType.Macro << 8 | (byte) OperandType.InlineNone << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Ldc_I4_0 = new OpCode (
			0xFF << 0 | 0x16 << 8 | (byte) StackBehaviour.Pushi << 16 | (byte) StackBehaviour.Pop0 << 24,
			1 << 0 | (byte) OpCodeType.Macro << 8 | (byte) OperandType.InlineNone << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Ldc_I4_1 = new OpCode (
			0xFF << 0 | 0x17 << 8 | (byte) StackBehaviour.Pushi << 16 | (byte) StackBehaviour.Pop0 << 24,
			1 << 0 | (byte) OpCodeType.Macro << 8 | (byte) OperandType.InlineNone << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Ldc_I4_2 = new OpCode (
			0xFF << 0 | 0x18 << 8 | (byte) StackBehaviour.Pushi << 16 | (byte) StackBehaviour.Pop0 << 24,
			1 << 0 | (byte) OpCodeType.Macro << 8 | (byte) OperandType.InlineNone << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Ldc_I4_3 = new OpCode (
			0xFF << 0 | 0x19 << 8 | (byte) StackBehaviour.Pushi << 16 | (byte) StackBehaviour.Pop0 << 24,
			1 << 0 | (byte) OpCodeType.Macro << 8 | (byte) OperandType.InlineNone << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Ldc_I4_4 = new OpCode (
			0xFF << 0 | 0x1A << 8 | (byte) StackBehaviour.Pushi << 16 | (byte) StackBehaviour.Pop0 << 24,
			1 << 0 | (byte) OpCodeType.Macro << 8 | (byte) OperandType.InlineNone << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Ldc_I4_5 = new OpCode (
			0xFF << 0 | 0x1B << 8 | (byte) StackBehaviour.Pushi << 16 | (byte) StackBehaviour.Pop0 << 24,
			1 << 0 | (byte) OpCodeType.Macro << 8 | (byte) OperandType.InlineNone << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Ldc_I4_6 = new OpCode (
			0xFF << 0 | 0x1C << 8 | (byte) StackBehaviour.Pushi << 16 | (byte) StackBehaviour.Pop0 << 24,
			1 << 0 | (byte) OpCodeType.Macro << 8 | (byte) OperandType.InlineNone << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Ldc_I4_7 = new OpCode (
			0xFF << 0 | 0x1D << 8 | (byte) StackBehaviour.Pushi << 16 | (byte) StackBehaviour.Pop0 << 24,
			1 << 0 | (byte) OpCodeType.Macro << 8 | (byte) OperandType.InlineNone << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Ldc_I4_8 = new OpCode (
			0xFF << 0 | 0x1E << 8 | (byte) StackBehaviour.Pushi << 16 | (byte) StackBehaviour.Pop0 << 24,
			1 << 0 | (byte) OpCodeType.Macro << 8 | (byte) OperandType.InlineNone << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Ldc_I4_S = new OpCode (
			0xFF << 0 | 0x1F << 8 | (byte) StackBehaviour.Pushi << 16 | (byte) StackBehaviour.Pop0 << 24,
			1 << 0 | (byte) OpCodeType.Macro << 8 | (byte) OperandType.ShortInlineI << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Ldc_I4 = new OpCode (
			0xFF << 0 | 0x20 << 8 | (byte) StackBehaviour.Pushi << 16 | (byte) StackBehaviour.Pop0 << 24,
			1 << 0 | (byte) OpCodeType.Primitive << 8 | (byte) OperandType.InlineI << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Ldc_I8 = new OpCode (
			0xFF << 0 | 0x21 << 8 | (byte) StackBehaviour.Pushi8 << 16 | (byte) StackBehaviour.Pop0 << 24,
			1 << 0 | (byte) OpCodeType.Primitive << 8 | (byte) OperandType.InlineI8 << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Ldc_R4 = new OpCode (
			0xFF << 0 | 0x22 << 8 | (byte) StackBehaviour.Pushr4 << 16 | (byte) StackBehaviour.Pop0 << 24,
			1 << 0 | (byte) OpCodeType.Primitive << 8 | (byte) OperandType.ShortInlineR << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Ldc_R8 = new OpCode (
			0xFF << 0 | 0x23 << 8 | (byte) StackBehaviour.Pushr8 << 16 | (byte) StackBehaviour.Pop0 << 24,
			1 << 0 | (byte) OpCodeType.Primitive << 8 | (byte) OperandType.InlineR << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Dup = new OpCode (
			0xFF << 0 | 0x25 << 8 | (byte) StackBehaviour.Push1_push1 << 16 | (byte) StackBehaviour.Pop1 << 24,
			1 << 0 | (byte) OpCodeType.Primitive << 8 | (byte) OperandType.InlineNone << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Pop = new OpCode (
			0xFF << 0 | 0x26 << 8 | (byte) StackBehaviour.Push0 << 16 | (byte) StackBehaviour.Pop1 << 24,
			1 << 0 | (byte) OpCodeType.Primitive << 8 | (byte) OperandType.InlineNone << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Jmp = new OpCode (
			0xFF << 0 | 0x27 << 8 | (byte) StackBehaviour.Push0 << 16 | (byte) StackBehaviour.Pop0 << 24,
			1 << 0 | (byte) OpCodeType.Primitive << 8 | (byte) OperandType.InlineMethod << 16 | (byte) FlowControl.Call << 24);

		public static readonly OpCode Call = new OpCode (
			0xFF << 0 | 0x28 << 8 | (byte) StackBehaviour.Varpush << 16 | (byte) StackBehaviour.Varpop << 24,
			1 << 0 | (byte) OpCodeType.Primitive << 8 | (byte) OperandType.InlineMethod << 16 | (byte) FlowControl.Call << 24);

		public static readonly OpCode Calli = new OpCode (
			0xFF << 0 | 0x29 << 8 | (byte) StackBehaviour.Varpush << 16 | (byte) StackBehaviour.Varpop << 24,
			1 << 0 | (byte) OpCodeType.Primitive << 8 | (byte) OperandType.InlineSig << 16 | (byte) FlowControl.Call << 24);

		public static readonly OpCode Ret = new OpCode (
			0xFF << 0 | 0x2A << 8 | (byte) StackBehaviour.Push0 << 16 | (byte) StackBehaviour.Varpop << 24,
			1 << 0 | (byte) OpCodeType.Primitive << 8 | (byte) OperandType.InlineNone << 16 | (byte) FlowControl.Return << 24);

		public static readonly OpCode Br_S = new OpCode (
			0xFF << 0 | 0x2B << 8 | (byte) StackBehaviour.Push0 << 16 | (byte) StackBehaviour.Pop0 << 24,
			1 << 0 | (byte) OpCodeType.Macro << 8 | (byte) OperandType.ShortInlineBrTarget << 16 | (byte) FlowControl.Branch << 24);

		public static readonly OpCode Brfalse_S = new OpCode (
			0xFF << 0 | 0x2C << 8 | (byte) StackBehaviour.Push0 << 16 | (byte) StackBehaviour.Popi << 24,
			1 << 0 | (byte) OpCodeType.Macro << 8 | (byte) OperandType.ShortInlineBrTarget << 16 | (byte) FlowControl.Cond_Branch << 24);

		public static readonly OpCode Brtrue_S = new OpCode (
			0xFF << 0 | 0x2D << 8 | (byte) StackBehaviour.Push0 << 16 | (byte) StackBehaviour.Popi << 24,
			1 << 0 | (byte) OpCodeType.Macro << 8 | (byte) OperandType.ShortInlineBrTarget << 16 | (byte) FlowControl.Cond_Branch << 24);

		public static readonly OpCode Beq_S = new OpCode (
			0xFF << 0 | 0x2E << 8 | (byte) StackBehaviour.Push0 << 16 | (byte) StackBehaviour.Pop1_pop1 << 24,
			1 << 0 | (byte) OpCodeType.Macro << 8 | (byte) OperandType.ShortInlineBrTarget << 16 | (byte) FlowControl.Cond_Branch << 24);

		public static readonly OpCode Bge_S = new OpCode (
			0xFF << 0 | 0x2F << 8 | (byte) StackBehaviour.Push0 << 16 | (byte) StackBehaviour.Pop1_pop1 << 24,
			1 << 0 | (byte) OpCodeType.Macro << 8 | (byte) OperandType.ShortInlineBrTarget << 16 | (byte) FlowControl.Cond_Branch << 24);

		public static readonly OpCode Bgt_S = new OpCode (
			0xFF << 0 | 0x30 << 8 | (byte) StackBehaviour.Push0 << 16 | (byte) StackBehaviour.Pop1_pop1 << 24,
			1 << 0 | (byte) OpCodeType.Macro << 8 | (byte) OperandType.ShortInlineBrTarget << 16 | (byte) FlowControl.Cond_Branch << 24);

		public static readonly OpCode Ble_S = new OpCode (
			0xFF << 0 | 0x31 << 8 | (byte) StackBehaviour.Push0 << 16 | (byte) StackBehaviour.Pop1_pop1 << 24,
			1 << 0 | (byte) OpCodeType.Macro << 8 | (byte) OperandType.ShortInlineBrTarget << 16 | (byte) FlowControl.Cond_Branch << 24);

		public static readonly OpCode Blt_S = new OpCode (
			0xFF << 0 | 0x32 << 8 | (byte) StackBehaviour.Push0 << 16 | (byte) StackBehaviour.Pop1_pop1 << 24,
			1 << 0 | (byte) OpCodeType.Macro << 8 | (byte) OperandType.ShortInlineBrTarget << 16 | (byte) FlowControl.Cond_Branch << 24);

		public static readonly OpCode Bne_Un_S = new OpCode (
			0xFF << 0 | 0x33 << 8 | (byte) StackBehaviour.Push0 << 16 | (byte) StackBehaviour.Pop1_pop1 << 24,
			1 << 0 | (byte) OpCodeType.Macro << 8 | (byte) OperandType.ShortInlineBrTarget << 16 | (byte) FlowControl.Cond_Branch << 24);

		public static readonly OpCode Bge_Un_S = new OpCode (
			0xFF << 0 | 0x34 << 8 | (byte) StackBehaviour.Push0 << 16 | (byte) StackBehaviour.Pop1_pop1 << 24,
			1 << 0 | (byte) OpCodeType.Macro << 8 | (byte) OperandType.ShortInlineBrTarget << 16 | (byte) FlowControl.Cond_Branch << 24);

		public static readonly OpCode Bgt_Un_S = new OpCode (
			0xFF << 0 | 0x35 << 8 | (byte) StackBehaviour.Push0 << 16 | (byte) StackBehaviour.Pop1_pop1 << 24,
			1 << 0 | (byte) OpCodeType.Macro << 8 | (byte) OperandType.ShortInlineBrTarget << 16 | (byte) FlowControl.Cond_Branch << 24);

		public static readonly OpCode Ble_Un_S = new OpCode (
			0xFF << 0 | 0x36 << 8 | (byte) StackBehaviour.Push0 << 16 | (byte) StackBehaviour.Pop1_pop1 << 24,
			1 << 0 | (byte) OpCodeType.Macro << 8 | (byte) OperandType.ShortInlineBrTarget << 16 | (byte) FlowControl.Cond_Branch << 24);

		public static readonly OpCode Blt_Un_S = new OpCode (
			0xFF << 0 | 0x37 << 8 | (byte) StackBehaviour.Push0 << 16 | (byte) StackBehaviour.Pop1_pop1 << 24,
			1 << 0 | (byte) OpCodeType.Macro << 8 | (byte) OperandType.ShortInlineBrTarget << 16 | (byte) FlowControl.Cond_Branch << 24);

		public static readonly OpCode Br = new OpCode (
			0xFF << 0 | 0x38 << 8 | (byte) StackBehaviour.Push0 << 16 | (byte) StackBehaviour.Pop0 << 24,
			1 << 0 | (byte) OpCodeType.Primitive << 8 | (byte) OperandType.InlineBrTarget << 16 | (byte) FlowControl.Branch << 24);

		public static readonly OpCode Brfalse = new OpCode (
			0xFF << 0 | 0x39 << 8 | (byte) StackBehaviour.Push0 << 16 | (byte) StackBehaviour.Popi << 24,
			1 << 0 | (byte) OpCodeType.Primitive << 8 | (byte) OperandType.InlineBrTarget << 16 | (byte) FlowControl.Cond_Branch << 24);

		public static readonly OpCode Brtrue = new OpCode (
			0xFF << 0 | 0x3A << 8 | (byte) StackBehaviour.Push0 << 16 | (byte) StackBehaviour.Popi << 24,
			1 << 0 | (byte) OpCodeType.Primitive << 8 | (byte) OperandType.InlineBrTarget << 16 | (byte) FlowControl.Cond_Branch << 24);

		public static readonly OpCode Beq = new OpCode (
			0xFF << 0 | 0x3B << 8 | (byte) StackBehaviour.Push0 << 16 | (byte) StackBehaviour.Pop1_pop1 << 24,
			1 << 0 | (byte) OpCodeType.Macro << 8 | (byte) OperandType.InlineBrTarget << 16 | (byte) FlowControl.Cond_Branch << 24);

		public static readonly OpCode Bge = new OpCode (
			0xFF << 0 | 0x3C << 8 | (byte) StackBehaviour.Push0 << 16 | (byte) StackBehaviour.Pop1_pop1 << 24,
			1 << 0 | (byte) OpCodeType.Macro << 8 | (byte) OperandType.InlineBrTarget << 16 | (byte) FlowControl.Cond_Branch << 24);

		public static readonly OpCode Bgt = new OpCode (
			0xFF << 0 | 0x3D << 8 | (byte) StackBehaviour.Push0 << 16 | (byte) StackBehaviour.Pop1_pop1 << 24,
			1 << 0 | (byte) OpCodeType.Macro << 8 | (byte) OperandType.InlineBrTarget << 16 | (byte) FlowControl.Cond_Branch << 24);

		public static readonly OpCode Ble = new OpCode (
			0xFF << 0 | 0x3E << 8 | (byte) StackBehaviour.Push0 << 16 | (byte) StackBehaviour.Pop1_pop1 << 24,
			1 << 0 | (byte) OpCodeType.Macro << 8 | (byte) OperandType.InlineBrTarget << 16 | (byte) FlowControl.Cond_Branch << 24);

		public static readonly OpCode Blt = new OpCode (
			0xFF << 0 | 0x3F << 8 | (byte) StackBehaviour.Push0 << 16 | (byte) StackBehaviour.Pop1_pop1 << 24,
			1 << 0 | (byte) OpCodeType.Macro << 8 | (byte) OperandType.InlineBrTarget << 16 | (byte) FlowControl.Cond_Branch << 24);

		public static readonly OpCode Bne_Un = new OpCode (
			0xFF << 0 | 0x40 << 8 | (byte) StackBehaviour.Push0 << 16 | (byte) StackBehaviour.Pop1_pop1 << 24,
			1 << 0 | (byte) OpCodeType.Macro << 8 | (byte) OperandType.InlineBrTarget << 16 | (byte) FlowControl.Cond_Branch << 24);

		public static readonly OpCode Bge_Un = new OpCode (
			0xFF << 0 | 0x41 << 8 | (byte) StackBehaviour.Push0 << 16 | (byte) StackBehaviour.Pop1_pop1 << 24,
			1 << 0 | (byte) OpCodeType.Macro << 8 | (byte) OperandType.InlineBrTarget << 16 | (byte) FlowControl.Cond_Branch << 24);

		public static readonly OpCode Bgt_Un = new OpCode (
			0xFF << 0 | 0x42 << 8 | (byte) StackBehaviour.Push0 << 16 | (byte) StackBehaviour.Pop1_pop1 << 24,
			1 << 0 | (byte) OpCodeType.Macro << 8 | (byte) OperandType.InlineBrTarget << 16 | (byte) FlowControl.Cond_Branch << 24);

		public static readonly OpCode Ble_Un = new OpCode (
			0xFF << 0 | 0x43 << 8 | (byte) StackBehaviour.Push0 << 16 | (byte) StackBehaviour.Pop1_pop1 << 24,
			1 << 0 | (byte) OpCodeType.Macro << 8 | (byte) OperandType.InlineBrTarget << 16 | (byte) FlowControl.Cond_Branch << 24);

		public static readonly OpCode Blt_Un = new OpCode (
			0xFF << 0 | 0x44 << 8 | (byte) StackBehaviour.Push0 << 16 | (byte) StackBehaviour.Pop1_pop1 << 24,
			1 << 0 | (byte) OpCodeType.Macro << 8 | (byte) OperandType.InlineBrTarget << 16 | (byte) FlowControl.Cond_Branch << 24);

		public static readonly OpCode Switch = new OpCode (
			0xFF << 0 | 0x45 << 8 | (byte) StackBehaviour.Push0 << 16 | (byte) StackBehaviour.Popi << 24,
			1 << 0 | (byte) OpCodeType.Primitive << 8 | (byte) OperandType.InlineSwitch << 16 | (byte) FlowControl.Cond_Branch << 24);

		public static readonly OpCode Ldind_I1 = new OpCode (
			0xFF << 0 | 0x46 << 8 | (byte) StackBehaviour.Pushi << 16 | (byte) StackBehaviour.Popi << 24,
			1 << 0 | (byte) OpCodeType.Primitive << 8 | (byte) OperandType.InlineNone << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Ldind_U1 = new OpCode (
			0xFF << 0 | 0x47 << 8 | (byte) StackBehaviour.Pushi << 16 | (byte) StackBehaviour.Popi << 24,
			1 << 0 | (byte) OpCodeType.Primitive << 8 | (byte) OperandType.InlineNone << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Ldind_I2 = new OpCode (
			0xFF << 0 | 0x48 << 8 | (byte) StackBehaviour.Pushi << 16 | (byte) StackBehaviour.Popi << 24,
			1 << 0 | (byte) OpCodeType.Primitive << 8 | (byte) OperandType.InlineNone << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Ldind_U2 = new OpCode (
			0xFF << 0 | 0x49 << 8 | (byte) StackBehaviour.Pushi << 16 | (byte) StackBehaviour.Popi << 24,
			1 << 0 | (byte) OpCodeType.Primitive << 8 | (byte) OperandType.InlineNone << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Ldind_I4 = new OpCode (
			0xFF << 0 | 0x4A << 8 | (byte) StackBehaviour.Pushi << 16 | (byte) StackBehaviour.Popi << 24,
			1 << 0 | (byte) OpCodeType.Primitive << 8 | (byte) OperandType.InlineNone << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Ldind_U4 = new OpCode (
			0xFF << 0 | 0x4B << 8 | (byte) StackBehaviour.Pushi << 16 | (byte) StackBehaviour.Popi << 24,
			1 << 0 | (byte) OpCodeType.Primitive << 8 | (byte) OperandType.InlineNone << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Ldind_I8 = new OpCode (
			0xFF << 0 | 0x4C << 8 | (byte) StackBehaviour.Pushi8 << 16 | (byte) StackBehaviour.Popi << 24,
			1 << 0 | (byte) OpCodeType.Primitive << 8 | (byte) OperandType.InlineNone << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Ldind_I = new OpCode (
			0xFF << 0 | 0x4D << 8 | (byte) StackBehaviour.Pushi << 16 | (byte) StackBehaviour.Popi << 24,
			1 << 0 | (byte) OpCodeType.Primitive << 8 | (byte) OperandType.InlineNone << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Ldind_R4 = new OpCode (
			0xFF << 0 | 0x4E << 8 | (byte) StackBehaviour.Pushr4 << 16 | (byte) StackBehaviour.Popi << 24,
			1 << 0 | (byte) OpCodeType.Primitive << 8 | (byte) OperandType.InlineNone << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Ldind_R8 = new OpCode (
			0xFF << 0 | 0x4F << 8 | (byte) StackBehaviour.Pushr8 << 16 | (byte) StackBehaviour.Popi << 24,
			1 << 0 | (byte) OpCodeType.Primitive << 8 | (byte) OperandType.InlineNone << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Ldind_Ref = new OpCode (
			0xFF << 0 | 0x50 << 8 | (byte) StackBehaviour.Pushref << 16 | (byte) StackBehaviour.Popi << 24,
			1 << 0 | (byte) OpCodeType.Primitive << 8 | (byte) OperandType.InlineNone << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Stind_Ref = new OpCode (
			0xFF << 0 | 0x51 << 8 | (byte) StackBehaviour.Push0 << 16 | (byte) StackBehaviour.Popi_popi << 24,
			1 << 0 | (byte) OpCodeType.Primitive << 8 | (byte) OperandType.InlineNone << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Stind_I1 = new OpCode (
			0xFF << 0 | 0x52 << 8 | (byte) StackBehaviour.Push0 << 16 | (byte) StackBehaviour.Popi_popi << 24,
			1 << 0 | (byte) OpCodeType.Primitive << 8 | (byte) OperandType.InlineNone << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Stind_I2 = new OpCode (
			0xFF << 0 | 0x53 << 8 | (byte) StackBehaviour.Push0 << 16 | (byte) StackBehaviour.Popi_popi << 24,
			1 << 0 | (byte) OpCodeType.Primitive << 8 | (byte) OperandType.InlineNone << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Stind_I4 = new OpCode (
			0xFF << 0 | 0x54 << 8 | (byte) StackBehaviour.Push0 << 16 | (byte) StackBehaviour.Popi_popi << 24,
			1 << 0 | (byte) OpCodeType.Primitive << 8 | (byte) OperandType.InlineNone << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Stind_I8 = new OpCode (
			0xFF << 0 | 0x55 << 8 | (byte) StackBehaviour.Push0 << 16 | (byte) StackBehaviour.Popi_popi8 << 24,
			1 << 0 | (byte) OpCodeType.Primitive << 8 | (byte) OperandType.InlineNone << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Stind_R4 = new OpCode (
			0xFF << 0 | 0x56 << 8 | (byte) StackBehaviour.Push0 << 16 | (byte) StackBehaviour.Popi_popr4 << 24,
			1 << 0 | (byte) OpCodeType.Primitive << 8 | (byte) OperandType.InlineNone << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Stind_R8 = new OpCode (
			0xFF << 0 | 0x57 << 8 | (byte) StackBehaviour.Push0 << 16 | (byte) StackBehaviour.Popi_popr8 << 24,
			1 << 0 | (byte) OpCodeType.Primitive << 8 | (byte) OperandType.InlineNone << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Add = new OpCode (
			0xFF << 0 | 0x58 << 8 | (byte) StackBehaviour.Push1 << 16 | (byte) StackBehaviour.Pop1_pop1 << 24,
			1 << 0 | (byte) OpCodeType.Primitive << 8 | (byte) OperandType.InlineNone << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Sub = new OpCode (
			0xFF << 0 | 0x59 << 8 | (byte) StackBehaviour.Push1 << 16 | (byte) StackBehaviour.Pop1_pop1 << 24,
			1 << 0 | (byte) OpCodeType.Primitive << 8 | (byte) OperandType.InlineNone << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Mul = new OpCode (
			0xFF << 0 | 0x5A << 8 | (byte) StackBehaviour.Push1 << 16 | (byte) StackBehaviour.Pop1_pop1 << 24,
			1 << 0 | (byte) OpCodeType.Primitive << 8 | (byte) OperandType.InlineNone << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Div = new OpCode (
			0xFF << 0 | 0x5B << 8 | (byte) StackBehaviour.Push1 << 16 | (byte) StackBehaviour.Pop1_pop1 << 24,
			1 << 0 | (byte) OpCodeType.Primitive << 8 | (byte) OperandType.InlineNone << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Div_Un = new OpCode (
			0xFF << 0 | 0x5C << 8 | (byte) StackBehaviour.Push1 << 16 | (byte) StackBehaviour.Pop1_pop1 << 24,
			1 << 0 | (byte) OpCodeType.Primitive << 8 | (byte) OperandType.InlineNone << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Rem = new OpCode (
			0xFF << 0 | 0x5D << 8 | (byte) StackBehaviour.Push1 << 16 | (byte) StackBehaviour.Pop1_pop1 << 24,
			1 << 0 | (byte) OpCodeType.Primitive << 8 | (byte) OperandType.InlineNone << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Rem_Un = new OpCode (
			0xFF << 0 | 0x5E << 8 | (byte) StackBehaviour.Push1 << 16 | (byte) StackBehaviour.Pop1_pop1 << 24,
			1 << 0 | (byte) OpCodeType.Primitive << 8 | (byte) OperandType.InlineNone << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode And = new OpCode (
			0xFF << 0 | 0x5F << 8 | (byte) StackBehaviour.Push1 << 16 | (byte) StackBehaviour.Pop1_pop1 << 24,
			1 << 0 | (byte) OpCodeType.Primitive << 8 | (byte) OperandType.InlineNone << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Or = new OpCode (
			0xFF << 0 | 0x60 << 8 | (byte) StackBehaviour.Push1 << 16 | (byte) StackBehaviour.Pop1_pop1 << 24,
			1 << 0 | (byte) OpCodeType.Primitive << 8 | (byte) OperandType.InlineNone << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Xor = new OpCode (
			0xFF << 0 | 0x61 << 8 | (byte) StackBehaviour.Push1 << 16 | (byte) StackBehaviour.Pop1_pop1 << 24,
			1 << 0 | (byte) OpCodeType.Primitive << 8 | (byte) OperandType.InlineNone << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Shl = new OpCode (
			0xFF << 0 | 0x62 << 8 | (byte) StackBehaviour.Push1 << 16 | (byte) StackBehaviour.Pop1_pop1 << 24,
			1 << 0 | (byte) OpCodeType.Primitive << 8 | (byte) OperandType.InlineNone << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Shr = new OpCode (
			0xFF << 0 | 0x63 << 8 | (byte) StackBehaviour.Push1 << 16 | (byte) StackBehaviour.Pop1_pop1 << 24,
			1 << 0 | (byte) OpCodeType.Primitive << 8 | (byte) OperandType.InlineNone << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Shr_Un = new OpCode (
			0xFF << 0 | 0x64 << 8 | (byte) StackBehaviour.Push1 << 16 | (byte) StackBehaviour.Pop1_pop1 << 24,
			1 << 0 | (byte) OpCodeType.Primitive << 8 | (byte) OperandType.InlineNone << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Neg = new OpCode (
			0xFF << 0 | 0x65 << 8 | (byte) StackBehaviour.Push1 << 16 | (byte) StackBehaviour.Pop1 << 24,
			1 << 0 | (byte) OpCodeType.Primitive << 8 | (byte) OperandType.InlineNone << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Not = new OpCode (
			0xFF << 0 | 0x66 << 8 | (byte) StackBehaviour.Push1 << 16 | (byte) StackBehaviour.Pop1 << 24,
			1 << 0 | (byte) OpCodeType.Primitive << 8 | (byte) OperandType.InlineNone << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Conv_I1 = new OpCode (
			0xFF << 0 | 0x67 << 8 | (byte) StackBehaviour.Pushi << 16 | (byte) StackBehaviour.Pop1 << 24,
			1 << 0 | (byte) OpCodeType.Primitive << 8 | (byte) OperandType.InlineNone << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Conv_I2 = new OpCode (
			0xFF << 0 | 0x68 << 8 | (byte) StackBehaviour.Pushi << 16 | (byte) StackBehaviour.Pop1 << 24,
			1 << 0 | (byte) OpCodeType.Primitive << 8 | (byte) OperandType.InlineNone << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Conv_I4 = new OpCode (
			0xFF << 0 | 0x69 << 8 | (byte) StackBehaviour.Pushi << 16 | (byte) StackBehaviour.Pop1 << 24,
			1 << 0 | (byte) OpCodeType.Primitive << 8 | (byte) OperandType.InlineNone << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Conv_I8 = new OpCode (
			0xFF << 0 | 0x6A << 8 | (byte) StackBehaviour.Pushi8 << 16 | (byte) StackBehaviour.Pop1 << 24,
			1 << 0 | (byte) OpCodeType.Primitive << 8 | (byte) OperandType.InlineNone << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Conv_R4 = new OpCode (
			0xFF << 0 | 0x6B << 8 | (byte) StackBehaviour.Pushr4 << 16 | (byte) StackBehaviour.Pop1 << 24,
			1 << 0 | (byte) OpCodeType.Primitive << 8 | (byte) OperandType.InlineNone << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Conv_R8 = new OpCode (
			0xFF << 0 | 0x6C << 8 | (byte) StackBehaviour.Pushr8 << 16 | (byte) StackBehaviour.Pop1 << 24,
			1 << 0 | (byte) OpCodeType.Primitive << 8 | (byte) OperandType.InlineNone << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Conv_U4 = new OpCode (
			0xFF << 0 | 0x6D << 8 | (byte) StackBehaviour.Pushi << 16 | (byte) StackBehaviour.Pop1 << 24,
			1 << 0 | (byte) OpCodeType.Primitive << 8 | (byte) OperandType.InlineNone << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Conv_U8 = new OpCode (
			0xFF << 0 | 0x6E << 8 | (byte) StackBehaviour.Pushi8 << 16 | (byte) StackBehaviour.Pop1 << 24,
			1 << 0 | (byte) OpCodeType.Primitive << 8 | (byte) OperandType.InlineNone << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Callvirt = new OpCode (
			0xFF << 0 | 0x6F << 8 | (byte) StackBehaviour.Varpush << 16 | (byte) StackBehaviour.Varpop << 24,
			1 << 0 | (byte) OpCodeType.Objmodel << 8 | (byte) OperandType.InlineMethod << 16 | (byte) FlowControl.Call << 24);

		public static readonly OpCode Cpobj = new OpCode (
			0xFF << 0 | 0x70 << 8 | (byte) StackBehaviour.Push0 << 16 | (byte) StackBehaviour.Popi_popi << 24,
			1 << 0 | (byte) OpCodeType.Objmodel << 8 | (byte) OperandType.InlineType << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Ldobj = new OpCode (
			0xFF << 0 | 0x71 << 8 | (byte) StackBehaviour.Push1 << 16 | (byte) StackBehaviour.Popi << 24,
			1 << 0 | (byte) OpCodeType.Objmodel << 8 | (byte) OperandType.InlineType << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Ldstr = new OpCode (
			0xFF << 0 | 0x72 << 8 | (byte) StackBehaviour.Pushref << 16 | (byte) StackBehaviour.Pop0 << 24,
			1 << 0 | (byte) OpCodeType.Objmodel << 8 | (byte) OperandType.InlineString << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Newobj = new OpCode (
			0xFF << 0 | 0x73 << 8 | (byte) StackBehaviour.Pushref << 16 | (byte) StackBehaviour.Varpop << 24,
			1 << 0 | (byte) OpCodeType.Objmodel << 8 | (byte) OperandType.InlineMethod << 16 | (byte) FlowControl.Call << 24);

	[ComVisible (true)]
		public static readonly OpCode Castclass = new OpCode (
			0xFF << 0 | 0x74 << 8 | (byte) StackBehaviour.Pushref << 16 | (byte) StackBehaviour.Popref << 24,
			1 << 0 | (byte) OpCodeType.Objmodel << 8 | (byte) OperandType.InlineType << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Isinst = new OpCode (
			0xFF << 0 | 0x75 << 8 | (byte) StackBehaviour.Pushi << 16 | (byte) StackBehaviour.Popref << 24,
			1 << 0 | (byte) OpCodeType.Objmodel << 8 | (byte) OperandType.InlineType << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Conv_R_Un = new OpCode (
			0xFF << 0 | 0x76 << 8 | (byte) StackBehaviour.Pushr8 << 16 | (byte) StackBehaviour.Pop1 << 24,
			1 << 0 | (byte) OpCodeType.Primitive << 8 | (byte) OperandType.InlineNone << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Unbox = new OpCode (
			0xFF << 0 | 0x79 << 8 | (byte) StackBehaviour.Pushi << 16 | (byte) StackBehaviour.Popref << 24,
			1 << 0 | (byte) OpCodeType.Primitive << 8 | (byte) OperandType.InlineType << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Throw = new OpCode (
			0xFF << 0 | 0x7A << 8 | (byte) StackBehaviour.Push0 << 16 | (byte) StackBehaviour.Popref << 24,
			1 << 0 | (byte) OpCodeType.Objmodel << 8 | (byte) OperandType.InlineNone << 16 | (byte) FlowControl.Throw << 24);

		public static readonly OpCode Ldfld = new OpCode (
			0xFF << 0 | 0x7B << 8 | (byte) StackBehaviour.Push1 << 16 | (byte) StackBehaviour.Popref << 24,
			1 << 0 | (byte) OpCodeType.Objmodel << 8 | (byte) OperandType.InlineField << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Ldflda = new OpCode (
			0xFF << 0 | 0x7C << 8 | (byte) StackBehaviour.Pushi << 16 | (byte) StackBehaviour.Popref << 24,
			1 << 0 | (byte) OpCodeType.Objmodel << 8 | (byte) OperandType.InlineField << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Stfld = new OpCode (
			0xFF << 0 | 0x7D << 8 | (byte) StackBehaviour.Push0 << 16 | (byte) StackBehaviour.Popref_pop1 << 24,
			1 << 0 | (byte) OpCodeType.Objmodel << 8 | (byte) OperandType.InlineField << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Ldsfld = new OpCode (
			0xFF << 0 | 0x7E << 8 | (byte) StackBehaviour.Push1 << 16 | (byte) StackBehaviour.Pop0 << 24,
			1 << 0 | (byte) OpCodeType.Objmodel << 8 | (byte) OperandType.InlineField << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Ldsflda = new OpCode (
			0xFF << 0 | 0x7F << 8 | (byte) StackBehaviour.Pushi << 16 | (byte) StackBehaviour.Pop0 << 24,
			1 << 0 | (byte) OpCodeType.Objmodel << 8 | (byte) OperandType.InlineField << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Stsfld = new OpCode (
			0xFF << 0 | 0x80 << 8 | (byte) StackBehaviour.Push0 << 16 | (byte) StackBehaviour.Pop1 << 24,
			1 << 0 | (byte) OpCodeType.Objmodel << 8 | (byte) OperandType.InlineField << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Stobj = new OpCode (
			0xFF << 0 | 0x81 << 8 | (byte) StackBehaviour.Push0 << 16 | (byte) StackBehaviour.Popi_pop1 << 24,
			1 << 0 | (byte) OpCodeType.Primitive << 8 | (byte) OperandType.InlineType << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Conv_Ovf_I1_Un = new OpCode (
			0xFF << 0 | 0x82 << 8 | (byte) StackBehaviour.Pushi << 16 | (byte) StackBehaviour.Pop1 << 24,
			1 << 0 | (byte) OpCodeType.Primitive << 8 | (byte) OperandType.InlineNone << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Conv_Ovf_I2_Un = new OpCode (
			0xFF << 0 | 0x83 << 8 | (byte) StackBehaviour.Pushi << 16 | (byte) StackBehaviour.Pop1 << 24,
			1 << 0 | (byte) OpCodeType.Primitive << 8 | (byte) OperandType.InlineNone << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Conv_Ovf_I4_Un = new OpCode (
			0xFF << 0 | 0x84 << 8 | (byte) StackBehaviour.Pushi << 16 | (byte) StackBehaviour.Pop1 << 24,
			1 << 0 | (byte) OpCodeType.Primitive << 8 | (byte) OperandType.InlineNone << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Conv_Ovf_I8_Un = new OpCode (
			0xFF << 0 | 0x85 << 8 | (byte) StackBehaviour.Pushi8 << 16 | (byte) StackBehaviour.Pop1 << 24,
			1 << 0 | (byte) OpCodeType.Primitive << 8 | (byte) OperandType.InlineNone << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Conv_Ovf_U1_Un = new OpCode (
			0xFF << 0 | 0x86 << 8 | (byte) StackBehaviour.Pushi << 16 | (byte) StackBehaviour.Pop1 << 24,
			1 << 0 | (byte) OpCodeType.Primitive << 8 | (byte) OperandType.InlineNone << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Conv_Ovf_U2_Un = new OpCode (
			0xFF << 0 | 0x87 << 8 | (byte) StackBehaviour.Pushi << 16 | (byte) StackBehaviour.Pop1 << 24,
			1 << 0 | (byte) OpCodeType.Primitive << 8 | (byte) OperandType.InlineNone << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Conv_Ovf_U4_Un = new OpCode (
			0xFF << 0 | 0x88 << 8 | (byte) StackBehaviour.Pushi << 16 | (byte) StackBehaviour.Pop1 << 24,
			1 << 0 | (byte) OpCodeType.Primitive << 8 | (byte) OperandType.InlineNone << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Conv_Ovf_U8_Un = new OpCode (
			0xFF << 0 | 0x89 << 8 | (byte) StackBehaviour.Pushi8 << 16 | (byte) StackBehaviour.Pop1 << 24,
			1 << 0 | (byte) OpCodeType.Primitive << 8 | (byte) OperandType.InlineNone << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Conv_Ovf_I_Un = new OpCode (
			0xFF << 0 | 0x8A << 8 | (byte) StackBehaviour.Pushi << 16 | (byte) StackBehaviour.Pop1 << 24,
			1 << 0 | (byte) OpCodeType.Primitive << 8 | (byte) OperandType.InlineNone << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Conv_Ovf_U_Un = new OpCode (
			0xFF << 0 | 0x8B << 8 | (byte) StackBehaviour.Pushi << 16 | (byte) StackBehaviour.Pop1 << 24,
			1 << 0 | (byte) OpCodeType.Primitive << 8 | (byte) OperandType.InlineNone << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Box = new OpCode (
			0xFF << 0 | 0x8C << 8 | (byte) StackBehaviour.Pushref << 16 | (byte) StackBehaviour.Pop1 << 24,
			1 << 0 | (byte) OpCodeType.Primitive << 8 | (byte) OperandType.InlineType << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Newarr = new OpCode (
			0xFF << 0 | 0x8D << 8 | (byte) StackBehaviour.Pushref << 16 | (byte) StackBehaviour.Popi << 24,
			1 << 0 | (byte) OpCodeType.Objmodel << 8 | (byte) OperandType.InlineType << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Ldlen = new OpCode (
			0xFF << 0 | 0x8E << 8 | (byte) StackBehaviour.Pushi << 16 | (byte) StackBehaviour.Popref << 24,
			1 << 0 | (byte) OpCodeType.Objmodel << 8 | (byte) OperandType.InlineNone << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Ldelema = new OpCode (
			0xFF << 0 | 0x8F << 8 | (byte) StackBehaviour.Pushi << 16 | (byte) StackBehaviour.Popref_popi << 24,
			1 << 0 | (byte) OpCodeType.Objmodel << 8 | (byte) OperandType.InlineType << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Ldelem_I1 = new OpCode (
			0xFF << 0 | 0x90 << 8 | (byte) StackBehaviour.Pushi << 16 | (byte) StackBehaviour.Popref_popi << 24,
			1 << 0 | (byte) OpCodeType.Objmodel << 8 | (byte) OperandType.InlineNone << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Ldelem_U1 = new OpCode (
			0xFF << 0 | 0x91 << 8 | (byte) StackBehaviour.Pushi << 16 | (byte) StackBehaviour.Popref_popi << 24,
			1 << 0 | (byte) OpCodeType.Objmodel << 8 | (byte) OperandType.InlineNone << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Ldelem_I2 = new OpCode (
			0xFF << 0 | 0x92 << 8 | (byte) StackBehaviour.Pushi << 16 | (byte) StackBehaviour.Popref_popi << 24,
			1 << 0 | (byte) OpCodeType.Objmodel << 8 | (byte) OperandType.InlineNone << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Ldelem_U2 = new OpCode (
			0xFF << 0 | 0x93 << 8 | (byte) StackBehaviour.Pushi << 16 | (byte) StackBehaviour.Popref_popi << 24,
			1 << 0 | (byte) OpCodeType.Objmodel << 8 | (byte) OperandType.InlineNone << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Ldelem_I4 = new OpCode (
			0xFF << 0 | 0x94 << 8 | (byte) StackBehaviour.Pushi << 16 | (byte) StackBehaviour.Popref_popi << 24,
			1 << 0 | (byte) OpCodeType.Objmodel << 8 | (byte) OperandType.InlineNone << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Ldelem_U4 = new OpCode (
			0xFF << 0 | 0x95 << 8 | (byte) StackBehaviour.Pushi << 16 | (byte) StackBehaviour.Popref_popi << 24,
			1 << 0 | (byte) OpCodeType.Objmodel << 8 | (byte) OperandType.InlineNone << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Ldelem_I8 = new OpCode (
			0xFF << 0 | 0x96 << 8 | (byte) StackBehaviour.Pushi8 << 16 | (byte) StackBehaviour.Popref_popi << 24,
			1 << 0 | (byte) OpCodeType.Objmodel << 8 | (byte) OperandType.InlineNone << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Ldelem_I = new OpCode (
			0xFF << 0 | 0x97 << 8 | (byte) StackBehaviour.Pushi << 16 | (byte) StackBehaviour.Popref_popi << 24,
			1 << 0 | (byte) OpCodeType.Objmodel << 8 | (byte) OperandType.InlineNone << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Ldelem_R4 = new OpCode (
			0xFF << 0 | 0x98 << 8 | (byte) StackBehaviour.Pushr4 << 16 | (byte) StackBehaviour.Popref_popi << 24,
			1 << 0 | (byte) OpCodeType.Objmodel << 8 | (byte) OperandType.InlineNone << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Ldelem_R8 = new OpCode (
			0xFF << 0 | 0x99 << 8 | (byte) StackBehaviour.Pushr8 << 16 | (byte) StackBehaviour.Popref_popi << 24,
			1 << 0 | (byte) OpCodeType.Objmodel << 8 | (byte) OperandType.InlineNone << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Ldelem_Ref = new OpCode (
			0xFF << 0 | 0x9A << 8 | (byte) StackBehaviour.Pushref << 16 | (byte) StackBehaviour.Popref_popi << 24,
			1 << 0 | (byte) OpCodeType.Objmodel << 8 | (byte) OperandType.InlineNone << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Stelem_I = new OpCode (
			0xFF << 0 | 0x9B << 8 | (byte) StackBehaviour.Push0 << 16 | (byte) StackBehaviour.Popref_popi_popi << 24,
			1 << 0 | (byte) OpCodeType.Objmodel << 8 | (byte) OperandType.InlineNone << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Stelem_I1 = new OpCode (
			0xFF << 0 | 0x9C << 8 | (byte) StackBehaviour.Push0 << 16 | (byte) StackBehaviour.Popref_popi_popi << 24,
			1 << 0 | (byte) OpCodeType.Objmodel << 8 | (byte) OperandType.InlineNone << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Stelem_I2 = new OpCode (
			0xFF << 0 | 0x9D << 8 | (byte) StackBehaviour.Push0 << 16 | (byte) StackBehaviour.Popref_popi_popi << 24,
			1 << 0 | (byte) OpCodeType.Objmodel << 8 | (byte) OperandType.InlineNone << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Stelem_I4 = new OpCode (
			0xFF << 0 | 0x9E << 8 | (byte) StackBehaviour.Push0 << 16 | (byte) StackBehaviour.Popref_popi_popi << 24,
			1 << 0 | (byte) OpCodeType.Objmodel << 8 | (byte) OperandType.InlineNone << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Stelem_I8 = new OpCode (
			0xFF << 0 | 0x9F << 8 | (byte) StackBehaviour.Push0 << 16 | (byte) StackBehaviour.Popref_popi_popi8 << 24,
			1 << 0 | (byte) OpCodeType.Objmodel << 8 | (byte) OperandType.InlineNone << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Stelem_R4 = new OpCode (
			0xFF << 0 | 0xA0 << 8 | (byte) StackBehaviour.Push0 << 16 | (byte) StackBehaviour.Popref_popi_popr4 << 24,
			1 << 0 | (byte) OpCodeType.Objmodel << 8 | (byte) OperandType.InlineNone << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Stelem_R8 = new OpCode (
			0xFF << 0 | 0xA1 << 8 | (byte) StackBehaviour.Push0 << 16 | (byte) StackBehaviour.Popref_popi_popr8 << 24,
			1 << 0 | (byte) OpCodeType.Objmodel << 8 | (byte) OperandType.InlineNone << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Stelem_Ref = new OpCode (
			0xFF << 0 | 0xA2 << 8 | (byte) StackBehaviour.Push0 << 16 | (byte) StackBehaviour.Popref_popi_popref << 24,
			1 << 0 | (byte) OpCodeType.Objmodel << 8 | (byte) OperandType.InlineNone << 16 | (byte) FlowControl.Next << 24);
		public static readonly OpCode Ldelem = new OpCode (
			0xFF << 0 | 0xA3 << 8 | (byte) StackBehaviour.Push1 << 16 | (byte) StackBehaviour.Popref_popi << 24,
			1 << 0 | (byte) OpCodeType.Objmodel << 8 | (byte) OperandType.InlineType << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Stelem = new OpCode (
			0xFF << 0 | 0xA4 << 8 | (byte) StackBehaviour.Push0 << 16 | (byte) StackBehaviour.Popref_popi_pop1 << 24,
			1 << 0 | (byte) OpCodeType.Objmodel << 8 | (byte) OperandType.InlineType << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Unbox_Any = new OpCode (
			0xFF << 0 | 0xA5 << 8 | (byte) StackBehaviour.Push1 << 16 | (byte) StackBehaviour.Popref << 24,
			1 << 0 | (byte) OpCodeType.Objmodel << 8 | (byte) OperandType.InlineType << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Conv_Ovf_I1 = new OpCode (
			0xFF << 0 | 0xB3 << 8 | (byte) StackBehaviour.Pushi << 16 | (byte) StackBehaviour.Pop1 << 24,
			1 << 0 | (byte) OpCodeType.Primitive << 8 | (byte) OperandType.InlineNone << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Conv_Ovf_U1 = new OpCode (
			0xFF << 0 | 0xB4 << 8 | (byte) StackBehaviour.Pushi << 16 | (byte) StackBehaviour.Pop1 << 24,
			1 << 0 | (byte) OpCodeType.Primitive << 8 | (byte) OperandType.InlineNone << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Conv_Ovf_I2 = new OpCode (
			0xFF << 0 | 0xB5 << 8 | (byte) StackBehaviour.Pushi << 16 | (byte) StackBehaviour.Pop1 << 24,
			1 << 0 | (byte) OpCodeType.Primitive << 8 | (byte) OperandType.InlineNone << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Conv_Ovf_U2 = new OpCode (
			0xFF << 0 | 0xB6 << 8 | (byte) StackBehaviour.Pushi << 16 | (byte) StackBehaviour.Pop1 << 24,
			1 << 0 | (byte) OpCodeType.Primitive << 8 | (byte) OperandType.InlineNone << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Conv_Ovf_I4 = new OpCode (
			0xFF << 0 | 0xB7 << 8 | (byte) StackBehaviour.Pushi << 16 | (byte) StackBehaviour.Pop1 << 24,
			1 << 0 | (byte) OpCodeType.Primitive << 8 | (byte) OperandType.InlineNone << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Conv_Ovf_U4 = new OpCode (
			0xFF << 0 | 0xB8 << 8 | (byte) StackBehaviour.Pushi << 16 | (byte) StackBehaviour.Pop1 << 24,
			1 << 0 | (byte) OpCodeType.Primitive << 8 | (byte) OperandType.InlineNone << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Conv_Ovf_I8 = new OpCode (
			0xFF << 0 | 0xB9 << 8 | (byte) StackBehaviour.Pushi8 << 16 | (byte) StackBehaviour.Pop1 << 24,
			1 << 0 | (byte) OpCodeType.Primitive << 8 | (byte) OperandType.InlineNone << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Conv_Ovf_U8 = new OpCode (
			0xFF << 0 | 0xBA << 8 | (byte) StackBehaviour.Pushi8 << 16 | (byte) StackBehaviour.Pop1 << 24,
			1 << 0 | (byte) OpCodeType.Primitive << 8 | (byte) OperandType.InlineNone << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Refanyval = new OpCode (
			0xFF << 0 | 0xC2 << 8 | (byte) StackBehaviour.Pushi << 16 | (byte) StackBehaviour.Pop1 << 24,
			1 << 0 | (byte) OpCodeType.Primitive << 8 | (byte) OperandType.InlineType << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Ckfinite = new OpCode (
			0xFF << 0 | 0xC3 << 8 | (byte) StackBehaviour.Pushr8 << 16 | (byte) StackBehaviour.Pop1 << 24,
			1 << 0 | (byte) OpCodeType.Primitive << 8 | (byte) OperandType.InlineNone << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Mkrefany = new OpCode (
			0xFF << 0 | 0xC6 << 8 | (byte) StackBehaviour.Push1 << 16 | (byte) StackBehaviour.Popi << 24,
			1 << 0 | (byte) OpCodeType.Primitive << 8 | (byte) OperandType.InlineType << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Ldtoken = new OpCode (
			0xFF << 0 | 0xD0 << 8 | (byte) StackBehaviour.Pushi << 16 | (byte) StackBehaviour.Pop0 << 24,
			1 << 0 | (byte) OpCodeType.Primitive << 8 | (byte) OperandType.InlineTok << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Conv_U2 = new OpCode (
			0xFF << 0 | 0xD1 << 8 | (byte) StackBehaviour.Pushi << 16 | (byte) StackBehaviour.Pop1 << 24,
			1 << 0 | (byte) OpCodeType.Primitive << 8 | (byte) OperandType.InlineNone << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Conv_U1 = new OpCode (
			0xFF << 0 | 0xD2 << 8 | (byte) StackBehaviour.Pushi << 16 | (byte) StackBehaviour.Pop1 << 24,
			1 << 0 | (byte) OpCodeType.Primitive << 8 | (byte) OperandType.InlineNone << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Conv_I = new OpCode (
			0xFF << 0 | 0xD3 << 8 | (byte) StackBehaviour.Pushi << 16 | (byte) StackBehaviour.Pop1 << 24,
			1 << 0 | (byte) OpCodeType.Primitive << 8 | (byte) OperandType.InlineNone << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Conv_Ovf_I = new OpCode (
			0xFF << 0 | 0xD4 << 8 | (byte) StackBehaviour.Pushi << 16 | (byte) StackBehaviour.Pop1 << 24,
			1 << 0 | (byte) OpCodeType.Primitive << 8 | (byte) OperandType.InlineNone << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Conv_Ovf_U = new OpCode (
			0xFF << 0 | 0xD5 << 8 | (byte) StackBehaviour.Pushi << 16 | (byte) StackBehaviour.Pop1 << 24,
			1 << 0 | (byte) OpCodeType.Primitive << 8 | (byte) OperandType.InlineNone << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Add_Ovf = new OpCode (
			0xFF << 0 | 0xD6 << 8 | (byte) StackBehaviour.Push1 << 16 | (byte) StackBehaviour.Pop1_pop1 << 24,
			1 << 0 | (byte) OpCodeType.Primitive << 8 | (byte) OperandType.InlineNone << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Add_Ovf_Un = new OpCode (
			0xFF << 0 | 0xD7 << 8 | (byte) StackBehaviour.Push1 << 16 | (byte) StackBehaviour.Pop1_pop1 << 24,
			1 << 0 | (byte) OpCodeType.Primitive << 8 | (byte) OperandType.InlineNone << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Mul_Ovf = new OpCode (
			0xFF << 0 | 0xD8 << 8 | (byte) StackBehaviour.Push1 << 16 | (byte) StackBehaviour.Pop1_pop1 << 24,
			1 << 0 | (byte) OpCodeType.Primitive << 8 | (byte) OperandType.InlineNone << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Mul_Ovf_Un = new OpCode (
			0xFF << 0 | 0xD9 << 8 | (byte) StackBehaviour.Push1 << 16 | (byte) StackBehaviour.Pop1_pop1 << 24,
			1 << 0 | (byte) OpCodeType.Primitive << 8 | (byte) OperandType.InlineNone << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Sub_Ovf = new OpCode (
			0xFF << 0 | 0xDA << 8 | (byte) StackBehaviour.Push1 << 16 | (byte) StackBehaviour.Pop1_pop1 << 24,
			1 << 0 | (byte) OpCodeType.Primitive << 8 | (byte) OperandType.InlineNone << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Sub_Ovf_Un = new OpCode (
			0xFF << 0 | 0xDB << 8 | (byte) StackBehaviour.Push1 << 16 | (byte) StackBehaviour.Pop1_pop1 << 24,
			1 << 0 | (byte) OpCodeType.Primitive << 8 | (byte) OperandType.InlineNone << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Endfinally = new OpCode (
			0xFF << 0 | 0xDC << 8 | (byte) StackBehaviour.Push0 << 16 | (byte) StackBehaviour.Pop0 << 24,
			1 << 0 | (byte) OpCodeType.Primitive << 8 | (byte) OperandType.InlineNone << 16 | (byte) FlowControl.Return << 24);

		public static readonly OpCode Leave = new OpCode (
			0xFF << 0 | 0xDD << 8 | (byte) StackBehaviour.Push0 << 16 | (byte) StackBehaviour.Pop0 << 24,
			1 << 0 | (byte) OpCodeType.Primitive << 8 | (byte) OperandType.InlineBrTarget << 16 | (byte) FlowControl.Branch << 24);

		public static readonly OpCode Leave_S = new OpCode (
			0xFF << 0 | 0xDE << 8 | (byte) StackBehaviour.Push0 << 16 | (byte) StackBehaviour.Pop0 << 24,
			1 << 0 | (byte) OpCodeType.Primitive << 8 | (byte) OperandType.ShortInlineBrTarget << 16 | (byte) FlowControl.Branch << 24);

		public static readonly OpCode Stind_I = new OpCode (
			0xFF << 0 | 0xDF << 8 | (byte) StackBehaviour.Push0 << 16 | (byte) StackBehaviour.Popi_popi << 24,
			1 << 0 | (byte) OpCodeType.Primitive << 8 | (byte) OperandType.InlineNone << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Conv_U = new OpCode (
			0xFF << 0 | 0xE0 << 8 | (byte) StackBehaviour.Pushi << 16 | (byte) StackBehaviour.Pop1 << 24,
			1 << 0 | (byte) OpCodeType.Primitive << 8 | (byte) OperandType.InlineNone << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Prefix7 = new OpCode (
			0xFF << 0 | 0xF8 << 8 | (byte) StackBehaviour.Push0 << 16 | (byte) StackBehaviour.Pop0 << 24,
			1 << 0 | (byte) OpCodeType.Nternal << 8 | (byte) OperandType.InlineNone << 16 | (byte) FlowControl.Meta << 24);

		public static readonly OpCode Prefix6 = new OpCode (
			0xFF << 0 | 0xF9 << 8 | (byte) StackBehaviour.Push0 << 16 | (byte) StackBehaviour.Pop0 << 24,
			1 << 0 | (byte) OpCodeType.Nternal << 8 | (byte) OperandType.InlineNone << 16 | (byte) FlowControl.Meta << 24);

		public static readonly OpCode Prefix5 = new OpCode (
			0xFF << 0 | 0xFA << 8 | (byte) StackBehaviour.Push0 << 16 | (byte) StackBehaviour.Pop0 << 24,
			1 << 0 | (byte) OpCodeType.Nternal << 8 | (byte) OperandType.InlineNone << 16 | (byte) FlowControl.Meta << 24);

		public static readonly OpCode Prefix4 = new OpCode (
			0xFF << 0 | 0xFB << 8 | (byte) StackBehaviour.Push0 << 16 | (byte) StackBehaviour.Pop0 << 24,
			1 << 0 | (byte) OpCodeType.Nternal << 8 | (byte) OperandType.InlineNone << 16 | (byte) FlowControl.Meta << 24);

		public static readonly OpCode Prefix3 = new OpCode (
			0xFF << 0 | 0xFC << 8 | (byte) StackBehaviour.Push0 << 16 | (byte) StackBehaviour.Pop0 << 24,
			1 << 0 | (byte) OpCodeType.Nternal << 8 | (byte) OperandType.InlineNone << 16 | (byte) FlowControl.Meta << 24);

		public static readonly OpCode Prefix2 = new OpCode (
			0xFF << 0 | 0xFD << 8 | (byte) StackBehaviour.Push0 << 16 | (byte) StackBehaviour.Pop0 << 24,
			1 << 0 | (byte) OpCodeType.Nternal << 8 | (byte) OperandType.InlineNone << 16 | (byte) FlowControl.Meta << 24);

		public static readonly OpCode Prefix1 = new OpCode (
			0xFF << 0 | 0xFE << 8 | (byte) StackBehaviour.Push0 << 16 | (byte) StackBehaviour.Pop0 << 24,
			1 << 0 | (byte) OpCodeType.Nternal << 8 | (byte) OperandType.InlineNone << 16 | (byte) FlowControl.Meta << 24);

		public static readonly OpCode Prefixref = new OpCode (
			0xFF << 0 | 0xFF << 8 | (byte) StackBehaviour.Push0 << 16 | (byte) StackBehaviour.Pop0 << 24,
			1 << 0 | (byte) OpCodeType.Nternal << 8 | (byte) OperandType.InlineNone << 16 | (byte) FlowControl.Meta << 24);

		public static readonly OpCode Arglist = new OpCode (
			0xFE << 0 | 0x00 << 8 | (byte) StackBehaviour.Pushi << 16 | (byte) StackBehaviour.Pop0 << 24,
			2 << 0 | (byte) OpCodeType.Primitive << 8 | (byte) OperandType.InlineNone << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Ceq = new OpCode (
			0xFE << 0 | 0x01 << 8 | (byte) StackBehaviour.Pushi << 16 | (byte) StackBehaviour.Pop1_pop1 << 24,
			2 << 0 | (byte) OpCodeType.Primitive << 8 | (byte) OperandType.InlineNone << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Cgt = new OpCode (
			0xFE << 0 | 0x02 << 8 | (byte) StackBehaviour.Pushi << 16 | (byte) StackBehaviour.Pop1_pop1 << 24,
			2 << 0 | (byte) OpCodeType.Primitive << 8 | (byte) OperandType.InlineNone << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Cgt_Un = new OpCode (
			0xFE << 0 | 0x03 << 8 | (byte) StackBehaviour.Pushi << 16 | (byte) StackBehaviour.Pop1_pop1 << 24,
			2 << 0 | (byte) OpCodeType.Primitive << 8 | (byte) OperandType.InlineNone << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Clt = new OpCode (
			0xFE << 0 | 0x04 << 8 | (byte) StackBehaviour.Pushi << 16 | (byte) StackBehaviour.Pop1_pop1 << 24,
			2 << 0 | (byte) OpCodeType.Primitive << 8 | (byte) OperandType.InlineNone << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Clt_Un = new OpCode (
			0xFE << 0 | 0x05 << 8 | (byte) StackBehaviour.Pushi << 16 | (byte) StackBehaviour.Pop1_pop1 << 24,
			2 << 0 | (byte) OpCodeType.Primitive << 8 | (byte) OperandType.InlineNone << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Ldftn = new OpCode (
			0xFE << 0 | 0x06 << 8 | (byte) StackBehaviour.Pushi << 16 | (byte) StackBehaviour.Pop0 << 24,
			2 << 0 | (byte) OpCodeType.Primitive << 8 | (byte) OperandType.InlineMethod << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Ldvirtftn = new OpCode (
			0xFE << 0 | 0x07 << 8 | (byte) StackBehaviour.Pushi << 16 | (byte) StackBehaviour.Popref << 24,
			2 << 0 | (byte) OpCodeType.Primitive << 8 | (byte) OperandType.InlineMethod << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Ldarg = new OpCode (
			0xFE << 0 | 0x09 << 8 | (byte) StackBehaviour.Push1 << 16 | (byte) StackBehaviour.Pop0 << 24,
			2 << 0 | (byte) OpCodeType.Primitive << 8 | (byte) OperandType.InlineVar << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Ldarga = new OpCode (
			0xFE << 0 | 0x0A << 8 | (byte) StackBehaviour.Pushi << 16 | (byte) StackBehaviour.Pop0 << 24,
			2 << 0 | (byte) OpCodeType.Primitive << 8 | (byte) OperandType.InlineVar << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Starg = new OpCode (
			0xFE << 0 | 0x0B << 8 | (byte) StackBehaviour.Push0 << 16 | (byte) StackBehaviour.Pop1 << 24,
			2 << 0 | (byte) OpCodeType.Primitive << 8 | (byte) OperandType.InlineVar << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Ldloc = new OpCode (
			0xFE << 0 | 0x0C << 8 | (byte) StackBehaviour.Push1 << 16 | (byte) StackBehaviour.Pop0 << 24,
			2 << 0 | (byte) OpCodeType.Primitive << 8 | (byte) OperandType.InlineVar << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Ldloca = new OpCode (
			0xFE << 0 | 0x0D << 8 | (byte) StackBehaviour.Pushi << 16 | (byte) StackBehaviour.Pop0 << 24,
			2 << 0 | (byte) OpCodeType.Primitive << 8 | (byte) OperandType.InlineVar << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Stloc = new OpCode (
			0xFE << 0 | 0x0E << 8 | (byte) StackBehaviour.Push0 << 16 | (byte) StackBehaviour.Pop1 << 24,
			2 << 0 | (byte) OpCodeType.Primitive << 8 | (byte) OperandType.InlineVar << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Localloc = new OpCode (
			0xFE << 0 | 0x0F << 8 | (byte) StackBehaviour.Pushi << 16 | (byte) StackBehaviour.Popi << 24,
			2 << 0 | (byte) OpCodeType.Primitive << 8 | (byte) OperandType.InlineNone << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Endfilter = new OpCode (
			0xFE << 0 | 0x11 << 8 | (byte) StackBehaviour.Push0 << 16 | (byte) StackBehaviour.Popi << 24,
			2 << 0 | (byte) OpCodeType.Primitive << 8 | (byte) OperandType.InlineNone << 16 | (byte) FlowControl.Return << 24);

		public static readonly OpCode Unaligned = new OpCode (
			0xFE << 0 | 0x12 << 8 | (byte) StackBehaviour.Push0 << 16 | (byte) StackBehaviour.Pop0 << 24,
			2 << 0 | (byte) OpCodeType.Prefix << 8 | (byte) OperandType.ShortInlineI << 16 | (byte) FlowControl.Meta << 24);

		public static readonly OpCode Volatile = new OpCode (
			0xFE << 0 | 0x13 << 8 | (byte) StackBehaviour.Push0 << 16 | (byte) StackBehaviour.Pop0 << 24,
			2 << 0 | (byte) OpCodeType.Prefix << 8 | (byte) OperandType.InlineNone << 16 | (byte) FlowControl.Meta << 24);

		public static readonly OpCode Tailcall = new OpCode (
			0xFE << 0 | 0x14 << 8 | (byte) StackBehaviour.Push0 << 16 | (byte) StackBehaviour.Pop0 << 24,
			2 << 0 | (byte) OpCodeType.Prefix << 8 | (byte) OperandType.InlineNone << 16 | (byte) FlowControl.Meta << 24);

		public static readonly OpCode Initobj = new OpCode (
			0xFE << 0 | 0x15 << 8 | (byte) StackBehaviour.Push0 << 16 | (byte) StackBehaviour.Popi << 24,
			2 << 0 | (byte) OpCodeType.Objmodel << 8 | (byte) OperandType.InlineType << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Constrained = new OpCode (
			0xFE << 0 | 0x16 << 8 | (byte) StackBehaviour.Push0 << 16 | (byte) StackBehaviour.Pop0 << 24,
			2 << 0 | (byte) OpCodeType.Prefix << 8 | (byte) OperandType.InlineType << 16 | (byte) FlowControl.Meta << 24);

		public static readonly OpCode Cpblk = new OpCode (
			0xFE << 0 | 0x17 << 8 | (byte) StackBehaviour.Push0 << 16 | (byte) StackBehaviour.Popi_popi_popi << 24,
			2 << 0 | (byte) OpCodeType.Primitive << 8 | (byte) OperandType.InlineNone << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Initblk = new OpCode (
			0xFE << 0 | 0x18 << 8 | (byte) StackBehaviour.Push0 << 16 | (byte) StackBehaviour.Popi_popi_popi << 24,
			2 << 0 | (byte) OpCodeType.Primitive << 8 | (byte) OperandType.InlineNone << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Rethrow = new OpCode (
			0xFE << 0 | 0x1A << 8 | (byte) StackBehaviour.Push0 << 16 | (byte) StackBehaviour.Pop0 << 24,
			2 << 0 | (byte) OpCodeType.Objmodel << 8 | (byte) OperandType.InlineNone << 16 | (byte) FlowControl.Throw << 24);

		public static readonly OpCode Sizeof = new OpCode (
			0xFE << 0 | 0x1C << 8 | (byte) StackBehaviour.Pushi << 16 | (byte) StackBehaviour.Pop0 << 24,
			2 << 0 | (byte) OpCodeType.Primitive << 8 | (byte) OperandType.InlineType << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Refanytype = new OpCode (
			0xFE << 0 | 0x1D << 8 | (byte) StackBehaviour.Pushi << 16 | (byte) StackBehaviour.Pop1 << 24,
			2 << 0 | (byte) OpCodeType.Primitive << 8 | (byte) OperandType.InlineNone << 16 | (byte) FlowControl.Next << 24);

		public static readonly OpCode Readonly = new OpCode (
			0xFE << 0 | 0x1E << 8 | (byte) StackBehaviour.Push0 << 16 | (byte) StackBehaviour.Pop0 << 24,
			2 << 0 | (byte) OpCodeType.Prefix << 8 | (byte) OperandType.InlineNone << 16 | (byte) FlowControl.Meta << 24);

                public static bool TakesSingleByteArgument (OpCode inst)
                {
                       OperandType t = inst.OperandType;
                       
                       // check for short-inline instructions
                       return t == OperandType.ShortInlineBrTarget
                       || t == OperandType.ShortInlineI
                           || t == OperandType.ShortInlineR
                           || t == OperandType.ShortInlineVar;
               }
	}
}
#endif
