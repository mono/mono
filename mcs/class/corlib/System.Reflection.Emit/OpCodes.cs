//
// System.Reflection.Emit.OpCodes.cs
//
// Authors:
//   Sergey Chaban (serge@wildwestsoftware.com)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) Sergey Chaban
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

namespace System.Reflection.Emit
{
	public class OpCodes
	{
		public static readonly OpCode Add = new OpCode ("add", 1, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Pop1_pop1, StackBehaviour.Push1, FlowControl.Next, 0xFF, 0x58);
		public static readonly OpCode Add_Ovf = new OpCode ("add.ovf", 1, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Pop1_pop1, StackBehaviour.Push1, FlowControl.Next, 0xFF, 0xD6);
		public static readonly OpCode Add_Ovf_Un = new OpCode ("add.ovf.un", 1, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Pop1_pop1, StackBehaviour.Push1, FlowControl.Next, 0xFF, 0xD7);
		public static readonly OpCode And = new OpCode ("and", 1, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Pop1_pop1, StackBehaviour.Push1, FlowControl.Next, 0xFF, 0x5F);
		public static readonly OpCode Arglist = new OpCode ("arglist", 2, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Pop0, StackBehaviour.Pushi, FlowControl.Next, 0xFE, 0x0);
		public static readonly OpCode Beq = new OpCode ("beq", 1, OpCodeType.Macro, OperandType.InlineBrTarget, StackBehaviour.Pop1_pop1, StackBehaviour.Push0, FlowControl.Cond_Branch, 0xFF, 0x3B);
		public static readonly OpCode Beq_S = new OpCode ("beq.s", 1, OpCodeType.Macro, OperandType.ShortInlineBrTarget, StackBehaviour.Pop1_pop1, StackBehaviour.Push0, FlowControl.Cond_Branch, 0xFF, 0x2E);
		public static readonly OpCode Bge = new OpCode ("bge", 1, OpCodeType.Macro, OperandType.InlineBrTarget, StackBehaviour.Pop1_pop1, StackBehaviour.Push0, FlowControl.Cond_Branch, 0xFF, 0x3C);
		public static readonly OpCode Bge_S = new OpCode ("bge.s", 1, OpCodeType.Macro, OperandType.ShortInlineBrTarget, StackBehaviour.Pop1_pop1, StackBehaviour.Push0, FlowControl.Cond_Branch, 0xFF, 0x2F);
		public static readonly OpCode Bge_Un = new OpCode ("bge.un", 1, OpCodeType.Macro, OperandType.InlineBrTarget, StackBehaviour.Pop1_pop1, StackBehaviour.Push0, FlowControl.Cond_Branch, 0xFF, 0x41);
		public static readonly OpCode Bge_Un_S = new OpCode ("bge.un.s", 1, OpCodeType.Macro, OperandType.ShortInlineBrTarget, StackBehaviour.Pop1_pop1, StackBehaviour.Push0, FlowControl.Cond_Branch, 0xFF, 0x34);
		public static readonly OpCode Bgt = new OpCode ("bgt", 1, OpCodeType.Macro, OperandType.InlineBrTarget, StackBehaviour.Pop1_pop1, StackBehaviour.Push0, FlowControl.Cond_Branch, 0xFF, 0x3D);
		public static readonly OpCode Bgt_S = new OpCode ("bgt.s", 1, OpCodeType.Macro, OperandType.ShortInlineBrTarget, StackBehaviour.Pop1_pop1, StackBehaviour.Push0, FlowControl.Cond_Branch, 0xFF, 0x30);
		public static readonly OpCode Bgt_Un = new OpCode ("bgt.un", 1, OpCodeType.Macro, OperandType.InlineBrTarget, StackBehaviour.Pop1_pop1, StackBehaviour.Push0, FlowControl.Cond_Branch, 0xFF, 0x42);
		public static readonly OpCode Bgt_Un_S = new OpCode ("bgt.un.s", 1, OpCodeType.Macro, OperandType.ShortInlineBrTarget, StackBehaviour.Pop1_pop1, StackBehaviour.Push0, FlowControl.Cond_Branch, 0xFF, 0x35);
		public static readonly OpCode Ble = new OpCode ("ble", 1, OpCodeType.Macro, OperandType.InlineBrTarget, StackBehaviour.Pop1_pop1, StackBehaviour.Push0, FlowControl.Cond_Branch, 0xFF, 0x3E);
		public static readonly OpCode Ble_S = new OpCode ("ble.s", 1, OpCodeType.Macro, OperandType.ShortInlineBrTarget, StackBehaviour.Pop1_pop1, StackBehaviour.Push0, FlowControl.Cond_Branch, 0xFF, 0x31);
		public static readonly OpCode Ble_Un = new OpCode ("ble.un", 1, OpCodeType.Macro, OperandType.InlineBrTarget, StackBehaviour.Pop1_pop1, StackBehaviour.Push0, FlowControl.Cond_Branch, 0xFF, 0x43);
		public static readonly OpCode Ble_Un_S = new OpCode ("ble.un.s", 1, OpCodeType.Macro, OperandType.ShortInlineBrTarget, StackBehaviour.Pop1_pop1, StackBehaviour.Push0, FlowControl.Cond_Branch, 0xFF, 0x36);
		public static readonly OpCode Blt = new OpCode ("blt", 1, OpCodeType.Macro, OperandType.InlineBrTarget, StackBehaviour.Pop1_pop1, StackBehaviour.Push0, FlowControl.Cond_Branch, 0xFF, 0x3F);
		public static readonly OpCode Blt_S = new OpCode ("blt.s", 1, OpCodeType.Macro, OperandType.ShortInlineBrTarget, StackBehaviour.Pop1_pop1, StackBehaviour.Push0, FlowControl.Cond_Branch, 0xFF, 0x32);
		public static readonly OpCode Blt_Un = new OpCode ("blt.un", 1, OpCodeType.Macro, OperandType.InlineBrTarget, StackBehaviour.Pop1_pop1, StackBehaviour.Push0, FlowControl.Cond_Branch, 0xFF, 0x44);
		public static readonly OpCode Blt_Un_S = new OpCode ("blt.un.s", 1, OpCodeType.Macro, OperandType.ShortInlineBrTarget, StackBehaviour.Pop1_pop1, StackBehaviour.Push0, FlowControl.Cond_Branch, 0xFF, 0x37);
		public static readonly OpCode Bne_Un = new OpCode ("bne.un", 1, OpCodeType.Macro, OperandType.InlineBrTarget, StackBehaviour.Pop1_pop1, StackBehaviour.Push0, FlowControl.Cond_Branch, 0xFF, 0x40);
		public static readonly OpCode Bne_Un_S = new OpCode ("bne.un.s", 1, OpCodeType.Macro, OperandType.ShortInlineBrTarget, StackBehaviour.Pop1_pop1, StackBehaviour.Push0, FlowControl.Cond_Branch, 0xFF, 0x33);
		public static readonly OpCode Box = new OpCode ("box", 1, OpCodeType.Primitive, OperandType.InlineType, StackBehaviour.Pop1, StackBehaviour.Pushref, FlowControl.Next, 0xFF, 0x8C);
#if NET_1_0
[Obsolete]	public static readonly OpCode Boxval = new OpCode ("boxval", 1, OpCodeType.Primitive, OperandType.InlineType, StackBehaviour.Pop1, StackBehaviour.Pushref, FlowControl.Next, 0xFF, 0x8C);
#endif
		public static readonly OpCode Br = new OpCode ("br", 1, OpCodeType.Primitive, OperandType.InlineBrTarget, StackBehaviour.Pop0, StackBehaviour.Push0, FlowControl.Branch, 0xFF, 0x38);
		public static readonly OpCode Br_S = new OpCode ("br.s", 1, OpCodeType.Macro, OperandType.ShortInlineBrTarget, StackBehaviour.Pop0, StackBehaviour.Push0, FlowControl.Branch, 0xFF, 0x2B);
		public static readonly OpCode Break = new OpCode ("break", 1, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Pop0, StackBehaviour.Push0, FlowControl.Break, 0xFF, 0x1);
		public static readonly OpCode Brfalse = new OpCode ("brfalse", 1, OpCodeType.Primitive, OperandType.InlineBrTarget, StackBehaviour.Popi, StackBehaviour.Push0, FlowControl.Cond_Branch, 0xFF, 0x39);
		public static readonly OpCode Brfalse_S = new OpCode ("brfalse.s", 1, OpCodeType.Macro, OperandType.ShortInlineBrTarget, StackBehaviour.Popi, StackBehaviour.Push0, FlowControl.Cond_Branch, 0xFF, 0x2C);
		public static readonly OpCode Brtrue = new OpCode ("brtrue", 1, OpCodeType.Primitive, OperandType.InlineBrTarget, StackBehaviour.Popi, StackBehaviour.Push0, FlowControl.Cond_Branch, 0xFF, 0x3A);
		public static readonly OpCode Brtrue_S = new OpCode ("brtrue.s", 1, OpCodeType.Macro, OperandType.ShortInlineBrTarget, StackBehaviour.Popi, StackBehaviour.Push0, FlowControl.Cond_Branch, 0xFF, 0x2D);
		public static readonly OpCode Call = new OpCode ("call", 1, OpCodeType.Primitive, OperandType.InlineMethod, StackBehaviour.Varpop, StackBehaviour.Varpush, FlowControl.Call, 0xFF, 0x28);
		public static readonly OpCode Calli = new OpCode ("calli", 1, OpCodeType.Primitive, OperandType.InlineSig, StackBehaviour.Varpop, StackBehaviour.Varpush, FlowControl.Call, 0xFF, 0x29);
		public static readonly OpCode Callvirt = new OpCode ("callvirt", 1, OpCodeType.Objmodel, OperandType.InlineMethod, StackBehaviour.Varpop, StackBehaviour.Varpush, FlowControl.Call, 0xFF, 0x6F);
		public static readonly OpCode Castclass = new OpCode ("castclass", 1, OpCodeType.Objmodel, OperandType.InlineType, StackBehaviour.Popref, StackBehaviour.Pushref, FlowControl.Next, 0xFF, 0x74);
		public static readonly OpCode Ceq = new OpCode ("ceq", 2, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Pop1_pop1, StackBehaviour.Pushi, FlowControl.Next, 0xFE, 0x1);
		public static readonly OpCode Cgt = new OpCode ("cgt", 2, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Pop1_pop1, StackBehaviour.Pushi, FlowControl.Next, 0xFE, 0x2);
		public static readonly OpCode Cgt_Un = new OpCode ("cgt.un", 2, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Pop1_pop1, StackBehaviour.Pushi, FlowControl.Next, 0xFE, 0x3);
		public static readonly OpCode Ckfinite = new OpCode ("ckfinite", 1, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Pop1, StackBehaviour.Pushr8, FlowControl.Next, 0xFF, 0xC3);
		public static readonly OpCode Clt = new OpCode ("clt", 2, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Pop1_pop1, StackBehaviour.Pushi, FlowControl.Next, 0xFE, 0x4);
		public static readonly OpCode Clt_Un = new OpCode ("clt.un", 2, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Pop1_pop1, StackBehaviour.Pushi, FlowControl.Next, 0xFE, 0x5);
#if NET_2_0 || BOOTSTRAP_NET_2_0
		public static readonly OpCode Constrained = new OpCode ("constrained.", 2, OpCodeType.Prefix, OperandType.InlineNone, StackBehaviour.Pop0, StackBehaviour.Push0, FlowControl.Meta, 0xFE, 0x16);
#endif
		public static readonly OpCode Conv_I = new OpCode ("conv.i", 1, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Pop1, StackBehaviour.Pushi, FlowControl.Next, 0xFF, 0xD3);
		public static readonly OpCode Conv_I1 = new OpCode ("conv.i1", 1, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Pop1, StackBehaviour.Pushi, FlowControl.Next, 0xFF, 0x67);
		public static readonly OpCode Conv_I2 = new OpCode ("conv.i2", 1, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Pop1, StackBehaviour.Pushi, FlowControl.Next, 0xFF, 0x68);
		public static readonly OpCode Conv_I4 = new OpCode ("conv.i4", 1, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Pop1, StackBehaviour.Pushi, FlowControl.Next, 0xFF, 0x69);
		public static readonly OpCode Conv_I8 = new OpCode ("conv.i8", 1, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Pop1, StackBehaviour.Pushi8, FlowControl.Next, 0xFF, 0x6A);
		public static readonly OpCode Conv_Ovf_I = new OpCode ("conv.ovf.i", 1, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Pop1, StackBehaviour.Pushi, FlowControl.Next, 0xFF, 0xD4);
		public static readonly OpCode Conv_Ovf_I_Un = new OpCode ("conv.ovf.i.un", 1, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Pop1, StackBehaviour.Pushi, FlowControl.Next, 0xFF, 0x8A);
		public static readonly OpCode Conv_Ovf_I1 = new OpCode ("conv.ovf.i1", 1, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Pop1, StackBehaviour.Pushi, FlowControl.Next, 0xFF, 0xB3);
		public static readonly OpCode Conv_Ovf_I1_Un = new OpCode ("conv.ovf.i1.un", 1, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Pop1, StackBehaviour.Pushi, FlowControl.Next, 0xFF, 0x82);
		public static readonly OpCode Conv_Ovf_I2 = new OpCode ("conv.ovf.i2", 1, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Pop1, StackBehaviour.Pushi, FlowControl.Next, 0xFF, 0xB5);
		public static readonly OpCode Conv_Ovf_I2_Un = new OpCode ("conv.ovf.i2.un", 1, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Pop1, StackBehaviour.Pushi, FlowControl.Next, 0xFF, 0x83);
		public static readonly OpCode Conv_Ovf_I4 = new OpCode ("conv.ovf.i4", 1, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Pop1, StackBehaviour.Pushi, FlowControl.Next, 0xFF, 0xB7);
		public static readonly OpCode Conv_Ovf_I4_Un = new OpCode ("conv.ovf.i4.un", 1, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Pop1, StackBehaviour.Pushi, FlowControl.Next, 0xFF, 0x84);
		public static readonly OpCode Conv_Ovf_I8 = new OpCode ("conv.ovf.i8", 1, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Pop1, StackBehaviour.Pushi8, FlowControl.Next, 0xFF, 0xB9);
		public static readonly OpCode Conv_Ovf_I8_Un = new OpCode ("conv.ovf.i8.un", 1, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Pop1, StackBehaviour.Pushi8, FlowControl.Next, 0xFF, 0x85);
		public static readonly OpCode Conv_Ovf_U = new OpCode ("conv.ovf.u", 1, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Pop1, StackBehaviour.Pushi, FlowControl.Next, 0xFF, 0xD5);
		public static readonly OpCode Conv_Ovf_U_Un = new OpCode ("conv.ovf.u.un", 1, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Pop1, StackBehaviour.Pushi, FlowControl.Next, 0xFF, 0x8B);
		public static readonly OpCode Conv_Ovf_U1 = new OpCode ("conv.ovf.u1", 1, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Pop1, StackBehaviour.Pushi, FlowControl.Next, 0xFF, 0xB4);
		public static readonly OpCode Conv_Ovf_U1_Un = new OpCode ("conv.ovf.u1.un", 1, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Pop1, StackBehaviour.Pushi, FlowControl.Next, 0xFF, 0x86);
		public static readonly OpCode Conv_Ovf_U2 = new OpCode ("conv.ovf.u2", 1, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Pop1, StackBehaviour.Pushi, FlowControl.Next, 0xFF, 0xB6);
		public static readonly OpCode Conv_Ovf_U2_Un = new OpCode ("conv.ovf.u2.un", 1, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Pop1, StackBehaviour.Pushi, FlowControl.Next, 0xFF, 0x87);
		public static readonly OpCode Conv_Ovf_U4 = new OpCode ("conv.ovf.u4", 1, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Pop1, StackBehaviour.Pushi, FlowControl.Next, 0xFF, 0xB8);
		public static readonly OpCode Conv_Ovf_U4_Un = new OpCode ("conv.ovf.u4.un", 1, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Pop1, StackBehaviour.Pushi, FlowControl.Next, 0xFF, 0x88);
		public static readonly OpCode Conv_Ovf_U8 = new OpCode ("conv.ovf.u8", 1, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Pop1, StackBehaviour.Pushi8, FlowControl.Next, 0xFF, 0xBA);
		public static readonly OpCode Conv_Ovf_U8_Un = new OpCode ("conv.ovf.u8.un", 1, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Pop1, StackBehaviour.Pushi8, FlowControl.Next, 0xFF, 0x89);
		public static readonly OpCode Conv_R_Un = new OpCode ("conv.r.un", 1, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Pop1, StackBehaviour.Pushr8, FlowControl.Next, 0xFF, 0x76);
		public static readonly OpCode Conv_R4 = new OpCode ("conv.r4", 1, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Pop1, StackBehaviour.Pushr4, FlowControl.Next, 0xFF, 0x6B);
		public static readonly OpCode Conv_R8 = new OpCode ("conv.r8", 1, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Pop1, StackBehaviour.Pushr8, FlowControl.Next, 0xFF, 0x6C);
		public static readonly OpCode Conv_U = new OpCode ("conv.u", 1, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Pop1, StackBehaviour.Pushi, FlowControl.Next, 0xFF, 0xE0);
		public static readonly OpCode Conv_U1 = new OpCode ("conv.u1", 1, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Pop1, StackBehaviour.Pushi, FlowControl.Next, 0xFF, 0xD2);
		public static readonly OpCode Conv_U2 = new OpCode ("conv.u2", 1, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Pop1, StackBehaviour.Pushi, FlowControl.Next, 0xFF, 0xD1);
		public static readonly OpCode Conv_U4 = new OpCode ("conv.u4", 1, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Pop1, StackBehaviour.Pushi, FlowControl.Next, 0xFF, 0x6D);
		public static readonly OpCode Conv_U8 = new OpCode ("conv.u8", 1, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Pop1, StackBehaviour.Pushi8, FlowControl.Next, 0xFF, 0x6E);
		public static readonly OpCode Cpblk = new OpCode ("cpblk", 2, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Popi_popi_popi, StackBehaviour.Push0, FlowControl.Next, 0xFE, 0x17);
		public static readonly OpCode Cpobj = new OpCode ("cpobj", 1, OpCodeType.Objmodel, OperandType.InlineType, StackBehaviour.Popi_popi, StackBehaviour.Push0, FlowControl.Next, 0xFF, 0x70);
		public static readonly OpCode Div = new OpCode ("div", 1, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Pop1_pop1, StackBehaviour.Push1, FlowControl.Next, 0xFF, 0x5B);
		public static readonly OpCode Div_Un = new OpCode ("div.un", 1, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Pop1_pop1, StackBehaviour.Push1, FlowControl.Next, 0xFF, 0x5C);
		public static readonly OpCode Dup = new OpCode ("dup", 1, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Pop1, StackBehaviour.Push1_push1, FlowControl.Next, 0xFF, 0x25);
		public static readonly OpCode Endfilter = new OpCode ("endfilter", 2, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Popi, StackBehaviour.Push0, FlowControl.Return, 0xFE, 0x11);
		public static readonly OpCode Endfinally = new OpCode ("endfinally", 1, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Pop0, StackBehaviour.Push0, FlowControl.Return, 0xFF, 0xDC);
		public static readonly OpCode Initblk = new OpCode ("initblk", 2, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Popi_popi_popi, StackBehaviour.Push0, FlowControl.Next, 0xFE, 0x18);
		public static readonly OpCode Initobj = new OpCode ("initobj", 2, OpCodeType.Objmodel, OperandType.InlineType, StackBehaviour.Popi, StackBehaviour.Push0, FlowControl.Next, 0xFE, 0x15);
		public static readonly OpCode Isinst = new OpCode ("isinst", 1, OpCodeType.Objmodel, OperandType.InlineType, StackBehaviour.Popref, StackBehaviour.Pushi, FlowControl.Next, 0xFF, 0x75);
		public static readonly OpCode Jmp = new OpCode ("jmp", 1, OpCodeType.Primitive, OperandType.InlineMethod, StackBehaviour.Pop0, StackBehaviour.Push0, FlowControl.Call, 0xFF, 0x27);
		public static readonly OpCode Ldarg = new OpCode ("ldarg", 2, OpCodeType.Primitive, OperandType.InlineVar, StackBehaviour.Pop0, StackBehaviour.Push1, FlowControl.Next, 0xFE, 0x9);
		public static readonly OpCode Ldarg_0 = new OpCode ("ldarg.0", 1, OpCodeType.Macro, OperandType.InlineNone, StackBehaviour.Pop0, StackBehaviour.Push1, FlowControl.Next, 0xFF, 0x2);
		public static readonly OpCode Ldarg_1 = new OpCode ("ldarg.1", 1, OpCodeType.Macro, OperandType.InlineNone, StackBehaviour.Pop0, StackBehaviour.Push1, FlowControl.Next, 0xFF, 0x3);
		public static readonly OpCode Ldarg_2 = new OpCode ("ldarg.2", 1, OpCodeType.Macro, OperandType.InlineNone, StackBehaviour.Pop0, StackBehaviour.Push1, FlowControl.Next, 0xFF, 0x4);
		public static readonly OpCode Ldarg_3 = new OpCode ("ldarg.3", 1, OpCodeType.Macro, OperandType.InlineNone, StackBehaviour.Pop0, StackBehaviour.Push1, FlowControl.Next, 0xFF, 0x5);
		public static readonly OpCode Ldarg_S = new OpCode ("ldarg.s", 1, OpCodeType.Macro, OperandType.ShortInlineVar, StackBehaviour.Pop0, StackBehaviour.Push1, FlowControl.Next, 0xFF, 0xE);
		public static readonly OpCode Ldarga = new OpCode ("ldarga", 2, OpCodeType.Primitive, OperandType.InlineVar, StackBehaviour.Pop0, StackBehaviour.Pushi, FlowControl.Next, 0xFE, 0xA);
		public static readonly OpCode Ldarga_S = new OpCode ("ldarga.s", 1, OpCodeType.Macro, OperandType.ShortInlineVar, StackBehaviour.Pop0, StackBehaviour.Pushi, FlowControl.Next, 0xFF, 0xF);
		public static readonly OpCode Ldc_I4 = new OpCode ("ldc.i4", 1, OpCodeType.Primitive, OperandType.InlineI, StackBehaviour.Pop0, StackBehaviour.Pushi, FlowControl.Next, 0xFF, 0x20);
		public static readonly OpCode Ldc_I4_0 = new OpCode ("ldc.i4.0", 1, OpCodeType.Macro, OperandType.InlineNone, StackBehaviour.Pop0, StackBehaviour.Pushi, FlowControl.Next, 0xFF, 0x16);
		public static readonly OpCode Ldc_I4_1 = new OpCode ("ldc.i4.1", 1, OpCodeType.Macro, OperandType.InlineNone, StackBehaviour.Pop0, StackBehaviour.Pushi, FlowControl.Next, 0xFF, 0x17);
		public static readonly OpCode Ldc_I4_2 = new OpCode ("ldc.i4.2", 1, OpCodeType.Macro, OperandType.InlineNone, StackBehaviour.Pop0, StackBehaviour.Pushi, FlowControl.Next, 0xFF, 0x18);
		public static readonly OpCode Ldc_I4_3 = new OpCode ("ldc.i4.3", 1, OpCodeType.Macro, OperandType.InlineNone, StackBehaviour.Pop0, StackBehaviour.Pushi, FlowControl.Next, 0xFF, 0x19);
		public static readonly OpCode Ldc_I4_4 = new OpCode ("ldc.i4.4", 1, OpCodeType.Macro, OperandType.InlineNone, StackBehaviour.Pop0, StackBehaviour.Pushi, FlowControl.Next, 0xFF, 0x1A);
		public static readonly OpCode Ldc_I4_5 = new OpCode ("ldc.i4.5", 1, OpCodeType.Macro, OperandType.InlineNone, StackBehaviour.Pop0, StackBehaviour.Pushi, FlowControl.Next, 0xFF, 0x1B);
		public static readonly OpCode Ldc_I4_6 = new OpCode ("ldc.i4.6", 1, OpCodeType.Macro, OperandType.InlineNone, StackBehaviour.Pop0, StackBehaviour.Pushi, FlowControl.Next, 0xFF, 0x1C);
		public static readonly OpCode Ldc_I4_7 = new OpCode ("ldc.i4.7", 1, OpCodeType.Macro, OperandType.InlineNone, StackBehaviour.Pop0, StackBehaviour.Pushi, FlowControl.Next, 0xFF, 0x1D);
		public static readonly OpCode Ldc_I4_8 = new OpCode ("ldc.i4.8", 1, OpCodeType.Macro, OperandType.InlineNone, StackBehaviour.Pop0, StackBehaviour.Pushi, FlowControl.Next, 0xFF, 0x1E);
		public static readonly OpCode Ldc_I4_M1 = new OpCode ("ldc.i4.m1", 1, OpCodeType.Macro, OperandType.InlineNone, StackBehaviour.Pop0, StackBehaviour.Pushi, FlowControl.Next, 0xFF, 0x15);
		public static readonly OpCode Ldc_I4_S = new OpCode ("ldc.i4.s", 1, OpCodeType.Macro, OperandType.ShortInlineI, StackBehaviour.Pop0, StackBehaviour.Pushi, FlowControl.Next, 0xFF, 0x1F);
		public static readonly OpCode Ldc_I8 = new OpCode ("ldc.i8", 1, OpCodeType.Primitive, OperandType.InlineI8, StackBehaviour.Pop0, StackBehaviour.Pushi8, FlowControl.Next, 0xFF, 0x21);
		public static readonly OpCode Ldc_R4 = new OpCode ("ldc.r4", 1, OpCodeType.Primitive, OperandType.ShortInlineR, StackBehaviour.Pop0, StackBehaviour.Pushr4, FlowControl.Next, 0xFF, 0x22);
		public static readonly OpCode Ldc_R8 = new OpCode ("ldc.r8", 1, OpCodeType.Primitive, OperandType.InlineR, StackBehaviour.Pop0, StackBehaviour.Pushr8, FlowControl.Next, 0xFF, 0x23);
		public static readonly OpCode Ldelem_I = new OpCode ("ldelem.i", 1, OpCodeType.Objmodel, OperandType.InlineNone, StackBehaviour.Popref_popi, StackBehaviour.Pushi, FlowControl.Next, 0xFF, 0x97);
		public static readonly OpCode Ldelem_I1 = new OpCode ("ldelem.i1", 1, OpCodeType.Objmodel, OperandType.InlineNone, StackBehaviour.Popref_popi, StackBehaviour.Pushi, FlowControl.Next, 0xFF, 0x90);
		public static readonly OpCode Ldelem_I2 = new OpCode ("ldelem.i2", 1, OpCodeType.Objmodel, OperandType.InlineNone, StackBehaviour.Popref_popi, StackBehaviour.Pushi, FlowControl.Next, 0xFF, 0x92);
		public static readonly OpCode Ldelem_I4 = new OpCode ("ldelem.i4", 1, OpCodeType.Objmodel, OperandType.InlineNone, StackBehaviour.Popref_popi, StackBehaviour.Pushi, FlowControl.Next, 0xFF, 0x94);
		public static readonly OpCode Ldelem_I8 = new OpCode ("ldelem.i8", 1, OpCodeType.Objmodel, OperandType.InlineNone, StackBehaviour.Popref_popi, StackBehaviour.Pushi8, FlowControl.Next, 0xFF, 0x96);
		public static readonly OpCode Ldelem_R4 = new OpCode ("ldelem.r4", 1, OpCodeType.Objmodel, OperandType.InlineNone, StackBehaviour.Popref_popi, StackBehaviour.Pushr4, FlowControl.Next, 0xFF, 0x98);
		public static readonly OpCode Ldelem_R8 = new OpCode ("ldelem.r8", 1, OpCodeType.Objmodel, OperandType.InlineNone, StackBehaviour.Popref_popi, StackBehaviour.Pushr8, FlowControl.Next, 0xFF, 0x99);
		public static readonly OpCode Ldelem_Ref = new OpCode ("ldelem.ref", 1, OpCodeType.Objmodel, OperandType.InlineNone, StackBehaviour.Popref_popi, StackBehaviour.Pushref, FlowControl.Next, 0xFF, 0x9A);
		public static readonly OpCode Ldelem_U1 = new OpCode ("ldelem.u1", 1, OpCodeType.Objmodel, OperandType.InlineNone, StackBehaviour.Popref_popi, StackBehaviour.Pushi, FlowControl.Next, 0xFF, 0x91);
		public static readonly OpCode Ldelem_U2 = new OpCode ("ldelem.u2", 1, OpCodeType.Objmodel, OperandType.InlineNone, StackBehaviour.Popref_popi, StackBehaviour.Pushi, FlowControl.Next, 0xFF, 0x93);
		public static readonly OpCode Ldelem_U4 = new OpCode ("ldelem.u4", 1, OpCodeType.Objmodel, OperandType.InlineNone, StackBehaviour.Popref_popi, StackBehaviour.Pushi, FlowControl.Next, 0xFF, 0x95);
		public static readonly OpCode Ldelema = new OpCode ("ldelema", 1, OpCodeType.Objmodel, OperandType.InlineType, StackBehaviour.Popref_popi, StackBehaviour.Pushi, FlowControl.Next, 0xFF, 0x8F);
#if NET_2_0 || BOOTSTRAP_NET_2_0
		public static readonly OpCode Ldelem_Any = new OpCode ("ldelem.any", 1, OpCodeType.Objmodel, OperandType.InlineType, StackBehaviour.Popref_popi, StackBehaviour.Push1, FlowControl.Next, 0xFF, 0xA3);
#endif
		public static readonly OpCode Ldfld = new OpCode ("ldfld", 1, OpCodeType.Objmodel, OperandType.InlineField, StackBehaviour.Popref, StackBehaviour.Push1, FlowControl.Next, 0xFF, 0x7B);
		public static readonly OpCode Ldflda = new OpCode ("ldflda", 1, OpCodeType.Objmodel, OperandType.InlineField, StackBehaviour.Popref, StackBehaviour.Pushi, FlowControl.Next, 0xFF, 0x7C);
		public static readonly OpCode Ldftn = new OpCode ("ldftn", 2, OpCodeType.Primitive, OperandType.InlineMethod, StackBehaviour.Pop0, StackBehaviour.Pushi, FlowControl.Next, 0xFE, 0x6);
		public static readonly OpCode Ldind_I = new OpCode ("ldind.i", 1, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Popi, StackBehaviour.Pushi, FlowControl.Next, 0xFF, 0x4D);
		public static readonly OpCode Ldind_I1 = new OpCode ("ldind.i1", 1, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Popi, StackBehaviour.Pushi, FlowControl.Next, 0xFF, 0x46);
		public static readonly OpCode Ldind_I2 = new OpCode ("ldind.i2", 1, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Popi, StackBehaviour.Pushi, FlowControl.Next, 0xFF, 0x48);
		public static readonly OpCode Ldind_I4 = new OpCode ("ldind.i4", 1, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Popi, StackBehaviour.Pushi, FlowControl.Next, 0xFF, 0x4A);
		public static readonly OpCode Ldind_I8 = new OpCode ("ldind.i8", 1, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Popi, StackBehaviour.Pushi8, FlowControl.Next, 0xFF, 0x4C);
		public static readonly OpCode Ldind_R4 = new OpCode ("ldind.r4", 1, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Popi, StackBehaviour.Pushr4, FlowControl.Next, 0xFF, 0x4E);
		public static readonly OpCode Ldind_R8 = new OpCode ("ldind.r8", 1, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Popi, StackBehaviour.Pushr8, FlowControl.Next, 0xFF, 0x4F);
		public static readonly OpCode Ldind_Ref = new OpCode ("ldind.ref", 1, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Popi, StackBehaviour.Pushref, FlowControl.Next, 0xFF, 0x50);
		public static readonly OpCode Ldind_U1 = new OpCode ("ldind.u1", 1, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Popi, StackBehaviour.Pushi, FlowControl.Next, 0xFF, 0x47);
		public static readonly OpCode Ldind_U2 = new OpCode ("ldind.u2", 1, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Popi, StackBehaviour.Pushi, FlowControl.Next, 0xFF, 0x49);
		public static readonly OpCode Ldind_U4 = new OpCode ("ldind.u4", 1, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Popi, StackBehaviour.Pushi, FlowControl.Next, 0xFF, 0x4B);
		public static readonly OpCode Ldlen = new OpCode ("ldlen", 1, OpCodeType.Objmodel, OperandType.InlineNone, StackBehaviour.Popref, StackBehaviour.Pushi, FlowControl.Next, 0xFF, 0x8E);
		public static readonly OpCode Ldloc = new OpCode ("ldloc", 2, OpCodeType.Primitive, OperandType.InlineVar, StackBehaviour.Pop0, StackBehaviour.Push1, FlowControl.Next, 0xFE, 0xC);
		public static readonly OpCode Ldloc_0 = new OpCode ("ldloc.0", 1, OpCodeType.Macro, OperandType.InlineNone, StackBehaviour.Pop0, StackBehaviour.Push1, FlowControl.Next, 0xFF, 0x6);
		public static readonly OpCode Ldloc_1 = new OpCode ("ldloc.1", 1, OpCodeType.Macro, OperandType.InlineNone, StackBehaviour.Pop0, StackBehaviour.Push1, FlowControl.Next, 0xFF, 0x7);
		public static readonly OpCode Ldloc_2 = new OpCode ("ldloc.2", 1, OpCodeType.Macro, OperandType.InlineNone, StackBehaviour.Pop0, StackBehaviour.Push1, FlowControl.Next, 0xFF, 0x8);
		public static readonly OpCode Ldloc_3 = new OpCode ("ldloc.3", 1, OpCodeType.Macro, OperandType.InlineNone, StackBehaviour.Pop0, StackBehaviour.Push1, FlowControl.Next, 0xFF, 0x9);
		public static readonly OpCode Ldloc_S = new OpCode ("ldloc.s", 1, OpCodeType.Macro, OperandType.ShortInlineVar, StackBehaviour.Pop0, StackBehaviour.Push1, FlowControl.Next, 0xFF, 0x11);
		public static readonly OpCode Ldloca = new OpCode ("ldloca", 2, OpCodeType.Primitive, OperandType.InlineVar, StackBehaviour.Pop0, StackBehaviour.Pushi, FlowControl.Next, 0xFE, 0xD);
		public static readonly OpCode Ldloca_S = new OpCode ("ldloca.s", 1, OpCodeType.Macro, OperandType.ShortInlineVar, StackBehaviour.Pop0, StackBehaviour.Pushi, FlowControl.Next, 0xFF, 0x12);
		public static readonly OpCode Ldnull = new OpCode ("ldnull", 1, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Pop0, StackBehaviour.Pushref, FlowControl.Next, 0xFF, 0x14);
		public static readonly OpCode Ldobj = new OpCode ("ldobj", 1, OpCodeType.Objmodel, OperandType.InlineType, StackBehaviour.Popi, StackBehaviour.Push1, FlowControl.Next, 0xFF, 0x71);
		public static readonly OpCode Ldsfld = new OpCode ("ldsfld", 1, OpCodeType.Objmodel, OperandType.InlineField, StackBehaviour.Pop0, StackBehaviour.Push1, FlowControl.Next, 0xFF, 0x7E);
		public static readonly OpCode Ldsflda = new OpCode ("ldsflda", 1, OpCodeType.Objmodel, OperandType.InlineField, StackBehaviour.Pop0, StackBehaviour.Pushi, FlowControl.Next, 0xFF, 0x7F);
		public static readonly OpCode Ldstr = new OpCode ("ldstr", 1, OpCodeType.Objmodel, OperandType.InlineString, StackBehaviour.Pop0, StackBehaviour.Pushref, FlowControl.Next, 0xFF, 0x72);
		public static readonly OpCode Ldtoken = new OpCode ("ldtoken", 1, OpCodeType.Primitive, OperandType.InlineTok, StackBehaviour.Pop0, StackBehaviour.Pushi, FlowControl.Next, 0xFF, 0xD0);
		public static readonly OpCode Ldvirtftn = new OpCode ("ldvirtftn", 2, OpCodeType.Primitive, OperandType.InlineMethod, StackBehaviour.Popref, StackBehaviour.Pushi, FlowControl.Next, 0xFE, 0x7);
		public static readonly OpCode Leave = new OpCode ("leave", 1, OpCodeType.Primitive, OperandType.InlineBrTarget, StackBehaviour.Pop0, StackBehaviour.Push0, FlowControl.Branch, 0xFF, 0xDD);
		public static readonly OpCode Leave_S = new OpCode ("leave.s", 1, OpCodeType.Primitive, OperandType.ShortInlineBrTarget, StackBehaviour.Pop0, StackBehaviour.Push0, FlowControl.Branch, 0xFF, 0xDE);
		public static readonly OpCode Localloc = new OpCode ("localloc", 2, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Popi, StackBehaviour.Pushi, FlowControl.Next, 0xFE, 0xF);
		public static readonly OpCode Mkrefany = new OpCode ("mkrefany", 1, OpCodeType.Primitive, OperandType.InlineType, StackBehaviour.Popi, StackBehaviour.Push1, FlowControl.Next, 0xFF, 0xC6);
		public static readonly OpCode Mul = new OpCode ("mul", 1, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Pop1_pop1, StackBehaviour.Push1, FlowControl.Next, 0xFF, 0x5A);
		public static readonly OpCode Mul_Ovf = new OpCode ("mul.ovf", 1, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Pop1_pop1, StackBehaviour.Push1, FlowControl.Next, 0xFF, 0xD8);
		public static readonly OpCode Mul_Ovf_Un = new OpCode ("mul.ovf.un", 1, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Pop1_pop1, StackBehaviour.Push1, FlowControl.Next, 0xFF, 0xD9);
		public static readonly OpCode Neg = new OpCode ("neg", 1, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Pop1, StackBehaviour.Push1, FlowControl.Next, 0xFF, 0x65);
		public static readonly OpCode Newarr = new OpCode ("newarr", 1, OpCodeType.Objmodel, OperandType.InlineType, StackBehaviour.Popi, StackBehaviour.Pushref, FlowControl.Next, 0xFF, 0x8D);
		public static readonly OpCode Newobj = new OpCode ("newobj", 1, OpCodeType.Objmodel, OperandType.InlineMethod, StackBehaviour.Varpop, StackBehaviour.Pushref, FlowControl.Call, 0xFF, 0x73);
		public static readonly OpCode Nop = new OpCode ("nop", 1, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Pop0, StackBehaviour.Push0, FlowControl.Next, 0xFF, 0x0);
		public static readonly OpCode Not = new OpCode ("not", 1, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Pop1, StackBehaviour.Push1, FlowControl.Next, 0xFF, 0x66);
		public static readonly OpCode Or = new OpCode ("or", 1, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Pop1_pop1, StackBehaviour.Push1, FlowControl.Next, 0xFF, 0x60);
		public static readonly OpCode Pop = new OpCode ("pop", 1, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Pop1, StackBehaviour.Push0, FlowControl.Next, 0xFF, 0x26);
		public static readonly OpCode Prefix1 = new OpCode ("prefix1", 1, OpCodeType.Nternal, OperandType.InlineNone, StackBehaviour.Pop0, StackBehaviour.Push0, FlowControl.Meta, 0xFF, 0xFE);
		public static readonly OpCode Prefix2 = new OpCode ("prefix2", 1, OpCodeType.Nternal, OperandType.InlineNone, StackBehaviour.Pop0, StackBehaviour.Push0, FlowControl.Meta, 0xFF, 0xFD);
		public static readonly OpCode Prefix3 = new OpCode ("prefix3", 1, OpCodeType.Nternal, OperandType.InlineNone, StackBehaviour.Pop0, StackBehaviour.Push0, FlowControl.Meta, 0xFF, 0xFC);
		public static readonly OpCode Prefix4 = new OpCode ("prefix4", 1, OpCodeType.Nternal, OperandType.InlineNone, StackBehaviour.Pop0, StackBehaviour.Push0, FlowControl.Meta, 0xFF, 0xFB);
		public static readonly OpCode Prefix5 = new OpCode ("prefix5", 1, OpCodeType.Nternal, OperandType.InlineNone, StackBehaviour.Pop0, StackBehaviour.Push0, FlowControl.Meta, 0xFF, 0xFA);
		public static readonly OpCode Prefix6 = new OpCode ("prefix6", 1, OpCodeType.Nternal, OperandType.InlineNone, StackBehaviour.Pop0, StackBehaviour.Push0, FlowControl.Meta, 0xFF, 0xF9);
		public static readonly OpCode Prefix7 = new OpCode ("prefix7", 1, OpCodeType.Nternal, OperandType.InlineNone, StackBehaviour.Pop0, StackBehaviour.Push0, FlowControl.Meta, 0xFF, 0xF8);
		public static readonly OpCode Prefixref = new OpCode ("prefixref", 1, OpCodeType.Nternal, OperandType.InlineNone, StackBehaviour.Pop0, StackBehaviour.Push0, FlowControl.Meta, 0xFF, 0xFF);
		public static readonly OpCode Refanytype = new OpCode ("refanytype", 2, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Pop1, StackBehaviour.Pushi, FlowControl.Next, 0xFE, 0x1D);
		public static readonly OpCode Refanyval = new OpCode ("refanyval", 1, OpCodeType.Primitive, OperandType.InlineType, StackBehaviour.Pop1, StackBehaviour.Pushi, FlowControl.Next, 0xFF, 0xC2);
		public static readonly OpCode Rem = new OpCode ("rem", 1, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Pop1_pop1, StackBehaviour.Push1, FlowControl.Next, 0xFF, 0x5D);
		public static readonly OpCode Rem_Un = new OpCode ("rem.un", 1, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Pop1_pop1, StackBehaviour.Push1, FlowControl.Next, 0xFF, 0x5E);
		public static readonly OpCode Ret = new OpCode ("ret", 1, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Varpop, StackBehaviour.Push0, FlowControl.Return, 0xFF, 0x2A);
		public static readonly OpCode Rethrow = new OpCode ("rethrow", 2, OpCodeType.Objmodel, OperandType.InlineNone, StackBehaviour.Pop0, StackBehaviour.Push0, FlowControl.Throw, 0xFE, 0x1A);
		public static readonly OpCode Shl = new OpCode ("shl", 1, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Pop1_pop1, StackBehaviour.Push1, FlowControl.Next, 0xFF, 0x62);
		public static readonly OpCode Shr = new OpCode ("shr", 1, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Pop1_pop1, StackBehaviour.Push1, FlowControl.Next, 0xFF, 0x63);
		public static readonly OpCode Shr_Un = new OpCode ("shr.un", 1, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Pop1_pop1, StackBehaviour.Push1, FlowControl.Next, 0xFF, 0x64);
		public static readonly OpCode Sizeof = new OpCode ("sizeof", 2, OpCodeType.Primitive, OperandType.InlineType, StackBehaviour.Pop0, StackBehaviour.Pushi, FlowControl.Next, 0xFE, 0x1C);
		public static readonly OpCode Starg = new OpCode ("starg", 2, OpCodeType.Primitive, OperandType.InlineVar, StackBehaviour.Pop1, StackBehaviour.Push0, FlowControl.Next, 0xFE, 0xB);
		public static readonly OpCode Starg_S = new OpCode ("starg.s", 1, OpCodeType.Macro, OperandType.ShortInlineVar, StackBehaviour.Pop1, StackBehaviour.Push0, FlowControl.Next, 0xFF, 0x10);
		public static readonly OpCode Stelem_I = new OpCode ("stelem.i", 1, OpCodeType.Objmodel, OperandType.InlineNone, StackBehaviour.Popref_popi_popi, StackBehaviour.Push0, FlowControl.Next, 0xFF, 0x9B);
		public static readonly OpCode Stelem_I1 = new OpCode ("stelem.i1", 1, OpCodeType.Objmodel, OperandType.InlineNone, StackBehaviour.Popref_popi_popi, StackBehaviour.Push0, FlowControl.Next, 0xFF, 0x9C);
		public static readonly OpCode Stelem_I2 = new OpCode ("stelem.i2", 1, OpCodeType.Objmodel, OperandType.InlineNone, StackBehaviour.Popref_popi_popi, StackBehaviour.Push0, FlowControl.Next, 0xFF, 0x9D);
		public static readonly OpCode Stelem_I4 = new OpCode ("stelem.i4", 1, OpCodeType.Objmodel, OperandType.InlineNone, StackBehaviour.Popref_popi_popi, StackBehaviour.Push0, FlowControl.Next, 0xFF, 0x9E);
		public static readonly OpCode Stelem_I8 = new OpCode ("stelem.i8", 1, OpCodeType.Objmodel, OperandType.InlineNone, StackBehaviour.Popref_popi_popi8, StackBehaviour.Push0, FlowControl.Next, 0xFF, 0x9F);
		public static readonly OpCode Stelem_R4 = new OpCode ("stelem.r4", 1, OpCodeType.Objmodel, OperandType.InlineNone, StackBehaviour.Popref_popi_popr4, StackBehaviour.Push0, FlowControl.Next, 0xFF, 0xA0);
		public static readonly OpCode Stelem_R8 = new OpCode ("stelem.r8", 1, OpCodeType.Objmodel, OperandType.InlineNone, StackBehaviour.Popref_popi_popr8, StackBehaviour.Push0, FlowControl.Next, 0xFF, 0xA1);
		public static readonly OpCode Stelem_Ref = new OpCode ("stelem.ref", 1, OpCodeType.Objmodel, OperandType.InlineNone, StackBehaviour.Popref_popi_popref, StackBehaviour.Push0, FlowControl.Next, 0xFF, 0xA2);
#if NET_2_0 || BOOTSTRAP_NET_2_0
		public static readonly OpCode Stelem_Any = new OpCode ("stelem.any", 1, OpCodeType.Objmodel, OperandType.InlineType, StackBehaviour.Popref_popi_popref, StackBehaviour.Push0, FlowControl.Next, 0xFF, 0xA4);
#endif
		public static readonly OpCode Stfld = new OpCode ("stfld", 1, OpCodeType.Objmodel, OperandType.InlineField, StackBehaviour.Popref_pop1, StackBehaviour.Push0, FlowControl.Next, 0xFF, 0x7D);
		public static readonly OpCode Stind_I = new OpCode ("stind.i", 1, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Popi_popi, StackBehaviour.Push0, FlowControl.Next, 0xFF, 0xDF);
		public static readonly OpCode Stind_I1 = new OpCode ("stind.i1", 1, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Popi_popi, StackBehaviour.Push0, FlowControl.Next, 0xFF, 0x52);
		public static readonly OpCode Stind_I2 = new OpCode ("stind.i2", 1, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Popi_popi, StackBehaviour.Push0, FlowControl.Next, 0xFF, 0x53);
		public static readonly OpCode Stind_I4 = new OpCode ("stind.i4", 1, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Popi_popi, StackBehaviour.Push0, FlowControl.Next, 0xFF, 0x54);
		public static readonly OpCode Stind_I8 = new OpCode ("stind.i8", 1, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Popi_popi8, StackBehaviour.Push0, FlowControl.Next, 0xFF, 0x55);
		public static readonly OpCode Stind_R4 = new OpCode ("stind.r4", 1, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Popi_popr4, StackBehaviour.Push0, FlowControl.Next, 0xFF, 0x56);
		public static readonly OpCode Stind_R8 = new OpCode ("stind.r8", 1, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Popi_popr8, StackBehaviour.Push0, FlowControl.Next, 0xFF, 0x57);
		public static readonly OpCode Stind_Ref = new OpCode ("stind.ref", 1, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Popi_popi, StackBehaviour.Push0, FlowControl.Next, 0xFF, 0x51);
		public static readonly OpCode Stloc = new OpCode ("stloc", 2, OpCodeType.Primitive, OperandType.InlineVar, StackBehaviour.Pop1, StackBehaviour.Push0, FlowControl.Next, 0xFE, 0xE);
		public static readonly OpCode Stloc_0 = new OpCode ("stloc.0", 1, OpCodeType.Macro, OperandType.InlineNone, StackBehaviour.Pop1, StackBehaviour.Push0, FlowControl.Next, 0xFF, 0xA);
		public static readonly OpCode Stloc_1 = new OpCode ("stloc.1", 1, OpCodeType.Macro, OperandType.InlineNone, StackBehaviour.Pop1, StackBehaviour.Push0, FlowControl.Next, 0xFF, 0xB);
		public static readonly OpCode Stloc_2 = new OpCode ("stloc.2", 1, OpCodeType.Macro, OperandType.InlineNone, StackBehaviour.Pop1, StackBehaviour.Push0, FlowControl.Next, 0xFF, 0xC);
		public static readonly OpCode Stloc_3 = new OpCode ("stloc.3", 1, OpCodeType.Macro, OperandType.InlineNone, StackBehaviour.Pop1, StackBehaviour.Push0, FlowControl.Next, 0xFF, 0xD);
		public static readonly OpCode Stloc_S = new OpCode ("stloc.s", 1, OpCodeType.Macro, OperandType.ShortInlineVar, StackBehaviour.Pop1, StackBehaviour.Push0, FlowControl.Next, 0xFF, 0x13);
		public static readonly OpCode Stobj = new OpCode ("stobj", 1, OpCodeType.Primitive, OperandType.InlineType, StackBehaviour.Popi_pop1, StackBehaviour.Push0, FlowControl.Next, 0xFF, 0x81);
		public static readonly OpCode Stsfld = new OpCode ("stsfld", 1, OpCodeType.Objmodel, OperandType.InlineField, StackBehaviour.Pop1, StackBehaviour.Push0, FlowControl.Next, 0xFF, 0x80);
		public static readonly OpCode Sub = new OpCode ("sub", 1, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Pop1_pop1, StackBehaviour.Push1, FlowControl.Next, 0xFF, 0x59);
		public static readonly OpCode Sub_Ovf = new OpCode ("sub.ovf", 1, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Pop1_pop1, StackBehaviour.Push1, FlowControl.Next, 0xFF, 0xDA);
		public static readonly OpCode Sub_Ovf_Un = new OpCode ("sub.ovf.un", 1, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Pop1_pop1, StackBehaviour.Push1, FlowControl.Next, 0xFF, 0xDB);
		public static readonly OpCode Switch = new OpCode ("switch", 1, OpCodeType.Primitive, OperandType.InlineSwitch, StackBehaviour.Popi, StackBehaviour.Push0, FlowControl.Cond_Branch, 0xFF, 0x45);
		public static readonly OpCode Tailcall = new OpCode ("tail.", 2, OpCodeType.Prefix, OperandType.InlineNone, StackBehaviour.Pop0, StackBehaviour.Push0, FlowControl.Meta, 0xFE, 0x14);
		public static readonly OpCode Throw = new OpCode ("throw", 1, OpCodeType.Objmodel, OperandType.InlineNone, StackBehaviour.Popref, StackBehaviour.Push0, FlowControl.Throw, 0xFF, 0x7A);
		public static readonly OpCode Unaligned = new OpCode ("unaligned.", 2, OpCodeType.Prefix, OperandType.ShortInlineI, StackBehaviour.Pop0, StackBehaviour.Push0, FlowControl.Meta, 0xFE, 0x12);
		public static readonly OpCode Unbox = new OpCode ("unbox", 1, OpCodeType.Primitive, OperandType.InlineType, StackBehaviour.Popref, StackBehaviour.Pushi, FlowControl.Next, 0xFF, 0x79);
#if NET_2_0 || BOOTSTRAP_NET_2_0
		public static readonly OpCode Unbox_Any = new OpCode ("unbox.any", 1, OpCodeType.Objmodel, OperandType.InlineType, StackBehaviour.Popref, StackBehaviour.Push1, FlowControl.Next, 0xFF, 0xA5);
#endif
		public static readonly OpCode Volatile = new OpCode ("volatile.", 2, OpCodeType.Prefix, OperandType.InlineNone, StackBehaviour.Pop0, StackBehaviour.Push0, FlowControl.Meta, 0xFE, 0x13);
		public static readonly OpCode Xor = new OpCode ("xor", 1, OpCodeType.Primitive, OperandType.InlineNone, StackBehaviour.Pop1_pop1, StackBehaviour.Push1, FlowControl.Next, 0xFF, 0x61);

		private OpCodes ()
		{
		}

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
