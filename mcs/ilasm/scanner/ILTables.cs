// ILTables.cs
// Mechanically generated  - DO NOT EDIT!
//
// (C) Sergey Chaban (serge@wildwestsoftware.com)

using System;
using System.Collections;
using System.Reflection.Emit;

namespace Mono.ILASM {





        public sealed class ILTables {

                private static Hashtable opcodes = null;
                private static Hashtable keywords = null;
                private static Hashtable directives = null;
                private static readonly object mutex;


                private ILTables ()
                {
                }

                static ILTables ()
                {
                        mutex = new object ();
                }

                private static void AllocTable (ref Hashtable tbl, int size)
                {
                        lock (mutex) {
                                if (tbl == null)
                                        tbl = new Hashtable (size);
                        }
                }

                public static Hashtable Opcodes
                {
                        get {
                                if (opcodes != null) return opcodes;

                                AllocTable (ref opcodes, 300);

                                opcodes ["nop"] = new InstrToken (OpCodes.Nop);
                                opcodes ["break"] = new InstrToken (OpCodes.Break);
                                opcodes ["ldarg.0"] = new InstrToken (OpCodes.Ldarg_0);
                                opcodes ["ldarg.1"] = new InstrToken (OpCodes.Ldarg_1);
                                opcodes ["ldarg.2"] = new InstrToken (OpCodes.Ldarg_2);
                                opcodes ["ldarg.3"] = new InstrToken (OpCodes.Ldarg_3);
                                opcodes ["ldloc.0"] = new InstrToken (OpCodes.Ldloc_0);
                                opcodes ["ldloc.1"] = new InstrToken (OpCodes.Ldloc_1);
                                opcodes ["ldloc.2"] = new InstrToken (OpCodes.Ldloc_2);
                                opcodes ["ldloc.3"] = new InstrToken (OpCodes.Ldloc_3);
                                opcodes ["stloc.0"] = new InstrToken (OpCodes.Stloc_0);
                                opcodes ["stloc.1"] = new InstrToken (OpCodes.Stloc_1);
                                opcodes ["stloc.2"] = new InstrToken (OpCodes.Stloc_2);
                                opcodes ["stloc.3"] = new InstrToken (OpCodes.Stloc_3);
                                opcodes ["ldarg.s"] = new InstrToken (OpCodes.Ldarg_S);
                                opcodes ["ldarga.s"] = new InstrToken (OpCodes.Ldarga_S);
                                opcodes ["starg.s"] = new InstrToken (OpCodes.Starg_S);
                                opcodes ["ldloc.s"] = new InstrToken (OpCodes.Ldloc_S);
                                opcodes ["ldloca.s"] = new InstrToken (OpCodes.Ldloca_S);
                                opcodes ["stloc.s"] = new InstrToken (OpCodes.Stloc_S);
                                opcodes ["ldnull"] = new InstrToken (OpCodes.Ldnull);
                                opcodes ["ldc.i4.m1"] = new InstrToken (OpCodes.Ldc_I4_M1);
                                opcodes ["ldc.i4.0"] = new InstrToken (OpCodes.Ldc_I4_0);
                                opcodes ["ldc.i4.1"] = new InstrToken (OpCodes.Ldc_I4_1);
                                opcodes ["ldc.i4.2"] = new InstrToken (OpCodes.Ldc_I4_2);
                                opcodes ["ldc.i4.3"] = new InstrToken (OpCodes.Ldc_I4_3);
                                opcodes ["ldc.i4.4"] = new InstrToken (OpCodes.Ldc_I4_4);
                                opcodes ["ldc.i4.5"] = new InstrToken (OpCodes.Ldc_I4_5);
                                opcodes ["ldc.i4.6"] = new InstrToken (OpCodes.Ldc_I4_6);
                                opcodes ["ldc.i4.7"] = new InstrToken (OpCodes.Ldc_I4_7);
                                opcodes ["ldc.i4.8"] = new InstrToken (OpCodes.Ldc_I4_8);
                                opcodes ["ldc.i4.s"] = new InstrToken (OpCodes.Ldc_I4_S);
                                opcodes ["ldc.i4"] = new InstrToken (OpCodes.Ldc_I4);
                                opcodes ["ldc.i8"] = new InstrToken (OpCodes.Ldc_I8);
                                opcodes ["ldc.r4"] = new InstrToken (OpCodes.Ldc_R4);
                                opcodes ["ldc.r8"] = new InstrToken (OpCodes.Ldc_R8);
                                opcodes ["dup"] = new InstrToken (OpCodes.Dup);
                                opcodes ["pop"] = new InstrToken (OpCodes.Pop);
                                opcodes ["jmp"] = new InstrToken (OpCodes.Jmp);
                                opcodes ["call"] = new InstrToken (OpCodes.Call);
                                opcodes ["calli"] = new InstrToken (OpCodes.Calli);
                                opcodes ["ret"] = new InstrToken (OpCodes.Ret);
                                opcodes ["br.s"] = new InstrToken (OpCodes.Br_S);
                                opcodes ["brfalse.s"] = new InstrToken (OpCodes.Brfalse_S);
                                opcodes ["brtrue.s"] = new InstrToken (OpCodes.Brtrue_S);
                                opcodes ["beq.s"] = new InstrToken (OpCodes.Beq_S);
                                opcodes ["bge.s"] = new InstrToken (OpCodes.Bge_S);
                                opcodes ["bgt.s"] = new InstrToken (OpCodes.Bgt_S);
                                opcodes ["ble.s"] = new InstrToken (OpCodes.Ble_S);
                                opcodes ["blt.s"] = new InstrToken (OpCodes.Blt_S);
                                opcodes ["bne.un.s"] = new InstrToken (OpCodes.Bne_Un_S);
                                opcodes ["bge.un.s"] = new InstrToken (OpCodes.Bge_Un_S);
                                opcodes ["bgt.un.s"] = new InstrToken (OpCodes.Bgt_Un_S);
                                opcodes ["ble.un.s"] = new InstrToken (OpCodes.Ble_Un_S);
                                opcodes ["blt.un.s"] = new InstrToken (OpCodes.Blt_Un_S);
                                opcodes ["br"] = new InstrToken (OpCodes.Br);
                                opcodes ["brfalse"] = new InstrToken (OpCodes.Brfalse);
                                opcodes ["brtrue"] = new InstrToken (OpCodes.Brtrue);
                                opcodes ["beq"] = new InstrToken (OpCodes.Beq);
                                opcodes ["bge"] = new InstrToken (OpCodes.Bge);
                                opcodes ["bgt"] = new InstrToken (OpCodes.Bgt);
                                opcodes ["ble"] = new InstrToken (OpCodes.Ble);
                                opcodes ["blt"] = new InstrToken (OpCodes.Blt);
                                opcodes ["bne.un"] = new InstrToken (OpCodes.Bne_Un);
                                opcodes ["bge.un"] = new InstrToken (OpCodes.Bge_Un);
                                opcodes ["bgt.un"] = new InstrToken (OpCodes.Bgt_Un);
                                opcodes ["ble.un"] = new InstrToken (OpCodes.Ble_Un);
                                opcodes ["blt.un"] = new InstrToken (OpCodes.Blt_Un);
                                opcodes ["switch"] = new InstrToken (OpCodes.Switch);
                                opcodes ["ldind.i1"] = new InstrToken (OpCodes.Ldind_I1);
                                opcodes ["ldind.u1"] = new InstrToken (OpCodes.Ldind_U1);
                                opcodes ["ldind.i2"] = new InstrToken (OpCodes.Ldind_I2);
                                opcodes ["ldind.u2"] = new InstrToken (OpCodes.Ldind_U2);
                                opcodes ["ldind.i4"] = new InstrToken (OpCodes.Ldind_I4);
                                opcodes ["ldind.u4"] = new InstrToken (OpCodes.Ldind_U4);
                                opcodes ["ldind.i8"] = new InstrToken (OpCodes.Ldind_I8);
                                opcodes ["ldind.i"] = new InstrToken (OpCodes.Ldind_I);
                                opcodes ["ldind.r4"] = new InstrToken (OpCodes.Ldind_R4);
                                opcodes ["ldind.r8"] = new InstrToken (OpCodes.Ldind_R8);
                                opcodes ["ldind.ref"] = new InstrToken (OpCodes.Ldind_Ref);
                                opcodes ["stind.ref"] = new InstrToken (OpCodes.Stind_Ref);
                                opcodes ["stind.i1"] = new InstrToken (OpCodes.Stind_I1);
                                opcodes ["stind.i2"] = new InstrToken (OpCodes.Stind_I2);
                                opcodes ["stind.i4"] = new InstrToken (OpCodes.Stind_I4);
                                opcodes ["stind.i8"] = new InstrToken (OpCodes.Stind_I8);
                                opcodes ["stind.r4"] = new InstrToken (OpCodes.Stind_R4);
                                opcodes ["stind.r8"] = new InstrToken (OpCodes.Stind_R8);
                                opcodes ["add"] = new InstrToken (OpCodes.Add);
                                opcodes ["sub"] = new InstrToken (OpCodes.Sub);
                                opcodes ["mul"] = new InstrToken (OpCodes.Mul);
                                opcodes ["div"] = new InstrToken (OpCodes.Div);
                                opcodes ["div.un"] = new InstrToken (OpCodes.Div_Un);
                                opcodes ["rem"] = new InstrToken (OpCodes.Rem);
                                opcodes ["rem.un"] = new InstrToken (OpCodes.Rem_Un);
                                opcodes ["and"] = new InstrToken (OpCodes.And);
                                opcodes ["or"] = new InstrToken (OpCodes.Or);
                                opcodes ["xor"] = new InstrToken (OpCodes.Xor);
                                opcodes ["shl"] = new InstrToken (OpCodes.Shl);
                                opcodes ["shr"] = new InstrToken (OpCodes.Shr);
                                opcodes ["shr.un"] = new InstrToken (OpCodes.Shr_Un);
                                opcodes ["neg"] = new InstrToken (OpCodes.Neg);
                                opcodes ["not"] = new InstrToken (OpCodes.Not);
                                opcodes ["conv.i1"] = new InstrToken (OpCodes.Conv_I1);
                                opcodes ["conv.i2"] = new InstrToken (OpCodes.Conv_I2);
                                opcodes ["conv.i4"] = new InstrToken (OpCodes.Conv_I4);
                                opcodes ["conv.i8"] = new InstrToken (OpCodes.Conv_I8);
                                opcodes ["conv.r4"] = new InstrToken (OpCodes.Conv_R4);
                                opcodes ["conv.r8"] = new InstrToken (OpCodes.Conv_R8);
                                opcodes ["conv.u4"] = new InstrToken (OpCodes.Conv_U4);
                                opcodes ["conv.u8"] = new InstrToken (OpCodes.Conv_U8);
                                opcodes ["callvirt"] = new InstrToken (OpCodes.Callvirt);
                                opcodes ["cpobj"] = new InstrToken (OpCodes.Cpobj);
                                opcodes ["ldobj"] = new InstrToken (OpCodes.Ldobj);
                                opcodes ["ldstr"] = new InstrToken (OpCodes.Ldstr);
                                opcodes ["newobj"] = new InstrToken (OpCodes.Newobj);
                                opcodes ["castclass"] = new InstrToken (OpCodes.Castclass);
                                opcodes ["isinst"] = new InstrToken (OpCodes.Isinst);
                                opcodes ["conv.r.un"] = new InstrToken (OpCodes.Conv_R_Un);
                                opcodes ["unbox"] = new InstrToken (OpCodes.Unbox);
                                opcodes ["throw"] = new InstrToken (OpCodes.Throw);
                                opcodes ["ldfld"] = new InstrToken (OpCodes.Ldfld);
                                opcodes ["ldflda"] = new InstrToken (OpCodes.Ldflda);
                                opcodes ["stfld"] = new InstrToken (OpCodes.Stfld);
                                opcodes ["ldsfld"] = new InstrToken (OpCodes.Ldsfld);
                                opcodes ["ldsflda"] = new InstrToken (OpCodes.Ldsflda);
                                opcodes ["stsfld"] = new InstrToken (OpCodes.Stsfld);
                                opcodes ["stobj"] = new InstrToken (OpCodes.Stobj);
                                opcodes ["conv.ovf.i1.un"] = new InstrToken (OpCodes.Conv_Ovf_I1_Un);
                                opcodes ["conv.ovf.i2.un"] = new InstrToken (OpCodes.Conv_Ovf_I2_Un);
                                opcodes ["conv.ovf.i4.un"] = new InstrToken (OpCodes.Conv_Ovf_I4_Un);
                                opcodes ["conv.ovf.i8.un"] = new InstrToken (OpCodes.Conv_Ovf_I8_Un);
                                opcodes ["conv.ovf.u1.un"] = new InstrToken (OpCodes.Conv_Ovf_U1_Un);
                                opcodes ["conv.ovf.u2.un"] = new InstrToken (OpCodes.Conv_Ovf_U2_Un);
                                opcodes ["conv.ovf.u4.un"] = new InstrToken (OpCodes.Conv_Ovf_U4_Un);
                                opcodes ["conv.ovf.u8.un"] = new InstrToken (OpCodes.Conv_Ovf_U8_Un);
                                opcodes ["conv.ovf.i.un"] = new InstrToken (OpCodes.Conv_Ovf_I_Un);
                                opcodes ["conv.ovf.u.un"] = new InstrToken (OpCodes.Conv_Ovf_U_Un);
                                // /* Obsolete! */ opcodes ["boxval"] = new InstrToken (OpCodes.Boxval);
                                opcodes ["box"] = new InstrToken (OpCodes.Box);
                                opcodes ["newarr"] = new InstrToken (OpCodes.Newarr);
                                opcodes ["ldlen"] = new InstrToken (OpCodes.Ldlen);
                                opcodes ["ldelema"] = new InstrToken (OpCodes.Ldelema);
                                opcodes ["ldelem.i1"] = new InstrToken (OpCodes.Ldelem_I1);
                                opcodes ["ldelem.u1"] = new InstrToken (OpCodes.Ldelem_U1);
                                opcodes ["ldelem.i2"] = new InstrToken (OpCodes.Ldelem_I2);
                                opcodes ["ldelem.u2"] = new InstrToken (OpCodes.Ldelem_U2);
                                opcodes ["ldelem.i4"] = new InstrToken (OpCodes.Ldelem_I4);
                                opcodes ["ldelem.u4"] = new InstrToken (OpCodes.Ldelem_U4);
                                opcodes ["ldelem.i8"] = new InstrToken (OpCodes.Ldelem_I8);
                                opcodes ["ldelem.i"] = new InstrToken (OpCodes.Ldelem_I);
                                opcodes ["ldelem.r4"] = new InstrToken (OpCodes.Ldelem_R4);
                                opcodes ["ldelem.r8"] = new InstrToken (OpCodes.Ldelem_R8);
                                opcodes ["ldelem.ref"] = new InstrToken (OpCodes.Ldelem_Ref);
                                opcodes ["stelem.i"] = new InstrToken (OpCodes.Stelem_I);
                                opcodes ["stelem.i1"] = new InstrToken (OpCodes.Stelem_I1);
                                opcodes ["stelem.i2"] = new InstrToken (OpCodes.Stelem_I2);
                                opcodes ["stelem.i4"] = new InstrToken (OpCodes.Stelem_I4);
                                opcodes ["stelem.i8"] = new InstrToken (OpCodes.Stelem_I8);
                                opcodes ["stelem.r4"] = new InstrToken (OpCodes.Stelem_R4);
                                opcodes ["stelem.r8"] = new InstrToken (OpCodes.Stelem_R8);
                                opcodes ["stelem.ref"] = new InstrToken (OpCodes.Stelem_Ref);
                                opcodes ["conv.ovf.i1"] = new InstrToken (OpCodes.Conv_Ovf_I1);
                                opcodes ["conv.ovf.u1"] = new InstrToken (OpCodes.Conv_Ovf_U1);
                                opcodes ["conv.ovf.i2"] = new InstrToken (OpCodes.Conv_Ovf_I2);
                                opcodes ["conv.ovf.u2"] = new InstrToken (OpCodes.Conv_Ovf_U2);
                                opcodes ["conv.ovf.i4"] = new InstrToken (OpCodes.Conv_Ovf_I4);
                                opcodes ["conv.ovf.u4"] = new InstrToken (OpCodes.Conv_Ovf_U4);
                                opcodes ["conv.ovf.i8"] = new InstrToken (OpCodes.Conv_Ovf_I8);
                                opcodes ["conv.ovf.u8"] = new InstrToken (OpCodes.Conv_Ovf_U8);
                                opcodes ["refanyval"] = new InstrToken (OpCodes.Refanyval);
                                opcodes ["ckfinite"] = new InstrToken (OpCodes.Ckfinite);
                                opcodes ["mkrefany"] = new InstrToken (OpCodes.Mkrefany);
                                opcodes ["ldtoken"] = new InstrToken (OpCodes.Ldtoken);
                                opcodes ["conv.u2"] = new InstrToken (OpCodes.Conv_U2);
                                opcodes ["conv.u1"] = new InstrToken (OpCodes.Conv_U1);
                                opcodes ["conv.i"] = new InstrToken (OpCodes.Conv_I);
                                opcodes ["conv.ovf.i"] = new InstrToken (OpCodes.Conv_Ovf_I);
                                opcodes ["conv.ovf.u"] = new InstrToken (OpCodes.Conv_Ovf_U);
                                opcodes ["add.ovf"] = new InstrToken (OpCodes.Add_Ovf);
                                opcodes ["add.ovf.un"] = new InstrToken (OpCodes.Add_Ovf_Un);
                                opcodes ["mul.ovf"] = new InstrToken (OpCodes.Mul_Ovf);
                                opcodes ["mul.ovf.un"] = new InstrToken (OpCodes.Mul_Ovf_Un);
                                opcodes ["sub.ovf"] = new InstrToken (OpCodes.Sub_Ovf);
                                opcodes ["sub.ovf.un"] = new InstrToken (OpCodes.Sub_Ovf_Un);
                                opcodes ["endfinally"] = new InstrToken (OpCodes.Endfinally);
                                opcodes ["leave"] = new InstrToken (OpCodes.Leave);
                                opcodes ["leave.s"] = new InstrToken (OpCodes.Leave_S);
                                opcodes ["stind.i"] = new InstrToken (OpCodes.Stind_I);
                                opcodes ["conv.u"] = new InstrToken (OpCodes.Conv_U);
                                opcodes ["prefix7"] = new InstrToken (OpCodes.Prefix7);
                                opcodes ["prefix6"] = new InstrToken (OpCodes.Prefix6);
                                opcodes ["prefix5"] = new InstrToken (OpCodes.Prefix5);
                                opcodes ["prefix4"] = new InstrToken (OpCodes.Prefix4);
                                opcodes ["prefix3"] = new InstrToken (OpCodes.Prefix3);
                                opcodes ["prefix2"] = new InstrToken (OpCodes.Prefix2);
                                opcodes ["prefix1"] = new InstrToken (OpCodes.Prefix1);
                                opcodes ["prefixref"] = new InstrToken (OpCodes.Prefixref);
                                opcodes ["arglist"] = new InstrToken (OpCodes.Arglist);
                                opcodes ["ceq"] = new InstrToken (OpCodes.Ceq);
                                opcodes ["cgt"] = new InstrToken (OpCodes.Cgt);
                                opcodes ["cgt.un"] = new InstrToken (OpCodes.Cgt_Un);
                                opcodes ["clt"] = new InstrToken (OpCodes.Clt);
                                opcodes ["clt.un"] = new InstrToken (OpCodes.Clt_Un);
                                opcodes ["ldftn"] = new InstrToken (OpCodes.Ldftn);
                                opcodes ["ldvirtftn"] = new InstrToken (OpCodes.Ldvirtftn);
                                opcodes ["ldarg"] = new InstrToken (OpCodes.Ldarg);
                                opcodes ["ldarga"] = new InstrToken (OpCodes.Ldarga);
                                opcodes ["starg"] = new InstrToken (OpCodes.Starg);
                                opcodes ["ldloc"] = new InstrToken (OpCodes.Ldloc);
                                opcodes ["ldloca"] = new InstrToken (OpCodes.Ldloca);
                                opcodes ["stloc"] = new InstrToken (OpCodes.Stloc);
                                opcodes ["localloc"] = new InstrToken (OpCodes.Localloc);
                                opcodes ["endfilter"] = new InstrToken (OpCodes.Endfilter);
                                opcodes ["unaligned."] = new InstrToken (OpCodes.Unaligned);
                                opcodes ["volatile."] = new InstrToken (OpCodes.Volatile);
                                opcodes ["tail."] = new InstrToken (OpCodes.Tailcall);
                                opcodes ["initobj"] = new InstrToken (OpCodes.Initobj);
                                opcodes ["cpblk"] = new InstrToken (OpCodes.Cpblk);
                                opcodes ["initblk"] = new InstrToken (OpCodes.Initblk);
                                opcodes ["rethrow"] = new InstrToken (OpCodes.Rethrow);
                                opcodes ["sizeof"] = new InstrToken (OpCodes.Sizeof);
                                opcodes ["refanytype"] = new InstrToken (OpCodes.Refanytype);

                                return opcodes;
                        }
                }


