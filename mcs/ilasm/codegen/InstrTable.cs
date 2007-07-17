//
// Mono.ILASM.InstrTable
//
// Author(s):
//  Jackson Harper (Jackson@LatitudeGeo.com)
//
// (C) 2003 Jackson Harper, All rights reserved
//

using PEAPI;
using System;
using System.Collections;

namespace Mono.ILASM {

        public class InstrTable {

                private static Hashtable inst_table;

                static InstrTable ()
                {
                        CreateInstTable ();
                }

                public static ILToken GetToken (string str)
                {
                        return inst_table[str] as ILToken;
                }

                public static bool IsInstr (string str)
                {
                        return inst_table.Contains (str);
                }

                private static void CreateInstTable ()
                {
                        inst_table = new Hashtable ();

                        inst_table["nop"] = new ILToken (Token.INSTR_NONE, Op.nop);
                        inst_table["break"] = new ILToken (Token.INSTR_NONE, Op.breakOp);
                        inst_table["ldarg.0"] = new ILToken (Token.INSTR_NONE, Op.ldarg_0);
                        inst_table["ldarg.1"] = new ILToken (Token.INSTR_NONE, Op.ldarg_1);
                        inst_table["ldarg.2"] = new ILToken (Token.INSTR_NONE, Op.ldarg_2);
                        inst_table["ldarg.3"] = new ILToken (Token.INSTR_NONE, Op.ldarg_3);
                        inst_table["ldloc.0"] = new ILToken (Token.INSTR_NONE, Op.ldloc_0);
                        inst_table["ldloc.1"] = new ILToken (Token.INSTR_NONE, Op.ldloc_1);
                        inst_table["ldloc.2"] = new ILToken (Token.INSTR_NONE, Op.ldloc_2);
                        inst_table["ldloc.3"] = new ILToken (Token.INSTR_NONE, Op.ldloc_3);
                        inst_table["stloc.0"] = new ILToken (Token.INSTR_NONE, Op.stloc_0);
                        inst_table["stloc.1"] = new ILToken (Token.INSTR_NONE, Op.stloc_1);
                        inst_table["stloc.2"] = new ILToken (Token.INSTR_NONE, Op.stloc_2);
                        inst_table["stloc.3"] = new ILToken (Token.INSTR_NONE, Op.stloc_3);
                        inst_table["ldnull"] = new ILToken (Token.INSTR_NONE, Op.ldnull);
                        inst_table["ldc.i4.m1"] = new ILToken (Token.INSTR_NONE, Op.ldc_i4_m1);
                        inst_table["ldc.i4.M1"] = new ILToken (Token.INSTR_NONE, Op.ldc_i4_m1);
                        inst_table["ldc.i4.0"] = new ILToken (Token.INSTR_NONE, Op.ldc_i4_0);
                        inst_table["ldc.i4.1"] = new ILToken (Token.INSTR_NONE, Op.ldc_i4_1);
                        inst_table["ldc.i4.2"] = new ILToken (Token.INSTR_NONE, Op.ldc_i4_2);
                        inst_table["ldc.i4.3"] = new ILToken (Token.INSTR_NONE, Op.ldc_i4_3);
                        inst_table["ldc.i4.4"] = new ILToken (Token.INSTR_NONE, Op.ldc_i4_4);
                        inst_table["ldc.i4.5"] = new ILToken (Token.INSTR_NONE, Op.ldc_i4_5);
                        inst_table["ldc.i4.6"] = new ILToken (Token.INSTR_NONE, Op.ldc_i4_6);
                        inst_table["ldc.i4.7"] = new ILToken (Token.INSTR_NONE, Op.ldc_i4_7);
                        inst_table["ldc.i4.8"] = new ILToken (Token.INSTR_NONE, Op.ldc_i4_8);
                        inst_table["dup"] = new ILToken (Token.INSTR_NONE, Op.dup);
                        inst_table["pop"] = new ILToken (Token.INSTR_NONE, Op.pop);
                        inst_table["ret"] = new ILToken (Token.INSTR_NONE, Op.ret);
                        inst_table["ldind.i1"] = new ILToken (Token.INSTR_NONE, Op.ldind_i1);
                        inst_table["ldind.u1"] = new ILToken (Token.INSTR_NONE, Op.ldind_u1);
                        inst_table["ldind.i2"] = new ILToken (Token.INSTR_NONE, Op.ldind_i2);
                        inst_table["ldind.u2"] = new ILToken (Token.INSTR_NONE, Op.ldind_u2);
                        inst_table["ldind.i4"] = new ILToken (Token.INSTR_NONE, Op.ldind_i4);
                        inst_table["ldind.u4"] = new ILToken (Token.INSTR_NONE, Op.ldind_u4);
                        inst_table["ldind.i8"] = new ILToken (Token.INSTR_NONE, Op.ldind_i8);
                        inst_table["ldind.u8"] = new ILToken (Token.INSTR_NONE, Op.ldind_i8);
                        inst_table["ldind.i"] = new ILToken (Token.INSTR_NONE, Op.ldind_i);
                        inst_table["ldind.r4"] = new ILToken (Token.INSTR_NONE, Op.ldind_r4);
                        inst_table["ldind.r8"] = new ILToken (Token.INSTR_NONE, Op.ldind_r8);
                        inst_table["ldind.ref"] = new ILToken (Token.INSTR_NONE, Op.ldind_ref);
                        inst_table["stind.ref"] = new ILToken (Token.INSTR_NONE, Op.stind_ref);
                        inst_table["stind.i1"] = new ILToken (Token.INSTR_NONE, Op.stind_i1);
                        inst_table["stind.i2"] = new ILToken (Token.INSTR_NONE, Op.stind_i2);
                        inst_table["stind.i4"] = new ILToken (Token.INSTR_NONE, Op.stind_i4);
                        inst_table["stind.i8"] = new ILToken (Token.INSTR_NONE, Op.stind_i8);
                        inst_table["stind.r4"] = new ILToken (Token.INSTR_NONE, Op.stind_r4);
                        inst_table["stind.r8"] = new ILToken (Token.INSTR_NONE, Op.stind_r8);
                        inst_table["add"] = new ILToken (Token.INSTR_NONE, Op.add);
                        inst_table["sub"] = new ILToken (Token.INSTR_NONE, Op.sub);
                        inst_table["mul"] = new ILToken (Token.INSTR_NONE, Op.mul);
                        inst_table["div"] = new ILToken (Token.INSTR_NONE, Op.div);
                        inst_table["div.un"] = new ILToken (Token.INSTR_NONE, Op.div_un);
                        inst_table["rem"] = new ILToken (Token.INSTR_NONE, Op.rem);
                        inst_table["rem.un"] = new ILToken (Token.INSTR_NONE, Op.rem_un);
                        inst_table["and"] = new ILToken (Token.INSTR_NONE, Op.and);
                        inst_table["or"] = new ILToken (Token.INSTR_NONE, Op.or);
                        inst_table["xor"] = new ILToken (Token.INSTR_NONE, Op.xor);
                        inst_table["shl"] = new ILToken (Token.INSTR_NONE, Op.shl);
                        inst_table["shr"] = new ILToken (Token.INSTR_NONE, Op.shr);
                        inst_table["shr.un"] = new ILToken (Token.INSTR_NONE, Op.shr_un);
                        inst_table["neg"] = new ILToken (Token.INSTR_NONE, Op.neg);
                        inst_table["not"] = new ILToken (Token.INSTR_NONE, Op.not);
                        inst_table["conv.i1"] = new ILToken (Token.INSTR_NONE, Op.conv_i1);
                        inst_table["conv.i2"] = new ILToken (Token.INSTR_NONE, Op.conv_i2);
                        inst_table["conv.i4"] = new ILToken (Token.INSTR_NONE, Op.conv_i4);
                        inst_table["conv.i8"] = new ILToken (Token.INSTR_NONE, Op.conv_i8);
                        inst_table["conv.r4"] = new ILToken (Token.INSTR_NONE, Op.conv_r4);
                        inst_table["conv.r8"] = new ILToken (Token.INSTR_NONE, Op.conv_r8);
                        inst_table["conv.u4"] = new ILToken (Token.INSTR_NONE, Op.conv_u4);
                        inst_table["conv.u8"] = new ILToken (Token.INSTR_NONE, Op.conv_u8);
                        inst_table["conv.r.un"] = new ILToken (Token.INSTR_NONE, Op.conv_r_un);
                        inst_table["throw"] = new ILToken (Token.INSTR_NONE, Op.throwOp);
                        inst_table["conv.ovf.i1.un"] = new ILToken (Token.INSTR_NONE, Op.conv_ovf_i1_un);
                        inst_table["conv.ovf.i2.un"] = new ILToken (Token.INSTR_NONE, Op.conv_ovf_i2_un);
                        inst_table["conv.ovf.i4.un"] = new ILToken (Token.INSTR_NONE, Op.conv_ovf_i4_un);
                        inst_table["conv.ovf.i8.un"] = new ILToken (Token.INSTR_NONE, Op.conv_ovf_i8_un);
                        inst_table["conf.ovf.u1.un"] = new ILToken (Token.INSTR_NONE, Op.conf_ovf_u1_un);
                        inst_table["conv.ovf.u2.un"] = new ILToken (Token.INSTR_NONE, Op.conv_ovf_u2_un);
                        inst_table["conv.ovf.u4.un"] = new ILToken (Token.INSTR_NONE, Op.conv_ovf_u4_un);
                        inst_table["conv.ovf.u8.un"] = new ILToken (Token.INSTR_NONE, Op.conv_ovf_u8_un);
                        inst_table["conv.ovf.i.un"] = new ILToken (Token.INSTR_NONE, Op.conv_ovf_i_un);
                        inst_table["conv.ovf.u.un"] = new ILToken (Token.INSTR_NONE, Op.conv_ovf_u_un);
                        inst_table["ldlen"] = new ILToken (Token.INSTR_NONE, Op.ldlen);
                        inst_table["ldelem.i1"] = new ILToken (Token.INSTR_NONE, Op.ldelem_i1);
                        inst_table["ldelem.u1"] = new ILToken (Token.INSTR_NONE, Op.ldelem_u1);
                        inst_table["ldelem.i2"] = new ILToken (Token.INSTR_NONE, Op.ldelem_i2);
                        inst_table["ldelem.u2"] = new ILToken (Token.INSTR_NONE, Op.ldelem_u2);
                        inst_table["ldelem.i4"] = new ILToken (Token.INSTR_NONE, Op.ldelem_i4);
                        inst_table["ldelem.u4"] = new ILToken (Token.INSTR_NONE, Op.ldelem_u4);
                        inst_table["ldelem.i8"] = new ILToken (Token.INSTR_NONE, Op.ldelem_i8);
                        inst_table["ldelem.u8"] = new ILToken (Token.INSTR_NONE, Op.ldelem_i8);
                        inst_table["ldelem.i"] = new ILToken (Token.INSTR_NONE, Op.ldelem_i);
                        inst_table["ldelem.r4"] = new ILToken (Token.INSTR_NONE, Op.ldelem_r4);
                        inst_table["ldelem.r8"] = new ILToken (Token.INSTR_NONE, Op.ldelem_r8);
                        inst_table["ldelem.ref"] = new ILToken (Token.INSTR_NONE, Op.ldelem_ref);
                        inst_table["stelem.i"] = new ILToken (Token.INSTR_NONE, Op.stelem_i);
                        inst_table["stelem.i1"] = new ILToken (Token.INSTR_NONE, Op.stelem_i1);
                        inst_table["stelem.i2"] = new ILToken (Token.INSTR_NONE, Op.stelem_i2);
                        inst_table["stelem.i4"] = new ILToken (Token.INSTR_NONE, Op.stelem_i4);
                        inst_table["stelem.i8"] = new ILToken (Token.INSTR_NONE, Op.stelem_i8);
                        inst_table["stelem.r4"] = new ILToken (Token.INSTR_NONE, Op.stelem_r4);
                        inst_table["stelem.r8"] = new ILToken (Token.INSTR_NONE, Op.stelem_r8);
                        inst_table["stelem.ref"] = new ILToken (Token.INSTR_NONE, Op.stelem_ref);
                        inst_table["conv.ovf.i1"] = new ILToken (Token.INSTR_NONE, Op.conv_ovf_i1);
                        inst_table["conv.ovf.u1"] = new ILToken (Token.INSTR_NONE, Op.conv_ovf_u1);
                        inst_table["conv.ovf.i2"] = new ILToken (Token.INSTR_NONE, Op.conv_ovf_i2);
                        inst_table["conv.ovf.u2"] = new ILToken (Token.INSTR_NONE, Op.conv_ovf_u2);
                        inst_table["conv.ovf.i4"] = new ILToken (Token.INSTR_NONE, Op.conv_ovf_i4);
                        inst_table["conv.ovf.u4"] = new ILToken (Token.INSTR_NONE, Op.conv_ovf_u4);
                        inst_table["conv.ovf.i8"] = new ILToken (Token.INSTR_NONE, Op.conv_ovf_i8);
                        inst_table["conv.ovf.u8"] = new ILToken (Token.INSTR_NONE, Op.conv_ovf_u8);
                        inst_table["conv.ovf.u1.un"] = new ILToken (Token.INSTR_NONE, Op.conf_ovf_u1_un);
                        inst_table["conv.ovf.u2.un"] = new ILToken (Token.INSTR_NONE, Op.conv_ovf_u2_un);
                        inst_table["conv.ovf.u4.un"] = new ILToken (Token.INSTR_NONE, Op.conv_ovf_u4_un);
                        inst_table["conv.ovf.u8.un"] = new ILToken (Token.INSTR_NONE, Op.conv_ovf_u8_un);
                        inst_table["conv.ovf.i1.un"] = new ILToken (Token.INSTR_NONE, Op.conv_ovf_i1_un);
                        inst_table["conv.ovf.i2.un"] = new ILToken (Token.INSTR_NONE, Op.conv_ovf_i2_un);
                        inst_table["conv.ovf.i4.un"] = new ILToken (Token.INSTR_NONE, Op.conv_ovf_i4_un);
                        inst_table["conv.ovf.i8.un"] = new ILToken (Token.INSTR_NONE, Op.conv_ovf_i8_un);
                        inst_table["ckfinite"] = new ILToken (Token.INSTR_NONE, Op.ckfinite);
                        inst_table["conv.u2"] = new ILToken (Token.INSTR_NONE, Op.conv_u2);
                        inst_table["conv.u1"] = new ILToken (Token.INSTR_NONE, Op.conv_u1);
                        inst_table["conv.i"] = new ILToken (Token.INSTR_NONE, Op.conv_i);
                        inst_table["conv.ovf.i"] = new ILToken (Token.INSTR_NONE, Op.conv_ovf_i);
                        inst_table["conv.ovf.u"] = new ILToken (Token.INSTR_NONE, Op.conv_ovf_u);
                        inst_table["add.ovf"] = new ILToken (Token.INSTR_NONE, Op.add_ovf);
                        inst_table["add.ovf.un"] = new ILToken (Token.INSTR_NONE, Op.add_ovf_un);
                        inst_table["mul.ovf"] = new ILToken (Token.INSTR_NONE, Op.mul_ovf);
                        inst_table["mul.ovf.un"] = new ILToken (Token.INSTR_NONE, Op.mul_ovf_un);
                        inst_table["sub.ovf"] = new ILToken (Token.INSTR_NONE, Op.sub_ovf);
                        inst_table["sub.ovf.un"] = new ILToken (Token.INSTR_NONE, Op.sub_ovf_un);
                        inst_table["endfinally"] = new ILToken (Token.INSTR_NONE, Op.endfinally);
                        // endfault is really just an alias for endfinally
                        inst_table["endfault"] = new ILToken (Token.INSTR_NONE, Op.endfinally);
                        inst_table["stind.i"] = new ILToken (Token.INSTR_NONE, Op.stind_i);
                        inst_table["conv.u"] = new ILToken (Token.INSTR_NONE, Op.conv_u);
                        inst_table["arglist"] = new ILToken (Token.INSTR_NONE, Op.arglist);
                        inst_table["ceq"] = new ILToken (Token.INSTR_NONE, Op.ceq);
                        inst_table["cgt"] = new ILToken (Token.INSTR_NONE, Op.cgt);
                        inst_table["cgt.un"] = new ILToken (Token.INSTR_NONE, Op.cgt_un);
                        inst_table["clt"] = new ILToken (Token.INSTR_NONE, Op.clt);
                        inst_table["clt.un"] = new ILToken (Token.INSTR_NONE, Op.clt_un);
                        inst_table["localloc"] = new ILToken (Token.INSTR_NONE, Op.localloc);
                        inst_table["endfilter"] = new ILToken (Token.INSTR_NONE, Op.endfilter);
                        inst_table["volatile."] = new ILToken (Token.INSTR_NONE, Op.volatile_);
                        inst_table["tail."] = new ILToken (Token.INSTR_NONE, Op.tail_);
                        inst_table["cpblk"] = new ILToken (Token.INSTR_NONE, Op.cpblk);
                        inst_table["initblk"] = new ILToken (Token.INSTR_NONE, Op.initblk);
                        inst_table["rethrow"] = new ILToken (Token.INSTR_NONE, Op.rethrow);
                        inst_table["refanytype"] = new ILToken (Token.INSTR_NONE, Op.refanytype);
                        inst_table["readonly."] = new ILToken (Token.INSTR_NONE, Op.readonly_);

                        //
                        // Int operations
                        //

                        // param
                        inst_table["ldarg"] = new ILToken (Token.INSTR_PARAM, IntOp.ldarg);
                        inst_table["ldarga"] = new ILToken (Token.INSTR_PARAM, IntOp.ldarga);
                        inst_table["starg"] = new ILToken (Token.INSTR_PARAM, IntOp.starg);
                        inst_table["ldarg.s"] = new ILToken (Token.INSTR_PARAM, IntOp.ldarg_s);
                        inst_table["ldarga.s"] = new ILToken (Token.INSTR_PARAM, IntOp.ldarga_s);
                        inst_table["starg.s"] = new ILToken (Token.INSTR_PARAM, IntOp.starg_s);

                        // local
                        inst_table["ldloc"] = new ILToken (Token.INSTR_LOCAL, IntOp.ldloc);
                        inst_table["ldloca"] = new ILToken (Token.INSTR_LOCAL, IntOp.ldloca);
                        inst_table["stloc"] = new ILToken (Token.INSTR_LOCAL, IntOp.stloc);
                        inst_table["ldloc.s"] = new ILToken (Token.INSTR_LOCAL, IntOp.ldloc_s);
                        inst_table["ldloca.s"] = new ILToken (Token.INSTR_LOCAL, IntOp.ldloca_s);
                        inst_table["stloc.s"] = new ILToken (Token.INSTR_LOCAL, IntOp.stloc_s);

                        inst_table["ldc.i4.s"] = new ILToken (Token.INSTR_I, IntOp.ldc_i4_s);
                        inst_table["ldc.i4"] = new ILToken (Token.INSTR_I, IntOp.ldc_i4);
                        inst_table["unaligned."] =  new ILToken (Token.INSTR_I, IntOp.unaligned);

                        //
                        // Type operations
                        //

                        inst_table["cpobj"] = new ILToken (Token.INSTR_TYPE, TypeOp.cpobj);
                        inst_table["ldobj"] = new ILToken (Token.INSTR_TYPE, TypeOp.ldobj);
                        inst_table["castclass"] = new ILToken (Token.INSTR_TYPE, TypeOp.castclass);
                        inst_table["isinst"] = new ILToken (Token.INSTR_TYPE, TypeOp.isinst);
                        inst_table["unbox"] = new ILToken (Token.INSTR_TYPE, TypeOp.unbox);
                        inst_table["unbox.any"] = new ILToken (Token.INSTR_TYPE, TypeOp.unbox_any);
                        inst_table["stobj"] = new ILToken (Token.INSTR_TYPE, TypeOp.stobj);
                        inst_table["box"] = new ILToken (Token.INSTR_TYPE, TypeOp.box);
                        inst_table["newarr"] = new ILToken (Token.INSTR_TYPE, TypeOp.newarr);
                        inst_table["ldelema"] = new ILToken (Token.INSTR_TYPE, TypeOp.ldelema);
                        inst_table["refanyval"] = new ILToken (Token.INSTR_TYPE, TypeOp.refanyval);
                        inst_table["mkrefany"] = new ILToken (Token.INSTR_TYPE, TypeOp.mkrefany);
                        inst_table["initobj"] = new ILToken (Token.INSTR_TYPE, TypeOp.initobj);
                        inst_table["sizeof"] = new ILToken (Token.INSTR_TYPE, TypeOp.sizeOf);
                        inst_table["stelem"] = new ILToken (Token.INSTR_TYPE, TypeOp.stelem);
                        inst_table["ldelem"] = new ILToken (Token.INSTR_TYPE, TypeOp.ldelem);
                        inst_table["stelem.any"] = new ILToken (Token.INSTR_TYPE, TypeOp.stelem);
                        inst_table["ldelem.any"] = new ILToken (Token.INSTR_TYPE, TypeOp.ldelem);
                        inst_table["constrained."] = new ILToken (Token.INSTR_TYPE, TypeOp.constrained);

                        //
                        // MethodRef operations
                        //

                        inst_table["jmp"] = new ILToken (Token.INSTR_METHOD, MethodOp.jmp);
                        inst_table["call"] = new ILToken (Token.INSTR_METHOD, MethodOp.call);
                        inst_table["callvirt"] = new ILToken (Token.INSTR_METHOD, MethodOp.callvirt);
                        inst_table["newobj"] = new ILToken (Token.INSTR_METHOD, MethodOp.newobj);
                        inst_table["ldftn"] = new ILToken (Token.INSTR_METHOD, MethodOp.ldftn);
                        inst_table["ldvirtftn"] = new ILToken (Token.INSTR_METHOD, MethodOp.ldvirtfn);

                        //
                        // FieldRef instructions
                        //

                        inst_table["ldfld"] = new ILToken (Token.INSTR_FIELD, FieldOp.ldfld);
                        inst_table["ldflda"] = new ILToken (Token.INSTR_FIELD, FieldOp.ldflda);
                        inst_table["stfld"] = new ILToken (Token.INSTR_FIELD, FieldOp.stfld);
                        inst_table["ldsfld"] = new ILToken (Token.INSTR_FIELD, FieldOp.ldsfld);
                        inst_table["ldsflda"] = new ILToken (Token.INSTR_FIELD, FieldOp.ldsflda);
                        inst_table["stsfld"] = new ILToken (Token.INSTR_FIELD, FieldOp.stsfld);

                        //
                        // Branch Instructions
                        //

                        inst_table["br"] = new ILToken (Token.INSTR_BRTARGET, BranchOp.br);
                        inst_table["brfalse"] = new ILToken (Token.INSTR_BRTARGET, BranchOp.brfalse);
                        inst_table["brzero"] = new ILToken (Token.INSTR_BRTARGET, BranchOp.brfalse);
                        inst_table["brnull"] = new ILToken (Token.INSTR_BRTARGET, BranchOp.brfalse);
                        inst_table["brtrue"] = new ILToken (Token.INSTR_BRTARGET, BranchOp.brtrue);
                        inst_table["beq"] = new ILToken (Token.INSTR_BRTARGET, BranchOp.beq);
                        inst_table["bge"] = new ILToken (Token.INSTR_BRTARGET, BranchOp.bge);
                        inst_table["bgt"] = new ILToken (Token.INSTR_BRTARGET, BranchOp.bgt);
                        inst_table["ble"] = new ILToken (Token.INSTR_BRTARGET, BranchOp.ble);
                        inst_table["blt"] = new ILToken (Token.INSTR_BRTARGET, BranchOp.blt);
                        inst_table["bne.un"] = new ILToken (Token.INSTR_BRTARGET, BranchOp.bne_un);
                        inst_table["bge.un"] = new ILToken (Token.INSTR_BRTARGET, BranchOp.bge_un);
                        inst_table["bgt.un"] = new ILToken (Token.INSTR_BRTARGET, BranchOp.bgt_un);
                        inst_table["ble.un"] = new ILToken (Token.INSTR_BRTARGET, BranchOp.ble_un);
                        inst_table["blt.un"] = new ILToken (Token.INSTR_BRTARGET, BranchOp.blt_un);
                        inst_table["leave"] = new ILToken (Token.INSTR_BRTARGET, BranchOp.leave);

                        inst_table["br.s"] = new ILToken (Token.INSTR_BRTARGET, BranchOp.br_s);
                        inst_table["brfalse.s"] = new ILToken (Token.INSTR_BRTARGET, BranchOp.brfalse_s);
                        inst_table["brtrue.s"] = new ILToken (Token.INSTR_BRTARGET, BranchOp.brtrue_s);
                        inst_table["beq.s"] = new ILToken (Token.INSTR_BRTARGET, BranchOp.beq_s);
                        inst_table["bge.s"] = new ILToken (Token.INSTR_BRTARGET, BranchOp.bge_s);
                        inst_table["bgt.s"] = new ILToken (Token.INSTR_BRTARGET, BranchOp.bgt_s);
                        inst_table["ble.s"] = new ILToken (Token.INSTR_BRTARGET, BranchOp.ble_s);
                        inst_table["blt.s"] = new ILToken (Token.INSTR_BRTARGET, BranchOp.blt_s);
                        inst_table["bne.un.s"] = new ILToken (Token.INSTR_BRTARGET, BranchOp.bne_un_s);
                        inst_table["bge.un.s"] = new ILToken (Token.INSTR_BRTARGET, BranchOp.bge_un_s);
                        inst_table["bgt.un.s"] = new ILToken (Token.INSTR_BRTARGET, BranchOp.bgt_un_s);
                        inst_table["ble.un.s"] = new ILToken (Token.INSTR_BRTARGET, BranchOp.ble_un_s);
                        inst_table["blt.un.s"] = new ILToken (Token.INSTR_BRTARGET, BranchOp.blt_un_s);
                        inst_table["leave.s"] = new ILToken (Token.INSTR_BRTARGET, BranchOp.leave_s);

                        //
                        // Misc other instructions
                        //

                        inst_table["ldstr"] = new ILToken (Token.INSTR_STRING, MiscInstr.ldstr);
                        inst_table["ldc.r4"] = new ILToken (Token.INSTR_R, MiscInstr.ldc_r4);
                        inst_table["ldc.r8"] = new ILToken (Token.INSTR_R, MiscInstr.ldc_r8);
                        inst_table["ldc.i8"] = new ILToken (Token.INSTR_I8, MiscInstr.ldc_i8);
                        inst_table["switch"] = new ILToken (Token.INSTR_SWITCH, MiscInstr._switch);
                        inst_table["calli"] = new ILToken (Token.INSTR_SIG, MiscInstr.calli);
                        inst_table["ldtoken"] = new ILToken (Token.INSTR_TOK, MiscInstr.ldtoken);
                }

        }

}