                public static Hashtable Directives
                {
                        get {
                                if (directives != null) return directives;

                                AllocTable (ref directives, 300);

                                directives [".addon"] = new ILToken (Token.D_ADDON, ".addon");
                                directives [".algorithm"] = new ILToken (Token.D_ALGORITHM, ".algorithm");
                                directives [".assembly"] = new ILToken (Token.D_ASSEMBLY, ".assembly");
                                directives [".backing"] = new ILToken (Token.D_BACKING, ".backing");
                                directives [".blob"] = new ILToken (Token.D_BLOB, ".blob");
                                directives [".capability"] = new ILToken (Token.D_CAPABILITY, ".capability");
                                directives [".cctor"] = new ILToken (Token.D_CCTOR, ".cctor");
                                directives [".class"] = new ILToken (Token.D_CLASS, ".class");
                                directives [".comtype"] = new ILToken (Token.D_COMTYPE, ".comtype");
                                directives [".config"] = new ILToken (Token.D_CONFIG, ".config");
                                directives [".corflags"] = new ILToken (Token.D_CORFLAGS, ".corflags");
                                directives [".ctor"] = new ILToken (Token.D_CTOR, ".ctor");
                                directives [".custom"] = new ILToken (Token.D_CUSTOM, ".custom");
                                directives [".data"] = new ILToken (Token.D_DATA, ".data");
                                directives [".emitbyte"] = new ILToken (Token.D_EMITBYTE, ".emitbyte");
                                directives [".entrypoint"] = new ILToken (Token.D_ENTRYPOINT, ".entrypoint");
                                directives [".event"] = new ILToken (Token.D_EVENT, ".event");
                                directives [".exeloc"] = new ILToken (Token.D_EXELOC, ".exeloc");
                                directives [".export"] = new ILToken (Token.D_EXPORT, ".export");
                                directives [".field"] = new ILToken (Token.D_FIELD, ".field");
                                directives [".file"] = new ILToken (Token.D_FILE, ".file");
                                directives [".fire"] = new ILToken (Token.D_FIRE, ".fire");
                                directives [".get"] = new ILToken (Token.D_GET, ".get");
                                directives [".hash"] = new ILToken (Token.D_HASH, ".hash");
                                directives [".implicitcom"] = new ILToken (Token.D_IMPLICITCOM, ".implicitcom");
                                directives [".language"] = new ILToken (Token.D_LANGUAGE, ".language");
                                directives [".line"] = new ILToken (Token.D_LINE, ".line");
                                directives ["#line"] = new ILToken (Token.D_XLINE, "#line");
                                directives [".locale"] = new ILToken (Token.D_LOCALE, ".locale");
                                directives [".locals"] = new ILToken (Token.D_LOCALS, ".locals");
                                directives [".manifestres"] = new ILToken (Token.D_MANIFESTRES, ".manifestres");
                                directives [".maxstack"] = new ILToken (Token.D_MAXSTACK, ".maxstack");
                                directives [".method"] = new ILToken (Token.D_METHOD, ".method");
                                directives [".mime"] = new ILToken (Token.D_MIME, ".mime");
                                directives [".module"] = new ILToken (Token.D_MODULE, ".module");
                                directives [".mresource"] = new ILToken (Token.D_MRESOURCE, ".mresource");
                                directives [".namespace"] = new ILToken (Token.D_NAMESPACE, ".namespace");
                                directives [".originator"] = new ILToken (Token.D_ORIGINATOR, ".originator");
                                directives [".os"] = new ILToken (Token.D_OS, ".os");
                                directives [".other"] = new ILToken (Token.D_OTHER, ".other");
                                directives [".override"] = new ILToken (Token.D_OVERRIDE, ".override");
                                directives [".pack"] = new ILToken (Token.D_PACK, ".pack");
                                directives [".param"] = new ILToken (Token.D_PARAM, ".param");
                                directives [".permission"] = new ILToken (Token.D_PERMISSION, ".permission");
                                directives [".permissionset"] = new ILToken (Token.D_PERMISSIONSET, ".permissionset");
                                directives [".processor"] = new ILToken (Token.D_PROCESSOR, ".processor");
                                directives [".property"] = new ILToken (Token.D_PROPERTY, ".property");
                                directives [".publickey"] = new ILToken (Token.D_PUBLICKEY, ".publickey");
                                directives [".publickeytoken"] = new ILToken (Token.D_PUBLICKEYTOKEN, ".publickeytoken");
                                directives [".removeon"] = new ILToken (Token.D_REMOVEON, ".removeon");
                                directives [".set"] = new ILToken (Token.D_SET, ".set");
                                directives [".size"] = new ILToken (Token.D_SIZE, ".size");
                                directives [".subsystem"] = new ILToken (Token.D_SUBSYSTEM, ".subsystem");
                                directives [".title"] = new ILToken (Token.D_TITLE, ".title");
                                directives [".try"] = new ILToken (Token.D_TRY, ".try");
                                directives [".ver"] = new ILToken (Token.D_VER, ".ver");
                                directives [".vtable"] = new ILToken (Token.D_VTABLE, ".vtable");
                                directives [".vtentry"] = new ILToken (Token.D_VTENTRY, ".vtentry");
                                directives [".vtfixup"] = new ILToken (Token.D_VTFIXUP, ".vtfixup");
                                directives [".zeroinit"] = new ILToken (Token.D_ZEROINIT, ".zeroinit");

                                return directives;
                        }
                }



                public static Hashtable Keywords
                {
                        get {
                                if (keywords != null) return keywords;

                                AllocTable (ref keywords, 300);

                                keywords ["at"] = new ILToken (Token.K_AT, "at");
                                keywords ["as"] = new ILToken (Token.K_AS, "as");
                                keywords ["implicitcom"] = new ILToken (Token.K_IMPLICITCOM, "implicitcom");
                                keywords ["implicitres"] = new ILToken (Token.K_IMPLICITRES, "implicitres");
                                keywords ["noappdomain"] = new ILToken (Token.K_NOAPPDOMAIN, "noappdomain");
                                keywords ["noprocess"] = new ILToken (Token.K_NOPROCESS, "noprocess");
                                keywords ["nomachine"] = new ILToken (Token.K_NOMACHINE, "nomachine");
                                keywords ["extern"] = new ILToken (Token.K_EXTERN, "extern");
                                keywords ["instance"] = new ILToken (Token.K_INSTANCE, "instance");
                                keywords ["explicit"] = new ILToken (Token.K_EXPLICIT, "explicit");
                                keywords ["default"] = new ILToken (Token.K_DEFAULT, "default");
                                keywords ["vararg"] = new ILToken (Token.K_VARARG, "vararg");
                                keywords ["unmanaged"] = new ILToken (Token.K_UNMANAGED, "unmanaged");
                                keywords ["cdecl"] = new ILToken (Token.K_CDECL, "cdecl");
                                keywords ["stdcall"] = new ILToken (Token.K_STDCALL, "stdcall");
                                keywords ["thiscall"] = new ILToken (Token.K_THISCALL, "thiscall");
                                keywords ["fastcall"] = new ILToken (Token.K_FASTCALL, "fastcall");
                                keywords ["marshal"] = new ILToken (Token.K_MARSHAL, "marshal");
                                keywords ["in"] = new ILToken (Token.K_IN, "in");
                                keywords ["out"] = new ILToken (Token.K_OUT, "out");
                                keywords ["opt"] = new ILToken (Token.K_OPT, "opt");
                                keywords ["lcid"] = new ILToken (Token.K_LCID, "lcid");
                                keywords ["retval"] = new ILToken (Token.K_RETVAL, "retval");
                                keywords ["static"] = new ILToken (Token.K_STATIC, "static");
                                keywords ["public"] = new ILToken (Token.K_PUBLIC, "public");
                                keywords ["private"] = new ILToken (Token.K_PRIVATE, "private");
                                keywords ["family"] = new ILToken (Token.K_FAMILY, "family");
                                keywords ["initonly"] = new ILToken (Token.K_INITONLY, "initonly");
                                keywords ["rtspecialname"] = new ILToken (Token.K_RTSPECIALNAME, "rtspecialname");
                                keywords ["specialname"] = new ILToken (Token.K_SPECIALNAME, "specialname");
                                keywords ["assembly"] = new ILToken (Token.K_ASSEMBLY, "assembly");
                                keywords ["famandassem"] = new ILToken (Token.K_FAMANDASSEM, "famandassem");
                                keywords ["famorassem"] = new ILToken (Token.K_FAMORASSEM, "famorassem");
                                keywords ["privatescope"] = new ILToken (Token.K_PRIVATESCOPE, "privatescope");
                                keywords ["literal"] = new ILToken (Token.K_LITERAL, "literal");
                                keywords ["notserialized"] = new ILToken (Token.K_NOTSERIALIZED, "notserialized");
                                keywords ["value"] = new ILToken (Token.K_VALUE, "value");
                                keywords ["not_in_gc_heap"] = new ILToken (Token.K_NOT_IN_GC_HEAP, "not_in_gc_heap");
                                keywords ["interface"] = new ILToken (Token.K_INTERFACE, "interface");
                                keywords ["sealed"] = new ILToken (Token.K_SEALED, "sealed");
                                keywords ["abstract"] = new ILToken (Token.K_ABSTRACT, "abstract");
                                keywords ["auto"] = new ILToken (Token.K_AUTO, "auto");
                                keywords ["sequential"] = new ILToken (Token.K_SEQUENTIAL, "sequential");
                                keywords ["ansi"] = new ILToken (Token.K_ANSI, "ansi");
                                keywords ["unicode"] = new ILToken (Token.K_UNICODE, "unicode");
                                keywords ["autochar"] = new ILToken (Token.K_AUTOCHAR, "autochar");
                                keywords ["import"] = new ILToken (Token.K_IMPORT, "import");
                                keywords ["serializable"] = new ILToken (Token.K_SERIALIZABLE, "serializable");
                                keywords ["nested"] = new ILToken (Token.K_NESTED, "nested");
                                keywords ["lateinit"] = new ILToken (Token.K_LATEINIT, "lateinit");
                                keywords ["extends"] = new ILToken (Token.K_EXTENDS, "extends");
                                keywords ["implements"] = new ILToken (Token.K_IMPLEMENTS, "implements");
                                keywords ["final"] = new ILToken (Token.K_FINAL, "final");
                                keywords ["virtual"] = new ILToken (Token.K_VIRTUAL, "virtual");
                                keywords ["hidebysig"] = new ILToken (Token.K_HIDEBYSIG, "hidebysig");
                                keywords ["newslot"] = new ILToken (Token.K_NEWSLOT, "newslot");
                                keywords ["unmanagedexp"] = new ILToken (Token.K_UNMANAGEDEXP, "unmanagedexp");
                                keywords ["pinvokeimpl"] = new ILToken (Token.K_PINVOKEIMPL, "pinvokeimpl");
                                keywords ["nomangle"] = new ILToken (Token.K_NOMANGLE, "nomangle");
                                keywords ["ole"] = new ILToken (Token.K_OLE, "ole");
                                keywords ["lasterr"] = new ILToken (Token.K_LASTERR, "lasterr");
                                keywords ["winapi"] = new ILToken (Token.K_WINAPI, "winapi");
                                keywords ["native"] = new ILToken (Token.K_NATIVE, "native");
                                keywords ["il"] = new ILToken (Token.K_IL, "il");
                                keywords ["cil"] = new ILToken (Token.K_CIL, "cil");
                                keywords ["optil"] = new ILToken (Token.K_OPTIL, "optil");
                                keywords ["managed"] = new ILToken (Token.K_MANAGED, "managed");
                                keywords ["forwardref"] = new ILToken (Token.K_FORWARDREF, "forwardref");
                                keywords ["runtime"] = new ILToken (Token.K_RUNTIME, "runtime");
                                keywords ["internalcall"] = new ILToken (Token.K_INTERNALCALL, "internalcall");
                                keywords ["synchronized"] = new ILToken (Token.K_SYNCHRONIZED, "synchronized");
                                keywords ["noinlining"] = new ILToken (Token.K_NOINLINING, "noinlining");
                                keywords ["custom"] = new ILToken (Token.K_CUSTOM, "custom");
                                keywords ["fixed"] = new ILToken (Token.K_FIXED, "fixed");
                                keywords ["sysstring"] = new ILToken (Token.K_SYSSTRING, "sysstring");
                                keywords ["array"] = new ILToken (Token.K_ARRAY, "array");
                                keywords ["variant"] = new ILToken (Token.K_VARIANT, "variant");
                                keywords ["currency"] = new ILToken (Token.K_CURRENCY, "currency");
                                keywords ["syschar"] = new ILToken (Token.K_SYSCHAR, "syschar");
                                keywords ["void"] = new ILToken (Token.K_VOID, "void");
                                keywords ["bool"] = new ILToken (Token.K_BOOL, "bool");
                                keywords ["int8"] = new ILToken (Token.K_INT8, "int8");
                                keywords ["int16"] = new ILToken (Token.K_INT16, "int16");
                                keywords ["int32"] = new ILToken (Token.K_INT32, "int32");
                                keywords ["int64"] = new ILToken (Token.K_INT64, "int64");
                                keywords ["float32"] = new ILToken (Token.K_FLOAT32, "float32");
                                keywords ["float64"] = new ILToken (Token.K_FLOAT64, "float64");
                                keywords ["error"] = new ILToken (Token.K_ERROR, "error");
                                keywords ["unsigned"] = new ILToken (Token.K_UNSIGNED, "unsigned");
                                keywords ["decimal"] = new ILToken (Token.K_DECIMAL, "decimal");
                                keywords ["date"] = new ILToken (Token.K_DATE, "date");
                                keywords ["bstr"] = new ILToken (Token.K_BSTR, "bstr");
                                keywords ["lpstr"] = new ILToken (Token.K_LPSTR, "lpstr");
                                keywords ["lpwstr"] = new ILToken (Token.K_LPWSTR, "lpwstr");
                                keywords ["lptstr"] = new ILToken (Token.K_LPTSTR, "lptstr");
                                keywords ["objectref"] = new ILToken (Token.K_OBJECTREF, "objectref");
                                keywords ["iunknown"] = new ILToken (Token.K_IUNKNOWN, "iunknown");
                                keywords ["idispatch"] = new ILToken (Token.K_IDISPATCH, "idispatch");
                                keywords ["struct"] = new ILToken (Token.K_STRUCT, "struct");
                                keywords ["safearray"] = new ILToken (Token.K_SAFEARRAY, "safearray");
                                keywords ["int"] = new ILToken (Token.K_INT, "int");
                                keywords ["byvalstr"] = new ILToken (Token.K_BYVALSTR, "byvalstr");
                                keywords ["tbstr"] = new ILToken (Token.K_TBSTR, "tbstr");
                                keywords ["lpvoid"] = new ILToken (Token.K_LPVOID, "lpvoid");
                                keywords ["any"] = new ILToken (Token.K_ANY, "any");
                                keywords ["float"] = new ILToken (Token.K_FLOAT, "float");
                                keywords ["lpstruct"] = new ILToken (Token.K_LPSTRUCT, "lpstruct");
                                keywords ["null"] = new ILToken (Token.K_NULL, "null");
                                //              keywords ["ptr"] = new ILToken (Token.K_PTR, "ptr");
                                keywords ["vector"] = new ILToken (Token.K_VECTOR, "vector");
                                keywords ["hresult"] = new ILToken (Token.K_HRESULT, "hresult");
                                keywords ["carray"] = new ILToken (Token.K_CARRAY, "carray");
                                keywords ["userdefined"] = new ILToken (Token.K_USERDEFINED, "userdefined");
                                keywords ["record"] = new ILToken (Token.K_RECORD, "record");
                                keywords ["filetime"] = new ILToken (Token.K_FILETIME, "filetime");
                                keywords ["blob"] = new ILToken (Token.K_BLOB, "blob");
                                keywords ["stream"] = new ILToken (Token.K_STREAM, "stream");
                                keywords ["storage"] = new ILToken (Token.K_STORAGE, "storage");
                                keywords ["streamed_object"] = new ILToken (Token.K_STREAMED_OBJECT, "streamed_object");
                                keywords ["stored_object"] = new ILToken (Token.K_STORED_OBJECT, "stored_object");
                                keywords ["blob_object"] = new ILToken (Token.K_BLOB_OBJECT, "blob_object");
                                keywords ["cf"] = new ILToken (Token.K_CF, "cf");
                                keywords ["clsid"] = new ILToken (Token.K_CLSID, "clsid");
                                keywords ["method"] = new ILToken (Token.K_METHOD, "method");
                                keywords ["class"] = new ILToken (Token.K_CLASS, "class");
                                keywords ["pinned"] = new ILToken (Token.K_PINNED, "pinned");
                                keywords ["modreq"] = new ILToken (Token.K_MODREQ, "modreq");
                                keywords ["modopt"] = new ILToken (Token.K_MODOPT, "modopt");
                                keywords ["typedref"] = new ILToken (Token.K_TYPEDREF, "typedref");
                                keywords ["refany"] = new ILToken (Token.K_TYPEDREF, "typedref");
                                keywords ["wchar"] = new ILToken (Token.K_WCHAR, "wchar");
                                keywords ["char"] = new ILToken (Token.K_CHAR, "char");
                                keywords ["fromunmanaged"] = new ILToken (Token.K_FROMUNMANAGED, "fromunmanaged");
                                keywords ["callmostderived"] = new ILToken (Token.K_CALLMOSTDERIVED, "callmostderived");
                                keywords ["bytearray"] = new ILToken (Token.K_BYTEARRAY, "bytearray");
                                keywords ["with"] = new ILToken (Token.K_WITH, "with");
                                keywords ["init"] = new ILToken (Token.K_INIT, "init");
                                keywords ["to"] = new ILToken (Token.K_TO, "to");
                                keywords ["catch"] = new ILToken (Token.K_CATCH, "catch");
                                keywords ["filter"] = new ILToken (Token.K_FILTER, "filter");
                                keywords ["finally"] = new ILToken (Token.K_FINALLY, "finally");
                                keywords ["fault"] = new ILToken (Token.K_FAULT, "fault");
                                keywords ["handler"] = new ILToken (Token.K_HANDLER, "handler");
                                keywords ["tls"] = new ILToken (Token.K_TLS, "tls");
                                keywords ["field"] = new ILToken (Token.K_FIELD, "field");
                                keywords ["request"] = new ILToken (Token.K_REQUEST, "request");
                                keywords ["demand"] = new ILToken (Token.K_DEMAND, "demand");
                                keywords ["assert"] = new ILToken (Token.K_ASSERT, "assert");
                                keywords ["deny"] = new ILToken (Token.K_DENY, "deny");
                                keywords ["permitonly"] = new ILToken (Token.K_PERMITONLY, "permitonly");
                                keywords ["linkcheck"] = new ILToken (Token.K_LINKCHECK, "linkcheck");
                                keywords ["inheritcheck"] = new ILToken (Token.K_INHERITCHECK, "inheritcheck");
                                keywords ["reqmin"] = new ILToken (Token.K_REQMIN, "reqmin");
                                keywords ["reqopt"] = new ILToken (Token.K_REQOPT, "reqopt");
                                keywords ["reqrefuse"] = new ILToken (Token.K_REQREFUSE, "reqrefuse");
                                keywords ["prejitgrant"] = new ILToken (Token.K_PREJITGRANT, "prejitgrant");
                                keywords ["prejitdeny"] = new ILToken (Token.K_PREJITDENY, "prejitdeny");
                                keywords ["noncasdemand"] = new ILToken (Token.K_NONCASDEMAND, "noncasdemand");
                                keywords ["noncaslinkdemand"] = new ILToken (Token.K_NONCASLINKDEMAND, "noncaslinkdemand");
                                keywords ["noncasinheritance"] = new ILToken (Token.K_NONCASINHERITANCE, "noncasinheritance");
                                keywords ["readonly"] = new ILToken (Token.K_READONLY, "readonly");
                                keywords ["nometadata"] = new ILToken (Token.K_NOMETADATA, "nometadata");
                                keywords ["algorithm"] = new ILToken (Token.K_ALGORITHM, "algorithm");
                                keywords ["fullorigin"] = new ILToken (Token.K_FULLORIGIN, "fullorigin");
                                // keywords ["nan"] = new ILToken (Token.K_NAN, "nan");
                                // keywords ["inf"] = new ILToken (Token.K_INF, "inf");
                                keywords ["publickey"] = new ILToken (Token.K_PUBLICKEY, "publickey");
                                keywords ["enablejittracking"] = new ILToken (Token.K_ENABLEJITTRACKING, "enablejittracking");
                                keywords ["disablejitoptimizer"] = new ILToken (Token.K_DISABLEJITOPTIMIZER, "disablejitoptimizer");
                                keywords ["preservesig"] = new ILToken (Token.K_PRESERVESIG, "preservesig");
                                keywords ["beforefieldinit"] = new ILToken (Token.K_BEFOREFIELDINIT, "beforefieldinit");
                                keywords ["alignment"] = new ILToken (Token.K_ALIGNMENT, "alignment");
                                keywords ["nullref"] = new ILToken (Token.K_NULLREF, "nullref");
                                keywords ["valuetype"] = new ILToken (Token.K_VALUETYPE, "valuetype");
                                keywords ["Compilercontrolled"] = new ILToken (Token.K_COMPILERCONTROLLED, "Compilercontrolled");
                                keywords ["reqsecobj"] = new ILToken (Token.K_REQSECOBJ, "reqsecobj");
                                keywords ["enum"] = new ILToken (Token.K_ENUM, "enum");
                                keywords ["object"] = new ILToken (Token.K_OBJECT, "object");
                                keywords ["string"] = new ILToken (Token.K_STRING, "string");
                                keywords ["true"] = new ILToken (Token.K_TRUE, "true");
                                keywords ["false"] = new ILToken (Token.K_FALSE, "false");

                                return keywords;
                        }
                }




        } // class ILTables




} // namespace Mono.ILASM
